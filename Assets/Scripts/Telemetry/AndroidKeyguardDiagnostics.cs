#if UNITY_ANDROID && DEVELOPMENT_BUILD
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// DEVELOPMENT_BUILD Android-only: read-only keyguard / window / input snapshots.
/// Does not change flags, sleepTimeout, GameActivity, immersive, or gameplay.
/// </summary>
public sealed class AndroidKeyguardDiagnostics : MonoBehaviour
{
    private const string KeyguardTag = "[KeyguardDiag]";
    private const string WindowTag = "[WindowDiag]";
    private const string InputTag = "[InputDiag]";
    private const string SoftInputTag = "[SoftInputDiag]";

    private const float DelayedSnapshotSeconds = 0.75f;

    // WindowManager.LayoutParams legacy flag bits.
    private const int FlagAllowLockWhileScreenOn = 0x00000001;
    private const int FlagNotFocusable = 0x00000008;
    private const int FlagNotTouchable = 0x00000010;
    private const int FlagKeepScreenOn = 0x00000080;
    private const int FlagShowWhenLocked = 0x00080000;
    private const int FlagTurnScreenOn = 0x00200000;
    private const int FlagDismissKeyguard = 0x00400000;

    private static AndroidKeyguardDiagnostics instance;
    private static bool pauseState;
    private static bool lastSoftInputVisible;
    private static bool hasSoftInputSample;

    private Coroutine delayedRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
        {
            return;
        }

        GameObject go = new GameObject("AndroidKeyguardDiagnostics");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<AndroidKeyguardDiagnostics>();
        Debug.Log(KeyguardTag + " bootstrap created (DEVELOPMENT_BUILD Android only)");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        CaptureSnapshot("startup");
        ScheduleDelayed("startup+delay");
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        CaptureSnapshot(hasFocus ? "focus=true" : "focus=false");
        if (hasFocus)
        {
            ScheduleDelayed("focus=true+delay");
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        pauseState = pauseStatus;
        CaptureSnapshot(pauseStatus ? "pause=true" : "pause=false");
        if (!pauseStatus)
        {
            ScheduleDelayed("pause=false+delay");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CaptureSnapshot("sceneLoaded:" + scene.name);
        ScheduleDelayed("sceneLoaded+delay:" + scene.name);
    }

    private void ScheduleDelayed(string reason)
    {
        if (delayedRoutine != null)
        {
            StopCoroutine(delayedRoutine);
        }

        delayedRoutine = StartCoroutine(DelayedCapture(reason));
    }

    private IEnumerator DelayedCapture(string reason)
    {
        yield return new WaitForSecondsRealtime(DelayedSnapshotSeconds);
        CaptureSnapshot(reason);
        delayedRoutine = null;
    }

    private static void CaptureSnapshot(string reason)
    {
        try
        {
            LogSoftInputDelta(reason);
            LogKeyguard(reason);
            LogWindowFlags();
            LogInput();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(
                KeyguardTag + " snapshot failed reason=" + reason + " err=" + ex.Message);
        }
    }

    private static void LogSoftInputDelta(string reason)
    {
        bool visible = false;
        try
        {
            visible = TouchScreenKeyboard.visible;
        }
        catch
        {
            visible = false;
        }

        if (!hasSoftInputSample)
        {
            hasSoftInputSample = true;
            lastSoftInputVisible = visible;
            Debug.Log(
                SoftInputTag + "\n" +
                "reason=" + reason + "\n" +
                "touchScreenKeyboardVisible=" + visible + "\n" +
                "note=initial sample (Unity soft-input Dialog path only if keyboard opens)"
            );
            return;
        }

        if (visible == lastSoftInputVisible)
        {
            return;
        }

        bool wasVisible = lastSoftInputVisible;
        lastSoftInputVisible = visible;
        Debug.Log(
            SoftInputTag + "\n" +
            "reason=" + reason + "\n" +
            "transition=" + wasVisible + "->" + visible + "\n" +
            "note=Unity TouchScreenKeyboard visibility changed — capturing window/keyguard"
        );

        LogKeyguard(reason + ":softInputTransition");
        LogWindowFlags();
    }

    private static void LogKeyguard(string reason)
    {
        string activityName = "n/a";
        string keyguardLocked = "n/a";
        string deviceLocked = "n/a";
        string deviceSecure = "n/a";
        string interactive = "n/a";
        string showWhenLockedModern = "n/a";
        string turnScreenOnModern = "n/a";

        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject activity =
                   unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            if (activity == null)
            {
                Debug.LogWarning(KeyguardTag + " currentActivity=null reason=" + reason);
                return;
            }

            using (AndroidJavaObject activityClass = activity.Call<AndroidJavaObject>("getClass"))
            {
                activityName = activityClass.Call<string>("getName");
            }

            using (AndroidJavaObject context =
                       activity.Call<AndroidJavaObject>("getApplicationContext"))
            {
                using (AndroidJavaObject km =
                           context.Call<AndroidJavaObject>("getSystemService", "keyguard"))
                {
                    if (km != null)
                    {
                        keyguardLocked = BoolStr(SafeCallBool(km, "isKeyguardLocked"));
                        deviceLocked = BoolStr(SafeCallBool(km, "isDeviceLocked"));
                        deviceSecure = BoolStr(SafeCallBool(km, "isDeviceSecure"));
                    }
                }

                using (AndroidJavaObject pm =
                           context.Call<AndroidJavaObject>("getSystemService", "power"))
                {
                    if (pm != null)
                    {
                        interactive = BoolStr(SafeCallBool(pm, "isInteractive"));
                    }
                }
            }

            // Modern Activity APIs have setters but no public getters — best-effort reflection.
            showWhenLockedModern = TryReflectActivityBoolean(activity, "mShowWhenLocked");
            turnScreenOnModern = TryReflectActivityBoolean(activity, "mTurnScreenOn");
        }

        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log(
            KeyguardTag + "\n" +
            "reason=" + reason + "\n" +
            "focus=" + Application.isFocused + "\n" +
            "pause=" + pauseState + "\n" +
            "keyguardLocked=" + keyguardLocked + "\n" +
            "deviceLocked=" + deviceLocked + "\n" +
            "deviceSecure=" + deviceSecure + "\n" +
            "interactive=" + interactive + "\n" +
            "activity=" + activityName + "\n" +
            "scene=" + sceneName + "\n" +
            "timeScale=" + Time.timeScale.ToString("0.###") + "\n" +
            "showWhenLockedModern=" + showWhenLockedModern + "\n" +
            "turnScreenOnModern=" + turnScreenOnModern
        );
    }

    private static void LogWindowFlags()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject activity =
                   unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            if (activity == null)
            {
                return;
            }

            using (AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow"))
            using (AndroidJavaObject attrs = window.Call<AndroidJavaObject>("getAttributes"))
            {
                int flags = attrs.Get<int>("flags");
                Debug.Log(
                    WindowTag + "\n" +
                    "flags=0x" + flags.ToString("X8") + "\n" +
                    "SHOW_WHEN_LOCKED=" + FlagBit(flags, FlagShowWhenLocked) + "\n" +
                    "DISMISS_KEYGUARD=" + FlagBit(flags, FlagDismissKeyguard) + "\n" +
                    "TURN_SCREEN_ON=" + FlagBit(flags, FlagTurnScreenOn) + "\n" +
                    "KEEP_SCREEN_ON=" + FlagBit(flags, FlagKeepScreenOn) + "\n" +
                    "NOT_FOCUSABLE=" + FlagBit(flags, FlagNotFocusable) + "\n" +
                    "NOT_TOUCHABLE=" + FlagBit(flags, FlagNotTouchable) + "\n" +
                    "ALLOW_LOCK_WHILE_SCREEN_ON=" + FlagBit(flags, FlagAllowLockWhileScreenOn)
                );
            }
        }
    }

    private static void LogInput()
    {
        EventSystem es = EventSystem.current;
        bool esExists = es != null;
        bool esEnabled = esExists && es.enabled;

        Touchscreen ts = Touchscreen.current;
        bool tsExists = ts != null;
        int touchCount = 0;
        if (tsExists)
        {
            var touches = ts.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                if (touches[i].isInProgress)
                {
                    touchCount++;
                }
            }
        }

        string canAccept = "n/a";
        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            canAccept = gm.CanAcceptVehicleInput.ToString();
        }

        Debug.Log(
            InputTag + "\n" +
            "eventSystemExists=" + esExists + "\n" +
            "eventSystemEnabled=" + esEnabled + "\n" +
            "touchscreenExists=" + tsExists + "\n" +
            "activeTouches=" + touchCount + "\n" +
            "canAcceptVehicleInput=" + canAccept
        );
    }

    private static string TryReflectActivityBoolean(AndroidJavaObject activity, string fieldName)
    {
        AndroidJavaObject clazz = null;
        try
        {
            clazz = activity.Call<AndroidJavaObject>("getClass");
            while (clazz != null)
            {
                AndroidJavaObject field = null;
                try
                {
                    field = clazz.Call<AndroidJavaObject>("getDeclaredField", fieldName);
                }
                catch
                {
                    field = null;
                }

                if (field != null)
                {
                    try
                    {
                        field.Call("setAccessible", true);
                        bool value = field.Call<bool>("getBoolean", activity);
                        return value ? "true" : "false";
                    }
                    finally
                    {
                        field.Dispose();
                    }
                }

                AndroidJavaObject parent = null;
                try
                {
                    parent = clazz.Call<AndroidJavaObject>("getSuperclass");
                }
                catch
                {
                    parent = null;
                }

                clazz.Dispose();
                clazz = parent;
            }

            return "unavailable";
        }
        catch (System.Exception ex)
        {
            return "unavailable:" + ex.GetType().Name;
        }
        finally
        {
            if (clazz != null)
            {
                clazz.Dispose();
            }
        }
    }

    private static bool? SafeCallBool(AndroidJavaObject obj, string method)
    {
        try
        {
            return obj.Call<bool>(method);
        }
        catch
        {
            return null;
        }
    }

    private static string BoolStr(bool? value)
    {
        return value.HasValue ? (value.Value ? "true" : "false") : "n/a";
    }

    private static string FlagBit(int flags, int bit)
    {
        return ((flags & bit) != 0).ToString();
    }
}
#endif

using System.Threading.Tasks;
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public enum NotificationOptInResult
{
    Disabled = 0,
    Enabled = 1,
    NeedsSettings = 2
}

public enum AndroidNotificationPermissionStatus
{
    Granted = 0,
    CanRequest = 1,
    NeedsSettings = 2
}

/// <summary>
/// Android 13+ POST_NOTIFICATIONS runtime permission helper.
/// One shared OS permission for Daily (FCM) and Lives (local alarms).
/// Never prompts at startup. A request happens only after an explicit user action.
/// </summary>
public static class AndroidNotificationPermission
{
    public const string PostNotifications = "android.permission.POST_NOTIFICATIONS";
    private const string AttemptedKey = "RushOut_PostNotificationsRequested";
    private const string BlockedKey = "RushOut_PostNotificationsDontAskAgain";

    public static bool IsRuntimePermissionRequired
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return GetAndroidSdkInt() >= 33;
#else
            return false;
#endif
        }
    }

    public static bool HasPermission
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!IsRuntimePermissionRequired)
            {
                return true;
            }

            return Permission.HasUserAuthorizedPermission(PostNotifications);
#else
            // Editor / non-Android: treat as not granted for opt-in UI honesty,
            // but IsRuntimePermissionRequired is false so enable flow skips the dialog.
            return !IsRuntimePermissionRequired;
#endif
        }
    }

    public static AndroidNotificationPermissionStatus GetStatus()
    {
#if UNITY_EDITOR
        return AndroidNotificationPermissionStatus.Granted;
#elif UNITY_ANDROID
        if (!IsRuntimePermissionRequired || HasPermission)
        {
            return AndroidNotificationPermissionStatus.Granted;
        }

        if (PlayerPrefs.GetInt(BlockedKey, 0) == 1)
        {
            return AndroidNotificationPermissionStatus.NeedsSettings;
        }

        bool attempted = PlayerPrefs.GetInt(AttemptedKey, 0) == 1;
        if (attempted && !ShouldShowRationale())
        {
            return AndroidNotificationPermissionStatus.NeedsSettings;
        }

        return AndroidNotificationPermissionStatus.CanRequest;
#else
        return AndroidNotificationPermissionStatus.NeedsSettings;
#endif
    }

    /// <summary>
    /// Requests POST_NOTIFICATIONS on Android 13+ only when the OS can still show the dialog.
    /// Returns whether notifications may proceed. Does not open system settings.
    /// </summary>
    public static async Task<bool> RequestIfNeededAsync()
    {
#if UNITY_EDITOR
        await Task.Yield();
        return false;
#elif UNITY_ANDROID
        if (!IsRuntimePermissionRequired || HasPermission)
        {
            return true;
        }

        if (GetStatus() == AndroidNotificationPermissionStatus.NeedsSettings)
        {
            return false;
        }

        var tcs = new TaskCompletionSource<bool>();
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionGranted += _ =>
        {
            MarkAttempted();
            tcs.TrySetResult(true);
        };
        callbacks.PermissionDenied += _ =>
        {
            MarkAttempted();
            tcs.TrySetResult(false);
        };
        callbacks.PermissionDeniedAndDontAskAgain += _ =>
        {
            MarkAttempted();
            PlayerPrefs.SetInt(BlockedKey, 1);
            PlayerPrefs.Save();
            tcs.TrySetResult(false);
        };
        Permission.RequestUserPermission(PostNotifications, callbacks);

        // Timeout so a stuck dialog never freezes Settings forever.
        Task delay = Task.Delay(60000);
        Task finished = await Task.WhenAny(tcs.Task, delay);
        if (finished != tcs.Task)
        {
            return Permission.HasUserAuthorizedPermission(PostNotifications);
        }

        return await tcs.Task;
#else
        await Task.Yield();
        // Future Apple: use FirebaseMessaging.RequestPermissionAsync after APNs setup.
        return false;
#endif
    }

    /// <summary>Opens this app's Android system settings. Only call from an explicit button.</summary>
    public static void OpenAppSettings()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
            using (AndroidJavaObject intent = new AndroidJavaObject(
                       "android.content.Intent",
                       "android.settings.APPLICATION_DETAILS_SETTINGS"))
            {
                if (activity == null)
                {
                    return;
                }

                string packageName = activity.Call<string>("getPackageName");
                using (AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>(
                           "fromParts",
                           "package",
                           packageName,
                           null))
                {
                    intent.Call<AndroidJavaObject>("setData", uri);
                    activity.Call("startActivity", intent);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[Notifications] Open settings failed: " + ex.GetType().Name);
        }
#endif
    }

    private static void MarkAttempted()
    {
        PlayerPrefs.SetInt(AttemptedKey, 1);
        PlayerPrefs.Save();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static bool ShouldShowRationale()
    {
        try
        {
            return Permission.ShouldShowRequestPermissionRationale(PostNotifications);
        }
        catch
        {
            return false;
        }
    }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    private static int GetAndroidSdkInt()
    {
        try
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }
        catch
        {
            return 0;
        }
    }
#endif
}

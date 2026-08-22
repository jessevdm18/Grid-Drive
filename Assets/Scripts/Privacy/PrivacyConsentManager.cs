using System;
using System.Collections.Generic;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

/// <summary>
/// Central UMP privacy/consent lifecycle owner for ads.
/// Analytics collection is a separate app-level policy (not driven by CanRequestAds).
/// Gameplay never waits on this manager.
///
/// UMP callbacks are NOT guaranteed on the Unity main thread. All consent-form
/// completions are marshalled onto Update before touching PlayerPrefs / Firebase /
/// downstream listeners.
/// </summary>
public class PrivacyConsentManager : MonoBehaviour
{
    private const string AnalyticsConsentPrefsKey = "RushOut_AnalyticsConsent";
    private const string AnalyticsConsentUnknown = "Unknown";
    private const string AnalyticsConsentEnabled = "Enabled";
    private const string AnalyticsConsentDisabled = "Disabled";

    /// <summary>
    /// App-level Firebase Analytics collection policy.
    /// Independent from UMP / CanRequestAds (ad-request status).
    /// </summary>
    public enum AnalyticsConsentMode
    {
        /// <summary>v1 default: Analytics collection stays off.</summary>
        Disabled = 0,

        /// <summary>Always enable Analytics collection (requires legal/product approval).</summary>
        Enabled = 1,

        /// <summary>Follow persisted RushOut_AnalyticsConsent preference.</summary>
        ExplicitUserChoice = 2
    }

    public static PrivacyConsentManager Instance { get; private set; }

    /// <summary>True after Update (+ optional form) finished or Editor fallback resolved.</summary>
    public static bool IsResolved { get; private set; }

    /// <summary>Mirror of ConsentInformation.CanRequestAds after resolve (Editor: true).</summary>
    public static bool CanRequestAds { get; private set; }

    /// <summary>True when UMP says privacy options entry point is required.</summary>
    public static bool PrivacyOptionsRequired { get; private set; }

    /// <summary>Last known ConsentStatus snapshot.</summary>
    public static ConsentStatus LastConsentStatus { get; private set; } = ConsentStatus.Unknown;

    /// <summary>
    /// Analytics policy. Default Disabled — UMP resolve does NOT enable Analytics.
    /// </summary>
    public static AnalyticsConsentMode AnalyticsMode { get; set; } =
        AnalyticsConsentMode.Disabled;

    /// <summary>
    /// Effective Analytics collection preference for Settings / ExplicitUserChoice.
    /// Unknown preference is treated as disabled.
    /// </summary>
    public static bool AnalyticsEnabled
    {
        get { return EvaluateAnalyticsCollectionEnabled(); }
        set { SetAnalyticsConsent(value); }
    }

    /// <summary>Fired once when consent flow completes (success, skip, or fail-soft).</summary>
    public static event Action OnConsentResolved;

    private static bool flowStarted;
    private static bool resolvedEventRaised;
    private static bool analyticsPolicyRestored;

    /// <summary>
    /// In-memory mirror of RushOut_AnalyticsConsent. Avoids PlayerPrefs during
    /// EvaluateAnalyticsCollectionEnabled on non-main / loading threads.
    /// </summary>
    private static string cachedAnalyticsConsentPreference = AnalyticsConsentUnknown;

    /// <summary>
    /// Captured only from Awake / Update (true Unity main thread). Never from
    /// SubsystemRegistration — that can run off the player-loop thread on device.
    /// </summary>
    private static int mainThreadId = -1;

    private static readonly Queue<Action> mainThreadQueue = new Queue<Action>(8);
    private static readonly object mainThreadQueueLock = new object();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>Dev-only: force EEA debug geography on next Update.</summary>
    public static bool DebugForceEeaGeography { get; set; }

    /// <summary>Dev-only hashed test device ids for UMP debug.</summary>
    public static List<string> DebugTestDeviceHashedIds { get; set; }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
        IsResolved = false;
        CanRequestAds = false;
        PrivacyOptionsRequired = false;
        LastConsentStatus = ConsentStatus.Unknown;
        flowStarted = false;
        resolvedEventRaised = false;
        analyticsPolicyRestored = false;
        cachedAnalyticsConsentPreference = AnalyticsConsentUnknown;
        // Do NOT capture mainThreadId here — SubsystemRegistration is not reliable.
        mainThreadId = -1;
        lock (mainThreadQueueLock)
        {
            mainThreadQueue.Clear();
        }

        OnConsentResolved = null;
        AnalyticsMode = AnalyticsConsentMode.Disabled;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugForceEeaGeography = false;
        DebugTestDeviceHashedIds = null;
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapAfterSceneLoad()
    {
        EnsureInstance();
    }

    public static PrivacyConsentManager EnsureInstance()
    {
        // Do NOT read PlayerPrefs here — EnsureInstance may be reached while a scene
        // is still activating. Preference restore happens in Awake / explicit restore.
        if (Instance != null)
        {
            return Instance;
        }

        PrivacyConsentManager existing = FindAnyObjectByType<PrivacyConsentManager>();
        if (existing != null)
        {
            Instance = existing;
            if (!flowStarted)
            {
                existing.BeginConsentFlow();
            }

            return Instance;
        }

        GameObject go = new GameObject("PrivacyConsentManager");
        go.AddComponent<PrivacyConsentManager>();
        return Instance;
    }

    private static void CaptureMainThreadIdFromPlayerLoop()
    {
        mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
    }

    private static bool IsMainThread()
    {
        if (mainThreadId < 0)
        {
            return false;
        }

        return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;
    }

    /// <summary>
    /// Public main-thread marshal used by privacy / ads callback paths.
    /// Invokes immediately when already on main; otherwise queues for Update.
    /// </summary>
    public static void RunOnUnityMainThread(Action action)
    {
        if (action == null)
        {
            return;
        }

        if (IsMainThread())
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError("[Privacy] MainThread action exception — " + ex.Message);
            }

            return;
        }

        lock (mainThreadQueueLock)
        {
            mainThreadQueue.Enqueue(action);
        }
    }

    private void Update()
    {
        if (mainThreadId < 0)
        {
            CaptureMainThreadIdFromPlayerLoop();
        }

        PumpMainThreadQueue();
    }

    private static void PumpMainThreadQueue()
    {
        while (true)
        {
            Action action = null;
            lock (mainThreadQueueLock)
            {
                if (mainThreadQueue.Count == 0)
                {
                    break;
                }

                action = mainThreadQueue.Dequeue();
            }

            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError("[Privacy] MainThread queue exception — " + ex.Message);
            }
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private static void LogThreadAudit(string method)
    {
        bool isMain = IsMainThread();
        int currentId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        Debug.Log(
            "[ThreadAudit]\n" +
            "Method=" + method + "\n" +
            "MainThread=" + isMain + "\n" +
            "CurrentThreadId=" + currentId + "\n" +
            "MainThreadId=" + mainThreadId
        );

        if (!isMain)
        {
            Debug.LogWarning(
                "[ThreadAudit] OFF-MAIN stack:\n" + System.Environment.StackTrace
            );
        }
    }

    private static void LogPrivacyThread(string stage)
    {
        Debug.Log(
            "[PrivacyThread]\n" +
            "Stage=" + stage + "\n" +
            "MainThread=" + IsMainThread() + "\n" +
            "CurrentThreadId=" +
            System.Threading.Thread.CurrentThread.ManagedThreadId + "\n" +
            "MainThreadId=" + mainThreadId
        );
    }
#endif

    /// <summary>
    /// If a prior ExplicitUserChoice was persisted, restore that mode after static reset.
    /// v1 default (no key) stays Disabled. Main-thread only — never call from field init.
    /// </summary>
    private static void RestoreAnalyticsPolicyFromPersistence()
    {
        if (analyticsPolicyRestored)
        {
            return;
        }

        if (!IsMainThread())
        {
            LogPrivacy("AnalyticsRestoreDeferred", "Reason=NotMainThread");
            RunOnUnityMainThread(RestoreAnalyticsPolicyFromPersistence);
            return;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LogThreadAudit("RestoreAnalyticsPolicyFromPersistence");
#endif

        analyticsPolicyRestored = true;
        cachedAnalyticsConsentPreference = ReadAnalyticsConsentPreferenceFromDisk();
        if (cachedAnalyticsConsentPreference == AnalyticsConsentEnabled ||
            cachedAnalyticsConsentPreference == AnalyticsConsentDisabled)
        {
            AnalyticsMode = AnalyticsConsentMode.ExplicitUserChoice;
        }

        LogPrivacy(
            "AnalyticsPreferenceRestored",
            "Choice=" + cachedAnalyticsConsentPreference + " Mode=" + AnalyticsMode
        );
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CaptureMainThreadIdFromPlayerLoop();
        RestoreAnalyticsPolicyFromPersistence();

        if (!flowStarted)
        {
            BeginConsentFlow();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Starts ConsentInformation.Update and shows the form only when UMP requires it.
    /// Failures resolve soft — gameplay continues.
    /// </summary>
    public void BeginConsentFlow()
    {
        if (flowStarted)
        {
            return;
        }

        flowStarted = true;

#if UNITY_EDITOR
        ResolveEditorFallback();
#else
        RunMobileConsentFlow();
#endif
    }

    /// <summary>
    /// Opens UMP privacy options when required. Safe no-op otherwise.
    /// Wire from Settings via MainMenuSettingsUI.OnPrivacyOptionsButton().
    /// </summary>
    public static void ShowPrivacyOptions(Action onComplete = null)
    {
        EnsureInstance();

#if UNITY_EDITOR
        LogPrivacy("PrivacyOptionsSkipped", "Reason=EditorFallback");
        onComplete?.Invoke();
        return;
#else
        if (!IsResolved)
        {
            LogPrivacy("PrivacyOptionsDeferred", "Reason=ConsentNotResolved");
            Action handler = null;
            handler = () =>
            {
                OnConsentResolved -= handler;
                ShowPrivacyOptions(onComplete);
            };
            OnConsentResolved += handler;
            return;
        }

        if (ConsentInformation.PrivacyOptionsRequirementStatus !=
            PrivacyOptionsRequirementStatus.Required)
        {
            LogPrivacy(
                "PrivacyOptionsNotRequired",
                "Status=" + ConsentInformation.PrivacyOptionsRequirementStatus
            );
            onComplete?.Invoke();
            return;
        }

        LogPrivacy("PrivacyOptionsShowStarted");
        ConsentForm.ShowPrivacyOptionsForm(error =>
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogPrivacyThread("PrivacyOptionsCallback");
#endif
            // UMP may invoke this off the Unity main thread — marshal before PlayerPrefs.
            RunOnUnityMainThread(() =>
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                LogPrivacyThread("PrivacyOptionsCallback_Marshalled");
                LogThreadAudit("ShowPrivacyOptions.OnComplete");
#endif
                if (error != null)
                {
                    LogPrivacy(
                        "PrivacyOptionsError",
                        "Code=" + error.ErrorCode + " Message=" + error.Message
                    );
                }
                else
                {
                    LogPrivacy("PrivacyOptionsDismissed");
                }

                RefreshConsentSnapshot();
                ApplyDownstreamPolicies("PrivacyOptionsClosed");
                onComplete?.Invoke();
            });
        });
#endif
    }

#if !UNITY_EDITOR
    private void RunMobileConsentFlow()
    {
        LogPrivacy("Stage=UpdateStarted");

        ConsentRequestParameters request = BuildRequestParameters();

        try
        {
            ConsentInformation.Update(request, OnConsentInfoUpdated);
        }
        catch (Exception ex)
        {
            LogPrivacy("UpdateException", "Message=" + ex.Message);
            FailSoftResolve("UpdateException");
        }
    }

    private ConsentRequestParameters BuildRequestParameters()
    {
        var request = new ConsentRequestParameters
        {
            TagForUnderAgeOfConsent = false
        };

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (DebugForceEeaGeography ||
            (DebugTestDeviceHashedIds != null && DebugTestDeviceHashedIds.Count > 0))
        {
            var debug = new ConsentDebugSettings();
            if (DebugForceEeaGeography)
            {
                debug.DebugGeography = DebugGeography.EEA;
            }

            if (DebugTestDeviceHashedIds != null && DebugTestDeviceHashedIds.Count > 0)
            {
                debug.TestDeviceHashedIds = DebugTestDeviceHashedIds;
            }

            request.ConsentDebugSettings = debug;
            LogPrivacy(
                "DebugSettingsApplied",
                "ForceEea=" + DebugForceEeaGeography +
                " TestDevices=" + DebugTestDeviceHashedIds.Count
            );
        }
#endif

        return request;
    }

    private void OnConsentInfoUpdated(FormError updateError)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LogPrivacyThread("UpdateCallback");
#endif
        RunOnUnityMainThread(() =>
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogPrivacyThread("UpdateCallback_Marshalled");
            LogThreadAudit("OnConsentInfoUpdated");
#endif
            if (updateError != null)
            {
                LogPrivacy(
                    "UpdateFailed",
                    "Code=" + updateError.ErrorCode + " Message=" + updateError.Message
                );
                RefreshConsentSnapshot();
                CompleteResolve("UpdateFailed");
                return;
            }

            RefreshConsentSnapshot();
            LogPrivacy("ConsentStatus=" + LastConsentStatus);
            LogPrivacy("FormAvailable=" + ConsentInformation.IsConsentFormAvailable());

            try
            {
                ConsentForm.LoadAndShowConsentFormIfRequired(OnConsentFormDismissed);
            }
            catch (Exception ex)
            {
                LogPrivacy("FormShowException", "Message=" + ex.Message);
                FailSoftResolve("FormShowException");
            }
        });
    }

    private void OnConsentFormDismissed(FormError formError)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LogPrivacyThread("ConsentFormCallback");
#endif
        RunOnUnityMainThread(() =>
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogPrivacyThread("ConsentFormCallback_Marshalled");
            LogThreadAudit("OnConsentFormDismissed");
#endif
            if (formError != null)
            {
                LogPrivacy(
                    "FormError",
                    "Code=" + formError.ErrorCode + " Message=" + formError.Message
                );
            }
            else
            {
                LogPrivacy("FormShownOrNotRequired");
            }

            RefreshConsentSnapshot();
            CompleteResolve(formError != null ? "FormError" : "FormComplete");
        });
    }
#endif

#if UNITY_EDITOR
    private void ResolveEditorFallback()
    {
        IsResolved = true;
        CanRequestAds = true;
        PrivacyOptionsRequired = false;
        LastConsentStatus = ConsentStatus.NotRequired;
        LogPrivacy("EditorFallback=True");
        LogPrivacy("Resolved=True");
        LogPrivacy("CanRequestAds=True");
        LogPrivacy("PrivacyOptionsRequired=False");
        CompleteResolve("EditorFallback");
    }
#endif

    private void FailSoftResolve(string reason)
    {
        RefreshConsentSnapshot();
        CompleteResolve(reason);
    }

    private void CompleteResolve(string reason)
    {
        if (!IsMainThread())
        {
            RunOnUnityMainThread(() => CompleteResolve(reason));
            return;
        }

        IsResolved = true;
        LogPrivacy("Resolved=True", "Reason=" + reason);
        LogPrivacy("CanRequestAds=" + CanRequestAds);
        LogPrivacy("PrivacyOptionsRequired=" + PrivacyOptionsRequired);
        LogPrivacy("ConsentStatus=" + LastConsentStatus);

        ApplyDownstreamPolicies(reason);

        if (!resolvedEventRaised)
        {
            resolvedEventRaised = true;
            try
            {
                OnConsentResolved?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[Privacy] OnConsentResolved listener exception — " + ex.Message
                );
            }
        }
    }

    private static void RefreshConsentSnapshot()
    {
#if UNITY_EDITOR
        CanRequestAds = true;
        PrivacyOptionsRequired = false;
        LastConsentStatus = ConsentStatus.NotRequired;
#else
        try
        {
            LastConsentStatus = ConsentInformation.ConsentStatus;
            CanRequestAds = ConsentInformation.CanRequestAds();
            PrivacyOptionsRequired =
                ConsentInformation.PrivacyOptionsRequirementStatus ==
                PrivacyOptionsRequirementStatus.Required;
        }
        catch (Exception ex)
        {
            LogPrivacy("SnapshotException", "Message=" + ex.Message);
            CanRequestAds = false;
            PrivacyOptionsRequired = false;
            LastConsentStatus = ConsentStatus.Unknown;
        }
#endif
    }

    private static void ApplyDownstreamPolicies(string reason)
    {
        if (!IsMainThread())
        {
            RunOnUnityMainThread(() => ApplyDownstreamPolicies(reason));
            return;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LogThreadAudit("ApplyDownstreamPolicies");
#endif

        // Ads gate stays UMP-only. Analytics is independent and does NOT follow CanRequestAds.
        bool analyticsEnabled = EvaluateAnalyticsCollectionEnabled();
        LogPrivacy("AdsCanRequest=" + CanRequestAds, "Reason=" + reason);
        LogPrivacy("AnalyticsMode=" + AnalyticsMode);
        LogPrivacy("AnalyticsCollectionEnabled=" + analyticsEnabled);
        LogPrivacy(
            "CrashlyticsCollectionEnabled=" +
            FirebaseManager.GetCrashlyticsCollectionEnabledOrUnknown()
        );
        FirebaseManager.ApplyAnalyticsConsentPolicy(analyticsEnabled);
    }

    /// <summary>
    /// Explicit Analytics consent API (Settings / future UI).
    /// Switches mode to ExplicitUserChoice, persists preference, applies to Firebase when ready.
    /// Not a substitute for UMP ad consent.
    /// </summary>
    public static void SetAnalyticsConsent(bool enabled)
    {
        SaveAnalyticsConsentPreference(enabled);
        bool effective = EvaluateAnalyticsCollectionEnabled();
        LogPrivacy(
            "SetAnalyticsConsent",
            "Requested=" + enabled + " Effective=" + effective
        );
        FirebaseManager.ApplyAnalyticsConsentPolicy(effective);
    }

    /// <summary>Settings-friendly alias for <see cref="SetAnalyticsConsent"/>.</summary>
    public static void SetAnalyticsEnabled(bool enabled)
    {
        SetAnalyticsConsent(enabled);
    }

    /// <summary>
    /// Effective Analytics collection flag from AnalyticsMode + preference.
    /// Never uses CanRequestAds.
    /// </summary>
    public static bool EvaluateAnalyticsCollectionEnabled()
    {
        // Prefer in-memory cache. Restore from disk only on the main thread.
        if (!analyticsPolicyRestored)
        {
            if (IsMainThread())
            {
                RestoreAnalyticsPolicyFromPersistence();
            }
            else
            {
                // Do not touch PlayerPrefs here — schedule restore for Update.
                RunOnUnityMainThread(RestoreAnalyticsPolicyFromPersistence);
            }
        }

        switch (AnalyticsMode)
        {
            case AnalyticsConsentMode.Enabled:
                return true;
            case AnalyticsConsentMode.ExplicitUserChoice:
                return cachedAnalyticsConsentPreference == AnalyticsConsentEnabled;
            case AnalyticsConsentMode.Disabled:
            default:
                return false;
        }
    }

    private static void SaveAnalyticsConsentPreference(bool enabled)
    {
        string value = enabled ? AnalyticsConsentEnabled : AnalyticsConsentDisabled;
        cachedAnalyticsConsentPreference = value;
        analyticsPolicyRestored = true;
        AnalyticsMode = AnalyticsConsentMode.ExplicitUserChoice;

        if (!IsMainThread())
        {
            LogPrivacy(
                "AnalyticsPreferenceCacheOnly",
                "Reason=NotMainThread Value=" + value
            );
            RunOnUnityMainThread(() =>
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                LogThreadAudit("SaveAnalyticsConsentPreference.PlayerPrefs");
#endif
                PlayerPrefs.SetString(AnalyticsConsentPrefsKey, value);
                PlayerPrefs.Save();
            });
            return;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LogThreadAudit("SaveAnalyticsConsentPreference");
#endif
        PlayerPrefs.SetString(AnalyticsConsentPrefsKey, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Returns Unknown / Enabled / Disabled for RushOut_AnalyticsConsent.
    /// Uses in-memory cache after restore; disk read is main-thread only.
    /// </summary>
    public static string GetAnalyticsConsentPreference()
    {
        if (!analyticsPolicyRestored)
        {
            if (IsMainThread())
            {
                RestoreAnalyticsPolicyFromPersistence();
            }
            else
            {
                RunOnUnityMainThread(RestoreAnalyticsPolicyFromPersistence);
            }
        }

        return cachedAnalyticsConsentPreference;
    }

    private static string ReadAnalyticsConsentPreferenceFromDisk()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LogThreadAudit("ReadAnalyticsConsentPreferenceFromDisk.HasKey");
#endif
        // Exact HasKey site that threw off-main before UMP marshalling.
        if (!PlayerPrefs.HasKey(AnalyticsConsentPrefsKey))
        {
            return AnalyticsConsentUnknown;
        }

        string value = PlayerPrefs.GetString(AnalyticsConsentPrefsKey, AnalyticsConsentUnknown);
        if (value == AnalyticsConsentEnabled || value == AnalyticsConsentDisabled)
        {
            return value;
        }

        return AnalyticsConsentUnknown;
    }

    private static string LoadAnalyticsConsentPreference()
    {
        return GetAnalyticsConsentPreference();
    }

    /// <summary>
    /// Clears app-level Analytics preference back to Unknown and disables collection.
    /// Does NOT reset UMP ad consent.
    /// </summary>
    public static void ResetAnalyticsConsentChoice()
    {
        cachedAnalyticsConsentPreference = AnalyticsConsentUnknown;
        AnalyticsMode = AnalyticsConsentMode.Disabled;
        analyticsPolicyRestored = true;

        if (IsMainThread())
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogThreadAudit("ResetAnalyticsConsentChoice.HasKey");
#endif
            if (PlayerPrefs.HasKey(AnalyticsConsentPrefsKey))
            {
                PlayerPrefs.DeleteKey(AnalyticsConsentPrefsKey);
                PlayerPrefs.Save();
            }
        }
        else
        {
            RunOnUnityMainThread(() =>
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                LogThreadAudit("ResetAnalyticsConsentChoice.HasKey_Marshalled");
#endif
                if (PlayerPrefs.HasKey(AnalyticsConsentPrefsKey))
                {
                    PlayerPrefs.DeleteKey(AnalyticsConsentPrefsKey);
                    PlayerPrefs.Save();
                }
            });
        }

        FirebaseManager.ApplyAnalyticsConsentPolicy(false);
        LogPrivacy("AnalyticsConsentReset", "Choice=Unknown Collection=False");
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>Dev-only: reset UMP consent state (mobile/player only; Editor no-op).</summary>
    public static void DebugResetConsent()
    {
#if UNITY_EDITOR
        LogPrivacy("DebugResetSkipped", "Reason=Editor");
#else
        try
        {
            ConsentInformation.Reset();
            LogPrivacy("DebugResetDone");
        }
        catch (Exception ex)
        {
            LogPrivacy("DebugResetFailed", "Message=" + ex.Message);
        }
#endif
    }
#endif

    private static void LogPrivacy(string stage, string detail = null)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (string.IsNullOrEmpty(detail))
        {
            Debug.Log("[Privacy] " + stage);
        }
        else
        {
            Debug.Log("[Privacy] " + stage + " " + detail);
        }
#endif
    }
}

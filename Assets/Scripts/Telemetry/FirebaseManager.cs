using System;
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.Extensions;
using UnityEngine;

/// <summary>
/// Central Firebase runtime owner. Telemetry-only: never required for gameplay.
/// Creation is owned by <see cref="FirebaseRuntimeBootstrap"/> (no scene YAML).
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    /// <summary>True only after DependencyStatus.Available and DefaultInstance usable.</summary>
    public static bool IsReady { get; private set; }

    /// <summary>Last init stage label for Editor diagnostics.</summary>
    public static string InitStage { get; private set; } = "None";

    /// <summary>Last failure reason, if any.</summary>
    public static string LastFailureReason { get; private set; } = string.Empty;

    /// <summary>Last DependencyStatus from CheckAndFixDependenciesAsync, if completed.</summary>
    public static string LastDependencyStatus { get; private set; } = string.Empty;

    /// <summary>Debug: whether BeginInit has been started this Play session.</summary>
    public static bool DebugInitStarted => initStarted;

    /// <summary>Debug: EnsureInstance is currently adding the component.</summary>
    public static bool DebugCreatingRuntimeInstance => creatingRuntimeInstance;

    /// <summary>
    /// True while EnsureInstance is adding the component (Awake runs inline).
    /// </summary>
    private static bool creatingRuntimeInstance;

    private static bool initStarted;

    private static string lastNonFatalMessage;
    private static float lastNonFatalUnscaledTime;

    /// <summary>
    /// Clears Play-Mode statics. Safe to call multiple times (SubsystemRegistration + BeforeSceneLoad).
    /// </summary>
    public static void ResetStaticsForPlayMode()
    {
        Instance = null;
        IsReady = false;
        initStarted = false;
        creatingRuntimeInstance = false;
        InitStage = "None";
        LastFailureReason = string.Empty;
        LastDependencyStatus = string.Empty;
        lastNonFatalMessage = null;
        lastNonFatalUnscaledTime = 0f;
        pendingAnalyticsCollectionEnabled = null;
        lastAppliedAnalyticsCollectionEnabled = null;
        LogInit("StaticStateReset");
    }

    /// <summary>
    /// Returns existing DDOL instance, or creates one.
    /// </summary>
    public static FirebaseManager EnsureInstance()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[FirebaseInit] Stage=EnsureInstanceEntered" +
            " instanceExists=" + (Instance != null) +
            " isPlaying=" + Application.isPlaying
        );
#endif

        if (Instance != null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[FirebaseInit] BootstrapEarlyReturn Reason=instance_already_exists");
#endif
            return Instance;
        }

        FirebaseManager existing = UnityEngine.Object.FindAnyObjectByType<FirebaseManager>();
        if (existing != null)
        {
            Instance = existing;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                "[FirebaseInit] BootstrapEarlyReturn Reason=found_existing_in_scene name=" +
                existing.name
            );
#endif
            return existing;
        }

        LogInit("CreatingManager");
        GameObject go = new GameObject("FirebaseManager");
        creatingRuntimeInstance = true;
        try
        {
            go.AddComponent<FirebaseManager>();
        }
        finally
        {
            creatingRuntimeInstance = false;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[FirebaseInit] Stage=ManagerComponentAdded" +
            " instanceExists=" + (Instance != null)
        );
#endif

        return Instance;
    }

    private void Awake()
    {
        LogInit("Awake");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[FirebaseInit] Stage=AwakeDetails" +
            " creatingRuntimeInstance=" + creatingRuntimeInstance +
            " initStarted=" + initStarted +
            " isPlaying=" + Application.isPlaying
        );
#endif

        if (Instance != null && Instance != this)
        {
            LogInit(
                "DuplicateDestroyed",
                "creatingRuntimeInstance=" + creatingRuntimeInstance
            );
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (initStarted)
        {
            LogInit(
                "InitializeSkipped",
                "Reason=initStarted_already_true"
            );
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                "[FirebaseInit] BootstrapEarlyReturn Reason=initStarted_already_true"
            );
#endif
            return;
        }

        initStarted = true;
        BeginInit();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void BeginInit()
    {
        IsReady = false;
        LastFailureReason = string.Empty;
        LastDependencyStatus = string.Empty;
        LogInit("InitializeStarted");
        LogInit("DependencyCheckStarted");

        try
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                try
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Exception ex = task.Exception != null
                            ? task.Exception.Flatten().InnerException ?? task.Exception
                            : null;
                        string reason = ex != null ? ex.Message : "task faulted/canceled";
                        LastFailureReason = reason;
                        LogInit("Failed", "Reason=" + reason);
                        LogInitException(ex);
                        IsReady = false;
                        return;
                    }

                    DependencyStatus status = task.Result;
                    LastDependencyStatus = status.ToString();
                    LogInit("DependencyCheckCompleted", "DependencyStatus=" + status);

                    if (status != DependencyStatus.Available)
                    {
                        LastFailureReason = "DependencyStatus=" + status;
                        LogInit("Failed", "Reason=" + LastFailureReason);
                        IsReady = false;
                        return;
                    }

                    // "Database URL not set" is harmless (Realtime Database unused).
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                    bool defaultOk = app != null;
                    LogInit(
                        "DefaultInstanceChecked",
                        "DefaultInstance!=null=" + defaultOk +
                        (defaultOk ? " AppName=" + app.Name : string.Empty)
                    );

                    if (!defaultOk)
                    {
                        LastFailureReason = "DefaultInstance null after Available";
                        LogInit("Failed", "Reason=" + LastFailureReason);
                        IsReady = false;
                        return;
                    }

                    TryEnableAnalytics();
                    TryEnableCrashlytics();

                    IsReady = true;
                    LogInit("Ready", "AppName=" + app.Name);
                }
                catch (Exception ex)
                {
                    LastFailureReason = ex.Message;
                    LogInit("Failed", "Reason=" + ex.Message);
                    LogInitException(ex);
                    IsReady = false;
                }
            });
        }
        catch (Exception ex)
        {
            LastFailureReason = ex.Message;
            LogInit(
                "Failed",
                "Reason=could not start CheckAndFixDependenciesAsync: " + ex.Message
            );
            LogInitException(ex);
            IsReady = false;
        }
    }

    private static bool? pendingAnalyticsCollectionEnabled;
    private static bool? lastAppliedAnalyticsCollectionEnabled;

    private static void TryEnableAnalytics()
    {
        // Default: keep Analytics collection disabled until explicit policy says otherwise.
        // UMP / CanRequestAds must NEVER auto-enable Analytics.
        try
        {
            bool desired = pendingAnalyticsCollectionEnabled.HasValue
                ? pendingAnalyticsCollectionEnabled.Value
                : PrivacyConsentManager.EvaluateAnalyticsCollectionEnabled();

            FirebaseAnalytics.SetAnalyticsCollectionEnabled(desired);
            lastAppliedAnalyticsCollectionEnabled = desired;
            LogInit(
                "AnalyticsCollection=" + desired,
                "Mode=" + PrivacyConsentManager.AnalyticsMode
            );
        }
        catch (Exception ex)
        {
            LogInit("AnalyticsEnableSkipped", "Reason=" + ex.Message);
            LogInitException(ex);
        }
    }

    private static void TryEnableCrashlytics()
    {
        // Crashlytics is intentionally NOT tied to Analytics or Ads consent.
        // Separate privacy-policy decision — see SetCrashlyticsCollectionEnabled.
        try
        {
            Crashlytics.IsCrashlyticsCollectionEnabled = true;
            LogInit("CrashlyticsCollectionEnabled=True");
        }
        catch (Exception ex)
        {
            LogInit("CrashlyticsEnableSkipped", "Reason=" + ex.Message);
            LogInitException(ex);
        }
    }

    /// <summary>
    /// Applies Analytics collection enable/disable.
    /// If Firebase is not ready yet, caches the desired state and applies on ready.
    /// </summary>
    public static void ApplyAnalyticsConsentPolicy(bool collectionEnabled)
    {
        pendingAnalyticsCollectionEnabled = collectionEnabled;

        if (!IsReady)
        {
            LogInit(
                "AnalyticsPolicyDeferred",
                "Enabled=" + collectionEnabled + " Reason=FirebaseNotReady"
            );
            return;
        }

        try
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(collectionEnabled);
            lastAppliedAnalyticsCollectionEnabled = collectionEnabled;
            LogInit("AnalyticsCollectionEnabled=" + collectionEnabled);
        }
        catch (Exception ex)
        {
            LogInit("AnalyticsPolicyFailed", "Reason=" + ex.Message);
            LogInitException(ex);
        }
    }

    /// <summary>
    /// Best-effort read for privacy debug logs.
    /// </summary>
    public static string GetAnalyticsCollectionEnabledOrUnknown()
    {
        if (lastAppliedAnalyticsCollectionEnabled.HasValue)
        {
            return lastAppliedAnalyticsCollectionEnabled.Value ? "True" : "False";
        }

        if (pendingAnalyticsCollectionEnabled.HasValue)
        {
            return pendingAnalyticsCollectionEnabled.Value
                ? "PendingTrue"
                : "PendingFalse";
        }

        if (!IsReady)
        {
            return "Unknown";
        }

        return PrivacyConsentManager.EvaluateAnalyticsCollectionEnabled()
            ? "True"
            : "False";
    }

    /// <summary>
    /// Optional Crashlytics collection control. Not auto-linked to Analytics toggle.
    /// </summary>
    public static void SetCrashlyticsCollectionEnabled(bool collectionEnabled)
    {
        if (!IsReady)
        {
            LogInit(
                "CrashlyticsPolicyDeferred",
                "Enabled=" + collectionEnabled + " Reason=FirebaseNotReady"
            );
            return;
        }

        try
        {
            Crashlytics.IsCrashlyticsCollectionEnabled = collectionEnabled;
            LogInit("CrashlyticsCollectionEnabled=" + collectionEnabled);
        }
        catch (Exception ex)
        {
            LogInit("CrashlyticsPolicyFailed", "Reason=" + ex.Message);
            LogInitException(ex);
        }
    }

    /// <summary>
    /// Best-effort read for privacy debug logs. Returns "Unknown" if Firebase is not ready.
    /// </summary>
    public static string GetCrashlyticsCollectionEnabledOrUnknown()
    {
        if (!IsReady)
        {
            return "Unknown";
        }

        try
        {
            return Crashlytics.IsCrashlyticsCollectionEnabled ? "True" : "False";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Crashlytics keys for the currently loaded level. No-op if not ready.
    /// </summary>
    public static void SetLevelCrashContext(
        int levelNumber,
        string levelAsset,
        string difficulty,
        string objective,
        string grid,
        int minimumMoves)
    {
        if (!IsReady)
        {
            return;
        }

        try
        {
            Crashlytics.SetCustomKey("current_level_number", levelNumber.ToString());
            Crashlytics.SetCustomKey("level_asset", levelAsset ?? string.Empty);
            Crashlytics.SetCustomKey("difficulty", difficulty ?? string.Empty);
            Crashlytics.SetCustomKey("objective", objective ?? string.Empty);
            Crashlytics.SetCustomKey("grid", grid ?? string.Empty);
            Crashlytics.SetCustomKey("minimum_moves", minimumMoves.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError("FirebaseManager: SetLevelCrashContext failed — " + ex.Message);
        }
    }

    /// <summary>
    /// Non-fatal for serious unexpected runtime issues only. No-op if not ready.
    /// </summary>
    public static void ReportNonFatal(string message, Exception exception = null)
    {
        if (!IsReady)
        {
            return;
        }

        if (!string.IsNullOrEmpty(message) &&
            message == lastNonFatalMessage &&
            Time.unscaledTime - lastNonFatalUnscaledTime < 5f)
        {
            return;
        }

        lastNonFatalMessage = message;
        lastNonFatalUnscaledTime = Time.unscaledTime;

        try
        {
            if (!string.IsNullOrEmpty(message))
            {
                Crashlytics.Log(message);
            }

            Exception toReport = exception ?? new Exception(message ?? "non_fatal");
            Crashlytics.LogException(toReport);
        }
        catch (Exception ex)
        {
            Debug.LogError("FirebaseManager: ReportNonFatal failed — " + ex.Message);
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>Editor/dev menu: force a test non-fatal.</summary>
    public static void SendTestNonFatal()
    {
        EnsureInstance();
        if (!IsReady)
        {
            Debug.LogWarning(
                "FirebaseManager: not ready — test non-fatal skipped. InitStage=" + InitStage
            );
            return;
        }

        ReportNonFatal("RushOut Firebase test non-fatal");
        Debug.Log("FirebaseManager: test non-fatal sent.");
    }

    private static void LogInit(string stage, string detail = null)
    {
        InitStage = stage;
        if (string.IsNullOrEmpty(detail))
        {
            Debug.Log("[FirebaseInit] Stage=" + stage);
        }
        else
        {
            Debug.Log("[FirebaseInit] Stage=" + stage + " " + detail);
        }
    }

    private static void LogInitException(Exception ex)
    {
        if (ex != null)
        {
            Debug.LogError("[FirebaseInit] Exception=" + ex);
        }
    }
#else
    private static void LogInit(string stage, string detail = null)
    {
        InitStage = stage;
    }

    private static void LogInitException(Exception ex)
    {
        InitStage = "Failed";
        if (ex != null)
        {
            LastFailureReason = ex.Message;
        }
    }
#endif
}

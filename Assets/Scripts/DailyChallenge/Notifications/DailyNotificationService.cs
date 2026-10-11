using System;
using System.Threading.Tasks;
using Firebase.Messaging;
using UnityEngine;

/// <summary>
/// Phase 4.4 — optional Daily Challenge FCM opt-in.
/// Separate from DailyChallengeIdentityService. Does not require Google Play linking.
/// Default OFF. Never requests permission at startup.
/// </summary>
public static class DailyNotificationService
{
    private const string PrefsEnabledKey = "RushOut_DailyNotificationsEnabled";
    private const string AnalyticsSourceSettings = "settings";

    private static bool bootstrapped;
    private static bool enableInFlight;
    private static bool handlersReady;
    private static bool loggedNotReady;
    private static bool pendingUnsubscribe;
    private static bool subscribeAgain;
    private static Task<bool> subscribeTask;

    /// <summary>User preference (persisted). Default false.</summary>
    public static bool PreferredEnabled => PlayerPrefs.GetInt(PrefsEnabledKey, 0) == 1;

    public static bool IsMessagingSdkAvailable => FirebaseMessagingBridge.IsSdkAvailable;

    public static string MessagingSdkStatus => FirebaseMessagingBridge.SdkStatus;

    /// <summary>True when preference ON and platform/SDK can actually receive topic pushes.</summary>
    public static bool IsEffectivelyEnabled =>
        PreferredEnabled &&
        IsMessagingSdkAvailable &&
        AndroidNotificationPermission.HasPermission;

    public static bool IsEditorUnsupported
    {
        get
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }
    }

    /// <summary>
    /// Call once after Firebase core is ready (or on Settings open).
    /// Does NOT prompt for permission. Keeps FCM registration off until opt-in.
    /// </summary>
    public static void Bootstrap()
    {
        if (bootstrapped)
        {
            return;
        }

#if UNITY_EDITOR
        bootstrapped = true;
        // Still disable auto token registration in Editor so prefs stay consistent.
        FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(false);
        Debug.Log(
            "[DailyNotify] Editor bootstrap — push unsupported. " + MessagingSdkStatus);
        return;
#else
        // Settings can open before CheckAndFixDependenciesAsync finishes.
        // Do not touch FirebaseMessaging, and do not consume bootstrap, until it has.
        if (!FirebaseManager.IsReady)
        {
            if (!loggedNotReady)
            {
                loggedNotReady = true;
                Debug.Log("[DailyNotify] Firebase not ready — notification bootstrap deferred.");
            }

            return;
        }

        if (bootstrapped)
        {
            return;
        }

        bootstrapped = true;

        if (pendingUnsubscribe)
        {
            pendingUnsubscribe = false;
            _ = FirebaseMessagingBridge.UnsubscribeAsync(DailyNotificationTopics.ProductionNewDay);
            _ = FirebaseMessagingBridge.UnsubscribeAsync(DailyNotificationTopics.Dev);
            FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(false);
            Debug.Log("[DailyNotify] Opt-out applied after Firebase became ready.");
            return;
        }

        bool canReceive = PreferredEnabled && AndroidNotificationPermission.HasPermission;
        // 13.15.0 API: TokenRegistrationOnInitEnabled (not RegistrationOnInitEnabled / RegisterAsync).
        FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(canReceive);

        if (!PreferredEnabled)
        {
            return;
        }

        if (!AndroidNotificationPermission.HasPermission)
        {
            Debug.Log("[DailyNotify] Permission denied — not subscribing.");
            return;
        }

        _ = EnsureSubscribedAsync();
#endif
    }

    /// <summary>
    /// Settings toggle. Never auto-called at first launch.
    /// </summary>
    public static async Task<bool> SetEnabledFromSettingsAsync(bool wantEnabled)
    {
        if (enableInFlight)
        {
            return PreferredEnabled;
        }

        enableInFlight = true;
        try
        {
#if UNITY_EDITOR
            // Preview preference only — no permission dialogs, no FCM subscribe.
            PersistPreference(wantEnabled);
            FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(false);
            if (wantEnabled)
            {
                GameAnalytics.LogDailyNotificationOptIn(AnalyticsSourceSettings);
            }
            else
            {
                GameAnalytics.LogDailyNotificationOptOut(AnalyticsSourceSettings);
            }

            Debug.Log(
                "[DailyNotify] Editor preference set enabled=" + wantEnabled +
                " (FCM subscribe skipped in Editor)");
            return wantEnabled;
#else
            if (!wantEnabled)
            {
                if (!FirebaseManager.IsReady)
                {
                    PersistPreference(false);
                    pendingUnsubscribe = true;
                    Debug.Log("[DailyNotify] Firebase not ready — opt-out saved, unsubscribe deferred.");
                    GameAnalytics.LogDailyNotificationOptOut(AnalyticsSourceSettings);
                    return false;
                }

                await DisableInternalAsync();
                GameAnalytics.LogDailyNotificationOptOut(AnalyticsSourceSettings);
                return false;
            }

            bool permitted = await AndroidNotificationPermission.RequestIfNeededAsync();
            if (!permitted)
            {
                PersistPreference(false);
                if (FirebaseManager.IsReady)
                {
                    FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(false);
                }

                Debug.Log("[DailyNotify] Permission denied — toggle stays OFF.");
                return false;
            }

            // Preference is user intent. A failed subscribe must not turn it back off.
            PersistPreference(true);
            GameAnalytics.LogDailyNotificationOptIn(AnalyticsSourceSettings);
            bool subscribed = await EnsureSubscribedAsync();
            if (subscribed)
            {
                Debug.Log(
                    "[DailyNotify] Opted in — subscribed topic=" +
                    DailyNotificationTopics.ProductionNewDay);
            }

            return true;
#endif
        }
        finally
        {
            enableInFlight = false;
        }
    }

    /// <summary>Dev-only: subscribe to daily_challenge_dev for safe test pushes.</summary>
    public static async Task<bool> SubscribeDevTopicAsync()
    {
#if UNITY_EDITOR
        Debug.LogWarning("[DailyNotify] Dev topic subscribe skipped in Editor.");
        return false;
#else
        if (!FirebaseManager.IsReady)
        {
            Debug.LogWarning("[DailyNotify] Dev topic skipped — Firebase not ready.");
            return false;
        }

        if (!FirebaseMessagingBridge.IsSdkAvailable)
        {
            return false;
        }

        if (!PreferredEnabled || !AndroidNotificationPermission.HasPermission)
        {
            Debug.LogWarning(
                "[DailyNotify] Enable Daily notifications first before dev topic.");
            return false;
        }

        EnsureHandlers();
        return await FirebaseMessagingBridge.SubscribeAsync(DailyNotificationTopics.Dev);
#endif
    }

    private static async Task DisableInternalAsync()
    {
        PersistPreference(false);
        await FirebaseMessagingBridge.UnsubscribeAsync(
            DailyNotificationTopics.ProductionNewDay);
        await FirebaseMessagingBridge.UnsubscribeAsync(DailyNotificationTopics.Dev);
        FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(false);
    }

    /// <summary>
    /// Subscribe to the production topic. Overlapping calls share one in-flight task
    /// and a follow-up pass if a token refresh arrives mid-subscribe.
    /// </summary>
    private static Task<bool> EnsureSubscribedAsync()
    {
        if (subscribeTask != null && !subscribeTask.IsCompleted)
        {
            subscribeAgain = true;
            return subscribeTask;
        }

        subscribeTask = RunSubscribeLoopAsync();
        return subscribeTask;
    }

    private static async Task<bool> RunSubscribeLoopAsync()
    {
        bool ok = false;
        do
        {
            subscribeAgain = false;
            ok = await SubscribeProductionOnceAsync();
        }
        while (subscribeAgain);

        return ok;
    }

    private static async Task<bool> SubscribeProductionOnceAsync()
    {
        if (!PreferredEnabled)
        {
            return false;
        }

        if (!FirebaseManager.IsReady)
        {
            Debug.Log("[DailyNotify] Subscription deferred — Firebase not ready.");
            return false;
        }

        if (!AndroidNotificationPermission.HasPermission)
        {
            Debug.Log("[DailyNotify] Permission denied — not subscribing.");
            return false;
        }

        EnsureHandlers();
        FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(true);

        bool hasToken = await FirebaseMessagingBridge.EnsureTokenAsync();
        if (!hasToken)
        {
            Debug.LogWarning("[DailyNotify] Subscription failed — token unavailable.");
            return false;
        }

        bool subscribed = await FirebaseMessagingBridge.SubscribeAsync(
            DailyNotificationTopics.ProductionNewDay);
        if (!subscribed)
        {
            Debug.LogWarning(
                "[DailyNotify] Subscription failed topic=" +
                DailyNotificationTopics.ProductionNewDay);
            return false;
        }

        Debug.Log(
            "[DailyNotify] Subscription succeeded topic=" +
            DailyNotificationTopics.ProductionNewDay);
        return true;
    }

    private static void EnsureHandlers()
    {
        if (handlersReady)
        {
            return;
        }

        FirebaseMessagingBridge.AttachHandlers(OnMessageReceived, OnTokenReceived);
        handlersReady = true;
    }

    private static void OnTokenReceived(object sender, TokenReceivedEventArgs e)
    {
        // Intentionally no token / FID logging.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[DailyNotify] FCM TokenReceived (token not logged).");
#endif
        if (!PreferredEnabled || !AndroidNotificationPermission.HasPermission)
        {
            return;
        }

        // Topic subscriptions do not survive FID / token rotation.
        _ = EnsureSubscribedAsync();
    }

    private static void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        string type;
        string dayId;
        FirebaseMessagingBridge.TryReadDailyChallengePayload(e, out type, out dayId);

        bool isDaily = string.Equals(type, "daily_challenge", StringComparison.Ordinal);
        GameAnalytics.LogDailyNotificationReceived(dayId);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[DailyNotify] MessageReceived type=" + type +
            " dayId=" + dayId +
            " (no custom popup)");
#endif

        if (!isDaily)
        {
            return;
        }

        GameAnalytics.LogDailyNotificationOpened(dayId);

        try
        {
            DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
            if (manager == null)
            {
                return;
            }

            string current = manager.CurrentDayId ?? string.Empty;
            if (!string.IsNullOrEmpty(dayId) &&
                !string.Equals(dayId, current, StringComparison.Ordinal))
            {
                _ = manager.SyncFromAuthorityAsync();
            }
            else
            {
                manager.EnsureDaySynced();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[DailyNotify] Daily refresh after message failed: " + ex.GetType().Name);
        }
    }

    private static void PersistPreference(bool enabled)
    {
        PlayerPrefs.SetInt(PrefsEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}

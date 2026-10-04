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

        bootstrapped = true;

#if UNITY_EDITOR
        // Still disable auto token registration in Editor so prefs stay consistent.
        FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(false);
        Debug.Log(
            "[DailyNotify] Editor bootstrap — push unsupported. " + MessagingSdkStatus);
        return;
#endif

        // Explicit opt-in: do not auto-register FCM token until the user enables Daily notifications.
        // 13.15.0 API: TokenRegistrationOnInitEnabled (not RegistrationOnInitEnabled / RegisterAsync).
        FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(PreferredEnabled);

        if (PreferredEnabled && AndroidNotificationPermission.HasPermission)
        {
            _ = EnsureSubscribedAsync();
        }
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
                await DisableInternalAsync();
                GameAnalytics.LogDailyNotificationOptOut(AnalyticsSourceSettings);
                return false;
            }

            if (!FirebaseMessagingBridge.IsSdkAvailable)
            {
                PersistPreference(false);
                Debug.LogWarning(
                    "[DailyNotify] Cannot enable — " + FirebaseMessagingBridge.SdkStatus);
                return false;
            }

            bool permitted = await AndroidNotificationPermission.RequestIfNeededAsync();
            if (!permitted)
            {
                PersistPreference(false);
                FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(false);
#if DEVELOPMENT_BUILD
                Debug.Log("[DailyNotify] Permission denied — toggle stays OFF.");
#endif
                return false;
            }

            PersistPreference(true);
            FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(true);
            EnsureHandlers();
            bool hasToken = await FirebaseMessagingBridge.EnsureTokenAsync();
            if (!hasToken)
            {
                PersistPreference(false);
                FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(false);
                Debug.LogWarning("[DailyNotify] GetTokenAsync failed — toggle OFF.");
                return false;
            }

            bool subscribed = await FirebaseMessagingBridge.SubscribeAsync(
                DailyNotificationTopics.ProductionNewDay);
            if (!subscribed)
            {
                PersistPreference(false);
                FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(false);
                Debug.LogWarning("[DailyNotify] Topic subscribe failed — toggle OFF.");
                return false;
            }

            GameAnalytics.LogDailyNotificationOptIn(AnalyticsSourceSettings);
            Debug.Log(
                "[DailyNotify] Opted in — subscribed topic=" +
                DailyNotificationTopics.ProductionNewDay);
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

    private static async Task EnsureSubscribedAsync()
    {
        if (!PreferredEnabled)
        {
            return;
        }

        EnsureHandlers();
        FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(true);
        await FirebaseMessagingBridge.EnsureTokenAsync();
        await FirebaseMessagingBridge.SubscribeAsync(
            DailyNotificationTopics.ProductionNewDay);
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

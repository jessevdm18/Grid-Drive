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
    public const string AnalyticsSourceSettings = "settings";
    public const string AnalyticsSourcePrompt = "prompt";

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
            _ = DisableMessagingAsync();
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
        NotificationOptInResult result = await SetEnabledAsync(wantEnabled, AnalyticsSourceSettings);
        return result == NotificationOptInResult.Enabled;
    }

    /// <summary>
    /// User intent for Daily notifications. Does not touch the Lives preference.
    /// Already-granted POST_NOTIFICATIONS does not show another OS dialog.
    /// </summary>
    public static async Task<NotificationOptInResult> SetEnabledAsync(bool wantEnabled, string source)
    {
        if (enableInFlight)
        {
            return PreferredEnabled
                ? NotificationOptInResult.Enabled
                : NotificationOptInResult.Disabled;
        }

        if (string.IsNullOrEmpty(source))
        {
            source = AnalyticsSourceSettings;
        }

        enableInFlight = true;
        try
        {
#if UNITY_EDITOR
            PersistPreference(wantEnabled);
            FirebaseMessagingBridge.SetTokenRegistrationOnInitEnabled(false);
            if (wantEnabled)
            {
                GameAnalytics.LogDailyNotificationOptIn(source);
            }
            else
            {
                GameAnalytics.LogDailyNotificationOptOut(source);
            }

            Debug.Log(
                "[DailyNotify] Editor preference set enabled=" + wantEnabled +
                " source=" + source +
                " (FCM subscribe skipped in Editor)");
            return wantEnabled
                ? NotificationOptInResult.Enabled
                : NotificationOptInResult.Disabled;
#else
            if (!wantEnabled)
            {
                if (!FirebaseManager.IsReady)
                {
                    PersistPreference(false);
                    pendingUnsubscribe = true;
                    Debug.Log("[DailyNotify] Firebase not ready — opt-out saved, unsubscribe deferred.");
                    GameAnalytics.LogDailyNotificationOptOut(source);
                    return NotificationOptInResult.Disabled;
                }

                await DisableInternalAsync();
                GameAnalytics.LogDailyNotificationOptOut(source);
                return NotificationOptInResult.Disabled;
            }

            if (AndroidNotificationPermission.GetStatus() ==
                AndroidNotificationPermissionStatus.NeedsSettings)
            {
                PersistPreference(false);
                Debug.Log("[DailyNotify] Permission needs system settings — preference stays OFF.");
                return NotificationOptInResult.NeedsSettings;
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
                return NotificationOptInResult.Disabled;
            }

            // Preference is user intent. A failed subscribe must not turn it back off.
            PersistPreference(true);
            GameAnalytics.LogDailyNotificationOptIn(source);
            bool subscribed = await EnsureSubscribedAsync();
            if (subscribed)
            {
                Debug.Log(
                    "[DailyNotify] Opted in — subscribed topic=" +
                    DailyNotificationTopics.ProductionNewDay);
            }

            return NotificationOptInResult.Enabled;
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
        await DisableMessagingAsync();
    }

    private static async Task DisableMessagingAsync()
    {
        string token = await FirebaseMessagingBridge.GetTokenOrEmptyAsync();
        await RegisterTokenWithServerAsync(false, token);
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

        bool registered = await RegisterTokenWithServerAsync(
            true,
            await FirebaseMessagingBridge.GetTokenOrEmptyAsync());
        if (!registered)
        {
            Debug.LogWarning("[DailyNotify] Token registration failed — topic subscribe continues.");
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

    private static async Task<bool> RegisterTokenWithServerAsync(bool enabled, string token)
    {
        try
        {
            bool signedIn = await DailyChallengeIdentityService.EnsureSignedInAsync();
            if (!signedIn || string.IsNullOrEmpty(DailyChallengeIdentityService.IdToken))
            {
                Debug.LogWarning(
                    "[DailyNotify] Token registration skipped — not signed in. enabled=" +
                    enabled);
                return false;
            }

            int offset = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;
            string zone = TimeZoneInfo.Local.Id ?? string.Empty;
            string json =
                "{\"enabled\":" + (enabled ? "true" : "false") +
                ",\"token\":\"" + EscapeJson(token) +
                "\",\"utcOffsetMinutes\":" + offset.ToString() +
                ",\"timeZoneId\":\"" + EscapeJson(zone) + "\"}";

            DailyChallengeFirebaseSettings settings =
                DailyChallengeFirebaseSettings.LoadDefault();
            if (settings == null || !settings.IsConfigured)
            {
                Debug.LogWarning(
                    "[DailyNotify] Token registration skipped — Firebase settings missing.");
                return false;
            }

            FirebaseRestClient.CallableResponse response =
                await FirebaseRestClient.CallCallableAsync(
                    settings.GetCallableBaseUrl(),
                    "registerDailyNotificationToken",
                    json,
                    DailyChallengeIdentityService.IdToken);

            Debug.Log(
                "[DailyNotify] Token registration enabled=" + enabled +
                " tokenPresent=" + (!string.IsNullOrEmpty(token)) +
                " tokenLength=" + (token != null ? token.Length : 0) +
                " ok=" + response.Ok +
                " http=" + response.HttpStatus);
            return response.Ok;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[DailyNotify] Token registration failed: " + ex.GetType().Name);
            return false;
        }
    }

    private static string EscapeJson(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
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
        int tokenLength = e != null && e.Token != null ? e.Token.Length : 0;
        Debug.Log(
            "[DailyNotify] FCM TokenReceived tokenPresent=" + (tokenLength > 0) +
            " tokenLength=" + tokenLength);
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

        bool isResult = string.Equals(type, "daily_result", StringComparison.Ordinal);
        if (isResult)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                "[DailyNotify] Result push received dayId=" + dayId +
                " (seen flag unchanged; Main Menu still fetches the result)");
#endif
            return;
        }

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

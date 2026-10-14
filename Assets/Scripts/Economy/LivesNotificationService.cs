using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Schedules one local Android notification for the moment lives reach MaxLives.
/// Does not use Firebase Cloud Messaging and does not change refill math.
/// Requires both the Lives preference and POST_NOTIFICATIONS.
/// </summary>
public static class LivesNotificationService
{
    public const string PrefsEnabledKey = "RushOut_LivesNotificationsEnabled";
    public const string ChannelId = "lives";
    public const int NotificationId = 51005;
    public const string Title = "LIVES FULL";
    public const string Body = "Your lives are full. Come back and play!";

    /// <summary>User intent. Default off. Not the same as a successful schedule.</summary>
    public static bool PreferredEnabled => PlayerPrefs.GetInt(PrefsEnabledKey, 0) == 1;

    public static void Sync(LivesManager lives)
    {
        if (TestHoldActive)
        {
            Log("Test alarm held — skipping reschedule.");
            return;
        }

        bool enabled = PreferredEnabled;
        bool permission = HasNotificationPermission();
        int currentLives = lives != null ? lives.CurrentLives : -1;
        DateTime fullUtc = default;
        bool hasFull = lives != null && lives.TryGetLivesFullUtc(out fullUtc);
        string nextLife = lives != null && lives.NextLifeUtc.HasValue
            ? FormatUtc(lives.NextLifeUtc.Value)
            : "(none)";
        Log(
            "enabled=" + enabled +
            " permission=" + permission +
            " lives=" + currentLives +
            " nextLifeUtc=" + nextLife +
            " fullLivesUtc=" + (hasFull ? FormatUtc(fullUtc) : "(none)"));

        if (lives == null || !enabled)
        {
            Cancel("preference off");
            return;
        }

        if (!hasFull || fullUtc <= DateTime.UtcNow)
        {
            Cancel(hasFull ? "full time already passed" : "lives already full or no timer");
            return;
        }

        DateTime deliverUtc = LocalNotificationPolicy.DelayForQuietHours(fullUtc);
        if (deliverUtc != DateTime.SpecifyKind(fullUtc, DateTimeKind.Utc))
        {
            Log(
                "quiet hours moved delivery from " + FormatUtc(fullUtc) +
                " to " + FormatUtc(deliverUtc));
        }

        if (deliverUtc <= DateTime.UtcNow)
        {
            Cancel("delivery time already passed");
            return;
        }

        if (!permission)
        {
            Cancel("permission not granted");
            Log("Not scheduled — notification permission is not granted. fullUtc=" + FormatUtc(fullUtc));
            return;
        }

        LocalNotificationPolicy.SetDueUtc(LocalNotificationPolicy.LivesDueUtcTicksKey, deliverUtc);
#if UNITY_EDITOR
        Log("Would schedule full lives notification for " + FormatUtc(deliverUtc));
#else
        Schedule(deliverUtc, Title, Body, skipRelevance: false);
#endif
        FreeCoinsNotificationService.Sync();
    }

    /// <summary>
    /// Settings or contextual opt-in. Does not change Daily. Does not change lives.
    /// </summary>
    public static async Task<NotificationOptInResult> SetEnabledFromUserAsync(bool wantEnabled)
    {
        if (!wantEnabled)
        {
            PersistPreference(false);
            Cancel("user disabled");
            return NotificationOptInResult.Disabled;
        }

#if UNITY_EDITOR
        PersistPreference(true);
        Sync(LivesManager.Instance);
        return NotificationOptInResult.Enabled;
#else
        if (AndroidNotificationPermission.GetStatus() ==
            AndroidNotificationPermissionStatus.NeedsSettings)
        {
            PersistPreference(false);
            Cancel("permission needs settings");
            return NotificationOptInResult.NeedsSettings;
        }

        bool permitted = await AndroidNotificationPermission.RequestIfNeededAsync();
        if (!permitted)
        {
            PersistPreference(false);
            Cancel("permission denied");
            return NotificationOptInResult.Disabled;
        }

        PersistPreference(true);
        Sync(LivesManager.Instance);
        return NotificationOptInResult.Enabled;
#endif
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// Development helper. Schedules the lives-full copy one minute from now.
    /// Does not read or write lives.
    /// </summary>
    public static void ScheduleTestInOneMinute()
    {
        DateTime fullUtc = DateTime.UtcNow.AddMinutes(1);
        if (!HasNotificationPermission())
        {
            Log("Test not scheduled — notification permission is not granted. fullUtc=" + FormatUtc(fullUtc));
            return;
        }

#if UNITY_EDITOR
        Log("Would schedule full lives notification for " + FormatUtc(fullUtc) + " (test, lives unchanged)");
#else
        testHoldUntilUtc = DateTime.UtcNow.AddSeconds(70);
        Schedule(fullUtc, Title, Body, skipRelevance: true);
#endif
    }

#if DEVELOPMENT_BUILD && !UNITY_EDITOR && UNITY_ANDROID
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ScheduleTestFromLaunchIntent()
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                if (activity == null)
                {
                    return;
                }

                using (AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent"))
                {
                    if (intent != null &&
                        intent.Call<bool>("getBooleanExtra", "rushout_test_lives_notification", false))
                    {
                        ScheduleTestInOneMinute();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[LivesNotification] Test launch intent failed: " + ex.GetType().Name);
        }
    }
#endif
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private static DateTime testHoldUntilUtc;
    private static bool TestHoldActive => DateTime.UtcNow < testHoldUntilUtc;
#else
    private static bool TestHoldActive => false;
#endif

    private static bool HasNotificationPermission()
    {
#if UNITY_EDITOR
        return true;
#elif UNITY_ANDROID
        return AndroidNotificationPermission.HasPermission;
#else
        return false;
#endif
    }

    private static void PersistPreference(bool enabled)
    {
        PlayerPrefs.SetInt(PrefsEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private static void Schedule(DateTime fullUtc, string title, string body, bool skipRelevance)
    {
        string suffix = skipRelevance ? " (test, lives unchanged)" : string.Empty;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            long epochMs = new DateTimeOffset(DateTime.SpecifyKind(fullUtc, DateTimeKind.Utc))
                .ToUnixTimeMilliseconds();
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass notifier =
                       new AndroidJavaClass("com.raddergames.griddrive.notifications.LivesFullNotifier"))
            {
                if (activity == null)
                {
                    Log("Not scheduled — no current activity. fullUtc=" + FormatUtc(fullUtc) + suffix);
                    return;
                }

                string method = notifier.CallStatic<string>(
                    "schedule", activity, epochMs, title, body, skipRelevance);
                Log(
                    "schedule requested method=" + (method ?? "(null)") +
                    " alarmId=" + NotificationId +
                    " fullUtc=" + FormatUtc(fullUtc) + suffix);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[LivesNotification] Schedule failed: " + ex.GetType().Name);
        }
#else
        Log("Would schedule full lives notification for " + FormatUtc(fullUtc) + suffix);
#endif
    }

    private static void Cancel(string reason)
    {
        LocalNotificationPolicy.ClearDue(LocalNotificationPolicy.LivesDueUtcTicksKey);
        Log("alarm cancelled reason=" + reason + " alarmId=" + NotificationId);
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass notifier =
                       new AndroidJavaClass("com.raddergames.griddrive.notifications.LivesFullNotifier"))
            {
                if (activity == null)
                {
                    Log("alarm cancel skipped — no current activity");
                }
                else
                {
                    notifier.CallStatic("cancel", activity, reason);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[LivesNotification] Cancel failed: " + ex.GetType().Name);
        }
#else
        Log("Would cancel pending full lives notification");
#endif
        FreeCoinsNotificationService.Sync();
    }

    private static void Log(string message)
    {
        Debug.Log("[LivesNotification] " + message);
    }

    private static string FormatUtc(DateTime utc)
    {
        return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("u");
    }
}

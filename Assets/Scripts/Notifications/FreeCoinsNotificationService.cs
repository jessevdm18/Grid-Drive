using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// One local Android alarm for the transition from fewer than 3 free coin videos
/// to all 3 available again. Does not use Firebase and does not change ad counters.
/// </summary>
public static class FreeCoinsNotificationService
{
    public const string PrefsEnabledKey = "RushOut_FreeCoinNotificationsEnabled";
    public const string ChannelId = "rewards";
    public const int NotificationId = 51006;
    public const string Title = "FREE COINS READY";
    public const string Body = "3 free coin videos are ready to watch!";

    private static bool syncing;

    public static bool PreferredEnabled => PlayerPrefs.GetInt(PrefsEnabledKey, 0) == 1;

    public static void Sync()
    {
        if (syncing || TestHoldActive)
        {
            if (TestHoldActive)
            {
                Log("Test alarm held — skipping reschedule.");
            }

            return;
        }

        syncing = true;
        try
        {
            bool enabled = PreferredEnabled;
            bool permission = HasNotificationPermission();
            int remaining = FreeCoinsRewardedUI.GetRemainingAvailable();
            bool hasNext = FreeCoinsRewardedUI.TryGetNextAllAvailableLocal(out DateTime nextLocal);
            Log(
                "enabled=" + enabled +
                " permission=" + permission +
                " remaining=" + remaining +
                " nextLocal=" + (hasNext ? nextLocal.ToString("yyyy-MM-dd HH:mm") : "(none)"));

            if (!enabled || !hasNext)
            {
                Cancel(enabled ? "all 3 already available" : "preference off");
                return;
            }

            DateTime naturalUtc = ToUtc(nextLocal);
            DateTime afterQuiet = LocalNotificationPolicy.DelayForQuietHours(naturalUtc);
            DateTime deliverUtc = LocalNotificationPolicy.SpaceFreeCoinsAfterLives(afterQuiet);
            if (afterQuiet != naturalUtc)
            {
                Log("quiet hours moved delivery from " + FormatUtc(naturalUtc) + " to " + FormatUtc(afterQuiet));
            }

            if (deliverUtc != afterQuiet)
            {
                Log(
                    "spacing moved delivery from " + FormatUtc(afterQuiet) +
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
                Log("Not scheduled — notification permission is not granted. deliverUtc=" + FormatUtc(deliverUtc));
                return;
            }

            LocalNotificationPolicy.SetDueUtc(LocalNotificationPolicy.FreeCoinsDueUtcTicksKey, deliverUtc);
#if UNITY_EDITOR
            Log("Would schedule free coins notification for " + FormatUtc(deliverUtc));
#else
            Schedule(deliverUtc, Title, Body, skipRelevance: false);
#endif
        }
        finally
        {
            syncing = false;
        }
    }

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
        Sync();
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
        Sync();
        return NotificationOptInResult.Enabled;
#endif
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// Development helper. Schedules the free-coins copy one minute from now.
    /// Does not grant coins or change rewarded-ad counters.
    /// </summary>
    public static void ScheduleTestInOneMinute()
    {
        DateTime deliverUtc = DateTime.UtcNow.AddMinutes(1);
        if (!HasNotificationPermission())
        {
            Log("Test not scheduled — notification permission is not granted. deliverUtc=" + FormatUtc(deliverUtc));
            return;
        }

#if UNITY_EDITOR
        Log("Would schedule free coins notification for " + FormatUtc(deliverUtc) + " (test, counters unchanged)");
#else
        testHoldUntilUtc = DateTime.UtcNow.AddSeconds(70);
        Schedule(deliverUtc, Title, Body, skipRelevance: true);
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
                        intent.Call<bool>("getBooleanExtra", "rushout_test_free_coins_notification", false))
                    {
                        ScheduleTestInOneMinute();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[FreeCoinNotification] Test launch intent failed: " + ex.GetType().Name);
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

    private static void Schedule(DateTime deliverUtc, string title, string body, bool skipRelevance)
    {
        string suffix = skipRelevance ? " (test, counters unchanged)" : string.Empty;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            long epochMs = new DateTimeOffset(DateTime.SpecifyKind(deliverUtc, DateTimeKind.Utc))
                .ToUnixTimeMilliseconds();
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass notifier =
                       new AndroidJavaClass("com.raddergames.griddrive.notifications.FreeCoinsNotifier"))
            {
                if (activity == null)
                {
                    Log("Not scheduled — no current activity. deliverUtc=" + FormatUtc(deliverUtc) + suffix);
                    return;
                }

                string method = notifier.CallStatic<string>(
                    "schedule", activity, epochMs, title, body, skipRelevance);
                Log(
                    "schedule requested method=" + (method ?? "(null)") +
                    " alarmId=" + NotificationId +
                    " deliverUtc=" + FormatUtc(deliverUtc) + suffix);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[FreeCoinNotification] Schedule failed: " + ex.GetType().Name);
        }
#else
        Log("Would schedule free coins notification for " + FormatUtc(deliverUtc) + suffix);
#endif
    }

    private static void Cancel(string reason)
    {
        LocalNotificationPolicy.ClearDue(LocalNotificationPolicy.FreeCoinsDueUtcTicksKey);
        Log("alarm cancelled reason=" + reason + " alarmId=" + NotificationId);
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass notifier =
                       new AndroidJavaClass("com.raddergames.griddrive.notifications.FreeCoinsNotifier"))
            {
                if (activity == null)
                {
                    Log("alarm cancel skipped — no current activity");
                    return;
                }

                notifier.CallStatic("cancel", activity, reason);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[FreeCoinNotification] Cancel failed: " + ex.GetType().Name);
        }
#else
        Log("Would cancel pending free coins notification");
#endif
    }

    private static DateTime ToUtc(DateTime local)
    {
        DateTime specified = DateTime.SpecifyKind(local, DateTimeKind.Local);
        return DateTime.SpecifyKind(specified.ToUniversalTime(), DateTimeKind.Utc);
    }

    private static void Log(string message)
    {
        Debug.Log("[FreeCoinNotification] " + message);
    }

    private static string FormatUtc(DateTime utc)
    {
        return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("u");
    }
}

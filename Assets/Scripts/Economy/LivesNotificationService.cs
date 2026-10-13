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

        if (lives == null || !PreferredEnabled)
        {
            Cancel(logInEditor: false);
            return;
        }

        if (!lives.TryGetLivesFullUtc(out DateTime fullUtc) || fullUtc <= DateTime.UtcNow)
        {
            Cancel(logInEditor: false);
            return;
        }

        if (!HasNotificationPermission())
        {
            Cancel(logInEditor: false);
            Log("Not scheduled — notification permission is not granted. fullUtc=" + FormatUtc(fullUtc));
            return;
        }

#if UNITY_EDITOR
        Log("Would schedule full lives notification for " + FormatUtc(fullUtc));
#else
        Schedule(fullUtc, Title, Body, livesUnchangedTest: false);
#endif
    }

    /// <summary>
    /// Settings or contextual opt-in. Does not change Daily. Does not change lives.
    /// </summary>
    public static async Task<NotificationOptInResult> SetEnabledFromUserAsync(bool wantEnabled)
    {
        if (!wantEnabled)
        {
            PersistPreference(false);
            Cancel(logInEditor: false);
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
            Cancel(logInEditor: false);
            return NotificationOptInResult.NeedsSettings;
        }

        bool permitted = await AndroidNotificationPermission.RequestIfNeededAsync();
        if (!permitted)
        {
            PersistPreference(false);
            Cancel(logInEditor: false);
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
        Schedule(fullUtc, Title, Body, livesUnchangedTest: true);
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

    private static void Schedule(DateTime fullUtc, string title, string body, bool livesUnchangedTest)
    {
        string suffix = livesUnchangedTest ? " (test, lives unchanged)" : string.Empty;
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

                notifier.CallStatic("schedule", activity, epochMs, title, body);
            }

            Log("Scheduled full lives notification for " + FormatUtc(fullUtc) + suffix);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[LivesNotification] Schedule failed: " + ex.GetType().Name);
        }
#else
        Log("Would schedule full lives notification for " + FormatUtc(fullUtc) + suffix);
#endif
    }

    private static void Cancel(bool logInEditor)
    {
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
                    return;
                }

                notifier.CallStatic("cancel", activity);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[LivesNotification] Cancel failed: " + ex.GetType().Name);
        }
#else
        if (logInEditor)
        {
            Log("Would cancel pending full lives notification");
        }
#endif
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

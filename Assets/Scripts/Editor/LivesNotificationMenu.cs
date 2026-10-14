#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Development-only lives notification check. Does not change lives.
/// </summary>
public static class LivesNotificationMenu
{
    [MenuItem("Rush Out/Notifications/Test Lives Full Notification")]
    public static void TestLivesFullNotification()
    {
        LivesNotificationService.ScheduleTestInOneMinute();
        Debug.Log(
            "[LivesNotification] Editor test helper finished without changing lives. " +
            "A development Android build can schedule the same 1-minute local notification with " +
            "adb shell am start -n com.raddergames.griddrive/com.google.firebase.MessagingUnityPlayerActivity --ez rushout_test_lives_notification true");
    }

    [MenuItem("Rush Out/Notifications/Test Free Coins Notification")]
    public static void TestFreeCoinsNotification()
    {
        FreeCoinsNotificationService.ScheduleTestInOneMinute();
        Debug.Log(
            "[FreeCoinNotification] Editor test helper finished without changing coin ads. " +
            "A development Android build can schedule the same 1-minute local notification with " +
            "adb shell am start -n com.raddergames.griddrive/com.google.firebase.MessagingUnityPlayerActivity --ez rushout_test_free_coins_notification true");
    }

    [MenuItem("Rush Out/Notifications/Show Daily Notification Prompt")]
    public static void ShowDailyPrompt()
    {
        NotificationPromptCoordinator.ShowDailyPromptForDevelopment();
    }

    [MenuItem("Rush Out/Notifications/Show Lives Notification Prompt")]
    public static void ShowLivesPrompt()
    {
        NotificationPromptCoordinator.ShowLivesPromptForDevelopment();
    }

    [MenuItem("Rush Out/Notifications/Show Free Coins Notification Prompt")]
    public static void ShowFreeCoinsPrompt()
    {
        NotificationPromptCoordinator.ShowFreeCoinsPromptForDevelopment();
    }

    [MenuItem("Rush Out/Notifications/Reset Notification Prompt Flags")]
    public static void ResetPromptFlags()
    {
        NotificationPromptCoordinator.ResetPromptFlags();
    }
}
#endif

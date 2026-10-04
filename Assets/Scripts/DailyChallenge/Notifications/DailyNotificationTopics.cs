/// <summary>Stable FCM topic names for Daily Challenge push (Phase 4.4).</summary>
public static class DailyNotificationTopics
{
    /// <summary>Production topic — scheduled publishDailyChallengeAndNotify.</summary>
    public const string ProductionNewDay = "daily_challenge_new";

    /// <summary>Dev/test topic only — never used by the production scheduler.</summary>
    public const string Dev = "daily_challenge_dev";
}

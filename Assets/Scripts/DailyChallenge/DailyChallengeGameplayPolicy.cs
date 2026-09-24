using UnityEngine;

/// <summary>
/// Single Daily Challenge gameplay policy.
/// When active: no performance-based FailLevel (global/special move limits, timed,
/// NoTouch, FragileCargo). Puzzle stays playable until solved or abandoned.
/// LimitedVehicle lock (non-FailLevel) remains — it does not end the attempt.
/// </summary>
public static class DailyChallengeGameplayPolicy
{
    /// <summary>
    /// True while Daily Challenge Gameplay is the active session.
    /// </summary>
    public static bool IsActive
    {
        get
        {
            LevelManager lm = Object.FindFirstObjectByType<LevelManager>();
            if (lm != null && lm.IsDailyChallengeSession)
            {
                return true;
            }

            return DailyChallengeContext.IsActiveSession;
        }
    }

    /// <summary>
    /// Suppress terminal performance failures and limit denominators on the MOVES HUD.
    /// </summary>
    public static bool SuppressPerformanceFailures => IsActive;

    public static bool IsPerformanceFailReason(string failReason)
    {
        if (string.IsNullOrEmpty(failReason))
        {
            return false;
        }

        switch (failReason)
        {
            case "global_move_limit":
            case "move_limit":
            case "timed_out":
            case "no_touch":
            case "fragile_cargo":
            case "mission_failed":
                return true;
            default:
                return false;
        }
    }

    public static void LogSuppressed(string system, string detail)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[DailyChallenge] Performance fail suppressed | system=" + system +
            " | " + detail);
#endif
    }
}

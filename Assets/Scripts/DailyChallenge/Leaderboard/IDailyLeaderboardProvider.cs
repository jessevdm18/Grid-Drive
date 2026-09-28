/// <summary>
/// Abstraction for Daily leaderboard data. Phase 3 = simulated; later = backend.
/// UI must not depend on which provider is active.
/// </summary>
public interface IDailyLeaderboardProvider
{
    DailyLeaderboardSnapshot GetLeaderboard(DailyLeaderboardRequest request);
}

/// <summary>Inputs required to build a day/level snapshot.</summary>
public struct DailyLeaderboardRequest
{
    public string DayId;
    public string LevelAssetName;
    public int MinimumMoves;
    public DailyChallengeConfig Config;
    public bool HasLocalPlayerResult;
    public DailyChallengeResult LocalPlayerResult;
    public DailyChallengeState DailyState;
}

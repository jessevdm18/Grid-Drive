using System;

/// <summary>
/// One ranked Daily Challenge leaderboard row (local, simulated, or future remote).
/// </summary>
[Serializable]
public struct DailyLeaderboardEntry
{
    public string PlayerId;
    public string DisplayName;
    public int Score;
    public int Moves;
    public long CompletionTimeMilliseconds;
    public bool IsCurrentPlayer;
    public bool IsSimulated;
    public DailyLeaderboardEntrySource Source;
    public int Rank;

    public bool IsEligibleForAuthoritativeRewards =>
        !IsSimulated && Source != DailyLeaderboardEntrySource.Simulated;
}

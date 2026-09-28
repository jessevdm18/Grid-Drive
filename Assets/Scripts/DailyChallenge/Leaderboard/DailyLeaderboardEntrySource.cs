/// <summary>
/// Origin of a leaderboard entry.
/// Simulated entries MUST NEVER be eligible for authoritative rewards / prize settlement.
/// </summary>
public enum DailyLeaderboardEntrySource
{
    /// <summary>Local device player result for today's Daily.</summary>
    LocalPlayer = 0,

    /// <summary>
    /// Deterministic placeholder / display-fill only.
    /// Not a real competitor. Never qualifies for prizes or authoritative winner.
    /// </summary>
    Simulated = 1,

    /// <summary>Reserved legacy alias — prefer <see cref="Online"/>.</summary>
    FutureRemotePlayer = 2,

    /// <summary>Real remote player result from Online Daily authority.</summary>
    Online = 3
}

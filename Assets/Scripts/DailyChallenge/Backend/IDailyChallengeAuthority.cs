using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Authoritative Daily Challenge backend. Device PlayerPrefs must not be trusted
/// for attempt ownership when <see cref="DailyChallengeBackendMode.Online"/> is active.
/// </summary>
public interface IDailyChallengeAuthority
{
    Task<DailyChallengeAuthorityResult> GetCurrentChallengeAsync();

    Task<DailyChallengeAuthorityResult> GetAttemptStateAsync(string dayId);

    /// <summary>
    /// Atomically claim today's attempt BEFORE Gameplay loads.
    /// </summary>
    Task<DailyChallengeAuthorityResult> TryStartAttemptAsync(
        DailyChallengeDescriptor challenge);

    Task<DailyChallengeAuthorityResult> SubmitResultAsync(
        DailyChallengeDescriptor challenge,
        int moves,
        long completionTimeMs,
        int clientComputedScore);

    Task<DailyChallengeAuthorityResult> MarkAbandonedAsync(string dayId);

    /// <summary>Completed results for GLOBAL leaderboard (real players only).</summary>
    Task<IReadOnlyList<DailyLeaderboardEntry>> GetCompletedLeaderboardAsync(
        string dayId,
        int maxEntries);
}

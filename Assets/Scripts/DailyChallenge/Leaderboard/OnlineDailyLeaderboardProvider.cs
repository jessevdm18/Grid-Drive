using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Fetches real Completed Daily results via <see cref="IDailyChallengeAuthority"/>.
/// Does not generate simulated rows.
/// </summary>
public sealed class OnlineDailyLeaderboardProvider
{
    public async Task<IReadOnlyList<DailyLeaderboardEntry>> FetchRealEntriesAsync(
        string dayId,
        int maxEntries)
    {
        return await DailyChallengeAuthority.Current.GetCompletedLeaderboardAsync(
            dayId,
            maxEntries);
    }
}

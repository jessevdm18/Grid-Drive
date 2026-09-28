using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Facade for Daily leaderboard snapshots. UI talks to this, not PlayerPrefs / bot gen.
/// SimulatedLocal → SimulatedDailyLeaderboardProvider.
/// Online → HybridDailyLeaderboardProvider (real + simulated fillers).
/// </summary>
public static class DailyLeaderboardService
{
    private static IDailyLeaderboardProvider provider;
    private static DailyLeaderboardSnapshot cachedSnapshot;
    private static string cachedKey;

    public static IDailyLeaderboardProvider Provider
    {
        get
        {
            if (provider == null)
            {
                provider = CreateDefaultProvider();
            }

            return provider;
        }
        set
        {
            provider = value;
            InvalidateCache();
        }
    }

    private static IDailyLeaderboardProvider CreateDefaultProvider()
    {
        if (DailyChallengeAuthority.IsOnlineMode)
        {
            return new HybridDailyLeaderboardProvider();
        }

        return new SimulatedDailyLeaderboardProvider();
    }

    public static void InvalidateCache()
    {
        cachedSnapshot = null;
        cachedKey = null;
        HybridDailyLeaderboardCache.Clear();
        provider = null;
    }

    public static DailyLeaderboardSnapshot GetTodaysSnapshot()
    {
        DailyLeaderboardRequest request = BuildTodaysRequest(
            out bool hasResult,
            out DailyChallengeResult result);
        if (request.DayId == null)
        {
            return EmptySnapshot(string.Empty, string.Empty);
        }

        string key =
            request.DayId + "|" + request.LevelAssetName + "|" + request.DailyState + "|" +
            (hasResult
                ? (result.Score + ":" + result.Moves + ":" + result.CompletionTimeMilliseconds)
                : "none");

        if (cachedSnapshot != null && cachedKey == key)
        {
            return cachedSnapshot;
        }

        cachedSnapshot = Provider.GetLeaderboard(request);
        cachedKey = key;
        return cachedSnapshot;
    }

    public static async Task<DailyLeaderboardSnapshot> RefreshTodaysSnapshotAsync()
    {
        DailyLeaderboardRequest request = BuildTodaysRequest(out _, out _);
        if (Provider is HybridDailyLeaderboardProvider hybrid)
        {
            DailyLeaderboardSnapshot snap = await hybrid.RefreshAsync(request);
            cachedSnapshot = snap;
            cachedKey = request.DayId + "|async|" + request.DailyState;
            GameAnalytics.LogDailyLeaderboardOnlineView(
                request.DayId,
                snap.HasCurrentPlayer,
                snap.CurrentPlayerRank,
                snap.Entries != null ? snap.Entries.Count : 0);
            return snap;
        }

        return GetTodaysSnapshot();
    }

    public static bool TryGetCurrentPlayerRank(out int rank)
    {
        DailyLeaderboardSnapshot snap = GetTodaysSnapshot();
        if (snap != null && snap.HasCurrentPlayer)
        {
            rank = snap.CurrentPlayerRank;
            return true;
        }

        rank = 0;
        return false;
    }

    private static DailyLeaderboardRequest BuildTodaysRequest(
        out bool hasResult,
        out DailyChallengeResult result)
    {
        hasResult = false;
        result = default;
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        if (manager == null)
        {
            return default;
        }

        manager.EnsureDaySynced();
        DailyChallengeConfig config = manager.Config;
        string dayId = manager.CurrentDayId ?? string.Empty;
        string levelAsset = manager.SelectedLevelAssetName ?? string.Empty;
        LevelData level = manager.SelectedLevel;
        int minMoves = level != null ? level.minimumMoves : 0;

        result = default;
        hasResult =
            manager.CurrentState == DailyChallengeState.Completed &&
            manager.TryGetTodaysResult(out result) &&
            result.Completed;

        return new DailyLeaderboardRequest
        {
            DayId = dayId,
            LevelAssetName = levelAsset,
            MinimumMoves = minMoves,
            Config = config,
            HasLocalPlayerResult = hasResult,
            LocalPlayerResult = hasResult ? result : default,
            DailyState = manager.CurrentState
        };
    }

    private static DailyLeaderboardSnapshot EmptySnapshot(string dayId, string level)
    {
        return new DailyLeaderboardSnapshot
        {
            DayId = dayId,
            LevelAssetName = level,
            StatusMessage = "COMPLETE TODAY'S CHALLENGE TO GET YOUR RANK"
        };
    }
}

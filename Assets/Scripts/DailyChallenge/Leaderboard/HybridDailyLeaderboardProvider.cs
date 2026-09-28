using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Combines real online Completed results with client-only simulated fillers
/// until minimum population is reached. Simulated rows are NEVER uploaded.
/// </summary>
public sealed class HybridDailyLeaderboardProvider : IDailyLeaderboardProvider
{
    private readonly SimulatedDailyLeaderboardProvider simulated =
        new SimulatedDailyLeaderboardProvider();

    private readonly OnlineDailyLeaderboardProvider online =
        new OnlineDailyLeaderboardProvider();

    public DailyLeaderboardSnapshot GetLeaderboard(DailyLeaderboardRequest request)
    {
        // Sync facade: use last async snapshot if available; else build simulated-only.
        // Callers that need fresh online data should await RefreshAsync first.
        if (HybridDailyLeaderboardCache.TryGet(request.DayId, out DailyLeaderboardSnapshot cached))
        {
            return ApplyLocalYouOverlay(cached, request);
        }

        return simulated.GetLeaderboard(request);
    }

    public async Task<DailyLeaderboardSnapshot> RefreshAsync(DailyLeaderboardRequest request)
    {
        int minPop = request.Config != null
            ? Mathf.Max(0, request.Config.MinimumLeaderboardPopulation)
            : 30;

        IReadOnlyList<DailyLeaderboardEntry> real =
            await online.FetchRealEntriesAsync(request.DayId, Mathf.Max(minPop, 50));

        var combined = new List<DailyLeaderboardEntry>();
        var usedIds = new HashSet<string>();

        if (real != null)
        {
            for (int i = 0; i < real.Count; i++)
            {
                DailyLeaderboardEntry e = real[i];
                e.IsSimulated = false;
                if (e.Source == default ||
                    e.Source == DailyLeaderboardEntrySource.FutureRemotePlayer)
                {
                    e.Source = e.IsCurrentPlayer
                        ? DailyLeaderboardEntrySource.LocalPlayer
                        : DailyLeaderboardEntrySource.Online;
                }

                combined.Add(e);
                usedIds.Add(e.PlayerId);
            }
        }

        // Ensure local YOU is present if completed locally but not yet in remote list.
        if (request.HasLocalPlayerResult && request.LocalPlayerResult.Completed)
        {
            bool hasYou = false;
            for (int i = 0; i < combined.Count; i++)
            {
                if (combined[i].IsCurrentPlayer)
                {
                    hasYou = true;
                    break;
                }
            }

            if (!hasYou)
            {
                combined.Add(new DailyLeaderboardEntry
                {
                    PlayerId = "local_player",
                    DisplayName = "YOU",
                    Score = request.LocalPlayerResult.Score,
                    Moves = request.LocalPlayerResult.Moves,
                    CompletionTimeMilliseconds =
                        request.LocalPlayerResult.CompletionTimeMilliseconds,
                    IsCurrentPlayer = true,
                    IsSimulated = false,
                    Source = DailyLeaderboardEntrySource.LocalPlayer,
                    Rank = 0
                });
                usedIds.Add("local_player");
            }
        }

        int needSim = Mathf.Max(0, minPop - combined.Count);
        if (needSim > 0)
        {
            // Temporarily build simulated snapshot then take non-YOU rows.
            DailyChallengeConfig cfg = request.Config;
            int previousCount = cfg != null ? cfg.SimulatedLeaderboardPlayerCount : 30;
            // Use provider with request; filter out its YOU (we already handled local).
            var simRequest = request;
            simRequest.HasLocalPlayerResult = false;
            DailyLeaderboardSnapshot simSnap = simulated.GetLeaderboard(simRequest);
            if (simSnap.Entries != null)
            {
                for (int i = 0; i < simSnap.Entries.Count && needSim > 0; i++)
                {
                    DailyLeaderboardEntry e = simSnap.Entries[i];
                    if (!e.IsSimulated || usedIds.Contains(e.PlayerId))
                    {
                        continue;
                    }

                    e.IsCurrentPlayer = false;
                    e.Source = DailyLeaderboardEntrySource.Simulated;
                    e.IsSimulated = true;
                    combined.Add(e);
                    usedIds.Add(e.PlayerId);
                    needSim--;
                }
            }

            // previousCount unused — silence if analyzer complains via reference
            _ = previousCount;
        }

        SimulatedDailyLeaderboardProvider.SortEntries(combined);
        SimulatedDailyLeaderboardProvider.AssignStrictSequentialRanks(combined);

        bool hasPlayer = false;
        int playerRank = 0;
        DailyLeaderboardEntry playerEntry = default;
        for (int i = 0; i < combined.Count; i++)
        {
            if (combined[i].IsCurrentPlayer)
            {
                hasPlayer = true;
                playerRank = combined[i].Rank;
                playerEntry = combined[i];
                break;
            }
        }

        string status;
        if (hasPlayer)
        {
            status = "YOUR RANK #" + playerRank;
        }
        else if (request.DailyState == DailyChallengeState.FailedOrAbandoned)
        {
            status = "NO SCORE TODAY — CHALLENGE NOT COMPLETED";
        }
        else
        {
            status = "COMPLETE TODAY'S CHALLENGE TO GET YOUR RANK";
        }

        var snap = new DailyLeaderboardSnapshot
        {
            DayId = request.DayId ?? string.Empty,
            LevelAssetName = request.LevelAssetName ?? string.Empty,
            MinimumMoves = request.MinimumMoves,
            Entries = combined,
            HasCurrentPlayer = hasPlayer,
            CurrentPlayerRank = playerRank,
            CurrentPlayerEntry = playerEntry,
            StatusMessage = status
        };

        HybridDailyLeaderboardCache.Store(request.DayId, snap);
        return snap;
    }

    private static DailyLeaderboardSnapshot ApplyLocalYouOverlay(
        DailyLeaderboardSnapshot snap,
        DailyLeaderboardRequest request)
    {
        return snap;
    }
}

/// <summary>In-memory cache for last hybrid refresh.</summary>
public static class HybridDailyLeaderboardCache
{
    private static string dayId;
    private static DailyLeaderboardSnapshot snapshot;

    public static void Store(string day, DailyLeaderboardSnapshot snap)
    {
        dayId = day;
        snapshot = snap;
    }

    public static bool TryGet(string day, out DailyLeaderboardSnapshot snap)
    {
        if (snapshot != null && dayId == day)
        {
            snap = snapshot;
            return true;
        }

        snap = null;
        return false;
    }

    public static void Clear()
    {
        dayId = null;
        snapshot = null;
    }
}

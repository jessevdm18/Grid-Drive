using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phase 3 provider: deterministic simulated placeholder players + optional local YOU.
/// Simulated entries are display-fill only — never authoritative for prizes/rewards.
/// Scores always go through <see cref="DailyChallengeScoreCalculator"/>.
/// </summary>
public sealed class SimulatedDailyLeaderboardProvider : IDailyLeaderboardProvider
{
    private const string SeedSalt = "RushOut.DailyLeaderboard.Simulated.v1";
    private const string LocalPlayerId = "local_player";

    public DailyLeaderboardSnapshot GetLeaderboard(DailyLeaderboardRequest request)
    {
        DailyChallengeConfig config = request.Config;
        int simCount = config != null ? Mathf.Max(0, config.SimulatedLeaderboardPlayerCount) : 30;
        int minMoves = Mathf.Max(1, request.MinimumMoves);

        uint seed = DailyChallengeHash.StableHash32(
            (request.DayId ?? string.Empty) + "|" +
            (request.LevelAssetName ?? string.Empty) + "|" +
            SeedSalt);
        var rng = new DailyChallengeDeterministicRng(seed);

        var entries = new List<DailyLeaderboardEntry>(simCount + 1);
        var usedNames = new HashSet<string>();

        for (int i = 0; i < simCount; i++)
        {
            string displayName = PickUniqueName(ref rng, i, usedNames);
            int extraMoves = SampleExtraMoves(ref rng);
            int moves = minMoves + extraMoves;
            long timeMs = SampleCompletionTimeMs(ref rng, extraMoves);

            int score = DailyChallengeScoreCalculator.Calculate(
                config,
                moves,
                timeMs,
                minMoves);

            entries.Add(new DailyLeaderboardEntry
            {
                PlayerId = "sim_" + request.DayId + "_" + i.ToString("D3"),
                DisplayName = displayName,
                Score = score,
                Moves = moves,
                CompletionTimeMilliseconds = timeMs,
                IsCurrentPlayer = false,
                IsSimulated = true,
                Source = DailyLeaderboardEntrySource.Simulated,
                Rank = 0
            });
        }

        bool hasPlayer = false;
        DailyLeaderboardEntry playerEntry = default;
        if (request.HasLocalPlayerResult &&
            request.LocalPlayerResult.Completed &&
            request.DailyState == DailyChallengeState.Completed)
        {
            hasPlayer = true;
            playerEntry = new DailyLeaderboardEntry
            {
                PlayerId = LocalPlayerId,
                DisplayName = "YOU",
                Score = request.LocalPlayerResult.Score,
                Moves = request.LocalPlayerResult.Moves,
                CompletionTimeMilliseconds = request.LocalPlayerResult.CompletionTimeMilliseconds,
                IsCurrentPlayer = true,
                IsSimulated = false,
                Source = DailyLeaderboardEntrySource.LocalPlayer,
                Rank = 0
            };
            entries.Add(playerEntry);
        }

        SortEntries(entries);
        AssignStrictSequentialRanks(entries);

        int playerRank = 0;
        if (hasPlayer)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].IsCurrentPlayer)
                {
                    playerRank = entries[i].Rank;
                    playerEntry = entries[i];
                    break;
                }
            }
        }

        string status = BuildStatusMessage(request, hasPlayer, playerRank);

        return new DailyLeaderboardSnapshot
        {
            DayId = request.DayId ?? string.Empty,
            LevelAssetName = request.LevelAssetName ?? string.Empty,
            MinimumMoves = minMoves,
            Entries = entries,
            HasCurrentPlayer = hasPlayer,
            CurrentPlayerRank = playerRank,
            CurrentPlayerEntry = playerEntry,
            StatusMessage = status
        };
    }

    private static string BuildStatusMessage(
        DailyLeaderboardRequest request,
        bool hasPlayer,
        int playerRank)
    {
        if (hasPlayer)
        {
            return "YOUR RANK #" + playerRank;
        }

        if (request.DailyState == DailyChallengeState.FailedOrAbandoned)
        {
            return "NO SCORE TODAY — CHALLENGE NOT COMPLETED";
        }

        return "COMPLETE TODAY'S CHALLENGE TO GET YOUR RANK";
    }

    private static string PickUniqueName(
        ref DailyChallengeDeterministicRng rng,
        int index,
        HashSet<string> used)
    {
        for (int attempt = 0; attempt < 64; attempt++)
        {
            string name = SimulatedDailyLeaderboardNames.BuildDisplayName(rng, index + attempt * 17);
            if (used.Add(name))
            {
                return name;
            }
        }

        string fallback = "Player " + (index + 1);
        used.Add(fallback);
        return fallback;
    }

    /// <summary>
    /// Approximate distribution:
    /// 10% +0, 25% +1–2, 35% +3–5, 20% +6–9, 10% +10–15.
    /// </summary>
    private static int SampleExtraMoves(ref DailyChallengeDeterministicRng rng)
    {
        int roll = rng.NextInt(0, 100);
        if (roll < 10)
        {
            return 0;
        }

        if (roll < 35)
        {
            return rng.NextInt(1, 3);
        }

        if (roll < 70)
        {
            return rng.NextInt(3, 6);
        }

        if (roll < 90)
        {
            return rng.NextInt(6, 10);
        }

        return rng.NextInt(10, 16);
    }

    /// <summary>
    /// Believable times correlated with extra moves, with variance.
    /// Strongest entries stay beatable (no absurd sub-20s optima).
    /// </summary>
    private static long SampleCompletionTimeMs(
        ref DailyChallengeDeterministicRng rng,
        int extraMoves)
    {
        int minSec;
        int maxSec;

        if (extraMoves <= 0)
        {
            minSec = 28;
            maxSec = 55;
        }
        else if (extraMoves <= 2)
        {
            minSec = 32;
            maxSec = 75;
        }
        else if (extraMoves <= 5)
        {
            minSec = 50;
            maxSec = 140;
        }
        else if (extraMoves <= 9)
        {
            minSec = 90;
            maxSec = 220;
        }
        else
        {
            minSec = 140;
            maxSec = 300;
        }

        // Occasional cross-bracket variance (~15%).
        if (rng.NextFloat01() < 0.15f)
        {
            minSec = Mathf.Max(22, minSec - 12);
            maxSec = Mathf.Min(320, maxSec + 25);
        }

        int seconds = rng.NextInt(minSec, maxSec + 1);
        int centiseconds = rng.NextInt(0, 100);
        return seconds * 1000L + centiseconds * 10L;
    }

    public static void SortEntries(List<DailyLeaderboardEntry> entries)
    {
        entries.Sort(CompareEntries);
    }

    public static int CompareEntries(DailyLeaderboardEntry a, DailyLeaderboardEntry b)
    {
        // Primary: score descending
        int c = b.Score.CompareTo(a.Score);
        if (c != 0)
        {
            return c;
        }

        // Tie-break 1: moves ascending
        c = a.Moves.CompareTo(b.Moves);
        if (c != 0)
        {
            return c;
        }

        // Tie-break 2: time ascending
        c = a.CompletionTimeMilliseconds.CompareTo(b.CompletionTimeMilliseconds);
        if (c != 0)
        {
            return c;
        }

        // Tie-break 3: stable playerId ordinal
        return string.CompareOrdinal(a.PlayerId, b.PlayerId);
    }

    /// <summary>Strict sequential ranks 1..N (no shared ranks).</summary>
    public static void AssignStrictSequentialRanks(List<DailyLeaderboardEntry> entries)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            DailyLeaderboardEntry e = entries[i];
            e.Rank = i + 1;
            entries[i] = e;
        }
    }
}

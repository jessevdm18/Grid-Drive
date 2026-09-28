using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// DEV/local authority wrapping existing PlayerPrefs Daily flow.
/// Not secure — Online mode must be used for production one-attempt guarantees.
/// </summary>
public sealed class LocalSimulatedDailyChallengeAuthority : IDailyChallengeAuthority
{
    public Task<DailyChallengeAuthorityResult> GetCurrentChallengeAsync()
    {
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        manager.EnsureDaySynced();
        LevelData level = manager.SelectedLevel;
        var challenge = new DailyChallengeDescriptor
        {
            DayId = manager.CurrentDayId ?? string.Empty,
            LevelId = level != null ? level.name : manager.SelectedLevelAssetName,
            LevelVersion = level != null ? level.name : string.Empty,
            ScoreVersion = DailyChallengeScoreVersion.Current,
            OpensAtUtc = string.Empty,
            ClosesAtUtc = string.Empty
        };
        return Task.FromResult(DailyChallengeAuthorityResult.Ok(challenge, default));
    }

    public Task<DailyChallengeAuthorityResult> GetAttemptStateAsync(string dayId)
    {
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        manager.EnsureDaySynced();
        var attempt = new DailyChallengeAttemptRecord
        {
            DayId = manager.CurrentDayId,
            UserId = "local",
            LevelId = manager.SelectedLevelAssetName,
            State = MapState(manager.CurrentState)
        };
        if (manager.TryGetTodaysResult(out DailyChallengeResult result) && result.Completed)
        {
            attempt.Moves = result.Moves;
            attempt.CompletionTimeMs = result.CompletionTimeMilliseconds;
            attempt.Score = result.Score;
            attempt.ScoreVersion = DailyChallengeScoreVersion.Current;
        }

        return Task.FromResult(
            DailyChallengeAuthorityResult.Ok(default, attempt));
    }

    public Task<DailyChallengeAuthorityResult> TryStartAttemptAsync(
        DailyChallengeDescriptor challenge)
    {
        // Local start remains on DailyChallengeManager.TryCommitStartAndArmGameplay.
        return Task.FromResult(
            DailyChallengeAuthorityResult.Ok(challenge, new DailyChallengeAttemptRecord
            {
                DayId = challenge.DayId,
                State = DailyChallengeServerAttemptState.Started,
                LevelId = challenge.LevelId
            }));
    }

    public Task<DailyChallengeAuthorityResult> SubmitResultAsync(
        DailyChallengeDescriptor challenge,
        int moves,
        long completionTimeMs,
        int clientComputedScore)
    {
        return Task.FromResult(
            DailyChallengeAuthorityResult.Ok(challenge, new DailyChallengeAttemptRecord
            {
                DayId = challenge.DayId,
                State = DailyChallengeServerAttemptState.Completed,
                Moves = moves,
                CompletionTimeMs = completionTimeMs,
                Score = clientComputedScore,
                ScoreVersion = DailyChallengeScoreVersion.Current,
                LevelId = challenge.LevelId
            }));
    }

    public Task<DailyChallengeAuthorityResult> MarkAbandonedAsync(string dayId)
    {
        return Task.FromResult(
            DailyChallengeAuthorityResult.Ok(default, new DailyChallengeAttemptRecord
            {
                DayId = dayId,
                State = DailyChallengeServerAttemptState.Abandoned
            }));
    }

    public Task<IReadOnlyList<DailyLeaderboardEntry>> GetCompletedLeaderboardAsync(
        string dayId,
        int maxEntries)
    {
        var list = new List<DailyLeaderboardEntry>();
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        if (manager.CurrentState == DailyChallengeState.Completed &&
            manager.TryGetTodaysResult(out DailyChallengeResult result) &&
            result.Completed)
        {
            list.Add(new DailyLeaderboardEntry
            {
                PlayerId = "local_player",
                DisplayName = "YOU",
                Score = result.Score,
                Moves = result.Moves,
                CompletionTimeMilliseconds = result.CompletionTimeMilliseconds,
                IsCurrentPlayer = true,
                IsSimulated = false,
                Source = DailyLeaderboardEntrySource.LocalPlayer,
                Rank = 0
            });
        }

        return Task.FromResult((IReadOnlyList<DailyLeaderboardEntry>)list);
    }

    private static DailyChallengeServerAttemptState MapState(DailyChallengeState state)
    {
        switch (state)
        {
            case DailyChallengeState.InProgress:
                return DailyChallengeServerAttemptState.Started;
            case DailyChallengeState.Completed:
                return DailyChallengeServerAttemptState.Completed;
            case DailyChallengeState.FailedOrAbandoned:
                return DailyChallengeServerAttemptState.Abandoned;
            default:
                return DailyChallengeServerAttemptState.None;
        }
    }
}

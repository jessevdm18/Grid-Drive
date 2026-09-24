using UnityEngine;

/// <summary>
/// Deterministic V1 Daily Challenge score. Tunable via <see cref="DailyChallengeConfig"/>.
/// Safe to reimplement server-side — do not trust a client-submitted score later.
/// </summary>
public static class DailyChallengeScoreCalculator
{
    public struct ScoreBreakdown
    {
        public int ReferenceMoves;
        public int Moves;
        public long CompletionTimeMilliseconds;
        public int MoveOverPar;
        public int MoveScore;
        public int TimeBonus;
        public int TotalScore;
    }

    public static int Calculate(
        DailyChallengeConfig config,
        int moves,
        long completionTimeMilliseconds,
        int levelMinimumMoves)
    {
        return CalculateBreakdown(
            config,
            moves,
            completionTimeMilliseconds,
            levelMinimumMoves).TotalScore;
    }

    public static ScoreBreakdown CalculateBreakdown(
        DailyChallengeConfig config,
        int moves,
        long completionTimeMilliseconds,
        int levelMinimumMoves)
    {
        int baseMove = config != null ? config.BaseMoveScore : 100000;
        int penalty = config != null ? config.MovePenaltyPerExtraMove : 2500;
        int minMoveScore = config != null ? config.MinimumMoveScore : 10000;
        int timeBonusMax = config != null ? config.TimeBonusMax : 20000;
        float timeRef = config != null ? config.TimeReferenceSeconds : 60f;

        int referenceMoves = levelMinimumMoves;
        if (referenceMoves <= 0)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(
                "[DailyChallenge] Invalid minimumMoves=" + levelMinimumMoves +
                " — falling back to moves as reference (no over-par penalty).");
#endif
            referenceMoves = Mathf.Max(1, moves);
        }

        int safeMoves = Mathf.Max(0, moves);
        long safeMs = completionTimeMilliseconds < 0 ? 0 : completionTimeMilliseconds;

        int moveOverPar = Mathf.Max(0, safeMoves - referenceMoves);
        int moveScore = Mathf.Max(minMoveScore, baseMove - moveOverPar * penalty);

        float timeSeconds = safeMs / 1000f;
        float denom = timeRef + timeSeconds;
        int timeBonus = 0;
        if (denom > 0.0001f && timeBonusMax > 0 && timeRef > 0f)
        {
            timeBonus = Mathf.RoundToInt(timeBonusMax * (timeRef / denom));
            timeBonus = Mathf.Clamp(timeBonus, 0, timeBonusMax);
        }

        int total = Mathf.Max(0, moveScore + timeBonus);

        return new ScoreBreakdown
        {
            ReferenceMoves = referenceMoves,
            Moves = safeMoves,
            CompletionTimeMilliseconds = safeMs,
            MoveOverPar = moveOverPar,
            MoveScore = moveScore,
            TimeBonus = timeBonus,
            TotalScore = total
        };
    }
}

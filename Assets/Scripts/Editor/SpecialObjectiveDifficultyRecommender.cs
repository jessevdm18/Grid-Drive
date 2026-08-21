using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Editor-only special-parameter recommendations by LevelDifficulty + minimumMoves.
/// Does not write assets — planner seeds planned values until APPLY.
/// </summary>
public static class SpecialObjectiveDifficultyRecommender
{
    public enum Strictness
    {
        TooEasy = 0,
        Ok = 1,
        VeryStrict = 2,
        Unknown = 3
    }

    public struct TimedRecommendation
    {
        public float recommendedSeconds;
        public float secondsPerMoveUsed;
        public float bufferUsed;
        public Strictness strictness;
    }

    public struct IntRecommendation
    {
        public int recommended;
        public int requiredMinimum;
        public Strictness strictness;
    }

    public static TimedRecommendation RecommendTimed(
        LevelData level,
        LevelDifficulty difficulty,
        float localProgress01,
        SpecialObjectiveDifficultyConfig.Settings settings)
    {
        TimedRecommendation result = new TimedRecommendation
        {
            recommendedSeconds = SpecialMissionProgressionUtility.AmbulanceFallbackSeconds,
            strictness = Strictness.Unknown
        };

        if (level == null)
        {
            return result;
        }

        int minMoves = Mathf.Max(0, level.minimumMoves);
        if (minMoves <= 0)
        {
            return result;
        }

        ResolveTimedKnobs(
            difficulty,
            settings,
            out float secPerMove,
            out float buffer,
            out float minSec,
            out float maxSec
        );

        // Base formula dominates. Late-local nudge: up to ~5% stricter at end of tier.
        float nudgeMax = Mathf.Clamp(settings.localProgressNudge, 0f, 0.05f);
        float nudge = Mathf.Clamp01(localProgress01) * nudgeMax;
        float raw = minMoves * secPerMove + buffer;
        raw *= 1f - nudge;
        float clamped = Mathf.Clamp(raw, minSec, maxSec);
        // Integer seconds for authoring.
        result.recommendedSeconds = Mathf.Round(clamped);
        result.secondsPerMoveUsed = secPerMove;
        result.bufferUsed = buffer;
        result.strictness = EvaluateStrictness(
            level.timeLimitSeconds,
            result.recommendedSeconds
        );
        return result;
    }

    public static IntRecommendation RecommendMoveLimit(
        LevelData level,
        LevelDifficulty difficulty,
        float localProgress01,
        SpecialObjectiveDifficultyConfig.Settings settings)
    {
        IntRecommendation result = new IntRecommendation
        {
            recommended = SpecialMissionProgressionUtility.MoveLimitFallbackMoves,
            requiredMinimum = 0,
            strictness = Strictness.Unknown
        };

        if (level == null)
        {
            return result;
        }

        int minMoves = Mathf.Max(0, level.minimumMoves);
        if (minMoves <= 0)
        {
            return result;
        }

        int margin = ResolveMoveLimitMargin(difficulty, settings);
        // Late in tier → slightly less margin (still floored below).
        if (localProgress01 > 0.66f && margin > 0)
        {
            margin = Mathf.Max(0, margin - 1);
        }

        result.requiredMinimum = minMoves;
        // Hard safety: never recommend tighter than minMoves + 2.
        const int MinimumExtraMoves = 2;
        result.recommended = Mathf.Max(minMoves + margin, minMoves + MinimumExtraMoves);
        result.strictness = EvaluateStrictnessInt(
            level.moveLimit,
            result.recommended,
            higherIsEasier: true
        );
        return result;
    }

    public static IntRecommendation RecommendFragile(
        int requiredCargoMoves,
        LevelData level,
        LevelDifficulty difficulty,
        SpecialObjectiveDifficultyConfig.Settings settings)
    {
        IntRecommendation result = new IntRecommendation
        {
            recommended = 0,
            requiredMinimum = Mathf.Max(0, requiredCargoMoves),
            strictness = Strictness.Unknown
        };

        if (requiredCargoMoves <= 1)
        {
            return result;
        }

        int margin = ResolveFragileMargin(difficulty, settings);
        result.recommended = Mathf.Max(requiredCargoMoves, requiredCargoMoves + margin);
        int current = level != null ? level.fragileCargoMoveLimit : 0;
        result.strictness = EvaluateStrictnessInt(
            current,
            result.recommended,
            higherIsEasier: true
        );
        return result;
    }

    public static IntRecommendation RecommendLimited(
        int requiredLimitedMoves,
        LevelData level,
        LevelDifficulty difficulty,
        SpecialObjectiveDifficultyConfig.Settings settings)
    {
        IntRecommendation result = new IntRecommendation
        {
            recommended = 0,
            requiredMinimum = Mathf.Max(0, requiredLimitedMoves),
            strictness = Strictness.Unknown
        };

        if (requiredLimitedMoves <= 0)
        {
            return result;
        }

        int margin = ResolveLimitedMargin(difficulty, settings);
        result.recommended = Mathf.Max(requiredLimitedMoves, requiredLimitedMoves + margin);
        int current = level != null ? level.limitedVehicleMoveLimit : 0;
        result.strictness = EvaluateStrictnessInt(
            current,
            result.recommended,
            higherIsEasier: true
        );
        return result;
    }

    /// <summary>
    /// 0 = first in difficulty-local order, 1 = last.
    /// Unknown / single-level → 0 (full formula, no late nudge).
    /// </summary>
    public static float GetLocalProgress01(
        LevelDatabase database,
        int databaseIndex,
        LevelDifficulty difficulty)
    {
        if (database == null || databaseIndex < 0)
        {
            return 0f;
        }

        List<int> ordered = LevelDifficultyOrder.GetOrderedLevelIndicesForDifficulty(
            database,
            difficulty
        );
        if (ordered == null || ordered.Count <= 1)
        {
            return 0f;
        }

        int position = -1;
        for (int i = 0; i < ordered.Count; i++)
        {
            if (ordered[i] == databaseIndex)
            {
                position = i;
                break;
            }
        }

        if (position < 0)
        {
            return 0f;
        }

        return position / (float)(ordered.Count - 1);
    }

    public static Strictness EvaluateStrictness(float current, float recommended)
    {
        if (recommended <= 0.01f || current <= 0.01f)
        {
            return Strictness.Unknown;
        }

        if (current > recommended * 1.20f)
        {
            return Strictness.TooEasy;
        }

        if (current < recommended * 0.80f)
        {
            return Strictness.VeryStrict;
        }

        return Strictness.Ok;
    }

    public static Strictness EvaluateStrictnessInt(
        int current,
        int recommended,
        bool higherIsEasier)
    {
        if (recommended <= 0 || current <= 0)
        {
            return Strictness.Unknown;
        }

        float ratio = current / (float)recommended;
        if (higherIsEasier)
        {
            if (ratio > 1.20f)
            {
                return Strictness.TooEasy;
            }

            if (ratio < 0.80f)
            {
                return Strictness.VeryStrict;
            }
        }
        else
        {
            if (ratio < 0.80f)
            {
                return Strictness.TooEasy;
            }

            if (ratio > 1.20f)
            {
                return Strictness.VeryStrict;
            }
        }

        return Strictness.Ok;
    }

    public static string StrictnessLabel(Strictness s)
    {
        switch (s)
        {
            case Strictness.TooEasy:
                return "TOO EASY";
            case Strictness.VeryStrict:
                return "TOO STRICT";
            case Strictness.Ok:
                return "OK";
            default:
                return "—";
        }
    }

    public static string BuildProgressionReport(
        LevelDatabase database,
        IList<LevelData> levels,
        System.Func<LevelData, int> getDbIndex,
        System.Func<int, int> getFragileRequired,
        System.Func<int, int> getLimitedRequired)
    {
        SpecialObjectiveDifficultyConfig.Settings settings =
            SpecialObjectiveDifficultyConfig.Settings.Load();
        StringBuilder sb = new StringBuilder(2048);
        sb.AppendLine("SPECIAL DIFFICULTY PROGRESSION REPORT");
        sb.AppendLine("(planned LevelData current vs recommended)");
        sb.AppendLine();

        AppendTimedReport(sb, database, levels, getDbIndex, settings);
        AppendMoveLimitReport(sb, database, levels, getDbIndex, settings);
        AppendFragileReport(sb, levels, getDbIndex, getFragileRequired, settings);
        AppendLimitedReport(sb, levels, getDbIndex, getLimitedRequired, settings);
        AppendCrossTierWarnings(sb, levels, getDbIndex, settings);

        sb.AppendLine();
        sb.AppendLine("NoTouch: no numeric limit — progression via protected vehicle choice.");
        sb.AppendLine("MultiTarget: no numeric limit — progression via target pair / minMoves.");
        return sb.ToString();
    }

    private static void AppendTimedReport(
        StringBuilder sb,
        LevelDatabase database,
        IList<LevelData> levels,
        System.Func<LevelData, int> getDbIndex,
        SpecialObjectiveDifficultyConfig.Settings settings)
    {
        sb.AppendLine("TIMED AMBULANCE");
        for (int d = 0; d < 3; d++)
        {
            LevelDifficulty difficulty = (LevelDifficulty)d;
            float sumMin = 0f;
            float sumCur = 0f;
            float sumRec = 0f;
            int count = 0;

            for (int i = 0; i < levels.Count; i++)
            {
                LevelData level = levels[i];
                if (level == null ||
                    level.objectiveType != LevelObjectiveType.TimedAmbulance ||
                    level.difficulty != difficulty)
                {
                    continue;
                }

                int dbIndex = getDbIndex != null ? getDbIndex(level) : i;
                float progress = GetLocalProgress01(database, dbIndex, difficulty);
                TimedRecommendation rec = RecommendTimed(level, difficulty, progress, settings);
                sumMin += level.minimumMoves;
                sumCur += level.timeLimitSeconds;
                sumRec += rec.recommendedSeconds;
                count++;
            }

            sb.AppendLine(
                "  " + difficulty + ": n=" + count +
                " avgMinMoves=" + Avg(sumMin, count).ToString("0.#") +
                " avgCurrent=" + Avg(sumCur, count).ToString("0.#") + "s" +
                " avgRecommended=" + Avg(sumRec, count).ToString("0.#") + "s"
            );
        }

        sb.AppendLine();
    }

    private static void AppendMoveLimitReport(
        StringBuilder sb,
        LevelDatabase database,
        IList<LevelData> levels,
        System.Func<LevelData, int> getDbIndex,
        SpecialObjectiveDifficultyConfig.Settings settings)
    {
        sb.AppendLine("MOVE LIMIT");
        for (int d = 0; d < 3; d++)
        {
            LevelDifficulty difficulty = (LevelDifficulty)d;
            float sumMin = 0f;
            float sumCur = 0f;
            float sumRec = 0f;
            int count = 0;

            for (int i = 0; i < levels.Count; i++)
            {
                LevelData level = levels[i];
                if (level == null ||
                    level.objectiveType != LevelObjectiveType.MoveLimit ||
                    level.difficulty != difficulty)
                {
                    continue;
                }

                int dbIndex = getDbIndex != null ? getDbIndex(level) : i;
                float progress = GetLocalProgress01(database, dbIndex, difficulty);
                IntRecommendation rec = RecommendMoveLimit(
                    level,
                    difficulty,
                    progress,
                    settings
                );
                sumMin += level.minimumMoves;
                sumCur += level.moveLimit;
                sumRec += rec.recommended;
                count++;
            }

            sb.AppendLine(
                "  " + difficulty + ": n=" + count +
                " avgMinMoves=" + Avg(sumMin, count).ToString("0.#") +
                " avgCurrent=" + Avg(sumCur, count).ToString("0.#") +
                " avgRecommended=" + Avg(sumRec, count).ToString("0.#")
            );
        }

        sb.AppendLine();
    }

    private static void AppendFragileReport(
        StringBuilder sb,
        IList<LevelData> levels,
        System.Func<LevelData, int> getDbIndex,
        System.Func<int, int> getFragileRequired,
        SpecialObjectiveDifficultyConfig.Settings settings)
    {
        sb.AppendLine("FRAGILE CARGO");
        for (int d = 0; d < 3; d++)
        {
            LevelDifficulty difficulty = (LevelDifficulty)d;
            float sumCur = 0f;
            float sumRec = 0f;
            int count = 0;

            for (int i = 0; i < levels.Count; i++)
            {
                LevelData level = levels[i];
                if (level == null ||
                    level.objectiveType != LevelObjectiveType.FragileCargo ||
                    level.difficulty != difficulty)
                {
                    continue;
                }

                int dbIndex = getDbIndex != null ? getDbIndex(level) : i;
                int required = getFragileRequired != null ? getFragileRequired(dbIndex) : 0;
                if (required <= 0)
                {
                    required = Mathf.Max(1, level.fragileCargoMoveLimit - 1);
                }

                IntRecommendation rec = RecommendFragile(
                    required,
                    level,
                    difficulty,
                    settings
                );
                sumCur += level.fragileCargoMoveLimit;
                sumRec += rec.recommended;
                count++;
            }

            sb.AppendLine(
                "  " + difficulty + ": n=" + count +
                " avgCurrent=" + Avg(sumCur, count).ToString("0.#") +
                " avgRecommended=" + Avg(sumRec, count).ToString("0.#")
            );
        }

        sb.AppendLine();
    }

    private static void AppendLimitedReport(
        StringBuilder sb,
        IList<LevelData> levels,
        System.Func<LevelData, int> getDbIndex,
        System.Func<int, int> getLimitedRequired,
        SpecialObjectiveDifficultyConfig.Settings settings)
    {
        sb.AppendLine("LIMITED VEHICLE");
        for (int d = 0; d < 3; d++)
        {
            LevelDifficulty difficulty = (LevelDifficulty)d;
            float sumCur = 0f;
            float sumRec = 0f;
            int count = 0;

            for (int i = 0; i < levels.Count; i++)
            {
                LevelData level = levels[i];
                if (level == null ||
                    level.objectiveType != LevelObjectiveType.LimitedVehicle ||
                    level.difficulty != difficulty)
                {
                    continue;
                }

                int dbIndex = getDbIndex != null ? getDbIndex(level) : i;
                int required = getLimitedRequired != null ? getLimitedRequired(dbIndex) : 0;
                if (required <= 0)
                {
                    required = Mathf.Max(1, level.limitedVehicleMoveLimit);
                }

                IntRecommendation rec = RecommendLimited(
                    required,
                    level,
                    difficulty,
                    settings
                );
                sumCur += level.limitedVehicleMoveLimit;
                sumRec += rec.recommended;
                count++;
            }

            sb.AppendLine(
                "  " + difficulty + ": n=" + count +
                " avgCurrent=" + Avg(sumCur, count).ToString("0.#") +
                " avgRecommended=" + Avg(sumRec, count).ToString("0.#")
            );
        }

        sb.AppendLine();
    }

    private static void AppendCrossTierWarnings(
        StringBuilder sb,
        IList<LevelData> levels,
        System.Func<LevelData, int> getDbIndex,
        SpecialObjectiveDifficultyConfig.Settings settings)
    {
        float easyTimed = AvgCurrentTimed(levels, LevelDifficulty.Easy);
        float hardTimed = AvgCurrentTimed(levels, LevelDifficulty.Hard);
        if (easyTimed > 0f && hardTimed > 0f && hardTimed > easyTimed * 1.05f)
        {
            sb.AppendLine(
                "WARNING: Hard Timed avg current (" + hardTimed.ToString("0.#") +
                "s) is more generous than Easy (" + easyTimed.ToString("0.#") + "s)."
            );
        }

        float easyMl = AvgCurrentMoveLimit(levels, LevelDifficulty.Easy);
        float hardMl = AvgCurrentMoveLimit(levels, LevelDifficulty.Hard);
        // Compare slack (current - minMoves) would be better; use sec/move proxy via recommend.
        _ = getDbIndex;
        _ = settings;
        if (easyMl > 0f && hardMl > 0f && hardMl > easyMl)
        {
            sb.AppendLine(
                "NOTE: Hard MoveLimit raw average (" + hardMl.ToString("0.#") +
                ") > Easy (" + easyMl.ToString("0.#") +
                ") — expected if Hard has higher minMoves; check margins separately."
            );
        }
    }

    private static float AvgCurrentTimed(IList<LevelData> levels, LevelDifficulty difficulty)
    {
        float sum = 0f;
        int n = 0;
        for (int i = 0; i < levels.Count; i++)
        {
            LevelData level = levels[i];
            if (level != null &&
                level.objectiveType == LevelObjectiveType.TimedAmbulance &&
                level.difficulty == difficulty)
            {
                sum += level.timeLimitSeconds;
                n++;
            }
        }

        return Avg(sum, n);
    }

    private static float AvgCurrentMoveLimit(IList<LevelData> levels, LevelDifficulty difficulty)
    {
        float sum = 0f;
        int n = 0;
        for (int i = 0; i < levels.Count; i++)
        {
            LevelData level = levels[i];
            if (level != null &&
                level.objectiveType == LevelObjectiveType.MoveLimit &&
                level.difficulty == difficulty)
            {
                sum += level.moveLimit;
                n++;
            }
        }

        return Avg(sum, n);
    }

    private static float Avg(float sum, int count)
    {
        return count > 0 ? sum / count : 0f;
    }

    private static void ResolveTimedKnobs(
        LevelDifficulty difficulty,
        SpecialObjectiveDifficultyConfig.Settings settings,
        out float secPerMove,
        out float buffer,
        out float minSec,
        out float maxSec)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Medium:
                secPerMove = settings.mediumSecondsPerMove;
                buffer = settings.mediumTimedBuffer;
                minSec = settings.mediumTimedMin;
                maxSec = settings.mediumTimedMax;
                break;
            case LevelDifficulty.Hard:
                secPerMove = settings.hardSecondsPerMove;
                buffer = settings.hardTimedBuffer;
                minSec = settings.hardTimedMin;
                maxSec = settings.hardTimedMax;
                break;
            default:
                secPerMove = settings.easySecondsPerMove;
                buffer = settings.easyTimedBuffer;
                minSec = settings.easyTimedMin;
                maxSec = settings.easyTimedMax;
                break;
        }
    }

    private static int ResolveMoveLimitMargin(
        LevelDifficulty difficulty,
        SpecialObjectiveDifficultyConfig.Settings settings)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Medium:
                return settings.mediumMoveLimitMargin;
            case LevelDifficulty.Hard:
                return settings.hardMoveLimitMargin;
            default:
                return settings.easyMoveLimitMargin;
        }
    }

    private static int ResolveFragileMargin(
        LevelDifficulty difficulty,
        SpecialObjectiveDifficultyConfig.Settings settings)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Medium:
                return settings.mediumFragileMargin;
            case LevelDifficulty.Hard:
                return settings.hardFragileMargin;
            default:
                return settings.easyFragileMargin;
        }
    }

    private static int ResolveLimitedMargin(
        LevelDifficulty difficulty,
        SpecialObjectiveDifficultyConfig.Settings settings)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Medium:
                return settings.mediumLimitedMargin;
            case LevelDifficulty.Hard:
                return settings.hardLimitedMargin;
            default:
                return settings.easyLimitedMargin;
        }
    }
}

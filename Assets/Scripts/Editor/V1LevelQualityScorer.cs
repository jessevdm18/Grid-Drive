using UnityEngine;

/// <summary>
/// Editor-only quality / difficulty-fit / triviality scoring for V1 auto-curation.
/// </summary>
public static class V1LevelQualityScorer
{
    // Soft difficultyScore bands (primary), moves as secondary.
    public const int EasyScoreMax = 900;
    public const int MediumScoreMax = 1800;

    public const int EasyMovesMax = 10;
    public const int MediumMovesMax = 18;

    public struct ScoreBreakdown
    {
        public LevelDifficulty suggestedDifficulty;
        public float difficultyFit;      // 0..1
        public float validityConfidence; // 0..1
        public float solutionQuality;    // 0..1
        public float varietyPotential;   // 0..1
        public float trivialityPenalty;  // 0..1
        public float baseQuality;        // before near-dupe
        public string notes;
    }

    public static ScoreBreakdown Evaluate(LevelData level, LevelFeatureSignature signature)
    {
        ScoreBreakdown b = new ScoreBreakdown
        {
            suggestedDifficulty = LevelDifficulty.Medium,
            notes = string.Empty
        };

        if (level == null || !signature.valid)
        {
            b.validityConfidence = 0f;
            b.baseQuality = 0f;
            b.notes = "invalid";
            return b;
        }

        b.suggestedDifficulty = SuggestDifficulty(signature);
        b.difficultyFit = DifficultyFit(signature, b.suggestedDifficulty);
        b.validityConfidence = 1f;
        b.solutionQuality = SolutionQuality(signature);
        b.varietyPotential = VarietyPotential(signature);
        b.trivialityPenalty = TrivialityPenalty(signature, b.suggestedDifficulty);

        b.baseQuality =
            b.difficultyFit * 0.35f +
            b.validityConfidence * 0.15f +
            b.solutionQuality * 0.30f +
            b.varietyPotential * 0.20f -
            b.trivialityPenalty * 0.45f;

        b.baseQuality = Mathf.Clamp(b.baseQuality, 0f, 1.5f);
        return b;
    }

    public static LevelDifficulty SuggestDifficulty(LevelFeatureSignature s)
    {
        int score = s.difficultyScore;
        int moves = s.minimumMoves;
        int area = s.GridArea;

        // Soft grid nudge (not dominant): larger boards bias slightly harder.
        int scoreAdj = score;
        if (area >= 56)
        {
            scoreAdj += 120;
        }
        else if (area <= 30)
        {
            scoreAdj -= 80;
        }

        if (scoreAdj <= EasyScoreMax && moves <= EasyMovesMax + 2)
        {
            return LevelDifficulty.Easy;
        }

        if (scoreAdj <= MediumScoreMax && moves <= MediumMovesMax + 3)
        {
            return LevelDifficulty.Medium;
        }

        return LevelDifficulty.Hard;
    }

    public static float DifficultyFit(LevelFeatureSignature s, LevelDifficulty tier)
    {
        LevelDifficulty suggested = SuggestDifficulty(s);
        if (suggested == tier)
        {
            return 1f;
        }

        int delta = Mathf.Abs((int)suggested - (int)tier);
        if (delta == 1)
        {
            return 0.55f;
        }

        return 0.15f;
    }

    private static float SolutionQuality(LevelFeatureSignature s)
    {
        float movesNorm = Mathf.Clamp01(s.minimumMoves / 25f);
        float uniqueNorm = Mathf.Clamp01(s.distinctVehiclesMoved / Mathf.Max(1f, s.vehicleCount));
        float blockerNorm = Mathf.Clamp01(s.corridorBlockers / 4f);
        return Mathf.Clamp01(0.45f * movesNorm + 0.35f * uniqueNorm + 0.20f * blockerNorm);
    }

    private static float VarietyPotential(LevelFeatureSignature s)
    {
        float orientMix = 0f;
        if (s.vehicleCount > 0)
        {
            float h = s.horizontalCount / (float)s.vehicleCount;
            orientMix = 1f - Mathf.Abs(0.5f - h) * 2f;
        }

        float lengthMix = 0f;
        if (s.vehicleCount > 0)
        {
            float l3 = s.length3PlusCount / (float)s.vehicleCount;
            lengthMix = Mathf.Clamp01(l3 * 2f);
        }

        return Mathf.Clamp01(0.6f * orientMix + 0.4f * lengthMix);
    }

    private static float TrivialityPenalty(LevelFeatureSignature s, LevelDifficulty tier)
    {
        float penalty = 0f;

        if (s.targetDistanceToExit <= 0 && s.corridorBlockers <= 0)
        {
            penalty += 0.55f;
        }
        else if (s.targetDistanceToExit <= 1 && s.corridorBlockers <= 1)
        {
            penalty += 0.25f;
        }

        if (s.distinctVehiclesMoved > 0 && s.distinctVehiclesMoved <= 2)
        {
            penalty += 0.30f;
        }

        int minMovesForTier = tier == LevelDifficulty.Easy
            ? 3
            : tier == LevelDifficulty.Medium ? 6 : 10;
        if (s.minimumMoves > 0 && s.minimumMoves < minMovesForTier)
        {
            penalty += 0.35f;
        }

        if (s.vehicleCount >= 8 && s.distinctVehiclesMoved > 0 &&
            s.distinctVehiclesMoved <= 3)
        {
            penalty += 0.20f;
        }

        return Mathf.Clamp01(penalty);
    }
}

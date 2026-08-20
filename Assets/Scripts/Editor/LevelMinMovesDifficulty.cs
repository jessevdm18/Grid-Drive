using UnityEngine;

/// <summary>
/// Editor/content classification: difficulty from minimumMoves (hard ranges).
/// Easy 1–9 · Medium 10–15 · Hard 16+ · moves ≤ 0 = invalid.
/// </summary>
public static class LevelMinMovesDifficulty
{
    public const int EasyMinMoves = 1;
    public const int EasyMaxMoves = 9;
    public const int MediumMinMoves = 10;
    public const int MediumMaxMoves = 15;
    public const int HardMinMoves = 16;

    public static string RangesSummary =>
        "Easy " + EasyMinMoves + "–" + EasyMaxMoves +
        " · Medium " + MediumMinMoves + "–" + MediumMaxMoves +
        " · Hard " + HardMinMoves + "+";

    /// <summary>
    /// True when minimumMoves maps to a difficulty tier.
    /// </summary>
    public static bool TryGetDifficultyForMinimumMoves(
        int minimumMoves,
        out LevelDifficulty difficulty)
    {
        if (minimumMoves <= 0)
        {
            difficulty = LevelDifficulty.Easy;
            return false;
        }

        if (minimumMoves <= EasyMaxMoves)
        {
            difficulty = LevelDifficulty.Easy;
            return true;
        }

        if (minimumMoves <= MediumMaxMoves)
        {
            difficulty = LevelDifficulty.Medium;
            return true;
        }

        difficulty = LevelDifficulty.Hard;
        return true;
    }

    /// <summary>
    /// Alias matching planner naming.
    /// </summary>
    public static bool TryClassify(int minimumMoves, out LevelDifficulty difficulty)
    {
        return TryGetDifficultyForMinimumMoves(minimumMoves, out difficulty);
    }

    public static bool FitsDifficulty(int minimumMoves, LevelDifficulty difficulty)
    {
        if (!TryGetDifficultyForMinimumMoves(minimumMoves, out LevelDifficulty expected))
        {
            return false;
        }

        return expected == difficulty;
    }

    /// <summary>
    /// Warning when assigned difficulty does not match minMoves ranges, or empty if OK/invalid.
    /// </summary>
    public static string GetMismatchWarning(LevelDifficulty assigned, int minimumMoves)
    {
        if (minimumMoves <= 0)
        {
            return "Invalid / Analyze (minimumMoves <= 0)";
        }

        if (!TryGetDifficultyForMinimumMoves(minimumMoves, out LevelDifficulty expected))
        {
            return "Invalid / Analyze (minimumMoves <= 0)";
        }

        if (expected == assigned)
        {
            return string.Empty;
        }

        return "Should be " + expected + " (" + DescribeRange(expected) + ")";
    }

    public static string DescribeRange(LevelDifficulty difficulty)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Easy:
                return EasyMinMoves + "–" + EasyMaxMoves;
            case LevelDifficulty.Medium:
                return MediumMinMoves + "–" + MediumMaxMoves;
            default:
                return HardMinMoves + "+";
        }
    }
}

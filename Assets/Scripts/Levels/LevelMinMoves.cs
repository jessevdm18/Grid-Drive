using UnityEngine;

/// <summary>
/// Authoritative minimum-moves / PAR for runtime scoring and WinPanel.
/// Stored LevelData.minimumMoves must be &gt; 0 for a playable level.
/// Never treat -1/0 as a visible PAR or as a valid star threshold base.
/// </summary>
public static class LevelMinMoves
{
    /// <summary>
    /// True when stored solver metadata is usable for stars/PAR.
    /// </summary>
    public static bool IsValid(int minimumMoves)
    {
        return minimumMoves > 0;
    }

    public static bool IsValid(LevelData level)
    {
        return level != null && IsValid(level.minimumMoves);
    }

    /// <summary>
    /// Authoritative PAR for scoring UI. Returns false when invalid (do not show -1).
    /// </summary>
    public static bool TryGetPar(LevelData level, out int parMoves)
    {
        parMoves = 0;
        if (!IsValid(level))
        {
            return false;
        }

        parMoves = level.minimumMoves;
        return true;
    }

    /// <summary>
    /// Inclusive 3★ / 2★ ceilings relative to PAR, by authoritative LevelData.difficulty.
    /// Easy:   3★ ≤ PAR,     2★ ≤ PAR+2
    /// Medium: 3★ ≤ PAR+1,   2★ ≤ PAR+4
    /// Hard:   3★ ≤ PAR+2,   2★ ≤ PAR+6
    /// </summary>
    public static void GetStarThresholds(
        LevelDifficulty difficulty,
        int parMoves,
        out int threeStarMaxInclusive,
        out int twoStarMaxInclusive)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Medium:
                threeStarMaxInclusive = parMoves + 1;
                twoStarMaxInclusive = parMoves + 4;
                break;
            case LevelDifficulty.Hard:
                threeStarMaxInclusive = parMoves + 2;
                twoStarMaxInclusive = parMoves + 6;
                break;
            case LevelDifficulty.Easy:
            default:
                threeStarMaxInclusive = parMoves;
                twoStarMaxInclusive = parMoves + 2;
                break;
        }
    }

    /// <summary>
    /// Star rating using difficulty-specific inclusive PAR offsets.
    /// When par is invalid (minimumMoves &lt;= 0), returns 0★ — not a valid completion.
    /// </summary>
    public static int CalculateStars(int playerMoves, LevelData level, out int parMoves)
    {
        if (!TryGetPar(level, out parMoves))
        {
            parMoves = 0;
            return 0;
        }

        LevelDifficulty difficulty = level != null ? level.difficulty : LevelDifficulty.Easy;
        GetStarThresholds(
            difficulty,
            parMoves,
            out int threeStarMax,
            out int twoStarMax
        );

        if (playerMoves <= threeStarMax)
        {
            return 3;
        }

        if (playerMoves <= twoStarMax)
        {
            return 2;
        }

        return 1;
    }

    /// <summary>
    /// Log once-style diagnostic for invalid metadata.
    /// </summary>
    public static void LogInvalidIfNeeded(
        LevelData level,
        int dbIndex,
        string context)
    {
        if (IsValid(level))
        {
            return;
        }

        string name = level != null ? level.name : "null";
        string difficulty = level != null ? level.difficulty.ToString() : "?";
        int stored = level != null ? level.minimumMoves : 0;
        Debug.LogError(
            "[InvalidMinMoves] context=" + context +
            " asset=" + name +
            " dbIndex=" + dbIndex +
            " difficulty=" + difficulty +
            " storedMinimumMoves=" + stored +
            " — playable levels must have minimumMoves > 0. " +
            "Run Level Content Planner → RECALCULATE MIN MOVES."
        );

        FirebaseManager.ReportNonFatal(
            "InvalidMinMoves context=" + context +
            " asset=" + name +
            " dbIndex=" + dbIndex +
            " stored=" + stored
        );
    }

    /// <summary>
    /// For difficulty ordering: invalid minMoves sort last (not as Easy #1).
    /// </summary>
    public static int SortKey(int minimumMoves)
    {
        return IsValid(minimumMoves) ? minimumMoves : int.MaxValue;
    }
}

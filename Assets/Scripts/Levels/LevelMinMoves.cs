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
    /// Star rating with unchanged thresholds: 3★ ≤ par, 2★ ≤ par+2, else 1★.
    /// When par is invalid, returns 1★ and does not use -1 in comparisons.
    /// </summary>
    public static int CalculateStars(int playerMoves, LevelData level, out int parMoves)
    {
        if (!TryGetPar(level, out parMoves))
        {
            parMoves = 0;
            return 1;
        }

        if (playerMoves <= parMoves)
        {
            return 3;
        }

        if (playerMoves <= parMoves + 2)
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

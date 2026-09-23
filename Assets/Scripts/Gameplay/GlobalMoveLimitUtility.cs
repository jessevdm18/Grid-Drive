using UnityEngine;

/// <summary>
/// Authoritative effective move-limit resolver.
/// Special <see cref="LevelObjectiveType.MoveLimit"/> uses authored LevelData.moveLimit.
/// All other objectives use the forgiving global formula from minimumMoves + difficulty.
/// </summary>
public static class GlobalMoveLimitUtility
{
    public const float EasyMultiplier = 1.75f;
    public const float MediumMultiplier = 1.60f;
    public const float HardMultiplier = 1.50f;
    public const int AdditiveBuffer = 3;

    public enum LimitMode
    {
        /// <summary>No enforceable forgiving/global check (invalid data or null).</summary>
        None = 0,
        /// <summary>Forgiving global formula.</summary>
        Global = 1,
        /// <summary>Special MoveLimit objective — authored LevelData.moveLimit only.</summary>
        SpecialMoveLimit = 2
    }

    /// <summary>
    /// Effective limit for display / special path. For Global mode this is the
    /// forgiving formula. For SpecialMoveLimit this is LevelData.moveLimit.
    /// Mode None → 0 (global failure disabled).
    /// </summary>
    public static int GetEffectiveMoveLimit(LevelData level)
    {
        return GetEffectiveMoveLimit(level, out _);
    }

    public static int GetEffectiveMoveLimit(LevelData level, out LimitMode mode)
    {
        if (level == null)
        {
            mode = LimitMode.None;
            return 0;
        }

        if (level.objectiveType == LevelObjectiveType.MoveLimit)
        {
            mode = LimitMode.SpecialMoveLimit;
            return Mathf.Max(0, level.moveLimit);
        }

        if (!LevelMinMoves.IsValid(level))
        {
            mode = LimitMode.None;
            return 0;
        }

        mode = LimitMode.Global;
        return ComputeForgivingGlobalLimit(level.minimumMoves, level.difficulty);
    }

    /// <summary>
    /// Forgiving global formula. Returns 0 when minimumMoves is invalid (&lt;= 0).
    /// </summary>
    public static int ComputeForgivingGlobalLimit(
        int minimumMoves,
        LevelDifficulty difficulty)
    {
        if (minimumMoves <= 0)
        {
            return 0;
        }

        float multiplier;
        switch (difficulty)
        {
            case LevelDifficulty.Medium:
                multiplier = MediumMultiplier;
                break;
            case LevelDifficulty.Hard:
                multiplier = HardMultiplier;
                break;
            case LevelDifficulty.Easy:
            default:
                multiplier = EasyMultiplier;
                break;
        }

        return Mathf.CeilToInt(minimumMoves * multiplier) + AdditiveBuffer;
    }

    /// <summary>True when the forgiving global failure check should run.</summary>
    public static bool IsGlobalFailureCheckActive(LevelData level)
    {
        GetEffectiveMoveLimit(level, out LimitMode mode);
        return mode == LimitMode.Global;
    }
}

using UnityEngine;

/// <summary>
/// Centrale unlock-drempels voor Easy → Medium → Hard progressie.
/// Counts zijn unieke completed levels per difficulty (afgeleid uit stars + LevelData).
/// </summary>
[CreateAssetMenu(
    fileName = "DifficultyProgressionConfig",
    menuName = "RushOut/Difficulty Progression Config"
)]
public class DifficultyProgressionConfig : ScriptableObject
{
    public const int DefaultMediumRequiredEasy = 3;

    /// <summary>
    /// Kept for asset/field compatibility. Hard unlock no longer requires Easy completions
    /// (Medium already gates Hard). Default 0 = no Easy gate.
    /// </summary>
    public const int DefaultHardRequiredEasy = 0;

    public const int DefaultHardRequiredMedium = 3;

    [Header("Medium Unlock")]
    [Tooltip("Unieke Easy-completions nodig om Medium te unlocken.")]
    [Min(0)]
    public int mediumRequiredEasy = DefaultMediumRequiredEasy;

    [Header("Hard Unlock")]
    [Tooltip(
        "Legacy field. Hard unlock ignores Easy when this is 0 (recommended). " +
        "Medium completions alone unlock Hard."
    )]
    [Min(0)]
    public int hardRequiredEasy = DefaultHardRequiredEasy;

    [Tooltip("Unieke Medium-completions nodig om Hard te unlocken.")]
    [Min(0)]
    public int hardRequiredMedium = DefaultHardRequiredMedium;

    /// <summary>
    /// Pure unlock-evaluatie op basis van unieke completion-counts (geen UI).
    /// </summary>
    public bool IsDifficultyUnlocked(
        LevelDifficulty difficulty,
        int easyCompletedCount,
        int mediumCompletedCount)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Easy:
                return true;

            case LevelDifficulty.Medium:
                return easyCompletedCount >= mediumRequiredEasy;

            case LevelDifficulty.Hard:
                // Medium gate alone is authoritative; Easy gate only if explicitly configured > 0.
                if (mediumCompletedCount < hardRequiredMedium)
                {
                    return false;
                }

                if (hardRequiredEasy <= 0)
                {
                    return true;
                }

                return easyCompletedCount >= hardRequiredEasy;

            default:
                return false;
        }
    }

    /// <summary>
    /// Fallback wanneer geen config-asset is toegewezen (zelfde defaults als asset-velden).
    /// </summary>
    public static bool IsDifficultyUnlockedWithDefaults(
        LevelDifficulty difficulty,
        int easyCompletedCount,
        int mediumCompletedCount)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Easy:
                return true;

            case LevelDifficulty.Medium:
                return easyCompletedCount >= DefaultMediumRequiredEasy;

            case LevelDifficulty.Hard:
                if (mediumCompletedCount < DefaultHardRequiredMedium)
                {
                    return false;
                }

                if (DefaultHardRequiredEasy <= 0)
                {
                    return true;
                }

                return easyCompletedCount >= DefaultHardRequiredEasy;

            default:
                return false;
        }
    }
}

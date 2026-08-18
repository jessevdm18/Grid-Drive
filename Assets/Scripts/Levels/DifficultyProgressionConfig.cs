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
    public const int DefaultMediumRequiredEasy = 10;
    public const int DefaultHardRequiredEasy = 10;
    public const int DefaultHardRequiredMedium = 10;

    [Header("Medium Unlock")]
    [Tooltip("Unieke Easy-completions nodig om Medium te unlocken.")]
    [Min(0)]
    public int mediumRequiredEasy = DefaultMediumRequiredEasy;

    [Header("Hard Unlock")]
    [Tooltip("Unieke Easy-completions nodig (naast Medium) om Hard te unlocken.")]
    [Min(0)]
    public int hardRequiredEasy = DefaultHardRequiredEasy;

    [Tooltip("Unieke Medium-completions nodig (naast Easy) om Hard te unlocken.")]
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
                return easyCompletedCount >= hardRequiredEasy
                    && mediumCompletedCount >= hardRequiredMedium;

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
                return easyCompletedCount >= DefaultHardRequiredEasy
                    && mediumCompletedCount >= DefaultHardRequiredMedium;

            default:
                return false;
        }
    }
}

using UnityEngine;

/// <summary>
/// One-time Medium/Hard unlock toast seen-state (PlayerPrefs).
/// Separate from FeatureTutorialPrefs.
/// </summary>
public static class DifficultyUnlockNoticePrefs
{
    private const string MediumSeenKey = "RushOut_DifficultyUnlockNoticeSeen_Medium";
    private const string HardSeenKey = "RushOut_DifficultyUnlockNoticeSeen_Hard";
    private const string MigratedKey = "RushOut_DifficultyUnlockNotice_MigratedV1";

    public static bool HasSeenMedium()
    {
        return PlayerPrefs.GetInt(MediumSeenKey, 0) == 1;
    }

    public static bool HasSeenHard()
    {
        return PlayerPrefs.GetInt(HardSeenKey, 0) == 1;
    }

    public static bool HasSeen(LevelDifficulty difficulty)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Medium:
                return HasSeenMedium();
            case LevelDifficulty.Hard:
                return HasSeenHard();
            default:
                return true;
        }
    }

    public static void MarkSeen(LevelDifficulty difficulty)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Medium:
                PlayerPrefs.SetInt(MediumSeenKey, 1);
                break;
            case LevelDifficulty.Hard:
                PlayerPrefs.SetInt(HardSeenKey, 1);
                break;
            default:
                return;
        }

        PlayerPrefs.Save();
    }

    public static void ResetMedium()
    {
        PlayerPrefs.DeleteKey(MediumSeenKey);
        PlayerPrefs.Save();
    }

    public static void ResetHard()
    {
        PlayerPrefs.DeleteKey(HardSeenKey);
        PlayerPrefs.Save();
    }

    /// <summary>Editor/fresh-player: clear Medium + Hard seen and migration flag.</summary>
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(MediumSeenKey);
        PlayerPrefs.DeleteKey(HardSeenKey);
        PlayerPrefs.DeleteKey(MigratedKey);
        PlayerPrefs.Save();
    }

    public static bool HasMigrated()
    {
        return PlayerPrefs.GetInt(MigratedKey, 0) == 1;
    }

    /// <summary>
    /// Existing saves that already unlocked Medium/Hard before this feature:
    /// mark notices seen once so we never spam retroactive toasts.
    /// </summary>
    public static void MigrateExistingUnlocks(
        SaveManager saveManager,
        LevelDatabase database,
        DifficultyProgressionConfig config)
    {
        if (PlayerPrefs.GetInt(MigratedKey, 0) == 1)
        {
            return;
        }

        if (saveManager != null && database != null)
        {
            if (saveManager.IsDifficultyUnlocked(
                    LevelDifficulty.Medium,
                    database,
                    config))
            {
                PlayerPrefs.SetInt(MediumSeenKey, 1);
            }

            if (saveManager.IsDifficultyUnlocked(
                    LevelDifficulty.Hard,
                    database,
                    config))
            {
                PlayerPrefs.SetInt(HardSeenKey, 1);
            }
        }

        PlayerPrefs.SetInt(MigratedKey, 1);
        PlayerPrefs.Save();
    }

    public static string MediumKey => MediumSeenKey;
    public static string HardKey => HardSeenKey;
}

using UnityEngine;

/// <summary>
/// Pre-release content/save compatibility for index-based MainLevelDatabase saves.
///
/// BUMP <see cref="Current"/> when you rebuild/reorder/remove DB levels so
/// existing devices wipe stale index progression instead of mapping old indices
/// onto new LevelData assets.
///
/// Location: Assets/Scripts/Core/LevelDatabaseContentVersion.cs
///
/// TODO (post-launch): migrate save identity to stable LevelData GUID/id —
/// index-based keys are not safe for public content reordering.
/// </summary>
public static class LevelDatabaseContentVersion
{
    /// <summary>
    /// Bump this intentionally when MainLevelDatabase content/order changes
    /// in a way that invalidates existing index-based PlayerPrefs.
    /// </summary>
    public const int Current = 2;

    public const string PrefsKey = "RushOut_LevelDatabaseContentVersion";

    public static int GetSavedVersion()
    {
        return PlayerPrefs.GetInt(PrefsKey, 0);
    }

    public static void SaveCurrentVersion()
    {
        PlayerPrefs.SetInt(PrefsKey, Current);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// If saved version != <see cref="Current"/>, reset level progression prefs
    /// (not audio/settings), then write Current. Safe to call every startup.
    /// </summary>
    /// <returns>True when a mismatch reset ran.</returns>
    public static bool ApplyIfNeeded()
    {
        int saved = GetSavedVersion();
        if (saved == Current)
        {
            return false;
        }

        Debug.LogWarning(
            "[LevelDatabaseContentVersion]\n" +
            "Mismatch detected — resetting level progression.\n" +
            "SavedVersion=" + saved + "\n" +
            "CurrentVersion=" + Current + "\n" +
            "Reason=Index-based saves are invalid after DB content reorder."
        );

        SaveManager.ResetLevelProgressForContentVersionMismatch();
        DifficultyUnlockNoticePrefs.ResetAll();
        SaveCurrentVersion();

        Debug.Log(
            "[LevelDatabaseContentVersion]\n" +
            "Reset complete.\n" +
            "SavedVersionNow=" + GetSavedVersion() + "\n" +
            "NextRoute=first-launch Easy Level 1 expected"
        );

        return true;
    }
}

using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tools for index-based save vs MainLevelDatabase content compatibility.
/// </summary>
public static class SaveDatabaseCompatibilityEditorMenu
{
    private const string SignaturePrefsKey =
        "RushOut_Editor_MainLevelDatabaseSignature";

    [MenuItem("RushOut/Testing/Log Save/Database Compatibility State", priority = 20)]
    private static void LogCompatibilityState()
    {
        LevelDatabase database = LoadMainLevelDatabase();
        StringBuilder sb = new StringBuilder(1024);

        sb.AppendLine("[SaveDatabaseCompatibility]");
        sb.AppendLine("CurrentContentVersion=" + LevelDatabaseContentVersion.Current);
        sb.AppendLine("SavedContentVersion=" + LevelDatabaseContentVersion.GetSavedVersion());
        sb.AppendLine(
            "Mismatch=" +
            (LevelDatabaseContentVersion.GetSavedVersion() !=
             LevelDatabaseContentVersion.Current)
        );
        sb.AppendLine("DB Count=" + (database != null ? database.LevelCount : 0));

        int currentLevel = SaveManager.EditorGetCurrentLevelOrDefault();
        sb.AppendLine(
            "CurrentLevelKeyPresent=" + SaveManager.HasCurrentLevelKey()
        );
        sb.AppendLine("CurrentLevel=" + currentLevel);

        LevelData asset = database != null ? database.GetLevel(currentLevel) : null;
        sb.AppendLine("CurrentAsset=" + (asset != null ? asset.name : "?"));
        sb.AppendLine(
            "CurrentDifficulty=" +
            (asset != null ? asset.difficulty.ToString() : "?")
        );
        sb.AppendLine(
            "CurrentObjective=" +
            (asset != null ? asset.objectiveType.ToString() : "?")
        );

        if (database != null)
        {
            SaveManager probe = Object.FindAnyObjectByType<SaveManager>();
            // Use a temporary probe via static-friendly counts when possible.
            int easy = CountCompleted(database, LevelDifficulty.Easy);
            int medium = CountCompleted(database, LevelDifficulty.Medium);
            int hard = CountCompleted(database, LevelDifficulty.Hard);
            sb.AppendLine("EasyCompleted=" + easy);
            sb.AppendLine("MediumCompleted=" + medium);
            sb.AppendLine("HardCompleted=" + hard);
            sb.AppendLine(
                "MediumUnlocked=" +
                DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
                    LevelDifficulty.Medium,
                    easy,
                    medium
                )
            );
            sb.AppendLine(
                "HardUnlocked=" +
                DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
                    LevelDifficulty.Hard,
                    easy,
                    medium
                )
            );

            string signature = BuildDatabaseSignature(database);
            string previous = EditorPrefs.GetString(SignaturePrefsKey, string.Empty);
            sb.AppendLine("DbSignature=" + signature);
            if (!string.IsNullOrEmpty(previous) && previous != signature)
            {
                sb.AppendLine(
                    "WARNING=MainLevelDatabase changed since last recorded signature. " +
                    "Consider bumping LevelDatabaseContentVersion.Current " +
                    "(Assets/Scripts/Core/LevelDatabaseContentVersion.cs)."
                );
            }

            EditorPrefs.SetString(SignaturePrefsKey, signature);
            _ = probe;
        }

        sb.AppendLine(
            "Note=Index-based saves; bump Current after DB rebuild/reorder."
        );
        sb.AppendLine(
            "TODO=Post-launch migrate to stable LevelData GUID identity."
        );

        Debug.Log(sb.ToString());
    }

    private static int CountCompleted(LevelDatabase database, LevelDifficulty difficulty)
    {
        if (database == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < database.LevelCount; i++)
        {
            LevelData level = database.GetLevel(i);
            if (level == null || level.difficulty != difficulty)
            {
                continue;
            }

            if (PlayerPrefs.GetInt("LevelStars_" + i, 0) > 0)
            {
                count++;
            }
        }

        return count;
    }

    private static string BuildDatabaseSignature(LevelDatabase database)
    {
        if (database == null)
        {
            return "null";
        }

        StringBuilder sb = new StringBuilder(256);
        sb.Append(database.LevelCount).Append('|');
        for (int i = 0; i < database.LevelCount; i++)
        {
            LevelData level = database.GetLevel(i);
            string path = level != null
                ? AssetDatabase.GetAssetPath(level)
                : "null";
            string guid = string.IsNullOrEmpty(path)
                ? "missing"
                : AssetDatabase.AssetPathToGUID(path);
            sb.Append(guid).Append(';');
        }

        return sb.ToString().GetHashCode().ToString("X8");
    }

    private static LevelDatabase LoadMainLevelDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("MainLevelDatabase t:LevelDatabase");
        if (guids == null || guids.Length == 0)
        {
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<LevelDatabase>(
            AssetDatabase.GUIDToAssetPath(guids[0])
        );
    }
}

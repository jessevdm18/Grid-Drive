using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Read-only reports for difficulty × objective balancing.
/// </summary>
public static class DifficultyProgressionEditorMenu
{
    private const string DatabaseFilter = "MainLevelDatabase t:LevelDatabase";

    [MenuItem("RushOut/Levels/Report V1 Grid Distribution Targets")]
    public static void ReportV1GridDistributionTargets()
    {
        string table = V1ReleaseContentPlan.FormatDistributionTable();
        Debug.Log(table);
        EditorUtility.DisplayDialog("V1 Grid Distribution Targets", table, "OK");
    }

    [MenuItem("RushOut/Levels/Report Difficulty Composition")]
    public static void ReportDifficultyComposition()
    {
        LevelDatabase database = LoadMainLevelDatabase();
        if (database == null)
        {
            return;
        }

        int[] tierTotals = new int[3];
        int[,] objectiveByTier = new int[3, 7];

        for (int i = 0; i < database.LevelCount; i++)
        {
            LevelData level = database.GetLevel(i);
            if (level == null)
            {
                continue;
            }

            int tier = (int)level.difficulty;
            if (tier < 0 || tier > 2)
            {
                continue;
            }

            tierTotals[tier]++;

            int objective = (int)level.objectiveType;
            if (objective >= 0 && objective <= 6)
            {
                objectiveByTier[tier, objective]++;
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== Difficulty Composition (" + database.name + ") ===");
        sb.AppendLine("Easy total:   " + tierTotals[0]);
        sb.AppendLine("Medium total: " + tierTotals[1]);
        sb.AppendLine("Hard total:   " + tierTotals[2]);
        sb.AppendLine();

        AppendObjectiveBreakdown(sb, "Easy", objectiveByTier, 0);
        AppendObjectiveBreakdown(sb, "Medium", objectiveByTier, 1);
        AppendObjectiveBreakdown(sb, "Hard", objectiveByTier, 2);

        Debug.Log(sb.ToString());
        EditorUtility.DisplayDialog(
            "Difficulty Composition",
            "Report logged to Console.\n\n" +
            "Easy: " + tierTotals[0] +
            " | Medium: " + tierTotals[1] +
            " | Hard: " + tierTotals[2],
            "OK"
        );
    }

    [MenuItem("RushOut/Levels/Log Difficulty Unlock Status (Live Save)")]
    public static void LogDifficultyUnlockStatus()
    {
        LevelDatabase database = LoadMainLevelDatabase();
        if (database == null)
        {
            return;
        }

        // Tijdelijke SaveManager om PlayerPrefs-stars te lezen (geen scene nodig).
        GameObject probeObject = new GameObject("DifficultyProgression_SaveProbe");
        probeObject.hideFlags = HideFlags.HideAndDontSave;
        SaveManager probe = probeObject.AddComponent<SaveManager>();
        try
        {
            int easy = probe.GetCompletedCount(LevelDifficulty.Easy, database);
            int medium = probe.GetCompletedCount(LevelDifficulty.Medium, database);
            int hard = probe.GetCompletedCount(LevelDifficulty.Hard, database);

            bool easyUnlocked = probe.IsDifficultyUnlocked(
                LevelDifficulty.Easy,
                database,
                null
            );
            bool mediumUnlocked = probe.IsDifficultyUnlocked(
                LevelDifficulty.Medium,
                database,
                null
            );
            bool hardUnlocked = probe.IsDifficultyUnlocked(
                LevelDifficulty.Hard,
                database,
                null
            );

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== Difficulty Unlock Status (live PlayerPrefs) ===");
            sb.AppendLine("Completed Easy:   " + easy);
            sb.AppendLine("Completed Medium: " + medium);
            sb.AppendLine("Completed Hard:   " + hard);
            sb.AppendLine(
                "Unlock Easy/Medium/Hard: " +
                easyUnlocked + " / " + mediumUnlocked + " / " + hardUnlocked
            );
            sb.AppendLine(
                "Defaults: Medium needs " +
                DifficultyProgressionConfig.DefaultMediumRequiredEasy +
                " Easy; Hard needs " +
                DifficultyProgressionConfig.DefaultHardRequiredEasy +
                " Easy + " +
                DifficultyProgressionConfig.DefaultHardRequiredMedium +
                " Medium."
            );

            Debug.Log(sb.ToString());
            EditorUtility.DisplayDialog("Difficulty Unlock Status", sb.ToString(), "OK");
        }
        finally
        {
            Object.DestroyImmediate(probeObject);
        }
    }

    [MenuItem("RushOut/Levels/Log Level Select Difficulty State")]
    public static void LogLevelSelectDifficultyState()
    {
        LevelDatabase database = LoadMainLevelDatabase();
        if (database == null)
        {
            return;
        }

        GameObject probeObject = new GameObject("LevelSelectDifficulty_SaveProbe");
        probeObject.hideFlags = HideFlags.HideAndDontSave;
        SaveManager probe = probeObject.AddComponent<SaveManager>();
        try
        {
            int easyCompleted = probe.GetCompletedCount(LevelDifficulty.Easy, database);
            int mediumCompleted = probe.GetCompletedCount(LevelDifficulty.Medium, database);
            int hardCompleted = probe.GetCompletedCount(LevelDifficulty.Hard, database);

            bool easyUnlocked = probe.IsDifficultyUnlocked(
                LevelDifficulty.Easy,
                database,
                null
            );
            bool mediumUnlocked = probe.IsDifficultyUnlocked(
                LevelDifficulty.Medium,
                database,
                null
            );
            bool hardUnlocked = probe.IsDifficultyUnlocked(
                LevelDifficulty.Hard,
                database,
                null
            );

            int easyLevels = database.GetLevelIndicesByDifficulty(LevelDifficulty.Easy).Count;
            int mediumLevels = database.GetLevelIndicesByDifficulty(LevelDifficulty.Medium).Count;
            int hardLevels = database.GetLevelIndicesByDifficulty(LevelDifficulty.Hard).Count;

            LevelDifficulty lastSelected = probe.GetLastSelectedDifficulty();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== Level Select Difficulty State ===");
            sb.AppendLine("LastSelectedDifficulty=" + lastSelected);
            sb.AppendLine();
            sb.AppendLine("Easy:");
            sb.AppendLine("  Completed=" + easyCompleted);
            sb.AppendLine("  Unlocked=" + easyUnlocked);
            sb.AppendLine("  Levels=" + easyLevels);
            sb.AppendLine();
            sb.AppendLine("Medium:");
            sb.AppendLine("  Completed=" + mediumCompleted);
            sb.AppendLine(
                "  RequiredEasy=" +
                DifficultyProgressionConfig.DefaultMediumRequiredEasy
            );
            sb.AppendLine("  Unlocked=" + mediumUnlocked);
            sb.AppendLine("  Levels=" + mediumLevels);
            sb.AppendLine();
            sb.AppendLine("Hard:");
            sb.AppendLine("  Completed=" + hardCompleted);
            sb.AppendLine(
                "  RequiredEasy=" +
                DifficultyProgressionConfig.DefaultHardRequiredEasy
            );
            sb.AppendLine(
                "  RequiredMedium=" +
                DifficultyProgressionConfig.DefaultHardRequiredMedium
            );
            sb.AppendLine("  Unlocked=" + hardUnlocked);
            sb.AppendLine("  Levels=" + hardLevels);

            Debug.Log(sb.ToString());
            EditorUtility.DisplayDialog(
                "Level Select Difficulty State",
                sb.ToString(),
                "OK"
            );
        }
        finally
        {
            Object.DestroyImmediate(probeObject);
        }
    }

    [MenuItem("RushOut/Levels/Run Difficulty Progression Logic Tests")]
    public static void RunDifficultyProgressionLogicTests()
    {
        int pass = 0;
        int fail = 0;

        void Assert(bool condition, string name)
        {
            if (condition)
            {
                pass++;
            }
            else
            {
                fail++;
                Debug.LogError("FAIL: " + name);
            }
        }

        // Fresh / threshold matrix (pure config, geen save).
        Assert(
            DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
                LevelDifficulty.Easy, 0, 0),
            "Fresh: Easy unlocked"
        );
        Assert(
            !DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
                LevelDifficulty.Medium, 0, 0),
            "Fresh: Medium locked"
        );
        Assert(
            !DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
                LevelDifficulty.Hard, 0, 0),
            "Fresh: Hard locked"
        );
        Assert(
            !DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
                LevelDifficulty.Medium, 9, 0),
            "9 Easy: Medium locked"
        );
        Assert(
            DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
                LevelDifficulty.Medium, 10, 0),
            "10 Easy: Medium unlocked"
        );
        Assert(
            !DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
                LevelDifficulty.Hard, 10, 9),
            "10 Easy + 9 Medium: Hard locked"
        );
        Assert(
            DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
                LevelDifficulty.Hard, 10, 10),
            "10 Easy + 10 Medium: Hard unlocked"
        );

        // Replay does not change unique counts (same count in / same count out).
        Assert(
            DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
                LevelDifficulty.Medium, 10, 0)
            == DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
                LevelDifficulty.Medium, 10, 0),
            "Replay Easy: unlock state stable at count 10"
        );

        string summary =
            "Difficulty progression logic tests: " + pass + " passed, " + fail + " failed.";
        if (fail == 0)
        {
            Debug.Log(summary);
        }
        else
        {
            Debug.LogError(summary);
        }

        EditorUtility.DisplayDialog(
            "Difficulty Progression Tests",
            summary,
            "OK"
        );
    }

    private static void AppendObjectiveBreakdown(
        StringBuilder sb,
        string tierName,
        int[,] objectiveByTier,
        int tierIndex)
    {
        sb.AppendLine("--- " + tierName + " ---");
        sb.AppendLine("  Classic:           " + objectiveByTier[tierIndex, 0]);
        sb.AppendLine("  TimedAmbulance:    " + objectiveByTier[tierIndex, 1]);
        sb.AppendLine("  MoveLimit:         " + objectiveByTier[tierIndex, 2]);
        sb.AppendLine("  MultiTargetRescue: " + objectiveByTier[tierIndex, 3]);
        sb.AppendLine("  NoTouchChallenge:  " + objectiveByTier[tierIndex, 4]);
        sb.AppendLine("  FragileCargo:      " + objectiveByTier[tierIndex, 5]);
        sb.AppendLine("  LimitedVehicle:    " + objectiveByTier[tierIndex, 6]);
        sb.AppendLine();
    }

    private static LevelDatabase LoadMainLevelDatabase()
    {
        string[] guids = AssetDatabase.FindAssets(DatabaseFilter);
        if (guids == null || guids.Length == 0)
        {
            Debug.LogError("DifficultyProgression: MainLevelDatabase niet gevonden.");
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);
    }
}

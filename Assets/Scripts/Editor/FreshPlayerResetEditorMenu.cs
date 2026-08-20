using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only: simulate a brand-new RushOut player install (PlayerPrefs only).
/// Does not touch project assets, curation, EditorPrefs, or MainLevelDatabase.
/// </summary>
public static class FreshPlayerResetEditorMenu
{
    private const string MenuPath = "RushOut/Testing/FULL RESET / SIMULATE FRESH PLAYER";

    [MenuItem(MenuPath, priority = 10)]
    private static void FullFreshPlayerReset()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "FULL FRESH PLAYER RESET",
                "Exit Play Mode before full reset.\n\n" +
                "A mid-play reset would leave live managers half-updated.",
                "OK"
            );
            return;
        }

        bool confirm = EditorUtility.DisplayDialog(
            "FULL FRESH PLAYER RESET",
            "FULL FRESH PLAYER RESET\n\n" +
            "This will erase all local player progress, stars, coins, tutorial state,\n" +
            "difficulty progression and skin ownership for testing.\n\n" +
            "Project assets and V1 curation data will NOT be changed.",
            "RESET PLAYER DATA",
            "CANCEL"
        );

        if (!confirm)
        {
            return;
        }

        // Wipe player prefs. CurrentLevel stays missing until Splash first-launch Prepare
        // writes first ordered Easy DB index (avoids first-launch migration false positive).
        string pendingBefore = V1PlaytestOverride.GetPendingGuid();
        string activeBefore = V1PlaytestOverride.GetActiveGuid();

        SaveManager.EditorResetAllPlayerProgressPrefs();

        ObjectiveTutorialPrefs.ResetAllTutorials();
        FeatureTutorialPrefs.ResetAll();
        DifficultyUnlockNoticePrefs.ResetAll();
        SkinManager.EditorResetSkinPrefsToFreshDefaults();

        // Critical: leftover Pending GUID would override SaveManager in LevelManager.Start.
        V1PlaytestOverride.Clear();

        PlayerPrefs.Save();

        string validation = BuildValidationReport(pendingBefore, activeBefore);
        Debug.Log("[FreshPlayerReset]\nPlayer save reset complete.\n" + validation);
        EditorUtility.DisplayDialog(
            "FULL FRESH PLAYER RESET",
            "Player save reset complete.\n\n" +
            "Next launch: Splash → Gameplay Easy Level 1 " +
            "(first ordered Easy DB index).\n\n" +
            "See Console for validation checklist.",
            "OK"
        );
    }

    private static string BuildValidationReport(string pendingBefore, string activeBefore)
    {
        LevelDatabase database = LoadMainLevelDatabase();
        int freshDb = SaveManager.ResolveFreshStartDatabaseIndex(database);
        LevelData level = database != null ? database.GetLevel(freshDb) : null;
        int display = database != null
            ? LevelDifficultyOrder.GetDifficultyDisplayNumber(database, freshDb)
            : 0;

        bool firstLaunchKey = SaveManager.EditorHasFirstLaunchCompletedKey();
        bool hasProgressEvidence = SaveManager.HasExistingProgressEvidenceStatic();
        bool shouldRoute = !firstLaunchKey && !hasProgressEvidence;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Next launch should follow first-launch flow.");
        sb.AppendLine();
        sb.AppendLine("Validation:");
        sb.AppendLine("FirstLaunchCompleted=" + (firstLaunchKey ? "true" : "false"));
        sb.AppendLine(
            "CurrentLevelKeyPresent=" + PlayerPrefs.HasKey("RushOut_CurrentLevel")
        );
        sb.AppendLine("FreshEasyDbIndex=" + freshDb);
        sb.AppendLine(
            "CurrentDifficulty=" +
            (level != null ? level.difficulty.ToString() : "?")
        );
        sb.AppendLine("CurrentDisplayNumber=" + display);
        sb.AppendLine("Asset=" + (level != null ? level.name : "?"));
        sb.AppendLine(
            "LevelSelectEasy1Match=same asset as FreshEasyDbIndex above"
        );
        sb.AppendLine(
            "V1PendingBeforeClear=" +
            (string.IsNullOrEmpty(pendingBefore) ? "(none)" : pendingBefore)
        );
        sb.AppendLine(
            "V1ActiveBeforeClear=" +
            (string.IsNullOrEmpty(activeBefore) ? "(none)" : activeBefore)
        );
        sb.AppendLine(
            "V1PendingAfterClear=" +
            (V1PlaytestOverride.HasPending ? V1PlaytestOverride.GetPendingGuid() : "(none)")
        );
        sb.AppendLine(
            "V1ActiveAfterClear=" +
            (V1PlaytestOverride.HasActive ? V1PlaytestOverride.GetActiveGuid() : "(none)")
        );
        sb.AppendLine("EasyCompleted=0");
        sb.AppendLine(
            "MediumUnlocked=" +
            DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
                LevelDifficulty.Medium,
                0,
                0
            )
        );
        sb.AppendLine(
            "HardUnlocked=" +
            DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
                LevelDifficulty.Hard,
                0,
                0
            )
        );
        sb.AppendLine("HasProgressEvidence=" + hasProgressEvidence);
        sb.AppendLine("ShouldRouteFirstLaunchToGameplay(expected)=" + shouldRoute);
        sb.AppendLine("Preserved: MusicEnabled / SfxEnabled / EditorPrefs / curation / DB");
        return sb.ToString();
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

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor menus for V1 curation playtest / reject without scene YAML edits.
/// </summary>
public static class V1CurationEditorMenu
{
    [MenuItem("RushOut/Levels/V1 Auto Curate")]
    public static void MenuAutoCurate()
    {
        string report = V1AutoCurator.AutoCurate();
        EditorUtility.DisplayDialog("V1 Auto Curate", TrimForDialog(report), "OK");
    }

    [MenuItem("RushOut/Levels/Reject Selected Level")]
    public static void MenuRejectSelected()
    {
        LevelData level = Selection.activeObject as LevelData;
        if (level == null)
        {
            EditorUtility.DisplayDialog(
                "Reject Selected Level",
                "Select a LevelData asset in the Project window.",
                "OK"
            );
            return;
        }

        if (V1AutoCurator.RejectLevel(level))
        {
            Debug.Log("Rejected: " + level.name);
            EditorUtility.DisplayDialog("Rejected", level.name + " marked Rejected.", "OK");
        }
    }

    [MenuItem("RushOut/Levels/Manual Keep Selected Level")]
    public static void MenuManualKeepSelected()
    {
        LevelData level = Selection.activeObject as LevelData;
        if (level == null)
        {
            EditorUtility.DisplayDialog(
                "Manual Keep",
                "Select a LevelData asset in the Project window.",
                "OK"
            );
            return;
        }

        if (V1AutoCurator.MarkManualKeep(level))
        {
            Debug.Log("ManualKeep: " + level.name);
            EditorUtility.DisplayDialog("Manual Keep", level.name + " marked ManualKeep.", "OK");
        }
    }

    [MenuItem("RushOut/Levels/Replace Rejected Levels")]
    public static void MenuReplaceRejected()
    {
        string report = V1AutoCurator.ReplaceRejected();
        EditorUtility.DisplayDialog("Replace Rejected", TrimForDialog(report), "OK");
    }

    [MenuItem("RushOut/Levels/Apply Final 150 To MainLevelDatabase")]
    public static void MenuApplyFinal()
    {
        string report = V1AutoCurator.ApplyFinalToMainLevelDatabase();
        EditorUtility.DisplayDialog("Apply Final Set", TrimForDialog(report), "OK");
    }

    [MenuItem("RushOut/Levels/Playtest Next Final Level")]
    public static void MenuPlaytestNextFinal()
    {
        string[] dbGuids = AssetDatabase.FindAssets("MainLevelDatabase t:LevelDatabase");
        if (dbGuids == null || dbGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Playtest", "MainLevelDatabase not found.", "OK");
            return;
        }

        LevelDatabase database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(
            AssetDatabase.GUIDToAssetPath(dbGuids[0])
        );
        if (database == null || database.LevelCount == 0)
        {
            EditorUtility.DisplayDialog("Playtest", "Database empty.", "OK");
            return;
        }

        int current = PlayerPrefs.GetInt("RushOut_CurrentLevel", 0);
        int next = current + 1;
        if (next >= database.LevelCount)
        {
            next = 0;
        }

        PlayerPrefs.SetInt("RushOut_CurrentLevel", next);
        PlayerPrefs.Save();

        // Prefer Gameplay scene if present.
        string[] gameplayGuids = AssetDatabase.FindAssets("Gameplay t:Scene");
        if (gameplayGuids != null && gameplayGuids.Length > 0)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(gameplayGuids[0]);
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);
            }
        }

        LevelData level = database.GetLevel(next);
        Debug.Log(
            "Playtest Next Final | index=" + next +
            " | level=" + (level != null ? level.name : "?")
        );
        EditorUtility.DisplayDialog(
            "Playtest Next Final",
            "SaveCurrentLevel → " + next +
            "\n" + (level != null ? level.name : "(null)") +
            "\nOpened Gameplay scene if found. Enter Play Mode to test.",
            "OK"
        );
    }

    [MenuItem("RushOut/Levels/Log V1 Curation Summary")]
    public static void MenuLogSummary()
    {
        V1CurationState state = V1CurationState.LoadOrCreate();
        Debug.Log(
            "V1 Curation | FinalEasy=" + state.CountFinalDifficulty(LevelDifficulty.Easy) +
            " FinalMed=" + state.CountFinalDifficulty(LevelDifficulty.Medium) +
            " FinalHard=" + state.CountFinalDifficulty(LevelDifficulty.Hard) +
            " | Reserve=" + state.CountStatus(V1CurationStatus.Reserve) +
            " Rejected=" + state.CountStatus(V1CurationStatus.Rejected) +
            " ManualKeep=" + state.CountStatus(V1CurationStatus.ManualKeep) +
            "\n" + state.lastReport
        );
    }

    private static string TrimForDialog(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "(empty)";
        }

        if (text.Length <= 1200)
        {
            return text;
        }

        return text.Substring(0, 1200) + "\n…(see Console)";
    }
}

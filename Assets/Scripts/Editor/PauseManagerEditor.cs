#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Edit Mode Pause menu preview — visual authoring only, no gameplay side effects.
/// </summary>
[CustomEditor(typeof(PauseManager))]
public class PauseManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PauseManager pause = (PauseManager)target;
        if (pause == null)
        {
            return;
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Pause Menu Authoring", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "PREVIEW IS NON-DESTRUCTIVE\n" +
            "Shows PausePanel and refreshes only the dynamic restart CostText number.\n" +
            "Does NOT rewrite the authored RESTART label or move CostText / CoinIcon.\n" +
            "Hide restores visibility only — RectTransforms, TMP styling, fonts, " +
            "colors, and sprites you edit are kept (scene is source of truth).\n" +
            "Does not pause the game, spend coins, touch lives, prefs, analytics, or scenes.",
            MessageType.Info);

        using (new EditorGUI.DisabledScope(Application.isPlaying))
        {
            if (pause.IsEditorPausePreviewActive)
            {
                EditorGUILayout.HelpBox("Pause menu preview ACTIVE", MessageType.Warning);
            }

            if (GUILayout.Button("Preview Pause Menu", GUILayout.Height(32)))
            {
                if (pause.PausePanel != null)
                {
                    Undo.RecordObject(pause.PausePanel, "Preview Pause Menu");
                }

                Undo.RecordObject(pause, "Preview Pause Menu");

                GameplayLayoutController layout =
                    Object.FindAnyObjectByType<GameplayLayoutController>();
                if (layout != null && layout.IsAuthoringPreviewActive)
                {
                    layout.EditorRefreshPausePanelPresentation();
                }

                pause.EditorPreviewPauseMenu();
                // Do NOT MarkSceneDirty — visibility preview must not force a save.
                EditorUtility.SetDirty(pause);
            }

            if (GUILayout.Button("Hide Pause Menu Preview", GUILayout.Height(28)))
            {
                if (pause.PausePanel != null)
                {
                    Undo.RecordObject(pause.PausePanel, "Hide Pause Menu Preview");
                }

                Undo.RecordObject(pause, "Hide Pause Menu Preview");
                pause.EditorHidePauseMenuPreview();
                EditorUtility.SetDirty(pause);
            }
        }

        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Pause preview buttons are Edit Mode only. Use the in-game Pause button in Play Mode.",
                MessageType.None);
        }
    }
}

/// <summary>
/// Clears Pause preview before/after Play Mode so ActiveSelf does not pollute Play.
/// Authored styling is never reverted by Hide — only visibility.
/// </summary>
[InitializeOnLoad]
internal static class PausePreviewPlayModeGuard
{
    static PausePreviewPlayModeGuard()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode &&
            state != PlayModeStateChange.EnteredEditMode)
        {
            return;
        }

        PauseManager pause = Object.FindAnyObjectByType<PauseManager>();
        if (pause != null && pause.IsEditorPausePreviewActive)
        {
            pause.EditorHidePauseMenuPreview();
        }
    }
}
#endif

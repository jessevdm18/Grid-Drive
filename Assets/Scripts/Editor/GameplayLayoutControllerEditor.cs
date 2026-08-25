#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Explicit Edit Mode preview/capture with non-destructive preview isolation.
/// </summary>
[CustomEditor(typeof(GameplayLayoutController))]
public class GameplayLayoutControllerEditor : Editor
{
    private GameplayLayoutKind previewKind = GameplayLayoutKind.CompactPhonePortrait;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameplayLayoutController controller = (GameplayLayoutController)target;
        if (controller == null)
        {
            return;
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Layout Authoring", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "PREVIEW IS NON-DESTRUCTIVE\n" +
            "Apply Preview snapshots the live scene first.\n" +
            "Clear Preview / Game View mode mismatch restores that snapshot.\n" +
            "Tall baseline + Compact/Wide captures are never overwritten by preview.\n" +
            "Capture only serializes the selected mode profile.",
            MessageType.Info);

        using (new EditorGUI.DisabledScope(Application.isPlaying))
        {
            previewKind = (GameplayLayoutKind)EditorGUILayout.EnumPopup(
                "Authoring Mode",
                previewKind);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Tall Phone", GUILayout.Height(28)))
            {
                previewKind = GameplayLayoutKind.TallPhonePortrait;
            }

            if (GUILayout.Button("Compact Phone", GUILayout.Height(28)))
            {
                previewKind = GameplayLayoutKind.CompactPhonePortrait;
            }

            if (GUILayout.Button("Wide Tablet", GUILayout.Height(28)))
            {
                previewKind = GameplayLayoutKind.WideTabletLandscape;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            if (GUILayout.Button("Apply Preview Layout", GUILayout.Height(32)))
            {
                Undo.RegisterFullObjectHierarchyUndo(
                    controller.gameObject,
                    "Apply Layout Preview");
                if (controller.transform.root != null)
                {
                    Undo.RegisterFullObjectHierarchyUndo(
                        controller.transform.root.gameObject,
                        "Apply Layout Preview Hierarchy");
                }

                controller.EditorApplyPreview(previewKind);
                // Do NOT MarkSceneDirty — preview must not persist via accidental save.
                EditorUtility.SetDirty(controller);
            }

            bool tallCaptureBlocked = previewKind == GameplayLayoutKind.TallPhonePortrait;
            bool compactCaptureBlocked =
                previewKind == GameplayLayoutKind.CompactPhonePortrait &&
                !controller.AllowCompactCapturedProfile;
            bool wideCaptureBlocked =
                previewKind == GameplayLayoutKind.WideTabletLandscape &&
                !controller.AllowWideCapturedProfile;

            using (new EditorGUI.DisabledScope(
                       tallCaptureBlocked || compactCaptureBlocked || wideCaptureBlocked))
            {
                string captureLabel = tallCaptureBlocked
                    ? "Capture Current Layout (Tall disabled)"
                    : "Capture Current Layout";

                if (GUILayout.Button(captureLabel, GUILayout.Height(32)))
                {
                    Undo.RecordObject(controller, "Capture Layout Profile");
                    controller.EditorCaptureCurrentLayout(previewKind);
                    MarkProfileDirty(controller);
                }

                if (GUILayout.Button("Restore Captured Layout", GUILayout.Height(28)))
                {
                    Undo.RegisterFullObjectHierarchyUndo(
                        controller.gameObject,
                        "Restore Captured Layout");
                    controller.EditorRestoreCapturedLayout(previewKind);
                    EditorUtility.SetDirty(controller);
                }
            }

            if (GUILayout.Button("Clear Preview / Restore Editor Snapshot", GUILayout.Height(28)))
            {
                Undo.RegisterFullObjectHierarchyUndo(
                    controller.gameObject,
                    "Clear Layout Preview");
                if (controller.transform.root != null)
                {
                    Undo.RegisterFullObjectHierarchyUndo(
                        controller.transform.root.gameObject,
                        "Clear Layout Preview Hierarchy");
                }

                controller.EditorClearPreview();
                EditorUtility.SetDirty(controller);
            }

            if (GUILayout.Button("Repair Phone Hierarchy (TopHUD cards)", GUILayout.Height(28)))
            {
                Undo.RegisterFullObjectHierarchyUndo(
                    controller.gameObject,
                    "Repair Phone Hierarchy");
                if (controller.transform.root != null)
                {
                    Undo.RegisterFullObjectHierarchyUndo(
                        controller.transform.root.gameObject,
                        "Repair Phone Hierarchy");
                }

                controller.EditorRepairPhoneHierarchy(markSceneDirty: true);
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Baseline Maintenance", EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Only rebake when Scene view shows the clean Tall phone layout " +
                "(after Clear Preview, with Game view ~1080x2400).",
                MessageType.None);

            if (GUILayout.Button("Rebake Immutable Phone Baseline From Scene", GUILayout.Height(26)))
            {
                if (EditorUtility.DisplayDialog(
                        "Rebake phone baseline?",
                        "This overwrites the immutable tall-phone baseline with " +
                        "whatever is currently in the Scene hierarchy.\n\n" +
                        "Only continue if the Scene shows the correct Tall phone layout " +
                        "with preview CLEARED.",
                        "Rebake",
                        "Cancel"))
                {
                    Undo.RecordObject(controller, "Rebake Phone Baseline");
                    controller.EditorRebakeImmutablePhoneBaseline();
                    MarkProfileDirty(controller);
                }
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Suggested Game View", EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                SuggestedGameView(previewKind),
                MessageType.None);
        }

        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Authoring buttons are disabled in Play Mode.\n" +
                "Runtime mode: " + controller.AppliedKind,
                MessageType.Warning);
        }
        else if (controller.IsAuthoringPreviewActive)
        {
            EditorGUILayout.HelpBox(
                "Preview active: " + controller.AuthoringPreviewKind + "\n" +
                "Changing Game View to another mode auto-clears preview.\n" +
                "Clear Preview restores the exact pre-preview editor snapshot.",
                MessageType.Warning);
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.HelpBox(
            "Wide Pause/Win: tune Wide Tablet Modal Panels fields above.\n" +
            "Live update while Wide Preview (Edit) or Wide Play is active.",
            MessageType.None);

        DrawProfileStatus(controller);
    }

    private static void DrawProfileStatus(GameplayLayoutController controller)
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Layout Status", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Tall Phone", "Immutable baseline");

        string compactStatus;
        if (!controller.AllowCompactCapturedProfile)
        {
            compactStatus = "Disabled — procedural Compact";
        }
        else if (controller.HasCompactCapture)
        {
            int count = controller.CompactPhoneProfile.entries != null
                ? controller.CompactPhoneProfile.entries.Count
                : 0;
            compactStatus = "Captured — runtime enabled (" + count + ")";
        }
        else
        {
            compactStatus = "Not captured — procedural fallback";
        }

        EditorGUILayout.LabelField("Compact Phone", compactStatus);

        string wideStatus;
        if (!controller.AllowWideCapturedProfile)
        {
            wideStatus = "Disabled — procedural Wide";
        }
        else if (controller.HasWideCapture)
        {
            int count = controller.WideTabletProfile.entries != null
                ? controller.WideTabletProfile.entries.Count
                : 0;
            wideStatus = "Captured — runtime enabled (" + count + ")";
        }
        else
        {
            wideStatus = "Not captured — procedural fallback";
        }

        EditorGUILayout.LabelField("Wide Tablet", wideStatus);
    }

    private static string SuggestedGameView(GameplayLayoutKind kind)
    {
        switch (kind)
        {
            case GameplayLayoutKind.CompactPhonePortrait:
                return "Set Game view to 1080x1920 or 480x800 (portrait).";
            case GameplayLayoutKind.WideTabletLandscape:
                return "Set Game view to 2160x1080 or 1280x800 (landscape).";
            default:
                return "Set Game view to 1080x2400 (portrait).";
        }
    }

    private static void MarkProfileDirty(GameplayLayoutController controller)
    {
        EditorUtility.SetDirty(controller);
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        }
    }
}

/// <summary>
/// Clears layout preview when Game View aspect no longer matches the preview mode,
/// and before play mode / scene save so tablet hierarchy cannot stick.
/// </summary>
[InitializeOnLoad]
public static class GameplayLayoutPreviewGuard
{
    private static int lastW = -1;
    private static int lastH = -1;

    static GameplayLayoutPreviewGuard()
    {
        EditorApplication.update += OnEditorUpdate;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorSceneManager.sceneSaving += OnSceneSaving;
    }

    private static void OnEditorUpdate()
    {
        if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        int w;
        int h;
        if (!TryGetGameViewSize(out w, out h))
        {
            w = Screen.width;
            h = Screen.height;
        }

        if (w == lastW && h == lastH)
        {
            return;
        }

        lastW = w;
        lastH = h;

        GameplayLayoutController[] controllers =
            Object.FindObjectsByType<GameplayLayoutController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        for (int i = 0; i < controllers.Length; i++)
        {
            GameplayLayoutController c = controllers[i];
            if (c == null || !c.IsAuthoringPreviewActive)
            {
                continue;
            }

            // Resolve using Game View pixels, not Screen (may track Scene view).
            GameplayLayoutKind resolved = ResolveForSize(w, h);
            if (resolved != c.AuthoringPreviewKind)
            {
                Undo.RegisterFullObjectHierarchyUndo(
                    c.gameObject,
                    "Auto Clear Layout Preview");
                c.EditorAutoClearPreviewForResolutionChange();
                EditorUtility.SetDirty(c);
            }
        }
    }

    private static GameplayLayoutKind ResolveForSize(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return GameplayLayoutKind.TallPhonePortrait;
        }

        float aspect = (float)width / height;
        if (width > height && aspect >= GameplayLayoutMode.DefaultWideAspectThreshold)
        {
            return GameplayLayoutKind.WideTabletLandscape;
        }

        if (width <= height)
        {
            return aspect > GameplayLayoutMode.DefaultTallPhoneMaxAspect
                ? GameplayLayoutKind.CompactPhonePortrait
                : GameplayLayoutKind.TallPhonePortrait;
        }

        return GameplayLayoutKind.CompactPhonePortrait;
    }

    private static bool TryGetGameViewSize(out int width, out int height)
    {
        width = 0;
        height = 0;
        System.Type gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        if (gameViewType == null)
        {
            return false;
        }

        System.Reflection.MethodInfo getSize =
            gameViewType.GetMethod(
                "GetSizeOfMainGameView",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Static);
        if (getSize != null)
        {
            object result = getSize.Invoke(null, null);
            if (result is Vector2 size)
            {
                width = Mathf.RoundToInt(size.x);
                height = Mathf.RoundToInt(size.y);
                return width > 0 && height > 0;
            }
        }

        return false;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
        {
            return;
        }

        ClearAllActivePreviews("play mode");
    }

    private static void OnSceneSaving(UnityEngine.SceneManagement.Scene scene, string path)
    {
        ClearAllActivePreviews("scene save");
    }

    private static void ClearAllActivePreviews(string reason)
    {
        GameplayLayoutController[] controllers =
            Object.FindObjectsByType<GameplayLayoutController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        for (int i = 0; i < controllers.Length; i++)
        {
            GameplayLayoutController c = controllers[i];
            if (c == null)
            {
                continue;
            }

            if (c.IsAuthoringPreviewActive)
            {
                Debug.Log(
                    "[GameplayLayoutPreview] Auto-clearing preview before " + reason);
                c.EditorClearPreview();
            }

            // Always repair phone card parents when leaving edit contamination paths.
            c.EditorRepairPhoneHierarchy(markSceneDirty: reason == "scene save");
            EditorUtility.SetDirty(c);
        }
    }
}
#endif

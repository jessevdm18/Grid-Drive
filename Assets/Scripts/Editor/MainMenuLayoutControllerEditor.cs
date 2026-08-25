#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Non-destructive MainMenu layout preview + explicit Wide capture.
/// </summary>
[CustomEditor(typeof(MainMenuLayoutController))]
public class MainMenuLayoutControllerEditor : Editor
{
    private GameplayLayoutKind previewKind = GameplayLayoutKind.WideTabletLandscape;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MainMenuLayoutController controller = (MainMenuLayoutController)target;
        if (controller == null)
        {
            return;
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("MainMenu Layout Authoring", EditorStyles.boldLabel);

        if (controller.IsAuthoringPreviewActive)
        {
            EditorGUILayout.HelpBox(
                "Preview state: " + controller.AuthoringPreviewKind + " Preview Active",
                MessageType.Warning);

            if (controller.AuthoringPreviewKind == GameplayLayoutKind.WideTabletLandscape)
            {
                EditorGUILayout.HelpBox(
                    "Drag UI freely, then Capture Current Wide Layout.\n" +
                    "Preview applies ONCE — manual edits will not snap back.",
                    MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Preview inactive.\n" +
                "Apply Preview once → drag → Capture Wide → Clear Preview → Save.",
                MessageType.None);
        }

        using (new EditorGUI.DisabledScope(Application.isPlaying))
        {
            previewKind = (GameplayLayoutKind)EditorGUILayout.EnumPopup(
                "Authoring Mode",
                previewKind);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Tall Phone", GUILayout.Height(26)))
            {
                previewKind = GameplayLayoutKind.TallPhonePortrait;
            }

            if (GUILayout.Button("Compact Phone", GUILayout.Height(26)))
            {
                previewKind = GameplayLayoutKind.CompactPhonePortrait;
            }

            if (GUILayout.Button("Wide Tablet", GUILayout.Height(26)))
            {
                previewKind = GameplayLayoutKind.WideTabletLandscape;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            if (GUILayout.Button("Apply Preview Layout", GUILayout.Height(30)))
            {
                Undo.RegisterFullObjectHierarchyUndo(
                    controller.gameObject,
                    "Apply MainMenu Layout Preview");
                if (controller.transform.root != null)
                {
                    Undo.RegisterFullObjectHierarchyUndo(
                        controller.transform.root.gameObject,
                        "Apply MainMenu Layout Preview Hierarchy");
                }

                controller.EditorApplyPreview(previewKind);
                EditorUtility.SetDirty(controller);
            }

            using (new EditorGUI.DisabledScope(
                       !controller.IsAuthoringPreviewActive ||
                       controller.AuthoringPreviewKind !=
                       GameplayLayoutKind.WideTabletLandscape))
            {
                if (GUILayout.Button("Capture Current Wide Layout", GUILayout.Height(30)))
                {
                    Undo.RecordObject(controller, "Capture MainMenu Wide Layout");
                    controller.EditorCaptureCurrentWideLayout();
                    serializedObject.Update();
                    EditorUtility.SetDirty(controller);
                    EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
                }
            }

            if (GUILayout.Button("Clear Preview / Restore Snapshot", GUILayout.Height(26)))
            {
                Undo.RegisterFullObjectHierarchyUndo(
                    controller.gameObject,
                    "Clear MainMenu Layout Preview");
                if (controller.transform.root != null)
                {
                    Undo.RegisterFullObjectHierarchyUndo(
                        controller.transform.root.gameObject,
                        "Clear MainMenu Layout Preview Hierarchy");
                }

                controller.EditorClearPreview();
                EditorUtility.SetDirty(controller);
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(SuggestedGameView(previewKind), MessageType.None);
        }

        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Runtime mode: " + controller.AppliedKind + "\n" +
                "Inspector Wide field edits re-apply in Play only.",
                MessageType.Warning);
        }
    }

    private static string SuggestedGameView(GameplayLayoutKind kind)
    {
        switch (kind)
        {
            case GameplayLayoutKind.CompactPhonePortrait:
                return "Suggested Game View: 1080x1920 or 480x800";
            case GameplayLayoutKind.WideTabletLandscape:
                return "Suggested Game View: 2160x1080 / 1920x1200 / 1280x800";
            default:
                return "Suggested Game View: 1080x2400 (Pixel 7)";
        }
    }
}

/// <summary>
/// Auto-clear MainMenu Wide preview when Game View aspect no longer matches.
/// </summary>
[InitializeOnLoad]
public static class MainMenuLayoutPreviewGuard
{
    private static int lastW = -1;
    private static int lastH = -1;

    static MainMenuLayoutPreviewGuard()
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

        MainMenuLayoutController[] controllers =
            Object.FindObjectsByType<MainMenuLayoutController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        for (int i = 0; i < controllers.Length; i++)
        {
            MainMenuLayoutController c = controllers[i];
            if (c == null || !c.IsAuthoringPreviewActive)
            {
                continue;
            }

            GameplayLayoutKind resolved = ResolveForSize(w, h);
            if (resolved != c.AuthoringPreviewKind)
            {
                Undo.RegisterFullObjectHierarchyUndo(
                    c.gameObject,
                    "Auto Clear MainMenu Layout Preview");
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

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode ||
            state == PlayModeStateChange.EnteredEditMode)
        {
            ClearAllPreviews("play mode change");
        }
    }

    private static void OnSceneSaving(UnityEngine.SceneManagement.Scene scene, string path)
    {
        ClearAllPreviews("scene save");
    }

    private static void ClearAllPreviews(string reason)
    {
        MainMenuLayoutController[] controllers =
            Object.FindObjectsByType<MainMenuLayoutController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        for (int i = 0; i < controllers.Length; i++)
        {
            MainMenuLayoutController c = controllers[i];
            if (c == null || !c.IsAuthoringPreviewActive)
            {
                continue;
            }

            Debug.Log("[MainMenuLayout] Auto-clearing preview before " + reason);
            c.EditorClearPreview();
            EditorUtility.SetDirty(c);
        }
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

        var method = gameViewType.GetMethod(
            "GetSizeOfMainGameView",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (method == null)
        {
            return false;
        }

        object result = method.Invoke(null, null);
        if (result is Vector2 size)
        {
            width = Mathf.RoundToInt(size.x);
            height = Mathf.RoundToInt(size.y);
            return width > 0 && height > 0;
        }

        return false;
    }
}
#endif

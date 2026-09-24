#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One-time visual migration: copy MissionFailedLimitPanel panel/button visuals
/// onto the independent OutOfLivesPanel hierarchy. Does not modify MissionFailed
/// and does not run at runtime.
/// </summary>
[InitializeOnLoad]
public static class OutOfLivesVisualMigrationMenu
{
    private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
    private const string PendingTriggerPath =
        "Assets/Scripts/Editor/OutOfLivesVisualMigration.pending";

    static OutOfLivesVisualMigrationMenu()
    {
        EditorApplication.delayCall += TryConsumePendingTrigger;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.delayCall += TryConsumePendingTrigger;
        }
    }

    private static void TryConsumePendingTrigger()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (!File.Exists(PendingTriggerPath))
        {
            return;
        }

        // Do not delete until migration succeeds — Play Mode may interrupt.
        Debug.Log("[OutOfLivesMigrate] Pending trigger found — migrating Gameplay.unity…");
        try
        {
            if (MigrateGameplayAndSave())
            {
                try
                {
                    if (File.Exists(PendingTriggerPath))
                    {
                        File.Delete(PendingTriggerPath);
                    }

                    AssetDatabase.Refresh();
                }
                catch
                {
                    // Ignore delete failures; idempotent skip-guard handles re-runs.
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[OutOfLivesMigrate] Exception: " + ex);
        }
    }

    [MenuItem("RushOut/UI/Migrate OutOfLives Visuals From MissionFailed", priority = 110)]
    private static void MigrateOpenOrGameplay()
    {
        EditorUtility.DisplayDialog(
            "Gameplay-only migration",
            "This migrates OutOfLives visuals FROM MissionFailed INTO Gameplay only.\n\n" +
            "To copy the final Gameplay Lives UI into MainMenu + LevelSelect, use:\n" +
            "Rush Out → UI → Sync Lives UI From Gameplay To MainMenu + LevelSelect",
            "Continue");

        string report;
        bool ok = MigrateGameplayAndSave(out report);
        EditorUtility.DisplayDialog(
            ok ? "OutOfLives Visuals Migrated" : "Migration Failed",
            report,
            "OK");
    }

    /// <summary>
    /// Batch entry: Unity -batchmode -executeMethod OutOfLivesVisualMigrationMenu.MigrateGameplayBatch
    /// </summary>
    public static void MigrateGameplayBatch()
    {
        string report;
        bool ok = MigrateGameplayAndSave(out report);
        Debug.Log(ok
            ? "[OutOfLivesMigrate] SUCCESS\n" + report
            : "[OutOfLivesMigrate] FAIL\n" + report);
        EditorApplication.Exit(ok ? 0 : 1);
    }

    private static bool MigrateGameplayAndSave()
    {
        string report;
        return MigrateGameplayAndSave(out report);
    }

    private static bool MigrateGameplayAndSave(out string report)
    {
        report = string.Empty;
        Scene gameplay = default;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name == "Gameplay")
            {
                gameplay = s;
                break;
            }
        }

        string previousPath = EditorSceneManager.GetActiveScene().path;
        bool openedTransient = false;
        if (!gameplay.IsValid())
        {
            if (!File.Exists(GameplayScenePath))
            {
                report = "Gameplay.unity not found.";
                return false;
            }

            gameplay = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            openedTransient = true;
        }

        bool ok = MigrateScene(gameplay, out report);
        if (ok)
        {
            EditorSceneManager.MarkSceneDirty(gameplay);
            EditorSceneManager.SaveScene(gameplay);
            AssetDatabase.SaveAssets();
            Debug.Log("[OutOfLivesMigrate] Saved Gameplay.unity\n" + report);
        }
        else
        {
            Debug.LogError("[OutOfLivesMigrate] " + report);
        }

        if (openedTransient && !string.IsNullOrEmpty(previousPath) &&
            previousPath != GameplayScenePath)
        {
            EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
        }

        return ok;
    }

    private static bool MigrateScene(Scene scene, out string report)
    {
        report = string.Empty;
        if (!scene.IsValid() || !scene.isLoaded)
        {
            report = "Scene not loaded.";
            return false;
        }

        Transform missionFailed = FindDeep(scene, "MissionFailedLimitPanel");
        OutOfLivesUI outUi = Object.FindAnyObjectByType<OutOfLivesUI>(FindObjectsInactive.Include);
        if (missionFailed == null)
        {
            report = "MissionFailedLimitPanel not found.";
            return false;
        }

        if (outUi == null)
        {
            report = "OutOfLivesUI not found.";
            return false;
        }

        SerializedObject so = new SerializedObject(outUi);
        GameObject panelRoot = so.FindProperty("root").objectReferenceValue as GameObject;
        if (panelRoot == null)
        {
            panelRoot = FindDeepChild(outUi.transform, "OutOfLivesPanel")?.gameObject;
        }

        if (panelRoot == null)
        {
            report = "OutOfLivesPanel root missing.";
            return false;
        }

        // Already migrated? Panel has Settings_Background sprite and UseCoins has Button_Blue.
        Image existingPanelImg = FindDeepChild(panelRoot.transform, "Panel")?.GetComponent<Image>();
        Image existingCoinImg =
            (so.FindProperty("useCoinsButton").objectReferenceValue as Button)?.GetComponent<Image>();
        if (existingPanelImg != null &&
            existingPanelImg.sprite != null &&
            existingPanelImg.sprite.name.IndexOf(
                "Settings_Background", System.StringComparison.OrdinalIgnoreCase) >= 0 &&
            existingCoinImg != null &&
            existingCoinImg.sprite != null &&
            existingCoinImg.sprite.name.IndexOf(
                "Button_Blue", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            report =
                "OutOfLivesPanel already looks migrated " +
                "(panel sprite + coin button sprite present). Skipped.";
            return true;
        }

        Transform srcDim = FindDirect(missionFailed, "DimOverlay");
        Transform srcPanel = FindDirect(missionFailed, "PanelBackground");
        Transform srcTitle = FindDirect(missionFailed, "MissonFailedTitle");
        Transform srcDesc = FindDirect(missionFailed, "MissonFailedDescription");
        Transform srcRestart = FindDirect(missionFailed, "RestartButton");
        Transform srcLevels = FindDirect(missionFailed, "LevelSelectButton");
        Transform srcMenu = FindDirect(missionFailed, "BackButton");

        if (srcDim == null || srcPanel == null || srcRestart == null ||
            srcLevels == null || srcMenu == null)
        {
            report =
                "MissionFailedLimitPanel is missing expected children " +
                "(DimOverlay / PanelBackground / RestartButton / " +
                "LevelSelectButton / BackButton).";
            return false;
        }

        Transform dstDim = ResolveTarget(
            panelRoot.transform,
            so.FindProperty("inputBlocker").objectReferenceValue as GameObject,
            "DimBackground",
            "DimOverlay");
        Transform dstPanel = FindDeepChild(panelRoot.transform, "Panel") ??
                             FindDeepChild(panelRoot.transform, "PanelBackground");

        TextMeshProUGUI dstTitle =
            so.FindProperty("titleText").objectReferenceValue as TextMeshProUGUI;
        TextMeshProUGUI dstBody =
            so.FindProperty("bodyText").objectReferenceValue as TextMeshProUGUI;
        TextMeshProUGUI dstNextLife =
            so.FindProperty("nextLifeLabelText").objectReferenceValue as TextMeshProUGUI;
        TextMeshProUGUI dstCountdown =
            so.FindProperty("countdownText").objectReferenceValue as TextMeshProUGUI;
        TextMeshProUGUI dstStatus =
            so.FindProperty("statusText").objectReferenceValue as TextMeshProUGUI;

        Button dstCoins = so.FindProperty("useCoinsButton").objectReferenceValue as Button;
        Button dstWatch = so.FindProperty("watchAdButton").objectReferenceValue as Button;
        Button dstOk = so.FindProperty("okButton").objectReferenceValue as Button;
        TextMeshProUGUI dstCoinsLabel =
            so.FindProperty("useCoinsButtonLabel").objectReferenceValue as TextMeshProUGUI;
        TextMeshProUGUI dstWatchLabel =
            so.FindProperty("watchAdButtonLabel").objectReferenceValue as TextMeshProUGUI;
        TextMeshProUGUI dstOkLabel =
            so.FindProperty("okButtonLabel").objectReferenceValue as TextMeshProUGUI;

        if (dstDim == null || dstPanel == null || dstCoins == null ||
            dstWatch == null || dstOk == null)
        {
            report = "OutOfLivesPanel is missing Dim/Panel or one of the three buttons.";
            return false;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Migrate OutOfLives Visuals From MissionFailed");

        ReparentUnder(panelRoot.transform, dstPanel);
        if (dstTitle != null)
        {
            ReparentUnder(panelRoot.transform, dstTitle.transform);
        }

        if (dstBody != null)
        {
            ReparentUnder(panelRoot.transform, dstBody.transform);
        }

        if (dstNextLife != null)
        {
            ReparentUnder(panelRoot.transform, dstNextLife.transform);
        }

        if (dstCountdown != null)
        {
            ReparentUnder(panelRoot.transform, dstCountdown.transform);
        }

        if (dstStatus != null)
        {
            ReparentUnder(panelRoot.transform, dstStatus.transform);
        }

        ReparentUnder(panelRoot.transform, dstCoins.transform);
        ReparentUnder(panelRoot.transform, dstWatch.transform);
        ReparentUnder(panelRoot.transform, dstOk.transform);

        CopyImageVisual(srcDim.GetComponent<Image>(), dstDim.GetComponent<Image>());
        StretchFull(dstDim as RectTransform);
        Image dimImage = dstDim.GetComponent<Image>();
        if (dimImage != null)
        {
            dimImage.raycastTarget = true;
        }

        CopyRectTransform(srcPanel as RectTransform, dstPanel as RectTransform);
        CopyImageVisual(srcPanel.GetComponent<Image>(), dstPanel.GetComponent<Image>());

        if (srcTitle != null && dstTitle != null)
        {
            CopyRectTransform(srcTitle as RectTransform, dstTitle.rectTransform);
            CopyTmpVisual(srcTitle.GetComponent<TextMeshProUGUI>(), dstTitle, true);
        }

        if (srcDesc != null && dstBody != null)
        {
            CopyRectTransform(srcDesc as RectTransform, dstBody.rectTransform);
            CopyTmpVisual(srcDesc.GetComponent<TextMeshProUGUI>(), dstBody, true);
        }

        MigrateButton(srcRestart, dstCoins, dstCoinsLabel);
        MigrateButton(srcLevels, dstWatch, dstWatchLabel);
        MigrateButton(srcMenu, dstOk, dstOkLabel);

        PlaceOutOfLivesOnlyText(
            dstNextLife,
            dstCountdown,
            dstStatus,
            dstBody != null ? dstBody.rectTransform : null,
            dstCoins.transform as RectTransform);

        so.Update();
        so.FindProperty("root").objectReferenceValue = panelRoot;
        so.FindProperty("inputBlocker").objectReferenceValue = dstDim.gameObject;
        if (dstTitle != null)
        {
            so.FindProperty("titleText").objectReferenceValue = dstTitle;
        }

        if (dstBody != null)
        {
            so.FindProperty("bodyText").objectReferenceValue = dstBody;
        }

        if (dstNextLife != null)
        {
            so.FindProperty("nextLifeLabelText").objectReferenceValue = dstNextLife;
        }

        if (dstCountdown != null)
        {
            so.FindProperty("countdownText").objectReferenceValue = dstCountdown;
        }

        if (dstStatus != null)
        {
            so.FindProperty("statusText").objectReferenceValue = dstStatus;
        }

        so.FindProperty("useCoinsButton").objectReferenceValue = dstCoins;
        so.FindProperty("watchAdButton").objectReferenceValue = dstWatch;
        so.FindProperty("okButton").objectReferenceValue = dstOk;
        if (dstCoinsLabel != null)
        {
            so.FindProperty("useCoinsButtonLabel").objectReferenceValue = dstCoinsLabel;
        }

        if (dstWatchLabel != null)
        {
            so.FindProperty("watchAdButtonLabel").objectReferenceValue = dstWatchLabel;
        }

        if (dstOkLabel != null)
        {
            so.FindProperty("okButtonLabel").objectReferenceValue = dstOkLabel;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(outUi);
        EditorUtility.SetDirty(panelRoot);
        Undo.CollapseUndoOperations(undoGroup);

        MoveLimitMissionUI moveUi =
            Object.FindAnyObjectByType<MoveLimitMissionUI>(FindObjectsInactive.Include);
        if (moveUi != null)
        {
            SerializedObject moveSo = new SerializedObject(moveUi);
            Object missionPanel =
                moveSo.FindProperty("missionFailedPanel").objectReferenceValue;
            if (missionPanel == null ||
                ((GameObject)missionPanel).transform != missionFailed)
            {
                report = "Safety check failed: MoveLimitMissionUI.missionFailedPanel changed.";
                return false;
            }
        }

        report =
            "Migrated OutOfLivesPanel visuals from MissionFailedLimitPanel.\n\n" +
            "Sources → Targets:\n" +
            "  DimOverlay → DimBackground (color/sprite; stretch-full kept)\n" +
            "  PanelBackground → Panel (Image + RectTransform)\n" +
            "  RestartButton → UseCoinsButton\n" +
            "  LevelSelectButton → WatchAdButton\n" +
            "  BackButton → OkButton\n" +
            "  MissonFailedTitle → TitleText (style+RT; text preserved)\n" +
            "  MissonFailedDescription → MessageText (style+RT; text preserved)\n\n" +
            "OutOfLives copy/status/countdown preserved.\n" +
            "MissionFailedLimitPanel unchanged.\n" +
            "No runtime sync — edit OutOfLives RectTransforms freely after this.";
        return true;
    }

    private static void MigrateButton(
        Transform sourceButton,
        Button targetButton,
        TextMeshProUGUI targetLabel)
    {
        if (sourceButton == null || targetButton == null)
        {
            return;
        }

        Undo.RecordObject(targetButton.gameObject, "Migrate OutOfLives Button");

        CopyRectTransform(
            sourceButton as RectTransform,
            targetButton.transform as RectTransform);
        CopyImageVisual(
            sourceButton.GetComponent<Image>(),
            targetButton.GetComponent<Image>());
        CopyButtonVisual(sourceButton.GetComponent<Button>(), targetButton);

        Shadow srcShadow = sourceButton.GetComponent<Shadow>();
        if (srcShadow != null)
        {
            Shadow dstShadow = targetButton.GetComponent<Shadow>();
            if (dstShadow == null)
            {
                dstShadow = Undo.AddComponent<Shadow>(targetButton.gameObject);
            }

            dstShadow.effectColor = srcShadow.effectColor;
            dstShadow.effectDistance = srcShadow.effectDistance;
            dstShadow.useGraphicAlpha = srcShadow.useGraphicAlpha;
        }

        EnsureButtonExtras(sourceButton.gameObject, targetButton.gameObject);

        TextMeshProUGUI srcLabel = sourceButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (srcLabel != null && targetLabel != null)
        {
            CopyRectTransform(srcLabel.rectTransform, targetLabel.rectTransform);
            CopyTmpVisual(srcLabel, targetLabel, true);
        }
    }

    private static void EnsureButtonExtras(GameObject source, GameObject target)
    {
        if (source.GetComponent<UIButtonSound>() != null &&
            target.GetComponent<UIButtonSound>() == null)
        {
            Undo.AddComponent<UIButtonSound>(target);
        }

        if (source.GetComponent<UIButtonPressAnimation>() != null &&
            target.GetComponent<UIButtonPressAnimation>() == null)
        {
            Undo.AddComponent<UIButtonPressAnimation>(target);
        }
    }

    private static void PlaceOutOfLivesOnlyText(
        TextMeshProUGUI nextLife,
        TextMeshProUGUI countdown,
        TextMeshProUGUI status,
        RectTransform bodyRt,
        RectTransform coinButtonRt)
    {
        float topY = bodyRt != null ? bodyRt.anchoredPosition.y - 80f : 200f;
        float bottomY = coinButtonRt != null ? coinButtonRt.anchoredPosition.y + 120f : 0f;
        float mid = (topY + bottomY) * 0.5f;

        if (nextLife != null)
        {
            ConvertStretchTextToCentered(nextLife.rectTransform, mid + 36f, 420f, 36f);
        }

        if (countdown != null)
        {
            ConvertStretchTextToCentered(countdown.rectTransform, mid - 4f, 420f, 48f);
        }

        if (status != null)
        {
            ConvertStretchTextToCentered(status.rectTransform, mid - 52f, 420f, 36f);
        }
    }

    private static void ConvertStretchTextToCentered(
        RectTransform rt,
        float anchoredY,
        float width,
        float height)
    {
        if (rt == null)
        {
            return;
        }

        Undo.RecordObject(rt, "Place OutOfLives Text");
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(10.3f, anchoredY);
        rt.localScale = Vector3.one;
    }

    private static void ReparentUnder(Transform newParent, Transform child)
    {
        if (child == null || newParent == null || child.parent == newParent)
        {
            return;
        }

        Undo.SetTransformParent(child, newParent, "Reparent OutOfLives child");
        child.SetAsLastSibling();
    }

    private static void CopyRectTransform(RectTransform src, RectTransform dst)
    {
        if (src == null || dst == null)
        {
            return;
        }

        Undo.RecordObject(dst, "Copy RectTransform");
        dst.anchorMin = src.anchorMin;
        dst.anchorMax = src.anchorMax;
        dst.pivot = src.pivot;
        dst.sizeDelta = src.sizeDelta;
        dst.anchoredPosition = src.anchoredPosition;
        dst.localRotation = src.localRotation;
        dst.localScale = src.localScale;
    }

    private static void CopyImageVisual(Image src, Image dst)
    {
        if (src == null || dst == null)
        {
            return;
        }

        Undo.RecordObject(dst, "Copy Image");
        dst.sprite = src.sprite;
        dst.color = src.color;
        dst.material = src.material;
        dst.type = src.type;
        dst.preserveAspect = src.preserveAspect;
        dst.fillCenter = src.fillCenter;
        dst.fillMethod = src.fillMethod;
        dst.fillAmount = src.fillAmount;
        dst.fillClockwise = src.fillClockwise;
        dst.fillOrigin = src.fillOrigin;
        dst.useSpriteMesh = src.useSpriteMesh;
        dst.pixelsPerUnitMultiplier = src.pixelsPerUnitMultiplier;
        dst.raycastTarget = src.raycastTarget;
        // Do not copy alphaHitTestMinimumThreshold — Unity throws if the sprite
        // texture is not readable / not crunch-compressed.
    }

    private static void CopyButtonVisual(Button src, Button dst)
    {
        if (src == null || dst == null)
        {
            return;
        }

        Undo.RecordObject(dst, "Copy Button");
        dst.transition = src.transition;
        dst.colors = src.colors;
        dst.spriteState = src.spriteState;
        dst.animationTriggers = src.animationTriggers;
        dst.navigation = src.navigation;
        Graphic g = dst.targetGraphic != null
            ? dst.targetGraphic
            : dst.GetComponent<Graphic>();
        dst.targetGraphic = g;
        dst.onClick.RemoveAllListeners();
    }

    private static void CopyTmpVisual(
        TextMeshProUGUI src,
        TextMeshProUGUI dst,
        bool preserveText)
    {
        if (src == null || dst == null)
        {
            return;
        }

        string keep = dst.text;
        Undo.RecordObject(dst, "Copy TMP Visual");
        dst.font = src.font;
        dst.fontSharedMaterial = src.fontSharedMaterial;
        dst.fontSize = src.fontSize;
        dst.fontStyle = src.fontStyle;
        dst.alignment = src.alignment;
        dst.color = src.color;
        dst.enableAutoSizing = src.enableAutoSizing;
        dst.fontSizeMin = src.fontSizeMin;
        dst.fontSizeMax = src.fontSizeMax;
        dst.characterSpacing = src.characterSpacing;
        dst.wordSpacing = src.wordSpacing;
        dst.lineSpacing = src.lineSpacing;
        dst.textWrappingMode = src.textWrappingMode;
        dst.overflowMode = src.overflowMode;
        dst.margin = src.margin;
        dst.richText = src.richText;
        dst.raycastTarget = false;
        if (preserveText)
        {
            dst.text = keep;
        }
    }

    private static void StretchFull(RectTransform rt)
    {
        if (rt == null)
        {
            return;
        }

        Undo.RecordObject(rt, "Stretch Dim");
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    private static Transform ResolveTarget(
        Transform root,
        GameObject serialized,
        params string[] names)
    {
        if (serialized != null)
        {
            return serialized.transform;
        }

        for (int i = 0; i < names.Length; i++)
        {
            Transform t = FindDeepChild(root, names[i]);
            if (t != null)
            {
                return t;
            }
        }

        return null;
    }

    private static Transform FindDirect(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform c = parent.GetChild(i);
            if (c.name == childName)
            {
                return c;
            }
        }

        return null;
    }

    private static Transform FindDeep(Scene scene, string objectName)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform found = FindDeepChild(roots[i].transform, objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static Transform FindDeepChild(Transform parent, string objectName)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == objectName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
#endif

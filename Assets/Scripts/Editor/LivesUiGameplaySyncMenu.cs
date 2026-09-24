#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One-time / idempotent Editor sync: copy authored LivesHUD + OutOfLivesUI visuals
/// from Gameplay.unity into MainMenu.unity and LevelSelect.unity.
/// Gameplay is never modified. No runtime layout sync.
/// </summary>
[InitializeOnLoad]
public static class LivesUiGameplaySyncMenu
{
    private const string GameplayPath = "Assets/Scenes/Gameplay.unity";
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
    private const string LevelSelectPath = "Assets/Scenes/LevelSelect.unity";
    private const string PendingTriggerPath =
        "Assets/Scripts/Editor/LivesUiGameplaySync.pending";

    static LivesUiGameplaySyncMenu()
    {
        EditorApplication.delayCall += TryConsumePendingTrigger;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += TryConsumePendingTrigger;
            }
        };
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

        Debug.Log("[LivesUiSync] Pending trigger — syncing MainMenu + LevelSelect from Gameplay…");
        try
        {
            string report;
            if (SyncMainMenuAndLevelSelect(out report))
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
                    // Idempotent re-run is safe.
                }

                Debug.Log("[LivesUiSync] SUCCESS\n" + report);
            }
            else
            {
                Debug.LogError("[LivesUiSync] FAIL\n" + report);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[LivesUiSync] Exception: " + ex);
        }
    }

    [MenuItem("RushOut/UI/Sync Lives UI From Gameplay To MainMenu + LevelSelect", priority = 120)]
    private static void SyncMenuItem()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog(
                "Sync Lives UI",
                "Exit Play Mode before syncing authored Lives UI.",
                "OK");
            return;
        }

        string report;
        bool ok = SyncMainMenuAndLevelSelect(out report);
        EditorUtility.DisplayDialog(
            ok ? "Lives UI Synced" : "Sync Failed",
            report,
            "OK");
    }

    /// <summary>
    /// Batch: Unity -batchmode -executeMethod LivesUiGameplaySyncMenu.SyncBatch
    /// </summary>
    public static void SyncBatch()
    {
        string report;
        bool ok = SyncMainMenuAndLevelSelect(out report);
        Debug.Log(ok
            ? "[LivesUiSync] SUCCESS\n" + report
            : "[LivesUiSync] FAIL\n" + report);
        EditorApplication.Exit(ok ? 0 : 1);
    }

    private static bool SyncMainMenuAndLevelSelect(out string report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Gameplay = visual source of truth (not modified).");
        string previousPath = EditorSceneManager.GetActiveScene().path;

        try
        {
            bool okMain = SyncTargetScene(MainMenuPath, "MainMenu", sb);
            bool okSelect = SyncTargetScene(LevelSelectPath, "LevelSelect", sb);
            AssetDatabase.SaveAssets();

            if (!string.IsNullOrEmpty(previousPath) && File.Exists(previousPath))
            {
                EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
            }

            report = sb.ToString();
            return okMain && okSelect;
        }
        catch (System.Exception ex)
        {
            report = "Exception: " + ex;
            Debug.LogException(ex);
            return false;
        }
    }

    private static bool SyncTargetScene(string targetPath, string label, StringBuilder sb)
    {
        sb.AppendLine("--- " + label + " ---");

        Scene target = EditorSceneManager.OpenScene(targetPath, OpenSceneMode.Single);
        Scene gameplay = EditorSceneManager.OpenScene(GameplayPath, OpenSceneMode.Additive);

        try
        {
            LivesHUD srcHud = FindInScene<LivesHUD>(gameplay);
            OutOfLivesUI srcOut = FindInScene<OutOfLivesUI>(gameplay);
            if (srcHud == null || srcOut == null)
            {
                sb.AppendLine("FAIL: Gameplay missing LivesHUD or OutOfLivesUI.");
                return false;
            }

            ReportCanvasScaler(gameplay, "Gameplay", sb);
            ReportCanvasScaler(target, label, sb);

            Transform targetCanvas =
                FindDeep(target, "MainMenuCanvas") ??
                FindDeep(target, "LevelSelectCanvas") ??
                FindFirstCanvas(target);
            if (targetCanvas == null)
            {
                sb.AppendLine("FAIL: No canvas in " + label + ".");
                return false;
            }

            Transform safeArea = EnsureSafeArea(targetCanvas);
            sb.AppendLine("SafeArea parent for LivesHUD: " +
                          (safeArea != null ? GetPath(safeArea) : "missing"));

            LivesHUD dstHud = FindInScene<LivesHUD>(target);
            if (dstHud == null)
            {
                sb.AppendLine("FAIL: " + label + " has no LivesHUD (run Author Lives UI once first).");
                return false;
            }

            OutOfLivesUI dstOut = FindInScene<OutOfLivesUI>(target);
            if (dstOut == null)
            {
                sb.AppendLine("FAIL: " + label + " has no OutOfLivesUI.");
                return false;
            }

            // Cleanup duplicates (keep first found component's GO).
            int removedHud = RemoveDuplicateComponents<LivesHUD>(target, dstHud);
            int removedOut = RemoveDuplicateComponents<OutOfLivesUI>(target, dstOut);
            if (removedHud > 0 || removedOut > 0)
            {
                sb.AppendLine(
                    "Removed duplicates: LivesHUD=" + removedHud +
                    " OutOfLivesUI=" + removedOut);
            }

            SyncLivesHud(srcHud, dstHud, safeArea, sb);
            SyncOutOfLives(srcOut, dstOut, targetCanvas, sb);

            EditorSceneManager.MarkSceneDirty(target);
            EditorSceneManager.SaveScene(target);
            sb.AppendLine("Saved " + targetPath);
            sb.AppendLine("Gameplay untouched.");
            return true;
        }
        finally
        {
            if (gameplay.IsValid())
            {
                EditorSceneManager.CloseScene(gameplay, true);
            }
        }
    }

    private static void ReportCanvasScaler(Scene scene, string label, StringBuilder sb)
    {
        CanvasScaler scaler = null;
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length && scaler == null; i++)
        {
            scaler = roots[i].GetComponentInChildren<CanvasScaler>(true);
        }

        if (scaler == null)
        {
            sb.AppendLine(label + " CanvasScaler: none");
            return;
        }

        sb.AppendLine(
            label + " CanvasScaler: ref=" + scaler.referenceResolution +
            " match=" + scaler.matchWidthOrHeight +
            " mode=" + scaler.uiScaleMode);
    }

    private static string GetPath(Transform t)
    {
        if (t == null)
        {
            return string.Empty;
        }

        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }

    private static Transform EnsureSafeArea(Transform canvas)
    {
        Transform existing = FindDirect(canvas, "SafeArea");
        if (existing != null)
        {
            return existing;
        }

        // Also search deeper in case it exists elsewhere under canvas.
        existing = FindDeepChild(canvas, "SafeArea");
        if (existing != null)
        {
            return existing;
        }

        GameObject go = new GameObject("SafeArea", typeof(RectTransform));
        go.layer = canvas.gameObject.layer;
        go.transform.SetParent(canvas, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        StretchFull(rt);
        Undo.RegisterCreatedObjectUndo(go, "Create SafeArea for LivesHUD");
        return go.transform;
    }

    private static void SyncLivesHud(
        LivesHUD srcHud,
        LivesHUD dstHud,
        Transform safeArea,
        StringBuilder sb)
    {
        Undo.RecordObject(dstHud.gameObject, "Sync LivesHUD");

        if (safeArea != null && dstHud.transform.parent != safeArea)
        {
            Undo.SetTransformParent(dstHud.transform, safeArea, "Parent LivesHUD to SafeArea");
        }

        CopyRectTransform(
            srcHud.transform as RectTransform,
            dstHud.transform as RectTransform);
        CopyImageVisual(srcHud.GetComponent<Image>(), dstHud.GetComponent<Image>());

        // Match by name under HUD root.
        CopyNamedImage(srcHud.transform, dstHud.transform, "LivesIcon");
        CopyNamedTmp(srcHud.transform, dstHud.transform, "LivesText", preserveText: true);
        CopyNamedRect(srcHud.transform, dstHud.transform, "CountdownRoot");

        Transform srcCdRoot = FindDeepChild(srcHud.transform, "CountdownRoot");
        Transform dstCdRoot = FindDeepChild(dstHud.transform, "CountdownRoot");
        if (srcCdRoot != null && dstCdRoot != null)
        {
            dstCdRoot.gameObject.SetActive(srcCdRoot.gameObject.activeSelf);
            CopyNamedTmp(srcCdRoot, dstCdRoot, "RegenTimerText", preserveText: true);
        }

        // Rebind functional refs; keep preview strings (edit-mode only).
        SerializedObject so = new SerializedObject(dstHud);
        TextMeshProUGUI livesText =
            FindDeepChild(dstHud.transform, "LivesText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI regen =
            FindDeepChild(dstHud.transform, "RegenTimerText")?.GetComponent<TextMeshProUGUI>();
        Transform countdownRoot = FindDeepChild(dstHud.transform, "CountdownRoot");
        Image icon = FindDeepChild(dstHud.transform, "LivesIcon")?.GetComponent<Image>();

        so.FindProperty("livesText").objectReferenceValue = livesText;
        so.FindProperty("countdownText").objectReferenceValue = regen;
        so.FindProperty("countdownRoot").objectReferenceValue =
            countdownRoot != null ? countdownRoot.gameObject : null;
        so.FindProperty("livesIcon").objectReferenceValue = icon;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(dstHud);

        sb.AppendLine(
            "LivesHUD synced → parent=" +
            (dstHud.transform.parent != null ? dstHud.transform.parent.name : "null") +
            " pos=" + ((RectTransform)dstHud.transform).anchoredPosition);
    }

    private static void SyncOutOfLives(
        OutOfLivesUI srcUi,
        OutOfLivesUI dstUi,
        Transform targetCanvas,
        StringBuilder sb)
    {
        Undo.RecordObject(dstUi.gameObject, "Sync OutOfLivesUI");

        if (dstUi.transform.parent != targetCanvas)
        {
            Undo.SetTransformParent(dstUi.transform, targetCanvas, "Parent OutOfLivesUI to Canvas");
        }

        StretchFull(dstUi.transform as RectTransform);

        SerializedObject srcSo = new SerializedObject(srcUi);
        SerializedObject dstSo = new SerializedObject(dstUi);

        GameObject srcPanelRoot =
            srcSo.FindProperty("root").objectReferenceValue as GameObject;
        GameObject dstPanelRoot =
            dstSo.FindProperty("root").objectReferenceValue as GameObject;

        if (srcPanelRoot == null)
        {
            srcPanelRoot = FindDeepChild(srcUi.transform, "OutOfLivesPanel")?.gameObject;
        }

        if (dstPanelRoot == null)
        {
            dstPanelRoot = FindDeepChild(dstUi.transform, "OutOfLivesPanel")?.gameObject;
        }

        if (srcPanelRoot == null || dstPanelRoot == null)
        {
            sb.AppendLine("FAIL: OutOfLivesPanel missing on source or target.");
            return;
        }

        StretchFull(dstPanelRoot.transform as RectTransform);

        // Flatten: move content from under Panel up to OutOfLivesPanel (Gameplay layout).
        Transform dstPanelBg = FindDeepChild(dstPanelRoot.transform, "Panel");
        FlattenOutOfLivesContent(dstPanelRoot.transform, dstPanelBg);

        // Ensure named targets exist (find deep).
        string[] contentNames =
        {
            "DimBackground", "Panel", "TitleText", "MessageText",
            "CountdownLabel", "CountdownText", "StatusText",
            "UseCoinsButton", "WatchAdButton", "OkButton"
        };

        foreach (string name in contentNames)
        {
            Transform src = FindDeepChild(srcPanelRoot.transform, name);
            Transform dst = FindDeepChild(dstPanelRoot.transform, name);
            if (src == null || dst == null)
            {
                sb.AppendLine("WARN: missing '" + name + "' src=" + (src != null) +
                              " dst=" + (dst != null));
                continue;
            }

            // Ensure destination is direct child of OutOfLivesPanel (except Dim/Panel already).
            if (dst.parent != dstPanelRoot.transform &&
                name != "Label")
            {
                ReparentKeepLocal(dst, dstPanelRoot.transform);
            }

            CopyRectTransform(src as RectTransform, dst as RectTransform);
            CopyImageVisual(src.GetComponent<Image>(), dst.GetComponent<Image>());

            TextMeshProUGUI srcTmp = src.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dstTmp = dst.GetComponent<TextMeshProUGUI>();
            if (srcTmp != null && dstTmp != null)
            {
                CopyTmpVisual(srcTmp, dstTmp, preserveText: true);
            }

            Button srcBtn = src.GetComponent<Button>();
            Button dstBtn = dst.GetComponent<Button>();
            if (srcBtn != null && dstBtn != null)
            {
                CopyButtonVisual(srcBtn, dstBtn);
                EnsureButtonExtras(src.gameObject, dst.gameObject);
                CopyShadow(src.gameObject, dst.gameObject);

                TextMeshProUGUI srcLabel = src.GetComponentInChildren<TextMeshProUGUI>(true);
                TextMeshProUGUI dstLabel = dst.GetComponentInChildren<TextMeshProUGUI>(true);
                if (srcLabel != null && dstLabel != null)
                {
                    CopyRectTransform(srcLabel.rectTransform, dstLabel.rectTransform);
                    CopyTmpVisual(srcLabel, dstLabel, preserveText: true);
                }
            }

            dst.gameObject.SetActive(src.gameObject.activeSelf);
        }

        // Panel stays inactive by default (popup closed).
        dstPanelRoot.SetActive(false);

        // Rebind functional serialized refs on destination OutOfLivesUI.
        Transform dim = FindDeepChild(dstPanelRoot.transform, "DimBackground");
        TextMeshProUGUI title =
            FindDeepChild(dstPanelRoot.transform, "TitleText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI body =
            FindDeepChild(dstPanelRoot.transform, "MessageText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI nextLife =
            FindDeepChild(dstPanelRoot.transform, "CountdownLabel")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI countdown =
            FindDeepChild(dstPanelRoot.transform, "CountdownText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI status =
            FindDeepChild(dstPanelRoot.transform, "StatusText")?.GetComponent<TextMeshProUGUI>();

        Button useCoins =
            FindDeepChild(dstPanelRoot.transform, "UseCoinsButton")?.GetComponent<Button>();
        Button watchAd =
            FindDeepChild(dstPanelRoot.transform, "WatchAdButton")?.GetComponent<Button>();
        Button ok =
            FindDeepChild(dstPanelRoot.transform, "OkButton")?.GetComponent<Button>();

        dstSo.Update();
        dstSo.FindProperty("root").objectReferenceValue = dstPanelRoot;
        dstSo.FindProperty("inputBlocker").objectReferenceValue =
            dim != null ? dim.gameObject : null;
        dstSo.FindProperty("titleText").objectReferenceValue = title;
        dstSo.FindProperty("bodyText").objectReferenceValue = body;
        dstSo.FindProperty("nextLifeLabelText").objectReferenceValue = nextLife;
        dstSo.FindProperty("countdownText").objectReferenceValue = countdown;
        dstSo.FindProperty("statusText").objectReferenceValue = status;
        dstSo.FindProperty("useCoinsButton").objectReferenceValue = useCoins;
        dstSo.FindProperty("watchAdButton").objectReferenceValue = watchAd;
        dstSo.FindProperty("okButton").objectReferenceValue = ok;
        if (useCoins != null)
        {
            dstSo.FindProperty("useCoinsButtonLabel").objectReferenceValue =
                useCoins.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (watchAd != null)
        {
            dstSo.FindProperty("watchAdButtonLabel").objectReferenceValue =
                watchAd.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (ok != null)
        {
            dstSo.FindProperty("okButtonLabel").objectReferenceValue =
                ok.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        // Preserve OutOfLives-specific copy strings already on the component
        // (title / bodyNoLives / etc.) — do not overwrite from Gameplay if present.
        dstSo.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(dstUi);
        EditorUtility.SetDirty(dstPanelRoot);

        sb.AppendLine(
            "OutOfLivesPanel synced (flat hierarchy, inactive). Buttons: " +
            "UseCoins=" + (useCoins != null) +
            " WatchAd=" + (watchAd != null) +
            " Ok=" + (ok != null));
    }

    private static void FlattenOutOfLivesContent(Transform panelRoot, Transform panelBg)
    {
        if (panelBg == null || panelBg.parent != panelRoot)
        {
            return;
        }

        // Move all non-empty children of Panel up to OutOfLivesPanel.
        List<Transform> toMove = new List<Transform>();
        for (int i = 0; i < panelBg.childCount; i++)
        {
            toMove.Add(panelBg.GetChild(i));
        }

        for (int i = 0; i < toMove.Count; i++)
        {
            ReparentKeepLocal(toMove[i], panelRoot);
        }
    }

    private static void ReparentKeepLocal(Transform child, Transform newParent)
    {
        if (child == null || newParent == null || child.parent == newParent)
        {
            return;
        }

        Undo.SetTransformParent(child, newParent, "Flatten OutOfLives");
        child.SetAsLastSibling();
    }

    private static void CopyNamedImage(Transform srcRoot, Transform dstRoot, string name)
    {
        Transform src = FindDeepChild(srcRoot, name);
        Transform dst = FindDeepChild(dstRoot, name);
        if (src == null || dst == null)
        {
            return;
        }

        CopyRectTransform(src as RectTransform, dst as RectTransform);
        CopyImageVisual(src.GetComponent<Image>(), dst.GetComponent<Image>());
        dst.gameObject.SetActive(src.gameObject.activeSelf);
    }

    private static void CopyNamedTmp(
        Transform srcRoot,
        Transform dstRoot,
        string name,
        bool preserveText)
    {
        Transform src = FindDeepChild(srcRoot, name);
        Transform dst = FindDeepChild(dstRoot, name);
        if (src == null || dst == null)
        {
            return;
        }

        CopyRectTransform(src as RectTransform, dst as RectTransform);
        CopyTmpVisual(
            src.GetComponent<TextMeshProUGUI>(),
            dst.GetComponent<TextMeshProUGUI>(),
            preserveText);
        dst.gameObject.SetActive(src.gameObject.activeSelf);
    }

    private static void CopyNamedRect(Transform srcRoot, Transform dstRoot, string name)
    {
        Transform src = FindDeepChild(srcRoot, name);
        Transform dst = FindDeepChild(dstRoot, name);
        if (src == null || dst == null)
        {
            return;
        }

        CopyRectTransform(src as RectTransform, dst as RectTransform);
        dst.gameObject.SetActive(src.gameObject.activeSelf);
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
        dst.anchoredPosition = src.anchoredPosition;
        dst.sizeDelta = src.sizeDelta;
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
        // Never copy Gameplay persistent onClick — OutOfLivesUI wires at runtime.
        dst.onClick = new Button.ButtonClickedEvent();
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
        Undo.RecordObject(dst, "Copy TMP");
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

    private static void CopyShadow(GameObject source, GameObject target)
    {
        Shadow src = source.GetComponent<Shadow>();
        if (src == null)
        {
            return;
        }

        Shadow dst = target.GetComponent<Shadow>();
        if (dst == null)
        {
            dst = Undo.AddComponent<Shadow>(target);
        }

        dst.effectColor = src.effectColor;
        dst.effectDistance = src.effectDistance;
        dst.useGraphicAlpha = src.useGraphicAlpha;
    }

    private static void StretchFull(RectTransform rt)
    {
        if (rt == null)
        {
            return;
        }

        Undo.RecordObject(rt, "StretchFull");
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    private static int RemoveDuplicateComponents<T>(Scene scene, T keep)
        where T : Component
    {
        int removed = 0;
        T[] all = Object.FindObjectsByType<T>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            T c = all[i];
            if (c == null || c == keep || c.gameObject.scene != scene)
            {
                continue;
            }

            Undo.DestroyObjectImmediate(c.gameObject);
            removed++;
        }

        return removed;
    }

    private static T FindInScene<T>(Scene scene) where T : Component
    {
        T[] all = Object.FindObjectsByType<T>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].gameObject.scene == scene)
            {
                return all[i];
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

    private static Transform FindFirstCanvas(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Canvas c = roots[i].GetComponentInChildren<Canvas>(true);
            if (c != null)
            {
                return c.transform;
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

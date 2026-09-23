#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Places Inspector-editable Lives HUD / OutOfLives UI into player scenes.
/// Auto-authors on scene open (toggleable). Does not alter economy logic.
/// Global move-limit failures reuse MissionFailedLimitPanel (MoveLimitMissionUI).
/// </summary>
[InitializeOnLoad]
public static class LivesUiSceneAuthoringMenu
{
    private const string FontGuid = "2c91d7ab60b3e9e43ba878f2b7f64a83";
    private const string AutoAuthorPrefKey = "RushOut_AutoAuthorLivesUi";

    static LivesUiSceneAuthoringMenu()
    {
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (!EditorPrefs.GetBool(AutoAuthorPrefKey, true))
        {
            return;
        }

        if (AuthorScene(scene))
        {
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log(
                "[LivesUI] Auto-authored missing lives/failure UI in '" + scene.name +
                "'. Save the scene to keep it."
            );
        }
    }

    [MenuItem("RushOut/UI/Auto-Author Lives UI On Scene Open", priority = 99)]
    private static void ToggleAutoAuthor()
    {
        bool next = !EditorPrefs.GetBool(AutoAuthorPrefKey, true);
        EditorPrefs.SetBool(AutoAuthorPrefKey, next);
        Debug.Log("[LivesUI] Auto-author on scene open = " + next);
    }

    [MenuItem("RushOut/UI/Auto-Author Lives UI On Scene Open", true)]
    private static bool ToggleAutoAuthorValidate()
    {
        Menu.SetChecked(
            "RushOut/UI/Auto-Author Lives UI On Scene Open",
            EditorPrefs.GetBool(AutoAuthorPrefKey, true));
        return true;
    }

    [MenuItem("RushOut/UI/Author Lives & Failure UI Into Open Scenes", priority = 100)]
    private static void AuthorIntoOpenScenes()
    {
        int scenesTouched = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
            {
                continue;
            }

            if (AuthorScene(scene))
            {
                scenesTouched++;
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog(
            "Lives UI Authored",
            scenesTouched + " scene(s) updated (0 = already authored).\n\n" +
            "Popups start inactive. Host → right-click → Preview Popup.\nSave scenes (Ctrl+S).",
            "OK"
        );
    }

    [MenuItem("RushOut/UI/Author Lives & Failure UI (Gameplay + MainMenu + LevelSelect)", priority = 101)]
    private static void AuthorAllPlayerScenes()
    {
        string[] paths =
        {
            "Assets/Scenes/Gameplay.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/LevelSelect.unity"
        };

        string previousPath = EditorSceneManager.GetActiveScene().path;
        for (int i = 0; i < paths.Length; i++)
        {
            Scene scene = EditorSceneManager.OpenScene(paths[i], OpenSceneMode.Single);
            AuthorScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(previousPath))
        {
            EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
        }

        EditorUtility.DisplayDialog(
            "Lives UI Authored",
            "Gameplay, MainMenu, and LevelSelect updated and saved.",
            "OK"
        );
    }

    private static bool AuthorScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return false;
        }

        string name = scene.name;
        if (name == "Gameplay")
        {
            return AuthorGameplay(scene);
        }

        if (name == "MainMenu" || name == "LevelSelect")
        {
            return AuthorMenuLikeScene(scene);
        }

        return false;
    }

    private static bool AuthorGameplay(Scene scene)
    {
        bool changed = false;
        Transform safeArea = FindDeep(scene, "SafeArea");
        Transform gameCanvas = FindDeep(scene, "GameCanvas");
        if (gameCanvas == null)
        {
            Debug.LogError("[LivesUI] GameCanvas not found in Gameplay.");
            return false;
        }

        Transform hudParent = safeArea != null ? safeArea : gameCanvas;
        changed |= EnsureLivesHud(hudParent);
        changed |= EnsureOutOfLives(gameCanvas);
        // Global move-limit failures reuse MissionFailedLimitPanel via MoveLimitMissionUI.
        changed |= CleanupObsoleteOutOfMovesUi(scene);
        return changed;
    }

    private static bool AuthorMenuLikeScene(Scene scene)
    {
        Transform canvas =
            FindDeep(scene, "MainMenuCanvas") ??
            FindDeep(scene, "LevelSelectCanvas") ??
            FindFirstCanvas(scene);

        if (canvas == null)
        {
            Debug.LogError("[LivesUI] No canvas found in " + scene.name);
            return false;
        }

        bool changed = false;
        changed |= EnsureLivesHud(canvas);
        changed |= EnsureOutOfLives(canvas);
        return changed;
    }

    private static bool EnsureLivesHud(Transform parent)
    {
        if (Object.FindAnyObjectByType<LivesHUD>() != null)
        {
            return false;
        }

        GameObject root = CreateUi("LivesHUD", parent);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(220f, 88f);
        rt.anchoredPosition = new Vector2(-24f, -24f);

        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.11f, 0.16f, 0.72f);
        bg.raycastTarget = false;

        GameObject iconGo = CreateUi("LivesIcon", root.transform);
        RectTransform iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.04f, 0.35f);
        iconRt.anchorMax = new Vector2(0.28f, 0.92f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        Image icon = iconGo.AddComponent<Image>();
        icon.color = new Color(1f, 0.45f, 0.45f, 1f);
        icon.raycastTarget = false;

        TextMeshProUGUI livesText = CreateTmp(
            "LivesText", root.transform, "5 / 5", 28f, FontStyles.Bold,
            new Vector2(0.28f, 0.42f), new Vector2(0.96f, 0.92f));

        GameObject countdownRoot = CreateUi("CountdownRoot", root.transform);
        RectTransform cdRt = countdownRoot.GetComponent<RectTransform>();
        cdRt.anchorMin = new Vector2(0.08f, 0.08f);
        cdRt.anchorMax = new Vector2(0.94f, 0.42f);
        cdRt.offsetMin = Vector2.zero;
        cdRt.offsetMax = Vector2.zero;

        TextMeshProUGUI countdownText = CreateTmp(
            "RegenTimerText", countdownRoot.transform, "1:42:18", 20f, FontStyles.Normal,
            Vector2.zero, Vector2.one);

        LivesHUD hud = root.AddComponent<LivesHUD>();
        SerializedObject so = new SerializedObject(hud);
        so.FindProperty("livesText").objectReferenceValue = livesText;
        so.FindProperty("countdownText").objectReferenceValue = countdownText;
        so.FindProperty("countdownRoot").objectReferenceValue = countdownRoot;
        so.FindProperty("livesIcon").objectReferenceValue = icon;
        so.ApplyModifiedPropertiesWithoutUndo();

        Undo.RegisterCreatedObjectUndo(root, "Author LivesHUD");
        return true;
    }

    private static bool EnsureOutOfLives(Transform canvas)
    {
        if (Object.FindAnyObjectByType<OutOfLivesUI>() != null)
        {
            return false;
        }

        GameObject host = CreateUi("OutOfLivesUI", canvas);
        StretchFull(host.GetComponent<RectTransform>());

        GameObject panelRoot = CreateUi("OutOfLivesPanel", host.transform);
        StretchFull(panelRoot.GetComponent<RectTransform>());
        panelRoot.SetActive(false);

        GameObject dim = CreateUi("DimBackground", panelRoot.transform);
        StretchFull(dim.GetComponent<RectTransform>());
        Image dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0.04f, 0.06f, 0.10f, 0.82f);

        GameObject panel = CreateUi("Panel", panelRoot.transform);
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.12f, 0.16f, 0.22f, 0.98f);
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(460f, 460f);
        panelRt.anchoredPosition = Vector2.zero;

        TextMeshProUGUI title = CreateTmp(
            "TitleText", panel.transform, "OUT OF LIVES", 36f, FontStyles.Bold,
            new Vector2(0.06f, 0.84f), new Vector2(0.94f, 0.96f));
        TextMeshProUGUI body = CreateTmp(
            "MessageText", panel.transform, "You need a life to play.", 22f, FontStyles.Normal,
            new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.82f));
        TextMeshProUGUI countdownLabel = CreateTmp(
            "CountdownLabel", panel.transform, "Next life in:", 18f, FontStyles.Normal,
            new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.70f));
        TextMeshProUGUI countdown = CreateTmp(
            "CountdownText", panel.transform, "1:42:18", 30f, FontStyles.Bold,
            new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.62f));
        TextMeshProUGUI status = CreateTmp(
            "StatusText", panel.transform, string.Empty, 18f, FontStyles.Bold,
            new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.50f));
        status.color = new Color(1f, 0.55f, 0.45f, 1f);
        status.gameObject.SetActive(false);

        Button useCoins = CreateButton(
            "UseCoinsButton", panel.transform,
            "USE " + LivesEconomyConfig.LifeCoinCost + " COINS",
            new Vector2(0f, -40f));
        Button watchAd = CreateButton(
            "WatchAdButton", panel.transform, "WATCH AD", new Vector2(0f, -95f));
        Button ok = CreateButton("OkButton", panel.transform, "OK", new Vector2(0f, -150f));

        OutOfLivesUI ui = host.AddComponent<OutOfLivesUI>();
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("root").objectReferenceValue = panelRoot;
        so.FindProperty("inputBlocker").objectReferenceValue = dim;
        so.FindProperty("titleText").objectReferenceValue = title;
        so.FindProperty("bodyText").objectReferenceValue = body;
        so.FindProperty("nextLifeLabelText").objectReferenceValue = countdownLabel;
        so.FindProperty("countdownText").objectReferenceValue = countdown;
        so.FindProperty("statusText").objectReferenceValue = status;
        so.FindProperty("okButton").objectReferenceValue = ok;
        so.FindProperty("okButtonLabel").objectReferenceValue =
            ok.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("useCoinsButton").objectReferenceValue = useCoins;
        so.FindProperty("useCoinsButtonLabel").objectReferenceValue =
            useCoins.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("watchAdButton").objectReferenceValue = watchAd;
        so.FindProperty("watchAdButtonLabel").objectReferenceValue =
            watchAd.GetComponentInChildren<TextMeshProUGUI>();
        so.ApplyModifiedPropertiesWithoutUndo();

        Undo.RegisterCreatedObjectUndo(host, "Author OutOfLivesUI");
        return true;
    }

    /// <summary>
    /// Removes the obsolete auto-authored GlobalMoveLimitFailureUI / OutOfMovesPanel
    /// hierarchy. MissionFailedLimitPanel is owned by MoveLimitMissionUI and is never deleted.
    /// </summary>
    private static bool CleanupObsoleteOutOfMovesUi(Scene scene)
    {
        bool changed = false;

        Transform obsoleteHost =
            FindDeep(scene, "GlobalMoveLimitFailureUI") ??
            FindDeep(scene, "GlobalMoveLimitFailureUI_OBSOLETE");
        if (obsoleteHost != null)
        {
            // Only remove the duplicate presenter host (and its OutOfMovesPanel child).
            Undo.DestroyObjectImmediate(obsoleteHost.gameObject);
            changed = true;
            Debug.Log(
                "[LivesUI] Removed obsolete GlobalMoveLimitFailureUI / OutOfMovesPanel. " +
                "Global fails now use MissionFailedLimitPanel via MoveLimitMissionUI."
            );
        }

        return changed;
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

    private static GameObject CreateUi(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = parent != null ? parent.gameObject.layer : 5;
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static TextMeshProUGUI CreateTmp(
        string name,
        Transform parent,
        string text,
        float size,
        FontStyles style,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        GameObject go = CreateUi(name, parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            AssetDatabase.GUIDToAssetPath(FontGuid));
        if (font != null)
        {
            tmp.font = font;
        }

        RectTransform rt = tmp.rectTransform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return tmp;
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        string label,
        Vector2 anchoredPos)
    {
        GameObject go = CreateUi(name, parent);
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.18f, 0.42f, 0.68f, 1f);
        Button button = go.AddComponent<Button>();
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(260f, 48f);
        rt.anchoredPosition = anchoredPos;

        TextMeshProUGUI tmp = CreateTmp(
            "Label", go.transform, label, 22f, FontStyles.Bold, Vector2.zero, Vector2.one);
        StretchFull(tmp.rectTransform);
        tmp.raycastTarget = false;
        return button;
    }
}
#endif

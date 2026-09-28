#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Authors Daily Challenge MainMenu / Gameplay UI and creates the default config asset.
/// </summary>
public static class DailyChallengeAuthoringMenu
{
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
    private const string GameplayPath = "Assets/Scenes/Gameplay.unity";
    private const string ConfigAssetPath = "Assets/Resources/DailyChallengeConfig.asset";
    private const string FontGuid = "2c91d7ab60b3e9e43ba878f2b7f64a83";

    [MenuItem("Rush Out/UI/Create Daily Challenge MainMenu UI")]
    public static void CreateMainMenuUi()
    {
        EnsureConfigAsset();
        Scene scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Daily Challenge", "No Canvas in MainMenu.", "OK");
            return;
        }

        Transform parent = canvas.transform;
        RectTransform safe = FindNamed(parent, "SafeArea");
        if (safe != null)
        {
            parent = safe;
        }

        DailyChallengeMainMenuUI existing =
            Object.FindAnyObjectByType<DailyChallengeMainMenuUI>(FindObjectsInactive.Include);
        if (existing != null)
        {
            Selection.activeObject = existing.gameObject;
            EditorUtility.DisplayDialog(
                "Daily Challenge",
                "DailyChallengeMainMenuUI already exists. Select it to style in Inspector.",
                "OK");
            return;
        }

        GameObject root = CreateUi("DailyChallengeCard", parent);
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = new Vector2(0.5f, 0.5f);
        rootRt.anchorMax = new Vector2(0.5f, 0.5f);
        rootRt.pivot = new Vector2(0.5f, 0.5f);
        rootRt.anchoredPosition = new Vector2(0f, -280f);
        rootRt.sizeDelta = new Vector2(420f, 220f);
        Image rootBg = root.AddComponent<Image>();
        rootBg.color = new Color(0.10f, 0.16f, 0.28f, 0.92f);

        TextMeshProUGUI title = CreateTmp("TitleText", root.transform, "DAILY CHALLENGE", 36f);
        SetRect(title.rectTransform, new Vector2(0f, 70f), new Vector2(380f, 48f));

        TextMeshProUGUI status = CreateTmp("StatusText", root.transform, "ONE ATTEMPT", 26f);
        SetRect(status.rectTransform, new Vector2(0f, 28f), new Vector2(380f, 36f));

        TextMeshProUGUI cdLabel = CreateTmp("CountdownLabel", root.transform, "NEW CHALLENGE IN", 18f);
        SetRect(cdLabel.rectTransform, new Vector2(0f, -10f), new Vector2(380f, 28f));

        TextMeshProUGUI cdValue = CreateTmp("CountdownValue", root.transform, "00:00:00", 28f);
        SetRect(cdValue.rectTransform, new Vector2(0f, -42f), new Vector2(380f, 36f));

        Button play = CreateButton("PlayButton", root.transform, "PLAY", new Vector2(0f, -85f));
        Button view = CreateButton(
            "ViewResultButton", root.transform, "VIEW RESULT", new Vector2(0f, -85f));
        view.gameObject.SetActive(false);

        GameObject confirm = CreateUi("DailyChallengeConfirm", parent);
        RectTransform confirmRt = confirm.GetComponent<RectTransform>();
        Stretch(confirmRt);
        Image confirmDim = confirm.AddComponent<Image>();
        confirmDim.color = new Color(0f, 0f, 0f, 0.65f);
        confirm.SetActive(false);

        GameObject confirmPanel = CreateUi("Panel", confirm.transform);
        RectTransform panelRt = confirmPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = panelRt.anchorMax = panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(520f, 360f);
        Image panelBg = confirmPanel.AddComponent<Image>();
        panelBg.color = new Color(0.09f, 0.14f, 0.24f, 1f);

        TextMeshProUGUI confirmTitle = CreateTmp(
            "ConfirmTitle", confirmPanel.transform, "DAILY CHALLENGE", 34f);
        SetRect(confirmTitle.rectTransform, new Vector2(0f, 120f), new Vector2(480f, 44f));
        TextMeshProUGUI oneShot = CreateTmp(
            "OneShotLabel", confirmPanel.transform, "ONE SHOT", 28f);
        SetRect(oneShot.rectTransform, new Vector2(0f, 70f), new Vector2(480f, 36f));
        TextMeshProUGUI body = CreateTmp(
            "ConfirmBody",
            confirmPanel.transform,
            "You only get one attempt at today's Daily Challenge.\n\nOnce you start, your attempt is used — even if you quit early.",
            22f);
        SetRect(body.rectTransform, new Vector2(0f, -10f), new Vector2(460f, 140f));

        Button start = CreateButton(
            "StartChallengeButton",
            confirmPanel.transform,
            "START CHALLENGE",
            new Vector2(0f, -110f));
        Button cancel = CreateButton(
            "CancelButton", confirmPanel.transform, "CANCEL", new Vector2(0f, -160f));

        GameObject result = CreateUi("DailyChallengeResult", parent);
        RectTransform resultRt = result.GetComponent<RectTransform>();
        Stretch(resultRt);
        Image resultDim = result.AddComponent<Image>();
        resultDim.color = new Color(0f, 0f, 0f, 0.65f);
        result.SetActive(false);

        GameObject resultPanel = CreateUi("Panel", result.transform);
        RectTransform rpRt = resultPanel.GetComponent<RectTransform>();
        rpRt.anchorMin = rpRt.anchorMax = rpRt.pivot = new Vector2(0.5f, 0.5f);
        rpRt.sizeDelta = new Vector2(520f, 300f);
        resultPanel.AddComponent<Image>().color = new Color(0.09f, 0.14f, 0.24f, 1f);
        TextMeshProUGUI resultBody = CreateTmp(
            "ResultBody", resultPanel.transform, "RESULT", 22f);
        SetRect(resultBody.rectTransform, new Vector2(0f, 20f), new Vector2(460f, 180f));
        Button resultClose = CreateButton(
            "ResultCloseButton", resultPanel.transform, "CLOSE", new Vector2(0f, -110f));

        DailyChallengeMainMenuUI ui = root.AddComponent<DailyChallengeMainMenuUI>();
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("entryRoot").objectReferenceValue = root;
        so.FindProperty("playButton").objectReferenceValue = play;
        so.FindProperty("viewResultButton").objectReferenceValue = view;
        so.FindProperty("titleText").objectReferenceValue = title;
        so.FindProperty("statusText").objectReferenceValue = status;
        so.FindProperty("countdownLabelText").objectReferenceValue = cdLabel;
        so.FindProperty("countdownValueText").objectReferenceValue = cdValue;
        so.FindProperty("confirmRoot").objectReferenceValue = confirm;
        so.FindProperty("startChallengeButton").objectReferenceValue = start;
        so.FindProperty("cancelButton").objectReferenceValue = cancel;
        so.FindProperty("confirmBodyText").objectReferenceValue = body;
        so.FindProperty("resultRoot").objectReferenceValue = result;
        so.FindProperty("resultCloseButton").objectReferenceValue = resultClose;
        so.FindProperty("resultBodyText").objectReferenceValue = resultBody;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = root;
        Debug.Log("[DailyChallenge] MainMenu UI authored under " + parent.name);
    }

    [MenuItem("Rush Out/UI/Create Daily Challenge Gameplay Result UI")]
    public static void CreateGameplayResultUi()
    {
        EnsureConfigAsset();
        Scene scene = EditorSceneManager.OpenScene(GameplayPath, OpenSceneMode.Single);
        Transform parent = FindGameplayUiParent();
        if (parent == null)
        {
            EditorUtility.DisplayDialog(
                "Daily Challenge",
                "No GameCanvas / SafeArea found in Gameplay.",
                "OK");
            return;
        }

        DailyChallengeResultUI existing =
            Object.FindAnyObjectByType<DailyChallengeResultUI>(FindObjectsInactive.Include);
        if (existing != null)
        {
            // Repair accidental parenting under BackgroundCanvas (scale 0).
            if (existing.transform.parent != parent)
            {
                existing.transform.SetParent(parent, false);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.LogWarning(
                    "[DailyChallenge] Reparented existing ResultUI under " + parent.name);
            }

            Selection.activeObject = existing.gameObject;
            EditorUtility.DisplayDialog(
                "Daily Challenge",
                "DailyChallengeResultUI already exists under " + parent.name +
                ". Style it in Inspector.",
                "OK");
            return;
        }

        GameObject root = CreateUi("DailyChallengeResultPanel", parent);
        RectTransform rootRt = root.GetComponent<RectTransform>();
        Stretch(rootRt);
        Image dim = root.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.72f);
        root.SetActive(false);

        GameObject panel = CreateUi("Panel", root.transform);
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = panelRt.anchorMax = panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(560f, 520f);
        panel.AddComponent<Image>().color = new Color(0.09f, 0.14f, 0.24f, 1f);

        TextMeshProUGUI title = CreateTmp(
            "TitleText", panel.transform, "DAILY CHALLENGE COMPLETE!", 34f);
        SetRect(title.rectTransform, new Vector2(0f, 200f), new Vector2(520f, 48f));

        TextMeshProUGUI scoreLabel = CreateTmp("ScoreLabel", panel.transform, "SCORE", 20f);
        SetRect(scoreLabel.rectTransform, new Vector2(0f, 140f), new Vector2(520f, 28f));
        TextMeshProUGUI scoreValue = CreateTmp("ScoreValue", panel.transform, "0", 48f);
        SetRect(scoreValue.rectTransform, new Vector2(0f, 90f), new Vector2(520f, 56f));

        TextMeshProUGUI movesLabel = CreateTmp("MovesLabel", panel.transform, "MOVES", 18f);
        SetRect(movesLabel.rectTransform, new Vector2(-120f, 20f), new Vector2(200f, 24f));
        TextMeshProUGUI movesValue = CreateTmp("MovesValue", panel.transform, "0", 32f);
        SetRect(movesValue.rectTransform, new Vector2(-120f, -20f), new Vector2(200f, 40f));

        TextMeshProUGUI timeLabel = CreateTmp("TimeLabel", panel.transform, "TIME", 18f);
        SetRect(timeLabel.rectTransform, new Vector2(120f, 20f), new Vector2(200f, 24f));
        TextMeshProUGUI timeValue = CreateTmp("TimeValue", panel.transform, "00:00.00", 32f);
        SetRect(timeValue.rectTransform, new Vector2(120f, -20f), new Vector2(200f, 40f));

        TextMeshProUGUI footer = CreateTmp(
            "FooterText", panel.transform, "ONE ATTEMPT COMPLETE", 18f);
        SetRect(footer.rectTransform, new Vector2(0f, -80f), new Vector2(480f, 28f));

        Button cont = CreateButton(
            "ContinueButton", panel.transform, "CONTINUE", new Vector2(0f, -160f));

        DailyChallengeResultUI ui = root.AddComponent<DailyChallengeResultUI>();
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("root").objectReferenceValue = root;
        so.FindProperty("titleText").objectReferenceValue = title;
        so.FindProperty("scoreLabelText").objectReferenceValue = scoreLabel;
        so.FindProperty("scoreValueText").objectReferenceValue = scoreValue;
        so.FindProperty("movesLabelText").objectReferenceValue = movesLabel;
        so.FindProperty("movesValueText").objectReferenceValue = movesValue;
        so.FindProperty("timeLabelText").objectReferenceValue = timeLabel;
        so.FindProperty("timeValueText").objectReferenceValue = timeValue;
        so.FindProperty("footerText").objectReferenceValue = footer;
        so.FindProperty("continueButton").objectReferenceValue = cont;
        so.ApplyModifiedPropertiesWithoutUndo();

        EnsureGameplayHud(parent);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = root;
        Debug.Log("[DailyChallenge] Gameplay Result UI authored under " + parent.name);
    }

    [MenuItem("Rush Out/UI/Create Daily Challenge Gameplay HUD Badge")]
    public static void CreateGameplayHudBadge()
    {
        Scene scene = EditorSceneManager.OpenScene(GameplayPath, OpenSceneMode.Single);
        Transform parent = FindGameplayUiParent();
        if (parent == null)
        {
            EditorUtility.DisplayDialog(
                "Daily Challenge",
                "No GameCanvas / SafeArea found in Gameplay.",
                "OK");
            return;
        }

        GameObject hud = EnsureGameplayHud(parent);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = hud;
    }

    /// <summary>
    /// Prefer GameCanvas/SafeArea — never BackgroundCanvas (often scale 0).
    /// </summary>
    private static Transform FindGameplayUiParent()
    {
        GameObject gameCanvasGo = GameObject.Find("GameCanvas");
        if (gameCanvasGo != null)
        {
            RectTransform safe = FindNamed(gameCanvasGo.transform, "SafeArea");
            return safe != null ? safe : gameCanvasGo.transform;
        }

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas c = canvases[i];
            if (c == null || c.name == "BackgroundCanvas")
            {
                continue;
            }

            RectTransform safe = FindNamed(c.transform, "SafeArea");
            if (safe != null)
            {
                return safe;
            }
        }

        return null;
    }

    [MenuItem("Rush Out/UI/Create Daily Challenge Leaderboard UI (MainMenu)")]
    public static void CreateLeaderboardMainMenu()
    {
        Scene scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
        Transform parent = FindMainMenuUiParent();
        CreateLeaderboardUnder(parent, scene);
        EnsureMainMenuLeaderboardButton();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    [MenuItem("Rush Out/UI/Create Daily Challenge Leaderboard UI (Gameplay)")]
    public static void CreateLeaderboardGameplay()
    {
        Scene scene = EditorSceneManager.OpenScene(GameplayPath, OpenSceneMode.Single);
        Transform parent = FindGameplayUiParent();
        CreateLeaderboardUnder(parent, scene);
        UpgradeGameplayResultUiPhase3();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    [MenuItem("Rush Out/UI/Upgrade Daily Result UI for Leaderboard (Gameplay)")]
    public static void UpgradeGameplayResultUiPhase3Menu()
    {
        Scene scene = EditorSceneManager.OpenScene(GameplayPath, OpenSceneMode.Single);
        UpgradeGameplayResultUiPhase3();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static Transform FindMainMenuUiParent()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            return null;
        }

        RectTransform safe = FindNamed(canvas.transform, "SafeArea");
        return safe != null ? safe : canvas.transform;
    }

    private static void CreateLeaderboardUnder(Transform parent, Scene scene)
    {
        if (parent == null)
        {
            EditorUtility.DisplayDialog("Daily Challenge", "No UI parent found.", "OK");
            return;
        }

        DailyLeaderboardUI existing =
            Object.FindAnyObjectByType<DailyLeaderboardUI>(FindObjectsInactive.Include);
        if (existing != null)
        {
            if (existing.transform.parent != parent)
            {
                existing.transform.SetParent(parent, false);
            }

            Selection.activeGameObject = existing.gameObject;
            EditorUtility.DisplayDialog(
                "Daily Challenge",
                "DailyLeaderboardUI already exists. Style it in Inspector.",
                "OK");
            return;
        }

        GameObject root = CreateUi("DailyLeaderboardPanel", parent);
        Stretch(root.GetComponent<RectTransform>());
        root.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
        root.SetActive(false);

        GameObject panel = CreateUi("Panel", root.transform);
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(640f, 920f);
        panel.AddComponent<Image>().color = new Color(0.09f, 0.14f, 0.24f, 1f);

        TextMeshProUGUI title = CreateTmp("Title", panel.transform, "DAILY LEADERBOARD", 34f);
        SetRect(title.rectTransform, new Vector2(0f, 400f), new Vector2(600f, 44f));

        Button globalTab = CreateButton("GlobalTab", panel.transform, "GLOBAL", new Vector2(-120f, 340f));
        SetRect(globalTab.GetComponent<RectTransform>(), new Vector2(-120f, 340f), new Vector2(200f, 44f));
        Button friendsTab = CreateButton("FriendsTab", panel.transform, "FRIENDS", new Vector2(120f, 340f));
        SetRect(friendsTab.GetComponent<RectTransform>(), new Vector2(120f, 340f), new Vector2(200f, 44f));
        friendsTab.interactable = false;

        GameObject comingSoon = CreateUi("FriendsComingSoon", panel.transform);
        Stretch(comingSoon.GetComponent<RectTransform>());
        comingSoon.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        TextMeshProUGUI soonTmp = CreateTmp("Label", comingSoon.transform, "COMING SOON", 36f);
        Stretch(soonTmp.rectTransform);
        comingSoon.SetActive(false);

        TextMeshProUGUI hRank = CreateTmp("RankHeader", panel.transform, "RANK", 16f);
        SetRect(hRank.rectTransform, new Vector2(-220f, 290f), new Vector2(100f, 28f));
        TextMeshProUGUI hPlayer = CreateTmp("PlayerHeader", panel.transform, "PLAYER", 16f);
        SetRect(hPlayer.rectTransform, new Vector2(-40f, 290f), new Vector2(200f, 28f));
        TextMeshProUGUI hScore = CreateTmp("ScoreHeader", panel.transform, "SCORE", 16f);
        SetRect(hScore.rectTransform, new Vector2(200f, 290f), new Vector2(160f, 28f));

        GameObject scrollGo = CreateUi("ScrollView", panel.transform);
        RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRt.pivot = new Vector2(0.5f, 0.5f);
        scrollRt.anchoredPosition = new Vector2(0f, 20f);
        scrollRt.sizeDelta = new Vector2(600f, 520f);
        Image scrollImg = scrollGo.AddComponent<Image>();
        scrollImg.color = new Color(0.06f, 0.10f, 0.16f, 0.9f);
        ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        GameObject viewport = CreateUi("Viewport", scrollGo.transform);
        Stretch(viewport.GetComponent<RectTransform>());
        viewport.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        scroll.viewport = viewport.GetComponent<RectTransform>();

        GameObject content = CreateUi("Content", viewport.transform);
        RectTransform contentRt = content.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0f, 0f);
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = contentRt;

        DailyLeaderboardRowUI rowPrefab = CreateRowTemplate(content.transform, "RowPrefab");
        rowPrefab.gameObject.SetActive(false);

        DailyLeaderboardRowUI pinned = CreateRowTemplate(panel.transform, "PlayerPinnedRow");
        SetRect(pinned.GetComponent<RectTransform>(), new Vector2(0f, -280f), new Vector2(600f, 72f));
        pinned.gameObject.SetActive(false);

        TextMeshProUGUI status = CreateTmp(
            "StatusText", panel.transform, "COMPLETE TODAY'S CHALLENGE TO GET YOUR RANK", 18f);
        SetRect(status.rectTransform, new Vector2(0f, -340f), new Vector2(580f, 36f));

        Button close = CreateButton("CloseButton", panel.transform, "CLOSE", new Vector2(0f, -400f));

        DailyLeaderboardUI ui = root.AddComponent<DailyLeaderboardUI>();
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("root").objectReferenceValue = root;
        so.FindProperty("titleText").objectReferenceValue = title;
        so.FindProperty("statusText").objectReferenceValue = status;
        so.FindProperty("closeButton").objectReferenceValue = close;
        so.FindProperty("globalTabButton").objectReferenceValue = globalTab;
        so.FindProperty("friendsTabButton").objectReferenceValue = friendsTab;
        so.FindProperty("friendsTabLabel").objectReferenceValue =
            friendsTab.GetComponentInChildren<TextMeshProUGUI>(true);
        so.FindProperty("friendsComingSoonRoot").objectReferenceValue = comingSoon;
        so.FindProperty("contentRoot").objectReferenceValue = content.transform;
        so.FindProperty("rowPrefab").objectReferenceValue = rowPrefab;
        so.FindProperty("pinnedPlayerRow").objectReferenceValue = pinned;
        so.FindProperty("scrollRect").objectReferenceValue = scroll;
        so.ApplyModifiedPropertiesWithoutUndo();

        Selection.activeGameObject = root;
        Debug.Log("[DailyChallenge] Leaderboard UI authored under " + parent.name);
    }

    private static DailyLeaderboardRowUI CreateRowTemplate(Transform parent, string name)
    {
        GameObject row = CreateUi(name, parent);
        RectTransform rt = row.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(584f, 72f);
        LayoutElement le = row.AddComponent<LayoutElement>();
        le.minHeight = 72f;
        le.preferredHeight = 72f;
        Image bg = row.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.18f, 0.28f, 1f);

        GameObject highlight = CreateUi("Highlight", row.transform);
        Stretch(highlight.GetComponent<RectTransform>());
        highlight.AddComponent<Image>().color = new Color(0.25f, 0.55f, 0.85f, 0.35f);
        highlight.SetActive(false);

        TextMeshProUGUI rank = CreateTmp("Rank", row.transform, "#1", 22f);
        SetRect(rank.rectTransform, new Vector2(-240f, 10f), new Vector2(80f, 28f));
        TextMeshProUGUI player = CreateTmp("Name", row.transform, "Maya K.", 22f);
        SetRect(player.rectTransform, new Vector2(-40f, 10f), new Vector2(220f, 28f));
        TextMeshProUGUI score = CreateTmp("Score", row.transform, "100,000", 22f);
        SetRect(score.rectTransform, new Vector2(200f, 10f), new Vector2(160f, 28f));
        TextMeshProUGUI secondary = CreateTmp("Secondary", row.transform, "9 MOVES • 00:42.00", 14f);
        SetRect(secondary.rectTransform, new Vector2(0f, -18f), new Vector2(540f, 22f));

        GameObject sim = CreateUi("SimMarker", row.transform);
        TextMeshProUGUI simTmp = CreateTmp("Label", sim.transform, "SIM", 12f);
        SetRect(sim.GetComponent<RectTransform>(), new Vector2(260f, -18f), new Vector2(48f, 18f));
        Stretch(simTmp.rectTransform);
        sim.SetActive(false);

        DailyLeaderboardRowUI rowUi = row.AddComponent<DailyLeaderboardRowUI>();
        SerializedObject so = new SerializedObject(rowUi);
        so.FindProperty("rankText").objectReferenceValue = rank;
        so.FindProperty("nameText").objectReferenceValue = player;
        so.FindProperty("scoreText").objectReferenceValue = score;
        so.FindProperty("secondaryText").objectReferenceValue = secondary;
        so.FindProperty("highlightRoot").objectReferenceValue = highlight;
        so.FindProperty("simulatedMarkerRoot").objectReferenceValue = sim;
        so.ApplyModifiedPropertiesWithoutUndo();
        return rowUi;
    }

    private static void EnsureMainMenuLeaderboardButton()
    {
        DailyChallengeMainMenuUI menu =
            Object.FindAnyObjectByType<DailyChallengeMainMenuUI>(FindObjectsInactive.Include);
        if (menu == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(menu);
        if (so.FindProperty("leaderboardButton").objectReferenceValue != null)
        {
            return;
        }

        Transform entry = menu.transform;
        Button lb = CreateButton("LeaderboardButton", entry, "LEADERBOARD", new Vector2(0f, -130f));
        SetRect(lb.GetComponent<RectTransform>(), new Vector2(0f, -130f), new Vector2(280f, 44f));

        TextMeshProUGUI scoreLabel = CreateTmp("ScoreLabel", entry, "SCORE", 16f);
        SetRect(scoreLabel.rectTransform, new Vector2(-80f, -5f), new Vector2(120f, 22f));
        TextMeshProUGUI scoreValue = CreateTmp("ScoreValue", entry, "", 22f);
        SetRect(scoreValue.rectTransform, new Vector2(-80f, -32f), new Vector2(140f, 28f));
        TextMeshProUGUI rankLabel = CreateTmp("RankLabel", entry, "RANK", 16f);
        SetRect(rankLabel.rectTransform, new Vector2(100f, -5f), new Vector2(120f, 22f));
        TextMeshProUGUI rankValue = CreateTmp("RankValue", entry, "", 22f);
        SetRect(rankValue.rectTransform, new Vector2(100f, -32f), new Vector2(120f, 28f));
        scoreLabel.gameObject.SetActive(false);
        scoreValue.gameObject.SetActive(false);
        rankLabel.gameObject.SetActive(false);
        rankValue.gameObject.SetActive(false);

        so.FindProperty("leaderboardButton").objectReferenceValue = lb;
        so.FindProperty("scoreLabelText").objectReferenceValue = scoreLabel;
        so.FindProperty("scoreValueText").objectReferenceValue = scoreValue;
        so.FindProperty("rankLabelText").objectReferenceValue = rankLabel;
        so.FindProperty("rankValueText").objectReferenceValue = rankValue;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void UpgradeGameplayResultUiPhase3()
    {
        DailyChallengeResultUI resultUi =
            Object.FindAnyObjectByType<DailyChallengeResultUI>(FindObjectsInactive.Include);
        if (resultUi == null)
        {
            Debug.LogWarning("[DailyChallenge] No ResultUI to upgrade.");
            return;
        }

        SerializedObject so = new SerializedObject(resultUi);
        Transform panel = resultUi.transform.Find("Panel");
        if (panel == null && resultUi.transform.childCount > 0)
        {
            panel = resultUi.transform.GetChild(0);
        }

        if (panel == null)
        {
            return;
        }

        if (so.FindProperty("rankLabelText").objectReferenceValue == null)
        {
            TextMeshProUGUI rankLabel = CreateTmp("RankLabel", panel, "RANK", 18f);
            SetRect(rankLabel.rectTransform, new Vector2(0f, -50f), new Vector2(200f, 24f));
            TextMeshProUGUI rankValue = CreateTmp("RankValue", panel, "#1", 32f);
            SetRect(rankValue.rectTransform, new Vector2(0f, -90f), new Vector2(200f, 40f));
            so.FindProperty("rankLabelText").objectReferenceValue = rankLabel;
            so.FindProperty("rankValueText").objectReferenceValue = rankValue;
        }

        if (so.FindProperty("leaderboardButton").objectReferenceValue == null)
        {
            Button lb = CreateButton(
                "LeaderboardButton", panel, "LEADERBOARD", new Vector2(0f, -200f));
            so.FindProperty("leaderboardButton").objectReferenceValue = lb;

            // Nudge CONTINUE down if present.
            Button cont = so.FindProperty("continueButton").objectReferenceValue as Button;
            if (cont != null)
            {
                RectTransform crt = cont.GetComponent<RectTransform>();
                if (crt != null)
                {
                    crt.anchoredPosition = new Vector2(0f, -260f);
                }
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log("[DailyChallenge] ResultUI upgraded with RANK + LEADERBOARD.");
    }

    [MenuItem("Rush Out/UI/Create Daily Challenge Config Asset")]
    public static void EnsureConfigAsset()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        DailyChallengeConfig existing =
            AssetDatabase.LoadAssetAtPath<DailyChallengeConfig>(ConfigAssetPath);
        if (existing != null)
        {
            FillPoolIfEmpty(existing);
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            return;
        }

        DailyChallengeConfig config = ScriptableObject.CreateInstance<DailyChallengeConfig>();
        AssetDatabase.CreateAsset(config, ConfigAssetPath);
        FillPoolIfEmpty(config);
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        Debug.Log("[DailyChallenge] Created " + ConfigAssetPath);
    }

    private static GameObject EnsureGameplayHud(Transform parent)
    {
        DailyChallengeGameplayHud existing =
            Object.FindAnyObjectByType<DailyChallengeGameplayHud>(FindObjectsInactive.Include);
        if (existing != null)
        {
            if (existing.transform.parent != parent)
            {
                existing.transform.SetParent(parent, false);
                Debug.LogWarning(
                    "[DailyChallenge] Reparented existing GameplayHud under " + parent.name);
            }

            // Host must start active so Start() can decide Daily visibility.
            existing.gameObject.SetActive(true);
            return existing.gameObject;
        }

        GameObject root = CreateUi("DailyChallengeGameplayHud", parent);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -24f);
        rt.sizeDelta = new Vector2(360f, 72f);
        root.AddComponent<Image>().color = new Color(0.10f, 0.16f, 0.28f, 0.85f);
        // Host stays active; Start() hides for non-Daily sessions.

        TextMeshProUGUI title = CreateTmp("TitleText", root.transform, "DAILY CHALLENGE", 24f);
        SetRect(title.rectTransform, new Vector2(0f, 12f), new Vector2(340f, 32f));
        TextMeshProUGUI sub = CreateTmp("SubtitleText", root.transform, "ONE ATTEMPT", 16f);
        SetRect(sub.rectTransform, new Vector2(0f, -16f), new Vector2(340f, 24f));

        DailyChallengeGameplayHud hud = root.AddComponent<DailyChallengeGameplayHud>();
        SerializedObject so = new SerializedObject(hud);
        so.FindProperty("root").objectReferenceValue = root;
        so.FindProperty("titleText").objectReferenceValue = title;
        so.FindProperty("subtitleText").objectReferenceValue = sub;
        so.ApplyModifiedPropertiesWithoutUndo();
        return root;
    }

    private static void FillPoolIfEmpty(DailyChallengeConfig config)
    {
        SerializedObject so = new SerializedObject(config);
        SerializedProperty pool = so.FindProperty("dailyLevelPool");
        if (pool.arraySize > 0)
        {
            return;
        }

        string[] paths =
        {
            "Assets/Data/Levels/Level_001.asset",
            "Assets/Data/Levels/Level_002.asset",
            "Assets/Data/Levels/Level_003.asset",
            "Assets/Data/Levels/Level_004.asset",
            "Assets/Data/Levels/Level_005.asset",
            "Assets/Data/Levels/Level_006.asset",
            "Assets/Data/Levels/Level_007.asset",
            "Assets/Data/Levels/Level_008.asset",
            "Assets/Data/Levels/Level_009.asset",
            "Assets/Data/Levels/Level_010.asset"
        };

        pool.arraySize = paths.Length;
        for (int i = 0; i < paths.Length; i++)
        {
            LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(paths[i]);
            pool.GetArrayElementAtIndex(i).objectReferenceValue = level;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreateUi(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetRect(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static TextMeshProUGUI CreateTmp(
        string name, Transform parent, string text, float size)
    {
        GameObject go = CreateUi(name, parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            AssetDatabase.GUIDToAssetPath(FontGuid));
        if (font != null)
        {
            tmp.font = font;
        }

        return tmp;
    }

    private static Button CreateButton(
        string name, Transform parent, string label, Vector2 pos)
    {
        GameObject go = CreateUi(name, parent);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.20f, 0.55f, 0.85f, 1f);
        Button button = go.AddComponent<Button>();
        RectTransform rt = go.GetComponent<RectTransform>();
        SetRect(rt, pos, new Vector2(280f, 48f));
        TextMeshProUGUI tmp = CreateTmp("Label", go.transform, label, 22f);
        Stretch(tmp.rectTransform);
        tmp.raycastTarget = false;
        return button;
    }

    private static RectTransform FindNamed(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }

        Transform t = root.Find(name);
        if (t != null)
        {
            return t as RectTransform;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            RectTransform found = FindNamed(root.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
#endif

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Authored Daily GLOBAL leaderboard panel. Instantiates row prefab into ScrollView Content.
/// Panel chrome is scene-authored; rows are dynamic data items.
/// </summary>
public class DailyLeaderboardUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button closeButton;

    [Header("Tabs")]
    [SerializeField] private Button globalTabButton;
    [SerializeField] private Button friendsTabButton;
    [SerializeField] private TextMeshProUGUI friendsTabLabel;
    [SerializeField] private GameObject friendsComingSoonRoot;

    [Header("List")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private DailyLeaderboardRowUI rowPrefab;
    [SerializeField] private DailyLeaderboardRowUI pinnedPlayerRow;
    [SerializeField] private ScrollRect scrollRect;

    [Header("DEV")]
    [SerializeField] private bool showSimulatedMarkersInEditor = true;

    private readonly List<DailyLeaderboardRowUI> spawnedRows = new List<DailyLeaderboardRowUI>();
    private bool buttonsBound;
    private bool viewLoggedForOpen;
    private bool showingFriends;
    private int friendsGeneration;
    private bool friendsConsentDeclined;
    private bool friendsBusy;
    private FriendsAction friendsAction;
    private FriendsAction friendsSecondaryAction;
    private Button friendsActionButton;
    private TextMeshProUGUI friendsActionLabel;
    private Button friendsSecondaryButton;
    private TextMeshProUGUI friendsSecondaryLabel;

    private enum FriendsAction
    {
        None = 0,
        Link = 1,
        Continue = 2,
        Retry = 3,
        Refresh = 4,
        OpenPlayGames = 5,
        OpenPlayGamesSettings = 6
    }

    private void Awake()
    {
        if (root != null && root != gameObject)
        {
            root.SetActive(false);
        }
        else if (root == null)
        {
            // Host may start inactive in scene.
        }

        if (friendsComingSoonRoot != null)
        {
            friendsComingSoonRoot.SetActive(false);
        }

        if (pinnedPlayerRow != null)
        {
            pinnedPlayerRow.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        BindButtons();
    }

    private void OnDisable()
    {
        UnbindButtons();
    }

    public void Show()
    {
        BindButtons();
        showingFriends = false;
        friendsGeneration++;

        GameObject target = root != null ? root : gameObject;
        if (!target.activeSelf)
        {
            target.SetActive(true);
        }

        transform.SetAsLastSibling();
        Refresh();

        if (titleText != null)
        {
            titleText.text = "DAILY LEADERBOARD";
        }

        if (!viewLoggedForOpen)
        {
            viewLoggedForOpen = true;
            DailyLeaderboardSnapshot snap = DailyLeaderboardService.GetTodaysSnapshot();
            GameAnalytics.LogDailyLeaderboardView(
                snap != null ? snap.DayId : string.Empty,
                snap != null && snap.HasCurrentPlayer,
                snap != null ? snap.CurrentPlayerRank : 0,
                snap != null && snap.Entries != null ? snap.Entries.Count : 0);
        }
    }

    public void Hide()
    {
        viewLoggedForOpen = false;
        showingFriends = false;
        friendsGeneration++;
        GameObject target = root != null ? root : gameObject;
        if (target.activeSelf)
        {
            target.SetActive(false);
        }

        HideFriendsOverlay();
    }

    public void Refresh()
    {
        ApplySnapshot(DailyLeaderboardService.GetTodaysSnapshot());
        if (DailyChallengeAuthority.IsOnlineMode)
        {
            _ = RefreshOnlineAsync();
        }
    }

    private async Task RefreshOnlineAsync()
    {
        DailyLeaderboardSnapshot snap =
            await DailyLeaderboardService.RefreshTodaysSnapshotAsync();
        if (this == null || showingFriends)
        {
            return;
        }

        ApplySnapshot(snap);
    }

    private void ApplySnapshot(DailyLeaderboardSnapshot snapshot)
    {
        if (showingFriends)
        {
            return;
        }

        HideFriendsOverlay();
        ClearRows();

        bool showSimMarker = ResolveShowSimulatedMarkers();

        if (snapshot != null && snapshot.Entries != null && rowPrefab != null && contentRoot != null)
        {
            for (int i = 0; i < snapshot.Entries.Count; i++)
            {
                DailyLeaderboardRowUI row = Instantiate(rowPrefab, contentRoot);
                row.gameObject.SetActive(true);
                row.Bind(snapshot.Entries[i], showSimMarker);
                spawnedRows.Add(row);
            }
        }

        if (statusText != null)
        {
            statusText.text = snapshot != null ? snapshot.StatusMessage : string.Empty;
        }

        if (pinnedPlayerRow != null)
        {
            bool showPinned = snapshot != null && snapshot.HasCurrentPlayer;
            pinnedPlayerRow.gameObject.SetActive(showPinned);
            if (showPinned)
            {
                pinnedPlayerRow.Bind(snapshot.CurrentPlayerEntry, showSimulatedMarker: false);
            }
        }

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }

        ConfigureFriendsTab();
    }

    private void ConfigureFriendsTab()
    {
        if (friendsTabButton != null)
        {
            friendsTabButton.interactable = true;
        }

        if (friendsTabLabel != null)
        {
            friendsTabLabel.text = "FRIENDS";
        }
    }

    private bool ResolveShowSimulatedMarkers()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DailyChallengeConfig config = DailyChallengeConfig.LoadDefault();
        if (config != null && config.ShowSimulatedLeaderboardMarkers)
        {
            return true;
        }

        return showSimulatedMarkersInEditor;
#else
        return false;
#endif
    }

    private void ClearRows()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (spawnedRows[i] != null)
            {
                Destroy(spawnedRows[i].gameObject);
            }
        }

        spawnedRows.Clear();

        if (contentRoot == null)
        {
            return;
        }

        // Clean leftover children (e.g. authoring template left active).
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = contentRoot.GetChild(i);
            if (rowPrefab != null && child.gameObject == rowPrefab.gameObject)
            {
                child.gameObject.SetActive(false);
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private void BindButtons()
    {
        if (buttonsBound)
        {
            return;
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }

        if (globalTabButton != null)
        {
            globalTabButton.onClick.AddListener(OnGlobalTab);
        }

        if (friendsTabButton != null)
        {
            friendsTabButton.onClick.AddListener(OnFriendsTab);
        }

        buttonsBound = true;
    }

    private void UnbindButtons()
    {
        if (!buttonsBound)
        {
            return;
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
        }

        if (globalTabButton != null)
        {
            globalTabButton.onClick.RemoveListener(OnGlobalTab);
        }

        if (friendsTabButton != null)
        {
            friendsTabButton.onClick.RemoveListener(OnFriendsTab);
        }

        buttonsBound = false;
    }

    private void OnGlobalTab()
    {
        showingFriends = false;
        friendsGeneration++;
        HideFriendsOverlay();
        Refresh();
    }

    private void OnFriendsTab()
    {
        if (friendsBusy)
        {
            return;
        }

        showingFriends = true;
        friendsGeneration++;
        _ = ShowFriendsAsync(friendsGeneration, requestConsent: false);
    }

    private async Task ShowFriendsAsync(int generation, bool requestConsent)
    {
        if (friendsBusy)
        {
            return;
        }

        friendsBusy = true;
        try
        {
            ClearRows();
            if (pinnedPlayerRow != null)
            {
                pinnedPlayerRow.gameObject.SetActive(false);
            }

            if (statusText != null)
            {
                statusText.text = string.Empty;
            }

#if !UNITY_EDITOR
            if (friendsConsentDeclined && !requestConsent)
            {
                ShowFriendsAccessDisabled();
                return;
            }
#endif

            ShowFriendsOverlay("LOADING FRIENDS", null, null, FriendsAction.None);
            await Task.Yield();

#if UNITY_EDITOR
            if (generation != friendsGeneration || !showingFriends)
            {
                return;
            }

            ShowFriendsOverlay(
                "FRIENDS COMPARISON IS AVAILABLE ON ANDROID",
                null,
                null,
                FriendsAction.None);
#else
            try
            {
                if (!DailyChallengeAuthority.IsOnlineMode)
                {
                    ShowFriendsOverlay(
                        "NO FRIEND RESULTS YET",
                        "None of your Play Games friends have a Daily Challenge result yet.",
                        "REFRESH",
                        FriendsAction.Refresh);
                    return;
                }

                await DailyChallengeIdentityService.EnsureSignedInAsync();
                await DailyChallengeIdentityService.RefreshAccountStateAsync();
                if (generation != friendsGeneration || !showingFriends)
                {
                    return;
                }

                if (!DailyChallengeIdentityService.IsGooglePlayLinked)
                {
                    ShowFriendsOverlay(
                        "LINK GOOGLE PLAY TO COMPARE WITH FRIENDS",
                        null,
                        "LINK GOOGLE PLAY",
                        FriendsAction.Link);
                    return;
                }

                await DailyFriendsLeaderboardClient.RegisterLinkedPlayerAsync();
                if (generation != friendsGeneration || !showingFriends)
                {
                    return;
                }

                PlayGamesFriendsAccess.Result access =
                    await PlayGamesFriendsAccess.LoadAsync(requestConsent);
                if (generation != friendsGeneration || !showingFriends)
                {
                    return;
                }

                if (access.Status == PlayGamesFriendsAccess.Status.ConsentRequired)
                {
                    ShowFriendsOverlay(
                        "CONNECT WITH FRIENDS",
                        "Allow Google Play Games access to your gaming community to compare Daily Challenge scores.",
                        "CONTINUE",
                        FriendsAction.Continue);
                    return;
                }

                if (access.Status == PlayGamesFriendsAccess.Status.Denied ||
                    access.Status == PlayGamesFriendsAccess.Status.Unavailable ||
                    access.Status == PlayGamesFriendsAccess.Status.NotAuthenticated)
                {
                    friendsConsentDeclined =
                        access.Status != PlayGamesFriendsAccess.Status.NotAuthenticated;
                    ShowFriendsAccessDisabled();
                    return;
                }

                if (access.Status != PlayGamesFriendsAccess.Status.Loaded)
                {
                    ShowFriendsLoadError();
                    return;
                }

                friendsConsentDeclined = false;
                if (access.PlayerIds == null || access.PlayerIds.Count == 0)
                {
                    ShowFriendsOverlay(
                        "NO FRIENDS YET",
                        "Follow players in Google Play Games to compare Daily Challenge scores.",
                        "OPEN PLAY GAMES",
                        FriendsAction.OpenPlayGames,
                        "REFRESH",
                        FriendsAction.Refresh);
                    return;
                }

                DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
                if (manager != null)
                {
                    manager.EnsureDaySynced();
                }

                string dayId = manager != null ? manager.CurrentDayId : string.Empty;
                DailyLeaderboardSnapshot snap =
                    await DailyFriendsLeaderboardClient.FetchAsync(dayId, access.PlayerIds);
                if (generation != friendsGeneration || !showingFriends || this == null)
                {
                    return;
                }

                if (snap == null)
                {
                    ShowFriendsLoadError();
                    return;
                }

                ApplyFriendsSnapshot(snap);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Friends] load failed: " + ex.GetType().Name);
                if (generation == friendsGeneration && showingFriends && this != null)
                {
                    ShowFriendsLoadError();
                }
            }
#endif
        }
        finally
        {
            friendsBusy = false;
        }
    }

    private void ShowFriendsAccessDisabled()
    {
        if (PlayGamesFriendsAccess.CanOpenPlayGamesSettings())
        {
            ShowFriendsOverlay(
                "FRIENDS ACCESS NOT ENABLED",
                null,
                "TRY AGAIN",
                FriendsAction.Retry,
                "OPEN PLAY GAMES SETTINGS",
                FriendsAction.OpenPlayGamesSettings);
            return;
        }

        ShowFriendsOverlay(
            "FRIENDS ACCESS NOT ENABLED",
            null,
            "TRY AGAIN",
            FriendsAction.Retry);
    }

    private void ShowFriendsLoadError()
    {
        ShowFriendsOverlay(
            "COULD NOT LOAD FRIEND RESULTS",
            null,
            "RETRY",
            FriendsAction.Retry);
    }

    private void ApplyFriendsSnapshot(DailyLeaderboardSnapshot snapshot)
    {
        int otherResults = 0;
        if (snapshot != null && snapshot.Entries != null)
        {
            for (int i = 0; i < snapshot.Entries.Count; i++)
            {
                if (!snapshot.Entries[i].IsCurrentPlayer)
                {
                    otherResults++;
                }
            }
        }

        const string noResultsBody =
            "None of your Play Games friends have a Daily Challenge result yet.";
        bool hasPlayer = snapshot != null && snapshot.HasCurrentPlayer;
        if (otherResults == 0)
        {
            if (hasPlayer)
            {
                if (friendsComingSoonRoot != null)
                {
                    friendsComingSoonRoot.SetActive(false);
                }

                RenderFriendsRows(snapshot);
                if (statusText != null)
                {
                    statusText.text = "NO FRIEND RESULTS YET\n" + noResultsBody;
                }

                ShowFriendsButtons(
                    "REFRESH",
                    FriendsAction.Refresh,
                    null,
                    FriendsAction.None,
                    parentToPanel: true);
                return;
            }

            ShowFriendsOverlay(
                "NO FRIEND RESULTS YET",
                noResultsBody,
                "REFRESH",
                FriendsAction.Refresh);
            return;
        }

        HideFriendsOverlay();
        RenderFriendsRows(snapshot);
        if (statusText != null)
        {
            statusText.text = snapshot.StatusMessage ?? string.Empty;
        }
    }

    private void RenderFriendsRows(DailyLeaderboardSnapshot snapshot)
    {
        ClearRows();
        if (snapshot != null && snapshot.Entries != null && rowPrefab != null && contentRoot != null)
        {
            for (int i = 0; i < snapshot.Entries.Count; i++)
            {
                DailyLeaderboardRowUI row = Instantiate(rowPrefab, contentRoot);
                row.gameObject.SetActive(true);
                row.Bind(snapshot.Entries[i], showSimulatedMarker: false);
                spawnedRows.Add(row);
            }
        }

        if (pinnedPlayerRow != null)
        {
            bool showPinned = snapshot != null && snapshot.HasCurrentPlayer;
            pinnedPlayerRow.gameObject.SetActive(showPinned);
            if (showPinned)
            {
                pinnedPlayerRow.Bind(snapshot.CurrentPlayerEntry, showSimulatedMarker: false);
            }
        }

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private async Task OnFriendsActionAsync(FriendsAction action)
    {
        if (action == FriendsAction.OpenPlayGames)
        {
            PlayGamesFriendsAccess.OpenPlayGames();
            return;
        }

        if (action == FriendsAction.OpenPlayGamesSettings)
        {
            PlayGamesFriendsAccess.OpenPlayGamesSettings();
            return;
        }

        if (action == FriendsAction.None || friendsBusy)
        {
            return;
        }

        if (action == FriendsAction.Link)
        {
            friendsBusy = true;
            try
            {
                ShowFriendsOverlay("LOADING FRIENDS", null, null, FriendsAction.None);
                AccountLinkOutcome outcome = await DailyChallengeIdentityService.LinkGooglePlayAsync();
                if (!showingFriends)
                {
                    return;
                }

                if (outcome.Result != AccountLinkResult.Success &&
                    outcome.Result != AccountLinkResult.AlreadyLinked)
                {
                    ShowFriendsOverlay(
                        "LINK GOOGLE PLAY TO COMPARE WITH FRIENDS",
                        string.IsNullOrEmpty(outcome.Message) ? null : outcome.Message,
                        "LINK GOOGLE PLAY",
                        FriendsAction.Link);
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Friends] link failed: " + ex.GetType().Name);
                if (showingFriends)
                {
                    ShowFriendsOverlay(
                        "LINK GOOGLE PLAY TO COMPARE WITH FRIENDS",
                        null,
                        "LINK GOOGLE PLAY",
                        FriendsAction.Link);
                }

                return;
            }
            finally
            {
                friendsBusy = false;
            }
        }
        else if (action == FriendsAction.Retry)
        {
            friendsConsentDeclined = false;
        }

        friendsGeneration++;
        bool consent = action == FriendsAction.Continue;
        await ShowFriendsAsync(friendsGeneration, consent);
    }

    private void ShowFriendsOverlay(
        string title,
        string body,
        string buttonText,
        FriendsAction action,
        string secondaryText = null,
        FriendsAction secondaryAction = FriendsAction.None)
    {
        if (friendsComingSoonRoot != null)
        {
            friendsComingSoonRoot.SetActive(true);
            TextMeshProUGUI tmp = FindFriendsMessageLabel();
            if (tmp != null)
            {
                tmp.text = string.IsNullOrEmpty(body) ? title : title + "\n\n" + body;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.enableWordWrapping = true;
            }
        }
        else if (statusText != null)
        {
            statusText.text = string.IsNullOrEmpty(body) ? title : title + "\n" + body;
        }

        ShowFriendsButtons(buttonText, action, secondaryText, secondaryAction, parentToPanel: false);
    }

    private void ShowFriendsButtons(
        string buttonText,
        FriendsAction action,
        string secondaryText,
        FriendsAction secondaryAction,
        bool parentToPanel)
    {
        friendsAction = action;
        friendsSecondaryAction = secondaryAction;
        EnsureFriendsActionButtons();
        Transform parent = parentToPanel
            ? (root != null ? root.transform : transform)
            : (friendsComingSoonRoot != null ? friendsComingSoonRoot.transform : transform);
        bool showSecondary = !string.IsNullOrEmpty(secondaryText);
        // CLOSE occupies y 22–100. Keep REFRESH 28px above it, OPEN PLAY GAMES 20px above REFRESH.
        const float refreshBottom = 128f;
        const float buttonHeight = 68f;
        const float buttonGap = 20f;
        PlaceFriendsButton(
            friendsActionButton,
            friendsActionLabel,
            parent,
            buttonText,
            showSecondary ? refreshBottom + buttonHeight + buttonGap : refreshBottom);
        PlaceFriendsButton(
            friendsSecondaryButton,
            friendsSecondaryLabel,
            parent,
            secondaryText,
            refreshBottom);
    }

    private void HideFriendsOverlay()
    {
        friendsAction = FriendsAction.None;
        friendsSecondaryAction = FriendsAction.None;
        if (friendsComingSoonRoot != null)
        {
            friendsComingSoonRoot.SetActive(false);
        }

        if (friendsActionButton != null)
        {
            friendsActionButton.gameObject.SetActive(false);
        }

        if (friendsSecondaryButton != null)
        {
            friendsSecondaryButton.gameObject.SetActive(false);
        }
    }

    private void EnsureFriendsActionButtons()
    {
        if (friendsActionButton == null)
        {
            friendsActionButton = CreateFriendsButton("FriendsActionButton", out friendsActionLabel);
            friendsActionButton.onClick.AddListener(() =>
            {
                _ = OnFriendsActionAsync(friendsAction);
            });
        }

        if (friendsSecondaryButton == null)
        {
            friendsSecondaryButton = CreateFriendsButton(
                "FriendsSecondaryButton",
                out friendsSecondaryLabel);
            friendsSecondaryButton.onClick.AddListener(() =>
            {
                _ = OnFriendsActionAsync(friendsSecondaryAction);
            });
        }
    }

    private Button CreateFriendsButton(string objectName, out TextMeshProUGUI label)
    {
        var buttonObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        Transform parent = friendsComingSoonRoot != null
            ? friendsComingSoonRoot.transform
            : transform;
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.18f, 0.55f, 0.28f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        var labelObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        label = labelObject.GetComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 24f;
        label.color = Color.white;
        label.raycastTarget = false;
        TextMeshProUGUI existing = FindFriendsMessageLabel();
        if (existing != null && existing.font != null)
        {
            label.font = existing.font;
        }

        return button;
    }

    private static void PlaceFriendsButton(
        Button button,
        TextMeshProUGUI label,
        Transform parent,
        string text,
        float y)
    {
        if (button == null)
        {
            return;
        }

        bool show = !string.IsNullOrEmpty(text);
        button.gameObject.SetActive(show);
        if (!show)
        {
            return;
        }

        button.transform.SetParent(parent, false);
        button.transform.SetAsLastSibling();
        var rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.12f, 0f);
        rect.anchorMax = new Vector2(0.88f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(0f, 68f);
        if (label != null)
        {
            label.text = text;
        }
    }

    private TextMeshProUGUI FindFriendsMessageLabel()
    {
        if (friendsComingSoonRoot == null)
        {
            return null;
        }

        Transform overlay = friendsComingSoonRoot.transform;
        for (int i = 0; i < overlay.childCount; i++)
        {
            Transform child = overlay.GetChild(i);
            if (child == null ||
                child.name == "FriendsActionButton" ||
                child.name == "FriendsSecondaryButton")
            {
                continue;
            }

            TextMeshProUGUI label = child.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                return label;
            }
        }

        return null;
    }

    public static DailyLeaderboardUI FindInScene()
    {
        return UnityEngine.Object.FindAnyObjectByType<DailyLeaderboardUI>(FindObjectsInactive.Include);
    }
}

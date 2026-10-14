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
    private FriendsAction friendsAction;
    private Button friendsActionButton;
    private TextMeshProUGUI friendsActionLabel;

    private enum FriendsAction
    {
        None = 0,
        Link = 1,
        Continue = 2,
        Retry = 3
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

        if (friendsComingSoonRoot != null)
        {
            friendsComingSoonRoot.SetActive(false);
        }
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
        showingFriends = true;
        friendsGeneration++;
        _ = ShowFriendsAsync(friendsGeneration, requestConsent: false);
    }

    private async Task ShowFriendsAsync(int generation, bool requestConsent)
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
        if (!DailyChallengeAuthority.IsOnlineMode)
        {
            ShowFriendsOverlay("NO FRIEND RESULTS YET", null, null, FriendsAction.None);
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

        if (friendsConsentDeclined && !requestConsent)
        {
            ShowFriendsOverlay(
                "FRIENDS ACCESS NOT ENABLED",
                "You can try again from this tab.",
                "TRY AGAIN",
                FriendsAction.Retry);
            return;
        }

        await DailyFriendsLeaderboardClient.RegisterLinkedPlayerAsync();
        if (generation != friendsGeneration || !showingFriends)
        {
            return;
        }

        PlayGamesFriendsAccess.Result access = await PlayGamesFriendsAccess.LoadAsync(requestConsent);
        if (generation != friendsGeneration || !showingFriends)
        {
            return;
        }

        if (access.Status == PlayGamesFriendsAccess.Status.ConsentRequired)
        {
            ShowFriendsOverlay(
                "CONNECT WITH FRIENDS",
                "Allow Google Play Games access to your friends to compare Daily Challenge scores.",
                "CONTINUE",
                FriendsAction.Continue);
            return;
        }

        if (access.Status == PlayGamesFriendsAccess.Status.Denied ||
            access.Status == PlayGamesFriendsAccess.Status.Unavailable)
        {
            friendsConsentDeclined = true;
            ShowFriendsOverlay(
                "FRIENDS ACCESS NOT ENABLED",
                "You can try again from this tab.",
                "TRY AGAIN",
                FriendsAction.Retry);
            return;
        }

        if (access.Status != PlayGamesFriendsAccess.Status.Loaded)
        {
            ShowFriendsOverlay(
                "COULD NOT LOAD FRIEND RESULTS",
                null,
                "TRY AGAIN",
                FriendsAction.Retry);
            return;
        }

        friendsConsentDeclined = false;
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
            ShowFriendsOverlay(
                "COULD NOT LOAD FRIEND RESULTS",
                null,
                "TRY AGAIN",
                FriendsAction.Retry);
            return;
        }

        ApplyFriendsSnapshot(snap);
#endif
    }

    private void ApplyFriendsSnapshot(DailyLeaderboardSnapshot snapshot)
    {
        bool hasRows = snapshot.Entries != null && snapshot.Entries.Count > 0;
        if (!hasRows)
        {
            ShowFriendsOverlay("NO FRIEND RESULTS YET", null, null, FriendsAction.None);
            if (statusText != null)
            {
                statusText.text = string.Empty;
            }

            return;
        }

        HideFriendsOverlay();
        ClearRows();
        if (rowPrefab != null && contentRoot != null)
        {
            for (int i = 0; i < snapshot.Entries.Count; i++)
            {
                DailyLeaderboardRowUI row = Instantiate(rowPrefab, contentRoot);
                row.gameObject.SetActive(true);
                row.Bind(snapshot.Entries[i], showSimulatedMarker: false);
                spawnedRows.Add(row);
            }
        }

        if (statusText != null)
        {
            statusText.text = snapshot.StatusMessage ?? string.Empty;
        }

        if (pinnedPlayerRow != null)
        {
            pinnedPlayerRow.gameObject.SetActive(snapshot.HasCurrentPlayer);
            if (snapshot.HasCurrentPlayer)
            {
                pinnedPlayerRow.Bind(snapshot.CurrentPlayerEntry, showSimulatedMarker: false);
            }
        }

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private async Task OnFriendsActionAsync()
    {
        FriendsAction action = friendsAction;
        if (action == FriendsAction.Link)
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
        FriendsAction action)
    {
        friendsAction = action;
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

        EnsureFriendsActionButton();
        bool showButton = !string.IsNullOrEmpty(buttonText);
        if (friendsActionButton != null)
        {
            friendsActionButton.gameObject.SetActive(showButton);
            if (friendsActionLabel != null)
            {
                friendsActionLabel.text = buttonText ?? string.Empty;
            }
        }
    }

    private void HideFriendsOverlay()
    {
        friendsAction = FriendsAction.None;
        if (friendsComingSoonRoot != null)
        {
            friendsComingSoonRoot.SetActive(false);
        }

        if (friendsActionButton != null)
        {
            friendsActionButton.gameObject.SetActive(false);
        }
    }

    private void EnsureFriendsActionButton()
    {
        if (friendsActionButton != null || friendsComingSoonRoot == null)
        {
            return;
        }

        var buttonObject = new GameObject(
            "FriendsActionButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        buttonObject.transform.SetParent(friendsComingSoonRoot.transform, false);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 72f);
        rect.sizeDelta = new Vector2(460f, 68f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.18f, 0.55f, 0.28f, 1f);
        friendsActionButton = buttonObject.GetComponent<Button>();
        friendsActionButton.targetGraphic = image;
        friendsActionButton.onClick.AddListener(() => { _ = OnFriendsActionAsync(); });

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
        friendsActionLabel = labelObject.GetComponent<TextMeshProUGUI>();
        friendsActionLabel.alignment = TextAlignmentOptions.Center;
        friendsActionLabel.fontSize = 28f;
        friendsActionLabel.color = Color.white;
        friendsActionLabel.raycastTarget = false;
        TextMeshProUGUI existing = FindFriendsMessageLabel();
        if (existing != null && existing.font != null)
        {
            friendsActionLabel.font = existing.font;
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
            if (child == null || child.name == "FriendsActionButton")
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
        return Object.FindAnyObjectByType<DailyLeaderboardUI>(FindObjectsInactive.Include);
    }
}

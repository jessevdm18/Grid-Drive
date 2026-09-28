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
        if (this == null)
        {
            return;
        }

        ApplySnapshot(snap);
    }

    private void ApplySnapshot(DailyLeaderboardSnapshot snapshot)
    {
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
            friendsTabButton.interactable = false;
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
        if (friendsComingSoonRoot != null)
        {
            friendsComingSoonRoot.SetActive(false);
        }

        Refresh();
    }

    private void OnFriendsTab()
    {
        if (friendsComingSoonRoot != null)
        {
            friendsComingSoonRoot.SetActive(true);
            TextMeshProUGUI tmp = friendsComingSoonRoot.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.text = "COMING SOON";
            }
        }
    }

    public static DailyLeaderboardUI FindInScene()
    {
        return Object.FindAnyObjectByType<DailyLeaderboardUI>(FindObjectsInactive.Include);
    }
}

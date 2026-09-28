using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Authored MainMenu Daily Challenge card + confirmation + result / leaderboard entry.
/// Scene owns styling/layout; this script owns text VALUES and visibility.
/// </summary>
public class DailyChallengeMainMenuUI : MonoBehaviour
{
    [Header("Entry card")]
    [SerializeField] private GameObject entryRoot;
    [SerializeField] private Button playButton;
    [SerializeField] private Button viewResultButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI scoreLabelText;
    [SerializeField] private TextMeshProUGUI scoreValueText;
    [SerializeField] private TextMeshProUGUI rankLabelText;
    [SerializeField] private TextMeshProUGUI rankValueText;
    [SerializeField] private TextMeshProUGUI countdownLabelText;
    [SerializeField] private TextMeshProUGUI countdownValueText;

    [Header("Confirm panel")]
    [SerializeField] private GameObject confirmRoot;
    [SerializeField] private Button startChallengeButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI confirmBodyText;

    [Header("Result placeholder (optional legacy)")]
    [SerializeField] private GameObject resultRoot;
    [SerializeField] private Button resultCloseButton;
    [SerializeField] private TextMeshProUGUI resultBodyText;

    [Header("Config (optional override)")]
    [SerializeField] private DailyChallengeConfig configOverride;

    private DailyChallengeManager manager;
    private float nextCountdownRefreshUnscaled;
    private string lastCountdown = string.Empty;
    private bool viewLogged;
    private bool startInFlight;
    private bool syncInFlight;

    private void Awake()
    {
        if (confirmRoot != null)
        {
            confirmRoot.SetActive(false);
        }

        if (resultRoot != null)
        {
            resultRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        manager = DailyChallengeManager.EnsureInstance();

        if (manager != null)
        {
            manager.OnDailyStateChanged += RefreshPresentation;
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (viewResultButton != null)
        {
            viewResultButton.onClick.AddListener(OnViewResultClicked);
        }

        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        }

        if (startChallengeButton != null)
        {
            startChallengeButton.onClick.AddListener(OnStartChallengeClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        if (resultCloseButton != null)
        {
            resultCloseButton.onClick.AddListener(OnResultCloseClicked);
        }

        RefreshPresentation();
        _ = SyncAuthorityOnEnableAsync();
        if (!viewLogged && manager != null)
        {
            viewLogged = true;
            GameAnalytics.LogDailyChallengeView(manager.CurrentDayId);
        }
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.OnDailyStateChanged -= RefreshPresentation;
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
        }

        if (viewResultButton != null)
        {
            viewResultButton.onClick.RemoveListener(OnViewResultClicked);
        }

        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.RemoveListener(OnLeaderboardClicked);
        }

        if (startChallengeButton != null)
        {
            startChallengeButton.onClick.RemoveListener(OnStartChallengeClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        if (resultCloseButton != null)
        {
            resultCloseButton.onClick.RemoveListener(OnResultCloseClicked);
        }
    }

    private void Update()
    {
        if (Time.unscaledTime < nextCountdownRefreshUnscaled)
        {
            return;
        }

        nextCountdownRefreshUnscaled = Time.unscaledTime + 1f;
        RefreshCountdownOnly();

        if (manager != null)
        {
            string before = manager.CurrentDayId;
            manager.EnsureDaySynced();
            if (!string.Equals(before, manager.CurrentDayId))
            {
                DailyLeaderboardService.InvalidateCache();
                RefreshPresentation();
            }
        }
    }

    public void RefreshPresentation()
    {
        if (manager == null)
        {
            manager = DailyChallengeManager.EnsureInstance();
        }

        if (manager == null)
        {
            return;
        }

        manager.EnsureDaySynced();
        bool needsConnect = manager.NeedsConnectToPlay;
        bool available = manager.IsAttemptAvailable;
        bool hasEligible = manager.HasEligibleDailyLevel();
        bool canStart = available && hasEligible && !startInFlight;
        bool completed = manager.CurrentState == DailyChallengeState.Completed;
        bool abandoned = manager.CurrentState == DailyChallengeState.FailedOrAbandoned;

        if (titleText != null)
        {
            titleText.text = "DAILY CHALLENGE";
        }

        if (statusText != null)
        {
            if (needsConnect)
            {
                statusText.text = "CONNECT TO PLAY";
            }
            else if (available && !hasEligible)
            {
                statusText.text = "UNAVAILABLE";
            }
            else if (available)
            {
                statusText.text = "ONE ATTEMPT";
            }
            else if (completed)
            {
                statusText.text = "COMPLETED";
            }
            else
            {
                statusText.text = "ATTEMPT USED";
            }
        }

        bool showScore = false;
        string scoreFmt = string.Empty;
        string rankFmt = string.Empty;

        if (completed && manager.TryGetTodaysResult(out DailyChallengeResult result))
        {
            showScore = true;
            scoreFmt = DailyChallengeTimeFormat.FormatScore(result.Score);
            if (DailyLeaderboardService.TryGetCurrentPlayerRank(out int rank))
            {
                rankFmt = "#" + rank;
            }
        }

        SetOptionalText(scoreLabelText, showScore ? "SCORE" : string.Empty, showScore);
        SetOptionalText(scoreValueText, scoreFmt, showScore);
        SetOptionalText(rankLabelText, !string.IsNullOrEmpty(rankFmt) ? "RANK" : string.Empty,
            !string.IsNullOrEmpty(rankFmt));
        SetOptionalText(rankValueText, rankFmt, !string.IsNullOrEmpty(rankFmt));

        if (abandoned && scoreLabelText != null)
        {
            scoreLabelText.gameObject.SetActive(true);
            scoreLabelText.text = "NO SCORE TODAY";
            if (scoreValueText != null)
            {
                scoreValueText.gameObject.SetActive(false);
            }
        }

        if (playButton != null)
        {
            // Show PLAY for available, or RETRY when Online needs connectivity.
            playButton.gameObject.SetActive(available || needsConnect);
            playButton.interactable = canStart || needsConnect;
            var playLabel = playButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (playLabel != null)
            {
                playLabel.text = needsConnect ? "RETRY" : "PLAY";
            }
        }

        if (viewResultButton != null)
        {
            // Prefer LEADERBOARD as the post-attempt action when available.
            bool useLegacyView = leaderboardButton == null && !available;
            viewResultButton.gameObject.SetActive(useLegacyView);
            if (useLegacyView)
            {
                var label = viewResultButton.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                {
                    label.text = completed ? "VIEW RESULT" : "ATTEMPT USED";
                }
            }
        }

        if (leaderboardButton != null)
        {
            // Simulated GLOBAL board is always viewable (no fake YOU until completed).
            leaderboardButton.gameObject.SetActive(true);
            var lbLabel = leaderboardButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (lbLabel != null)
            {
                lbLabel.text = "LEADERBOARD";
            }
        }

        if (countdownLabelText != null)
        {
            countdownLabelText.text = "NEW CHALLENGE IN";
        }

        RefreshCountdownOnly();
    }

    private static void SetOptionalText(TextMeshProUGUI tmp, string value, bool visible)
    {
        if (tmp == null)
        {
            return;
        }

        tmp.gameObject.SetActive(visible);
        if (visible)
        {
            tmp.text = value ?? string.Empty;
        }
    }

    private void RefreshCountdownOnly()
    {
        if (manager == null || countdownValueText == null)
        {
            return;
        }

        string value = manager.FormatCountdown();
        if (value == lastCountdown)
        {
            return;
        }

        lastCountdown = value;
        countdownValueText.text = value;
    }

    private void OnPlayClicked()
    {
        if (manager == null)
        {
            return;
        }

        if (manager.NeedsConnectToPlay)
        {
            _ = SyncAuthorityOnEnableAsync();
            return;
        }

        if (!manager.IsAttemptAvailable)
        {
            return;
        }

        if (!manager.HasEligibleDailyLevel())
        {
            Debug.LogError(
                "[DailyChallenge] PLAY blocked — no eligible Classic levels in pool.");
            return;
        }

        if (confirmRoot != null)
        {
            if (confirmBodyText != null)
            {
                confirmBodyText.text =
                    "You only get one attempt at today's Daily Challenge.\n\n" +
                    "Once you start, your attempt is used — even if you quit early.";
            }

            confirmRoot.SetActive(true);
        }
        else
        {
            OnStartChallengeClicked();
        }
    }

    private void OnCancelClicked()
    {
        if (confirmRoot != null)
        {
            confirmRoot.SetActive(false);
        }
    }

    private void OnStartChallengeClicked()
    {
        if (manager == null || startInFlight)
        {
            return;
        }

        _ = StartChallengeAsync();
    }

    private async Task SyncAuthorityOnEnableAsync()
    {
        if (manager == null || syncInFlight)
        {
            return;
        }

        if (!manager.RequiresOnlineToStart)
        {
            return;
        }

        syncInFlight = true;
        try
        {
            await manager.SyncFromAuthorityAsync();
        }
        finally
        {
            syncInFlight = false;
            RefreshPresentation();
        }
    }

    private async Task StartChallengeAsync()
    {
        if (manager == null || startInFlight)
        {
            return;
        }

        startInFlight = true;
        RefreshPresentation();
        try
        {
            bool ok = await manager.TryCommitStartAndArmGameplayAsync();
            if (confirmRoot != null)
            {
                confirmRoot.SetActive(false);
            }

            if (!ok)
            {
                RefreshPresentation();
                return;
            }

            SceneTransition.LoadScene("Gameplay");
        }
        finally
        {
            startInFlight = false;
        }
    }

    private void OnLeaderboardClicked()
    {
        DailyLeaderboardUI board = DailyLeaderboardUI.FindInScene();
        if (board != null)
        {
            board.Show();
            return;
        }

        Debug.LogWarning(
            "[DailyChallenge] No DailyLeaderboardUI in MainMenu — " +
            "run Rush Out → UI → Create Daily Challenge Leaderboard UI.");

        if (resultRoot != null)
        {
            if (resultBodyText != null && manager != null)
            {
                resultBodyText.text = BuildResultBodyText();
            }

            resultRoot.SetActive(true);
        }
    }

    private void OnViewResultClicked()
    {
        DailyLeaderboardUI board = DailyLeaderboardUI.FindInScene();
        if (board != null)
        {
            board.Show();
            return;
        }

        if (resultRoot == null)
        {
            return;
        }

        if (resultBodyText != null && manager != null)
        {
            resultBodyText.text = BuildResultBodyText();
        }

        resultRoot.SetActive(true);
    }

    private string BuildResultBodyText()
    {
        if (manager == null)
        {
            return string.Empty;
        }

        if (manager.CurrentState == DailyChallengeState.Completed &&
            manager.TryGetTodaysResult(out DailyChallengeResult result))
        {
            string rankLine = DailyLeaderboardService.TryGetCurrentPlayerRank(out int rank)
                ? ("\n\nRANK\n#" + rank)
                : string.Empty;

            return
                "DAILY CHALLENGE COMPLETE!\n\n" +
                "SCORE\n" + DailyChallengeTimeFormat.FormatScore(result.Score) + "\n\n" +
                "MOVES\n" + result.Moves + "\n\n" +
                "TIME\n" + DailyChallengeTimeFormat.Format(result.CompletionTimeMilliseconds) +
                rankLine +
                "\n\nONE ATTEMPT COMPLETE";
        }

        return
            "ATTEMPT USED\n\n" +
            "NO SCORE TODAY\n\n" +
            "Challenge not completed.\n\n" +
            "New challenge in:\n" +
            manager.FormatCountdown();
    }

    private void OnResultCloseClicked()
    {
        if (resultRoot != null)
        {
            resultRoot.SetActive(false);
        }
    }
}

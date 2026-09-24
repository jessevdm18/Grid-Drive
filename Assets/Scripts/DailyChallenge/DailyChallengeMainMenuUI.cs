using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Authored MainMenu Daily Challenge card + confirmation + result placeholder.
/// Scene owns styling/layout; this script owns text VALUES and visibility.
/// </summary>
public class DailyChallengeMainMenuUI : MonoBehaviour
{
    [Header("Entry card")]
    [SerializeField] private GameObject entryRoot;
    [SerializeField] private Button playButton;
    [SerializeField] private Button viewResultButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI countdownLabelText;
    [SerializeField] private TextMeshProUGUI countdownValueText;

    [Header("Confirm panel")]
    [SerializeField] private GameObject confirmRoot;
    [SerializeField] private Button startChallengeButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI confirmBodyText;

    [Header("Result placeholder")]
    [SerializeField] private GameObject resultRoot;
    [SerializeField] private Button resultCloseButton;
    [SerializeField] private TextMeshProUGUI resultBodyText;

    [Header("Config (optional override)")]
    [SerializeField] private DailyChallengeConfig configOverride;

    private DailyChallengeManager manager;
    private float nextCountdownRefreshUnscaled;
    private string lastCountdown = string.Empty;
    private bool viewLogged;

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
        if (configOverride != null && manager != null)
        {
            // Manager loads Resources by default; override stays on this UI for authoring.
        }

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

        // Detect UTC day rollover while MainMenu stays open.
        if (manager != null)
        {
            string before = manager.CurrentDayId;
            manager.EnsureDaySynced();
            if (!string.Equals(before, manager.CurrentDayId))
            {
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
        bool available = manager.IsAttemptAvailable;

        if (titleText != null)
        {
            titleText.text = "DAILY CHALLENGE";
        }

        if (statusText != null)
        {
            switch (manager.CurrentState)
            {
                case DailyChallengeState.Available:
                    statusText.text = "ONE ATTEMPT";
                    break;
                case DailyChallengeState.Completed:
                    statusText.text = "COMPLETE";
                    break;
                default:
                    statusText.text = "ATTEMPT USED";
                    break;
            }
        }

        if (playButton != null)
        {
            playButton.gameObject.SetActive(available);
        }

        if (viewResultButton != null)
        {
            viewResultButton.gameObject.SetActive(!available);
            if (!available)
            {
                var label = viewResultButton.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                {
                    label.text = manager.CurrentState == DailyChallengeState.Completed
                        ? "VIEW RESULT"
                        : "ATTEMPT USED";
                }
            }
        }

        if (countdownLabelText != null)
        {
            countdownLabelText.text = "NEW CHALLENGE IN";
        }

        RefreshCountdownOnly();
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
        if (manager == null || !manager.IsAttemptAvailable)
        {
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
            // Fallback if confirm panel missing — still require explicit start path.
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
        if (manager == null)
        {
            return;
        }

        if (!manager.TryCommitStartAndArmGameplay())
        {
            if (confirmRoot != null)
            {
                confirmRoot.SetActive(false);
            }

            RefreshPresentation();
            return;
        }

        if (confirmRoot != null)
        {
            confirmRoot.SetActive(false);
        }

        SceneTransition.LoadScene("Gameplay");
    }

    private void OnViewResultClicked()
    {
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
            return
                "DAILY CHALLENGE COMPLETE!\n\n" +
                "SCORE\n" + DailyChallengeTimeFormat.FormatScore(result.Score) + "\n\n" +
                "MOVES\n" + result.Moves + "\n\n" +
                "TIME\n" + DailyChallengeTimeFormat.Format(result.CompletionTimeMilliseconds) +
                "\n\nONE ATTEMPT COMPLETE";
        }

        // Abandoned / attempt used — no score, no performance-fail messaging.
        return
            "ATTEMPT USED\n\n" +
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

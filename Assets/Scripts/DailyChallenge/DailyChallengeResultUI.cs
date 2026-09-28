using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Authored Daily Challenge completion result panel (Gameplay).
/// Scene owns styling; runtime fills score / moves / time / rank values.
/// </summary>
public class DailyChallengeResultUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI scoreLabelText;
    [SerializeField] private TextMeshProUGUI scoreValueText;
    [SerializeField] private TextMeshProUGUI movesLabelText;
    [SerializeField] private TextMeshProUGUI movesValueText;
    [SerializeField] private TextMeshProUGUI timeLabelText;
    [SerializeField] private TextMeshProUGUI timeValueText;
    [SerializeField] private TextMeshProUGUI rankLabelText;
    [SerializeField] private TextMeshProUGUI rankValueText;
    [SerializeField] private TextMeshProUGUI footerText;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button continueButton;

    private bool continueBound;
    private bool leaderboardBound;
    private bool showing;

    private void Awake()
    {
        if (root != null && root != gameObject)
        {
            root.SetActive(false);
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

    public void Show(DailyChallengeResult result)
    {
        showing = true;

        GameObject target = root != null ? root : gameObject;
        if (!target.activeSelf)
        {
            target.SetActive(true);
        }

        transform.SetAsLastSibling();

        if (titleText != null)
        {
            titleText.text = "DAILY CHALLENGE COMPLETE!";
        }

        if (scoreLabelText != null)
        {
            scoreLabelText.text = "SCORE";
        }

        if (scoreValueText != null)
        {
            scoreValueText.text = DailyChallengeTimeFormat.FormatScore(result.Score);
        }

        if (movesLabelText != null)
        {
            movesLabelText.text = "MOVES";
        }

        if (movesValueText != null)
        {
            movesValueText.text = result.Moves.ToString();
        }

        if (timeLabelText != null)
        {
            timeLabelText.text = "TIME";
        }

        if (timeValueText != null)
        {
            timeValueText.text = DailyChallengeTimeFormat.Format(result.CompletionTimeMilliseconds);
        }

        DailyLeaderboardService.InvalidateCache();
        int rank = 0;
        bool hasRank = DailyLeaderboardService.TryGetCurrentPlayerRank(out rank);

        if (rankLabelText != null)
        {
            rankLabelText.text = "RANK";
            rankLabelText.gameObject.SetActive(hasRank);
        }

        if (rankValueText != null)
        {
            rankValueText.text = hasRank ? ("#" + rank) : string.Empty;
            rankValueText.gameObject.SetActive(hasRank);
        }

        if (footerText != null)
        {
            footerText.text = "ONE ATTEMPT COMPLETE";
        }

        if (leaderboardButton != null)
        {
            leaderboardButton.gameObject.SetActive(true);
        }

        BindButtons();
        Time.timeScale = 0f;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[DailyChallenge] ResultUI shown score=" + result.Score +
            " moves=" + result.Moves +
            " ms=" + result.CompletionTimeMilliseconds +
            " rank=" + (hasRank ? rank.ToString() : "n/a"));
#endif
    }

    public void Hide()
    {
        showing = false;
        GameObject target = root != null ? root : gameObject;
        if (target.activeSelf)
        {
            target.SetActive(false);
        }
    }

    private void BindButtons()
    {
        if (continueButton != null && !continueBound)
        {
            continueButton.onClick.AddListener(OnContinue);
            continueBound = true;
        }

        if (leaderboardButton != null && !leaderboardBound)
        {
            leaderboardButton.onClick.AddListener(OnLeaderboard);
            leaderboardBound = true;
        }
    }

    private void UnbindButtons()
    {
        if (continueButton != null && continueBound)
        {
            continueButton.onClick.RemoveListener(OnContinue);
            continueBound = false;
        }

        if (leaderboardButton != null && leaderboardBound)
        {
            leaderboardButton.onClick.RemoveListener(OnLeaderboard);
            leaderboardBound = false;
        }
    }

    private void OnLeaderboard()
    {
        DailyLeaderboardUI board = DailyLeaderboardUI.FindInScene();
        if (board != null)
        {
            board.Show();
        }
        else
        {
            Debug.LogWarning(
                "[DailyChallenge] No DailyLeaderboardUI in Gameplay — " +
                "run Rush Out → UI → Create Daily Challenge Leaderboard UI.");
        }
    }

    private void OnContinue()
    {
        if (!showing)
        {
            return;
        }

        showing = false;
        Time.timeScale = 1f;
        DailyChallengeContext.ClearSession();
        SceneTransition.LoadScene("MainMenu");
    }
}

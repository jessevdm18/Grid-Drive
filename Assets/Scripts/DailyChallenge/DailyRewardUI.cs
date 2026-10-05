using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Authored Daily #1 winner reward panel. Scene owns layout; this script owns values + claim.
/// Grant local coins only after server claim returns granted=true.
/// </summary>
public class DailyRewardUI : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private GameObject panelRoot;

    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI claimButtonLabel;

    [Header("Actions")]
    [SerializeField] private Button claimButton;

    [Header("Refs (optional)")]
    [SerializeField] private CoinManager coinManager;

    private DailyRewardRecord currentReward;
    private bool hasReward;
    private bool claimInFlight;
    private bool resultOnly;

    public bool IsShowing =>
        panelRoot != null ? panelRoot.activeSelf : gameObject.activeSelf;

    private void Awake()
    {
        // panelRoot is this object in MainMenu. Deactivating self here runs on the
        // first Show and immediately hides the panel. The scene already starts inactive.
        if (panelRoot != null && panelRoot != gameObject)
        {
            panelRoot.SetActive(false);
        }

        if (titleText != null)
        {
            titleText.text = "DAILY WINNER";
        }
    }

    private void OnEnable()
    {
        if (claimButton != null)
        {
            claimButton.onClick.AddListener(OnClaimClicked);
        }
    }

    private void OnDisable()
    {
        if (claimButton != null)
        {
            claimButton.onClick.RemoveListener(OnClaimClicked);
        }
    }

    public static DailyRewardUI FindInScene()
    {
        return FindAnyObjectByType<DailyRewardUI>(FindObjectsInactive.Include);
    }

    /// <summary>Show reward if pending; hide otherwise.</summary>
    public void PresentPendingIfAny()
    {
        if (!DailyRewardService.TryGetFirstPending(out DailyRewardRecord reward))
        {
            Hide();
            return;
        }

        Show(reward);
    }

    public void Show(DailyRewardRecord reward)
    {
        currentReward = reward;
        hasReward = true;
        claimInFlight = false;
        resultOnly = false;

        if (titleText != null)
        {
            titleText.text = "DAILY WINNER";
        }

        if (rankText != null)
        {
            rankText.text = "#" + Mathf.Max(1, reward.Rank);
        }

        if (bodyText != null)
        {
            int amount = reward.RewardAmount > 0
                ? reward.RewardAmount
                : DailyRewardService.FirstPlaceCoinReward;
            bodyText.text =
                "YOU WON " + amount + " COINS\n\n" +
                FormatStats(reward.Score, reward.Moves, reward.CompletionTimeMilliseconds);
        }

        SetClaimInteractable(true);
        if (claimButtonLabel != null)
        {
            claimButtonLabel.text = "CLAIM";
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>Editor/dev preview without server.</summary>
    public void ShowPreview(string dayId = null)
    {
        DailyRewardRecord preview = DailyRewardRecord.PreviewFirstPlace(
            string.IsNullOrEmpty(dayId) ? "preview" : dayId);
        DailyRewardService.SetPreviewPending(preview);
        Show(preview);
    }

    /// <summary>
    /// Final settled placement. Rank #1 with an unclaimed reward uses the existing claim panel.
    /// Other ranks dismiss only — no coin grant.
    /// </summary>
    public bool ShowSettledResult(DailySettledResult settled)
    {
        if (!settled.Found || !settled.Settled || settled.Rank <= 0)
        {
            return false;
        }

        if (settled.HasUnclaimedReward)
        {
            int amount = settled.RewardAmount > 0
                ? settled.RewardAmount
                : DailyRewardService.FirstPlaceCoinReward;
            Show(new DailyRewardRecord(
                settled.DayId,
                settled.Rank,
                "coins",
                amount,
                claimed: false,
                source: "daily_first_place",
                settled.Score,
                settled.Moves,
                settled.CompletionTimeMilliseconds,
                isDevelopmentTest: false));
            return IsShowing;
        }

        resultOnly = true;
        hasReward = false;
        claimInFlight = false;
        currentReward = default;

        if (titleText != null)
        {
            titleText.text = "DAILY CHALLENGE RESULTS";
        }

        if (rankText != null)
        {
            rankText.text = "#" + settled.Rank;
        }

        if (bodyText != null)
        {
            bodyText.text = FormatStats(
                settled.Score,
                settled.Moves,
                settled.CompletionTimeMilliseconds);
        }

        SetClaimInteractable(true);
        if (claimButtonLabel != null)
        {
            claimButtonLabel.text = "OK";
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

        return IsShowing;
    }

    public void Hide()
    {
        hasReward = false;
        claimInFlight = false;
        resultOnly = false;
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void OnClaimClicked()
    {
        if (resultOnly)
        {
            Hide();
            return;
        }

        if (claimInFlight || !hasReward)
        {
            return;
        }

        _ = ClaimAsync();
    }

    private static string FormatStats(int score, int moves, long completionTimeMs)
    {
        return
            "SCORE\n" + DailyChallengeTimeFormat.FormatScore(score) + "\n\n" +
            "MOVES\n" + moves + "\n\n" +
            "TIME\n" + DailyChallengeTimeFormat.Format(completionTimeMs);
    }

    private async Task ClaimAsync()
    {
        if (claimInFlight || !hasReward)
        {
            return;
        }

        claimInFlight = true;
        SetClaimInteractable(false);
        if (claimButtonLabel != null)
        {
            claimButtonLabel.text = "CLAIMING...";
        }

        string dayId = currentReward.DayId;
        try
        {
            // Preview path: no server — do not grant real coins.
            if (string.Equals(dayId, "preview", System.StringComparison.Ordinal))
            {
                Hide();
                DailyRewardService.ClearCachedPending();
                return;
            }

            DailyRewardClaimResult result = await DailyRewardService.ClaimAsync(dayId);
            if (!result.Success)
            {
                if (claimButtonLabel != null)
                {
                    claimButtonLabel.text = "CLAIM";
                }

                SetClaimInteractable(true);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning(
                    "[DailyReward] Claim failed code=" + result.ErrorCode +
                    " " + result.ErrorMessage);
#endif
                return;
            }

            // Only grant local coins when server says this claim newly granted.
            // AlreadyClaimed must NOT add coins again (crash-window / double-tap safety).
            if (result.Granted)
            {
                CoinManager coins = coinManager != null
                    ? coinManager
                    : CoinManager.EnsureInstance();
                if (coins != null)
                {
                    int amount = result.RewardAmount > 0
                        ? result.RewardAmount
                        : DailyRewardService.FirstPlaceCoinReward;
                    coins.AddCoins(amount);
                }

                AudioManager.TryPlayRewardReceived();
            }

            Hide();
        }
        finally
        {
            claimInFlight = false;
        }
    }

    private void SetClaimInteractable(bool interactable)
    {
        if (claimButton != null)
        {
            claimButton.interactable = interactable && !claimInFlight;
        }
    }
}

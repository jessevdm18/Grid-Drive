using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Settled Daily result panel. Every completed player sees rank, score, moves, and time.
/// Rank #1 with an unclaimed reward also shows the coin line and CLAIM.
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
    [SerializeField] private DailyChallengeResultSettings presentation;

    private DailyChallengeResultSettings Presentation
    {
        get
        {
            if (presentation == null)
            {
                presentation = DailyChallengeResultSettings.LoadDefault();
            }

            return presentation;
        }
    }

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
            titleText.text = Presentation.resultTitle;
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

        int amount = reward.RewardAmount > 0
            ? reward.RewardAmount
            : DailyRewardService.CoinsForRank(reward.Rank);
        ApplyPresentation(
            reward.Rank,
            reward.Score,
            reward.Moves,
            reward.CompletionTimeMilliseconds,
            includeReward: true,
            amount,
            Presentation.claimButtonLabel);
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
                : DailyRewardService.CoinsForRank(settled.Rank);
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
        ApplyPresentation(
            settled.Rank,
            settled.Score,
            settled.Moves,
            settled.CompletionTimeMilliseconds,
            includeReward: false,
            0,
            Presentation.okButtonLabel);
        return IsShowing;
    }

    private void ApplyPresentation(
        int rank,
        int score,
        int moves,
        long completionTimeMs,
        bool includeReward,
        int rewardAmount,
        string buttonLabel)
    {
        if (titleText != null)
        {
            titleText.text = Presentation.resultTitle;
        }

        if (rankText != null)
        {
            rankText.gameObject.SetActive(true);
            rankText.alignment = TextAlignmentOptions.Top;
            rankText.text =
                "<size=32>" + Presentation.rankLabel + "</size>\n<size=68>#" +
                Mathf.Max(1, rank) + "</size>";
        }

        if (bodyText != null)
        {
            bodyText.alignment = TextAlignmentOptions.TopLeft;
            bodyText.text = FormatResult(
                rank,
                score,
                moves,
                completionTimeMs,
                includeReward,
                rewardAmount,
                rankText != null);
        }

        PlaceResultBlock(includeReward);

        SetClaimInteractable(true);
        if (claimButtonLabel != null)
        {
            claimButtonLabel.text = buttonLabel;
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

    private string FormatResult(
        int rank,
        int score,
        int moves,
        long completionTimeMs,
        bool includeReward,
        int rewardAmount,
        bool rankIsSeparate)
    {
        string scoreText = DailyChallengeTimeFormat.FormatScore(score);
        string timeText = DailyChallengeTimeFormat.Format(completionTimeMs);
        DailyChallengeResultSettings copy = Presentation;
        string stats =
            "<size=26>" + copy.scoreLabel + "<pos=33%>" + copy.movesLabel +
            "<pos=66%>" + copy.timeLabel + "</size>\n" +
            "<size=40><b>" + scoreText + "<pos=33%>" + moves + "<pos=66%>" + timeText + "</b></size>";

        string text = rankIsSeparate
            ? stats
            : "<size=32>" + copy.rankLabel + "</size>\n<size=68>#" +
              Mathf.Max(1, rank) + "</size>\n" + stats;

        if (includeReward)
        {
            text +=
                "\n<align=center><size=28>" + copy.rewardLabel + "</size>\n<size=44><b>" +
                rewardAmount + " " + copy.coinsSuffix + "</b></size></align>";
        }

        return text;
    }

    /// <summary>
    /// Keeps title, rank, and stats stacked tightly in the upper-middle of the large card.
    /// Reward grows the body downward. No reward leaves no empty placeholder.
    /// </summary>
    private void PlaceResultBlock(bool includeReward)
    {
        if (titleText == null || bodyText == null)
        {
            return;
        }

        float top = includeReward ? 48f : 120f;
        const float titleHeight = 100f;
        const float rankHeight = 176f;
        const float gap = 12f;
        float bodyHeight = includeReward ? 250f : 120f;

        PlaceTop(titleText.rectTransform, top, titleHeight);
        float cursor = top + titleHeight + gap;
        if (rankText != null)
        {
            PlaceTop(rankText.rectTransform, cursor, rankHeight);
            cursor += rankHeight + gap;
        }

        PlaceTop(bodyText.rectTransform, cursor, bodyHeight);
    }

    private static void PlaceTop(RectTransform rect, float top, float height)
    {
        if (rect == null)
        {
            return;
        }

        float xMin = rect.anchorMin.x;
        float xMax = rect.anchorMax.x;
        if (xMax - xMin < 0.5f)
        {
            xMin = 0.05f;
            xMax = 0.95f;
        }

        rect.anchorMin = new Vector2(xMin, 1f);
        rect.anchorMax = new Vector2(xMax, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -top);
        rect.sizeDelta = new Vector2(0f, height);
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
            claimButtonLabel.text = Presentation.claimingButtonLabel;
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
                    claimButtonLabel.text = Presentation.claimButtonLabel;
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
                        : DailyRewardService.CoinsForRank(result.Rank);
                    if (amount > 0)
                    {
                        coins.AddCoins(amount);
                        GameAnalytics.LogCoinReward(amount, "daily_reward");
                    }
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

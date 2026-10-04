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

    private void Awake()
    {
        if (panelRoot != null)
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
            bodyText.text = "YOU WON " + amount + " COINS";
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

    public void Hide()
    {
        hasReward = false;
        claimInFlight = false;
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void OnClaimClicked()
    {
        if (claimInFlight || !hasReward)
        {
            return;
        }

        _ = ClaimAsync();
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

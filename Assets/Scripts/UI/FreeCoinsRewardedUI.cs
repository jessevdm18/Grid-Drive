using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shop Coins tab: watch rewarded ad for +50 coins, max 3 per local calendar day.
/// Reuses AdsManager.ShowRewardedAd; does not touch FREE HINT.
/// </summary>
public class FreeCoinsRewardedUI : MonoBehaviour
{
    public const int RewardAmount = 50;
    public const int DailyMax = 3;

    public const string DatePrefsKey = "GridDrive_FreeCoinsRewardedDate";
    public const string UsedPrefsKey = "GridDrive_FreeCoinsRewardedUsed";
    private const string PlacementId = AdsManager.PlacementShopFreeCoins;

    private const string CtaWatchLabel = "WATCH AD — +50";
    private const string CtaTomorrowLabel = "COME BACK TOMORROW";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI availabilityText;
    [SerializeField] private Button ctaButton;
    [SerializeField] private TextMeshProUGUI ctaLabelText;

    [Header("Refs")]
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private AdsManager adsManager;
    [SerializeField] private AudioManager audioManager;

    private bool isAdFlowActive;
    private bool rewardHandledThisFlow;
    private bool offerPromptAfterFlow;

    /// <summary>
    /// Remaining free coin videos for the current local calendar day.
    /// Does not write PlayerPrefs. A new local day has all <see cref="DailyMax"/> available.
    /// </summary>
    public static int GetRemainingAvailable()
    {
        string today = GetTodayLocalDateString();
        string saved = PlayerPrefs.GetString(DatePrefsKey, string.Empty);
        if (!string.Equals(saved, today, StringComparison.Ordinal))
        {
            return DailyMax;
        }

        int used = Mathf.Clamp(PlayerPrefs.GetInt(UsedPrefsKey, 0), 0, DailyMax);
        return Mathf.Max(0, DailyMax - used);
    }

    /// <summary>
    /// Next local midnight, when a partial day becomes 3 available again.
    /// False while all 3 are already available.
    /// </summary>
    public static bool TryGetNextAllAvailableLocal(out DateTime nextLocalMidnight)
    {
        nextLocalMidnight = default;
        if (GetRemainingAvailable() >= DailyMax)
        {
            return false;
        }

        nextLocalMidnight = DateTime.Now.Date.AddDays(1);
        return true;
    }

    private void Awake()
    {
        ResolveRefs();

        if (titleText != null)
        {
            titleText.text = "FREE COINS";
        }

        if (ctaButton != null)
        {
            ctaButton.onClick.RemoveListener(OnWatchAdClicked);
            ctaButton.onClick.AddListener(OnWatchAdClicked);
        }
    }

    private void OnEnable()
    {
        ResolveRefs();
        Refresh();
    }

    /// <summary>Call when Shop opens or Coins tab becomes active.</summary>
    public void Refresh()
    {
        EnsureDayRollover();

        int remaining = GetRemainingToday();
        bool canWatch = remaining > 0 && !isAdFlowActive;

        if (availabilityText != null)
        {
            availabilityText.text = remaining + "/" + DailyMax + " available today";
        }

        if (ctaLabelText != null)
        {
            ctaLabelText.text = remaining > 0 ? CtaWatchLabel : CtaTomorrowLabel;
        }

        if (ctaButton != null)
        {
            ctaButton.interactable = canWatch;
        }
    }

    private void OnWatchAdClicked()
    {
        if (isAdFlowActive)
        {
            return;
        }

        EnsureDayRollover();

        if (GetRemainingToday() <= 0)
        {
            Refresh();
            return;
        }

        ResolveRefs();

        if (adsManager == null)
        {
            Debug.LogWarning("FreeCoinsRewardedUI: AdsManager missing.");
            return;
        }

        if (coinManager == null)
        {
            Debug.LogWarning("FreeCoinsRewardedUI: CoinManager missing.");
            return;
        }

        if (!adsManager.IsRewardedAdReady(PlacementId))
        {
            Debug.LogWarning("FreeCoinsRewardedUI: rewarded ad not ready.");
            return;
        }

        isAdFlowActive = true;
        rewardHandledThisFlow = false;
        Refresh();

        adsManager.ShowRewardedAd(OnRewardEarned, PlacementId, OnAdFlowEnded);
    }

    private void OnRewardEarned()
    {
        if (rewardHandledThisFlow)
        {
            Debug.Log("FreeCoinsRewardedUI: ignoring duplicate reward callback.");
            return;
        }

        rewardHandledThisFlow = true;

        EnsureDayRollover();

        if (GetRemainingToday() <= 0)
        {
            return;
        }

        if (coinManager == null)
        {
            coinManager = FindAnyObjectByType<CoinManager>();
        }

        if (coinManager == null)
        {
            return;
        }

        coinManager.AddCoins(RewardAmount);
        IncrementUsedToday();
        if (GetRemainingToday() <= 0)
        {
            offerPromptAfterFlow = true;
        }

        audioManager?.PlayUpgrade();
        Refresh();
    }

    private void OnAdFlowEnded()
    {
        isAdFlowActive = false;
        Refresh();
        if (!offerPromptAfterFlow)
        {
            return;
        }

        offerPromptAfterFlow = false;
        NotificationPromptCoordinator.NotifyFreeCoinsExhausted();
    }

    private void EnsureDayRollover()
    {
        string today = GetTodayLocalDateString();
        string saved = PlayerPrefs.GetString(DatePrefsKey, string.Empty);

        if (string.Equals(saved, today, StringComparison.Ordinal))
        {
            return;
        }

        PlayerPrefs.SetString(DatePrefsKey, today);
        PlayerPrefs.SetInt(UsedPrefsKey, 0);
        PlayerPrefs.Save();
        FreeCoinsNotificationService.Sync();
    }

    private int GetUsedToday()
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(UsedPrefsKey, 0), 0, DailyMax);
    }

    private int GetRemainingToday()
    {
        return Mathf.Max(0, DailyMax - GetUsedToday());
    }

    private void IncrementUsedToday()
    {
        int used = GetUsedToday() + 1;
        if (used > DailyMax)
        {
            used = DailyMax;
        }

        PlayerPrefs.SetString(DatePrefsKey, GetTodayLocalDateString());
        PlayerPrefs.SetInt(UsedPrefsKey, used);
        PlayerPrefs.Save();
        FreeCoinsNotificationService.Sync();
    }

    private static string GetTodayLocalDateString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }

    private void ResolveRefs()
    {
        if (coinManager == null)
        {
            coinManager = FindAnyObjectByType<CoinManager>();
        }

        if (adsManager == null)
        {
            adsManager = FindAnyObjectByType<AdsManager>();
        }

        if (audioManager == null)
        {
            audioManager = AudioManager.Resolve();
        }
    }
}

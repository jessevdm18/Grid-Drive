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

    private const string DatePrefsKey = "GridDrive_FreeCoinsRewardedDate";
    private const string UsedPrefsKey = "GridDrive_FreeCoinsRewardedUsed";
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
        audioManager?.PlayUpgrade();
        Refresh();
    }

    private void OnAdFlowEnded()
    {
        isAdFlowActive = false;
        Refresh();
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
            audioManager = FindAnyObjectByType<AudioManager>();
        }
    }
}

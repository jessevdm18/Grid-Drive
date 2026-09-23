using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentation for LivesManager.OnLevelAttemptBlocked.
/// Visual hierarchy is authored in-scene — this script only drives state/actions.
/// Host GameObject stays active; <see cref="root"/> is the inactive panel.
/// </summary>
public class OutOfLivesUI : MonoBehaviour
{
    private const float TickSeconds = 1f;
    private const float StatusMessageSeconds = 1.6f;

    [Header("Authored panel (required)")]
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject inputBlocker;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI nextLifeLabelText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button okButton;
    [SerializeField] private TextMeshProUGUI okButtonLabel;
    [SerializeField] private Button useCoinsButton;
    [SerializeField] private TextMeshProUGUI useCoinsButtonLabel;
    [SerializeField] private Button watchAdButton;
    [SerializeField] private TextMeshProUGUI watchAdButtonLabel;

    [Header("Copy")]
    [SerializeField] private string title = "OUT OF LIVES";
    [SerializeField] private string bodyNoLives = "You need a life to play.";
    [SerializeField] private string bodyLifeAvailable = "A life is available.";
    [SerializeField] private string nextLifeLabel = "Next life in:";
    [SerializeField] private string okLabel = "OK";
    [SerializeField] private string notEnoughCoinsMessage = "NOT ENOUGH COINS";
    [SerializeField] private string adNotAvailableMessage = "AD NOT AVAILABLE";

    private LivesManager livesManager;
    private CoinManager coinManager;
    private AdsManager adsManager;
    private AudioManager audioManager;
    private Coroutine tickRoutine;
    private Coroutine statusRoutine;
    private bool isOpen;
    private bool purchaseInProgress;
    private bool coinSubscribed;
    private bool adFlowActive;
    private int activeRewardedAttemptToken;
    private bool rewardHandledThisAttempt;
    private string lastBlockedSource = string.Empty;

    public bool IsOpen => isOpen;

    /// <summary>Finds authored OutOfLivesUI. Does not create visuals.</summary>
    public static OutOfLivesUI FindInLoadedScenes()
    {
        return FindAnyObjectByType<OutOfLivesUI>();
    }

    private void Awake()
    {
        if (root != null && Application.isPlaying)
        {
            root.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        WireButtons();
        Subscribe(true);
        HideImmediate();
    }

    private void OnDisable()
    {
        UnsubscribeOnly();
        StopTick();
        StopStatusMessage();
        purchaseInProgress = false;
    }

    private void OnDestroy()
    {
        UnsubscribeOnly();
        StopTick();
        StopStatusMessage();
        purchaseInProgress = false;
    }

    private void ResolveManager(bool allowCreate)
    {
        if (allowCreate)
        {
            livesManager = LivesManager.EnsureInstance();
        }
        else if (livesManager == null)
        {
            livesManager = LivesManager.Instance;
        }
    }

    private void ResolveCoinManager()
    {
        if (coinManager == null)
        {
            coinManager = LifePurchaseService.ResolveCoinManager();
        }
    }

    private void ResolveAdsManager()
    {
        if (adsManager == null)
        {
            adsManager = LifePurchaseService.ResolveAdsManager();
        }
    }

    private void ResolveAudioManager()
    {
        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }
    }

    private void Subscribe(bool bind)
    {
        if (bind)
        {
            ResolveManager(allowCreate: true);
            ResolveCoinManager();
            ResolveAdsManager();
        }
        else
        {
            ResolveManager(allowCreate: false);
        }

        if (livesManager != null)
        {
            livesManager.OnLevelAttemptBlocked -= OnLevelAttemptBlocked;
            livesManager.OnLivesChanged -= OnLivesChanged;
            if (bind)
            {
                livesManager.OnLevelAttemptBlocked += OnLevelAttemptBlocked;
                livesManager.OnLivesChanged += OnLivesChanged;
            }
        }

        SubscribeCoins(bind);
    }

    private void SubscribeCoins(bool bind)
    {
        if (coinManager != null && coinSubscribed)
        {
            coinManager.OnCoinsChanged -= OnCoinsChanged;
            coinSubscribed = false;
        }

        if (!bind)
        {
            return;
        }

        ResolveCoinManager();
        if (coinManager == null)
        {
            return;
        }

        coinManager.OnCoinsChanged -= OnCoinsChanged;
        coinManager.OnCoinsChanged += OnCoinsChanged;
        coinSubscribed = true;
    }

    private void UnsubscribeOnly()
    {
        if (livesManager == null)
        {
            livesManager = LivesManager.Instance;
        }

        if (livesManager != null)
        {
            livesManager.OnLevelAttemptBlocked -= OnLevelAttemptBlocked;
            livesManager.OnLivesChanged -= OnLivesChanged;
        }

        SubscribeCoins(false);
    }

    private void WireButtons()
    {
        if (okButton != null)
        {
            okButton.onClick.RemoveListener(OnOkClicked);
            okButton.onClick.AddListener(OnOkClicked);
        }

        if (useCoinsButton != null)
        {
            useCoinsButton.onClick.RemoveListener(OnUseCoinsClicked);
            useCoinsButton.onClick.AddListener(OnUseCoinsClicked);
        }

        if (watchAdButton != null)
        {
            watchAdButton.onClick.RemoveListener(OnWatchAdClicked);
            watchAdButton.onClick.AddListener(OnWatchAdClicked);
        }

        RefreshActionButtons();
    }

    private void ApplyPurchaseButtonVisibility(bool showPurchaseActions)
    {
        if (useCoinsButton != null)
        {
            useCoinsButton.gameObject.SetActive(showPurchaseActions);
        }

        if (watchAdButton != null)
        {
            watchAdButton.gameObject.SetActive(showPurchaseActions);
        }
    }

    private void OnLevelAttemptBlocked(string source)
    {
        lastBlockedSource = source ?? string.Empty;
        Show();
    }

    private void OnLivesChanged(int _)
    {
        if (!isOpen)
        {
            return;
        }

        RefreshOpenContent();
    }

    private void OnCoinsChanged(int _)
    {
        if (!isOpen)
        {
            return;
        }

        RefreshUseCoinsButton();
    }

    public void Show()
    {
        if (root == null)
        {
            Debug.LogError(
                "OutOfLivesUI: authored root panel is missing — assign OutOfLivesPanel in Inspector."
            );
            return;
        }

        WireButtons();
        isOpen = true;
        purchaseInProgress = false;
        if (!adFlowActive)
        {
            rewardHandledThisAttempt = false;
            activeRewardedAttemptToken = 0;
        }

        ClearStatusMessageImmediate();

        root.SetActive(true);
        if (inputBlocker != null)
        {
            inputBlocker.SetActive(true);
        }

        RefreshOpenContent();
        StartTick();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[Lives] OutOfLivesUI shown | source=" + lastBlockedSource +
            " lives=" + (livesManager != null ? livesManager.CurrentLives : -1) +
            " coins=" + (coinManager != null ? coinManager.Coins : -1)
        );
#endif
    }

    public void Hide()
    {
        isOpen = false;
        purchaseInProgress = false;
        StopTick();
        StopStatusMessage();
        ClearStatusMessageImmediate();
        HideImmediate();
    }

    private void HideImmediate()
    {
        if (root != null)
        {
            root.SetActive(false);
        }

        if (inputBlocker != null)
        {
            inputBlocker.SetActive(false);
        }
    }

    private void OnOkClicked()
    {
        Hide();
    }

    private void OnUseCoinsClicked()
    {
        if (purchaseInProgress || adFlowActive || !isOpen)
        {
            return;
        }

        purchaseInProgress = true;
        RefreshActionButtons();

        try
        {
            ResolveCoinManager();
            ResolveManager(allowCreate: true);
            ResolveAudioManager();

            LifePurchaseResult result = LifePurchaseService.TryPurchaseLifeWithCoins(
                coinManager,
                livesManager
            );

            switch (result)
            {
                case LifePurchaseResult.Success:
                    audioManager?.PlayCoinSpend();
                    Hide();
                    break;

                case LifePurchaseResult.NotEnoughCoins:
                    audioManager?.PlayInsufficientCoins();
                    ShowStatusMessage(notEnoughCoinsMessage);
                    break;

                case LifePurchaseResult.LivesFull:
                    Hide();
                    break;

                default:
                    ShowStatusMessage("UNAVAILABLE");
                    break;
            }
        }
        finally
        {
            purchaseInProgress = false;
            if (isOpen)
            {
                RefreshOpenContent();
            }
        }
    }

    private void OnWatchAdClicked()
    {
        if (adFlowActive || purchaseInProgress || !isOpen)
        {
            return;
        }

        ResolveAdsManager();
        ResolveManager(allowCreate: true);
        ResolveAudioManager();

        if (adsManager == null)
        {
            ShowStatusMessage(adNotAvailableMessage);
            return;
        }

        if (!adsManager.IsRewardedAdReady(AdsManager.PlacementExtraLife))
        {
            ShowStatusMessage(adNotAvailableMessage);
            RefreshActionButtons();
            return;
        }

        adFlowActive = true;
        rewardHandledThisAttempt = false;
        activeRewardedAttemptToken = LifePurchaseService.BeginRewardedLifeAttempt();
        RefreshActionButtons();

        int attemptToken = activeRewardedAttemptToken;
        adsManager.ShowRewardedAd(
            () => OnExtraLifeRewardEarned(attemptToken),
            AdsManager.PlacementExtraLife,
            () => OnExtraLifeAdFlowEnded(attemptToken)
        );
    }

    private void OnExtraLifeRewardEarned(int attemptToken)
    {
        if (rewardHandledThisAttempt &&
            (activeRewardedAttemptToken == 0 || attemptToken == activeRewardedAttemptToken))
        {
            return;
        }

        rewardHandledThisAttempt = true;

        LifePurchaseResult result = LifePurchaseService.GrantLifeFromRewardedAd(
            attemptToken,
            LivesManager.Instance != null ? LivesManager.Instance : livesManager
        );

        switch (result)
        {
            case LifePurchaseResult.Success:
                if (this != null)
                {
                    Hide();
                }

                break;

            case LifePurchaseResult.LivesFull:
                if (this != null && isOpen)
                {
                    ShowStatusMessage("LIVES FULL");
                }

                break;

            default:
                if (this != null && isOpen)
                {
                    ShowStatusMessage("UNAVAILABLE");
                }

                break;
        }
    }

    private void OnExtraLifeAdFlowEnded(int attemptToken)
    {
        if (attemptToken != activeRewardedAttemptToken && activeRewardedAttemptToken != 0)
        {
            return;
        }

        adFlowActive = false;

        if (this == null)
        {
            return;
        }

        if (isOpen)
        {
            if (!rewardHandledThisAttempt)
            {
                RefreshOpenContent();
            }
            else
            {
                RefreshActionButtons();
            }
        }
    }

    private void RefreshOpenContent()
    {
        ResolveManager(allowCreate: true);
        ResolveCoinManager();
        ResolveAdsManager();
        if (livesManager == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        bool hasLife = livesManager.HasLives;
        if (bodyText != null && statusRoutine == null)
        {
            bodyText.text = hasLife ? bodyLifeAvailable : bodyNoLives;
        }

        if (nextLifeLabelText != null)
        {
            nextLifeLabelText.text = hasLife ? string.Empty : nextLifeLabel;
            nextLifeLabelText.gameObject.SetActive(!hasLife);
        }

        if (countdownText != null)
        {
            if (hasLife)
            {
                countdownText.text = LivesDisplayFormatting.FormatLivesFraction(
                    livesManager.CurrentLives,
                    LivesManager.MaxLives
                );
            }
            else
            {
                countdownText.text = LivesDisplayFormatting.FormatCountdown(
                    livesManager.TimeUntilNextLife
                );
            }
        }

        if (okButtonLabel != null)
        {
            okButtonLabel.text = okLabel;
        }

        if (watchAdButtonLabel != null)
        {
            watchAdButtonLabel.text = "WATCH AD";
        }

        ApplyPurchaseButtonVisibility(!hasLife);
        RefreshActionButtons();
    }

    private void RefreshActionButtons()
    {
        RefreshUseCoinsButton();
        RefreshWatchAdButton();
    }

    private void RefreshUseCoinsButton()
    {
        if (useCoinsButtonLabel != null)
        {
            useCoinsButtonLabel.text = "USE " + LivesEconomyConfig.LifeCoinCost + " COINS";
        }

        if (useCoinsButton == null)
        {
            return;
        }

        ResolveCoinManager();
        bool canAfford = coinManager != null &&
                         coinManager.CanAfford(LivesEconomyConfig.LifeCoinCost);
        bool livesNeedFill = livesManager == null || !livesManager.IsFull;
        useCoinsButton.interactable =
            isOpen &&
            !purchaseInProgress &&
            !adFlowActive &&
            canAfford &&
            livesNeedFill &&
            useCoinsButton.gameObject.activeSelf;
    }

    private void RefreshWatchAdButton()
    {
        if (watchAdButton == null)
        {
            return;
        }

        bool livesNeedFill = livesManager == null || !livesManager.IsFull;
        watchAdButton.interactable =
            isOpen &&
            !purchaseInProgress &&
            !adFlowActive &&
            livesNeedFill &&
            watchAdButton.gameObject.activeSelf;
    }

    private void ShowStatusMessage(string message)
    {
        StopStatusMessage();
        if (statusText != null)
        {
            statusText.text = message;
            statusText.gameObject.SetActive(true);
        }
        else if (bodyText != null)
        {
            bodyText.text = message;
        }

        if (isActiveAndEnabled && Application.isPlaying)
        {
            statusRoutine = StartCoroutine(ClearStatusAfterDelay());
        }
    }

    private IEnumerator ClearStatusAfterDelay()
    {
        yield return new WaitForSecondsRealtime(StatusMessageSeconds);
        statusRoutine = null;
        ClearStatusMessageImmediate();
        if (isOpen)
        {
            RefreshOpenContent();
        }
    }

    private void StopStatusMessage()
    {
        if (statusRoutine != null)
        {
            StopCoroutine(statusRoutine);
            statusRoutine = null;
        }
    }

    private void ClearStatusMessageImmediate()
    {
        if (statusText != null)
        {
            statusText.text = string.Empty;
            statusText.gameObject.SetActive(false);
        }
    }

    private void StartTick()
    {
        StopTick();
        if (!isActiveAndEnabled || !isOpen || !Application.isPlaying)
        {
            return;
        }

        tickRoutine = StartCoroutine(CountdownTick());
    }

    private void StopTick()
    {
        if (tickRoutine != null)
        {
            StopCoroutine(tickRoutine);
            tickRoutine = null;
        }
    }

    private IEnumerator CountdownTick()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(TickSeconds);
        while (enabled && isOpen && livesManager != null)
        {
            yield return wait;

            if (!isOpen || livesManager == null)
            {
                yield break;
            }

            if (!livesManager.HasLives &&
                livesManager.TimeUntilNextLife <= TimeSpan.Zero)
            {
                livesManager.RecalculateRegeneration();
            }

            RefreshOpenContent();
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Preview Popup")]
    private void ContextPreviewPopup()
    {
        if (root == null)
        {
            Debug.LogWarning("OutOfLivesUI: root not assigned.");
            return;
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (bodyText != null)
        {
            bodyText.text = bodyNoLives;
        }

        if (nextLifeLabelText != null)
        {
            nextLifeLabelText.text = nextLifeLabel;
            nextLifeLabelText.gameObject.SetActive(true);
        }

        if (countdownText != null)
        {
            countdownText.text = "1:42:18";
        }

        if (statusText != null)
        {
            statusText.text = string.Empty;
            statusText.gameObject.SetActive(false);
        }

        if (useCoinsButtonLabel != null)
        {
            useCoinsButtonLabel.text = "USE " + LivesEconomyConfig.LifeCoinCost + " COINS";
        }

        if (watchAdButtonLabel != null)
        {
            watchAdButtonLabel.text = "WATCH AD";
        }

        if (okButtonLabel != null)
        {
            okButtonLabel.text = okLabel;
        }

        if (useCoinsButton != null)
        {
            useCoinsButton.gameObject.SetActive(true);
        }

        if (watchAdButton != null)
        {
            watchAdButton.gameObject.SetActive(true);
        }

        root.SetActive(true);
        if (inputBlocker != null)
        {
            inputBlocker.SetActive(true);
        }

        UnityEditor.EditorUtility.SetDirty(root);
    }

    [ContextMenu("Hide Popup Preview")]
    private void ContextHidePopupPreview()
    {
        if (root != null)
        {
            root.SetActive(false);
            UnityEditor.EditorUtility.SetDirty(root);
        }

        if (inputBlocker != null)
        {
            inputBlocker.SetActive(false);
        }
    }
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void DebugSimulateRewardedLifeSuccess()
    {
        if (adFlowActive || purchaseInProgress)
        {
            return;
        }

        adFlowActive = true;
        rewardHandledThisAttempt = false;
        activeRewardedAttemptToken = LifePurchaseService.BeginRewardedLifeAttempt();
        int token = activeRewardedAttemptToken;
        OnExtraLifeRewardEarned(token);
        OnExtraLifeAdFlowEnded(token);
    }

    public void DebugSimulateRewardedLifeNoReward()
    {
        if (adFlowActive || purchaseInProgress)
        {
            return;
        }

        adFlowActive = true;
        rewardHandledThisAttempt = false;
        activeRewardedAttemptToken = LifePurchaseService.BeginRewardedLifeAttempt();
        int token = activeRewardedAttemptToken;
        OnExtraLifeAdFlowEnded(token);
    }
#endif
}

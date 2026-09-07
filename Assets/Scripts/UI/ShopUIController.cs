using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shop UI gedrag: open/close, tabs, coin display.
/// Visuele hierarchy staat serialized in de scene (MainMenu of Gameplay) — hier geen UI-bouw.
/// </summary>
public class ShopUIController : MonoBehaviour
{
    private static readonly Color TabInactive = new Color(0.18f, 0.24f, 0.34f, 1f);
    private static readonly Color TabActive = new Color(0.20f, 0.55f, 0.85f, 1f);

    [Header("Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject coinsPanel;
    [SerializeField] private GameObject skinsPanel;

    [Header("Buttons")]
    [SerializeField] private Button shopButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button coinsTabButton;
    [SerializeField] private Button skinsTabButton;

    /// <summary>Bestaande Shop-knop (spotlight voor Skins feature tutorial).</summary>
    public RectTransform ShopButtonRect =>
        shopButton != null ? shopButton.transform as RectTransform : null;

    /// <summary>Shop coin-balance tekst (optionele secondary spotlight).</summary>
    public RectTransform CoinBalanceRect =>
        coinBalanceText != null ? coinBalanceText.rectTransform : null;

    public bool IsShopOpen => shopPanel != null && shopPanel.activeSelf;

    /// <summary>Fired nadat shopPanel zichtbaar is gezet.</summary>
    public event Action OnShopOpened;

    /// <summary>Fired nadat shopPanel gesloten is.</summary>
    public event Action OnShopClosed;

    [Header("Tab visuals")]
    [SerializeField] private Image coinsTabImage;
    [SerializeField] private Image skinsTabImage;

    [Header("Coin display")]
    [SerializeField] private TextMeshProUGUI coinBalanceText;
    [SerializeField] private CoinManager coinManager;

    [Header("Skins (optioneel)")]
    [SerializeField] private SkinManager skinManager;

    [Header("Free coins (optioneel)")]
    [SerializeField] private FreeCoinsRewardedUI freeCoinsRewarded;

    [Header("Audio (optioneel)")]
    [SerializeField] private AudioManager audioManager;

    [Header("Gameplay pause (optioneel)")]
    [Tooltip("When true, uses Time.timeScale=0 while shop is open (same approach as PauseManager).")]
    [SerializeField] private bool pauseGameplayWhileOpen;

    [Tooltip("Optional. Used so CloseShop does not unpause an open Pause menu.")]
    [SerializeField] private PauseManager pauseManager;

    /// <summary>True while this shop instance owns a timeScale pause it applied.</summary>
    private bool ownsTimeScalePause;

    private void Awake()
    {
        if (skinManager == null)
        {
            skinManager = FindAnyObjectByType<SkinManager>();
        }

        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }

        if (pauseManager == null)
        {
            pauseManager = FindAnyObjectByType<PauseManager>();
        }

        WireButtons();

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        ownsTimeScalePause = false;
    }

    private void Start()
    {
        RefreshCoinBalance();
    }

    private void OnEnable()
    {
        if (coinManager != null)
        {
            coinManager.OnCoinsChanged += UpdateCoinDisplay;
            UpdateCoinDisplay(coinManager.GetCoins());
        }
    }

    private void OnDisable()
    {
        if (coinManager != null)
        {
            coinManager.OnCoinsChanged -= UpdateCoinDisplay;
        }

        // Scene unload / disable: release owned pause without fighting PauseManager.
        ReleaseOwnedPauseIfSafe();
    }

    public void OpenShop()
    {
        if (shopPanel == null)
        {
            return;
        }

        if (shopPanel.activeSelf)
        {
            return;
        }

        shopPanel.SetActive(true);
        ApplyGameplayPauseForShop();
        audioManager?.PlayPanelOpen();
        ShowCoinsTab();
        RefreshCoinBalance();
        freeCoinsRewarded?.Refresh();
        OnShopOpened?.Invoke();
    }

    public void CloseShop()
    {
        if (shopPanel == null || !shopPanel.activeSelf)
        {
            return;
        }

        shopPanel.SetActive(false);
        ReleaseOwnedPauseIfSafe();
        audioManager?.PlayPanelClose();
        OnShopClosed?.Invoke();
    }

    public void ShowCoinsTab()
    {
        if (coinsPanel != null)
        {
            coinsPanel.SetActive(true);
        }

        if (skinsPanel != null)
        {
            skinsPanel.SetActive(false);
        }

        SetTabVisual(coinsActive: true);
        freeCoinsRewarded?.Refresh();
        RefreshCoinBalance();
    }

    public void ShowSkinsTab()
    {
        if (coinsPanel != null)
        {
            coinsPanel.SetActive(false);
        }

        if (skinsPanel != null)
        {
            skinsPanel.SetActive(true);
        }

        SetTabVisual(coinsActive: false);
        RefreshSkinCards();
    }

    /// <summary>
    /// Zoekt een unowned skin-card voor tutorial spotlight.
    /// Prefer Buy (affordable), anders unowned Unavailable. Null als geen candidate.
    /// </summary>
    public RectTransform FindBuyableSkinSpotlightTarget()
    {
        if (shopPanel == null)
        {
            return null;
        }

        SkinShopCardUI[] cards = shopPanel.GetComponentsInChildren<SkinShopCardUI>(true);

        RectTransform affordable = null;
        RectTransform anyUnowned = null;

        for (int i = 0; i < cards.Length; i++)
        {
            SkinShopCardUI card = cards[i];
            if (card == null || !card.isActiveAndEnabled)
            {
                continue;
            }

            if (!card.IsUnowned)
            {
                continue;
            }

            RectTransform rect = card.SpotlightRect;
            if (rect == null)
            {
                continue;
            }

            if (card.IsBuyableAffordable && affordable == null)
            {
                affordable = rect;
            }

            if (anyUnowned == null)
            {
                anyUnowned = rect;
            }
        }

        return affordable != null ? affordable : anyUnowned;
    }

    /// <summary>
    /// Helper voor Inspector OnClick: koop skin via SkinManager.
    /// </summary>
    public void OnSkinBuyClicked(string skinId)
    {
        if (skinManager == null)
        {
            skinManager = FindAnyObjectByType<SkinManager>();
        }

        if (skinManager == null || string.IsNullOrWhiteSpace(skinId))
        {
            return;
        }

        skinManager.TryPurchaseSkin(skinId);
        RefreshSkinCards();
        RefreshCoinBalance();
    }

    /// <summary>
    /// Helper voor Inspector OnClick: selecteer owned skin.
    /// </summary>
    public void OnSkinSelectClicked(string skinId)
    {
        if (skinManager == null)
        {
            skinManager = FindAnyObjectByType<SkinManager>();
        }

        if (skinManager == null || string.IsNullOrWhiteSpace(skinId))
        {
            return;
        }

        skinManager.SelectSkin(skinId);
        RefreshSkinCards();
    }

    private void RefreshSkinCards()
    {
        // Scope to this shop hierarchy so MainMenu/Gameplay never cross-refresh.
        if (shopPanel == null)
        {
            return;
        }

        SkinShopCardUI[] cards = shopPanel.GetComponentsInChildren<SkinShopCardUI>(true);

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null)
            {
                cards[i].Refresh();
            }
        }
    }

    private void WireButtons()
    {
        if (shopButton != null)
        {
            shopButton.onClick.RemoveListener(OpenShop);
            shopButton.onClick.AddListener(OpenShop);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseShop);
            closeButton.onClick.AddListener(CloseShop);
        }

        if (coinsTabButton != null)
        {
            coinsTabButton.onClick.RemoveListener(ShowCoinsTab);
            coinsTabButton.onClick.AddListener(ShowCoinsTab);
        }

        if (skinsTabButton != null)
        {
            skinsTabButton.onClick.RemoveListener(ShowSkinsTab);
            skinsTabButton.onClick.AddListener(ShowSkinsTab);
        }
    }

    private void SetTabVisual(bool coinsActive)
    {
        if (coinsTabImage != null)
        {
            coinsTabImage.color = coinsActive ? TabActive : TabInactive;
        }

        if (skinsTabImage != null)
        {
            skinsTabImage.color = coinsActive ? TabInactive : TabActive;
        }
    }

    private void RefreshCoinBalance()
    {
        if (coinManager != null)
        {
            UpdateCoinDisplay(coinManager.GetCoins());
        }
    }

    private void UpdateCoinDisplay(int amount)
    {
        if (coinBalanceText != null)
        {
            coinBalanceText.text = amount.ToString("N0");
        }
    }

    private void ApplyGameplayPauseForShop()
    {
        if (!pauseGameplayWhileOpen)
        {
            ownsTimeScalePause = false;
            return;
        }

        // If pause menu (or anything else) already froze time, do not claim ownership.
        if (Time.timeScale <= 0f)
        {
            ownsTimeScalePause = false;
            return;
        }

        Time.timeScale = 0f;
        ownsTimeScalePause = true;
    }

    private void ReleaseOwnedPauseIfSafe()
    {
        if (!ownsTimeScalePause)
        {
            return;
        }

        ownsTimeScalePause = false;

        if (pauseManager == null)
        {
            pauseManager = FindAnyObjectByType<PauseManager>();
        }

        // Keep paused if the pause menu is still open.
        if (pauseManager != null && pauseManager.IsPauseMenuOpen)
        {
            return;
        }

        Time.timeScale = 1f;
    }
}

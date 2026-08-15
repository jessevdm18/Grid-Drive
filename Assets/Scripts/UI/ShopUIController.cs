using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shop UI gedrag: open/close, tabs, coin display.
/// Visuele hierarchy staat serialized in MainMenu.unity — hier geen UI-bouw.
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

    [Header("Tab visuals")]
    [SerializeField] private Image coinsTabImage;
    [SerializeField] private Image skinsTabImage;

    [Header("Coin display")]
    [SerializeField] private TextMeshProUGUI coinBalanceText;
    [SerializeField] private CoinManager coinManager;

    [Header("Skins (optioneel)")]
    [SerializeField] private SkinManager skinManager;

    private void Awake()
    {
        if (skinManager == null)
        {
            skinManager = FindFirstObjectByType<SkinManager>();
        }

        WireButtons();

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
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
    }

    public void OpenShop()
    {
        if (shopPanel == null)
        {
            return;
        }

        shopPanel.SetActive(true);
        ShowCoinsTab();
        RefreshCoinBalance();
    }

    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
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
    /// Helper voor Inspector OnClick: koop skin via SkinManager.
    /// </summary>
    public void OnSkinBuyClicked(string skinId)
    {
        if (skinManager == null)
        {
            skinManager = FindFirstObjectByType<SkinManager>();
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
            skinManager = FindFirstObjectByType<SkinManager>();
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
        SkinShopCardUI[] cards = FindObjectsByType<SkinShopCardUI>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

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
}

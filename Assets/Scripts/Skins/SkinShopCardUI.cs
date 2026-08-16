using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Zet dit op een bestaande skin-card GameObject. Maak zelf de UI; koppel refs in Inspector.
/// </summary>
public class SkinShopCardUI : MonoBehaviour
{
    public enum CardState
    {
        Selected,
        Select,
        Buy,
        Unavailable
    }

    [Header("Skin")]
    [SerializeField] private VehicleSkinData skinData;

    [Header("Optional overrides")]
    [SerializeField] private SkinManager skinManager;
    [SerializeField] private CoinManager coinManager;

    [Header("UI refs (koppel zelf)")]
    [SerializeField] private Image previewImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionLabelText;
    [SerializeField] private GameObject selectedIndicator;
    [SerializeField] private GameObject ownedIndicator;

    private CardState currentState = CardState.Unavailable;

    private void Awake()
    {
        ResolveManagers();

        if (actionButton != null)
        {
            actionButton.onClick.RemoveListener(OnActionClicked);
            actionButton.onClick.AddListener(OnActionClicked);
        }
    }

    private void OnEnable()
    {
        ResolveManagers();

        if (skinManager != null)
        {
            skinManager.OnSkinsChanged += Refresh;
            skinManager.OnSkinPurchased += OnSkinEvent;
            skinManager.OnSkinSelected += OnSkinEvent;
        }

        if (coinManager != null)
        {
            coinManager.OnCoinsChanged += OnCoinsChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (skinManager != null)
        {
            skinManager.OnSkinsChanged -= Refresh;
            skinManager.OnSkinPurchased -= OnSkinEvent;
            skinManager.OnSkinSelected -= OnSkinEvent;
        }

        if (coinManager != null)
        {
            coinManager.OnCoinsChanged -= OnCoinsChanged;
        }
    }

    public void SetSkinData(VehicleSkinData data)
    {
        skinData = data;
        Refresh();
    }

    public void Refresh()
    {
        ResolveManagers();

        if (skinData == null || !skinData.HasValidId)
        {
            SetOwnedIndicator(false);
            ApplyVisuals(CardState.Unavailable);
            return;
        }

        if (nameText != null)
        {
            nameText.text = string.IsNullOrEmpty(skinData.DisplayName)
                ? skinData.SkinId
                : skinData.DisplayName;
        }

        if (previewImage != null && skinData.PreviewSprite != null)
        {
            previewImage.sprite = skinData.PreviewSprite;
            previewImage.enabled = true;
        }

        if (skinManager == null)
        {
            SetOwnedIndicator(false);
            ApplyVisuals(CardState.Unavailable);
            return;
        }

        bool owned = skinManager.IsOwned(skinData.SkinId);
        SetOwnedIndicator(owned);

        bool selected = owned &&
            skinManager.SelectedSkinId == skinData.SkinId;

        if (selected)
        {
            ApplyVisuals(CardState.Selected);
            return;
        }

        if (owned)
        {
            ApplyVisuals(CardState.Select);
            return;
        }

        bool canAfford = skinManager.CanAfford(skinData.SkinId);
        ApplyVisuals(canAfford ? CardState.Buy : CardState.Unavailable);
    }

    /// <summary>
    /// Koppel dit aan de action Button OnClick, of laat Awake het doen.
    /// </summary>
    public void OnActionClicked()
    {
        if (skinData == null || skinManager == null)
        {
            return;
        }

        switch (currentState)
        {
            case CardState.Buy:
                skinManager.TryPurchaseSkin(skinData.SkinId);
                break;
            case CardState.Select:
                skinManager.SelectSkin(skinData.SkinId);
                break;
            case CardState.Selected:
            case CardState.Unavailable:
            default:
                break;
        }

        Refresh();
    }

    private void ApplyVisuals(CardState state)
    {
        currentState = state;

        if (priceText != null)
        {
            bool showPrice = skinData != null &&
                (state == CardState.Buy || state == CardState.Unavailable);

            priceText.gameObject.SetActive(showPrice);
            if (showPrice)
            {
                priceText.text = skinData.CoinPrice <= 0
                    ? "FREE"
                    : skinData.CoinPrice.ToString("N0");
            }
        }

        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(state == CardState.Selected);
        }

        if (actionLabelText != null)
        {
            switch (state)
            {
                case CardState.Selected:
                    actionLabelText.text = "SELECTED";
                    break;
                case CardState.Select:
                    actionLabelText.text = "SELECT";
                    break;
                case CardState.Buy:
                case CardState.Unavailable:
                    actionLabelText.text = "BUY";
                    break;
            }
        }

        if (actionButton != null)
        {
            actionButton.interactable =
                state == CardState.Buy || state == CardState.Select;
        }
    }

    private void SetOwnedIndicator(bool owned)
    {
        if (ownedIndicator != null)
        {
            ownedIndicator.SetActive(owned);
        }
    }

    private void ResolveManagers()
    {
        if (skinManager == null)
        {
            skinManager = FindAnyObjectByType<SkinManager>();
        }

        if (coinManager == null)
        {
            coinManager = FindAnyObjectByType<CoinManager>();
        }
    }

    private void OnSkinEvent(VehicleSkinData _)
    {
        Refresh();
    }

    private void OnCoinsChanged(int _)
    {
        Refresh();
    }
}

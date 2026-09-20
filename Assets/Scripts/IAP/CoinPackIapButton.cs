using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Thin binder for a Shop coin-pack CTA/Label → IapService.
/// Identifies only the Google Play product ID; reward amount comes from IapService mapping.
/// </summary>
public class CoinPackIapButton : MonoBehaviour
{
    private const string PlaceholderPrice = "...";

    [SerializeField] private Button ctaButton;
    [SerializeField] private TextMeshProUGUI priceLabel;
    [SerializeField] private string productId = IapService.ProductIdCoins1000;

    private void Awake()
    {
        ResolveRefs();

        if (ctaButton != null)
        {
            ctaButton.onClick.RemoveListener(OnBuyClicked);
            ctaButton.onClick.AddListener(OnBuyClicked);
        }

        ApplyUi();
    }

    private void OnEnable()
    {
        IapService service = IapService.EnsureInstance();
        service.OnStateChanged += ApplyUi;
        ApplyUi();
    }

    private void OnDisable()
    {
        if (IapService.Instance != null)
        {
            IapService.Instance.OnStateChanged -= ApplyUi;
        }
    }

    private void OnDestroy()
    {
        if (ctaButton != null)
        {
            ctaButton.onClick.RemoveListener(OnBuyClicked);
        }
    }

    private void OnBuyClicked()
    {
        if (!IapService.TryGetCoinRewardForProduct(productId, out _))
        {
            return;
        }

        IapService.EnsureInstance().PurchaseProduct(productId);
    }

    private void ApplyUi()
    {
        ResolveRefs();

        IapService service = IapService.Instance != null ? IapService.Instance : IapService.EnsureInstance();
        bool ready = service.IsProductReady(productId);
        bool purchasing = service.IsPurchaseInProgress;
        string localizedPrice = service.GetLocalizedPriceString(productId);

        if (priceLabel != null)
        {
            if (ready && !string.IsNullOrEmpty(localizedPrice))
            {
                priceLabel.text = localizedPrice;
            }
            else
            {
                priceLabel.text = PlaceholderPrice;
            }
        }

        if (ctaButton != null)
        {
            ctaButton.interactable = ready && !purchasing;
        }
    }

    private void ResolveRefs()
    {
        if (ctaButton == null)
        {
            Transform cta = transform.Find("Cta");
            if (cta != null)
            {
                ctaButton = cta.GetComponent<Button>();
            }
        }

        if (priceLabel == null)
        {
            Transform label = transform.Find("Cta/Label");
            if (label != null)
            {
                priceLabel = label.GetComponent<TextMeshProUGUI>();
            }
        }
    }
}

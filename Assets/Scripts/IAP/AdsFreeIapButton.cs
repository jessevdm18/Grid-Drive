using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shop binder for the Ads Free non-consumable. Price text comes from the store.
/// </summary>
public class AdsFreeIapButton : MonoBehaviour
{
    private const string PlaceholderPrice = "...";
    private const string OwnedLabel = "OWNED";

    [SerializeField] private Button ctaButton;
    [SerializeField] private TextMeshProUGUI priceLabel;

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
        AdsFreeService.OnChanged += ApplyUi;
        ApplyUi();
    }

    private void OnDisable()
    {
        if (IapService.Instance != null)
        {
            IapService.Instance.OnStateChanged -= ApplyUi;
        }

        AdsFreeService.OnChanged -= ApplyUi;
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
        if (AdsFreeService.IsOwned)
        {
            return;
        }

        IapService.EnsureInstance().PurchaseProduct(IapService.ProductIdAdsFree);
    }

    private void ApplyUi()
    {
        ResolveRefs();

        IapService service = IapService.Instance != null ? IapService.Instance : IapService.EnsureInstance();
        bool owned = AdsFreeService.IsOwned;
        bool ready = service.IsProductReady(IapService.ProductIdAdsFree);
        string localizedPrice = service.GetLocalizedPriceString(IapService.ProductIdAdsFree);

        if (priceLabel != null)
        {
            if (owned)
            {
                priceLabel.text = OwnedLabel;
            }
            else if (ready && !string.IsNullOrEmpty(localizedPrice))
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
            ctaButton.interactable = !owned && ready && !service.IsPurchaseInProgress;
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

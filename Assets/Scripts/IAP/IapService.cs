using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using UnityEngine.SceneManagement;

/// <summary>
/// Shared Unity IAP 5.4.3 StoreController host (DDOL).
/// Consumable coin packs → CoinManager.TryAddCoinsForIapTransaction via authoritative product mapping.
/// Android player builds validate Google Play receipts locally before new grants.
/// </summary>
public class IapService : MonoBehaviour
{
    public const string ProductIdCoins1000 = "coins_1000";
    public const string ProductIdCoins5000 = "coins_5000";
    public const string ProductIdCoins12000 = "coins_12000";

    public const int Coins1000GrantAmount = 1000;
    public const int Coins5000GrantAmount = 5000;
    public const int Coins12000GrantAmount = 12000;

    private static readonly string[] AllCoinPackProductIds =
    {
        ProductIdCoins1000,
        ProductIdCoins5000,
        ProductIdCoins12000
    };

    public static IapService Instance { get; private set; }

    public bool IsStoreConnected { get; private set; }

    /// <summary>True when the store is connected and at least one coin pack has a usable price.</summary>
    public bool IsProductAvailable { get; private set; }

    public bool IsPurchaseInProgress { get; private set; }

    /// <summary>Localized price for coins_1000 when available (legacy convenience).</summary>
    public string LocalizedPriceString { get; private set; } = string.Empty;

    /// <summary>Store connected and at least one coin pack is purchasable.</summary>
    public bool IsReady => IsStoreConnected && IsProductAvailable;

    public event Action OnStateChanged;
    public event Action OnStoreReady;
    public event Action OnProductReady;
    public event Action<string> OnPurchaseFailedMessage;
    public event Action OnPurchaseDeferred;
    public event Action<string, int> OnPurchaseFulfilled;

    private StoreController storeController;
    private bool initStarted;
    private bool eventsSubscribed;
    private readonly Dictionary<string, string> localizedPriceByProductId =
        new Dictionary<string, string>(StringComparer.Ordinal);
    private readonly HashSet<string> readyProductIds = new HashSet<string>(StringComparer.Ordinal);

#if UNITY_ANDROID && !UNITY_EDITOR
    private CrossPlatformValidator googlePlayReceiptValidator;
#endif

    /// <summary>Authoritative product ID → coin grant. UI must not supply amounts.</summary>
    public static bool TryGetCoinRewardForProduct(string productId, out int coinAmount)
    {
        switch (productId)
        {
            case ProductIdCoins1000:
                coinAmount = Coins1000GrantAmount;
                return true;
            case ProductIdCoins5000:
                coinAmount = Coins5000GrantAmount;
                return true;
            case ProductIdCoins12000:
                coinAmount = Coins12000GrantAmount;
                return true;
            default:
                coinAmount = 0;
                return false;
        }
    }

    public bool IsProductReady(string productId)
    {
        return IsStoreConnected
               && !string.IsNullOrEmpty(productId)
               && readyProductIds.Contains(productId);
    }

    public string GetLocalizedPriceString(string productId)
    {
        if (string.IsNullOrEmpty(productId))
        {
            return string.Empty;
        }

        return localizedPriceByProductId.TryGetValue(productId, out string price) ? price : string.Empty;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapAfterSceneLoad()
    {
        EnsureInstance();
    }

    public static IapService EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        IapService existing = FindAnyObjectByType<IapService>();
        if (existing != null)
        {
            Instance = existing;
            return Instance;
        }

        GameObject go = new GameObject("IapService");
        go.AddComponent<IapService>();
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartInitialize();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        UnsubscribeStoreEvents();
    }

    /// <summary>Purchase a known coin pack by Google Play product ID.</summary>
    public void PurchaseProduct(string productId)
    {
        if (IsPurchaseInProgress)
        {
            return;
        }

        if (!TryGetCoinRewardForProduct(productId, out _))
        {
            OnPurchaseFailedMessage?.Invoke("Unknown store product.");
            NotifyStateChanged();
            return;
        }

        if (storeController == null || !IsProductReady(productId))
        {
            OnPurchaseFailedMessage?.Invoke("Store product is not ready.");
            NotifyStateChanged();
            return;
        }

        Product product = storeController.GetProductById(productId);
        if (product == null)
        {
            OnPurchaseFailedMessage?.Invoke(productId + " is unavailable.");
            NotifyStateChanged();
            return;
        }

        IsPurchaseInProgress = true;
        NotifyStateChanged();
        storeController.PurchaseProduct(product);
    }

    /// <summary>Backward-compatible entry for coins_1000.</summary>
    public void PurchaseCoins1000()
    {
        PurchaseProduct(ProductIdCoins1000);
    }

    private void StartInitialize()
    {
        if (initStarted)
        {
            return;
        }

        initStarted = true;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            storeController = UnityIAPServices.StoreController();
            SubscribeStoreEvents();
            await storeController.Connect();
        }
        catch (Exception ex)
        {
            LogInitializeExceptionChain(ex);
            IsStoreConnected = false;
            ClearProductMetadata();
            IsPurchaseInProgress = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Logs type/message/stack for the full InnerException chain (no receipts/tokens/keys).
    /// </summary>
    private static void LogInitializeExceptionChain(Exception ex)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[IapService] Initialize failed");

        Exception current = ex;
        int depth = 0;
        while (current != null)
        {
            sb.AppendLine();
            sb.Append("Exception[").Append(depth).AppendLine("]");
            sb.Append("Type: ").AppendLine(current.GetType().FullName ?? current.GetType().Name);
            sb.Append("Message: ").AppendLine(current.Message ?? string.Empty);
            sb.Append("StackTrace: ").AppendLine(
                string.IsNullOrEmpty(current.StackTrace) ? "(none)" : current.StackTrace);

            current = current.InnerException;
            depth++;
        }

        Debug.LogError(sb.ToString());
    }

    private void SubscribeStoreEvents()
    {
        if (storeController == null || eventsSubscribed)
        {
            return;
        }

        storeController.OnStoreConnected += OnStoreConnected;
        storeController.OnStoreDisconnected += OnStoreDisconnected;
        storeController.OnAuthAccountChanged += OnAuthAccountChanged;

        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnProductsFetchFailed += OnProductsFetchFailed;

        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;

        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
        storeController.OnPurchaseDeferred += OnPurchaseDeferredInternal;

        eventsSubscribed = true;
    }

    private void UnsubscribeStoreEvents()
    {
        if (storeController == null || !eventsSubscribed)
        {
            return;
        }

        storeController.OnStoreConnected -= OnStoreConnected;
        storeController.OnStoreDisconnected -= OnStoreDisconnected;
        storeController.OnAuthAccountChanged -= OnAuthAccountChanged;

        storeController.OnProductsFetched -= OnProductsFetched;
        storeController.OnProductsFetchFailed -= OnProductsFetchFailed;

        storeController.OnPurchasesFetched -= OnPurchasesFetched;
        storeController.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;

        storeController.OnPurchasePending -= OnPurchasePending;
        storeController.OnPurchaseConfirmed -= OnPurchaseConfirmed;
        storeController.OnPurchaseFailed -= OnPurchaseFailed;
        storeController.OnPurchaseDeferred -= OnPurchaseDeferredInternal;

        eventsSubscribed = false;
    }

    private void OnStoreConnected()
    {
        IsStoreConnected = true;
        NotifyStateChanged();
        OnStoreReady?.Invoke();
        FetchCoinPackProducts();
        storeController.FetchPurchases();
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription failure)
    {
        IsStoreConnected = false;
        ClearProductMetadata();
        IsPurchaseInProgress = false;
        Debug.LogWarning("[IapService] Store disconnected: " + failure.Message);
        NotifyStateChanged();
    }

    private void OnAuthAccountChanged()
    {
        // v5.4 clears product/purchase caches before this event.
        ClearProductMetadata();
        IsPurchaseInProgress = false;
        NotifyStateChanged();

        if (storeController == null)
        {
            return;
        }

        FetchCoinPackProducts();
        storeController.FetchPurchases();
    }

    private void FetchCoinPackProducts()
    {
        if (storeController == null)
        {
            return;
        }

        var defs = new List<ProductDefinition>(AllCoinPackProductIds.Length);
        for (int i = 0; i < AllCoinPackProductIds.Length; i++)
        {
            defs.Add(new ProductDefinition(AllCoinPackProductIds[i], ProductType.Consumable));
        }

        storeController.FetchProducts(defs);
    }

    private void OnProductsFetched(List<Product> products)
    {
        RefreshCoinPackMetadata();
        NotifyStateChanged();
        if (IsProductAvailable)
        {
            OnProductReady?.Invoke();
        }
    }

    private void OnProductsFetchFailed(ProductFetchFailed failure)
    {
        ClearProductMetadata();
        Debug.LogWarning("[IapService] Product fetch failed: " + failure.FailureReason);
        NotifyStateChanged();
    }

    private void ClearProductMetadata()
    {
        localizedPriceByProductId.Clear();
        readyProductIds.Clear();
        IsProductAvailable = false;
        LocalizedPriceString = string.Empty;
    }

    private void RefreshCoinPackMetadata()
    {
        ClearProductMetadata();

        if (storeController == null)
        {
            return;
        }

        for (int i = 0; i < AllCoinPackProductIds.Length; i++)
        {
            string productId = AllCoinPackProductIds[i];
            Product product = storeController.GetProductById(productId);
            if (product == null || product.metadata == null)
            {
                continue;
            }

            string price = product.metadata.localizedPriceString;
            if (string.IsNullOrEmpty(price))
            {
                continue;
            }

            localizedPriceByProductId[productId] = price;
            if (product.availableToPurchase)
            {
                readyProductIds.Add(productId);
            }
        }

        IsProductAvailable = readyProductIds.Count > 0;
        LocalizedPriceString = GetLocalizedPriceString(ProductIdCoins1000);
    }

    private void OnPurchasesFetched(Orders orders)
    {
        // Pending orders are processed via OnPurchasePending when enabled (default).
    }

    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
    {
        Debug.LogWarning("[IapService] Purchases fetch failed: " + failure.Message);
    }

    private void OnPurchasePending(PendingOrder order)
    {
        Product product = GetFirstProduct(order);
        string productId = product != null && product.definition != null ? product.definition.id : null;

        if (!TryGetCoinRewardForProduct(productId, out int coinAmount))
        {
            Debug.LogWarning("[IapService] Ignoring pending order for unexpected product: " + productId);
            // Do not ConfirmPurchase unknown products.
            IsPurchaseInProgress = false;
            NotifyStateChanged();
            return;
        }

        string transactionId = GetStableTransactionId(order);
        if (string.IsNullOrEmpty(transactionId))
        {
            Debug.LogError("[IapService] Pending " + productId + " order has no TransactionID/PurchaseToken. Leaving unconfirmed.");
            IsPurchaseInProgress = false;
            OnPurchaseFailedMessage?.Invoke("Purchase could not be verified.");
            NotifyStateChanged();
            return;
        }

        CoinManager coinManager = ResolveActiveCoinManager();
        if (coinManager == null)
        {
            Debug.LogError("[IapService] No active CoinManager; leaving purchase unconfirmed for redelivery.");
            IsPurchaseInProgress = false;
            OnPurchaseFailedMessage?.Invoke("Coin wallet unavailable. Please reopen the shop.");
            NotifyStateChanged();
            return;
        }

        // Already durably fulfilled (e.g. crash after grant+save, before ConfirmPurchase).
        if (coinManager.HasFulfilledIapTransaction(transactionId))
        {
            storeController.ConfirmPurchase(order);
            return;
        }

        if (!TryValidateGooglePlayReceiptForNewGrant(order))
        {
            IsPurchaseInProgress = false;
            OnPurchaseFailedMessage?.Invoke("Purchase could not be verified.");
            NotifyStateChanged();
            return;
        }

        // Atomic: RushOut_Coins + fulfilled tx id in one PlayerPrefs.Save().
        bool newlyGranted = coinManager.TryAddCoinsForIapTransaction(coinAmount, transactionId);
        if (newlyGranted)
        {
            OnPurchaseFulfilled?.Invoke(productId, coinAmount);
            AudioManager.TryPlayRewardReceived();
        }

        // Confirm/consume only after durable fulfillment (or prior fulfillment).
        storeController.ConfirmPurchase(order);
    }

    /// <summary>
    /// Android player: require a valid Google Play receipt before a new grant.
    /// Editor / non-Android: skip local validation (Security stub cannot run).
    /// </summary>
    private bool TryValidateGooglePlayReceiptForNewGrant(PendingOrder order)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string receipt = order != null && order.Info != null ? order.Info.Receipt : null;
        if (string.IsNullOrEmpty(receipt))
        {
            Debug.LogWarning("[IAP] Google Play receipt validation failed.");
            return false;
        }

        try
        {
            CrossPlatformValidator validator = GetOrCreateGooglePlayReceiptValidator();
            if (validator == null)
            {
                Debug.LogWarning("[IAP] Google Play receipt validation failed.");
                return false;
            }

            validator.Validate(receipt);
            return true;
        }
        catch (IAPSecurityException)
        {
            Debug.LogWarning("[IAP] Google Play receipt validation failed.");
            return false;
        }
        catch (Exception)
        {
            Debug.LogWarning("[IAP] Google Play receipt validation failed.");
            return false;
        }
#else
        return true;
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private CrossPlatformValidator GetOrCreateGooglePlayReceiptValidator()
    {
        if (googlePlayReceiptValidator != null)
        {
            return googlePlayReceiptValidator;
        }

        try
        {
            byte[] googlePublicKey = GooglePlayTangle.Data();
            if (googlePublicKey == null || googlePublicKey.Length == 0)
            {
                return null;
            }

            googlePlayReceiptValidator = new CrossPlatformValidator(googlePublicKey, Application.identifier);
            return googlePlayReceiptValidator;
        }
        catch (Exception)
        {
            Debug.LogWarning("[IAP] Google Play receipt validation failed.");
            return null;
        }
    }
#endif

    private void OnPurchaseConfirmed(Order order)
    {
        IsPurchaseInProgress = false;

        switch (order)
        {
            case ConfirmedOrder:
                break;
            case FailedOrder failed:
                Debug.LogWarning(
                    "[IapService] Confirm failed: " + failed.FailureReason + " — " + failed.Details);
                // Coins remain granted if already persisted; redelivery will Confirm without re-grant.
                OnPurchaseFailedMessage?.Invoke("Purchase confirmation failed. Your coins are safe if already granted.");
                break;
            default:
                break;
        }

        NotifyStateChanged();
    }

    private void OnPurchaseFailed(FailedOrder order)
    {
        IsPurchaseInProgress = false;
        string reason = order != null ? order.FailureReason.ToString() : "Unknown";
        string details = order != null ? order.Details : string.Empty;
        Debug.LogWarning("[IapService] Purchase failed: " + reason + " — " + details);
        OnPurchaseFailedMessage?.Invoke("Purchase cancelled or failed.");
        NotifyStateChanged();
    }

    private void OnPurchaseDeferredInternal(DeferredOrder order)
    {
        IsPurchaseInProgress = false;
        OnPurchaseDeferred?.Invoke();
        NotifyStateChanged();
    }

    private static string GetStableTransactionId(PendingOrder order)
    {
        if (order == null || order.Info == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(order.Info.TransactionID))
        {
            return order.Info.TransactionID;
        }

        if (order.Info.Google != null && !string.IsNullOrEmpty(order.Info.Google.PurchaseToken))
        {
            return order.Info.Google.PurchaseToken;
        }

        return null;
    }

    private static Product GetFirstProduct(Order order)
    {
        if (order == null || order.CartOrdered == null)
        {
            return null;
        }

        CartItem item = order.CartOrdered.Items()?.FirstOrDefault();
        return item != null ? item.Product : null;
    }

    private static CoinManager ResolveActiveCoinManager()
    {
        CoinManager[] managers = FindObjectsByType<CoinManager>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (managers == null || managers.Length == 0)
        {
            return null;
        }

        Scene active = SceneManager.GetActiveScene();
        for (int i = 0; i < managers.Length; i++)
        {
            CoinManager manager = managers[i];
            if (manager != null && manager.gameObject.scene == active)
            {
                return manager;
            }
        }

        return managers[0];
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}

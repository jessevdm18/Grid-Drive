using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

/// <summary>
/// Manages Rewarded and Interstitial Ads via Google Mobile Ads.
/// Editor / DEVELOPMENT_BUILD → Google official test ad units.
/// Android Release → Grid Drive production ad units.
/// iOS production ad units are not wired yet (keeps Google test IDs).
/// Initializes only after PrivacyConsentManager resolves and CanRequestAds is true.
/// Google Mobile Ads callbacks are marshalled onto the Unity main thread before
/// touching PlayerPrefs / scene / gameplay code (prevents GetInt off-thread crashes).
/// </summary>
public class AdsManager : MonoBehaviour
{
    // Official Google test ad units (safe for Editor + Development builds).
    private const string GoogleTestRewardedAndroid =
        "ca-app-pub-3940256099942544/5224354917";
    private const string GoogleTestInterstitialAndroid =
        "ca-app-pub-3940256099942544/1033173712";
    private const string GoogleTestRewardedIos =
        "ca-app-pub-3940256099942544/1712485313";
    private const string GoogleTestInterstitialIos =
        "ca-app-pub-3940256099942544/4411468910";

    // Grid Drive v1 Android production ad units (Release builds only).
    private const string ProductionRewardedHintAndroid =
        "ca-app-pub-8657245895551337/4707151288";
    private const string ProductionRewardedFreeCoinsAndroid =
        "ca-app-pub-8657245895551337/7369665574";
    private const string ProductionInterstitialAndroid =
        "ca-app-pub-8657245895551337/6318731373";

    /// <summary>Gameplay FREE HINT placement id (also analytics).</summary>
    public const string PlacementHint = "hint";

    /// <summary>Shop FREE COINS placement id (also analytics).</summary>
    public const string PlacementShopFreeCoins = "shop_free_coins";

    /// <summary>Out-of-lives +1 life placement id (also analytics). Shares hint ad unit/slot.</summary>
    public const string PlacementExtraLife = "extra_life";

    private RewardedAd rewardedHintAd;
    private RewardedAd rewardedFreeCoinsAd;
    private InterstitialAd interstitialAd;

    // Callback after interstitial close/fail — at most once.
    private Action interstitialClosedCallback;
    private bool interstitialCallbackInvoked;

    /// <summary>Optional one-shot after rewarded fullscreen closes/fails (shop UI unlock).</summary>
    private Action pendingRewardedFlowEnded;
    private bool rewardedFlowEndedInvoked;

    /// <summary>True while a rewarded Show is in flight (blocks overlapping shows).</summary>
    private bool rewardedShowInProgress;

    private bool adsInitialized;
    private bool subscribedToConsent;

    private readonly Queue<Action> mainThreadQueue = new Queue<Action>(8);
    private readonly object mainThreadQueueLock = new object();
    private int mainThreadId = -1;

    private void Awake()
    {
        mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        LogAdsConfigOnce();
    }

    /// <summary>
    /// Central rewarded ad unit selection for a placement.
    /// Editor / Development → Google test (same test unit for all rewarded placements).
    /// Android Release → shop_free_coins unit vs hint/extra_life unit.
    /// </summary>
    public static string GetRewardedAdUnitId(string placement = PlacementHint)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_IOS
        return GoogleTestRewardedIos;
#else
        return GoogleTestRewardedAndroid;
#endif
#elif UNITY_ANDROID
        if (IsShopFreeCoinsPlacement(placement))
        {
            return ProductionRewardedFreeCoinsAndroid;
        }

        // hint + extra_life share the existing production rewarded hint unit.
        return ProductionRewardedHintAndroid;
#elif UNITY_IOS
        // iOS production IDs not configured yet — keep Google test / no live fill.
        return GoogleTestRewardedIos;
#else
        return "unused";
#endif
    }

    private static bool IsShopFreeCoinsPlacement(string placement)
    {
        return string.Equals(
            placement,
            PlacementShopFreeCoins,
            StringComparison.Ordinal);
    }

    private static bool IsExtraLifePlacement(string placement)
    {
        return string.Equals(
            placement,
            PlacementExtraLife,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Central interstitial ad unit selection.
    /// Editor / Development → Google test. Android Release → production.
    /// </summary>
    public static string GetInterstitialAdUnitId()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_IOS
        return GoogleTestInterstitialIos;
#else
        return GoogleTestInterstitialAndroid;
#endif
#elif UNITY_ANDROID
        return ProductionInterstitialAndroid;
#elif UNITY_IOS
        // iOS production IDs not configured yet — keep Google test / no live fill.
        return GoogleTestInterstitialIos;
#else
        return "unused";
#endif
    }

    private static bool adsConfigLogged;

    private static void LogAdsConfigOnce()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (adsConfigLogged)
        {
            return;
        }

        adsConfigLogged = true;
        Debug.Log("[AdsConfig] Environment=TEST");
        Debug.Log("[AdsConfig] Interstitial=test");
        Debug.Log("[AdsConfig] Rewarded=test");
#endif
    }

    private void OnEnable()
    {
        SubscribeToConsent();
    }

    private void Update()
    {
        PumpMainThreadQueue();
    }

    private void Start()
    {
        // Do NOT MobileAds.Initialize here — wait for PrivacyConsentManager.
        PrivacyConsentManager.EnsureInstance();
        SubscribeToConsent();

        if (PrivacyConsentManager.IsResolved)
        {
            TryInitializeAdsFromConsent("StartAlreadyResolved");
        }
    }

    private void OnDisable()
    {
        if (subscribedToConsent)
        {
            PrivacyConsentManager.OnConsentResolved -= OnConsentResolved;
            subscribedToConsent = false;
        }
    }

    private void SubscribeToConsent()
    {
        if (subscribedToConsent)
        {
            return;
        }

        PrivacyConsentManager.OnConsentResolved += OnConsentResolved;
        subscribedToConsent = true;
    }

    private void OnConsentResolved()
    {
        TryInitializeAdsFromConsent("ConsentResolved");
    }

    /// <summary>
    /// Initializes Mobile Ads once when consent allows ad requests.
    /// </summary>
    public void InitializeAds()
    {
        TryInitializeAdsFromConsent("InitializeAdsExplicit");
    }

    private void TryInitializeAdsFromConsent(string reason)
    {
        if (adsInitialized)
        {
            return;
        }

        if (!PrivacyConsentManager.IsResolved)
        {
            LogAds("InitDeferred Reason=ConsentNotResolved Source=" + reason);
            return;
        }

        if (!PrivacyConsentManager.CanRequestAds)
        {
            LogAds("InitSkipped Reason=CanRequestAdsFalse Source=" + reason);
            return;
        }

        adsInitialized = true;
        LogAds("Initialized=True Source=" + reason);

        // Plugin helper + our queue as hard fallback. Default is false; without
        // marshalling, interstitial close can call PlayerPrefs.GetInt off-thread.
#pragma warning disable 0618
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
#pragma warning restore 0618

        MobileAds.Initialize(initStatus =>
        {
            RunOnMainThread(() =>
            {
                Debug.Log("Mobile Ads initialized");
                LoadRewardedAd();
                LoadInterstitialAd();
            });
        });
    }

    // --- Rewarded Ads ---

    /// <summary>
    /// Preloads rewarded ads for hint and shop_free_coins placements.
    /// </summary>
    public void LoadRewardedAd()
    {
        LoadRewardedAdForPlacement(PlacementHint);
        LoadRewardedAdForPlacement(PlacementShopFreeCoins);
    }

    /// <summary>
    /// Loads one rewarded placement. Destroys any previous ad for that slot first.
    /// No-op when ads are not allowed yet.
    /// extra_life shares the hint load slot / unit.
    /// </summary>
    public void LoadRewardedAdForPlacement(string placement)
    {
        if (!adsInitialized || !PrivacyConsentManager.CanRequestAds)
        {
            return;
        }

        string analyticsPlacement = NormalizeAnalyticsPlacement(placement);
        string slotPlacement = ResolveRewardedSlot(analyticsPlacement);
        DestroyRewardedAdForPlacement(slotPlacement);

        string adUnitId = GetRewardedAdUnitId(analyticsPlacement);
        AdRequest request = new AdRequest();

        RewardedAd.Load(adUnitId, request, (RewardedAd ad, LoadAdError error) =>
        {
            RunOnMainThread(() =>
            {
                if (error != null)
                {
                    Debug.Log(
                        "Rewarded ad failed to load placement=" + analyticsPlacement +
                        " slot=" + slotPlacement);
                    Debug.Log(error.ToString());
                    return;
                }

                if (ad == null)
                {
                    Debug.Log(
                        "Rewarded ad failed to load placement=" + analyticsPlacement +
                        " slot=" + slotPlacement);
                    return;
                }

                SetRewardedAdForPlacement(slotPlacement, ad);
                RegisterFullScreenCallbacks(ad, slotPlacement);

                Debug.Log(
                    "Rewarded ad loaded placement=" + analyticsPlacement +
                    " slot=" + slotPlacement +
                    " unit=" + adUnitId);
            });
        });
    }

    /// <summary>
    /// True when the FREE HINT rewarded ad is ready (backward-compatible default).
    /// </summary>
    public bool IsRewardedAdReady()
    {
        return IsRewardedAdReady(PlacementHint);
    }

    /// <summary>
    /// True when a rewarded ad for the given placement is ready to show.
    /// </summary>
    public bool IsRewardedAdReady(string placement)
    {
        string slotPlacement = ResolveRewardedSlot(NormalizeAnalyticsPlacement(placement));
        RewardedAd ad = GetRewardedAdForPlacement(slotPlacement);
        return adsInitialized &&
               PrivacyConsentManager.CanRequestAds &&
               !rewardedShowInProgress &&
               ad != null &&
               ad.CanShowAd();
    }

    /// <summary>
    /// Shows the rewarded ad for the given placement.
    /// onRewardEarned is only called when the UserEarnedReward callback fires (not on open/close/click).
    /// onFlowEnded (optional) is called once when the fullscreen closes or fails to show —
    /// used by shop free-coins / OutOfLives to re-enable the CTA. FREE HINT may omit it.
    /// Concurrent ShowRewardedAd calls are rejected (onFlowEnded invoked, no second show).
    /// </summary>
    public void ShowRewardedAd(
        Action onRewardEarned,
        string placement = "unknown",
        Action onFlowEnded = null)
    {
        string analyticsPlacement = NormalizeAnalyticsPlacement(placement);
        string slotPlacement = ResolveRewardedSlot(analyticsPlacement);

        if (rewardedShowInProgress)
        {
            Debug.Log(
                "Rewarded ad already in progress — rejecting placement=" +
                analyticsPlacement);
            onFlowEnded?.Invoke();
            return;
        }

        RewardedAd ad = GetRewardedAdForPlacement(slotPlacement);

        if (!adsInitialized ||
            !PrivacyConsentManager.CanRequestAds ||
            ad == null ||
            !ad.CanShowAd())
        {
            Debug.Log(
                "Rewarded ad not ready placement=" + analyticsPlacement);
            onFlowEnded?.Invoke();
            return;
        }

        pendingRewardedFlowEnded = onFlowEnded;
        rewardedFlowEndedInvoked = false;
        rewardedShowInProgress = true;

        // Analytics uses the caller placement (extra_life stays extra_life, not hint).
        GameAnalytics.LogRewardedAdStarted(analyticsPlacement);

        bool rewardCallbackInvoked = false;
        ad.Show(reward =>
        {
            RunOnMainThread(() =>
            {
                if (rewardCallbackInvoked)
                {
                    return;
                }

                rewardCallbackInvoked = true;
                Debug.Log("Reward earned placement=" + analyticsPlacement);
                GameAnalytics.LogRewardedAdCompleted(analyticsPlacement);
                onRewardEarned?.Invoke();
            });
        });
    }

    private void NotifyRewardedFlowEnded()
    {
        if (rewardedFlowEndedInvoked)
        {
            return;
        }

        rewardedFlowEndedInvoked = true;
        rewardedShowInProgress = false;
        Action ended = pendingRewardedFlowEnded;
        pendingRewardedFlowEnded = null;
        ended?.Invoke();
    }

    /// <summary>
    /// After close or error: destroy that slot's ad, end flow, reload that slot only.
    /// </summary>
    private void RegisterFullScreenCallbacks(RewardedAd ad, string slotPlacement)
    {
        string boundSlot = slotPlacement;

        ad.OnAdFullScreenContentClosed += () =>
        {
            RunOnMainThread(() =>
            {
                DestroyRewardedAdForPlacement(boundSlot);
                NotifyRewardedFlowEnded();
                LoadRewardedAdForPlacement(boundSlot);
            });
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            RunOnMainThread(() =>
            {
                Debug.Log(
                    "Rewarded ad failed to show placement slot=" + boundSlot +
                    ": " + error);

                DestroyRewardedAdForPlacement(boundSlot);
                NotifyRewardedFlowEnded();
                LoadRewardedAdForPlacement(boundSlot);
            });
        };
    }

    /// <summary>
    /// Analytics / API placement id. Preserves extra_life vs hint vs shop_free_coins.
    /// </summary>
    private static string NormalizeAnalyticsPlacement(string placement)
    {
        if (IsShopFreeCoinsPlacement(placement))
        {
            return PlacementShopFreeCoins;
        }

        if (IsExtraLifePlacement(placement))
        {
            return PlacementExtraLife;
        }

        // Default / unknown / "hint" → hint (preserves FREE HINT callers).
        return PlacementHint;
    }

    /// <summary>
    /// Physical load/show slot. extra_life shares the hint RewardedAd instance + unit.
    /// </summary>
    private static string ResolveRewardedSlot(string analyticsPlacement)
    {
        if (IsShopFreeCoinsPlacement(analyticsPlacement))
        {
            return PlacementShopFreeCoins;
        }

        return PlacementHint;
    }

    private RewardedAd GetRewardedAdForPlacement(string slotPlacement)
    {
        return IsShopFreeCoinsPlacement(slotPlacement)
            ? rewardedFreeCoinsAd
            : rewardedHintAd;
    }

    private void SetRewardedAdForPlacement(string slotPlacement, RewardedAd ad)
    {
        if (IsShopFreeCoinsPlacement(slotPlacement))
        {
            rewardedFreeCoinsAd = ad;
        }
        else
        {
            rewardedHintAd = ad;
        }
    }

    private void DestroyRewardedAdForPlacement(string slotPlacement)
    {
        if (IsShopFreeCoinsPlacement(slotPlacement))
        {
            if (rewardedFreeCoinsAd != null)
            {
                rewardedFreeCoinsAd.Destroy();
                rewardedFreeCoinsAd = null;
            }
        }
        else if (rewardedHintAd != null)
        {
            rewardedHintAd.Destroy();
            rewardedHintAd = null;
        }
    }

    // --- Interstitial Ads ---

    /// <summary>
    /// Loads a new interstitial ad. Destroys any previous ad first.
    /// No-op when ads are not allowed yet.
    /// </summary>
    public void LoadInterstitialAd()
    {
        if (!adsInitialized || !PrivacyConsentManager.CanRequestAds)
        {
            return;
        }

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        AdRequest request = new AdRequest();

        InterstitialAd.Load(GetInterstitialAdUnitId(), request, (InterstitialAd ad, LoadAdError error) =>
        {
            RunOnMainThread(() =>
            {
                if (error != null)
                {
                    Debug.Log("Interstitial failed to load: " + error);
                    return;
                }

                if (ad == null)
                {
                    Debug.Log("Interstitial failed to load");
                    return;
                }

                interstitialAd = ad;
                RegisterInterstitialCallbacks(interstitialAd);

                Debug.Log("Interstitial loaded");
            });
        });
    }

    /// <summary>
    /// True when an interstitial is ready to show.
    /// </summary>
    public bool IsInterstitialReady()
    {
        return adsInitialized &&
               PrivacyConsentManager.CanRequestAds &&
               interstitialAd != null &&
               interstitialAd.CanShowAd();
    }

    /// <summary>
    /// Shows the interstitial when available.
    /// onAdClosed is invoked once after close or failure (always on main thread).
    /// </summary>
    public void ShowInterstitialAd(Action onAdClosed = null)
    {
        if (!IsInterstitialReady())
        {
            Debug.Log("Interstitial not ready");
            RunOnMainThread(() => onAdClosed?.Invoke());
            return;
        }

        interstitialClosedCallback = onAdClosed;
        interstitialCallbackInvoked = false;

        Debug.Log("Interstitial shown");
        interstitialAd.Show();
    }

    /// <summary>
    /// Invokes the closed callback exactly once (caller must already be on main thread).
    /// </summary>
    private void InvokeInterstitialClosedOnce()
    {
        if (interstitialCallbackInvoked)
        {
            return;
        }

        interstitialCallbackInvoked = true;

        Action callback = interstitialClosedCallback;
        interstitialClosedCallback = null;
        callback?.Invoke();
    }

    /// <summary>
    /// After close or error: destroy interstitial, callback, reload.
    /// </summary>
    private void RegisterInterstitialCallbacks(InterstitialAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            RunOnMainThread(() =>
            {
                if (interstitialAd != null)
                {
                    interstitialAd.Destroy();
                    interstitialAd = null;
                }

                InvokeInterstitialClosedOnce();
                LoadInterstitialAd();
            });
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            RunOnMainThread(() =>
            {
                Debug.Log("Interstitial failed to show: " + error);

                if (interstitialAd != null)
                {
                    interstitialAd.Destroy();
                    interstitialAd = null;
                }

                InvokeInterstitialClosedOnce();
                LoadInterstitialAd();
            });
        };
    }

    /// <summary>
    /// Runs on the Unity main thread. Invokes immediately when already on main;
    /// otherwise queues for the next AdsManager.Update.
    /// Safe to call from Google Mobile Ads native/JNI callback threads.
    /// </summary>
    private void RunOnMainThread(Action action)
    {
        if (action == null)
        {
            return;
        }

        if (mainThreadId >= 0 &&
            System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError("[Ads] MainThread callback exception — " + ex.Message);
            }

            return;
        }

        lock (mainThreadQueueLock)
        {
            mainThreadQueue.Enqueue(action);
        }
    }

    private void PumpMainThreadQueue()
    {
        while (true)
        {
            Action action = null;
            lock (mainThreadQueueLock)
            {
                if (mainThreadQueue.Count == 0)
                {
                    break;
                }

                action = mainThreadQueue.Dequeue();
            }

            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError("[Ads] MainThread callback exception — " + ex.Message);
            }
        }
    }

    private static void LogAds(string message)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[Privacy] Ads " + message);
#endif
    }
}

using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

/// <summary>
/// Manages Rewarded and Interstitial Ads via Google Mobile Ads.
/// Uses official test ad unit IDs during development.
/// Initializes only after PrivacyConsentManager resolves and CanRequestAds is true.
/// Google Mobile Ads callbacks are marshalled onto the Unity main thread before
/// touching PlayerPrefs / scene / gameplay code (prevents GetInt off-thread crashes).
/// </summary>
public class AdsManager : MonoBehaviour
{
#if UNITY_ANDROID
    private const string adUnitId =
        "ca-app-pub-3940256099942544/5224354917";
    private const string interstitialAdUnitId =
        "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IOS
    private const string adUnitId =
        "ca-app-pub-3940256099942544/1712485313";
    private const string interstitialAdUnitId =
        "ca-app-pub-3940256099942544/4411468910";
#else
    private const string adUnitId = "unused";
    private const string interstitialAdUnitId = "unused";
#endif

    private RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;

    // Callback after interstitial close/fail — at most once.
    private Action interstitialClosedCallback;
    private bool interstitialCallbackInvoked;

    private bool adsInitialized;
    private bool subscribedToConsent;

    private readonly Queue<Action> mainThreadQueue = new Queue<Action>(8);
    private readonly object mainThreadQueueLock = new object();
    private int mainThreadId = -1;

    private void Awake()
    {
        mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
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
    /// Loads a new rewarded ad. Destroys any previous ad first.
    /// No-op when ads are not allowed yet.
    /// </summary>
    public void LoadRewardedAd()
    {
        if (!adsInitialized || !PrivacyConsentManager.CanRequestAds)
        {
            return;
        }

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        AdRequest request = new AdRequest();

        RewardedAd.Load(adUnitId, request, (RewardedAd ad, LoadAdError error) =>
        {
            RunOnMainThread(() =>
            {
                if (error != null)
                {
                    Debug.Log("Rewarded ad failed to load");
                    Debug.Log(error.ToString());
                    return;
                }

                if (ad == null)
                {
                    Debug.Log("Rewarded ad failed to load");
                    return;
                }

                rewardedAd = ad;
                RegisterFullScreenCallbacks(rewardedAd);

                Debug.Log("Rewarded ad loaded");
            });
        });
    }

    /// <summary>
    /// True when a rewarded ad is ready to show.
    /// </summary>
    public bool IsRewardedAdReady()
    {
        return adsInitialized &&
               PrivacyConsentManager.CanRequestAds &&
               rewardedAd != null &&
               rewardedAd.CanShowAd();
    }

    /// <summary>
    /// Shows the rewarded ad. onRewardEarned is only called when the reward is earned.
    /// </summary>
    public void ShowRewardedAd(Action onRewardEarned, string placement = "unknown")
    {
        if (!IsRewardedAdReady())
        {
            Debug.Log("Rewarded ad not ready");
            return;
        }

        GameAnalytics.LogRewardedAdStarted(placement);

        bool rewardCallbackInvoked = false;
        rewardedAd.Show(reward =>
        {
            RunOnMainThread(() =>
            {
                if (rewardCallbackInvoked)
                {
                    return;
                }

                rewardCallbackInvoked = true;
                Debug.Log("Reward earned");
                GameAnalytics.LogRewardedAdCompleted(placement);
                onRewardEarned?.Invoke();
            });
        });
    }

    /// <summary>
    /// After close or error: destroy old rewarded ad and reload.
    /// </summary>
    private void RegisterFullScreenCallbacks(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            RunOnMainThread(() =>
            {
                if (rewardedAd != null)
                {
                    rewardedAd.Destroy();
                    rewardedAd = null;
                }

                LoadRewardedAd();
            });
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            RunOnMainThread(() =>
            {
                Debug.Log("Rewarded ad failed to show: " + error);

                if (rewardedAd != null)
                {
                    rewardedAd.Destroy();
                    rewardedAd = null;
                }

                LoadRewardedAd();
            });
        };
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

        InterstitialAd.Load(interstitialAdUnitId, request, (InterstitialAd ad, LoadAdError error) =>
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

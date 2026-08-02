using System;
using GoogleMobileAds.Api;
using UnityEngine;

/// <summary>
/// Beheert Rewarded en Interstitial Ads via Google Mobile Ads.
/// Gebruik tijdens development de officiële test ad unit IDs.
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

    // Callback na sluiten/falen van interstitial — maximaal één keer.
    private Action interstitialClosedCallback;
    private bool interstitialCallbackInvoked;

    private void Start()
    {
        // Initialiseer Google Mobile Ads één keer.
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("Mobile Ads initialized");
            LoadRewardedAd();
            LoadInterstitialAd();
        });
    }

    // --- Rewarded Ads ---

    /// <summary>
    /// Laadt een nieuwe rewarded ad.
    /// Vernietigt eerst een eventuele oude ad.
    /// </summary>
    public void LoadRewardedAd()
    {
        // Oude ad opruimen voordat we een nieuwe laden.
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        AdRequest request = new AdRequest();

        RewardedAd.Load(adUnitId, request, (RewardedAd ad, LoadAdError error) =>
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
    }

    /// <summary>
    /// True als er een rewarded ad klaarstaat om te tonen.
    /// </summary>
    public bool IsRewardedAdReady()
    {
        return rewardedAd != null && rewardedAd.CanShowAd();
    }

    /// <summary>
    /// Toont de rewarded ad. onRewardEarned wordt alleen aangeroepen
    /// wanneer de gebruiker de reward daadwerkelijk heeft verdiend.
    /// </summary>
    public void ShowRewardedAd(Action onRewardEarned)
    {
        if (!IsRewardedAdReady())
        {
            Debug.Log("Rewarded ad not ready");
            return;
        }

        rewardedAd.Show(reward =>
        {
            Debug.Log("Reward earned");
            onRewardEarned?.Invoke();
        });
    }

    /// <summary>
    /// Na sluiten of fout: oude rewarded ad vernietigen en opnieuw laden.
    /// </summary>
    private void RegisterFullScreenCallbacks(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }

            LoadRewardedAd();
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.Log("Rewarded ad failed to show: " + error);

            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }

            LoadRewardedAd();
        };
    }

    // --- Interstitial Ads ---

    /// <summary>
    /// Laadt een nieuwe interstitial ad.
    /// Vernietigt eerst een eventuele oude ad.
    /// </summary>
    public void LoadInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        AdRequest request = new AdRequest();

        InterstitialAd.Load(interstitialAdUnitId, request, (InterstitialAd ad, LoadAdError error) =>
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
    }

    /// <summary>
    /// True als er een interstitial klaarstaat om te tonen.
    /// </summary>
    public bool IsInterstitialReady()
    {
        return interstitialAd != null && interstitialAd.CanShowAd();
    }

    /// <summary>
    /// Toont de interstitial ad als die beschikbaar is.
    /// onAdClosed wordt één keer aangeroepen na sluiten of bij failure.
    /// </summary>
    public void ShowInterstitialAd(Action onAdClosed = null)
    {
        if (!IsInterstitialReady())
        {
            Debug.Log("Interstitial not ready");
            return;
        }

        interstitialClosedCallback = onAdClosed;
        interstitialCallbackInvoked = false;

        Debug.Log("Interstitial shown");
        interstitialAd.Show();
    }

    /// <summary>
    /// Voert de closed-callback precies één keer uit.
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
    /// Na sluiten of fout: oude interstitial vernietigen, callback, opnieuw laden.
    /// </summary>
    private void RegisterInterstitialCallbacks(InterstitialAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }

            InvokeInterstitialClosedOnce();
            LoadInterstitialAd();
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.Log("Interstitial failed to show: " + error);

            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }

            InvokeInterstitialClosedOnce();
            LoadInterstitialAd();
        };
    }
}

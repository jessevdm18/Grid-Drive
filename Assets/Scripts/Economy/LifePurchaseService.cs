using UnityEngine;

/// <summary>
/// Authoritative life economy transactions (coins + rewarded). Presentation owns UI only.
/// </summary>
public static class LifePurchaseService
{
    private static int rewardedLifeAttemptSerial;
    private static int rewardedLifeGrantConsumedForAttempt = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        rewardedLifeAttemptSerial = 0;
        rewardedLifeGrantConsumedForAttempt = -1;
    }

    /// <summary>
    /// Attempts to buy exactly +1 life for <see cref="LivesEconomyConfig.LifeCoinCost"/> coins.
    /// Works for any CurrentLives &lt; MaxLives; Phase 5 UI only exposes it from OutOfLivesUI.
    /// </summary>
    public static LifePurchaseResult TryPurchaseLifeWithCoins(
        CoinManager coinManager = null,
        LivesManager livesManager = null)
    {
        if (livesManager == null)
        {
            livesManager = LivesManager.EnsureInstance();
        }

        if (coinManager == null)
        {
            coinManager = ResolveCoinManager();
        }

        if (livesManager == null || coinManager == null)
        {
            return LifePurchaseResult.Unavailable;
        }

        livesManager.RecalculateRegeneration();

        int livesBefore = livesManager.CurrentLives;
        if (livesBefore >= LivesManager.MaxLives)
        {
            return LifePurchaseResult.LivesFull;
        }

        int cost = LivesEconomyConfig.LifeCoinCost;
        if (!coinManager.CanAfford(cost))
        {
            return LifePurchaseResult.NotEnoughCoins;
        }

        // Spend first — never grant a life without a confirmed debit.
        if (!coinManager.SpendCoins(cost))
        {
            return LifePurchaseResult.NotEnoughCoins;
        }

        if (!livesManager.AddLife(1))
        {
            // Should be unreachable after the lives-full check; refund to stay consistent.
            coinManager.AddCoins(cost);
            return livesManager.CurrentLives >= LivesManager.MaxLives
                ? LifePurchaseResult.LivesFull
                : LifePurchaseResult.Unavailable;
        }

        int livesAfter = livesManager.CurrentLives;
        GameAnalytics.LogLifePurchase(
            currency: "coins",
            cost: cost,
            livesBefore: livesBefore,
            livesAfter: livesAfter
        );

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[Lives] Purchase life with coins OK | cost=" + cost +
            " lives " + livesBefore + "→" + livesAfter +
            " coins=" + coinManager.Coins
        );
#endif

        return LifePurchaseResult.Success;
    }

    /// <summary>
    /// Begins a rewarded-life attempt token. Pair with <see cref="GrantLifeFromRewardedAd"/>.
    /// </summary>
    public static int BeginRewardedLifeAttempt()
    {
        rewardedLifeAttemptSerial++;
        return rewardedLifeAttemptSerial;
    }

    /// <summary>
    /// Grants exactly +1 life after a rewarded-ad UserEarnedReward callback.
    /// No coins. Exactly-once per <paramref name="attemptToken"/> from
    /// <see cref="BeginRewardedLifeAttempt"/>.
    /// </summary>
    public static LifePurchaseResult GrantLifeFromRewardedAd(
        int attemptToken = 0,
        LivesManager livesManager = null,
        bool logAnalytics = true)
    {
        if (attemptToken > 0)
        {
            if (attemptToken != rewardedLifeAttemptSerial)
            {
                return LifePurchaseResult.Unavailable;
            }

            if (rewardedLifeGrantConsumedForAttempt == attemptToken)
            {
                // Duplicate reward callback for the same ad attempt — do not grant again.
                return LifePurchaseResult.Success;
            }

            // Claim before AddLife so concurrent duplicate callbacks cannot double-grant.
            rewardedLifeGrantConsumedForAttempt = attemptToken;
        }

        if (livesManager == null)
        {
            livesManager = LivesManager.EnsureInstance();
        }

        if (livesManager == null)
        {
            return LifePurchaseResult.Unavailable;
        }

        livesManager.RecalculateRegeneration();

        int livesBefore = livesManager.CurrentLives;
        if (livesBefore >= LivesManager.MaxLives)
        {
            return LifePurchaseResult.LivesFull;
        }

        if (!livesManager.AddLife(1))
        {
            return livesManager.CurrentLives >= LivesManager.MaxLives
                ? LifePurchaseResult.LivesFull
                : LifePurchaseResult.Unavailable;
        }

        int livesAfter = livesManager.CurrentLives;

        if (logAnalytics)
        {
            GameAnalytics.LogLifeRewardedAd(livesBefore, livesAfter);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[Lives] Grant life from rewarded ad OK | attempt=" + attemptToken +
            " lives " + livesBefore + "→" + livesAfter
        );
#endif

        return LifePurchaseResult.Success;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// Editor/DEV: exercise grant path without a real ad or production ad analytics.
    /// Does not call rewarded_ad_started / rewarded_ad_completed.
    /// </summary>
    public static LifePurchaseResult SimulateRewardedLifeSuccess(LivesManager livesManager = null)
    {
        int token = BeginRewardedLifeAttempt();
        return GrantLifeFromRewardedAd(token, livesManager, logAnalytics: false);
    }
#endif

    /// <summary>Resolves the active scene CoinManager (authoritative balance owner).</summary>
    public static CoinManager ResolveCoinManager()
    {
        return UnityEngine.Object.FindAnyObjectByType<CoinManager>();
    }

    /// <summary>Resolves the active scene AdsManager if present.</summary>
    public static AdsManager ResolveAdsManager()
    {
        return UnityEngine.Object.FindAnyObjectByType<AdsManager>();
    }
}

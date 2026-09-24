using UnityEngine;

/// <summary>
/// Result of <see cref="RestartPurchaseService.TryRestartWithEconomy"/>.
/// </summary>
public enum RestartPurchaseResult
{
    /// <summary>Reload started (paid active, free failed, free completed, or V1).</summary>
    Restarted = 0,
    /// <summary>Active restart blocked — not enough coins; board unchanged.</summary>
    NotEnoughCoins = 1,
    /// <summary>Lives gate blocked reload (e.g. 0 lives after failure); OutOfLives owns UI.</summary>
    LivesBlocked = 2,
    /// <summary>Missing deps or restart already in progress.</summary>
    Unavailable = 3
}

/// <summary>
/// Authoritative restart economy: ACTIVE attempt costs
/// <see cref="LivesEconomyConfig.RestartCoinCost"/> coins; FAILED retry is free.
/// All gameplay restart buttons must call here — never SpendCoins locally.
/// </summary>
public static class RestartPurchaseService
{
    private static bool restartInProgress;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        restartInProgress = false;
    }

    /// <summary>
    /// Pays (when required) then reloads via <see cref="LevelManager.RestartLevel"/>.
    /// </summary>
    public static RestartPurchaseResult TryRestartWithEconomy(
        LevelManager levelManager = null,
        GameManager gameManager = null,
        CoinManager coinManager = null,
        string source = "restart")
    {
        if (restartInProgress || SceneTransition.IsTransitioning)
        {
            return RestartPurchaseResult.Unavailable;
        }

        if (levelManager == null)
        {
            levelManager = Object.FindFirstObjectByType<LevelManager>();
        }

        if (levelManager == null)
        {
            return RestartPurchaseResult.Unavailable;
        }

        if (levelManager.IsDailyChallengeSession || DailyChallengeContext.IsActiveSession)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(
                "[DailyChallenge] Restart blocked — one attempt only | source=" + source);
#endif
            return RestartPurchaseResult.Unavailable;
        }

        if (gameManager == null)
        {
            gameManager = Object.FindFirstObjectByType<GameManager>();
        }

        bool isFailed = gameManager != null && gameManager.IsLevelFailed;
        bool isCompleted = gameManager != null && gameManager.IsLevelCompleted;
        bool isV1 = levelManager.IsV1CandidatePlaytest;

        // Active mid-run only. Failed retry and win-panel restart are free.
        bool requiresPayment = !isFailed && !isCompleted && !isV1;
        int cost = requiresPayment ? LivesEconomyConfig.RestartCoinCost : 0;
        string restartType = isV1
            ? "free_v1"
            : isFailed
                ? "free_after_failure"
                : isCompleted
                    ? "free_completed"
                    : "paid_active";

        if (requiresPayment)
        {
            if (coinManager == null)
            {
                coinManager = CoinManager.EnsureInstance();
            }

            if (coinManager == null)
            {
                return RestartPurchaseResult.Unavailable;
            }

            if (cost > 0 && !coinManager.CanAfford(cost))
            {
                AudioManager.Resolve()?.PlayInsufficientCoins();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log(
                    "[Restart] Blocked — not enough coins | need=" + cost +
                    " have=" + coinManager.Coins + " source=" + source
                );
#endif
                return RestartPurchaseResult.NotEnoughCoins;
            }

            if (cost > 0 && !coinManager.SpendCoins(cost))
            {
                AudioManager.Resolve()?.PlayInsufficientCoins();
                return RestartPurchaseResult.NotEnoughCoins;
            }

            if (cost > 0)
            {
                AudioManager.Resolve()?.PlayCoinSpend();
            }
        }

        restartInProgress = true;
        bool reloaded;
        try
        {
            reloaded = levelManager.RestartLevel(
                restartCost: cost,
                restartType: restartType);
        }
        finally
        {
            restartInProgress = false;
        }

        if (!reloaded)
        {
            // Lives gate blocked — refund a paid active spend so 0-life failed
            // retry never loses coins, and a rare active-block is fair.
            if (requiresPayment && cost > 0)
            {
                if (coinManager == null)
                {
                    coinManager = CoinManager.EnsureInstance();
                }

                coinManager?.AddCoins(cost);
            }

            return RestartPurchaseResult.LivesBlocked;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[Restart] OK | type=" + restartType +
            " cost=" + cost +
            " source=" + source +
            " coins=" + (coinManager != null ? coinManager.Coins.ToString() : "n/a")
        );
#endif

        return RestartPurchaseResult.Restarted;
    }
}

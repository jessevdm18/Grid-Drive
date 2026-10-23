using System;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;

/// <summary>
/// Sole analytics façade. Other scripts must not call FirebaseAnalytics directly.
/// Events requested before Firebase is ready are queued and sent from FlushPending.
/// </summary>
public static class GameAnalytics
{
    private static int levelLoadSerial;
    private static int startLoggedSerial = -1;
    private static int completeLoggedSerial = -1;
    private static float levelStartUnscaledTime;
    private static int rewardedShowSerial;
    private static int rewardedCompletedSerial = -1;
    private static readonly List<PendingEvent> pendingEvents = new List<PendingEvent>();

    private struct PendingEvent
    {
        public string Name;
        public Parameter[] Parameters;
    }

    public static void LogLevelStart(
        int levelNumber,
        string difficulty,
        string objective,
        int minimumMoves,
        int gridWidth,
        int gridHeight,
        string levelAsset = null)
    {
        levelLoadSerial++;
        completeLoggedSerial = -1;
        levelStartUnscaledTime = Time.unscaledTime;

        if (startLoggedSerial == levelLoadSerial)
        {
            return;
        }

        startLoggedSerial = levelLoadSerial;

        try
        {
            var parameters = new List<Parameter>
            {
                new Parameter("level_number", levelNumber),
                new Parameter("difficulty", Safe(difficulty)),
                new Parameter("objective", Safe(objective)),
                new Parameter("minimum_moves", minimumMoves),
                new Parameter("grid_width", gridWidth),
                new Parameter("grid_height", gridHeight)
            };

            if (!string.IsNullOrEmpty(levelAsset))
            {
                parameters.Add(new Parameter("level_asset", levelAsset));
            }

            Emit("level_start", parameters.ToArray());
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogLevelStart failed — " + ex.Message);
        }
    }

    public static void LogLevelComplete(
        int levelNumber,
        string difficulty,
        string objective,
        int movesUsed,
        int par,
        int stars,
        string levelAsset = null,
        string sessionType = "campaign")
    {
        if (completeLoggedSerial == levelLoadSerial && levelLoadSerial > 0)
        {
            return;
        }

        completeLoggedSerial = levelLoadSerial;

        try
        {
            int durationSeconds = Mathf.Max(
                0,
                Mathf.RoundToInt(Time.unscaledTime - levelStartUnscaledTime)
            );

            var parameters = new List<Parameter>
            {
                new Parameter("level_number", levelNumber),
                new Parameter("difficulty", Safe(difficulty)),
                new Parameter("objective", Safe(objective)),
                new Parameter("moves_used", movesUsed),
                new Parameter("par", par),
                new Parameter("stars", stars),
                new Parameter("duration_seconds", durationSeconds),
                new Parameter("session_type", Safe(sessionType))
            };

            string levelName = string.IsNullOrEmpty(levelAsset)
                ? "level_" + levelNumber
                : levelAsset;
            parameters.Add(new Parameter("level_name", levelName));
            parameters.Add(new Parameter("success", 1));
            if (!string.IsNullOrEmpty(levelAsset))
            {
                parameters.Add(new Parameter("level_asset", levelAsset));
            }

            Emit("level_complete", parameters.ToArray());
            Emit(
                "level_end",
                new Parameter("level_name", levelName),
                new Parameter("success", 1),
                new Parameter("level_number", levelNumber),
                new Parameter("difficulty", Safe(difficulty)),
                new Parameter("moves_used", movesUsed),
                new Parameter("minimum_moves", par),
                new Parameter("stars", stars),
                new Parameter("session_type", Safe(sessionType)));
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogLevelComplete failed — " + ex.Message);
        }
    }

    public static void LogLevelFail(
        int levelNumber,
        string difficulty,
        string objective,
        string failReason,
        int movesUsed)
    {
        try
        {
            Emit(
                "level_fail",
                new Parameter("level_number", levelNumber),
                new Parameter("difficulty", Safe(difficulty)),
                new Parameter("objective", Safe(objective)),
                new Parameter("fail_reason", Safe(failReason)),
                new Parameter("moves_used", movesUsed)
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogLevelFail failed — " + ex.Message);
        }
    }

    public static void LogLevelRestart(
        int levelNumber,
        string difficulty,
        int movesUsed,
        int restartCost = 0,
        string restartType = "unspecified")
    {
        try
        {
            Emit(
                "level_restart",
                new Parameter("level_number", levelNumber),
                new Parameter("difficulty", Safe(difficulty)),
                new Parameter("moves_used", movesUsed),
                new Parameter("restart_cost", restartCost),
                new Parameter("restart_type", Safe(restartType))
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogLevelRestart failed — " + ex.Message);
        }
    }

    public static void LogHintUsed(
        int levelNumber,
        string difficulty,
        string source)
    {
        try
        {
            Emit(
                "hint_used",
                new Parameter("level_number", levelNumber),
                new Parameter("difficulty", Safe(difficulty)),
                new Parameter("source", Safe(source))
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogHintUsed failed — " + ex.Message);
        }
    }

    public static void LogRewardedAdStarted(string placement)
    {
        rewardedShowSerial++;
        rewardedCompletedSerial = -1;

        try
        {
            Emit(
                "rewarded_ad_started",
                new Parameter("placement", Safe(placement))
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogRewardedAdStarted failed — " + ex.Message);
        }
    }

    public static void LogRewardedAdCompleted(string placement)
    {
        if (rewardedCompletedSerial == rewardedShowSerial && rewardedShowSerial > 0)
        {
            return;
        }

        rewardedCompletedSerial = rewardedShowSerial;

        try
        {
            Emit(
                "rewarded_ad_completed",
                new Parameter("placement", Safe(placement))
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogRewardedAdCompleted failed — " + ex.Message);
        }
    }

    public static void LogDifficultyUnlocked(string difficulty)
    {
        try
        {
            Emit(
                "difficulty_unlocked",
                new Parameter("difficulty", Safe(difficulty))
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogDifficultyUnlocked failed — " + ex.Message);
        }
    }

    public static void LogDifficultySelected(string difficulty)
    {
        try
        {
            Emit(
                "difficulty_selected",
                new Parameter("difficulty", Safe(difficulty))
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogDifficultySelected failed — " + ex.Message);
        }
    }

    public static void LogCoinReward(int amount, string source)
    {
        try
        {
            Emit(
                "coin_reward",
                new Parameter("amount", amount),
                new Parameter("source", Safe(source))
            );
            Emit(
                "earn_virtual_currency",
                new Parameter("virtual_currency_name", "coins"),
                new Parameter("value", amount),
                new Parameter("source", Safe(source))
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogCoinReward failed — " + ex.Message);
        }
    }

    /// <summary>
    /// Successful life-for-coins purchase (currency spend + life grant).
    /// </summary>
    public static void LogLifePurchase(
        string currency,
        int cost,
        int livesBefore,
        int livesAfter)
    {
        try
        {
            Emit(
                "life_purchase",
                new Parameter("currency", Safe(currency)),
                new Parameter("cost", cost),
                new Parameter("lives_before", livesBefore),
                new Parameter("lives_after", livesAfter)
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogLifePurchase failed — " + ex.Message);
        }
    }

    /// <summary>
    /// Life granted after a rewarded-ad UserEarnedReward callback (not on ad close alone).
    /// </summary>
    public static void LogLifeRewardedAd(int livesBefore, int livesAfter)
    {
        try
        {
            Emit(
                "life_rewarded_ad",
                new Parameter("lives_before", livesBefore),
                new Parameter("lives_after", livesAfter)
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogLifeRewardedAd failed — " + ex.Message);
        }
    }

    public static void LogObjectiveFailed(
        int levelNumber,
        string difficulty,
        string objective,
        string reason)
    {
        try
        {
            Emit(
                "objective_failed",
                new Parameter("level_number", levelNumber),
                new Parameter("difficulty", Safe(difficulty)),
                new Parameter("objective", Safe(objective)),
                new Parameter("reason", Safe(reason))
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogObjectiveFailed failed — " + ex.Message);
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static void SendTestEvent()
    {
        FirebaseManager.EnsureInstance();
        if (!FirebaseManager.IsReady)
        {
            Debug.LogWarning(
                "GameAnalytics: Firebase not ready — test event skipped. InitStage=" +
                FirebaseManager.InitStage
            );
            return;
        }

        try
        {
            Emit(
                "firebase_test_event",
                new Parameter("source", "editor_menu")
            );
            Debug.Log("GameAnalytics: test event firebase_test_event sent.");
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.SendTestEvent failed — " + ex.Message);
        }
    }
#endif

    public static void LogDailyChallengeView(string dayId)
    {
        LogDailySimple("daily_challenge_view", dayId, null, null);
    }

    public static void LogDailyChallengeStart(string dayId, string levelAsset, int poolIndex)
    {
        try
        {
            Emit(
                "daily_challenge_start",
                new Parameter("day_id", Safe(dayId)),
                new Parameter("level_asset", Safe(levelAsset)),
                new Parameter("pool_index", poolIndex));
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogDailyChallengeStart failed — " + ex.Message);
        }
    }

    public static void LogDailyChallengeComplete(
        string dayId,
        string levelAsset,
        int moves = 0,
        long completionTimeMs = 0,
        int score = 0)
    {
        try
        {
            Emit(
                "daily_challenge_complete",
                new Parameter("day_id", Safe(dayId)),
                new Parameter("level_id", Safe(levelAsset)),
                new Parameter("moves", moves),
                new Parameter("completion_time_ms", completionTimeMs),
                new Parameter("score", score));
            Emit(
                "post_score",
                new Parameter("score", score),
                new Parameter("level_name", Safe(levelAsset)));
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogDailyChallengeComplete failed — " + ex.Message);
        }
    }

    public static void LogDailyChallengeFail(string dayId, string levelAsset, string failReason)
    {
        // Phase 2: performance fails are suppressed; kept for any residual telemetry.
        LogDailySimple("daily_challenge_fail", dayId, levelAsset, failReason);
    }

    public static void LogDailyChallengeAbandon(string dayId, string levelAsset, string reason)
    {
        LogDailySimple("daily_challenge_abandon", dayId, levelAsset, reason);
    }

    public static void LogDailyLeaderboardView(
        string dayId,
        bool playerHasResult,
        int playerRank,
        int entryCount)
    {
        try
        {
            Emit(
                "daily_leaderboard_view",
                new Parameter("day_id", Safe(dayId)),
                new Parameter("player_has_result", playerHasResult ? 1L : 0L),
                new Parameter("player_rank", (long)playerRank),
                new Parameter("entry_count", (long)entryCount));
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogDailyLeaderboardView failed — " + ex.Message);
        }
    }

    public static void LogDailyLeaderboardOnlineView(
        string dayId,
        bool playerHasResult,
        int playerRank,
        int entryCount)
    {
        LogDailySimple("daily_leaderboard_online_view", dayId, null, null);
        // Also emit structured fields when Firebase ready.
        try
        {
            Emit(
                "daily_leaderboard_online_view",
                new Parameter("day_id", Safe(dayId)),
                new Parameter("player_has_result", playerHasResult ? 1L : 0L),
                new Parameter("player_rank", (long)playerRank),
                new Parameter("entry_count", (long)entryCount));
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogDailyLeaderboardOnlineView failed — " + ex.Message);
        }
    }

    public static void LogDailyBackendStartSuccess(string dayId)
    {
        LogDailySimple("daily_backend_start_success", dayId, null, null);
    }

    public static void LogDailyBackendStartRejected(string dayId, string reason)
    {
        LogDailySimple("daily_backend_start_rejected", dayId, null, reason);
    }

    public static void LogDailyBackendStartError(string dayId, string reason)
    {
        LogDailySimple("daily_backend_start_error", dayId, null, reason);
    }

    public static void LogDailyResultSubmitSuccess(string dayId)
    {
        LogDailySimple("daily_result_submit_success", dayId, null, null);
    }

    public static void LogDailyResultSubmitRetry(string dayId)
    {
        LogDailySimple("daily_result_submit_retry", dayId, null, null);
    }

    public static void LogDailyResultSubmitError(string dayId, string reason)
    {
        LogDailySimple("daily_result_submit_error", dayId, null, reason);
    }

    public static void LogDailyNotificationOptIn(string source)
    {
        LogDailyNotificationSimple("daily_notification_opt_in", source, null);
    }

    public static void LogDailyNotificationOptOut(string source)
    {
        LogDailyNotificationSimple("daily_notification_opt_out", source, null);
    }

    public static void LogDailyNotificationReceived(string dayId)
    {
        LogDailyNotificationSimple("daily_notification_received", "runtime", dayId);
    }

    public static void LogDailyNotificationOpened(string dayId)
    {
        LogDailyNotificationSimple("daily_notification_opened", "runtime", dayId);
    }

    private static void LogDailyNotificationSimple(string eventName, string source, string dayId)
    {
        try
        {
            var parameters = new List<Parameter>
            {
                new Parameter("source", Safe(source))
            };
            if (!string.IsNullOrEmpty(dayId))
            {
                parameters.Add(new Parameter("day_id", Safe(dayId)));
            }

            Emit(eventName, parameters.ToArray());
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics." + eventName + " failed — " + ex.Message);
        }
    }

    public static void LogDailyRewardCreated(int rank, string rewardType, int rewardAmount)
    {
        LogDailyReward("daily_reward_created", rank, rewardType, rewardAmount);
    }

    public static void LogDailyRewardClaimed(int rank, string rewardType, int rewardAmount)
    {
        LogDailyReward("daily_reward_claimed", rank, rewardType, rewardAmount);
    }

    private static void LogDailyReward(
        string eventName,
        int rank,
        string rewardType,
        int rewardAmount)
    {
        try
        {
            Emit(
                eventName,
                new Parameter("rank", (long)rank),
                new Parameter("reward_type", Safe(rewardType)),
                new Parameter("reward_amount", (long)rewardAmount));
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics." + eventName + " failed — " + ex.Message);
        }
    }

    public static void LogAccountLinkStarted(string provider)
    {
        LogAccountLink("account_link_started", provider, null);
    }

    public static void LogAccountLinkSuccess(string provider)
    {
        LogAccountLink("account_link_success", provider, null);
        try
        {
            Emit("login", new Parameter("method", Safe(provider)));
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.login failed — " + ex.Message);
        }
    }

    public static void LogCoinSpend(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        try
        {
            Emit(
                "spend_virtual_currency",
                new Parameter("virtual_currency_name", "coins"),
                new Parameter("value", amount));
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogCoinSpend failed — " + ex.Message);
        }
    }

    public static void LogAccountLinkFailed(string provider, string reason)
    {
        LogAccountLink("account_link_failed", provider, reason);
    }

    private static void LogAccountLink(string eventName, string provider, string reason)
    {
        try
        {
            var parameters = new List<Parameter>
            {
                new Parameter("provider", Safe(provider))
            };
            if (!string.IsNullOrEmpty(reason))
            {
                parameters.Add(new Parameter("reason", Safe(reason)));
            }

            Emit(eventName, parameters.ToArray());
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics." + eventName + " failed — " + ex.Message);
        }
    }

    private static void LogDailySimple(
        string eventName,
        string dayId,
        string levelAsset,
        string extraReason)
    {
        try
        {
            var parameters = new List<Parameter>
            {
                new Parameter("day_id", Safe(dayId))
            };
            if (!string.IsNullOrEmpty(levelAsset))
            {
                parameters.Add(new Parameter("level_asset", Safe(levelAsset)));
            }

            if (!string.IsNullOrEmpty(extraReason))
            {
                parameters.Add(new Parameter("fail_reason", Safe(extraReason)));
            }

            Emit(eventName, parameters.ToArray());
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics." + eventName + " failed — " + ex.Message);
        }
    }

    public static void FlushPending()
    {
        if (!FirebaseManager.IsReady || pendingEvents.Count == 0)
        {
            return;
        }

        PendingEvent[] queued = pendingEvents.ToArray();
        pendingEvents.Clear();
        for (int i = 0; i < queued.Length; i++)
        {
            Emit(queued[i].Name, queued[i].Parameters);
        }
    }

    private static void Emit(string eventName, params Parameter[] parameters)
    {
        Trace(
            eventName + " requested ready=" + (FirebaseManager.IsReady ? "yes" : "no") +
            " collection=" + FirebaseManager.GetAnalyticsCollectionEnabledOrUnknown());
        if (!FirebaseManager.IsReady)
        {
            pendingEvents.Add(new PendingEvent
            {
                Name = eventName,
                Parameters = parameters
            });
            Trace(eventName + " queued");
            return;
        }

        try
        {
            FirebaseAnalytics.LogEvent(eventName, parameters ?? new Parameter[0]);
            string collection = FirebaseManager.GetAnalyticsCollectionEnabledOrUnknown();
            Trace(
                string.Equals(collection, "True", StringComparison.Ordinal)
                    ? eventName + " sent"
                    : eventName + " sent while collection=" + collection);
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics." + eventName + " failed — " + ex.Message);
        }
    }

    private static void Trace(string message)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[Analytics] " + message);
#endif
    }

    private static string Safe(string value)
    {
        return string.IsNullOrEmpty(value) ? "unknown" : value;
    }
}

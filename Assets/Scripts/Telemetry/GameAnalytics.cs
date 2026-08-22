using System;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;

/// <summary>
/// Sole analytics façade. Other scripts must not call FirebaseAnalytics directly.
/// All methods no-op when FirebaseManager.IsReady is false.
/// </summary>
public static class GameAnalytics
{
    private static int levelLoadSerial;
    private static int startLoggedSerial = -1;
    private static int completeLoggedSerial = -1;
    private static float levelStartUnscaledTime;
    private static int rewardedShowSerial;
    private static int rewardedCompletedSerial = -1;

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

        if (!FirebaseManager.IsReady)
        {
            return;
        }

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

            FirebaseAnalytics.LogEvent("level_start", parameters.ToArray());
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
        string levelAsset = null)
    {
        if (completeLoggedSerial == levelLoadSerial && levelLoadSerial > 0)
        {
            return;
        }

        completeLoggedSerial = levelLoadSerial;

        if (!FirebaseManager.IsReady)
        {
            return;
        }

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
                new Parameter("duration_seconds", durationSeconds)
            };

            if (!string.IsNullOrEmpty(levelAsset))
            {
                parameters.Add(new Parameter("level_asset", levelAsset));
            }

            FirebaseAnalytics.LogEvent("level_complete", parameters.ToArray());
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
        if (!FirebaseManager.IsReady)
        {
            return;
        }

        try
        {
            FirebaseAnalytics.LogEvent(
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
        int movesUsed)
    {
        if (!FirebaseManager.IsReady)
        {
            return;
        }

        try
        {
            FirebaseAnalytics.LogEvent(
                "level_restart",
                new Parameter("level_number", levelNumber),
                new Parameter("difficulty", Safe(difficulty)),
                new Parameter("moves_used", movesUsed)
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
        if (!FirebaseManager.IsReady)
        {
            return;
        }

        try
        {
            FirebaseAnalytics.LogEvent(
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

        if (!FirebaseManager.IsReady)
        {
            return;
        }

        try
        {
            FirebaseAnalytics.LogEvent(
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

        if (!FirebaseManager.IsReady)
        {
            return;
        }

        try
        {
            FirebaseAnalytics.LogEvent(
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
        if (!FirebaseManager.IsReady)
        {
            return;
        }

        try
        {
            FirebaseAnalytics.LogEvent(
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
        if (!FirebaseManager.IsReady)
        {
            return;
        }

        try
        {
            FirebaseAnalytics.LogEvent(
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
        if (!FirebaseManager.IsReady)
        {
            return;
        }

        try
        {
            FirebaseAnalytics.LogEvent(
                "coin_reward",
                new Parameter("amount", amount),
                new Parameter("source", Safe(source))
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("GameAnalytics.LogCoinReward failed — " + ex.Message);
        }
    }

    public static void LogObjectiveFailed(
        int levelNumber,
        string difficulty,
        string objective,
        string reason)
    {
        if (!FirebaseManager.IsReady)
        {
            return;
        }

        try
        {
            FirebaseAnalytics.LogEvent(
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
            FirebaseAnalytics.LogEvent(
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

    private static string Safe(string value)
    {
        return string.IsNullOrEmpty(value) ? "unknown" : value;
    }
}

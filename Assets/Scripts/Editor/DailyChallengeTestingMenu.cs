#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor/dev-only Daily Challenge testing tools.
/// Does not mutate normal lives / campaign progression.
/// </summary>
public static class DailyChallengeTestingMenu
{
    private const string Root = "Rush Out/Testing/Daily Challenge/";

    [MenuItem(Root + "Log Daily State")]
    public static void LogDailyState()
    {
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        manager.RefreshFromStoreAndRecover();
        Debug.Log(
            "[DailyChallenge]\n" +
            "DayId=" + manager.CurrentDayId + "\n" +
            "State=" + manager.CurrentState + "\n" +
            "Level=" + manager.SelectedLevelAssetName + "\n" +
            "PoolIndex=" + manager.SelectedPoolIndex + "\n" +
            "Countdown=" + manager.FormatCountdown() + "\n" +
            "ContextPending=" + DailyChallengeContext.HasPending + "\n" +
            "ContextActive=" + DailyChallengeContext.IsActiveSession + "\n" +
            "PolicyActive=" + DailyChallengeGameplayPolicy.IsActive + "\n" +
            "UtcNow=" + DailyChallengeClock.UtcNow.ToString("o"));
    }

    [MenuItem(Root + "Log Selected Daily Level")]
    public static void LogSelectedLevel()
    {
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        manager.EnsureDaySynced();
        LevelData level = manager.SelectedLevel;
        Debug.Log(
            "[DailyChallenge] Selected level day=" + manager.CurrentDayId +
            " asset=" + (level != null ? level.name : "null") +
            " poolIndex=" + manager.SelectedPoolIndex +
            " minMoves=" + (level != null ? level.minimumMoves.ToString() : "?"));
    }

    [MenuItem(Root + "Log Current Run")]
    public static void LogCurrentRun()
    {
        DailyChallengeRunTracker tracker =
            Object.FindAnyObjectByType<DailyChallengeRunTracker>();
        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        LevelManager lm = Object.FindAnyObjectByType<LevelManager>();
        Debug.Log(
            "[DailyChallenge] Current Run\n" +
            "Tracker=" + (tracker != null) + "\n" +
            "IsTiming=" + (tracker != null && tracker.IsTiming) + "\n" +
            "ElapsedMs=" + (tracker != null ? tracker.ElapsedMilliseconds : 0) + "\n" +
            "HasResult=" + (tracker != null && tracker.HasResult) + "\n" +
            "CurrentMoves=" + (gm != null ? gm.CurrentMoves : -1) + "\n" +
            "DailySession=" + (lm != null && lm.IsDailyChallengeSession) + "\n" +
            "CanAcceptInput=" + (gm != null && gm.CanAcceptVehicleInput));
    }

    [MenuItem(Root + "Log Today's Result")]
    public static void LogTodaysResult()
    {
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        manager.EnsureDaySynced();
        if (!manager.TryGetTodaysResult(out DailyChallengeResult result))
        {
            Debug.Log(
                "[DailyChallenge] No completed result for today. state=" +
                manager.CurrentState);
            return;
        }

        Debug.Log(
            "[DailyChallenge] Today's Result\n" +
            "DayId=" + result.DayId + "\n" +
            "Level=" + result.LevelAssetName + "\n" +
            "PoolIndex=" + result.PoolIndex + "\n" +
            "Moves=" + result.Moves + "\n" +
            "TimeMs=" + result.CompletionTimeMilliseconds + "\n" +
            "TimeFmt=" + DailyChallengeTimeFormat.Format(result.CompletionTimeMilliseconds) +
            "\nScore=" + result.Score +
            "\nScoreFmt=" + DailyChallengeTimeFormat.FormatScore(result.Score));
    }

    [MenuItem(Root + "Log Score Examples")]
    public static void LogScoreExamples()
    {
        DailyChallengeConfig config = ResolveConfig();
        int par = 9;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("[DailyChallenge] Score Examples (par/minMoves=" + par + ")");
        sb.AppendLine(
            "BaseMove=" + config.BaseMoveScore +
            " Penalty=" + config.MovePenaltyPerExtraMove +
            " MinMoveScore=" + config.MinimumMoveScore +
            " TimeBonusMax=" + config.TimeBonusMax +
            " TimeRefSec=" + config.TimeReferenceSeconds);
        AppendExample(sb, config, "optimal+30s", par, 30000, par);
        AppendExample(sb, config, "optimal+60s", par, 60000, par);
        AppendExample(sb, config, "opt+1move+60s", par + 1, 60000, par);
        AppendExample(sb, config, "opt+4moves+60s", par + 4, 60000, par);
        AppendExample(sb, config, "opt+20moves+60s", par + 20, 60000, par);
        AppendExample(sb, config, "opt+10min", par, 600000, par);
        AppendExample(sb, config, "opt+1hour", par, 3600000, par);
        AppendExample(sb, config, "manyMoves+slow", par + 40, 3600000, par);
        AppendExample(sb, config, "invalidMinMoves=0", 12, 45000, 0);
        Debug.Log(sb.ToString());
    }

    [MenuItem(Root + "Validate Score Formula")]
    public static void ValidateScoreFormula()
    {
        DailyChallengeConfig config = ResolveConfig();
        int par = 9;
        int a = DailyChallengeScoreCalculator.Calculate(config, par, 30000, par);
        int b = DailyChallengeScoreCalculator.Calculate(config, par, 60000, par);
        int c = DailyChallengeScoreCalculator.Calculate(config, par + 1, 60000, par);
        int d = DailyChallengeScoreCalculator.Calculate(config, par + 4, 60000, par);
        int e = DailyChallengeScoreCalculator.Calculate(config, par + 50, 600000, par);
        int f = DailyChallengeScoreCalculator.Calculate(config, par, 3600000, par);
        int g = DailyChallengeScoreCalculator.Calculate(config, 20, 45000, 0);
        int h1 = DailyChallengeScoreCalculator.Calculate(config, 11, 42000, par);
        int h2 = DailyChallengeScoreCalculator.Calculate(config, 11, 42000, par);

        bool pass =
            a > b && // A faster > slower same moves
            b > c && // B fewer moves > +1 move same time
            (b - c) >= 2000 && // C meaningful +1 move penalty
            e >= 0 && // D many extras valid
            f >= 0 && // E/F long solves valid
            g >= 0 && // G invalid minMoves no crash
            h1 == h2; // H deterministic

        Debug.Log(
            "[DailyChallenge] Validate Score Formula\n" +
            "A opt+30s=" + a + " > opt+60s=" + b + " => " + (a > b) + "\n" +
            "B opt+60s=" + b + " > +1move+60s=" + c + " => " + (b > c) + "\n" +
            "C +1 move delta=" + (b - c) + " (>=2000?) => " + ((b - c) >= 2000) + "\n" +
            "D many extras score=" + e + " >=0 => " + (e >= 0) + "\n" +
            "E/F 10min+/1h scores=" + e + "/" + f + "\n" +
            "G invalid minMoves score=" + g + "\n" +
            "H deterministic " + h1 + "==" + h2 + " => " + (h1 == h2) + "\n" +
            "PASS=" + pass);

        if (!pass)
        {
            Debug.LogError("[DailyChallenge] Score formula validation FAILED.");
        }
    }

    [MenuItem(Root + "Simulate Completed Result")]
    public static void SimulateCompletedResult()
    {
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        manager.EnsureDaySynced();
        LevelData level = manager.SelectedLevel;
        int minMoves = level != null ? Mathf.Max(1, level.minimumMoves) : 9;
        int moves = minMoves + 1;
        long ms = 42380;
        int score = DailyChallengeScoreCalculator.Calculate(
            manager.Config,
            moves,
            ms,
            level != null ? level.minimumMoves : 0);

        DailyChallengeResult result = new DailyChallengeResult
        {
            DayId = manager.CurrentDayId,
            LevelAssetName = level != null ? level.name : manager.SelectedLevelAssetName,
            PoolIndex = manager.SelectedPoolIndex,
            Moves = moves,
            CompletionTimeMilliseconds = ms,
            Score = score,
            Completed = true
        };

        manager.NotifyGameplayCompleted(result);
        Debug.LogWarning(
            "[DailyChallenge] DEV simulated completed result score=" + score +
            " moves=" + moves + " ms=" + ms +
            " (does not mutate lives/campaign).");
        LogTodaysResult();
    }

    [MenuItem(Root + "Clear Today's Result (DEV ONLY)")]
    public static void ClearTodaysResult()
    {
        DailyChallengeLocalStore.ClearResultFields();
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        manager.EditorForceState(DailyChallengeState.FailedOrAbandoned);
        Debug.LogWarning(
            "[DailyChallenge] DEV cleared today's result fields; state=FailedOrAbandoned.");
    }

    [MenuItem(Root + "Simulate Daily Gameplay Mode (log only)")]
    public static void SimulateDailyGameplayMode()
    {
        Debug.Log(
            "[DailyChallenge] Simulate Daily Gameplay Mode (log only)\n" +
            "Policy.IsActive=" + DailyChallengeGameplayPolicy.IsActive + "\n" +
            "SuppressFailures=" + DailyChallengeGameplayPolicy.SuppressPerformanceFailures +
            "\nContextActive=" + DailyChallengeContext.IsActiveSession +
            "\n(Arm via MainMenu START or set Context — does not mutate lives.)");
    }

    [MenuItem(Root + "Reset Today's Daily (DEV ONLY)")]
    public static void ResetToday()
    {
        DailyChallengeManager.EnsureInstance().EditorResetToday();
        Debug.LogWarning("[DailyChallenge] DEV reset — today set Available.");
    }

    [MenuItem(Root + "Set Today Available")]
    public static void SetAvailable()
    {
        DailyChallengeManager.EnsureInstance().EditorForceState(DailyChallengeState.Available);
    }

    [MenuItem(Root + "Set Today InProgress")]
    public static void SetInProgress()
    {
        DailyChallengeManager.EnsureInstance().EditorForceState(DailyChallengeState.InProgress);
    }

    [MenuItem(Root + "Set Today Completed")]
    public static void SetCompleted()
    {
        DailyChallengeManager.EnsureInstance().EditorForceState(DailyChallengeState.Completed);
    }

    [MenuItem(Root + "Set Today Failed")]
    public static void SetFailed()
    {
        DailyChallengeManager.EnsureInstance()
            .EditorForceState(DailyChallengeState.FailedOrAbandoned);
    }

    [MenuItem(Root + "Simulate Next UTC Day")]
    public static void SimulateNextDay()
    {
        DailyChallengeManager.EnsureInstance().EditorSimulateNextUtcDay();
        LogDailyState();
    }

    [MenuItem(Root + "Clear Simulated UTC Clock")]
    public static void ClearSimClock()
    {
        DailyChallengeClock.EditorClearSimulatedUtc();
        DailyChallengeManager.EnsureInstance().EnsureDaySynced(forceNotify: true);
        LogDailyState();
    }

    [MenuItem(Root + "Clear All Daily Prefs (DEV ONLY)")]
    public static void ClearPrefs()
    {
        DailyChallengeLocalStore.ClearAll();
        DailyChallengeContext.ClearSession();
        Debug.LogWarning("[DailyChallenge] DEV cleared all Daily PlayerPrefs + context.");
    }

    private static DailyChallengeConfig ResolveConfig()
    {
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        DailyChallengeConfig config = manager != null ? manager.Config : null;
        if (config == null)
        {
            config = DailyChallengeConfig.LoadDefault();
        }

        return config != null
            ? config
            : ScriptableObject.CreateInstance<DailyChallengeConfig>();
    }

    private static void AppendExample(
        System.Text.StringBuilder sb,
        DailyChallengeConfig config,
        string label,
        int moves,
        long ms,
        int minMoves)
    {
        var b = DailyChallengeScoreCalculator.CalculateBreakdown(config, moves, ms, minMoves);
        sb.AppendLine(
            label +
            " | moves=" + moves +
            " ms=" + ms +
            " ref=" + b.ReferenceMoves +
            " moveScore=" + b.MoveScore +
            " timeBonus=" + b.TimeBonus +
            " TOTAL=" + b.TotalScore +
            " (" + DailyChallengeTimeFormat.FormatScore(b.TotalScore) + ")");
    }
}
#endif

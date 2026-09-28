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

    [MenuItem(Root + "Validate Daily Level Pool")]
    public static void ValidateDailyLevelPool()
    {
        DailyChallengeConfig config = ResolveConfig();
        if (config == null)
        {
            Debug.LogError("[DailyChallengePool] No DailyChallengeConfig.");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("[DailyChallengePool]");
        int total = config.PoolCount;
        int eligible = 0;
        int excludedSpecial = 0;
        int excludedInvalid = 0;

        for (int i = 0; i < total; i++)
        {
            LevelData level = config.GetLevelAt(i);
            bool ok = DailyChallengeLevelEligibility.TryEvaluate(
                level,
                out DailyChallengeLevelEligibility.ExclusionReason reason);
            sb.AppendLine(DailyChallengeLevelEligibility.FormatReason(level, reason));
            if (ok)
            {
                eligible++;
            }
            else if (reason == DailyChallengeLevelEligibility.ExclusionReason.NotClassicObjective)
            {
                excludedSpecial++;
            }
            else
            {
                excludedInvalid++;
            }
        }

        sb.AppendLine();
        sb.AppendLine(
            "RESULT: " + eligible + "/" + total + " eligible | specialExcluded=" +
            excludedSpecial + " | invalidExcluded=" + excludedInvalid);

        string dayId = DailyChallengeClock.GetCurrentUtcDayId();
        if (config.TrySelectEligibleLevelForDay(dayId, out LevelData selected, out int idx))
        {
            bool selectedOk = DailyChallengeLevelEligibility.IsEligible(selected);
            sb.AppendLine(
                "Today select day=" + dayId +
                " asset=" + selected.name +
                " candidateIndex=" + idx +
                " objective=" + selected.objectiveType +
                " minMoves=" + selected.minimumMoves +
                " eligibleCheck=" + selectedOk);
            if (!selectedOk || selected.objectiveType != LevelObjectiveType.Classic)
            {
                sb.AppendLine("ERROR: selected Daily level is not Classic/eligible.");
            }

            // Determinism: same call twice.
            config.TrySelectEligibleLevelForDay(dayId, out LevelData again, out int idx2);
            sb.AppendLine(
                "Deterministic=" +
                (again != null && selected != null && again.name == selected.name && idx == idx2));
        }
        else
        {
            sb.AppendLine(
                "Today select: NONE (zero eligible) — Daily must not start / attempt stays Available.");
        }

        Debug.Log(sb.ToString());
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
        DailyLeaderboardService.InvalidateCache();
        Debug.LogWarning("[DailyChallenge] DEV cleared all Daily PlayerPrefs + context.");
    }

    [MenuItem(Root + "Log Simulated Leaderboard")]
    public static void LogSimulatedLeaderboard()
    {
        DailyLeaderboardService.InvalidateCache();
        DailyLeaderboardSnapshot snap = DailyLeaderboardService.GetTodaysSnapshot();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("[DailyLeaderboard] day=" + snap.DayId + " level=" + snap.LevelAssetName);
        sb.AppendLine("status=" + snap.StatusMessage);
        sb.AppendLine("hasPlayer=" + snap.HasCurrentPlayer + " rank=" + snap.CurrentPlayerRank);
        sb.AppendLine("entries=" + (snap.Entries != null ? snap.Entries.Count : 0));
        int limit = snap.Entries != null ? Mathf.Min(12, snap.Entries.Count) : 0;
        for (int i = 0; i < limit; i++)
        {
            DailyLeaderboardEntry e = snap.Entries[i];
            sb.AppendLine(
                "#" + e.Rank + " " + e.DisplayName +
                " score=" + e.Score +
                " moves=" + e.Moves +
                " ms=" + e.CompletionTimeMilliseconds +
                " sim=" + e.IsSimulated +
                " you=" + e.IsCurrentPlayer);
        }

        if (snap.Entries != null && snap.Entries.Count > limit)
        {
            sb.AppendLine("... +" + (snap.Entries.Count - limit) + " more");
        }

        Debug.Log(sb.ToString());
    }

    [MenuItem(Root + "Log Player Rank")]
    public static void LogPlayerRank()
    {
        if (DailyLeaderboardService.TryGetCurrentPlayerRank(out int rank))
        {
            Debug.Log("[DailyLeaderboard] Player rank #" + rank);
        }
        else
        {
            Debug.Log("[DailyLeaderboard] No player rank (no completed result today).");
        }
    }

    [MenuItem(Root + "Validate Simulated Leaderboard")]
    public static void ValidateSimulatedLeaderboard()
    {
        DailyChallengeConfig config = ResolveConfig();
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        manager.EnsureDaySynced();
        int expectedSim = config.SimulatedLeaderboardPlayerCount;

        DailyLeaderboardService.InvalidateCache();
        DailyLeaderboardSnapshot a = DailyLeaderboardService.GetTodaysSnapshot();
        DailyLeaderboardService.InvalidateCache();
        DailyLeaderboardSnapshot b = DailyLeaderboardService.GetTodaysSnapshot();

        bool pass = true;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("[DailyLeaderboard] Validate");

        int simCount = 0;
        var ids = new System.Collections.Generic.HashSet<string>();
        if (a.Entries == null)
        {
            pass = false;
            sb.AppendLine("FAIL: null entries");
        }
        else
        {
            for (int i = 0; i < a.Entries.Count; i++)
            {
                DailyLeaderboardEntry e = a.Entries[i];
                if (e.IsSimulated)
                {
                    simCount++;
                }

                if (!ids.Add(e.PlayerId))
                {
                    pass = false;
                    sb.AppendLine("FAIL: duplicate id " + e.PlayerId);
                }

                if (e.Score < 0 || e.Moves < a.MinimumMoves || e.CompletionTimeMilliseconds <= 0)
                {
                    pass = false;
                    sb.AppendLine("FAIL: invalid stats @" + e.Rank);
                }

                if (e.IsSimulated != (e.Source == DailyLeaderboardEntrySource.Simulated))
                {
                    pass = false;
                    sb.AppendLine("FAIL: isSimulated/source mismatch @" + e.Rank);
                }

                if (e.Rank != i + 1)
                {
                    pass = false;
                    sb.AppendLine("FAIL: non-sequential rank @" + i);
                }

                if (i > 0)
                {
                    int cmp = SimulatedDailyLeaderboardProvider.CompareEntries(a.Entries[i - 1], e);
                    if (cmp > 0)
                    {
                        pass = false;
                        sb.AppendLine("FAIL: sort order @" + i);
                    }
                }
            }

            int youCount = 0;
            for (int i = 0; i < a.Entries.Count; i++)
            {
                if (a.Entries[i].IsCurrentPlayer)
                {
                    youCount++;
                    if (a.Entries[i].IsSimulated)
                    {
                        pass = false;
                        sb.AppendLine("FAIL: YOU marked simulated");
                    }
                }
            }

            if (youCount > 1)
            {
                pass = false;
                sb.AppendLine("FAIL: YOU inserted more than once");
            }

            int expectedTotal = expectedSim + (a.HasCurrentPlayer ? 1 : 0);
            if (simCount != expectedSim)
            {
                pass = false;
                sb.AppendLine("FAIL: simCount=" + simCount + " expected=" + expectedSim);
            }

            if (a.Entries.Count != expectedTotal)
            {
                pass = false;
                sb.AppendLine("FAIL: total=" + a.Entries.Count + " expected=" + expectedTotal);
            }
        }

        bool identical = SnapshotsIdentical(a, b);
        if (!identical)
        {
            pass = false;
            sb.AppendLine("FAIL: not deterministic across rebuilds");
        }

        // Next day should differ.
        string dayBefore = manager.CurrentDayId;
        manager.EditorSimulateNextUtcDay();
        DailyLeaderboardService.InvalidateCache();
        DailyLeaderboardSnapshot next = DailyLeaderboardService.GetTodaysSnapshot();
        bool differentDay = next.DayId != dayBefore && !SnapshotsIdentical(a, next);
        DailyChallengeClock.EditorClearSimulatedUtc();
        manager.EnsureDaySynced(forceNotify: true);
        DailyLeaderboardService.InvalidateCache();

        if (!differentDay)
        {
            pass = false;
            sb.AppendLine("FAIL: next UTC day did not change snapshot");
        }

        sb.AppendLine("PASS=" + pass);
        if (pass)
        {
            Debug.Log(sb.ToString());
        }
        else
        {
            Debug.LogError(sb.ToString());
        }
    }

    [MenuItem(Root + "Preview Leaderboard With Result")]
    public static void PreviewLeaderboardWithResult()
    {
        SimulateCompletedResult();
        DailyLeaderboardService.InvalidateCache();
        LogSimulatedLeaderboard();
        LogPlayerRank();
    }

    [MenuItem(Root + "Preview Leaderboard Without Result")]
    public static void PreviewLeaderboardWithoutResult()
    {
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        manager.EditorForceState(DailyChallengeState.Available);
        DailyChallengeLocalStore.ClearResultFields();
        DailyLeaderboardService.InvalidateCache();
        LogSimulatedLeaderboard();
        LogPlayerRank();
    }

    // --- Backend (Phase 4.1) ---

    private const string BackendRoot = Root + "Backend/";

    [MenuItem(BackendRoot + "Print Backend Configuration")]
    public static void PrintBackendConfiguration()
    {
        DailyChallengeConfig config = ResolveConfig();
        DailyChallengeFirebaseSettings settings = DailyChallengeFirebaseSettings.LoadDefault();
        Debug.Log(
            "[DailyChallenge Backend]\n" +
            "BackendMode=" + (config != null ? config.BackendMode.ToString() : "?") + "\n" +
            "Authority=" + DailyChallengeAuthority.Current.GetType().Name + "\n" +
            "IsOnlineMode=" + DailyChallengeAuthority.IsOnlineMode + "\n" +
            "ScoreVersion=" + DailyChallengeScoreVersion.Current + "\n" +
            "MinLeaderboardPop=" +
            (config != null ? config.MinimumLeaderboardPopulation.ToString() : "?") + "\n" +
            "FirebaseSettings=" + (settings != null) + "\n" +
            "ProjectId=" + (settings != null ? settings.ProjectId : "") + "\n" +
            "FunctionsRegion=" + (settings != null ? settings.FunctionsRegion : "") + "\n" +
            "CallableBase=" + (settings != null ? settings.GetCallableBaseUrl() : "") + "\n" +
            "UseEmulator=" + (settings != null && settings.UseFunctionsEmulator) + "\n" +
            "ApiKeyConfigured=" + (settings != null && settings.IsConfigured) + "\n" +
            "IdentityReady=" + DailyChallengeIdentityService.IsReady + "\n" +
            "UserIdLen=" +
            (DailyChallengeIdentityService.UserId != null
                ? DailyChallengeIdentityService.UserId.Length.ToString()
                : "0") + "\n" +
            "Mutations=CloudFunctions (client Firestore writes disabled)\n" +
            "NOTE: UID is not printed (privacy).");
    }

    [MenuItem(BackendRoot + "Log Identity")]
    public static async void LogIdentity()
    {
        bool ok = await DailyChallengeIdentityService.EnsureSignedInAsync();
        string uid = DailyChallengeIdentityService.UserId;
        string shortId = string.IsNullOrEmpty(uid)
            ? "(none)"
            : DailyChallengeDisplayName.FromUserId(uid);
        Debug.Log(
            "[DailyChallenge Identity]\n" +
            "SignedIn=" + ok + "\n" +
            "IsReady=" + DailyChallengeIdentityService.IsReady + "\n" +
            "DisplayName=" + shortId + "\n" +
            "UserIdLength=" + (uid != null ? uid.Length : 0) +
            " (full UID not logged)");
    }

    [MenuItem(BackendRoot + "Log Current Server Challenge")]
    public static async void LogCurrentServerChallenge()
    {
        DailyChallengeAuthorityResult r =
            await DailyChallengeAuthority.Current.GetCurrentChallengeAsync();
        Debug.Log(
            "[DailyChallenge Server Challenge]\n" +
            "Success=" + r.Success + "\n" +
            "Error=" + r.ErrorCode + " " + r.ErrorMessage + "\n" +
            "DayId=" + r.Challenge.DayId + "\n" +
            "LevelId=" + r.Challenge.LevelId + "\n" +
            "LevelVersion=" + r.Challenge.LevelVersion + "\n" +
            "ScoreVersion=" + r.Challenge.ScoreVersion + "\n" +
            "Opens=" + r.Challenge.OpensAtUtc + "\n" +
            "Closes=" + r.Challenge.ClosesAtUtc);
    }

    [MenuItem(BackendRoot + "Log Server Attempt State")]
    public static async void LogServerAttemptState()
    {
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        string dayId = manager.CurrentDayId;
        if (string.IsNullOrEmpty(dayId))
        {
            var challenge = await DailyChallengeAuthority.Current.GetCurrentChallengeAsync();
            dayId = challenge.Challenge.DayId;
        }

        DailyChallengeAuthorityResult r =
            await DailyChallengeAuthority.Current.GetAttemptStateAsync(dayId);
        Debug.Log(
            "[DailyChallenge Server Attempt]\n" +
            "DayId=" + dayId + "\n" +
            "Success=" + r.Success + "\n" +
            "Exists=" + r.Attempt.Exists + "\n" +
            "State=" + r.Attempt.State + "\n" +
            "LevelId=" + r.Attempt.LevelId + "\n" +
            "Moves=" + r.Attempt.Moves + "\n" +
            "TimeMs=" + r.Attempt.CompletionTimeMs + "\n" +
            "Score=" + r.Attempt.Score + "\n" +
            "ScoreVersion=" + r.Attempt.ScoreVersion + "\n" +
            "LocalCacheState=" + manager.CurrentState);
    }

    [MenuItem(BackendRoot + "Refresh Online Leaderboard")]
    public static async void RefreshOnlineLeaderboard()
    {
        DailyLeaderboardService.InvalidateCache();
        DailyLeaderboardSnapshot snap =
            await DailyLeaderboardService.RefreshTodaysSnapshotAsync();
        int real = 0;
        int sim = 0;
        if (snap != null && snap.Entries != null)
        {
            for (int i = 0; i < snap.Entries.Count; i++)
            {
                if (snap.Entries[i].IsSimulated)
                {
                    sim++;
                }
                else
                {
                    real++;
                }
            }
        }

        Debug.Log(
            "[DailyChallenge Online Leaderboard]\n" +
            "DayId=" + (snap != null ? snap.DayId : "") + "\n" +
            "Total=" + (snap != null && snap.Entries != null ? snap.Entries.Count : 0) + "\n" +
            "Real=" + real + " Simulated=" + sim + "\n" +
            "HasYou=" + (snap != null && snap.HasCurrentPlayer) + "\n" +
            "Rank=" + (snap != null ? snap.CurrentPlayerRank : 0) + "\n" +
            "Status=" + (snap != null ? snap.StatusMessage : ""));
    }

    [MenuItem(BackendRoot + "Clear LOCAL Daily Cache")]
    public static void ClearLocalDailyCache()
    {
        DailyChallengeLocalStore.ClearAll();
        DailyChallengePendingSubmissionStore.Clear();
        DailyLeaderboardService.InvalidateCache();
        DailyChallengeAuthority.ResetCached();
        HybridDailyLeaderboardCache.Clear();
        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        manager.RefreshFromStoreAndRecover();
        Debug.Log(
            "[DailyChallenge] Cleared LOCAL Daily cache + pending submit. " +
            "Authoritative server attempts were NOT deleted.");
    }

    [MenuItem(BackendRoot + "Log Identity State")]
    public static void LogIdentityState()
    {
        LogIdentity();
    }

    [MenuItem(BackendRoot + "Fetch Server Challenge")]
    public static void FetchServerChallenge()
    {
        LogCurrentServerChallenge();
    }

    [MenuItem(BackendRoot + "Fetch My Attempt State")]
    public static void FetchMyAttemptState()
    {
        LogServerAttemptState();
    }

    [MenuItem(BackendRoot + "Retry Pending Submission")]
    public static async void RetryPendingSubmission()
    {
        if (!DailyChallengePendingSubmissionStore.HasPending)
        {
            Debug.Log("[DailyBackend] No pending submission.");
            return;
        }

        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        await manager.RetryPendingSubmissionAsync();
        Debug.Log(
            "[DailyBackend] Retry finished. HasPending=" +
            DailyChallengePendingSubmissionStore.HasPending +
            " localState=" + manager.CurrentState);
    }

    [MenuItem(BackendRoot + "Export Backend Daily Level Catalog")]
    public static void ExportBackendDailyLevelCatalog()
    {
        DailyChallengeConfig config = ResolveConfig();
        if (config == null)
        {
            Debug.LogError("[DailyBackend] No DailyChallengeConfig.");
            return;
        }

        var levels = new System.Collections.Generic.List<string>();
        for (int i = 0; i < config.PoolCount; i++)
        {
            LevelData level = config.GetLevelAt(i);
            if (!DailyChallengeLevelEligibility.IsEligible(level))
            {
                continue;
            }

            levels.Add(
                "    { \"levelId\": \"" + level.name +
                "\", \"levelVersion\": \"" + level.name +
                "\", \"minimumMoves\": " + level.minimumMoves + " }");
        }

        string json =
            "{\n" +
            "  \"scoreVersion\": " + DailyChallengeScoreVersion.Current + ",\n" +
            "  \"scoring\": {\n" +
            "    \"baseMoveScore\": " + config.BaseMoveScore + ",\n" +
            "    \"movePenaltyPerExtraMove\": " + config.MovePenaltyPerExtraMove + ",\n" +
            "    \"minimumMoveScore\": " + config.MinimumMoveScore + ",\n" +
            "    \"timeBonusMax\": " + config.TimeBonusMax + ",\n" +
            "    \"timeReferenceSeconds\": " +
            config.TimeReferenceSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            "\n" +
            "  },\n" +
            "  \"validation\": {\n" +
            "    \"minMoves\": 1,\n" +
            "    \"maxMoves\": 5000,\n" +
            "    \"minCompletionTimeMs\": 500,\n" +
            "    \"maxCompletionTimeMs\": 21600000\n" +
            "  },\n" +
            "  \"levels\": [\n" +
            string.Join(",\n", levels) + "\n" +
            "  ],\n" +
            "  \"notes\": \"Exported from eligible Classic Daily pool. Deploy functions/config/daily-levels.json.\"\n" +
            "}\n";

        string projectRoot = System.IO.Path.GetFullPath(
            System.IO.Path.Combine(Application.dataPath, ".."));
        string[] targets =
        {
            System.IO.Path.Combine(
                projectRoot, "Backend", "DailyChallenge", "config", "daily-levels.json"),
            System.IO.Path.Combine(
                projectRoot, "Backend", "DailyChallenge", "functions", "config",
                "daily-levels.json")
        };

        foreach (string path in targets)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            System.IO.File.WriteAllText(path, json);
        }

        AssetDatabase.Refresh();
        Debug.Log(
            "[DailyBackend] Exported " + levels.Count +
            " eligible levels to Backend/DailyChallenge/config and functions/config.");
    }

    [MenuItem(BackendRoot + "Compare Local vs Server Score Test Vectors")]
    public static void CompareLocalVsServerScoreTestVectors()
    {
        DailyChallengeConfig config = ResolveConfig();
        // Mirror Backend/DailyChallenge/config/score-vectors-v1.json
        var vectors = new[]
        {
            new { name = "par_fast", min = 9, moves = 9, ms = 30000L, expected = 113333 },
            new { name = "par_60s", min = 9, moves = 9, ms = 60000L, expected = 110000 },
            new { name = "plus1_60s", min = 9, moves = 10, ms = 60000L, expected = 107500 },
            new { name = "plus5_60s", min = 9, moves = 14, ms = 60000L, expected = 97500 },
            new { name = "very_slow", min = 9, moves = 9, ms = 600000L, expected = 101818 },
            new { name = "large_moves", min = 9, moves = 100, ms = 60000L, expected = 20000 },
            new { name = "min_move_floor", min = 3, moves = 50, ms = 60000L, expected = 20000 },
            new { name = "round_half_even_check", min = 9, moves = 9, ms = 45000L, expected = 111429 },
            new { name = "par_1s", min = 3, moves = 3, ms = 1000L, expected = 119672 },
            new { name = "boundary_500ms", min = 6, moves = 6, ms = 500L, expected = 119835 }
        };

        int fail = 0;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[DailyBackend] Unity Score V1 vs expected vectors (must match server)");
        for (int i = 0; i < vectors.Length; i++)
        {
            var v = vectors[i];
            int score = DailyChallengeScoreCalculator.Calculate(
                config, v.moves, v.ms, v.min);
            bool ok = score == v.expected;
            if (!ok)
            {
                fail++;
            }

            sb.AppendLine(
                (ok ? "PASS" : "FAIL") + " " + v.name +
                " got=" + score + " expected=" + v.expected);
        }

        sb.AppendLine("FAIL_COUNT=" + fail);
        if (fail > 0)
        {
            Debug.LogError(sb.ToString());
        }
        else
        {
            Debug.Log(sb.ToString());
        }
    }

    private static bool SnapshotsIdentical(DailyLeaderboardSnapshot a, DailyLeaderboardSnapshot b)
    {
        if (a == null || b == null || a.Entries == null || b.Entries == null)
        {
            return false;
        }

        if (a.Entries.Count != b.Entries.Count || a.DayId != b.DayId)
        {
            return false;
        }

        for (int i = 0; i < a.Entries.Count; i++)
        {
            DailyLeaderboardEntry x = a.Entries[i];
            DailyLeaderboardEntry y = b.Entries[i];
            if (x.PlayerId != y.PlayerId ||
                x.DisplayName != y.DisplayName ||
                x.Score != y.Score ||
                x.Moves != y.Moves ||
                x.CompletionTimeMilliseconds != y.CompletionTimeMilliseconds ||
                x.Rank != y.Rank)
            {
                return false;
            }
        }

        return true;
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

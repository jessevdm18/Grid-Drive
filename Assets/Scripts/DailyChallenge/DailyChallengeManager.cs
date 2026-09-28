using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Daily Challenge domain service.
/// Online mode: Firebase authority for attempts/results. Local PlayerPrefs is cache only.
/// </summary>
[DefaultExecutionOrder(-80)]
public class DailyChallengeManager : MonoBehaviour
{
    public static DailyChallengeManager Instance { get; private set; }

    [SerializeField] private DailyChallengeConfig config;

    private string cachedDayId;
    private DailyChallengeState cachedState = DailyChallengeState.Available;
    private string cachedLevelAssetName = string.Empty;
    private int cachedPoolIndex = -1;
    private LevelData cachedLevel;
    private DailyChallengeDescriptor cachedOnlineChallenge;
    private bool startInFlight;

    public event Action OnDailyStateChanged;

    public DailyChallengeConfig Config =>
        config != null ? config : (config = DailyChallengeConfig.LoadDefault());

    public string CurrentDayId => cachedDayId;

    public DailyChallengeState CurrentState => cachedState;

    public bool IsAttemptAvailable =>
        EnsureDaySynced() &&
        cachedState == DailyChallengeState.Available &&
        (!RequiresOnlineToStart || IsOnlineReady);

    public bool IsAttemptUsed =>
        EnsureDaySynced() && cachedState != DailyChallengeState.Available;

    public bool RequiresOnlineToStart =>
        Config != null && Config.BackendMode == DailyChallengeBackendMode.Online;

    public bool NeedsConnectToPlay =>
        RequiresOnlineToStart &&
        !IsOnlineReady &&
        cachedState == DailyChallengeState.Available;

    public string LastBackendError { get; private set; } = string.Empty;

    public bool IsOnlineReady { get; private set; }

    public LevelData SelectedLevel => cachedLevel;

    public string SelectedLevelAssetName => cachedLevelAssetName;

    public int SelectedPoolIndex => cachedPoolIndex;

    public static DailyChallengeManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        Instance = FindAnyObjectByType<DailyChallengeManager>();
        if (Instance != null)
        {
            return Instance;
        }

        GameObject go = new GameObject("DailyChallengeManager");
        Instance = go.AddComponent<DailyChallengeManager>();
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

        if (config == null)
        {
            config = DailyChallengeConfig.LoadDefault();
        }

        RefreshFromStoreAndRecover();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            // Returning from background — catch UTC midnight rollover.
            EnsureDaySynced(forceNotify: true);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Phase 1: no resume. If we land on MainMenu while InProgress without an
        // active Gameplay session, the attempt was abandoned / force-closed.
        if (scene.name == "MainMenu")
        {
            RecoverStaleInProgressIfNeeded();
            EnsureDaySynced(forceNotify: true);
        }
    }

    /// <summary>
    /// Ensures cached state matches the current UTC day. Rolls over to Available on new day.
    /// </summary>
    public bool EnsureDaySynced(bool forceNotify = false)
    {
        string today = DailyChallengeClock.GetCurrentUtcDayId();
        bool changed = false;

        if (!string.Equals(cachedDayId, today, StringComparison.Ordinal))
        {
            BeginNewDay(today);
            changed = true;
        }

        if (forceNotify || changed)
        {
            OnDailyStateChanged?.Invoke();
        }

        return true;
    }

    public TimeSpan GetTimeUntilNextChallenge()
    {
        EnsureDaySynced();
        return DailyChallengeClock.GetTimeUntilNextUtcDay();
    }

    public string FormatCountdown()
    {
        TimeSpan t = GetTimeUntilNextChallenge();
        return string.Format(
            "{0:00}:{1:00}:{2:00}",
            (int)t.TotalHours,
            t.Minutes,
            t.Seconds);
    }

    /// <summary>
    /// Local/SimulatedLocal start path. Prefer <see cref="TryCommitStartAndArmGameplayAsync"/>
    /// so Online mode can atomically claim the attempt first.
    /// </summary>
    public bool TryCommitStartAndArmGameplay()
    {
        if (RequiresOnlineToStart)
        {
            Debug.LogError(
                "[DailyChallenge] Online mode requires TryCommitStartAndArmGameplayAsync.");
            return false;
        }

        return CommitLocalStart();
    }

    /// <summary>
    /// Authoritative start: claim attempt (Online) THEN arm Gameplay. Never load Gameplay first.
    /// </summary>
    public async Task<bool> TryCommitStartAndArmGameplayAsync()
    {
        if (startInFlight)
        {
            return false;
        }

        startInFlight = true;
        LastBackendError = string.Empty;
        try
        {
            EnsureDaySynced();
            if (cachedState != DailyChallengeState.Available)
            {
                LastBackendError = "Attempt already used.";
                return false;
            }

            if (!RequiresOnlineToStart)
            {
                return CommitLocalStart();
            }

            DailyChallengeAuthorityResult challengeResult =
                await DailyChallengeAuthority.Current.GetCurrentChallengeAsync();
            if (!challengeResult.Success || !challengeResult.Challenge.IsValid)
            {
                LastBackendError = string.IsNullOrEmpty(challengeResult.ErrorMessage)
                    ? "CONNECT TO PLAY"
                    : challengeResult.ErrorMessage;
                IsOnlineReady = false;
                OnDailyStateChanged?.Invoke();
                return false;
            }

            cachedOnlineChallenge = challengeResult.Challenge;
            cachedDayId = challengeResult.Challenge.DayId;
            ApplyOnlineLevel(challengeResult.Challenge.LevelId);

            if (cachedLevel == null ||
                !DailyChallengeLevelEligibility.IsEligible(cachedLevel))
            {
                LastBackendError = "Daily level invalid on this client build.";
                return false;
            }

            DailyChallengeAuthorityResult claim =
                await DailyChallengeAuthority.Current.TryStartAttemptAsync(
                    challengeResult.Challenge);
            if (!claim.Success)
            {
                ApplyServerAttemptToLocalCache(claim.Attempt);
                LastBackendError = string.IsNullOrEmpty(claim.ErrorMessage)
                    ? "Attempt already used."
                    : claim.ErrorMessage;
                OnDailyStateChanged?.Invoke();
                return false;
            }

            // Claim succeeded — only now mark local InProgress + arm Gameplay.
            cachedState = DailyChallengeState.InProgress;
            Persist();
            DailyChallengeContext.ArmPending(cachedLevel);
            IsOnlineReady = true;

            GameAnalytics.LogDailyChallengeStart(
                cachedDayId,
                cachedLevelAssetName,
                cachedPoolIndex);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                "[DailyChallenge] Online start claimed day=" + cachedDayId +
                " level=" + cachedLevelAssetName);
#endif
            OnDailyStateChanged?.Invoke();
            return true;
        }
        finally
        {
            startInFlight = false;
        }
    }

    /// <summary>Sync local presentation cache from Online authority (MainMenu).</summary>
    public async Task SyncFromAuthorityAsync()
    {
        if (!RequiresOnlineToStart)
        {
            IsOnlineReady = true;
            await RetryPendingSubmissionAsync();
            return;
        }

        LastBackendError = string.Empty;
        bool signedIn = await DailyChallengeIdentityService.EnsureSignedInAsync();
        if (!signedIn)
        {
            IsOnlineReady = false;
            LastBackendError = "CONNECT TO PLAY";
            OnDailyStateChanged?.Invoke();
            return;
        }

        DailyChallengeAuthorityResult challengeResult =
            await DailyChallengeAuthority.Current.GetCurrentChallengeAsync();
        if (!challengeResult.Success)
        {
            IsOnlineReady = false;
            LastBackendError = "CONNECT TO PLAY";
            OnDailyStateChanged?.Invoke();
            return;
        }

        cachedOnlineChallenge = challengeResult.Challenge;
        cachedDayId = challengeResult.Challenge.DayId;
        ApplyOnlineLevel(challengeResult.Challenge.LevelId);

        DailyChallengeAuthorityResult attemptResult =
            await DailyChallengeAuthority.Current.GetAttemptStateAsync(cachedDayId);
        if (attemptResult.Success)
        {
            ApplyServerAttemptToLocalCache(attemptResult.Attempt);
        }

        IsOnlineReady = true;
        await RetryPendingSubmissionAsync();
        DailyLeaderboardService.InvalidateCache();
        _ = DailyLeaderboardService.RefreshTodaysSnapshotAsync();
        OnDailyStateChanged?.Invoke();
    }

    public void NotifyGameplayCompleted(DailyChallengeResult result)
    {
        EnsureDaySynced();
        if (cachedState == DailyChallengeState.Completed && result.Completed)
        {
            return;
        }

        cachedState = DailyChallengeState.Completed;
        cachedLevelAssetName = result.LevelAssetName ?? cachedLevelAssetName;
        cachedPoolIndex = result.PoolIndex >= 0 ? result.PoolIndex : cachedPoolIndex;
        Persist();
        DailyChallengeLocalStore.SaveResult(result);
        DailyLeaderboardService.InvalidateCache();

        GameAnalytics.LogDailyChallengeComplete(
            result.DayId,
            result.LevelAssetName,
            result.Moves,
            result.CompletionTimeMilliseconds,
            result.Score);

        if (RequiresOnlineToStart)
        {
            // Provisional local score for immediate UI; server recalculates authoritatively.
            DailyChallengePendingSubmissionStore.Save(
                result.DayId,
                result.LevelAssetName,
                result.Moves,
                result.CompletionTimeMilliseconds,
                result.Score);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[DailyBackend] Result pending (provisional score=" + result.Score + ")");
#endif
            _ = SubmitCompletedResultAsync(result);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[DailyChallenge] Completed score=" + result.Score +
            " moves=" + result.Moves +
            " ms=" + result.CompletionTimeMilliseconds +
            " (context kept until ResultUI CONTINUE)");
#endif
        OnDailyStateChanged?.Invoke();
    }

    /// <summary>Abandoned / unsolved exit — no score.</summary>
    public void NotifyAbandoned(string reason = "abandon")
    {
        EnsureDaySynced();
        if (cachedState == DailyChallengeState.Completed)
        {
            DailyChallengeContext.ClearSession();
            return;
        }

        if (cachedState == DailyChallengeState.InProgress ||
            DailyChallengeContext.IsActiveSession)
        {
            cachedState = DailyChallengeState.FailedOrAbandoned;
            Persist();
            DailyChallengeLocalStore.ClearResultFields();
            DailyLeaderboardService.InvalidateCache();
            GameAnalytics.LogDailyChallengeAbandon(
                cachedDayId,
                cachedLevelAssetName,
                reason);

            if (RequiresOnlineToStart && !string.IsNullOrEmpty(cachedDayId))
            {
                _ = DailyChallengeAuthority.Current.MarkAbandonedAsync(cachedDayId);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[DailyChallenge] Abandoned reason=" + reason);
#endif
        }

        DailyChallengeContext.ClearSession();
        OnDailyStateChanged?.Invoke();
    }

    private bool CommitLocalStart()
    {
        EnsureDaySynced();

        if (cachedState != DailyChallengeState.Available)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(
                "[DailyChallenge] Start rejected — attempt already used. state=" + cachedState);
#endif
            return false;
        }

        DailyChallengeConfig cfg = Config;
        if (cfg == null || cfg.PoolCount <= 0)
        {
            Debug.LogError("[DailyChallenge] No DailyChallengeConfig / empty pool.");
            return false;
        }

        if (!cfg.HasEligibleLevels)
        {
            Debug.LogError(
                "[DailyChallenge] No eligible normal (Classic) levels in Daily pool. " +
                "Special/objective levels are not allowed. Attempt NOT consumed.");
            return false;
        }

        ResolveSelectedLevel(cfg, cachedDayId, persistSelection: true);
        if (cachedLevel == null ||
            !DailyChallengeLevelEligibility.IsEligible(cachedLevel))
        {
            Debug.LogError(
                "[DailyChallenge] Could not resolve an eligible Daily level for " +
                cachedDayId + ". Attempt NOT consumed.");
            return false;
        }

        cachedState = DailyChallengeState.InProgress;
        Persist();
        DailyChallengeContext.ArmPending(cachedLevel);

        GameAnalytics.LogDailyChallengeStart(
            cachedDayId,
            cachedLevelAssetName,
            cachedPoolIndex);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[DailyChallenge] Start committed InProgress day=" + cachedDayId +
            " level=" + cachedLevelAssetName + " poolIndex=" + cachedPoolIndex);
#endif

        OnDailyStateChanged?.Invoke();
        return true;
    }

    private void ApplyOnlineLevel(string levelId)
    {
        DailyChallengeConfig cfg = Config;
        cachedLevel = DailyChallengeContext.FindInPool(cfg, levelId);
        cachedLevelAssetName = cachedLevel != null ? cachedLevel.name : (levelId ?? string.Empty);
        cachedPoolIndex = -1;
        if (cfg != null && cachedLevel != null)
        {
            for (int i = 0; i < cfg.PoolCount; i++)
            {
                LevelData entry = cfg.GetLevelAt(i);
                if (entry != null && entry.name == cachedLevelAssetName)
                {
                    cachedPoolIndex = i;
                    break;
                }
            }
        }

        Persist();
    }

    private void ApplyServerAttemptToLocalCache(DailyChallengeAttemptRecord attempt)
    {
        if (!attempt.Exists)
        {
            // Server has no attempt — do NOT restore a local InProgress as Available.
            if (cachedState == DailyChallengeState.InProgress)
            {
                cachedState = DailyChallengeState.FailedOrAbandoned;
                Persist();
            }

            return;
        }

        switch (attempt.State)
        {
            case DailyChallengeServerAttemptState.Started:
                // Force-close / unfinished — attempt consumed, no score.
                cachedState = DailyChallengeState.FailedOrAbandoned;
                DailyChallengeLocalStore.ClearResultFields();
                break;
            case DailyChallengeServerAttemptState.Completed:
                cachedState = DailyChallengeState.Completed;
                if (attempt.Score > 0 || attempt.Moves > 0)
                {
                    DailyChallengeLocalStore.SaveResult(new DailyChallengeResult
                    {
                        DayId = attempt.DayId,
                        LevelAssetName = attempt.LevelId,
                        PoolIndex = cachedPoolIndex,
                        Moves = attempt.Moves,
                        CompletionTimeMilliseconds = attempt.CompletionTimeMs,
                        Score = attempt.Score,
                        Completed = true
                    });
                }

                break;
            case DailyChallengeServerAttemptState.Abandoned:
                cachedState = DailyChallengeState.FailedOrAbandoned;
                DailyChallengeLocalStore.ClearResultFields();
                break;
        }

        Persist();
    }

    private async Task SubmitCompletedResultAsync(DailyChallengeResult result)
    {
        DailyChallengeDescriptor challenge = cachedOnlineChallenge;
        if (!challenge.IsValid)
        {
            challenge = new DailyChallengeDescriptor
            {
                DayId = result.DayId,
                LevelId = result.LevelAssetName,
                LevelVersion = result.LevelAssetName,
                ScoreVersion = DailyChallengeScoreVersion.Current
            };
        }

        DailyChallengeAuthorityResult submit =
            await DailyChallengeAuthority.Current.SubmitResultAsync(
                challenge,
                result.Moves,
                result.CompletionTimeMilliseconds,
                result.Score);

        if (submit.Success)
        {
            ApplyAuthoritativeCompletedResult(submit);
            DailyChallengePendingSubmissionStore.Clear();
            DailyLeaderboardService.InvalidateCache();
            _ = DailyLeaderboardService.RefreshTodaysSnapshotAsync();
        }
        else
        {
            // Keep pending for retry. Sync authoritative state if attempt invalid.
            if (submit.Attempt.Exists &&
                submit.Attempt.State != DailyChallengeServerAttemptState.Started)
            {
                ApplyServerAttemptToLocalCache(submit.Attempt);
                if (submit.Attempt.State != DailyChallengeServerAttemptState.Completed)
                {
                    DailyChallengePendingSubmissionStore.Clear();
                }
            }

            GameAnalytics.LogDailyResultSubmitError(
                result.DayId,
                submit.ErrorCode ?? "submit_failed");
        }
    }

    public async Task RetryPendingSubmissionAsync()
    {
        if (!RequiresOnlineToStart)
        {
            return;
        }

        if (!DailyChallengePendingSubmissionStore.TryLoad(
                out string dayId,
                out string levelId,
                out int moves,
                out long ms,
                out int provisionalScore))
        {
            return;
        }

        GameAnalytics.LogDailyResultSubmitRetry(dayId);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[DailyBackend] Result submit retry day=" + dayId);
#endif
        var challenge = new DailyChallengeDescriptor
        {
            DayId = dayId,
            LevelId = levelId,
            LevelVersion = levelId,
            ScoreVersion = DailyChallengeScoreVersion.Current
        };

        DailyChallengeAuthorityResult submit =
            await DailyChallengeAuthority.Current.SubmitResultAsync(
                challenge,
                moves,
                ms,
                provisionalScore);
        if (submit.Success)
        {
            ApplyAuthoritativeCompletedResult(submit);
            DailyChallengePendingSubmissionStore.Clear();
            DailyLeaderboardService.InvalidateCache();
            _ = DailyLeaderboardService.RefreshTodaysSnapshotAsync();
            OnDailyStateChanged?.Invoke();
        }
        else if (submit.Attempt.Exists &&
                 submit.Attempt.State != DailyChallengeServerAttemptState.Started)
        {
            ApplyServerAttemptToLocalCache(submit.Attempt);
            if (submit.Attempt.State != DailyChallengeServerAttemptState.Completed)
            {
                DailyChallengePendingSubmissionStore.Clear();
            }

            OnDailyStateChanged?.Invoke();
        }
    }

    private void ApplyAuthoritativeCompletedResult(DailyChallengeAuthorityResult submit)
    {
        if (!submit.Attempt.Exists ||
            submit.Attempt.State != DailyChallengeServerAttemptState.Completed)
        {
            return;
        }

        cachedState = DailyChallengeState.Completed;
        cachedDayId = string.IsNullOrEmpty(submit.Challenge.DayId)
            ? cachedDayId
            : submit.Challenge.DayId;
        cachedLevelAssetName = string.IsNullOrEmpty(submit.Attempt.LevelId)
            ? cachedLevelAssetName
            : submit.Attempt.LevelId;
        Persist();
        DailyChallengeLocalStore.SaveResult(new DailyChallengeResult
        {
            DayId = cachedDayId,
            LevelAssetName = cachedLevelAssetName,
            PoolIndex = cachedPoolIndex,
            Moves = submit.Attempt.Moves,
            CompletionTimeMilliseconds = submit.Attempt.CompletionTimeMs,
            Score = submit.Attempt.Score,
            Completed = true
        });

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[DailyBackend] Local cache updated from authoritative score=" +
            submit.Attempt.Score + " trusted=" + submit.Attempt.ScoreTrusted);
#endif
    }

    public bool TryGetTodaysResult(out DailyChallengeResult result)
    {
        EnsureDaySynced();
        if (cachedState != DailyChallengeState.Completed)
        {
            result = default;
            return false;
        }

        return DailyChallengeLocalStore.TryLoadResult(out result);
    }

    public void RefreshFromStoreAndRecover()
    {
        if (DailyChallengeLocalStore.TryLoad(
                out string dayId,
                out DailyChallengeState state,
                out string levelAsset,
                out int poolIndex))
        {
            cachedDayId = dayId;
            cachedState = state;
            cachedLevelAssetName = levelAsset;
            cachedPoolIndex = poolIndex;
        }
        else
        {
            cachedDayId = null;
        }

        EnsureDaySynced();
        RecoverStaleInProgressIfNeeded();
        ResolveSelectedLevel(Config, cachedDayId, persistSelection: false);
    }

    private void RecoverStaleInProgressIfNeeded()
    {
        if (cachedState != DailyChallengeState.InProgress)
        {
            return;
        }

        // Still launching into Gameplay — keep InProgress.
        if (DailyChallengeContext.HasPending)
        {
            return;
        }

        // Actively playing in Gameplay — keep InProgress.
        Scene active = SceneManager.GetActiveScene();
        if (active.IsValid() &&
            active.name == "Gameplay" &&
            DailyChallengeContext.IsActiveSession)
        {
            return;
        }

        // Stale InProgress (force-close / leftover) — attempt used.
        cachedState = DailyChallengeState.FailedOrAbandoned;
        Persist();
        DailyChallengeContext.ClearSession();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[DailyChallenge] Recovered stale InProgress → FailedOrAbandoned day=" +
            cachedDayId);
#endif
    }

    private void BeginNewDay(string dayId)
    {
        cachedDayId = dayId;
        cachedState = DailyChallengeState.Available;
        cachedLevelAssetName = string.Empty;
        cachedPoolIndex = -1;
        cachedLevel = null;
        DailyChallengeLocalStore.ClearResultFields();
        DailyLeaderboardService.InvalidateCache();
        ResolveSelectedLevel(Config, dayId, persistSelection: true);
        Persist();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[DailyChallenge] New UTC day=" + dayId +
            " level=" + cachedLevelAssetName +
            " poolIndex=" + cachedPoolIndex);
#endif
    }

    private void ResolveSelectedLevel(
        DailyChallengeConfig cfg,
        string dayId,
        bool persistSelection)
    {
        cachedLevel = null;
        if (cfg == null || string.IsNullOrEmpty(dayId))
        {
            cachedLevelAssetName = string.Empty;
            cachedPoolIndex = -1;
            return;
        }

        if (!string.IsNullOrEmpty(cachedLevelAssetName))
        {
            cachedLevel = DailyChallengeContext.FindInPool(cfg, cachedLevelAssetName);
        }

        // Persisted selection may be a special level from older builds.
        if (cachedLevel != null &&
            !DailyChallengeLevelEligibility.IsEligible(cachedLevel))
        {
            if (cachedState == DailyChallengeState.Available)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning(
                    "[DailyChallenge] Persisted selection ineligible (" +
                    cachedLevelAssetName + " / " + cachedLevel.objectiveType +
                    "). Reselecting from eligible Classic pool (attempt still Available).");
#endif
                cachedLevel = null;
                cachedLevelAssetName = string.Empty;
                cachedPoolIndex = -1;
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning(
                    "[DailyChallenge] Persisted selection ineligible (" +
                    cachedLevelAssetName + " / " + cachedLevel.objectiveType +
                    ") but state=" + cachedState +
                    " — NOT resetting attempt / NOT reselecting.");
#endif
                // Keep identity for history; do not arm/start from this path.
            }
        }

        if (cachedLevel == null && cachedState == DailyChallengeState.Available)
        {
            if (cfg.TrySelectEligibleLevelForDay(
                    dayId,
                    out LevelData selected,
                    out int candidateIndex))
            {
                cachedLevel = selected;
                cachedPoolIndex = candidateIndex;
                cachedLevelAssetName = selected != null ? selected.name : string.Empty;
            }
            else
            {
                cachedLevel = null;
                cachedPoolIndex = -1;
                cachedLevelAssetName = string.Empty;
            }
        }
        else if (cachedLevel != null &&
                 cachedPoolIndex < 0 &&
                 DailyChallengeLevelEligibility.IsEligible(cachedLevel))
        {
            // Recover candidate index for an eligible persisted asset.
            for (int i = 0; i < cfg.PoolCount; i++)
            {
                LevelData entry = cfg.GetLevelAt(i);
                if (entry != null && entry.name == cachedLevelAssetName)
                {
                    cachedPoolIndex = i;
                    break;
                }
            }
        }

        if (persistSelection)
        {
            Persist();
        }
    }

    public bool HasEligibleDailyLevel()
    {
        DailyChallengeConfig cfg = Config;
        return cfg != null && cfg.HasEligibleLevels;
    }

    private void Persist()
    {
        DailyChallengeLocalStore.Save(
            cachedDayId,
            cachedState,
            cachedLevelAssetName,
            cachedPoolIndex);
    }

#if UNITY_EDITOR
    public void EditorForceState(DailyChallengeState state)
    {
        EnsureDaySynced();
        cachedState = state;
        Persist();
        if (state == DailyChallengeState.Available)
        {
            DailyChallengeContext.ClearSession();
        }

        DailyLeaderboardService.InvalidateCache();
        OnDailyStateChanged?.Invoke();
    }

    public void EditorResetToday()
    {
        BeginNewDay(DailyChallengeClock.GetCurrentUtcDayId());
        DailyChallengeContext.ClearSession();
        OnDailyStateChanged?.Invoke();
    }

    public void EditorSimulateNextUtcDay()
    {
        DateTime next = DailyChallengeClock.GetNextUtcMidnight().AddMinutes(1);
        DailyChallengeClock.EditorSetSimulatedUtc(next);
        EnsureDaySynced(forceNotify: true);
    }
#endif
}

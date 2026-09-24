using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Daily Challenge domain service. UI queries this — not PlayerPrefs.
/// Local persistence is Phase 1 development only; backend will replace authority later.
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

    public event Action OnDailyStateChanged;

    public DailyChallengeConfig Config =>
        config != null ? config : (config = DailyChallengeConfig.LoadDefault());

    public string CurrentDayId => cachedDayId;

    public DailyChallengeState CurrentState => cachedState;

    public bool IsAttemptAvailable =>
        EnsureDaySynced() && cachedState == DailyChallengeState.Available;

    public bool IsAttemptUsed =>
        EnsureDaySynced() && cachedState != DailyChallengeState.Available;

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
    /// Commits today's attempt (InProgress), persists immediately, arms Gameplay context.
    /// Call ONLY from confirmation START — not from the entry card Play button.
    /// Returns false if unavailable / no level.
    /// </summary>
    public bool TryCommitStartAndArmGameplay()
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

        ResolveSelectedLevel(cfg, cachedDayId, persistSelection: true);
        if (cachedLevel == null)
        {
            Debug.LogError("[DailyChallenge] Could not resolve daily level for " + cachedDayId);
            return false;
        }

        // Persist InProgress BEFORE scene transition (force-close must not refund).
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

    public void NotifyGameplayCompleted(DailyChallengeResult result)
    {
        EnsureDaySynced();
        if (cachedState == DailyChallengeState.Completed && result.Completed)
        {
            // Duplicate completion callback — keep first persisted result.
            DailyChallengeContext.ClearSession();
            return;
        }

        cachedState = DailyChallengeState.Completed;
        cachedLevelAssetName = result.LevelAssetName ?? cachedLevelAssetName;
        cachedPoolIndex = result.PoolIndex >= 0 ? result.PoolIndex : cachedPoolIndex;
        Persist();
        DailyChallengeLocalStore.SaveResult(result);

        GameAnalytics.LogDailyChallengeComplete(
            result.DayId,
            result.LevelAssetName,
            result.Moves,
            result.CompletionTimeMilliseconds,
            result.Score);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[DailyChallenge] Completed score=" + result.Score +
            " moves=" + result.Moves +
            " ms=" + result.CompletionTimeMilliseconds);
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
            GameAnalytics.LogDailyChallengeAbandon(
                cachedDayId,
                cachedLevelAssetName,
                reason);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[DailyChallenge] Abandoned reason=" + reason);
#endif
        }

        DailyChallengeContext.ClearSession();
        OnDailyStateChanged?.Invoke();
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

        if (cachedLevel == null)
        {
            cachedPoolIndex = cfg.GetStableIndexForDay(dayId);
            cachedLevel = cfg.GetLevelAt(cachedPoolIndex);
            cachedLevelAssetName = cachedLevel != null ? cachedLevel.name : string.Empty;
        }
        else if (cachedPoolIndex < 0)
        {
            cachedPoolIndex = cfg.GetStableIndexForDay(dayId);
        }

        if (persistSelection)
        {
            Persist();
        }
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

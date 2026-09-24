using UnityEngine;

/// <summary>
/// Monotonic Daily Challenge run stopwatch + result build.
/// Timing starts when the player can first make a valid move (CanAcceptVehicleInput).
/// Timing stops at the authoritative CompleteLevel acceptance point (before win UI).
/// </summary>
public class DailyChallengeRunTracker : MonoBehaviour
{
    public static DailyChallengeRunTracker Instance { get; private set; }

    private GameManager gameManager;
    private LevelManager levelManager;
    private bool waitingForInputReady;
    private bool timing;
    private bool stopped;
    private float startUnscaledTime;
    private float stopUnscaledTime;
    private DailyChallengeResult lastResult;
    private bool hasResult;

    public bool IsTiming => timing && !stopped;

    public long ElapsedMilliseconds
    {
        get
        {
            if (!timing && !stopped)
            {
                return 0;
            }

            float end = stopped ? stopUnscaledTime : Time.unscaledTime;
            return (long)Mathf.Max(0f, (end - startUnscaledTime) * 1000f);
        }
    }

    public bool HasResult => hasResult;

    public DailyChallengeResult LastResult => lastResult;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        levelManager = FindAnyObjectByType<LevelManager>();

        if (!DailyChallengeGameplayPolicy.IsActive)
        {
            enabled = false;
            return;
        }

        waitingForInputReady = true;
        timing = false;
        stopped = false;
        hasResult = false;
    }

    private void Update()
    {
        if (!waitingForInputReady || timing || stopped)
        {
            return;
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        if (gameManager != null && gameManager.CanAcceptVehicleInput)
        {
            BeginTiming();
        }
    }

    private void BeginTiming()
    {
        waitingForInputReady = false;
        timing = true;
        stopped = false;
        startUnscaledTime = Time.unscaledTime;
        stopUnscaledTime = startUnscaledTime;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[DailyChallenge] Run timer started (input ready)");
#endif
    }

    /// <summary>
    /// Freeze timer and build result. Call once from CompleteLevel (Daily path).
    /// </summary>
    public DailyChallengeResult StopAndBuildResult()
    {
        if (hasResult)
        {
            return lastResult;
        }

        if (timing && !stopped)
        {
            stopUnscaledTime = Time.unscaledTime;
            stopped = true;
            timing = false;
        }
        else if (!timing && !stopped)
        {
            // Solved before input-ready edge case — zero duration.
            startUnscaledTime = Time.unscaledTime;
            stopUnscaledTime = startUnscaledTime;
            stopped = true;
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        DailyChallengeManager manager = DailyChallengeManager.EnsureInstance();
        LevelData data = levelManager != null ? levelManager.CurrentLevelData : null;
        int moves = gameManager != null ? gameManager.CurrentMoves : 0;
        long ms = ElapsedMilliseconds;
        int minMoves = data != null ? data.minimumMoves : 0;

        int score = DailyChallengeScoreCalculator.Calculate(
            manager != null ? manager.Config : DailyChallengeConfig.LoadDefault(),
            moves,
            ms,
            minMoves);

        lastResult = new DailyChallengeResult
        {
            DayId = manager != null ? manager.CurrentDayId : DailyChallengeClock.GetCurrentUtcDayId(),
            LevelAssetName = data != null ? data.name : string.Empty,
            PoolIndex = manager != null ? manager.SelectedPoolIndex : -1,
            Moves = moves,
            CompletionTimeMilliseconds = ms,
            Score = score,
            Completed = true
        };
        hasResult = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[DailyChallenge] Run stopped moves=" + moves +
            " ms=" + ms + " score=" + score);
#endif
        return lastResult;
    }

    public static DailyChallengeRunTracker EnsureInScene()
    {
        if (Instance != null)
        {
            return Instance;
        }

        DailyChallengeRunTracker existing =
            FindAnyObjectByType<DailyChallengeRunTracker>();
        if (existing != null)
        {
            Instance = existing;
            return existing;
        }

        GameObject go = new GameObject("DailyChallengeRunTracker");
        return go.AddComponent<DailyChallengeRunTracker>();
    }
}

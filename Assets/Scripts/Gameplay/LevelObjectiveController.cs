using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Runtime objective voor special missions (TimedAmbulance, MoveLimit).
/// Classic: inactive. Geen UI — alleen API/events.
/// </summary>
public class LevelObjectiveController : MonoBehaviour
{
    public enum RuntimeState
    {
        Inactive = 0,
        WaitingToStart = 1,
        Running = 2,
        Completed = 3,
        Failed = 4
    }

    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameManager gameManager;

    private RuntimeState state = RuntimeState.Inactive;

    // TimedAmbulance
    private bool isTimedLevel;
    private float timeLimit;
    private float remainingTime;
    private Coroutine startRoutine;

    // MoveLimit
    private bool isMoveLimitLevel;
    private int moveLimit;
    private int movesUsed;
    private int movesRemaining;
    private bool pendingMoveLimitFailCheck;

    public RuntimeState State => state;
    public bool IsTimedLevel => isTimedLevel;
    public bool IsMoveLimitLevel => isMoveLimitLevel;
    public float TimeLimit => timeLimit;
    public float RemainingTime => remainingTime;
    public int MoveLimit => moveLimit;
    public int MovesUsed => movesUsed;
    public int MovesRemaining => movesRemaining;

    public event Action<float> OnTimerChanged;
    public event Action OnTimedMissionFailed;

    /// <summary>
    /// TimedAmbulance: timer gaat daadwerkelijk lopen (na fade / gameplay-ready).
    /// Presentation-only; geen gameplay-side effects.
    /// </summary>
    public event Action OnTimedMissionStarted;

    public event Action<int> OnMovesRemainingChanged;
    public event Action OnMoveLimitMissionFailed;

    private void Awake()
    {
        if (levelManager == null)
        {
            levelManager = FindFirstObjectByType<LevelManager>();
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    /// <summary>
    /// Aanroepen na level load (board + vehicles klaar).
    /// </summary>
    public void BeginForLevel(LevelData levelData)
    {
        StopStartRoutine();
        ClearMoveLimitState();

        isTimedLevel = false;
        timeLimit = 0f;
        remainingTime = 0f;
        state = RuntimeState.Inactive;

        if (levelData == null ||
            levelData.objectiveType == LevelObjectiveType.Classic)
        {
            OnTimerChanged?.Invoke(0f);
            OnMovesRemainingChanged?.Invoke(0);
            return;
        }

        if (levelData.objectiveType == LevelObjectiveType.TimedAmbulance)
        {
            isTimedLevel = true;
            timeLimit = Mathf.Max(0f, levelData.timeLimitSeconds);
            remainingTime = timeLimit;
            state = RuntimeState.WaitingToStart;
            OnTimerChanged?.Invoke(remainingTime);
            OnMovesRemainingChanged?.Invoke(0);
            startRoutine = StartCoroutine(StartTimerWhenGameplayReady());
            return;
        }

        if (levelData.objectiveType == LevelObjectiveType.MoveLimit)
        {
            OnTimerChanged?.Invoke(0f);

            int configuredLimit = Mathf.Max(0, levelData.moveLimit);
            if (configuredLimit <= 0)
            {
                Debug.LogWarning(
                    "LevelObjectiveController: MoveLimit objective zonder moveLimit > 0."
                );
                OnMovesRemainingChanged?.Invoke(0);
                return;
            }

            isMoveLimitLevel = true;
            moveLimit = configuredLimit;
            movesUsed = 0;
            movesRemaining = moveLimit;
            state = RuntimeState.Running;
            OnMovesRemainingChanged?.Invoke(movesRemaining);
            return;
        }

        OnTimerChanged?.Invoke(0f);
        OnMovesRemainingChanged?.Invoke(0);
    }

    /// <summary>
    /// Win bevestigd — stopt timer / move-limit; voorkomt fail.
    /// </summary>
    public void NotifyLevelCompleted()
    {
        if (state == RuntimeState.Failed || state == RuntimeState.Completed)
        {
            return;
        }

        StopStartRoutine();
        pendingMoveLimitFailCheck = false;
        state = RuntimeState.Completed;
    }

    /// <summary>
    /// Eén geldige gameplay-move (zelfde moment als GameManager.RegisterMove).
    /// Alleen actief bij MoveLimit + Running.
    /// </summary>
    public void NotifyValidMove()
    {
        if (!isMoveLimitLevel || state != RuntimeState.Running)
        {
            return;
        }

        if (gameManager != null &&
            (gameManager.IsLevelCompleted || gameManager.IsTargetExitInProgress))
        {
            return;
        }

        movesUsed++;
        movesRemaining = Mathf.Max(0, moveLimit - movesUsed);
        OnMovesRemainingChanged?.Invoke(movesRemaining);

        if (movesRemaining > 0)
        {
            return;
        }

        // Laatste move: mogelijk winning exit in dezelfde frame ná RegisterMove.
        // Fail-check deferred naar LateUpdate (na NotifyTargetExitStarted).
        pendingMoveLimitFailCheck = true;
    }

    private void LateUpdate()
    {
        if (!pendingMoveLimitFailCheck)
        {
            return;
        }

        pendingMoveLimitFailCheck = false;

        if (!isMoveLimitLevel || state != RuntimeState.Running)
        {
            return;
        }

        if (gameManager != null)
        {
            if (gameManager.IsLevelCompleted || gameManager.IsTargetExitInProgress)
            {
                return;
            }
        }

        FailMoveLimitMission();
    }

    private IEnumerator StartTimerWhenGameplayReady()
    {
        // Wacht tot SceneTransition-fade klaar is (unscaled), zodat timer
        // geen seconden verliest tijdens load/fade-in.
        while (SceneTransition.IsTransitioning)
        {
            yield return null;
        }

        // Eén frame extra: board/UI stabiel onder actieve gameplay.
        yield return null;

        startRoutine = null;

        if (!isTimedLevel || state != RuntimeState.WaitingToStart)
        {
            yield break;
        }

        if (gameManager != null &&
            (gameManager.IsLevelCompleted || gameManager.IsTargetExitInProgress))
        {
            state = RuntimeState.Completed;
            yield break;
        }

        state = RuntimeState.Running;
        OnTimedMissionStarted?.Invoke();
        OnTimerChanged?.Invoke(remainingTime);
    }

    private void Update()
    {
        if (!isTimedLevel || state != RuntimeState.Running)
        {
            return;
        }

        if (gameManager != null)
        {
            if (gameManager.IsLevelCompleted)
            {
                state = RuntimeState.Completed;
                return;
            }

            // Exit al gestart: fail niet meer — CompleteLevel wint de race.
            if (gameManager.IsTargetExitInProgress)
            {
                return;
            }
        }

        // Time.deltaTime == 0 bij pause (timeScale = 0).
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            OnTimerChanged?.Invoke(remainingTime);
            FailTimedMission();
            return;
        }

        OnTimerChanged?.Invoke(remainingTime);
    }

    private void FailTimedMission()
    {
        if (state != RuntimeState.Running)
        {
            return;
        }

        state = RuntimeState.Failed;

        if (gameManager != null)
        {
            gameManager.FailLevel();
        }

        OnTimedMissionFailed?.Invoke();
    }

    private void FailMoveLimitMission()
    {
        if (state != RuntimeState.Running)
        {
            return;
        }

        state = RuntimeState.Failed;

        if (gameManager != null)
        {
            gameManager.FailLevel();
        }

        OnMoveLimitMissionFailed?.Invoke();
    }

    private void ClearMoveLimitState()
    {
        isMoveLimitLevel = false;
        moveLimit = 0;
        movesUsed = 0;
        movesRemaining = 0;
        pendingMoveLimitFailCheck = false;
    }

    private void StopStartRoutine()
    {
        if (startRoutine != null)
        {
            StopCoroutine(startRoutine);
            startRoutine = null;
        }
    }
}

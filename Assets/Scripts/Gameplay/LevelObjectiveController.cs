using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime objective voor special missions
/// (TimedAmbulance, MoveLimit, MultiTargetRescue, NoTouchChallenge, FragileCargo, LimitedVehicle).
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

    // MultiTargetRescue
    private bool isMultiTargetRescueLevel;
    private int targetsTotal;
    private int targetsRescued;
    private int targetsRemaining;
    private readonly HashSet<EntityId> rescuedVehicleEntityIds = new HashSet<EntityId>();

    // NoTouchChallenge
    private bool isNoTouchChallengeLevel;
    private VehicleController protectedVehicle;
    private bool noTouchViolated;

    // FragileCargo
    private bool isFragileCargoLevel;
    private VehicleController fragileCargoVehicle;
    private int cargoMoveLimit;
    private int cargoMovesUsed;
    private int cargoMovesRemaining;
    private bool pendingFragileCargoFailCheck;

    // LimitedVehicle
    private bool isLimitedVehicleLevel;
    private VehicleController limitedVehicle;
    private int limitedVehicleMoveLimit;
    private int limitedVehicleMovesUsed;
    private int limitedVehicleMovesRemaining;
    private bool limitedVehicleLocked;

    public RuntimeState State => state;
    public bool IsTimedLevel => isTimedLevel;
    public bool IsMoveLimitLevel => isMoveLimitLevel;
    public bool IsMultiTargetRescueLevel => isMultiTargetRescueLevel;
    public bool IsNoTouchChallengeLevel => isNoTouchChallengeLevel;
    public bool IsFragileCargoLevel => isFragileCargoLevel;
    public bool IsLimitedVehicleLevel => isLimitedVehicleLevel;
    public float TimeLimit => timeLimit;
    public float RemainingTime => remainingTime;
    public int MoveLimit => moveLimit;
    public int MovesUsed => movesUsed;
    public int MovesRemaining => movesRemaining;
    public int TargetsTotal => targetsTotal;
    public int TargetsRescued => targetsRescued;
    public int TargetsRemaining => targetsRemaining;
    public bool HasProtectedVehicle => protectedVehicle != null;
    public bool NoTouchViolated => noTouchViolated;
    public VehicleController ProtectedVehicle => protectedVehicle;
    public VehicleController FragileCargoVehicle => fragileCargoVehicle;
    public int CargoMoveLimit => cargoMoveLimit;
    public int CargoMovesUsed => cargoMovesUsed;
    public int CargoMovesRemaining => cargoMovesRemaining;
    public VehicleController LimitedVehicle => limitedVehicle;
    public int LimitedVehicleMoveLimit => limitedVehicleMoveLimit;
    public int LimitedVehicleMovesUsed => limitedVehicleMovesUsed;
    public int LimitedVehicleMovesRemaining => limitedVehicleMovesRemaining;
    public bool IsLimitedVehicleLocked => limitedVehicleLocked;
    public bool IsWaitingForSpecialIntro => waitingForSpecialIntro;
    public int SpecialIntroSessionId => specialIntroSessionId;
    public bool IsWaitingForObjectiveTutorial => waitingForObjectiveTutorial;
    public int ObjectiveTutorialSessionId => objectiveTutorialSessionId;

    public event Action<float> OnTimerChanged;
    public event Action OnTimedMissionFailed;

    /// <summary>
    /// TimedAmbulance: timer gaat daadwerkelijk lopen (na fade / gameplay-ready).
    /// Presentation-only; geen gameplay-side effects.
    /// </summary>
    public event Action OnTimedMissionStarted;

    public event Action<int> OnMovesRemainingChanged;
    public event Action OnMoveLimitMissionFailed;

    /// <summary>
    /// MultiTargetRescue: remaining + total voor HUD (bijv. RESCUE 1/2).
    /// </summary>
    public event Action<int, int> OnTargetsRemainingChanged;

    public event Action OnTargetRescued;

    /// <summary>
    /// NoTouchChallenge: protected vehicle deed een geldige grid-move.
    /// Presentation-only (later: fail panel / flash / SFX).
    /// </summary>
    public event Action OnNoTouchMissionFailed;

    /// <summary>
    /// FragileCargo: remaining + total cargo moves voor HUD.
    /// </summary>
    public event Action<int, int> OnCargoMovesRemainingChanged;

    public event Action OnFragileCargoMissionFailed;

    /// <summary>
    /// LimitedVehicle: remaining + total moves voor HUD.
    /// </summary>
    public event Action<int, int> OnLimitedVehicleMovesRemainingChanged;

    /// <summary>
    /// LimitedVehicle: budget op — alleen dit voertuig is gelocked (geen FailLevel).
    /// </summary>
    public event Action OnLimitedVehicleLocked;

    /// <summary>
    /// LimitedVehicle: lock opgeheven (undo van de locking move). Presentation-only.
    /// </summary>
    public event Action OnLimitedVehicleUnlocked;

    // Special mission intro gate (presentation): Timed wacht hierop vóór Running.
    private bool waitingForSpecialIntro;
    private int specialIntroSessionId;

    // First-time objective tutorial gate: Timed wacht hierop na special intro.
    private bool waitingForObjectiveTutorial;
    private int objectiveTutorialSessionId;

    private void Awake()
    {
        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
    }

    /// <summary>
    /// Aanroepen na level load (board + vehicles klaar).
    /// </summary>
    public void BeginForLevel(LevelData levelData)
    {
        StopStartRoutine();
        ClearMoveLimitState();
        ClearMultiTargetState();
        ClearNoTouchState();
        ClearFragileCargoState();
        ClearLimitedVehicleState();
        ClearSpecialIntroGate();
        ClearObjectiveTutorialGate();

        isTimedLevel = false;
        timeLimit = 0f;
        remainingTime = 0f;
        state = RuntimeState.Inactive;

        if (levelData == null ||
            levelData.objectiveType == LevelObjectiveType.Classic)
        {
            OnTimerChanged?.Invoke(0f);
            OnMovesRemainingChanged?.Invoke(0);
            OnTargetsRemainingChanged?.Invoke(0, 0);
            OnCargoMovesRemainingChanged?.Invoke(0, 0);
            OnLimitedVehicleMovesRemainingChanged?.Invoke(0, 0);
            MaybeBeginObjectiveTutorialGate(LevelObjectiveType.Classic);
            return;
        }

        if (levelData.objectiveType == LevelObjectiveType.TimedAmbulance)
        {
            BeginSpecialIntroGate();
            MaybeBeginObjectiveTutorialGate(LevelObjectiveType.TimedAmbulance);
            isTimedLevel = true;
            timeLimit = Mathf.Max(0f, levelData.timeLimitSeconds);
            remainingTime = timeLimit;
            state = RuntimeState.WaitingToStart;
            OnTimerChanged?.Invoke(remainingTime);
            OnMovesRemainingChanged?.Invoke(0);
            OnTargetsRemainingChanged?.Invoke(0, 0);
            OnCargoMovesRemainingChanged?.Invoke(0, 0);
            OnLimitedVehicleMovesRemainingChanged?.Invoke(0, 0);
            startRoutine = StartCoroutine(StartTimerWhenGameplayReady());
            return;
        }

        if (levelData.objectiveType == LevelObjectiveType.MoveLimit)
        {
            OnTimerChanged?.Invoke(0f);
            OnTargetsRemainingChanged?.Invoke(0, 0);
            OnCargoMovesRemainingChanged?.Invoke(0, 0);
            OnLimitedVehicleMovesRemainingChanged?.Invoke(0, 0);

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
            BeginSpecialIntroGate();
            MaybeBeginObjectiveTutorialGate(LevelObjectiveType.MoveLimit);
            state = RuntimeState.Running;
            OnMovesRemainingChanged?.Invoke(movesRemaining);
            return;
        }

        if (levelData.objectiveType == LevelObjectiveType.MultiTargetRescue)
        {
            OnTimerChanged?.Invoke(0f);
            OnMovesRemainingChanged?.Invoke(0);
            OnCargoMovesRemainingChanged?.Invoke(0, 0);
            OnLimitedVehicleMovesRemainingChanged?.Invoke(0, 0);

            ValidateMultiTargetRescueLayout(levelData);

            int targetCount = CountTargetsInLevelData(levelData);
            if (targetCount < 2)
            {
                Debug.LogWarning(
                    "[MultiTargetRescue] Verwacht ≥ 2 vehicles met canExitRight " +
                    "(nu: " + targetCount + ")."
                );
            }

            if (targetCount <= 0)
            {
                OnTargetsRemainingChanged?.Invoke(0, 0);
                return;
            }

            isMultiTargetRescueLevel = true;
            targetsTotal = targetCount;
            targetsRescued = 0;
            targetsRemaining = targetsTotal;
            BeginSpecialIntroGate();
            MaybeBeginObjectiveTutorialGate(LevelObjectiveType.MultiTargetRescue);
            state = RuntimeState.Running;
            OnTargetsRemainingChanged?.Invoke(targetsRemaining, targetsTotal);
            return;
        }

        if (levelData.objectiveType == LevelObjectiveType.NoTouchChallenge)
        {
            OnTimerChanged?.Invoke(0f);
            OnMovesRemainingChanged?.Invoke(0);
            OnTargetsRemainingChanged?.Invoke(0, 0);
            OnCargoMovesRemainingChanged?.Invoke(0, 0);
            OnLimitedVehicleMovesRemainingChanged?.Invoke(0, 0);

            ValidateNoTouchChallengeLayout(levelData);
            BindProtectedVehicleFromActiveVehicles();

            isNoTouchChallengeLevel = true;
            noTouchViolated = false;
            BeginSpecialIntroGate();
            MaybeBeginObjectiveTutorialGate(LevelObjectiveType.NoTouchChallenge);
            state = RuntimeState.Running;
            return;
        }

        if (levelData.objectiveType == LevelObjectiveType.FragileCargo)
        {
            OnTimerChanged?.Invoke(0f);
            OnMovesRemainingChanged?.Invoke(0);
            OnTargetsRemainingChanged?.Invoke(0, 0);
            OnLimitedVehicleMovesRemainingChanged?.Invoke(0, 0);

            ValidateFragileCargoLayout(levelData);

            int configuredLimit = Mathf.Max(0, levelData.fragileCargoMoveLimit);
            if (configuredLimit <= 0)
            {
                Debug.LogWarning(
                    "[FragileCargo] Level " + levelData.levelNumber +
                    " heeft geen fragileCargoMoveLimit > 0."
                );
                OnCargoMovesRemainingChanged?.Invoke(0, 0);
                return;
            }

            BindFragileCargoVehicleFromActiveVehicles();

            isFragileCargoLevel = true;
            cargoMoveLimit = configuredLimit;
            cargoMovesUsed = 0;
            cargoMovesRemaining = cargoMoveLimit;
            pendingFragileCargoFailCheck = false;
            BeginSpecialIntroGate();
            MaybeBeginObjectiveTutorialGate(LevelObjectiveType.FragileCargo);
            state = RuntimeState.Running;
            OnCargoMovesRemainingChanged?.Invoke(cargoMovesRemaining, cargoMoveLimit);
            return;
        }

        if (levelData.objectiveType == LevelObjectiveType.LimitedVehicle)
        {
            OnTimerChanged?.Invoke(0f);
            OnMovesRemainingChanged?.Invoke(0);
            OnTargetsRemainingChanged?.Invoke(0, 0);
            OnCargoMovesRemainingChanged?.Invoke(0, 0);

            ValidateLimitedVehicleLayout(levelData);

            int configuredLimit = Mathf.Max(0, levelData.limitedVehicleMoveLimit);
            if (configuredLimit <= 0)
            {
                Debug.LogWarning(
                    "[LimitedVehicle] Level " + levelData.levelNumber +
                    " heeft geen limitedVehicleMoveLimit > 0."
                );
                OnLimitedVehicleMovesRemainingChanged?.Invoke(0, 0);
                return;
            }

            BindLimitedVehicleFromActiveVehicles();

            isLimitedVehicleLevel = true;
            limitedVehicleMoveLimit = configuredLimit;
            limitedVehicleMovesUsed = 0;
            limitedVehicleMovesRemaining = limitedVehicleMoveLimit;
            limitedVehicleLocked = false;

            if (limitedVehicle != null)
            {
                limitedVehicle.SetLimitedVehicleLocked(false);
            }
            else
            {
                Debug.LogWarning(
                    "[LimitedVehicle] Level " + levelData.levelNumber +
                    " heeft geen gebonden LimitedVehicle na spawn."
                );
            }

            BeginSpecialIntroGate();
            MaybeBeginObjectiveTutorialGate(LevelObjectiveType.LimitedVehicle);
            state = RuntimeState.Running;
            OnLimitedVehicleMovesRemainingChanged?.Invoke(
                limitedVehicleMovesRemaining,
                limitedVehicleMoveLimit
            );
            return;
        }

        OnTimerChanged?.Invoke(0f);
        OnMovesRemainingChanged?.Invoke(0);
        OnTargetsRemainingChanged?.Invoke(0, 0);
        OnCargoMovesRemainingChanged?.Invoke(0, 0);
        OnLimitedVehicleMovesRemainingChanged?.Invoke(0, 0);
    }

    /// <summary>
    /// Win bevestigd — stopt timer / move-limit / multi-target / no-touch / fragile cargo /
    /// limited vehicle; voorkomt fail.
    /// </summary>
    public void NotifyLevelCompleted()
    {
        if (state == RuntimeState.Failed || state == RuntimeState.Completed)
        {
            return;
        }

        StopStartRoutine();
        pendingMoveLimitFailCheck = false;
        pendingFragileCargoFailCheck = false;
        ClearSpecialIntroGate();
        ClearObjectiveTutorialGate();
        state = RuntimeState.Completed;
    }

    /// <summary>
    /// MultiTargetRescue: één target succesvol geëxit.
    /// Returns true als alle targets gered zijn (CompleteLevel mag).
    /// </summary>
    public bool NotifyTargetRescued(VehicleController vehicle)
    {
        if (!isMultiTargetRescueLevel || state != RuntimeState.Running)
        {
            return true;
        }

        if (vehicle == null)
        {
            return targetsRemaining <= 0;
        }

        EntityId entityId = vehicle.GetEntityId();
        if (!rescuedVehicleEntityIds.Add(entityId))
        {
            // Dubbele callback — niet opnieuw aftellen.
            return targetsRemaining <= 0;
        }

        targetsRescued = Mathf.Min(targetsTotal, targetsRescued + 1);
        targetsRemaining = Mathf.Max(0, targetsTotal - targetsRescued);

        OnTargetRescued?.Invoke();
        OnTargetsRemainingChanged?.Invoke(targetsRemaining, targetsTotal);

        if (targetsRemaining > 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Eén geldige gameplay-move (zelfde moment als GameManager.RegisterMove).
    /// MoveLimit: telt alle moves. FragileCargo: alleen cargo-target moves.
    /// LimitedVehicle: alleen limited blocker moves → lock bij 0 (geen FailLevel).
    /// NoTouchChallenge: faalt bij protected vehicle move.
    /// </summary>
    public void NotifyValidMove(VehicleController vehicle)
    {
        if (isNoTouchChallengeLevel && state == RuntimeState.Running)
        {
            if (vehicle != null && vehicle.IsProtectedVehicle)
            {
                FailNoTouchMission();
                return;
            }
        }

        if (isFragileCargoLevel && state == RuntimeState.Running)
        {
            if (vehicle == null || !vehicle.IsFragileCargo)
            {
                return;
            }

            if (gameManager != null &&
                (gameManager.IsLevelCompleted || gameManager.IsTargetExitInProgress))
            {
                return;
            }

            cargoMovesUsed++;
            cargoMovesRemaining = Mathf.Max(0, cargoMoveLimit - cargoMovesUsed);
            OnCargoMovesRemainingChanged?.Invoke(cargoMovesRemaining, cargoMoveLimit);

            if (cargoMovesRemaining > 0)
            {
                return;
            }

            // Laatste cargo-move: mogelijk winning exit in dezelfde frame ná RegisterMove.
            // Fail-check deferred naar LateUpdate (na NotifyTargetExitStarted).
            pendingFragileCargoFailCheck = true;
            return;
        }

        if (isLimitedVehicleLevel && state == RuntimeState.Running)
        {
            if (vehicle == null || !vehicle.IsLimitedVehicle)
            {
                return;
            }

            if (limitedVehicleLocked)
            {
                return;
            }

            limitedVehicleMovesUsed++;
            limitedVehicleMovesRemaining =
                Mathf.Max(0, limitedVehicleMoveLimit - limitedVehicleMovesUsed);
            OnLimitedVehicleMovesRemainingChanged?.Invoke(
                limitedVehicleMovesRemaining,
                limitedVehicleMoveLimit
            );

            // Laatste toegestane move is al uitgevoerd (RegisterMove ná grid-change).
            // Lock pas hierna — toekomstige input op dit voertuig weigeren.
            if (limitedVehicleMovesRemaining <= 0)
            {
                LockLimitedVehicle();
            }

            return;
        }

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

    /// <summary>
    /// Draait objective-side effects van één geldige board-move terug.
    /// Alleen tijdens Running (of Inactive Classic — no-op). Geen reinitialize.
    /// </summary>
    public void UndoValidMove(VehicleController vehicle)
    {
        if (state == RuntimeState.Completed || state == RuntimeState.Failed)
        {
            return;
        }

        if (isFragileCargoLevel && state == RuntimeState.Running)
        {
            if (vehicle == null || !vehicle.IsFragileCargo)
            {
                return;
            }

            pendingFragileCargoFailCheck = false;

            if (cargoMovesUsed <= 0)
            {
                return;
            }

            cargoMovesUsed--;
            cargoMovesRemaining = Mathf.Max(0, cargoMoveLimit - cargoMovesUsed);
            OnCargoMovesRemainingChanged?.Invoke(cargoMovesRemaining, cargoMoveLimit);
            return;
        }

        if (isLimitedVehicleLevel && state == RuntimeState.Running)
        {
            if (vehicle == null || !vehicle.IsLimitedVehicle)
            {
                return;
            }

            if (limitedVehicleMovesUsed <= 0)
            {
                return;
            }

            limitedVehicleMovesUsed--;
            limitedVehicleMovesRemaining =
                Mathf.Max(0, limitedVehicleMoveLimit - limitedVehicleMovesUsed);

            if (limitedVehicleLocked && limitedVehicleMovesRemaining > 0)
            {
                UnlockLimitedVehicle();
            }

            OnLimitedVehicleMovesRemainingChanged?.Invoke(
                limitedVehicleMovesRemaining,
                limitedVehicleMoveLimit
            );
            return;
        }

        if (!isMoveLimitLevel || state != RuntimeState.Running)
        {
            return;
        }

        pendingMoveLimitFailCheck = false;

        if (movesUsed <= 0)
        {
            return;
        }

        movesUsed--;
        movesRemaining = Mathf.Max(0, moveLimit - movesUsed);
        OnMovesRemainingChanged?.Invoke(movesRemaining);
    }

    private void LateUpdate()
    {
        if (pendingMoveLimitFailCheck)
        {
            pendingMoveLimitFailCheck = false;
            ResolvePendingMoveLimitFail();
        }

        if (pendingFragileCargoFailCheck)
        {
            pendingFragileCargoFailCheck = false;
            ResolvePendingFragileCargoFail();
        }
    }

    private void ResolvePendingMoveLimitFail()
    {
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

    private void ResolvePendingFragileCargoFail()
    {
        if (!isFragileCargoLevel || state != RuntimeState.Running)
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

        FailFragileCargoMission();
    }

    private IEnumerator StartTimerWhenGameplayReady()
    {
        // 1) Scene fade klaar (unscaled).
        while (SceneTransition.IsTransitioning)
        {
            yield return null;
        }

        // 2) Special mission intro klaar (unscaled).
        while (waitingForSpecialIntro)
        {
            yield return null;
        }

        // 3) First-time tutorial klaar (of al gezien → gate nooit gezet).
        while (waitingForObjectiveTutorial)
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

    /// <summary>
    /// Presentation: special mission intro is klaar.
    /// Tutorial-gate (indien actief) houdt input/timer nog geblokkeerd.
    /// </summary>
    public void NotifySpecialMissionIntroCompleted()
    {
        if (!waitingForSpecialIntro)
        {
            return;
        }

        ClearSpecialIntroGate();
    }

    /// <summary>
    /// Presentation: first-time tutorial dismissed (Got It) of al gezien.
    /// </summary>
    public void NotifyObjectiveTutorialCompleted()
    {
        if (!waitingForObjectiveTutorial)
        {
            return;
        }

        ClearObjectiveTutorialGate();
    }

    private void BeginSpecialIntroGate()
    {
        waitingForSpecialIntro = true;
        specialIntroSessionId++;

        if (gameManager != null)
        {
            gameManager.SetSpecialMissionIntroBlocked(true);
        }
    }

    private void ClearSpecialIntroGate()
    {
        waitingForSpecialIntro = false;

        if (gameManager != null)
        {
            gameManager.SetSpecialMissionIntroBlocked(false);
        }
    }

    private void MaybeBeginObjectiveTutorialGate(LevelObjectiveType objectiveType)
    {
        if (ObjectiveTutorialPrefs.HasSeenTutorial(objectiveType))
        {
            return;
        }

        BeginObjectiveTutorialGate();
    }

    private void BeginObjectiveTutorialGate()
    {
        waitingForObjectiveTutorial = true;
        objectiveTutorialSessionId++;

        if (gameManager != null)
        {
            gameManager.SetObjectiveTutorialBlocked(true);
        }
    }

    private void ClearObjectiveTutorialGate()
    {
        waitingForObjectiveTutorial = false;

        if (gameManager != null)
        {
            gameManager.SetObjectiveTutorialBlocked(false);
        }
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

    private void FailNoTouchMission()
    {
        if (state != RuntimeState.Running)
        {
            return;
        }

        noTouchViolated = true;
        state = RuntimeState.Failed;

        if (gameManager != null)
        {
            gameManager.FailLevel();
        }

        OnNoTouchMissionFailed?.Invoke();
    }

    private void FailFragileCargoMission()
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

        OnFragileCargoMissionFailed?.Invoke();
    }

    /// <summary>
    /// LimitedVehicle: budget op — alleen dit voertuig locken. Geen FailLevel.
    /// </summary>
    private void LockLimitedVehicle()
    {
        if (limitedVehicleLocked)
        {
            return;
        }

        limitedVehicleLocked = true;

        if (limitedVehicle != null)
        {
            limitedVehicle.SetLimitedVehicleLocked(true);
        }

        OnLimitedVehicleLocked?.Invoke();
    }

    /// <summary>
    /// LimitedVehicle: unlock na undo van de locking move. Geen FailLevel-herstel.
    /// </summary>
    private void UnlockLimitedVehicle()
    {
        if (!limitedVehicleLocked)
        {
            return;
        }

        limitedVehicleLocked = false;

        if (limitedVehicle != null)
        {
            limitedVehicle.SetLimitedVehicleLocked(false);
        }

        OnLimitedVehicleUnlocked?.Invoke();
    }

    /// <summary>
    /// Vindt protected vehicle via IsProtectedVehicle op gespawnede controllers.
    /// Geen FindObjects — gebruikt LevelManager.ActiveVehicles.
    /// </summary>
    private void BindProtectedVehicleFromActiveVehicles()
    {
        protectedVehicle = null;

        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        if (levelManager == null)
        {
            return;
        }

        IReadOnlyList<VehicleController> vehicles = levelManager.ActiveVehicles;
        if (vehicles == null)
        {
            return;
        }

        for (int i = 0; i < vehicles.Count; i++)
        {
            VehicleController vehicle = vehicles[i];
            if (vehicle == null || !vehicle.IsProtectedVehicle)
            {
                continue;
            }

            if (protectedVehicle == null)
            {
                protectedVehicle = vehicle;
            }
        }
    }

    /// <summary>
    /// Vindt cargo vehicle via IsFragileCargo op gespawnede controllers.
    /// Geen FindObjects — gebruikt LevelManager.ActiveVehicles.
    /// </summary>
    private void BindFragileCargoVehicleFromActiveVehicles()
    {
        fragileCargoVehicle = null;

        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        if (levelManager == null)
        {
            return;
        }

        IReadOnlyList<VehicleController> vehicles = levelManager.ActiveVehicles;
        if (vehicles == null)
        {
            return;
        }

        for (int i = 0; i < vehicles.Count; i++)
        {
            VehicleController vehicle = vehicles[i];
            if (vehicle == null || !vehicle.IsFragileCargo)
            {
                continue;
            }

            if (fragileCargoVehicle == null)
            {
                fragileCargoVehicle = vehicle;
            }
        }
    }

    /// <summary>
    /// Vindt limited vehicle via IsLimitedVehicle op gespawnede controllers.
    /// Geen FindObjects — gebruikt LevelManager.ActiveVehicles.
    /// </summary>
    private void BindLimitedVehicleFromActiveVehicles()
    {
        limitedVehicle = null;

        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        if (levelManager == null)
        {
            return;
        }

        IReadOnlyList<VehicleController> vehicles = levelManager.ActiveVehicles;
        if (vehicles == null)
        {
            return;
        }

        for (int i = 0; i < vehicles.Count; i++)
        {
            VehicleController vehicle = vehicles[i];
            if (vehicle == null || !vehicle.IsLimitedVehicle)
            {
                continue;
            }

            if (limitedVehicle == null)
            {
                limitedVehicle = vehicle;
            }
        }
    }

    private static void ValidateLimitedVehicleLayout(LevelData levelData)
    {
        if (levelData == null || levelData.vehicles == null)
        {
            Debug.LogWarning(
                "[LimitedVehicle] Level heeft geen vehicles-lijst."
            );
            return;
        }

        string levelLabel = "Level " + levelData.levelNumber;
        int limitedCount = 0;

        if (levelData.limitedVehicleMoveLimit <= 0)
        {
            Debug.LogWarning(
                "[LimitedVehicle] " + levelLabel +
                " heeft geen limitedVehicleMoveLimit > 0."
            );
        }

        for (int i = 0; i < levelData.vehicles.Count; i++)
        {
            VehicleData vehicle = levelData.vehicles[i];
            if (vehicle == null || !vehicle.isLimitedVehicle)
            {
                continue;
            }

            limitedCount++;

            string name = string.IsNullOrEmpty(vehicle.vehicleName)
                ? ("vehicle[" + i + "]")
                : vehicle.vehicleName;

            if (vehicle.canExitRight)
            {
                Debug.LogWarning(
                    "[LimitedVehicle] Limited vehicle '" + name +
                    "' is ook target/canExitRight."
                );
            }

            if (vehicle.isProtectedVehicle)
            {
                Debug.LogWarning(
                    "[LimitedVehicle] Limited vehicle '" + name +
                    "' is ook isProtectedVehicle."
                );
            }

            if (vehicle.isFragileCargo)
            {
                Debug.LogWarning(
                    "[LimitedVehicle] Limited vehicle '" + name +
                    "' is ook isFragileCargo."
                );
            }
        }

        if (limitedCount == 0)
        {
            Debug.LogWarning(
                "[LimitedVehicle] " + levelLabel +
                " heeft geen limited vehicle."
            );
        }
        else if (limitedCount > 1)
        {
            Debug.LogWarning(
                "[LimitedVehicle] " + levelLabel +
                " heeft " + limitedCount +
                " limited vehicles (verwacht exact 1)."
            );
        }
    }

    private static void ValidateFragileCargoLayout(LevelData levelData)
    {
        if (levelData == null || levelData.vehicles == null)
        {
            Debug.LogWarning(
                "[FragileCargo] Level heeft geen vehicles-lijst."
            );
            return;
        }

        string levelLabel = "Level " + levelData.levelNumber;
        int cargoCount = 0;
        int targetCount = 0;
        int exitRow = levelData.exitRow;

        if (levelData.fragileCargoMoveLimit <= 0)
        {
            Debug.LogWarning(
                "[FragileCargo] " + levelLabel +
                " heeft geen fragileCargoMoveLimit > 0."
            );
        }

        for (int i = 0; i < levelData.vehicles.Count; i++)
        {
            VehicleData vehicle = levelData.vehicles[i];
            if (vehicle == null)
            {
                continue;
            }

            if (vehicle.canExitRight)
            {
                targetCount++;
            }

            if (!vehicle.isFragileCargo)
            {
                continue;
            }

            cargoCount++;

            string name = string.IsNullOrEmpty(vehicle.vehicleName)
                ? ("vehicle[" + i + "]")
                : vehicle.vehicleName;

            if (!vehicle.canExitRight)
            {
                Debug.LogWarning(
                    "[FragileCargo] Cargo vehicle '" + name +
                    "' is niet target/canExitRight."
                );
            }

            if (vehicle.orientation != VehicleController.VehicleOrientation.Horizontal)
            {
                Debug.LogWarning(
                    "[FragileCargo] Cargo vehicle '" + name +
                    "' is niet Horizontal — kan nooit via de exit ontsnappen."
                );
            }

            if (vehicle.gridPosition.y != exitRow)
            {
                Debug.LogWarning(
                    "[FragileCargo] Cargo vehicle '" + name +
                    "' staat niet op exitRow " + exitRow +
                    " (row=" + vehicle.gridPosition.y +
                    ") en kan nooit ontsnappen."
                );
            }
        }

        if (cargoCount == 0)
        {
            Debug.LogWarning(
                "[FragileCargo] " + levelLabel +
                " heeft geen fragile cargo vehicle."
            );
        }
        else if (cargoCount > 1)
        {
            Debug.LogWarning(
                "[FragileCargo] " + levelLabel +
                " heeft " + cargoCount +
                " fragile cargo vehicles (verwacht exact 1)."
            );
        }

        if (targetCount != 1)
        {
            Debug.LogWarning(
                "[FragileCargo] " + levelLabel +
                " verwacht exact 1 target/canExitRight (nu: " +
                targetCount + ")."
            );
        }
    }

    private static void ValidateNoTouchChallengeLayout(LevelData levelData)
    {
        if (levelData == null || levelData.vehicles == null)
        {
            Debug.LogWarning(
                "[NoTouchChallenge] Level heeft geen vehicles-lijst."
            );
            return;
        }

        string levelLabel = "Level " + levelData.levelNumber;
        int protectedCount = 0;
        int targetCount = 0;

        for (int i = 0; i < levelData.vehicles.Count; i++)
        {
            VehicleData vehicle = levelData.vehicles[i];
            if (vehicle == null)
            {
                continue;
            }

            if (vehicle.canExitRight)
            {
                targetCount++;
            }

            if (!vehicle.isProtectedVehicle)
            {
                continue;
            }

            protectedCount++;

            string name = string.IsNullOrEmpty(vehicle.vehicleName)
                ? ("vehicle[" + i + "]")
                : vehicle.vehicleName;

            if (vehicle.canExitRight)
            {
                Debug.LogWarning(
                    "[NoTouchChallenge] Protected vehicle '" + name +
                    "' is ook target/canExitRight."
                );
            }
        }

        if (protectedCount == 0)
        {
            Debug.LogWarning(
                "[NoTouchChallenge] " + levelLabel +
                " heeft geen protected vehicle."
            );
        }
        else if (protectedCount > 1)
        {
            Debug.LogWarning(
                "[NoTouchChallenge] " + levelLabel +
                " heeft " + protectedCount +
                " protected vehicles (verwacht exact 1)."
            );
        }

        if (targetCount != 1)
        {
            Debug.LogWarning(
                "[NoTouchChallenge] " + levelLabel +
                " verwacht exact 1 target/canExitRight (nu: " +
                targetCount + ")."
            );
        }
    }

    private static int CountTargetsInLevelData(LevelData levelData)
    {
        if (levelData == null || levelData.vehicles == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < levelData.vehicles.Count; i++)
        {
            VehicleData vehicle = levelData.vehicles[i];
            if (vehicle != null && vehicle.canExitRight)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// v1: één gedeelde exitRow. Horizontale targets kunnen niet van rij wisselen,
    /// dus alle canExitRight-targets moeten op exitRow staan.
    /// Geen auto-fix — alleen warnings.
    /// </summary>
    private static void ValidateMultiTargetRescueLayout(LevelData levelData)
    {
        if (levelData == null || levelData.vehicles == null)
        {
            return;
        }

        int exitRow = levelData.exitRow;

        for (int i = 0; i < levelData.vehicles.Count; i++)
        {
            VehicleData vehicle = levelData.vehicles[i];
            if (vehicle == null || !vehicle.canExitRight)
            {
                continue;
            }

            string name = string.IsNullOrEmpty(vehicle.vehicleName)
                ? ("vehicle[" + i + "]")
                : vehicle.vehicleName;

            if (vehicle.orientation != VehicleController.VehicleOrientation.Horizontal)
            {
                Debug.LogWarning(
                    "[MultiTargetRescue] Target '" + name +
                    "' is niet Horizontal — kan nooit via de exit ontsnappen."
                );
            }

            if (vehicle.gridPosition.y != exitRow)
            {
                Debug.LogWarning(
                    "[MultiTargetRescue] Target '" + name +
                    "' staat niet op exitRow " + exitRow +
                    " (row=" + vehicle.gridPosition.y +
                    ") en kan nooit ontsnappen."
                );
            }
        }
    }

    private void ClearMoveLimitState()
    {
        isMoveLimitLevel = false;
        moveLimit = 0;
        movesUsed = 0;
        movesRemaining = 0;
        pendingMoveLimitFailCheck = false;
    }

    private void ClearMultiTargetState()
    {
        isMultiTargetRescueLevel = false;
        targetsTotal = 0;
        targetsRescued = 0;
        targetsRemaining = 0;
        rescuedVehicleEntityIds.Clear();
    }

    private void ClearNoTouchState()
    {
        isNoTouchChallengeLevel = false;
        protectedVehicle = null;
        noTouchViolated = false;
    }

    private void ClearFragileCargoState()
    {
        isFragileCargoLevel = false;
        fragileCargoVehicle = null;
        cargoMoveLimit = 0;
        cargoMovesUsed = 0;
        cargoMovesRemaining = 0;
        pendingFragileCargoFailCheck = false;
    }

    private void ClearLimitedVehicleState()
    {
        isLimitedVehicleLevel = false;
        limitedVehicle = null;
        limitedVehicleMoveLimit = 0;
        limitedVehicleMovesUsed = 0;
        limitedVehicleMovesRemaining = 0;
        limitedVehicleLocked = false;
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

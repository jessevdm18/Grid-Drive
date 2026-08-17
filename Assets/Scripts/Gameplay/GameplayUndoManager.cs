using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Undo van de laatste geldige board-move tegen coins.
/// Geen exit-moves, geen failure/win recovery. Alleen presentation via objective events.
/// </summary>
public class GameplayUndoManager : MonoBehaviour
{
    private struct UndoRecord
    {
        public VehicleController Vehicle;
        public Vector2Int FromPosition;
        public Vector2Int ToPosition;
    }

    private static GameplayUndoManager instance;

    [Header("Refs")]
    [SerializeField] private Button undoButton;
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private LevelObjectiveController objectiveController;
    [SerializeField] private AudioManager audioManager;

    [Header("Undo")]
    [SerializeField, Min(0)] private int undoCoinCost = 25;
    [SerializeField, Min(1)] private int maxUndoHistory = 20;

    private readonly List<UndoRecord> history = new List<UndoRecord>(20);
    private bool lastKnownCanUndo;

    public int UndoCoinCost => undoCoinCost;
    public int HistoryCount => history.Count;

    public bool CanUndo => EvaluateCanUndo();

    public event Action<bool> OnUndoAvailabilityChanged;
    public event Action OnUndoFailedInsufficientCoins;
    public event Action OnUndoPerformed;

    private void Awake()
    {
        instance = this;

        if (coinManager == null)
        {
            coinManager = FindAnyObjectByType<CoinManager>();
        }

        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        if (objectiveController == null)
        {
            objectiveController = FindAnyObjectByType<LevelObjectiveController>();
        }

        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }
    }

    private void OnEnable()
    {
        if (undoButton != null)
        {
            undoButton.onClick.AddListener(OnUndoButtonClicked);
        }

        RefreshUndoAvailability(forceNotify: true);
    }

    private void OnDisable()
    {
        if (undoButton != null)
        {
            undoButton.onClick.RemoveListener(OnUndoButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void LateUpdate()
    {
        RefreshUndoAvailability(forceNotify: false);
    }

    /// <summary>
    /// Knop-callback (ook aanroepbaar vanuit Inspector OnClick).
    /// </summary>
    public void OnUndoButtonClicked()
    {
        TryUndoLastMove();
    }

    /// <summary>
    /// Probeert de laatste geldige board-move terug te draaien.
    /// Coins alleen bij succesvolle undo.
    /// </summary>
    public bool TryUndoLastMove()
    {
        if (!EvaluateCanUndo())
        {
            RefreshUndoAvailability(forceNotify: true);
            return false;
        }

        if (coinManager == null)
        {
            Debug.LogError("GameplayUndoManager: geen CoinManager gekoppeld.");
            return false;
        }

        if (undoCoinCost > 0 && !coinManager.CanAfford(undoCoinCost))
        {
            OnUndoFailedInsufficientCoins?.Invoke();
            audioManager?.PlayInsufficientCoins();
            return false;
        }

        UndoRecord record = history[history.Count - 1];
        if (!ValidateRecord(record))
        {
            history.RemoveAt(history.Count - 1);
            RefreshUndoAvailability(forceNotify: true);
            return false;
        }

        if (undoCoinCost > 0 && !coinManager.SpendCoins(undoCoinCost))
        {
            OnUndoFailedInsufficientCoins?.Invoke();
            audioManager?.PlayInsufficientCoins();
            return false;
        }

        if (!ApplyUndo(record))
        {
            if (undoCoinCost > 0)
            {
                coinManager.AddCoins(undoCoinCost);
            }

            Debug.LogWarning("GameplayUndoManager: undo apply faalde — coins teruggestort.");
            RefreshUndoAvailability(forceNotify: true);
            return false;
        }

        if (undoCoinCost > 0)
        {
            audioManager?.PlayCoinSpend();
        }

        history.RemoveAt(history.Count - 1);
        OnUndoPerformed?.Invoke();
        RefreshUndoAvailability(forceNotify: true);
        return true;
    }

    public void ClearHistory()
    {
        if (history.Count == 0)
        {
            RefreshUndoAvailability(forceNotify: false);
            return;
        }

        history.Clear();
        RefreshUndoAvailability(forceNotify: true);
    }

    /// <summary>
    /// Intern: record één normale board-move (geen exit).
    /// </summary>
    public void RecordValidBoardMove(
        VehicleController vehicle,
        Vector2Int fromPosition,
        Vector2Int toPosition
    )
    {
        if (vehicle == null || fromPosition == toPosition)
        {
            return;
        }

        if (!IsLevelPlayableForUndo())
        {
            return;
        }

        // NoTouch protected move → FailLevel in dezelfde NotifyValidMove-call.
        // Niet stacken: failure wist history sowieso; voorkom stale record.
        if (gameManager != null && gameManager.IsLevelFailed)
        {
            ClearHistory();
            return;
        }

        if (objectiveController != null &&
            objectiveController.State == LevelObjectiveController.RuntimeState.Failed)
        {
            ClearHistory();
            return;
        }

        history.Add(new UndoRecord
        {
            Vehicle = vehicle,
            FromPosition = fromPosition,
            ToPosition = toPosition
        });

        while (history.Count > maxUndoHistory)
        {
            history.RemoveAt(0);
        }

        RefreshUndoAvailability(forceNotify: true);
    }

    public static void RecordValidBoardMoveStatic(
        VehicleController vehicle,
        Vector2Int fromPosition,
        Vector2Int toPosition
    )
    {
        if (instance == null)
        {
            instance = FindAnyObjectByType<GameplayUndoManager>();
        }

        instance?.RecordValidBoardMove(vehicle, fromPosition, toPosition);
    }

    public static void ClearHistoryStatic()
    {
        if (instance == null)
        {
            instance = FindAnyObjectByType<GameplayUndoManager>();
        }

        instance?.ClearHistory();
    }

    private bool ApplyUndo(UndoRecord record)
    {
        VehicleController vehicle = record.Vehicle;
        if (vehicle == null)
        {
            return false;
        }

        if (!vehicle.TryRestoreGridPosition(record.FromPosition))
        {
            return false;
        }

        if (gameManager != null)
        {
            gameManager.UndoRegisteredMove();
        }

        if (objectiveController == null)
        {
            objectiveController = FindAnyObjectByType<LevelObjectiveController>();
        }

        objectiveController?.UndoValidMove(vehicle);
        return true;
    }

    private bool ValidateRecord(UndoRecord record)
    {
        if (record.Vehicle == null || !record.Vehicle.isActiveAndEnabled)
        {
            return false;
        }

        if (record.Vehicle.GridPosition != record.ToPosition)
        {
            return false;
        }

        return true;
    }

    private bool EvaluateCanUndo()
    {
        if (history.Count == 0)
        {
            return false;
        }

        if (!IsLevelPlayableForUndo())
        {
            return false;
        }

        UndoRecord top = history[history.Count - 1];
        return ValidateRecord(top);
    }

    /// <summary>
    /// Undo alleen tijdens speelbare board-state.
    /// Classic = Inactive objective; specials = Running (WaitingToStart toegestaan voor Timed).
    /// </summary>
    private bool IsLevelPlayableForUndo()
    {
        if (gameManager != null)
        {
            if (gameManager.IsLevelCompleted ||
                gameManager.IsLevelFailed ||
                gameManager.IsTargetExitInProgress)
            {
                return false;
            }
        }

        if (objectiveController == null)
        {
            return true;
        }

        LevelObjectiveController.RuntimeState state = objectiveController.State;
        if (state == LevelObjectiveController.RuntimeState.Completed ||
            state == LevelObjectiveController.RuntimeState.Failed)
        {
            return false;
        }

        return true;
    }

    private void RefreshUndoAvailability(bool forceNotify)
    {
        bool canUndo = EvaluateCanUndo();

        if (undoButton != null)
        {
            // Zelfde UX als Hint: knop blijft bruikbaar bij insufficient coins
            // zolang er een undoable move is (feedback via SFX/event).
            undoButton.interactable = canUndo;
        }

        if (forceNotify || canUndo != lastKnownCanUndo)
        {
            lastKnownCanUndo = canUndo;
            OnUndoAvailabilityChanged?.Invoke(canUndo);
        }
    }
}

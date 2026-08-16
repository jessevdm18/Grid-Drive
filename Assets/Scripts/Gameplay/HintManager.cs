using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Hint via RushOutSolver vanaf ACTUELE runtime-bordstand.
/// Coins worden pas afgeschreven nadat een geldige first move gevonden is.
/// </summary>
public class HintManager : MonoBehaviour
{
    public enum HintResult
    {
        Success,
        NotEnoughCoins,
        SearchLimitReached,
        Unsolvable,
        InvalidRuntimeState,
        NoMoveFound,
        SpendFailed
    }

    private const int HintArrowSortingOrder = 20;

    [SerializeField] private CoinManager coinManager;
    [SerializeField] private AdsManager adsManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioManager audioManager;

    [SerializeField] private int hintCost = 100;

    [Tooltip("Ruimere search-limiet dan level-generation — mid-game 8x8 states.")]
    [SerializeField] private int hintSolverMaxStates = 200000;

    [Header("Status Message")]
    [SerializeField] private TextMeshProUGUI hintStatusText;

    [SerializeField] private float hintStatusDuration = 2f;

    [SerializeField, TextArea]
    private string hintUnavailableMessage = "HINT UNAVAILABLE\nNo coins spent";

    [SerializeField, TextArea]
    private string notEnoughCoinsMessage = "NOT ENOUGH COINS";

    [Header("Hint Visual")]
    [Tooltip("Fallback: highlight verdwijnt na deze tijd als hinted vehicle niet beweegt.")]
    [SerializeField] private float hintHighlightMaxDuration = 8f;

    [Tooltip("Pulse-frequentie in Hz (unscaled).")]
    [SerializeField] private float hintPulseSpeed = 1.2f;

    [SerializeField] private float hintArrowMinScale = 0.92f;
    [SerializeField] private float hintArrowMaxScale = 1.12f;

    [Tooltip("Max tint-mix naar highlightColor (0 = alleen origineel, 1 = vol highlight).")]
    [SerializeField, Range(0f, 1f)]
    private float maxHighlightStrength = 0.40f;

    [Tooltip("Heldere pulse-kleur; wordt gemengd met originele vehicle color.")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.95f, 0.35f, 1f);

    [SerializeField] private Sprite directionArrowSprite;
    [SerializeField] private float directionOffset = 0.85f;

    private Coroutine hintPulseCoroutine;
    private Coroutine hintStatusCoroutine;
    private Coroutine hintTimeoutCoroutine;

    // Runtime vehicle dat de huidige hint heeft (stabiele ActiveVehicles-index mapping).
    private VehicleController highlightedVehicle;
    private int highlightedVehicleIndex = -1;

    private VehicleController activeHintVehicle;

    private SpriteRenderer highlightedRenderer;
    private Color originalVehicleColor;
    private bool hasStoredOriginalColor;

    private Transform activeHintArrowTransform;
    private Vector3 originalArrowScale = Vector3.one;
    private bool hasStoredArrowScale;

    private bool isHintRequestInProgress;
    private bool isSubscribedToHighlightedVehicle;

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

        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioManager>();
        }

        if (hintStatusText != null)
        {
            hintStatusText.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        ClearCurrentHint();
    }

    private void OnDestroy()
    {
        ClearCurrentHint();
    }

    /// <summary>
    /// Knop-callback: check coins → solve → alleen bij succes SpendCoins + toon hint.
    /// </summary>
    public void UseHint()
    {
        if (isHintRequestInProgress)
        {
            Debug.Log("HintManager: hint request already in progress.");
            return;
        }

        if (coinManager == null)
        {
            Debug.LogError("HintManager: geen CoinManager gekoppeld.");
            return;
        }

        int coinsBefore = coinManager.GetCoins();
        if (!coinManager.CanAfford(hintCost))
        {
            LogHintRequest(
                HintResult.NotEnoughCoins,
                null,
                coinsBefore,
                coinsSpent: false,
                paidHint: true
            );
            Debug.Log("Not enough coins");
            audioManager?.PlayInsufficientCoins();
            ShowHintStatus(notEnoughCoinsMessage);
            return;
        }

        isHintRequestInProgress = true;

        try
        {
            HintComputeResult compute = TryComputeHint();

            if (compute.result != HintResult.Success)
            {
                LogHintRequest(
                    compute.result,
                    compute.solverResult,
                    coinsBefore,
                    coinsSpent: false,
                    paidHint: true
                );
                HandleHintFailure(compute.result, paidHint: true);
                return;
            }

            if (!coinManager.SpendCoins(hintCost))
            {
                Debug.LogWarning(
                    "HintManager: SpendCoins faalde onverwacht na CanAfford — geen hint getoond."
                );
                LogHintRequest(
                    HintResult.SpendFailed,
                    compute.solverResult,
                    coinsBefore,
                    coinsSpent: false,
                    paidHint: true
                );
                return;
            }

            // Alleen na echte afschrijving (niet bij shop — die gebruikt PlayUpgrade).
            audioManager?.PlayCoinSpend();

            LogHintRequest(
                HintResult.Success,
                compute.solverResult,
                coinsBefore,
                coinsSpent: true,
                paidHint: true
            );

            ShowHintVisual(compute.vehicle, compute.direction, compute.vehicleIndex);
        }
        finally
        {
            isHintRequestInProgress = false;
        }
    }

    /// <summary>
    /// Rewarded hint: eerst availability check, dan ad, daarna opnieuw solven + tonen.
    /// Geen coins.
    /// </summary>
    public void UseRewardedHint()
    {
        if (isHintRequestInProgress)
        {
            Debug.Log("HintManager: hint request already in progress.");
            return;
        }

        if (adsManager == null)
        {
            Debug.LogError("HintManager: geen AdsManager gekoppeld.");
            return;
        }

        isHintRequestInProgress = true;

        try
        {
            int coinsBefore = coinManager != null ? coinManager.GetCoins() : -1;
            HintComputeResult probe = TryComputeHint();
            LogHintRequest(probe.result, probe.solverResult, coinsBefore, false, false);

            if (probe.result != HintResult.Success)
            {
                HandleHintFailure(probe.result, paidHint: false);
                return;
            }

            if (!adsManager.IsRewardedAdReady())
            {
                Debug.LogWarning(
                    "HintManager: hint beschikbaar maar rewarded ad not ready — geen ad gestart."
                );
                ShowHintStatus(hintUnavailableMessage);
                return;
            }

            // Ad starten; na reward opnieuw solven (board kan veranderd zijn).
            adsManager.ShowRewardedAd(OnRewardedAdCompleted);
        }
        finally
        {
            // Ad is async — lock vrijgeven na probe; reward path heeft eigen guard.
            isHintRequestInProgress = false;
        }
    }

    private void OnRewardedAdCompleted()
    {
        if (isHintRequestInProgress)
        {
            return;
        }

        isHintRequestInProgress = true;

        try
        {
            int coinsBefore = coinManager != null ? coinManager.GetCoins() : -1;
            HintComputeResult compute = TryComputeHint();
            LogHintRequest(compute.result, compute.solverResult, coinsBefore, false, false);

            if (compute.result != HintResult.Success)
            {
                Debug.LogWarning(
                    "Hint failed after rewarded ad: " + compute.result +
                    " — ad was watched, no coins spent, no hint shown."
                );
                HandleHintFailure(compute.result, paidHint: false);
                return;
            }

            ShowHintVisual(compute.vehicle, compute.direction, compute.vehicleIndex);
        }
        finally
        {
            isHintRequestInProgress = false;
        }
    }

    public void ClearCurrentHint()
    {
        ClearCurrentHint("ManualClear");
    }

    public void ClearCurrentHint(string reason)
    {
        if (hintTimeoutCoroutine != null)
        {
            StopCoroutine(hintTimeoutCoroutine);
            hintTimeoutCoroutine = null;
        }

        if (hintPulseCoroutine != null)
        {
            StopCoroutine(hintPulseCoroutine);
            hintPulseCoroutine = null;
        }

        UnsubscribeFromHighlightedVehicle();

        HideHintDirection(activeHintVehicle);
        activeHintVehicle = null;
        activeHintArrowTransform = null;
        hasStoredArrowScale = false;

        RestorePreviousHighlight();

        if (highlightedVehicle != null || highlightedVehicleIndex >= 0)
        {
            Debug.Log("Hint cleared\nReason: " + reason);
        }

        highlightedVehicle = null;
        highlightedVehicleIndex = -1;
    }

    private struct HintComputeResult
    {
        public HintResult result;
        public VehicleController vehicle;
        public int vehicleIndex;
        public HintDirection direction;
        public RushOutSolver.SolverResult solverResult;
    }

    /// <summary>
    /// Snapshot + solve. Geen coins, geen visual (behalve clear van oude hint).
    /// </summary>
    private HintComputeResult TryComputeHint()
    {
        HintComputeResult outcome = new HintComputeResult
        {
            result = HintResult.InvalidRuntimeState,
            vehicle = null,
            vehicleIndex = -1,
            direction = HintDirection.Right,
            solverResult = null
        };

        ClearCurrentHint("NewHintRequested");

        if (!TryBuildCurrentSolverState(
                out List<VehicleController> controllers,
                out List<RushOutSolver.VehicleDefinition> definitions,
                out RushOutSolver.BoardState boardState,
                out int exitRow,
                out int gridWidth,
                out int gridHeight,
                out LevelData levelData))
        {
            outcome.result = HintResult.InvalidRuntimeState;
            return outcome;
        }

        int maxStates = Mathf.Max(1, hintSolverMaxStates);
        RushOutSolver.SolverResult result = RushOutSolver.Solve(
            definitions,
            boardState,
            exitRow,
            gridWidth,
            gridHeight,
            maxStates
        );

        outcome.solverResult = result;
        LogHintDiagnostics(
            controllers,
            definitions,
            boardState,
            levelData,
            gridWidth,
            gridHeight,
            result,
            maxStates
        );

        if (result.searchLimitReached)
        {
            outcome.result = HintResult.SearchLimitReached;
            return outcome;
        }

        if (!result.solvable)
        {
            outcome.result = HintResult.Unsolvable;
            return outcome;
        }

        if (result.solution == null || result.solution.Count == 0)
        {
            outcome.result = HintResult.NoMoveFound;
            return outcome;
        }

        RushOutSolver.SolverMove firstMove = result.solution[0];
        if (firstMove.vehicleIndex < 0 || firstMove.vehicleIndex >= controllers.Count)
        {
            outcome.result = HintResult.NoMoveFound;
            return outcome;
        }

        VehicleController vehicle = controllers[firstMove.vehicleIndex];
        if (vehicle == null || !vehicle.gameObject.activeInHierarchy)
        {
            outcome.result = HintResult.NoMoveFound;
            return outcome;
        }

        outcome.result = HintResult.Success;
        outcome.vehicle = vehicle;
        outcome.vehicleIndex = firstMove.vehicleIndex;
        outcome.direction = GetHintDirection(firstMove);
        return outcome;
    }

    private void HandleHintFailure(HintResult result, bool paidHint)
    {
        Debug.Log(
            "Hint failed: " + result +
            (paidHint ? " — no coins spent" : " — no coins spent (rewarded)")
        );

        if (result == HintResult.NotEnoughCoins)
        {
            ShowHintStatus(notEnoughCoinsMessage);
            return;
        }

        ShowHintStatus(hintUnavailableMessage);
    }

    private void ShowHintVisual(
        VehicleController vehicle,
        HintDirection direction,
        int vehicleIndex)
    {
        // Nieuwe hint vervangt altijd de oude (veilig bij dubbele calls).
        ClearCurrentHint("NewHintRequested");

        if (vehicle == null)
        {
            return;
        }

        // Succesvolle hint-presentatie (paid of rewarded).
        audioManager?.PlayHint();

        highlightedVehicle = vehicle;
        highlightedVehicleIndex = vehicleIndex;
        activeHintVehicle = vehicle;

        SubscribeToHighlightedVehicle();
        BeginHighlight(vehicle);
        ShowHintDirection(vehicle, direction);

        if (hintPulseCoroutine != null)
        {
            StopCoroutine(hintPulseCoroutine);
        }

        hintPulseCoroutine = StartCoroutine(HintPulseRoutine());

        Debug.Log(
            "=== HINT VISUAL ===\n" +
            "Vehicle: " + vehicle.name + "\n" +
            "Vehicle index: " + vehicleIndex + "\n" +
            "Max duration: " + hintHighlightMaxDuration + "\n" +
            "Pulse speed: " + hintPulseSpeed + "\n" +
            "Waiting for hinted vehicle movement"
        );

        if (hintTimeoutCoroutine != null)
        {
            StopCoroutine(hintTimeoutCoroutine);
        }

        hintTimeoutCoroutine = StartCoroutine(HintFallbackTimeoutRoutine());
    }

    /// <summary>
    /// Eén gedeelde pulse voor arrow scale + vehicle highlight (unscaledTime).
    /// </summary>
    private IEnumerator HintPulseRoutine()
    {
        while (highlightedVehicle != null)
        {
            float pulse =
                (Mathf.Sin(Time.unscaledTime * hintPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;

            if (hasStoredArrowScale && activeHintArrowTransform != null)
            {
                float scale = Mathf.Lerp(hintArrowMinScale, hintArrowMaxScale, pulse);
                activeHintArrowTransform.localScale = originalArrowScale * scale;
            }

            if (highlightedRenderer != null && hasStoredOriginalColor)
            {
                float strength = pulse * maxHighlightStrength;
                highlightedRenderer.color = Color.Lerp(
                    originalVehicleColor,
                    highlightColor,
                    strength
                );
            }

            yield return null;
        }

        hintPulseCoroutine = null;
    }

    private IEnumerator HintFallbackTimeoutRoutine()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, hintHighlightMaxDuration));
        hintTimeoutCoroutine = null;
        ClearCurrentHint("Timeout");
    }

    private void SubscribeToHighlightedVehicle()
    {
        if (highlightedVehicle == null || isSubscribedToHighlightedVehicle)
        {
            return;
        }

        highlightedVehicle.OnVehicleMoved += OnHintedVehicleMoved;
        isSubscribedToHighlightedVehicle = true;
    }

    private void UnsubscribeFromHighlightedVehicle()
    {
        if (highlightedVehicle != null && isSubscribedToHighlightedVehicle)
        {
            highlightedVehicle.OnVehicleMoved -= OnHintedVehicleMoved;
        }

        isSubscribedToHighlightedVehicle = false;
    }

    private void OnHintedVehicleMoved(VehicleController movedVehicle)
    {
        if (highlightedVehicle == null)
        {
            return;
        }

        // Alleen de hinted auto mag zijn eigen highlight verwijderen.
        if (movedVehicle != highlightedVehicle)
        {
            return;
        }

        ClearCurrentHint("HintedVehicleMoved");
    }

    private void ShowHintStatus(string message)
    {
        if (hintStatusText == null)
        {
            Debug.Log("HintStatus: " + message);
            return;
        }

        if (hintStatusCoroutine != null)
        {
            StopCoroutine(hintStatusCoroutine);
            hintStatusCoroutine = null;
        }

        hintStatusCoroutine = StartCoroutine(ShowHintStatusRoutine(message));
    }

    private IEnumerator ShowHintStatusRoutine(string message)
    {
        hintStatusText.text = message;
        hintStatusText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, hintStatusDuration));

        if (hintStatusText != null)
        {
            hintStatusText.gameObject.SetActive(false);
        }

        hintStatusCoroutine = null;
    }

    private void LogHintRequest(
        HintResult result,
        RushOutSolver.SolverResult solverResult,
        int coinsBefore,
        bool coinsSpent,
        bool paidHint)
    {
        StringBuilder log = new StringBuilder(512);
        log.AppendLine("=== HINT REQUEST ===");
        log.AppendLine("Paid hint: " + paidHint);
        log.AppendLine("Cost: " + hintCost);
        log.AppendLine("Coins before: " + coinsBefore);

        if (solverResult != null)
        {
            log.AppendLine("Solver:");
            log.AppendLine("solvable = " + solverResult.solvable);
            log.AppendLine("searchLimitReached = " + solverResult.searchLimitReached);
            log.AppendLine("statesExplored = " + solverResult.statesExplored);
            log.AppendLine(
                "solutionMoves = " +
                (solverResult.solution != null ? solverResult.solution.Count : 0)
            );
        }
        else
        {
            log.AppendLine("Solver: (not run / invalid state)");
        }

        log.AppendLine("Result: " + result);
        log.AppendLine("Coins spent: " + coinsSpent);
        log.AppendLine(
            "Coins after: " + (coinManager != null ? coinManager.GetCoins() : -1)
        );
        Debug.Log(log.ToString());
    }

    private bool TryBuildCurrentSolverState(
        out List<VehicleController> controllers,
        out List<RushOutSolver.VehicleDefinition> definitions,
        out RushOutSolver.BoardState boardState,
        out int exitRow,
        out int gridWidth,
        out int gridHeight,
        out LevelData levelData)
    {
        controllers = new List<VehicleController>();
        definitions = new List<RushOutSolver.VehicleDefinition>();
        boardState = null;
        exitRow = 0;
        gridWidth = RushOutSolver.DefaultGridSize;
        gridHeight = RushOutSolver.DefaultGridSize;
        levelData = levelManager != null ? levelManager.CurrentLevelData : null;

        if (levelData == null)
        {
            Debug.LogWarning("HintManager: geen CurrentLevelData beschikbaar.");
            return false;
        }

        exitRow = levelData.exitRow;
        gridWidth = levelData.ResolvedGridWidth;
        gridHeight = levelData.ResolvedGridHeight;

        IReadOnlyList<VehicleController> runtimeVehicles =
            levelManager != null ? levelManager.ActiveVehicles : null;

        if (runtimeVehicles == null || runtimeVehicles.Count == 0)
        {
            Debug.LogWarning(
                "HintManager: ActiveVehicles leeg — fallback FindObjectsByType."
            );
            runtimeVehicles = FindObjectsByType<VehicleController>(FindObjectsSortMode.None);
        }

        for (int i = 0; i < runtimeVehicles.Count; i++)
        {
            VehicleController vehicle = runtimeVehicles[i];
            if (vehicle == null || !vehicle.gameObject.activeInHierarchy)
            {
                continue;
            }

            controllers.Add(vehicle);
            definitions.Add(new RushOutSolver.VehicleDefinition
            {
                name = vehicle.gameObject.name,
                orientation = vehicle.Orientation,
                lengthInCells = vehicle.LengthInCells,
                canExitRight = vehicle.CanExitRight
            });
        }

        if (controllers.Count == 0)
        {
            Debug.LogWarning("HintManager: geen actieve voertuigen gevonden.");
            return false;
        }

        Vector2Int[] positions = new Vector2Int[controllers.Count];
        for (int i = 0; i < controllers.Count; i++)
        {
            positions[i] = controllers[i].GridPosition;
        }

        boardState = new RushOutSolver.BoardState(positions);
        return true;
    }

    private void LogHintDiagnostics(
        List<VehicleController> controllers,
        List<RushOutSolver.VehicleDefinition> definitions,
        RushOutSolver.BoardState boardState,
        LevelData levelData,
        int gridWidth,
        int gridHeight,
        RushOutSolver.SolverResult result,
        int maxStates)
    {
        StringBuilder log = new StringBuilder(2048);
        int moveCount = gameManager != null ? gameManager.CurrentMoves : -1;

        log.AppendLine("=== HINT SOLVER ===");
        log.AppendLine("Current move count: " + moveCount);
        log.AppendLine("Grid: " + gridWidth + "x" + gridHeight);
        log.AppendLine("Runtime vehicles: " + controllers.Count);
        log.AppendLine("hintSolverMaxStates: " + maxStates);

        for (int i = 0; i < controllers.Count; i++)
        {
            VehicleController v = controllers[i];
            Vector2Int pos = boardState.positions[i];
            RushOutSolver.VehicleDefinition def = definitions[i];
            log.AppendLine(
                "[" + i + "] name=" + v.name +
                " pos=(" + pos.x + "," + pos.y + ")" +
                " orientation=" + def.orientation +
                " length=" + def.lengthInCells +
                " target=" + def.canExitRight
            );
        }

        log.AppendLine("Solver result:");
        log.AppendLine("solvable = " + result.solvable);
        log.AppendLine("searchLimitReached = " + result.searchLimitReached);
        if (result.searchLimitReached)
        {
            log.AppendLine("searchLimitReason = " + result.searchLimitReason);
        }

        log.AppendLine("statesExplored = " + result.statesExplored);
        log.AppendLine("discoveredStates = " + result.discoveredStates);
        log.AppendLine(
            "solutionMoves = " +
            (result.solution != null ? result.solution.Count : 0)
        );

        if (result.solvable && result.solution != null && result.solution.Count > 0)
        {
            RushOutSolver.SolverMove first = result.solution[0];
            log.AppendLine("First hint:");
            log.AppendLine("vehicle index = " + first.vehicleIndex);
            log.AppendLine(
                "from = (" + first.fromPosition.x + "," + first.fromPosition.y + ")"
            );
            log.AppendLine(
                "to = (" + first.toPosition.x + "," + first.toPosition.y + ")"
            );
            log.AppendLine("exitsBoard = " + first.exitsBoard);
            log.AppendLine("direction = " + GetHintDirection(first));
        }
        else if (!result.solvable && !result.searchLimitReached && levelData != null)
        {
            log.AppendLine("--- FAIL DIAGNOSTIC: LevelData start vs runtime ---");
            int count = Mathf.Min(
                controllers.Count,
                levelData.vehicles != null ? levelData.vehicles.Count : 0
            );
            for (int i = 0; i < count; i++)
            {
                VehicleData data = levelData.vehicles[i];
                Vector2Int runtimePos = boardState.positions[i];
                log.AppendLine(
                    "[" + i + "] LevelData start=(" +
                    data.gridPosition.x + "," + data.gridPosition.y + ")" +
                    " runtime=(" + runtimePos.x + "," + runtimePos.y + ")" +
                    " same=" + (data.gridPosition == runtimePos)
                );
            }
        }

        Debug.Log(log.ToString());
    }

    private enum HintDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    private static HintDirection GetHintDirection(RushOutSolver.SolverMove move)
    {
        if (move.exitsBoard)
        {
            return HintDirection.Right;
        }

        Vector2Int delta = move.toPosition - move.fromPosition;

        if (delta.x > 0) return HintDirection.Right;
        if (delta.x < 0) return HintDirection.Left;
        if (delta.y > 0) return HintDirection.Up;
        if (delta.y < 0) return HintDirection.Down;

        return HintDirection.Right;
    }

    private void BeginHighlight(VehicleController vehicle)
    {
        RestorePreviousHighlight();

        if (vehicle == null)
        {
            return;
        }

        SpriteRenderer targetRenderer = vehicle.VisualSpriteRenderer;
        if (targetRenderer == null)
        {
            return;
        }

        highlightedRenderer = targetRenderer;
        originalVehicleColor = targetRenderer.color;
        hasStoredOriginalColor = true;

        Debug.Log(
            "HINT VISUAL: highlight vehicle " + vehicle.name +
            ", originalColor=" + originalVehicleColor +
            ", highlightColor=" + highlightColor +
            ", maxStrength=" + maxHighlightStrength
        );
    }

    private void RestorePreviousHighlight()
    {
        if (highlightedRenderer != null && hasStoredOriginalColor)
        {
            Debug.Log(
                "HINT VISUAL: restore vehicle " + highlightedRenderer.gameObject.name +
                " to " + originalVehicleColor
            );

            highlightedRenderer.color = originalVehicleColor;
        }

        highlightedRenderer = null;
        hasStoredOriginalColor = false;
    }

    private void ShowHintDirection(VehicleController vehicle, HintDirection direction)
    {
        GameObject hintObject = vehicle.HintDirection;
        SpriteRenderer arrowRenderer = vehicle.HintDirectionRenderer;

        if (hintObject == null || arrowRenderer == null)
        {
            Debug.LogWarning(
                "HintManager: HintDirection-refs ontbreken op " + vehicle.name
            );
            return;
        }

        hintObject.SetActive(true);

        // Basis-scale van prefab — pulse rekent altijd vanaf deze originele scale.
        originalArrowScale = vehicle.HintDirectionBaseScale;
        activeHintArrowTransform = hintObject.transform;
        activeHintArrowTransform.localScale = originalArrowScale;
        hasStoredArrowScale = true;

        Color c = arrowRenderer.color;
        c.a = 1f;
        arrowRenderer.color = c;

        if (directionArrowSprite != null)
        {
            arrowRenderer.sprite = directionArrowSprite;
        }

        if (vehicle.VisualSpriteRenderer != null)
        {
            arrowRenderer.sortingOrder =
                vehicle.VisualSpriteRenderer.sortingOrder + HintArrowSortingOrder;
        }
        else
        {
            arrowRenderer.sortingOrder = HintArrowSortingOrder;
        }

        ApplyHintArrowFacing(hintObject.transform, arrowRenderer, direction);
    }

    private static void ApplyHintArrowFacing(
        Transform hintTransform,
        SpriteRenderer arrowRenderer,
        HintDirection direction)
    {
        arrowRenderer.flipX = false;
        arrowRenderer.flipY = false;

        switch (direction)
        {
            case HintDirection.Right:
                hintTransform.localRotation = Quaternion.identity;
                break;

            case HintDirection.Left:
                hintTransform.localRotation = Quaternion.identity;
                arrowRenderer.flipX = true;
                break;

            case HintDirection.Up:
                hintTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                break;

            case HintDirection.Down:
                hintTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                arrowRenderer.flipX = true;
                break;
        }
    }

    private void HideHintDirection(VehicleController vehicle)
    {
        if (vehicle == null)
        {
            return;
        }

        GameObject hintObject = vehicle.HintDirection;
        SpriteRenderer arrowRenderer = vehicle.HintDirectionRenderer;

        if (arrowRenderer != null)
        {
            arrowRenderer.flipX = false;
            arrowRenderer.flipY = false;
        }

        if (hintObject != null)
        {
            // Altijd terug naar opgeslagen/base scale (niet mid-pulse laten hangen).
            if (hasStoredArrowScale)
            {
                hintObject.transform.localScale = originalArrowScale;
            }
            else
            {
                hintObject.transform.localScale = vehicle.HintDirectionBaseScale;
            }

            hintObject.transform.localRotation = Quaternion.identity;
            hintObject.SetActive(false);
        }
    }
}

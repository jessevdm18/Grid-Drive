using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hint via RushOutSolver: toont de eerste zet van een minimale oplossing
/// vanaf de ACTUELE bordstand (highlight + richting).
/// </summary>
public class HintManager : MonoBehaviour
{
    private const int HintMaxStates = 50000;

    // Hint-pijl altijd boven CarSprite (CarSprite sortingOrder = 0 in prefab).
    private const int HintArrowSortingOrder = 20;

    [SerializeField] private CoinManager coinManager;
    [SerializeField] private AdsManager adsManager;
    [SerializeField] private LevelManager levelManager;

    [SerializeField] private int hintCost = 100;

    [Tooltip("Hoe lang de highlight zichtbaar blijft (seconden).")]
    [SerializeField] private float highlightDuration = 1f;

    [Tooltip("Kleur tijdens de hint-highlight.")]
    [SerializeField] private Color highlightColor = Color.yellow;

    [Tooltip("Optionele pijl-sprite; leeg = sprite op prefab HintDirection.")]
    [SerializeField] private Sprite directionArrowSprite;

    [Tooltip("Niet meer gebruikt: HintDirection behoudt prefab localPosition/scale.")]
    [SerializeField] private float directionOffset = 0.85f;

    // Voorkomt dat meerdere visual-coroutines door elkaar lopen.
    private Coroutine hintVisualCoroutine;

    // Laatst getoonde pijl (zodat we die kunnen uitzetten bij een nieuwe hint).
    private VehicleController activeHintVehicle;

    private void Awake()
    {
        if (levelManager == null)
        {
            levelManager = FindFirstObjectByType<LevelManager>();
        }
    }

    /// <summary>
    /// Knop-callback: betaal met coins en toon een hint.
    /// </summary>
    public void UseHint()
    {
        if (coinManager == null)
        {
            Debug.LogError("HintManager: geen CoinManager gekoppeld.");
            return;
        }

        if (!coinManager.SpendCoins(hintCost))
        {
            Debug.Log("Not enough coins");
            return;
        }

        GiveHint();
    }

    /// <summary>
    /// Knop-callback: toon een rewarded ad. Hint alleen via de reward-callback.
    /// </summary>
    public void UseRewardedHint()
    {
        if (adsManager == null)
        {
            Debug.LogError("HintManager: geen AdsManager gekoppeld.");
            return;
        }

        adsManager.ShowRewardedAd(() => GiveHint());
    }

    /// <summary>
    /// Berekent de optimale volgende zet vanaf de actuele state en toont die visueel.
    /// Beweegt voertuigen NIET.
    /// </summary>
    private void GiveHint()
    {
        if (!TryBuildCurrentSolverInput(
                out List<VehicleController> controllers,
                out List<RushOutSolver.VehicleDefinition> definitions,
                out RushOutSolver.BoardState boardState,
                out int exitRow,
                out int gridWidth,
                out int gridHeight))
        {
            return;
        }

        RushOutSolver.SolverResult result = RushOutSolver.Solve(
            definitions,
            boardState,
            exitRow,
            gridWidth,
            gridHeight,
            HintMaxStates
        );

        if (result.searchLimitReached)
        {
            Debug.LogWarning("Hint solver search limit reached");
            return;
        }

        if (!result.solvable)
        {
            Debug.LogWarning("No solution found from current board state");
            return;
        }

        if (result.solution == null || result.solution.Count == 0)
        {
            return;
        }

        RushOutSolver.SolverMove firstMove = result.solution[0];
        if (firstMove.vehicleIndex < 0 || firstMove.vehicleIndex >= controllers.Count)
        {
            Debug.LogWarning("HintManager: ongeldige vehicleIndex in solver-move.");
            return;
        }

        VehicleController vehicle = controllers[firstMove.vehicleIndex];
        if (vehicle == null || !vehicle.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("HintManager: hint-voertuig is niet beschikbaar.");
            return;
        }

        HintDirection direction = GetHintDirection(firstMove);

        // Nieuwe hint vervangt een lopende visual (solver/coins/ads ongemoeid).
        if (hintVisualCoroutine != null)
        {
            StopCoroutine(hintVisualCoroutine);
            hintVisualCoroutine = null;
            HideHintDirection(activeHintVehicle);
            activeHintVehicle = null;
        }

        hintVisualCoroutine = StartCoroutine(ShowHintFeedback(vehicle, direction));
    }

    /// <summary>
    /// Bouwt solver-input uit actieve VehicleControllers (actuele posities).
    /// </summary>
    private bool TryBuildCurrentSolverInput(
        out List<VehicleController> controllers,
        out List<RushOutSolver.VehicleDefinition> definitions,
        out RushOutSolver.BoardState boardState,
        out int exitRow,
        out int gridWidth,
        out int gridHeight)
    {
        controllers = new List<VehicleController>();
        definitions = new List<RushOutSolver.VehicleDefinition>();
        boardState = null;
        exitRow = 0;
        gridWidth = RushOutSolver.DefaultGridSize;
        gridHeight = RushOutSolver.DefaultGridSize;

        LevelData levelData = levelManager != null ? levelManager.CurrentLevelData : null;
        if (levelData == null)
        {
            Debug.LogWarning("HintManager: geen CurrentLevelData beschikbaar.");
            return false;
        }

        exitRow = levelData.exitRow;
        gridWidth = levelData.ResolvedGridWidth;
        gridHeight = levelData.ResolvedGridHeight;

        VehicleController[] found = FindObjectsByType<VehicleController>(FindObjectsSortMode.None);
        foreach (VehicleController vehicle in found)
        {
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

    /// <summary>
    /// Pulse-kleur + prefab HintDirection-pijl ("deze auto → deze kant").
    /// </summary>
    private IEnumerator ShowHintFeedback(VehicleController vehicle, HintDirection direction)
    {
        activeHintVehicle = vehicle;

        Debug.Log("Showing hint on: " + vehicle.name);
        Debug.Log("Hint direction: " + direction);

        // CarSprite via serialized ref — niet GetComponentInChildren (root heeft disabled SR).
        SpriteRenderer spriteRenderer = vehicle.VisualSpriteRenderer;
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        ShowHintDirection(vehicle, direction);

        GameObject hintObject = vehicle.HintDirection;
        Debug.Log(
            "HintDirection active: " +
            (hintObject != null && hintObject.activeSelf)
        );

        float elapsed = 0f;
        while (elapsed < highlightDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 4f, 1f);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(originalColor, highlightColor, eased);
            }

            yield return null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        HideHintDirection(vehicle);
        activeHintVehicle = null;
        hintVisualCoroutine = null;
    }

    /// <summary>
    /// Zet prefab-HintDirection aan via VehicleController-refs (werkt ook als inactive).
    /// localPosition blijft onaangeroerd; scale/alpha worden gereset.
    /// </summary>
    private void ShowHintDirection(VehicleController vehicle, HintDirection direction)
    {
        GameObject hintObject = vehicle.HintDirection;
        SpriteRenderer arrowRenderer = vehicle.HintDirectionRenderer;

        if (hintObject == null || arrowRenderer == null)
        {
            Debug.LogWarning(
                "HintManager: HintDirection-refs ontbreken op " + vehicle.name +
                " (koppel ze in de prefab Inspector)."
            );
            return;
        }

        // 1) Activeren
        hintObject.SetActive(true);

        // 2) Scale + alpha resetten vóór facing
        hintObject.transform.localScale = vehicle.HintDirectionBaseScale;

        Color c = arrowRenderer.color;
        c.a = 1f;
        arrowRenderer.color = c;

        if (directionArrowSprite != null)
        {
            arrowRenderer.sprite = directionArrowSprite;
        }

        // Boven CarSprite tekenen
        if (vehicle.VisualSpriteRenderer != null)
        {
            arrowRenderer.sortingOrder =
                vehicle.VisualSpriteRenderer.sortingOrder + HintArrowSortingOrder;
        }
        else
        {
            arrowRenderer.sortingOrder = HintArrowSortingOrder;
        }

        // 3) Pas daarna richting/flip
        ApplyHintArrowFacing(hintObject.transform, arrowRenderer, direction);
    }

    /// <summary>
    /// RIGHT/LEFT: Z=0 + optioneel flipX.
    /// UP/DOWN: Z=90 + voor DOWN flipX (local X → world Y).
    /// Geen 180° rotatie. Flip deactiveert het GameObject nooit.
    /// </summary>
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
                // Na Z=90° wisselt flipX omhoog ↔ omlaag (geen 180°).
                hintTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                arrowRenderer.flipX = true;
                break;
        }
    }

    /// <summary>
    /// Zet de prefab-pijl uit en reset flip/rotatie (geen Destroy).
    /// </summary>
    private static void HideHintDirection(VehicleController vehicle)
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
            hintObject.transform.localRotation = Quaternion.identity;
            hintObject.SetActive(false);
        }
    }
}

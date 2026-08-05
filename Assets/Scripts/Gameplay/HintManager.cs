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

    [SerializeField] private CoinManager coinManager;
    [SerializeField] private AdsManager adsManager;
    [SerializeField] private LevelManager levelManager;

    [SerializeField] private int hintCost = 100;

    [Tooltip("Hoe lang de highlight zichtbaar blijft (seconden).")]
    [SerializeField] private float highlightDuration = 1f;

    [Tooltip("Kleur tijdens de hint-highlight.")]
    [SerializeField] private Color highlightColor = Color.yellow;

    [Tooltip("Optionele pijl-sprite voor de richting-indicator.")]
    [SerializeField] private Sprite directionArrowSprite;

    [Tooltip("Afstand van de pijl t.o.v. het voertuig-midden (world units).")]
    [SerializeField] private float directionOffset = 0.85f;

    // Voorkomt dat meerdere hints tegelijk lopen.
    private bool isHighlighting;

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
        if (isHighlighting)
        {
            Debug.Log("HintManager: hint is al bezig.");
            return;
        }

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
        if (isHighlighting)
        {
            Debug.Log("HintManager: hint is al bezig.");
            return;
        }

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
                out int exitRow))
        {
            return;
        }

        RushOutSolver.SolverResult result = RushOutSolver.Solve(
            definitions,
            boardState,
            exitRow,
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
        StartCoroutine(ShowHintFeedback(vehicle, direction));
    }

    /// <summary>
    /// Bouwt solver-input uit actieve VehicleControllers (actuele posities).
    /// </summary>
    private bool TryBuildCurrentSolverInput(
        out List<VehicleController> controllers,
        out List<RushOutSolver.VehicleDefinition> definitions,
        out RushOutSolver.BoardState boardState,
        out int exitRow)
    {
        controllers = new List<VehicleController>();
        definitions = new List<RushOutSolver.VehicleDefinition>();
        boardState = null;
        exitRow = 0;

        LevelData levelData = levelManager != null ? levelManager.CurrentLevelData : null;
        if (levelData == null)
        {
            Debug.LogWarning("HintManager: geen CurrentLevelData beschikbaar.");
            return false;
        }

        exitRow = levelData.exitRow;

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
    /// Pulse-kleur ~1s + tijdelijke richtingspijl ("deze auto → deze kant").
    /// </summary>
    private IEnumerator ShowHintFeedback(VehicleController vehicle, HintDirection direction)
    {
        isHighlighting = true;

        SpriteRenderer spriteRenderer = vehicle.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("HintManager: geen SpriteRenderer op " + vehicle.name);
            isHighlighting = false;
            yield break;
        }

        Color originalColor = spriteRenderer.color;
        GameObject arrow = CreateDirectionIndicator(vehicle, direction);

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

        if (arrow != null)
        {
            Destroy(arrow);
        }

        isHighlighting = false;
    }

    /// <summary>
    /// Maakt een eenvoudige pijl-child die de optimale richting toont.
    /// </summary>
    private GameObject CreateDirectionIndicator(VehicleController vehicle, HintDirection direction)
    {
        GameObject arrow = new GameObject("HintDirection");
        arrow.transform.SetParent(vehicle.transform, worldPositionStays: false);

        Vector3 localOffset = Vector3.zero;
        float zRotation = 0f;

        switch (direction)
        {
            case HintDirection.Right:
                localOffset = new Vector3(directionOffset, 0f, 0f);
                zRotation = 0f;
                break;
            case HintDirection.Left:
                localOffset = new Vector3(-directionOffset, 0f, 0f);
                zRotation = 180f;
                break;
            case HintDirection.Up:
                localOffset = new Vector3(0f, directionOffset, 0f);
                zRotation = 90f;
                break;
            case HintDirection.Down:
                localOffset = new Vector3(0f, -directionOffset, 0f);
                zRotation = -90f;
                break;
        }

        // Compenseer parent-scale zodat de pijl ongeveer even groot blijft.
        Vector3 parentScale = vehicle.transform.localScale;
        float sx = Mathf.Abs(parentScale.x) < 0.0001f ? 1f : parentScale.x;
        float sy = Mathf.Abs(parentScale.y) < 0.0001f ? 1f : parentScale.y;

        arrow.transform.localPosition = new Vector3(
            localOffset.x / sx,
            localOffset.y / sy,
            -0.1f
        );
        arrow.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
        arrow.transform.localScale = new Vector3(0.45f / sx, 0.45f / sy, 1f);

        SpriteRenderer arrowRenderer = arrow.AddComponent<SpriteRenderer>();
        arrowRenderer.sortingOrder = 50;
        arrowRenderer.color = highlightColor;

        if (directionArrowSprite != null)
        {
            arrowRenderer.sprite = directionArrowSprite;
        }
        else
        {
            // Fallback: witte unity-default sprite (quad) als pijlvorm-placeholder.
            arrowRenderer.sprite = CreateFallbackArrowSprite();
        }

        return arrow;
    }

    private static Sprite fallbackArrowSprite;

    private static Sprite CreateFallbackArrowSprite()
    {
        if (fallbackArrowSprite != null)
        {
            return fallbackArrowSprite;
        }

        // 8x8 witte texture — eenvoudige zichtbare indicator zonder asset.
        Texture2D tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        fallbackArrowSprite = Sprite.Create(
            tex,
            new Rect(0f, 0f, 8f, 8f),
            new Vector2(0.5f, 0.5f),
            8f
        );

        return fallbackArrowSprite;
    }
}

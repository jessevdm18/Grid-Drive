using UnityEngine;

/// <summary>
/// Past orthographic camera framing aan op het actieve grid (variabele WxH),
/// zodat bord + borders + exit tussen HUD/buttons zichtbaar blijven.
/// </summary>
public class CameraFitter : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("De orthographic camera die aangepast wordt (meestal Main Camera).")]
    [SerializeField] private Camera targetCamera;

    [Header("Board Padding")]
    [SerializeField] private float horizontalPadding = 1.0f;
    [SerializeField] private float verticalPadding = 1.0f;

    [Tooltip("Extra world-units rechts voor exit/curb.")]
    [SerializeField] private float rightExitPadding = 0.75f;

    [Tooltip("Totale extra world-space voor left+right borders (≈ 1 cell).")]
    [SerializeField] private float borderPaddingX = 1.0f;

    [Tooltip("Totale extra world-space voor top+bottom borders (≈ 1 cell).")]
    [SerializeField] private float borderPaddingY = 1.0f;

    [Header("UI Reserved Space")]
    [SerializeField] private float topReservedWorldSpace = 2.2f;
    [SerializeField] private float bottomReservedWorldSpace = 2.2f;

    [Header("Orthographic Limits")]
    [SerializeField] private float minOrthographicSize = 5f;
    [SerializeField] private float maxOrthographicSize = 9f;

    [Header("Framing")]
    [Tooltip("Verschuift camera Y t.o.v. grid-center (negatief = iets omlaag, meer ruimte voor top-HUD).")]
    [SerializeField] private float gameplayCenterYOffset = 0f;

    private float lastAspect = -1f;
    private int lastGridWidth = 6;
    private int lastGridHeight = 6;
    private float lastCellSize = 1f;
    private bool hasFit;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        // Fallback tot LevelManager FitToGrid aanroept.
        if (!hasFit)
        {
            FitToGrid(lastGridWidth, lastGridHeight, lastCellSize);
        }
    }

    private void Update()
    {
        if (targetCamera == null || !hasFit)
        {
            return;
        }

        float aspect = targetCamera.aspect;
        if (!Mathf.Approximately(aspect, lastAspect))
        {
            FitToGrid(lastGridWidth, lastGridHeight, lastCellSize);
        }
    }

    /// <summary>
    /// Frame de camera op het gegeven grid (na LevelData load / ParkingGridVisual build).
    /// </summary>
    public void FitToGrid(int gridWidth, int gridHeight, float cellSize)
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null || !targetCamera.orthographic)
        {
            Debug.LogWarning("CameraFitter: targetCamera ontbreekt of is niet orthographic.");
            return;
        }

        gridWidth = Mathf.Max(1, gridWidth);
        gridHeight = Mathf.Max(1, gridHeight);
        cellSize = Mathf.Max(0.01f, cellSize);

        lastGridWidth = gridWidth;
        lastGridHeight = gridHeight;
        lastCellSize = cellSize;
        hasFit = true;

        float aspect = targetCamera.aspect;
        lastAspect = aspect;

        // Board extents inclusief curb + exit + padding.
        float boardWidth =
            gridWidth * cellSize +
            borderPaddingX +
            horizontalPadding +
            rightExitPadding;

        float boardHeight =
            gridHeight * cellSize +
            borderPaddingY +
            verticalPadding +
            topReservedWorldSpace +
            bottomReservedWorldSpace;

        // Ortho: visible height = 2 * size, visible width = 2 * size * aspect
        float requiredForWidth = boardWidth / (2f * Mathf.Max(0.01f, aspect));
        float requiredForHeight = boardHeight / 2f;
        float requiredSize = Mathf.Max(requiredForWidth, requiredForHeight);

        targetCamera.orthographicSize = Mathf.Clamp(
            requiredSize,
            minOrthographicSize,
            maxOrthographicSize
        );

        // Grid is gecentreerd rond world origin (GridManager).
        Vector3 camPos = targetCamera.transform.position;
        camPos.x = 0f;
        camPos.y = gameplayCenterYOffset;
        targetCamera.transform.position = camPos;

        Debug.Log(
            "CameraFitter FitToGrid " + gridWidth + "x" + gridHeight +
            " | size=" + targetCamera.orthographicSize.ToString("0.00") +
            " | board=" + boardWidth.ToString("0.00") + "x" + boardHeight.ToString("0.00")
        );
    }
}

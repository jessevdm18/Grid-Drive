using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Enige eigenaar van gameplay orthographic framing.
/// Camera X = boardBounds.center.x (nooit exit).
/// ExitArrow telt volledig mee; ExitRoad alleen tot maxExitExtensionBeyondBoard.
/// </summary>
public class CameraFitter : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Board Visual Roots")]
    [Tooltip("ParkingGridVisual — tiles/borders/corners/gridlijnen (GEEN exit).")]
    [SerializeField] private Transform boardVisualRoot;

    [Tooltip("Exit root (road + arrow). Road telt beperkt mee; arrow volledig.")]
    [SerializeField] private Transform exitVisualRoot;

    [Tooltip("Exit road renderer — telt alleen beperkt mee voor zoom.")]
    [SerializeField] private Renderer exitRenderer;

    [Tooltip("Exit arrow renderer — altijd volledig zichtbaar.")]
    [SerializeField] private Renderer exitArrowRenderer;

    [Header("Padding")]
    [SerializeField] private float exitPadding = 0.15f;

    [Tooltip("Max world-units rechts van board.max.x die ExitRoad mag afdwingen.")]
    [SerializeField] private float maxExitExtensionBeyondBoard = 0.8f;

    [SerializeField] private float boardVerticalPadding = 0.15f;
    [SerializeField] private float safetyMultiplier = 1.03f;

    [Header("UI Reserved Viewport (normalized 0-1 of view height)")]
    [SerializeField, Range(0f, 0.45f)] private float topReservedFraction = 0.16f;
    [SerializeField, Range(0f, 0.45f)] private float bottomReservedFraction = 0.16f;

    [Header("Orthographic Limits")]
    [SerializeField] private float minOrthographicSize = 4.5f;

    [Tooltip("Mag 8x8+exit niet blokkeren. Scene had 9 → te laag.")]
    [SerializeField] private float maxOrthographicSize = 50f;

    [Header("Debug")]
    [SerializeField] private bool drawBoundsGizmo = true;
    [SerializeField] private bool verifyNoOverrideNextFrame = true;

    private int lastGridWidth = 6;
    private int lastGridHeight = 6;
    private float lastCellSize = 1f;
    private bool hasFit;

    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;
    private Rect lastSafeArea;

    private Bounds lastBoardBounds;
    private bool hasBoardBounds;

    private float lastAppliedOrthoSize;
    private float lastAppliedCameraX;
    private Coroutine verifyOverrideRoutine;

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

        // Fallback als LevelManager later fit; voorkomt Start-order races niet als primary.
        StartCoroutine(FitAfterFirstFrameFallback());
    }

    private IEnumerator FitAfterFirstFrameFallback()
    {
        yield return null;
        if (!hasFit)
        {
            FitToCurrentBoard();
        }
    }

    private void Update()
    {
        if (targetCamera == null || !hasFit)
        {
            return;
        }

        Rect safe = Screen.safeArea;
        if (Screen.width != lastScreenWidth ||
            Screen.height != lastScreenHeight ||
            !ApproximatelyRect(safe, lastSafeArea))
        {
            FitToCurrentBoard();
        }
    }

    public void FitToGrid(int gridWidth, int gridHeight, float cellSize)
    {
        lastGridWidth = Mathf.Max(1, gridWidth);
        lastGridHeight = Mathf.Max(1, gridHeight);
        lastCellSize = Mathf.Max(0.01f, cellSize);
        FitToCurrentBoard();
    }

    public void SetExitVisualRoot(Transform root)
    {
        exitVisualRoot = root;
    }

    public void SetBoardVisualRoot(Transform root)
    {
        if (root != null)
        {
            boardVisualRoot = root;
        }
    }

    /// <summary>
    /// Expliciete fit na volledige board + exit visual build.
    /// </summary>
    public void FitToCurrentBoard()
    {
        FitToVisualBounds();
    }

    public void FitToVisualBounds()
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

        StringBuilder log = new StringBuilder(2048);
        log.AppendLine("--- CAMERA FIT START ---");
        log.AppendLine("Screen = " + Screen.width + "x" + Screen.height);
        log.AppendLine("Camera aspect = " + targetCamera.aspect.ToString("0.0000"));
        log.AppendLine("Camera rect = " + targetCamera.rect);
        log.AppendLine("Camera pixelRect = " + targetCamera.pixelRect);
        log.AppendLine("Screen.safeArea = " + Screen.safeArea);

        if (!TryBuildBoardBounds(out Bounds boardBounds, out int boardRendererCount))
        {
            log.AppendLine("ERROR: geen board bounds (boardVisualRoot leeg/ontbreekt).");
            Debug.LogWarning(log.ToString());
            return;
        }

        lastBoardBounds = boardBounds;
        hasBoardBounds = true;

        log.AppendLine("Board renderer count = " + boardRendererCount);
        log.AppendLine("Board bounds min = " + boardBounds.min);
        log.AppendLine("Board bounds max = " + boardBounds.max);
        log.AppendLine("Board bounds center = " + boardBounds.center);
        log.AppendLine("Board bounds size = " + boardBounds.size);

        LogSingleRenderer(log, "Exit renderer (road)", exitRenderer);
        LogSingleRenderer(log, "Exit arrow renderer", exitArrowRenderer);

        List<Renderer> exitRootRenderers = CollectExitRootRenderers();
        log.AppendLine("ExitVisualRoot renderer count = " + exitRootRenderers.Count);
        for (int i = 0; i < exitRootRenderers.Count; i++)
        {
            Renderer r = exitRootRenderers[i];
            if (r == null)
            {
                continue;
            }

            log.AppendLine(
                "  Exit[" + i + "] path=" + GetHierarchyPath(r.transform) +
                " type=" + r.GetType().Name +
                " min.x=" + r.bounds.min.x.ToString("0.000") +
                " max.x=" + r.bounds.max.x.ToString("0.000")
            );
        }

        float boardCenterX = boardBounds.center.x;
        float boardHalfWidth = boardBounds.extents.x;
        float boardRight = boardBounds.max.x;

        float exitArrowRightX = float.NaN;
        float exitRoadActualRightX = float.NaN;
        float exitRoadLimitedRightX = float.NaN;

        float exitVisualRight = boardRight;

        // Arrow: altijd volledig.
        if (IsValidRenderer(exitArrowRenderer))
        {
            exitArrowRightX = exitArrowRenderer.bounds.max.x;
            exitVisualRight = Mathf.Max(exitVisualRight, exitArrowRightX);
        }

        // Road: beperkt tot boardRight + maxExitExtensionBeyondBoard.
        float roadCap = boardRight + Mathf.Max(0f, maxExitExtensionBeyondBoard);
        if (IsValidRenderer(exitRenderer))
        {
            exitRoadActualRightX = exitRenderer.bounds.max.x;
            exitRoadLimitedRightX = Mathf.Min(exitRoadActualRightX, roadCap);
            exitVisualRight = Mathf.Max(exitVisualRight, exitRoadLimitedRightX);
        }

        // Overige exit-root renderers (niet arrow): ook limited als road-achtig.
        for (int i = 0; i < exitRootRenderers.Count; i++)
        {
            Renderer r = exitRootRenderers[i];
            if (!IsValidRenderer(r))
            {
                continue;
            }

            if (r == exitArrowRenderer)
            {
                continue;
            }

            if (r == exitRenderer)
            {
                continue; // al verwerkt
            }

            float actualRight = r.bounds.max.x;
            float limitedRight = Mathf.Min(actualRight, roadCap);
            if (float.IsNaN(exitRoadActualRightX))
            {
                exitRoadActualRightX = actualRight;
                exitRoadLimitedRightX = limitedRight;
            }
            else
            {
                exitRoadActualRightX = Mathf.Max(exitRoadActualRightX, actualRight);
                exitRoadLimitedRightX = Mathf.Max(exitRoadLimitedRightX, limitedRight);
            }

            exitVisualRight = Mathf.Max(exitVisualRight, limitedRight);
        }

        float leftRequirement = boardCenterX - boardBounds.min.x;
        float rightRequirement = exitVisualRight - boardCenterX;
        float requiredHalfWidth = Mathf.Max(leftRequirement, rightRequirement);
        requiredHalfWidth += exitPadding;

        // Exact één keer: visibleHalfWidth = orthoSize * camera.aspect
        float availableAspect = Mathf.Max(0.01f, targetCamera.aspect);
        float requiredForWidth = requiredHalfWidth / availableAspect;

        float usableHeightFraction = Mathf.Clamp01(
            1f - topReservedFraction - bottomReservedFraction
        );
        usableHeightFraction = Mathf.Max(0.2f, usableHeightFraction);
        float requiredHalfHeight = boardBounds.extents.y + boardVerticalPadding;
        float requiredForHeight = requiredHalfHeight / usableHeightFraction;

        float requiredSizeBeforeClamp =
            Mathf.Max(requiredForWidth, requiredForHeight) * Mathf.Max(1f, safetyMultiplier);

        if (requiredSizeBeforeClamp > maxOrthographicSize + 0.0001f)
        {
            Debug.LogWarning(
                "CameraFitter: required size exceeds maxOrthographicSize (" +
                requiredSizeBeforeClamp.ToString("0.00") + " > " +
                maxOrthographicSize.ToString("0.00") + "). Raise maxOrthographicSize."
            );
        }

        float finalOrthographicSize = Mathf.Clamp(
            requiredSizeBeforeClamp,
            minOrthographicSize,
            maxOrthographicSize
        );

        targetCamera.orthographicSize = finalOrthographicSize;

        float playCenterOffsetY =
            finalOrthographicSize * (bottomReservedFraction - topReservedFraction);

        Vector3 camPos = targetCamera.transform.position;
        camPos.x = boardCenterX; // ABSOLUTE: nooit exit/combined center
        camPos.y = boardBounds.center.y - playCenterOffsetY;
        targetCamera.transform.position = camPos;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastSafeArea = Screen.safeArea;
        hasFit = true;
        lastAppliedOrthoSize = targetCamera.orthographicSize;
        lastAppliedCameraX = targetCamera.transform.position.x;

        log.AppendLine("Board Right X = " + boardRight.ToString("0.000"));
        log.AppendLine("ExitArrow Right X = " + FormatOptional(exitArrowRightX));
        log.AppendLine("ExitRoad Actual Right X = " + FormatOptional(exitRoadActualRightX));
        log.AppendLine("ExitRoad Limited Right X = " + FormatOptional(exitRoadLimitedRightX));
        log.AppendLine("Final Relevant Right X = " + exitVisualRight.ToString("0.000"));
        log.AppendLine("boardCenterX = " + boardCenterX.ToString("0.000"));
        log.AppendLine("boardHalfWidth = " + boardHalfWidth.ToString("0.000"));
        log.AppendLine("Required Half Width = " + requiredHalfWidth.ToString("0.000"));
        log.AppendLine("Available Aspect = " + availableAspect.ToString("0.0000"));
        log.AppendLine("Required For Width = " + requiredForWidth.ToString("0.000"));
        log.AppendLine("Required For Height = " + requiredForHeight.ToString("0.000"));
        log.AppendLine(
            "Required Size Before Clamp = " + requiredSizeBeforeClamp.ToString("0.000")
        );
        log.AppendLine("Final Orthographic Size = " + finalOrthographicSize.ToString("0.000"));
        log.AppendLine("cameraXAfter = " + camPos.x.ToString("0.000"));
        log.AppendLine("--- CAMERA FIT END ---");
        Debug.Log(log.ToString());

        if (verifyNoOverrideNextFrame)
        {
            if (verifyOverrideRoutine != null)
            {
                StopCoroutine(verifyOverrideRoutine);
            }

            verifyOverrideRoutine = StartCoroutine(VerifyNoOverrideNextFrame());
        }
    }

    private IEnumerator VerifyNoOverrideNextFrame()
    {
        float appliedSize = lastAppliedOrthoSize;
        float appliedX = lastAppliedCameraX;
        yield return null;

        if (targetCamera == null)
        {
            yield break;
        }

        float sizeNow = targetCamera.orthographicSize;
        float xNow = targetCamera.transform.position.x;

        Debug.Log(
            "Camera size one frame after fit: " + sizeNow.ToString("0.000") +
            " (applied " + appliedSize.ToString("0.000") + ")" +
            "\nCamera X one frame after fit: " + xNow.ToString("0.000") +
            " (applied " + appliedX.ToString("0.000") + ")"
        );

        if (!Mathf.Approximately(sizeNow, appliedSize))
        {
            Debug.LogError(
                "CameraFitter OVERRIDE DETECTED: orthographicSize changed after fit. " +
                "applied=" + appliedSize.ToString("0.000") +
                " now=" + sizeNow.ToString("0.000") +
                ". Zoek ander script dat Main Camera wijzigt."
            );
        }

        if (!Mathf.Approximately(xNow, appliedX))
        {
            Debug.LogError(
                "CameraFitter OVERRIDE DETECTED: camera X changed after fit. " +
                "applied=" + appliedX.ToString("0.000") +
                " now=" + xNow.ToString("0.000")
            );
        }
    }

    private List<Renderer> CollectExitRootRenderers()
    {
        List<Renderer> list = new List<Renderer>(16);

        if (exitVisualRoot != null)
        {
            Renderer[] found = exitVisualRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < found.Length; i++)
            {
                AddUnique(list, found[i]);
            }
        }

        return list;
    }

    private static string FormatOptional(float value)
    {
        return float.IsNaN(value) ? "n/a" : value.ToString("0.000");
    }

    private bool TryBuildBoardBounds(out Bounds bounds, out int rendererCount)
    {
        bounds = default;
        rendererCount = 0;

        if (boardVisualRoot == null)
        {
            return false;
        }

        Renderer[] found = boardVisualRoot.GetComponentsInChildren<Renderer>(true);
        bool initialized = false;

        for (int i = 0; i < found.Length; i++)
        {
            Renderer r = found[i];
            if (!IsValidRenderer(r))
            {
                continue;
            }

            if (!initialized)
            {
                bounds = r.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }

            rendererCount++;
        }

        return initialized;
    }

    private static bool IsValidRenderer(Renderer r)
    {
        return r != null &&
               r.enabled &&
               r.gameObject.activeInHierarchy &&
               r.bounds.size.sqrMagnitude >= 0.0001f;
    }

    private static void AddUnique(List<Renderer> list, Renderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == renderer)
            {
                return;
            }
        }

        list.Add(renderer);
    }

    private static void LogSingleRenderer(StringBuilder log, string label, Renderer r)
    {
        if (r == null)
        {
            log.AppendLine(label + ": null");
            return;
        }

        log.AppendLine(label + ":");
        log.AppendLine("  object name = " + r.gameObject.name);
        log.AppendLine("  bounds min = " + r.bounds.min);
        log.AppendLine("  bounds max = " + r.bounds.max);
        log.AppendLine("  bounds size = " + r.bounds.size);
    }

    private static string GetHierarchyPath(Transform t)
    {
        if (t == null)
        {
            return "(null)";
        }

        string path = t.name;
        Transform p = t.parent;
        while (p != null)
        {
            path = p.name + "/" + path;
            p = p.parent;
        }

        return path;
    }

    private static bool ApproximatelyRect(Rect a, Rect b)
    {
        return Mathf.Approximately(a.x, b.x) &&
               Mathf.Approximately(a.y, b.y) &&
               Mathf.Approximately(a.width, b.width) &&
               Mathf.Approximately(a.height, b.height);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawBoundsGizmo || !hasBoardBounds)
        {
            return;
        }

        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.95f);
        Gizmos.DrawWireCube(lastBoardBounds.center, lastBoardBounds.size);
    }
}

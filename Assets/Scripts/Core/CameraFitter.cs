using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Enige eigenaar van gameplay orthographic framing.
/// Phone: camera X = boardBounds.center.x (nooit exit); UI top/bottom reserved.
/// Wide tablet landscape: board framed in the right gameplay column (HUD on left).
/// ExitArrow telt volledig mee; ExitRoad alleen tot maxExitExtensionBeyondBoard.
///
/// Belangrijk: iedere FitToGrid zet orthographicSize ABSOLUUT (geen Mathf.Max op vorige size).
/// Board visuals moeten inactive/destroyed zijn vóór fit bij size-change (zie ParkingGridVisual).
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

    [Header("UI Reserved Viewport (phone — normalized 0-1 of view height)")]
    [SerializeField, Range(0f, 0.45f)] private float topReservedFraction = 0.16f;
    [SerializeField, Range(0f, 0.45f)] private float bottomReservedFraction = 0.16f;

    [Header("Wide Tablet Landscape")]
    [Tooltip("Must match GameplayLayoutController / GameplayLayoutMode.")]
    [SerializeField] private float wideAspectThreshold = GameplayLayoutMode.DefaultWideAspectThreshold;

    [Tooltip("Left HUD column width (0-1). Board uses the remaining width.")]
    [SerializeField, Range(0.22f, 0.36f)] private float leftHudWidthFraction = 0.30f;

    [Tooltip("Small vertical inset when HUD sits in the left column.")]
    [SerializeField, Range(0f, 0.2f)] private float wideTopReservedFraction = 0.04f;
    [SerializeField, Range(0f, 0.2f)] private float wideBottomReservedFraction = 0.04f;

    [Header("Compact Phone Portrait")]
    [Tooltip("Extra top UI reserve on shorter portrait phones.")]
    [SerializeField, Range(0f, 0.45f)] private float compactTopReservedFraction = 0.18f;
    [Tooltip("Extra bottom UI reserve on shorter portrait phones.")]
    [SerializeField, Range(0f, 0.45f)] private float compactBottomReservedFraction = 0.16f;

    [Header("Orthographic Limits")]
    [SerializeField] private float minOrthographicSize = 4.5f;

    [Tooltip("Mag 8x8+exit niet blokkeren. Scene had 9 → te laag.")]
    [SerializeField] private float maxOrthographicSize = 50f;

    [Header("Debug")]
    [SerializeField] private bool drawBoundsGizmo = true;
    [SerializeField] private bool verifyNoOverrideNextFrame = true;
    [SerializeField] private bool logVerboseFit = false;

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
    private Vector3 lastAppliedCameraPos;
    private Vector3 lastBoardCenter;

    private float baselineOrthographicSize;
    private Vector3 baselineCameraPosition;
    private bool hasBaseline;

    private int fitGeneration;
    private Coroutine verifyOverrideRoutine;
    private Coroutine deferredRefitRoutine;

    private bool hasLayoutKindOverride;
    private GameplayLayoutKind layoutKindOverride = GameplayLayoutKind.TallPhonePortrait;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        CacheBaselineIfNeeded();
    }

    private GameplayLayoutKind ResolveLayoutKind()
    {
        if (hasLayoutKindOverride)
        {
            return layoutKindOverride;
        }

        return GameplayLayoutMode.Resolve(
            GameplayLayoutMode.DefaultTallPhoneMaxAspect,
            wideAspectThreshold);
    }

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        CacheBaselineIfNeeded();

        // Fallback als LevelManager later fit; voorkomt Start-order races niet als primary.
        StartCoroutine(FitAfterFirstFrameFallback());
    }

    private void OnDisable()
    {
        StopFitCoroutines();
        if (targetCamera != null)
        {
            targetCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }
    }

    public bool TryGetBoardWorldTop(out float worldY)
    {
        worldY = 0f;
        if (!hasBoardBounds)
        {
            return false;
        }

        worldY = lastBoardBounds.max.y;
        return true;
    }

    public void ApplyModeCameraValues(
        GameplayLayoutKind kind,
        float topReserved,
        float bottomReserved,
        float leftHudFraction)
    {
        leftHudWidthFraction = Mathf.Clamp(leftHudFraction, 0.15f, 0.45f);

        switch (kind)
        {
            case GameplayLayoutKind.WideTabletLandscape:
                wideTopReservedFraction = Mathf.Clamp(topReserved, 0f, 0.45f);
                wideBottomReservedFraction = Mathf.Clamp(bottomReserved, 0f, 0.45f);
                break;
            case GameplayLayoutKind.CompactPhonePortrait:
                compactTopReservedFraction = Mathf.Clamp(topReserved, 0f, 0.45f);
                compactBottomReservedFraction = Mathf.Clamp(bottomReserved, 0f, 0.45f);
                break;
            default:
                topReservedFraction = Mathf.Clamp(topReserved, 0f, 0.45f);
                bottomReservedFraction = Mathf.Clamp(bottomReserved, 0f, 0.45f);
                break;
        }

        SetLayoutKind(kind, leftHudWidthFraction);
    }

    /// <summary>
    /// Called by GameplayLayoutController. Drives wide/compact/tall framing.
    /// Full camera rect is always kept (background is Screen Space–Camera).
    /// </summary>
    public void SetLayoutKind(GameplayLayoutKind kind, float leftHudFraction)
    {
        hasLayoutKindOverride = true;
        layoutKindOverride = kind;
        leftHudWidthFraction = Mathf.Clamp(leftHudFraction, 0.15f, 0.45f);

        if (targetCamera != null)
        {
            targetCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }

        if (hasFit)
        {
            FitToCurrentBoard();
        }
    }

    /// <summary>Backward-compatible wrapper.</summary>
    public void SetWideGameplayLayout(bool enabled, float leftHudFraction)
    {
        SetLayoutKind(
            enabled
                ? GameplayLayoutKind.WideTabletLandscape
                : GameplayLayoutMode.Resolve(
                    GameplayLayoutMode.DefaultTallPhoneMaxAspect,
                    wideAspectThreshold),
            leftHudFraction);
    }

    private void CacheBaselineIfNeeded()
    {
        if (hasBaseline || targetCamera == null)
        {
            return;
        }

        baselineOrthographicSize = targetCamera.orthographicSize;
        baselineCameraPosition = targetCamera.transform.position;
        hasBaseline = true;
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

        fitGeneration++;
        int generation = fitGeneration;

        // Immediate fit (ParkingGridVisual deactivates old tiles first).
        FitToVisualBounds();

        // End-of-frame refit after Destroy() completes — belt-and-suspenders for size changes.
        if (deferredRefitRoutine != null)
        {
            StopCoroutine(deferredRefitRoutine);
        }

        deferredRefitRoutine = StartCoroutine(DeferredRefitAfterDestroy(generation));
    }

    private IEnumerator DeferredRefitAfterDestroy(int generation)
    {
        yield return null;
        if (generation != fitGeneration)
        {
            yield break;
        }

        FitToVisualBounds();
        deferredRefitRoutine = null;
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

        CacheBaselineIfNeeded();

        // Never leave a cropped rect from experimental setups.
        targetCamera.rect = new Rect(0f, 0f, 1f, 1f);

        StringBuilder log = logVerboseFit ? new StringBuilder(2048) : null;
        if (log != null)
        {
            log.AppendLine("--- CAMERA FIT START ---");
            log.AppendLine("Screen = " + Screen.width + "x" + Screen.height);
            log.AppendLine("LayoutKind = " + ResolveLayoutKind());
            log.AppendLine("LeftHudFraction = " + leftHudWidthFraction.ToString("0.000"));
            log.AppendLine("Camera aspect = " + targetCamera.aspect.ToString("0.0000"));
            log.AppendLine("Camera rect = " + targetCamera.rect);
            log.AppendLine("Camera pixelRect = " + targetCamera.pixelRect);
            log.AppendLine("Screen.safeArea = " + Screen.safeArea);
        }

        if (!TryBuildBoardBounds(out Bounds boardBounds, out int boardRendererCount))
        {
            Debug.LogWarning(
                "CameraFitter: geen board bounds (boardVisualRoot leeg/ontbreekt)."
            );
            return;
        }

        lastBoardBounds = boardBounds;
        hasBoardBounds = true;
        lastBoardCenter = boardBounds.center;

        if (log != null)
        {
            log.AppendLine("Board renderer count = " + boardRendererCount);
            log.AppendLine("Board bounds min = " + boardBounds.min);
            log.AppendLine("Board bounds max = " + boardBounds.max);
            log.AppendLine("Board bounds center = " + boardBounds.center);
            log.AppendLine("Board bounds size = " + boardBounds.size);

            LogSingleRenderer(log, "Exit renderer (road)", exitRenderer);
            LogSingleRenderer(log, "Exit arrow renderer", exitArrowRenderer);
        }

        List<Renderer> exitRootRenderers = CollectExitRootRenderers();

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

            if (r == exitArrowRenderer || r == exitRenderer)
            {
                continue;
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

        float fullAspect = Mathf.Max(0.01f, targetCamera.aspect);
        GameplayLayoutKind layoutKind = ResolveLayoutKind();
        bool wideGameplayLayout = layoutKind == GameplayLayoutKind.WideTabletLandscape;
        float hudFraction = Mathf.Clamp(leftHudWidthFraction, 0.15f, 0.45f);
        float gameplayWidthFraction = wideGameplayLayout
            ? Mathf.Clamp01(1f - hudFraction)
            : 1f;
        gameplayWidthFraction = Mathf.Max(0.4f, gameplayWidthFraction);

        // Width limited to the usable gameplay column.
        float requiredForWidth = requiredHalfWidth / (fullAspect * gameplayWidthFraction);

        float topReserved;
        float bottomReserved;
        ResolveReservedFractions(layoutKind, out topReserved, out bottomReserved);
        ComposePhoneReservedWithSafeArea(layoutKind, ref topReserved, ref bottomReserved);

        float usableHeightFraction = Mathf.Clamp01(
            1f - topReserved - bottomReserved
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

        // ABSOLUTE per-level size — never Mathf.Max(currentOrtho, required).
        float finalOrthographicSize = Mathf.Clamp(
            requiredSizeBeforeClamp,
            minOrthographicSize,
            maxOrthographicSize
        );

        float playCenterOffsetY =
            finalOrthographicSize * (bottomReserved - topReserved);

        float visibleWidth = 2f * finalOrthographicSize * fullAspect;
        float cameraX = boardCenterX;
        float gameplayCenterScreenX = 0.5f;
        if (wideGameplayLayout)
        {
            // Place board center at the horizontal center of the right gameplay column.
            // e.g. hud=0.30 → screen X ≈ 0.65
            gameplayCenterScreenX = hudFraction + gameplayWidthFraction * 0.5f;
            cameraX = boardCenterX - (gameplayCenterScreenX - 0.5f) * visibleWidth;
        }

        Vector3 camPos = targetCamera.transform.position;
        camPos.x = cameraX;
        camPos.y = boardBounds.center.y - playCenterOffsetY;
        // Keep baseline Z if available.
        if (hasBaseline)
        {
            camPos.z = baselineCameraPosition.z;
        }

        targetCamera.orthographicSize = finalOrthographicSize;
        targetCamera.transform.position = camPos;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastSafeArea = Screen.safeArea;
        hasFit = true;
        lastAppliedOrthoSize = targetCamera.orthographicSize;
        lastAppliedCameraX = targetCamera.transform.position.x;
        lastAppliedCameraPos = targetCamera.transform.position;

        Debug.Log(
            "[LevelFraming]\n" +
            "Grid=" + lastGridWidth + "x" + lastGridHeight + "\n" +
            "Aspect=" + fullAspect.ToString("0.000") + "\n" +
            "LayoutKind=" + layoutKind + "\n" +
            "WideLayout=" + wideGameplayLayout + "\n" +
            "HudFraction=" + hudFraction.ToString("0.000") + "\n" +
            "GameplayCenterScreenX=" + gameplayCenterScreenX.ToString("0.000") + "\n" +
            "GameplayWidthFraction=" + gameplayWidthFraction.ToString("0.000") + "\n" +
            "TopReserved=" + topReserved.ToString("0.000") + "\n" +
            "BottomReserved=" + bottomReserved.ToString("0.000") + "\n" +
            "BaseOrtho=" +
            (hasBaseline ? baselineOrthographicSize.ToString("0.000") : "n/a") + "\n" +
            "RequiredOrtho=" + requiredSizeBeforeClamp.ToString("0.000") + "\n" +
            "AppliedOrtho=" + finalOrthographicSize.ToString("0.000") + "\n" +
            "BoardCenter=" + boardBounds.center + "\n" +
            "BoardSize=" + boardBounds.size + "\n" +
            "BoardRenderers=" + boardRendererCount + "\n" +
            "CameraPos=" + camPos
        );

        if (log != null)
        {
            log.AppendLine("Board Right X = " + boardRight.ToString("0.000"));
            log.AppendLine("ExitArrow Right X = " + FormatOptional(exitArrowRightX));
            log.AppendLine("ExitRoad Actual Right X = " + FormatOptional(exitRoadActualRightX));
            log.AppendLine("ExitRoad Limited Right X = " + FormatOptional(exitRoadLimitedRightX));
            log.AppendLine("Final Relevant Right X = " + exitVisualRight.ToString("0.000"));
            log.AppendLine("boardCenterX = " + boardCenterX.ToString("0.000"));
            log.AppendLine("boardHalfWidth = " + boardHalfWidth.ToString("0.000"));
            log.AppendLine("Required Half Width = " + requiredHalfWidth.ToString("0.000"));
            log.AppendLine("Full Aspect = " + fullAspect.ToString("0.0000"));
            log.AppendLine("Required For Width = " + requiredForWidth.ToString("0.000"));
            log.AppendLine("Required For Height = " + requiredForHeight.ToString("0.000"));
            log.AppendLine(
                "Required Size Before Clamp = " + requiredSizeBeforeClamp.ToString("0.000")
            );
            log.AppendLine("Final Orthographic Size = " + finalOrthographicSize.ToString("0.000"));
            log.AppendLine("cameraXAfter = " + camPos.x.ToString("0.000"));
            log.AppendLine("--- CAMERA FIT END ---");
            Debug.Log(log.ToString());
        }

        if (verifyNoOverrideNextFrame)
        {
            if (verifyOverrideRoutine != null)
            {
                StopCoroutine(verifyOverrideRoutine);
            }

            verifyOverrideRoutine = StartCoroutine(VerifyNoOverrideNextFrame(fitGeneration));
        }
    }

    private void ResolveReservedFractions(
        GameplayLayoutKind layoutKind,
        out float topReserved,
        out float bottomReserved)
    {
        switch (layoutKind)
        {
            case GameplayLayoutKind.WideTabletLandscape:
                topReserved = wideTopReservedFraction;
                bottomReserved = wideBottomReservedFraction;
                break;
            case GameplayLayoutKind.CompactPhonePortrait:
                topReserved = compactTopReservedFraction;
                bottomReserved = compactBottomReservedFraction;
                break;
            default:
                topReserved = topReservedFraction;
                bottomReserved = bottomReservedFraction;
                break;
        }
    }

    /// <summary>
    /// Phone only: map HUD reserved fractions into the SafeArea span and add
    /// unsafe chrome (status/nav). Editor with no insets is unchanged.
    /// Keeps board framing in the same vertical band as SafeArea UI — no device constants.
    /// </summary>
    private static void ComposePhoneReservedWithSafeArea(
        GameplayLayoutKind layoutKind,
        ref float topReserved,
        ref float bottomReserved)
    {
        if (layoutKind == GameplayLayoutKind.WideTabletLandscape)
        {
            return;
        }

        if (Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        Rect safe = Screen.safeArea;
        float safeBottom = Mathf.Clamp01(safe.yMin / Screen.height);
        float safeTop = Mathf.Clamp01(1f - safe.yMax / Screen.height);
        float safeSpan = Mathf.Max(0.2f, 1f - safeTop - safeBottom);

        topReserved = safeTop + Mathf.Clamp01(topReserved) * safeSpan;
        bottomReserved = safeBottom + Mathf.Clamp01(bottomReserved) * safeSpan;

        float sum = topReserved + bottomReserved;
        if (sum > 0.8f && sum > 0.0001f)
        {
            float scale = 0.8f / sum;
            topReserved *= scale;
            bottomReserved *= scale;
        }
    }

    private void StopFitCoroutines()
    {
        fitGeneration++;
        if (verifyOverrideRoutine != null)
        {
            StopCoroutine(verifyOverrideRoutine);
            verifyOverrideRoutine = null;
        }

        if (deferredRefitRoutine != null)
        {
            StopCoroutine(deferredRefitRoutine);
            deferredRefitRoutine = null;
        }
    }

    private IEnumerator VerifyNoOverrideNextFrame(int generation)
    {
        float appliedSize = lastAppliedOrthoSize;
        float appliedX = lastAppliedCameraX;
        yield return null;

        if (generation != fitGeneration || targetCamera == null)
        {
            yield break;
        }

        float sizeNow = targetCamera.orthographicSize;
        float xNow = targetCamera.transform.position.x;

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

        // includeInactive=true so we find objects, but IsValidRenderer skips inactive
        // (critical after ClearGeneratedVisuals deactivates old tiles pending Destroy).
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LevelSelect-only responsive presentation. Sole owner of GridLayoutGroup
/// presentation settings and Wide chrome/background. LevelSelectUI reads grid
/// and refreshes scroll height only.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-40)]
public class LevelSelectLayoutController : MonoBehaviour
{
    [Serializable]
    public struct WideElementLayout
    {
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector3 localScale;
        public bool hasSizeDelta;

        public static WideElementLayout From(Vector2 pos, Vector2 size, float scale)
        {
            return new WideElementLayout
            {
                anchoredPosition = pos,
                sizeDelta = size,
                localScale = Vector3.one * scale,
                hasSizeDelta = true
            };
        }

        public static WideElementLayout FromRect(RectTransform rt)
        {
            if (rt == null)
            {
                return default;
            }

            return new WideElementLayout
            {
                anchoredPosition = rt.anchoredPosition,
                sizeDelta = rt.sizeDelta,
                localScale = rt.localScale,
                hasSizeDelta = true
            };
        }
    }

    private struct RectSnap
    {
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 Pivot;
        public Vector2 AnchoredPosition;
        public Vector2 SizeDelta;
        public Vector3 LocalScale;
        public Quaternion LocalRotation;
        public bool Valid;
    }

    private struct GridSnap
    {
        public Vector2 CellSize;
        public Vector2 Spacing;
        public RectOffset Padding;
        public GridLayoutGroup.Constraint Constraint;
        public int ConstraintCount;
        public TextAnchor ChildAlignment;
        public bool Valid;
    }

    private struct TransformSnap
    {
        public Vector3 LocalPosition;
        public Vector3 LocalScale;
        public Quaternion LocalRotation;
        public bool Valid;
    }

    [Header("Detection")]
    [SerializeField] private float wideAspectThreshold = GameplayLayoutMode.DefaultWideAspectThreshold;
    [SerializeField] private float tallPhoneMaxAspect = GameplayLayoutMode.DefaultTallPhoneMaxAspect;

    [Header("Chrome")]
    [SerializeField] private Transform topBar;
    [SerializeField] private RectTransform titleText;
    [SerializeField] private RectTransform backButton;
    [SerializeField] private RectTransform difficultyTabs;
    [SerializeField] private RectTransform levelScrollView;
    [SerializeField] private RectTransform levelGrid;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private LevelSelectUI levelSelectUI;

    [Header("Background")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform backgroundOverlay;
    [SerializeField] private Image backgroundImage;

    [Header("Wide Tablet — Chrome")]
    [SerializeField] private WideElementLayout wideTitle =
        WideElementLayout.From(new Vector2(0f, 40f), new Vector2(900f, 100f), 0.55f);
    [Tooltip("TopBar-LOCAL anchoredPosition (BackButton stays under TopBar).")]
    [SerializeField] private Vector2 wideBackButtonPosition = new Vector2(-200f, 50f);
    [Tooltip("TopBar-LOCAL uniform-ish scale. Phone authored ≈ 0.41.")]
    [SerializeField] private Vector3 wideBackButtonScale = new Vector3(0.45f, 0.45f, 0.45f);
    [SerializeField] private WideElementLayout wideDifficultyTabs =
        WideElementLayout.From(new Vector2(0f, 380f), new Vector2(1400f, 160f), 1f);
    [Tooltip("Used when canvas size known; otherwise stretch fractions are used.")]
    [SerializeField] private WideElementLayout wideLevelScrollView =
        WideElementLayout.From(new Vector2(0f, -60f), new Vector2(1800f, 880f), 1f);
    [SerializeField] private Vector3 wideTopBarLocalPosition = new Vector3(0f, 420f, 0f);
    [SerializeField] private Vector3 wideTopBarLocalScale = new Vector3(1.6f, 1.6f, 1.6f);

    [Header("Wide Tablet — Scroll stretch (preferred over absolute size)")]
    [SerializeField] private Vector2 wideScrollAnchorMin = new Vector2(0.03f, 0.03f);
    [SerializeField] private Vector2 wideScrollAnchorMax = new Vector2(0.97f, 0.70f);

    [Header("Wide Tablet — Grid")]
    [SerializeField, Range(4, 8)] private int wideColumnCount = 6;
    [SerializeField] private Vector2 wideCellSize = new Vector2(200f, 200f);
    [SerializeField] private Vector2 wideSpacing = new Vector2(28f, 28f);
    [SerializeField] private int widePaddingLeft = 24;
    [SerializeField] private int widePaddingRight = 24;
    [SerializeField] private int widePaddingTop = 16;
    [SerializeField] private int widePaddingBottom = 16;

    [Header("Phone Stack (SafeArea-relative) — Tall + Compact only")]
    [Tooltip("SafeArea under LevelSelectCanvas. LivesHUD is parented here.")]
    [SerializeField] private RectTransform safeArea;
    [SerializeField] private RectTransform livesHud;
    [Tooltip("SafeArea-local: inset from SafeArea top down to the header row top.")]
    [SerializeField, Min(0f)] private float phoneHeaderTopPadding = 16f;
    [Tooltip("SafeArea-local gap between Back right edge and SELECT LEVEL left edge.")]
    [SerializeField, Min(0f)] private float phoneBackToTitleGap = 24f;
    [Tooltip("SafeArea-local gap between SELECT LEVEL right edge and LivesHUD left edge.")]
    [SerializeField, Min(0f)] private float phoneTitleToLivesGap = 24f;
    [Tooltip("Minimum SafeArea-local horizontal gap / side inset when fitting the header.")]
    [SerializeField, Min(0f)] private float phoneHeaderMinHorizontalGap = 8f;
    [Tooltip("SafeArea-local gap between header bottom and difficulty row top.")]
    [SerializeField, Min(0f)] private float phoneHeaderToDifficultyGap = 20f;
    [Tooltip("SafeArea-local gap between difficulty bottom and level scroll/grid top.")]
    [SerializeField, Min(0f)] private float phoneDifficultyToGridGap = 16f;
    [Tooltip("SafeArea-local inset above SafeArea bottom for the level scroll viewport.")]
    [SerializeField, Min(0f)] private float phoneGridBottomPadding = 16f;
    [Tooltip("Floor for uniform header fit scale (never shrink below this).")]
    [SerializeField, Range(0.5f, 1f)] private float phoneHeaderMinFitScale = 0.75f;

    private Vector3? chromeTopBarScaleOverride;

    private Coroutine authoritativeLayoutRoutine;
    private int layoutGeneration;
    private string pendingLayoutReason;
    private bool layoutDirtyDuringApply;
    private bool isApplyingLayout;
    private EntityId controllerSessionId;
    private int geometryUnusableRetries;

    private Rect lastTrackedSafeAreaPixels;
    private Vector2 lastTrackedSafeAreaSize;
    private float lastTrackedCanvasScaleFactor = -1f;
    private bool geometryFingerprintValid;

    private readonly Dictionary<RectTransform, RectSnap> phoneSnaps =
        new Dictionary<RectTransform, RectSnap>(16);

    private GridSnap phoneGridSnap;
    private TransformSnap phoneTopBarSnap;
    private RectTransform canvasRoot;
    private Canvas rootCanvas;
    private GridLayoutGroup gridLayout;
    private bool phoneCached;
    private GameplayLayoutKind appliedKind = (GameplayLayoutKind)(-1);
    private int lastWidth = -1;
    private int lastHeight = -1;

#if UNITY_EDITOR
    private readonly List<EditorLiveSnap> editorPreviewSnapshot = new List<EditorLiveSnap>(16);
    private bool editorPreviewSnapshotValid;
    private TransformSnap editorTopBarSnap;
    private GridSnap editorGridSnap;
    private bool editorTopBarSnapValid;
    private bool editorGridSnapValid;

    private struct EditorLiveSnap
    {
        public RectTransform Rt;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 Pivot;
        public Vector2 AnchoredPosition;
        public Vector2 SizeDelta;
        public Vector3 LocalScale;
        public Vector3 LocalEuler;
    }
#endif

    public GameplayLayoutKind AppliedKind => appliedKind;
    public bool IsAuthoringPreviewActive { get; private set; }
    public GameplayLayoutKind AuthoringPreviewKind { get; private set; } =
        GameplayLayoutKind.TallPhonePortrait;

    private void Awake()
    {
        controllerSessionId = GetEntityId();
        LogTrigger("Awake", queued: false, running: false, generation: layoutGeneration);
        ResolveRefs();
        // Capture authored baseline ONCE before any runtime mutation.
        CachePhonePresentation();
        LogOwnershipAudit("Awake");
        // Do NOT apply layout here — Canvas/SafeArea rects are often stale.
    }

    private void OnEnable()
    {
        SafeArea.OnApplied -= OnSafeAreaApplied;
        SafeArea.OnApplied += OnSafeAreaApplied;
        QueueAuthoritativeLayout("OnEnable");
    }

    private void OnDisable()
    {
        SafeArea.OnApplied -= OnSafeAreaApplied;
        // Invalidate any in-flight deferred pass belonging to this instance.
        layoutGeneration++;
        if (authoritativeLayoutRoutine != null)
        {
            StopCoroutine(authoritativeLayoutRoutine);
            authoritativeLayoutRoutine = null;
        }

        pendingLayoutReason = null;
        layoutDirtyDuringApply = false;
        isApplyingLayout = false;
        LogTrigger(
            "OnDisable",
            queued: false,
            running: false,
            generation: layoutGeneration);
    }

    private void OnDestroy()
    {
        SafeArea.OnApplied -= OnSafeAreaApplied;
        layoutGeneration++;
    }

    private void Start()
    {
        QueueAuthoritativeLayout("Start");
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (HasRelevantGeometryChanged())
        {
            // Never drop — even mid-apply; mark dirty so a newer generation runs.
            QueueAuthoritativeLayout("GeometryChanged");
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
        {
            return;
        }

        if (appliedKind == GameplayLayoutKind.WideTabletLandscape ||
            appliedKind == GameplayLayoutKind.TallPhonePortrait ||
            appliedKind == GameplayLayoutKind.CompactPhonePortrait)
        {
            QueueAuthoritativeLayout("OnValidate.Play");
        }
    }
#endif

    /// <summary>
    /// Called by LevelSelectUI after tiles are rebuilt so Wide is never lost
    /// to later content setup.
    /// </summary>
    public void ApplyAfterContentReady()
    {
        QueueAuthoritativeLayout("AfterContentReady");
    }

    private void OnSafeAreaApplied(SafeArea applied)
    {
        if (!isActiveAndEnabled || applied == null)
        {
            return;
        }

        // Only react to THIS scene's SafeArea — never a DDOL / other-scene instance.
        if (safeArea != null)
        {
            if (applied.transform != safeArea)
            {
                return;
            }
        }
        else if (applied.gameObject.scene != gameObject.scene)
        {
            return;
        }

        // Ignore the echo from our own ForceApply inside RunAuthoritativeLayoutPass.
        // Other writers still go through QueueAuthoritativeLayout → DirtyDuringApply.
        if (isApplyingLayout)
        {
            return;
        }

        QueueAuthoritativeLayout("SafeArea.OnApplied");
    }

    /// <summary>
    /// Schedules ONE authoritative layout for the latest generation.
    /// Older deferred coroutines are stopped — they must never overwrite newer work.
    /// </summary>
    private void QueueAuthoritativeLayout(string reason)
    {
        if (!isActiveAndEnabled && Application.isPlaying)
        {
            return;
        }

        if (isApplyingLayout)
        {
            layoutDirtyDuringApply = true;
            pendingLayoutReason = reason;
            LogTrigger(
                reason + "+DirtyDuringApply",
                queued: true,
                running: true,
                generation: layoutGeneration);
            return;
        }

        pendingLayoutReason = reason;
        layoutGeneration++;
        int myGeneration = layoutGeneration;

        LogTrigger(
            reason,
            queued: true,
            running: authoritativeLayoutRoutine != null,
            generation: myGeneration);

        if (!Application.isPlaying)
        {
            RunAuthoritativeLayoutPass(reason, myGeneration);
            return;
        }

        if (authoritativeLayoutRoutine != null)
        {
            StopCoroutine(authoritativeLayoutRoutine);
            authoritativeLayoutRoutine = null;
        }

        authoritativeLayoutRoutine =
            StartCoroutine(AuthoritativeLayoutRoutine(myGeneration, reason));
    }

    private IEnumerator AuthoritativeLayoutRoutine(int myGeneration, string reason)
    {
        // One frame: allow Start/content rebuild + Canvas to produce real rects.
        yield return null;

        if (myGeneration != layoutGeneration || !isActiveAndEnabled)
        {
            LogDev(
                "ABORT gen=" + myGeneration +
                " current=" + layoutGeneration +
                " reason=" + reason +
                " (superseded before apply)");
            if (myGeneration == layoutGeneration)
            {
                authoritativeLayoutRoutine = null;
            }

            yield break;
        }

        RunAuthoritativeLayoutPass(reason, myGeneration);

        if (authoritativeLayoutRoutine != null && myGeneration == layoutGeneration)
        {
            authoritativeLayoutRoutine = null;
        }
    }

    private void RunAuthoritativeLayoutPass(string reason, int myGeneration)
    {
        if (Application.isPlaying && myGeneration != layoutGeneration)
        {
            LogDev("ABORT gen=" + myGeneration + " before apply (generation moved)");
            return;
        }

        isApplyingLayout = true;
        layoutDirtyDuringApply = false;
        try
        {
            ResolveRefs();
            Canvas.ForceUpdateCanvases();

            if (safeArea != null)
            {
                SafeArea sa = safeArea.GetComponent<SafeArea>();
                if (sa != null)
                {
                    sa.ForceApply();
                }
            }

            Canvas.ForceUpdateCanvases();

            if (Application.isPlaying && myGeneration != layoutGeneration)
            {
                LogDev("ABORT gen=" + myGeneration + " after SafeArea (superseded)");
                return;
            }

            if (!IsLayoutGeometryUsable())
            {
                geometryUnusableRetries++;
                LogDev(
                    "GEOMETRY_UNUSABLE gen=" + myGeneration +
                    " reason=" + reason +
                    " retries=" + geometryUnusableRetries);
                if (geometryUnusableRetries <= 8)
                {
                    layoutDirtyDuringApply = true;
                    pendingLayoutReason = "GeometryUnusable";
                }

                return;
            }

            geometryUnusableRetries = 0;

            LogGeometrySnapshot("PRE.gen" + myGeneration + "." + reason);
            LogDev("APPLY_START gen=" + myGeneration + " reason=" + reason);
            ApplyCurrentResponsiveLayout(force: true, reason: reason);
            Canvas.ForceUpdateCanvases();
            StoreGeometryFingerprint();
            LogGeometrySnapshot("POST.gen" + myGeneration + "." + reason);
            LogFinalSnapshot(myGeneration, reason);
            LogDev("APPLY_END gen=" + myGeneration + " reason=" + reason);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Application.isPlaying)
            {
                RunIdempotenceCheck(myGeneration);
            }
#endif
        }
        finally
        {
            isApplyingLayout = false;
            if (layoutDirtyDuringApply && isActiveAndEnabled && Application.isPlaying)
            {
                string dirtyReason = string.IsNullOrEmpty(pendingLayoutReason)
                    ? "DirtyDuringApply"
                    : pendingLayoutReason;
                layoutDirtyDuringApply = false;
                QueueAuthoritativeLayout(dirtyReason);
            }
        }
    }

    private bool IsLayoutGeometryUsable()
    {
        ResolveRefs();
        if (safeArea == null || canvasRoot == null)
        {
            return false;
        }

        Rect sa = safeArea.rect;
        if (sa.width < 8f || sa.height < 8f)
        {
            return false;
        }

        if (rootCanvas != null && rootCanvas.scaleFactor < 0.01f)
        {
            return false;
        }

        if (canvasRoot.rect.width < 8f || canvasRoot.rect.height < 8f)
        {
            // Overlay can report 0 briefly; Screen fallback still allows header math
            // via SafeArea world corners — treat as usable if SafeArea is valid.
            return sa.width >= 8f && sa.height >= 8f;
        }

        return true;
    }

    private bool HasRelevantGeometryChanged()
    {
        // Before the first successful apply, OnEnable/Start/AfterContentReady own
        // scheduling. Do not thrash generations every LateUpdate.
        if (!geometryFingerprintValid)
        {
            return false;
        }

        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            return true;
        }

        if (Screen.safeArea != lastTrackedSafeAreaPixels)
        {
            return true;
        }

        ResolveRefs();
        if (safeArea != null)
        {
            Vector2 size = safeArea.rect.size;
            if (Vector2.Distance(size, lastTrackedSafeAreaSize) > 0.5f)
            {
                return true;
            }
        }

        if (rootCanvas != null)
        {
            float scale = rootCanvas.scaleFactor;
            if (Mathf.Abs(scale - lastTrackedCanvasScaleFactor) > 0.0001f)
            {
                return true;
            }
        }

        return false;
    }

    private void StoreGeometryFingerprint()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        lastTrackedSafeAreaPixels = Screen.safeArea;
        if (safeArea != null)
        {
            lastTrackedSafeAreaSize = safeArea.rect.size;
        }

        if (rootCanvas != null)
        {
            lastTrackedCanvasScaleFactor = rootCanvas.scaleFactor;
        }

        geometryFingerprintValid = true;
    }

    private void LogTrigger(
        string reason,
        bool queued,
        bool running,
        int generation)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LogDev(
            "TRIGGER reason=" + reason +
            " frame=" + Time.frameCount +
            " t=" + Time.realtimeSinceStartup.ToString("0.000") +
            " session=" + controllerSessionId.ToString() +
            " gen=" + generation +
            " queued=" + queued +
            " running=" + running +
            " applying=" + isApplyingLayout +
            " active=" + isActiveAndEnabled +
            " screen=" + Screen.width + "x" + Screen.height +
            " safeAreaPx=" + Screen.safeArea +
            " canvasScale=" +
            (rootCanvas != null ? rootCanvas.scaleFactor.ToString("0.####") : "null") +
            " pixelRect=" +
            (rootCanvas != null ? rootCanvas.pixelRect.ToString() : "null") +
            " saRect=" +
            (safeArea != null ? safeArea.rect.ToString() : "null") +
            " kind=" + appliedKind);
#endif
    }

    private void LogOwnershipAudit(string tag)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LevelSelectLayoutController[] controllers =
            FindObjectsByType<LevelSelectLayoutController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
        SafeArea[] safeAreas =
            FindObjectsByType<SafeArea>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
        Canvas[] canvases =
            FindObjectsByType<Canvas>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
        LivesHUD[] huds =
            FindObjectsByType<LivesHUD>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
        LevelSelectUI[] uis =
            FindObjectsByType<LevelSelectUI>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        System.Text.StringBuilder canvasDetail = new System.Text.StringBuilder();
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas c = canvases[i];
            if (c == null)
            {
                continue;
            }

            canvasDetail.Append(" [")
                .Append(c.GetEntityId().ToString())
                .Append(':')
                .Append(c.gameObject.scene.name)
                .Append(c.isRootCanvas ? ":root" : "")
                .Append(c.gameObject.scene == gameObject.scene ? ":OURS" : ":FOREIGN")
                .Append(']');
        }

        LogDev(
            "OWNERSHIP." + tag +
            " session=" + controllerSessionId.ToString() +
            " scene=" + gameObject.scene.name +
            " resolvedCanvas=" +
            (rootCanvas != null
                ? rootCanvas.GetEntityId().ToString() + ":" + rootCanvas.gameObject.scene.name
                : "null") +
            " controllers=" + controllers.Length +
            " safeAreas=" + safeAreas.Length +
            " canvases=" + canvases.Length + canvasDetail +
            " livesHud=" + huds.Length +
            " levelSelectUI=" + uis.Length);
#endif
    }

    private void LogGeometrySnapshot(string tag)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        ResolveRefs();
        string saRect = safeArea != null ? safeArea.rect.ToString() : "null";
        string scroll = levelScrollView != null
            ? levelScrollView.rect.size.ToString()
            : "null";
        float scale = rootCanvas != null ? rootCanvas.scaleFactor : -1f;
        LogDev(
            "GEO." + tag +
            " screen=" + Screen.width + "x" + Screen.height +
            " safeAreaPx=" + Screen.safeArea +
            " canvasScale=" + scale.ToString("0.####") +
            " saRect=" + saRect +
            " scrollSize=" + scroll +
            " kind=" + appliedKind);
#endif
    }

    private void LogFinalSnapshot(int generation, string reason)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LogDev(
            "[LevelSelectLayoutFinal] gen=" + generation +
            " reason=" + reason +
            " profile=" + appliedKind +
            " session=" + controllerSessionId.ToString() +
            " back=" + FormatRt(backButton) +
            " title=" + FormatRt(titleText) +
            " lives=" + FormatRt(livesHud) +
            " difficulty=" + FormatRt(difficultyTabs) +
            " scroll=" + FormatRt(levelScrollView) +
            " safeArea=" + FormatRt(safeArea));
#endif
    }

    private static string FormatRt(RectTransform rt)
    {
        if (rt == null)
        {
            return "null";
        }

        return "{pos=" + rt.anchoredPosition.ToString("0.##") +
               " size=" + rt.sizeDelta.ToString("0.##") +
               " scale=" + rt.localScale.ToString("0.###") +
               " anchors=" + rt.anchorMin.ToString("0.##") +
               "→" + rt.anchorMax.ToString("0.##") + "}";
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void RunIdempotenceCheck(int generation)
    {
        if (titleText == null || levelScrollView == null)
        {
            return;
        }

        Vector2 titlePos = titleText.anchoredPosition;
        Vector3 titleScale = titleText.localScale;
        Vector2 scrollPos = levelScrollView.anchoredPosition;
        Vector2 scrollSize = levelScrollView.sizeDelta;
        Vector2 backPos = backButton != null ? backButton.anchoredPosition : Vector2.zero;
        Vector2 livesPos = livesHud != null ? livesHud.anchoredPosition : Vector2.zero;
        Vector2 diffPos =
            difficultyTabs != null ? difficultyTabs.anchoredPosition : Vector2.zero;

        // Second apply with unchanged inputs must not drift.
        ApplyCurrentResponsiveLayout(force: true, reason: "IdempotenceCheck");

        const float Eps = 0.05f;
        bool drift =
            Vector2.Distance(titlePos, titleText.anchoredPosition) > Eps ||
            Vector3.Distance(titleScale, titleText.localScale) > Eps ||
            Vector2.Distance(scrollPos, levelScrollView.anchoredPosition) > Eps ||
            Vector2.Distance(scrollSize, levelScrollView.sizeDelta) > Eps ||
            (backButton != null &&
             Vector2.Distance(backPos, backButton.anchoredPosition) > Eps) ||
            (livesHud != null &&
             Vector2.Distance(livesPos, livesHud.anchoredPosition) > Eps) ||
            (difficultyTabs != null &&
             Vector2.Distance(diffPos, difficultyTabs.anchoredPosition) > Eps);

        if (drift)
        {
            Debug.LogWarning(
                "[LevelSelectLayout] IDEMPOTENCE_FAIL gen=" + generation +
                " session=" + controllerSessionId.ToString() +
                " title " + titlePos + "→" + titleText.anchoredPosition +
                " scrollSize " + scrollSize + "→" + levelScrollView.sizeDelta +
                " back " + backPos + "→" +
                (backButton != null ? backButton.anchoredPosition.ToString() : "null") +
                " lives " + livesPos + "→" +
                (livesHud != null ? livesHud.anchoredPosition.ToString() : "null"));
        }
        else
        {
            LogDev("IDEMPOTENCE_OK gen=" + generation + " session=" + controllerSessionId.ToString());
        }
    }
#endif

    /// <summary>
    /// Single authoritative apply path for runtime + editor preview.
    /// Prefer <see cref="QueueAuthoritativeLayout"/> at runtime so Canvas/SafeArea are valid.
    /// </summary>
    public void ApplyCurrentResponsiveLayout(bool force, string reason)
    {
        ResolveRefs();

        GameplayLayoutKind want =
            GameplayLayoutMode.Resolve(tallPhoneMaxAspect, wideAspectThreshold);

        float aspect = GameplayLayoutMode.GetScreenAspect();
        LogDev(
            "Resolve width=" + Screen.width +
            " height=" + Screen.height +
            " aspect=" + aspect.ToString("0.000") +
            " kind=" + want +
            " reason=" + reason);

        if (!force &&
            want == appliedKind &&
            !HasRelevantGeometryChanged())
        {
            return;
        }

        appliedKind = want;

        // Always ensure missing snaps are filled from current authored values;
        // never re-cache keys already sealed (prevents baseline drift).
        CachePhonePresentation();
        RestorePhonePresentation();

        int columnsBefore = ReadColumns();

        if (want == GameplayLayoutKind.WideTabletLandscape)
        {
            ApplyWidePresentation();
        }
        else
        {
            ApplyPhoneHeaderLayout();
        }

        ForceUnityLayoutRebuild();
        NotifyLevelSelectScroll();
        StoreGeometryFingerprint();

        int columnsAfter = ReadColumns();
        LogDev(
            "APPLY kind=" + want +
            " columns before=" + columnsBefore +
            " columns after=" + columnsAfter +
            " cell=" + (gridLayout != null ? gridLayout.cellSize.ToString() : "null") +
            " spacing=" + (gridLayout != null ? gridLayout.spacing.ToString() : "null") +
            " scrollRect size=" +
            (levelScrollView != null ? levelScrollView.rect.size.ToString() : "null") +
            " scroll anchors=" +
            (levelScrollView != null
                ? levelScrollView.anchorMin + "→" + levelScrollView.anchorMax
                : "null"));

        if (Application.isPlaying && isActiveAndEnabled)
        {
            StopCoroutine(nameof(LogColumnsAfterFrames));
            StartCoroutine(LogColumnsAfterFrames());
        }
    }

    private int ReadColumns()
    {
        if (gridLayout == null && levelGrid != null)
        {
            gridLayout = levelGrid.GetComponent<GridLayoutGroup>();
        }

        return gridLayout != null ? gridLayout.constraintCount : -1;
    }

    private void ResolveRefs()
    {
        if (canvasRoot == null || rootCanvas == null)
        {
            Canvas found = ResolveSceneCanvas();
            if (found != null)
            {
                rootCanvas = found;
                canvasRoot = found.transform as RectTransform;
            }
        }
        else if (rootCanvas.gameObject.scene != gameObject.scene)
        {
            // Never keep a Canvas from MainMenu / DDOL after scene re-entry.
            LogDev(
                "ResolveRefs REJECTED foreign canvas id=" +
                rootCanvas.GetEntityId().ToString() +
                " scene=" + rootCanvas.gameObject.scene.name);
            rootCanvas = null;
            canvasRoot = null;
            Canvas found = ResolveSceneCanvas();
            if (found != null)
            {
                rootCanvas = found;
                canvasRoot = found.transform as RectTransform;
            }
        }

        Transform root = canvasRoot != null ? (Transform)canvasRoot : transform;

        if (topBar == null)
        {
            topBar = FindDeep(root, "TopBar");
        }

        if (titleText == null && topBar != null)
        {
            titleText = topBar.Find("Text") as RectTransform;
        }

        if (backButton == null && topBar != null)
        {
            backButton = topBar.Find("BackButton") as RectTransform;
        }

        if (difficultyTabs == null)
        {
            difficultyTabs = FindNamedRect(root, "DifficultyTabs");
        }

        if (levelScrollView == null)
        {
            levelScrollView = FindNamedRect(root, "LevelScrollView");
        }

        if (viewport == null && levelScrollView != null)
        {
            viewport = levelScrollView.Find("Viewport") as RectTransform;
        }

        if (levelGrid == null)
        {
            levelGrid = FindNamedRect(root, "LevelGrid");
        }

        if (levelGrid != null)
        {
            gridLayout = levelGrid.GetComponent<GridLayoutGroup>();
        }

        if (background == null)
        {
            background = FindNamedRect(root, "Background", preferRootChild: true);
        }

        if (backgroundOverlay == null)
        {
            backgroundOverlay = FindNamedRect(root, "BackgroundOverlay", preferRootChild: true);
        }

        if (backgroundImage == null && background != null)
        {
            backgroundImage = background.GetComponent<Image>();
        }

        if (levelSelectUI == null)
        {
            levelSelectUI = GetComponent<LevelSelectUI>();
            if (levelSelectUI == null)
            {
                levelSelectUI = GetComponentInChildren<LevelSelectUI>(true);
            }

            if (levelSelectUI == null)
            {
                levelSelectUI = FindInActiveScene<LevelSelectUI>();
            }
        }

        if (safeArea == null)
        {
            Transform sa = FindDeep(root, "SafeArea");
            if (sa != null)
            {
                safeArea = sa as RectTransform;
            }
        }

        if (safeArea != null && safeArea.GetComponent<SafeArea>() == null)
        {
            safeArea.gameObject.AddComponent<SafeArea>();
        }

        if (livesHud == null)
        {
            Transform hud = FindDeep(root, "LivesHUD");
            if (hud != null)
            {
                livesHud = hud as RectTransform;
            }
        }
    }

    /// <summary>
    /// Canvas must belong to THIS LevelSelect scene — never DDOL SceneTransition
    /// or a leftover MainMenu canvas (FindAnyObjectByType order is nondeterministic).
    /// </summary>
    private Canvas ResolveSceneCanvas()
    {
        Canvas selfCanvas = GetComponent<Canvas>();
        if (selfCanvas != null && selfCanvas.gameObject.scene == gameObject.scene)
        {
            return selfCanvas;
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.gameObject.scene == gameObject.scene)
        {
            return parentCanvas.rootCanvas != null ? parentCanvas.rootCanvas : parentCanvas;
        }

        Canvas childCanvas = GetComponentInChildren<Canvas>(true);
        if (childCanvas != null && childCanvas.gameObject.scene == gameObject.scene)
        {
            return childCanvas.rootCanvas != null ? childCanvas.rootCanvas : childCanvas;
        }

        return FindInActiveScene<Canvas>();
    }

    private T FindInActiveScene<T>() where T : Component
    {
        T[] found = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        T best = null;
        for (int i = 0; i < found.Length; i++)
        {
            T candidate = found[i];
            if (candidate == null || candidate.gameObject.scene != gameObject.scene)
            {
                continue;
            }

            // Prefer root canvases when resolving Canvas (T == Canvas).
            // Always return T — never a hard-coded Canvas typed value.
            if (candidate is Canvas canvas)
            {
                if (canvas.isRootCanvas)
                {
                    return candidate;
                }

                if (best == null)
                {
                    best = candidate;
                }

                continue;
            }

            return candidate;
        }

        return best;
    }

    private static Transform FindDeep(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == name)
            {
                return child;
            }

            Transform nested = FindDeep(child, name);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static RectTransform FindNamedRect(
        Transform root,
        string name,
        bool preferRootChild = false)
    {
        if (root == null)
        {
            return null;
        }

        if (preferRootChild)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child != null && child.name == name)
                {
                    return child as RectTransform;
                }
            }
        }

        Transform t = root.Find(name);
        if (t != null)
        {
            return t as RectTransform;
        }

        return FindDeep(root, name) as RectTransform;
    }

    private void CachePhonePresentation()
    {
        ResolveRefs();

        // Fill any missing authored snaps. Never overwrite a snap already captured —
        // that would bake a previously-applied runtime layout into the baseline.
        CapturePhone(titleText);
        CapturePhone(backButton);
        CapturePhone(livesHud);
        CapturePhone(difficultyTabs);
        CapturePhone(levelScrollView);
        CapturePhone(levelGrid);
        CapturePhone(background);
        CapturePhone(backgroundOverlay);

        if (topBar != null && !phoneTopBarSnap.Valid)
        {
            phoneTopBarSnap = new TransformSnap
            {
                LocalPosition = topBar.localPosition,
                LocalScale = topBar.localScale,
                LocalRotation = topBar.localRotation,
                Valid = true
            };
        }

        if (gridLayout != null && !phoneGridSnap.Valid)
        {
            phoneGridSnap = CaptureGrid(gridLayout);
        }

        // Only seal the baseline once critical authored refs exist.
        if (!phoneCached && titleText != null && levelScrollView != null)
        {
            phoneCached = true;
            LogDev(
                "BASELINE_CACHE session=" + controllerSessionId.ToString() +
                " columns=" +
                (phoneGridSnap.Valid ? phoneGridSnap.ConstraintCount.ToString() : "invalid") +
                " title=" + FormatRt(titleText) +
                " back=" + FormatRt(backButton) +
                " lives=" + FormatRt(livesHud) +
                " scroll=" + FormatRt(levelScrollView) +
                " topBarScale=" +
                (topBar != null ? topBar.localScale.ToString("0.###") : "null"));
        }
    }

    private static GridSnap CaptureGrid(GridLayoutGroup grid)
    {
        return new GridSnap
        {
            CellSize = grid.cellSize,
            Spacing = grid.spacing,
            Padding = new RectOffset(
                grid.padding.left,
                grid.padding.right,
                grid.padding.top,
                grid.padding.bottom),
            Constraint = grid.constraint,
            ConstraintCount = grid.constraintCount,
            ChildAlignment = grid.childAlignment,
            Valid = true
        };
    }

    private void CapturePhone(RectTransform rt)
    {
        if (rt == null || phoneSnaps.ContainsKey(rt))
        {
            return;
        }

        phoneSnaps[rt] = new RectSnap
        {
            AnchorMin = rt.anchorMin,
            AnchorMax = rt.anchorMax,
            Pivot = rt.pivot,
            AnchoredPosition = rt.anchoredPosition,
            SizeDelta = rt.sizeDelta,
            LocalScale = rt.localScale,
            LocalRotation = rt.localRotation,
            Valid = true
        };
    }

    private void RestorePhonePresentation()
    {
        RestorePhone(titleText);
        RestorePhone(backButton);
        RestorePhone(livesHud);
        RestorePhone(difficultyTabs);
        RestorePhone(levelScrollView);
        RestorePhone(levelGrid);
        RestorePhone(background);
        RestorePhone(backgroundOverlay);

        if (topBar != null && phoneTopBarSnap.Valid)
        {
            topBar.localPosition = phoneTopBarSnap.LocalPosition;
            topBar.localScale = phoneTopBarSnap.LocalScale;
            topBar.localRotation = phoneTopBarSnap.LocalRotation;
        }

        if (gridLayout != null && phoneGridSnap.Valid)
        {
            ApplyGridSnap(gridLayout, phoneGridSnap);
        }
    }

    private static void ApplyGridSnap(GridLayoutGroup grid, GridSnap snap)
    {
        grid.cellSize = snap.CellSize;
        grid.spacing = snap.Spacing;
        grid.padding = new RectOffset(
            snap.Padding.left,
            snap.Padding.right,
            snap.Padding.top,
            snap.Padding.bottom);
        grid.constraint = snap.Constraint;
        grid.constraintCount = snap.ConstraintCount;
        grid.childAlignment = snap.ChildAlignment;
    }

    private void RestorePhone(RectTransform rt)
    {
        if (rt == null || !phoneSnaps.TryGetValue(rt, out RectSnap snap) || !snap.Valid)
        {
            return;
        }

        rt.anchorMin = snap.AnchorMin;
        rt.anchorMax = snap.AnchorMax;
        rt.pivot = snap.Pivot;
        rt.anchoredPosition = snap.AnchoredPosition;
        rt.sizeDelta = snap.SizeDelta;
        rt.localScale = snap.LocalScale;
        rt.localRotation = snap.LocalRotation;
    }

    /// <summary>
    /// SafeArea chrome stack: one header row (Back | SELECT LEVEL | LivesHUD),
    /// then difficulty, then scroll viewport to SafeArea bottom.
    /// Used by Tall/Compact phone and Wide Tablet (Wide overrides TopBar base scale).
    /// </summary>
    private void ApplyPhoneHeaderLayout()
    {
        ResolveRefs();
        if (safeArea == null || titleText == null)
        {
            LogDev("Phone stack skipped — SafeArea or title missing");
            return;
        }

        SafeArea saComp = safeArea.GetComponent<SafeArea>();
        if (saComp != null)
        {
            saComp.ForceApply();
        }

        Canvas.ForceUpdateCanvases();

        if (livesHud != null && livesHud.parent != safeArea)
        {
            livesHud.SetParent(safeArea, false);
        }

        if (backButton != null && topBar != null && backButton.parent != topBar)
        {
            backButton.SetParent(topBar, false);
        }

        // Restore authored base scales before fit math.
        Vector3 topBarBaseScale = phoneTopBarSnap.Valid
            ? phoneTopBarSnap.LocalScale
            : Vector3.one;
        if (chromeTopBarScaleOverride.HasValue)
        {
            topBarBaseScale = chromeTopBarScaleOverride.Value;
            chromeTopBarScaleOverride = null;
        }

        if (topBar != null)
        {
            topBar.localScale = topBarBaseScale;
        }

        if (livesHud != null &&
            phoneSnaps.TryGetValue(livesHud, out RectSnap livesSnap) &&
            livesSnap.Valid)
        {
            livesHud.localScale = livesSnap.LocalScale;
        }

        Canvas.ForceUpdateCanvases();

        float backW;
        float backH;
        float titleW;
        float titleH;
        float livesW;
        float livesH;
        MeasureHeaderElementSizes(
            out backW, out backH, out titleW, out titleH, out livesW, out livesH);

        float gapBack = Mathf.Max(phoneHeaderMinHorizontalGap, phoneBackToTitleGap);
        float gapLives = Mathf.Max(phoneHeaderMinHorizontalGap, phoneTitleToLivesGap);
        float sideInset = phoneHeaderMinHorizontalGap;
        Rect saRect = safeArea.rect;
        float available = Mathf.Max(1f, saRect.width - sideInset * 2f);

        float NaturalTotal()
        {
            return backW + gapBack + titleW + gapLives + livesW;
        }

        // 1) Shrink gaps toward minimum before scaling.
        float total = NaturalTotal();
        if (total > available)
        {
            float excess = total - available;
            float reducible =
                (gapBack - phoneHeaderMinHorizontalGap) +
                (gapLives - phoneHeaderMinHorizontalGap);
            if (reducible > 0.01f)
            {
                float t = Mathf.Clamp01(excess / reducible);
                gapBack = Mathf.Lerp(gapBack, phoneHeaderMinHorizontalGap, t);
                gapLives = Mathf.Lerp(gapLives, phoneHeaderMinHorizontalGap, t);
            }
        }

        // 2) Uniform header fit scale if still too wide.
        total = NaturalTotal();
        float fit = 1f;
        if (total > available)
        {
            fit = Mathf.Clamp(available / total, phoneHeaderMinFitScale, 1f);
            if (topBar != null)
            {
                topBar.localScale = topBarBaseScale * fit;
            }

            if (livesHud != null &&
                phoneSnaps.TryGetValue(livesHud, out RectSnap ls) &&
                ls.Valid)
            {
                livesHud.localScale = ls.LocalScale * fit;
            }

            Canvas.ForceUpdateCanvases();
            MeasureHeaderElementSizes(
                out backW, out backH, out titleW, out titleH, out livesW, out livesH);
            gapBack *= fit;
            gapLives *= fit;
        }

        float rowH = Mathf.Max(backH, Mathf.Max(titleH, livesH));
        float headerTop = saRect.yMax - Mathf.Max(0f, phoneHeaderTopPadding);
        float headerCenterY = headerTop - rowH * 0.5f;
        float titleCenterX = saRect.center.x;
        float titleCenterY = headerCenterY;

        float backCenterX = titleCenterX - titleW * 0.5f - gapBack - backW * 0.5f;
        float livesCenterX = titleCenterX + titleW * 0.5f + gapLives + livesW * 0.5f;

        // Keep the whole row inside SafeArea horizontally if fit floor still overflows.
        float rowLeft = backCenterX - backW * 0.5f;
        float rowRight = livesCenterX + livesW * 0.5f;
        float shift = 0f;
        if (rowLeft < saRect.xMin + sideInset)
        {
            shift = (saRect.xMin + sideInset) - rowLeft;
        }
        else if (rowRight > saRect.xMax - sideInset)
        {
            shift = (saRect.xMax - sideInset) - rowRight;
        }

        titleCenterX += shift;
        backCenterX += shift;
        livesCenterX += shift;

        SetRectCenterInSafeArea(titleText, titleCenterX, titleCenterY);
        if (backButton != null)
        {
            SetRectCenterInSafeArea(backButton, backCenterX, titleCenterY);
        }

        if (livesHud != null)
        {
            SetRectCenterInSafeArea(livesHud, livesCenterX, titleCenterY);
        }

        Canvas.ForceUpdateCanvases();

        float headerBottom = headerCenterY - rowH * 0.5f;
        if (TryGetRectBoundsInSafeArea(titleText, out _, out _, out float tMinY, out float tMaxY))
        {
            headerBottom = tMinY;
            headerTop = tMaxY;
        }

        if (backButton != null &&
            TryGetRectBoundsInSafeArea(backButton, out _, out _, out float bMinY, out float bMaxY))
        {
            headerBottom = Mathf.Min(headerBottom, bMinY);
            headerTop = Mathf.Max(headerTop, bMaxY);
        }

        if (livesHud != null &&
            TryGetRectBoundsInSafeArea(livesHud, out _, out _, out float hMinY, out float hMaxY))
        {
            headerBottom = Mathf.Min(headerBottom, hMinY);
            headerTop = Mathf.Max(headerTop, hMaxY);
        }

        // Difficulty directly under header.
        float difficultyTop = headerBottom - Mathf.Max(0f, phoneHeaderToDifficultyGap);
        if (difficultyTabs != null)
        {
            AlignRectTopToSafeAreaY(difficultyTabs, difficultyTop);
            Canvas.ForceUpdateCanvases();
        }

        float difficultyBottom = difficultyTop;
        if (difficultyTabs != null &&
            TryGetRectBoundsInSafeArea(
                difficultyTabs, out _, out _, out float dMinY, out _))
        {
            difficultyBottom = dMinY;
        }

        // Level scroll viewport: top under difficulty, bottom to SafeArea + padding.
        // Content (LevelGrid) height stays row-driven via LevelSelectUI — not clamped here.
        float gridTop = difficultyBottom - Mathf.Max(0f, phoneDifficultyToGridGap);
        StretchScrollViewportInSafeArea(gridTop, phoneGridBottomPadding);

        LogDev(
            "Phone stack headerTop=" + headerTop.ToString("0.#") +
            " headerBottom=" + headerBottom.ToString("0.#") +
            " diffTop=" + difficultyTop.ToString("0.#") +
            " gridTop=" + gridTop.ToString("0.#") +
            " fit=" + fit.ToString("0.###") +
            " gapBack=" + gapBack.ToString("0.#") +
            " gapLives=" + gapLives.ToString("0.#"));
    }

    /// <summary>
    /// Sizes LevelScrollView (viewport host) to fill SafeArea from gridTop down to
    /// SafeArea bottom + padding. Does not change LevelGrid/content row height.
    /// </summary>
    private void StretchScrollViewportInSafeArea(float gridTopSa, float bottomPadding)
    {
        if (levelScrollView == null || safeArea == null)
        {
            return;
        }

        Rect saRect = safeArea.rect;
        float gridBottomSa = saRect.yMin + Mathf.Max(0f, bottomPadding);
        if (gridTopSa < gridBottomSa + 8f)
        {
            gridBottomSa = gridTopSa - 8f;
        }

        RectTransform parentRt = levelScrollView.parent as RectTransform;
        if (parentRt == null)
        {
            parentRt = canvasRoot;
        }

        if (parentRt == null)
        {
            return;
        }

        float midX = saRect.center.x;
        Vector3 topWorld = safeArea.TransformPoint(new Vector3(midX, gridTopSa, 0f));
        Vector3 botWorld = safeArea.TransformPoint(new Vector3(midX, gridBottomSa, 0f));
        Vector3 leftWorld = safeArea.TransformPoint(new Vector3(saRect.xMin, gridTopSa, 0f));
        Vector3 rightWorld = safeArea.TransformPoint(new Vector3(saRect.xMax, gridTopSa, 0f));

        Vector3 topLocal = parentRt.InverseTransformPoint(topWorld);
        Vector3 botLocal = parentRt.InverseTransformPoint(botWorld);
        Vector3 leftLocal = parentRt.InverseTransformPoint(leftWorld);
        Vector3 rightLocal = parentRt.InverseTransformPoint(rightWorld);

        float width = Mathf.Abs(rightLocal.x - leftLocal.x);
        float height = Mathf.Max(1f, topLocal.y - botLocal.y);
        float cx = (leftLocal.x + rightLocal.x) * 0.5f;
        float cy = (topLocal.y + botLocal.y) * 0.5f;

        levelScrollView.anchorMin = new Vector2(0.5f, 0.5f);
        levelScrollView.anchorMax = new Vector2(0.5f, 0.5f);
        levelScrollView.pivot = new Vector2(0.5f, 0.5f);
        levelScrollView.localScale = Vector3.one;
        levelScrollView.sizeDelta = new Vector2(width, height);
        levelScrollView.anchoredPosition = new Vector2(cx, cy);

        if (viewport != null)
        {
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.pivot = new Vector2(0.5f, 1f);
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.localScale = Vector3.one;
        }

        if (levelGrid != null)
        {
            // Width fills viewport; vertical size owned by LevelSelectUI row math.
            levelGrid.anchorMin = new Vector2(0f, 1f);
            levelGrid.anchorMax = new Vector2(1f, 1f);
            levelGrid.pivot = new Vector2(0.5f, 1f);
            levelGrid.anchoredPosition = Vector2.zero;
            levelGrid.offsetMin = new Vector2(0f, levelGrid.offsetMin.y);
            levelGrid.offsetMax = new Vector2(0f, 0f);
        }
    }

    private void MeasureHeaderElementSizes(
        out float backW,
        out float backH,
        out float titleW,
        out float titleH,
        out float livesW,
        out float livesH)
    {
        backW = backH = titleW = titleH = livesW = livesH = 0f;
        if (backButton != null &&
            TryGetRectBoundsInSafeArea(
                backButton, out float bMinX, out float bMaxX, out float bMinY, out float bMaxY))
        {
            backW = bMaxX - bMinX;
            backH = bMaxY - bMinY;
        }

        if (TryGetRectBoundsInSafeArea(
                titleText, out float tMinX, out float tMaxX, out float tMinY, out float tMaxY))
        {
            titleW = tMaxX - tMinX;
            titleH = tMaxY - tMinY;
        }

        if (livesHud != null &&
            TryGetRectBoundsInSafeArea(
                livesHud, out float hMinX, out float hMaxX, out float hMinY, out float hMaxY))
        {
            livesW = hMaxX - hMinX;
            livesH = hMaxY - hMinY;
        }
    }

    private void SetRectCenterInSafeArea(RectTransform rt, float centerX, float centerY)
    {
        if (rt == null || safeArea == null)
        {
            return;
        }

        Vector3 desiredWorld = safeArea.TransformPoint(new Vector3(centerX, centerY, 0f));
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector3 currentCenter = (corners[0] + corners[2]) * 0.5f;
        rt.position += desiredWorld - currentCenter;
    }

    private void AlignRectTopToSafeAreaY(RectTransform rt, float desiredTopY)
    {
        if (rt == null || safeArea == null)
        {
            return;
        }

        if (!TryGetRectBoundsInSafeArea(rt, out _, out _, out _, out float curTop))
        {
            return;
        }

        float deltaLocalY = desiredTopY - curTop;
        Vector3 deltaWorld = safeArea.TransformVector(new Vector3(0f, deltaLocalY, 0f));
        rt.position += deltaWorld;
    }

    private bool TryGetRectBoundsInSafeArea(
        RectTransform rt,
        out float minX,
        out float maxX,
        out float minY,
        out float maxY)
    {
        minX = maxX = minY = maxY = 0f;
        if (rt == null || safeArea == null || !rt.gameObject.activeInHierarchy)
        {
            return false;
        }

        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        minX = float.PositiveInfinity;
        maxX = float.NegativeInfinity;
        minY = float.PositiveInfinity;
        maxY = float.NegativeInfinity;
        for (int i = 0; i < 4; i++)
        {
            Vector3 local = safeArea.InverseTransformPoint(corners[i]);
            if (local.x < minX)
            {
                minX = local.x;
            }

            if (local.x > maxX)
            {
                maxX = local.x;
            }

            if (local.y < minY)
            {
                minY = local.y;
            }

            if (local.y > maxY)
            {
                maxY = local.y;
            }
        }

        return true;
    }

    private void ApplyWidePresentation()
    {
        ResolveRefs();

        // Keep authored Wide title/back hit sizes when present, then reuse the
        // SafeArea chrome stack so Wide gets [Back | SELECT LEVEL | Lives] like phone.
        if (titleText != null && wideTitle.hasSizeDelta)
        {
            titleText.sizeDelta = wideTitle.sizeDelta;
        }

        if (backButton != null)
        {
            if (!backButton.gameObject.activeSelf)
            {
                backButton.gameObject.SetActive(true);
            }

            if (topBar != null && backButton.parent != topBar)
            {
                backButton.SetParent(topBar, false);
            }

            Vector3 scale = wideBackButtonScale;
            if (scale.sqrMagnitude < 0.0001f)
            {
                scale = new Vector3(0.45f, 0.45f, 0.45f);
            }

            backButton.localScale = scale;
            if (phoneSnaps.TryGetValue(backButton, out RectSnap phone) && phone.Valid)
            {
                backButton.sizeDelta = phone.SizeDelta;
            }
        }

        if (difficultyTabs != null && wideDifficultyTabs.hasSizeDelta)
        {
            // Preserve Wide difficulty horizontal size; vertical place is stack-owned.
            difficultyTabs.sizeDelta = wideDifficultyTabs.sizeDelta;
            if (wideDifficultyTabs.localScale.sqrMagnitude > 0.0001f)
            {
                difficultyTabs.localScale = wideDifficultyTabs.localScale;
            }
        }

        chromeTopBarScaleOverride = wideTopBarLocalScale.sqrMagnitude > 0.0001f
            ? wideTopBarLocalScale
            : new Vector3(1.6f, 1.6f, 1.6f);

        ApplyPhoneHeaderLayout();
        ApplyWideGrid();
        ApplyWideBackgroundCover();
    }

    /// <summary>
    /// BackButton is a RectTransform child of TopBar (plain Transform).
    /// Positions must be TopBar-local — canvas-space values like -820 push it off-screen.
    /// Legacy helper kept for editor capture of Wide back size/scale only.
    /// </summary>
    private void ApplyWideBackButton()
    {
        if (backButton == null)
        {
            LogDev("Wide BackButton MISSING");
            return;
        }

        if (!backButton.gameObject.activeSelf)
        {
            backButton.gameObject.SetActive(true);
        }

        // Stay under TopBar; only rewrite local presentation.
        if (topBar != null && backButton.parent != topBar)
        {
            backButton.SetParent(topBar, false);
        }

        backButton.anchorMin = new Vector2(0.5f, 0.5f);
        backButton.anchorMax = new Vector2(0.5f, 0.5f);
        backButton.pivot = new Vector2(0.5f, 0.5f);
        backButton.anchoredPosition = wideBackButtonPosition;

        Vector3 scale = wideBackButtonScale;
        if (scale.sqrMagnitude < 0.0001f)
        {
            scale = new Vector3(0.45f, 0.45f, 0.45f);
        }

        backButton.localScale = scale;

        // Keep authored phone hit size when available.
        if (phoneSnaps.TryGetValue(backButton, out RectSnap phone) && phone.Valid)
        {
            backButton.sizeDelta = phone.SizeDelta;
        }

        LogDev(
            "Wide BackButton parent=" +
            (backButton.parent != null ? backButton.parent.name : "null") +
            " pos=" + backButton.anchoredPosition +
            " scale=" + backButton.localScale +
            " active=" + backButton.gameObject.activeInHierarchy);
    }

    private void ApplyWideScrollView()
    {
        // Obsolete for layout: chrome stack + StretchScrollViewportInSafeArea owns
        // scroll geometry on Wide as well. Kept so Capture can still read anchors
        // if an older scene expects this method name during reflection-free edits.
        if (levelScrollView == null)
        {
            LogDev("Wide scrollView MISSING");
            return;
        }

        if (viewport != null)
        {
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.pivot = new Vector2(0.5f, 1f);
        }

        if (levelGrid != null)
        {
            levelGrid.anchorMin = new Vector2(0f, 1f);
            levelGrid.anchorMax = new Vector2(1f, 1f);
            levelGrid.pivot = new Vector2(0.5f, 1f);
            levelGrid.anchoredPosition = Vector2.zero;
            levelGrid.offsetMin = new Vector2(0f, levelGrid.offsetMin.y);
            levelGrid.offsetMax = new Vector2(0f, levelGrid.offsetMax.y);
        }
    }

    private void ApplyWideGrid()
    {
        if (gridLayout == null && levelGrid != null)
        {
            gridLayout = levelGrid.GetComponent<GridLayoutGroup>();
        }

        if (gridLayout == null)
        {
            LogDev("Wide gridLayout MISSING — cannot set columns");
            return;
        }

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = Mathf.Clamp(wideColumnCount, 4, 8);
        gridLayout.cellSize = wideCellSize;
        gridLayout.spacing = wideSpacing;
        // Assign padding via ints — never construct RectOffset in a field initializer.
        RectOffset pad = gridLayout.padding;
        pad.left = widePaddingLeft;
        pad.right = widePaddingRight;
        pad.top = widePaddingTop;
        pad.bottom = widePaddingBottom;
        gridLayout.padding = pad;

        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.enabled = true;
    }

    private static void ApplyWideElement(RectTransform rt, WideElementLayout layout)
    {
        if (rt == null)
        {
            return;
        }

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = layout.anchoredPosition;
        if (layout.hasSizeDelta)
        {
            rt.sizeDelta = layout.sizeDelta;
        }

        Vector3 scale = layout.localScale;
        if (scale.sqrMagnitude < 0.0001f)
        {
            scale = Vector3.one;
        }

        rt.localScale = scale;
    }

    private void ApplyWideBackgroundCover()
    {
        LogDev("Apply Wide background cover");

        if (canvasRoot == null)
        {
            ResolveRefs();
        }

        if (canvasRoot == null)
        {
            LogDev("background cover aborted — canvasRoot null");
            return;
        }

        Canvas.ForceUpdateCanvases();

        if (!TryGetCanvasSize(out float containerW, out float containerH))
        {
            LogDev("background cover aborted — canvas size 0");
            return;
        }

        LogDev(
            "viewport/canvas size=" + containerW.ToString("0") + "x" + containerH.ToString("0"));

        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetParent(canvasRoot, false);
            backgroundOverlay.SetAsFirstSibling();
            if (background != null)
            {
                background.SetParent(canvasRoot, false);
                background.SetSiblingIndex(backgroundOverlay.GetSiblingIndex());
            }

            backgroundOverlay.anchorMin = Vector2.zero;
            backgroundOverlay.anchorMax = Vector2.one;
            backgroundOverlay.pivot = new Vector2(0.5f, 0.5f);
            backgroundOverlay.offsetMin = Vector2.zero;
            backgroundOverlay.offsetMax = Vector2.zero;
            backgroundOverlay.localScale = Vector3.one;
        }

        if (background == null)
        {
            LogDev("background MISSING");
            return;
        }

        Sprite sprite = backgroundImage != null ? backgroundImage.sprite : null;
        float spriteW = 941f;
        float spriteH = 1672f;
        if (sprite != null && sprite.rect.width > 0.01f && sprite.rect.height > 0.01f)
        {
            spriteW = sprite.rect.width;
            spriteH = sprite.rect.height;
        }
        else if (phoneSnaps.TryGetValue(background, out RectSnap phone) && phone.Valid &&
                 phone.SizeDelta.x > 0.01f && phone.SizeDelta.y > 0.01f)
        {
            spriteW = phone.SizeDelta.x;
            spriteH = phone.SizeDelta.y;
        }

        float containerAspect = containerW / containerH;
        float spriteAspect = spriteW / spriteH;
        float targetW;
        float targetH;
        if (containerAspect > spriteAspect)
        {
            targetW = containerW;
            targetH = containerW / spriteAspect;
        }
        else
        {
            targetH = containerH;
            targetW = containerH * spriteAspect;
        }

        background.anchorMin = new Vector2(0.5f, 0.5f);
        background.anchorMax = new Vector2(0.5f, 0.5f);
        background.pivot = new Vector2(0.5f, 0.5f);
        background.anchoredPosition = Vector2.zero;
        background.sizeDelta = new Vector2(targetW, targetH);
        background.localScale = Vector3.one;

        if (backgroundImage != null)
        {
            backgroundImage.preserveAspect = false;
            backgroundImage.type = Image.Type.Simple;
        }

        LogDev(
            "background rect sizeDelta=" + background.sizeDelta +
            " localScale=" + background.localScale);
    }

    private bool TryGetCanvasSize(out float width, out float height)
    {
        width = 0f;
        height = 0f;
        if (canvasRoot == null)
        {
            return false;
        }

        width = canvasRoot.rect.width;
        height = canvasRoot.rect.height;
        if (width > 1f && height > 1f)
        {
            return true;
        }

        Canvas.ForceUpdateCanvases();
        width = canvasRoot.rect.width;
        height = canvasRoot.rect.height;
        if (width > 1f && height > 1f)
        {
            return true;
        }

        // Overlay canvas often reports 0 before first layout — derive from Screen.
        float scale = 1f;
        if (rootCanvas == null)
        {
            rootCanvas = canvasRoot.GetComponent<Canvas>();
        }

        if (rootCanvas != null && rootCanvas.scaleFactor > 0.01f)
        {
            scale = rootCanvas.scaleFactor;
        }

        width = Screen.width / scale;
        height = Screen.height / scale;
        return width > 1f && height > 1f;
    }

    private void ForceUnityLayoutRebuild()
    {
        Canvas.ForceUpdateCanvases();
        if (levelGrid != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(levelGrid);
        }

        if (viewport != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);
        }

        if (levelScrollView != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(levelScrollView);
        }

        if (canvasRoot != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(canvasRoot);
        }
    }

    private void NotifyLevelSelectScroll()
    {
        if (levelSelectUI != null)
        {
            levelSelectUI.RefreshScrollContentForCurrentGrid();
        }
    }

    private static void LogDev(string message)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[LevelSelectLayout] " + message);
#endif
    }

    private IEnumerator LogColumnsAfterFrames()
    {
        yield return null;
        LogDev("FRAME+1 columns=" + ReadColumns() +
               " scroll=" + (levelScrollView != null ? levelScrollView.rect.size.ToString() : "null"));
        yield return null;
        LogDev("FRAME+2 columns=" + ReadColumns() +
               " scroll=" + (levelScrollView != null ? levelScrollView.rect.size.ToString() : "null"));
    }

#if UNITY_EDITOR
    public void EditorApplyPreview(GameplayLayoutKind kind)
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[LevelSelectLayout] Apply Preview is Edit Mode only.");
            return;
        }

        ResolveRefs();
        CachePhonePresentation();

        if (editorPreviewSnapshotValid)
        {
            RestoreEditorPreviewSnapshot();
        }
        else
        {
            CaptureEditorPreviewSnapshot();
        }

        IsAuthoringPreviewActive = true;
        AuthoringPreviewKind = kind;
        appliedKind = kind;
        lastWidth = Screen.width;
        lastHeight = Screen.height;

        RestorePhonePresentation();
        if (kind == GameplayLayoutKind.WideTabletLandscape)
        {
            ApplyWidePresentation();
            ForceUnityLayoutRebuild();
        }
        else
        {
            ApplyPhoneHeaderLayout();
            ForceUnityLayoutRebuild();
        }

        LogDev(
            "Preview applied once kind=" + kind +
            " columns=" + ReadColumns() +
            " scrollAnchors=" +
            (levelScrollView != null
                ? levelScrollView.anchorMin + "→" + levelScrollView.anchorMax
                : "null"));
    }

    /// <summary>
    /// Captures SafeArea-relative header/difficulty/grid RELATIONSHIPS (gaps),
    /// not absolute Y positions. Tall/Compact only.
    /// </summary>
    public void EditorCapturePhoneHeaderPaddings()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[LevelSelectLayout] Capture Phone Header is Edit Mode only.");
            return;
        }

        ResolveRefs();
        if (safeArea == null || titleText == null)
        {
            Debug.LogWarning("[LevelSelectLayout] SafeArea/title missing — cannot capture.");
            return;
        }

        SafeArea saComp = safeArea.GetComponent<SafeArea>();
        if (saComp != null)
        {
            saComp.ForceApply();
        }

        Canvas.ForceUpdateCanvases();
        Rect saRect = safeArea.rect;

        if (!TryGetRectBoundsInSafeArea(
                titleText, out float tMinX, out float tMaxX, out float tMinY, out float tMaxY))
        {
            return;
        }

        phoneHeaderTopPadding = Mathf.Max(0f, saRect.yMax - tMaxY);

        if (backButton != null &&
            TryGetRectBoundsInSafeArea(backButton, out _, out float bMaxX, out _, out _))
        {
            phoneBackToTitleGap = Mathf.Max(0f, tMinX - bMaxX);
        }

        if (livesHud != null &&
            TryGetRectBoundsInSafeArea(livesHud, out float hMinX, out _, out _, out _))
        {
            phoneTitleToLivesGap = Mathf.Max(0f, hMinX - tMaxX);
        }

        float headerBottom = tMinY;
        if (backButton != null &&
            TryGetRectBoundsInSafeArea(backButton, out _, out _, out float bMinY, out _))
        {
            headerBottom = Mathf.Min(headerBottom, bMinY);
        }

        if (livesHud != null &&
            TryGetRectBoundsInSafeArea(livesHud, out _, out _, out float hMinY, out _))
        {
            headerBottom = Mathf.Min(headerBottom, hMinY);
        }

        if (difficultyTabs != null &&
            TryGetRectBoundsInSafeArea(
                difficultyTabs, out _, out _, out float dMinY, out float dMaxY))
        {
            phoneHeaderToDifficultyGap = Mathf.Max(0f, headerBottom - dMaxY);

            if (levelScrollView != null &&
                TryGetRectBoundsInSafeArea(
                    levelScrollView, out _, out _, out float sMinY, out float sMaxY))
            {
                phoneDifficultyToGridGap = Mathf.Max(0f, dMinY - sMaxY);
                phoneGridBottomPadding = Mathf.Max(0f, sMinY - saRect.yMin);
            }
        }

        Debug.Log(
            "[LevelSelectLayout] Captured phone stack gaps: " +
            "headerTopPad=" + phoneHeaderTopPadding.ToString("0.##") +
            " backToTitle=" + phoneBackToTitleGap.ToString("0.##") +
            " titleToLives=" + phoneTitleToLivesGap.ToString("0.##") +
            " headerToDiff=" + phoneHeaderToDifficultyGap.ToString("0.##") +
            " diffToGrid=" + phoneDifficultyToGridGap.ToString("0.##") +
            " gridBottomPad=" + phoneGridBottomPadding.ToString("0.##"));
    }

    public void EditorCaptureCurrentWideLayout()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[LevelSelectLayout] Capture Wide is Edit Mode only.");
            return;
        }

        ResolveRefs();

        if (titleText != null)
        {
            wideTitle = WideElementLayout.FromRect(titleText);
        }

        if (backButton != null)
        {
            wideBackButtonPosition = backButton.anchoredPosition;
            wideBackButtonScale = backButton.localScale;
        }

        if (difficultyTabs != null)
        {
            wideDifficultyTabs = WideElementLayout.FromRect(difficultyTabs);
        }

        if (levelScrollView != null)
        {
            wideLevelScrollView = WideElementLayout.FromRect(levelScrollView);
            wideScrollAnchorMin = levelScrollView.anchorMin;
            wideScrollAnchorMax = levelScrollView.anchorMax;
        }

        if (topBar != null)
        {
            wideTopBarLocalPosition = topBar.localPosition;
            wideTopBarLocalScale = topBar.localScale;
        }

        if (gridLayout != null)
        {
            wideColumnCount = gridLayout.constraintCount;
            wideCellSize = gridLayout.cellSize;
            wideSpacing = gridLayout.spacing;
            widePaddingLeft = gridLayout.padding.left;
            widePaddingRight = gridLayout.padding.right;
            widePaddingTop = gridLayout.padding.top;
            widePaddingBottom = gridLayout.padding.bottom;
        }

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        LogDev("Captured Wide columns=" + wideColumnCount);
    }

    public void EditorClearPreview()
    {
        if (Application.isPlaying)
        {
            return;
        }

        ResolveRefs();
        if (editorPreviewSnapshotValid)
        {
            RestoreEditorPreviewSnapshot();
        }
        else
        {
            RestorePhonePresentation();
        }

        ClearEditorPreviewSnapshot();
        IsAuthoringPreviewActive = false;
        AuthoringPreviewKind = GameplayLayoutKind.TallPhonePortrait;
        appliedKind = GameplayLayoutKind.TallPhonePortrait;
        LogDev("Preview cleared");
    }

    public void EditorAutoClearPreviewForResolutionChange()
    {
        if (IsAuthoringPreviewActive)
        {
            EditorClearPreview();
        }
    }

    private void CaptureEditorPreviewSnapshot()
    {
        editorPreviewSnapshot.Clear();
        RectTransform[] targets =
        {
            titleText, backButton, difficultyTabs, levelScrollView, levelGrid,
            viewport, background, backgroundOverlay
        };

        for (int i = 0; i < targets.Length; i++)
        {
            RectTransform rt = targets[i];
            if (rt == null)
            {
                continue;
            }

            editorPreviewSnapshot.Add(new EditorLiveSnap
            {
                Rt = rt,
                AnchorMin = rt.anchorMin,
                AnchorMax = rt.anchorMax,
                Pivot = rt.pivot,
                AnchoredPosition = rt.anchoredPosition,
                SizeDelta = rt.sizeDelta,
                LocalScale = rt.localScale,
                LocalEuler = rt.localEulerAngles
            });
        }

        editorTopBarSnapValid = false;
        if (topBar != null)
        {
            editorTopBarSnap = new TransformSnap
            {
                LocalPosition = topBar.localPosition,
                LocalScale = topBar.localScale,
                LocalRotation = topBar.localRotation,
                Valid = true
            };
            editorTopBarSnapValid = true;
        }

        editorGridSnapValid = false;
        if (gridLayout != null)
        {
            editorGridSnap = CaptureGrid(gridLayout);
            editorGridSnapValid = true;
        }

        editorPreviewSnapshotValid = editorPreviewSnapshot.Count > 0 || editorTopBarSnapValid;
    }

    private void RestoreEditorPreviewSnapshot()
    {
        for (int i = 0; i < editorPreviewSnapshot.Count; i++)
        {
            EditorLiveSnap snap = editorPreviewSnapshot[i];
            if (snap.Rt == null)
            {
                continue;
            }

            snap.Rt.anchorMin = snap.AnchorMin;
            snap.Rt.anchorMax = snap.AnchorMax;
            snap.Rt.pivot = snap.Pivot;
            snap.Rt.anchoredPosition = snap.AnchoredPosition;
            snap.Rt.sizeDelta = snap.SizeDelta;
            snap.Rt.localScale = snap.LocalScale;
            snap.Rt.localEulerAngles = snap.LocalEuler;
        }

        if (editorTopBarSnapValid && topBar != null)
        {
            topBar.localPosition = editorTopBarSnap.LocalPosition;
            topBar.localScale = editorTopBarSnap.LocalScale;
            topBar.localRotation = editorTopBarSnap.LocalRotation;
        }

        if (editorGridSnapValid && gridLayout != null)
        {
            ApplyGridSnap(gridLayout, editorGridSnap);
        }
    }

    private void ClearEditorPreviewSnapshot()
    {
        editorPreviewSnapshot.Clear();
        editorPreviewSnapshotValid = false;
        editorTopBarSnapValid = false;
        editorGridSnapValid = false;
    }
#endif
}

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
        LogDev("Awake begin");
        ResolveRefs();
        CachePhonePresentation();
        ApplyCurrentResponsiveLayout(force: true, reason: "Awake");
    }

    private void Start()
    {
        ApplyCurrentResponsiveLayout(force: true, reason: "Start");
    }

    private void LateUpdate()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            ApplyCurrentResponsiveLayout(force: true, reason: "ScreenChanged");
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
        {
            return;
        }

        if (appliedKind == GameplayLayoutKind.WideTabletLandscape)
        {
            ApplyCurrentResponsiveLayout(force: true, reason: "OnValidate.Play");
        }
    }
#endif

    /// <summary>
    /// Called by LevelSelectUI after tiles are rebuilt so Wide is never lost
    /// to later content setup.
    /// </summary>
    public void ApplyAfterContentReady()
    {
        ApplyCurrentResponsiveLayout(force: true, reason: "AfterContentReady");
    }

    /// <summary>
    /// Single authoritative apply path for runtime + editor preview.
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
            Screen.width == lastWidth &&
            Screen.height == lastHeight)
        {
            return;
        }

        lastWidth = Screen.width;
        lastHeight = Screen.height;
        appliedKind = want;

        CachePhonePresentation();
        RestorePhonePresentation();

        int columnsBefore = ReadColumns();

        if (want == GameplayLayoutKind.WideTabletLandscape)
        {
            ApplyWidePresentation();
        }

        ForceUnityLayoutRebuild();
        NotifyLevelSelectScroll();

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
        if (canvasRoot == null)
        {
            Canvas selfCanvas = GetComponent<Canvas>();
            if (selfCanvas != null)
            {
                rootCanvas = selfCanvas;
                canvasRoot = selfCanvas.transform as RectTransform;
            }
            else
            {
                Canvas found = FindAnyObjectByType<Canvas>();
                if (found != null)
                {
                    rootCanvas = found;
                    canvasRoot = found.transform as RectTransform;
                }
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
                levelSelectUI = FindAnyObjectByType<LevelSelectUI>();
            }
        }
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
        if (phoneCached)
        {
            return;
        }

        CapturePhone(titleText);
        CapturePhone(backButton);
        CapturePhone(difficultyTabs);
        CapturePhone(levelScrollView);
        CapturePhone(levelGrid);
        CapturePhone(background);
        CapturePhone(backgroundOverlay);

        if (topBar != null)
        {
            phoneTopBarSnap = new TransformSnap
            {
                LocalPosition = topBar.localPosition,
                LocalScale = topBar.localScale,
                LocalRotation = topBar.localRotation,
                Valid = true
            };
        }

        if (gridLayout != null)
        {
            phoneGridSnap = CaptureGrid(gridLayout);
        }

        phoneCached = true;
        LogDev(
            "Phone grid cached columns=" +
            (phoneGridSnap.Valid ? phoneGridSnap.ConstraintCount.ToString() : "invalid"));
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

    private void ApplyWidePresentation()
    {
        if (topBar != null)
        {
            topBar.localPosition = wideTopBarLocalPosition;
            topBar.localScale = wideTopBarLocalScale;
        }

        ApplyWideElement(titleText, wideTitle);
        ApplyWideBackButton();
        ApplyWideElement(difficultyTabs, wideDifficultyTabs);
        ApplyWideScrollView();
        ApplyWideGrid();
        ApplyWideBackgroundCover();
    }

    /// <summary>
    /// BackButton is a RectTransform child of TopBar (plain Transform).
    /// Positions must be TopBar-local — canvas-space values like -820 push it off-screen.
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
        if (levelScrollView == null)
        {
            LogDev("Wide scrollView MISSING");
            return;
        }

        // Stretch to tablet width — absolute 1800px fails when canvas units differ.
        levelScrollView.anchorMin = wideScrollAnchorMin;
        levelScrollView.anchorMax = wideScrollAnchorMax;
        levelScrollView.pivot = new Vector2(0.5f, 0.5f);
        levelScrollView.offsetMin = Vector2.zero;
        levelScrollView.offsetMax = Vector2.zero;
        levelScrollView.localScale = Vector3.one;

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
            // Full width of viewport; height owned by LevelSelectUI scroll refresh.
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

        LogDev(
            "Preview applied once kind=" + kind +
            " columns=" + ReadColumns() +
            " scrollAnchors=" +
            (levelScrollView != null
                ? levelScrollView.anchorMin + "→" + levelScrollView.anchorMax
                : "null"));
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

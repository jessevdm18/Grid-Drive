using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Switches Gameplay HUD between authored phone layout and wide-landscape
/// two-column layout (left HUD ~30%, right board).
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-50)]
public class GameplayLayoutController : MonoBehaviour
{
    private struct RectSnapshot
    {
        public Transform Parent;
        public int SiblingIndex;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 Pivot;
        public Vector2 AnchoredPosition;
        public Vector2 SizeDelta;
        public Vector3 LocalScale;
        public Quaternion LocalRotation;
        public bool HasHorizontalLayout;
        public bool HorizontalLayoutEnabled;
        public TextAnchor ChildAlignment;
        public float Spacing;
        public bool ChildForceExpandWidth;
        public bool ChildForceExpandHeight;
        public bool ChildControlWidth;
        public bool ChildControlHeight;
    }

    [Header("Authored Layout Profiles")]
    [Tooltip("Tall phone camera reserves only. Transform captures are ignored at runtime (procedural).")]
    [SerializeField] private GameplayLayoutProfile tallPhoneProfile = new GameplayLayoutProfile
    {
        topReservedFraction = 0.16f,
        bottomReservedFraction = 0.16f,
        leftHudWidthFraction = 0.30f
    };

    [Tooltip("Compact phone camera reserves only. Transform captures are ignored at runtime (procedural).")]
    [SerializeField] private GameplayLayoutProfile compactPhoneProfile = new GameplayLayoutProfile
    {
        topReservedFraction = 0.18f,
        bottomReservedFraction = 0.16f,
        leftHudWidthFraction = 0.30f
    };

    [SerializeField] private GameplayLayoutProfile wideTabletProfile = new GameplayLayoutProfile
    {
        topReservedFraction = 0.04f,
        bottomReservedFraction = 0.04f,
        leftHudWidthFraction = 0.30f
    };

    [Tooltip(
        "When true, a captured Compact Phone profile is applied at runtime " +
        "(after restoring the immutable Tall baseline). Tall captures are never used.")]
    [SerializeField] private bool allowCompactCapturedProfile = true;

    [Tooltip(
        "When true, a captured Wide Tablet profile is applied at runtime " +
        "(after restoring the immutable phone baseline and building ONE tablet scaffold).")]
    [SerializeField] private bool allowWideCapturedProfile = true;

    /// <summary>
    /// Immutable tall-phone hierarchy/transforms. Never overwritten by preview/capture/runtime.
    /// Seeded once from the scene-authored phone layout.
    /// </summary>
    [SerializeField, HideInInspector]
    private GameplayLayoutProfile immutablePhoneBaseline = new GameplayLayoutProfile();

    [Header("Detection")]
    [SerializeField] private float wideAspectThreshold = GameplayLayoutMode.DefaultWideAspectThreshold;
    [SerializeField] private float tallPhoneMaxAspect = GameplayLayoutMode.DefaultTallPhoneMaxAspect;

    [Header("Compact Phone Portrait (fractions of SafeArea height)")]
    [SerializeField, Range(0.01f, 0.08f)] private float compactTopHudPaddingFrac = 0.015f;
    [SerializeField, Range(0.08f, 0.18f)] private float compactTopHudHeightFrac = 0.11f;
    [SerializeField, Range(0.005f, 0.04f)] private float compactObjectiveGapFrac = 0.01f;
    [SerializeField, Range(0.035f, 0.08f)] private float compactDifficultyBlockFrac = 0.045f;
    [SerializeField, Range(0.06f, 0.12f)] private float compactMissionBlockFrac = 0.085f;
    [SerializeField, Range(1.2f, 2.0f)] private float compactObjectiveScale = 1.55f;
    [SerializeField, Range(1.0f, 1.35f)] private float compactCardScale = 1.12f;
    [SerializeField, Range(0.10f, 0.20f)] private float compactHintFrac = 0.155f;
    [SerializeField, Range(0.05f, 0.12f)] private float compactRewardedHintFrac = 0.075f;
    [SerializeField, Range(0.05f, 0.12f)] private float compactPauseTopFrac = 0.07f;

    [Header("Phone Special Objective Layout")]
    [Tooltip("Tall + Compact only. Multiplier on Mission Label localScale (AMBULANCE RESCUE, etc.). 1 = current size.")]
    [SerializeField, Min(0.1f)] private float phoneMissionLabelScale = 1f;
    [Tooltip("Tall + Compact only. Multiplier on secondary text localScale (timer, count, rule text). 1 = current size.")]
    [SerializeField, Min(0.1f)] private float phoneSecondaryTextScale = 1f;
    [Tooltip("Screen-pixel gap between Mission Label and secondary text.")]
    [SerializeField, Min(0f)] private float phoneObjectiveInternalGapPx = 8f;
    [Tooltip("Screen-pixel gap between secondary objective bottom and board top.")]
    [SerializeField, Min(0f)] private float phoneObjectiveBoardGapPx = 30f;

    [Header("Tablet Column")]
    [SerializeField, Range(0.22f, 0.36f)] private float leftHudWidthFraction = 0.30f;
    [SerializeField] private float tabletPanelPadding = 16f;
    [SerializeField] private float tabletSectionSpacing = 12f;
    [SerializeField] private float tabletControlSpacing = 24f;
    [SerializeField] private float objectiveHudTabletScale = 1.05f;

    [Header("Wide Tablet Modal Panels")]
    [Tooltip("Multiplier on authored PausePanel localScale for Wide Tablet.")]
    [SerializeField, Range(0.45f, 1f)] private float widePausePanelScale = 0.75f;
    [Tooltip("Multiplier on authored WinPanel localScale for Wide Tablet.")]
    [SerializeField, Range(0.45f, 1f)] private float wideWinPanelScale = 0.80f;
    [SerializeField] private Vector2 widePausePanelOffset = Vector2.zero;
    [SerializeField] private Vector2 wideWinPanelOffset = Vector2.zero;

    [Header("Core HUD")]
    [SerializeField] private CameraFitter cameraFitter;
    [SerializeField] private RectTransform safeArea;
    [SerializeField] private RectTransform topHud;
    [SerializeField] private RectTransform pauseButton;
    [SerializeField] private RectTransform undoButton;
    [SerializeField] private RectTransform hintButton;
    [SerializeField] private RectTransform rewardedHintButton;
    [Tooltip("Insufficient-coins / hint status message. Layout-owned above HintButton.")]
    [SerializeField] private RectTransform hintStatusText;
    [SerializeField, Range(8f, 64f)] private float hintStatusGap = 28f;

    [Header("Modal Panels (Pause / Win)")]
    [SerializeField] private RectTransform pausePanel;
    [SerializeField] private RectTransform winPanel;

    [Header("Objective / special HUD roots")]
    [SerializeField] private RectTransform[] objectiveHudRoots;

    private readonly Dictionary<RectTransform, RectSnapshot> phoneSnapshots =
        new Dictionary<RectTransform, RectSnapshot>(16);

    private readonly Dictionary<RectTransform, RectSnapshot> objectiveChildPhoneSnapshots =
        new Dictionary<RectTransform, RectSnapshot>(32);

    // Base localScale after restore/capture; Inspector multipliers apply without compounding.
    private readonly Dictionary<RectTransform, Vector3> phoneObjectiveTextBaseScales =
        new Dictionary<RectTransform, Vector3>(8);

    private RectTransform tabletHudPanel;
    private RectTransform objectiveSection;
    private RectTransform statsSection;
    private RectTransform levelMovesRow;
    private RectTransform controlsRow;
    private RectTransform flexSpacer;

    private HorizontalLayoutGroup topHudHorizontalLayout;
    private RectTransform levelCard;
    private RectTransform movesCard;
    private RectTransform coinCard;
    private RectTransform difficultyLabelRoot;

    private bool phoneCached;
    private bool modalPanelsPhoneCached;
    private RectSnapshot pausePanelPhoneSnap;
    private RectSnapshot winPanelPhoneSnap;
    private GameplayLayoutKind appliedKind = (GameplayLayoutKind)(-1);
    private int lastWidth = -1;
    private int lastHeight = -1;
    private int lastCompactObjectiveActiveMask = int.MinValue;
    private Rect lastSafeAreaCanvasRect;
    private bool hasLastSafeAreaCanvasRect;
    private bool safeAreaReapplyPending;
    private SafeArea safeAreaComponent;

    // Compact Mission Label final/home — written by board-relative phone mission layout.
    private RectTransform resolvedCompactMissionLabel;
    private RectTransformState resolvedCompactMissionLabelHome;
    private bool hasResolvedCompactMissionLabelHome;

    // Tall / phone Mission Label final/home — written by ApplyPhoneMissionLayoutRelativeToBoard.
    private RectTransform resolvedTallMissionLabel;
    private RectTransformState resolvedTallMissionLabelHome;
    private bool hasResolvedTallMissionLabelHome;

    // Shared phone mission home alias (Tall + Compact board-relative).
    private RectTransform resolvedPhoneMissionLabel;
    private RectTransformState resolvedPhoneMissionLabelHome;
    private bool hasResolvedPhoneMissionLabelHome;

    public bool IsWideLayoutActive =>
        appliedKind == GameplayLayoutKind.WideTabletLandscape;

    public GameplayLayoutKind AppliedKind => appliedKind;
    public float LeftHudWidthFraction => leftHudWidthFraction;

    public GameplayLayoutProfile TallPhoneProfile => tallPhoneProfile;
    public GameplayLayoutProfile CompactPhoneProfile => compactPhoneProfile;
    public GameplayLayoutProfile WideTabletProfile => wideTabletProfile;

    /// <summary>Editor preview mode currently applied (or runtime applied kind).</summary>
    public GameplayLayoutKind AuthoringPreviewKind { get; private set; } =
        GameplayLayoutKind.TallPhonePortrait;

    public bool IsAuthoringPreviewActive { get; private set; }

    private void Awake()
    {
        ResolveRefs();
        EnsureSafeAreaApplied();
        DiscardTallCapturedProfile();
        EnsureImmutablePhoneBaseline();
        CachePhoneLayout();
        CacheModalPanelsPhonePresentation();
        // If Edit Mode preview left the hierarchy dirty, restore before layout.
        RestoreAuthoredPhoneBaseline();
        EnsureTabletScaffold();
        ApplyModalPanelsForKind(GameplayLayoutKind.TallPhonePortrait);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[GameplayLayout] Awake\n" +
            "Screen=" + Screen.width + "x" + Screen.height + "\n" +
            "Aspect=" + GameplayLayoutMode.GetScreenAspect().ToString("0.000") + "\n" +
            "Kind=" + GameplayLayoutMode.Resolve(tallPhoneMaxAspect, wideAspectThreshold) + "\n" +
            "compactCaptureAllowed=" + allowCompactCapturedProfile + "\n" +
            "compactHasCapture=" +
            (compactPhoneProfile != null && compactPhoneProfile.hasCapture) + "\n" +
            "baselineEntries=" +
            (immutablePhoneBaseline != null && immutablePhoneBaseline.entries != null
                ? immutablePhoneBaseline.entries.Count
                : 0) + "\n" +
            "safeArea=" + (safeArea != null) +
            " topHud=" + (topHud != null) +
            " pause=" + (pauseButton != null) +
            " undo=" + (undoButton != null) +
            " hint=" + (hintButton != null) +
            " rewarded=" + (rewardedHintButton != null) +
            " objectives=" + (objectiveHudRoots != null ? objectiveHudRoots.Length : 0)
        );
        LogUILayoutAudit("Awake");
#endif
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        SafeArea.OnApplied += OnSafeAreaApplied;

        if (Application.isPlaying || IsAuthoringPreviewActive)
        {
            return;
        }

        // Repair contaminated phone hierarchy after domain reload / dirty preview.
        UnityEditor.EditorApplication.delayCall += EditorAutoRepairPhoneHierarchy;
    }

    private void OnDisable()
    {
        SafeArea.OnApplied -= OnSafeAreaApplied;
    }
#else
    private void OnEnable()
    {
        SafeArea.OnApplied += OnSafeAreaApplied;
    }

    private void OnDisable()
    {
        SafeArea.OnApplied -= OnSafeAreaApplied;
    }
#endif

    private void OnSafeAreaApplied(SafeArea applied)
    {
        if (applied == null || safeArea == null || applied.transform != safeArea)
        {
            return;
        }

        safeAreaReapplyPending = true;
    }

#if UNITY_EDITOR
    private void EditorAutoRepairPhoneHierarchy()
    {
        if (this == null || Application.isPlaying || IsAuthoringPreviewActive)
        {
            return;
        }

        EditorRepairPhoneHierarchy(markSceneDirty: true);
    }

    /// <summary>
    /// Edit Mode: restore Level/Moves/Coins under TopHUD and remove duplicate tablet panels.
    /// Does not rebake Tall baseline or overwrite Compact/Wide captures.
    /// </summary>
    public void EditorRepairPhoneHierarchy(bool markSceneDirty)
    {
        ResolveRefs();
        RestoreCanonicalPhoneHierarchy();
        CleanupDuplicateTabletHudPanels();

        Debug.Log(
            "[GameplayLayoutPreview] Phone hierarchy repaired\n" +
            "TopHUD children=" + (topHud != null ? topHud.childCount : 0) + "\n" +
            "Level parent=" + (levelCard != null && levelCard.parent != null
                ? levelCard.parent.name
                : "null") + "\n" +
            "Moves parent=" + (movesCard != null && movesCard.parent != null
                ? movesCard.parent.name
                : "null") + "\n" +
            "Coin parent=" + (coinCard != null && coinCard.parent != null
                ? coinCard.parent.name
                : "null"));

        if (markSceneDirty)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            if (topHud != null)
            {
                UnityEditor.EditorUtility.SetDirty(topHud.gameObject);
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }
#endif

    private void Start()
    {
        EnsureSafeAreaApplied();
        ApplyLayout(force: true);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LogUILayoutAudit("Start");
        StartCoroutine(LogUILayoutAuditAfterFrames());
#endif
    }

    private void LateUpdate()
    {
        // Catch Game View resolution changes, SafeArea inset changes, and late HUD activation.
        if (Screen.width != lastWidth ||
            Screen.height != lastHeight ||
            safeAreaReapplyPending ||
            HasSafeAreaCanvasRectChanged())
        {
            safeAreaReapplyPending = false;
            ApplyLayout(force: true);
            return;
        }

        if (appliedKind == GameplayLayoutKind.WideTabletLandscape)
        {
            // Ownership guard: nothing may leave TabletHUDPanel inactive while Wide.
            SyncTabletHudPanelActiveToKind(
                GameplayLayoutKind.WideTabletLandscape,
                "LateUpdate.guard");
            EnsureActiveObjectivesParented();
        }
        else if (appliedKind == GameplayLayoutKind.CompactPhonePortrait)
        {
            int mask = BuildActiveObjectiveMask();

            // Intro owns Mission Label transforms only.
            if (SpecialMissionIntroController.IsIntroPlaying)
            {
                lastCompactObjectiveActiveMask = mask;
                return;
            }

            if (mask == lastCompactObjectiveActiveMask)
            {
                return;
            }

            lastCompactObjectiveActiveMask = mask;

            // Keep Compact non-mission HUD (incl. Difficulty) from capture/procedural,
            // then override ONLY active special mission text to board-relative.
            if (HasUsableCompactCapture())
            {
                RestoreAuthoredPhoneBaseline();
                ApplyCapturedProfile(
                    GameplayLayoutKind.CompactPhonePortrait,
                    compactPhoneProfile);
                ClearPhoneObjectiveTextBaseScales();
            }
            else
            {
                float safeH = Mathf.Max(1f, safeArea != null ? safeArea.rect.height : 1f);
                float topPad = safeH * compactTopHudPaddingFrac;
                float topHudH = safeH * compactTopHudHeightFrac;
                StackCompactDifficultyOnly(
                    topPad + topHudH + safeH * compactObjectiveGapFrac,
                    safeH);
                ClearPhoneObjectiveTextBaseScales();
            }

            ApplyPhoneMissionLayoutRelativeToBoard();
            ApplyHintStatusLayout();
        }
        else if (appliedKind == GameplayLayoutKind.TallPhonePortrait)
        {
            int mask = BuildActiveObjectiveMask();
            if (mask != lastCompactObjectiveActiveMask)
            {
                lastCompactObjectiveActiveMask = mask;
                ApplyPhoneMissionLayoutRelativeToBoard();
                ApplyHintStatusLayout();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                LogUILayoutAudit("TallObjectiveActivated");
#endif
            }
        }
    }

    private void ResolveRefs()
    {
        if (cameraFitter == null)
        {
            cameraFitter = FindAnyObjectByType<CameraFitter>();
        }

        if (safeArea == null)
        {
            SafeArea sa = GetComponent<SafeArea>();
            if (sa == null)
            {
                sa = FindAnyObjectByType<SafeArea>();
            }

            if (sa != null)
            {
                safeAreaComponent = sa;
                safeArea = sa.transform as RectTransform;
            }
        }

        if (safeArea == null)
        {
            return;
        }

        if (topHud == null)
        {
            topHud = FindChildRect(safeArea, "TopHUD");
        }

        if (pauseButton == null)
        {
            pauseButton = FindChildRect(safeArea, "PauseButton");
        }

        if (undoButton == null)
        {
            undoButton = FindChildRect(safeArea, "UndoButton");
        }

        if (hintButton == null)
        {
            hintButton = FindChildRect(safeArea, "HintButton");
        }

        if (rewardedHintButton == null)
        {
            rewardedHintButton = FindChildRect(safeArea, "RewardedHintButton");
        }

        if (hintStatusText == null)
        {
            hintStatusText = FindChildRect(safeArea, "HintStatusText");
        }

        if (pausePanel == null)
        {
            pausePanel = FindChildRect(safeArea, "PausePanel");
        }

        if (winPanel == null)
        {
            RectTransform canvasRt = safeArea.parent as RectTransform;
            if (canvasRt != null)
            {
                winPanel = FindChildRect(canvasRt, "WinPanel");
            }
        }

        if (topHud != null)
        {
            topHudHorizontalLayout = topHud.GetComponent<HorizontalLayoutGroup>();
        }

        // Cards may be under TopHUD (phone) OR tablet rows after Wide preview.
        // Never only search TopHUD — that nulls refs and blocks hierarchy restore.
        levelCard = ResolveHudCard("LevelCard", levelCard);
        movesCard = ResolveHudCard("MovesCard", movesCard);
        coinCard = ResolveHudCard("CoinCard", coinCard);

        if (difficultyLabelRoot == null)
        {
            difficultyLabelRoot = FindChildRect(safeArea, "DifficultyLabel");
        }

        if (objectiveHudRoots == null || objectiveHudRoots.Length == 0)
        {
            AutoFindObjectiveRoots();
        }

        if (difficultyLabelRoot == null && objectiveHudRoots != null)
        {
            for (int i = 0; i < objectiveHudRoots.Length; i++)
            {
                if (objectiveHudRoots[i] != null &&
                    objectiveHudRoots[i].name == "DifficultyLabel")
                {
                    difficultyLabelRoot = objectiveHudRoots[i];
                    break;
                }
            }
        }
    }

    private void AutoFindObjectiveRoots()
    {
        string[] names =
        {
            "TimedHudRoot",
            "MoveLimitHudRoot",
            "MultiTargetHudRoot",
            "MultiTargetHudRoot (1)",
            "NoTouchHudRoot",
            "FragileCargoHudRoot",
            "LimitedVehicleHudRoot",
            "DifficultyLabel"
        };

        List<RectTransform> found = new List<RectTransform>(names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            RectTransform rt = FindChildRect(safeArea, names[i]);
            if (rt != null)
            {
                found.Add(rt);
            }
        }

        objectiveHudRoots = found.ToArray();
    }

    private static RectTransform FindChildRect(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        Transform t = parent.Find(childName);
        return t as RectTransform;
    }

    private void CachePhoneLayout()
    {
        if (phoneCached)
        {
            return;
        }

        // Prefer immutable baseline snapshots over whatever the live hierarchy
        // currently shows (preview/capture may have mutated the scene).
        if (immutablePhoneBaseline != null &&
            immutablePhoneBaseline.hasCapture &&
            immutablePhoneBaseline.entries != null &&
            immutablePhoneBaseline.entries.Count > 0)
        {
            RebuildRuntimeSnapshotsFromBaseline();
            phoneCached = true;
            return;
        }

        CaptureInto(topHud);
        CaptureInto(pauseButton);
        CaptureInto(undoButton);
        CaptureInto(hintButton);
        CaptureInto(rewardedHintButton);
        CaptureInto(levelCard);
        CaptureInto(movesCard);
        CaptureInto(coinCard);

        if (objectiveHudRoots != null)
        {
            for (int i = 0; i < objectiveHudRoots.Length; i++)
            {
                CaptureInto(objectiveHudRoots[i]);
                CacheObjectiveChildren(objectiveHudRoots[i]);
            }
        }

        phoneCached = true;
    }

    /// <summary>
    /// Seeds or refreshes runtime restore caches from the immutable phone baseline.
    /// </summary>
    private void RebuildRuntimeSnapshotsFromBaseline()
    {
        phoneSnapshots.Clear();
        objectiveChildPhoneSnapshots.Clear();

        if (immutablePhoneBaseline == null || immutablePhoneBaseline.entries == null)
        {
            return;
        }

        for (int i = 0; i < immutablePhoneBaseline.entries.Count; i++)
        {
            RectTransformState state = immutablePhoneBaseline.entries[i];
            RectTransform rt = ResolveKey(state.key);
            if (rt == null)
            {
                continue;
            }

            RectSnapshot snap = SnapshotFromState(state, rt);
            if (state.key != null && state.key.IndexOf('/') >= 0)
            {
                objectiveChildPhoneSnapshots[rt] = snap;
            }
            else
            {
                phoneSnapshots[rt] = snap;
            }
        }
    }

    private RectSnapshot SnapshotFromState(RectTransformState state, RectTransform rt)
    {
        RectSnapshot snap = default;
        RectTransform parent = ResolveParentKey(state.parentKey);
        snap.Parent = parent != null ? parent : (rt != null ? rt.parent : null);
        snap.SiblingIndex = state.siblingIndex;
        snap.AnchorMin = state.anchorMin;
        snap.AnchorMax = state.anchorMax;
        snap.Pivot = state.pivot;
        snap.AnchoredPosition = state.anchoredPosition;
        snap.SizeDelta = state.sizeDelta;
        snap.LocalScale = state.localScale;
        snap.LocalRotation = Quaternion.Euler(state.localEulerAngles);
        snap.HasHorizontalLayout = state.hasHorizontalLayout;
        snap.HorizontalLayoutEnabled = state.horizontalLayoutEnabled;
        snap.ChildAlignment = state.childAlignment;
        snap.Spacing = state.spacing;
        snap.ChildForceExpandWidth = state.childForceExpandWidth;
        snap.ChildForceExpandHeight = state.childForceExpandHeight;
        snap.ChildControlWidth = state.childControlWidth;
        snap.ChildControlHeight = state.childControlHeight;
        return snap;
    }

    /// <summary>
    /// One-time bake of scene-authored tall phone hierarchy. Never updated by
    /// preview, capture, or mode switching.
    /// </summary>
    private void EnsureImmutablePhoneBaseline()
    {
        if (immutablePhoneBaseline == null)
        {
            immutablePhoneBaseline = new GameplayLayoutProfile();
        }

        if (immutablePhoneBaseline.hasCapture &&
            immutablePhoneBaseline.entries != null &&
            immutablePhoneBaseline.entries.Count > 0)
        {
            return;
        }

        // Must capture from live scene while it still matches authored tall phone.
        // Call before any preview/tablet mutate.
        immutablePhoneBaseline.entries = new List<RectTransformState>(48);
        CapturePhoneBaselineTransforms(immutablePhoneBaseline);
        immutablePhoneBaseline.hasCapture = true;
        immutablePhoneBaseline.topReservedFraction = 0.16f;
        immutablePhoneBaseline.bottomReservedFraction = 0.16f;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[GameplayLayout] Seeded immutable phone baseline (" +
            immutablePhoneBaseline.entries.Count + " entries)");
#endif
    }

    private void CapturePhoneBaselineTransforms(GameplayLayoutProfile profile)
    {
        CaptureOne(profile, "TopHUD", topHud);
        CaptureOne(profile, "PauseButton", pauseButton);
        CaptureOne(profile, "UndoButton", undoButton);
        CaptureOne(profile, "HintButton", hintButton);
        CaptureOne(profile, "RewardedHintButton", rewardedHintButton);
        CaptureOne(profile, "DifficultyLabel", difficultyLabelRoot);
        CaptureOne(profile, "LevelCard", levelCard);
        CaptureOne(profile, "MovesCard", movesCard);
        CaptureOne(profile, "CoinCard", coinCard);

        if (objectiveHudRoots == null)
        {
            return;
        }

        for (int i = 0; i < objectiveHudRoots.Length; i++)
        {
            RectTransform root = objectiveHudRoots[i];
            if (root == null)
            {
                continue;
            }

            CaptureOne(profile, root.name, root);
            for (int c = 0; c < root.childCount; c++)
            {
                RectTransform child = root.GetChild(c) as RectTransform;
                if (child == null)
                {
                    continue;
                }

                CaptureOne(profile, root.name + "/" + child.name, child);
            }
        }
    }

    /// <summary>
    /// Tall captures are never used — immutable baseline is Tall source of truth.
    /// Compact captures are preserved for runtime authoring.
    /// </summary>
    private void DiscardTallCapturedProfile()
    {
        if (tallPhoneProfile == null || !tallPhoneProfile.hasCapture)
        {
            return;
        }

        DiscardTransformCapture(tallPhoneProfile);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[GameplayLayout] Discarded Tall captured profile — " +
            "immutable baseline owns Tall layout.");
#endif
    }

    private static void DiscardTransformCapture(GameplayLayoutProfile profile)
    {
        if (profile == null)
        {
            return;
        }

        profile.hasCapture = false;
        if (profile.entries != null)
        {
            profile.entries.Clear();
        }
    }

    private bool HasUsableCompactCapture()
    {
        return allowCompactCapturedProfile &&
               compactPhoneProfile != null &&
               compactPhoneProfile.hasCapture &&
               compactPhoneProfile.entries != null &&
               compactPhoneProfile.entries.Count > 0;
    }

    private bool HasUsableWideCapture()
    {
        return allowWideCapturedProfile &&
               wideTabletProfile != null &&
               wideTabletProfile.hasCapture &&
               wideTabletProfile.entries != null &&
               wideTabletProfile.entries.Count > 0;
    }

    private bool ShouldApplyCapturedProfile(
        GameplayLayoutKind kind,
        GameplayLayoutProfile profile)
    {
        if (profile == null || !profile.hasCapture)
        {
            return false;
        }

        switch (kind)
        {
            case GameplayLayoutKind.TallPhonePortrait:
                // Tall is always the immutable baseline — never a mode capture.
                return false;
            case GameplayLayoutKind.CompactPhonePortrait:
                return allowCompactCapturedProfile;
            case GameplayLayoutKind.WideTabletLandscape:
                return allowWideCapturedProfile;
            default:
                return false;
        }
    }

    private void CacheObjectiveChildren(RectTransform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            RectTransform child = root.GetChild(i) as RectTransform;
            if (child == null || objectiveChildPhoneSnapshots.ContainsKey(child))
            {
                continue;
            }

            objectiveChildPhoneSnapshots[child] = Capture(child);
        }
    }

    private void CompactObjectiveChildren(RectTransform root)
    {
        if (root == null)
        {
            return;
        }

        float y = 20f;
        for (int i = 0; i < root.childCount; i++)
        {
            RectTransform child = root.GetChild(i) as RectTransform;
            if (child == null)
            {
                continue;
            }

            child.anchorMin = new Vector2(0.5f, 0.5f);
            child.anchorMax = new Vector2(0.5f, 0.5f);
            child.pivot = new Vector2(0.5f, 0.5f);
            child.anchoredPosition = new Vector2(0f, y);
            y -= 42f;
        }
    }

    private void RestoreObjectiveChildren()
    {
        foreach (KeyValuePair<RectTransform, RectSnapshot> pair in objectiveChildPhoneSnapshots)
        {
            RectTransform child = pair.Key;
            if (child == null)
            {
                continue;
            }

            RectSnapshot snap = pair.Value;
            child.anchorMin = snap.AnchorMin;
            child.anchorMax = snap.AnchorMax;
            child.pivot = snap.Pivot;
            child.anchoredPosition = snap.AnchoredPosition;
            child.sizeDelta = snap.SizeDelta;
            child.localScale = snap.LocalScale;
            child.localRotation = snap.LocalRotation;
        }
    }

    private void CaptureInto(RectTransform rt)
    {
        if (rt == null || phoneSnapshots.ContainsKey(rt))
        {
            return;
        }

        phoneSnapshots[rt] = Capture(rt);
    }

    private static RectSnapshot Capture(RectTransform rt)
    {
        RectSnapshot snap = default;
        snap.Parent = rt.parent;
        snap.SiblingIndex = rt.GetSiblingIndex();
        snap.AnchorMin = rt.anchorMin;
        snap.AnchorMax = rt.anchorMax;
        snap.Pivot = rt.pivot;
        snap.AnchoredPosition = rt.anchoredPosition;
        snap.SizeDelta = rt.sizeDelta;
        snap.LocalScale = rt.localScale;
        snap.LocalRotation = rt.localRotation;

        HorizontalLayoutGroup hlg = rt.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            snap.HasHorizontalLayout = true;
            snap.HorizontalLayoutEnabled = hlg.enabled;
            snap.ChildAlignment = hlg.childAlignment;
            snap.Spacing = hlg.spacing;
            snap.ChildForceExpandWidth = hlg.childForceExpandWidth;
            snap.ChildForceExpandHeight = hlg.childForceExpandHeight;
            snap.ChildControlWidth = hlg.childControlWidth;
            snap.ChildControlHeight = hlg.childControlHeight;
        }

        return snap;
    }

    private void RestoreFromCache(RectTransform rt)
    {
        if (rt == null || !phoneSnapshots.TryGetValue(rt, out RectSnapshot snap))
        {
            return;
        }

        if (snap.Parent == null)
        {
            return;
        }

        rt.SetParent(snap.Parent, false);
        rt.SetSiblingIndex(Mathf.Clamp(snap.SiblingIndex, 0, snap.Parent.childCount - 1));
        rt.anchorMin = snap.AnchorMin;
        rt.anchorMax = snap.AnchorMax;
        rt.pivot = snap.Pivot;
        rt.anchoredPosition = snap.AnchoredPosition;
        rt.sizeDelta = snap.SizeDelta;
        rt.localScale = snap.LocalScale;
        rt.localRotation = snap.LocalRotation;

        if (snap.HasHorizontalLayout)
        {
            HorizontalLayoutGroup hlg = rt.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                hlg.enabled = snap.HorizontalLayoutEnabled;
                hlg.childAlignment = snap.ChildAlignment;
                hlg.spacing = snap.Spacing;
                hlg.childForceExpandWidth = snap.ChildForceExpandWidth;
                hlg.childForceExpandHeight = snap.ChildForceExpandHeight;
                hlg.childControlWidth = snap.ChildControlWidth;
                hlg.childControlHeight = snap.ChildControlHeight;
            }
        }
    }

    private void EnsureTabletScaffold()
    {
        if (safeArea == null)
        {
            return;
        }

        CleanupDuplicateTabletHudPanels();

        if (tabletHudPanel == null)
        {
            // Reuse a leftover panel from Edit Mode preview — never duplicate.
            Transform existing = safeArea.Find("TabletHUDPanel");
            if (existing != null)
            {
                tabletHudPanel = existing as RectTransform;
            }
        }

        if (tabletHudPanel != null)
        {
            WireTabletScaffoldRefs(force: true);
            return;
        }

        tabletHudPanel = CreateStretchPanel("TabletHUDPanel", safeArea);
        // Created inactive; layout mode owns activation (Wide → true, phone → false).
        SetTabletHudPanelActiveOwned(false, "EnsureTabletScaffold.create");

        VerticalLayoutGroup rootLayout = tabletHudPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(
            Mathf.RoundToInt(tabletPanelPadding),
            Mathf.RoundToInt(tabletPanelPadding),
            Mathf.RoundToInt(tabletPanelPadding),
            Mathf.RoundToInt(tabletPanelPadding)
        );
        rootLayout.spacing = tabletSectionSpacing;
        rootLayout.childAlignment = TextAnchor.UpperCenter;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = false;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;

        objectiveSection = CreateSection("TabletObjectiveSection", tabletHudPanel, 90f);
        VerticalLayoutGroup objectiveLayout =
            objectiveSection.gameObject.AddComponent<VerticalLayoutGroup>();
        objectiveLayout.spacing = 4f;
        objectiveLayout.childAlignment = TextAnchor.UpperCenter;
        objectiveLayout.childControlWidth = true;
        objectiveLayout.childControlHeight = false;
        objectiveLayout.childForceExpandWidth = true;
        objectiveLayout.childForceExpandHeight = false;

        statsSection = CreateSection("TabletStatsSection", tabletHudPanel, 280f);
        VerticalLayoutGroup statsLayout = statsSection.gameObject.AddComponent<VerticalLayoutGroup>();
        statsLayout.spacing = 8f;
        statsLayout.childAlignment = TextAnchor.UpperCenter;
        statsLayout.childControlWidth = true;
        statsLayout.childControlHeight = false;
        statsLayout.childForceExpandWidth = true;
        statsLayout.childForceExpandHeight = false;

        levelMovesRow = CreateSection("TabletLevelMovesRow", statsSection, 140f);
        HorizontalLayoutGroup levelMovesLayout =
            levelMovesRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        levelMovesLayout.spacing = 8f;
        levelMovesLayout.childAlignment = TextAnchor.MiddleCenter;
        levelMovesLayout.childControlWidth = false;
        levelMovesLayout.childControlHeight = false;
        levelMovesLayout.childForceExpandWidth = false;
        levelMovesLayout.childForceExpandHeight = false;

        controlsRow = CreateSection("TabletControlsRow", tabletHudPanel, 130f);
        HorizontalLayoutGroup controlsLayout =
            controlsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        controlsLayout.spacing = tabletControlSpacing;
        controlsLayout.childAlignment = TextAnchor.MiddleCenter;
        controlsLayout.childControlWidth = false;
        controlsLayout.childControlHeight = false;
        controlsLayout.childForceExpandWidth = false;
        controlsLayout.childForceExpandHeight = false;

        flexSpacer = CreateSection("TabletFlexSpacer", tabletHudPanel, 20f);
        LayoutElement spacerLe = flexSpacer.GetComponent<LayoutElement>();
        spacerLe.minHeight = 8f;
        spacerLe.preferredHeight = 8f;
        spacerLe.flexibleHeight = 1f;
    }

    private void WireTabletScaffoldRefs(bool force = false)
    {
        if (tabletHudPanel == null)
        {
            return;
        }

        if (force || objectiveSection == null)
        {
            objectiveSection = tabletHudPanel.Find("TabletObjectiveSection") as RectTransform;
        }

        if (force || statsSection == null)
        {
            statsSection = tabletHudPanel.Find("TabletStatsSection") as RectTransform;
        }

        if (force || levelMovesRow == null)
        {
            if (statsSection == null)
            {
                statsSection = tabletHudPanel.Find("TabletStatsSection") as RectTransform;
            }

            levelMovesRow = statsSection != null
                ? statsSection.Find("TabletLevelMovesRow") as RectTransform
                : null;
        }

        if (force || controlsRow == null)
        {
            controlsRow = tabletHudPanel.Find("TabletControlsRow") as RectTransform;
        }

        if (force || flexSpacer == null)
        {
            flexSpacer = tabletHudPanel.Find("TabletFlexSpacer") as RectTransform;
        }
    }

    private static RectTransform CreateStretchPanel(string name, RectTransform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        return rt;
    }

    private static RectTransform CreateSection(
        string name,
        RectTransform parent,
        float preferredHeight)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0f, preferredHeight);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.minHeight = preferredHeight * 0.5f;
        le.preferredHeight = preferredHeight;
        le.flexibleWidth = 1f;
        return rt;
    }

    private void ApplyLayout(bool force)
    {
        EnsureSafeAreaApplied();

        GameplayLayoutKind wantKind =
            GameplayLayoutMode.Resolve(tallPhoneMaxAspect, wideAspectThreshold);

        if (!force &&
            wantKind == appliedKind &&
            Screen.width == lastWidth &&
            Screen.height == lastHeight &&
            !HasSafeAreaCanvasRectChanged())
        {
            return;
        }

        lastWidth = Screen.width;
        lastHeight = Screen.height;
        appliedKind = wantKind;
        CaptureSafeAreaCanvasRect();
        safeAreaReapplyPending = false;

        if (wantKind != GameplayLayoutKind.CompactPhonePortrait)
        {
            hasResolvedCompactMissionLabelHome = false;
            resolvedCompactMissionLabel = null;
        }

        if (wantKind != GameplayLayoutKind.TallPhonePortrait)
        {
            hasResolvedTallMissionLabelHome = false;
            resolvedTallMissionLabel = null;
        }

        if (wantKind != GameplayLayoutKind.TallPhonePortrait &&
            wantKind != GameplayLayoutKind.CompactPhonePortrait)
        {
            hasResolvedPhoneMissionLabelHome = false;
            resolvedPhoneMissionLabel = null;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[GameplayLayout] Apply\n" +
            "kind=" + wantKind + "\n" +
            "Screen=" + Screen.width + "x" + Screen.height + "\n" +
            "Aspect=" + GameplayLayoutMode.GetScreenAspect().ToString("0.000")
        );
#endif

        // Always restore immutable authored baseline first — prevents drift when
        // switching Tall ↔ Compact ↔ Tablet repeatedly in Game View.
        RestoreAuthoredPhoneBaseline();
        ClearPhoneObjectiveTextBaseScales();

        GameplayLayoutProfile profile = GetProfile(wantKind);
        if (ShouldApplyCapturedProfile(wantKind, profile))
        {
            ApplyCapturedProfile(wantKind, profile);
            ClearPhoneObjectiveTextBaseScales();
        }
        else
        {
            switch (wantKind)
            {
                case GameplayLayoutKind.WideTabletLandscape:
                    ApplyWideTabletPresentation();
                    break;
                case GameplayLayoutKind.CompactPhonePortrait:
                    ApplyCompactPhoneLayout();
                    break;
                default:
                    // TallPhonePortrait: immutable baseline owns Difficulty + other HUD.
                    // Special mission text is applied below via board-relative pass.
                    break;
            }
        }

        // Phone special mission text ONLY — never DifficultyLabel / EASY.
        if (wantKind == GameplayLayoutKind.TallPhonePortrait ||
            wantKind == GameplayLayoutKind.CompactPhonePortrait)
        {
            ApplyPhoneMissionLayoutRelativeToBoard();
        }

        // Final ownership sync — survives EnsureTabletScaffold / cleanup side effects.
        SyncTabletHudPanelActiveToKind(wantKind, "ApplyLayout.end");
        ApplyModalPanelsForKind(wantKind);
        ApplyHintStatusLayout();

        NotifyCameraFitter(wantKind, profile);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LogUILayoutAudit("ApplyLayout." + wantKind);
#endif
    }

    private void NotifyCameraFitter(GameplayLayoutKind kind, GameplayLayoutProfile profile)
    {
        if (cameraFitter == null)
        {
            return;
        }

        float leftHud = leftHudWidthFraction;
        float top = 0.16f;
        float bottom = 0.16f;

        if (profile != null)
        {
            leftHud = profile.leftHudWidthFraction;
            top = profile.topReservedFraction;
            bottom = profile.bottomReservedFraction;
        }
        else
        {
            switch (kind)
            {
                case GameplayLayoutKind.WideTabletLandscape:
                    leftHud = leftHudWidthFraction;
                    top = 0.04f;
                    bottom = 0.04f;
                    break;
                case GameplayLayoutKind.CompactPhonePortrait:
                    top = 0.18f;
                    bottom = 0.16f;
                    break;
            }
        }

        cameraFitter.ApplyModeCameraValues(kind, top, bottom, leftHud);
    }

    /// <summary>
    /// Restores authored tall-phone hierarchy/anchors/scales from the Awake snapshot.
    /// </summary>
    private void RestoreAuthoredPhoneBaseline()
    {
        // Structural first — cards must be under TopHUD before transform restore.
        RestoreCanonicalPhoneHierarchy(deactivateTabletPanel: true);

        CleanupDuplicateTabletHudPanels();
        // Phone baseline always leaves tablet scaffold inactive (mode apply re-enables Wide).
        SetTabletHudPanelActiveOwned(false, "RestoreAuthoredPhoneBaseline");

        ClearRuntimeLayoutElement(pauseButton);
        ClearRuntimeLayoutElement(undoButton);
        ClearRuntimeLayoutElement(hintButton);
        ClearRuntimeLayoutElement(rewardedHintButton);
        ClearRuntimeLayoutElement(levelCard);
        ClearRuntimeLayoutElement(movesCard);
        ClearRuntimeLayoutElement(coinCard);

        if (objectiveHudRoots != null)
        {
            for (int i = 0; i < objectiveHudRoots.Length; i++)
            {
                ClearRuntimeLayoutElement(objectiveHudRoots[i]);
                RestoreFromCache(objectiveHudRoots[i]);
            }
        }

        RestoreFromCache(levelCard);
        RestoreFromCache(movesCard);
        RestoreFromCache(coinCard);
        RestoreFromCache(topHud);
        RestoreFromCache(pauseButton);
        RestoreFromCache(undoButton);
        RestoreFromCache(hintButton);
        RestoreFromCache(rewardedHintButton);

        if (topHud != null)
        {
            topHud.gameObject.SetActive(true);
        }

        RestoreObjectiveChildren();

        if (topHudHorizontalLayout != null &&
            phoneSnapshots.TryGetValue(topHud, out RectSnapshot topSnap) &&
            topSnap.HasHorizontalLayout)
        {
            topHudHorizontalLayout.enabled = topSnap.HorizontalLayoutEnabled;
        }

        if (topHud != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(topHud);
        }
    }

    private void ApplyCompactPhoneLayout()
    {
        if (safeArea == null)
        {
            return;
        }

        float safeH = Mathf.Max(1f, safeArea.rect.height);
        float topPad = safeH * compactTopHudPaddingFrac;
        float topHudH = safeH * compactTopHudHeightFrac;

        // TopHUD: pin to top with safe-area-relative height (no center-based drift).
        if (topHud != null)
        {
            topHud.anchorMin = new Vector2(0.5f, 1f);
            topHud.anchorMax = new Vector2(0.5f, 1f);
            topHud.pivot = new Vector2(0.5f, 1f);
            topHud.anchoredPosition = new Vector2(0f, -topPad);
            topHud.sizeDelta = new Vector2(topHud.sizeDelta.x, topHudH);

            if (topHudHorizontalLayout != null)
            {
                topHudHorizontalLayout.spacing = -20f;
                topHudHorizontalLayout.childAlignment = TextAnchor.MiddleCenter;
            }

            ScaleCard(levelCard, compactCardScale);
            ScaleCard(movesCard, compactCardScale);
            ScaleCard(coinCard, compactCardScale);
        }

        // Difficulty only — special mission text is board-relative (ApplyPhoneMissionLayoutRelativeToBoard).
        float objectiveCursor = topPad + topHudH + safeH * compactObjectiveGapFrac;
        StackCompactDifficultyOnly(objectiveCursor, safeH);
        lastCompactObjectiveActiveMask = BuildActiveObjectiveMask();

        // Pause: top-right, closer to the top edge.
        if (pauseButton != null)
        {
            pauseButton.anchorMin = new Vector2(1f, 1f);
            pauseButton.anchorMax = new Vector2(1f, 1f);
            pauseButton.pivot = new Vector2(0.5f, 0.5f);
            pauseButton.anchoredPosition = new Vector2(-120f, -safeH * compactPauseTopFrac);
        }

        // Undo: top-right near Pause (avoid center+738 which collides / clips).
        if (undoButton != null)
        {
            undoButton.anchorMin = new Vector2(1f, 1f);
            undoButton.anchorMax = new Vector2(1f, 1f);
            undoButton.pivot = new Vector2(0.5f, 0.5f);
            undoButton.anchoredPosition =
                new Vector2(-280f, -safeH * compactPauseTopFrac);
            undoButton.localScale = Vector3.one * 1.85f;
        }

        // Hints stay bottom-anchored; Y from safe-area height.
        if (hintButton != null)
        {
            hintButton.anchorMin = new Vector2(0.5f, 0f);
            hintButton.anchorMax = new Vector2(0.5f, 0f);
            hintButton.pivot = new Vector2(0.5f, 0.5f);
            hintButton.anchoredPosition = new Vector2(0f, safeH * compactHintFrac);
            hintButton.localScale = Vector3.one * 1.45f;
        }

        if (rewardedHintButton != null)
        {
            rewardedHintButton.anchorMin = new Vector2(0.5f, 0f);
            rewardedHintButton.anchorMax = new Vector2(0.5f, 0f);
            rewardedHintButton.pivot = new Vector2(0.5f, 0.5f);
            rewardedHintButton.anchoredPosition =
                new Vector2(0f, safeH * compactRewardedHintFrac);
            rewardedHintButton.localScale = Vector3.one * 1.15f;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[GameplayLayout] CompactPhone applied\n" +
            "safeH=" + safeH.ToString("0") + "\n" +
            "topHudH=" + topHudH.ToString("0") + "\n" +
            "objectiveCursor=" + objectiveCursor.ToString("0") + "\n" +
            "activeMask=" + lastCompactObjectiveActiveMask
        );
#endif
    }

    /// <summary>
    /// Compact: place DifficultyLabel only. Active special mission text is owned by
    /// ApplyPhoneMissionLayoutRelativeToBoard (board-relative).
    /// </summary>
    private void StackCompactDifficultyOnly(float startFromTop, float safeH)
    {
        if (safeArea == null || objectiveHudRoots == null)
        {
            return;
        }

        float difficultyH = safeH * compactDifficultyBlockFrac;

        for (int i = 0; i < objectiveHudRoots.Length; i++)
        {
            RectTransform root = objectiveHudRoots[i];
            if (root == null || root == difficultyLabelRoot)
            {
                continue;
            }

            if (!root.gameObject.activeInHierarchy)
            {
                RestoreFromCache(root);
                RestoreObjectiveChildrenForRoot(root);
            }
        }

        if (difficultyLabelRoot != null && difficultyLabelRoot.gameObject.activeInHierarchy)
        {
            PlaceCompactObjectiveRoot(difficultyLabelRoot, startFromTop, difficultyH);
            CompactObjectiveChildrenForPhone(difficultyLabelRoot);
        }
    }

    /// <summary>
    /// Legacy Compact stack entry. Mission Y is owned by
    /// ApplyPhoneMissionLayoutRelativeToBoard — only Difficulty is placed here.
    /// </summary>
    private void StackCompactObjectiveBlock(float startFromTop, float safeH)
    {
        StackCompactDifficultyOnly(startFromTop, safeH);
    }

    private void PlaceCompactObjectiveRoot(RectTransform root, float fromTop, float height)
    {
        root.SetParent(safeArea, false);
        root.anchorMin = new Vector2(0.5f, 1f);
        root.anchorMax = new Vector2(0.5f, 1f);
        root.pivot = new Vector2(0.5f, 1f);
        root.anchoredPosition = new Vector2(0f, -fromTop);
        root.sizeDelta = new Vector2(520f, height);
        root.localScale = Vector3.one * compactObjectiveScale;
    }

    private int BuildActiveObjectiveMask()
    {
        int mask = 0;
        if (objectiveHudRoots == null)
        {
            return 0;
        }

        for (int i = 0; i < objectiveHudRoots.Length; i++)
        {
            RectTransform root = objectiveHudRoots[i];
            if (root != null && root.gameObject.activeInHierarchy)
            {
                mask |= 1 << i;
            }
        }

        return mask;
    }

    private void RestoreObjectiveChildrenForRoot(RectTransform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            RectTransform child = root.GetChild(i) as RectTransform;
            if (child == null ||
                !objectiveChildPhoneSnapshots.TryGetValue(child, out RectSnapshot snap))
            {
                continue;
            }

            child.anchorMin = snap.AnchorMin;
            child.anchorMax = snap.AnchorMax;
            child.pivot = snap.Pivot;
            child.anchoredPosition = snap.AnchoredPosition;
            child.sizeDelta = snap.SizeDelta;
            child.localScale = snap.LocalScale;
            child.localRotation = snap.LocalRotation;
            phoneObjectiveTextBaseScales.Remove(child);
        }
    }

    private void ClearPhoneObjectiveTextBaseScales()
    {
        phoneObjectiveTextBaseScales.Clear();
    }

    private static void ScaleCard(RectTransform card, float scale)
    {
        if (card == null)
        {
            return;
        }

        card.localScale = Vector3.one * scale;
    }

    private void CompactObjectiveChildrenForPhone(RectTransform root)
    {
        if (root == null)
        {
            return;
        }

        RectTransform missionLabel = null;
        List<RectTransform> secondary = new List<RectTransform>(root.childCount);
        for (int i = 0; i < root.childCount; i++)
        {
            RectTransform child = root.GetChild(i) as RectTransform;
            if (child == null)
            {
                continue;
            }

            if (IsMissionIntroLabel(child))
            {
                missionLabel = child;
                continue;
            }

            secondary.Add(child);
        }

        secondary.Sort((a, b) =>
        {
            float ay = objectiveChildPhoneSnapshots.TryGetValue(a, out RectSnapshot sa)
                ? sa.AnchoredPosition.y
                : a.anchoredPosition.y;
            float by = objectiveChildPhoneSnapshots.TryGetValue(b, out RectSnapshot sb)
                ? sb.AnchoredPosition.y
                : b.anchoredPosition.y;
            return by.CompareTo(ay);
        });

        // Reserve top of mission root for Mission Label; stack secondary lines below.
        float titleReserve = Mathf.Max(36f, root.sizeDelta.y * 0.42f);
        float line = Mathf.Max(30f, (root.sizeDelta.y - titleReserve) /
            Mathf.Max(1, secondary.Count + 1));

        if (missionLabel != null)
        {
            // Never fight the intro animation; home was already cached before intro.
            if (!SpecialMissionIntroController.IsIntroPlaying)
            {
                PlaceCompactMissionLabelHome(missionLabel, titleReserve);
            }
        }
        float y = -titleReserve;
        for (int i = 0; i < secondary.Count; i++)
        {
            RectTransform child = secondary[i];
            child.anchorMin = new Vector2(0.5f, 1f);
            child.anchorMax = new Vector2(0.5f, 1f);
            child.pivot = new Vector2(0.5f, 1f);
            child.anchoredPosition = new Vector2(0f, y);
            y -= line;
        }
    }

    /// <summary>
    /// Permanent Compact home for Mission Label inside its objective root.
    /// Cached so SpecialMissionIntroController can animate TO this exact state.
    /// </summary>
    private void PlaceCompactMissionLabelHome(RectTransform label, float titleReserve)
    {
        Vector3 scale = Vector3.one * 0.72f;
        if (objectiveChildPhoneSnapshots.TryGetValue(label, out RectSnapshot snap))
        {
            scale = snap.LocalScale;
            if (Mathf.Abs(scale.x) < 0.01f)
            {
                scale = Vector3.one * 0.72f;
            }
        }

        // Sit in the reserved title band above secondary lines (secondary starts at -titleReserve).
        float homeY = -Mathf.Max(4f, titleReserve * 0.08f);

        label.anchorMin = new Vector2(0.5f, 1f);
        label.anchorMax = new Vector2(0.5f, 1f);
        label.pivot = new Vector2(0.5f, 1f);
        label.anchoredPosition = new Vector2(0f, homeY);
        label.localScale = scale;
        label.SetAsFirstSibling();

        resolvedCompactMissionLabel = label;
        resolvedCompactMissionLabelHome = new RectTransformState
        {
            key = BuildChildKey(label),
            parentKey = label.parent != null ? label.parent.name : string.Empty,
            siblingIndex = label.GetSiblingIndex(),
            anchorMin = label.anchorMin,
            anchorMax = label.anchorMax,
            pivot = label.pivot,
            anchoredPosition = label.anchoredPosition,
            sizeDelta = label.sizeDelta,
            localScale = label.localScale,
            localEulerAngles = label.localEulerAngles
        };
        hasResolvedCompactMissionLabelHome = true;
    }

    private static bool IsMissionIntroLabel(RectTransform child)
    {
        if (child == null)
        {
            return false;
        }

        string n = child.name;
        return n == "Mission Label" || n == "MissionLabel";
    }

    private void ApplyPhoneLayout()
    {
        RestoreAuthoredPhoneBaseline();
    }

    /// <summary>
    /// Wide-only Pause/Win sizing. Phone modes restore authored scene presentation.
    /// Scales panel roots (no wrappers — show/hide does not animate root scale).
    /// </summary>
    private void ApplyModalPanelsForKind(GameplayLayoutKind kind)
    {
        ResolveRefs();
        CacheModalPanelsPhonePresentation();

        if (kind == GameplayLayoutKind.WideTabletLandscape)
        {
            ApplyWideModalPanel(pausePanel, pausePanelPhoneSnap, widePausePanelScale, widePausePanelOffset);
            ApplyWideModalPanel(winPanel, winPanelPhoneSnap, wideWinPanelScale, wideWinPanelOffset);
            return;
        }

        RestoreModalPanelFromPhoneSnap(pausePanel, pausePanelPhoneSnap);
        RestoreModalPanelFromPhoneSnap(winPanel, winPanelPhoneSnap);
    }

    private void CacheModalPanelsPhonePresentation()
    {
        if (modalPanelsPhoneCached)
        {
            return;
        }

        ResolveRefs();
        if (pausePanel != null)
        {
            pausePanelPhoneSnap = Capture(pausePanel);
        }

        if (winPanel != null)
        {
            winPanelPhoneSnap = Capture(winPanel);
        }

        modalPanelsPhoneCached = pausePanel != null || winPanel != null;
    }

    private static void ApplyWideModalPanel(
        RectTransform panel,
        RectSnapshot phoneSnap,
        float scaleMul,
        Vector2 offset)
    {
        if (panel == null)
        {
            return;
        }

        Vector3 phoneScale = phoneSnap.LocalScale.sqrMagnitude > 0.0001f
            ? phoneSnap.LocalScale
            : Vector3.one;

        if (phoneSnap.SizeDelta.sqrMagnitude > 0.0001f)
        {
            panel.sizeDelta = phoneSnap.SizeDelta;
        }

        // Centered overlay — ignore phone pixel nudge; apply inspector offset.
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = offset;
        float mul = Mathf.Clamp(scaleMul, 0.45f, 1f);
        panel.localScale = phoneScale * mul;
    }

    private static void RestoreModalPanelFromPhoneSnap(RectTransform panel, RectSnapshot snap)
    {
        if (panel == null)
        {
            return;
        }

        // Default struct = never cached for this panel.
        if (snap.LocalScale.sqrMagnitude < 0.0001f && snap.SizeDelta.sqrMagnitude < 0.0001f)
        {
            return;
        }

        panel.anchorMin = snap.AnchorMin;
        panel.anchorMax = snap.AnchorMax;
        panel.pivot = snap.Pivot;
        panel.anchoredPosition = snap.AnchoredPosition;
        panel.sizeDelta = snap.SizeDelta;
        panel.localScale = snap.LocalScale;
        panel.localRotation = snap.LocalRotation;
    }

    /// <summary>
    /// Single procedural Wide HUD path for Editor Preview and Play Mode.
    /// (Wide capture uses ApplyCapturedProfile instead.)
    /// </summary>
    private void ApplyWideTabletPresentation()
    {
        LogWideState("APPLY BEGIN");

        // Clean phone hierarchy so card refs resolve; tablet panel stays available.
        RestoreCanonicalPhoneHierarchy(deactivateTabletPanel: false);

        EnsureTabletScaffold();
        WireTabletScaffoldRefs(force: true);

        LogWideTabletActive("after ensure");

        if (!ValidateTabletScaffold(out string scaffoldError))
        {
            Debug.LogError(
                "[WideLayout] Scaffold invalid — aborting Wide HUD apply " +
                "(TopHUD left active).\n" + scaffoldError);
            if (topHud != null)
            {
                topHud.gameObject.SetActive(true);
            }

            LogWideState("APPLY ABORT");
            return;
        }

        // Re-resolve cards after canonical restore (must not be null).
        levelCard = ResolveHudCard("LevelCard", levelCard);
        movesCard = ResolveHudCard("MovesCard", movesCard);
        coinCard = ResolveHudCard("CoinCard", coinCard);

        float width = Mathf.Clamp(leftHudWidthFraction, 0.22f, 0.36f);
        if (wideTabletProfile != null)
        {
            width = Mathf.Clamp(wideTabletProfile.leftHudWidthFraction, 0.22f, 0.36f);
        }

        // Layout mode owns container active state — activate BEFORE reparent/hide TopHUD.
        SetTabletHudPanelActiveOwned(true, "ApplyWideTabletPresentation");
        tabletHudPanel.SetParent(safeArea, false);
        tabletHudPanel.anchorMin = new Vector2(0f, 0f);
        tabletHudPanel.anchorMax = new Vector2(width, 1f);
        tabletHudPanel.pivot = new Vector2(0.5f, 0.5f);
        tabletHudPanel.offsetMin = Vector2.zero;
        tabletHudPanel.offsetMax = Vector2.zero;
        tabletHudPanel.localScale = Vector3.one;
        tabletHudPanel.SetAsFirstSibling();

        if (topHudHorizontalLayout != null)
        {
            topHudHorizontalLayout.enabled = false;
        }

        ParentObjectivesToSection();

        if (levelCard != null)
        {
            PlaceCardInRow(levelCard, levelMovesRow);
        }

        if (movesCard != null)
        {
            PlaceCardInRow(movesCard, levelMovesRow);
        }

        if (coinCard != null)
        {
            PlaceCardInRow(coinCard, statsSection);
            coinCard.SetAsLastSibling();
        }

        // Only hide TopHUD after cards have actually left it.
        bool cardsOnTablet =
            (levelCard == null || levelCard.parent == levelMovesRow) &&
            (movesCard == null || movesCard.parent == levelMovesRow) &&
            (coinCard == null || coinCard.parent == statsSection);

        if (!cardsOnTablet)
        {
            Debug.LogError(
                "[WideLayout] Cards failed to reparent into tablet rows — " +
                "keeping TopHUD active.\n" +
                "Level parent=" + ParentName(levelCard) +
                " Moves parent=" + ParentName(movesCard) +
                " Coin parent=" + ParentName(coinCard) +
                " levelMovesRow=" + (levelMovesRow != null) +
                " statsSection=" + (statsSection != null));
        }
        else if (topHud != null)
        {
            topHud.gameObject.SetActive(false);
        }

        PlaceControlButton(pauseButton);
        PlaceControlButton(undoButton);

        PlaceHint(hintButton, 1.25f, 100f);
        PlaceHint(rewardedHintButton, 1.05f, 110f);

        objectiveSection.SetSiblingIndex(0);
        statsSection.SetSiblingIndex(1);
        controlsRow.SetSiblingIndex(2);
        if (flexSpacer != null)
        {
            flexSpacer.SetSiblingIndex(3);
        }

        if (hintButton != null)
        {
            hintButton.SetSiblingIndex(4);
        }

        if (rewardedHintButton != null)
        {
            rewardedHintButton.SetAsLastSibling();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(tabletHudPanel);

        // Re-assert ownership after all reparent/layout work.
        SetTabletHudPanelActiveOwned(true, "ApplyWideTabletPresentation.afterApply");
        LogWideTabletActive("after apply");

        if (tabletHudPanel != null && !tabletHudPanel.gameObject.activeInHierarchy)
        {
            Debug.LogError(
                "[WideLayout] TabletHUDPanel inactive in hierarchy after Wide apply " +
                "(parent inactive?). activeSelf=" + tabletHudPanel.gameObject.activeSelf);
        }

        LogWideState("APPLY END");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Application.isPlaying && isActiveAndEnabled)
        {
            StopCoroutine(nameof(LogWideStateAfterFrames));
            StartCoroutine(LogWideStateAfterFrames());
        }
#endif
    }

    /// <summary>Legacy name — routes to unified Wide presentation.</summary>
    private void ApplyTabletLayout()
    {
        ApplyWideTabletPresentation();
    }

    private bool ValidateTabletScaffold(out string error)
    {
        error = null;
        if (safeArea == null)
        {
            error = "safeArea is null";
            return false;
        }

        if (tabletHudPanel == null)
        {
            error = "tabletHudPanel is null";
            return false;
        }

        if (objectiveSection == null ||
            statsSection == null ||
            levelMovesRow == null ||
            controlsRow == null)
        {
            error =
                "section nulls: objective=" + (objectiveSection != null) +
                " stats=" + (statsSection != null) +
                " levelMoves=" + (levelMovesRow != null) +
                " controls=" + (controlsRow != null);
            return false;
        }

        // Sections must be under the active panel (not a destroyed duplicate).
        if (objectiveSection.parent != tabletHudPanel ||
            statsSection.parent != tabletHudPanel ||
            controlsRow.parent != tabletHudPanel ||
            levelMovesRow.parent != statsSection)
        {
            error =
                "section parent mismatch vs TabletHUDPanel — " +
                "stale refs after duplicate cleanup";
            return false;
        }

        return true;
    }

    private void LogWideState(string tag)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[WideLayout] " + tag + "\n" +
            "playing=" + Application.isPlaying + "\n" +
            "TabletHUDPanel active=" +
            (tabletHudPanel != null && tabletHudPanel.gameObject.activeSelf) + "\n" +
            "TabletHUDPanel activeInHierarchy=" +
            (tabletHudPanel != null && tabletHudPanel.gameObject.activeInHierarchy) + "\n" +
            "TopHUD active=" + (topHud != null && topHud.gameObject.activeSelf) + "\n" +
            "LevelCard parent=" + ParentName(levelCard) + "\n" +
            "MovesCard parent=" + ParentName(movesCard) + "\n" +
            "CoinCard parent=" + ParentName(coinCard) + "\n" +
            "Pause parent=" + ParentName(pauseButton) + "\n" +
            "Undo parent=" + ParentName(undoButton) + "\n" +
            "Hint parent=" + ParentName(hintButton) + "\n" +
            "Rewarded parent=" + ParentName(rewardedHintButton)
        );
#endif
    }

    private void LogWideTabletActive(string phase)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[WideLayout] TabletHUDPanel active " + phase + " = " +
            (tabletHudPanel != null && tabletHudPanel.gameObject.activeSelf));
#endif
    }

    /// <summary>
    /// Sole owner of TabletHUDPanel.activeSelf. Layout mode decides; nothing else may.
    /// </summary>
    private void SetTabletHudPanelActiveOwned(bool active, string method)
    {
        if (tabletHudPanel == null)
        {
            return;
        }

        if (tabletHudPanel.gameObject.activeSelf == active)
        {
            return;
        }

        tabletHudPanel.gameObject.SetActive(active);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!active)
        {
            Debug.Log(
                "[WideLayout] TabletHUDPanel DISABLED by " + method);
        }
        else
        {
            Debug.Log(
                "[WideLayout] TabletHUDPanel ENABLED by " + method);
        }
#endif
    }

    /// <summary>
    /// Wide → active; Tall / Compact → inactive.
    /// </summary>
    private void SyncTabletHudPanelActiveToKind(GameplayLayoutKind kind, string method)
    {
        bool wantActive = kind == GameplayLayoutKind.WideTabletLandscape;
        SetTabletHudPanelActiveOwned(wantActive, method);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private System.Collections.IEnumerator LogWideStateAfterFrames()
    {
        yield return null;
        LogWideTabletActive("FRAME+1");
        LogWideState("FRAME+1");
        // Re-assert if something disabled the panel between apply and next frame.
        if (appliedKind == GameplayLayoutKind.WideTabletLandscape)
        {
            SyncTabletHudPanelActiveToKind(
                GameplayLayoutKind.WideTabletLandscape,
                "LogWideStateAfterFrames.FRAME+1");
        }

        yield return null;
        LogWideTabletActive("FRAME+2");
        LogWideState("FRAME+2");
        if (appliedKind == GameplayLayoutKind.WideTabletLandscape)
        {
            SyncTabletHudPanelActiveToKind(
                GameplayLayoutKind.WideTabletLandscape,
                "LogWideStateAfterFrames.FRAME+2");
        }
    }
#endif

    private void ParentObjectivesToSection()
    {
        if (objectiveHudRoots == null || objectiveSection == null)
        {
            return;
        }

        for (int i = 0; i < objectiveHudRoots.Length; i++)
        {
            RectTransform root = objectiveHudRoots[i];
            if (root == null)
            {
                continue;
            }

            // Always keep under objective section so late SetActive still lands correctly.
            root.SetParent(objectiveSection, false);
            root.anchorMin = new Vector2(0.5f, 1f);
            root.anchorMax = new Vector2(0.5f, 1f);
            root.pivot = new Vector2(0.5f, 1f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = new Vector2(420f, 120f);
            root.localScale = Vector3.one * objectiveHudTabletScale;

            LayoutElement le = GetOrAddLayoutElement(root);
            le.minHeight = 70f;
            le.preferredHeight = 120f;
            le.flexibleWidth = 1f;
            // Inactive roots should not reserve huge space.
            le.ignoreLayout = !root.gameObject.activeInHierarchy;

            CompactObjectiveChildren(root);
        }
    }

    private void EnsureActiveObjectivesParented()
    {
        if (tabletHudPanel == null || !tabletHudPanel.gameObject.activeInHierarchy)
        {
            return;
        }

        WireTabletScaffoldRefs(force: false);
        if (objectiveSection == null ||
            (objectiveSection.parent != null && objectiveSection.parent != tabletHudPanel))
        {
            WireTabletScaffoldRefs(force: true);
        }

        if (objectiveHudRoots == null || objectiveSection == null)
        {
            return;
        }

        // Captured Wide layout: never run procedural ParentObjectivesToSection —
        // that would overwrite authored sizes/positions. Only reparent + ignoreLayout.
        if (HasUsableWideCapture())
        {
            EnsureActiveObjectivesParentedForCapturedWide();
            return;
        }

        bool dirty = false;
        for (int i = 0; i < objectiveHudRoots.Length; i++)
        {
            RectTransform root = objectiveHudRoots[i];
            if (root == null)
            {
                continue;
            }

            LayoutElement le = root.GetComponent<LayoutElement>();
            bool shouldIgnore = !root.gameObject.activeInHierarchy;
            if (le != null && le.ignoreLayout != shouldIgnore)
            {
                le.ignoreLayout = shouldIgnore;
                dirty = true;
            }

            if (root.parent != objectiveSection)
            {
                ParentObjectivesToSection();
                dirty = true;
                break;
            }
        }

        if (dirty && tabletHudPanel != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tabletHudPanel);
        }
    }

    /// <summary>
    /// Late objective activation under a captured Wide profile: reparent only,
    /// restore that root's captured presentation, never procedural tablet sizes.
    /// </summary>
    private void EnsureActiveObjectivesParentedForCapturedWide()
    {
        bool dirty = false;
        for (int i = 0; i < objectiveHudRoots.Length; i++)
        {
            RectTransform root = objectiveHudRoots[i];
            if (root == null)
            {
                continue;
            }

            LayoutElement le = root.GetComponent<LayoutElement>();
            if (le == null)
            {
                le = root.gameObject.AddComponent<LayoutElement>();
            }

            bool shouldIgnore = !root.gameObject.activeInHierarchy;
            if (le.ignoreLayout != shouldIgnore)
            {
                le.ignoreLayout = shouldIgnore;
                dirty = true;
            }

            if (root.parent != objectiveSection)
            {
                root.SetParent(objectiveSection, false);
                dirty = true;

                if (wideTabletProfile.TryGet(root.name, out RectTransformState state))
                {
                    RectTransform parent = ResolveParentKey(state.parentKey);
                    if (parent != null && root.parent != parent)
                    {
                        root.SetParent(parent, false);
                    }

                    state.ApplyTo(root);
                    if (parent != null)
                    {
                        int max = Mathf.Max(0, parent.childCount - 1);
                        root.SetSiblingIndex(Mathf.Clamp(state.siblingIndex, 0, max));
                    }

                    // Re-apply children presentation from capture.
                    for (int c = 0; c < root.childCount; c++)
                    {
                        RectTransform child = root.GetChild(c) as RectTransform;
                        if (child == null)
                        {
                            continue;
                        }

                        string childKey = root.name + "/" + child.name;
                        if (wideTabletProfile.TryGet(childKey, out RectTransformState childState))
                        {
                            childState.ApplyTo(child);
                        }
                    }
                }
            }
        }

        if (dirty && tabletHudPanel != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tabletHudPanel);
        }
    }

    private void PlaceCardInRow(RectTransform card, RectTransform row)
    {
        if (card == null || row == null)
        {
            return;
        }

        card.SetParent(row, false);
        card.anchorMin = new Vector2(0.5f, 0.5f);
        card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot = new Vector2(0.5f, 0.5f);
        card.anchoredPosition = Vector2.zero;
        card.localScale = Vector3.one * 1.15f;

        LayoutElement le = GetOrAddLayoutElement(card);
        le.minWidth = 120f;
        le.preferredWidth = 140f;
        le.minHeight = 120f;
        le.preferredHeight = 130f;
    }

    private void PlaceControlButton(RectTransform button)
    {
        if (button == null || controlsRow == null)
        {
            return;
        }

        button.SetParent(controlsRow, false);
        button.anchorMin = new Vector2(0.5f, 0.5f);
        button.anchorMax = new Vector2(0.5f, 0.5f);
        button.pivot = new Vector2(0.5f, 0.5f);
        button.anchoredPosition = Vector2.zero;
        button.sizeDelta = new Vector2(110f, 110f);

        if (button == undoButton)
        {
            button.localScale = Vector3.one * 1.55f;
        }
        else
        {
            button.localScale = Vector3.one;
        }

        LayoutElement le = GetOrAddLayoutElement(button);
        le.minWidth = 100f;
        le.minHeight = 100f;
        le.preferredWidth = 110f;
        le.preferredHeight = 110f;
    }

    private void PlaceHint(RectTransform button, float scale, float height)
    {
        if (button == null || tabletHudPanel == null)
        {
            return;
        }

        button.SetParent(tabletHudPanel, false);
        button.anchorMin = new Vector2(0.5f, 0.5f);
        button.anchorMax = new Vector2(0.5f, 0.5f);
        button.pivot = new Vector2(0.5f, 0.5f);
        button.anchoredPosition = Vector2.zero;
        button.localScale = Vector3.one * scale;

        LayoutElement le = GetOrAddLayoutElement(button);
        le.minHeight = height;
        le.preferredHeight = height + 10f;
        le.flexibleWidth = 1f;
    }

    private static string ParentName(RectTransform rt)
    {
        if (rt == null || rt.parent == null)
        {
            return "null";
        }

        return rt.parent.name;
    }

    private static void ClearRuntimeLayoutElement(RectTransform rt)
    {
        if (rt == null)
        {
            return;
        }

        LayoutElement le = rt.GetComponent<LayoutElement>();
        if (le == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(le);
            return;
        }
#endif
        Destroy(le);
    }

    private static LayoutElement GetOrAddLayoutElement(RectTransform rt)
    {
        LayoutElement le = rt.GetComponent<LayoutElement>();
        if (le == null)
        {
            le = rt.gameObject.AddComponent<LayoutElement>();
        }

        return le;
    }

    // -------------------------------------------------------------------------
    // Layout profile authoring / capture / apply
    // -------------------------------------------------------------------------

    public GameplayLayoutProfile GetProfile(GameplayLayoutKind kind)
    {
        switch (kind)
        {
            case GameplayLayoutKind.CompactPhonePortrait:
                return compactPhoneProfile;
            case GameplayLayoutKind.WideTabletLandscape:
                return wideTabletProfile;
            default:
                return tallPhoneProfile;
        }
    }

    /// <summary>
    /// Forces Compact / Wide objective layout before mission intro ownership.
    /// Captured modes re-apply profile; procedural Compact stacks; Wide parents only.
    /// </summary>
    public void EnsureCompactObjectiveLayoutUpToDate()
    {
        if (SpecialMissionIntroController.IsIntroPlaying)
        {
            return;
        }

        if (appliedKind == GameplayLayoutKind.WideTabletLandscape)
        {
            // Scaffold reuse must NOT deactivate the panel — layout mode owns active.
            EnsureTabletScaffold();
            WireTabletScaffoldRefs(force: true);
            SyncTabletHudPanelActiveToKind(
                GameplayLayoutKind.WideTabletLandscape,
                "EnsureCompactObjectiveLayoutUpToDate.Wide");
            EnsureActiveObjectivesParented();
            return;
        }

        if (appliedKind != GameplayLayoutKind.CompactPhonePortrait)
        {
            return;
        }

        int mask = BuildActiveObjectiveMask();
        bool needsHome = !hasResolvedCompactMissionLabelHome;
        if (mask == lastCompactObjectiveActiveMask && !needsHome)
        {
            return;
        }

        lastCompactObjectiveActiveMask = mask;

        if (HasUsableCompactCapture())
        {
            RestoreAuthoredPhoneBaseline();
            ApplyCapturedProfile(
                GameplayLayoutKind.CompactPhonePortrait,
                compactPhoneProfile);
            ClearPhoneObjectiveTextBaseScales();
            ApplyPhoneMissionLayoutRelativeToBoard();
            return;
        }

        float safeH = Mathf.Max(1f, safeArea != null ? safeArea.rect.height : 1f);
        float topPad = safeH * compactTopHudPaddingFrac;
        float topHudH = safeH * compactTopHudHeightFrac;
        StackCompactDifficultyOnly(topPad + topHudH + safeH * compactObjectiveGapFrac, safeH);
        ClearPhoneObjectiveTextBaseScales();
        ApplyPhoneMissionLayoutRelativeToBoard();
    }

    /// <summary>
    /// Permanent Mission Label home. Phone (Tall/Compact): board-relative layout.
    /// Wide: captured profile when present.
    /// </summary>
    public bool TryGetMissionLabelHomeState(
        RectTransform label,
        out RectTransformState state)
    {
        state = default;
        if (label == null)
        {
            return false;
        }

        string key = BuildChildKey(label);

        if (appliedKind == GameplayLayoutKind.WideTabletLandscape &&
            HasUsableWideCapture() &&
            wideTabletProfile.TryGet(key, out state))
        {
            return true;
        }

        if (appliedKind == GameplayLayoutKind.TallPhonePortrait ||
            appliedKind == GameplayLayoutKind.CompactPhonePortrait)
        {
            if (hasResolvedPhoneMissionLabelHome &&
                resolvedPhoneMissionLabel == label)
            {
                state = resolvedPhoneMissionLabelHome;
                return true;
            }

            ApplyPhoneMissionLayoutRelativeToBoard();
            if (hasResolvedPhoneMissionLabelHome &&
                resolvedPhoneMissionLabel == label)
            {
                state = resolvedPhoneMissionLabelHome;
                return true;
            }

            // Fall back to Tall/Compact caches if set by the same pass.
            if (hasResolvedTallMissionLabelHome && resolvedTallMissionLabel == label)
            {
                state = resolvedTallMissionLabelHome;
                return true;
            }

            if (hasResolvedCompactMissionLabelHome &&
                resolvedCompactMissionLabel == label)
            {
                state = resolvedCompactMissionLabelHome;
                return true;
            }

            state = RectTransformState.From(
                key,
                label.parent != null ? label.parent.name : string.Empty,
                label);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Legacy alias — prefer <see cref="TryGetMissionLabelHomeState"/>.
    /// </summary>
    public bool TryGetCapturedMissionLabelHome(
        GameplayLayoutKind kind,
        RectTransform label,
        out RectTransformState state)
    {
        if (kind == GameplayLayoutKind.CompactPhonePortrait ||
            kind == GameplayLayoutKind.WideTabletLandscape)
        {
            // Uses AppliedKind internally; temporarily rely on profile lookup by kind.
            if (kind == GameplayLayoutKind.WideTabletLandscape &&
                HasUsableWideCapture() &&
                label != null &&
                wideTabletProfile.TryGet(BuildChildKey(label), out state))
            {
                return true;
            }

            return TryGetMissionLabelHomeState(label, out state);
        }

        state = default;
        return false;
    }

    /// <summary>
    /// Editor: apply a mode in Edit Mode for visual authoring.
    /// Uses a temporary editor snapshot so Wide/Compact preview never permanently
    /// contaminates the phone scene hierarchy. Does not modify immutable Tall
    /// baseline or Compact/Wide serialized captures (except when Capture is pressed).
    /// </summary>
    public void EditorApplyPreview(GameplayLayoutKind kind)
    {
#if !UNITY_EDITOR
        return;
#else
        if (Application.isPlaying)
        {
            Debug.LogWarning("[GameplayLayoutPreview] Apply Preview is Edit Mode only.");
            return;
        }

        ResolveRefs();
        DiscardTallCapturedProfile();
        EnsureImmutablePhoneBaseline();
        CacheModalPanelsPhonePresentation();

        // Mode switch: always return to the pre-preview editor snapshot first.
        if (editorPreviewSnapshotValid)
        {
            RestoreEditorPreviewSnapshot();
            // TabletHUDPanel created mid-preview is not in the snapshot — force off
            // so Compact/Tall never keep a live tablet hierarchy.
            ForceDeactivateTabletScaffoldKeepRefs();
            Debug.Log("[GameplayLayoutPreview] Restoring snapshot before mode switch");
        }
        else
        {
            CaptureEditorPreviewSnapshot();
            Debug.Log("[GameplayLayoutPreview] Snapshot captured");
        }

        // Runtime restore caches stay tied to immutable baseline (Play Mode), not preview.
        phoneCached = false;
        phoneSnapshots.Clear();
        objectiveChildPhoneSnapshots.Clear();
        CachePhoneLayout();

        bool hadTabletBefore = tabletHudPanel != null ||
            (safeArea != null && safeArea.Find("TabletHUDPanel") != null);
        EnsureTabletScaffold();
        if (!hadTabletBefore && tabletHudPanel != null)
        {
            editorPreviewCreatedTabletScaffold = true;
        }

        AuthoringPreviewKind = kind;
        IsAuthoringPreviewActive = true;
        appliedKind = kind;

        // Apply from the restored editor snapshot (already restored above), then mode.
        // Do NOT call RestoreAuthoredPhoneBaseline here — that can differ from the
        // editor snapshot and would mutate what Clear Preview must restore.

        GameplayLayoutProfile profile = GetProfile(kind);
        if (ShouldApplyCapturedProfile(kind, profile))
        {
            ApplyCapturedProfile(kind, profile);
        }
        else
        {
            switch (kind)
            {
                case GameplayLayoutKind.WideTabletLandscape:
                    ApplyWideTabletPresentation();
                    break;
                case GameplayLayoutKind.CompactPhonePortrait:
                    ApplyCompactPhoneLayout();
                    break;
                default:
                    // Tall preview = restored editor snapshot (already applied).
                    break;
            }
        }

        NotifyCameraFitter(kind, profile);
        lastCompactObjectiveActiveMask = BuildActiveObjectiveMask();
        SyncTabletHudPanelActiveToKind(kind, "EditorApplyPreview.end");
        ApplyModalPanelsForKind(kind);

        Debug.Log("[GameplayLayoutPreview] Applying " + kind);
#endif
    }

    /// <summary>
    /// Editor: capture Compact or Wide into their mode profile.
    /// Tall capture is refused — immutable baseline owns Tall.
    /// Never writes into the immutable phone baseline or the editor preview snapshot.
    /// </summary>
    public void EditorCaptureCurrentLayout(GameplayLayoutKind kind)
    {
        ResolveRefs();
        EnsureTabletScaffold();

        if (kind == GameplayLayoutKind.TallPhonePortrait)
        {
            Debug.LogWarning(
                "[GameplayLayoutAuthoring] Tall capture DISABLED — " +
                "Tall uses the immutable baseline. Use Rebake Immutable Phone Baseline " +
                "only when intentionally updating Tall.");
            return;
        }

        if (kind == GameplayLayoutKind.CompactPhonePortrait && !allowCompactCapturedProfile)
        {
            Debug.LogWarning(
                "[GameplayLayoutAuthoring] Compact capture disabled " +
                "(allowCompactCapturedProfile=false).");
            return;
        }

        if (kind == GameplayLayoutKind.WideTabletLandscape && !allowWideCapturedProfile)
        {
            Debug.LogWarning(
                "[GameplayLayoutAuthoring] Wide capture disabled " +
                "(allowWideCapturedProfile=false).");
            return;
        }

        GameplayLayoutProfile profile = GetProfile(kind);
        if (profile == null)
        {
            return;
        }

        // Preserve manually tuned camera reserves when re-capturing.
        if (!profile.hasCapture)
        {
            switch (kind)
            {
                case GameplayLayoutKind.WideTabletLandscape:
                    profile.topReservedFraction = 0.04f;
                    profile.bottomReservedFraction = 0.04f;
                    profile.leftHudWidthFraction = leftHudWidthFraction;
                    break;
                case GameplayLayoutKind.CompactPhonePortrait:
                    profile.topReservedFraction = 0.18f;
                    profile.bottomReservedFraction = 0.16f;
                    break;
            }
        }

        profile.entries = new List<RectTransformState>(48);
        bool includeTablet = kind == GameplayLayoutKind.WideTabletLandscape;
        CaptureKnownTransforms(profile, includeTablet);
        profile.hasCapture = true;
        AuthoringPreviewKind = kind;

        Debug.Log("[GameplayLayoutAuthoring] Captured " + kind +
                  " (" + profile.entries.Count + " entries) — immutable baseline untouched");
    }

    /// <summary>
    /// Editor: restore the temporary pre-preview scene snapshot and end preview.
    /// Does not modify immutable Tall baseline or Compact/Wide profiles.
    /// </summary>
    public void EditorClearPreview()
    {
#if !UNITY_EDITOR
        return;
#else
        if (Application.isPlaying)
        {
            return;
        }

        ResolveRefs();

        if (editorPreviewSnapshotValid)
        {
            RestoreEditorPreviewSnapshot();
            Debug.Log("[GameplayLayoutPreview] Restoring snapshot");
        }
        else
        {
            // No snapshot (e.g. leftover contamination): fall back to immutable baseline.
            EnsureImmutablePhoneBaseline();
            phoneCached = false;
            phoneSnapshots.Clear();
            objectiveChildPhoneSnapshots.Clear();
            CachePhoneLayout();
            RestoreAuthoredPhoneBaseline();
            DestroyOrDeactivatePreviewTabletScaffold(forceDestroyCreated: true);
            Debug.Log(
                "[GameplayLayoutPreview] No snapshot — restored immutable phone baseline");
        }

        // Structural guarantee even if snapshot was incomplete / domain-reloaded.
        RestoreCanonicalPhoneHierarchy();
        CleanupDuplicateTabletHudPanels();
        CleanupPreviewTabletScaffoldAfterRestore();
        ClearEditorPreviewSnapshot();

        IsAuthoringPreviewActive = false;
        AuthoringPreviewKind = GameplayLayoutKind.TallPhonePortrait;
        appliedKind = GameplayLayoutKind.TallPhonePortrait;
        NotifyCameraFitter(GameplayLayoutKind.TallPhonePortrait, tallPhoneProfile);
        ApplyModalPanelsForKind(GameplayLayoutKind.TallPhonePortrait);

        Debug.Log("[GameplayLayoutPreview] Preview cleared");
#endif
    }

    /// <summary>
    /// Editor: re-apply the captured profile for the current authoring kind.
    /// </summary>
    public void EditorRestoreCapturedLayout(GameplayLayoutKind kind)
    {
        if (kind == GameplayLayoutKind.TallPhonePortrait)
        {
            Debug.LogWarning(
                "[GameplayLayoutAuthoring] Tall has no captured profile — " +
                "use Clear Preview / immutable baseline.");
            return;
        }

        if (kind == GameplayLayoutKind.CompactPhonePortrait && !allowCompactCapturedProfile)
        {
            Debug.LogWarning(
                "[GameplayLayoutAuthoring] Compact captured profiles disabled.");
            return;
        }

        if (kind == GameplayLayoutKind.WideTabletLandscape && !allowWideCapturedProfile)
        {
            Debug.LogWarning(
                "[GameplayLayoutAuthoring] Wide captured profiles disabled.");
            return;
        }

        GameplayLayoutProfile profile = GetProfile(kind);
        if (profile == null || !profile.hasCapture)
        {
            Debug.LogWarning("[GameplayLayoutAuthoring] No captured profile for " + kind);
            return;
        }

        EditorApplyPreview(kind);
    }

    public bool AllowCompactCapturedProfile => allowCompactCapturedProfile;
    public bool AllowWideCapturedProfile => allowWideCapturedProfile;

    /// <summary>True when Compact has a usable captured profile for runtime.</summary>
    public bool HasCompactCapture => HasUsableCompactCapture();

    /// <summary>True when Wide Tablet has a usable captured profile for runtime.</summary>
    public bool HasWideCapture => HasUsableWideCapture();

#if UNITY_EDITOR
    private struct EditorLiveSnap
    {
        public RectTransform Rt;
        public Transform Parent;
        public int SiblingIndex;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 Pivot;
        public Vector2 AnchoredPosition;
        public Vector2 SizeDelta;
        public Vector3 LocalScale;
        public Vector3 LocalEuler;
        public bool ActiveSelf;
        public bool HasHorizontalLayout;
        public bool HorizontalLayoutEnabled;
        public TextAnchor ChildAlignment;
        public float Spacing;
        public bool ChildForceExpandWidth;
        public bool ChildForceExpandHeight;
        public bool ChildControlWidth;
        public bool ChildControlHeight;
        public bool HadLayoutElement;
    }

    // Temporary Edit Mode only — never serialized into profiles / Tall baseline.
    private readonly List<EditorLiveSnap> editorPreviewLiveSnapshot =
        new List<EditorLiveSnap>(64);
    private bool editorPreviewSnapshotValid;
    private bool editorPreviewCreatedTabletScaffold;

    private void CaptureEditorPreviewSnapshot()
    {
        editorPreviewLiveSnapshot.Clear();
        editorPreviewCreatedTabletScaffold = false;

        List<RectTransform> targets = CollectEditorPreviewTargets();
        for (int i = 0; i < targets.Count; i++)
        {
            RectTransform rt = targets[i];
            if (rt == null)
            {
                continue;
            }

            EditorLiveSnap snap = default;
            snap.Rt = rt;
            snap.Parent = rt.parent;
            snap.SiblingIndex = rt.GetSiblingIndex();
            snap.AnchorMin = rt.anchorMin;
            snap.AnchorMax = rt.anchorMax;
            snap.Pivot = rt.pivot;
            snap.AnchoredPosition = rt.anchoredPosition;
            snap.SizeDelta = rt.sizeDelta;
            snap.LocalScale = rt.localScale;
            snap.LocalEuler = rt.localEulerAngles;
            snap.ActiveSelf = rt.gameObject.activeSelf;
            snap.HadLayoutElement = rt.GetComponent<LayoutElement>() != null;

            HorizontalLayoutGroup hlg = rt.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                snap.HasHorizontalLayout = true;
                snap.HorizontalLayoutEnabled = hlg.enabled;
                snap.ChildAlignment = hlg.childAlignment;
                snap.Spacing = hlg.spacing;
                snap.ChildForceExpandWidth = hlg.childForceExpandWidth;
                snap.ChildForceExpandHeight = hlg.childForceExpandHeight;
                snap.ChildControlWidth = hlg.childControlWidth;
                snap.ChildControlHeight = hlg.childControlHeight;
            }

            editorPreviewLiveSnapshot.Add(snap);
        }

        editorPreviewSnapshotValid = editorPreviewLiveSnapshot.Count > 0;
    }

    private List<RectTransform> CollectEditorPreviewTargets()
    {
        List<RectTransform> list = new List<RectTransform>(48);
        void Add(RectTransform rt)
        {
            if (rt != null && !list.Contains(rt))
            {
                list.Add(rt);
            }
        }

        Add(topHud);
        Add(pauseButton);
        Add(undoButton);
        Add(hintButton);
        Add(rewardedHintButton);
        Add(difficultyLabelRoot);
        Add(levelCard);
        Add(movesCard);
        Add(coinCard);
        Add(pausePanel);
        Add(winPanel);

        if (objectiveHudRoots != null)
        {
            for (int i = 0; i < objectiveHudRoots.Length; i++)
            {
                RectTransform root = objectiveHudRoots[i];
                Add(root);
                if (root == null)
                {
                    continue;
                }

                for (int c = 0; c < root.childCount; c++)
                {
                    Add(root.GetChild(c) as RectTransform);
                }
            }
        }

        // Include existing tablet scaffold only if already in scene (pre-preview).
        if (safeArea != null)
        {
            Transform existing = safeArea.Find("TabletHUDPanel");
            if (existing != null)
            {
                Add(existing as RectTransform);
            }
        }

        return list;
    }

    private void RestoreEditorPreviewSnapshot()
    {
        if (!editorPreviewSnapshotValid)
        {
            return;
        }

        // Parents first, then transforms (two passes).
        for (int pass = 0; pass < 2; pass++)
        {
            for (int i = 0; i < editorPreviewLiveSnapshot.Count; i++)
            {
                EditorLiveSnap snap = editorPreviewLiveSnapshot[i];
                RectTransform rt = snap.Rt;
                if (rt == null)
                {
                    continue;
                }

                if (pass == 0)
                {
                    if (snap.Parent != null && rt.parent != snap.Parent)
                    {
                        rt.SetParent(snap.Parent, false);
                    }

                    continue;
                }

                if (snap.Parent != null)
                {
                    int max = Mathf.Max(0, snap.Parent.childCount - 1);
                    rt.SetSiblingIndex(Mathf.Clamp(snap.SiblingIndex, 0, max));
                }

                rt.anchorMin = snap.AnchorMin;
                rt.anchorMax = snap.AnchorMax;
                rt.pivot = snap.Pivot;
                rt.anchoredPosition = snap.AnchoredPosition;
                rt.sizeDelta = snap.SizeDelta;
                rt.localScale = snap.LocalScale;
                rt.localEulerAngles = snap.LocalEuler;

                if (rt.gameObject.activeSelf != snap.ActiveSelf)
                {
                    rt.gameObject.SetActive(snap.ActiveSelf);
                }

                if (snap.HasHorizontalLayout)
                {
                    HorizontalLayoutGroup hlg = rt.GetComponent<HorizontalLayoutGroup>();
                    if (hlg != null)
                    {
                        hlg.enabled = snap.HorizontalLayoutEnabled;
                        hlg.childAlignment = snap.ChildAlignment;
                        hlg.spacing = snap.Spacing;
                        hlg.childForceExpandWidth = snap.ChildForceExpandWidth;
                        hlg.childForceExpandHeight = snap.ChildForceExpandHeight;
                        hlg.childControlWidth = snap.ChildControlWidth;
                        hlg.childControlHeight = snap.ChildControlHeight;
                    }
                }

                LayoutElement le = rt.GetComponent<LayoutElement>();
                if (le != null && !snap.HadLayoutElement)
                {
                    DestroyImmediate(le);
                }
            }
        }
    }

    private void ForceDeactivateTabletScaffoldKeepRefs()
    {
        if (safeArea != null)
        {
            Transform existing = safeArea.Find("TabletHUDPanel");
            if (existing != null)
            {
                tabletHudPanel = existing as RectTransform;
                WireTabletScaffoldRefs(force: true);
            }
        }

        SetTabletHudPanelActiveOwned(false, "ForceDeactivateTabletScaffoldKeepRefs");
        RestorePhoneParentsToSafeArea();
    }

    private void CleanupPreviewTabletScaffoldAfterRestore()
    {
        DestroyOrDeactivatePreviewTabletScaffold(forceDestroyCreated: true);
    }

    private void DestroyOrDeactivatePreviewTabletScaffold(bool forceDestroyCreated)
    {
        if (safeArea == null)
        {
            return;
        }

        // Ensure phone HUD is not under any tablet panel.
        RestorePhoneParentsToSafeArea();

        Transform existing = safeArea.Find("TabletHUDPanel");
        if (existing == null)
        {
            tabletHudPanel = null;
            objectiveSection = null;
            statsSection = null;
            levelMovesRow = null;
            controlsRow = null;
            flexSpacer = null;
            editorPreviewCreatedTabletScaffold = false;
            return;
        }

        if (forceDestroyCreated && editorPreviewCreatedTabletScaffold)
        {
            DestroyImmediate(existing.gameObject);
            tabletHudPanel = null;
            objectiveSection = null;
            statsSection = null;
            levelMovesRow = null;
            controlsRow = null;
            flexSpacer = null;
            editorPreviewCreatedTabletScaffold = false;
            return;
        }

        // Panel existed before preview — keep but force inactive.
        tabletHudPanel = existing as RectTransform;
        WireTabletScaffoldRefs(force: true);
        SetTabletHudPanelActiveOwned(false, "DestroyOrDeactivatePreviewTabletScaffold");
    }

    private void ClearEditorPreviewSnapshot()
    {
        editorPreviewLiveSnapshot.Clear();
        editorPreviewSnapshotValid = false;
        editorPreviewCreatedTabletScaffold = false;
    }

    /// <summary>
    /// Called from editor when Game View aspect no longer matches active preview.
    /// </summary>
    public void EditorAutoClearPreviewForResolutionChange()
    {
        if (!IsAuthoringPreviewActive || Application.isPlaying)
        {
            return;
        }

        Debug.Log(
            "[GameplayLayoutPreview] Resolution changed — preview cleared " +
            "(was " + AuthoringPreviewKind + ", resolved now " +
            GameplayLayoutMode.Resolve(tallPhoneMaxAspect, wideAspectThreshold) + ")");
        EditorClearPreview();
    }
#endif

    /// <summary>
    /// Editor utility: force re-bake immutable baseline from the CURRENT live hierarchy.
    /// Only use when the scene is known to be the clean tall phone layout.
    /// </summary>
    public void EditorRebakeImmutablePhoneBaseline()
    {
        ResolveRefs();
#if UNITY_EDITOR
        if (IsAuthoringPreviewActive)
        {
            EditorClearPreview();
        }
#endif
        RestorePhoneParentsToSafeArea();

        if (immutablePhoneBaseline == null)
        {
            immutablePhoneBaseline = new GameplayLayoutProfile();
        }

        immutablePhoneBaseline.entries = new List<RectTransformState>(48);
        CapturePhoneBaselineTransforms(immutablePhoneBaseline);
        immutablePhoneBaseline.hasCapture = true;
        immutablePhoneBaseline.topReservedFraction = 0.16f;
        immutablePhoneBaseline.bottomReservedFraction = 0.16f;

        phoneCached = false;
        phoneSnapshots.Clear();
        objectiveChildPhoneSnapshots.Clear();
        CachePhoneLayout();
        RestoreAuthoredPhoneBaseline();

        Debug.Log(
            "[GameplayLayoutAuthoring] Rebaked immutable phone baseline (" +
            immutablePhoneBaseline.entries.Count + " entries)");
    }

    /// <summary>
    /// Deterministic phone hierarchy. Wide may reparent cards temporarily; any return
    /// to phone mode must put Level/Moves/Coins back under TopHUD.
    /// </summary>
    public void RestoreCanonicalPhoneHierarchy(bool deactivateTabletPanel = true)
    {
        ResolveRefs();
        if (safeArea == null)
        {
            return;
        }

        if (topHud == null)
        {
            topHud = FindChildRect(safeArea, "TopHUD");
        }

        levelCard = ResolveHudCard("LevelCard", levelCard);
        movesCard = ResolveHudCard("MovesCard", movesCard);
        coinCard = ResolveHudCard("CoinCard", coinCard);

        ReparentIfNeeded(topHud, safeArea);
        ReparentIfNeeded(pauseButton, safeArea);
        ReparentIfNeeded(undoButton, safeArea);
        ReparentIfNeeded(hintButton, safeArea);
        ReparentIfNeeded(rewardedHintButton, safeArea);
        ReparentIfNeeded(difficultyLabelRoot, safeArea);

        if (objectiveHudRoots != null)
        {
            for (int i = 0; i < objectiveHudRoots.Length; i++)
            {
                ReparentIfNeeded(objectiveHudRoots[i], safeArea);
            }
        }

        if (topHud != null)
        {
            // Explicit phone card parenting — never leave cards under tablet rows.
            ReparentIfNeeded(levelCard, topHud);
            ReparentIfNeeded(movesCard, topHud);
            ReparentIfNeeded(coinCard, topHud);

            if (levelCard != null)
            {
                levelCard.SetSiblingIndex(0);
            }

            if (movesCard != null)
            {
                movesCard.SetSiblingIndex(1);
            }

            if (coinCard != null)
            {
                coinCard.SetSiblingIndex(2);
            }

            if (topHudHorizontalLayout == null)
            {
                topHudHorizontalLayout = topHud.GetComponent<HorizontalLayoutGroup>();
            }

            if (topHudHorizontalLayout != null)
            {
                // Prefer immutable / phone snapshot HLG settings when available.
                if (phoneSnapshots.TryGetValue(topHud, out RectSnapshot topSnap) &&
                    topSnap.HasHorizontalLayout)
                {
                    topHudHorizontalLayout.enabled = topSnap.HorizontalLayoutEnabled;
                    topHudHorizontalLayout.childAlignment = topSnap.ChildAlignment;
                    topHudHorizontalLayout.spacing = topSnap.Spacing;
                    topHudHorizontalLayout.childForceExpandWidth = topSnap.ChildForceExpandWidth;
                    topHudHorizontalLayout.childForceExpandHeight = topSnap.ChildForceExpandHeight;
                    topHudHorizontalLayout.childControlWidth = topSnap.ChildControlWidth;
                    topHudHorizontalLayout.childControlHeight = topSnap.ChildControlHeight;
                }
                else
                {
                    topHudHorizontalLayout.enabled = true;
                }
            }

            topHud.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(topHud);
        }

        if (deactivateTabletPanel)
        {
            // Guard: never kill TabletHUDPanel while Wide mode is the applied kind.
            if (appliedKind == GameplayLayoutKind.WideTabletLandscape)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log(
                    "[WideLayout] RestoreCanonicalPhoneHierarchy skipped tablet " +
                    "deactivate (appliedKind=Wide)");
#endif
            }
            else
            {
                SetTabletHudPanelActiveOwned(false, "RestoreCanonicalPhoneHierarchy");
            }
        }
    }

    private RectTransform ResolveHudCard(string cardName, RectTransform current)
    {
        if (current != null && current.name == cardName)
        {
            // Keep if still a valid gameplay card (not WinPanel duplicate names).
            if (!IsUnderNamedAncestor(current, "WinPanel") &&
                !IsUnderNamedAncestor(current, "PausePanel"))
            {
                return current;
            }
        }

        if (topHud != null)
        {
            RectTransform underTop = FindChildRect(topHud, cardName);
            if (underTop != null)
            {
                return underTop;
            }
        }

        if (safeArea != null)
        {
            RectTransform found = FindDeepHudCard(safeArea, cardName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static RectTransform FindDeepHudCard(Transform root, string cardName)
    {
        if (root == null)
        {
            return null;
        }

        RectTransform[] all = root.GetComponentsInChildren<RectTransform>(true);
        RectTransform best = null;
        for (int i = 0; i < all.Length; i++)
        {
            RectTransform rt = all[i];
            if (rt == null || rt.name != cardName)
            {
                continue;
            }

            if (IsUnderNamedAncestor(rt, "WinPanel") ||
                IsUnderNamedAncestor(rt, "PausePanel"))
            {
                continue;
            }

            // Prefer tablet-orphaned cards, then any under SafeArea.
            if (IsUnderNamedAncestor(rt, "TabletHUDPanel") ||
                IsUnderNamedAncestor(rt, "TabletLevelMovesRow") ||
                IsUnderNamedAncestor(rt, "TabletStatsSection"))
            {
                return rt;
            }

            if (best == null)
            {
                best = rt;
            }
        }

        return best;
    }

    private static bool IsUnderNamedAncestor(Transform t, string ancestorName)
    {
        Transform p = t != null ? t.parent : null;
        while (p != null)
        {
            if (p.name == ancestorName)
            {
                return true;
            }

            p = p.parent;
        }

        return false;
    }

    /// <summary>
    /// Guarantees phone HUD elements are under SafeArea / TopHUD (not tablet).
    /// </summary>
    private void RestorePhoneParentsToSafeArea()
    {
        RestoreCanonicalPhoneHierarchy();
    }

    /// <summary>
    /// Keep at most one TabletHUDPanel under SafeArea. Duplicates are preview leaks.
    /// </summary>
    private void CleanupDuplicateTabletHudPanels()
    {
        if (safeArea == null)
        {
            return;
        }

        List<Transform> panels = new List<Transform>(4);
        for (int i = 0; i < safeArea.childCount; i++)
        {
            Transform child = safeArea.GetChild(i);
            if (child != null && child.name == "TabletHUDPanel")
            {
                panels.Add(child);
            }
        }

        if (panels.Count == 0)
        {
            tabletHudPanel = null;
            objectiveSection = null;
            statsSection = null;
            levelMovesRow = null;
            controlsRow = null;
            flexSpacer = null;
            return;
        }

        // Prefer the one we already reference; else keep first.
        Transform keep = null;
        if (tabletHudPanel != null)
        {
            for (int i = 0; i < panels.Count; i++)
            {
                if (panels[i] == tabletHudPanel)
                {
                    keep = panels[i];
                    break;
                }
            }
        }

        if (keep == null)
        {
            keep = panels[0];
        }

        for (int i = 0; i < panels.Count; i++)
        {
            Transform panel = panels[i];
            if (panel == keep)
            {
                continue;
            }

            // Rescue any phone cards before destroying duplicate scaffold.
            RescuePhoneCardsFromTransform(panel);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(panel.gameObject);
                continue;
            }
#endif
            Destroy(panel.gameObject);
        }

        tabletHudPanel = keep as RectTransform;
        if (tabletHudPanel != null)
        {
            // Do NOT touch active here — layout mode owns TabletHUDPanel.activeSelf.
            // (Previously SetActive(false) undid Wide after EnsureTabletScaffold.)
            objectiveSection = null;
            statsSection = null;
            levelMovesRow = null;
            controlsRow = null;
            flexSpacer = null;
            WireTabletScaffoldRefs(force: true);
        }
    }

    private void RescuePhoneCardsFromTransform(Transform root)
    {
        if (root == null)
        {
            return;
        }

        RectTransform[] all = root.GetComponentsInChildren<RectTransform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            RectTransform rt = all[i];
            if (rt == null)
            {
                continue;
            }

            string n = rt.name;
            if (n == "LevelCard" || n == "MovesCard" || n == "CoinCard" ||
                n == "PauseButton" || n == "UndoButton" || n == "HintButton" ||
                n == "RewardedHintButton" || n == "DifficultyLabel" ||
                n == "TopHUD" ||
                (objectiveHudRoots != null && System.Array.Exists(
                    objectiveHudRoots, o => o != null && o == rt)))
            {
                // Will be reparented by RestoreCanonicalPhoneHierarchy.
                if (topHud != null &&
                    (n == "LevelCard" || n == "MovesCard" || n == "CoinCard"))
                {
                    rt.SetParent(topHud, false);
                }
                else if (safeArea != null)
                {
                    rt.SetParent(safeArea, false);
                }
            }
        }
    }

    private static void ReparentIfNeeded(RectTransform rt, RectTransform parent)
    {
        if (rt == null || parent == null || rt.parent == parent)
        {
            return;
        }

        rt.SetParent(parent, false);
    }

    private void ApplyCapturedProfile(GameplayLayoutKind kind, GameplayLayoutProfile profile)
    {
        if (profile == null || profile.entries == null)
        {
            return;
        }

        // Scaffold-only visibility (layout containers). Never touch objective HUD /
        // Mission Label / RuleText / Timer — those stay under mission UI scripts.
        if (kind == GameplayLayoutKind.WideTabletLandscape)
        {
            EnsureTabletScaffold();
            WireTabletScaffoldRefs(force: true);
            SyncTabletHudPanelActiveToKind(kind, "ApplyCapturedProfile.Wide");
            if (tabletHudPanel != null)
            {
                tabletHudPanel.SetAsFirstSibling();
            }

            if (topHud != null)
            {
                // Tablet profiles usually hide TopHUD and reparent cards.
                topHud.gameObject.SetActive(false);
            }
        }
        else
        {
            SyncTabletHudPanelActiveToKind(kind, "ApplyCapturedProfile.Phone");
            if (topHud != null)
            {
                topHud.gameObject.SetActive(true);
            }
        }

        // Two passes: parents first (no slash in key), then children.
        for (int pass = 0; pass < 2; pass++)
        {
            for (int i = 0; i < profile.entries.Count; i++)
            {
                RectTransformState state = profile.entries[i];
                bool isChild = state.key != null && state.key.IndexOf('/') >= 0;
                if (pass == 0 && isChild)
                {
                    continue;
                }

                if (pass == 1 && !isChild)
                {
                    continue;
                }

                RectTransform rt = ResolveKey(state.key);
                if (rt == null)
                {
                    continue;
                }

                RectTransform parent = ResolveParentKey(state.parentKey);
                if (parent != null && rt.parent != parent)
                {
                    rt.SetParent(parent, false);
                }

                state.ApplyTo(rt);

                if (parent != null)
                {
                    int max = Mathf.Max(0, parent.childCount - 1);
                    rt.SetSiblingIndex(Mathf.Clamp(state.siblingIndex, 0, max));
                }

                // Cache Mission Label home for SpecialMissionIntro (Compact / Wide).
                if (state.key != null &&
                    state.key.IndexOf("Mission Label", System.StringComparison.Ordinal) >= 0 &&
                    (kind == GameplayLayoutKind.CompactPhonePortrait ||
                     kind == GameplayLayoutKind.WideTabletLandscape))
                {
                    resolvedCompactMissionLabel = rt;
                    resolvedCompactMissionLabelHome = state;
                    hasResolvedCompactMissionLabelHome = true;
                }
            }
        }

        // Re-assert container visibility after ResolveParentKey / EnsureTabletScaffold.
        SyncTabletHudPanelActiveToKind(kind, "ApplyCapturedProfile.end");
        lastCompactObjectiveActiveMask = BuildActiveObjectiveMask();
    }

    private void CaptureKnownTransforms(GameplayLayoutProfile profile, bool includeTablet = true)
    {
        CaptureOne(profile, "TopHUD", topHud);
        CaptureOne(profile, "PauseButton", pauseButton);
        CaptureOne(profile, "UndoButton", undoButton);
        CaptureOne(profile, "HintButton", hintButton);
        CaptureOne(profile, "RewardedHintButton", rewardedHintButton);
        CaptureOne(profile, "DifficultyLabel", difficultyLabelRoot);
        CaptureOne(profile, "LevelCard", levelCard);
        CaptureOne(profile, "MovesCard", movesCard);
        CaptureOne(profile, "CoinCard", coinCard);

        if (includeTablet)
        {
            if (tabletHudPanel != null)
            {
                CaptureOne(profile, "TabletHUDPanel", tabletHudPanel);
            }

            if (objectiveSection != null)
            {
                CaptureOne(profile, "TabletObjectiveSection", objectiveSection);
            }

            if (statsSection != null)
            {
                CaptureOne(profile, "TabletStatsSection", statsSection);
            }

            if (levelMovesRow != null)
            {
                CaptureOne(profile, "TabletLevelMovesRow", levelMovesRow);
            }

            if (controlsRow != null)
            {
                CaptureOne(profile, "TabletControlsRow", controlsRow);
            }

            if (flexSpacer != null)
            {
                CaptureOne(profile, "TabletFlexSpacer", flexSpacer);
            }
        }

        if (objectiveHudRoots != null)
        {
            for (int i = 0; i < objectiveHudRoots.Length; i++)
            {
                RectTransform root = objectiveHudRoots[i];
                if (root == null)
                {
                    continue;
                }

                CaptureOne(profile, root.name, root);

                for (int c = 0; c < root.childCount; c++)
                {
                    RectTransform child = root.GetChild(c) as RectTransform;
                    if (child == null)
                    {
                        continue;
                    }

                    CaptureOne(profile, root.name + "/" + child.name, child);
                }
            }
        }
    }

    private void CaptureOne(GameplayLayoutProfile profile, string key, RectTransform rt)
    {
        if (profile == null || rt == null || string.IsNullOrEmpty(key))
        {
            return;
        }

        string parentKey = GetParentKey(rt);
        profile.Set(RectTransformState.From(key, parentKey, rt));
    }

    private string GetParentKey(RectTransform rt)
    {
        if (rt == null || rt.parent == null)
        {
            return string.Empty;
        }

        Transform p = rt.parent;
        if (safeArea != null && p == safeArea)
        {
            return "SafeArea";
        }

        if (topHud != null && p == topHud)
        {
            return "TopHUD";
        }

        if (tabletHudPanel != null && p == tabletHudPanel)
        {
            return "TabletHUDPanel";
        }

        if (objectiveSection != null && p == objectiveSection)
        {
            return "TabletObjectiveSection";
        }

        if (statsSection != null && p == statsSection)
        {
            return "TabletStatsSection";
        }

        if (levelMovesRow != null && p == levelMovesRow)
        {
            return "TabletLevelMovesRow";
        }

        if (controlsRow != null && p == controlsRow)
        {
            return "TabletControlsRow";
        }

        // Objective root child
        if (objectiveHudRoots != null)
        {
            for (int i = 0; i < objectiveHudRoots.Length; i++)
            {
                if (objectiveHudRoots[i] != null && p == objectiveHudRoots[i])
                {
                    return objectiveHudRoots[i].name;
                }
            }
        }

        return p.name;
    }

    private RectTransform ResolveParentKey(string parentKey)
    {
        if (string.IsNullOrEmpty(parentKey))
        {
            return safeArea;
        }

        switch (parentKey)
        {
            case "SafeArea":
                return safeArea;
            case "TopHUD":
                return topHud;
            case "TabletHUDPanel":
                EnsureTabletScaffold();
                return tabletHudPanel;
            case "TabletObjectiveSection":
                EnsureTabletScaffold();
                return objectiveSection;
            case "TabletStatsSection":
                EnsureTabletScaffold();
                return statsSection;
            case "TabletLevelMovesRow":
                EnsureTabletScaffold();
                return levelMovesRow;
            case "TabletControlsRow":
                EnsureTabletScaffold();
                return controlsRow;
            default:
                return ResolveKey(parentKey) ?? safeArea;
        }
    }

    private RectTransform ResolveKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        int slash = key.IndexOf('/');
        if (slash > 0)
        {
            string rootName = key.Substring(0, slash);
            string childName = key.Substring(slash + 1);
            RectTransform root = ResolveKey(rootName);
            if (root == null)
            {
                return null;
            }

            Transform child = root.Find(childName);
            return child as RectTransform;
        }

        switch (key)
        {
            case "TopHUD":
                return topHud;
            case "PauseButton":
                return pauseButton;
            case "UndoButton":
                return undoButton;
            case "HintButton":
                return hintButton;
            case "RewardedHintButton":
                return rewardedHintButton;
            case "DifficultyLabel":
                return difficultyLabelRoot;
            case "LevelCard":
                return levelCard;
            case "MovesCard":
                return movesCard;
            case "CoinCard":
                return coinCard;
            case "TabletHUDPanel":
                EnsureTabletScaffold();
                return tabletHudPanel;
            case "TabletObjectiveSection":
                EnsureTabletScaffold();
                return objectiveSection;
            case "TabletStatsSection":
                EnsureTabletScaffold();
                return statsSection;
            case "TabletLevelMovesRow":
                EnsureTabletScaffold();
                return levelMovesRow;
            case "TabletControlsRow":
                EnsureTabletScaffold();
                return controlsRow;
        }

        if (objectiveHudRoots != null)
        {
            for (int i = 0; i < objectiveHudRoots.Length; i++)
            {
                if (objectiveHudRoots[i] != null && objectiveHudRoots[i].name == key)
                {
                    return objectiveHudRoots[i];
                }
            }
        }

        if (safeArea != null)
        {
            Transform t = safeArea.Find(key);
            if (t != null)
            {
                return t as RectTransform;
            }
        }

        return null;
    }

    private static string BuildChildKey(RectTransform label)
    {
        if (label == null)
        {
            return string.Empty;
        }

        if (label.parent != null)
        {
            return label.parent.name + "/" + label.name;
        }

        return label.name;
    }

    private void EnsureSafeAreaApplied()
    {
        if (safeAreaComponent == null && safeArea != null)
        {
            safeAreaComponent = safeArea.GetComponent<SafeArea>();
        }

        if (safeAreaComponent == null)
        {
            safeAreaComponent = FindAnyObjectByType<SafeArea>();
        }

        if (safeAreaComponent != null)
        {
            safeAreaComponent.ForceApply();
            if (safeArea == null)
            {
                safeArea = safeAreaComponent.transform as RectTransform;
            }
        }

        CaptureSafeAreaCanvasRect();
    }

    private void CaptureSafeAreaCanvasRect()
    {
        if (safeArea == null)
        {
            hasLastSafeAreaCanvasRect = false;
            return;
        }

        lastSafeAreaCanvasRect = safeArea.rect;
        hasLastSafeAreaCanvasRect = true;
    }

    private bool HasSafeAreaCanvasRectChanged()
    {
        if (safeArea == null || !hasLastSafeAreaCanvasRect)
        {
            return false;
        }

        Rect current = safeArea.rect;
        return !Mathf.Approximately(current.xMin, lastSafeAreaCanvasRect.xMin) ||
               !Mathf.Approximately(current.yMin, lastSafeAreaCanvasRect.yMin) ||
               !Mathf.Approximately(current.width, lastSafeAreaCanvasRect.width) ||
               !Mathf.Approximately(current.height, lastSafeAreaCanvasRect.height);
    }

    private static bool IsScreenSafeAreaFullyCovering()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
        {
            return true;
        }

        Rect safe = Screen.safeArea;
        const float tol = 1.5f;
        return safe.xMin <= tol &&
               safe.yMin <= tol &&
               safe.xMax >= Screen.width - tol &&
               safe.yMax >= Screen.height - tol;
    }

    /// <summary>
    /// Layout-owned position for HintStatusText: same parent/anchors as HintButton,
    /// fixed gap above the button, sibling order after the button so it draws on top.
    /// HintManager must only set text/visibility.
    /// </summary>
    public void EnsureHintStatusLayout()
    {
        ApplyHintStatusLayout();
    }

    private void ApplyHintStatusLayout()
    {
        if (hintStatusText == null || hintButton == null)
        {
            return;
        }

        Transform hintParent = hintButton.parent;
        if (hintParent != null && hintStatusText.parent != hintParent)
        {
            hintStatusText.SetParent(hintParent, false);
        }

        hintStatusText.anchorMin = hintButton.anchorMin;
        hintStatusText.anchorMax = hintButton.anchorMax;
        hintStatusText.pivot = new Vector2(0.5f, 0f);

        float btnScaleY = Mathf.Abs(hintButton.localScale.y);
        float buttonTopY = hintButton.anchoredPosition.y +
            (1f - hintButton.pivot.y) * hintButton.rect.height * btnScaleY;

        hintStatusText.anchoredPosition = new Vector2(
            hintButton.anchoredPosition.x,
            buttonTopY + hintStatusGap);

        if (hintStatusText.parent == hintButton.parent && hintButton.parent != null)
        {
            int hintIdx = hintButton.GetSiblingIndex();
            hintStatusText.SetSiblingIndex(
                Mathf.Min(hintIdx + 1, hintStatusText.parent.childCount - 1));
        }
    }

    /// <summary>
    /// Phone (Tall + Compact) only: place ACTIVE special Mission Label + secondary
    /// relative to actual board top. NEVER touches DifficultyLabel / EASY.
    /// </summary>
    private void ApplyPhoneMissionLayoutRelativeToBoard()
    {
        if (safeArea == null ||
            objectiveHudRoots == null ||
            (appliedKind != GameplayLayoutKind.TallPhonePortrait &&
             appliedKind != GameplayLayoutKind.CompactPhonePortrait))
        {
            return;
        }

        ParkInactiveTallMissionRoots();

        RectTransform activeRoot = FindActiveTallMissionRoot();
        float difficultyY = MeasureRectPivotFromSaTop(difficultyLabelRoot, safeArea.rect.height);

        if (activeRoot == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogPhoneMissionLayout(
                null,
                0f,
                0f,
                -1f,
                -1f,
                phoneObjectiveBoardGapPx,
                difficultyY);
#endif
            return;
        }

        if (!TryMeasureBoardTopInSafeArea(
                out float boardLocalY,
                out float boardTopScreenY))
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(
                "[PhoneMissionLayout] Board top unavailable — special mission text not placed.");
#endif
            return;
        }

        Canvas canvas = safeArea.GetComponentInParent<Canvas>();
        float scaleFactor = canvas != null ? Mathf.Max(0.001f, canvas.scaleFactor) : 1f;
        float boardGapLocal = phoneObjectiveBoardGapPx / scaleFactor;
        float internalGapLocal = phoneObjectiveInternalGapPx / scaleFactor;

        RectTransform missionLabel;
        RectTransform secondary;
        ResolvePhoneMissionTexts(activeRoot, out missionLabel, out secondary);

        // Inspector multipliers on top of restored/captured base scale (default 1 = unchanged).
        Vector3 missionScale =
            GetPhoneObjectiveTextBaseScale(missionLabel, 0.72f) * phoneMissionLabelScale;
        Vector3 secondaryScale =
            GetPhoneObjectiveTextBaseScale(secondary, 1f) * phoneSecondaryTextScale;
        Vector2 missionSize = GetPhoneChildSize(missionLabel, new Vector2(350f, 50f));
        Vector2 secondarySize = GetPhoneChildSize(secondary, new Vector2(200f, 50f));

        float missionH = Mathf.Max(20f, Mathf.Abs(missionSize.y) * Mathf.Abs(missionScale.y));
        float secondaryH = secondary != null
            ? Mathf.Max(16f, Mathf.Abs(secondarySize.y) * Mathf.Abs(secondaryScale.y))
            : 0f;

        // SafeArea local Y increases upward. Stack upward from board top.
        float secondaryBottomLocal = boardLocalY + boardGapLocal;
        float secondaryTopLocal = secondaryBottomLocal + secondaryH;
        float missionBottomLocal = secondary != null
            ? secondaryTopLocal + internalGapLocal
            : secondaryBottomLocal;
        float missionTopLocal = missionBottomLocal + missionH;

        // Parent root under SafeArea; children stacked with top anchors inside root.
        if (activeRoot.parent != safeArea)
        {
            activeRoot.SetParent(safeArea, false);
        }

        float contentHeight = missionH +
            (secondary != null ? internalGapLocal + secondaryH : 0f);
        float rootTopLocal = missionTopLocal;
        float fromTop = safeArea.rect.yMax - rootTopLocal;

        activeRoot.anchorMin = new Vector2(0.5f, 1f);
        activeRoot.anchorMax = new Vector2(0.5f, 1f);
        activeRoot.pivot = new Vector2(0.5f, 1f);
        activeRoot.anchoredPosition = new Vector2(0f, -fromTop);
        activeRoot.sizeDelta = new Vector2(
            Mathf.Max(activeRoot.sizeDelta.x, 520f),
            Mathf.Max(contentHeight, 40f));
        activeRoot.localScale = Vector3.one;

        float y = 0f;
        if (missionLabel != null)
        {
            if (!SpecialMissionIntroController.IsIntroPlaying)
            {
                missionLabel.anchorMin = new Vector2(0.5f, 1f);
                missionLabel.anchorMax = new Vector2(0.5f, 1f);
                missionLabel.pivot = new Vector2(0.5f, 1f);
                missionLabel.anchoredPosition = new Vector2(0f, y);
                missionLabel.localScale = missionScale;
                missionLabel.SetAsFirstSibling();
            }

            CachePhoneMissionLabelHome(missionLabel, activeRoot, missionScale, y);
            y -= missionH + (secondary != null ? internalGapLocal : 0f);
        }

        if (secondary != null)
        {
            secondary.anchorMin = new Vector2(0.5f, 1f);
            secondary.anchorMax = new Vector2(0.5f, 1f);
            secondary.pivot = new Vector2(0.5f, 1f);
            secondary.anchoredPosition = new Vector2(0f, y);
            secondary.localScale = secondaryScale;
        }

        float missionY = missionTopLocal;
        float secondaryY = secondary != null ? secondaryTopLocal : -1f;

        // Diagnostic only — never move DifficultyLabel.
        if (difficultyLabelRoot != null &&
            difficultyLabelRoot.gameObject.activeInHierarchy)
        {
            float difficultyBottom = MeasureRectBottomFromSaTop(
                difficultyLabelRoot,
                safeArea.rect.height);
            float missionTopFromSaTop = safeArea.rect.yMax - missionTopLocal;
            if (missionTopFromSaTop < difficultyBottom + (8f / scaleFactor))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning(
                    "[PhoneMissionLayout] Mission text is close to DifficultyLabel/EASY — " +
                    "Difficulty was NOT moved. missionTopFromTop=" +
                    missionTopFromSaTop.ToString("0.0") +
                    " difficultyBottom=" + difficultyBottom.ToString("0.0"));
#endif
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LogPhoneMissionLayout(
            activeRoot,
            boardTopScreenY,
            boardLocalY,
            missionY,
            secondaryY,
            phoneObjectiveBoardGapPx,
            difficultyY);
#endif
    }

    /// <summary>
    /// Phone-only: configured secondary text localScale (timer / moves / rule).
    /// Used by mission UIs so pulse resets do not snap back to authored scale.
    /// </summary>
    public bool TryGetPhoneSecondaryLocalScale(
        RectTransform secondary,
        out Vector3 localScale)
    {
        localScale = Vector3.one;
        if (secondary == null ||
            (appliedKind != GameplayLayoutKind.TallPhonePortrait &&
             appliedKind != GameplayLayoutKind.CompactPhonePortrait))
        {
            return false;
        }

        localScale =
            GetPhoneObjectiveTextBaseScale(secondary, 1f) * phoneSecondaryTextScale;
        return true;
    }

    /// <summary>
    /// Re-apply phone board-relative mission/secondary after intro releases Mission Label.
    /// Does not touch DifficultyLabel.
    /// </summary>
    public void ReapplyPhoneSpecialObjectiveLayout()
    {
        if (appliedKind != GameplayLayoutKind.TallPhonePortrait &&
            appliedKind != GameplayLayoutKind.CompactPhonePortrait)
        {
            return;
        }

        ApplyPhoneMissionLayoutRelativeToBoard();
    }

    private void CachePhoneMissionLabelHome(
        RectTransform missionLabel,
        RectTransform root,
        Vector3 scale,
        float localY)
    {
        if (missionLabel == null)
        {
            return;
        }

        RectTransformState home = new RectTransformState
        {
            key = BuildChildKey(missionLabel),
            parentKey = root != null ? root.name : string.Empty,
            siblingIndex = missionLabel.GetSiblingIndex(),
            anchorMin = new Vector2(0.5f, 1f),
            anchorMax = new Vector2(0.5f, 1f),
            pivot = new Vector2(0.5f, 1f),
            anchoredPosition = new Vector2(0f, localY),
            sizeDelta = missionLabel.sizeDelta,
            localScale = scale,
            localEulerAngles = missionLabel.localEulerAngles
        };

        resolvedPhoneMissionLabel = missionLabel;
        resolvedPhoneMissionLabelHome = home;
        hasResolvedPhoneMissionLabelHome = true;

        resolvedTallMissionLabel = missionLabel;
        resolvedTallMissionLabelHome = home;
        hasResolvedTallMissionLabelHome = true;

        resolvedCompactMissionLabel = missionLabel;
        resolvedCompactMissionLabelHome = home;
        hasResolvedCompactMissionLabelHome = true;
    }

    private void ResolvePhoneMissionTexts(
        RectTransform root,
        out RectTransform missionLabel,
        out RectTransform secondary)
    {
        missionLabel = null;
        secondary = null;
        if (root == null)
        {
            return;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            RectTransform child = root.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeSelf)
            {
                continue;
            }

            if (IsMissionIntroLabel(child))
            {
                missionLabel = child;
                continue;
            }

            if (secondary == null && IsTallSecondaryObjectiveChild(child))
            {
                secondary = child;
            }
        }
    }

    /// <summary>
    /// Base localScale for phone mission/secondary text, captured once after
    /// restore/capture so Inspector multipliers do not compound on re-layout.
    /// </summary>
    private Vector3 GetPhoneObjectiveTextBaseScale(RectTransform child, float fallback)
    {
        if (child == null)
        {
            return Vector3.one * fallback;
        }

        if (phoneObjectiveTextBaseScales.TryGetValue(child, out Vector3 cached) &&
            cached.sqrMagnitude > 0.0001f)
        {
            return cached;
        }

        Vector3 baseScale = GetPhoneChildScale(child, fallback);
        phoneObjectiveTextBaseScales[child] = baseScale;
        return baseScale;
    }

    private Vector3 GetPhoneChildScale(RectTransform child, float fallback)
    {
        if (child == null)
        {
            return Vector3.one * fallback;
        }

        // Prefer live scale (Compact capture / prior layout) so board-relative
        // placement does not shrink Compact mission text back to Tall baseline.
        if (child.localScale.sqrMagnitude > 0.0001f)
        {
            return child.localScale;
        }

        if (objectiveChildPhoneSnapshots.TryGetValue(child, out RectSnapshot snap) &&
            Mathf.Abs(snap.LocalScale.x) > 0.01f)
        {
            return snap.LocalScale;
        }

        return Vector3.one * fallback;
    }

    private Vector2 GetPhoneChildSize(RectTransform child, Vector2 fallback)
    {
        if (child == null)
        {
            return fallback;
        }

        if (child.sizeDelta.sqrMagnitude > 0.01f)
        {
            return child.sizeDelta;
        }

        if (objectiveChildPhoneSnapshots.TryGetValue(child, out RectSnapshot snap) &&
            snap.SizeDelta.sqrMagnitude > 0.01f)
        {
            return snap.SizeDelta;
        }

        return fallback;
    }

    private bool TryMeasureBoardTopInSafeArea(
        out float boardLocalY,
        out float boardTopScreenY)
    {
        boardLocalY = 0f;
        boardTopScreenY = 0f;

        Camera cam = Camera.main;
        if (cam == null)
        {
            return false;
        }

        float worldBoardTopY;
        if (cameraFitter == null ||
            !cameraFitter.TryGetBoardWorldTop(out worldBoardTopY))
        {
            float topReserved = appliedKind == GameplayLayoutKind.CompactPhonePortrait
                ? 0.18f
                : 0.16f;
            if (appliedKind == GameplayLayoutKind.CompactPhonePortrait &&
                compactPhoneProfile != null)
            {
                topReserved = compactPhoneProfile.topReservedFraction;
            }
            else if (tallPhoneProfile != null)
            {
                topReserved = tallPhoneProfile.topReservedFraction;
            }

            worldBoardTopY = cam.transform.position.y +
                cam.orthographicSize * (1f - 2f * topReserved);
        }

        Vector3 screen = cam.WorldToScreenPoint(
            new Vector3(cam.transform.position.x, worldBoardTopY, 0f));
        boardTopScreenY = screen.y;

        Canvas canvas = safeArea.GetComponentInParent<Canvas>();
        Camera eventCam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCam = canvas.worldCamera;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                safeArea,
                new Vector2(screen.x, screen.y),
                eventCam,
                out Vector2 local))
        {
            return false;
        }

        boardLocalY = local.y;
        return true;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void LogPhoneMissionLayout(
        RectTransform activeRoot,
        float boardTopScreenY,
        float boardTopLocalY,
        float missionY,
        float secondaryY,
        float gapToBoardPx,
        float difficultyY)
    {
        Debug.Log(
            "[PhoneMissionLayout]\n" +
            "kind=" + appliedKind + "\n" +
            "activeRoot=" + (activeRoot != null ? activeRoot.name : "none") + "\n" +
            "boardTopScreenY=" + boardTopScreenY.ToString("0.0") + "\n" +
            "boardTopLocalY=" + boardTopLocalY.ToString("0.0") + "\n" +
            "missionY=" + missionY.ToString("0.0") + "\n" +
            "secondaryY=" + secondaryY.ToString("0.0") + "\n" +
            "gapToBoardPx=" + gapToBoardPx.ToString("0.0") + "\n" +
            "difficultyY=" + difficultyY.ToString("0.0")
        );
    }
#endif

    private void ParkInactiveTallMissionRoots()
    {
        if (objectiveHudRoots == null)
        {
            return;
        }

        for (int i = 0; i < objectiveHudRoots.Length; i++)
        {
            RectTransform root = objectiveHudRoots[i];
            if (root == null || root == difficultyLabelRoot)
            {
                continue;
            }

            if (root.gameObject.activeInHierarchy)
            {
                continue;
            }

            RestoreFromCache(root);
            RestoreObjectiveChildrenForRoot(root);
        }
    }

    private RectTransform FindActiveTallMissionRoot()
    {
        if (objectiveHudRoots == null)
        {
            return null;
        }

        for (int i = 0; i < objectiveHudRoots.Length; i++)
        {
            RectTransform root = objectiveHudRoots[i];
            if (root == null || root == difficultyLabelRoot)
            {
                continue;
            }

            if (root.gameObject.activeInHierarchy)
            {
                return root;
            }
        }

        return null;
    }

    private static bool IsTallSecondaryObjectiveChild(RectTransform child)
    {
        if (child == null)
        {
            return false;
        }

        if (IsMissionIntroLabel(child))
        {
            return false;
        }

        string n = child.name;
        if (n.IndexOf("SecondaryVehicle", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return false;
        }

        return n.IndexOf("Timer", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               n.IndexOf("MovesRemaining", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               n.IndexOf("Targets", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               n.IndexOf("Rule", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               n.IndexOf("Remaining", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               n.Equals("MovesText", System.StringComparison.OrdinalIgnoreCase) ||
               n.Equals("SecondaryText", System.StringComparison.OrdinalIgnoreCase) ||
               n.IndexOf("Secondary", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               n.IndexOf("Count", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private float MeasureTopHudBottomFromTop(float safeH)
    {
        if (topHud == null)
        {
            return safeH * 0.12f;
        }

        // Top-anchored: anchoredPosition.y is typically negative.
        float pivotFromTop = -topHud.anchoredPosition.y;
        float h = topHud.rect.height * Mathf.Abs(topHud.localScale.y);
        float pivotY = topHud.pivot.y;
        // Distance from pivot to bottom edge along +down in from-top space.
        float pivotToBottom = h * pivotY;
        return Mathf.Max(0f, pivotFromTop + pivotToBottom);
    }

    /// <summary>
    /// Distance from SafeArea top edge down to the rect's pivot (diagnostic / proximity).
    /// </summary>
    private float MeasureRectPivotFromSaTop(RectTransform rt, float safeH)
    {
        if (rt == null || safeArea == null)
        {
            return 0f;
        }

        Vector3 world = rt.TransformPoint(rt.rect.center);
        Vector3 local = safeArea.InverseTransformPoint(world);
        return safeArea.rect.yMax - local.y;
    }

    /// <summary>
    /// Distance from SafeArea top edge down to the rect's bottom edge.
    /// </summary>
    private float MeasureRectBottomFromSaTop(RectTransform rt, float safeH)
    {
        if (rt == null || safeArea == null)
        {
            return 0f;
        }

        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        float minLocalY = float.PositiveInfinity;
        for (int i = 0; i < 4; i++)
        {
            float y = safeArea.InverseTransformPoint(corners[i]).y;
            if (y < minLocalY)
            {
                minLocalY = y;
            }
        }

        return safeArea.rect.yMax - minLocalY;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private System.Collections.IEnumerator LogUILayoutAuditAfterFrames()
    {
        yield return null;
        LogUILayoutAudit("Frame+1");
        yield return null;
        LogUILayoutAudit("Frame+2");
    }

    /// <summary>Legacy alias for Timed-only probes.</summary>
    public void LogTimedLayoutDiagnostics(string stage)
    {
        LogUILayoutAudit(stage);
    }

    /// <summary>Dev-only Editor vs device layout probe.</summary>
    public void LogUILayoutAudit(string stage)
    {
        Canvas canvas = safeArea != null ? safeArea.GetComponentInParent<Canvas>() : null;
        RectTransform canvasRt = canvas != null ? canvas.transform as RectTransform : null;

        System.Text.StringBuilder sb = new System.Text.StringBuilder(2048);
        sb.AppendLine("[UILayoutAudit]");
        sb.AppendLine("Stage=" + stage);
        sb.AppendLine("Screen=" + Screen.width + "x" + Screen.height);
        sb.AppendLine(
            "SafeArea=" + Screen.safeArea.x + "," + Screen.safeArea.y + "," +
            Screen.safeArea.width + "," + Screen.safeArea.height);
        sb.AppendLine("CanvasRect=" + (canvasRt != null ? canvasRt.rect.ToString() : "null"));
        sb.AppendLine(
            "CanvasScaleFactor=" +
            (canvas != null ? canvas.scaleFactor.ToString("0.###") : "n/a"));
        sb.AppendLine("LayoutKind=" + appliedKind);
        sb.AppendLine(
            "SafeAreaRect=" + (safeArea != null ? safeArea.rect.ToString() : "null"));
        sb.AppendLine("FullSafeCovering=" + IsScreenSafeAreaFullyCovering());
        sb.AppendLine(
            "BoardTopScreenYApprox=" +
            (Screen.height * (1f - (tallPhoneProfile != null
                ? tallPhoneProfile.topReservedFraction
                : 0.16f))).ToString("0"));

        if (objectiveHudRoots != null)
        {
            for (int i = 0; i < objectiveHudRoots.Length; i++)
            {
                RectTransform root = objectiveHudRoots[i];
                if (root == null || !root.gameObject.activeInHierarchy)
                {
                    continue;
                }

                AppendHudRootAudit(sb, root);
            }
        }

        AppendRectAudit(sb, "HintButton", hintButton);
        AppendRectAudit(sb, "HintStatusText", hintStatusText);
        if (hintStatusText != null)
        {
            sb.AppendLine(
                "HintStatusText siblingIndex=" + hintStatusText.GetSiblingIndex());
            sb.AppendLine(
                "HintStatusText parent=" +
                (hintStatusText.parent != null ? hintStatusText.parent.name : "null"));
        }

        Debug.Log(sb.ToString());
    }

    private static void AppendHudRootAudit(
        System.Text.StringBuilder sb,
        RectTransform root)
    {
        sb.AppendLine("--- " + root.name + " ---");
        sb.AppendLine(
            "parent=" + (root.parent != null ? root.parent.name : "null"));
        sb.AppendLine(
            "anchors=" + root.anchorMin + " → " + root.anchorMax);
        sb.AppendLine("anchoredPosition=" + root.anchoredPosition);
        sb.AppendLine("sizeDelta=" + root.sizeDelta);
        sb.AppendLine("scale=" + root.localScale);

        for (int c = 0; c < root.childCount; c++)
        {
            RectTransform child = root.GetChild(c) as RectTransform;
            if (child == null)
            {
                continue;
            }

            string n = child.name;
            bool interesting =
                n.IndexOf("Mission", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Timer", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Rule", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Target", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Remaining", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Label", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Move", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (!interesting)
            {
                continue;
            }

            sb.AppendLine(
                "  " + n + " anchoredPosition=" + child.anchoredPosition +
                " screenBounds=" + GetScreenBounds(child));
        }
    }

    private static void AppendRectAudit(
        System.Text.StringBuilder sb,
        string label,
        RectTransform rt)
    {
        if (rt == null)
        {
            sb.AppendLine(label + "=null");
            return;
        }

        sb.AppendLine(label + " anchoredPosition=" + rt.anchoredPosition);
        sb.AppendLine(label + " anchors=" + rt.anchorMin + " → " + rt.anchorMax);
        sb.AppendLine(label + " screenBounds=" + GetScreenBounds(rt));
    }

    private static string GetScreenBounds(RectTransform rt)
    {
        if (rt == null)
        {
            return "null";
        }

        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return "min=(" + corners[0].x.ToString("0") + "," + corners[0].y.ToString("0") +
               ") max=(" + corners[2].x.ToString("0") + "," + corners[2].y.ToString("0") + ")";
    }
#endif

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Live-tune Wide Pause/Win scales while Wide preview or Wide play is active.
        if (pausePanel == null && winPanel == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            if (appliedKind == GameplayLayoutKind.WideTabletLandscape)
            {
                ApplyModalPanelsForKind(GameplayLayoutKind.WideTabletLandscape);
            }

            return;
        }

        if (IsAuthoringPreviewActive &&
            AuthoringPreviewKind == GameplayLayoutKind.WideTabletLandscape)
        {
            ApplyModalPanelsForKind(GameplayLayoutKind.WideTabletLandscape);
        }
    }
#endif
}

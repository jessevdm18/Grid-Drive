using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainMenu-only responsive presentation. Phone modes restore authored scene layout.
/// Wide Tablet applies inspector-authorable positions/scales. Does not own panel
/// visibility or button behavior.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-40)]
public class MainMenuLayoutController : MonoBehaviour
{
    [Serializable]
    public struct WideElementLayout
    {
        public Vector2 anchoredPosition;
        public Vector3 localScale;
        public Vector2 sizeDelta;
        public bool hasSizeDelta;

        public static WideElementLayout From(Vector2 pos, float uniformScale)
        {
            return new WideElementLayout
            {
                anchoredPosition = pos,
                localScale = Vector3.one * uniformScale,
                sizeDelta = Vector2.zero,
                hasSizeDelta = false
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
                localScale = rt.localScale,
                sizeDelta = rt.sizeDelta,
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

    [Header("Detection (same thresholds as Gameplay; scenes are not coupled)")]
    [SerializeField] private float wideAspectThreshold = GameplayLayoutMode.DefaultWideAspectThreshold;
    [SerializeField] private float tallPhoneMaxAspect = GameplayLayoutMode.DefaultTallPhoneMaxAspect;

    [Header("Home Screen Elements")]
    [SerializeField] private RectTransform title;
    [SerializeField] private RectTransform playButton;
    [SerializeField] private RectTransform levelsButton;
    [SerializeField] private RectTransform shopButton;
    [SerializeField] private RectTransform settingsButton;
    [SerializeField] private RectTransform carDecoration;

    [Header("Background")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform backgroundOverlay;
    [SerializeField] private Image backgroundImage;

    [Header("Modal Panels")]
    [Tooltip("Full-screen ShopPanel root (stretch). Prefer scaling ShopFrame.")]
    [SerializeField] private RectTransform shopPanel;
    [SerializeField] private RectTransform shopFrame;
    [SerializeField] private RectTransform settingsPanel;

    [Header("Wide Tablet — Home")]
    [SerializeField] private WideElementLayout wideTitle =
        WideElementLayout.From(new Vector2(-340f, 240f), 0.42f);
    [SerializeField] private WideElementLayout widePlay =
        WideElementLayout.From(new Vector2(-200f, 40f), 0.52f);
    [SerializeField] private WideElementLayout wideLevels =
        WideElementLayout.From(new Vector2(-200f, -160f), 0.52f);
    [SerializeField] private WideElementLayout wideShop =
        WideElementLayout.From(new Vector2(300f, 40f), 0.95f);
    [SerializeField] private WideElementLayout wideSettings =
        WideElementLayout.From(new Vector2(300f, -160f), 1f);
    [SerializeField] private WideElementLayout wideCarDecoration =
        WideElementLayout.From(new Vector2(360f, 280f), 0.85f);

    [Header("Wide Tablet — Modals")]
    [SerializeField, Range(0.45f, 1f)] private float wideShopFrameScale = 0.72f;
    [SerializeField] private Vector2 wideShopFrameOffset = Vector2.zero;
    [SerializeField, Range(0.45f, 1f)] private float wideSettingsPanelScale = 0.72f;
    [SerializeField] private Vector2 wideSettingsPanelOffset = Vector2.zero;

    private readonly Dictionary<RectTransform, RectSnap> phoneSnaps =
        new Dictionary<RectTransform, RectSnap>(16);

    private RectTransform canvasRoot;
    private bool phoneCached;
    private GameplayLayoutKind appliedKind = (GameplayLayoutKind)(-1);
    private int lastWidth = -1;
    private int lastHeight = -1;

#if UNITY_EDITOR
    private readonly List<EditorLiveSnap> editorPreviewSnapshot = new List<EditorLiveSnap>(16);
    private bool editorPreviewSnapshotValid;

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
        ResolveRefs();
        CachePhonePresentation();
        ApplyLayout(force: true);
    }

    private void Start()
    {
        ApplyLayout(force: true);
    }

    private void LateUpdate()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            ApplyLayout(force: true);
            return;
        }

        // Keep cover correct if canvas scaler changes rect without a Screen change.
        if (appliedKind == GameplayLayoutKind.WideTabletLandscape)
        {
            ApplyWideBackgroundCover();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Play Mode only: re-apply when Inspector Wide fields change.
        // Edit Mode preview must NEVER re-apply here — that snaps manual drags back.
        if (!Application.isPlaying || !isActiveAndEnabled)
        {
            return;
        }

        if (appliedKind == GameplayLayoutKind.WideTabletLandscape)
        {
            ApplyWidePresentation();
        }
    }
#endif

    private void ResolveRefs()
    {
        canvasRoot = null;
        Canvas selfCanvas = GetComponent<Canvas>();
        if (selfCanvas != null)
        {
            canvasRoot = selfCanvas.transform as RectTransform;
        }
        else
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                canvasRoot = parentCanvas.transform as RectTransform;
            }
            else
            {
                Canvas sceneCanvas = FindAnyObjectByType<Canvas>();
                if (sceneCanvas != null)
                {
                    canvasRoot = sceneCanvas.transform as RectTransform;
                }
            }
        }

        Transform root = canvasRoot != null ? canvasRoot : transform;

        if (title == null)
        {
            title = FindNamedRect(root, "Title", preferRootChild: true);
        }

        if (playButton == null)
        {
            playButton = FindNamedRect(root, "PlayButton");
        }

        if (levelsButton == null)
        {
            levelsButton = FindNamedRect(root, "LevelsButton");
        }

        if (shopButton == null)
        {
            shopButton = FindNamedRect(root, "ShopButton");
        }

        if (settingsButton == null)
        {
            settingsButton = FindNamedRect(root, "SettingsButton");
        }

        if (carDecoration == null)
        {
            carDecoration = FindNamedRect(root, "CarDecoration");
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

        if (shopPanel == null)
        {
            shopPanel = FindNamedRect(root, "ShopPanel");
        }

        if (shopFrame == null && shopPanel != null)
        {
            shopFrame = FindNamedRect(shopPanel, "ShopFrame");
        }

        if (settingsPanel == null)
        {
            settingsPanel = FindNamedRect(root, "SettingsPanel");
        }
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

        RectTransform[] all = root.GetComponentsInChildren<RectTransform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].name == name && all[i] != root)
            {
                if (preferRootChild && all[i].parent != root)
                {
                    continue;
                }

                return all[i];
            }
        }

        if (preferRootChild)
        {
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].name == name && all[i].parent == root)
                {
                    return all[i];
                }
            }
        }

        return null;
    }

    private void CachePhonePresentation()
    {
        if (phoneCached)
        {
            return;
        }

        CapturePhone(title);
        CapturePhone(playButton);
        CapturePhone(levelsButton);
        CapturePhone(shopButton);
        CapturePhone(settingsButton);
        CapturePhone(carDecoration);
        CapturePhone(background);
        CapturePhone(backgroundOverlay);
        CapturePhone(shopFrame);
        CapturePhone(settingsPanel);
        phoneCached = true;
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

    private void ApplyLayout(bool force)
    {
        GameplayLayoutKind want =
            GameplayLayoutMode.Resolve(tallPhoneMaxAspect, wideAspectThreshold);

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

        ResolveRefs();
        CachePhonePresentation();
        RestorePhonePresentation();

        if (want == GameplayLayoutKind.WideTabletLandscape)
        {
            ApplyWidePresentation();
        }
    }

    private void RestorePhonePresentation()
    {
        RestorePhone(title);
        RestorePhone(playButton);
        RestorePhone(levelsButton);
        RestorePhone(shopButton);
        RestorePhone(settingsButton);
        RestorePhone(carDecoration);
        RestorePhone(background);
        RestorePhone(backgroundOverlay);
        RestorePhone(shopFrame);
        RestorePhone(settingsPanel);
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
        ApplyWideElement(title, wideTitle);
        ApplyWideElement(playButton, widePlay);
        ApplyWideElement(levelsButton, wideLevels);
        ApplyWideElement(shopButton, wideShop);
        ApplyWideElement(settingsButton, wideSettings);
        ApplyWideElement(carDecoration, wideCarDecoration);
        ApplyWideBackgroundCover();

        // ShopPanel root stays stretch (dim overlay). Scale the centered ShopFrame only.
        if (shopFrame != null)
        {
            Vector3 phoneScale = phoneSnaps.TryGetValue(shopFrame, out RectSnap snap) && snap.Valid
                ? snap.LocalScale
                : Vector3.one;
            shopFrame.anchorMin = new Vector2(0.5f, 0.5f);
            shopFrame.anchorMax = new Vector2(0.5f, 0.5f);
            shopFrame.pivot = new Vector2(0.5f, 0.5f);
            shopFrame.anchoredPosition = wideShopFrameOffset;
            shopFrame.localScale = phoneScale * Mathf.Clamp(wideShopFrameScale, 0.45f, 1f);
        }

        if (settingsPanel != null)
        {
            Vector3 phoneScale =
                phoneSnaps.TryGetValue(settingsPanel, out RectSnap snap) && snap.Valid
                    ? snap.LocalScale
                    : Vector3.one;
            settingsPanel.anchorMin = new Vector2(0.5f, 0.5f);
            settingsPanel.anchorMax = new Vector2(0.5f, 0.5f);
            settingsPanel.pivot = new Vector2(0.5f, 0.5f);
            settingsPanel.anchoredPosition = wideSettingsPanelOffset;
            settingsPanel.localScale =
                phoneScale * Mathf.Clamp(wideSettingsPanelScale, 0.45f, 1f);
        }
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

    /// <summary>
    /// CSS-like background-size: cover for Wide only. Uniform scale, crop overflow.
    /// Overlay stretches full canvas (no aspect preserve).
    /// </summary>
    private void ApplyWideBackgroundCover()
    {
        if (canvasRoot == null)
        {
            ResolveRefs();
        }

        if (canvasRoot == null)
        {
            return;
        }

        // Overlay: full viewport stretch (color layer).
        if (backgroundOverlay != null)
        {
            backgroundOverlay.anchorMin = Vector2.zero;
            backgroundOverlay.anchorMax = Vector2.one;
            backgroundOverlay.pivot = new Vector2(0.5f, 0.5f);
            backgroundOverlay.anchoredPosition = Vector2.zero;
            backgroundOverlay.offsetMin = Vector2.zero;
            backgroundOverlay.offsetMax = Vector2.zero;
            backgroundOverlay.localScale = Vector3.one;
        }

        if (background == null)
        {
            return;
        }

        Sprite sprite = backgroundImage != null ? backgroundImage.sprite : null;
        float spriteW = 1080f;
        float spriteH = 1920f;
        if (sprite != null && sprite.rect.width > 0.01f && sprite.rect.height > 0.01f)
        {
            spriteW = sprite.rect.width;
            spriteH = sprite.rect.height;
        }
        else if (phoneSnaps.TryGetValue(background, out RectSnap phone) && phone.Valid)
        {
            // Fall back to authored phone size aspect.
            if (phone.SizeDelta.x > 0.01f && phone.SizeDelta.y > 0.01f)
            {
                spriteW = phone.SizeDelta.x;
                spriteH = phone.SizeDelta.y;
            }
        }

        float containerW = canvasRoot.rect.width;
        float containerH = canvasRoot.rect.height;
        if (containerW <= 0.01f || containerH <= 0.01f)
        {
            return;
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
    }

    // -------------------------------------------------------------------------
    // Editor preview (non-destructive) + Wide capture
    // -------------------------------------------------------------------------

#if UNITY_EDITOR
    public void EditorApplyPreview(GameplayLayoutKind kind)
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[MainMenuLayout] Apply Preview is Edit Mode only.");
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

        RestorePhonePresentation();
        if (kind == GameplayLayoutKind.WideTabletLandscape)
        {
            // One-shot apply. Manual drags after this must stick (no OnValidate rewrite).
            ApplyWidePresentation();
        }

        Debug.Log("[MainMenuLayout] Preview applied once: " + kind);
    }

    /// <summary>
    /// Reads live RectTransforms into serialized Wide home fields. Explicit only.
    /// Does not capture modals, visibility, or phone baseline.
    /// </summary>
    public void EditorCaptureCurrentWideLayout()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[MainMenuLayout] Capture Wide is Edit Mode only.");
            return;
        }

        ResolveRefs();

        if (title != null)
        {
            wideTitle = WideElementLayout.FromRect(title);
        }

        if (playButton != null)
        {
            widePlay = WideElementLayout.FromRect(playButton);
        }

        if (levelsButton != null)
        {
            wideLevels = WideElementLayout.FromRect(levelsButton);
        }

        if (shopButton != null)
        {
            wideShop = WideElementLayout.FromRect(shopButton);
        }

        if (settingsButton != null)
        {
            wideSettings = WideElementLayout.FromRect(settingsButton);
        }

        if (carDecoration != null)
        {
            wideCarDecoration = WideElementLayout.FromRect(carDecoration);
        }

        UnityEditor.EditorUtility.SetDirty(this);
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        Debug.Log(
            "[MainMenuLayout] Captured Wide home layout into serialized fields.\n" +
            "Title pos=" + wideTitle.anchoredPosition + " scale=" + wideTitle.localScale + "\n" +
            "Play pos=" + widePlay.anchoredPosition + "\n" +
            "Levels pos=" + wideLevels.anchoredPosition + "\n" +
            "Shop pos=" + wideShop.anchoredPosition + "\n" +
            "Settings pos=" + wideSettings.anchoredPosition);
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
        // Serialized Wide* fields intentionally untouched.
        Debug.Log("[MainMenuLayout] Preview cleared (Wide capture fields preserved)");
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
            title, playButton, levelsButton, shopButton, settingsButton,
            carDecoration, background, backgroundOverlay, shopFrame, settingsPanel
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

        editorPreviewSnapshotValid = editorPreviewSnapshot.Count > 0;
    }

    private void RestoreEditorPreviewSnapshot()
    {
        for (int i = 0; i < editorPreviewSnapshot.Count; i++)
        {
            EditorLiveSnap snap = editorPreviewSnapshot[i];
            RectTransform rt = snap.Rt;
            if (rt == null)
            {
                continue;
            }

            rt.anchorMin = snap.AnchorMin;
            rt.anchorMax = snap.AnchorMax;
            rt.pivot = snap.Pivot;
            rt.anchoredPosition = snap.AnchoredPosition;
            rt.sizeDelta = snap.SizeDelta;
            rt.localScale = snap.LocalScale;
            rt.localEulerAngles = snap.LocalEuler;
        }
    }

    private void ClearEditorPreviewSnapshot()
    {
        editorPreviewSnapshot.Clear();
        editorPreviewSnapshotValid = false;
    }
#endif
}

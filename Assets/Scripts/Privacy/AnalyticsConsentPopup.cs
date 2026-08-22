using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Explicit Firebase Analytics consent UI (not UMP).
/// Presenter stays on an always-active root; overlay is shown/hidden via CanvasGroup.
/// Builds a runtime modal when Inspector refs are missing so Splash needs no YAML edits.
/// </summary>
public class AnalyticsConsentPopup : MonoBehaviour
{
    private const int OverlaySortingOrder = 25000;
    private const string UnknownChoice = "Unknown";

    private const string TitleCopy = "HELP IMPROVE GRID DRIVE";
    private const string DescriptionCopy =
        "ALLOW ANONYMOUS USAGE DATA TO HELP US IMPROVE LEVELS, BALANCE AND PERFORMANCE.";
    private const string SecondaryCopy = "YOU CAN CHANGE THIS ANY TIME IN SETTINGS.";
    private const string DeclineCopy = "NO THANKS";
    private const string AllowCopy = "ALLOW";

    public static AnalyticsConsentPopup Instance { get; private set; }

    [Header("Optional Inspector wiring (runtime builds if empty)")]
    [SerializeField] private GameObject overlayRoot;
    [SerializeField] private Button declineButton;
    [SerializeField] private Button allowButton;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text secondaryText;
    [SerializeField] private Text declineButtonText;
    [SerializeField] private Text allowButtonText;

    private CanvasGroup overlayCanvasGroup;
    private Action onChoiceComplete;
    private bool isShowing;
    private bool uiReady;
    private bool builtRuntimeUi;

    /// <summary>True when RushOut_AnalyticsConsent is still Unknown.</summary>
    public static bool NeedsConsentChoice()
    {
        return PrivacyConsentManager.GetAnalyticsConsentPreference() == UnknownChoice;
    }

    /// <summary>
    /// Gate for startup: if Unknown, show popup and invoke callback after ALLOW/NO THANKS.
    /// Otherwise invoke immediately. Fail-soft if UI cannot be built (Analytics stays disabled, no pref write).
    /// </summary>
    public static void RunGate(Action onComplete)
    {
        if (!NeedsConsentChoice())
        {
            onComplete?.Invoke();
            return;
        }

        AnalyticsConsentPopup popup = EnsureInstance();
        if (popup == null || !popup.EnsureUiReady())
        {
            Debug.LogWarning(
                "[Privacy] AnalyticsConsentPopup UI unavailable — " +
                "skipping popup, Analytics stays disabled, startup continues."
            );
            onComplete?.Invoke();
            return;
        }

        popup.Show(onComplete);
    }

    public static AnalyticsConsentPopup EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        AnalyticsConsentPopup existing =
            FindAnyObjectByType<AnalyticsConsentPopup>(FindObjectsInactive.Exclude);
        if (existing != null)
        {
            Instance = existing;
            return Instance;
        }

        GameObject root = new GameObject("AnalyticsConsentPresenter");
        AnalyticsConsentPopup popup = root.AddComponent<AnalyticsConsentPopup>();
        return popup;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Presenter stays active for the Splash gate only — do NOT DontDestroyOnLoad.
        // Persisting this object (or a child EventSystem) duplicates systems in later scenes.

        WireButtonListeners();
        HideOverlayImmediate();
    }

    private void OnDestroy()
    {
        if (declineButton != null)
        {
            declineButton.onClick.RemoveListener(OnDeclineClicked);
        }

        if (allowButton != null)
        {
            allowButton.onClick.RemoveListener(OnAllowClicked);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private bool EnsureUiReady()
    {
        if (uiReady)
        {
            return true;
        }

        if (overlayRoot != null && declineButton != null && allowButton != null)
        {
            ApplyCopy();
            EnsureOverlayCanvasGroup();
            WireButtonListeners();
            uiReady = true;
            return true;
        }

        try
        {
            BuildRuntimeUi();
            ApplyCopy();
            EnsureOverlayCanvasGroup();
            WireButtonListeners();
            EnsureEventSystemExists();
            uiReady = overlayRoot != null && declineButton != null && allowButton != null;
            if (!uiReady)
            {
                Debug.LogWarning("[Privacy] AnalyticsConsentPopup runtime UI build incomplete.");
            }

            return uiReady;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[Privacy] AnalyticsConsentPopup UI init failed — " + ex.Message
            );
            return false;
        }
    }

    private void Show(Action onComplete)
    {
        if (isShowing)
        {
            onChoiceComplete += onComplete;
            return;
        }

        // Do NOT write preference on open — only on ALLOW / NO THANKS.
        onChoiceComplete = onComplete;
        isShowing = true;
        EnsureEventSystemExists();
        ApplyCopy();
        SetOverlayVisible(true);
        LogPrivacy("AnalyticsConsentPopupShown Choice=Unknown");
    }

    private void OnAllowClicked()
    {
        CompleteChoice(enabled: true);
    }

    private void OnDeclineClicked()
    {
        CompleteChoice(enabled: false);
    }

    private void CompleteChoice(bool enabled)
    {
        if (!isShowing)
        {
            return;
        }

        isShowing = false;
        PrivacyConsentManager.SetAnalyticsConsent(enabled);
        SetOverlayVisible(false);
        DestroyTemporaryEventSystemIfOwned();
        LogPrivacy(
            "AnalyticsConsentPopupClosed",
            "Choice=" + (enabled ? "Enabled" : "Disabled")
        );

        Action callback = onChoiceComplete;
        onChoiceComplete = null;
        callback?.Invoke();
    }

    private void WireButtonListeners()
    {
        if (declineButton != null)
        {
            declineButton.onClick.RemoveListener(OnDeclineClicked);
            declineButton.onClick.AddListener(OnDeclineClicked);
        }

        if (allowButton != null)
        {
            allowButton.onClick.RemoveListener(OnAllowClicked);
            allowButton.onClick.AddListener(OnAllowClicked);
        }
    }

    private void ApplyCopy()
    {
        SetText(titleText, TitleCopy);
        SetText(descriptionText, DescriptionCopy);
        SetText(secondaryText, SecondaryCopy);
        SetText(declineButtonText, DeclineCopy);
        SetText(allowButtonText, AllowCopy);
    }

    private static void SetText(Text field, string content)
    {
        if (field != null)
        {
            field.text = content;
        }
    }

    private void EnsureOverlayCanvasGroup()
    {
        if (overlayRoot == null)
        {
            return;
        }

        if (overlayCanvasGroup == null)
        {
            overlayCanvasGroup = overlayRoot.GetComponent<CanvasGroup>();
            if (overlayCanvasGroup == null)
            {
                overlayCanvasGroup = overlayRoot.AddComponent<CanvasGroup>();
            }
        }
    }

    private void SetOverlayVisible(bool visible)
    {
        EnsureOverlayCanvasGroup();
        if (overlayRoot == null)
        {
            return;
        }

        // Keep overlay GameObject active; gate input via CanvasGroup (FeatureTutorial lesson).
        if (!overlayRoot.activeSelf)
        {
            overlayRoot.SetActive(true);
        }

        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = visible ? 1f : 0f;
            overlayCanvasGroup.interactable = visible;
            overlayCanvasGroup.blocksRaycasts = visible;
        }
    }

    private void HideOverlayImmediate()
    {
        if (overlayRoot == null)
        {
            return;
        }

        EnsureOverlayCanvasGroup();
        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = 0f;
            overlayCanvasGroup.interactable = false;
            overlayCanvasGroup.blocksRaycasts = false;
        }
    }

    private static GameObject temporaryEventSystemRoot;

    /// <summary>
    /// Scene-owned EventSystems are authoritative. Never create a DontDestroyOnLoad
    /// EventSystem — that duplicates MainMenu/Gameplay systems after Splash leaves.
    /// </summary>
    private static void EnsureEventSystemExists()
    {
        EventSystem existing = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (existing != null)
        {
            return;
        }

        if (temporaryEventSystemRoot != null)
        {
            return;
        }

        temporaryEventSystemRoot = new GameObject("AnalyticsConsent_EventSystem_Temp");
        temporaryEventSystemRoot.AddComponent<EventSystem>();
        temporaryEventSystemRoot.AddComponent<InputSystemUIInputModule>();
        // Intentionally NOT DontDestroyOnLoad — lives only in the current Splash scene.
        LogPrivacy("EventSystemCreated Reason=SplashHadNone Ownership=SceneLocalTemp");
    }

    /// <summary>
    /// Drops the Splash-only temp EventSystem once consent is chosen so later scenes
    /// keep exactly one scene EventSystem.
    /// </summary>
    private static void DestroyTemporaryEventSystemIfOwned()
    {
        if (temporaryEventSystemRoot == null)
        {
            return;
        }

        UnityEngine.Object.Destroy(temporaryEventSystemRoot);
        temporaryEventSystemRoot = null;
        LogPrivacy("EventSystemTempDestroyed Reason=ConsentResolved");
    }

    private void BuildRuntimeUi()
    {
        if (builtRuntimeUi)
        {
            return;
        }

        builtRuntimeUi = true;

        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = OverlaySortingOrder;

        if (gameObject.GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (gameObject.GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        Font font = ResolveUiFont();

        GameObject overlay = CreateUiObject("AnalyticsConsentOverlay", transform);
        overlayRoot = overlay;
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        StretchFull(overlayRect);

        GameObject dim = CreateUiObject("DimBackground", overlay.transform);
        StretchFull(dim.GetComponent<RectTransform>());
        Image dimImage = dim.AddComponent<Image>();
        dimImage.color = new Color(0f, 0f, 0f, 0.72f);
        dimImage.raycastTarget = true;

        GameObject panel = CreateUiObject("Panel", overlay.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(860f, 720f);
        panelRect.anchoredPosition = Vector2.zero;
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.10f, 0.14f, 0.20f, 0.98f);
        panelImage.raycastTarget = true;

        titleText = CreateLabel(
            "Title",
            panel.transform,
            font,
            42,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(0f, 240f),
            new Vector2(780f, 100f)
        );

        descriptionText = CreateLabel(
            "Description",
            panel.transform,
            font,
            28,
            FontStyle.Normal,
            TextAnchor.UpperCenter,
            new Vector2(0f, 60f),
            new Vector2(780f, 200f)
        );

        secondaryText = CreateLabel(
            "Secondary",
            panel.transform,
            font,
            22,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(0f, -90f),
            new Vector2(780f, 80f)
        );
        secondaryText.color = new Color(0.75f, 0.80f, 0.86f, 1f);

        GameObject buttons = CreateUiObject("Buttons", panel.transform);
        RectTransform buttonsRect = buttons.GetComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.5f, 0f);
        buttonsRect.anchorMax = new Vector2(0.5f, 0f);
        buttonsRect.pivot = new Vector2(0.5f, 0f);
        buttonsRect.sizeDelta = new Vector2(780f, 110f);
        buttonsRect.anchoredPosition = new Vector2(0f, 40f);

        HorizontalLayoutGroup layout = buttons.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 28f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        declineButton = CreateButton(
            "DeclineButton",
            buttons.transform,
            font,
            new Color(0.22f, 0.26f, 0.32f, 1f),
            out declineButtonText
        );
        allowButton = CreateButton(
            "AllowButton",
            buttons.transform,
            font,
            new Color(0.18f, 0.55f, 0.42f, 1f),
            out allowButtonText
        );

        HideOverlayImmediate();
        LogPrivacy("AnalyticsConsentPopupRuntimeUiBuilt");
    }

    private static Font ResolveUiFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static Text CreateLabel(
        string name,
        Transform parent,
        Font font,
        int fontSize,
        FontStyle style,
        TextAnchor anchor,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        GameObject go = CreateUiObject(name, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Text text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = anchor;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        Font font,
        Color background,
        out Text label)
    {
        GameObject go = CreateUiObject(name, parent);
        Image image = go.AddComponent<Image>();
        image.color = background;
        image.raycastTarget = true;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = background * 1.08f;
        colors.pressedColor = background * 0.88f;
        button.colors = colors;

        GameObject labelGo = CreateUiObject("Label", go.transform);
        StretchFull(labelGo.GetComponent<RectTransform>());
        label = labelGo.AddComponent<Text>();
        label.font = font;
        label.fontSize = 30;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
        return button;
    }

    private static void LogPrivacy(string stage, string detail = null)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (string.IsNullOrEmpty(detail))
        {
            Debug.Log("[Privacy] " + stage);
        }
        else
        {
            Debug.Log("[Privacy] " + stage + " " + detail);
        }
#endif
    }
}

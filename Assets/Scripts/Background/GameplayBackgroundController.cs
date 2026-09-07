using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Past selected-skin background theme toe op bestaande Image-slots.
/// Creëert GEEN GameObjects. Fullscreen cover blijft bij UIBackgroundCover.
/// </summary>
public class GameplayBackgroundController : MonoBehaviour
{
    [Header("Base (bestaande cover Image)")]
    [SerializeField] private Image baseBackgroundImage;

    [Tooltip("Optioneel: UIBackgroundCover op dezelfde CoverImage. Wordt na sprite-swap aangeroepen.")]
    [SerializeField] private UIBackgroundCover baseBackgroundCover;

    [Tooltip("Optionele dim-overlay Image. Alleen alpha wordt gezet vanuit theme.")]
    [SerializeField] private Image dimOverlay;

    [Header("Animated layer slots (zelf maken + koppelen)")]
    [Tooltip("Image slots onder de background root. Ongebruikte slots worden uitgezet.")]
    [SerializeField] private List<Image> animatedLayerImages = new List<Image>();

    [Header("Theme resolve")]
    [SerializeField] private SkinManager skinManager;

    [Tooltip("Gebruikt als skin/theme ontbreekt.")]
    [SerializeField] private GameplayBackgroundThemeData fallbackTheme;

    private LayerRuntime[] layerRuntimes = System.Array.Empty<LayerRuntime>();
    private GameplayBackgroundThemeData activeTheme;
    private bool loggedMissingTheme;
    private bool loggedLayerOverflow;

    private void Awake()
    {
        if (skinManager == null)
        {
            skinManager = FindAnyObjectByType<SkinManager>();
        }

        if (baseBackgroundCover == null && baseBackgroundImage != null)
        {
            baseBackgroundCover = baseBackgroundImage.GetComponent<UIBackgroundCover>();
        }
    }

    private void OnEnable()
    {
        if (skinManager == null)
        {
            skinManager = FindAnyObjectByType<SkinManager>();
        }

        if (skinManager != null)
        {
            skinManager.OnSkinSelected += OnSelectedSkinChanged;
        }
    }

    private void OnDisable()
    {
        if (skinManager != null)
        {
            skinManager.OnSkinSelected -= OnSelectedSkinChanged;
        }
    }

    private void Start()
    {
        ApplyResolvedTheme();
    }

    private void OnSelectedSkinChanged(VehicleSkinData _)
    {
        ApplyResolvedTheme();
    }

    private void Update()
    {
        if (layerRuntimes == null || layerRuntimes.Length == 0)
        {
            return;
        }

        float dt = Time.unscaledDeltaTime;
        for (int i = 0; i < layerRuntimes.Length; i++)
        {
            TickLayer(ref layerRuntimes[i], dt);
        }
    }

    /// <summary>
    /// Past theme uit selected skin toe, anders fallback.
    /// </summary>
    public void ApplyResolvedTheme()
    {
        GameplayBackgroundThemeData theme = ResolveTheme();
        ApplyTheme(theme);
    }

    /// <summary>
    /// Direct theme toepassen (ook handig voor debug/tests).
    /// </summary>
    public void ApplyTheme(GameplayBackgroundThemeData theme)
    {
        loggedLayerOverflow = false;

        if (theme == null || !theme.HasValidBase)
        {
            if (!loggedMissingTheme)
            {
                Debug.LogWarning(
                    "GameplayBackgroundController: geen geldig theme (skin/fallback). " +
                    "Bestaande background blijft ongewijzigd."
                );
                loggedMissingTheme = true;
            }

            // DimOverlay mag nooit via animated-slot cleanup verdwijnen.
            if (theme != null)
            {
                ApplyDimOverlay(theme.BackgroundDimAlpha);
            }

            DisableAllAnimatedSlots();
            layerRuntimes = System.Array.Empty<LayerRuntime>();
            activeTheme = null;
            return;
        }

        loggedMissingTheme = false;
        activeTheme = theme;

        ApplyBaseSprite(theme.BaseBackground);
        ApplyDimOverlay(theme.BackgroundDimAlpha);
        ApplyAnimatedLayers(theme);
    }

    public GameplayBackgroundThemeData ActiveTheme => activeTheme;

    private GameplayBackgroundThemeData ResolveTheme()
    {
        if (skinManager == null)
        {
            skinManager = FindAnyObjectByType<SkinManager>();
        }

        if (skinManager != null)
        {
            VehicleSkinData skin = skinManager.GetSelectedSkin();
            if (skin != null && skin.BackgroundTheme != null)
            {
                return skin.BackgroundTheme;
            }
        }

        return fallbackTheme;
    }

    private void ApplyBaseSprite(Sprite sprite)
    {
        if (baseBackgroundImage == null || sprite == null)
        {
            return;
        }

        baseBackgroundImage.sprite = sprite;
        baseBackgroundImage.enabled = true;

        if (baseBackgroundCover != null)
        {
            baseBackgroundCover.ApplyCover();
        }
    }

    private void ApplyDimOverlay(float dimAlpha)
    {
        if (dimOverlay == null)
        {
            Debug.Log(
                "[BackgroundTheme] DimOverlay assigned=False, active=False, alpha=n/a, themeDim=" +
                dimAlpha.ToString("0.###")
            );
            return;
        }

        // DimOverlay blijft actief zolang GameplayBackground actief is; alleen alpha wijzigt.
        if (!dimOverlay.gameObject.activeSelf)
        {
            dimOverlay.gameObject.SetActive(true);
        }

        Color color = dimOverlay.color;
        color.a = Mathf.Clamp01(dimAlpha);
        dimOverlay.color = color;

        Debug.Log(
            "[BackgroundTheme] DimOverlay assigned=True, active=" +
            dimOverlay.gameObject.activeSelf +
            ", alpha=" + dimOverlay.color.a.ToString("0.###") +
            ", themeDim=" + dimAlpha.ToString("0.###")
        );
    }

    private bool IsDimOverlay(Image image)
    {
        return dimOverlay != null && image == dimOverlay;
    }

    private void ApplyAnimatedLayers(GameplayBackgroundThemeData theme)
    {
        IReadOnlyList<BackgroundAnimatedLayerData> layers = theme.AnimatedLayers;
        int layerCount = layers != null ? layers.Count : 0;
        int slotCount = animatedLayerImages != null ? animatedLayerImages.Count : 0;

        if (layerCount > slotCount && !loggedLayerOverflow)
        {
            Debug.LogWarning(
                "GameplayBackgroundController: theme '" + theme.DisplayName +
                "' heeft " + layerCount + " animated layers, maar slechts " +
                slotCount + " Image-slots. Extra layers worden genegeerd."
            );
            loggedLayerOverflow = true;
        }

        int useCount = Mathf.Min(layerCount, slotCount);
        var runtimes = new List<LayerRuntime>(useCount);

        for (int i = 0; i < slotCount; i++)
        {
            Image image = animatedLayerImages[i];
            if (image == null || IsDimOverlay(image))
            {
                continue;
            }

            if (i >= useCount)
            {
                image.gameObject.SetActive(false);
                continue;
            }

            BackgroundAnimatedLayerData data = layers[i];
            if (data == null || !data.HasSprite)
            {
                image.gameObject.SetActive(false);
                continue;
            }

            RectTransform rect = image.rectTransform;
            Color color = image.color;

            bool pulseAlpha =
                data.AnimationMode == BackgroundAnimationMode.AlphaPulse ||
                data.AlsoPulseAlpha;

            // Alpha alleen forceren wanneer alpha-pulse actief is.
            // Anders: serialized Image-alpha behouden (nooit stilzwijgend 0 via min/max).
            if (pulseAlpha)
            {
                color.a = Mathf.Clamp01((data.MinAlpha + data.MaxAlpha) * 0.5f);
            }
            else if (color.a <= 0.001f)
            {
                // Veilige zichtbare start als de Image per ongeluk op alpha 0 staat.
                color.a = 1f;
            }

            image.sprite = data.Sprite;
            image.color = color;
            image.gameObject.SetActive(true);

            Debug.Log(
                "[BackgroundTheme] Layer " + i +
                " sprite=" + data.Sprite.name +
                ", active=" + image.gameObject.activeSelf +
                ", alpha=" + color.a.ToString("0.###") +
                ", mode=" + data.AnimationMode
            );

            runtimes.Add(new LayerRuntime
            {
                image = image,
                rect = rect,
                data = data,
                startAnchoredPosition = rect.anchoredPosition,
                startLocalScale = rect.localScale,
                startColor = color,
                elapsed = 0f,
                direction = NormalizeOrRight(data.MovementDirection)
            });
        }

        layerRuntimes = runtimes.ToArray();
    }

    private void DisableAllAnimatedSlots()
    {
        if (animatedLayerImages == null)
        {
            return;
        }

        for (int i = 0; i < animatedLayerImages.Count; i++)
        {
            Image image = animatedLayerImages[i];
            if (image != null && !IsDimOverlay(image))
            {
                image.gameObject.SetActive(false);
            }
        }
    }

    private static void TickLayer(ref LayerRuntime runtime, float dt)
    {
        if (runtime.image == null || runtime.rect == null || runtime.data == null)
        {
            return;
        }

        if (!runtime.data.Loop && runtime.elapsed > 1000f)
        {
            return;
        }

        runtime.elapsed += dt;
        BackgroundAnimatedLayerData data = runtime.data;

        // Position
        switch (data.AnimationMode)
        {
            case BackgroundAnimationMode.PingPongMove:
            {
                float distance = Mathf.Max(0f, data.MovementDistance);
                float speed = Mathf.Max(0f, data.MovementSpeed);
                float wave = Mathf.Sin(runtime.elapsed * speed * 0.1f);
                runtime.rect.anchoredPosition =
                    runtime.startAnchoredPosition + runtime.direction * (wave * distance);
                break;
            }
            case BackgroundAnimationMode.ContinuousScroll:
            {
                float distance = Mathf.Max(0.01f, data.MovementDistance);
                float speed = Mathf.Max(0f, data.MovementSpeed);
                float travel = Mathf.Repeat(runtime.elapsed * speed, distance);
                runtime.rect.anchoredPosition =
                    runtime.startAnchoredPosition + runtime.direction * travel;
                break;
            }
            default:
                runtime.rect.anchoredPosition = runtime.startAnchoredPosition;
                break;
        }

        bool pulseAlpha =
            data.AnimationMode == BackgroundAnimationMode.AlphaPulse ||
            data.AlsoPulseAlpha;

        bool pulseScale =
            data.AnimationMode == BackgroundAnimationMode.ScalePulse ||
            data.AlsoPulseScale;

        if (pulseAlpha)
        {
            float t = (Mathf.Sin(runtime.elapsed * data.PulseSpeed) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(data.MinAlpha, data.MaxAlpha, t);
            Color c = runtime.startColor;
            c.a = Mathf.Clamp01(alpha);
            runtime.image.color = c;
        }

        if (pulseScale)
        {
            float t = (Mathf.Sin(runtime.elapsed * data.PulseSpeed) + 1f) * 0.5f;
            float amount = Mathf.Max(0f, data.ScalePulseAmount);
            float scaleMul = 1f + Mathf.Lerp(0f, amount, t);
            runtime.rect.localScale = runtime.startLocalScale * scaleMul;
        }
    }

    private static Vector2 NormalizeOrRight(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return Vector2.right;
        }

        return direction.normalized;
    }

    private struct LayerRuntime
    {
        public Image image;
        public RectTransform rect;
        public BackgroundAnimatedLayerData data;
        public Vector2 startAnchoredPosition;
        public Vector3 startLocalScale;
        public Color startColor;
        public float elapsed;
        public Vector2 direction;
    }
}

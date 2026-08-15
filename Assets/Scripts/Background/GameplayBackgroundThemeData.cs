using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animatie-modus voor één background-overlay layer.
/// </summary>
public enum BackgroundAnimationMode
{
    None = 0,
    PingPongMove = 1,
    ContinuousScroll = 2,
    AlphaPulse = 3,
    ScalePulse = 4
}

/// <summary>
/// Data voor één geanimeerde overlay-layer binnen een theme.
/// </summary>
[Serializable]
public class BackgroundAnimatedLayerData
{
    [SerializeField] private Sprite sprite;

    [SerializeField] private BackgroundAnimationMode animationMode = BackgroundAnimationMode.None;

    [Tooltip("Extra alpha-pulse naast movement-mode.")]
    [SerializeField] private bool alsoPulseAlpha;

    [Tooltip("Extra scale-pulse naast movement-mode.")]
    [SerializeField] private bool alsoPulseScale;

    [Tooltip("Richting voor PingPongMove / ContinuousScroll (wordt genormaliseerd).")]
    [SerializeField] private Vector2 movementDirection = Vector2.right;

    [SerializeField] private float movementSpeed = 8f;

    [Tooltip("Maximale afwijking in UI-units (PingPong) of wrap-afstand (Scroll).")]
    [SerializeField] private float movementDistance = 20f;

    [SerializeField] private float pulseSpeed = 0.6f;

    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.35f;

    [Range(0f, 1f)]
    [SerializeField] private float maxAlpha = 0.85f;

    [Tooltip("Schaal-afwijking t.o.v. 1.0, bijv. 0.02 = 1.00↔1.02.")]
    [SerializeField] private float scalePulseAmount = 0.02f;

    [SerializeField] private bool loop = true;

    public Sprite Sprite => sprite;
    public BackgroundAnimationMode AnimationMode => animationMode;
    public bool AlsoPulseAlpha => alsoPulseAlpha;
    public bool AlsoPulseScale => alsoPulseScale;
    public Vector2 MovementDirection => movementDirection;
    public float MovementSpeed => movementSpeed;
    public float MovementDistance => movementDistance;
    public float PulseSpeed => pulseSpeed;
    public float MinAlpha => minAlpha;
    public float MaxAlpha => maxAlpha;
    public float ScalePulseAmount => scalePulseAmount;
    public bool Loop => loop;

    public bool HasSprite => sprite != null;
}

/// <summary>
/// Gameplay background theme (base + optionele animated overlays).
/// Create → Grid Drive → Gameplay Background Theme
/// </summary>
[CreateAssetMenu(
    fileName = "GameplayBackgroundTheme",
    menuName = "Grid Drive/Gameplay Background Theme"
)]
public class GameplayBackgroundThemeData : ScriptableObject
{
    [SerializeField] private string themeId = "default_city";

    [SerializeField] private string displayName = "Default City";

    [SerializeField] private Sprite baseBackground;

    [Tooltip("Alpha van de DimOverlay (0 = transparant, 1 = volledig donker).")]
    [Range(0f, 1f)]
    [SerializeField] private float backgroundDimAlpha = 0.22f;

    [SerializeField] private List<BackgroundAnimatedLayerData> animatedLayers =
        new List<BackgroundAnimatedLayerData>();

    public string ThemeId => themeId;
    public string DisplayName => displayName;
    public Sprite BaseBackground => baseBackground;
    public float BackgroundDimAlpha => backgroundDimAlpha;
    public IReadOnlyList<BackgroundAnimatedLayerData> AnimatedLayers => animatedLayers;

    public bool HasValidBase => baseBackground != null;
}

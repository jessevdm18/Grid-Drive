using UnityEngine;

/// <summary>
/// Data voor één voertuig-skin. Maak via Create → Grid Drive → Vehicle Skin.
/// </summary>
[CreateAssetMenu(
    fileName = "VehicleSkin",
    menuName = "Grid Drive/Vehicle Skin"
)]
public class VehicleSkinData : ScriptableObject
{
    public const string ClassicId = "classic";

    [Tooltip("Stabiele id, bijv. classic / neon / candy / police.")]
    [SerializeField] private string skinId = ClassicId;

    [SerializeField] private string displayName = "Classic";

    [Tooltip("0 = gratis (bijv. Classic).")]
    [SerializeField] private int coinPrice;

    [SerializeField] private Sprite previewSprite;

    [Tooltip("True = altijd unlocked (Classic).")]
    [SerializeField] private bool unlockedByDefault;

    [Tooltip("Sprite library voor deze skin. Leeg = fallback library gebruiken.")]
    [SerializeField] private VehicleSpriteLibrary spriteLibrary;

    [Tooltip("Gameplay background theme voor deze skin. Leeg = controller-fallback.")]
    [SerializeField] private GameplayBackgroundThemeData backgroundTheme;

    public string SkinId => skinId;
    public string DisplayName => displayName;
    public int CoinPrice => coinPrice;
    public Sprite PreviewSprite => previewSprite;
    public bool UnlockedByDefault => unlockedByDefault;
    public VehicleSpriteLibrary SpriteLibrary => spriteLibrary;
    public GameplayBackgroundThemeData BackgroundTheme => backgroundTheme;

    public bool HasValidId => !string.IsNullOrWhiteSpace(skinId);
}

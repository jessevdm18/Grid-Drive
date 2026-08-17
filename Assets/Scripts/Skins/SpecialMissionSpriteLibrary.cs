using UnityEngine;

/// <summary>
/// Centrale special-mission target sprites (geen gameplay-impact).
/// Create → RushOut → Special Mission Sprite Library
/// LevelData.specialTargetSprite blijft optionele per-level override.
/// </summary>
[CreateAssetMenu(
    fileName = "SpecialMissionSpriteLibrary",
    menuName = "RushOut/Special Mission Sprite Library"
)]
public class SpecialMissionSpriteLibrary : ScriptableObject
{
    [Header("Target Overrides")]
    [Tooltip("TimedAmbulance target. Null = val terug naar skin TargetCarSprite.")]
    [SerializeField] private Sprite timedAmbulanceTargetSprite;

    [Tooltip("FragileCargo cargo-target. Null = val terug naar skin TargetCarSprite.")]
    [SerializeField] private Sprite fragileCargoTargetSprite;

    public Sprite TimedAmbulanceTargetSprite => timedAmbulanceTargetSprite;
    public Sprite FragileCargoTargetSprite => fragileCargoTargetSprite;
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Visuele sprite-keuze voor voertuigen (geen gameplay-impact).
/// Create → RushOut → Vehicle Sprite Library
/// </summary>
[CreateAssetMenu(
    fileName = "VehicleSpriteLibrary",
    menuName = "RushOut/Vehicle Sprite Library"
)]
public class VehicleSpriteLibrary : ScriptableObject
{
    [SerializeField] private Sprite targetCarSprite;

    [SerializeField] private List<Sprite> horizontalLength2Sprites = new List<Sprite>();
    [SerializeField] private List<Sprite> horizontalLength3Sprites = new List<Sprite>();
    [SerializeField] private List<Sprite> verticalLength2Sprites = new List<Sprite>();
    [SerializeField] private List<Sprite> verticalLength3Sprites = new List<Sprite>();

    public Sprite TargetCarSprite => targetCarSprite;

    /// <summary>
    /// Geeft een sprite voor orientation + length.
    /// Prefer notEqual to avoidSameSprite (voorkomt twee dezelfde achter elkaar).
    /// </summary>
    public Sprite GetSpriteForVehicle(
        VehicleController.VehicleOrientation orientation,
        int lengthInCells,
        Sprite avoidSameSprite = null)
    {
        List<Sprite> pool = GetPool(orientation, lengthInCells);
        return PickFromPool(pool, avoidSameSprite);
    }

    private List<Sprite> GetPool(
        VehicleController.VehicleOrientation orientation,
        int lengthInCells)
    {
        bool horizontal =
            orientation == VehicleController.VehicleOrientation.Horizontal;

        if (horizontal)
        {
            return lengthInCells >= 3 ? horizontalLength3Sprites : horizontalLength2Sprites;
        }

        return lengthInCells >= 3 ? verticalLength3Sprites : verticalLength2Sprites;
    }

    private static Sprite PickFromPool(List<Sprite> pool, Sprite avoidSameSprite)
    {
        if (pool == null || pool.Count == 0)
        {
            return null;
        }

        // Verzamel geldige (niet-null) entries.
        List<Sprite> valid = new List<Sprite>();
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null)
            {
                valid.Add(pool[i]);
            }
        }

        if (valid.Count == 0)
        {
            return null;
        }

        if (valid.Count == 1)
        {
            return valid[0];
        }

        // Probeer iets anders dan de vorige sprite.
        if (avoidSameSprite != null)
        {
            List<Sprite> alternatives = new List<Sprite>();
            for (int i = 0; i < valid.Count; i++)
            {
                if (valid[i] != avoidSameSprite)
                {
                    alternatives.Add(valid[i]);
                }
            }

            if (alternatives.Count > 0)
            {
                return alternatives[Random.Range(0, alternatives.Count)];
            }
        }

        return valid[Random.Range(0, valid.Count)];
    }
}

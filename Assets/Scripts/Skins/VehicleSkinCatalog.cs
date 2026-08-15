using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centrale catalogus van skins. Maak via Create → Grid Drive → Vehicle Skin Catalog.
/// Vul de lijst in de Inspector.
/// </summary>
[CreateAssetMenu(
    fileName = "VehicleSkinCatalog",
    menuName = "Grid Drive/Vehicle Skin Catalog"
)]
public class VehicleSkinCatalog : ScriptableObject
{
    [SerializeField] private List<VehicleSkinData> skins = new List<VehicleSkinData>();

    public IReadOnlyList<VehicleSkinData> Skins => skins;

    public VehicleSkinData GetById(string skinId)
    {
        if (string.IsNullOrWhiteSpace(skinId) || skins == null)
        {
            return null;
        }

        for (int i = 0; i < skins.Count; i++)
        {
            VehicleSkinData skin = skins[i];
            if (skin != null && skin.HasValidId && skin.SkinId == skinId)
            {
                return skin;
            }
        }

        return null;
    }

    public VehicleSkinData GetDefaultSkin()
    {
        VehicleSkinData classic = GetById(VehicleSkinData.ClassicId);
        if (classic != null)
        {
            return classic;
        }

        if (skins == null)
        {
            return null;
        }

        for (int i = 0; i < skins.Count; i++)
        {
            if (skins[i] != null && skins[i].HasValidId)
            {
                return skins[i];
            }
        }

        return null;
    }
}

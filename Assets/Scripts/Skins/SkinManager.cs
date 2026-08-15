using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centrale skin ownership / selectie / aankoop.
/// Gebruikt CoinManager voor coins. Save keys apart van RushOut_Coins.
/// </summary>
public class SkinManager : MonoBehaviour
{
    public const string ClassicSkinId = VehicleSkinData.ClassicId;

    private const string OwnedSkinsKey = "GridDrive_OwnedSkins";
    private const string SelectedSkinKey = "GridDrive_SelectedSkin";

    [Header("Data")]
    [SerializeField] private VehicleSkinCatalog catalog;

    [Tooltip("Fallback als selected skin geen library heeft (bijv. MainVehicleSpriteLibrary).")]
    [SerializeField] private VehicleSpriteLibrary fallbackSpriteLibrary;

    [Header("Economy")]
    [SerializeField] private CoinManager coinManager;

    public event Action<VehicleSkinData> OnSkinPurchased;
    public event Action<VehicleSkinData> OnSkinSelected;
    public event Action OnSkinsChanged;

    private readonly HashSet<string> ownedSkinIds = new HashSet<string>();
    private string selectedSkinId = ClassicSkinId;

    public string SelectedSkinId => selectedSkinId;

    private void Awake()
    {
        if (coinManager == null)
        {
            coinManager = FindFirstObjectByType<CoinManager>();
        }

        Load();
        EnsureDefaults();
    }

    public VehicleSkinData GetSkin(string skinId)
    {
        return catalog != null ? catalog.GetById(skinId) : null;
    }

    public IReadOnlyList<VehicleSkinData> GetAllSkins()
    {
        if (catalog == null)
        {
            return Array.Empty<VehicleSkinData>();
        }

        return catalog.Skins;
    }

    public bool IsOwned(string skinId)
    {
        if (string.IsNullOrWhiteSpace(skinId))
        {
            return false;
        }

        return ownedSkinIds.Contains(skinId);
    }

    public bool CanAfford(string skinId)
    {
        VehicleSkinData skin = GetSkin(skinId);
        if (skin == null)
        {
            return false;
        }

        if (skin.CoinPrice <= 0)
        {
            return true;
        }

        if (coinManager == null)
        {
            return false;
        }

        return coinManager.CanAfford(skin.CoinPrice);
    }

    public bool TryPurchaseSkin(string skinId)
    {
        VehicleSkinData skin = GetSkin(skinId);
        if (skin == null || !skin.HasValidId)
        {
            return false;
        }

        if (IsOwned(skinId))
        {
            return false;
        }

        int price = Mathf.Max(0, skin.CoinPrice);
        if (price > 0)
        {
            if (coinManager == null)
            {
                return false;
            }

            if (!coinManager.CanAfford(price))
            {
                return false;
            }

            if (!coinManager.SpendCoins(price))
            {
                return false;
            }
        }

        ownedSkinIds.Add(skin.SkinId);
        SaveOwned();
        OnSkinPurchased?.Invoke(skin);
        OnSkinsChanged?.Invoke();
        return true;
    }

    public bool SelectSkin(string skinId)
    {
        VehicleSkinData skin = GetSkin(skinId);
        if (skin == null || !skin.HasValidId)
        {
            return false;
        }

        if (!IsOwned(skinId))
        {
            return false;
        }

        selectedSkinId = skin.SkinId;
        SaveSelected();
        OnSkinSelected?.Invoke(skin);
        OnSkinsChanged?.Invoke();
        return true;
    }

    public VehicleSkinData GetSelectedSkin()
    {
        VehicleSkinData selected = GetSkin(selectedSkinId);
        if (selected != null && IsOwned(selected.SkinId))
        {
            return selected;
        }

        return catalog != null ? catalog.GetDefaultSkin() : null;
    }

    /// <summary>
    /// Library voor de actieve skin, anders fallback. Null = LevelManager default behouden.
    /// </summary>
    public VehicleSpriteLibrary GetActiveSpriteLibrary()
    {
        VehicleSkinData selected = GetSelectedSkin();
        if (selected != null && selected.SpriteLibrary != null)
        {
            return selected.SpriteLibrary;
        }

        return fallbackSpriteLibrary;
    }

    private void EnsureDefaults()
    {
        if (catalog == null)
        {
            selectedSkinId = ClassicSkinId;
            return;
        }

        // Alle unlockedByDefault skins markeren als owned.
        IReadOnlyList<VehicleSkinData> skins = catalog.Skins;
        for (int i = 0; i < skins.Count; i++)
        {
            VehicleSkinData skin = skins[i];
            if (skin != null && skin.HasValidId && skin.UnlockedByDefault)
            {
                ownedSkinIds.Add(skin.SkinId);
            }
        }

        // Classic altijd owned als die in catalog staat.
        VehicleSkinData classic = catalog.GetById(ClassicSkinId);
        if (classic != null && classic.HasValidId)
        {
            ownedSkinIds.Add(ClassicSkinId);
        }

        SaveOwned();

        if (!IsOwned(selectedSkinId) || GetSkin(selectedSkinId) == null)
        {
            VehicleSkinData fallback = catalog.GetDefaultSkin();
            selectedSkinId = fallback != null ? fallback.SkinId : ClassicSkinId;
            SaveSelected();
        }
    }

    private void Load()
    {
        ownedSkinIds.Clear();

        string ownedRaw = PlayerPrefs.GetString(OwnedSkinsKey, string.Empty);
        if (!string.IsNullOrEmpty(ownedRaw))
        {
            string[] parts = ownedRaw.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string id = parts[i].Trim();
                if (!string.IsNullOrEmpty(id))
                {
                    ownedSkinIds.Add(id);
                }
            }
        }

        selectedSkinId = PlayerPrefs.GetString(SelectedSkinKey, ClassicSkinId);
        if (string.IsNullOrWhiteSpace(selectedSkinId))
        {
            selectedSkinId = ClassicSkinId;
        }
    }

    private void SaveOwned()
    {
        if (ownedSkinIds.Count == 0)
        {
            PlayerPrefs.SetString(OwnedSkinsKey, string.Empty);
            PlayerPrefs.Save();
            return;
        }

        var ids = new List<string>(ownedSkinIds);
        ids.Sort(StringComparer.Ordinal);
        PlayerPrefs.SetString(OwnedSkinsKey, string.Join(",", ids));
        PlayerPrefs.Save();
    }

    private void SaveSelected()
    {
        PlayerPrefs.SetString(SelectedSkinKey, selectedSkinId ?? ClassicSkinId);
        PlayerPrefs.Save();
    }
}

using System;
using UnityEngine;

/// <summary>
/// Player-owned Ads Free entitlement.
/// The store is authoritative when a purchase or restore is observed.
/// The local flag is only a cache: it is set true and never cleared because
/// the store is offline or a purchase fetch fails.
/// </summary>
public static class AdsFreeService
{
    public const string PrefsKey = "RushOut_AdsFreeOwned";

    public static event Action OnChanged;

    /// <summary>True after a successful purchase or a restored non-consumable.</summary>
    public static bool IsOwned => PlayerPrefs.GetInt(PrefsKey, 0) == 1;

    /// <summary>Record store ownership. No-op when already owned.</summary>
    public static void SetOwnedFromStore()
    {
        if (IsOwned)
        {
            return;
        }

        PlayerPrefs.SetInt(PrefsKey, 1);
        PlayerPrefs.Save();
        OnChanged?.Invoke();
    }
}

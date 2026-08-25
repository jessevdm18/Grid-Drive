using UnityEngine;

/// <summary>
/// Centralized gameplay presentation mode detection.
/// </summary>
public enum GameplayLayoutKind
{
    /// <summary>Tall portrait (e.g. 1080x2400). Authored phone layout.</summary>
    TallPhonePortrait = 0,

    /// <summary>Shorter / wider portrait (e.g. 1080x1920, 480x800).</summary>
    CompactPhonePortrait = 1,

    /// <summary>Wide landscape tablet two-column layout.</summary>
    WideTabletLandscape = 2
}

/// <summary>
/// Single source of truth for gameplay layout classification.
/// </summary>
public static class GameplayLayoutMode
{
    /// <summary>
    /// Portrait aspect (width/height) at or below this uses TallPhonePortrait.
    /// Pixel 7 1080x2400 ≈ 0.45 stays tall; 1080x1920 ≈ 0.56 becomes compact.
    /// </summary>
    public const float DefaultTallPhoneMaxAspect = 0.50f;

    /// <summary>Landscape aspect at or above this uses WideTabletLandscape.</summary>
    public const float DefaultWideAspectThreshold = 1.25f;

    public static float GetScreenAspect()
    {
        if (Screen.height <= 0)
        {
            return 1f;
        }

        return (float)Screen.width / Screen.height;
    }

    public static GameplayLayoutKind Resolve(
        float tallPhoneMaxAspect = DefaultTallPhoneMaxAspect,
        float wideAspectThreshold = DefaultWideAspectThreshold)
    {
        if (Screen.width <= 0 || Screen.height <= 0)
        {
            return GameplayLayoutKind.TallPhonePortrait;
        }

        float aspect = GetScreenAspect();
        float wideThreshold = Mathf.Max(1.01f, wideAspectThreshold);
        float tallMax = Mathf.Clamp(tallPhoneMaxAspect, 0.35f, 0.75f);

        // Tablet / wide landscape first — unchanged from prior behavior.
        if (Screen.width > Screen.height && aspect >= wideThreshold)
        {
            return GameplayLayoutKind.WideTabletLandscape;
        }

        // Portrait phones: tall vs compact by aspect.
        if (Screen.width <= Screen.height)
        {
            return aspect > tallMax
                ? GameplayLayoutKind.CompactPhonePortrait
                : GameplayLayoutKind.TallPhonePortrait;
        }

        // Non-tablet landscape fallback (rare): prefer compact over tall center offsets.
        return GameplayLayoutKind.CompactPhonePortrait;
    }

    /// <summary>Backward-compatible wide-tablet check.</summary>
    public static bool IsWideTabletLayout(float wideAspectThreshold = DefaultWideAspectThreshold)
    {
        return Resolve(DefaultTallPhoneMaxAspect, wideAspectThreshold) ==
               GameplayLayoutKind.WideTabletLandscape;
    }
}

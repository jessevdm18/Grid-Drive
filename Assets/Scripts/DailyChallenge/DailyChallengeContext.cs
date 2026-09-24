using UnityEngine;

/// <summary>
/// Cross-scene Daily Challenge gameplay arming (MainMenu → Gameplay).
/// Separate from V1PlaytestOverride. Survives scene load via static + PlayerPrefs backstop.
/// Cleared when the Daily session ends (complete / fail / abandon / return to menu).
/// </summary>
public static class DailyChallengeContext
{
    private const string PendingAssetKey = "RushOut_DailyChallenge_PendingAsset";
    private const string ActiveFlagKey = "RushOut_DailyChallenge_ActiveSession";

    private static LevelData pendingLevel;
    private static LevelData activeLevel;
    private static bool activeSession;

    /// <summary>True while a Daily Gameplay launch is armed or in flight.</summary>
    public static bool HasPending =>
        pendingLevel != null ||
        !string.IsNullOrEmpty(PlayerPrefs.GetString(PendingAssetKey, string.Empty));

    /// <summary>True while Daily Challenge Gameplay session is active.</summary>
    public static bool IsActiveSession =>
        activeSession ||
        activeLevel != null ||
        PlayerPrefs.GetInt(ActiveFlagKey, 0) == 1;

    public static LevelData ActiveLevel => activeLevel;

    /// <summary>
    /// Arm before <see cref="SceneTransition.LoadScene"/>("Gameplay").
    /// Persists asset name so force-close mid-transition still counts as used.
    /// </summary>
    public static void ArmPending(LevelData level)
    {
        if (level == null)
        {
            Debug.LogError("[DailyChallenge] ArmPending: level is null.");
            return;
        }

        pendingLevel = level;
        activeLevel = level;
        activeSession = true;
        PlayerPrefs.SetString(PendingAssetKey, level.name);
        PlayerPrefs.SetInt(ActiveFlagKey, 1);
        PlayerPrefs.Save();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[DailyChallenge] Context armed pending=" + level.name);
#endif
    }

    /// <summary>
    /// Consume pending once in <see cref="LevelManager.Start"/>. Keeps active session.
    /// </summary>
    public static bool TryConsumePendingLevel(DailyChallengeConfig config, out LevelData level)
    {
        level = pendingLevel;
        if (level == null)
        {
            string assetName = PlayerPrefs.GetString(PendingAssetKey, string.Empty);
            level = FindInPool(config, assetName);
        }

        pendingLevel = null;
        PlayerPrefs.DeleteKey(PendingAssetKey);
        PlayerPrefs.Save();

        if (level == null)
        {
            return false;
        }

        activeLevel = level;
        activeSession = true;
        PlayerPrefs.SetInt(ActiveFlagKey, 1);
        PlayerPrefs.Save();
        return true;
    }

    public static LevelData FindInPool(DailyChallengeConfig config, string assetName)
    {
        if (config == null || string.IsNullOrEmpty(assetName))
        {
            return null;
        }

        for (int i = 0; i < config.PoolCount; i++)
        {
            LevelData entry = config.GetLevelAt(i);
            if (entry != null && entry.name == assetName)
            {
                return entry;
            }
        }

        return null;
    }

    /// <summary>Clear session when leaving Daily Gameplay.</summary>
    public static void ClearSession()
    {
        pendingLevel = null;
        activeLevel = null;
        activeSession = false;
        PlayerPrefs.DeleteKey(PendingAssetKey);
        PlayerPrefs.DeleteKey(ActiveFlagKey);
        PlayerPrefs.Save();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[DailyChallenge] Context session cleared");
#endif
    }
}

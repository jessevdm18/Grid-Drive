using UnityEngine;

/// <summary>
/// First-time feature tutorial seen-state (PlayerPrefs).
/// Los van ObjectiveTutorialPrefs — meta features vs level rules.
/// </summary>
public enum FeatureTutorialType
{
    Coins = 0,
    /// <summary>Deprecated — geen Undo onboarding meer. Key mag blijven voor oude saves.</summary>
    Undo = 1,
    Hint = 2,
    Skins = 3,
    WatchAdForHint = 4
}

public static class FeatureTutorialPrefs
{
    private const string KeyPrefix = "RushOut_FeatureTutorial_";

    public static bool HasSeen(FeatureTutorialType type)
    {
        return PlayerPrefs.GetInt(GetKey(type), 0) == 1;
    }

    public static void MarkSeen(FeatureTutorialType type)
    {
        PlayerPrefs.SetInt(GetKey(type), 1);
        PlayerPrefs.Save();
    }

    public static void Reset(FeatureTutorialType type)
    {
        PlayerPrefs.DeleteKey(GetKey(type));
        PlayerPrefs.Save();
    }

    public static void ResetAll()
    {
        Reset(FeatureTutorialType.Coins);
        Reset(FeatureTutorialType.Undo);
        Reset(FeatureTutorialType.Hint);
        Reset(FeatureTutorialType.Skins);
        Reset(FeatureTutorialType.WatchAdForHint);
    }

    public static string KeyFor(FeatureTutorialType type)
    {
        return GetKey(type);
    }

    private static string GetKey(FeatureTutorialType type)
    {
        return KeyPrefix + type;
    }
}

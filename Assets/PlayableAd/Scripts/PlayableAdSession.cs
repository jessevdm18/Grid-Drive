using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Isolated marketing session. Production scenes never set this.
/// </summary>
public static class PlayableAdSession
{
    public const string SceneName = "PlayableAd";
    public const string StoreUrl =
        "https://play.google.com/store/apps/details?id=com.raddergames.griddrive";

    public static bool IsActive { get; private set; }

    public static LevelData Level { get; private set; }

    public static event Action LevelCompleted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        IsActive = false;
        Level = null;
        LevelCompleted = null;
    }

    public static void Begin(LevelData level)
    {
        Level = level;
        IsActive = level != null;
    }

    public static void NotifyLevelCompleted()
    {
        LevelCompleted?.Invoke();
    }

    /// <summary>
    /// True for the playable scene, and for a player build made with PLAYABLE_AD.
    /// </summary>
    public static bool ShouldSuppressProductionServices()
    {
#if PLAYABLE_AD
        return true;
#else
        Scene active = SceneManager.GetActiveScene();
        if (active.IsValid() && active.name == SceneName)
        {
            return true;
        }

#if UNITY_EDITOR
        Scene editorScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (editorScene.IsValid() && editorScene.name == SceneName)
        {
            return true;
        }
#endif
        return false;
#endif
    }
}

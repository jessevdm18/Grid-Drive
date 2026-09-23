using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Validates that authored Lives HUD / Out Of Lives UI exist in player-facing scenes.
/// Does not create visual hierarchy — scenes must be authored via
/// RushOut/UI/Author Lives &amp; Failure UI Into Open Scenes.
/// </summary>
public static class LivesUiBootstrap
{
    private const string Gameplay = "Gameplay";
    private const string MainMenu = "MainMenu";
    private const string LevelSelect = "LevelSelect";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void HookSceneLoaded()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (!Application.isPlaying)
        {
            return;
        }

        EnsureForActiveScene();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        EnsureForScene(scene.name);
    }

    /// <summary>Idempotent check for the active scene.</summary>
    public static void EnsureForActiveScene()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        EnsureForScene(SceneManager.GetActiveScene().name);
    }

    public static void EnsureForScene(string sceneName)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (!ShouldShowInScene(sceneName))
        {
            return;
        }

        LivesHUD hud = LivesHUD.FindInLoadedScenes();
        OutOfLivesUI popup = OutOfLivesUI.FindInLoadedScenes();

        if (hud == null)
        {
            Debug.LogError(
                "[Lives] LivesHUD missing in scene '" + sceneName +
                "'. Author it under the canvas SafeArea (Gameplay) or root canvas."
            );
        }

        if (popup == null)
        {
            Debug.LogError(
                "[Lives] OutOfLivesUI missing in scene '" + sceneName +
                "'. Author OutOfLivesPanel via RushOut/UI/Author Lives & Failure UI."
            );
        }
    }

    private static bool ShouldShowInScene(string sceneName)
    {
        return sceneName == Gameplay ||
               sceneName == MainMenu ||
               sceneName == LevelSelect;
    }
}

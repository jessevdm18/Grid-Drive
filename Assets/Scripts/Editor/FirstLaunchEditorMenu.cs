using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only tools voor first-launch startup testing.
/// </summary>
public static class FirstLaunchEditorMenu
{
    private const string MenuRoot = "RushOut/Testing/";

    [MenuItem(MenuRoot + "Reset First Launch Key", priority = 100)]
    private static void ResetFirstLaunchKey()
    {
        SaveManager.EditorResetFirstLaunchCompletedKey();
        Debug.Log(
            "[FirstLaunch] RushOut_FirstLaunchCompleted gewist. " +
            "Let op: bestaande progress-keys triggeren migratie → returning player. " +
            "Gebruik Simulate Fresh First Launch om de route te forceren."
        );
    }

    [MenuItem(MenuRoot + "Simulate Fresh First Launch", priority = 101)]
    private static void SimulateFreshFirstLaunch()
    {
        SaveManager.EditorArmSimulateFreshFirstLaunch();
        Debug.Log(
            "[FirstLaunch] Volgende Splash-start forceert Gameplay Level 1 (index 0). " +
            "Progress/coins/stars/tutorials blijven intact. Play vanaf Splash-scene."
        );
    }
}

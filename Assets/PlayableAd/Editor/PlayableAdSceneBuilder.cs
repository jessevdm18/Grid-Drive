using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Copies the gameplay objects a playable needs into a new scene.
/// Does not save Gameplay.unity.
/// </summary>
public static class PlayableAdSceneBuilder
{
    private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
    private const string PlayableScenePath = "Assets/PlayableAd/Scenes/PlayableAd.unity";
    private const string LevelPath = "Assets/Data/Levels/Level_038.asset";
    private const string LogoPath = "Assets/Art/GridDriveLogo.png";
    private const string FontPath = "Assets/Fonts/LuckiestGuy-Regular SDF.asset";

    private static readonly string[] RemoveRoots =
    {
        "AdsManager",
        "BackgroundCanvas",
        "CoinManager",
        "ConfettiLayer",
        "DailyChallengeManager",
        "GameCanvas",
        "HintManager",
        "LimitedVehicleMarker",
        "NoTouchProtectedMarker",
        "PauseManager",
        "SaveManager",
        "UIManager",
        "UndoManager",
        "WinConfettiCanvas",
    };

    [MenuItem("RushOut/Marketing/Create Playable Ad Scene")]
    public static void CreateScene()
    {
        if (!Application.isBatchMode &&
            !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Directory.CreateDirectory("Assets/PlayableAd/Scenes");
        File.Copy(
            Path.GetFullPath(GameplayScenePath),
            Path.GetFullPath(PlayableScenePath),
            overwrite: true);
        AssetDatabase.ImportAsset(PlayableScenePath);

        Scene playable = EditorSceneManager.OpenScene(PlayableScenePath, OpenSceneMode.Single);
        var remove = new HashSet<string>(RemoveRoots);
        GameObject[] roots = playable.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (remove.Contains(roots[i].name))
            {
                Object.DestroyImmediate(roots[i]);
            }
        }

        LevelManager levelManager = Object.FindAnyObjectByType<LevelManager>();
        if (levelManager != null)
        {
            SkinManager skin = levelManager.GetComponent<SkinManager>();
            if (skin != null)
            {
                Object.DestroyImmediate(skin);
            }
        }

        GameObject bootstrapObject = new GameObject("PlayableAdBootstrap");
        PlayableAdBootstrap bootstrap = bootstrapObject.AddComponent<PlayableAdBootstrap>();
        SerializedObject serialized = new SerializedObject(bootstrap);
        serialized.FindProperty("level").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<LevelData>(LevelPath);
        serialized.FindProperty("logo").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<Sprite>(LogoPath);
        serialized.FindProperty("font").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(playable);
        EditorSceneManager.SaveScene(playable, PlayableScenePath);
        Debug.Log("[PlayableAd] Scene written to " + PlayableScenePath + " from a copy of Gameplay. Gameplay.unity was not saved.");
    }
}

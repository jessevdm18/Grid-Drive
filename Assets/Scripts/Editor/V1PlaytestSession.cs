using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor-only helpers: resolve current Gameplay LevelData and load V1 finals for playtest.
/// Uses V1PlaytestOverride (SessionState GUID) so candidates need not be in MainLevelDatabase.
/// </summary>
public static class V1PlaytestSession
{
    public const string GameplaySceneName = "Gameplay";
    public const string CurrentLevelPrefsKey = "RushOut_CurrentLevel";

    public struct ResolvedLevel
    {
        public bool ok;
        public LevelData level;
        public int dbIndex;
        public string message;
        public V1CurationEntry curationEntry;
        /// <summary>True when loaded via direct candidate override (not Main DB prefs).</summary>
        public bool isDirectOverride;
    }

    [MenuItem("RushOut/Levels/Clear V1 Playtest Override")]
    public static void MenuClearOverride()
    {
        V1PlaytestOverride.Clear();
        Debug.Log("V1 Playtest Override cleared (Pending + Active).");
        EditorUtility.DisplayDialog(
            "Clear V1 Playtest Override",
            "Pending and Active playtest GUIDs cleared.",
            "OK"
        );
    }

    public static LevelDatabase LoadMainDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("MainLevelDatabase t:LevelDatabase");
        if (guids == null || guids.Length == 0)
        {
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<LevelDatabase>(
            AssetDatabase.GUIDToAssetPath(guids[0])
        );
    }

    /// <summary>
    /// Prefer live LevelManager in Play Mode; else Active override; else PlayerPrefs → DB.
    /// </summary>
    public static ResolvedLevel ResolveCurrentLevel()
    {
        ResolvedLevel result = new ResolvedLevel
        {
            ok = false,
            dbIndex = -1,
            message = "No active Gameplay level",
            isDirectOverride = false
        };

        LevelDatabase database = LoadMainDatabase();
        V1CurationState state = V1CurationState.LoadOrCreate();

        if (EditorApplication.isPlaying)
        {
            LevelManager levelManager = Object.FindAnyObjectByType<LevelManager>();
            if (levelManager != null)
            {
                LevelData live = levelManager.CurrentLevelData;
                if (live != null)
                {
                    result.ok = true;
                    result.level = live;
                    result.dbIndex = levelManager.IsV1CandidatePlaytest
                        ? FindDatabaseIndex(database, live)
                        : levelManager.CurrentLevelIndex;
                    result.isDirectOverride = levelManager.IsV1CandidatePlaytest;
                    result.curationEntry = state.FindByLevel(live);
                    result.message = result.curationEntry == null
                        ? "Level not in V1CurationState"
                        : string.Empty;
                    return result;
                }
            }
        }

        // Active V1 override (Editor session) — even when not playing.
        if (V1PlaytestOverride.TryGetActiveLevel(out LevelData activeLevel))
        {
            result.ok = true;
            result.level = activeLevel;
            result.dbIndex = FindDatabaseIndex(database, activeLevel);
            result.isDirectOverride = true;
            result.curationEntry = state.FindByLevel(activeLevel);
            result.message = result.curationEntry == null
                ? "Level not in V1CurationState"
                : string.Empty;
            return result;
        }

        if (database == null || database.LevelCount == 0)
        {
            result.message = "MainLevelDatabase missing/empty";
            return result;
        }

        int prefsIndex = PlayerPrefs.GetInt(CurrentLevelPrefsKey, 0);
        prefsIndex = Mathf.Clamp(prefsIndex, 0, database.LevelCount - 1);
        LevelData level = database.GetLevel(prefsIndex);
        if (level == null)
        {
            result.message = "Current index has null LevelData";
            return result;
        }

        result.ok = true;
        result.level = level;
        result.dbIndex = prefsIndex;
        result.isDirectOverride = false;
        result.curationEntry = state.FindByLevel(level);
        result.message = result.curationEntry == null
            ? "Level not in V1CurationState"
            : string.Empty;
        return result;
    }

    public static int FindDatabaseIndex(LevelDatabase database, LevelData level)
    {
        if (database == null || level == null || database.levels == null)
        {
            return -1;
        }

        for (int i = 0; i < database.levels.Count; i++)
        {
            if (database.levels[i] == level)
            {
                return i;
            }
        }

        // GUID fallback if object identity differs.
        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(level));
        if (string.IsNullOrEmpty(guid))
        {
            return -1;
        }

        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData other = database.levels[i];
            if (other == null)
            {
                continue;
            }

            string otherGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(other));
            if (otherGuid == guid)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Stable playtest order = curated finals sorted by difficulty → area → moves → guid.
    /// </summary>
    public static List<V1CurationEntry> GetOrderedFinals(
        V1CurationState state,
        LevelDifficulty? difficultyFilter)
    {
        List<V1CurationEntry> finals = state != null
            ? state.GetFinalEntries()
            : new List<V1CurationEntry>();

        List<V1CurationEntry> filtered = new List<V1CurationEntry>();
        for (int i = 0; i < finals.Count; i++)
        {
            V1CurationEntry e = finals[i];
            if (e == null || e.level == null)
            {
                continue;
            }

            if (difficultyFilter.HasValue && e.assignedDifficulty != difficultyFilter.Value)
            {
                continue;
            }

            filtered.Add(e);
        }

        filtered.Sort(CompareFinalsStable);
        return filtered;
    }

    public static List<V1CurationEntry> GetOrderedUnreviewedFinals(
        V1CurationState state,
        LevelDifficulty? difficultyFilter)
    {
        List<V1CurationEntry> all = GetOrderedFinals(state, difficultyFilter);
        List<V1CurationEntry> result = new List<V1CurationEntry>();
        for (int i = 0; i < all.Count; i++)
        {
            if (V1CurationState.IsUnreviewedFinal(all[i].status))
            {
                result.Add(all[i]);
            }
        }

        return result;
    }

    private static int CompareFinalsStable(V1CurationEntry a, V1CurationEntry b)
    {
        int d = a.assignedDifficulty.CompareTo(b.assignedDifficulty);
        if (d != 0)
        {
            return d;
        }

        LevelData la = a.level;
        LevelData lb = b.level;
        int areaA = la != null ? la.ResolvedGridWidth * la.ResolvedGridHeight : 0;
        int areaB = lb != null ? lb.ResolvedGridWidth * lb.ResolvedGridHeight : 0;
        int areaCmp = areaA.CompareTo(areaB);
        if (areaCmp != 0)
        {
            return areaCmp;
        }

        int moves = (la != null ? la.minimumMoves : 0).CompareTo(
            lb != null ? lb.minimumMoves : 0
        );
        if (moves != 0)
        {
            return moves;
        }

        return string.CompareOrdinal(a.assetGuid, b.assetGuid);
    }

    /// <summary>
    /// Arms V1PlaytestOverride and opens/reloads Gameplay.
    /// Does NOT require MainLevelDatabase membership. Does NOT write PlayerPrefs.
    /// </summary>
    public static string LoadLevelForPlaytest(LevelData level, out int dbIndex)
    {
        dbIndex = -1;
        if (level == null)
        {
            return "LevelData is null.";
        }

        LevelDatabase database = LoadMainDatabase();
        dbIndex = FindDatabaseIndex(database, level);

        V1PlaytestOverride.SetPendingLevel(level);

        string scenePath = FindGameplayScenePath();

        if (EditorApplication.isPlaying)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                SceneManager.LoadScene(GameplaySceneName);
            }
            else
            {
                SceneManager.LoadScene(scenePath);
            }

            return string.Empty;
        }

        if (string.IsNullOrEmpty(scenePath))
        {
            return "Gameplay scene not found.";
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return "Scene load cancelled.";
        }

        EditorSceneManager.OpenScene(scenePath);
        EditorApplication.isPlaying = true;
        return string.Empty;
    }

    public static void RestartCurrentIfPlaying()
    {
        if (!EditorApplication.isPlaying)
        {
            return;
        }

        // Prefer re-arm Pending from Active and reload Gameplay (full clean restart).
        if (V1PlaytestOverride.HasActive)
        {
            V1PlaytestOverride.SetPendingFromActive();
            string scenePath = FindGameplayScenePath();
            if (string.IsNullOrEmpty(scenePath))
            {
                SceneManager.LoadScene(GameplaySceneName);
            }
            else
            {
                SceneManager.LoadScene(scenePath);
            }

            return;
        }

        LevelManager levelManager = Object.FindAnyObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.RestartLevel();
        }
    }

    public static string FindGameplayScenePath()
    {
        string[] guids = AssetDatabase.FindAssets(GameplaySceneName + " t:Scene");
        if (guids == null || guids.Length == 0)
        {
            return string.Empty;
        }

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (path.EndsWith("/Gameplay.unity") || path.EndsWith("\\Gameplay.unity"))
            {
                return path;
            }
        }

        return AssetDatabase.GUIDToAssetPath(guids[0]);
    }
}

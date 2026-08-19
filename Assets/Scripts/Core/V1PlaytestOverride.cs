using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor-only bridge: direct LevelData playtest without MainLevelDatabase.
/// Runtime-safe — all UnityEditor usage is behind UNITY_EDITOR.
/// SessionState keys survive scene reload within the Editor session.
/// </summary>
public static class V1PlaytestOverride
{
#if UNITY_EDITOR
    private const string PendingGuidKey = "RushOut_V1PlaytestPendingGuid";
    private const string ActiveGuidKey = "RushOut_V1PlaytestActiveGuid";
#endif

    /// <summary>
    /// True when a pending one-shot GUID is waiting for the next Gameplay Start.
    /// Always false in player builds.
    /// </summary>
    public static bool HasPending
    {
        get
        {
#if UNITY_EDITOR
            return !string.IsNullOrEmpty(SessionState.GetString(PendingGuidKey, string.Empty));
#else
            return false;
#endif
        }
    }

    /// <summary>
    /// True when an Active playtest GUID is set (current candidate session).
    /// Always false in player builds.
    /// </summary>
    public static bool HasActive
    {
        get
        {
#if UNITY_EDITOR
            return !string.IsNullOrEmpty(SessionState.GetString(ActiveGuidKey, string.Empty));
#else
            return false;
#endif
        }
    }

    /// <summary>
    /// Arm Pending + Active for the next Gameplay load (Editor only).
    /// </summary>
    public static void SetPendingLevel(LevelData level)
    {
#if UNITY_EDITOR
        if (level == null)
        {
            return;
        }

        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(level));
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogError("V1PlaytestOverride: LevelData has no asset GUID: " + level.name);
            return;
        }

        SessionState.SetString(PendingGuidKey, guid);
        SessionState.SetString(ActiveGuidKey, guid);
#endif
    }

    /// <summary>
    /// Re-arm Pending from Active (Restart Current).
    /// </summary>
    public static void SetPendingFromActive()
    {
#if UNITY_EDITOR
        string active = SessionState.GetString(ActiveGuidKey, string.Empty);
        if (string.IsNullOrEmpty(active))
        {
            return;
        }

        SessionState.SetString(PendingGuidKey, active);
#endif
    }

    /// <summary>
    /// Consume Pending GUID once and resolve to LevelData. Clears Pending only.
    /// Active remains for Restart / ResolveCurrentLevel.
    /// </summary>
    public static bool TryConsumePendingLevel(out LevelData level)
    {
        level = null;
#if UNITY_EDITOR
        string guid = SessionState.GetString(PendingGuidKey, string.Empty);
        if (string.IsNullOrEmpty(guid))
        {
            return false;
        }

        SessionState.EraseString(PendingGuidKey);
        level = LoadLevelByGuid(guid);
        if (level == null)
        {
            Debug.LogError("V1PlaytestOverride: pending GUID not resolvable: " + guid);
            return false;
        }

        // Keep Active in sync with what we just loaded.
        SessionState.SetString(ActiveGuidKey, guid);
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    /// Resolve Active GUID without consuming Pending.
    /// </summary>
    public static bool TryGetActiveLevel(out LevelData level)
    {
        level = null;
#if UNITY_EDITOR
        string guid = SessionState.GetString(ActiveGuidKey, string.Empty);
        if (string.IsNullOrEmpty(guid))
        {
            return false;
        }

        level = LoadLevelByGuid(guid);
        return level != null;
#else
        return false;
#endif
    }

    /// <summary>
    /// Active GUID string (empty if none). Editor only.
    /// </summary>
    public static string GetActiveGuid()
    {
#if UNITY_EDITOR
        return SessionState.GetString(ActiveGuidKey, string.Empty);
#else
        return string.Empty;
#endif
    }

    /// <summary>
    /// Clear Pending + Active (Stop Playtest / Clear Override menu).
    /// </summary>
    public static void Clear()
    {
#if UNITY_EDITOR
        SessionState.EraseString(PendingGuidKey);
        SessionState.EraseString(ActiveGuidKey);
#endif
    }

#if UNITY_EDITOR
    private static LevelData LoadLevelByGuid(string guid)
    {
        if (string.IsNullOrEmpty(guid))
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<LevelData>(path);
    }
#endif
}

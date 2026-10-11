using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor-only local Daily preview. SessionState only — never PlayerPrefs,
/// DailyChallengeContext, or the backend.
/// Player builds always see this as inactive.
/// </summary>
public static class DailyChallengeEditorPreview
{
#if UNITY_EDITOR
    private const string PendingGuidKey = "RushOut_DailyEditorPreviewPendingGuid";
    private const string ActiveGuidKey = "RushOut_DailyEditorPreviewActiveGuid";
    private const string SuppressRecoverKey = "RushOut_DailyEditorPreviewSuppressRecover";

    static DailyChallengeEditorPreview()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingPlayMode)
        {
            return;
        }

        SessionState.EraseString(PendingGuidKey);
        SessionState.EraseString(ActiveGuidKey);
        SessionState.EraseString(SuppressRecoverKey);
    }
#endif

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

    /// <summary>True for the Gameplay session loaded from a preview request.</summary>
    public static bool IsActive
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

    public static void SetPending(LevelData level)
    {
#if UNITY_EDITOR
        if (level == null)
        {
            return;
        }

        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(level));
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogError("[DailyPreview] LevelData has no asset GUID: " + level.name);
            return;
        }

        SessionState.SetString(PendingGuidKey, guid);
        SessionState.SetString(ActiveGuidKey, guid);
        Debug.LogWarning(
            "[DailyPreview] LOCAL DAILY PREVIEW of " + level.name +
            ". This will not submit scores, start an attempt, or change today's Daily challenge.");
#else
        _ = level;
#endif
    }

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
        string path = AssetDatabase.GUIDToAssetPath(guid);
        level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        if (level == null)
        {
            SessionState.EraseString(ActiveGuidKey);
            Debug.LogError("[DailyPreview] Pending GUID was not a LevelData asset: " + guid);
            return false;
        }

        SessionState.SetString(ActiveGuidKey, guid);
        return true;
#else
        return false;
#endif
    }

    public static void Clear()
    {
#if UNITY_EDITOR
        SessionState.EraseString(PendingGuidKey);
        SessionState.EraseString(ActiveGuidKey);
#endif
    }

    /// <summary>
    /// Ends the preview and tells the next stale-attempt recover to leave the real Daily cache alone.
    /// </summary>
    public static void ClearWithoutTouchingDailyAttempt()
    {
#if UNITY_EDITOR
        SessionState.EraseString(PendingGuidKey);
        SessionState.EraseString(ActiveGuidKey);
        SessionState.SetString(SuppressRecoverKey, "1");
#else
        Clear();
#endif
    }

    /// <summary>One-shot. True when a preview just exited and recover must not rewrite today's attempt.</summary>
    public static bool ConsumeSuppressStaleRecover()
    {
#if UNITY_EDITOR
        if (SessionState.GetString(SuppressRecoverKey, string.Empty) != "1")
        {
            return false;
        }

        SessionState.EraseString(SuppressRecoverKey);
        return true;
#else
        return false;
#endif
    }
}

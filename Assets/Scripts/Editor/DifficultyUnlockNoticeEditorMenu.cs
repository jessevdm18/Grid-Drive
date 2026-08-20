using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tools for Difficulty Unlock notification presentation / prefs.
/// Does not change progression thresholds or completion data.
/// </summary>
public static class DifficultyUnlockNoticeEditorMenu
{
    [MenuItem("RushOut/Testing/Reset Medium Unlock Notice")]
    public static void ResetMediumUnlockNotice()
    {
        DifficultyUnlockNoticePrefs.ResetMedium();
        Debug.Log(
            "[DifficultyUnlock] Reset " + DifficultyUnlockNoticePrefs.MediumKey
        );
    }

    [MenuItem("RushOut/Testing/Reset Hard Unlock Notice")]
    public static void ResetHardUnlockNotice()
    {
        DifficultyUnlockNoticePrefs.ResetHard();
        Debug.Log(
            "[DifficultyUnlock] Reset " + DifficultyUnlockNoticePrefs.HardKey
        );
    }

    [MenuItem("RushOut/Testing/Show Medium Unlock Notification Now")]
    public static void ShowMediumUnlockNotificationNow()
    {
        ShowNow(LevelDifficulty.Medium);
    }

    [MenuItem("RushOut/Testing/Show Hard Unlock Notification Now")]
    public static void ShowHardUnlockNotificationNow()
    {
        ShowNow(LevelDifficulty.Hard);
    }

    private static void ShowNow(LevelDifficulty difficulty)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "[DifficultyUnlock] Show Notification Now requires Play Mode."
            );
            return;
        }

        DifficultyUnlockNotificationUI ui =
            Object.FindAnyObjectByType<DifficultyUnlockNotificationUI>(
                FindObjectsInactive.Include
            );

        if (ui == null)
        {
            Debug.LogError(
                "[DifficultyUnlock] No DifficultyUnlockNotificationUI in scene. " +
                "Wire under GameCanvas and assign on UIManager."
            );
            return;
        }

        if (!ui.isActiveAndEnabled)
        {
            ui.gameObject.SetActive(true);
        }

        Debug.Log(
            "[DifficultyUnlock] Editor preview opened: " + difficulty +
            " — click Stay/Try (prefs not marked)."
        );
        ui.PlayNow(difficulty);
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shared Daily Challenge presentation helpers for Pause / failure / win UI.
/// Hides Restart/Retry; rebinds Levels/Menu to safe Daily exit → MainMenu.
/// </summary>
public static class DailyChallengeUiGuard
{
    public static bool IsDailyActive
    {
        get
        {
            LevelManager lm = Object.FindFirstObjectByType<LevelManager>();
            if (lm != null && lm.IsDailyChallengeSession)
            {
                return true;
            }

            return DailyChallengeContext.IsActiveSession;
        }
    }

    /// <summary>
    /// Hide restart/retry. Optionally rebind exit button to MainMenu abandon.
    /// </summary>
    public static void ApplyFailureOrPausePolicy(
        Button restartOrRetryButton,
        Button exitButton,
        string abandonReason)
    {
        if (!IsDailyActive)
        {
            return;
        }

        if (restartOrRetryButton != null)
        {
            restartOrRetryButton.gameObject.SetActive(false);
        }

        if (exitButton != null)
        {
            FailureUiButtonBinding.BindExclusive(
                exitButton,
                () => ExitToMainMenu(abandonReason),
                "DailyChallenge.Exit");
        }
    }

    public static void ApplyWinPanelPolicy(Button restartButton, Button nextButton)
    {
        if (!IsDailyActive)
        {
            return;
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }

        if (nextButton != null)
        {
            FailureUiButtonBinding.BindExclusive(
                nextButton,
                () => ExitToMainMenu("daily_win_continue"),
                "DailyChallenge.WinContinue");
        }
    }

    private static GameObject hintButton;
    private static GameObject rewardedHintButton;
    private static GameObject undoButton;
    private static GameObject hintStatusText;

    /// <summary>
    /// Daily hides authored Hint and Undo. Non-Daily shows them again.
    /// HintStatusText stays under HintManager (hidden until a status message).
    /// There is no separate Redo control — UndoButton is the move-reversal button.
    /// </summary>
    public static void ApplyAssistControls()
    {
        bool show = !IsDailyActive;
        SetNamed("HintButton", ref hintButton, show);
        SetNamed("RewardedHintButton", ref rewardedHintButton, show);
        SetNamed("UndoButton", ref undoButton, show);
        if (!show)
        {
            SetNamed("HintStatusText", ref hintStatusText, false);
        }
    }

    private static void SetNamed(string objectName, ref GameObject cached, bool active)
    {
        if (cached == null)
        {
            cached = FindNamed(objectName);
        }

        if (cached != null && cached.activeSelf != active)
        {
            cached.SetActive(active);
        }
    }

    private static GameObject FindNamed(string objectName)
    {
        GameObject activeGo = GameObject.Find(objectName);
        if (activeGo != null)
        {
            return activeGo;
        }

        Transform[] transforms = Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform t = transforms[i];
            if (t != null && t.gameObject.scene.IsValid() && t.name == objectName)
            {
                return t.gameObject;
            }
        }

        return null;
    }

    public static void ExitToMainMenu(string reason)
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        if (DailyChallengeEditorPreview.IsActive)
        {
            DailyChallengeEditorPreview.ClearWithoutTouchingDailyAttempt();
            SceneTransition.LoadScene("MainMenu");
            return;
        }
#endif
        if (reason == "daily_win_continue")
        {
            // Completion already persisted in CompleteLevel.
            DailyChallengeContext.ClearSession();
        }
        else
        {
            DailyChallengeManager.EnsureInstance()?.NotifyAbandoned(reason);
        }

        SceneTransition.LoadScene("MainMenu");
    }
}

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

    public static void ExitToMainMenu(string reason)
    {
        Time.timeScale = 1f;
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

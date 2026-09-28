using TMPro;
using UnityEngine;

/// <summary>
/// Optional authored Daily Challenge Gameplay HUD badge (DAILY CHALLENGE / ONE ATTEMPT).
/// Host GameObject should start active so Start runs; non-Daily sessions hide it.
/// </summary>
public class DailyChallengeGameplayHud : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;

    private void Start()
    {
        bool daily = DailyChallengeGameplayPolicy.IsActive;
        GameObject visual = root != null ? root : gameObject;

        if (!daily)
        {
            visual.SetActive(false);
            return;
        }

        if (!visual.activeSelf)
        {
            visual.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = "DAILY CHALLENGE";
        }

        if (subtitleText != null)
        {
            subtitleText.text = "ONE ATTEMPT";
        }

        DailyChallengeRunTracker.EnsureInScene();
    }
}

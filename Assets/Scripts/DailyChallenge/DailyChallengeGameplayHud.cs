using TMPro;
using UnityEngine;

/// <summary>
/// Optional authored Daily Challenge Gameplay HUD badge (DAILY CHALLENGE / ONE ATTEMPT).
/// Activates only during Daily sessions.
/// </summary>
public class DailyChallengeGameplayHud : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;

    private void Start()
    {
        bool daily = DailyChallengeGameplayPolicy.IsActive;
        if (root != null)
        {
            root.SetActive(daily);
        }

        if (!daily)
        {
            return;
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

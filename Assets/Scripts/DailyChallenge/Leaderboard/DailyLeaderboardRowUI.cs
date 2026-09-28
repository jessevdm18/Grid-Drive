using TMPro;
using UnityEngine;

/// <summary>
/// Authored reusable leaderboard row. Scene/prefab owns styling; runtime fills values.
/// </summary>
public class DailyLeaderboardRowUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI secondaryText;
    [SerializeField] private GameObject highlightRoot;
    [SerializeField] private GameObject simulatedMarkerRoot;

    public void Bind(DailyLeaderboardEntry entry, bool showSimulatedMarker)
    {
        if (rankText != null)
        {
            rankText.text = "#" + entry.Rank;
        }

        if (nameText != null)
        {
            nameText.text = entry.DisplayName ?? string.Empty;
        }

        if (scoreText != null)
        {
            scoreText.text = DailyChallengeTimeFormat.FormatScore(entry.Score);
        }

        if (secondaryText != null)
        {
            secondaryText.text =
                entry.Moves + " MOVES • " +
                DailyChallengeTimeFormat.Format(entry.CompletionTimeMilliseconds);
        }

        if (highlightRoot != null)
        {
            highlightRoot.SetActive(entry.IsCurrentPlayer);
        }

        if (simulatedMarkerRoot != null)
        {
            simulatedMarkerRoot.SetActive(showSimulatedMarker && entry.IsSimulated);
        }
    }
}

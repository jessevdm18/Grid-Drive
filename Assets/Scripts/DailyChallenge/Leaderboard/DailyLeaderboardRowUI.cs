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
        Bind(entry, showSimulatedMarker, null);
    }

    public void Bind(
        DailyLeaderboardEntry entry,
        bool showSimulatedMarker,
        DailyChallengeResultSettings podium)
    {
        if (rankText != null)
        {
            rankText.text = "#" + entry.Rank;
            rankText.color = ColorForRank(entry.Rank, podium);
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

    private static Color ColorForRank(int rank, DailyChallengeResultSettings podium)
    {
        DailyChallengeResultSettings colors =
            podium != null ? podium : DailyChallengeResultSettings.LoadDefault();
        if (rank == 1)
        {
            return colors.firstPlaceColor;
        }

        if (rank == 2)
        {
            return colors.secondPlaceColor;
        }

        if (rank == 3)
        {
            return colors.thirdPlaceColor;
        }

        return colors.normalColor;
    }
}

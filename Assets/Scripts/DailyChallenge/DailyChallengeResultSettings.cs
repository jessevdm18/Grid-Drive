using UnityEngine;

/// <summary>
/// Inspector copy and podium colors for Daily result presentation.
/// Reward amounts, ranks, and settlement stay on the server.
/// </summary>
[CreateAssetMenu(
    fileName = "DailyChallengeResultSettings",
    menuName = "RushOut/Daily Challenge Result Settings")]
public class DailyChallengeResultSettings : ScriptableObject
{
    [Header("Result copy")]
    public string resultTitle = "DAILY CHALLENGE RESULTS";
    public string rankLabel = "FINAL RANK";
    public string scoreLabel = "SCORE";
    public string movesLabel = "MOVES";
    public string timeLabel = "TIME";
    public string rewardLabel = "REWARD";
    public string coinsSuffix = "COINS";
    public string claimButtonLabel = "CLAIM";
    public string claimingButtonLabel = "CLAIMING...";
    public string okButtonLabel = "OK";

    [Header("Podium colors")]
    public Color firstPlaceColor = new Color(1f, 0.84f, 0.2f, 1f);
    public Color secondPlaceColor = new Color(0.78f, 0.8f, 0.84f, 1f);
    public Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.28f, 1f);
    public Color normalColor = Color.white;

    public static DailyChallengeResultSettings LoadDefault()
    {
        DailyChallengeResultSettings settings =
            Resources.Load<DailyChallengeResultSettings>("DailyChallengeResultSettings");
        return settings != null ? settings : CreateInstance<DailyChallengeResultSettings>();
    }
}

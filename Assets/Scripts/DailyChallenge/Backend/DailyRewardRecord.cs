/// <summary>Server-owned Daily #1 reward (Phase 4.3). No provider IDs.</summary>
public readonly struct DailyRewardRecord
{
    public string DayId { get; }
    public int Rank { get; }
    public string RewardType { get; }
    public int RewardAmount { get; }
    public bool Claimed { get; }
    public string Source { get; }
    public bool IsDevelopmentTest { get; }
    public int Score { get; }
    public int Moves { get; }
    public long CompletionTimeMilliseconds { get; }

    public DailyRewardRecord(
        string dayId,
        int rank,
        string rewardType,
        int rewardAmount,
        bool claimed,
        string source,
        int score,
        int moves,
        long completionTimeMilliseconds,
        bool isDevelopmentTest = false)
    {
        DayId = dayId ?? string.Empty;
        Rank = rank;
        RewardType = rewardType ?? string.Empty;
        RewardAmount = rewardAmount;
        Claimed = claimed;
        Source = source ?? string.Empty;
        IsDevelopmentTest = isDevelopmentTest;
        Score = score;
        Moves = moves;
        CompletionTimeMilliseconds = completionTimeMilliseconds;
    }

    public static DailyRewardRecord PreviewFirstPlace(string dayId, int amount = 1000)
    {
        return new DailyRewardRecord(
            dayId,
            rank: 1,
            rewardType: "coins",
            rewardAmount: amount,
            claimed: false,
            source: "daily_first_place",
            score: 0,
            moves: 0,
            completionTimeMilliseconds: 0,
            isDevelopmentTest: true);
    }
}

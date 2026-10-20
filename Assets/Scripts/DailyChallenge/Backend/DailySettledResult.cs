/// <summary>
/// Server-authoritative final Daily placement after the UTC day has settled.
/// Prize coins stay on <see cref="DailyRewardRecord"/>. Ranks outside #1–#3 have no reward.
/// </summary>
public readonly struct DailySettledResult
{
    public bool Found { get; }
    public bool Settled { get; }
    public string DayId { get; }
    public int Rank { get; }
    public int Score { get; }
    public int Moves { get; }
    public long CompletionTimeMilliseconds { get; }
    public bool HasUnclaimedReward { get; }
    public int RewardAmount { get; }

    public DailySettledResult(
        bool found,
        bool settled,
        string dayId,
        int rank,
        int score,
        int moves,
        long completionTimeMilliseconds,
        bool hasUnclaimedReward,
        int rewardAmount)
    {
        Found = found;
        Settled = settled;
        DayId = dayId ?? string.Empty;
        Rank = rank;
        Score = score;
        Moves = moves;
        CompletionTimeMilliseconds = completionTimeMilliseconds;
        HasUnclaimedReward = hasUnclaimedReward;
        RewardAmount = rewardAmount;
    }

    public static DailySettledResult None => default;
}

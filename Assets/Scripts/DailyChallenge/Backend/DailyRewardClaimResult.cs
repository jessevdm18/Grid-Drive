/// <summary>Result of claimDailyReward callable.</summary>
public readonly struct DailyRewardClaimResult
{
    public bool Success { get; }
    public bool Granted { get; }
    public bool AlreadyClaimed { get; }
    public string DayId { get; }
    public string RewardType { get; }
    public int RewardAmount { get; }
    public int Rank { get; }
    public bool IsDevelopmentTest { get; }
    public string ErrorCode { get; }
    public string ErrorMessage { get; }

    public DailyRewardClaimResult(
        bool success,
        bool granted,
        bool alreadyClaimed,
        string dayId,
        string rewardType,
        int rewardAmount,
        int rank,
        string errorCode,
        string errorMessage,
        bool isDevelopmentTest = false)
    {
        Success = success;
        Granted = granted;
        AlreadyClaimed = alreadyClaimed;
        DayId = dayId ?? string.Empty;
        RewardType = rewardType ?? string.Empty;
        RewardAmount = rewardAmount;
        Rank = rank;
        IsDevelopmentTest = isDevelopmentTest;
        ErrorCode = errorCode ?? string.Empty;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    public static DailyRewardClaimResult Fail(string errorCode, string message = null)
    {
        return new DailyRewardClaimResult(
            success: false,
            granted: false,
            alreadyClaimed: false,
            dayId: string.Empty,
            rewardType: string.Empty,
            rewardAmount: 0,
            rank: 0,
            errorCode: errorCode,
            errorMessage: message);
    }
}

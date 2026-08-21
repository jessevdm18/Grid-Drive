/// <summary>
/// Authoritative 3★ first-claim coin amounts by <see cref="LevelDifficulty"/>.
/// Claim eligibility stays per-level in SaveManager — this only sets the amount.
/// </summary>
public static class ThreeStarCoinReward
{
    public const int Easy = 10;
    public const int Medium = 30;
    public const int Hard = 50;

    /// <summary>
    /// Coins granted on first 3★ clear for a level of this difficulty.
    /// </summary>
    public static int GetReward(LevelDifficulty difficulty)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Medium:
                return Medium;
            case LevelDifficulty.Hard:
                return Hard;
            case LevelDifficulty.Easy:
            default:
                return Easy;
        }
    }
}

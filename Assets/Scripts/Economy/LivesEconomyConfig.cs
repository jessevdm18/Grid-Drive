/// <summary>
/// Authoritative lives↔economy constants. UI and purchase logic must both read from here.
/// </summary>
public static class LivesEconomyConfig
{
    /// <summary>Coin cost to purchase exactly +1 life (never exceeds MaxLives).</summary>
    public const int LifeCoinCost = 100;

    /// <summary>
    /// Coin cost to restart an ACTIVE (non-failed) attempt.
    /// Failed-attempt Retry is free — the life was already consumed by FailLevel.
    /// </summary>
    public const int RestartCoinCost = 25;
}

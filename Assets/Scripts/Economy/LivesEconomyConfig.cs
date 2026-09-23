/// <summary>
/// Authoritative lives↔economy constants. UI and purchase logic must both read from here.
/// </summary>
public static class LivesEconomyConfig
{
    /// <summary>Coin cost to purchase exactly +1 life (never exceeds MaxLives).</summary>
    public const int LifeCoinCost = 100;
}

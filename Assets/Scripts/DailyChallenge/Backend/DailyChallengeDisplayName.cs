/// <summary>
/// Privacy-safe temporary display names for Global leaderboard V1.
/// Not a security identity — never show full Firebase UID.
/// </summary>
public static class DailyChallengeDisplayName
{
    public static string FromUserId(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return "Player";
        }

        uint hash = DailyChallengeHash.StableHash32(userId);
        string hex = (hash & 0xFFFFu).ToString("X4");
        return "Player " + hex;
    }
}

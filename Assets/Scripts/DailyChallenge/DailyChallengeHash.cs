/// <summary>
/// Stable 32-bit hash for Daily Challenge day → pool index.
/// Independent of string.GetHashCode() (which is not stable across runtimes).
/// FNV-1a 32-bit over UTF-16 code units.
/// </summary>
public static class DailyChallengeHash
{
    private const uint FnvOffset = 2166136261u;
    private const uint FnvPrime = 16777619u;

    public static uint StableHash32(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return FnvOffset;
        }

        uint hash = FnvOffset;
        for (int i = 0; i < value.Length; i++)
        {
            hash ^= value[i];
            hash *= FnvPrime;
        }

        return hash;
    }
}

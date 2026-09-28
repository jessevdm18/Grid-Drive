/// <summary>
/// Local deterministic PRNG (xorshift32). Does not touch UnityEngine.Random.
/// </summary>
public struct DailyChallengeDeterministicRng
{
    private uint state;

    public DailyChallengeDeterministicRng(uint seed)
    {
        state = seed == 0u ? 0xA341316Cu : seed;
    }

    public uint NextUInt()
    {
        uint x = state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        state = x;
        return x;
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
        {
            return minInclusive;
        }

        uint span = (uint)(maxExclusive - minInclusive);
        return minInclusive + (int)(NextUInt() % span);
    }

    public float NextFloat01()
    {
        return (NextUInt() & 0x00FFFFFFu) / 16777216f;
    }
}

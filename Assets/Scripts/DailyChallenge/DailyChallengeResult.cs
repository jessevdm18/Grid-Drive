using System;

/// <summary>
/// Immutable Daily Challenge result for a completed (solved) run.
/// Local persistence is Phase 1/2 development authority only — not anti-cheat.
/// </summary>
[Serializable]
public struct DailyChallengeResult
{
    public string DayId;
    public string LevelAssetName;
    public int PoolIndex;
    public int Moves;
    public long CompletionTimeMilliseconds;
    public int Score;
    public bool Completed;

    public static DailyChallengeResult CreateAbandoned(string dayId, string levelAsset)
    {
        return new DailyChallengeResult
        {
            DayId = dayId ?? string.Empty,
            LevelAssetName = levelAsset ?? string.Empty,
            PoolIndex = -1,
            Moves = 0,
            CompletionTimeMilliseconds = 0,
            Score = 0,
            Completed = false
        };
    }
}

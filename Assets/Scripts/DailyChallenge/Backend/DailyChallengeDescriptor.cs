using System;

/// <summary>Authoritative Daily challenge definition for one UTC day.</summary>
[Serializable]
public struct DailyChallengeDescriptor
{
    public string DayId;
    public string LevelId;
    public string LevelVersion;
    public int ScoreVersion;
    public string OpensAtUtc;
    public string ClosesAtUtc;

    public bool IsValid =>
        !string.IsNullOrEmpty(DayId) && !string.IsNullOrEmpty(LevelId);
}

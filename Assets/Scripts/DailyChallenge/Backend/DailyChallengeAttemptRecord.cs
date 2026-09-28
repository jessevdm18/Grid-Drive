using System;

/// <summary>Authoritative per-user attempt for a Daily challenge day.</summary>
[Serializable]
public struct DailyChallengeAttemptRecord
{
    public string DayId;
    public string UserId;
    public string LevelId;
    public DailyChallengeServerAttemptState State;
    public string StartedAtServer;
    public string CompletedAtServer;
    public int Moves;
    public long CompletionTimeMs;
    public int Score;
    public int ScoreVersion;

    /// <summary>True when score was computed by Cloud Functions (Phase 4.1B+).</summary>
    public bool ScoreTrusted;

    public bool Exists => State != DailyChallengeServerAttemptState.None;
}

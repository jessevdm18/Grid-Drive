/// <summary>
/// Lifecycle of today's Daily Challenge attempt.
/// Once past Available for a UTC day, the attempt can never become Available again that day.
/// </summary>
public enum DailyChallengeState
{
    /// <summary>Today's challenge has not been started.</summary>
    Available = 0,

    /// <summary>Attempt committed; Gameplay in progress (or mid scene-transition).</summary>
    InProgress = 1,

    /// <summary>Player finished the daily level successfully.</summary>
    Completed = 2,

    /// <summary>Failed in-level, abandoned (pause→menu), or force-closed while InProgress.</summary>
    FailedOrAbandoned = 3
}

using UnityEngine;

/// <summary>
/// Shared urgency bands for move-limit HUD (special MoveLimit + global forgiving card).
/// Thresholds for normal/global campaign warning: warning at &lt;=5, critical at &lt;=3.
/// </summary>
public static class MoveLimitUrgency
{
    public const int WarningRemaining = 5;
    public const int CriticalRemaining = 3;

    public enum Band
    {
        Normal = 0,
        Warning = 1,
        Critical = 2
    }

    public static Band Evaluate(int remaining)
    {
        if (remaining <= CriticalRemaining)
        {
            return Band.Critical;
        }

        if (remaining <= WarningRemaining)
        {
            return Band.Warning;
        }

        return Band.Normal;
    }

    public static Color ColorFor(
        Band band,
        Color normal,
        Color warning,
        Color critical)
    {
        switch (band)
        {
            case Band.Critical:
                return critical;
            case Band.Warning:
                return warning;
            default:
                return normal;
        }
    }
}

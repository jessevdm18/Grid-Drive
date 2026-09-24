using System;
using System.Text;

/// <summary>
/// Formats Daily Challenge elapsed time for UI (integer milliseconds → display).
/// </summary>
public static class DailyChallengeTimeFormat
{
    /// <summary>
    /// mm:ss.cs for under 1 hour; h:mm:ss.cs when >= 1 hour.
    /// </summary>
    public static string Format(long milliseconds)
    {
        if (milliseconds < 0)
        {
            milliseconds = 0;
        }

        long totalCs = milliseconds / 10; // centiseconds
        long cs = totalCs % 100;
        long totalSeconds = totalCs / 100;
        long seconds = totalSeconds % 60;
        long totalMinutes = totalSeconds / 60;
        long minutes = totalMinutes % 60;
        long hours = totalMinutes / 60;

        if (hours > 0)
        {
            return string.Format(
                "{0}:{1:00}:{2:00}.{3:00}",
                hours,
                minutes,
                seconds,
                cs);
        }

        return string.Format(
            "{0:00}:{1:00}.{2:00}",
            minutes,
            seconds,
            cs);
    }

    public static string FormatScore(int score)
    {
        return score.ToString("N0");
    }
}

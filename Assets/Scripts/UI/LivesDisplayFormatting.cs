using System;

/// <summary>
/// Shared lives display formatting. Presentation only — no regeneration logic.
/// </summary>
public static class LivesDisplayFormatting
{
    /// <summary>
    /// >= 1 hour → H:MM:SS; otherwise MM:SS. Never negative.
    /// </summary>
    public static string FormatCountdown(TimeSpan remaining)
    {
        if (remaining < TimeSpan.Zero)
        {
            remaining = TimeSpan.Zero;
        }

        int totalSeconds = (int)Math.Floor(remaining.TotalSeconds);
        if (totalSeconds < 0)
        {
            totalSeconds = 0;
        }

        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        if (hours >= 1)
        {
            return hours + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
        }

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public static string FormatLivesFraction(int current, int max)
    {
        return current + " / " + max;
    }
}

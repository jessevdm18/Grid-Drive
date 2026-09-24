using System;

/// <summary>
/// Single UTC time authority for Daily Challenge.
/// TEMPORARY DEVELOPMENT: uses device <see cref="DateTime.UtcNow"/>.
/// Later phases will replace this with server UTC — do not call DateTime.UtcNow
/// from Daily Challenge UI/domain code; always go through this clock.
/// </summary>
public static class DailyChallengeClock
{
#if UNITY_EDITOR
    private static DateTime? editorSimulatedUtc;
#endif

    /// <summary>
    /// Current UTC instant. Device clock is Phase 1 development authority only.
    /// </summary>
    public static DateTime UtcNow
    {
        get
        {
#if UNITY_EDITOR
            if (editorSimulatedUtc.HasValue)
            {
                return editorSimulatedUtc.Value.Kind == DateTimeKind.Utc
                    ? editorSimulatedUtc.Value
                    : editorSimulatedUtc.Value.ToUniversalTime();
            }
#endif
            return DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Deterministic UTC day id, e.g. "2026-09-24".
    /// </summary>
    public static string GetCurrentUtcDayId()
    {
        return FormatDayId(UtcNow);
    }

    /// <summary>
    /// Formats an arbitrary UTC timestamp as a day id.
    /// </summary>
    public static string FormatDayId(DateTime utc)
    {
        DateTime day = utc.Kind == DateTimeKind.Utc
            ? utc.Date
            : utc.ToUniversalTime().Date;
        return day.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Next UTC midnight after <see cref="UtcNow"/> (exclusive end of current day).
    /// </summary>
    public static DateTime GetNextUtcMidnight()
    {
        return UtcNow.Date.AddDays(1);
    }

    /// <summary>
    /// Remaining time until next UTC midnight. Never negative.
    /// </summary>
    public static TimeSpan GetTimeUntilNextUtcDay()
    {
        TimeSpan remaining = GetNextUtcMidnight() - UtcNow;
        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

#if UNITY_EDITOR
    /// <summary>Editor testing: override UTC now. Null = real clock.</summary>
    public static void EditorSetSimulatedUtc(DateTime? utc)
    {
        editorSimulatedUtc = utc;
    }

    /// <summary>Editor testing: clear simulated clock.</summary>
    public static void EditorClearSimulatedUtc()
    {
        editorSimulatedUtc = null;
    }
#endif
}

using System;
using UnityEngine;

/// <summary>
/// Quiet hours and spacing for local return notifications (Lives Full and Free Coins).
/// Daily Challenge push is sent by the server at a fixed UTC time and is not adjusted here.
/// </summary>
public static class LocalNotificationPolicy
{
    public const string LivesDueUtcTicksKey = "RushOut_LivesNotificationDueUtcTicks";
    public const string FreeCoinsDueUtcTicksKey = "RushOut_FreeCoinNotificationDueUtcTicks";

    /// <summary>Local 21:00 through just before 09:05. Release is 09:05 local.</summary>
    public static readonly TimeSpan QuietStart = TimeSpan.FromHours(21);

    public static readonly TimeSpan QuietRelease = new TimeSpan(9, 5, 0);
    public static readonly TimeSpan MinimumSpacing = TimeSpan.FromHours(2);

    public static DateTime DelayForQuietHours(DateTime dueUtc)
    {
        DateTime utc = SpecifyUtc(dueUtc);
        DateTime local = utc.ToLocalTime();
        TimeSpan timeOfDay = local.TimeOfDay;
        if (timeOfDay >= QuietStart)
        {
            return ToUtc(local.Date.AddDays(1).Add(QuietRelease));
        }

        if (timeOfDay < QuietRelease)
        {
            return ToUtc(local.Date.Add(QuietRelease));
        }

        return utc;
    }

    /// <summary>
    /// Lives keeps its quiet-hours time. Free Coins moves to at least two hours after that delivery.
    /// </summary>
    public static DateTime SpaceFreeCoinsAfterLives(DateTime quietAdjustedUtc)
    {
        DateTime deliver = SpecifyUtc(quietAdjustedUtc);
        if (!TryGetDueUtc(LivesDueUtcTicksKey, out DateTime livesDue))
        {
            return deliver;
        }

        if ((deliver - livesDue).Duration() >= MinimumSpacing)
        {
            return deliver;
        }

        deliver = DelayForQuietHours(livesDue.Add(MinimumSpacing));
        if ((deliver - livesDue).Duration() < MinimumSpacing)
        {
            deliver = DelayForQuietHours(deliver.Add(MinimumSpacing));
        }

        return deliver;
    }

    public static void SetDueUtc(string key, DateTime dueUtc)
    {
        PlayerPrefs.SetString(
            key,
            SpecifyUtc(dueUtc).Ticks.ToString());
        PlayerPrefs.Save();
    }

    public static void ClearDue(string key)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            return;
        }

        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }

    public static bool TryGetDueUtc(string key, out DateTime dueUtc)
    {
        dueUtc = default;
        string raw = PlayerPrefs.GetString(key, string.Empty);
        if (!long.TryParse(raw, out long ticks) || ticks <= 0)
        {
            return false;
        }

        dueUtc = new DateTime(ticks, DateTimeKind.Utc);
        return true;
    }

    private static DateTime SpecifyUtc(DateTime value)
    {
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private static DateTime ToUtc(DateTime localUnspecified)
    {
        DateTime local = DateTime.SpecifyKind(localUnspecified, DateTimeKind.Local);
        return DateTime.SpecifyKind(local.ToUniversalTime(), DateTimeKind.Utc);
    }
}

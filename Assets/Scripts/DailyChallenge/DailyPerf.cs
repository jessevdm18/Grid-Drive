using Stopwatch = System.Diagnostics.Stopwatch;
using UnityEngine;

/// <summary>
/// Device timing for Daily start and result fetch. Logs phase names and durations only.
/// </summary>
public static class DailyPerf
{
    private static readonly Stopwatch Clock = Stopwatch.StartNew();
    private static long lastMarkMs;

    public static void Mark(string phase)
    {
        long elapsed = Clock.ElapsedMilliseconds;
        long step = elapsed - lastMarkMs;
        lastMarkMs = elapsed;
        Debug.Log("[DailyPerf] " + phase + " elapsedMs=" + elapsed + " stepMs=" + step);
    }
}

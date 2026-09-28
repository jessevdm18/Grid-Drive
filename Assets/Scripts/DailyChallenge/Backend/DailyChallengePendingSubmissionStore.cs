using UnityEngine;

/// <summary>
/// Local pending Daily result for offline completion retry (idempotent submit).
/// Stores raw run metrics. Local score is provisional UI only — server recalculates.
/// </summary>
public static class DailyChallengePendingSubmissionStore
{
    private const string DayKey = "RushOut_Daily_PendingSubmit_DayId";
    private const string LevelKey = "RushOut_Daily_PendingSubmit_LevelId";
    private const string MovesKey = "RushOut_Daily_PendingSubmit_Moves";
    private const string TimeKey = "RushOut_Daily_PendingSubmit_TimeMs";
    private const string ScoreKey = "RushOut_Daily_PendingSubmit_ProvisionalScore";
    private const string FlagKey = "RushOut_Daily_PendingSubmit_Flag";
    private const string ScoreVersionKey = "RushOut_Daily_PendingSubmit_ScoreVersion";

    public static void Save(
        string dayId,
        string levelId,
        int moves,
        long completionTimeMs,
        int provisionalScore)
    {
        PlayerPrefs.SetString(DayKey, dayId ?? string.Empty);
        PlayerPrefs.SetString(LevelKey, levelId ?? string.Empty);
        PlayerPrefs.SetInt(MovesKey, moves);
        PlayerPrefs.SetString(TimeKey, completionTimeMs.ToString());
        PlayerPrefs.SetInt(ScoreKey, provisionalScore);
        PlayerPrefs.SetInt(ScoreVersionKey, DailyChallengeScoreVersion.Current);
        PlayerPrefs.SetInt(FlagKey, 1);
        PlayerPrefs.Save();
    }

    public static bool TryLoad(
        out string dayId,
        out string levelId,
        out int moves,
        out long completionTimeMs,
        out int provisionalScore)
    {
        dayId = PlayerPrefs.GetString(DayKey, string.Empty);
        levelId = PlayerPrefs.GetString(LevelKey, string.Empty);
        moves = PlayerPrefs.GetInt(MovesKey, 0);
        provisionalScore = PlayerPrefs.GetInt(ScoreKey, 0);
        string msRaw = PlayerPrefs.GetString(TimeKey, "0");
        if (!long.TryParse(msRaw, out completionTimeMs))
        {
            completionTimeMs = 0;
        }

        return PlayerPrefs.GetInt(FlagKey, 0) == 1 &&
               !string.IsNullOrEmpty(dayId) &&
               !string.IsNullOrEmpty(levelId) &&
               moves > 0 &&
               completionTimeMs > 0;
    }

    public static bool HasPending => PlayerPrefs.GetInt(FlagKey, 0) == 1;

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(DayKey);
        PlayerPrefs.DeleteKey(LevelKey);
        PlayerPrefs.DeleteKey(MovesKey);
        PlayerPrefs.DeleteKey(TimeKey);
        PlayerPrefs.DeleteKey(ScoreKey);
        PlayerPrefs.DeleteKey(ScoreVersionKey);
        PlayerPrefs.DeleteKey(FlagKey);
        PlayerPrefs.Save();
    }
}

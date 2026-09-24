using UnityEngine;

/// <summary>
/// Phase 1/2 DEVELOPMENT persistence for Daily Challenge attempt + completed result.
/// NOT secure. Backend will become authoritative later.
/// </summary>
public static class DailyChallengeLocalStore
{
    public const string DayIdKey = "RushOut_DailyChallenge_DayId";
    public const string StateKey = "RushOut_DailyChallenge_State";
    public const string LevelAssetKey = "RushOut_DailyChallenge_LevelAsset";
    public const string PoolIndexKey = "RushOut_DailyChallenge_PoolIndex";

    public const string ResultMovesKey = "RushOut_DailyChallenge_ResultMoves";
    public const string ResultTimeMsKey = "RushOut_DailyChallenge_ResultTimeMs";
    public const string ResultScoreKey = "RushOut_DailyChallenge_ResultScore";
    public const string ResultCompletedKey = "RushOut_DailyChallenge_ResultCompleted";

    public static void Save(
        string utcDayId,
        DailyChallengeState state,
        string levelAssetName,
        int poolIndex)
    {
        PlayerPrefs.SetString(DayIdKey, utcDayId ?? string.Empty);
        PlayerPrefs.SetInt(StateKey, (int)state);
        PlayerPrefs.SetString(LevelAssetKey, levelAssetName ?? string.Empty);
        PlayerPrefs.SetInt(PoolIndexKey, poolIndex);
        PlayerPrefs.Save();
    }

    public static void SaveResult(DailyChallengeResult result)
    {
        PlayerPrefs.SetString(DayIdKey, result.DayId ?? string.Empty);
        PlayerPrefs.SetString(LevelAssetKey, result.LevelAssetName ?? string.Empty);
        PlayerPrefs.SetInt(PoolIndexKey, result.PoolIndex);
        PlayerPrefs.SetInt(ResultMovesKey, result.Moves);
        PlayerPrefs.SetString(ResultTimeMsKey, result.CompletionTimeMilliseconds.ToString());
        PlayerPrefs.SetInt(ResultScoreKey, result.Score);
        PlayerPrefs.SetInt(ResultCompletedKey, result.Completed ? 1 : 0);
        PlayerPrefs.SetInt(
            StateKey,
            (int)(result.Completed
                ? DailyChallengeState.Completed
                : DailyChallengeState.FailedOrAbandoned));
        PlayerPrefs.Save();
    }

    public static bool TryLoad(
        out string utcDayId,
        out DailyChallengeState state,
        out string levelAssetName,
        out int poolIndex)
    {
        utcDayId = PlayerPrefs.GetString(DayIdKey, string.Empty);
        levelAssetName = PlayerPrefs.GetString(LevelAssetKey, string.Empty);
        poolIndex = PlayerPrefs.GetInt(PoolIndexKey, -1);
        int raw = PlayerPrefs.GetInt(StateKey, (int)DailyChallengeState.Available);
        state = (DailyChallengeState)raw;

        return !string.IsNullOrEmpty(utcDayId);
    }

    public static bool TryLoadResult(out DailyChallengeResult result)
    {
        result = default;
        if (!TryLoad(out string dayId, out DailyChallengeState state, out string asset, out int pool))
        {
            return false;
        }

        if (state != DailyChallengeState.Completed)
        {
            return false;
        }

        string msRaw = PlayerPrefs.GetString(ResultTimeMsKey, "0");
        if (!long.TryParse(msRaw, out long ms))
        {
            ms = 0;
        }

        result = new DailyChallengeResult
        {
            DayId = dayId,
            LevelAssetName = asset,
            PoolIndex = pool,
            Moves = PlayerPrefs.GetInt(ResultMovesKey, 0),
            CompletionTimeMilliseconds = ms,
            Score = PlayerPrefs.GetInt(ResultScoreKey, 0),
            Completed = PlayerPrefs.GetInt(ResultCompletedKey, 0) == 1
        };
        return result.Completed;
    }

    public static void ClearResultFields()
    {
        PlayerPrefs.DeleteKey(ResultMovesKey);
        PlayerPrefs.DeleteKey(ResultTimeMsKey);
        PlayerPrefs.DeleteKey(ResultScoreKey);
        PlayerPrefs.DeleteKey(ResultCompletedKey);
        PlayerPrefs.Save();
    }

    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(DayIdKey);
        PlayerPrefs.DeleteKey(StateKey);
        PlayerPrefs.DeleteKey(LevelAssetKey);
        PlayerPrefs.DeleteKey(PoolIndexKey);
        ClearResultFields();
    }
}

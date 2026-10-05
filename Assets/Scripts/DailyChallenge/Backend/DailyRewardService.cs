using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Phase 4.3 client: fetch pending Daily #1 rewards and claim via Cloud Functions.
/// Coins are local PlayerPrefs — grant only after a successful server claim with granted=true.
/// Duplicate AlreadyClaimed must NOT grant coins again (V1 crash-window limitation documented).
/// Anonymous Firebase UIDs are eligible; Google Play link is not required.
/// </summary>
public static class DailyRewardService
{
    public const int FirstPlaceCoinReward = 1000;
    /// <summary>Server dayId for createDevDailyReward smoke tests (not a real UTC day).</summary>
    public const string DevRewardDayId = "dev-reward-smoke-test";
    private const string CreatedAnalyticsPrefsPrefix = "RushOut_DailyRewardCreatedLogged_";

    private static readonly List<DailyRewardRecord> cachedPending = new List<DailyRewardRecord>();
    private static bool syncInFlight;
    private static bool claimInFlight;

    public static IReadOnlyList<DailyRewardRecord> CachedPending => cachedPending;

    public static bool HasPendingReward
    {
        get
        {
            for (int i = 0; i < cachedPending.Count; i++)
            {
                if (!cachedPending[i].Claimed)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static bool TryGetFirstPending(out DailyRewardRecord reward)
    {
        for (int i = 0; i < cachedPending.Count; i++)
        {
            if (!cachedPending[i].Claimed)
            {
                reward = cachedPending[i];
                return true;
            }
        }

        reward = default;
        return false;
    }

    /// <summary>
    /// Sync pending rewards from server. No-op when offline / not signed in.
    /// </summary>
    public static async Task<bool> SyncPendingAsync()
    {
        if (syncInFlight)
        {
            return HasPendingReward;
        }

        syncInFlight = true;
        try
        {
            if (!await DailyChallengeIdentityService.EnsureSignedInAsync())
            {
                return false;
            }

            DailyChallengeFirebaseSettings settings = DailyChallengeFirebaseSettings.LoadDefault();
            if (settings == null || !settings.IsConfigured)
            {
                return false;
            }

            FirebaseRestClient.CallableResponse response = await CallWithAuthRetryAsync(
                "getPendingDailyRewards",
                "{}");
            if (!response.Ok || string.IsNullOrEmpty(response.ResultJson))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning(
                    "[DailyReward] getPendingDailyRewards failed status=" +
                    response.ErrorStatus);
#endif
                return false;
            }

            cachedPending.Clear();
            ParseRewardsArray(response.ResultJson, cachedPending);

            for (int i = 0; i < cachedPending.Count; i++)
            {
                MaybeLogCreatedAnalytics(cachedPending[i]);
            }

            return HasPendingReward;
        }
        finally
        {
            syncInFlight = false;
        }
    }

    /// <summary>
    /// Claim via server. Only when granted=true should the caller add local coins.
    /// AlreadyClaimed → success without a second grant.
    /// </summary>
    public static async Task<DailyRewardClaimResult> ClaimAsync(string dayId)
    {
        if (claimInFlight)
        {
            return DailyRewardClaimResult.Fail("busy", "Claim already in progress.");
        }

        if (string.IsNullOrEmpty(dayId))
        {
            return DailyRewardClaimResult.Fail("invalid_day", "Missing dayId.");
        }

        claimInFlight = true;
        try
        {
            if (!await DailyChallengeIdentityService.EnsureSignedInAsync())
            {
                return DailyRewardClaimResult.Fail("unauthenticated", "Not signed in.");
            }

            string dataJson = "{\"dayId\":\"" + Escape(dayId) + "\"}";
            FirebaseRestClient.CallableResponse response =
                await CallWithAuthRetryAsync("claimDailyReward", dataJson);
            if (!response.Ok || string.IsNullOrEmpty(response.ResultJson))
            {
                return DailyRewardClaimResult.Fail(
                    string.IsNullOrEmpty(response.ErrorStatus)
                        ? "network"
                        : response.ErrorStatus,
                    "Claim failed.");
            }

            DailyRewardClaimResult parsed = ParseClaimResult(response.ResultJson);
            if (parsed.Success)
            {
                RemovePending(dayId);
                // Suppress production claim analytics for isDevelopmentTest smoke rewards.
                if (parsed.Granted && !parsed.IsDevelopmentTest)
                {
                    GameAnalytics.LogDailyRewardClaimed(
                        parsed.Rank,
                        parsed.RewardType,
                        parsed.RewardAmount);
                }
            }

            return parsed;
        }
        finally
        {
            claimInFlight = false;
        }
    }

    /// <summary>Editor/preview: inject a local pending reward without server writes.</summary>
    public static void SetPreviewPending(DailyRewardRecord reward)
    {
        cachedPending.Clear();
        cachedPending.Add(reward);
    }

    public static void ClearCachedPending()
    {
        cachedPending.Clear();
    }

    private const string SeenDaysKey = "RushOut_DailyResultSeenDays";
    private const int MaxSeenDays = 30;

    /// <summary>
    /// Local acknowledgement only. Never written when the result was not shown.
    /// </summary>
    public static bool IsResultSeen(string dayId)
    {
        if (!IsUtcDayId(dayId))
        {
            return false;
        }

        string raw = PlayerPrefs.GetString(SeenDaysKey, string.Empty);
        if (string.IsNullOrEmpty(raw))
        {
            return false;
        }

        string[] parts = raw.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            if (string.Equals(parts[i], dayId, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public static void MarkResultSeen(string dayId)
    {
        if (!IsUtcDayId(dayId) || IsResultSeen(dayId))
        {
            return;
        }

        var days = new List<string>();
        string raw = PlayerPrefs.GetString(SeenDaysKey, string.Empty);
        if (!string.IsNullOrEmpty(raw))
        {
            string[] parts = raw.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                if (IsUtcDayId(parts[i]))
                {
                    days.Add(parts[i]);
                }
            }
        }

        days.Add(dayId);
        if (days.Count > MaxSeenDays)
        {
            days.RemoveRange(0, days.Count - MaxSeenDays);
        }

        PlayerPrefs.SetString(SeenDaysKey, string.Join(",", days));
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Fetches the newest settled final placement the player has not acknowledged.
    /// success is false when the call did not succeed (offline / unsigned). Does not mark seen.
    /// success is true with <see cref="DailySettledResult.None"/> when the server has nothing to show.
    /// </summary>
    public static async Task<(bool success, DailySettledResult result)> TryFetchLatestSettledResultAsync()
    {
        try
        {
            if (!await DailyChallengeIdentityService.EnsureSignedInAsync())
            {
                return (false, DailySettledResult.None);
            }

            DailyChallengeFirebaseSettings settings = DailyChallengeFirebaseSettings.LoadDefault();
            if (settings == null || !settings.IsConfigured)
            {
                return (false, DailySettledResult.None);
            }

            string dataJson =
                "{\"acknowledgedDayIds\":" + BuildAcknowledgedDayIdsJson() + "}";
            FirebaseRestClient.CallableResponse response =
                await CallWithAuthRetryAsync("getSettledDailyResult", dataJson);
            if (!response.Ok || string.IsNullOrEmpty(response.ResultJson))
            {
                return (false, DailySettledResult.None);
            }

            if (!ExtractBool(response.ResultJson, "success"))
            {
                return (false, DailySettledResult.None);
            }

            if (!ExtractBool(response.ResultJson, "found"))
            {
                return (true, DailySettledResult.None);
            }

            DailySettledResult parsed = ParseSettledResult(response.ResultJson);
            if (!parsed.Found || !parsed.Settled || parsed.Rank <= 0 || !IsUtcDayId(parsed.DayId))
            {
                return (false, DailySettledResult.None);
            }

            return (true, parsed);
        }
        catch (Exception ex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("[DailyReward] getSettledDailyResult failed: " + ex.GetType().Name);
#endif
            return (false, DailySettledResult.None);
        }
    }

    private static void RemovePending(string dayId)
    {
        for (int i = cachedPending.Count - 1; i >= 0; i--)
        {
            if (string.Equals(cachedPending[i].DayId, dayId, StringComparison.Ordinal))
            {
                cachedPending.RemoveAt(i);
            }
        }
    }

    private static void MaybeLogCreatedAnalytics(DailyRewardRecord reward)
    {
        if (string.IsNullOrEmpty(reward.DayId) || reward.Claimed)
        {
            return;
        }

        // Never emit production daily_reward_created for DEV smoke rewards.
        if (reward.IsDevelopmentTest ||
            string.Equals(reward.DayId, DevRewardDayId, StringComparison.Ordinal) ||
            string.Equals(reward.Source, "daily_first_place_dev", StringComparison.Ordinal))
        {
            return;
        }

        string key = CreatedAnalyticsPrefsPrefix + reward.DayId;
        if (PlayerPrefs.GetInt(key, 0) == 1)
        {
            return;
        }

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        GameAnalytics.LogDailyRewardCreated(reward.Rank, reward.RewardType, reward.RewardAmount);
    }

    private static async Task<FirebaseRestClient.CallableResponse> CallWithAuthRetryAsync(
        string functionName,
        string dataJson)
    {
        DailyChallengeFirebaseSettings settings = DailyChallengeFirebaseSettings.LoadDefault();
        string baseUrl = settings.GetCallableBaseUrl();

        FirebaseRestClient.CallableResponse response =
            await FirebaseRestClient.CallCallableAsync(
                baseUrl,
                functionName,
                dataJson,
                DailyChallengeIdentityService.IdToken);

        if (!response.Ok && IsAuthFailure(response))
        {
            bool refreshed = await DailyChallengeIdentityService.TryRefreshSessionAsync();
            if (refreshed)
            {
                response = await FirebaseRestClient.CallCallableAsync(
                    baseUrl,
                    functionName,
                    dataJson,
                    DailyChallengeIdentityService.IdToken);
            }
        }

        return response;
    }

    private static bool IsAuthFailure(FirebaseRestClient.CallableResponse response)
    {
        string s = response.ErrorStatus ?? string.Empty;
        return s.IndexOf("UNAUTHENTICATED", StringComparison.OrdinalIgnoreCase) >= 0 ||
               response.HttpStatus == 401 ||
               response.HttpStatus == 403;
    }

    private static void ParseRewardsArray(string resultJson, List<DailyRewardRecord> into)
    {
        string arrayJson = ExtractJsonArray(resultJson, "rewards");
        if (string.IsNullOrEmpty(arrayJson))
        {
            return;
        }

        int i = 0;
        while (i < arrayJson.Length)
        {
            int objStart = arrayJson.IndexOf('{', i);
            if (objStart < 0)
            {
                break;
            }

            string obj = ExtractBalancedObject(arrayJson, objStart);
            if (string.IsNullOrEmpty(obj))
            {
                break;
            }

            into.Add(ParseRewardObject(obj));
            i = objStart + obj.Length;
        }
    }

    private static bool IsUtcDayId(string dayId)
    {
        if (string.IsNullOrEmpty(dayId) || dayId.Length != 10)
        {
            return false;
        }

        for (int i = 0; i < dayId.Length; i++)
        {
            char c = dayId[i];
            if (i == 4 || i == 7)
            {
                if (c != '-')
                {
                    return false;
                }
            }
            else if (c < '0' || c > '9')
            {
                return false;
            }
        }

        return true;
    }

    private static string BuildAcknowledgedDayIdsJson()
    {
        string raw = PlayerPrefs.GetString(SeenDaysKey, string.Empty);
        if (string.IsNullOrEmpty(raw))
        {
            return "[]";
        }

        var sb = new System.Text.StringBuilder();
        sb.Append('[');
        bool any = false;
        string[] parts = raw.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            if (!IsUtcDayId(parts[i]))
            {
                continue;
            }

            if (any)
            {
                sb.Append(',');
            }

            sb.Append('"');
            sb.Append(parts[i]);
            sb.Append('"');
            any = true;
        }

        sb.Append(']');
        return sb.ToString();
    }

    private static DailySettledResult ParseSettledResult(string json)
    {
        bool settled = ExtractBool(json, "settled");
        string dayId = FirebaseRestClient.ExtractJsonString(json, "dayId");
        int rank = (int)ExtractLong(json, "rank", 0);
        if (!settled || rank <= 0 || !IsUtcDayId(dayId))
        {
            return DailySettledResult.None;
        }

        return new DailySettledResult(
            found: true,
            settled: true,
            dayId: dayId,
            rank: rank,
            score: (int)ExtractLong(json, "score", 0),
            moves: (int)ExtractLong(json, "moves", 0),
            completionTimeMilliseconds: ExtractLong(json, "completionTimeMs", 0),
            hasUnclaimedReward: ExtractBool(json, "hasUnclaimedReward"),
            rewardAmount: (int)ExtractLong(json, "rewardAmount", 0));
    }

    private static DailyRewardRecord ParseRewardObject(string json)
    {
        string dayId = FirebaseRestClient.ExtractJsonString(json, "dayId");
        string source = FirebaseRestClient.ExtractJsonString(json, "source");
        bool isDev =
            ExtractBool(json, "isDevelopmentTest") ||
            string.Equals(dayId, DevRewardDayId, StringComparison.Ordinal) ||
            string.Equals(source, "daily_first_place_dev", StringComparison.Ordinal);

        return new DailyRewardRecord(
            dayId,
            (int)ExtractLong(json, "rank", 1),
            FirebaseRestClient.ExtractJsonString(json, "rewardType"),
            (int)ExtractLong(json, "rewardAmount", FirstPlaceCoinReward),
            ExtractBool(json, "claimed"),
            source,
            (int)ExtractLong(json, "score", 0),
            (int)ExtractLong(json, "moves", 0),
            ExtractLong(json, "completionTimeMilliseconds", 0),
            isDev);
    }

    private static DailyRewardClaimResult ParseClaimResult(string json)
    {
        bool success = ExtractBool(json, "success");
        string errorCode = FirebaseRestClient.ExtractJsonString(json, "errorCode");
        if (!success)
        {
            return DailyRewardClaimResult.Fail(
                string.IsNullOrEmpty(errorCode) ? "claim_failed" : errorCode,
                FirebaseRestClient.ExtractJsonString(json, "errorMessage"));
        }

        bool alreadyClaimed = ExtractBool(json, "alreadyClaimed") ||
                              string.Equals(errorCode, "already_claimed", StringComparison.Ordinal);
        bool granted = ExtractBool(json, "granted");
        // Never treat AlreadyClaimed as a spendable grant.
        if (alreadyClaimed)
        {
            granted = false;
        }

        string dayId = FirebaseRestClient.ExtractJsonString(json, "dayId");
        bool isDev =
            ExtractBool(json, "isDevelopmentTest") ||
            string.Equals(dayId, DevRewardDayId, StringComparison.Ordinal);

        return new DailyRewardClaimResult(
            success: true,
            granted: granted,
            alreadyClaimed: alreadyClaimed,
            dayId: dayId,
            rewardType: FirebaseRestClient.ExtractJsonString(json, "rewardType"),
            rewardAmount: (int)ExtractLong(json, "rewardAmount", FirstPlaceCoinReward),
            rank: (int)ExtractLong(json, "rank", 1),
            errorCode: errorCode,
            errorMessage: FirebaseRestClient.ExtractJsonString(json, "errorMessage"),
            isDevelopmentTest: isDev);
    }

    private static long ExtractLong(string json, string field, long fallback)
    {
        string asString = FirebaseRestClient.ExtractJsonString(json, field);
        if (long.TryParse(asString, NumberStyles.Integer, CultureInfo.InvariantCulture, out long v))
        {
            return v;
        }

        string pattern = "\"" + field + "\":";
        int idx = json.IndexOf(pattern, StringComparison.Ordinal);
        if (idx < 0)
        {
            return fallback;
        }

        int start = idx + pattern.Length;
        while (start < json.Length && char.IsWhiteSpace(json[start]))
        {
            start++;
        }

        int end = start;
        while (end < json.Length &&
               (char.IsDigit(json[end]) || json[end] == '-' || json[end] == '+'))
        {
            end++;
        }

        if (end > start &&
            long.TryParse(
                json.Substring(start, end - start),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out long n))
        {
            return n;
        }

        return fallback;
    }

    private static bool ExtractBool(string json, string field)
    {
        string pattern = "\"" + field + "\":";
        int idx = json.IndexOf(pattern, StringComparison.Ordinal);
        if (idx < 0)
        {
            return false;
        }

        int start = idx + pattern.Length;
        while (start < json.Length && char.IsWhiteSpace(json[start]))
        {
            start++;
        }

        return start < json.Length &&
               json.IndexOf("true", start, StringComparison.Ordinal) == start;
    }

    private static string ExtractJsonArray(string json, string field)
    {
        string marker = "\"" + field + "\":";
        int i = json.IndexOf(marker, StringComparison.Ordinal);
        if (i < 0)
        {
            return string.Empty;
        }

        int start = i + marker.Length;
        while (start < json.Length && char.IsWhiteSpace(json[start]))
        {
            start++;
        }

        if (start >= json.Length || json[start] != '[')
        {
            return string.Empty;
        }

        return ExtractBalanced(json, start, '[', ']');
    }

    private static string ExtractBalancedObject(string json, int start)
    {
        return ExtractBalanced(json, start, '{', '}');
    }

    private static string ExtractBalanced(string json, int start, char open, char close)
    {
        if (start < 0 || start >= json.Length || json[start] != open)
        {
            return string.Empty;
        }

        int depth = 0;
        bool inString = false;
        for (int i = start; i < json.Length; i++)
        {
            char c = json[i];
            if (c == '"' && (i == 0 || json[i - 1] != '\\'))
            {
                inString = !inString;
            }

            if (inString)
            {
                continue;
            }

            if (c == open)
            {
                depth++;
            }
            else if (c == close)
            {
                depth--;
                if (depth == 0)
                {
                    return json.Substring(start, i - start + 1);
                }
            }
        }

        return string.Empty;
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Phase 4.1B Online authority: security-sensitive mutations go through Cloud Functions.
/// Firestore client access is read-only (leaderboard / presentation).
///
/// Server is authoritative for: day, challenge, attempt state, timestamps, score calculation.
/// Client-reported moves/completionTimeMs remain a known trust boundary (not full anti-cheat).
/// </summary>
public sealed class FirebaseOnlineDailyChallengeAuthority : IDailyChallengeAuthority
{
    public async Task<DailyChallengeAuthorityResult> GetCurrentChallengeAsync()
    {
        FirebaseRestClient.CallableResponse response =
            await CallWithAuthRetryAsync("getDailyChallenge", "{}");
        if (!response.Ok)
        {
            return DailyChallengeAuthorityResult.Fail(
                MapTransportError(response),
                "CONNECT TO PLAY");
        }

        return ParseEnvelope(response.ResultJson, logLabel: "Server challenge");
    }

    public async Task<DailyChallengeAuthorityResult> GetAttemptStateAsync(string dayId)
    {
        // Prefer getDailyChallenge (server day) — ignore client dayId for authority.
        DailyChallengeAuthorityResult current = await GetCurrentChallengeAsync();
        if (!current.Success)
        {
            return current;
        }

        if (!string.IsNullOrEmpty(dayId) &&
            !string.Equals(dayId, current.Challenge.DayId, StringComparison.Ordinal))
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                "[DailyBackend] Requested dayId=" + dayId +
                " differs from server day=" + current.Challenge.DayId +
                " — returning server attempt.");
#endif
        }

        return current;
    }

    public async Task<DailyChallengeAuthorityResult> TryStartAttemptAsync(
        DailyChallengeDescriptor challenge)
    {
        // Server ignores client challenge fields for day/level authority.
        FirebaseRestClient.CallableResponse response =
            await CallWithAuthRetryAsync("startDailyAttempt", "{}");
        if (!response.Ok)
        {
            GameAnalytics.LogDailyBackendStartError(
                challenge.DayId ?? string.Empty,
                MapTransportError(response));
            return DailyChallengeAuthorityResult.Fail(
                MapTransportError(response),
                "Could not claim attempt.");
        }

        DailyChallengeAuthorityResult parsed = ParseEnvelope(response.ResultJson, "Attempt");
        if (parsed.Success)
        {
            GameAnalytics.LogDailyBackendStartSuccess(parsed.Challenge.DayId);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                "[DailyBackend] Attempt claimed: Started day=" + parsed.Challenge.DayId +
                " level=" + parsed.Challenge.LevelId);
#endif
            return parsed;
        }

        if (string.Equals(parsed.ErrorCode, "attempt_exists", StringComparison.Ordinal))
        {
            GameAnalytics.LogDailyBackendStartRejected(
                parsed.Challenge.DayId ?? challenge.DayId ?? string.Empty,
                parsed.Attempt.State.ToString());
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                "[DailyBackend] Attempt rejected: " + parsed.Attempt.State);
#endif
        }
        else
        {
            GameAnalytics.LogDailyBackendStartError(
                parsed.Challenge.DayId ?? challenge.DayId ?? string.Empty,
                parsed.ErrorCode ?? "start_failed");
        }

        return parsed;
    }

    public async Task<DailyChallengeAuthorityResult> SubmitResultAsync(
        DailyChallengeDescriptor challenge,
        int moves,
        long completionTimeMs,
        int clientComputedScore)
    {
        // Do NOT send clientComputedScore as authority — server recalculates.
        // clientComputedScore retained only for local provisional UI / logging.
        _ = clientComputedScore;

        var sb = new StringBuilder(160);
        sb.Append('{');
        // dayId is a HINT only (UTC midnight pending edge). Server verifies Started attempt.
        if (!string.IsNullOrEmpty(challenge.DayId))
        {
            sb.Append("\"dayId\":\"").Append(Escape(challenge.DayId)).Append("\",");
        }

        sb.Append("\"levelId\":\"").Append(Escape(challenge.LevelId)).Append("\",");
        sb.Append("\"moves\":").Append(moves).Append(',');
        sb.Append("\"completionTimeMs\":").Append(completionTimeMs).Append(',');
        sb.Append("\"scoreVersion\":").Append(DailyChallengeScoreVersion.Current);
        sb.Append('}');

        FirebaseRestClient.CallableResponse response =
            await CallWithAuthRetryAsync("submitDailyResult", sb.ToString());
        if (!response.Ok)
        {
            GameAnalytics.LogDailyResultSubmitError(
                challenge.DayId ?? string.Empty,
                MapTransportError(response));
            return DailyChallengeAuthorityResult.Fail(
                MapTransportError(response),
                "Submit failed.");
        }

        DailyChallengeAuthorityResult parsed =
            ParseEnvelope(response.ResultJson, "Result authoritative");
        if (parsed.Success)
        {
            GameAnalytics.LogDailyResultSubmitSuccess(parsed.Challenge.DayId);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                "[DailyBackend] Result authoritative: score=" + parsed.Attempt.Score +
                " trusted=" + parsed.Attempt.ScoreTrusted +
                " moves=" + parsed.Attempt.Moves +
                " ms=" + parsed.Attempt.CompletionTimeMs);
#endif
        }
        else
        {
            GameAnalytics.LogDailyResultSubmitError(
                parsed.Challenge.DayId ?? challenge.DayId ?? string.Empty,
                parsed.ErrorCode ?? "submit_rejected");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(
                "[DailyBackend] Result rejected: " + parsed.ErrorCode + " " +
                parsed.ErrorMessage);
#endif
        }

        return parsed;
    }

    public async Task<DailyChallengeAuthorityResult> MarkAbandonedAsync(string dayId)
    {
        FirebaseRestClient.CallableResponse response =
            await CallWithAuthRetryAsync("abandonDailyAttempt", "{}");
        if (!response.Ok)
        {
            return DailyChallengeAuthorityResult.Fail(
                MapTransportError(response),
                "Abandon failed.");
        }

        return ParseEnvelope(response.ResultJson, "Abandon");
    }

    public async Task<IReadOnlyList<DailyLeaderboardEntry>> GetCompletedLeaderboardAsync(
        string dayId,
        int maxEntries)
    {
        var list = new List<DailyLeaderboardEntry>();
        if (!await EnsureReadyAsync() || string.IsNullOrEmpty(dayId))
        {
            return list;
        }

        DailyChallengeFirebaseSettings settings = DailyChallengeFirebaseSettings.LoadDefault();
        int limit = Mathf.Clamp(maxEntries, 1, 100);
        string query =
            "{\"structuredQuery\":{" +
            "\"from\":[{\"collectionId\":\"attempts\"}]," +
            "\"where\":{\"fieldFilter\":{" +
            "\"field\":{\"fieldPath\":\"state\"}," +
            "\"op\":\"EQUAL\"," +
            "\"value\":{\"stringValue\":\"Completed\"}}}," +
            "\"orderBy\":[{\"field\":{\"fieldPath\":\"score\"},\"direction\":\"DESCENDING\"}]," +
            "\"limit\":" + limit +
            "}}";

        string parent = "dailyChallenges/" + dayId;
        string json = await FirebaseRestClient.RunQueryAsync(
            settings.ProjectId,
            parent,
            query,
            DailyChallengeIdentityService.IdToken);

        if (string.IsNullOrEmpty(json))
        {
            return list;
        }

        string[] parts = json.Split(new[] { "\"document\":" }, StringSplitOptions.None);
        string myUid = DailyChallengeIdentityService.UserId;
        for (int i = 1; i < parts.Length; i++)
        {
            string chunk = parts[i];
            string name = FirebaseRestClient.ExtractJsonString(chunk, "name");
            string uid = name;
            int idx = name.LastIndexOf('/');
            if (idx >= 0)
            {
                uid = name.Substring(idx + 1);
            }

            string state = FirebaseRestClient.ExtractFirestoreStringField(chunk, "state");
            if (!string.Equals(state, "Completed", StringComparison.Ordinal))
            {
                continue;
            }

            int score = (int)FirebaseRestClient.ExtractFirestoreLongField(chunk, "score");
            int moves = (int)FirebaseRestClient.ExtractFirestoreLongField(chunk, "moves");
            long ms = FirebaseRestClient.ExtractFirestoreLongField(chunk, "completionTimeMs");
            bool isYou = string.Equals(uid, myUid, StringComparison.Ordinal);

            list.Add(new DailyLeaderboardEntry
            {
                PlayerId = uid,
                DisplayName = isYou ? "YOU" : DailyChallengeDisplayName.FromUserId(uid),
                Score = score,
                Moves = moves,
                CompletionTimeMilliseconds = ms,
                IsCurrentPlayer = isYou,
                IsSimulated = false,
                Source = isYou
                    ? DailyLeaderboardEntrySource.LocalPlayer
                    : DailyLeaderboardEntrySource.Online,
                Rank = 0
            });
        }

        return list;
    }

    private static async Task<FirebaseRestClient.CallableResponse> CallWithAuthRetryAsync(
        string functionName,
        string dataJson)
    {
        if (!await EnsureReadyAsync())
        {
            return new FirebaseRestClient.CallableResponse
            {
                Ok = false,
                ErrorStatus = "UNAUTHENTICATED",
                HttpStatus = 401
            };
        }

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
            // Refresh token — do NOT create a new anonymous account for expiry alone.
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
        if (response.HttpStatus == 401 || response.HttpStatus == 403)
        {
            return true;
        }

        string status = response.ErrorStatus ?? string.Empty;
        return status.IndexOf("UNAUTHENTICATED", StringComparison.OrdinalIgnoreCase) >= 0 ||
               status.IndexOf("PERMISSION_DENIED", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string MapTransportError(FirebaseRestClient.CallableResponse response)
    {
        if (!string.IsNullOrEmpty(response.ErrorStatus))
        {
            return response.ErrorStatus;
        }

        return response.HttpStatus > 0 ? "http_" + response.HttpStatus : "network";
    }

    private static DailyChallengeAuthorityResult ParseEnvelope(string json, string logLabel)
    {
        if (string.IsNullOrEmpty(json))
        {
            return DailyChallengeAuthorityResult.Fail("empty", "Empty backend response.");
        }

        bool success = ExtractBool(json, "success", defaultValue: false);
        string errorCode = FirebaseRestClient.ExtractJsonString(json, "errorCode");
        string errorMessage = FirebaseRestClient.ExtractJsonString(json, "errorMessage");

        DailyChallengeDescriptor challenge = ParseChallenge(json);
        DailyChallengeAttemptRecord attempt = ParseAttempt(json);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!string.IsNullOrEmpty(logLabel) && challenge.IsValid)
        {
            Debug.Log(
                "[DailyBackend] " + logLabel + ": " + challenge.DayId +
                " / " + challenge.LevelId +
                (attempt.Exists ? (" state=" + attempt.State) : string.Empty) +
                (success ? string.Empty : (" err=" + errorCode)));
        }
#endif

        if (!success)
        {
            return DailyChallengeAuthorityResult.Fail(
                string.IsNullOrEmpty(errorCode) ? "rejected" : errorCode,
                string.IsNullOrEmpty(errorMessage) ? "Rejected." : errorMessage,
                challenge,
                attempt);
        }

        return DailyChallengeAuthorityResult.Ok(challenge, attempt);
    }

    private static DailyChallengeDescriptor ParseChallenge(string json)
    {
        // Prefer nested challenge object fields via flat extract (fields unique enough).
        string dayId = ExtractNestedString(json, "challenge", "dayId");
        if (string.IsNullOrEmpty(dayId))
        {
            dayId = FirebaseRestClient.ExtractJsonString(json, "dayId");
        }

        string levelId = ExtractNestedString(json, "challenge", "levelId");
        if (string.IsNullOrEmpty(levelId))
        {
            levelId = FirebaseRestClient.ExtractJsonString(json, "levelId");
        }

        string levelVersion = ExtractNestedString(json, "challenge", "levelVersion");
        if (string.IsNullOrEmpty(levelVersion))
        {
            levelVersion = levelId;
        }

        int scoreVersion = (int)ExtractNestedLong(json, "challenge", "scoreVersion");
        if (scoreVersion <= 0)
        {
            scoreVersion = DailyChallengeScoreVersion.Current;
        }

        return new DailyChallengeDescriptor
        {
            DayId = dayId,
            LevelId = levelId,
            LevelVersion = levelVersion,
            ScoreVersion = scoreVersion,
            OpensAtUtc = ExtractNestedString(json, "challenge", "opensAtUtc"),
            ClosesAtUtc = ExtractNestedString(json, "challenge", "closesAtUtc")
        };
    }

    private static DailyChallengeAttemptRecord ParseAttempt(string json)
    {
        string stateRaw = ExtractNestedString(json, "attempt", "state");
        DailyChallengeServerAttemptState state = DailyChallengeServerAttemptState.None;
        if (string.Equals(stateRaw, "Started", StringComparison.OrdinalIgnoreCase))
        {
            state = DailyChallengeServerAttemptState.Started;
        }
        else if (string.Equals(stateRaw, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            state = DailyChallengeServerAttemptState.Completed;
        }
        else if (string.Equals(stateRaw, "Abandoned", StringComparison.OrdinalIgnoreCase))
        {
            state = DailyChallengeServerAttemptState.Abandoned;
        }

        bool exists = ExtractNestedBool(json, "attempt", "exists", state != DailyChallengeServerAttemptState.None);
        if (!exists)
        {
            state = DailyChallengeServerAttemptState.None;
        }

        return new DailyChallengeAttemptRecord
        {
            DayId = ExtractNestedString(json, "attempt", "dayId"),
            UserId = ExtractNestedString(json, "attempt", "userId"),
            LevelId = ExtractNestedString(json, "attempt", "levelId"),
            State = state,
            StartedAtServer = ExtractNestedString(json, "attempt", "startedAtServer"),
            CompletedAtServer = ExtractNestedString(json, "attempt", "completedAtServer"),
            Moves = (int)ExtractNestedLong(json, "attempt", "moves"),
            CompletionTimeMs = ExtractNestedLong(json, "attempt", "completionTimeMs"),
            Score = (int)ExtractNestedLong(json, "attempt", "score"),
            ScoreVersion = (int)ExtractNestedLong(json, "attempt", "scoreVersion"),
            ScoreTrusted = ExtractNestedBool(json, "attempt", "scoreTrusted", false)
        };
    }

    private static string ExtractNestedString(string json, string objectKey, string field)
    {
        string obj = ExtractObjectByKey(json, objectKey);
        if (string.IsNullOrEmpty(obj))
        {
            return string.Empty;
        }

        return FirebaseRestClient.ExtractJsonString(obj, field);
    }

    private static long ExtractNestedLong(string json, string objectKey, string field)
    {
        string obj = ExtractObjectByKey(json, objectKey);
        if (string.IsNullOrEmpty(obj))
        {
            return 0;
        }

        string asString = FirebaseRestClient.ExtractJsonString(obj, field);
        if (long.TryParse(asString, NumberStyles.Integer, CultureInfo.InvariantCulture, out long v))
        {
            return v;
        }

        // Bare number
        string pattern = "\"" + field + "\":";
        int i = obj.IndexOf(pattern, StringComparison.Ordinal);
        if (i < 0)
        {
            return 0;
        }

        int start = i + pattern.Length;
        while (start < obj.Length && char.IsWhiteSpace(obj[start]))
        {
            start++;
        }

        int end = start;
        while (end < obj.Length && (char.IsDigit(obj[end]) || obj[end] == '-'))
        {
            end++;
        }

        long.TryParse(obj.Substring(start, end - start), out long bare);
        return bare;
    }

    private static bool ExtractNestedBool(
        string json,
        string objectKey,
        string field,
        bool defaultValue)
    {
        string obj = ExtractObjectByKey(json, objectKey);
        if (string.IsNullOrEmpty(obj))
        {
            return defaultValue;
        }

        return ExtractBool(obj, field, defaultValue);
    }

    private static bool ExtractBool(string json, string key, bool defaultValue)
    {
        string pattern = "\"" + key + "\":";
        int i = json.IndexOf(pattern, StringComparison.Ordinal);
        if (i < 0)
        {
            return defaultValue;
        }

        int start = i + pattern.Length;
        while (start < json.Length && char.IsWhiteSpace(json[start]))
        {
            start++;
        }

        if (start + 4 <= json.Length &&
            string.Compare(json, start, "true", 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
        {
            return true;
        }

        if (start + 5 <= json.Length &&
            string.Compare(json, start, "false", 0, 5, StringComparison.OrdinalIgnoreCase) == 0)
        {
            return false;
        }

        return defaultValue;
    }

    private static string ExtractObjectByKey(string json, string key)
    {
        string marker = "\"" + key + "\":";
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

        if (start >= json.Length || json[start] != '{')
        {
            return string.Empty;
        }

        return ExtractBalancedLocal(json, start);
    }

    private static string ExtractBalancedLocal(string json, int start)
    {
        int depth = 0;
        bool inString = false;
        bool escape = false;
        for (int i = start; i < json.Length; i++)
        {
            char c = json[i];
            if (inString)
            {
                if (escape)
                {
                    escape = false;
                }
                else if (c == '\\')
                {
                    escape = true;
                }
                else if (c == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (c == '"')
            {
                inString = true;
                continue;
            }

            if (c == '{')
            {
                depth++;
            }
            else if (c == '}')
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

    private static async Task<bool> EnsureReadyAsync()
    {
        DailyChallengeFirebaseSettings settings = DailyChallengeFirebaseSettings.LoadDefault();
        if (settings == null || !settings.IsConfigured)
        {
            return false;
        }

        return await DailyChallengeIdentityService.EnsureSignedInAsync();
    }

    private static string Escape(string value)
    {
        return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}

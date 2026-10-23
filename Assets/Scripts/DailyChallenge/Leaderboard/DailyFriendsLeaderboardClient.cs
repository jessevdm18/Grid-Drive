using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Registers the linked Play Games player from Firebase Auth and loads
/// friend Daily rows. May send this player's own public display name.
/// Never sends email, auth tokens, or another player's Play Games id.
/// </summary>
public static class DailyFriendsLeaderboardClient
{
    private static bool publishedOwnName;

    public static async Task PublishOwnDisplayNameIfAuthenticatedAsync()
    {
        if (publishedOwnName)
        {
            return;
        }

        string name = GooglePlayGamesAccountProvider.TryReadAuthenticatedDisplayName();
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        publishedOwnName = await RegisterLinkedPlayerAsync(name);
    }

    public static Task<bool> RegisterLinkedPlayerAsync()
    {
        return RegisterLinkedPlayerAsync(null);
    }

    public static async Task<bool> RegisterLinkedPlayerAsync(string publicDisplayName)
    {
        try
        {
            string dataJson = "{}";
            if (!string.IsNullOrEmpty(publicDisplayName))
            {
                dataJson =
                    "{\"displayName\":\"" + EscapeJson(publicDisplayName) + "\"}";
            }

            FirebaseRestClient.CallableResponse response = await CallAsync(
                "registerGooglePlayIdentity",
                dataJson);
            Debug.Log(
                "[Friends] identity register ok=" + response.Ok +
                " http=" + response.HttpStatus);
            return response.Ok &&
                   response.ResultJson != null &&
                   response.ResultJson.IndexOf("\"mapped\":true", StringComparison.Ordinal) >= 0;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Friends] identity register failed: " + ex.GetType().Name);
            return false;
        }
    }

    public static async Task<DailyLeaderboardSnapshot> FetchAsync(
        string dayId,
        IReadOnlyList<string> friendPlayerIds)
    {
        if (string.IsNullOrEmpty(dayId))
        {
            return null;
        }

        string json = BuildRequestJson(dayId, friendPlayerIds);
        FirebaseRestClient.CallableResponse response = await CallAsync(
            "getDailyFriendsLeaderboard",
            json);
        if (!response.Ok || string.IsNullOrEmpty(response.ResultJson))
        {
            Debug.LogWarning(
                "[Friends] leaderboard failed http=" + response.HttpStatus);
            return null;
        }

        return ParseSnapshot(dayId, response.ResultJson);
    }

    private static async Task<FirebaseRestClient.CallableResponse> CallAsync(
        string functionName,
        string dataJson)
    {
        bool signedIn = await DailyChallengeIdentityService.EnsureSignedInAsync();
        if (!signedIn || string.IsNullOrEmpty(DailyChallengeIdentityService.IdToken))
        {
            return new FirebaseRestClient.CallableResponse { Ok = false, HttpStatus = 0 };
        }

        DailyChallengeFirebaseSettings settings = DailyChallengeFirebaseSettings.LoadDefault();
        if (settings == null || !settings.IsConfigured)
        {
            return new FirebaseRestClient.CallableResponse { Ok = false, HttpStatus = 0 };
        }

        return await FirebaseRestClient.CallCallableAsync(
            settings.GetCallableBaseUrl(),
            functionName,
            dataJson,
            DailyChallengeIdentityService.IdToken);
    }

    private static string BuildRequestJson(string dayId, IReadOnlyList<string> friendPlayerIds)
    {
        var builder = new StringBuilder();
        builder.Append("{\"dayId\":\"");
        builder.Append(EscapeJson(dayId));
        builder.Append("\",\"friendPlayerIds\":[");
        int written = 0;
        if (friendPlayerIds != null)
        {
            for (int i = 0; i < friendPlayerIds.Count && written < 200; i++)
            {
                string id = friendPlayerIds[i];
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                if (written > 0)
                {
                    builder.Append(',');
                }

                builder.Append('"');
                builder.Append(EscapeJson(id));
                builder.Append('"');
                written++;
            }
        }

        builder.Append("]}");
        return builder.ToString();
    }

    private static DailyLeaderboardSnapshot ParseSnapshot(string dayId, string json)
    {
        var entries = new List<DailyLeaderboardEntry>();
        int rowsAt = json.IndexOf("\"rows\"", StringComparison.Ordinal);
        if (rowsAt >= 0)
        {
            int arrayAt = json.IndexOf('[', rowsAt);
            if (arrayAt >= 0)
            {
                string arrayJson = ExtractBalanced(json, arrayAt, '[', ']');
                ParseRows(arrayJson, entries);
            }
        }

        bool hasPlayer = false;
        int playerRank = 0;
        DailyLeaderboardEntry playerEntry = default;
        int otherResults = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].IsCurrentPlayer)
            {
                hasPlayer = true;
                playerRank = entries[i].Rank;
                playerEntry = entries[i];
            }
            else
            {
                otherResults++;
            }
        }

        string status = string.Empty;
        if (hasPlayer)
        {
            status = otherResults == 0
                ? "NO FRIEND RESULTS YET"
                : "YOUR RANK #" + playerRank.ToString(CultureInfo.InvariantCulture);
        }

        return new DailyLeaderboardSnapshot
        {
            DayId = dayId,
            Entries = entries,
            HasCurrentPlayer = hasPlayer,
            CurrentPlayerRank = playerRank,
            CurrentPlayerEntry = playerEntry,
            StatusMessage = status
        };
    }

    private static void ParseRows(string arrayJson, List<DailyLeaderboardEntry> entries)
    {
        if (string.IsNullOrEmpty(arrayJson))
        {
            return;
        }

        int index = 0;
        while (index < arrayJson.Length)
        {
            int start = arrayJson.IndexOf('{', index);
            if (start < 0)
            {
                break;
            }

            string row = ExtractBalanced(arrayJson, start, '{', '}');
            if (string.IsNullOrEmpty(row))
            {
                break;
            }

            bool current = row.IndexOf("\"isCurrentPlayer\":true", StringComparison.Ordinal) >= 0;
            entries.Add(new DailyLeaderboardEntry
            {
                PlayerId = current ? "you" : "friend",
                DisplayName = Unescape(FirebaseRestClient.ExtractJsonString(row, "displayName")),
                Score = (int)ReadJsonLong(row, "score"),
                Moves = (int)ReadJsonLong(row, "moves"),
                CompletionTimeMilliseconds = ReadJsonLong(row, "completionTimeMs"),
                IsCurrentPlayer = current,
                IsSimulated = false,
                Source = current
                    ? DailyLeaderboardEntrySource.LocalPlayer
                    : DailyLeaderboardEntrySource.Online,
                Rank = (int)ReadJsonLong(row, "rank")
            });
            index = start + row.Length;
        }
    }

    private static string ExtractBalanced(string json, int start, char open, char close)
    {
        int depth = 0;
        for (int i = start; i < json.Length; i++)
        {
            if (json[i] == open)
            {
                depth++;
            }
            else if (json[i] == close)
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

    private static long ReadJsonLong(string json, string key)
    {
        string pattern = "\"" + key + "\":";
        int i = json.IndexOf(pattern, StringComparison.Ordinal);
        if (i < 0)
        {
            return 0;
        }

        int start = i + pattern.Length;
        while (start < json.Length && char.IsWhiteSpace(json[start]))
        {
            start++;
        }

        int end = start;
        if (end < json.Length && json[end] == '-')
        {
            end++;
        }

        while (end < json.Length && char.IsDigit(json[end]))
        {
            end++;
        }

        if (end <= start)
        {
            return 0;
        }

        long value;
        return long.TryParse(
            json.Substring(start, end - start),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out value)
            ? value
            : 0;
    }

    private static string EscapeJson(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }

    private static string Unescape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
    }
}

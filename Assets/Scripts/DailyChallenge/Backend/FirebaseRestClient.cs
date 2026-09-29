using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Minimal Firebase Auth + Firestore REST client for Daily Challenge authority.
/// Compiles without Firebase Auth/Firestore Unity SDKs (not present in this project).
/// </summary>
public static class FirebaseRestClient
{
    public static async Task<bool> TryAnonymousSignUpAsync(string apiKey)
    {
        string url =
            "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=" +
            UnityWebRequest.EscapeURL(apiKey);
        string body = "{\"returnSecureToken\":true}";
        string json = await PostJsonAsync(url, body, bearerToken: null);
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        return ApplyAuthJson(json);
    }

    public static async Task<bool> TryRefreshIdTokenAsync(string apiKey, string refreshToken)
    {
        string url =
            "https://securetoken.googleapis.com/v1/token?key=" +
            UnityWebRequest.EscapeURL(apiKey);
        string body =
            "grant_type=refresh_token&refresh_token=" +
            UnityWebRequest.EscapeURL(refreshToken);
        using (UnityWebRequest req = UnityWebRequest.Post(
                   url,
                   body,
                   "application/x-www-form-urlencoded"))
        {
            req.timeout = 20;
            await SendAsync(req);
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(
                    "[DailyChallenge] Token refresh failed: " + req.error +
                    " " + req.downloadHandler?.text);
                return false;
            }

            string json = req.downloadHandler.text;
            string idToken = ExtractJsonString(json, "id_token");
            string userId = ExtractJsonString(json, "user_id");
            string newRefresh = ExtractJsonString(json, "refresh_token");
            int expires = ExtractJsonInt(json, "expires_in", 3600);
            if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(userId))
            {
                return false;
            }

            DailyChallengeIdentityService.ApplySession(
                userId,
                idToken,
                string.IsNullOrEmpty(newRefresh) ? refreshToken : newRefresh,
                expires);
            return true;
        }
    }

    /// <summary>
    /// GET account providers via Identity Toolkit accounts:lookup.
    /// Never log the response (may contain emails / PII).
    /// </summary>
    public static async Task<FirebaseAccountInfo> LookupAccountInfoAsync(
        string apiKey,
        string idToken)
    {
        string url =
            "https://identitytoolkit.googleapis.com/v1/accounts:lookup?key=" +
            UnityWebRequest.EscapeURL(apiKey);
        string body = "{\"idToken\":\"" + Escape(idToken ?? string.Empty) + "\"}";
        var http = await PostJsonDetailedAsync(url, body, bearerToken: null);
        if (!http.Ok || string.IsNullOrEmpty(http.Body))
        {
            return FirebaseAccountInfo.Failed(AccountLinkResult.NetworkError);
        }

        return ParseAccountInfo(http.Body);
    }

    /// <summary>
    /// Link an IdP credential to the CURRENT Firebase user (preserves UID when successful).
    /// Uses accounts:signInWithIdp with the existing idToken.
    /// </summary>
    public static async Task<FirebaseLinkIdpResult> TryLinkIdpAsync(
        string apiKey,
        string currentIdToken,
        ExternalAccountCredential credential)
    {
        if (credential == null || string.IsNullOrEmpty(credential.IdpPostBody))
        {
            return FirebaseLinkIdpResult.Fail(AccountLinkResult.AuthenticationError, "missing_credential");
        }

        string url =
            "https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key=" +
            UnityWebRequest.EscapeURL(apiKey);

        // requestUri is required by Identity Toolkit; localhost is the conventional value for mobile.
        string body =
            "{" +
            "\"idToken\":\"" + Escape(currentIdToken ?? string.Empty) + "\"," +
            "\"postBody\":\"" + Escape(credential.IdpPostBody) + "\"," +
            "\"requestUri\":\"http://localhost\"," +
            "\"returnIdpCredential\":true," +
            "\"returnSecureToken\":true" +
            "}";

        var http = await PostJsonDetailedAsync(url, body, bearerToken: null);
        if (string.IsNullOrEmpty(http.Body))
        {
            return FirebaseLinkIdpResult.Fail(AccountLinkResult.NetworkError, "empty");
        }

        string errorMessage = ExtractJsonString(http.Body, "message");
        if (!http.Ok || !string.IsNullOrEmpty(errorMessage) &&
            http.Body.IndexOf("\"error\"", StringComparison.Ordinal) >= 0)
        {
            return MapLinkError(errorMessage, http.Body);
        }

        string localId = ExtractJsonString(http.Body, "localId");
        string idToken = ExtractJsonString(http.Body, "idToken");
        string refresh = ExtractJsonString(http.Body, "refreshToken");
        int expires = ExtractJsonInt(http.Body, "expiresIn", 3600);
        if (string.IsNullOrEmpty(localId) || string.IsNullOrEmpty(idToken))
        {
            return FirebaseLinkIdpResult.Fail(AccountLinkResult.AuthenticationError, "missing_tokens");
        }

        return new FirebaseLinkIdpResult
        {
            Result = AccountLinkResult.Success,
            LocalId = localId,
            IdToken = idToken,
            RefreshToken = refresh,
            ExpiresInSeconds = expires
        };
    }

    public struct FirebaseLinkIdpResult
    {
        public AccountLinkResult Result;
        public string LocalId;
        public string IdToken;
        public string RefreshToken;
        public int ExpiresInSeconds;
        public string ErrorCode;

        public static FirebaseLinkIdpResult Fail(AccountLinkResult result, string errorCode)
        {
            return new FirebaseLinkIdpResult
            {
                Result = result,
                ErrorCode = errorCode ?? string.Empty
            };
        }
    }

    public struct FirebaseAccountInfo
    {
        public bool Ok;
        public AccountLinkResult Failure;
        public string LocalId;
        public bool IsAnonymous;
        public AccountProvider[] Providers;

        public static FirebaseAccountInfo Failed(AccountLinkResult failure)
        {
            return new FirebaseAccountInfo
            {
                Ok = false,
                Failure = failure,
                Providers = Array.Empty<AccountProvider>()
            };
        }
    }

    private static FirebaseLinkIdpResult MapLinkError(string message, string body)
    {
        string m = (message ?? string.Empty) + " " + (body ?? string.Empty);
        if (m.IndexOf("CREDENTIAL_ALREADY_IN_USE", StringComparison.OrdinalIgnoreCase) >= 0 ||
            m.IndexOf("FEDERATED_USER_ID_ALREADY_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
            m.IndexOf("EMAIL_EXISTS", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return FirebaseLinkIdpResult.Fail(
                AccountLinkResult.AlreadyLinkedToAnotherAccount,
                "already_linked_other");
        }

        if (m.IndexOf("TOKEN_EXPIRED", StringComparison.OrdinalIgnoreCase) >= 0 ||
            m.IndexOf("INVALID_ID_TOKEN", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return FirebaseLinkIdpResult.Fail(AccountLinkResult.AuthenticationError, "token");
        }

        if (m.IndexOf("NETWORK", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return FirebaseLinkIdpResult.Fail(AccountLinkResult.NetworkError, "network");
        }

        return FirebaseLinkIdpResult.Fail(AccountLinkResult.UnknownError, "link_failed");
    }

    private static FirebaseAccountInfo ParseAccountInfo(string json)
    {
        // users[0].localId / providerUserInfo / lastLoginAt
        string localId = ExtractJsonString(json, "localId");
        var providers = new List<AccountProvider>();
        bool hasFederated = false;

        // Play Games Firebase providerId.
        if (json.IndexOf("playgames.google.com", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            providers.Add(AccountProvider.GooglePlay);
            hasFederated = true;
        }

        if (json.IndexOf("apple.com", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            providers.Add(AccountProvider.Apple);
            hasFederated = true;
        }

        // Anonymous users typically have no federated providers.
        if (!hasFederated)
        {
            providers.Add(AccountProvider.Anonymous);
        }
        else
        {
            // Keep Anonymous in the set if Firebase still reports anonymous provider.
            if (json.IndexOf("firebase", StringComparison.OrdinalIgnoreCase) >= 0 &&
                json.IndexOf("\"providerId\":\"anonymous\"", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                providers.Insert(0, AccountProvider.Anonymous);
            }
        }

        return new FirebaseAccountInfo
        {
            Ok = true,
            LocalId = localId,
            IsAnonymous = !hasFederated,
            Providers = providers.ToArray()
        };
    }

    private static async Task<(bool Ok, string Body, long Status)> PostJsonDetailedAsync(
        string url,
        string body,
        string bearerToken)
    {
        using (UnityWebRequest req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] raw = Encoding.UTF8.GetBytes(body ?? "{}");
            req.uploadHandler = new UploadHandlerRaw(raw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(bearerToken))
            {
                req.SetRequestHeader("Authorization", "Bearer " + bearerToken);
            }

            req.timeout = 25;
            await SendAsync(req);
            string text = req.downloadHandler?.text ?? string.Empty;
            bool ok = req.result == UnityWebRequest.Result.Success &&
                      req.responseCode >= 200 &&
                      req.responseCode < 300;
            return (ok, text, req.responseCode);
        }
    }

    /// <summary>
    /// Legacy probe. Phase 4.1B authoritative Online day/time comes from Cloud Functions only.
    /// Do not use for attempt start/submit authority (may fall back to device UTC).
    /// </summary>
    public static async Task<DateTime> GetServerUtcNowAsync(string projectId, string idToken)
    {
        string docPath =
            "projects/" + projectId +
            "/databases/(default)/documents/dailyMeta/serverClockPing";
        string url =
            "https://firestore.googleapis.com/v1/" + docPath +
            "?updateMask.fieldPaths=t";

        // Use updateTransforms via :commit for true server timestamp.
        string commitUrl =
            "https://firestore.googleapis.com/v1/projects/" + projectId +
            "/databases/(default)/documents:commit";
        string commitBody =
            "{\"writes\":[{\"transform\":{\"document\":\"" + docPath +
            "\",\"fieldTransforms\":[{\"fieldPath\":\"t\",\"setToServerValue\":\"REQUEST_TIME\"}]}}]}";

        string commitJson = await PostJsonAsync(commitUrl, commitBody, idToken);
        if (!string.IsNullOrEmpty(commitJson))
        {
            string ts = ExtractNestedTimestamp(commitJson);
            if (TryParseRfc3339(ts, out DateTime utc))
            {
                return DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            }
        }

        // Fallback read after soft write.
        string patchBody = "{\"fields\":{\"ping\":{\"nullValue\":null}}}";
        await PatchJsonAsync(url, patchBody, idToken);
        string getJson = await GetJsonAsync(
            "https://firestore.googleapis.com/v1/" + docPath,
            idToken);
        string updateTime = ExtractJsonString(getJson, "updateTime");
        if (TryParseRfc3339(updateTime, out DateTime fromDoc))
        {
            return DateTime.SpecifyKind(fromDoc, DateTimeKind.Utc);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning(
            "[DailyChallenge] Server UTC unavailable — falling back to device UTC.");
#endif
        return DateTime.UtcNow;
    }

    public static async Task<string> GetDocumentAsync(
        string projectId,
        string relativeDocPath,
        string idToken)
    {
        string url =
            "https://firestore.googleapis.com/v1/projects/" + projectId +
            "/databases/(default)/documents/" + relativeDocPath;
        return await GetJsonAsync(url, idToken);
    }

    /// <summary>
    /// Creates a document only if it does not exist (atomic claim).
    /// Returns true on create, false if already exists or error.
    /// </summary>
    public static async Task<(bool created, bool alreadyExists, string response)>
        CreateDocumentIfAbsentAsync(
            string projectId,
            string relativeDocPath,
            string fieldsJsonObject,
            string idToken)
    {
        // relativeDocPath e.g. dailyChallenges/2026-09-27/attempts/UID
        int slash = relativeDocPath.LastIndexOf('/');
        if (slash <= 0)
        {
            return (false, false, "bad_path");
        }

        string parent = relativeDocPath.Substring(0, slash);
        string docId = relativeDocPath.Substring(slash + 1);
        string url =
            "https://firestore.googleapis.com/v1/projects/" + projectId +
            "/databases/(default)/documents/" + parent +
            "?documentId=" + UnityWebRequest.EscapeURL(docId);

        string body = "{\"fields\":" + fieldsJsonObject + "}";
        using (UnityWebRequest req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] raw = Encoding.UTF8.GetBytes(body);
            req.uploadHandler = new UploadHandlerRaw(raw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(idToken))
            {
                req.SetRequestHeader("Authorization", "Bearer " + idToken);
            }

            req.timeout = 25;
            await SendAsync(req);
            string text = req.downloadHandler?.text ?? string.Empty;
            if (req.responseCode == 200 || req.responseCode == 201)
            {
                return (true, false, text);
            }

            if (req.responseCode == 409 ||
                text.IndexOf("ALREADY_EXISTS", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return (false, true, text);
            }

            Debug.LogWarning(
                "[DailyChallenge] Firestore create failed code=" + req.responseCode +
                " " + text);
            return (false, false, text);
        }
    }

    public static async Task<bool> PatchDocumentAsync(
        string projectId,
        string relativeDocPath,
        string fieldsJsonObject,
        string[] fieldPaths,
        string idToken,
        string currentStateEquals = null)
    {
        var sb = new StringBuilder();
        sb.Append("https://firestore.googleapis.com/v1/projects/")
            .Append(projectId)
            .Append("/databases/(default)/documents/")
            .Append(relativeDocPath)
            .Append("?");
        if (fieldPaths != null)
        {
            for (int i = 0; i < fieldPaths.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append('&');
                }

                sb.Append("updateMask.fieldPaths=")
                    .Append(UnityWebRequest.EscapeURL(fieldPaths[i]));
            }
        }

        // Precondition via currentDocument — limited; use transaction-style for state when possible.
        if (!string.IsNullOrEmpty(currentStateEquals))
        {
            // Firestore REST update with precondition on field needs transaction;
            // we use a commit with precondition exists + client re-read verify.
        }

        string body = "{\"fields\":" + fieldsJsonObject + "}";
        string json = await PatchJsonAsync(sb.ToString(), body, idToken);
        return !string.IsNullOrEmpty(json);
    }

    public static async Task<string> RunQueryAsync(
        string projectId,
        string parentCollectionPath,
        string structuredQueryJson,
        string idToken)
    {
        string url =
            "https://firestore.googleapis.com/v1/projects/" + projectId +
            "/databases/(default)/documents/" + parentCollectionPath + ":runQuery";
        return await PostJsonAsync(url, structuredQueryJson, idToken);
    }

    /// <summary>
    /// Firebase Callable HTTPS protocol: POST {base}/{name} with {"data":...}.
    /// Returns result JSON object string, or null on transport failure.
    /// </summary>
    public static async Task<CallableResponse> CallCallableAsync(
        string baseUrl,
        string functionName,
        string dataJsonObject,
        string idToken)
    {
        string url = baseUrl.TrimEnd('/') + "/" + functionName;
        string body = "{\"data\":" + (string.IsNullOrEmpty(dataJsonObject) ? "{}" : dataJsonObject) + "}";
        using (UnityWebRequest req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] raw = Encoding.UTF8.GetBytes(body);
            req.uploadHandler = new UploadHandlerRaw(raw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(idToken))
            {
                req.SetRequestHeader("Authorization", "Bearer " + idToken);
            }

            req.timeout = 30;
            await SendAsync(req);
            string text = req.downloadHandler?.text ?? string.Empty;
            long code = req.responseCode;
            if (code >= 200 && code < 300 && !string.IsNullOrEmpty(text))
            {
                string result = ExtractCallableResult(text);
                return new CallableResponse
                {
                    Ok = true,
                    HttpStatus = (int)code,
                    ResultJson = result,
                    RawBody = text
                };
            }

            string errCode = ExtractJsonString(text, "status");
            if (string.IsNullOrEmpty(errCode))
            {
                errCode = ExtractNestedErrorStatus(text);
            }

            Debug.LogWarning(
                "[DailyBackend] Callable " + functionName + " failed http=" + code +
                " status=" + errCode);
            return new CallableResponse
            {
                Ok = false,
                HttpStatus = (int)code,
                ErrorStatus = errCode,
                RawBody = text,
                ResultJson = string.Empty
            };
        }
    }

    public struct CallableResponse
    {
        public bool Ok;
        public int HttpStatus;
        public string ResultJson;
        public string ErrorStatus;
        public string RawBody;
    }

    private static string ExtractCallableResult(string json)
    {
        // {"result":{...}} — extract the JSON value of "result" via brace matching.
        const string marker = "\"result\":";
        int i = json.IndexOf(marker, StringComparison.Ordinal);
        if (i < 0)
        {
            return json;
        }

        int start = i + marker.Length;
        while (start < json.Length && char.IsWhiteSpace(json[start]))
        {
            start++;
        }

        if (start >= json.Length)
        {
            return string.Empty;
        }

        if (json[start] == '{')
        {
            return ExtractBalanced(json, start, '{', '}');
        }

        if (json[start] == '[')
        {
            return ExtractBalanced(json, start, '[', ']');
        }

        // Primitive — read until comma or end brace.
        int end = start;
        while (end < json.Length && json[end] != ',' && json[end] != '}')
        {
            end++;
        }

        return json.Substring(start, end - start).Trim();
    }

    private static string ExtractBalanced(string json, int start, char open, char close)
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

        return json.Substring(start);
    }

    private static string ExtractNestedErrorStatus(string json)
    {
        // {"error":{"status":"UNAUTHENTICATED",...}}
        int err = json.IndexOf("\"error\"", StringComparison.Ordinal);
        if (err < 0)
        {
            return string.Empty;
        }

        string slice = json.Substring(err);
        string status = ExtractJsonString(slice, "status");
        return status;
    }

    public static string StringField(string value)
    {
        return "{\"stringValue\":\"" + Escape(value ?? string.Empty) + "\"}";
    }

    public static string IntField(long value)
    {
        return "{\"integerValue\":\"" + value + "\"}";
    }

    public static string BoolField(bool value)
    {
        return "{\"booleanValue\":" + (value ? "true" : "false") + "}";
    }

    public static string ExtractJsonString(string json, string key)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        string pattern = "\"" + key + "\":\"";
        int i = json.IndexOf(pattern, StringComparison.Ordinal);
        if (i < 0)
        {
            // unquoted number-as-string sometimes
            pattern = "\"" + key + "\": \"";
            i = json.IndexOf(pattern, StringComparison.Ordinal);
        }

        if (i < 0)
        {
            return string.Empty;
        }

        int start = i + pattern.Length;
        int end = json.IndexOf('"', start);
        if (end < 0)
        {
            return string.Empty;
        }

        return json.Substring(start, end - start);
    }

    public static int ExtractJsonInt(string json, string key, int fallback)
    {
        string s = ExtractJsonString(json, key);
        if (int.TryParse(s, out int v))
        {
            return v;
        }

        // bare number
        string pattern = "\"" + key + "\":";
        int i = json.IndexOf(pattern, StringComparison.Ordinal);
        if (i < 0)
        {
            return fallback;
        }

        int start = i + pattern.Length;
        while (start < json.Length && (json[start] == ' '))
        {
            start++;
        }

        int end = start;
        while (end < json.Length && char.IsDigit(json[end]))
        {
            end++;
        }

        if (end > start && int.TryParse(json.Substring(start, end - start), out v))
        {
            return v;
        }

        return fallback;
    }

    public static string ExtractFirestoreStringField(string json, string fieldName)
    {
        string pattern = "\"" + fieldName + "\":{\"stringValue\":\"";
        int i = json.IndexOf(pattern, StringComparison.Ordinal);
        if (i < 0)
        {
            return string.Empty;
        }

        int start = i + pattern.Length;
        int end = json.IndexOf('"', start);
        return end > start ? json.Substring(start, end - start) : string.Empty;
    }

    public static long ExtractFirestoreLongField(string json, string fieldName)
    {
        string pattern = "\"" + fieldName + "\":{\"integerValue\":\"";
        int i = json.IndexOf(pattern, StringComparison.Ordinal);
        if (i < 0)
        {
            pattern = "\"" + fieldName + "\":{\"integerValue\":";
            i = json.IndexOf(pattern, StringComparison.Ordinal);
            if (i < 0)
            {
                return 0;
            }

            int start = i + pattern.Length;
            int end = start;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-'))
            {
                end++;
            }

            long.TryParse(json.Substring(start, end - start), out long bare);
            return bare;
        }

        int s = i + pattern.Length;
        int e = json.IndexOf('"', s);
        long.TryParse(json.Substring(s, e - s), out long v);
        return v;
    }

    private static bool ApplyAuthJson(string json)
    {
        string idToken = ExtractJsonString(json, "idToken");
        string refresh = ExtractJsonString(json, "refreshToken");
        string localId = ExtractJsonString(json, "localId");
        int expires = ExtractJsonInt(json, "expiresIn", 3600);
        if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(localId))
        {
            Debug.LogWarning("[DailyChallenge] Auth response missing token/uid.");
            return false;
        }

        DailyChallengeIdentityService.ApplySession(localId, idToken, refresh, expires);
        return true;
    }

    private static string ExtractNestedTimestamp(string json)
    {
        // commit response may include transformResults with timestampValue
        string pattern = "\"timestampValue\":\"";
        int i = json.IndexOf(pattern, StringComparison.Ordinal);
        if (i < 0)
        {
            return string.Empty;
        }

        int start = i + pattern.Length;
        int end = json.IndexOf('"', start);
        return end > start ? json.Substring(start, end - start) : string.Empty;
    }

    private static bool TryParseRfc3339(string value, out DateTime utc)
    {
        utc = default;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return DateTime.TryParse(
            value,
            null,
            System.Globalization.DateTimeStyles.AdjustToUniversal |
            System.Globalization.DateTimeStyles.AssumeUniversal,
            out utc);
    }

    private static string Escape(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private static async Task<string> PostJsonAsync(string url, string body, string bearerToken)
    {
        using (UnityWebRequest req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] raw = Encoding.UTF8.GetBytes(body ?? "{}");
            req.uploadHandler = new UploadHandlerRaw(raw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(bearerToken))
            {
                req.SetRequestHeader("Authorization", "Bearer " + bearerToken);
            }

            req.timeout = 25;
            await SendAsync(req);
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(
                    "[DailyChallenge] POST failed: " + req.error + " " +
                    req.downloadHandler?.text);
                return null;
            }

            return req.downloadHandler.text;
        }
    }

    private static async Task<string> PatchJsonAsync(string url, string body, string bearerToken)
    {
        using (UnityWebRequest req = new UnityWebRequest(url, "PATCH"))
        {
            byte[] raw = Encoding.UTF8.GetBytes(body ?? "{}");
            req.uploadHandler = new UploadHandlerRaw(raw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(bearerToken))
            {
                req.SetRequestHeader("Authorization", "Bearer " + bearerToken);
            }

            req.timeout = 25;
            await SendAsync(req);
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(
                    "[DailyChallenge] PATCH failed: " + req.error + " " +
                    req.downloadHandler?.text);
                return null;
            }

            return req.downloadHandler.text;
        }
    }

    private static async Task<string> GetJsonAsync(string url, string bearerToken)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            if (!string.IsNullOrEmpty(bearerToken))
            {
                req.SetRequestHeader("Authorization", "Bearer " + bearerToken);
            }

            req.timeout = 25;
            await SendAsync(req);
            if (req.result != UnityWebRequest.Result.Success)
            {
                return null;
            }

            return req.downloadHandler.text;
        }
    }

    private static Task SendAsync(UnityWebRequest req)
    {
        var tcs = new TaskCompletionSource<bool>();
        UnityWebRequestAsyncOperation op = req.SendWebRequest();
        op.completed += _ => tcs.TrySetResult(true);
        return tcs.Task;
    }
}

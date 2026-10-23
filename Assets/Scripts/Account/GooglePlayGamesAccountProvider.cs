using System;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.Networking;
#endif

/// <summary>
/// Real Google Play Games adapter for optional Firebase account linking (Phase 4.2B).
///
/// Android player only. Editor / iOS / missing config → unavailable (no Play UI).
/// Never authenticates at startup — only when <see cref="AuthenticateForLinkAsync"/> runs
/// after an explicit LINK GOOGLE PLAY tap.
///
/// PGS v2 may already be platform-authenticated (banner at launch). That is OK and is
/// NOT Firebase linking. When already authenticated, we skip ManuallyAuthenticate and
/// request a server auth code directly.
/// </summary>
public sealed class GooglePlayGamesAccountProvider : IGooglePlayAccountProvider
{
    private const string FirebasePlayGamesProviderId = "playgames.google.com";
    private const int AuthenticateTimeoutMs = 45000;
    private const int ServerAuthCodeTimeoutMs = 30000;
    private const int OverallProviderTimeoutMs = 60000;

    public AccountProvider Provider => AccountProvider.GooglePlay;

    public bool IsAvailable
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return IsPlayGamesConfigured();
#else
            return false;
#endif
        }
    }

    public Task<ExternalAccountAuthResult> AuthenticateForLinkAsync()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return WithTimeoutAsync(
            AuthenticateForLinkAndroidAsync(),
            OverallProviderTimeoutMs,
            ExternalAccountAuthResult.Fail(
                AccountLinkResult.NetworkError,
                "Couldn't connect to Google Play Games. Check your internet connection and try again."));
#else
        return Task.FromResult(
            ExternalAccountAuthResult.Fail(
                AccountLinkResult.ProviderUnavailable,
                "Google Play linking is only available on Android device builds."));
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static bool IsPlayGamesConfigured()
    {
        PlayGamesSettings settings = PlayGamesSettings.LoadInstance();
        return settings != null &&
               !string.IsNullOrEmpty(settings.AppId) &&
               !string.IsNullOrEmpty(settings.WebClientId);
    }

    private static async Task<ExternalAccountAuthResult> AuthenticateForLinkAndroidAsync()
    {
        if (!IsPlayGamesConfigured())
        {
            Debug.LogWarning(
                "[Account] Play Games App ID / Web Client ID not configured. " +
                "Run Window → Google Play Games → Setup → Android setup.");
            Debug.Log("[GooglePlayLink] Play Games available: false");
            return ExternalAccountAuthResult.Fail(
                AccountLinkResult.ProviderUnavailable,
                "Google Play Games is unavailable on this device.");
        }

        string stage = "init";
        void OnFocus(bool focused)
        {
            Debug.Log("[GPG-Link] app " + (focused ? "resume" : "pause"));
        }

        Application.focusChanged += OnFocus;
        try
        {
            PlayGamesPlatform.LastSignInStatusCode = 0;
            PlayGamesPlatform.LastSignInStage = "start";
            Debug.Log("[GPG-Link] start auth inflight=yes");
            stage = "PlayGamesPlatform.Activate";
            Debug.Log("[Account] PlayGamesPlatform.Activate starting");
            PlayGamesPlatform.Activate();
            Debug.Log("[Account] PlayGamesPlatform.Activate completed");

            stage = "PGS IsAuthenticated check";
            Debug.Log("[Account] PGS IsAuthenticated check starting");
            bool pluginAuthenticated = PlayGamesPlatform.Instance.IsAuthenticated();
            Debug.Log("[Account] PGS IsAuthenticated=" + pluginAuthenticated);

            // Silent status check (isAuthenticated). Does NOT show sign-in UI.
            // Syncs plugin AuthState with PGS v2 platform auto-auth if already signed in.
            stage = "Silent Authenticate";
            Debug.Log("[GPG-Link] start silent auth");
            Debug.Log("[Account] Silent Authenticate starting");
            bool silentTimedOut = false;
            SignInStatus silent = await WithTimeoutAsync(
                SilentAuthenticateAsync(),
                AuthenticateTimeoutMs,
                SignInStatus.InternalError,
                () => silentTimedOut = true);
            Debug.Log(
                "[GPG-Link] auth callback status=" + silent +
                " stage=" + PlayGamesPlatform.LastSignInStage +
                " statusCode=" + PlayGamesPlatform.LastSignInStatusCode +
                " isAuthenticated=" + PlayGamesPlatform.Instance.IsAuthenticated() +
                " timedOut=" + silentTimedOut);
            Debug.Log("[Account] Silent Authenticate callback=" + silent);
            if (silentTimedOut)
            {
            return ExternalAccountAuthResult.Fail(
                AccountLinkResult.NetworkError,
                "Couldn't connect to Google Play Games. Check your internet connection and try again.");
            }

            bool alreadyAuthenticated =
                silent == SignInStatus.Success ||
                PlayGamesPlatform.Instance.IsAuthenticated();

            Debug.Log("[Account] PGS already authenticated? " + alreadyAuthenticated);
            Debug.Log("[GooglePlayLink] Play Games available: true");
            Debug.Log("[GooglePlayLink] Play Games authenticated: " + alreadyAuthenticated);

            if (!alreadyAuthenticated)
            {
                stage = "Manual Authenticate";
                Debug.Log("[GPG-Link] start manual auth");
                Debug.Log("[Account] Manual Authenticate starting");
                bool manualTimedOut = false;
                SignInStatus status = await WithTimeoutAsync(
                    ManuallyAuthenticateAsync(),
                    AuthenticateTimeoutMs,
                    SignInStatus.InternalError,
                    () => manualTimedOut = true);
                Debug.Log(
                    "[GPG-Link] auth callback status=" + status +
                    " stage=" + PlayGamesPlatform.LastSignInStage +
                    " statusCode=" + PlayGamesPlatform.LastSignInStatusCode +
                    " isAuthenticated=" + PlayGamesPlatform.Instance.IsAuthenticated() +
                    " timedOut=" + manualTimedOut);
                Debug.Log("[Account] Manual Authenticate callback=" + status);

                if (manualTimedOut)
                {
            return ExternalAccountAuthResult.Fail(
                AccountLinkResult.NetworkError,
                "Couldn't connect to Google Play Games. Check your internet connection and try again.");
                }

                if (status != SignInStatus.Success)
                {
                    return SignInFailure(status);
                }
            }
            else
            {
                Debug.Log(
                    "[Account] ManuallyAuthenticate skipped (already authenticated)");
            }

            stage = "RequestServerSideAccess";
            string playerId = null;
            try
            {
                playerId = PlayGamesPlatform.Instance.GetUserId();
            }
            catch (Exception)
            {
                playerId = null;
            }

            Debug.Log(
                "[GPG-Link] isAuthenticated=" + PlayGamesPlatform.Instance.IsAuthenticated() +
                " playerId present=" + (string.IsNullOrEmpty(playerId) ? "no" : "yes"));
            Debug.Log("[GPG-Link] link server auth code request start");
            Debug.Log("[Account] RequestServerSideAccess started");
            string authCode = await WithTimeoutAsync(
                RequestServerAuthCodeAsync(forceRefreshToken: true),
                ServerAuthCodeTimeoutMs,
                null);

            bool codePresent = !string.IsNullOrEmpty(authCode);
            Debug.Log(
                "[Account] RequestServerSideAccess callback received: codePresent=" +
                codePresent);
            Debug.Log(
                "[GooglePlayLink] server auth code received: " +
                (codePresent ? "yes" : "no") +
                " length=" + (authCode == null ? 0 : authCode.Length));

            if (!codePresent)
            {
                Debug.Log("[GPG-Link] server auth code unavailable");
                return ExternalAccountAuthResult.Fail(
                    AccountLinkResult.AuthenticationError,
                    "Google Play Games sign-in failed. Make sure you're signed in to Google Play Games and try again.");
            }

            stage = "GetUserDisplayName";
            string displayName = null;
            try
            {
                displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
            }
            catch (Exception displayEx)
            {
                LogAndroidAuthException("GetUserDisplayName", displayEx);
                displayName = null;
            }

            // PlayGamesAuthProvider.GetCredential sends the server auth code as "code".
            // Do not persist or log the auth code.
            string postBody =
                "code=" + UnityWebRequest.EscapeURL(authCode) +
                "&providerId=" + UnityWebRequest.EscapeURL(FirebasePlayGamesProviderId);

            var credential = new ExternalAccountCredential
            {
                Provider = AccountProvider.GooglePlay,
                FirebaseProviderId = FirebasePlayGamesProviderId,
                IdpPostBody = postBody,
                DisplayNameHint = SanitizePublicName(displayName)
            };

            return ExternalAccountAuthResult.Ok(credential);
        }
        catch (Exception ex)
        {
            LogAndroidAuthException(stage, ex);
            return ExternalAccountAuthResult.Fail(
                AccountLinkResult.AuthenticationError,
                "Google Play Games sign-in failed. Make sure you're signed in to Google Play Games and try again.");
        }
        finally
        {
            Application.focusChanged -= OnFocus;
        }
    }

    private static ExternalAccountAuthResult SignInFailure(SignInStatus status)
    {
        int code = PlayGamesPlatform.LastSignInStatusCode;
        Debug.Log(
            "[GPG-Link] sign-in failed status=" + status +
            " stage=" + PlayGamesPlatform.LastSignInStage +
            " statusCode=" + code);
        if (status == SignInStatus.Canceled || code == 16 || code == 12501)
        {
            Debug.Log("[GPG-Link] player cancelled sign-in");
            return ExternalAccountAuthResult.Fail(
                AccountLinkResult.Cancelled,
                "Google Play Games sign-in was cancelled.");
        }

        if (code == 7)
        {
            Debug.Log("[GPG-Link] network statusCode=7");
            return ExternalAccountAuthResult.Fail(
                AccountLinkResult.NetworkError,
                "Couldn't connect to Google Play Games. Check your internet connection and try again.");
        }

        if (code == 10)
        {
            Debug.Log(
                "[GPG-Link] developer error statusCode=10. " +
                "Signing certificate for this install may not be registered.");
            return ExternalAccountAuthResult.Fail(
                AccountLinkResult.AuthenticationError,
                "Google Play Games sign-in is temporarily unavailable. Please try again later.");
        }

        if (string.Equals(PlayGamesPlatform.LastSignInStage, "isAuthenticatedFalse", StringComparison.Ordinal))
        {
            Debug.Log(
                "[GPG-Link] isAuthenticated=false after sign-in UI. " +
                "Server auth code was not requested. Firebase REST was not reached. " +
                "Possible causes: user dismissed the flow, or this install's certificate is not registered.");
            return ExternalAccountAuthResult.Fail(
                AccountLinkResult.AuthenticationError,
                "Google Play Games sign-in is temporarily unavailable. Please try again later.");
        }

        Debug.Log("[GPG-Link] authentication failed without a more specific status");
        return ExternalAccountAuthResult.Fail(
            AccountLinkResult.AuthenticationError,
            "Google Play Games sign-in failed. Make sure you're signed in to Google Play Games and try again.");
    }

    /// <summary>
    /// Silent auth status (GamesSignInClient.isAuthenticated). No interactive UI.
    /// </summary>
    private static Task<SignInStatus> SilentAuthenticateAsync()
    {
        var tcs = new TaskCompletionSource<SignInStatus>();
        try
        {
            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                tcs.TrySetResult(status);
            });
        }
        catch (Exception ex)
        {
            LogAndroidAuthException("Silent Authenticate invoke", ex);
            tcs.TrySetResult(SignInStatus.InternalError);
        }

        return tcs.Task;
    }

    private static Task<SignInStatus> ManuallyAuthenticateAsync()
    {
        var tcs = new TaskCompletionSource<SignInStatus>();
        try
        {
            PlayGamesPlatform.Instance.ManuallyAuthenticate(status =>
            {
                tcs.TrySetResult(status);
            });
        }
        catch (Exception ex)
        {
            LogAndroidAuthException("Manual Authenticate invoke", ex);
            tcs.TrySetResult(SignInStatus.InternalError);
        }

        return tcs.Task;
    }

    private static Task<string> RequestServerAuthCodeAsync(bool forceRefreshToken)
    {
        var tcs = new TaskCompletionSource<string>();
        try
        {
            // Uses Web/Game Server OAuth client from PlayGamesSettings.WebClientId.
            PlayGamesPlatform.Instance.RequestServerSideAccess(
                forceRefreshToken,
                code => { tcs.TrySetResult(code); });
        }
        catch (Exception ex)
        {
            LogAndroidAuthException("RequestServerSideAccess invoke", ex);
            tcs.TrySetResult(null);
        }

        return tcs.Task;
    }

    private static async Task<T> WithTimeoutAsync<T>(
        Task<T> task,
        int timeoutMs,
        T timeoutValue,
        Action onTimeout = null)
    {
        if (task == null)
        {
            return timeoutValue;
        }

        Task delay = Task.Delay(timeoutMs);
        Task completed = await Task.WhenAny(task, delay);
        if (completed != task)
        {
            Debug.LogWarning("[GPG-Link] timed out after " + timeoutMs + "ms");
            if (onTimeout != null)
            {
                onTimeout();
            }

            return timeoutValue;
        }

        return await task;
    }

    /// <summary>
    /// Diagnostic-only logging for AndroidJavaException / auth failures.
    /// Never logs tokens, auth codes, secrets, or player IDs.
    /// </summary>
    private static void LogAndroidAuthException(string stage, Exception ex)
    {
        if (ex == null)
        {
            Debug.LogWarning(
                "[Account] Google Play auth exception at stage=" + stage + " (null)");
            return;
        }

        Debug.LogWarning(
            "[Account] Google Play auth exception at stage=" + stage +
            " type=" + ex.GetType().FullName);

        string message = ex.Message;
        if (!string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("[Account] Exception.Message: " + message);
        }

        string stack = ex.StackTrace;
        if (!string.IsNullOrEmpty(stack))
        {
            Debug.LogWarning("[Account] Exception.StackTrace: " + stack);
        }

        // AndroidJavaException.ToString() includes the Java exception class + message +
        // Java stack when Unity surfaces it — still no auth secrets.
        if (ex is AndroidJavaException)
        {
            string javaDetails = ex.ToString();
            if (!string.IsNullOrEmpty(javaDetails))
            {
                Debug.LogWarning("[Account] AndroidJavaException details: " + javaDetails);
            }
        }

        Exception inner = ex.InnerException;
        if (inner != null)
        {
            Debug.LogWarning(
                "[Account] InnerException type=" + inner.GetType().FullName +
                " message=" + (inner.Message ?? string.Empty));
            if (!string.IsNullOrEmpty(inner.StackTrace))
            {
                Debug.LogWarning("[Account] InnerException.StackTrace: " + inner.StackTrace);
            }
        }
    }
#endif

    /// <summary>
    /// Own public Play Games name when this device is already authenticated.
    /// Does not start a sign-in UI and does not read other players.
    /// </summary>
    public static string TryReadAuthenticatedDisplayName()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            PlayGamesPlatform platform = PlayGamesPlatform.Instance;
            if (platform == null || !platform.IsAuthenticated())
            {
                return string.Empty;
            }

            string name = platform.GetUserDisplayName();
            return SanitizePublicName(name);
        }
        catch (Exception ex)
        {
            Debug.Log("[GPG-Link] own display name unavailable type=" + ex.GetType().Name);
            return string.Empty;
        }
#else
        return string.Empty;
#endif
    }

    public static string SanitizePublicName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var builder = new System.Text.StringBuilder(value.Length);
        for (int i = 0; i < value.Length && builder.Length < 24; i++)
        {
            char c = value[i];
            if (c < ' ' || c == '@')
            {
                continue;
            }

            builder.Append(c);
        }

        return builder.ToString().Trim();
    }
}

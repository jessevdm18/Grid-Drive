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
/// </summary>
public sealed class GooglePlayGamesAccountProvider : IGooglePlayAccountProvider
{
    private const string FirebasePlayGamesProviderId = "playgames.google.com";

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
        return AuthenticateForLinkAndroidAsync();
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
            return ExternalAccountAuthResult.Fail(
                AccountLinkResult.ProviderUnavailable,
                "Google Play is not configured in this build.");
        }

        try
        {
            // Activate platform binding only when the user explicitly links.
            // Do not call Authenticate() here — that path is for silent status checks.
            PlayGamesPlatform.Activate();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[Account] Google Play auth started");
#endif

            SignInStatus status = await ManuallyAuthenticateAsync();
            if (status == SignInStatus.Canceled)
            {
                return ExternalAccountAuthResult.Fail(AccountLinkResult.Cancelled);
            }

            if (status != SignInStatus.Success)
            {
                return ExternalAccountAuthResult.Fail(
                    AccountLinkResult.AuthenticationError,
                    "Google Play authentication failed.");
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[Account] Google Play auth success");
#endif

            string authCode = await RequestServerAuthCodeAsync(forceRefreshToken: true);
            if (string.IsNullOrEmpty(authCode))
            {
                return ExternalAccountAuthResult.Fail(
                    AccountLinkResult.AuthenticationError,
                    "Could not get Google Play server auth code.");
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[Account] Server auth code received");
#endif

            string displayName = null;
            try
            {
                displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
            }
            catch (Exception)
            {
                displayName = null;
            }

            // Identity Toolkit postBody: authCode + playgames.google.com
            // Do not persist or log the auth code.
            string postBody =
                "authCode=" + UnityWebRequest.EscapeURL(authCode) +
                "&providerId=" + UnityWebRequest.EscapeURL(FirebasePlayGamesProviderId);

            var credential = new ExternalAccountCredential
            {
                Provider = AccountProvider.GooglePlay,
                FirebaseProviderId = FirebasePlayGamesProviderId,
                IdpPostBody = postBody,
                DisplayNameHint = displayName
            };

            return ExternalAccountAuthResult.Ok(credential);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Account] Google Play auth exception: " + ex.GetType().Name);
            return ExternalAccountAuthResult.Fail(
                AccountLinkResult.AuthenticationError,
                "Could not connect. Try again.");
        }
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
            Debug.LogWarning("[Account] ManuallyAuthenticate failed: " + ex.GetType().Name);
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
            Debug.LogWarning("[Account] RequestServerSideAccess failed: " + ex.GetType().Name);
            tcs.TrySetResult(null);
        }

        return tcs.Task;
    }
#endif
}

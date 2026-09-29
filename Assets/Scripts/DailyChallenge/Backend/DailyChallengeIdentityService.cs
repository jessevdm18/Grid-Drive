using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Firebase identity for Rush Out / Daily Challenge.
///
/// Startup: Anonymous Auth automatically — no provider popup.
/// Optional later: LinkGooglePlayAsync links a credential to the EXISTING Firebase user.
///
/// Ownership key is always the Firebase UID, never provider IDs.
///
/// Rush Out Account
///       |
/// Firebase UID  ← Daily attempts / results / leaderboard / future rewards
///       |
/// +-----+------------------+
/// |                        |
/// Anonymous (startup)   Linked providers (optional)
///                          |
///                 Google Play Games
///                 Sign in with Apple (future)
///
/// Successful linking MUST preserve Firebase UID. If UID changes, treat as failure.
///
/// LIMITATION: Anonymous Auth does not map one human → one UID across fresh installs.
/// </summary>
public static class DailyChallengeIdentityService
{
    private const string RefreshTokenKey = "RushOut_Daily_FirebaseRefreshToken";
    private const string LocalUidKey = "RushOut_Daily_FirebaseUid";
    private const string PresentationLinkCacheKey = "RushOut_Account_LinkStateCache";

    private static string cachedUserId;
    private static string cachedIdToken;
    private static DateTime idTokenExpiresUtc = DateTime.MinValue;
    private static bool signInInFlight;
    private static Task<bool> signInTask;
    private static bool linkInFlight;

    private static readonly List<AccountProvider> linkedProviders = new List<AccountProvider>();
    private static AccountLinkState linkState = AccountLinkState.Unknown;
    private static string optionalDisplayName = string.Empty;

    private static IGooglePlayAccountProvider googlePlayProvider =
        new UnavailableGooglePlayAccountProvider();

    private static IAppleAccountProvider appleProvider = new UnavailableAppleAccountProvider();

    public static bool IsReady => !string.IsNullOrEmpty(cachedUserId) &&
                                 !string.IsNullOrEmpty(cachedIdToken) &&
                                 DateTime.UtcNow < idTokenExpiresUtc;

    /// <summary>Firebase UID — the only identity key Daily backend trusts.</summary>
    public static string UserId => cachedUserId ?? string.Empty;

    public static string IdToken => cachedIdToken ?? string.Empty;

    /// <summary>True when Firebase reports no federated providers (guest).</summary>
    public static bool IsAnonymous =>
        linkState == AccountLinkState.GuestAnonymous ||
        linkState == AccountLinkState.Unknown ||
        (linkedProviders.Count == 0 ||
         (linkedProviders.Count == 1 && linkedProviders[0] == AccountProvider.Anonymous));

    public static AccountLinkState LinkState => linkState;

    public static IReadOnlyList<AccountProvider> LinkedProviders => linkedProviders;

    /// <summary>Presentation-only. Never used as ownership key.</summary>
    public static string OptionalDisplayName => optionalDisplayName ?? string.Empty;

    public static bool IsGooglePlayProviderAvailable =>
        googlePlayProvider != null && googlePlayProvider.IsAvailable;

    /// <summary>Inject Play Games adapter when plugin is installed. Editor-safe default is Unavailable.</summary>
    public static void SetGooglePlayAccountProvider(IGooglePlayAccountProvider provider)
    {
        googlePlayProvider = provider ?? new UnavailableGooglePlayAccountProvider();
    }

    public static void SetAppleAccountProvider(IAppleAccountProvider provider)
    {
        appleProvider = provider ?? new UnavailableAppleAccountProvider();
    }

    public static async Task<bool> EnsureSignedInAsync()
    {
        if (IsReady)
        {
            return true;
        }

        if (signInInFlight && signInTask != null)
        {
            return await signInTask;
        }

        signInInFlight = true;
        signInTask = SignInInternalAsync();
        try
        {
            bool ok = await signInTask;
            if (ok)
            {
                await RefreshAccountStateAsync();
            }

            return ok;
        }
        finally
        {
            signInInFlight = false;
            signInTask = null;
        }
    }

    /// <summary>
    /// Refresh ID token using stored refresh token. Does NOT create a new anonymous user.
    /// </summary>
    public static async Task<bool> TryRefreshSessionAsync()
    {
        DailyChallengeFirebaseSettings settings = DailyChallengeFirebaseSettings.LoadDefault();
        if (settings == null || !settings.IsConfigured)
        {
            return false;
        }

        string refresh = PlayerPrefs.GetString(RefreshTokenKey, string.Empty);
        if (string.IsNullOrEmpty(refresh))
        {
            return false;
        }

        bool refreshed = await FirebaseRestClient.TryRefreshIdTokenAsync(
            settings.ApiKey,
            refresh);
        return refreshed && IsReady;
    }

    /// <summary>
    /// Reload provider linkage from Firebase (authoritative). Presentation cache is non-authoritative.
    /// </summary>
    public static async Task<bool> RefreshAccountStateAsync()
    {
        if (!IsReady)
        {
            bool signedIn = await EnsureSignedInAsync();
            if (!signedIn)
            {
                linkState = AccountLinkState.Error;
                return false;
            }
        }

        DailyChallengeFirebaseSettings settings = DailyChallengeFirebaseSettings.LoadDefault();
        if (settings == null || !settings.IsConfigured)
        {
            linkState = AccountLinkState.Error;
            return false;
        }

        var info = await FirebaseRestClient.LookupAccountInfoAsync(
            settings.ApiKey,
            IdToken);

        if (!info.Ok)
        {
            // Expired token — refresh once and retry lookup.
            if (await TryRefreshSessionAsync())
            {
                info = await FirebaseRestClient.LookupAccountInfoAsync(
                    settings.ApiKey,
                    IdToken);
            }
        }

        if (!info.Ok)
        {
            // Keep last-known presentation; do not invent linked state.
            if (linkState == AccountLinkState.Unknown)
            {
                linkState = AccountLinkState.GuestAnonymous;
                linkedProviders.Clear();
                linkedProviders.Add(AccountProvider.Anonymous);
            }

            return false;
        }

        ApplyProviderInfo(info);
        CachePresentationState();
        return true;
    }

    /// <summary>
    /// Optional Google Play link to the EXISTING Firebase user.
    /// Must preserve Firebase UID. Never auto-runs at startup.
    /// </summary>
    public static async Task<AccountLinkOutcome> LinkGooglePlayAsync()
    {
        const string providerKey = "google_play";

        if (linkInFlight)
        {
            return AccountLinkOutcome.From(AccountLinkResult.UnknownError, providerKey, "Linking in progress.");
        }

        linkInFlight = true;
        AccountLinkState previousState = linkState;
        try
        {
            GameAnalytics.LogAccountLinkStarted(providerKey);

            if (!await EnsureSignedInAsync())
            {
                var fail = AccountLinkOutcome.From(
                    AccountLinkResult.AuthenticationError,
                    providerKey);
                GameAnalytics.LogAccountLinkFailed(providerKey, fail.Result.ToString());
                return fail;
            }

            await RefreshAccountStateAsync();
            if (HasProvider(AccountProvider.GooglePlay))
            {
                var already = AccountLinkOutcome.From(AccountLinkResult.AlreadyLinked, providerKey);
                GameAnalytics.LogAccountLinkSuccess(providerKey);
                return already;
            }

            if (googlePlayProvider == null || !googlePlayProvider.IsAvailable)
            {
                var unavailable = AccountLinkOutcome.From(
                    AccountLinkResult.ProviderUnavailable,
                    providerKey);
                GameAnalytics.LogAccountLinkFailed(providerKey, unavailable.Result.ToString());
                return unavailable;
            }

            string uidBefore = UserId;
            if (string.IsNullOrEmpty(uidBefore))
            {
                var authFail = AccountLinkOutcome.From(
                    AccountLinkResult.AuthenticationError,
                    providerKey);
                GameAnalytics.LogAccountLinkFailed(providerKey, authFail.Result.ToString());
                return authFail;
            }

            linkState = AccountLinkState.Linking;

            ExternalAccountAuthResult auth = await googlePlayProvider.AuthenticateForLinkAsync();
            if (auth.Result != AccountLinkResult.Success || auth.Credential == null)
            {
                linkState = previousState == AccountLinkState.Linking
                    ? AccountLinkState.GuestAnonymous
                    : previousState;
                var providerFail = AccountLinkOutcome.From(
                    auth.Result == AccountLinkResult.Success
                        ? AccountLinkResult.AuthenticationError
                        : auth.Result,
                    providerKey,
                    string.IsNullOrEmpty(auth.Message)
                        ? null
                        : auth.Message);
                GameAnalytics.LogAccountLinkFailed(providerKey, providerFail.Result.ToString());
                return providerFail;
            }

            DailyChallengeFirebaseSettings settings = DailyChallengeFirebaseSettings.LoadDefault();
            FirebaseRestClient.FirebaseLinkIdpResult link =
                await FirebaseRestClient.TryLinkIdpAsync(
                    settings.ApiKey,
                    IdToken,
                    auth.Credential);

            // Retry once after token refresh on auth failure.
            if (link.Result == AccountLinkResult.AuthenticationError &&
                await TryRefreshSessionAsync())
            {
                link = await FirebaseRestClient.TryLinkIdpAsync(
                    settings.ApiKey,
                    IdToken,
                    auth.Credential);
            }

            if (link.Result != AccountLinkResult.Success)
            {
                linkState = previousState == AccountLinkState.Linking
                    ? AccountLinkState.GuestAnonymous
                    : previousState;
                var fail = AccountLinkOutcome.From(link.Result, providerKey);
                GameAnalytics.LogAccountLinkFailed(providerKey, fail.Result.ToString());
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning(
                    "[Account] Google Play link failed: " + link.Result +
                    " code=" + link.ErrorCode);
#endif
                return fail;
            }

            // CRITICAL: Firebase UID must remain the same.
            if (!string.Equals(link.LocalId, uidBefore, StringComparison.Ordinal))
            {
                Debug.LogError(
                    "[Account] IDENTITY CONTINUITY ERROR: Firebase UID changed during Google Play link. " +
                    "Refusing to adopt new session. Daily data ownership is not guaranteed.");
                linkState = AccountLinkState.Error;
                var continuity = AccountLinkOutcome.From(
                    AccountLinkResult.IdentityContinuityError,
                    providerKey);
                GameAnalytics.LogAccountLinkFailed(providerKey, continuity.Result.ToString());
                return continuity;
            }

            ApplySession(
                link.LocalId,
                link.IdToken,
                string.IsNullOrEmpty(link.RefreshToken)
                    ? PlayerPrefs.GetString(RefreshTokenKey, string.Empty)
                    : link.RefreshToken,
                link.ExpiresInSeconds);

            if (!string.IsNullOrEmpty(auth.Credential.DisplayNameHint))
            {
                optionalDisplayName = auth.Credential.DisplayNameHint;
            }

            await RefreshAccountStateAsync();

            if (!HasProvider(AccountProvider.GooglePlay))
            {
                // Link API succeeded but lookup didn't show provider — still treat as soft success
                // if UID matched; mark LinkedGooglePlay for presentation.
                linkedProviders.Clear();
                linkedProviders.Add(AccountProvider.Anonymous);
                linkedProviders.Add(AccountProvider.GooglePlay);
                linkState = AccountLinkState.LinkedGooglePlay;
                CachePresentationState();
            }

            GameAnalytics.LogAccountLinkSuccess(providerKey);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[Account] Firebase provider link success");
            Debug.Log(
                "[Account] UID continuity verified (uidLength=" + uidBefore.Length + ")");
#endif
            return AccountLinkOutcome.From(AccountLinkResult.Success, providerKey);
        }
        finally
        {
            linkInFlight = false;
            if (linkState == AccountLinkState.Linking)
            {
                linkState = previousState;
            }
        }
    }

    /// <summary>Future Apple link — not implemented in Phase 4.2A.</summary>
    public static Task<AccountLinkOutcome> LinkAppleAsync()
    {
        const string providerKey = "apple";
        GameAnalytics.LogAccountLinkStarted(providerKey);
        var outcome = AccountLinkOutcome.From(AccountLinkResult.ProviderUnavailable, providerKey);
        GameAnalytics.LogAccountLinkFailed(providerKey, outcome.Result.ToString());
        return Task.FromResult(outcome);
    }

    /// <summary>
    /// V1: Sign-out is not supported for anonymous-first accounts (would orphan progress).
    /// </summary>
    public static Task<bool> SignOutAsync()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning(
            "[Account] SignOutAsync is not supported in V1 anonymous-first identity. " +
            "Signing out would risk losing Daily progress for this install.");
#endif
        return Task.FromResult(false);
    }

    internal static void ApplySession(
        string userId,
        string idToken,
        string refreshToken,
        int expiresInSeconds)
    {
        cachedUserId = userId ?? string.Empty;
        cachedIdToken = idToken ?? string.Empty;
        int safeExpires = Mathf.Max(60, expiresInSeconds - 60);
        idTokenExpiresUtc = DateTime.UtcNow.AddSeconds(safeExpires);

        if (!string.IsNullOrEmpty(userId))
        {
            PlayerPrefs.SetString(LocalUidKey, userId);
        }

        if (!string.IsNullOrEmpty(refreshToken))
        {
            PlayerPrefs.SetString(RefreshTokenKey, refreshToken);
        }

        PlayerPrefs.Save();
    }

    public static void ClearLocalSessionForTests()
    {
        cachedUserId = null;
        cachedIdToken = null;
        idTokenExpiresUtc = DateTime.MinValue;
        linkedProviders.Clear();
        linkState = AccountLinkState.Unknown;
        optionalDisplayName = string.Empty;
        PlayerPrefs.DeleteKey(RefreshTokenKey);
        PlayerPrefs.DeleteKey(LocalUidKey);
        PlayerPrefs.DeleteKey(PresentationLinkCacheKey);
        PlayerPrefs.Save();
    }

    private static async Task<bool> SignInInternalAsync()
    {
        DailyChallengeFirebaseSettings settings = DailyChallengeFirebaseSettings.LoadDefault();
        if (settings == null || !settings.IsConfigured)
        {
            Debug.LogWarning(
                "[DailyChallenge] Firebase settings missing — identity unavailable.");
            return false;
        }

        string refresh = PlayerPrefs.GetString(RefreshTokenKey, string.Empty);
        if (!string.IsNullOrEmpty(refresh))
        {
            bool refreshed = await FirebaseRestClient.TryRefreshIdTokenAsync(
                settings.ApiKey,
                refresh);
            if (refreshed)
            {
                return IsReady;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(
                "[DailyChallenge] Refresh failed — attempting anonymous sign-in. " +
                "A new UID means a new Daily identity (anonymous multi-device limitation).");
#endif
        }

        // Automatic anonymous — no Google Play / Apple prompt.
        bool signedUp = await FirebaseRestClient.TryAnonymousSignUpAsync(settings.ApiKey);
        if (signedUp)
        {
            linkState = AccountLinkState.GuestAnonymous;
            linkedProviders.Clear();
            linkedProviders.Add(AccountProvider.Anonymous);
        }

        return signedUp && IsReady;
    }

    private static void ApplyProviderInfo(FirebaseRestClient.FirebaseAccountInfo info)
    {
        linkedProviders.Clear();
        if (info.Providers != null && info.Providers.Length > 0)
        {
            for (int i = 0; i < info.Providers.Length; i++)
            {
                if (!linkedProviders.Contains(info.Providers[i]))
                {
                    linkedProviders.Add(info.Providers[i]);
                }
            }
        }
        else
        {
            linkedProviders.Add(AccountProvider.Anonymous);
        }

        bool hasPlay = HasProvider(AccountProvider.GooglePlay);
        bool hasApple = HasProvider(AccountProvider.Apple);

        if (hasPlay && hasApple)
        {
            linkState = AccountLinkState.LinkedMultiple;
        }
        else if (hasPlay)
        {
            linkState = AccountLinkState.LinkedGooglePlay;
        }
        else if (hasApple)
        {
            linkState = AccountLinkState.LinkedApple;
        }
        else
        {
            linkState = AccountLinkState.GuestAnonymous;
        }
    }

    private static bool HasProvider(AccountProvider provider)
    {
        for (int i = 0; i < linkedProviders.Count; i++)
        {
            if (linkedProviders[i] == provider)
            {
                return true;
            }
        }

        return false;
    }

    private static void CachePresentationState()
    {
        // Non-authoritative presentation hint only.
        PlayerPrefs.SetInt(PresentationLinkCacheKey, (int)linkState);
        PlayerPrefs.Save();
    }
}

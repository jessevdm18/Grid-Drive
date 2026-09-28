using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Firebase identity for Daily Challenge. V1 uses Anonymous Auth.
///
/// IMPORTANT — ownership key is always the Firebase UID, never the provider type.
///
/// Rush Out Account
///       |
/// Firebase UID  &lt;-- Daily attempts / results / leaderboard ownership
///       |
/// +-----+------------------+
/// |                        |
/// Anonymous (V1 now)    Linked providers (future)
///                          |
///                 Google Play Games
///                 Sign in with Apple
///
/// When a future LinkGooglePlayGamesAsync / LinkAppleAsync preserves the same
/// Firebase UID, all Daily data remains attached automatically — no backend redesign.
///
/// LIMITATION: Anonymous Auth does NOT guarantee the same human maps to one UID across
/// fresh installs/devices. "One attempt per day" means one attempt per Firebase UID.
/// </summary>
public static class DailyChallengeIdentityService
{
    private const string RefreshTokenKey = "RushOut_Daily_FirebaseRefreshToken";
    private const string LocalUidKey = "RushOut_Daily_FirebaseUid";

    private static string cachedUserId;
    private static string cachedIdToken;
    private static DateTime idTokenExpiresUtc = DateTime.MinValue;
    private static bool signInInFlight;
    private static Task<bool> signInTask;

    public static bool IsReady => !string.IsNullOrEmpty(cachedUserId) &&
                                 !string.IsNullOrEmpty(cachedIdToken) &&
                                 DateTime.UtcNow < idTokenExpiresUtc;

    /// <summary>Firebase UID — the only identity key Daily backend trusts.</summary>
    public static string UserId => cachedUserId ?? string.Empty;

    public static string IdToken => cachedIdToken ?? string.Empty;

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
            return await signInTask;
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

    /*
     * FUTURE (do not implement in Phase 4.1B):
     *
     * public static Task<bool> LinkGooglePlayGamesAsync(...)
     * public static Task<bool> LinkAppleAsync(...)
     *
     * These must link credentials to the EXISTING Firebase user so UserId stays stable.
     * Daily backend continues to key attempts by Firebase UID only.
     */

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

            // Refresh failed — do not silently mint a new anonymous UID if we still
            // have a stored UID expectation; try one anonymous sign-up as last resort
            // for brand-new / wiped refresh tokens.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(
                "[DailyChallenge] Refresh failed — attempting anonymous sign-in. " +
                "A new UID means a new Daily identity (anonymous multi-device limitation).");
#endif
        }

        bool signedUp = await FirebaseRestClient.TryAnonymousSignUpAsync(settings.ApiKey);
        return signedUp && IsReady;
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
        PlayerPrefs.DeleteKey(RefreshTokenKey);
        PlayerPrefs.DeleteKey(LocalUidKey);
        PlayerPrefs.Save();
    }
}

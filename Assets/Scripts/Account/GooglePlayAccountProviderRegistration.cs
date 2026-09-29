using UnityEngine;

/// <summary>
/// Registers the real Google Play Games account provider on Android player builds only.
/// Does NOT authenticate, open UI, or call Play Games at startup — only swaps the adapter.
/// </summary>
public static class GooglePlayAccountProviderRegistration
{
#if UNITY_ANDROID && !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterAndroidProvider()
    {
        DailyChallengeIdentityService.SetGooglePlayAccountProvider(
            new GooglePlayGamesAccountProvider());
    }
#endif
}

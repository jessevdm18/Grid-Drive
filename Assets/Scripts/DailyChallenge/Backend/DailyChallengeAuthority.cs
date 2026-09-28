using UnityEngine;

/// <summary>
/// Resolves the active <see cref="IDailyChallengeAuthority"/> from config backend mode.
/// </summary>
public static class DailyChallengeAuthority
{
    private static IDailyChallengeAuthority cached;

    public static IDailyChallengeAuthority Current
    {
        get
        {
            if (cached != null)
            {
                return cached;
            }

            DailyChallengeConfig config = DailyChallengeConfig.LoadDefault();
            DailyChallengeBackendMode mode = config != null
                ? config.BackendMode
                : DailyChallengeBackendMode.SimulatedLocal;

#if UNITY_EDITOR
            // Editor defaults to local unless Online is explicitly selected on config.
            if (mode == DailyChallengeBackendMode.Online)
            {
                DailyChallengeFirebaseSettings settings =
                    DailyChallengeFirebaseSettings.LoadDefault();
                if (settings == null || !settings.IsConfigured)
                {
                    Debug.LogWarning(
                        "[DailyChallenge] Online mode selected but Firebase settings " +
                        "missing — using SimulatedLocal.");
                    mode = DailyChallengeBackendMode.SimulatedLocal;
                }
            }
#endif

            cached = mode == DailyChallengeBackendMode.Online
                ? (IDailyChallengeAuthority)new FirebaseOnlineDailyChallengeAuthority()
                : new LocalSimulatedDailyChallengeAuthority();
            return cached;
        }
        set
        {
            cached = value;
        }
    }

    public static void ResetCached()
    {
        cached = null;
    }

    public static bool IsOnlineMode
    {
        get
        {
            DailyChallengeConfig config = DailyChallengeConfig.LoadDefault();
            return config != null &&
                   config.BackendMode == DailyChallengeBackendMode.Online &&
                   Current is FirebaseOnlineDailyChallengeAuthority;
        }
    }
}

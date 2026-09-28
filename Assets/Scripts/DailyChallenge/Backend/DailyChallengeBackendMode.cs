/// <summary>
/// Selects Daily Challenge authority source.
/// Editor defaults to SimulatedLocal unless Online is explicitly enabled.
/// </summary>
public enum DailyChallengeBackendMode
{
    /// <summary>Phase 1–3 local PlayerPrefs authority (DEV / offline UI).</summary>
    SimulatedLocal = 0,

    /// <summary>Firebase-backed authoritative attempts + results.</summary>
    Online = 1
}

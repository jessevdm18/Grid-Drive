using UnityEngine;

/// <summary>
/// Client Firebase settings for Daily Challenge (Auth REST + Callable Functions + Firestore reads).
/// API keys in client apps are expected; authoritative mutations are Cloud Functions only.
/// </summary>
[CreateAssetMenu(
    fileName = "DailyChallengeFirebaseSettings",
    menuName = "RushOut/Daily Challenge Firebase Settings")]
public class DailyChallengeFirebaseSettings : ScriptableObject
{
    public const string ResourcesPath = "DailyChallengeFirebaseSettings";

    [SerializeField] private string projectId = "grid-drive";
    [SerializeField] private string apiKey = "";

    [Tooltip("Must match Cloud Functions setGlobalOptions region (Phase 4.1B: europe-west1 for eur3).")]
    [SerializeField] private string functionsRegion = "europe-west1";

    [Header("Emulator (DEV only)")]
    [SerializeField] private bool useFunctionsEmulator = false;
    [SerializeField] private string functionsEmulatorHost = "127.0.0.1";
    [SerializeField] private int functionsEmulatorPort = 5001;

    public string ProjectId => projectId;
    public string ApiKey => apiKey;
    public string FunctionsRegion =>
        string.IsNullOrEmpty(functionsRegion) ? "europe-west1" : functionsRegion;

    public bool UseFunctionsEmulator => useFunctionsEmulator;
    public string FunctionsEmulatorHost => functionsEmulatorHost;
    public int FunctionsEmulatorPort => functionsEmulatorPort;

    public bool IsConfigured =>
        !string.IsNullOrEmpty(projectId) && !string.IsNullOrEmpty(apiKey);

    /// <summary>
    /// Base URL for callable HTTPS functions.
    /// Production: https://{region}-{project}.cloudfunctions.net
    /// Emulator: http://{host}:{port}/{project}/{region}
    /// </summary>
    public string GetCallableBaseUrl()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        // Never allow emulator endpoints in release player builds.
        return "https://" + FunctionsRegion + "-" + projectId + ".cloudfunctions.net";
#else
        if (useFunctionsEmulator &&
            !string.IsNullOrEmpty(functionsEmulatorHost) &&
            functionsEmulatorPort > 0)
        {
            return "http://" + functionsEmulatorHost + ":" + functionsEmulatorPort +
                   "/" + projectId + "/" + FunctionsRegion;
        }

        return "https://" + FunctionsRegion + "-" + projectId + ".cloudfunctions.net";
#endif
    }

    public static DailyChallengeFirebaseSettings LoadDefault()
    {
        return Resources.Load<DailyChallengeFirebaseSettings>(ResourcesPath);
    }
}

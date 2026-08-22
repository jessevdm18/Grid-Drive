using UnityEngine;

/// <summary>
/// Zero-dependency probe. Must NOT reference FirebaseManager or Firebase.*
/// so RuntimeInitializeOnLoadMethod registers even when Firebase types fail to load.
/// </summary>
public static class FirebaseBootstrapProbe
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void LogSubsystemRegistration()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[FirebaseInit] SUBSYSTEM RESET FIRED");
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LogBeforeSceneLoad()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[FirebaseInit] BOOTSTRAP ATTRIBUTE FIRED");
        Debug.Log(
            "[FirebaseInit] ProbeState isPlaying=" + Application.isPlaying
        );
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void LogAfterSceneLoad()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[FirebaseInit] AFTER_SCENE_LOAD CHECK FIRED");
#endif
    }
}

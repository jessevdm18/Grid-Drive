using UnityEngine;

/// <summary>
/// Authoritative mobile frame-rate target for Grid Drive v1.
/// Unity's default on mobile is 30 FPS when Application.targetFrameRate is unset.
/// </summary>
public static class FrameRateBootstrap
{
    private const int TargetFps = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyTargetFrameRate()
    {
        // Honor Application.targetFrameRate (vSync would ignore it).
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFps;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[FrameRate] targetFrameRate=" + Application.targetFrameRate +
            " vSyncCount=" + QualitySettings.vSyncCount
        );
#endif
    }
}

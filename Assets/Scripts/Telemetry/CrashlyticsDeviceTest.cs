#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using Firebase.Crashlytics;
using UnityEngine;

/// <summary>
/// Development-only Crashlytics non-fatal test for device builds.
/// Stripped from normal Release player builds via compile define.
/// </summary>
public static class CrashlyticsDeviceTest
{
    public static void SendTestNonFatal()
    {
        Debug.Log("[CrashlyticsTest] Triggered");
        Debug.Log("[CrashlyticsTest] FirebaseReady=" + FirebaseManager.IsReady);

        if (!FirebaseManager.IsReady)
        {
            Debug.LogWarning("[CrashlyticsTest] Firebase not ready");
            return;
        }

        try
        {
            var ex = new Exception("Grid Drive Crashlytics Device Non-Fatal Test");
            Crashlytics.LogException(ex);
            Debug.Log("[CrashlyticsTest] Non-fatal sent.");
        }
        catch (Exception ex)
        {
            Debug.LogError("[CrashlyticsTest] Failed to send non-fatal — " + ex.Message);
        }
    }
}
#endif

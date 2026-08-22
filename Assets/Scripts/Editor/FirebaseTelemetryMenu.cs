#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Development-only Firebase smoke tests + Crashlytics symbol upload notes.
/// Not included in production player UI.
/// </summary>
public static class FirebaseTelemetryMenu
{
    [MenuItem("RushOut/Firebase/Send Test Analytics Event", false, 200)]
    private static void SendTestAnalyticsEvent()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Firebase test",
                "Enter Play Mode to test Firebase runtime initialization.",
                "OK"
            );
            Debug.LogWarning(
                "GameAnalytics: Enter Play Mode to test Firebase runtime initialization."
            );
            return;
        }

        FirebaseManager.EnsureInstance();

        if (!FirebaseManager.IsReady)
        {
            string state =
                "Firebase state = " + FirebaseManager.InitStage +
                (string.IsNullOrEmpty(FirebaseManager.LastDependencyStatus)
                    ? string.Empty
                    : " | DependencyStatus=" + FirebaseManager.LastDependencyStatus) +
                (string.IsNullOrEmpty(FirebaseManager.LastFailureReason)
                    ? string.Empty
                    : " | Reason=" + FirebaseManager.LastFailureReason);

            EditorUtility.DisplayDialog(
                "Firebase not ready",
                state + "\n\nWait for [FirebaseInit] Stage=Ready in the Console, then retry.",
                "OK"
            );
            Debug.LogWarning("GameAnalytics: Firebase not ready — test event skipped. " + state);
            return;
        }

        GameAnalytics.SendTestEvent();
    }

    [MenuItem("RushOut/Firebase/Send Test Non-Fatal", false, 201)]
    private static void SendTestNonFatal()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Firebase test",
                "Enter Play Mode to test Firebase runtime initialization.",
                "OK"
            );
            return;
        }

        FirebaseManager.EnsureInstance();

        if (!FirebaseManager.IsReady)
        {
            string state =
                "Firebase state = " + FirebaseManager.InitStage +
                (string.IsNullOrEmpty(FirebaseManager.LastFailureReason)
                    ? string.Empty
                    : " | Reason=" + FirebaseManager.LastFailureReason);
            EditorUtility.DisplayDialog("Firebase not ready", state, "OK");
            return;
        }

        FirebaseManager.SendTestNonFatal();
    }

    [MenuItem("RushOut/Firebase/Crashlytics Symbol Upload Help", false, 220)]
    private static void SymbolUploadHelp()
    {
        const string message =
            "IL2CPP / Android release symbol upload (no hardcoded paths or App IDs):\n\n" +
            "1. Build Android (IL2CPP, ARM64, Release AAB/APK) with Create symbols.zip enabled " +
            "in Player Settings → Publishing Settings (or copy the symbols zip Unity emits next to the build).\n\n" +
            "2. Install Firebase CLI and log in.\n\n" +
            "3. Upload:\n" +
            "   firebase crashlytics:symbols:upload --app=<FIREBASE_APP_ID> <PATH_TO_SYMBOLS>\n\n" +
            "FIREBASE_APP_ID = mobilesdk_app_id from google-services.json " +
            "(Android client for com.raddergames.griddrive).\n" +
            "PATH_TO_SYMBOLS = path to the symbols.zip Unity produced for that build.\n\n" +
            "Re-upload whenever you ship a new IL2CPP binary so Crashlytics can symbolicate stacks.";

        EditorUtility.DisplayDialog(
            "Crashlytics symbols",
            message,
            "OK"
        );
        Debug.Log("[Firebase] Symbol upload workflow:\n" + message);
    }
}
#endif

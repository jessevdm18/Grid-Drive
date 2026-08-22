#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Development-only privacy helpers. Editor menu only.
/// </summary>
public static class PrivacyConsentDebugMenu
{
    [MenuItem("RushOut/Privacy/Log Consent State", false, 300)]
    private static void LogConsentState()
    {
        PrivacyConsentManager.EnsureInstance();
        Debug.Log(
            "[Privacy][Debug]" +
            " AdsCanRequest=" + PrivacyConsentManager.CanRequestAds +
            " AnalyticsMode=" + PrivacyConsentManager.AnalyticsMode +
            " AnalyticsCollectionEnabled=" +
            PrivacyConsentManager.EvaluateAnalyticsCollectionEnabled() +
            " AnalyticsPreference=" +
            PrivacyConsentManager.GetAnalyticsConsentPreference() +
            " CrashlyticsCollectionEnabled=" +
            FirebaseManager.GetCrashlyticsCollectionEnabledOrUnknown() +
            " IsResolved=" + PrivacyConsentManager.IsResolved +
            " ConsentStatus=" + PrivacyConsentManager.LastConsentStatus
        );
    }

    [MenuItem("RushOut/Privacy/Show Analytics Consent State", false, 301)]
    private static void ShowAnalyticsConsentState()
    {
        Debug.Log(
            "[Privacy][Debug] AnalyticsConsentState" +
            " Choice=" + PrivacyConsentManager.GetAnalyticsConsentPreference() +
            " AnalyticsEnabled=" + PrivacyConsentManager.AnalyticsEnabled +
            " FirebaseReady=" + FirebaseManager.IsReady +
            " AnalyticsCollectionEnabled=" +
            FirebaseManager.GetAnalyticsCollectionEnabledOrUnknown()
        );
    }

    [MenuItem("RushOut/Privacy/Reset Analytics Consent Choice", false, 302)]
    private static void ResetAnalyticsConsentChoice()
    {
        PrivacyConsentManager.ResetAnalyticsConsentChoice();
        Debug.Log(
            "[Privacy][Debug] Analytics consent reset to Unknown. " +
            "Collection disabled. UMP untouched."
        );
    }

    [MenuItem("RushOut/Privacy/Force EEA Geography (next Update)", false, 310)]
    private static void ForceEea()
    {
        PrivacyConsentManager.DebugForceEeaGeography = true;
        Debug.Log(
            "[Privacy][Debug] DebugForceEeaGeography=True (applies on next mobile Update)"
        );
    }

    [MenuItem("RushOut/Privacy/Clear EEA Geography Override", false, 311)]
    private static void ClearEea()
    {
        PrivacyConsentManager.DebugForceEeaGeography = false;
        Debug.Log("[Privacy][Debug] DebugForceEeaGeography=False");
    }

    [MenuItem("RushOut/Privacy/Reset Consent State (device)", false, 320)]
    private static void ResetConsent()
    {
        PrivacyConsentManager.DebugResetConsent();
    }

    [MenuItem("RushOut/Privacy/Show Privacy Options", false, 330)]
    private static void ShowPrivacyOptions()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Privacy][Debug] Enter Play Mode to show privacy options.");
            return;
        }

        PrivacyConsentManager.ShowPrivacyOptions();
    }

    [MenuItem("RushOut/Privacy/Analytics/Set Mode Disabled (v1 default)", false, 340)]
    private static void SetModeDisabled()
    {
        PrivacyConsentManager.AnalyticsMode =
            PrivacyConsentManager.AnalyticsConsentMode.Disabled;
        ApplyAnalyticsNow();
    }

    [MenuItem("RushOut/Privacy/Analytics/Set Mode Enabled", false, 341)]
    private static void SetModeEnabled()
    {
        PrivacyConsentManager.AnalyticsMode =
            PrivacyConsentManager.AnalyticsConsentMode.Enabled;
        ApplyAnalyticsNow();
    }

    [MenuItem("RushOut/Privacy/Analytics/Set Mode ExplicitUserChoice", false, 342)]
    private static void SetModeExplicit()
    {
        PrivacyConsentManager.AnalyticsMode =
            PrivacyConsentManager.AnalyticsConsentMode.ExplicitUserChoice;
        ApplyAnalyticsNow();
    }

    [MenuItem("RushOut/Privacy/Analytics/SetAnalyticsConsent(true)", false, 350)]
    private static void ConsentTrue()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Privacy][Debug] Enter Play Mode first.");
            return;
        }

        PrivacyConsentManager.SetAnalyticsConsent(true);
        ShowAnalyticsConsentState();
    }

    [MenuItem("RushOut/Privacy/Analytics/SetAnalyticsConsent(false)", false, 351)]
    private static void ConsentFalse()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Privacy][Debug] Enter Play Mode first.");
            return;
        }

        PrivacyConsentManager.SetAnalyticsConsent(false);
        ShowAnalyticsConsentState();
    }

    [MenuItem("RushOut/Privacy/Analytics/Show Consent Popup Now", false, 360)]
    private static void ShowPopupNow()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Privacy][Debug] Enter Play Mode first.");
            return;
        }

        AnalyticsConsentPopup.RunGate(() =>
            Debug.Log("[Privacy][Debug] AnalyticsConsentPopup gate completed.")
        );
    }

    private static void ApplyAnalyticsNow()
    {
        bool enabled = PrivacyConsentManager.EvaluateAnalyticsCollectionEnabled();
        FirebaseManager.ApplyAnalyticsConsentPolicy(enabled);
        Debug.Log(
            "[Privacy][Debug] AnalyticsMode=" + PrivacyConsentManager.AnalyticsMode +
            " AppliedEnabled=" + enabled
        );
    }
}
#endif

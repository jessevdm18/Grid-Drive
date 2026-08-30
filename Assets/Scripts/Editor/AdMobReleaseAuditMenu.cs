#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor audit for Grid Drive v1 AdMob App ID + ad-unit environment selection.
/// Does not request ads and does not change runtime consent/ad behavior.
/// </summary>
public static class AdMobReleaseAuditMenu
{
    private const string SettingsAssetPath =
        "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";

    private const string ExpectedAndroidAppId =
        "ca-app-pub-8657245895551337~6979159661";

    private const string ExpectedAndroidRewardedHintProduction =
        "ca-app-pub-8657245895551337/4707151288";

    private const string ExpectedAndroidRewardedFreeCoinsProduction =
        "ca-app-pub-8657245895551337/7369665574";

    private const string ExpectedAndroidInterstitialProduction =
        "ca-app-pub-8657245895551337/6318731373";

    private const string GoogleTestRewardedAndroid =
        "ca-app-pub-3940256099942544/5224354917";

    private const string GoogleTestInterstitialAndroid =
        "ca-app-pub-3940256099942544/1033173712";

    [MenuItem("RushOut/Release/Audit AdMob Configuration", false, 510)]
    private static void AuditAdMobConfiguration()
    {
        string androidAppId = ReadAndroidAppIdFromSettings();
        bool androidAppIdOk = androidAppId == ExpectedAndroidAppId;

        // Menu always runs in Editor → AdsManager selection is TEST.
        string editorRewardedHint =
            AdsManager.GetRewardedAdUnitId(AdsManager.PlacementHint);
        string editorRewardedCoins =
            AdsManager.GetRewardedAdUnitId(AdsManager.PlacementShopFreeCoins);
        string editorInterstitial = AdsManager.GetInterstitialAdUnitId();

        Debug.Log(
            "[AdMobAudit]\n" +
            "BuildEnvironment=Editor\n" +
            "AndroidAppIdStatus=" +
            (androidAppIdOk ? "ProductionOk" : "MismatchOrMissing") + "\n" +
            "AndroidAppId=" + (string.IsNullOrEmpty(androidAppId) ? "(empty)" : androidAppId) + "\n" +
            "RewardedIdMode=TEST (Editor/Dev)\n" +
            "InterstitialIdMode=TEST (Editor/Dev)\n" +
            "EditorRewardedHint=" + editorRewardedHint + "\n" +
            "EditorRewardedFreeCoins=" + editorRewardedCoins + "\n" +
            "EditorInterstitial=" + editorInterstitial + "\n" +
            "AndroidReleaseRewardedHint=" + ExpectedAndroidRewardedHintProduction + "\n" +
            "AndroidReleaseRewardedFreeCoins=" + ExpectedAndroidRewardedFreeCoinsProduction + "\n" +
            "AndroidReleaseInterstitial=" + ExpectedAndroidInterstitialProduction + "\n" +
            "GoogleTestRewarded=" + GoogleTestRewardedAndroid + "\n" +
            "GoogleTestInterstitial=" + GoogleTestInterstitialAndroid + "\n" +
            "BannerPresent=False\n" +
            "UMPConsentGate=True\n" +
            "IosProductionIds=NotConfigured"
        );

        if (!androidAppIdOk)
        {
            Debug.LogWarning(
                "[AdMobAudit] Android App ID is not the Grid Drive production App ID.\n" +
                "Expected=" + ExpectedAndroidAppId + "\n" +
                "Actual=" + androidAppId
            );
        }

        if (editorRewardedHint != GoogleTestRewardedAndroid &&
            editorRewardedHint != "ca-app-pub-3940256099942544/1712485313")
        {
            Debug.LogWarning(
                "[AdMobAudit] Editor hint rewarded ID is unexpectedly not a Google test unit."
            );
        }

        if (editorRewardedCoins != GoogleTestRewardedAndroid &&
            editorRewardedCoins != "ca-app-pub-3940256099942544/1712485313")
        {
            Debug.LogWarning(
                "[AdMobAudit] Editor free-coins rewarded ID is unexpectedly not a Google test unit."
            );
        }

        if (editorInterstitial != GoogleTestInterstitialAndroid &&
            editorInterstitial != "ca-app-pub-3940256099942544/4411468910")
        {
            Debug.LogWarning(
                "[AdMobAudit] Editor interstitial ID is unexpectedly not a Google test unit."
            );
        }
    }

    private static string ReadAndroidAppIdFromSettings()
    {
        Object settings = AssetDatabase.LoadMainAssetAtPath(SettingsAssetPath);
        if (settings == null)
        {
            return string.Empty;
        }

        SerializedObject so = new SerializedObject(settings);
        SerializedProperty prop = so.FindProperty("adMobAndroidAppId");
        if (prop == null)
        {
            return string.Empty;
        }

        return prop.stringValue != null ? prop.stringValue.Trim() : string.Empty;
    }
}
#endif

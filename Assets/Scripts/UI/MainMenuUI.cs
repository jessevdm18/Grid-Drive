using UnityEngine;

/// <summary>
/// Knoppen voor het MainMenu-scherm.
/// Koppel in de Inspector elke knop aan de juiste methode.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private MainMenuSettingsUI settingsUI;
    [SerializeField] private ShopUIController shopUI;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private const int CrashlyticsTestTapCount = 7;
    private const float CrashlyticsTestTapWindowSeconds = 3f;

    private int settingsTapCount;
    private float settingsTapWindowStartUnscaled;
#endif

    private void Start()
    {
        // Safety if Splash was skipped (Editor Play from MainMenu).
        LevelDatabaseContentVersion.ApplyIfNeeded();

        // Authoritative menu music — works whether MusicManager came from this
        // scene or was bootstrapped earlier (e.g. fresh Splash→Gameplay→Menu).
        MusicManager.PlayMenuMusic();
    }

    /// <summary>
    /// Start het spel → Gameplay-scene.
    /// </summary>
    public void OnPlayButton()
    {
        SceneTransition.LoadScene("Gameplay");
    }

    /// <summary>
    /// Open levelkeuze → LevelSelect-scene.
    /// </summary>
    public void OnLevelsButton()
    {
        SceneTransition.LoadScene("LevelSelect");
    }

    /// <summary>
    /// SettingsButton: opent het MainMenu settingspanel.
    /// Development builds: 7 taps within 3s also sends a Crashlytics non-fatal test.
    /// </summary>
    public void OnSettingsButton()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        RegisterSettingsTapForCrashlyticsTest();
#endif

        if (settingsUI == null)
        {
            settingsUI = FindAnyObjectByType<MainMenuSettingsUI>();
        }

        if (settingsUI != null)
        {
            settingsUI.OpenSettings();
        }
        else
        {
            Debug.LogWarning("MainMenuUI: MainMenuSettingsUI is not assigned.");
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void RegisterSettingsTapForCrashlyticsTest()
    {
        float now = Time.unscaledTime;
        if (settingsTapCount == 0 ||
            now - settingsTapWindowStartUnscaled > CrashlyticsTestTapWindowSeconds)
        {
            settingsTapCount = 0;
            settingsTapWindowStartUnscaled = now;
        }

        settingsTapCount++;
        if (settingsTapCount < CrashlyticsTestTapCount)
        {
            return;
        }

        settingsTapCount = 0;
        settingsTapWindowStartUnscaled = 0f;
        CrashlyticsDeviceTest.SendTestNonFatal();
    }
#endif

    /// <summary>
    /// ShopButton: opent het ShopPanel (geen scene load).
    /// </summary>
    public void OnShopButton()
    {
        if (shopUI == null)
        {
            shopUI = FindAnyObjectByType<ShopUIController>();
        }

        if (shopUI != null)
        {
            shopUI.OpenShop();
        }
        else
        {
            Debug.LogWarning("MainMenuUI: ShopUIController is not assigned.");
        }
    }
}

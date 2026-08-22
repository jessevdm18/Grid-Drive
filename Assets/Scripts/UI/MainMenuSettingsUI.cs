using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Klein settingspanel op het MainMenu voor Music/Sound + Analytics toggles.
/// Gebruikt de bestaande AudioManager + PrivacyConsentManager (geen tweede manager).
/// </summary>
public class MainMenuSettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private AudioManager audioManager;

    [Header("Audio Icons")]
    [SerializeField] private Image musicIcon;
    [SerializeField] private Image soundIcon;

    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    [Header("Analytics (optional Inspector wire)")]
    [Tooltip("Label in UI: SHARE ANONYMOUS USAGE DATA")]
    [SerializeField] private Toggle analyticsToggle;

    private void Awake()
    {
        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }

        // SettingsPanel start standaard dicht.
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (analyticsToggle != null)
        {
            analyticsToggle.onValueChanged.AddListener(OnAnalyticsToggleChanged);
        }
    }

    private void OnDestroy()
    {
        if (analyticsToggle != null)
        {
            analyticsToggle.onValueChanged.RemoveListener(OnAnalyticsToggleChanged);
        }
    }

    /// <summary>
    /// SettingsButton OnClick: opent het panel met actuele audio/analytics-state.
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("MainMenuSettingsUI: settingsPanel is not assigned.");
            return;
        }

        if (settingsPanel.activeSelf)
        {
            return;
        }

        settingsPanel.SetActive(true);
        audioManager?.PlayPanelOpen();
        UpdateIcons();
        SyncAnalyticsToggleFromPreference();
    }

    /// <summary>
    /// CloseButton OnClick: sluit het panel.
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("MainMenuSettingsUI: settingsPanel is not assigned.");
            return;
        }

        if (!settingsPanel.activeSelf)
        {
            return;
        }

        settingsPanel.SetActive(false);
        audioManager?.PlayPanelClose();
    }

    /// <summary>
    /// Optional Privacy options button OnClick.
    /// Wire in the Inspector when needed — no scene YAML edits from code.
    /// Safe no-op when UMP does not require privacy options.
    /// </summary>
    public void OnPrivacyOptionsButton()
    {
        PrivacyConsentManager.ShowPrivacyOptions();
    }

    /// <summary>
    /// MusicButton OnClick.
    /// </summary>
    public void ToggleMusic()
    {
        if (audioManager == null)
        {
            Debug.LogWarning("MainMenuSettingsUI: AudioManager is not assigned.");
            return;
        }

        audioManager.ToggleMusic();
        UpdateIcons();
    }

    /// <summary>
    /// SoundButton OnClick.
    /// </summary>
    public void ToggleSound()
    {
        if (audioManager == null)
        {
            Debug.LogWarning("MainMenuSettingsUI: AudioManager is not assigned.");
            return;
        }

        audioManager.ToggleSfx();
        UpdateIcons();
    }

    private void SyncAnalyticsToggleFromPreference()
    {
        if (analyticsToggle == null)
        {
            return;
        }

        analyticsToggle.SetIsOnWithoutNotify(PrivacyConsentManager.AnalyticsEnabled);
    }

    private void OnAnalyticsToggleChanged(bool enabled)
    {
        // Direct preference change — does NOT reopen AnalyticsConsentPopup.
        PrivacyConsentManager.SetAnalyticsEnabled(enabled);
    }

    private void UpdateIcons()
    {
        if (audioManager == null)
        {
            Debug.LogWarning("MainMenuSettingsUI: kan iconen niet updaten — AudioManager ontbreekt.");
            return;
        }

        if (musicIcon != null)
        {
            Sprite musicSprite = audioManager.MusicEnabled ? musicOnSprite : musicOffSprite;
            if (musicSprite != null)
            {
                musicIcon.sprite = musicSprite;
            }
            else
            {
                Debug.LogWarning(
                    "MainMenuSettingsUI: music ON/OFF sprite ontbreekt (" +
                    (audioManager.MusicEnabled ? "musicOnSprite" : "musicOffSprite") + ")."
                );
            }
        }
        else
        {
            Debug.LogWarning("MainMenuSettingsUI: musicIcon ontbreekt.");
        }

        if (soundIcon != null)
        {
            Sprite soundSprite = audioManager.SfxEnabled ? soundOnSprite : soundOffSprite;
            if (soundSprite != null)
            {
                soundIcon.sprite = soundSprite;
            }
            else
            {
                Debug.LogWarning(
                    "MainMenuSettingsUI: sound ON/OFF sprite ontbreekt (" +
                    (audioManager.SfxEnabled ? "soundOnSprite" : "soundOffSprite") + ")."
                );
            }
        }
        else
        {
            Debug.LogWarning("MainMenuSettingsUI: soundIcon ontbreekt.");
        }
    }
}

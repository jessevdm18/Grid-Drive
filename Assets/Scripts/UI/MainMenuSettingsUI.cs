using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainMenu Settings panel: Music/Sound, anonymous usage data, Ad Privacy (UMP),
/// and Privacy Policy URL. Uses AudioManager + PrivacyConsentManager (no second manager).
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

    [Header("Privacy Policy")]
    [Tooltip("Public HTTPS URL for the Grid Drive Privacy Policy. Leave empty until configured.")]
    [SerializeField] private string privacyPolicyUrl = "";

    [Header("Account (optional Phase 4.2A)")]
    [Tooltip("Optional ACCOUNT section under Settings. Linking is never forced at startup.")]
    [SerializeField] private AccountLinkUI accountLinkUI;

    [Header("Daily Notifications (optional Phase 4.4)")]
    [Tooltip("Optional Daily notifications toggle. Never prompts at startup.")]
    [SerializeField] private DailyNotificationSettingsUI dailyNotificationSettingsUI;

    private void Awake()
    {
        BindPersistentAudio();

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
        BindPersistentAudio();
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

        if (accountLinkUI == null)
        {
            accountLinkUI = GetComponentInChildren<AccountLinkUI>(true);
        }

        accountLinkUI?.RefreshFromSettingsOpen();

        if (dailyNotificationSettingsUI == null)
        {
            dailyNotificationSettingsUI =
                GetComponentInChildren<DailyNotificationSettingsUI>(true);
        }

        dailyNotificationSettingsUI?.RefreshFromSettingsOpen();
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
    /// Ad Privacy Settings button OnClick (Google UMP privacy options).
    /// Safe no-op when UMP does not require privacy options.
    /// </summary>
    public void OnPrivacyOptionsButton()
    {
        PrivacyConsentManager.ShowPrivacyOptions();
    }

    /// <summary>
    /// Privacy Policy button OnClick. Always available; opens the policy URL in the browser.
    /// Does not change UMP or analytics consent.
    /// </summary>
    public void OnPrivacyPolicyButton()
    {
        if (string.IsNullOrWhiteSpace(privacyPolicyUrl))
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("[Privacy] Privacy Policy URL is not configured.");
#endif
            return;
        }

        Application.OpenURL(privacyPolicyUrl.Trim());
    }

    /// <summary>
    /// MusicButton OnClick.
    /// </summary>
    public void ToggleMusic()
    {
        BindPersistentAudio();
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
        BindPersistentAudio();
        if (audioManager == null)
        {
            Debug.LogWarning("MainMenuSettingsUI: AudioManager is not assigned.");
            return;
        }

        audioManager.ToggleSfx();
        UpdateIcons();
    }

    /// <summary>
    /// Scene AudioManagers are destroyed when the persistent one already exists.
    /// Rebind so a return to MainMenu does not keep the duplicate reference.
    /// </summary>
    private void BindPersistentAudio()
    {
        // Assigning null drops a destroyed scene AudioManager (Unity fake-null).
        audioManager = AudioManager.Resolve();
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
        BindPersistentAudio();
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

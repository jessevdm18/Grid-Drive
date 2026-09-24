using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Eenvoudig pause-menu voor de Gameplay-scene.
/// Zet dit op een GameObject in Gameplay (bijv. naast UIManager).
/// </summary>
public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject dimOverlay;
    [SerializeField] private LevelManager levelManager;

    [SerializeField] private AudioManager audioManager;

    [Header("Restart (active attempt cost)")]
    [SerializeField] private Button restartButton;
    [Tooltip("Authored 'RESTART' label. Runtime never rewrites this text.")]
    [SerializeField] private TextMeshProUGUI restartButtonLabel;
    [Tooltip("Dynamic cost number only (e.g. 25). Layout/styling is scene-authored.")]
    [SerializeField] private TextMeshProUGUI restartCostLabel;
    [Tooltip("Authored coin icon beside the cost. Runtime does not change its sprite.")]
    [SerializeField] private Image restartCoinIcon;

    [Header("Audio Icons")]
    [SerializeField] private Image musicIcon;
    [SerializeField] private Image soundIcon;

    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    /// <summary>True while the pause panel GameObject is active.</summary>
    public bool IsPauseMenuOpen => pausePanel != null && pausePanel.activeSelf;

    /// <summary>Edit Mode only: pause panel is shown for visual authoring.</summary>
    public bool IsEditorPausePreviewActive
    {
        get
        {
#if UNITY_EDITOR
            return editorPausePreviewActive;
#else
            return false;
#endif
        }
    }

    public GameObject PausePanel => pausePanel;

#if UNITY_EDITOR
    private bool editorPausePreviewActive;
    private bool editorPausePanelWasActive;
    private bool editorDimWasActive;
#endif

    private void Start()
    {
        if (audioManager == null)
        {
            audioManager = AudioManager.Resolve();
        }

        ResolveRestartRefs();
        RefreshRestartPresentation();
        UpdateAudioIcons();
    }

    /// <summary>
    /// Updates only the dynamic restart cost number on <see cref="restartCostLabel"/>.
    /// Never rewrites the authored "RESTART" label text or any layout/styling.
    /// Safe for runtime and Edit Mode preview — no purchase / prefs side effects.
    /// </summary>
    public void RefreshRestartPresentation()
    {
        ResolveRestartRefs();
        if (restartCostLabel == null)
        {
            return;
        }

        restartCostLabel.text = LivesEconomyConfig.RestartCoinCost.ToString();
    }

    /// <summary>
    /// Full pause presentation refresh (restart cost number + audio icons).
    /// Edit Mode preview uses deterministic ON icons without reading/writing prefs.
    /// </summary>
    public void RefreshPausePresentation(bool forEditPreview)
    {
        RefreshRestartPresentation();
        if (forEditPreview)
        {
            ApplyPreviewAudioIcons();
        }
        else
        {
            UpdateAudioIcons();
        }
    }

    /// <summary>
    /// Opent het pause-menu en pauzeert de game.
    /// </summary>
    public void OpenPauseMenu()
    {
        bool opened = false;

        if (pausePanel != null && !pausePanel.activeSelf)
        {
            pausePanel.SetActive(true);
            opened = true;
        }

        if (opened)
        {
            audioManager?.PlayPanelOpen();
        }

        RefreshPausePresentation(forEditPreview: false);
        if (DailyChallengeUiGuard.IsDailyActive && restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }

        Time.timeScale = 0f;
    }

    /// <summary>
    /// Sluit het pause-menu en hervat de game.
    /// Keeps timeScale at 0 if the Gameplay shop overlay is still open.
    /// </summary>
    public void ResumeGame()
    {
        bool closed = false;

        if (pausePanel != null && pausePanel.activeSelf)
        {
            pausePanel.SetActive(false);
            closed = true;
        }

        if (closed)
        {
            audioManager?.PlayPanelClose();
        }

        ShopUIController shop = FindAnyObjectByType<ShopUIController>();
        if (shop != null && shop.IsShopOpen)
        {
            Time.timeScale = 0f;
            return;
        }

        Time.timeScale = 1f;
    }

    /// <summary>
    /// Herstart het huidige level via LevelManager.
    /// Active attempts cost <see cref="LivesEconomyConfig.RestartCoinCost"/> coins.
    /// </summary>
    public void RestartLevel()
    {
        bool wasPaused = pausePanel != null && pausePanel.activeSelf;

        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        RestartPurchaseResult result = RestartPurchaseService.TryRestartWithEconomy(
            levelManager: levelManager,
            source: "pause");

        if (result == RestartPurchaseResult.NotEnoughCoins)
        {
            // Keep the active attempt intact and restore pause for clarity.
            if (wasPaused && pausePanel != null)
            {
                pausePanel.SetActive(true);
                Time.timeScale = 0f;
            }

            return;
        }

        if (result == RestartPurchaseResult.Unavailable && levelManager == null)
        {
            Debug.LogError("PauseManager: geen LevelManager gekoppeld.");
        }
    }

    private void ResolveRestartRefs()
    {
        if (restartButton == null && pausePanel != null)
        {
            Button[] buttons = pausePanel.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && buttons[i].name == "RestartButton")
                {
                    restartButton = buttons[i];
                    break;
                }
            }
        }

        if (restartButton == null)
        {
            return;
        }

        if (restartButtonLabel == null)
        {
            restartButtonLabel = FindNamedTmp(restartButton.transform, "RestartText");
            if (restartButtonLabel == null)
            {
                // Legacy single-label child name.
                restartButtonLabel = FindNamedTmp(restartButton.transform, "Text (TMP)");
            }
        }

        if (restartCostLabel == null)
        {
            restartCostLabel = FindNamedTmp(restartButton.transform, "CostText");
        }

        if (restartCoinIcon == null)
        {
            Transform coin = restartButton.transform.Find("CoinIcon");
            if (coin == null)
            {
                coin = FindDeepChild(restartButton.transform, "CoinIcon");
            }

            if (coin != null)
            {
                restartCoinIcon = coin.GetComponent<Image>();
            }
        }
    }

    private static TextMeshProUGUI FindNamedTmp(Transform root, string name)
    {
        if (root == null || string.IsNullOrEmpty(name))
        {
            return null;
        }

        Transform child = root.Find(name);
        if (child == null)
        {
            child = FindDeepChild(root, name);
        }

        return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
    }

    private static Transform FindDeepChild(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child != null && child.name == name)
            {
                return child;
            }

            Transform nested = FindDeepChild(child, name);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    /// <summary>
    /// Gaat terug naar het hoofdmenu zonder progressie te wijzigen.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        if (DailyChallengeUiGuard.IsDailyActive)
        {
            DailyChallengeManager.EnsureInstance()?.NotifyAbandoned("pause_main_menu");
        }

        SceneTransition.LoadScene("MainMenu");
    }

    /// <summary>
    /// Gaat naar LevelSelect zonder progressie/voltooiing te wijzigen.
    /// </summary>
    public void GoToLevelSelect()
    {
        Time.timeScale = 1f;

        if (DailyChallengeUiGuard.IsDailyActive)
        {
            // Daily should return to MainMenu; LevelSelect is not a Daily destination.
            DailyChallengeManager.EnsureInstance()?.NotifyAbandoned("pause_level_select");
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            if (dimOverlay != null)
            {
                dimOverlay.SetActive(false);
            }

            SceneTransition.LoadScene("MainMenu");
            return;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (dimOverlay != null)
        {
            dimOverlay.SetActive(false);
        }

        SceneTransition.LoadScene("LevelSelect");
    }

    /// <summary>
    /// MusicButton OnClick: toggle muziek + update icoon.
    /// </summary>
    public void ToggleMusic()
    {
        if (audioManager == null)
        {
            Debug.LogWarning("PauseManager: geen AudioManager gekoppeld.");
            return;
        }

        audioManager.ToggleMusic();
        UpdateAudioIcons();
    }

    /// <summary>
    /// SoundButton OnClick: toggle SFX + update icoon.
    /// </summary>
    public void ToggleSound()
    {
        if (audioManager == null)
        {
            Debug.LogWarning("PauseManager: geen AudioManager gekoppeld.");
            return;
        }

        audioManager.ToggleSfx();
        UpdateAudioIcons();
    }

    /// <summary>
    /// Wisselt Music/Sound iconen tussen ON en OFF sprites.
    /// </summary>
    private void UpdateAudioIcons()
    {
        if (audioManager == null)
        {
            Debug.LogWarning("PauseManager: kan audio-iconen niet updaten — AudioManager ontbreekt.");
            return;
        }

        ApplyAudioIconSprites(
            musicEnabled: audioManager.MusicEnabled,
            soundEnabled: audioManager.SfxEnabled);
    }

    /// <summary>
    /// Edit Mode preview: deterministic ON icons — does not read or write prefs.
    /// </summary>
    private void ApplyPreviewAudioIcons()
    {
        ApplyAudioIconSprites(musicEnabled: true, soundEnabled: true);
    }

    private void ApplyAudioIconSprites(bool musicEnabled, bool soundEnabled)
    {
        if (musicIcon != null)
        {
            Sprite musicSprite = musicEnabled ? musicOnSprite : musicOffSprite;
            if (musicSprite != null)
            {
                musicIcon.sprite = musicSprite;
            }
        }

        if (soundIcon != null)
        {
            Sprite soundSprite = soundEnabled ? soundOnSprite : soundOffSprite;
            if (soundSprite != null)
            {
                soundIcon.sprite = soundSprite;
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Edit Mode: show PausePanel with final runtime presentation. No gameplay side effects.
    /// Scene-authored styling (RectTransforms, TMP presentation, sprites, colors) is never
    /// snapshotted — only temporary visibility is tracked for Hide.
    /// </summary>
    public void EditorPreviewPauseMenu()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[PausePreview] Preview Pause Menu is Edit Mode only.");
            return;
        }

        if (!editorPausePreviewActive)
        {
            // Visibility only — never snapshot RectTransforms / TMP styling / sprites.
            editorPausePanelWasActive = pausePanel != null && pausePanel.activeSelf;
            editorDimWasActive = dimOverlay != null && dimOverlay.activeSelf;
            editorPausePreviewActive = true;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (dimOverlay != null)
        {
            dimOverlay.SetActive(true);
        }

        // Dynamic cost number + deterministic audio icon state only.
        RefreshPausePresentation(forEditPreview: true);

        Debug.Log("[PausePreview] Pause menu preview shown");
    }

    /// <summary>
    /// Edit Mode: hide PausePanel. Restores only temporary visibility.
    /// Never reverts authored visual properties changed while preview was visible.
    /// </summary>
    public void EditorHidePauseMenuPreview()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (!editorPausePreviewActive)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            return;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(editorPausePanelWasActive);
        }

        if (dimOverlay != null)
        {
            dimOverlay.SetActive(editorDimWasActive);
        }

        editorPausePreviewActive = false;
        Debug.Log("[PausePreview] Pause menu preview hidden (visibility only)");
    }
#endif

    private void OnDestroy()
    {
        // Voorkom dat de game bevroren blijft na scene-wissel.
        Time.timeScale = 1f;
    }
}

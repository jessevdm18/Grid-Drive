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

    [Header("Audio Icons")]
    [SerializeField] private Image musicIcon;
    [SerializeField] private Image soundIcon;

    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    private void Start()
    {
        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioManager>();
        }

        UpdateAudioIcons();
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

        UpdateAudioIcons();
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Sluit het pause-menu en hervat de game.
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

        Time.timeScale = 1f;
    }

    /// <summary>
    /// Herstart het huidige level via LevelManager.
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (levelManager != null)
        {
            levelManager.RestartLevel();
        }
        else
        {
            Debug.LogError("PauseManager: geen LevelManager gekoppeld.");
        }
    }

    /// <summary>
    /// Gaat terug naar het hoofdmenu zonder progressie te wijzigen.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneTransition.LoadScene("MainMenu");
    }

    /// <summary>
    /// Gaat naar LevelSelect zonder progressie/voltooiing te wijzigen.
    /// </summary>
    public void GoToLevelSelect()
    {
        Time.timeScale = 1f;

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
                    "PauseManager: music ON/OFF sprite ontbreekt (" +
                    (audioManager.MusicEnabled ? "musicOnSprite" : "musicOffSprite") + ")."
                );
            }
        }
        else
        {
            Debug.LogWarning("PauseManager: musicIcon ontbreekt.");
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
                    "PauseManager: sound ON/OFF sprite ontbreekt (" +
                    (audioManager.SfxEnabled ? "soundOnSprite" : "soundOffSprite") + ")."
                );
            }
        }
        else
        {
            Debug.LogWarning("PauseManager: soundIcon ontbreekt.");
        }
    }

    private void OnDestroy()
    {
        // Voorkom dat de game bevroren blijft na scene-wissel.
        Time.timeScale = 1f;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Eenvoudig pause-menu voor de Gameplay-scene.
/// Zet dit op een GameObject in Gameplay (bijv. naast UIManager).
/// </summary>
public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private LevelManager levelManager;

    [SerializeField] private AudioManager audioManager;
    [SerializeField] private TMP_Text musicButtonText;
    [SerializeField] private TMP_Text soundButtonText;

    private void Start()
    {
        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioManager>();
        }

        UpdateAudioButtonTexts();
    }

    /// <summary>
    /// Opent het pause-menu en pauzeert de game.
    /// </summary>
    public void OpenPauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        UpdateAudioButtonTexts();
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Sluit het pause-menu en hervat de game.
    /// </summary>
    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
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
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// MusicButton OnClick: toggle muziek + update label.
    /// </summary>
    public void ToggleMusic()
    {
        if (audioManager == null)
        {
            Debug.LogWarning("PauseManager: geen AudioManager gekoppeld.");
            return;
        }

        audioManager.ToggleMusic();
        UpdateAudioButtonTexts();
    }

    /// <summary>
    /// SoundButton OnClick: toggle SFX + update label.
    /// </summary>
    public void ToggleSound()
    {
        if (audioManager == null)
        {
            Debug.LogWarning("PauseManager: geen AudioManager gekoppeld.");
            return;
        }

        audioManager.ToggleSfx();
        UpdateAudioButtonTexts();
    }

    /// <summary>
    /// Zet MUSIC: ON/OFF en SOUND: ON/OFF op de pause-knoppen.
    /// </summary>
    private void UpdateAudioButtonTexts()
    {
        if (audioManager == null)
        {
            Debug.LogWarning("PauseManager: kan audio-teksten niet updaten — AudioManager ontbreekt.");
            return;
        }

        if (musicButtonText != null)
        {
            musicButtonText.text = audioManager.MusicEnabled ? "MUSIC: ON" : "MUSIC: OFF";
        }
        else
        {
            Debug.LogWarning("PauseManager: musicButtonText ontbreekt.");
        }

        if (soundButtonText != null)
        {
            soundButtonText.text = audioManager.SfxEnabled ? "SOUND: ON" : "SOUND: OFF";
        }
        else
        {
            Debug.LogWarning("PauseManager: soundButtonText ontbreekt.");
        }
    }

    private void OnDestroy()
    {
        // Voorkom dat de game bevroren blijft na scene-wissel.
        Time.timeScale = 1f;
    }
}

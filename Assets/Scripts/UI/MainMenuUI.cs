using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Knoppen voor het MainMenu-scherm.
/// Koppel in de Inspector elke knop aan de juiste methode.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private MainMenuSettingsUI settingsUI;

    /// <summary>
    /// Start het spel → Gameplay-scene.
    /// </summary>
    public void OnPlayButton()
    {
        SceneManager.LoadScene("Gameplay");
    }

    /// <summary>
    /// Open levelkeuze → LevelSelect-scene.
    /// </summary>
    public void OnLevelsButton()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    /// <summary>
    /// SettingsButton: opent het MainMenu settingspanel.
    /// </summary>
    public void OnSettingsButton()
    {
        if (settingsUI == null)
        {
            settingsUI = FindFirstObjectByType<MainMenuSettingsUI>();
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
}

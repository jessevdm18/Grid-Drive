using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Knoppen voor het MainMenu-scherm.
/// Koppel in de Inspector elke knop aan de juiste methode.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
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
    /// Settings — later uitbreiden.
    /// </summary>
    public void OnSettingsButton()
    {
        Debug.Log("Settings clicked");
    }
}

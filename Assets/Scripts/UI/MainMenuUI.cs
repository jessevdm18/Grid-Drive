using UnityEngine;

/// <summary>
/// Knoppen voor het MainMenu-scherm.
/// Koppel in de Inspector elke knop aan de juiste methode.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private MainMenuSettingsUI settingsUI;
    [SerializeField] private ShopUIController shopUI;

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
    /// </summary>
    public void OnSettingsButton()
    {
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

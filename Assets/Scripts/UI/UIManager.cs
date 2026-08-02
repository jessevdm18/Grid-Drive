using UnityEngine;

/// <summary>
/// Bestuurt het win-scherm van Rush Out.
/// Koppel knoppen in de Inspector aan OnNextLevelButton / OnRestartButton.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Het panel dat verschijnt wanneer het level is gehaald.")]
    [SerializeField] private GameObject winPanel;

    [Header("Referenties")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AdsManager adsManager;

    // Voorkomt dubbele Next Level-acties (dubbele klik / dubbele ad-callback).
    private bool isHandlingNextLevel;

    private void Start()
    {
        // Win-scherm start verborgen.
        HideWinPanel();
    }

    /// <summary>
    /// Toont het win-scherm. Roep dit aan vanuit GameManager bij een win.
    /// </summary>
    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Verbergt het win-scherm.
    /// </summary>
    public void HideWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Wordt aangeroepen door de "Next Level"-knop.
    /// Toont eventueel een interstitial vóór het volgende level.
    /// </summary>
    public void OnNextLevelButton()
    {
        if (isHandlingNextLevel)
        {
            return;
        }

        HideWinPanel();

        // Interstitial tonen als de teller ≥ 3 én de ad klaar is.
        bool wantsInterstitial =
            gameManager != null &&
            gameManager.ShouldShowInterstitial();

        bool adReady =
            adsManager != null &&
            adsManager.IsInterstitialReady();

        if (wantsInterstitial && adReady)
        {
            isHandlingNextLevel = true;
            gameManager.ResetInterstitialCounter();

            adsManager.ShowInterstitialAd(() =>
            {
                LoadNextLevelOnce();
            });

            return;
        }

        // Geen ad nodig, of ad niet klaar: teller niet resetten, direct door.
        LoadNextLevelOnce();
    }

    /// <summary>
    /// Laadt het volgende level precies één keer.
    /// </summary>
    private void LoadNextLevelOnce()
    {
        if (levelManager != null)
        {
            levelManager.LoadNextLevel();
        }

        isHandlingNextLevel = false;
    }

    /// <summary>
    /// Wordt aangeroepen door de "Restart"-knop.
    /// </summary>
    public void OnRestartButton()
    {
        HideWinPanel();

        if (levelManager != null)
        {
            levelManager.RestartLevel();
        }
    }
}

using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private AdsManager adsManager;

    // Voorkomt dat CompleteLevel meerdere keren voor hetzelfde level draait.
    private bool levelCompleted;

    // Telt voltooide levels sinds de laatste interstitial.
    private int completedLevelsSinceAd = 0;

    /// <summary>
    /// Wordt aangeroepen wanneer de doelauto succesvol via de exit ontsnapt.
    /// </summary>
    public void CompleteLevel()
    {
        if (levelCompleted)
        {
            return;
        }

        levelCompleted = true;

        Debug.Log("LEVEL COMPLETED!");

        // Beloning: één keer per level (beschermd door levelCompleted).
        if (coinManager != null)
        {
            coinManager.AddCoins(50);
        }

        // Interstitial-teller: één keer per level (beschermd door levelCompleted).
        completedLevelsSinceAd++;
        Debug.Log("Completed levels since ad: " + completedLevelsSinceAd);

        if (uiManager != null)
        {
            uiManager.ShowWinPanel();
        }
    }

    /// <summary>
    /// True als er al 3 levels voltooid zijn sinds de laatste interstitial.
    /// </summary>
    public bool ShouldShowInterstitial()
    {
        return completedLevelsSinceAd >= 3;
    }

    /// <summary>
    /// Zet de interstitial-teller terug op 0 (na een getoonde advertentie).
    /// </summary>
    public void ResetInterstitialCounter()
    {
        completedLevelsSinceAd = 0;
    }

    /// <summary>
    /// Reset de win-vlag zodat een nieuw/herstart level opnieuw gewonnen kan worden.
    /// Roep dit aan vanuit LevelManager als je levels herlaadt (optioneel).
    /// </summary>
    public void ResetLevelCompleted()
    {
        levelCompleted = false;
    }
}

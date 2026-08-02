using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CoinManager coinManager;

    // Voorkomt dat CompleteLevel meerdere keren voor hetzelfde level draait.
    private bool levelCompleted;

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

        if (uiManager != null)
        {
            uiManager.ShowWinPanel();
        }
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

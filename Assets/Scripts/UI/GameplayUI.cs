using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Toont coins en levelnummer tijdens Gameplay.
/// </summary>
public class GameplayUI : MonoBehaviour
{
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private TextMeshProUGUI coinText;

    [SerializeField] private LevelManager levelManager;
    [SerializeField] private TextMeshProUGUI levelText;

    [SerializeField] private TextMeshProUGUI movesText;

    private void Start()
    {
        UpdateCoinText(coinManager != null ? coinManager.GetCoins() : 0);
        UpdateLevelText();
        UpdateMovesText(0);
    }

    private void OnEnable()
    {
        if (coinManager != null)
        {
            coinManager.OnCoinsChanged += UpdateCoinText;
        }
    }

    private void OnDisable()
    {
        if (coinManager != null)
        {
            coinManager.OnCoinsChanged -= UpdateCoinText;
        }
    }

    /// <summary>
    /// Gaat terug naar het hoofdmenu zonder progressie te wijzigen.
    /// </summary>
    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Zet de coin-tekst, bijv. "200 Coins".
    /// </summary>
    private void UpdateCoinText(int amount)
    {
        if (coinText != null)
        {
            coinText.text = amount + " Coins";
        }
    }

    /// <summary>
    /// Zet de leveltekst, bijv. "LEVEL 1" (index 0 → 1).
    /// </summary>
    public void UpdateLevelText()
    {
        if (levelText == null || levelManager == null)
        {
            return;
        }

        int displayNumber = levelManager.CurrentLevelIndex + 1;
        levelText.text = "LEVEL " + displayNumber;
    }

    /// <summary>
    /// Zet de move-teller tekst, bijv. "Moves: 3".
    /// </summary>
    public void UpdateMovesText(int moves)
    {
        if (movesText == null)
        {
            return;
        }

        movesText.text = "Moves: " + moves;
    }
}

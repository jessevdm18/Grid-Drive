using UnityEngine;
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

    private void Start()
    {
        UpdateCoinText(coinManager != null ? coinManager.GetCoins() : 0);
        UpdateLevelText();
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
}

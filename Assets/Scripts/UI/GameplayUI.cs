using UnityEngine;
using TMPro;

/// <summary>
/// Toont coins tijdens Gameplay.
/// Luistert naar CoinManager.OnCoinsChanged voor live updates.
/// </summary>
public class GameplayUI : MonoBehaviour
{
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private TextMeshProUGUI coinText;

    private void Start()
    {
        UpdateCoinText(coinManager != null ? coinManager.GetCoins() : 0);
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
}

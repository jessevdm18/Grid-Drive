using System;
using UnityEngine;

/// <summary>
/// Beheert coins en bewaart ze met PlayerPrefs.
/// Zet dit script op een GameObject in je scene (bijv. "CoinManager").
/// </summary>
public class CoinManager : MonoBehaviour
{
    private const string CoinsKey = "RushOut_Coins";
    private const int StartingCoins = 200;

    // Wordt aangeroepen wanneer het aantal coins verandert.
    // UI kan zich hierop abonneren: coinManager.OnCoinsChanged += UpdateText;
    public event Action<int> OnCoinsChanged;

    private int coins;

    /// <summary>
    /// Huidige aantal coins.
    /// </summary>
    public int Coins => coins;

    private void Awake()
    {
        // Laad opgeslagen coins. Nieuwe spelers starten met 200.
        coins = PlayerPrefs.GetInt(CoinsKey, StartingCoins);
    }

    /// <summary>
    /// Geeft het huidige aantal coins terug.
    /// </summary>
    public int GetCoins()
    {
        return coins;
    }

    /// <summary>
    /// Voegt coins toe en slaat op.
    /// </summary>
    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        coins += amount;
        SaveCoins();
        NotifyCoinsChanged();
    }

    /// <summary>
    /// Probeert coins uit te geven.
    /// True = gelukt, false = te weinig coins.
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (coins < amount)
        {
            return false;
        }

        coins -= amount;
        SaveCoins();
        NotifyCoinsChanged();
        return true;
    }

    /// <summary>
    /// Zet coins terug naar de startwaarde (200).
    /// </summary>
    public void ResetCoins()
    {
        coins = StartingCoins;
        SaveCoins();
        NotifyCoinsChanged();
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(CoinsKey, coins);
        PlayerPrefs.Save();
    }

    private void NotifyCoinsChanged()
    {
        OnCoinsChanged?.Invoke(coins);
    }
}

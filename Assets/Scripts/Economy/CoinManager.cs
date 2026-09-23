using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Beheert coins en bewaart ze met PlayerPrefs.
/// Zet dit script op een GameObject in je scene (bijv. "CoinManager").
/// </summary>
public class CoinManager : MonoBehaviour
{
    private const string CoinsKey = "RushOut_Coins";

    /// <summary>
    /// Persisted Google Play purchase tokens that already received their coin grant.
    /// Stored with the coin balance so grant + dedupe survive the same PlayerPrefs.Save().
    /// </summary>
    private const string IapFulfilledTxKey = "RushOut_IAP_FulfilledTx";

    private const char IapTxSeparator = '\n';

    /// <summary>Fresh-install default (also PlayerPrefs miss default).</summary>
    public const int StartingCoins = 200;

    // Wordt aangeroepen wanneer het aantal coins verandert.
    // UI kan zich hierop abonneren: coinManager.OnCoinsChanged += UpdateText;
    public event Action<int> OnCoinsChanged;

    private int coins;
    private readonly HashSet<string> fulfilledIapTransactions = new HashSet<string>();
    private readonly List<string> fulfilledIapOrder = new List<string>();

    /// <summary>
    /// Huidige aantal coins.
    /// </summary>
    public int Coins => coins;

    private void Awake()
    {
        // Laad opgeslagen coins. Nieuwe spelers starten met 200.
        coins = PlayerPrefs.GetInt(CoinsKey, StartingCoins);
        LoadFulfilledIapTransactions();
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
    /// True if this IAP transaction id already received its durable coin grant.
    /// </summary>
    public bool HasFulfilledIapTransaction(string transactionId)
    {
        if (string.IsNullOrEmpty(transactionId))
        {
            return false;
        }

        return fulfilledIapTransactions.Contains(transactionId);
    }

    /// <summary>
    /// Crash-safe IAP grant: coins and fulfilled-transaction id are written in one
    /// PlayerPrefs.Save(). Returns false if this transaction was already fulfilled
    /// (no second grant). Required so a crash cannot mark a purchase "done" without
    /// granting, or grant twice after redelivery.
    /// </summary>
    public bool TryAddCoinsForIapTransaction(int amount, string transactionId)
    {
        if (amount <= 0 || string.IsNullOrEmpty(transactionId))
        {
            return false;
        }

        if (fulfilledIapTransactions.Contains(transactionId))
        {
            return false;
        }

        coins += amount;
        RememberFulfilledIapTransaction(transactionId);
        PlayerPrefs.SetInt(CoinsKey, coins);
        PlayerPrefs.SetString(IapFulfilledTxKey, SerializeFulfilledIapTransactions());
        PlayerPrefs.Save();
        NotifyCoinsChanged();
        return true;
    }

    /// <summary>
    /// True als er genoeg coins zijn voor amount (schrijft niets af).
    /// </summary>
    public bool CanAfford(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        return coins >= amount;
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// Editor/DEV: set exact coin balance without touching other PlayerPrefs.
    /// </summary>
    public void EditorSetCoinsForTesting(int amount)
    {
        coins = Mathf.Max(0, amount);
        SaveCoins();
        NotifyCoinsChanged();
    }
#endif

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(CoinsKey, coins);
        PlayerPrefs.Save();
    }

    private void NotifyCoinsChanged()
    {
        OnCoinsChanged?.Invoke(coins);
    }

    private void LoadFulfilledIapTransactions()
    {
        fulfilledIapTransactions.Clear();
        fulfilledIapOrder.Clear();

        string raw = PlayerPrefs.GetString(IapFulfilledTxKey, string.Empty);
        if (string.IsNullOrEmpty(raw))
        {
            return;
        }

        string[] parts = raw.Split(IapTxSeparator);
        for (int i = 0; i < parts.Length; i++)
        {
            string id = parts[i];
            if (string.IsNullOrEmpty(id) || !fulfilledIapTransactions.Add(id))
            {
                continue;
            }

            fulfilledIapOrder.Add(id);
        }
    }

    private void RememberFulfilledIapTransaction(string transactionId)
    {
        if (!fulfilledIapTransactions.Add(transactionId))
        {
            return;
        }

        fulfilledIapOrder.Add(transactionId);
    }

    private string SerializeFulfilledIapTransactions()
    {
        if (fulfilledIapOrder.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(IapTxSeparator.ToString(), fulfilledIapOrder);
    }
}

using TMPro;
using UnityEngine;

/// <summary>
/// Shows CoinManager's balance on the Gameplay coin card text.
/// Same formatting as GameplayUI: digits only, no thousands separator.
/// </summary>
public class CoinBalanceView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private CoinManager coinManager;

    private void Awake()
    {
        if (balanceText == null)
        {
            Transform value = transform.Find("ValueText");
            if (value != null)
            {
                balanceText = value.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private void OnEnable()
    {
        if (coinManager == null)
        {
            coinManager = CoinManager.EnsureInstance();
        }

        if (coinManager == null)
        {
            return;
        }

        coinManager.OnCoinsChanged -= Refresh;
        coinManager.OnCoinsChanged += Refresh;
        Refresh(coinManager.GetCoins());
    }

    private void OnDisable()
    {
        if (coinManager != null)
        {
            coinManager.OnCoinsChanged -= Refresh;
        }
    }

    private void Refresh(int amount)
    {
        if (balanceText != null)
        {
            balanceText.text = amount.ToString();
        }
    }
}

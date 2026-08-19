using UnityEngine;
using TMPro;

/// <summary>
/// Toont coins, levelnummer en moves tijdens Gameplay.
/// Level/Moves: alleen ValueText (cijfers); LabelText blijft "LEVEL" / "MOVES".
/// </summary>
public class GameplayUI : MonoBehaviour
{
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private TextMeshProUGUI coinText;

    /// <summary>Bestaande coin counter (spotlight target voor Coins feature tutorial).</summary>
    public RectTransform CoinCounterRect =>
        coinText != null ? coinText.rectTransform : null;

    [SerializeField] private LevelManager levelManager;

    [Tooltip("LevelCard/ValueText — alleen het levelnummer, bijv. \"8\".")]
    [SerializeField] private TextMeshProUGUI levelValueText;

    [Tooltip("MovesCard/ValueText — alleen het aantal moves, bijv. \"0\".")]
    [SerializeField] private TextMeshProUGUI movesValueText;

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
        SceneTransition.LoadScene("MainMenu");
    }

    /// <summary>
    /// Zet CoinText op alleen het aantal coins (icoon staat apart in de UI).
    /// </summary>
    private void UpdateCoinText(int amount)
    {
        if (coinText != null)
        {
            coinText.text = amount.ToString();
        }
    }

    /// <summary>
    /// Zet alleen LevelCard/ValueText op het levelnummer (index 0 → "1").
    /// Raakt LabelText ("LEVEL") niet aan.
    /// </summary>
    public void UpdateLevelText()
    {
        if (levelValueText == null || levelManager == null)
        {
            return;
        }

        int displayNumber = levelManager.GetDisplayLevelNumber();
        levelValueText.text = displayNumber.ToString();
    }

    /// <summary>
    /// Zet alleen MovesCard/ValueText op het aantal moves.
    /// Raakt LabelText ("MOVES") niet aan.
    /// </summary>
    public void UpdateMovesText(int moves)
    {
        if (movesValueText == null)
        {
            return;
        }

        movesValueText.text = moves.ToString();
    }
}

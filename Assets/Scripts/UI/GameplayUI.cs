using UnityEngine;
using TMPro;

/// <summary>
/// Toont coins, levelnummer, difficulty en moves tijdens Gameplay.
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

    [Header("Difficulty Label")]
    [Tooltip("Optional. Shows EASY / MEDIUM / HARD from LevelData.difficulty.")]
    [SerializeField] private TextMeshProUGUI difficultyText;
    [Tooltip("Optional root to show/hide the difficulty label.")]
    [SerializeField] private GameObject difficultyLabelRoot;

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
    /// Zet LevelCard/ValueText + difficulty label (LevelData.difficulty).
    /// Raakt LabelText ("LEVEL") niet aan.
    /// </summary>
    public void UpdateLevelText()
    {
        if (levelValueText != null && levelManager != null)
        {
            int displayNumber = levelManager.GetDisplayLevelNumber();
            levelValueText.text = displayNumber.ToString();
        }

        UpdateDifficultyLabel();
    }

    /// <summary>
    /// HUD difficulty from authoritative LevelData.difficulty (incl. V1 playtest).
    /// </summary>
    public void UpdateDifficultyLabel()
    {
        if (difficultyText == null && difficultyLabelRoot == null)
        {
            return;
        }

        if (levelManager == null)
        {
            SetDifficultyLabelVisible(false);
            return;
        }

        LevelData data = levelManager.CurrentLevelData;
        if (data == null)
        {
            SetDifficultyLabelVisible(false);
            return;
        }

        if (difficultyText != null)
        {
            difficultyText.text = GetDifficultyLabelCopy(data.difficulty);
        }

        SetDifficultyLabelVisible(true);
    }

    private static string GetDifficultyLabelCopy(LevelDifficulty difficulty)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Medium:
                return "MEDIUM";
            case LevelDifficulty.Hard:
                return "HARD";
            case LevelDifficulty.Easy:
            default:
                return "EASY";
        }
    }

    private void SetDifficultyLabelVisible(bool visible)
    {
        if (difficultyLabelRoot != null)
        {
            difficultyLabelRoot.SetActive(visible);
        }
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

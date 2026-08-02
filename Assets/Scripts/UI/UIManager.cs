using UnityEngine;

/// <summary>
/// Bestuurt het win-scherm van Rush Out.
/// Koppel knoppen in de Inspector aan OnNextLevelButton / OnRestartButton.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Het panel dat verschijnt wanneer het level is gehaald.")]
    [SerializeField] private GameObject winPanel;

    [Header("Referenties")]
    [SerializeField] private LevelManager levelManager;

    private void Start()
    {
        // Win-scherm start verborgen.
        HideWinPanel();
    }

    /// <summary>
    /// Toont het win-scherm. Roep dit aan vanuit GameManager bij een win.
    /// </summary>
    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Verbergt het win-scherm.
    /// </summary>
    public void HideWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Wordt aangeroepen door de "Next Level"-knop.
    /// </summary>
    public void OnNextLevelButton()
    {
        HideWinPanel();

        if (levelManager != null)
        {
            levelManager.LoadNextLevel();
        }
    }

    /// <summary>
    /// Wordt aangeroepen door de "Restart"-knop.
    /// </summary>
    public void OnRestartButton()
    {
        HideWinPanel();

        if (levelManager != null)
        {
            levelManager.RestartLevel();
        }
    }
}

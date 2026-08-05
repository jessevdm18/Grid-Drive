using UnityEngine;
using TMPro;

/// <summary>
/// Optionele UI-helper op de LevelButton-prefab (alleen levelnummer).
/// Sterren worden via Images gezet door LevelSelectUI.
/// </summary>
public class LevelButtonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text levelNumberText;

    /// <summary>
    /// Zet het zichtbare levelnummer (1-based).
    /// </summary>
    public void SetLevelNumber(int displayNumber)
    {
        if (levelNumberText != null)
        {
            levelNumberText.text = displayNumber.ToString();
        }
    }
}

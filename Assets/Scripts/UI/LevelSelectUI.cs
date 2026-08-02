using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Bouwt automatisch levelknoppen in het LevelSelect-scherm.
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Prefab van één levelknop (met Button + TextMeshProUGUI).")]
    [SerializeField] private GameObject levelButtonPrefab;

    [Tooltip("Parent waar de knoppen onder komen (bijv. een Grid Layout Group).")]
    [SerializeField] private Transform levelGrid;

    [Header("Referenties")]
    [SerializeField] private SaveManager saveManager;

    [Tooltip("Centrale database met alle levels.")]
    [SerializeField] private LevelDatabase levelDatabase;

    private void Start()
    {
        Debug.Log("LevelSelectUI: Start() uitgevoerd.");
        Debug.Log("LevelSelectUI: levelDatabase is null = " + (levelDatabase == null));
        Debug.Log("LevelSelectUI: LevelCount = " + (levelDatabase != null ? levelDatabase.LevelCount : 0));
        Debug.Log("LevelSelectUI: levelButtonPrefab is null = " + (levelButtonPrefab == null));
        Debug.Log("LevelSelectUI: levelGrid is null = " + (levelGrid == null));
        Debug.Log("LevelSelectUI: saveManager is null = " + (saveManager == null));

        // Welk level is al unlocked? (index, dus 0 = level 1)
        int unlockedLevel = 0;

        if (saveManager != null)
        {
            unlockedLevel = saveManager.GetUnlockedLevel();
        }

        Debug.Log("LevelSelectUI: unlockedLevel = " + unlockedLevel);

        CreateLevelButtons(unlockedLevel);
    }

    /// <summary>
    /// Maakt één knop per level onder levelGrid.
    /// </summary>
    private void CreateLevelButtons(int unlockedLevel)
    {
        if (levelButtonPrefab == null || levelGrid == null)
        {
            Debug.LogError("LevelSelectUI: levelButtonPrefab of levelGrid ontbreekt.");
            return;
        }

        if (levelDatabase == null || levelDatabase.LevelCount == 0)
        {
            Debug.LogError("LevelSelectUI: levelDatabase ontbreekt of is leeg.");
            return;
        }

        for (int levelIndex = 0; levelIndex < levelDatabase.LevelCount; levelIndex++)
        {
            // Lokale kopie voor de knop-callback (voorkomt closure-bugs in de loop).
            int index = levelIndex;

            GameObject buttonObject = Instantiate(levelButtonPrefab, levelGrid);
            Debug.Log("LevelSelectUI: levelbutton geïnstantieerd voor levelIndex = " + index);

            // Tekst: levelnummer vanaf 1 (index 0 → "1").
            TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
            {
                label.text = (index + 1).ToString();
            }

            Button button = buttonObject.GetComponent<Button>();

            if (button == null)
            {
                Debug.LogError("LevelSelectUI: prefab heeft geen Button-component.");
                continue;
            }

            // Alleen unlocked levels zijn klikbaar.
            bool isUnlocked = index <= unlockedLevel;
            button.interactable = isUnlocked;

            if (isUnlocked)
            {
                button.onClick.AddListener(() => OnLevelButtonClicked(index));
            }
        }
    }

    /// <summary>
    /// Slaat het gekozen level op en start Gameplay.
    /// </summary>
    private void OnLevelButtonClicked(int levelIndex)
    {
        if (saveManager != null)
        {
            saveManager.SaveCurrentLevel(levelIndex);
        }

        SceneManager.LoadScene("Gameplay");
    }
}

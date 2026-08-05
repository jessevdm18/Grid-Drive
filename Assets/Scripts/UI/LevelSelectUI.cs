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
    [Tooltip("Prefab van één levelknop (Button + LevelNumberText + Stars/Star1-3).")]
    [SerializeField] private GameObject levelButtonPrefab;

    [Tooltip("Parent waar de knoppen onder komen (bijv. een Grid Layout Group).")]
    [SerializeField] private Transform levelGrid;

    [Header("Star Sprites")]
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private Sprite emptyStarSprite;

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
            // Herstel unlocked progress t.o.v. bestaande sterren (geen data wissen).
            if (levelDatabase != null)
            {
                saveManager.RepairUnlockedProgress(levelDatabase.LevelCount);
            }

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

            Button button = buttonObject.GetComponent<Button>();

            if (button == null)
            {
                Debug.LogError("LevelSelectUI: prefab heeft geen Button-component.");
                continue;
            }

            // Alleen unlocked levels zijn klikbaar.
            bool isUnlocked = index <= unlockedLevel;
            button.interactable = isUnlocked;

            // Levelnummer (TMP) blijven zetten.
            SetLevelNumberText(buttonObject.transform, index + 1);

            // Sterren via Image-sprites onder Stars/Star1-3.
            int stars = 0;
            if (saveManager != null)
            {
                stars = saveManager.GetStarsForLevel(index);
            }

            ApplyStarImages(buttonObject.transform, stars);

            if (isUnlocked)
            {
                button.onClick.AddListener(() => OnLevelButtonClicked(index));
            }
        }
    }

    /// <summary>
    /// Zet LevelNumberText op het zichtbare levelnummer (1-based).
    /// </summary>
    private static void SetLevelNumberText(Transform buttonRoot, int displayNumber)
    {
        Transform numberTransform = buttonRoot.Find("LevelNumberText");
        if (numberTransform == null)
        {
            // Fallback: LevelButtonUI of eerste TMP.
            LevelButtonUI buttonUI = buttonRoot.GetComponent<LevelButtonUI>();
            if (buttonUI != null)
            {
                buttonUI.SetLevelNumber(displayNumber);
                return;
            }

            TextMeshProUGUI label = buttonRoot.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = displayNumber.ToString();
            }

            return;
        }

        TMP_Text numberText = numberTransform.GetComponent<TMP_Text>();
        if (numberText != null)
        {
            numberText.text = displayNumber.ToString();
        }
    }

    /// <summary>
    /// Zet Star1/Star2/Star3 Images op filled/empty sprites.
    /// </summary>
    private void ApplyStarImages(Transform buttonRoot, int stars)
    {
        stars = Mathf.Clamp(stars, 0, 3);

        Transform starsRoot = buttonRoot.Find("Stars");
        if (starsRoot == null)
        {
            Debug.LogWarning("LevelSelectUI: Stars-child ontbreekt op levelbutton.");
            return;
        }

        SetStarImage(starsRoot.Find("Star1"), stars >= 1);
        SetStarImage(starsRoot.Find("Star2"), stars >= 2);
        SetStarImage(starsRoot.Find("Star3"), stars >= 3);
    }

    private void SetStarImage(Transform starTransform, bool filled)
    {
        if (starTransform == null)
        {
            return;
        }

        Image image = starTransform.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        image.sprite = filled ? filledStarSprite : emptyStarSprite;
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

    public void OnBackButton()
{
    SceneManager.LoadScene("MainMenu");
}
}

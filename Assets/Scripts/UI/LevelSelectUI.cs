using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bouwt automatisch levelknoppen in het LevelSelect-scherm.
/// Unlock/progress blijft via SaveManager; visuals via LevelButtonUI.
/// LevelGrid zit in een verticale ScrollRect en groeit mee met het aantal levels.
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    private const int GridColumns = 3;

    [Header("UI")]
    [Tooltip("Prefab van één levelknop (LevelButtonUI + hierarchy).")]
    [SerializeField] private GameObject levelButtonPrefab;

    [Tooltip("Parent waar de knoppen onder komen (GridLayoutGroup / ScrollRect content).")]
    [SerializeField] private Transform levelGrid;

    [Tooltip("Verticale ScrollRect voor de levelgrid.")]
    [SerializeField] private ScrollRect scrollRect;

    [Header("Star Sprites")]
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private Sprite emptyStarSprite;

    [Header("Referenties")]
    [SerializeField] private SaveManager saveManager;

    [Tooltip("Centrale database met alle levels.")]
    [SerializeField] private LevelDatabase levelDatabase;

    private void Start()
    {
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

        ClearExistingButtons();

        int levelCount = levelDatabase.LevelCount;
        for (int levelIndex = 0; levelIndex < levelCount; levelIndex++)
        {
            // Lokale kopie voor de knop-callback (voorkomt closure-bugs in de loop).
            int index = levelIndex;

            GameObject buttonObject = Instantiate(levelButtonPrefab, levelGrid);
            LevelButtonUI buttonUI = buttonObject.GetComponent<LevelButtonUI>();

            if (buttonUI == null)
            {
                Debug.LogError("LevelSelectUI: prefab mist LevelButtonUI.");
                continue;
            }

            // Bestaande progress/unlock-bepaling (niet wijzigen).
            bool isUnlocked = index <= unlockedLevel;

            int stars = 0;
            if (saveManager != null)
            {
                stars = saveManager.GetStarsForLevel(index);
            }

            buttonUI.Setup(
                index + 1,
                isUnlocked,
                stars,
                filledStarSprite,
                emptyStarSprite
            );

            Button button = buttonUI.Button != null
                ? buttonUI.Button
                : buttonObject.GetComponent<Button>();

            if (button == null)
            {
                Debug.LogError("LevelSelectUI: prefab heeft geen Button-component.");
                continue;
            }

            if (isUnlocked)
            {
                button.onClick.AddListener(() => OnLevelButtonClicked(index));
            }
        }

        Canvas.ForceUpdateCanvases();
        UpdateScrollContentHeight(levelCount);

        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void ClearExistingButtons()
    {
        for (int i = levelGrid.childCount - 1; i >= 0; i--)
        {
            Destroy(levelGrid.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Zet LevelGrid-hoogte zodat alle rijen (3 kolommen) erin passen.
    /// </summary>
    private void UpdateScrollContentHeight(int levelCount)
    {
        RectTransform gridRect = levelGrid as RectTransform;
        if (gridRect == null)
        {
            return;
        }

        GridLayoutGroup grid = levelGrid.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            Debug.LogWarning("LevelSelectUI: LevelGrid mist GridLayoutGroup.");
            return;
        }

        int rows = Mathf.CeilToInt(levelCount / (float)GridColumns);
        float height =
            grid.padding.top +
            grid.padding.bottom +
            rows * grid.cellSize.y +
            Mathf.Max(0, rows - 1) * grid.spacing.y;

        // Stretch X (anchors 0-1), groei in hoogte vanaf de top.
        gridRect.anchorMin = new Vector2(0f, 1f);
        gridRect.anchorMax = new Vector2(1f, 1f);
        gridRect.pivot = new Vector2(0.5f, 1f);
        gridRect.anchoredPosition = new Vector2(0f, 0f);
        gridRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        if (scrollRect != null && scrollRect.content != gridRect)
        {
            scrollRect.content = gridRect;
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

        SceneTransition.LoadScene("Gameplay");
    }

    public void OnBackButton()
    {
        SceneTransition.LoadScene("MainMenu");
    }
}

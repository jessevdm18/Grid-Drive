using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LevelSelect met Easy / Medium / Hard tabs.
/// Buttons runtime genereren; save identity = database-index.
/// Display numbering is lokaal per difficulty (1..N).
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    private const int FallbackGridColumns = 3;

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

    [Header("Difficulty Tabs")]
    [SerializeField] private Button easyTabButton;
    [SerializeField] private Button mediumTabButton;
    [SerializeField] private Button hardTabButton;

    [Tooltip("Optioneel lock-icoon op Medium-tab (zichtbaar zolang Medium locked).")]
    [SerializeField] private GameObject mediumLockIcon;

    [Tooltip("Optioneel lock-icoon op Hard-tab.")]
    [SerializeField] private GameObject hardLockIcon;

    [Tooltip("Optioneel. Bijv. \"7 / 10 EASY\". Verborgen wanneer Medium unlocked.")]
    [SerializeField] private TMP_Text mediumProgressText;

    [Tooltip("Optioneel. Bijv. \"EASY 7/10 • MEDIUM 3/10\". Verborgen wanneer Hard unlocked.")]
    [SerializeField] private TMP_Text hardProgressText;

    [Tooltip("Optioneel titel boven de grid (bijv. EASY).")]
    [SerializeField] private TMP_Text difficultyTitleText;

    [Tooltip("Optioneel. Getoond wanneer de geselecteerde difficulty 0 levels heeft.")]
    [SerializeField] private TMP_Text emptyStateText;

    [Header("Selected Tab Visual")]
    [SerializeField] private float selectedTabScale = 1.05f;
    [SerializeField] private float unselectedTabScale = 1f;

    [Tooltip("Optioneel. Active alleen wanneer Easy geselecteerd is.")]
    [SerializeField] private GameObject easySelectedIndicator;

    [SerializeField] private GameObject mediumSelectedIndicator;
    [SerializeField] private GameObject hardSelectedIndicator;

    [Header("Locked Tab Feedback")]
    [SerializeField] private float lockedTabPunchScale = 1.08f;
    [SerializeField] private float lockedTabPunchDuration = 0.12f;

    [Header("Referenties")]
    [SerializeField] private SaveManager saveManager;

    [Tooltip("Centrale database met alle levels.")]
    [SerializeField] private LevelDatabase levelDatabase;

    [Tooltip("Optioneel. Null = built-in unlock defaults (10 Easy / 10 Easy + 10 Medium).")]
    [SerializeField] private DifficultyProgressionConfig difficultyProgressionConfig;

    private LevelDifficulty selectedDifficulty = LevelDifficulty.Easy;
    private AudioManager audioManager;
    private Coroutine lockedTabPunchCoroutine;
    private Transform lockedTabPunchTarget;
    private Vector3 lockedTabPunchBaseScale = Vector3.one;

    private void Start()
    {
        audioManager = FindAnyObjectByType<AudioManager>();

        if (saveManager != null && levelDatabase != null)
        {
            saveManager.RepairUnlockedProgress(levelDatabase.LevelCount);
        }

        WireTabButtons();
        selectedDifficulty = ResolveInitialDifficulty();
        RefreshAll();
    }

    private void OnDisable()
    {
        StopLockedTabPunchImmediate();
    }

    private void WireTabButtons()
    {
        if (easyTabButton != null)
        {
            easyTabButton.onClick.RemoveListener(OnEasyTabClicked);
            easyTabButton.onClick.AddListener(OnEasyTabClicked);
        }

        if (mediumTabButton != null)
        {
            mediumTabButton.onClick.RemoveListener(OnMediumTabClicked);
            mediumTabButton.onClick.AddListener(OnMediumTabClicked);
        }

        if (hardTabButton != null)
        {
            hardTabButton.onClick.RemoveListener(OnHardTabClicked);
            hardTabButton.onClick.AddListener(OnHardTabClicked);
        }
    }

    private LevelDifficulty ResolveInitialDifficulty()
    {
        if (saveManager == null || levelDatabase == null)
        {
            return LevelDifficulty.Easy;
        }

        LevelDifficulty last = saveManager.GetLastSelectedDifficulty();
        if (saveManager.IsDifficultyUnlocked(
                last,
                levelDatabase,
                difficultyProgressionConfig))
        {
            return last;
        }

        return LevelDifficulty.Easy;
    }

    private void OnEasyTabClicked()
    {
        SelectDifficulty(LevelDifficulty.Easy);
    }

    private void OnMediumTabClicked()
    {
        TrySelectDifficulty(LevelDifficulty.Medium, mediumTabButton);
    }

    private void OnHardTabClicked()
    {
        TrySelectDifficulty(LevelDifficulty.Hard, hardTabButton);
    }

    private void TrySelectDifficulty(LevelDifficulty difficulty, Button tabButton)
    {
        if (saveManager == null || levelDatabase == null)
        {
            return;
        }

        if (saveManager.IsDifficultyUnlocked(
                difficulty,
                levelDatabase,
                difficultyProgressionConfig))
        {
            SelectDifficulty(difficulty);
            return;
        }

        PlayLockedTabFeedback(tabButton != null ? tabButton.transform : null);
    }

    /// <summary>
    /// Wisselt tab (alleen aanroepen wanneer difficulty unlocked is).
    /// </summary>
    private void SelectDifficulty(LevelDifficulty difficulty)
    {
        selectedDifficulty = difficulty;

        if (saveManager != null)
        {
            saveManager.SaveLastSelectedDifficulty(difficulty);
        }

        GameAnalytics.LogDifficultySelected(difficulty.ToString());

        RefreshAll();
    }

    /// <summary>
    /// Herrekent unlock/progress + rebuildt buttons voor selectedDifficulty.
    /// Geen cache: altijd live SaveManager counts.
    /// </summary>
    private void RefreshAll()
    {
        RefreshTabChrome();
        RebuildLevelButtonsForSelectedDifficulty();

        // Layout owns presentation — re-apply after tiles so Wide is not lost.
        LevelSelectLayoutController layout = GetComponent<LevelSelectLayoutController>();
        if (layout == null)
        {
            layout = FindAnyObjectByType<LevelSelectLayoutController>();
        }

        if (layout != null)
        {
            layout.ApplyAfterContentReady();
        }
    }

    private void RefreshTabChrome()
    {
        bool mediumUnlocked = IsDifficultyUnlockedSafe(LevelDifficulty.Medium);
        bool hardUnlocked = IsDifficultyUnlockedSafe(LevelDifficulty.Hard);

        if (mediumLockIcon != null)
        {
            mediumLockIcon.SetActive(!mediumUnlocked);
        }

        if (hardLockIcon != null)
        {
            hardLockIcon.SetActive(!hardUnlocked);
        }

        UpdateMediumProgressText(mediumUnlocked);
        UpdateHardProgressText(hardUnlocked);
        UpdateSelectedTabVisual();
        UpdateDifficultyTitle();
    }

    private void UpdateMediumProgressText(bool mediumUnlocked)
    {
        if (mediumProgressText == null)
        {
            return;
        }

        if (mediumUnlocked)
        {
            mediumProgressText.gameObject.SetActive(false);
            return;
        }

        int easyCompleted = GetCompletedCountSafe(LevelDifficulty.Easy);
        int required = GetMediumRequiredEasy();
        mediumProgressText.gameObject.SetActive(true);
        mediumProgressText.text = easyCompleted + " / " + required + " EASY";
    }

    private void UpdateHardProgressText(bool hardUnlocked)
    {
        if (hardProgressText == null)
        {
            return;
        }

        if (hardUnlocked)
        {
            hardProgressText.gameObject.SetActive(false);
            return;
        }

        int easyCompleted = GetCompletedCountSafe(LevelDifficulty.Easy);
        int mediumCompleted = GetCompletedCountSafe(LevelDifficulty.Medium);
        int easyRequired = GetHardRequiredEasy();
        int mediumRequired = GetHardRequiredMedium();

        string easyPart = easyCompleted >= easyRequired
            ? "EASY ✓"
            : "EASY " + easyCompleted + "/" + easyRequired;

        string mediumPart = mediumCompleted >= mediumRequired
            ? "MEDIUM ✓"
            : "MEDIUM " + mediumCompleted + "/" + mediumRequired;

        hardProgressText.gameObject.SetActive(true);
        hardProgressText.text = easyPart + " • " + mediumPart;
    }

    private void UpdateSelectedTabVisual()
    {
        ApplyTabScale(easyTabButton, selectedDifficulty == LevelDifficulty.Easy);
        ApplyTabScale(mediumTabButton, selectedDifficulty == LevelDifficulty.Medium);
        ApplyTabScale(hardTabButton, selectedDifficulty == LevelDifficulty.Hard);

        if (easySelectedIndicator != null)
        {
            easySelectedIndicator.SetActive(selectedDifficulty == LevelDifficulty.Easy);
        }

        if (mediumSelectedIndicator != null)
        {
            mediumSelectedIndicator.SetActive(selectedDifficulty == LevelDifficulty.Medium);
        }

        if (hardSelectedIndicator != null)
        {
            hardSelectedIndicator.SetActive(selectedDifficulty == LevelDifficulty.Hard);
        }
    }

    private void ApplyTabScale(Button tabButton, bool selected)
    {
        if (tabButton == null)
        {
            return;
        }

        // Niet overschrijven tijdens locked punch.
        if (lockedTabPunchCoroutine != null && lockedTabPunchTarget == tabButton.transform)
        {
            return;
        }

        float scale = selected ? selectedTabScale : unselectedTabScale;
        tabButton.transform.localScale = Vector3.one * scale;
    }

    private void UpdateDifficultyTitle()
    {
        if (difficultyTitleText == null)
        {
            return;
        }

        difficultyTitleText.text = selectedDifficulty.ToString().ToUpperInvariant();
    }

    private void RebuildLevelButtonsForSelectedDifficulty()
    {
        if (levelButtonPrefab == null || levelGrid == null)
        {
            Debug.LogError("LevelSelectUI: levelButtonPrefab of levelGrid ontbreekt.");
            return;
        }

        if (levelDatabase == null)
        {
            Debug.LogError("LevelSelectUI: levelDatabase ontbreekt.");
            return;
        }

        ClearExistingButtons();

        List<int> indices = LevelDifficultyOrder.GetOrderedLevelIndicesForDifficulty(
            levelDatabase,
            selectedDifficulty
        );
        int visibleCount = indices != null ? indices.Count : 0;

        if (emptyStateText != null)
        {
            emptyStateText.gameObject.SetActive(visibleCount == 0);
            if (visibleCount == 0)
            {
                emptyStateText.text = "NO LEVELS YET";
            }
        }

        for (int visibleIndex = 0; visibleIndex < visibleCount; visibleIndex++)
        {
            int databaseIndex = indices[visibleIndex];
            int displayNumber = visibleIndex + 1;

            GameObject buttonObject = Instantiate(levelButtonPrefab, levelGrid);
            LevelButtonUI buttonUI = buttonObject.GetComponent<LevelButtonUI>();

            if (buttonUI == null)
            {
                Debug.LogError("LevelSelectUI: prefab mist LevelButtonUI.");
                continue;
            }

            bool isUnlocked = saveManager != null
                && saveManager.IsLevelUnlocked(
                    databaseIndex,
                    levelDatabase,
                    difficultyProgressionConfig);

            int stars = saveManager != null
                ? saveManager.GetStarsForLevel(databaseIndex)
                : 0;

            LevelData levelData = levelDatabase.GetLevel(databaseIndex);
            LevelObjectiveType objectiveType = levelData != null
                ? levelData.objectiveType
                : LevelObjectiveType.Classic;

            buttonUI.Setup(
                databaseIndex,
                displayNumber,
                isUnlocked,
                stars,
                filledStarSprite,
                emptyStarSprite,
                objectiveType
            );

            Button button = buttonUI.Button != null
                ? buttonUI.Button
                : buttonObject.GetComponent<Button>();

            if (button == null)
            {
                Debug.LogError("LevelSelectUI: prefab heeft geen Button-component.");
                continue;
            }

            // Drop prefab-persistent + prior runtime listeners so only DB-index click remains.
            button.onClick = new Button.ButtonClickedEvent();

            if (isUnlocked)
            {
                // Capture LevelButtonUI (not loop index) — click uses stored DatabaseIndex.
                LevelButtonUI capturedButton = buttonUI;
                button.onClick.AddListener(() => OnLevelButtonClicked(capturedButton));
            }
        }

        Canvas.ForceUpdateCanvases();
        UpdateScrollContentHeight(visibleCount);

        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void ClearExistingButtons()
    {
        if (levelGrid == null)
        {
            return;
        }

        for (int i = levelGrid.childCount - 1; i >= 0; i--)
        {
            Destroy(levelGrid.GetChild(i).gameObject);
        }
    }

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

        int columns = GetGridColumnCount();
        int rows = Mathf.Max(1, Mathf.CeilToInt(levelCount / (float)columns));
        if (levelCount == 0)
        {
            rows = 1;
        }

        float height =
            grid.padding.top +
            grid.padding.bottom +
            rows * grid.cellSize.y +
            Mathf.Max(0, rows - 1) * grid.spacing.y;

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
    /// Prefer live GridLayoutGroup constraint so Wide layout can change columns
    /// without touching unlock/data logic.
    /// </summary>
    private int GetGridColumnCount()
    {
        if (levelGrid != null)
        {
            GridLayoutGroup grid = levelGrid.GetComponent<GridLayoutGroup>();
            if (grid != null &&
                grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount &&
                grid.constraintCount > 0)
            {
                return grid.constraintCount;
            }
        }

        return FallbackGridColumns;
    }

    /// <summary>
    /// Called by LevelSelectLayoutController after Wide/phone grid settings change.
    /// </summary>
    public void RefreshScrollContentForCurrentGrid()
    {
        if (levelGrid == null)
        {
            return;
        }

        UpdateScrollContentHeight(levelGrid.childCount);
    }

    private void OnLevelButtonClicked(LevelButtonUI buttonUI)
    {
        if (buttonUI == null)
        {
            Debug.LogError("LevelSelectUI: clicked LevelButtonUI is null.");
            return;
        }

        int databaseIndex = buttonUI.DatabaseIndex;
        if (databaseIndex < 0 || levelDatabase == null ||
            databaseIndex >= levelDatabase.LevelCount)
        {
            Debug.LogError(
                "LevelSelectUI: invalid DatabaseIndex on button: " + databaseIndex
            );
            return;
        }

        LevelData levelData = levelDatabase.GetLevel(databaseIndex);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[LevelSelectClick]\n" +
            "Difficulty=" + selectedDifficulty + "\n" +
            "DisplayNumber=" + buttonUI.DisplayNumber + "\n" +
            "DatabaseIndex=" + databaseIndex + "\n" +
            "Asset=" + (levelData != null ? levelData.name : "?") + "\n" +
            "MinMoves=" + (levelData != null ? levelData.minimumMoves : 0)
        );
#endif

        if (saveManager != null)
        {
            saveManager.SaveCurrentLevel(databaseIndex);
        }

        SceneTransition.LoadScene("Gameplay");
    }

    public void OnBackButton()
    {
        SceneTransition.LoadScene("MainMenu");
    }

    private void PlayLockedTabFeedback(Transform tabTransform)
    {
        audioManager?.PlayBlocked();

        if (tabTransform == null)
        {
            return;
        }

        StopLockedTabPunchImmediate();
        lockedTabPunchTarget = tabTransform;
        lockedTabPunchBaseScale = tabTransform.localScale;
        lockedTabPunchCoroutine = StartCoroutine(
            LockedTabPunchRoutine(tabTransform, lockedTabPunchBaseScale)
        );
    }

    private IEnumerator LockedTabPunchRoutine(Transform target, Vector3 baseScale)
    {
        float duration = Mathf.Max(0.01f, lockedTabPunchDuration);
        float half = duration * 0.5f;
        Vector3 peak = baseScale * GetLockedTabPunchPeakMultiplier();

        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / half);
            target.localScale = Vector3.LerpUnclamped(baseScale, peak, u);
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / half);
            target.localScale = Vector3.LerpUnclamped(peak, baseScale, u);
            yield return null;
        }

        target.localScale = baseScale;
        lockedTabPunchCoroutine = null;
        lockedTabPunchTarget = null;

        // Herstel selected/unselected scale na punch.
        UpdateSelectedTabVisual();
    }

    private float GetLockedTabPunchPeakMultiplier()
    {
        return Mathf.Max(1.01f, lockedTabPunchScale);
    }

    private void StopLockedTabPunchImmediate()
    {
        if (lockedTabPunchCoroutine != null)
        {
            StopCoroutine(lockedTabPunchCoroutine);
            lockedTabPunchCoroutine = null;
        }

        if (lockedTabPunchTarget != null)
        {
            lockedTabPunchTarget.localScale = lockedTabPunchBaseScale;
            lockedTabPunchTarget = null;
        }
    }

    private bool IsDifficultyUnlockedSafe(LevelDifficulty difficulty)
    {
        if (saveManager == null || levelDatabase == null)
        {
            return difficulty == LevelDifficulty.Easy;
        }

        return saveManager.IsDifficultyUnlocked(
            difficulty,
            levelDatabase,
            difficultyProgressionConfig
        );
    }

    private int GetCompletedCountSafe(LevelDifficulty difficulty)
    {
        if (saveManager == null || levelDatabase == null)
        {
            return 0;
        }

        return saveManager.GetCompletedCount(difficulty, levelDatabase);
    }

    private int GetMediumRequiredEasy()
    {
        return difficultyProgressionConfig != null
            ? difficultyProgressionConfig.mediumRequiredEasy
            : DifficultyProgressionConfig.DefaultMediumRequiredEasy;
    }

    private int GetHardRequiredEasy()
    {
        return difficultyProgressionConfig != null
            ? difficultyProgressionConfig.hardRequiredEasy
            : DifficultyProgressionConfig.DefaultHardRequiredEasy;
    }

    private int GetHardRequiredMedium()
    {
        return difficultyProgressionConfig != null
            ? difficultyProgressionConfig.hardRequiredMedium
            : DifficultyProgressionConfig.DefaultHardRequiredMedium;
    }
}

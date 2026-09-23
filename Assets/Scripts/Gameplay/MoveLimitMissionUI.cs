using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-bridge voor MoveLimit: remaining-HUD + MissionFailedLimitPanel.
/// Also presents forgiving global move-limit failures on the SAME panel
/// (one UI owner — no duplicate OutOfMovesPanel).
/// Maakt geen GameObjects — alles Inspector-gekoppeld. Alleen presentation.
/// </summary>
public class MoveLimitMissionUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LevelObjectiveController objectiveController;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioManager audioManager;

    [Header("Move Limit HUD")]
    [SerializeField] private GameObject moveLimitHudRoot;
    [SerializeField] private TextMeshProUGUI movesRemainingText;
    [SerializeField] private TextMeshProUGUI missionLabel;

    /// <summary>Bestaande MissionLabel RectTransform voor SpecialMissionIntro.</summary>
    public RectTransform MissionLabelRect =>
        missionLabel != null ? missionLabel.rectTransform : null;

    [SerializeField] private string moveLimitMissionLabel = "MOVE LIMIT";

    [Header("Failure panel (MissionFailedLimitPanel)")]
    [SerializeField] private GameObject missionFailedPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button levelSelectButton;
    [SerializeField] private Button backButton;

    [Header("Special MoveLimit failure copy (optioneel)")]
    [Tooltip("Uit = TMP-tekst die jij handmatig zette blijft staan bij special MoveLimit.")]
    [SerializeField] private bool applyFailureCopy = false;

    [SerializeField] private TextMeshProUGUI missionFailedTitle;
    [SerializeField] private TextMeshProUGUI missionFailedDescription;

    [SerializeField] private string failureTitle = "OUT OF MOVES!";
    [SerializeField] private string failureDescription =
        "TRY AGAIN AND FIND A SHORTER ROUTE";

    [Header("Global move-limit failure copy")]
    [SerializeField] private string globalFailureTitle = "OUT OF MOVES";
    [SerializeField] private string globalFailureDescription = "You ran out of moves.";

    [Header("Move Limit Urgency")]
    [SerializeField, Range(1, 20)] private int warningMoves = 3;
    [SerializeField, Range(1, 20)] private int dangerMoves = 2;
    [SerializeField, Range(1, 20)] private int criticalMoves = 1;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = new Color(1f, 0.78f, 0.15f, 1f);
    [SerializeField] private Color dangerColor = new Color(1f, 0.45f, 0.18f, 1f);
    [SerializeField] private Color criticalColor = new Color(1f, 0.28f, 0.22f, 1f);

    [SerializeField, Range(0.1f, 8f)] private float criticalPulseSpeed = 2f;
    [SerializeField, Range(1f, 1.25f)] private float criticalPulseScale = 1.08f;

    private Vector3 originalRemainingScale = Vector3.one;
    private Color originalRemainingColor = Color.white;
    private bool colorCached;
    private bool authoredScaleCached;
    private bool pulseActive;
    private GameplayLayoutController layoutController;
    private bool warningSfxPlayed;
    private bool failurePresentationPlayed;

    /// <summary>
    /// True while MissionFailedLimitPanel is shown for a forgiving global limit fail.
    /// Prevents RefreshFromController from closing it on Classic levels.
    /// </summary>
    private bool showingGlobalMoveLimitFailure;

    private void Awake()
    {
        ResolveRefs();

        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(false);
        }

        showingGlobalMoveLimitFailure = false;
        CachePulseColorIfNeeded();
        SetMoveLimitHudVisible(false);
    }

    private void OnEnable()
    {
        ResolveRefs();

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartClicked);
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (levelSelectButton != null)
        {
            levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
            levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (objectiveController != null)
        {
            objectiveController.OnMovesRemainingChanged += OnMovesRemainingChanged;
            objectiveController.OnMoveLimitMissionFailed += OnMoveLimitMissionFailed;
            RefreshFromController();
        }

        if (gameManager != null)
        {
            gameManager.OnGlobalMoveLimitFailed -= OnGlobalMoveLimitFailed;
            gameManager.OnGlobalMoveLimitFailed += OnGlobalMoveLimitFailed;
        }
    }

    private void OnDisable()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        if (levelSelectButton != null)
        {
            levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
        }

        if (objectiveController != null)
        {
            objectiveController.OnMovesRemainingChanged -= OnMovesRemainingChanged;
            objectiveController.OnMoveLimitMissionFailed -= OnMoveLimitMissionFailed;
        }

        if (gameManager != null)
        {
            gameManager.OnGlobalMoveLimitFailed -= OnGlobalMoveLimitFailed;
        }

        StopPulseAndResetScale();
    }

    private void ResolveRefs()
    {
        if (objectiveController == null)
        {
            objectiveController = FindAnyObjectByType<LevelObjectiveController>();
        }

        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }
    }

    private void Update()
    {
        if (!pulseActive || movesRemainingText == null)
        {
            return;
        }

        if (objectiveController == null ||
            !objectiveController.IsMoveLimitLevel ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Completed ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Failed ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Inactive)
        {
            StopPulseAndResetScale();
            return;
        }

        if (Time.timeScale <= 0f)
        {
            return;
        }

        float wave = (Mathf.Sin(Time.unscaledTime * criticalPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        float scaleMul = Mathf.Lerp(1f, criticalPulseScale, wave);
        movesRemainingText.rectTransform.localScale = ResolvePulseBaseScale() * scaleMul;
    }

    private void OnMovesRemainingChanged(int remaining)
    {
        RefreshFromController(remaining);
    }

    /// <summary>Special ObjectiveType.MoveLimit failure — existing mission flow.</summary>
    private void OnMoveLimitMissionFailed()
    {
        showingGlobalMoveLimitFailure = false;
        // Apply special copy when text refs exist so a prior global fail cannot leave
        // global wording on the shared MissionFailedLimitPanel.
        bool shouldApply =
            applyFailureCopy ||
            missionFailedTitle != null ||
            missionFailedDescription != null;
        PresentFailurePanel(applyCopy: shouldApply, useGlobalCopy: false);
    }

    /// <summary>
    /// Forgiving global move-limit failure. Reuses MissionFailedLimitPanel + buttons.
    /// </summary>
    private void OnGlobalMoveLimitFailed()
    {
        showingGlobalMoveLimitFailure = true;
        PresentFailurePanel(applyCopy: true, useGlobalCopy: true);
    }

    private void PresentFailurePanel(bool applyCopy, bool useGlobalCopy)
    {
        StopPulseAndResetScale();

        if (!failurePresentationPlayed)
        {
            failurePresentationPlayed = true;

            if (audioManager == null)
            {
                audioManager = FindAnyObjectByType<AudioManager>();
            }

            if (audioManager != null)
            {
                audioManager.PlayMoveLimitFailed();
            }

            HapticManager.PlayMediumImpact();
        }

        if (applyCopy)
        {
            ApplyFailureCopy(useGlobalCopy);
        }

        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "MoveLimitMissionUI: MissionFailedLimitPanel is not assigned."
            );
        }
    }

    private void OnRestartClicked()
    {
        StopPulseAndResetScale();
        ApplyRemainingColor(normalColor);
        ResetAudioPresentationState();

        bool keepGlobalContext = showingGlobalMoveLimitFailure;

        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(false);
        }

        showingGlobalMoveLimitFailure = false;
        Time.timeScale = 1f;

        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        if (levelManager != null)
        {
            // Zero-lives gate lives inside RestartLevel → OutOfLivesUI via LivesManager.
            levelManager.RestartLevel();
        }
        else
        {
            Debug.LogError("MoveLimitMissionUI: geen LevelManager voor RestartLevel.");
            return;
        }

        // Retry blocked at 0 lives — keep MissionFailedLimitPanel under OutOfLives
        // so the player can Retry again after earning a life (no auto-retry).
        if (gameManager != null && gameManager.IsLevelFailed)
        {
            failurePresentationPlayed = true;
            showingGlobalMoveLimitFailure = keepGlobalContext;
            if (missionFailedPanel != null)
            {
                missionFailedPanel.SetActive(true);
            }
        }
    }

    private void OnLevelSelectClicked()
    {
        StopPulseAndResetScale();
        showingGlobalMoveLimitFailure = false;

        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        SceneTransition.LoadScene("LevelSelect");
    }

    /// <summary>
    /// Zelfde Back-doel als PauseManager / GameplayUI: MainMenu.
    /// </summary>
    private void OnBackClicked()
    {
        StopPulseAndResetScale();
        showingGlobalMoveLimitFailure = false;

        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        SceneTransition.LoadScene("MainMenu");
    }

    private void RefreshFromController()
    {
        int remaining = objectiveController != null
            ? objectiveController.MovesRemaining
            : 0;
        RefreshFromController(remaining);
    }

    private void RefreshFromController(int remaining)
    {
        bool moveLimit = objectiveController != null &&
                         objectiveController.IsMoveLimitLevel;

        if (!moveLimit)
        {
            if (!showingGlobalMoveLimitFailure && missionFailedPanel != null)
            {
                missionFailedPanel.SetActive(false);
            }

            StopPulseAndResetScale();
            ApplyRemainingColor(normalColor);
            if (!showingGlobalMoveLimitFailure)
            {
                ResetAudioPresentationState();
            }

            SetMoveLimitHudVisible(false);
            return;
        }

        if (objectiveController.State != LevelObjectiveController.RuntimeState.Failed &&
            missionFailedPanel != null &&
            missionFailedPanel.activeSelf &&
            !showingGlobalMoveLimitFailure)
        {
            missionFailedPanel.SetActive(false);
        }

        if (remaining > warningMoves &&
            objectiveController.State == LevelObjectiveController.RuntimeState.Running)
        {
            warningSfxPlayed = false;
            failurePresentationPlayed = false;
            showingGlobalMoveLimitFailure = false;
        }

        SetMoveLimitHudVisible(true);

        if (missionLabel != null)
        {
            missionLabel.text = moveLimitMissionLabel;
        }

        if (movesRemainingText != null)
        {
            movesRemainingText.text = remaining.ToString();
        }

        ApplyUrgencyVisuals(remaining);
        ProcessWarningSfx(remaining);
    }

    private void ProcessWarningSfx(int remaining)
    {
        if (objectiveController == null ||
            !objectiveController.IsMoveLimitLevel ||
            objectiveController.State != LevelObjectiveController.RuntimeState.Running)
        {
            return;
        }

        if (remaining != warningMoves || warningSfxPlayed)
        {
            return;
        }

        warningSfxPlayed = true;

        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }

        if (audioManager != null)
        {
            audioManager.PlayMoveLimitWarning();
        }
    }

    private void ResetAudioPresentationState()
    {
        warningSfxPlayed = false;
        failurePresentationPlayed = false;
    }

    private void ApplyUrgencyVisuals(int remaining)
    {
        CachePulseColorIfNeeded();

        if (movesRemainingText == null)
        {
            return;
        }

        if (objectiveController != null &&
            objectiveController.State == LevelObjectiveController.RuntimeState.Completed)
        {
            StopPulseAndResetScale();
            ApplyRemainingColor(normalColor);
            return;
        }

        if (remaining <= 0)
        {
            StopPulseAndResetScale();
            ApplyRemainingColor(criticalColor);
            return;
        }

        if (remaining <= criticalMoves)
        {
            ApplyRemainingColor(criticalColor);
            pulseActive = true;
            return;
        }

        StopPulseAndResetScale();

        if (remaining <= dangerMoves)
        {
            ApplyRemainingColor(dangerColor);
            return;
        }

        if (remaining <= warningMoves)
        {
            ApplyRemainingColor(warningColor);
            return;
        }

        ApplyRemainingColor(normalColor);
    }

    private void CachePulseColorIfNeeded()
    {
        if (colorCached || movesRemainingText == null)
        {
            return;
        }

        originalRemainingColor = movesRemainingText.color;
        colorCached = true;
    }

    private Vector3 ResolvePulseBaseScale()
    {
        if (movesRemainingText == null)
        {
            return Vector3.one;
        }

        if (layoutController == null)
        {
            layoutController = FindAnyObjectByType<GameplayLayoutController>();
        }

        if (layoutController != null &&
            layoutController.TryGetPhoneSecondaryLocalScale(
                movesRemainingText.rectTransform,
                out Vector3 configured))
        {
            originalRemainingScale = configured;
            return configured;
        }

        if (!authoredScaleCached)
        {
            originalRemainingScale = movesRemainingText.rectTransform.localScale;
            authoredScaleCached = true;
        }

        return originalRemainingScale;
    }

    private void ApplyRemainingColor(Color color)
    {
        if (movesRemainingText == null)
        {
            return;
        }

        movesRemainingText.color = color;
    }

    private void StopPulseAndResetScale()
    {
        pulseActive = false;

        if (movesRemainingText == null)
        {
            return;
        }

        movesRemainingText.rectTransform.localScale = ResolvePulseBaseScale();
    }

    private void ApplyFailureCopy(bool useGlobalCopy)
    {
        string title = useGlobalCopy ? globalFailureTitle : failureTitle;
        string description = useGlobalCopy ? globalFailureDescription : failureDescription;

        if (missionFailedTitle != null)
        {
            missionFailedTitle.text = title;
        }

        if (missionFailedDescription != null)
        {
            missionFailedDescription.text = description;
        }
    }

    private void SetMoveLimitHudVisible(bool visible)
    {
        if (moveLimitHudRoot != null)
        {
            moveLimitHudRoot.SetActive(visible);
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-bridge voor MoveLimit: remaining-HUD + MissionFailedLimitPanel.
/// Maakt geen GameObjects — alles Inspector-gekoppeld. Alleen presentation.
/// </summary>
public class MoveLimitMissionUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LevelObjectiveController objectiveController;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private AudioManager audioManager;

    [Header("Move Limit HUD")]
    [SerializeField] private GameObject moveLimitHudRoot;
    [SerializeField] private TextMeshProUGUI movesRemainingText;
    [SerializeField] private TextMeshProUGUI missionLabel;

    /// <summary>Bestaande MissionLabel RectTransform voor SpecialMissionIntro.</summary>
    public RectTransform MissionLabelRect =>
        missionLabel != null ? missionLabel.rectTransform : null;

    [SerializeField] private string moveLimitMissionLabel = "MOVE LIMIT";

    [Header("Failure panel")]
    [SerializeField] private GameObject missionFailedPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button levelSelectButton;
    [SerializeField] private Button backButton;

    [Header("Failure copy (optioneel)")]
    [Tooltip("Uit = TMP-tekst die jij handmatig zette blijft staan.")]
    [SerializeField] private bool applyFailureCopy = false;

    [SerializeField] private TextMeshProUGUI missionFailedTitle;
    [SerializeField] private TextMeshProUGUI missionFailedDescription;

    [SerializeField] private string failureTitle = "OUT OF MOVES!";
    [SerializeField] private string failureDescription =
        "TRY AGAIN AND FIND A SHORTER ROUTE";

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
    private bool visualsCached;
    private bool pulseActive;
    private bool warningSfxPlayed;
    private bool failurePresentationPlayed;

    private void Awake()
    {
        if (objectiveController == null)
        {
            objectiveController = FindAnyObjectByType<LevelObjectiveController>();
        }

        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }

        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(false);
        }

        CacheRemainingVisualsIfNeeded();
        SetMoveLimitHudVisible(false);
    }

    private void OnEnable()
    {
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

        StopPulseAndResetScale();
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

        // Pause: freeze mid-pulse (zelfde patroon als TimedMissionUI).
        if (Time.timeScale <= 0f)
        {
            return;
        }

        float wave = (Mathf.Sin(Time.unscaledTime * criticalPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        float scaleMul = Mathf.Lerp(1f, criticalPulseScale, wave);
        movesRemainingText.rectTransform.localScale = originalRemainingScale * scaleMul;
    }

    private void OnMovesRemainingChanged(int remaining)
    {
        RefreshFromController(remaining);
    }

    private void OnMoveLimitMissionFailed()
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

        if (applyFailureCopy)
        {
            ApplyFailureCopy();
        }

        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(true);
        }
    }

    private void OnRestartClicked()
    {
        StopPulseAndResetScale();
        ApplyRemainingColor(normalColor);
        ResetAudioPresentationState();

        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(false);
        }

        Time.timeScale = 1f;

        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        if (levelManager != null)
        {
            levelManager.RestartLevel();
        }
        else
        {
            Debug.LogError("MoveLimitMissionUI: geen LevelManager voor RestartLevel.");
        }
    }

    private void OnLevelSelectClicked()
    {
        StopPulseAndResetScale();

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
            if (missionFailedPanel != null)
            {
                missionFailedPanel.SetActive(false);
            }

            StopPulseAndResetScale();
            ApplyRemainingColor(normalColor);
            ResetAudioPresentationState();
            SetMoveLimitHudVisible(false);
            return;
        }

        // Nieuw/restart MoveLimit: failure panel dicht tenzij Failed.
        if (objectiveController.State != LevelObjectiveController.RuntimeState.Failed &&
            missionFailedPanel != null &&
            missionFailedPanel.activeSelf)
        {
            missionFailedPanel.SetActive(false);
        }

        // Volledige limiet weer zichtbaar → warning opnieuw toestaan.
        if (remaining > warningMoves &&
            objectiveController.State == LevelObjectiveController.RuntimeState.Running)
        {
            warningSfxPlayed = false;
            failurePresentationPlayed = false;
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
        CacheRemainingVisualsIfNeeded();

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

    private void CacheRemainingVisualsIfNeeded()
    {
        if (visualsCached || movesRemainingText == null)
        {
            return;
        }

        originalRemainingScale = movesRemainingText.rectTransform.localScale;
        originalRemainingColor = movesRemainingText.color;
        visualsCached = true;
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

        CacheRemainingVisualsIfNeeded();
        movesRemainingText.rectTransform.localScale = originalRemainingScale;
    }

    private void ApplyFailureCopy()
    {
        if (missionFailedTitle != null)
        {
            missionFailedTitle.text = failureTitle;
        }

        if (missionFailedDescription != null)
        {
            missionFailedDescription.text = failureDescription;
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

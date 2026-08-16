using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-bridge voor FragileCargo: cargo-moves HUD + MissionFailedFragileCargoPanel.
/// Maakt geen GameObjects — alles Inspector-gekoppeld. Alleen presentation.
/// Geen warning-SFX/haptic in v1. Failure SFX/haptic via OnFragileCargoMissionFailed.
/// Failure panel uitsluitend via OnFragileCargoMissionFailed (remaining==0 ≠ fail).
/// </summary>
public class FragileCargoMissionUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LevelObjectiveController objectiveController;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private AudioManager audioManager;

    [Header("Fragile Cargo HUD")]
    [SerializeField] private GameObject fragileCargoHudRoot;
    [SerializeField] private TextMeshProUGUI missionLabel;
    [SerializeField] private TextMeshProUGUI movesText;

    [Tooltip("Uit = MissionLabel-tekst die jij handmatig zette blijft staan.")]
    [SerializeField] private bool applyMissionLabel = false;

    [SerializeField] private string fragileCargoMissionLabel = "FRAGILE CARGO";

    [Header("Failure panel")]
    [SerializeField] private GameObject missionFailedPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button levelSelectButton;
    [SerializeField] private Button menuButton;

    [Header("Failure copy (optioneel)")]
    [Tooltip("Uit = TMP-tekst die jij handmatig zette blijft staan.")]
    [SerializeField] private bool applyFailureCopy = false;

    [SerializeField] private TextMeshProUGUI missionFailedTitle;
    [SerializeField] private TextMeshProUGUI missionFailedDescription;

    [SerializeField] private string failureTitle = "MISSION FAILED";
    [SerializeField] private string failureDescription =
        "CARGO MOVE LIMIT EXCEEDED";

    [Header("Cargo Move Urgency")]
    [SerializeField, Range(1, 20)] private int warningMoves = 2;
    [SerializeField, Range(1, 20)] private int criticalMoves = 1;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = new Color(1f, 0.78f, 0.15f, 1f);
    [SerializeField] private Color criticalColor = new Color(1f, 0.28f, 0.22f, 1f);

    [SerializeField, Range(0.1f, 8f)] private float criticalPulseSpeed = 2f;
    [SerializeField, Range(1f, 1.25f)] private float criticalPulseScale = 1.08f;

    private bool lastKnownFragileCargo;
    private LevelObjectiveController.RuntimeState lastKnownState =
        LevelObjectiveController.RuntimeState.Inactive;

    private Vector3 originalMovesScale = Vector3.one;
    private Color originalMovesColor = Color.white;
    private bool visualsCached;
    private bool pulseActive;
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

        CacheMovesVisualsIfNeeded();
        SetFragileCargoHudVisible(false);
        lastKnownFragileCargo = false;
        lastKnownState = LevelObjectiveController.RuntimeState.Inactive;
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

        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(OnMenuClicked);
            menuButton.onClick.AddListener(OnMenuClicked);
        }

        if (objectiveController != null)
        {
            objectiveController.OnCargoMovesRemainingChanged += OnCargoMovesRemainingChanged;
            objectiveController.OnFragileCargoMissionFailed += OnFragileCargoMissionFailed;
            RefreshFromController();
            CacheObjectiveSnapshot();
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

        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(OnMenuClicked);
        }

        if (objectiveController != null)
        {
            objectiveController.OnCargoMovesRemainingChanged -= OnCargoMovesRemainingChanged;
            objectiveController.OnFragileCargoMissionFailed -= OnFragileCargoMissionFailed;
        }

        StopPulseAndResetScale();
    }

    private void Update()
    {
        if (!pulseActive || movesText == null)
        {
            return;
        }

        if (objectiveController == null ||
            !objectiveController.IsFragileCargoLevel ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Completed ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Failed ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Inactive)
        {
            StopPulseAndResetScale();
            return;
        }

        // Pause: freeze mid-pulse (zelfde patroon als MoveLimit/Timed).
        if (Time.timeScale <= 0f)
        {
            return;
        }

        float wave = (Mathf.Sin(Time.unscaledTime * criticalPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        float scaleMul = Mathf.Lerp(1f, criticalPulseScale, wave);
        movesText.rectTransform.localScale = originalMovesScale * scaleMul;
    }

    /// <summary>
    /// Sync HUD visibility bij Completed/Failed/restart zonder gameplay te wijzigen.
    /// Counter-updates komen via OnCargoMovesRemainingChanged.
    /// </summary>
    private void LateUpdate()
    {
        if (objectiveController == null)
        {
            return;
        }

        bool fragileCargo = objectiveController.IsFragileCargoLevel;
        LevelObjectiveController.RuntimeState state = objectiveController.State;

        if (fragileCargo == lastKnownFragileCargo && state == lastKnownState)
        {
            return;
        }

        CacheObjectiveSnapshot();
        RefreshFromController();
    }

    private void OnCargoMovesRemainingChanged(int remaining, int total)
    {
        RefreshFromController(remaining, total);
    }

    private void OnFragileCargoMissionFailed()
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
                audioManager.PlayFragileCargoFailed();
            }

            HapticManager.PlayMediumImpact();
        }

        if (applyFailureCopy)
        {
            ApplyFailureCopy();
        }

        SetFragileCargoHudVisible(false);

        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(true);
        }
    }

    private void OnRestartClicked()
    {
        StopPulseAndResetScale();
        ApplyMovesColor(normalColor);
        failurePresentationPlayed = false;

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
            Debug.LogError("FragileCargoMissionUI: geen LevelManager voor RestartLevel.");
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
    /// Zelfde Menu-doel als NoTouchMissionUI / MoveLimitMissionUI: MainMenu.
    /// </summary>
    private void OnMenuClicked()
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
            ? objectiveController.CargoMovesRemaining
            : 0;
        int total = objectiveController != null
            ? objectiveController.CargoMoveLimit
            : 0;
        RefreshFromController(remaining, total);
    }

    private void RefreshFromController(int remaining, int total)
    {
        bool fragileCargo = objectiveController != null &&
                            objectiveController.IsFragileCargoLevel;

        if (!fragileCargo)
        {
            if (missionFailedPanel != null)
            {
                missionFailedPanel.SetActive(false);
            }

            StopPulseAndResetScale();
            ApplyMovesColor(normalColor);
            failurePresentationPlayed = false;
            SetFragileCargoHudVisible(false);
            return;
        }

        // Nieuw/restart FragileCargo: failure panel dicht tenzij Failed.
        if (objectiveController.State != LevelObjectiveController.RuntimeState.Failed &&
            missionFailedPanel != null &&
            missionFailedPanel.activeSelf)
        {
            missionFailedPanel.SetActive(false);
        }

        // Running na restart: failure feedback opnieuw toestaan.
        if (objectiveController.State == LevelObjectiveController.RuntimeState.Running)
        {
            failurePresentationPlayed = false;
        }

        // Win: HUD uit, failure panel blijft uit. Geen failure visuals.
        if (objectiveController.State == LevelObjectiveController.RuntimeState.Completed)
        {
            if (missionFailedPanel != null)
            {
                missionFailedPanel.SetActive(false);
            }

            StopPulseAndResetScale();
            ApplyMovesColor(normalColor);
            SetFragileCargoHudVisible(false);
            return;
        }

        // Fail: HUD uit; panel wordt alleen via OnFragileCargoMissionFailed geopend.
        if (objectiveController.State == LevelObjectiveController.RuntimeState.Failed)
        {
            StopPulseAndResetScale();
            SetFragileCargoHudVisible(false);
            return;
        }

        SetFragileCargoHudVisible(true);

        if (applyMissionLabel && missionLabel != null)
        {
            missionLabel.text = fragileCargoMissionLabel;
        }

        if (movesText != null)
        {
            movesText.text = remaining.ToString();
        }

        ApplyUrgencyVisuals(remaining);
    }

    /// <summary>
    /// remaining==0 ≠ failure. Alleen pulse/scale stoppen; panel via failure-event.
    /// </summary>
    private void ApplyUrgencyVisuals(int remaining)
    {
        CacheMovesVisualsIfNeeded();

        if (movesText == null)
        {
            return;
        }

        if (objectiveController != null &&
            objectiveController.State == LevelObjectiveController.RuntimeState.Completed)
        {
            StopPulseAndResetScale();
            ApplyMovesColor(normalColor);
            return;
        }

        // Last-move exit of budget op: geen pulse, geen failure-forcening.
        if (remaining <= 0)
        {
            StopPulseAndResetScale();
            return;
        }

        if (remaining <= criticalMoves)
        {
            ApplyMovesColor(criticalColor);
            pulseActive = true;
            return;
        }

        StopPulseAndResetScale();

        if (remaining <= warningMoves)
        {
            ApplyMovesColor(warningColor);
            return;
        }

        ApplyMovesColor(normalColor);
    }

    private void CacheMovesVisualsIfNeeded()
    {
        if (visualsCached || movesText == null)
        {
            return;
        }

        originalMovesScale = movesText.rectTransform.localScale;
        originalMovesColor = movesText.color;
        visualsCached = true;
    }

    private void ApplyMovesColor(Color color)
    {
        if (movesText == null)
        {
            return;
        }

        movesText.color = color;
    }

    private void StopPulseAndResetScale()
    {
        pulseActive = false;

        if (movesText == null)
        {
            return;
        }

        CacheMovesVisualsIfNeeded();
        movesText.rectTransform.localScale = originalMovesScale;
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

    private void SetFragileCargoHudVisible(bool visible)
    {
        if (fragileCargoHudRoot != null)
        {
            fragileCargoHudRoot.SetActive(visible);
        }
    }

    private void CacheObjectiveSnapshot()
    {
        lastKnownFragileCargo = objectiveController != null &&
                                objectiveController.IsFragileCargoLevel;
        lastKnownState = objectiveController != null
            ? objectiveController.State
            : LevelObjectiveController.RuntimeState.Inactive;
    }
}

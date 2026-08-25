using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-bridge voor TimedAmbulance: timer-tekst + MissionFailedPanel.
/// Maakt geen GameObjects — alles Inspector-gekoppeld.
/// Urgency + timed SFX/haptics = alleen presentation.
/// </summary>
public class TimedMissionUI : MonoBehaviour
{
    [Header("Failure panel")]
    [SerializeField] private GameObject missionFailedPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button levelSelectButton;

    [Header("Timed HUD (optioneel)")]
    [SerializeField] private GameObject timedHudRoot;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI missionLabel;

    /// <summary>Bestaande MissionLabel RectTransform voor SpecialMissionIntro.</summary>
    public RectTransform MissionLabelRect =>
        missionLabel != null ? missionLabel.rectTransform : null;

    [Header("Refs")]
    [SerializeField] private LevelObjectiveController objectiveController;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private AudioManager audioManager;

    [SerializeField] private string timedMissionLabel = "AMBULANCE RESCUE";

    [Header("Timer Urgency")]
    [SerializeField] private float warningThreshold = 10f;
    [SerializeField] private float criticalThreshold = 5f;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = new Color(1f, 0.78f, 0.15f, 1f);
    [SerializeField] private Color criticalColor = new Color(1f, 0.28f, 0.22f, 1f);

    [SerializeField] private float criticalPulseSpeed = 2f;
    [SerializeField] private float criticalPulseScale = 1.08f;

    private Vector3 originalTimerScale = Vector3.one;
    private Color originalTimerColor = Color.white;
    private bool visualsCached;
    private bool pulseActive;

    // Countdown beep: CeilToInt-grenzen 3/2/1, één keer per seconde.
    private int lastCountdownCeil = -1;
    private bool beepPlayed3;
    private bool beepPlayed2;
    private bool beepPlayed1;
    private bool countdownAudioActive;
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

        CacheTimerVisualsIfNeeded();
        SetTimedHudVisible(false);
    }

    private void OnEnable()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(OnRetryClicked);
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        if (levelSelectButton != null)
        {
            levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
            levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
        }

        if (objectiveController != null)
        {
            objectiveController.OnTimedMissionFailed += OnTimedMissionFailed;
            objectiveController.OnTimedMissionStarted += OnTimedMissionStarted;
            objectiveController.OnTimerChanged += OnTimerChanged;
            RefreshFromController();
        }
    }

    private void OnDisable()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(OnRetryClicked);
        }

        if (levelSelectButton != null)
        {
            levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
        }

        if (objectiveController != null)
        {
            objectiveController.OnTimedMissionFailed -= OnTimedMissionFailed;
            objectiveController.OnTimedMissionStarted -= OnTimedMissionStarted;
            objectiveController.OnTimerChanged -= OnTimerChanged;
        }

        StopPulseAndResetScale();
        StopCountdownAudio();
    }

    private void Update()
    {
        if (!pulseActive || timerText == null)
        {
            return;
        }

        // Win/fail/classic: stop zonder gameplay te raken.
        if (objectiveController == null ||
            !objectiveController.IsTimedLevel ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Completed ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Failed ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Inactive)
        {
            StopPulseAndResetScale();
            StopCountdownAudio();
            return;
        }

        // Pause: freeze mid-pulse (geen drift; timer zelf staat al stil via timeScale).
        if (Time.timeScale <= 0f)
        {
            return;
        }

        float wave = (Mathf.Sin(Time.unscaledTime * criticalPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        float scaleMul = Mathf.Lerp(1f, criticalPulseScale, wave);
        timerText.rectTransform.localScale = originalTimerScale * scaleMul;
    }

    private void OnTimedMissionStarted()
    {
        ResetCountdownAudioState();
        failurePresentationPlayed = false;
        countdownAudioActive = true;

        // Ambulance mission-start SFX speelt bij Special Mission Intro (niet hier),
        // zodat timer-start en audio ontkoppeld blijven.

        // Seed countdown vanaf huidige remaining (bij limiet ≤ 3 meteen juiste beep).
        if (objectiveController != null)
        {
            ProcessCountdownBeeps(objectiveController.RemainingTime);
        }
    }

    private void OnTimedMissionFailed()
    {
        StopPulseAndResetScale();
        StopCountdownAudio();

        if (!failurePresentationPlayed)
        {
            failurePresentationPlayed = true;

            if (audioManager == null)
            {
                audioManager = FindAnyObjectByType<AudioManager>();
            }

            if (audioManager != null)
            {
                audioManager.PlayTimedMissionFailed();
            }

            HapticManager.PlayMediumImpact();
        }

        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(true);
        }
    }

    private void OnRetryClicked()
    {
        StopPulseAndResetScale();
        ApplyTimerColor(normalColor);
        ResetCountdownAudioState();
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
            Debug.LogError("TimedMissionUI: geen LevelManager voor RestartLevel.");
        }
    }

    private void OnLevelSelectClicked()
    {
        StopPulseAndResetScale();
        StopCountdownAudio();

        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        SceneTransition.LoadScene("LevelSelect");
    }

    private void OnTimerChanged(float remainingSeconds)
    {
        RefreshFromController(remainingSeconds);
    }

    private void RefreshFromController()
    {
        float remaining = objectiveController != null
            ? objectiveController.RemainingTime
            : 0f;
        RefreshFromController(remaining);
    }

    private void RefreshFromController(float remainingSeconds)
    {
        bool timed = objectiveController != null && objectiveController.IsTimedLevel;

        if (!timed)
        {
            if (missionFailedPanel != null)
            {
                missionFailedPanel.SetActive(false);
            }

            StopPulseAndResetScale();
            ApplyTimerColor(normalColor);
            StopCountdownAudio();
            failurePresentationPlayed = false;
            SetTimedHudVisible(false);
            return;
        }

        // Nieuw timed level / restart: failure panel dicht.
        if (objectiveController.State != LevelObjectiveController.RuntimeState.Failed &&
            missionFailedPanel != null &&
            missionFailedPanel.activeSelf)
        {
            missionFailedPanel.SetActive(false);
        }

        // WaitingToStart / Inactive: nog geen countdown; reset state voor herstart.
        if (objectiveController.State == LevelObjectiveController.RuntimeState.WaitingToStart ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Inactive)
        {
            ResetCountdownAudioState();
            failurePresentationPlayed = false;
        }

        if (objectiveController.State == LevelObjectiveController.RuntimeState.Completed)
        {
            StopCountdownAudio();
        }

        SetTimedHudVisible(true);

        if (missionLabel != null)
        {
            missionLabel.text = timedMissionLabel;
        }

        if (timerText != null)
        {
            timerText.text = FormatTime(remainingSeconds);
        }

        ApplyUrgencyVisuals(remainingSeconds);
        ProcessCountdownBeeps(remainingSeconds);
    }

    private void ProcessCountdownBeeps(float remainingSeconds)
    {
        if (!countdownAudioActive ||
            objectiveController == null ||
            objectiveController.State != LevelObjectiveController.RuntimeState.Running)
        {
            return;
        }

        if (remainingSeconds <= 0f)
        {
            lastCountdownCeil = 0;
            return;
        }

        int ceil = Mathf.CeilToInt(remainingSeconds);

        if (lastCountdownCeil < 0)
        {
            TryPlayCountdownBeep(ceil);
            lastCountdownCeil = ceil;
            return;
        }

        if (ceil < lastCountdownCeil)
        {
            // Catch-up bij frame skip: speel gemiste 3/2/1 in volgorde.
            for (int second = lastCountdownCeil - 1; second >= ceil; second--)
            {
                TryPlayCountdownBeep(second);
            }
        }

        lastCountdownCeil = ceil;
    }

    private void TryPlayCountdownBeep(int displaySecond)
    {
        if (displaySecond < 1 || displaySecond > 3)
        {
            return;
        }

        if (displaySecond == 3 && beepPlayed3)
        {
            return;
        }

        if (displaySecond == 2 && beepPlayed2)
        {
            return;
        }

        if (displaySecond == 1 && beepPlayed1)
        {
            return;
        }

        if (displaySecond == 3)
        {
            beepPlayed3 = true;
        }
        else if (displaySecond == 2)
        {
            beepPlayed2 = true;
        }
        else
        {
            beepPlayed1 = true;
        }

        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }

        if (audioManager == null)
        {
            return;
        }

        float pitch = displaySecond == 3 ? 1.00f
            : displaySecond == 2 ? 1.05f
            : 1.10f;

        audioManager.PlayCountdownBeep(pitch);
    }

    private void ResetCountdownAudioState()
    {
        lastCountdownCeil = -1;
        beepPlayed3 = false;
        beepPlayed2 = false;
        beepPlayed1 = false;
        countdownAudioActive = false;
    }

    private void StopCountdownAudio()
    {
        countdownAudioActive = false;
    }

    private void ApplyUrgencyVisuals(float remainingSeconds)
    {
        CacheTimerVisualsIfNeeded();

        if (timerText == null)
        {
            return;
        }

        // Win: stop urgency visuals (OnTimerChanged komt niet altijd na Complete).
        if (objectiveController != null &&
            objectiveController.State == LevelObjectiveController.RuntimeState.Completed)
        {
            StopPulseAndResetScale();
            ApplyTimerColor(normalColor);
            return;
        }

        if (remainingSeconds <= 0f)
        {
            StopPulseAndResetScale();
            ApplyTimerColor(criticalColor);
            return;
        }

        if (remainingSeconds <= criticalThreshold)
        {
            ApplyTimerColor(criticalColor);
            pulseActive = true;
            return;
        }

        StopPulseAndResetScale();

        if (remainingSeconds <= warningThreshold)
        {
            ApplyTimerColor(warningColor);
            return;
        }

        ApplyTimerColor(normalColor);
    }

    private void CacheTimerVisualsIfNeeded()
    {
        if (visualsCached || timerText == null)
        {
            return;
        }

        originalTimerScale = timerText.rectTransform.localScale;
        originalTimerColor = timerText.color;
        visualsCached = true;
    }

    private void ApplyTimerColor(Color color)
    {
        if (timerText == null)
        {
            return;
        }

        timerText.color = color;
    }

    private void StopPulseAndResetScale()
    {
        pulseActive = false;

        if (timerText == null)
        {
            return;
        }

        CacheTimerVisualsIfNeeded();
        timerText.rectTransform.localScale = originalTimerScale;
    }

    private void SetTimedHudVisible(bool visible)
    {
        if (timedHudRoot != null)
        {
            timedHudRoot.SetActive(visible);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (visible)
            {
                GameplayLayoutController layout =
                    FindAnyObjectByType<GameplayLayoutController>();
                layout?.LogTimedLayoutDiagnostics("TimedMissionUI.Activated");
            }
#endif
            return;
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(visible);
        }

        if (missionLabel != null)
        {
            missionLabel.gameObject.SetActive(visible);
        }
    }

    private static string FormatTime(float seconds)
    {
        int whole = Mathf.Max(0, Mathf.CeilToInt(seconds));
        int minutes = whole / 60;
        int secs = whole % 60;
        return minutes.ToString() + ":" + secs.ToString("00");
    }
}

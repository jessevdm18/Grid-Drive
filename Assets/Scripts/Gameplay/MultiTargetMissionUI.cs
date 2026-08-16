using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// UI-bridge voor MultiTargetRescue: remaining/total HUD + rescue presentation.
/// Maakt geen GameObjects — alles Inspector-gekoppeld. Alleen presentation.
/// </summary>
public class MultiTargetMissionUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LevelObjectiveController objectiveController;
    [SerializeField] private AudioManager audioManager;

    [Header("Multi Target HUD")]
    [SerializeField] private GameObject multiTargetHudRoot;
    [SerializeField] private TextMeshProUGUI missionLabel;
    [SerializeField] private TextMeshProUGUI targetsText;

    [Tooltip("Uit = MissionLabel-tekst die jij handmatig zette blijft staan.")]
    [SerializeField] private bool applyMissionLabel = true;

    [SerializeField] private string multiTargetMissionLabel = "RESCUE";

    [Header("Rescue Presentation")]
    [SerializeField, Range(1.01f, 1.3f)] private float rescuePopScale = 1.12f;
    [SerializeField, Range(0.1f, 0.5f)] private float rescuePopDuration = 0.25f;
    [SerializeField] private Color rescueFlashColor = new Color(0.45f, 1f, 0.75f, 1f);

    private Vector3 originalTargetsScale = Vector3.one;
    private Color originalTargetsColor = Color.white;
    private bool targetsVisualsCached;
    private Coroutine popCoroutine;

    private void Awake()
    {
        if (objectiveController == null)
        {
            objectiveController = FindAnyObjectByType<LevelObjectiveController>();
        }

        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }

        CacheTargetsVisualsIfNeeded();
        SetMultiTargetHudVisible(false);
    }

    private void OnEnable()
    {
        if (objectiveController != null)
        {
            objectiveController.OnTargetsRemainingChanged += OnTargetsRemainingChanged;
            objectiveController.OnTargetRescued += OnTargetRescued;
            RefreshFromController();
        }
    }

    private void OnDisable()
    {
        if (objectiveController != null)
        {
            objectiveController.OnTargetsRemainingChanged -= OnTargetsRemainingChanged;
            objectiveController.OnTargetRescued -= OnTargetRescued;
        }

        StopPopAndResetVisuals();
    }

    private void OnTargetsRemainingChanged(int remaining, int total)
    {
        RefreshFromController(remaining, total);
    }

    /// <summary>
    /// Remaining is al bijgewerkt vóór dit event. Tekst eerst syncen, dan feedback.
    /// </summary>
    private void OnTargetRescued()
    {
        if (objectiveController == null ||
            !objectiveController.IsMultiTargetRescueLevel)
        {
            return;
        }

        int remaining = objectiveController.TargetsRemaining;
        int total = objectiveController.TargetsTotal;

        // Nieuwe remaining zichtbaar vóór pop.
        RefreshFromController(remaining, total);

        // Laatste rescue: Win-flow neemt over — geen stacking.
        if (remaining <= 0)
        {
            return;
        }

        PlayNonFinalRescueFeedback();
    }

    private void PlayNonFinalRescueFeedback()
    {
        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }

        if (audioManager != null)
        {
            audioManager.PlayMultiTargetRescue();
        }

        HapticManager.PlayLightImpact();
        StartTargetsPop();
    }

    private void RefreshFromController()
    {
        int remaining = objectiveController != null
            ? objectiveController.TargetsRemaining
            : 0;
        int total = objectiveController != null
            ? objectiveController.TargetsTotal
            : 0;
        RefreshFromController(remaining, total);
    }

    private void RefreshFromController(int remaining, int total)
    {
        bool multiTarget = objectiveController != null &&
                           objectiveController.IsMultiTargetRescueLevel;

        if (!multiTarget)
        {
            StopPopAndResetVisuals();
            SetMultiTargetHudVisible(false);
            return;
        }

        // Restart / nieuwe run: stop stale pop.
        if (objectiveController.State == LevelObjectiveController.RuntimeState.Running &&
            remaining == total &&
            total > 0)
        {
            StopPopAndResetVisuals();
        }

        SetMultiTargetHudVisible(true);

        if (applyMissionLabel && missionLabel != null)
        {
            missionLabel.text = multiTargetMissionLabel;
        }

        if (targetsText != null)
        {
            targetsText.text = remaining.ToString() + " / " + total.ToString();
        }
    }

    private void StartTargetsPop()
    {
        if (targetsText == null)
        {
            return;
        }

        CacheTargetsVisualsIfNeeded();
        StopPopAndResetVisuals();
        popCoroutine = StartCoroutine(AnimateTargetsPop());
    }

    private IEnumerator AnimateTargetsPop()
    {
        CacheTargetsVisualsIfNeeded();

        float duration = Mathf.Max(0.05f, rescuePopDuration);
        float peakTime = duration * 0.32f;
        float peakScale = Mathf.Max(1.01f, rescuePopScale);

        // Naar peak (scale + flash).
        float elapsed = 0f;
        while (elapsed < peakTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / peakTime);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            ApplyTargetsVisual(
                Vector3.LerpUnclamped(originalTargetsScale, originalTargetsScale * peakScale, eased),
                Color.LerpUnclamped(originalTargetsColor, rescueFlashColor, eased)
            );
            yield return null;
        }

        ApplyTargetsVisual(originalTargetsScale * peakScale, rescueFlashColor);

        // Terug naar original.
        float backDuration = Mathf.Max(0.01f, duration - peakTime);
        elapsed = 0f;
        while (elapsed < backDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / backDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            ApplyTargetsVisual(
                Vector3.LerpUnclamped(originalTargetsScale * peakScale, originalTargetsScale, eased),
                Color.LerpUnclamped(rescueFlashColor, originalTargetsColor, eased)
            );
            yield return null;
        }

        ApplyTargetsVisual(originalTargetsScale, originalTargetsColor);
        popCoroutine = null;
    }

    private void ApplyTargetsVisual(Vector3 scale, Color color)
    {
        if (targetsText == null)
        {
            return;
        }

        targetsText.rectTransform.localScale = scale;
        targetsText.color = color;
    }

    private void CacheTargetsVisualsIfNeeded()
    {
        if (targetsVisualsCached || targetsText == null)
        {
            return;
        }

        originalTargetsScale = targetsText.rectTransform.localScale;
        originalTargetsColor = targetsText.color;
        targetsVisualsCached = true;
    }

    private void StopPopAndResetVisuals()
    {
        if (popCoroutine != null)
        {
            StopCoroutine(popCoroutine);
            popCoroutine = null;
        }

        if (targetsText == null)
        {
            return;
        }

        CacheTargetsVisualsIfNeeded();
        targetsText.rectTransform.localScale = originalTargetsScale;
        targetsText.color = originalTargetsColor;
    }

    private void SetMultiTargetHudVisible(bool visible)
    {
        if (multiTargetHudRoot != null)
        {
            multiTargetHudRoot.SetActive(visible);
        }
    }
}

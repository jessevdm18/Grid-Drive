using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-bridge voor LimitedVehicle: HUD + world-space marker (cijfer / lock-icoon).
/// Maakt geen GameObjects — alles Inspector-gekoppeld. Alleen presentation.
/// Geen failure panel, audio of haptics in v1.
/// </summary>
public class LimitedVehicleMissionUI : MonoBehaviour
{
    private const string LockedDisplayText = "LOCKED";

    [Header("Refs")]
    [SerializeField] private LevelObjectiveController objectiveController;
    [SerializeField] private AudioManager audioManager;

    [Header("Limited Vehicle HUD")]
    [SerializeField] private GameObject limitedVehicleHudRoot;
    [SerializeField] private TextMeshProUGUI missionLabel;

    /// <summary>Bestaande MissionLabel RectTransform voor SpecialMissionIntro.</summary>
    public RectTransform MissionLabelRect =>
        missionLabel != null ? missionLabel.rectTransform : null;
    [SerializeField] private TextMeshProUGUI movesText;

    [Tooltip("Uit = MissionLabel-tekst die jij handmatig zette blijft staan.")]
    [SerializeField] private bool applyMissionLabel = false;

    [SerializeField] private string missionLabelText = "LIMITED VEHICLE";

    [Header("Limited Vehicle Marker (world-space)")]
    [Tooltip("Serialized marker in de scene. Geen runtime create.")]
    [SerializeField] private GameObject limitedVehicleMarker;

    [Tooltip("Optioneel. Leeg = Transform van Limited Vehicle Marker.")]
    [SerializeField] private Transform markerTransform;

    [Tooltip("World TMP (of UI TMP) die alleen het remaining-cijfer toont.")]
    [SerializeField] private TMP_Text markerRemainingText;

    [Tooltip("Lock-icoon child. Active alleen wanneer locked.")]
    [SerializeField] private GameObject markerLockIcon;

    [Tooltip("Optioneel. Leeg = SpriteRenderer op Marker Lock Icon.")]
    [SerializeField] private SpriteRenderer markerLockRenderer;

    [Tooltip("World-space offset t.o.v. limited vehicle (bijv. Y = 0.6).")]
    [SerializeField] private Vector3 markerWorldOffset = new Vector3(0f, 0.6f, 0f);

    [Tooltip("Optioneel: sortingOrder = vehicle sprite + offset.")]
    [SerializeField] private bool matchVehicleSorting = true;

    [SerializeField] private int sortingOrderOffset = 10;

    [Header("Limited Vehicle Marker Polish")]
    [SerializeField] private Color normalNumberColor = Color.white;
    [SerializeField] private Color warningNumberColor = new Color(1f, 0.28f, 0.22f, 1f);

    [SerializeField, Range(0.1f, 8f)] private float warningPulseSpeed = 2f;
    [SerializeField, Range(1f, 1.25f)] private float warningPulseScale = 1.08f;

    [SerializeField, Range(1f, 1.5f)] private float lockPopScale = 1.18f;
    [SerializeField, Range(0.05f, 0.6f)] private float lockPopDuration = 0.22f;

    [Header("Limited Vehicle Pulse")]
    [SerializeField] private bool limitedVehiclePulseEnabled = true;

    [Tooltip("Warm amber / geel-oranje tint. Lerp met originele skin-kleur.")]
    [SerializeField] private Color limitedVehiclePulseColor =
        new Color(1f, 0.71f, 0.18f, 1f);

    [SerializeField, Range(0.1f, 8f)] private float limitedVehiclePulseSpeed = 1.25f;

    [SerializeField, Range(0f, 1f)] private float limitedVehiclePulseStrength = 0.30f;

    private bool lastKnownLimitedVehicle;
    private LevelObjectiveController.RuntimeState lastKnownState =
        LevelObjectiveController.RuntimeState.Inactive;
    private bool lastKnownLocked;

    private Transform resolvedMarkerTransform;
    private SpriteRenderer resolvedLockRenderer;
    private Renderer markerRemainingRenderer;
    private Transform lockIconTransform;
    private bool markerVisible;
    private VehicleController lastBoundLimitedVehicle;

    private Vector3 originalRemainingScale = Vector3.one;
    private Color originalRemainingColor = Color.white;
    private bool remainingVisualsCached;
    private bool remainingPulseActive;

    private Vector3 originalLockScale = Vector3.one;
    private bool lockVisualsCached;
    private Coroutine lockPopCoroutine;
    private bool lockPresentationPlayed;

    private SpriteRenderer limitedVehicleRenderer;
    private Color originalLimitedVehicleColor = Color.white;
    private bool limitedVehicleColorCached;
    private bool limitedVehiclePulseActive;

    public VehicleController LimitedVehicle =>
        objectiveController != null ? objectiveController.LimitedVehicle : null;

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

        ResolveMarkerRefs();
        DisableMarkerInputBlocking();
        CacheRemainingVisualsIfNeeded();
        CacheLockVisualsIfNeeded();
        SetLimitedVehicleHudVisible(false);
        SetMarkerVisible(false);
        lastKnownLimitedVehicle = false;
        lastKnownLocked = false;
        lastKnownState = LevelObjectiveController.RuntimeState.Inactive;
    }

    private void OnEnable()
    {
        if (objectiveController != null)
        {
            objectiveController.OnLimitedVehicleMovesRemainingChanged +=
                OnLimitedVehicleMovesRemainingChanged;
            objectiveController.OnLimitedVehicleLocked += OnLimitedVehicleLocked;
            objectiveController.OnLimitedVehicleUnlocked += OnLimitedVehicleUnlocked;
            RefreshFromController();
            CacheObjectiveSnapshot();
        }
    }

    private void OnDisable()
    {
        if (objectiveController != null)
        {
            objectiveController.OnLimitedVehicleMovesRemainingChanged -=
                OnLimitedVehicleMovesRemainingChanged;
            objectiveController.OnLimitedVehicleLocked -= OnLimitedVehicleLocked;
            objectiveController.OnLimitedVehicleUnlocked -= OnLimitedVehicleUnlocked;
        }

        StopRemainingPulseAndResetScale();
        StopLockPopAndResetScale();
        RestoreLimitedVehicleColor();
    }

    private void Update()
    {
        if (!remainingPulseActive || markerRemainingText == null)
        {
            return;
        }

        if (objectiveController == null ||
            !objectiveController.IsLimitedVehicleLevel ||
            objectiveController.IsLimitedVehicleLocked ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Completed ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Inactive)
        {
            StopRemainingPulseAndResetScale();
            return;
        }

        if (Time.timeScale <= 0f)
        {
            return;
        }

        float wave = (Mathf.Sin(Time.unscaledTime * warningPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        float scaleMul = Mathf.Lerp(1f, warningPulseScale, wave);
        markerRemainingText.transform.localScale = originalRemainingScale * scaleMul;
    }

    /// <summary>
    /// HUD sync bij state-change + marker follow (presentation only).
    /// </summary>
    private void LateUpdate()
    {
        if (objectiveController == null)
        {
            return;
        }

        bool limited = objectiveController.IsLimitedVehicleLevel;
        bool locked = objectiveController.IsLimitedVehicleLocked;
        LevelObjectiveController.RuntimeState state = objectiveController.State;

        if (limited != lastKnownLimitedVehicle ||
            locked != lastKnownLocked ||
            state != lastKnownState)
        {
            CacheObjectiveSnapshot();
            RefreshFromController();
        }

        UpdateMarkerFollow();
        UpdateLimitedVehiclePulse();
    }

    private void OnLimitedVehicleMovesRemainingChanged(int remaining, int total)
    {
        RefreshFromController(remaining, total);
    }

    private void OnLimitedVehicleLocked()
    {
        StopRemainingPulseAndResetScale();
        RestoreLimitedVehicleColor();
        ApplyHudMovesDisplay(locked: true, remaining: 0);
        // Event-transition: éénmalige lock-pop.
        ApplyMarkerContent(locked: true, remaining: 0, playLockPop: true);
        SetLimitedVehicleHudVisible(ShouldShowHud());
        SetMarkerVisible(ShouldShowMarker());

        if (!lockPresentationPlayed)
        {
            lockPresentationPlayed = true;

            if (audioManager == null)
            {
                audioManager = FindAnyObjectByType<AudioManager>();
            }

            if (audioManager != null)
            {
                audioManager.PlayLimitedVehicleLocked();
            }

            HapticManager.PlayLightImpact();
        }
    }

    private void OnLimitedVehicleUnlocked()
    {
        lockPresentationPlayed = false;
        RefreshFromController();
        CacheObjectiveSnapshot();
    }

    private void RefreshFromController()
    {
        int remaining = objectiveController != null
            ? objectiveController.LimitedVehicleMovesRemaining
            : 0;
        int total = objectiveController != null
            ? objectiveController.LimitedVehicleMoveLimit
            : 0;
        RefreshFromController(remaining, total);
    }

    private void RefreshFromController(int remaining, int total)
    {
        bool limited = objectiveController != null &&
                       objectiveController.IsLimitedVehicleLevel;

        if (!limited)
        {
            StopRemainingPulseAndResetScale();
            StopLockPopAndResetScale();
            RestoreLimitedVehicleColor();
            lockPresentationPlayed = false;
            SetLimitedVehicleHudVisible(false);
            SetMarkerVisible(false);
            return;
        }

        if (objectiveController.State == LevelObjectiveController.RuntimeState.Completed)
        {
            StopRemainingPulseAndResetScale();
            StopLockPopAndResetScale();
            RestoreLimitedVehicleColor();
            SetLimitedVehicleHudVisible(false);
            SetMarkerVisible(false);
            return;
        }

        // Restart / unlocked run: lock feedback opnieuw toestaan.
        if (!objectiveController.IsLimitedVehicleLocked)
        {
            lockPresentationPlayed = false;
        }

        SetLimitedVehicleHudVisible(true);

        if (applyMissionLabel && missionLabel != null)
        {
            missionLabel.text = missionLabelText;
        }

        bool locked = objectiveController.IsLimitedVehicleLocked;
        ApplyHudMovesDisplay(locked, remaining);

        VehicleController vehicle = objectiveController.LimitedVehicle;
        SetMarkerVisible(vehicle != null);
        if (vehicle != null)
        {
            BindLimitedVehiclePresentation(vehicle);
            // Refresh terwijl al locked: lock zichtbaar zonder pop-replay.
            ApplyMarkerContent(locked, remaining, playLockPop: false);
            SyncLimitedVehiclePulseActive(locked, remaining);
        }
        else
        {
            RestoreLimitedVehicleColor();
        }
    }

    private void ApplyHudMovesDisplay(bool locked, int remaining)
    {
        if (movesText == null)
        {
            return;
        }

        if (locked)
        {
            movesText.text = LockedDisplayText;
            movesText.color = normalNumberColor;
            return;
        }

        movesText.text = remaining.ToString();
        // Eenvoudige HUD-kleur sync; pulse blijft world-space RemainingText.
        movesText.color = remaining == 1 ? warningNumberColor : normalNumberColor;
    }

    private void ApplyMarkerContent(bool locked, int remaining, bool playLockPop)
    {
        if (locked)
        {
            StopRemainingPulseAndResetScale();
            limitedVehiclePulseActive = false;
            SetMarkerRemainingVisible(false);
            SetMarkerLockVisible(true);

            if (playLockPop)
            {
                StartLockPop();
            }
            else
            {
                StopLockPopAndResetScale();
            }

            return;
        }

        StopLockPopAndResetScale();
        SetMarkerLockVisible(false);
        SetMarkerRemainingVisible(true);

        if (markerRemainingText != null)
        {
            markerRemainingText.text = remaining.ToString();
        }

        if (remaining == 1)
        {
            ApplyRemainingColor(warningNumberColor);
            remainingPulseActive = true;
            return;
        }

        StopRemainingPulseAndResetScale();
        ApplyRemainingColor(normalNumberColor);
    }

    private void SyncLimitedVehiclePulseActive(bool locked, int remaining)
    {
        if (!limitedVehiclePulseEnabled || locked || remaining <= 0)
        {
            limitedVehiclePulseActive = false;

            // Locked / exhausted: snap tint back (no lingering amber).
            if (locked || remaining <= 0)
            {
                RestoreLimitedVehicleColor();
            }

            return;
        }

        limitedVehiclePulseActive = limitedVehicleColorCached;
    }

    private void BindLimitedVehiclePresentation(VehicleController vehicle)
    {
        if (vehicle == null)
        {
            RestoreLimitedVehicleColor();
            lastBoundLimitedVehicle = null;
            return;
        }

        if (vehicle == lastBoundLimitedVehicle &&
            limitedVehicleRenderer != null &&
            limitedVehicleColorCached)
        {
            BindMarkerSorting(vehicle);
            return;
        }

        RestoreLimitedVehicleColor();
        lastBoundLimitedVehicle = vehicle;

        limitedVehicleRenderer = vehicle.VisualSpriteRenderer;
        if (limitedVehicleRenderer == null)
        {
            limitedVehicleRenderer =
                vehicle.GetComponentInChildren<SpriteRenderer>(true);
        }

        limitedVehicleColorCached = false;
        if (limitedVehicleRenderer != null)
        {
            originalLimitedVehicleColor = limitedVehicleRenderer.color;
            limitedVehicleColorCached = true;
        }

        BindMarkerSorting(vehicle);
    }

    private void BindMarkerSorting(VehicleController vehicle)
    {
        if (!matchVehicleSorting || vehicle == null)
        {
            return;
        }

        SpriteRenderer vehicleSprite = vehicle.VisualSpriteRenderer;
        if (vehicleSprite == null)
        {
            vehicleSprite = limitedVehicleRenderer;
        }

        if (vehicleSprite == null)
        {
            return;
        }

        int order = vehicleSprite.sortingOrder + sortingOrderOffset;
        int layerId = vehicleSprite.sortingLayerID;

        if (resolvedLockRenderer != null)
        {
            resolvedLockRenderer.sortingLayerID = layerId;
            resolvedLockRenderer.sortingOrder = order;
        }

        if (markerRemainingRenderer != null)
        {
            markerRemainingRenderer.sortingLayerID = layerId;
            markerRemainingRenderer.sortingOrder = order;
        }
    }

    private void UpdateLimitedVehiclePulse()
    {
        if (!limitedVehiclePulseActive ||
            !limitedVehiclePulseEnabled ||
            limitedVehicleRenderer == null ||
            !limitedVehicleColorCached)
        {
            return;
        }

        if (objectiveController == null ||
            !objectiveController.IsLimitedVehicleLevel ||
            objectiveController.IsLimitedVehicleLocked ||
            objectiveController.LimitedVehicleMovesRemaining <= 0 ||
            objectiveController.State != LevelObjectiveController.RuntimeState.Running)
        {
            RestoreLimitedVehicleColor();
            return;
        }

        // Pause: freeze mid-pulse.
        if (Time.timeScale <= 0f)
        {
            return;
        }

        float wave =
            (Mathf.Sin(Time.unscaledTime * limitedVehiclePulseSpeed * Mathf.PI * 2f) + 1f) *
            0.5f;
        float t = wave * Mathf.Clamp01(limitedVehiclePulseStrength);

        Color pulsed = Color.Lerp(
            originalLimitedVehicleColor,
            limitedVehiclePulseColor,
            t
        );
        pulsed.a = originalLimitedVehicleColor.a;
        limitedVehicleRenderer.color = pulsed;
    }

    private void RestoreLimitedVehicleColor()
    {
        limitedVehiclePulseActive = false;

        if (limitedVehicleRenderer != null && limitedVehicleColorCached)
        {
            limitedVehicleRenderer.color = originalLimitedVehicleColor;
        }

        limitedVehicleRenderer = null;
        limitedVehicleColorCached = false;
    }

    private void StartLockPop()
    {
        StopLockPopAndResetScale();

        if (lockIconTransform == null)
        {
            return;
        }

        CacheLockVisualsIfNeeded();
        lockPopCoroutine = StartCoroutine(AnimateLockPop());
    }

    private IEnumerator AnimateLockPop()
    {
        CacheLockVisualsIfNeeded();

        if (lockIconTransform == null)
        {
            lockPopCoroutine = null;
            yield break;
        }

        float duration = Mathf.Max(0.05f, lockPopDuration);
        float peakTime = Mathf.Min(0.08f, duration * 0.4f);
        float peakScale = Mathf.Max(1.01f, lockPopScale);

        float elapsed = 0f;
        while (elapsed < peakTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / peakTime);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            lockIconTransform.localScale = Vector3.LerpUnclamped(
                originalLockScale,
                originalLockScale * peakScale,
                eased
            );
            yield return null;
        }

        lockIconTransform.localScale = originalLockScale * peakScale;

        float backDuration = Mathf.Max(0.01f, duration - peakTime);
        elapsed = 0f;
        while (elapsed < backDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / backDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            lockIconTransform.localScale = Vector3.LerpUnclamped(
                originalLockScale * peakScale,
                originalLockScale,
                eased
            );
            yield return null;
        }

        lockIconTransform.localScale = originalLockScale;
        lockPopCoroutine = null;
    }

    private void UpdateMarkerFollow()
    {
        if (!markerVisible || resolvedMarkerTransform == null)
        {
            return;
        }

        if (objectiveController == null ||
            !objectiveController.IsLimitedVehicleLevel)
        {
            SetMarkerVisible(false);
            return;
        }

        if (objectiveController.State == LevelObjectiveController.RuntimeState.Completed ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Inactive)
        {
            SetMarkerVisible(false);
            return;
        }

        VehicleController vehicle = objectiveController.LimitedVehicle;
        if (vehicle == null)
        {
            SetMarkerVisible(false);
            return;
        }

        if (vehicle != lastBoundLimitedVehicle)
        {
            BindMarkerToVehicle(vehicle);
        }

        resolvedMarkerTransform.position =
            vehicle.transform.position + markerWorldOffset;
    }

    private void BindMarkerToVehicle(VehicleController vehicle)
    {
        BindLimitedVehiclePresentation(vehicle);
    }

    private void ResolveMarkerRefs()
    {
        resolvedMarkerTransform = markerTransform;
        if (resolvedMarkerTransform == null && limitedVehicleMarker != null)
        {
            resolvedMarkerTransform = limitedVehicleMarker.transform;
        }

        resolvedLockRenderer = markerLockRenderer;
        if (resolvedLockRenderer == null && markerLockIcon != null)
        {
            resolvedLockRenderer = markerLockIcon.GetComponent<SpriteRenderer>();
            if (resolvedLockRenderer == null)
            {
                resolvedLockRenderer =
                    markerLockIcon.GetComponentInChildren<SpriteRenderer>(true);
            }
        }

        lockIconTransform = markerLockIcon != null ? markerLockIcon.transform : null;

        markerRemainingRenderer = null;
        if (markerRemainingText != null)
        {
            markerRemainingRenderer = markerRemainingText.GetComponent<Renderer>();
        }
    }

    private void DisableMarkerInputBlocking()
    {
        if (limitedVehicleMarker == null)
        {
            return;
        }

        Graphic[] graphics = limitedVehicleMarker.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            if (graphics[i] != null)
            {
                graphics[i].raycastTarget = false;
            }
        }
    }

    private void CacheRemainingVisualsIfNeeded()
    {
        if (remainingVisualsCached || markerRemainingText == null)
        {
            return;
        }

        originalRemainingScale = markerRemainingText.transform.localScale;
        originalRemainingColor = markerRemainingText.color;
        remainingVisualsCached = true;
    }

    private void CacheLockVisualsIfNeeded()
    {
        if (lockVisualsCached || lockIconTransform == null)
        {
            return;
        }

        originalLockScale = lockIconTransform.localScale;
        lockVisualsCached = true;
    }

    private void ApplyRemainingColor(Color color)
    {
        if (markerRemainingText == null)
        {
            return;
        }

        markerRemainingText.color = color;
    }

    private void StopRemainingPulseAndResetScale()
    {
        remainingPulseActive = false;

        if (markerRemainingText == null)
        {
            return;
        }

        CacheRemainingVisualsIfNeeded();
        markerRemainingText.transform.localScale = originalRemainingScale;
    }

    private void StopLockPopAndResetScale()
    {
        if (lockPopCoroutine != null)
        {
            StopCoroutine(lockPopCoroutine);
            lockPopCoroutine = null;
        }

        if (lockIconTransform == null)
        {
            return;
        }

        CacheLockVisualsIfNeeded();
        lockIconTransform.localScale = originalLockScale;
    }

    private void SetMarkerRemainingVisible(bool visible)
    {
        if (markerRemainingText != null)
        {
            markerRemainingText.gameObject.SetActive(visible);
        }
    }

    private void SetMarkerLockVisible(bool visible)
    {
        if (markerLockIcon != null)
        {
            markerLockIcon.SetActive(visible);
        }
    }

    private void SetMarkerVisible(bool visible)
    {
        markerVisible = visible;

        if (!visible)
        {
            lastBoundLimitedVehicle = null;
            StopRemainingPulseAndResetScale();
            StopLockPopAndResetScale();
            RestoreLimitedVehicleColor();
        }

        if (limitedVehicleMarker != null)
        {
            limitedVehicleMarker.SetActive(visible);
            return;
        }

        if (resolvedMarkerTransform != null)
        {
            resolvedMarkerTransform.gameObject.SetActive(visible);
        }
    }

    private bool ShouldShowHud()
    {
        return objectiveController != null &&
               objectiveController.IsLimitedVehicleLevel &&
               objectiveController.State != LevelObjectiveController.RuntimeState.Completed;
    }

    private bool ShouldShowMarker()
    {
        return ShouldShowHud() &&
               objectiveController.LimitedVehicle != null;
    }

    private void SetLimitedVehicleHudVisible(bool visible)
    {
        if (limitedVehicleHudRoot != null)
        {
            limitedVehicleHudRoot.SetActive(visible);
        }
    }

    private void CacheObjectiveSnapshot()
    {
        lastKnownLimitedVehicle = objectiveController != null &&
                                  objectiveController.IsLimitedVehicleLevel;
        lastKnownLocked = objectiveController != null &&
                          objectiveController.IsLimitedVehicleLocked;
        lastKnownState = objectiveController != null
            ? objectiveController.State
            : LevelObjectiveController.RuntimeState.Inactive;
    }
}

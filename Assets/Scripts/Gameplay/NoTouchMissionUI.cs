using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-bridge voor NoTouchChallenge: HUD + MissionFailedNoTouchPanel + protected marker.
/// Maakt geen GameObjects — alles Inspector-gekoppeld. Alleen presentation.
/// Failure SFX/haptic via OnNoTouchMissionFailed (zelfde patroon als Timed/MoveLimit).
/// Marker = world-space (SpriteRenderer of los Transform), volgt ProtectedVehicle.
/// </summary>
public class NoTouchMissionUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LevelObjectiveController objectiveController;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private AudioManager audioManager;

    [Header("No Touch HUD")]
    [SerializeField] private GameObject noTouchHudRoot;
    [SerializeField] private TextMeshProUGUI missionLabel;

    /// <summary>Bestaande MissionLabel RectTransform voor SpecialMissionIntro.</summary>
    public RectTransform MissionLabelRect =>
        missionLabel != null ? missionLabel.rectTransform : null;
    [SerializeField] private TextMeshProUGUI ruleText;

    [Tooltip("Uit = MissionLabel-tekst die jij handmatig zette blijft staan.")]
    [SerializeField] private bool applyMissionLabel = false;

    [Tooltip("Uit = RuleText die jij handmatig zette blijft staan.")]
    [SerializeField] private bool applyRuleText = false;

    [SerializeField] private string noTouchMissionLabel = "DON'T MOVE";
    [SerializeField] private string noTouchRuleText = "PROTECTED VEHICLE";

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
        "YOU MOVED THE PROTECTED VEHICLE";

    [Header("Protected Vehicle Marker (world-space)")]
    [Tooltip("Serialized marker in de scene. Geen runtime create.")]
    [SerializeField] private GameObject protectedVehicleMarker;

    [Tooltip("Optioneel. Leeg = Transform van Protected Vehicle Marker.")]
    [SerializeField] private Transform markerTransform;

    [Tooltip("World-space offset t.o.v. protected vehicle (bijv. Y = 1).")]
    [SerializeField] private Vector3 markerWorldOffset = new Vector3(0f, 1f, 0f);

    [Tooltip("Optioneel: sortingOrder = vehicle sprite + offset (alleen SpriteRenderer).")]
    [SerializeField] private bool matchVehicleSorting = true;

    [SerializeField] private int sortingOrderOffset = 10;

    [Header("Marker Pulse (optioneel)")]
    [SerializeField] private bool pulseEnabled = true;

    [SerializeField, Range(0.1f, 8f)] private float pulseSpeed = 1.5f;

    [SerializeField, Range(0f, 0.25f)] private float pulseScaleAmount = 0.08f;

    [Header("Protected Vehicle Pulse")]
    [SerializeField] private bool protectedVehiclePulseEnabled = true;

    [Tooltip("Zachte rode tint. Lerp met originele skin-kleur.")]
    [SerializeField] private Color protectedVehiclePulseColor =
        new Color(1f, 0.28f, 0.22f, 1f);

    [SerializeField, Range(0.1f, 8f)] private float protectedVehiclePulseSpeed = 1.5f;

    [SerializeField, Range(0f, 1f)] private float protectedVehiclePulseStrength = 0.35f;

    private bool lastKnownNoTouch;
    private LevelObjectiveController.RuntimeState lastKnownState =
        LevelObjectiveController.RuntimeState.Inactive;

    private Transform resolvedMarkerTransform;
    private SpriteRenderer markerSpriteRenderer;
    private Vector3 markerBaseScale = Vector3.one;
    private bool markerScaleCached;
    private bool markerVisible;
    private VehicleController lastBoundProtectedVehicle;
    private bool failurePresentationPlayed;

    private SpriteRenderer protectedVehicleRenderer;
    private Color originalProtectedVehicleColor = Color.white;
    private bool protectedVehicleColorCached;
    private bool protectedVehiclePulseActive;

    /// <summary>
    /// Authoritative protected vehicle (voor marker / debug).
    /// </summary>
    public VehicleController ProtectedVehicle =>
        objectiveController != null ? objectiveController.ProtectedVehicle : null;

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

        ResolveMarkerRefs();
        DisableMarkerInputBlocking();
        SetNoTouchHudVisible(false);
        SetMarkerVisible(false);
        lastKnownNoTouch = false;
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
            objectiveController.OnNoTouchMissionFailed += OnNoTouchMissionFailed;
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
            objectiveController.OnNoTouchMissionFailed -= OnNoTouchMissionFailed;
        }

        ResetMarkerPulseScale();
        RestoreProtectedVehicleColor();
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

        bool noTouch = objectiveController.IsNoTouchChallengeLevel;
        LevelObjectiveController.RuntimeState state = objectiveController.State;

        if (noTouch != lastKnownNoTouch || state != lastKnownState)
        {
            CacheObjectiveSnapshot();
            RefreshFromController();
        }

        UpdateMarkerPresentation();
        UpdateProtectedVehiclePulse();
    }

    private void OnNoTouchMissionFailed()
    {
        if (!failurePresentationPlayed)
        {
            failurePresentationPlayed = true;

            if (audioManager == null)
            {
                audioManager = FindAnyObjectByType<AudioManager>();
            }

            if (audioManager != null)
            {
                audioManager.PlayNoTouchFailed();
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

        RestoreProtectedVehicleColor();
        SetMarkerVisible(false);
    }

    private void OnRestartClicked()
    {
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
            Debug.LogError("NoTouchMissionUI: geen LevelManager voor RestartLevel.");
        }
    }

    private void OnLevelSelectClicked()
    {
        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(false);
        }

        SetMarkerVisible(false);
        Time.timeScale = 1f;
        SceneTransition.LoadScene("LevelSelect");
    }

    /// <summary>
    /// Zelfde Menu/Back-doel als MoveLimitMissionUI / PauseManager: MainMenu.
    /// </summary>
    private void OnMenuClicked()
    {
        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(false);
        }

        SetMarkerVisible(false);
        Time.timeScale = 1f;
        SceneTransition.LoadScene("MainMenu");
    }

    private void RefreshFromController()
    {
        bool noTouch = objectiveController != null &&
                       objectiveController.IsNoTouchChallengeLevel;

        if (!noTouch)
        {
            if (missionFailedPanel != null)
            {
                missionFailedPanel.SetActive(false);
            }

            failurePresentationPlayed = false;
            RestoreProtectedVehicleColor();
            SetNoTouchHudVisible(false);
            SetMarkerVisible(false);
            return;
        }

        // Nieuw/restart NoTouch: failure panel dicht tenzij Failed.
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

        // Win: HUD + marker uit. Fail: marker uit (panel blijft via OnNoTouchMissionFailed).
        if (objectiveController.State == LevelObjectiveController.RuntimeState.Completed)
        {
            if (missionFailedPanel != null)
            {
                missionFailedPanel.SetActive(false);
            }

            RestoreProtectedVehicleColor();
            SetNoTouchHudVisible(false);
            SetMarkerVisible(false);
            return;
        }

        if (objectiveController.State == LevelObjectiveController.RuntimeState.Failed)
        {
            RestoreProtectedVehicleColor();
            SetMarkerVisible(false);
            return;
        }

        SetNoTouchHudVisible(true);

        if (applyMissionLabel && missionLabel != null)
        {
            missionLabel.text = noTouchMissionLabel;
        }

        if (applyRuleText && ruleText != null)
        {
            ruleText.text = noTouchRuleText;
        }

        // Running / Waiting: marker aan als protected vehicle beschikbaar is.
        VehicleController protectedVehicle = objectiveController.ProtectedVehicle;
        SetMarkerVisible(protectedVehicle != null);
        if (protectedVehicle != null)
        {
            BindProtectedVehiclePresentation(protectedVehicle);
            protectedVehiclePulseActive = protectedVehiclePulseEnabled;
        }
        else
        {
            RestoreProtectedVehicleColor();
        }
    }

    private void UpdateMarkerPresentation()
    {
        if (!markerVisible || resolvedMarkerTransform == null)
        {
            return;
        }

        if (objectiveController == null ||
            !objectiveController.IsNoTouchChallengeLevel)
        {
            SetMarkerVisible(false);
            return;
        }

        if (objectiveController.State == LevelObjectiveController.RuntimeState.Failed ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Completed ||
            objectiveController.State == LevelObjectiveController.RuntimeState.Inactive)
        {
            SetMarkerVisible(false);
            return;
        }

        VehicleController protectedVehicle = objectiveController.ProtectedVehicle;
        if (protectedVehicle == null)
        {
            SetMarkerVisible(false);
            return;
        }

        if (protectedVehicle != lastBoundProtectedVehicle)
        {
            BindProtectedVehiclePresentation(protectedVehicle);
        }

        resolvedMarkerTransform.position =
            protectedVehicle.transform.position + markerWorldOffset;

        UpdateMarkerPulse();
    }

    private void BindProtectedVehiclePresentation(VehicleController vehicle)
    {
        if (vehicle == null)
        {
            RestoreProtectedVehicleColor();
            lastBoundProtectedVehicle = null;
            return;
        }

        if (vehicle == lastBoundProtectedVehicle &&
            protectedVehicleRenderer != null &&
            protectedVehicleColorCached)
        {
            BindMarkerSorting(vehicle);
            return;
        }

        RestoreProtectedVehicleColor();
        lastBoundProtectedVehicle = vehicle;

        protectedVehicleRenderer = vehicle.VisualSpriteRenderer;
        if (protectedVehicleRenderer == null)
        {
            protectedVehicleRenderer =
                vehicle.GetComponentInChildren<SpriteRenderer>(true);
        }

        protectedVehicleColorCached = false;
        if (protectedVehicleRenderer != null)
        {
            originalProtectedVehicleColor = protectedVehicleRenderer.color;
            protectedVehicleColorCached = true;
        }

        BindMarkerSorting(vehicle);
    }

    private void BindMarkerSorting(VehicleController vehicle)
    {
        if (!matchVehicleSorting || markerSpriteRenderer == null || vehicle == null)
        {
            return;
        }

        SpriteRenderer vehicleSprite = vehicle.VisualSpriteRenderer;
        if (vehicleSprite == null)
        {
            vehicleSprite = protectedVehicleRenderer;
        }

        if (vehicleSprite == null)
        {
            return;
        }

        markerSpriteRenderer.sortingLayerID = vehicleSprite.sortingLayerID;
        markerSpriteRenderer.sortingOrder =
            vehicleSprite.sortingOrder + sortingOrderOffset;
    }

    private void UpdateProtectedVehiclePulse()
    {
        if (!protectedVehiclePulseActive ||
            !protectedVehiclePulseEnabled ||
            protectedVehicleRenderer == null ||
            !protectedVehicleColorCached)
        {
            return;
        }

        if (objectiveController == null ||
            !objectiveController.IsNoTouchChallengeLevel ||
            objectiveController.State != LevelObjectiveController.RuntimeState.Running)
        {
            RestoreProtectedVehicleColor();
            return;
        }

        // Pause: freeze mid-pulse.
        if (Time.timeScale <= 0f)
        {
            return;
        }

        float wave =
            (Mathf.Sin(Time.unscaledTime * protectedVehiclePulseSpeed * Mathf.PI * 2f) + 1f) *
            0.5f;
        float t = wave * Mathf.Clamp01(protectedVehiclePulseStrength);

        Color pulsed = Color.Lerp(
            originalProtectedVehicleColor,
            protectedVehiclePulseColor,
            t
        );
        pulsed.a = originalProtectedVehicleColor.a;
        protectedVehicleRenderer.color = pulsed;
    }

    private void RestoreProtectedVehicleColor()
    {
        protectedVehiclePulseActive = false;

        if (protectedVehicleRenderer != null && protectedVehicleColorCached)
        {
            protectedVehicleRenderer.color = originalProtectedVehicleColor;
        }

        protectedVehicleRenderer = null;
        protectedVehicleColorCached = false;
    }

    private void BindMarkerToVehicle(VehicleController vehicle)
    {
        BindProtectedVehiclePresentation(vehicle);
    }

    private void UpdateMarkerPulse()
    {
        if (resolvedMarkerTransform == null)
        {
            return;
        }

        CacheMarkerScaleIfNeeded();

        if (!pulseEnabled || pulseScaleAmount <= 0f)
        {
            resolvedMarkerTransform.localScale = markerBaseScale;
            return;
        }

        // Pause: freeze mid-pulse.
        if (Time.timeScale <= 0f)
        {
            return;
        }

        float wave = (Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        float scaleMul = 1f + (pulseScaleAmount * wave);
        resolvedMarkerTransform.localScale = markerBaseScale * scaleMul;
    }

    private void ResolveMarkerRefs()
    {
        resolvedMarkerTransform = markerTransform;

        if (resolvedMarkerTransform == null && protectedVehicleMarker != null)
        {
            resolvedMarkerTransform = protectedVehicleMarker.transform;
        }

        markerSpriteRenderer = null;
        if (resolvedMarkerTransform != null)
        {
            markerSpriteRenderer =
                resolvedMarkerTransform.GetComponent<SpriteRenderer>();
            if (markerSpriteRenderer == null)
            {
                markerSpriteRenderer =
                    resolvedMarkerTransform.GetComponentInChildren<SpriteRenderer>(true);
            }
        }

        markerScaleCached = false;
    }

    private void DisableMarkerInputBlocking()
    {
        if (protectedVehicleMarker == null)
        {
            return;
        }

        Graphic[] graphics = protectedVehicleMarker.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            if (graphics[i] != null)
            {
                graphics[i].raycastTarget = false;
            }
        }
    }

    private void CacheMarkerScaleIfNeeded()
    {
        if (markerScaleCached || resolvedMarkerTransform == null)
        {
            return;
        }

        markerBaseScale = resolvedMarkerTransform.localScale;
        markerScaleCached = true;
    }

    private void ResetMarkerPulseScale()
    {
        if (resolvedMarkerTransform == null)
        {
            return;
        }

        CacheMarkerScaleIfNeeded();
        resolvedMarkerTransform.localScale = markerBaseScale;
    }

    private void SetMarkerVisible(bool visible)
    {
        markerVisible = visible;

        if (!visible)
        {
            RestoreProtectedVehicleColor();
            lastBoundProtectedVehicle = null;
            ResetMarkerPulseScale();
        }

        if (protectedVehicleMarker != null)
        {
            protectedVehicleMarker.SetActive(visible);
        }
        else if (resolvedMarkerTransform != null)
        {
            resolvedMarkerTransform.gameObject.SetActive(visible);
        }
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

    private void SetNoTouchHudVisible(bool visible)
    {
        if (noTouchHudRoot != null)
        {
            noTouchHudRoot.SetActive(visible);
        }
    }

    private void CacheObjectiveSnapshot()
    {
        lastKnownNoTouch = objectiveController != null &&
                           objectiveController.IsNoTouchChallengeLevel;
        lastKnownState = objectiveController != null
            ? objectiveController.State
            : LevelObjectiveController.RuntimeState.Inactive;
    }
}

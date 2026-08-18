using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Special mission intro: hergebruikt bestaande MissionLabel (center → HUD).
/// Presentation-only. Classic = geen intro.
/// </summary>
public class SpecialMissionIntroController : MonoBehaviour
{
    private struct LabelHomeState
    {
        public Vector2 AnchoredPosition;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 Pivot;
        public Vector3 LocalScale;
        public int SiblingIndex;
        public bool HasColor;
        public Color Color;
    }

    [Header("Refs")]
    [SerializeField] private LevelObjectiveController objectiveController;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private AudioManager audioManager;

    [Header("Mission UIs (MissionLabel via public Rect)")]
    [SerializeField] private TimedMissionUI timedMissionUI;
    [SerializeField] private MoveLimitMissionUI moveLimitMissionUI;
    [SerializeField] private MultiTargetMissionUI multiTargetMissionUI;
    [SerializeField] private NoTouchMissionUI noTouchMissionUI;
    [SerializeField] private FragileCargoMissionUI fragileCargoMissionUI;
    [SerializeField] private LimitedVehicleMissionUI limitedVehicleMissionUI;

    [Header("Special Mission Intro")]
    [SerializeField] private float introAppearDuration = 0.2f;
    [SerializeField] private float introHoldDuration = 1.0f;
    [SerializeField] private float introMoveDuration = 0.45f;
    [SerializeField] private float introScaleMultiplier = 1.8f;
    [SerializeField] private Vector2 introAnchoredPosition = Vector2.zero;

    [Tooltip("Optionele korte fade-in. Uit = alleen position/scale.")]
    [SerializeField] private bool introFadeIn = false;

    [Header("Special Mission Intro Sizing")]
    [Tooltip("Maximale intro-breedte als fractie van rootCanvas.rect.width.")]
    [SerializeField, Range(0.5f, 1f)] private float maxIntroWidthNormalized = 0.85f;

    public event Action OnSpecialMissionIntroStarted;
    public event Action OnSpecialMissionIntroCompleted;

    private Coroutine introCoroutine;
    private int handledSessionId = -1;
    private int ambulanceStartSfxSessionId = -1;
    private RectTransform activeLabel;
    private TextMeshProUGUI activeLabelTmp;
    private LabelHomeState homeState;
    private bool homeCached;

    private void Awake()
    {
        if (objectiveController == null)
        {
            objectiveController = FindAnyObjectByType<LevelObjectiveController>();
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }
    }

    private void OnDisable()
    {
        StopIntroImmediate(restoreLabel: true, notifyComplete: false);
    }

    private void LateUpdate()
    {
        if (objectiveController == null)
        {
            return;
        }

        bool waiting = objectiveController.IsWaitingForSpecialIntro;
        int sessionId = objectiveController.SpecialIntroSessionId;

        // Gate cleared of nieuwe session terwijl oude intro nog loopt.
        if (introCoroutine != null &&
            (!waiting || sessionId != handledSessionId))
        {
            StopIntroImmediate(restoreLabel: true, notifyComplete: false);
        }

        if (!waiting || sessionId == handledSessionId || introCoroutine != null)
        {
            return;
        }

        handledSessionId = sessionId;
        introCoroutine = StartCoroutine(RunIntroSequence(sessionId));
    }

    private IEnumerator RunIntroSequence(int sessionId)
    {
        OnSpecialMissionIntroStarted?.Invoke();

        // Wacht op scene fade (unscaled) vóór label-animatie.
        while (SceneTransition.IsTransitioning)
        {
            if (!StillCurrentSession(sessionId))
            {
                introCoroutine = null;
                yield break;
            }

            yield return null;
        }

        // Eén frame: MissionUI LateUpdate kan HUD/label activeren + tekst zetten.
        yield return null;

        if (!StillCurrentSession(sessionId))
        {
            introCoroutine = null;
            yield break;
        }

        // TimedAmbulance: mission-start SFX ×1 bij intro (niet bij timer Running).
        TryPlayTimedAmbulanceStartSfx(sessionId);

        RectTransform label = ResolveMissionLabel();
        if (label == null)
        {
            // Geen label gekoppeld: gate alsnog openen (geen hangende input-block).
            CompleteIntro(sessionId);
            introCoroutine = null;
            yield break;
        }

        EnsureLabelHierarchyActive(label);
        CacheHomeState(label);
        activeLabel = label;

        Vector3 homeScale = homeState.LocalScale;

        // Center in root-canvas screen space (via parent local), geen SafeArea.
        Vector2 introCenterLocal = ApplyIntroCenterLayout(label);

        // Eén keer berekenen; dezelfde introScale voor appear / hold / move-start.
        Vector3 introScale = ComputeFittedIntroScale(label, homeScale, introCenterLocal);

        // Start: center + (optioneel) kleinere scale voor pop-in.
        Vector3 appearStartScale = introScale * 0.85f;
        label.localScale = appearStartScale;

        if (introFadeIn && activeLabelTmp != null)
        {
            Color c = activeLabelTmp.color;
            c.a = 0f;
            activeLabelTmp.color = c;
        }

        // Appear / pop-in.
        float appear = Mathf.Max(0.01f, introAppearDuration);
        float elapsed = 0f;
        while (elapsed < appear)
        {
            if (!StillCurrentSession(sessionId))
            {
                RestoreHomeState();
                introCoroutine = null;
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / appear);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            label.localScale = Vector3.LerpUnclamped(appearStartScale, introScale, eased);

            if (introFadeIn && activeLabelTmp != null && homeState.HasColor)
            {
                Color c = homeState.Color;
                c.a = Mathf.Lerp(0f, homeState.Color.a, eased);
                activeLabelTmp.color = c;
            }

            yield return null;
        }

        label.localScale = introScale;
        if (introFadeIn && activeLabelTmp != null && homeState.HasColor)
        {
            activeLabelTmp.color = homeState.Color;
        }

        // Hold.
        float hold = Mathf.Max(0f, introHoldDuration);
        elapsed = 0f;
        while (elapsed < hold)
        {
            if (!StillCurrentSession(sessionId))
            {
                RestoreHomeState();
                introCoroutine = null;
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Move + scale + anchors: gefitte introScale → homeScale.
        Vector2 startPos = label.anchoredPosition;
        Vector2 startAnchorMin = label.anchorMin;
        Vector2 startAnchorMax = label.anchorMax;
        Vector2 startPivot = label.pivot;
        Vector3 startScale = introScale;

        float move = Mathf.Max(0.01f, introMoveDuration);
        elapsed = 0f;
        while (elapsed < move)
        {
            if (!StillCurrentSession(sessionId))
            {
                RestoreHomeState();
                introCoroutine = null;
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / move);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            label.anchorMin = Vector2.LerpUnclamped(startAnchorMin, homeState.AnchorMin, eased);
            label.anchorMax = Vector2.LerpUnclamped(startAnchorMax, homeState.AnchorMax, eased);
            label.pivot = Vector2.LerpUnclamped(startPivot, homeState.Pivot, eased);
            label.anchoredPosition = Vector2.LerpUnclamped(
                startPos,
                homeState.AnchoredPosition,
                eased
            );
            label.localScale = Vector3.LerpUnclamped(startScale, homeState.LocalScale, eased);
            yield return null;
        }

        RestoreHomeState();
        CompleteIntro(sessionId);
        introCoroutine = null;
    }

    private void CompleteIntro(int sessionId)
    {
        if (!StillCurrentSession(sessionId))
        {
            return;
        }

        if (objectiveController != null)
        {
            objectiveController.NotifySpecialMissionIntroCompleted();
        }
        else if (gameManager != null)
        {
            gameManager.SetSpecialMissionIntroBlocked(false);
        }

        OnSpecialMissionIntroCompleted?.Invoke();
    }

    private bool StillCurrentSession(int sessionId)
    {
        return objectiveController != null &&
               objectiveController.IsWaitingForSpecialIntro &&
               objectiveController.SpecialIntroSessionId == sessionId;
    }

    private void StopIntroImmediate(bool restoreLabel, bool notifyComplete)
    {
        if (introCoroutine != null)
        {
            StopCoroutine(introCoroutine);
            introCoroutine = null;
        }

        if (restoreLabel)
        {
            RestoreHomeState();
        }

        if (notifyComplete &&
            objectiveController != null &&
            objectiveController.IsWaitingForSpecialIntro)
        {
            objectiveController.NotifySpecialMissionIntroCompleted();
        }
    }

    private void TryPlayTimedAmbulanceStartSfx(int sessionId)
    {
        if (ambulanceStartSfxSessionId == sessionId)
        {
            return;
        }

        LevelData levelData = levelManager != null ? levelManager.CurrentLevelData : null;
        if (levelData == null ||
            levelData.objectiveType != LevelObjectiveType.TimedAmbulance)
        {
            return;
        }

        ambulanceStartSfxSessionId = sessionId;

        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }

        audioManager?.PlayAmbulanceMissionStart();
    }

    private RectTransform ResolveMissionLabel()
    {
        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        LevelData levelData = levelManager != null ? levelManager.CurrentLevelData : null;
        if (levelData == null)
        {
            return null;
        }

        switch (levelData.objectiveType)
        {
            case LevelObjectiveType.TimedAmbulance:
                return timedMissionUI != null ? timedMissionUI.MissionLabelRect : null;
            case LevelObjectiveType.MoveLimit:
                return moveLimitMissionUI != null ? moveLimitMissionUI.MissionLabelRect : null;
            case LevelObjectiveType.MultiTargetRescue:
                return multiTargetMissionUI != null ? multiTargetMissionUI.MissionLabelRect : null;
            case LevelObjectiveType.NoTouchChallenge:
                return noTouchMissionUI != null ? noTouchMissionUI.MissionLabelRect : null;
            case LevelObjectiveType.FragileCargo:
                return fragileCargoMissionUI != null ? fragileCargoMissionUI.MissionLabelRect : null;
            case LevelObjectiveType.LimitedVehicle:
                return limitedVehicleMissionUI != null
                    ? limitedVehicleMissionUI.MissionLabelRect
                    : null;
            default:
                return null;
        }
    }

    private void CacheHomeState(RectTransform label)
    {
        homeState = new LabelHomeState
        {
            AnchoredPosition = label.anchoredPosition,
            AnchorMin = label.anchorMin,
            AnchorMax = label.anchorMax,
            Pivot = label.pivot,
            LocalScale = label.localScale,
            SiblingIndex = label.GetSiblingIndex()
        };

        activeLabelTmp = label.GetComponent<TextMeshProUGUI>();
        homeState.HasColor = activeLabelTmp != null;
        if (homeState.HasColor)
        {
            homeState.Color = activeLabelTmp.color;
        }

        homeCached = true;
    }

    /// <summary>
    /// Zet label-anchors op center en plaatst het op het root-canvas midden,
    /// omgerekend naar MissionLabel.parent local/anchored space. Geen reparent.
    /// Gebruikt géén SafeArea.
    /// </summary>
    private Vector2 ApplyIntroCenterLayout(RectTransform label)
    {
        label.SetAsLastSibling();
        label.anchorMin = new Vector2(0.5f, 0.5f);
        label.anchorMax = new Vector2(0.5f, 0.5f);
        label.pivot = new Vector2(0.5f, 0.5f);

        Vector2 centerLocal = ComputeRootCanvasCenterAnchoredInParent(label);
        label.anchoredPosition = centerLocal + introAnchoredPosition;
        return label.anchoredPosition;
    }

    /// <summary>
    /// Root-canvas rect.center → world → parent local → anchoredPosition
    /// voor een child met anchors/pivot (0.5, 0.5).
    /// </summary>
    private static Vector2 ComputeRootCanvasCenterAnchoredInParent(RectTransform label)
    {
        RectTransform parent = label.parent as RectTransform;
        RectTransform rootCanvasRect = ResolveRootCanvasRect(label);
        if (parent == null || rootCanvasRect == null)
        {
            return Vector2.zero;
        }

        Canvas rootCanvas = rootCanvasRect.GetComponent<Canvas>();
        Camera eventCam = null;
        if (rootCanvas != null &&
            rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCam = rootCanvas.worldCamera;
        }

        Vector3 worldCenter = rootCanvasRect.TransformPoint(rootCanvasRect.rect.center);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(eventCam, worldCenter);

        Vector2 parentLocal;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                screenPoint,
                eventCam,
                out parentLocal))
        {
            // Fallback: InverseTransformPoint in parent space.
            Vector3 fallback = parent.InverseTransformPoint(worldCenter);
            parentLocal = new Vector2(fallback.x, fallback.y);
        }

        // Child anchors (0.5,0.5): anchoredPosition is offset t.o.v. parent.rect.center.
        return parentLocal - parent.rect.center;
    }

    /// <summary>
    /// Berekent één gefitte introScale = homeScale * actualMultiplier.
    /// Width-source: ALTIJD rootCanvas.rect.width (nooit SafeArea / HUD-parent).
    /// </summary>
    private Vector3 ComputeFittedIntroScale(
        RectTransform label,
        Vector3 homeScale,
        Vector2 introCenterLocal
    )
    {
        float requestedMultiplier = Mathf.Max(0.01f, introScaleMultiplier);
        float actualMultiplier = requestedMultiplier;

        float preferredWidth = 0f;
        float rootCanvasWidth = 0f;
        float allowedWidth = 0f;
        float parentScaleX = 1f;

        if (label != null)
        {
            preferredWidth = MeasureUnconstrainedLabelWidth(label);

            RectTransform rootCanvasRect = ResolveRootCanvasRect(label);
            if (rootCanvasRect != null)
            {
                rootCanvasWidth = rootCanvasRect.rect.width;
            }

            parentScaleX = GetParentScaleXRelativeToRootCanvas(label);

            float homeScaleX = Mathf.Abs(homeScale.x);
            if (homeScaleX < 0.0001f)
            {
                homeScaleX = 1f;
            }

            if (preferredWidth > 0.01f && rootCanvasWidth > 0.01f)
            {
                float normalized = Mathf.Clamp(maxIntroWidthNormalized, 0.5f, 1f);
                allowedWidth = rootCanvasWidth * normalized;

                // preferredWidth (label local) → root-canvas units via home + parent scales.
                float originalVisualWidth =
                    preferredWidth * homeScaleX * Mathf.Abs(parentScaleX);

                float requestedWidth = originalVisualWidth * requestedMultiplier;

                if (requestedWidth <= allowedWidth)
                {
                    actualMultiplier = requestedMultiplier;
                }
                else
                {
                    actualMultiplier = allowedWidth / originalVisualWidth;
                }

                actualMultiplier = Mathf.Min(actualMultiplier, requestedMultiplier);
            }
        }

        Vector3 introScale = homeScale * actualMultiplier;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        string labelText = activeLabelTmp != null ? activeLabelTmp.text : "(no tmp)";
        Debug.Log(
            "[SpecialIntroSizing]\n" +
            "Label=" + labelText + "\n" +
            "RootCanvasWidth=" + rootCanvasWidth + "\n" +
            "MaxWidthNormalized=" + maxIntroWidthNormalized + "\n" +
            "AllowedWidth=" + allowedWidth + "\n" +
            "PreferredWidth=" + preferredWidth + "\n" +
            "HomeScaleX=" + homeScale.x + "\n" +
            "ParentScaleX=" + parentScaleX + "\n" +
            "RequestedMultiplier=" + requestedMultiplier + "\n" +
            "ActualMultiplier=" + actualMultiplier + "\n" +
            "IntroCenterLocal=" + introCenterLocal
        );
#endif

        return introScale;
    }

    /// <summary>
    /// Volledige één-regel tekstbreedte in UI local units (geen wrap-constraint).
    /// </summary>
    private float MeasureUnconstrainedLabelWidth(RectTransform label)
    {
        TextMeshProUGUI tmp = activeLabelTmp;
        if (tmp == null)
        {
            tmp = label.GetComponent<TextMeshProUGUI>();
            activeLabelTmp = tmp;
        }

        if (tmp == null)
        {
            float rectWidthOnly = label.rect.width;
            return rectWidthOnly > 0.01f ? rectWidthOnly : 0f;
        }

        TextWrappingModes originalWrappingMode = tmp.textWrappingMode;
        TextOverflowModes overflow = tmp.overflowMode;

        try
        {
            // Tijdelijk unconstrained meten — wrap/overflow niet permanent wijzigen.
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.ForceMeshUpdate(ignoreActiveState: true);

            Vector2 preferredValues = tmp.GetPreferredValues(tmp.text);
            float preferred = preferredValues.x;

            if (preferred <= 0.01f)
            {
                preferred = tmp.preferredWidth;
            }

            if (preferred <= 0.01f)
            {
                Vector2 rendered = tmp.GetRenderedValues(onlyVisibleCharacters: false);
                preferred = rendered.x;
            }

            if (preferred > 0.01f)
            {
                return preferred;
            }
        }
        finally
        {
            tmp.textWrappingMode = originalWrappingMode;
            tmp.overflowMode = overflow;
            tmp.ForceMeshUpdate(ignoreActiveState: true);
        }

        float rectWidth = label.rect.width;
        return rectWidth > 0.01f ? rectWidth : 0f;
    }

    /// <summary>
    /// Cumulatieve localScale.x van parents tot (niet inclusief) rootCanvas.
    /// Label eigen scale zit hier niet in.
    /// </summary>
    private static float GetParentScaleXRelativeToRootCanvas(RectTransform label)
    {
        RectTransform rootCanvasRect = ResolveRootCanvasRect(label);
        if (label == null || rootCanvasRect == null)
        {
            return 1f;
        }

        float scaleX = 1f;
        Transform current = label.parent;
        Transform root = rootCanvasRect.transform;

        while (current != null && current != root)
        {
            scaleX *= current.localScale.x;
            current = current.parent;
        }

        return scaleX;
    }

    /// <summary>
    /// Root canvas RectTransform. Nooit SafeArea / HUD-parent.
    /// </summary>
    private static RectTransform ResolveRootCanvasRect(RectTransform label)
    {
        if (label == null)
        {
            return null;
        }

        Canvas canvas = label.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return null;
        }

        Canvas rootCanvas =
            canvas.rootCanvas != null ? canvas.rootCanvas : canvas;

        return rootCanvas.transform as RectTransform;
    }

    private void RestoreHomeState()
    {
        if (!homeCached || activeLabel == null)
        {
            homeCached = false;
            activeLabel = null;
            activeLabelTmp = null;
            return;
        }

        activeLabel.anchorMin = homeState.AnchorMin;
        activeLabel.anchorMax = homeState.AnchorMax;
        activeLabel.pivot = homeState.Pivot;
        activeLabel.anchoredPosition = homeState.AnchoredPosition;
        activeLabel.localScale = homeState.LocalScale;
        activeLabel.SetSiblingIndex(
            Mathf.Clamp(homeState.SiblingIndex, 0, activeLabel.parent.childCount - 1)
        );

        if (homeState.HasColor && activeLabelTmp != null)
        {
            activeLabelTmp.color = homeState.Color;
        }

        homeCached = false;
        activeLabel = null;
        activeLabelTmp = null;
    }

    private static void EnsureLabelHierarchyActive(RectTransform label)
    {
        if (label == null)
        {
            return;
        }

        // Activeer label + parents zodat intro zichtbaar is als HUD-root al aan staat
        // of als alleen het label zelf uit stond.
        Transform current = label;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
            }

            current = current.parent;
        }
    }
}

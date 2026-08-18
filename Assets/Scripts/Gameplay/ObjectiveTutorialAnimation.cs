using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tutorial illustration animations (UI RectTransforms / TMP only).
/// Classic / Timed / MoveLimit: target→exit loop + drag arrow.
/// Timed: ModifierIcon clock pulse.
/// MoveLimit: ModifierText demo countdown (geen gameplay moves).
/// MultiTarget: Secondary→Exit hide → Primary→Exit hide + 2/2→1/2→0/2 demo.
/// NoTouch: Primary=protected (stil+rood pulse), Secondary=target→Exit.
/// FragileCargo: cargo in 3 stappen naar exit + 3→2→1→0 (per stop).
/// LimitedVehicle: limited Primary 2→1→LOCK, daarna Secondary→Exit.
/// </summary>
public class ObjectiveTutorialAnimation : MonoBehaviour
{
    public enum TutorialAnimationType
    {
        None = 0,
        TargetToExit = 1,
        TimedTargetToExit = 2,
        MoveLimitTargetToExit = 3,
        MultiTargetSequence = 4,
        NoTouchProtected = 5,
        FragileCargoToExit = 6,
        LimitedVehicleSequence = 7
    }

    [Header("Illustration Refs")]
    [SerializeField] private RectTransform primaryVehicleRect;
    [SerializeField] private RectTransform secondaryVehicleRect;
    [SerializeField] private RectTransform exitRect;
    [SerializeField] private RectTransform dragArrowRect;
    [SerializeField] private GameObject dragArrowRoot;

    [Tooltip("Optioneel. Liever via ObjectiveTutorialController.SetModifierRect.")]
    [SerializeField] private RectTransform modifierRect;

    [Tooltip("Optioneel. Liever via ObjectiveTutorialController.SetModifierText.")]
    [SerializeField] private TMP_Text modifierText;

    [Header("Classic Tutorial Animation")]
    [SerializeField] private Vector2 carExitOffset = new Vector2(-40f, 0f);
    [SerializeField] private float initialDelay = 0.25f;
    [SerializeField] private float carMoveDuration = 0.9f;
    [SerializeField] private float endHoldDuration = 0.25f;
    [SerializeField] private float loopDelay = 0.35f;

    [Header("Drag Arrow")]
    [SerializeField] private bool animateArrow = true;
    [SerializeField] private float arrowMoveDistance = 20f;
    [SerializeField] private float arrowMoveSpeed = 2f;

    [Header("Multi Target Tutorial")]
    [SerializeField] private float multiTargetBetweenRescuesDelay = 0.3f;
    [SerializeField] private float multiTargetFinalHoldDuration = 0.4f;
    [SerializeField] private Vector2 primaryArrowOffset = Vector2.zero;
    [SerializeField] private Vector2 secondaryArrowOffset = Vector2.zero;

    [Header("No Touch Tutorial")]
    [SerializeField] private Color noTouchProtectedPulseColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float noTouchProtectedPulseSpeed = 2f;
    [SerializeField] private float noTouchProtectedPulseScale = 1.06f;
    [SerializeField] private float noTouchWarningDuration = 1.0f;
    [SerializeField] private float noTouchIconPulseScale = 1.08f;
    [SerializeField] private float noTouchMovePhasePulseStrength = 0.35f;

    [Header("Timed Tutorial Modifier")]
    [SerializeField] private bool animateTimedModifier = true;
    [SerializeField] private float timedModifierPulseSpeed = 2.0f;
    [SerializeField] private float timedModifierPulseScaleAmount = 0.12f;

    [Header("Move Limit Tutorial")]
    [SerializeField] private int moveLimitDemoStart = 5;
    [SerializeField] private int moveLimitDemoEnd = 3;
    [SerializeField] private float moveLimitDemoStepDuration = 0.45f;
    [SerializeField] private Color moveLimitNormalColor = Color.white;
    [SerializeField] private Color moveLimitWarningColor = new Color(1f, 0.55f, 0.15f, 1f);
    [SerializeField] private float moveLimitCounterPulseScale = 1.08f;

    [Header("Fragile Cargo Tutorial")]
    [SerializeField] private int fragileCargoDemoStart = 3;
    [SerializeField] private float fragileCargoSegmentDuration = 0.35f;
    [SerializeField] private float fragileCargoBetweenMovesDelay = 0.2f;
    [SerializeField] private float fragileCargoFinalHoldDuration = 0.5f;

    [Header("Limited Vehicle Tutorial")]
    [SerializeField] private Vector2 limitedMoveOffset = new Vector2(60f, 0f);
    [SerializeField] private float limitedMoveDuration = 0.35f;
    [SerializeField] private float limitedBetweenMovesDelay = 0.2f;
    [SerializeField] private float limitedLockHoldDuration = 0.5f;
    [SerializeField] private float limitedLockPopScale = 1.15f;
    [SerializeField] private float limitedLockPopDuration = 0.18f;
    [SerializeField] private float limitedFinalHoldDuration = 0.4f;

    private Coroutine carLoopCoroutine;
    private bool isPlaying;
    private TutorialAnimationType activeAnimationType = TutorialAnimationType.None;

    private Vector2 primaryStartPosition;
    private Vector2 secondaryStartPosition;
    private Vector2 arrowBasePosition;
    private Vector2 arrowPulseOrigin;
    private Vector3 modifierIconBaseScale = Vector3.one;
    private bool primaryStartCached;
    private bool secondaryStartCached;
    private bool arrowBaseCached;
    private bool modifierIconScaleCached;

    private string modifierTextBase = string.Empty;
    private Color modifierTextBaseColor = Color.white;
    private Vector3 modifierTextBaseScale = Vector3.one;
    private bool modifierTextCached;

    private int moveLimitDisplayedValue = -1;
    private int fragileCargoDisplayedValue = -1;

    private int primarySiblingIndex;
    private int secondarySiblingIndex;
    private bool siblingOrderCached;

    private Color protectedImageBaseColor = Color.white;
    private Vector3 protectedImageBaseScale = Vector3.one;
    private bool protectedImageCached;
    private float noTouchPulseStrength;
    private bool noTouchArrowPulseActive;
    private bool limitedArrowPulseActive;

    /// <summary>
    /// Bind PrimaryVehicle RectTransform vanuit ObjectiveTutorialController.
    /// Null laat bestaande serialized ref intact.
    /// </summary>
    public void SetPrimaryVehicleRect(RectTransform rect)
    {
        if (rect != null)
        {
            primaryVehicleRect = rect;
        }
    }

    /// <summary>
    /// Bind SecondaryVehicle RectTransform vanuit ObjectiveTutorialController.
    /// Null laat bestaande serialized ref intact.
    /// </summary>
    public void SetSecondaryVehicleRect(RectTransform rect)
    {
        if (rect != null)
        {
            secondaryVehicleRect = rect;
        }
    }

    /// <summary>
    /// Bind ModifierIcon RectTransform vanuit ObjectiveTutorialController.
    /// Null laat bestaande serialized ref intact.
    /// </summary>
    public void SetModifierRect(RectTransform rect)
    {
        if (rect != null)
        {
            modifierRect = rect;
        }
    }

    /// <summary>
    /// Bind ModifierText vanuit ObjectiveTutorialController.
    /// Null laat bestaande serialized ref intact.
    /// </summary>
    public void SetModifierText(TMP_Text text)
    {
        if (text != null)
        {
            modifierText = text;
        }
    }

    public Image ResolvePrimaryImage()
    {
        if (primaryVehicleRect == null)
        {
            return null;
        }

        return primaryVehicleRect.GetComponent<Image>();
    }

    public Image ResolveSecondaryImage()
    {
        if (secondaryVehicleRect == null)
        {
            return null;
        }

        return secondaryVehicleRect.GetComponent<Image>();
    }

    public Image ResolveModifierImage()
    {
        if (modifierRect == null)
        {
            return null;
        }

        return modifierRect.GetComponent<Image>();
    }

    /// <summary>
    /// Serialized/bound ModifierText (zelfde object als controller na SetModifierText).
    /// </summary>
    public TMP_Text ResolveModifierText()
    {
        return modifierText;
    }

    public void Play(LevelObjectiveType objectiveType)
    {
        StopAndReset();

        TutorialAnimationType animationType = ResolveAnimationType(objectiveType);
        if (animationType == TutorialAnimationType.None)
        {
            SetDragArrowVisible(false);
            return;
        }

        activeAnimationType = animationType;
        CacheBasePositions();
        ResetVisualsToBase();

        if (animationType == TutorialAnimationType.NoTouchProtected)
        {
            SetDragArrowVisible(false);
        }
        else
        {
            SetDragArrowVisible(true);
        }

        if (animationType == TutorialAnimationType.MoveLimitTargetToExit)
        {
            ApplyMoveLimitCounter(moveLimitDemoStart, forceWarning: false);
        }
        else if (animationType == TutorialAnimationType.MultiTargetSequence)
        {
            ApplyMultiTargetRemainingText(2);
        }
        else if (animationType == TutorialAnimationType.FragileCargoToExit)
        {
            ApplyFragileCargoCounter(fragileCargoDemoStart);
        }
        else if (animationType == TutorialAnimationType.LimitedVehicleSequence)
        {
            ApplyLimitedVehicleMovesAvailableState(2);
        }

        isPlaying = true;

        if (animationType == TutorialAnimationType.MultiTargetSequence)
        {
            carLoopCoroutine = StartCoroutine(RunMultiTargetSequenceLoop());
        }
        else if (animationType == TutorialAnimationType.NoTouchProtected)
        {
            carLoopCoroutine = StartCoroutine(RunNoTouchProtectedLoop());
        }
        else if (animationType == TutorialAnimationType.FragileCargoToExit)
        {
            carLoopCoroutine = StartCoroutine(RunFragileCargoToExitLoop());
        }
        else if (animationType == TutorialAnimationType.LimitedVehicleSequence)
        {
            carLoopCoroutine = StartCoroutine(RunLimitedVehicleSequenceLoop());
        }
        else
        {
            carLoopCoroutine = StartCoroutine(RunTargetToExitLoop());
        }
    }

    /// <summary>
    /// Stopt animaties en herstelt transforms / visibility / modifier text+color+scale.
    /// Library visibility ownership blijft bij controller (ApplyPresentation na Play).
    /// </summary>
    public void StopAndReset()
    {
        isPlaying = false;
        activeAnimationType = TutorialAnimationType.None;
        moveLimitDisplayedValue = -1;
        fragileCargoDisplayedValue = -1;
        noTouchPulseStrength = 0f;
        noTouchArrowPulseActive = false;
        limitedArrowPulseActive = false;

        if (carLoopCoroutine != null)
        {
            StopCoroutine(carLoopCoroutine);
            carLoopCoroutine = null;
        }

        ResetVisualsToBase();
    }

    private void OnDisable()
    {
        StopAndReset();
    }

    private void LateUpdate()
    {
        if (!isPlaying)
        {
            return;
        }

        if (UsesArrowPulse(activeAnimationType))
        {
            UpdateDragArrowPulse();
        }

        if (UsesTargetToExitMovement(activeAnimationType))
        {
            UpdateTimedModifierPulse();
            UpdateMoveLimitCounterPulse();
        }

        if (activeAnimationType == TutorialAnimationType.FragileCargoToExit)
        {
            UpdateFragileCargoCounterPulse();
        }

        if (activeAnimationType == TutorialAnimationType.NoTouchProtected)
        {
            PinNoTouchProtectedPosition();
            UpdateNoTouchProtectedPulse();
        }
    }

    private void UpdateDragArrowPulse()
    {
        if (!animateArrow || dragArrowRect == null || !arrowBaseCached)
        {
            return;
        }

        float wave = Mathf.Sin(Time.unscaledTime * arrowMoveSpeed * Mathf.PI * 2f);
        dragArrowRect.anchoredPosition =
            arrowPulseOrigin + (Vector2.right * (wave * arrowMoveDistance));
    }

    private void UpdateTimedModifierPulse()
    {
        if (activeAnimationType != TutorialAnimationType.TimedTargetToExit ||
            !animateTimedModifier ||
            modifierRect == null ||
            !modifierIconScaleCached ||
            !modifierRect.gameObject.activeInHierarchy)
        {
            return;
        }

        float wave = Mathf.Sin(Time.unscaledTime * timedModifierPulseSpeed * Mathf.PI * 2f);
        float pulse01 = 0.5f + 0.5f * wave;
        float scaleMul = 1f + pulse01 * timedModifierPulseScaleAmount;
        modifierRect.localScale = modifierIconBaseScale * scaleMul;
    }

    private void UpdateMoveLimitCounterPulse()
    {
        if (activeAnimationType != TutorialAnimationType.MoveLimitTargetToExit ||
            modifierText == null ||
            !modifierTextCached ||
            moveLimitDisplayedValue != moveLimitDemoEnd)
        {
            return;
        }

        RectTransform textRect = modifierText.rectTransform;
        if (textRect == null || !textRect.gameObject.activeInHierarchy)
        {
            return;
        }

        float wave = Mathf.Sin(Time.unscaledTime * 2f * Mathf.PI * 2f);
        float pulse01 = 0.5f + 0.5f * wave;
        float scaleMul = Mathf.Lerp(1f, Mathf.Max(1.01f, moveLimitCounterPulseScale), pulse01);
        textRect.localScale = modifierTextBaseScale * scaleMul;
    }

    private void UpdateFragileCargoCounterPulse()
    {
        if (modifierText == null ||
            !modifierTextCached ||
            fragileCargoDisplayedValue != 1)
        {
            return;
        }

        RectTransform textRect = modifierText.rectTransform;
        if (textRect == null || !textRect.gameObject.activeInHierarchy)
        {
            return;
        }

        float wave = Mathf.Sin(Time.unscaledTime * 2f * Mathf.PI * 2f);
        float pulse01 = 0.5f + 0.5f * wave;
        float scaleMul = Mathf.Lerp(1f, Mathf.Max(1.01f, moveLimitCounterPulseScale), pulse01);
        textRect.localScale = modifierTextBaseScale * scaleMul;
    }

    private static TutorialAnimationType ResolveAnimationType(LevelObjectiveType objectiveType)
    {
        switch (objectiveType)
        {
            case LevelObjectiveType.Classic:
                return TutorialAnimationType.TargetToExit;
            case LevelObjectiveType.TimedAmbulance:
                return TutorialAnimationType.TimedTargetToExit;
            case LevelObjectiveType.MoveLimit:
                return TutorialAnimationType.MoveLimitTargetToExit;
            case LevelObjectiveType.MultiTargetRescue:
                return TutorialAnimationType.MultiTargetSequence;
            case LevelObjectiveType.NoTouchChallenge:
                return TutorialAnimationType.NoTouchProtected;
            case LevelObjectiveType.FragileCargo:
                return TutorialAnimationType.FragileCargoToExit;
            case LevelObjectiveType.LimitedVehicle:
                return TutorialAnimationType.LimitedVehicleSequence;
            default:
                return TutorialAnimationType.None;
        }
    }

    private static bool UsesTargetToExitMovement(TutorialAnimationType animationType)
    {
        return animationType == TutorialAnimationType.TargetToExit ||
               animationType == TutorialAnimationType.TimedTargetToExit ||
               animationType == TutorialAnimationType.MoveLimitTargetToExit;
    }

    private bool UsesArrowPulse(TutorialAnimationType animationType)
    {
        if (UsesTargetToExitMovement(animationType) ||
            animationType == TutorialAnimationType.MultiTargetSequence ||
            animationType == TutorialAnimationType.FragileCargoToExit)
        {
            return true;
        }

        if (animationType == TutorialAnimationType.LimitedVehicleSequence)
        {
            return limitedArrowPulseActive;
        }

        return animationType == TutorialAnimationType.NoTouchProtected &&
               noTouchArrowPulseActive;
    }

    private void CacheBasePositions()
    {
        primaryStartCached = false;
        secondaryStartCached = false;
        arrowBaseCached = false;
        modifierIconScaleCached = false;
        modifierTextCached = false;
        siblingOrderCached = false;
        protectedImageCached = false;

        if (primaryVehicleRect != null)
        {
            primaryStartPosition = primaryVehicleRect.anchoredPosition;
            primarySiblingIndex = primaryVehicleRect.GetSiblingIndex();
            primaryStartCached = true;

            // NoTouch: Primary = protected — cache color/scale voor pulse restore.
            Image primaryImage = primaryVehicleRect.GetComponent<Image>();
            if (primaryImage != null)
            {
                protectedImageBaseColor = primaryImage.color;
                Vector3 scale = primaryVehicleRect.localScale;
                if (Mathf.Abs(scale.x) < 0.0001f ||
                    Mathf.Abs(scale.y) < 0.0001f ||
                    Mathf.Abs(scale.z) < 0.0001f)
                {
                    scale = Vector3.one;
                    primaryVehicleRect.localScale = scale;
                }

                protectedImageBaseScale = scale;
                protectedImageCached = true;
            }
        }

        if (secondaryVehicleRect != null)
        {
            secondaryStartPosition = secondaryVehicleRect.anchoredPosition;
            secondarySiblingIndex = secondaryVehicleRect.GetSiblingIndex();
            secondaryStartCached = true;
        }

        siblingOrderCached = primaryStartCached && secondaryStartCached;

        if (dragArrowRect != null)
        {
            arrowBasePosition = dragArrowRect.anchoredPosition;
            arrowPulseOrigin = arrowBasePosition;
            arrowBaseCached = true;
        }

        if (modifierRect != null)
        {
            Vector3 scale = modifierRect.localScale;
            if (Mathf.Abs(scale.x) < 0.0001f ||
                Mathf.Abs(scale.y) < 0.0001f ||
                Mathf.Abs(scale.z) < 0.0001f)
            {
                scale = Vector3.one;
                modifierRect.localScale = scale;
            }

            modifierIconBaseScale = scale;
            modifierIconScaleCached = true;
        }

        if (modifierText != null)
        {
            modifierTextBase = modifierText.text ?? string.Empty;
            modifierTextBaseColor = modifierText.color;
            Vector3 textScale = modifierText.rectTransform.localScale;
            if (Mathf.Abs(textScale.x) < 0.0001f ||
                Mathf.Abs(textScale.y) < 0.0001f ||
                Mathf.Abs(textScale.z) < 0.0001f)
            {
                textScale = Vector3.one;
                modifierText.rectTransform.localScale = textScale;
            }

            modifierTextBaseScale = textScale;
            modifierTextCached = true;
        }
    }

    private void ResetVisualsToBase()
    {
        if (primaryVehicleRect != null && primaryStartCached)
        {
            primaryVehicleRect.anchoredPosition = primaryStartPosition;
        }

        if (secondaryVehicleRect != null && secondaryStartCached)
        {
            secondaryVehicleRect.anchoredPosition = secondaryStartPosition;
        }

        RestoreNoTouchProtectedAppearance();

        SetVehicleImageVisible(primaryVehicleRect, true);
        SetVehicleImageVisible(secondaryVehicleRect, true);
        RestoreSiblingOrder();

        noTouchPulseStrength = 0f;
        noTouchArrowPulseActive = false;
        limitedArrowPulseActive = false;

        if (dragArrowRect != null && arrowBaseCached)
        {
            arrowPulseOrigin = arrowBasePosition;
            dragArrowRect.anchoredPosition = arrowBasePosition;
        }

        if (modifierRect != null && modifierIconScaleCached)
        {
            modifierRect.localScale = modifierIconBaseScale;
        }

        if (modifierText != null && modifierTextCached)
        {
            modifierText.text = modifierTextBase;
            modifierText.color = modifierTextBaseColor;
            modifierText.rectTransform.localScale = modifierTextBaseScale;
        }

        // Beëindig LimitedVehicle demo-toggle (controller ApplyPresentation zet daarna library-state).
        RestoreLimitedVehicleModifierLeavesToSafeBase();
    }

    private Vector2 ResolveCarEndPosition()
    {
        if (exitRect != null)
        {
            return exitRect.anchoredPosition + carExitOffset;
        }

        if (primaryStartCached)
        {
            return primaryStartPosition + carExitOffset;
        }

        return carExitOffset;
    }

    private void SetArrowPulseOrigin(Vector2 origin)
    {
        arrowPulseOrigin = origin;
        if (dragArrowRect != null)
        {
            dragArrowRect.anchoredPosition = origin;
        }
    }

    private IEnumerator RunTargetToExitLoop()
    {
        if (primaryVehicleRect == null || !primaryStartCached)
        {
            carLoopCoroutine = null;
            yield break;
        }

        SetArrowPulseOrigin(arrowBasePosition);

        if (initialDelay > 0f)
        {
            yield return WaitUnscaled(initialDelay);
        }

        while (isPlaying && UsesTargetToExitMovement(activeAnimationType))
        {
            if (activeAnimationType == TutorialAnimationType.MoveLimitTargetToExit)
            {
                ApplyMoveLimitCounter(moveLimitDemoStart, forceWarning: false);
            }

            Vector2 start = primaryStartPosition;
            Vector2 end = ResolveCarEndPosition();

            float moveDuration = Mathf.Max(0.01f, carMoveDuration);
            float elapsed = 0f;
            int lastDemoValue = moveLimitDemoStart;

            while (elapsed < moveDuration)
            {
                if (!isPlaying)
                {
                    carLoopCoroutine = null;
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                primaryVehicleRect.anchoredPosition =
                    Vector2.LerpUnclamped(start, end, EaseSmoothStep(t));

                if (activeAnimationType == TutorialAnimationType.MoveLimitTargetToExit)
                {
                    int demoValue = EvaluateMoveLimitDemoValue(elapsed, moveDuration);
                    if (demoValue != lastDemoValue)
                    {
                        lastDemoValue = demoValue;
                        ApplyMoveLimitCounter(
                            demoValue,
                            forceWarning: demoValue <= moveLimitDemoEnd
                        );
                    }
                }

                yield return null;
            }

            primaryVehicleRect.anchoredPosition = end;

            if (activeAnimationType == TutorialAnimationType.MoveLimitTargetToExit)
            {
                ApplyMoveLimitCounter(moveLimitDemoEnd, forceWarning: true);
            }

            if (endHoldDuration > 0f)
            {
                yield return WaitUnscaled(endHoldDuration);
            }

            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            primaryVehicleRect.anchoredPosition = primaryStartPosition;

            if (activeAnimationType == TutorialAnimationType.MoveLimitTargetToExit)
            {
                ApplyMoveLimitCounter(moveLimitDemoStart, forceWarning: false);
            }

            if (loopDelay > 0f)
            {
                yield return WaitUnscaled(loopDelay);
            }
        }

        carLoopCoroutine = null;
    }

    private IEnumerator RunMultiTargetSequenceLoop()
    {
        if (primaryVehicleRect == null || !primaryStartCached ||
            secondaryVehicleRect == null || !secondaryStartCached)
        {
            carLoopCoroutine = null;
            yield break;
        }

        if (initialDelay > 0f)
        {
            yield return WaitUnscaled(initialDelay);
        }

        Vector2 exitPos = ResolveCarEndPosition();

        while (isPlaying && activeAnimationType == TutorialAnimationType.MultiTargetSequence)
        {
            // Start state: beide zichtbaar, counter 2 / 2.
            primaryVehicleRect.anchoredPosition = primaryStartPosition;
            secondaryVehicleRect.anchoredPosition = secondaryStartPosition;
            SetVehicleImageVisible(primaryVehicleRect, true);
            SetVehicleImageVisible(secondaryVehicleRect, true);
            RestoreSiblingOrder();
            ApplyMultiTargetRemainingText(2);

            // Phase 1: Secondary eerst → exit (Primary blijft stil).
            PositionArrowFor(secondaryVehicleRect);
            BringVehicleToFront(secondaryVehicleRect);
            yield return MoveRectTo(
                secondaryVehicleRect,
                secondaryStartPosition,
                exitPos,
                carMoveDuration);
            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            if (endHoldDuration > 0f)
            {
                yield return WaitUnscaled(endHoldDuration);
            }

            SetVehicleImageVisible(secondaryVehicleRect, false);
            ApplyMultiTargetRemainingText(1);

            if (multiTargetBetweenRescuesDelay > 0f)
            {
                yield return WaitUnscaled(multiTargetBetweenRescuesDelay);
            }

            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            // Phase 2: Primary →zelfde exit (Secondary blijft verborgen).
            PositionArrowFor(primaryVehicleRect);
            BringVehicleToFront(primaryVehicleRect);
            yield return MoveRectTo(
                primaryVehicleRect,
                primaryStartPosition,
                exitPos,
                carMoveDuration);
            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            if (endHoldDuration > 0f)
            {
                yield return WaitUnscaled(endHoldDuration);
            }

            SetVehicleImageVisible(primaryVehicleRect, false);
            ApplyMultiTargetRemainingText(0);

            if (multiTargetFinalHoldDuration > 0f)
            {
                yield return WaitUnscaled(multiTargetFinalHoldDuration);
            }

            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            // Reset beide + counter.
            primaryVehicleRect.anchoredPosition = primaryStartPosition;
            secondaryVehicleRect.anchoredPosition = secondaryStartPosition;
            SetVehicleImageVisible(primaryVehicleRect, true);
            SetVehicleImageVisible(secondaryVehicleRect, true);
            RestoreSiblingOrder();
            ApplyMultiTargetRemainingText(2);
            SetArrowPulseOrigin(arrowBasePosition);

            if (loopDelay > 0f)
            {
                yield return WaitUnscaled(loopDelay);
            }
        }

        carLoopCoroutine = null;
    }

    private IEnumerator RunNoTouchProtectedLoop()
    {
        if (primaryVehicleRect == null || !primaryStartCached ||
            secondaryVehicleRect == null || !secondaryStartCached)
        {
            carLoopCoroutine = null;
            yield break;
        }

        if (initialDelay > 0f)
        {
            yield return WaitUnscaled(initialDelay);
        }

        Vector2 exitPos = ResolveCarEndPosition();

        while (isPlaying && activeAnimationType == TutorialAnimationType.NoTouchProtected)
        {
            // NoTouch mapping: Primary = protected (stil), Secondary = target.
            primaryVehicleRect.anchoredPosition = primaryStartPosition;
            secondaryVehicleRect.anchoredPosition = secondaryStartPosition;
            SetVehicleImageVisible(primaryVehicleRect, true);
            SetVehicleImageVisible(secondaryVehicleRect, true);
            RestoreNoTouchProtectedAppearance();
            SetDragArrowVisible(false);
            noTouchArrowPulseActive = false;
            noTouchPulseStrength = 1f;

            if (noTouchWarningDuration > 0f)
            {
                yield return WaitUnscaled(noTouchWarningDuration);
            }

            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            // Secondary/target beweegt; Primary/protected blijft stil.
            noTouchPulseStrength = Mathf.Clamp01(noTouchMovePhasePulseStrength);
            PositionArrowFor(secondaryVehicleRect);
            noTouchArrowPulseActive = true;
            SetDragArrowVisible(true);

            yield return MoveRectTo(
                secondaryVehicleRect,
                secondaryStartPosition,
                exitPos,
                carMoveDuration);
            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            // Protected Primary nooit verplaatsen.
            primaryVehicleRect.anchoredPosition = primaryStartPosition;

            if (endHoldDuration > 0f)
            {
                yield return WaitUnscaled(endHoldDuration);
            }

            SetVehicleImageVisible(secondaryVehicleRect, false);
            SetDragArrowVisible(false);
            noTouchArrowPulseActive = false;

            if (endHoldDuration > 0f)
            {
                yield return WaitUnscaled(endHoldDuration);
            }

            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            // Reset loop.
            primaryVehicleRect.anchoredPosition = primaryStartPosition;
            secondaryVehicleRect.anchoredPosition = secondaryStartPosition;
            SetVehicleImageVisible(primaryVehicleRect, true);
            SetVehicleImageVisible(secondaryVehicleRect, true);
            RestoreNoTouchProtectedAppearance();
            if (modifierRect != null && modifierIconScaleCached)
            {
                modifierRect.localScale = modifierIconBaseScale;
            }

            SetArrowPulseOrigin(arrowBasePosition);
            noTouchPulseStrength = 0f;

            if (loopDelay > 0f)
            {
                yield return WaitUnscaled(loopDelay);
            }
        }

        carLoopCoroutine = null;
    }

    private IEnumerator RunFragileCargoToExitLoop()
    {
        if (primaryVehicleRect == null || !primaryStartCached)
        {
            carLoopCoroutine = null;
            yield break;
        }

        if (initialDelay > 0f)
        {
            yield return WaitUnscaled(initialDelay);
        }

        Vector2 start = primaryStartPosition;
        Vector2 end = ResolveCarEndPosition();
        Vector2 waypoint1 = Vector2.Lerp(start, end, 0.33f);
        Vector2 waypoint2 = Vector2.Lerp(start, end, 0.66f);
        float segmentDuration = Mathf.Max(0.05f, fragileCargoSegmentDuration);

        while (isPlaying && activeAnimationType == TutorialAnimationType.FragileCargoToExit)
        {
            primaryVehicleRect.anchoredPosition = start;
            SetVehicleImageVisible(primaryVehicleRect, true);
            ApplyFragileCargoCounter(fragileCargoDemoStart);
            PositionArrowFor(primaryVehicleRect);
            SetDragArrowVisible(true);

            // Move 1: start → 33% → counter 2
            yield return MoveRectTo(primaryVehicleRect, start, waypoint1, segmentDuration);
            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            ApplyFragileCargoCounter(fragileCargoDemoStart - 1);
            SetDragArrowVisible(false);
            if (fragileCargoBetweenMovesDelay > 0f)
            {
                yield return WaitUnscaled(fragileCargoBetweenMovesDelay);
            }

            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            // Move 2: 33% → 66% → counter 1 (warning)
            PositionArrowFor(primaryVehicleRect);
            SetDragArrowVisible(true);
            yield return MoveRectTo(primaryVehicleRect, waypoint1, waypoint2, segmentDuration);
            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            ApplyFragileCargoCounter(1);
            SetDragArrowVisible(false);
            if (fragileCargoBetweenMovesDelay > 0f)
            {
                yield return WaitUnscaled(fragileCargoBetweenMovesDelay);
            }

            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            // Move 3: 66% → exit → counter 0
            PositionArrowFor(primaryVehicleRect);
            SetDragArrowVisible(true);
            yield return MoveRectTo(primaryVehicleRect, waypoint2, end, segmentDuration);
            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            ApplyFragileCargoCounter(0);
            SetDragArrowVisible(false);
            SetVehicleImageVisible(primaryVehicleRect, false);

            if (fragileCargoFinalHoldDuration > 0f)
            {
                yield return WaitUnscaled(fragileCargoFinalHoldDuration);
            }

            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            // Reset
            primaryVehicleRect.anchoredPosition = start;
            SetVehicleImageVisible(primaryVehicleRect, true);
            ApplyFragileCargoCounter(fragileCargoDemoStart);
            SetArrowPulseOrigin(arrowBasePosition);

            if (loopDelay > 0f)
            {
                yield return WaitUnscaled(loopDelay);
            }
        }

        carLoopCoroutine = null;
    }

    private IEnumerator RunLimitedVehicleSequenceLoop()
    {
        if (primaryVehicleRect == null || !primaryStartCached ||
            secondaryVehicleRect == null || !secondaryStartCached)
        {
            carLoopCoroutine = null;
            yield break;
        }

        if (initialDelay > 0f)
        {
            yield return WaitUnscaled(initialDelay);
        }

        Vector2 limitedWaypoint = primaryStartPosition + limitedMoveOffset;
        Vector2 exitPos = ResolveCarEndPosition();
        float moveDuration = Mathf.Max(0.05f, limitedMoveDuration);

        while (isPlaying && activeAnimationType == TutorialAnimationType.LimitedVehicleSequence)
        {
            // START: limited=2, lock icon uit, target stil.
            primaryVehicleRect.anchoredPosition = primaryStartPosition;
            secondaryVehicleRect.anchoredPosition = secondaryStartPosition;
            SetVehicleImageVisible(primaryVehicleRect, true);
            SetVehicleImageVisible(secondaryVehicleRect, true);
            ApplyLimitedVehicleMovesAvailableState(2);
            PositionArrowFor(primaryVehicleRect);
            limitedArrowPulseActive = true;
            SetDragArrowVisible(true);

            // MOVE 1: limited verschuift → 1
            yield return MoveRectTo(
                primaryVehicleRect,
                primaryStartPosition,
                limitedWaypoint,
                moveDuration);
            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            ApplyLimitedVehicleMovesAvailableState(1);

            if (limitedBetweenMovesDelay > 0f)
            {
                yield return WaitUnscaled(limitedBetweenMovesDelay);
            }

            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            // MOVE 2: terug naar start → LOCK
            PositionArrowFor(primaryVehicleRect);
            SetDragArrowVisible(true);
            yield return MoveRectTo(
                primaryVehicleRect,
                limitedWaypoint,
                primaryStartPosition,
                moveDuration);
            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            primaryVehicleRect.anchoredPosition = primaryStartPosition;
            ApplyLimitedVehicleLockedState();
            limitedArrowPulseActive = false;
            SetDragArrowVisible(false);

            yield return PopModifierIconOnce();
            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            if (limitedLockHoldDuration > 0f)
            {
                yield return WaitUnscaled(limitedLockHoldDuration);
            }

            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            // Primary blijft locked/stil; Secondary → exit.
            primaryVehicleRect.anchoredPosition = primaryStartPosition;
            PositionArrowFor(secondaryVehicleRect);
            limitedArrowPulseActive = true;
            SetDragArrowVisible(true);

            yield return MoveRectTo(
                secondaryVehicleRect,
                secondaryStartPosition,
                exitPos,
                carMoveDuration);
            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            if (endHoldDuration > 0f)
            {
                yield return WaitUnscaled(endHoldDuration);
            }

            SetVehicleImageVisible(secondaryVehicleRect, false);
            limitedArrowPulseActive = false;
            SetDragArrowVisible(false);

            // Primary pinned locked.
            primaryVehicleRect.anchoredPosition = primaryStartPosition;

            if (limitedFinalHoldDuration > 0f)
            {
                yield return WaitUnscaled(limitedFinalHoldDuration);
            }

            if (!isPlaying)
            {
                carLoopCoroutine = null;
                yield break;
            }

            // RESET
            primaryVehicleRect.anchoredPosition = primaryStartPosition;
            secondaryVehicleRect.anchoredPosition = secondaryStartPosition;
            SetVehicleImageVisible(primaryVehicleRect, true);
            SetVehicleImageVisible(secondaryVehicleRect, true);
            ApplyLimitedVehicleMovesAvailableState(2);
            SetArrowPulseOrigin(arrowBasePosition);

            if (loopDelay > 0f)
            {
                yield return WaitUnscaled(loopDelay);
            }
        }

        carLoopCoroutine = null;
    }

    private void ApplyLimitedVehicleMovesAvailableState(int remaining)
    {
        if (modifierText != null)
        {
            modifierText.text = Mathf.Max(0, remaining).ToString();
            modifierText.color = moveLimitNormalColor;
            if (modifierTextCached)
            {
                modifierText.rectTransform.localScale = modifierTextBaseScale;
            }

            modifierText.enabled = true;
            modifierText.gameObject.SetActive(true);
        }

        SetModifierIconLeafVisible(false);
    }

    private void ApplyLimitedVehicleLockedState()
    {
        if (modifierText != null)
        {
            modifierText.gameObject.SetActive(false);
        }

        SetModifierIconLeafVisible(true);
        if (modifierRect != null && modifierIconScaleCached)
        {
            modifierRect.localScale = modifierIconBaseScale;
        }
    }

    private void SetModifierIconLeafVisible(bool visible)
    {
        if (modifierRect == null)
        {
            return;
        }

        Image iconImage = modifierRect.GetComponent<Image>();
        if (visible)
        {
            if (iconImage != null)
            {
                iconImage.enabled = true;
            }

            modifierRect.gameObject.SetActive(true);
            return;
        }

        modifierRect.gameObject.SetActive(false);
    }

    private void RestoreLimitedVehicleModifierLeavesToSafeBase()
    {
        // Visibility ownership blijft bij controller ApplyPresentation.
        // Hier alleen lock-pop scale opruimen.
        if (modifierRect != null && modifierIconScaleCached)
        {
            modifierRect.localScale = modifierIconBaseScale;
        }
    }

    private IEnumerator PopModifierIconOnce()
    {
        if (modifierRect == null || !modifierIconScaleCached)
        {
            yield break;
        }

        Vector3 baseScale = modifierIconBaseScale;
        Vector3 peakScale = baseScale * Mathf.Max(1.01f, limitedLockPopScale);
        float half = Mathf.Max(0.02f, limitedLockPopDuration) * 0.5f;

        float elapsed = 0f;
        while (elapsed < half)
        {
            if (!isPlaying)
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            modifierRect.localScale = Vector3.LerpUnclamped(baseScale, peakScale, EaseSmoothStep(t));
            yield return null;
        }

        modifierRect.localScale = peakScale;
        elapsed = 0f;
        while (elapsed < half)
        {
            if (!isPlaying)
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            modifierRect.localScale = Vector3.LerpUnclamped(peakScale, baseScale, EaseSmoothStep(t));
            yield return null;
        }

        modifierRect.localScale = baseScale;
    }

    private void PinNoTouchProtectedPosition()
    {
        if (primaryVehicleRect != null && primaryStartCached)
        {
            primaryVehicleRect.anchoredPosition = primaryStartPosition;
        }
    }

    private void UpdateNoTouchProtectedPulse()
    {
        if (noTouchPulseStrength <= 0.0001f)
        {
            return;
        }

        float wave = Mathf.Sin(Time.unscaledTime * noTouchProtectedPulseSpeed * Mathf.PI * 2f);
        float pulse01 = 0.5f + 0.5f * wave;
        float amount = Mathf.Clamp01(noTouchPulseStrength) * pulse01;

        if (protectedImageCached && primaryVehicleRect != null)
        {
            Image protectedImage = primaryVehicleRect.GetComponent<Image>();
            if (protectedImage != null)
            {
                protectedImage.color = Color.Lerp(
                    protectedImageBaseColor,
                    noTouchProtectedPulseColor,
                    amount);
            }

            float scaleMul = Mathf.Lerp(1f, Mathf.Max(1.01f, noTouchProtectedPulseScale), amount);
            primaryVehicleRect.localScale = protectedImageBaseScale * scaleMul;
        }

        if (modifierRect != null && modifierIconScaleCached)
        {
            float iconMul = Mathf.Lerp(1f, Mathf.Max(1.01f, noTouchIconPulseScale), amount);
            modifierRect.localScale = modifierIconBaseScale * iconMul;
        }
    }

    private void RestoreNoTouchProtectedAppearance()
    {
        if (!protectedImageCached || primaryVehicleRect == null)
        {
            return;
        }

        Image protectedImage = primaryVehicleRect.GetComponent<Image>();
        if (protectedImage != null)
        {
            protectedImage.color = protectedImageBaseColor;
        }

        primaryVehicleRect.localScale = protectedImageBaseScale;
    }

    private void PositionArrowFor(RectTransform vehicle)
    {
        if (vehicle == null)
        {
            return;
        }

        if (vehicle == secondaryVehicleRect && secondaryStartCached)
        {
            SetArrowPulseOrigin(secondaryStartPosition + secondaryArrowOffset);
            return;
        }

        if (vehicle == primaryVehicleRect && primaryStartCached)
        {
            SetArrowPulseOrigin(primaryStartPosition + primaryArrowOffset);
            return;
        }

        SetArrowPulseOrigin(vehicle.anchoredPosition);
    }

    private static void BringVehicleToFront(RectTransform vehicle)
    {
        if (vehicle != null)
        {
            vehicle.SetAsLastSibling();
        }
    }

    private void RestoreSiblingOrder()
    {
        if (!siblingOrderCached)
        {
            return;
        }

        // Lagere index eerst zetten zodat beide eindigen op gecachte volgorde.
        if (primarySiblingIndex <= secondarySiblingIndex)
        {
            if (primaryVehicleRect != null)
            {
                primaryVehicleRect.SetSiblingIndex(primarySiblingIndex);
            }

            if (secondaryVehicleRect != null)
            {
                secondaryVehicleRect.SetSiblingIndex(secondarySiblingIndex);
            }
        }
        else
        {
            if (secondaryVehicleRect != null)
            {
                secondaryVehicleRect.SetSiblingIndex(secondarySiblingIndex);
            }

            if (primaryVehicleRect != null)
            {
                primaryVehicleRect.SetSiblingIndex(primarySiblingIndex);
            }
        }
    }

    private void ApplyMultiTargetRemainingText(int remaining)
    {
        if (modifierText == null)
        {
            return;
        }

        int clamped = Mathf.Clamp(remaining, 0, 2);
        modifierText.text = clamped + " / 2";
    }

    private IEnumerator MoveRectTo(
        RectTransform rect,
        Vector2 start,
        Vector2 end,
        float duration)
    {
        if (rect == null)
        {
            yield break;
        }

        float moveDuration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;
        rect.anchoredPosition = start;

        while (elapsed < moveDuration)
        {
            if (!isPlaying)
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            rect.anchoredPosition = Vector2.LerpUnclamped(start, end, EaseSmoothStep(t));
            yield return null;
        }

        rect.anchoredPosition = end;
    }

    private static float EaseSmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private static void SetVehicleImageVisible(RectTransform rect, bool visible)
    {
        if (rect == null)
        {
            return;
        }

        Image image = rect.GetComponent<Image>();
        if (image != null)
        {
            image.enabled = visible;
            return;
        }

        rect.gameObject.SetActive(visible);
    }

    /// <summary>
    /// Maps move progress to demo values start → … → end.
    /// Uses moveLimitDemoStepDuration as spacing hint within the move.
    /// </summary>
    private int EvaluateMoveLimitDemoValue(float elapsed, float moveDuration)
    {
        int startValue = Mathf.Max(moveLimitDemoEnd, moveLimitDemoStart);
        int endValue = Mathf.Min(startValue, moveLimitDemoEnd);

        float stepDuration = Mathf.Max(0.05f, moveLimitDemoStepDuration);
        int stepped = Mathf.FloorToInt(elapsed / stepDuration);
        int value = startValue - stepped;
        return Mathf.Clamp(value, endValue, startValue);
    }

    private void ApplyMoveLimitCounter(int value, bool forceWarning)
    {
        if (modifierText == null)
        {
            return;
        }

        moveLimitDisplayedValue = value;
        modifierText.text = value.ToString();

        bool warning = forceWarning || value <= moveLimitDemoEnd;
        modifierText.color = warning ? moveLimitWarningColor : moveLimitNormalColor;

        if (!warning && modifierTextCached)
        {
            modifierText.rectTransform.localScale = modifierTextBaseScale;
        }
    }

    /// <summary>
    /// FragileCargo demo counter. Warning styling hergebruikt MoveLimit colors.
    /// </summary>
    private void ApplyFragileCargoCounter(int remaining)
    {
        if (modifierText == null)
        {
            return;
        }

        int clamped = Mathf.Max(0, remaining);
        fragileCargoDisplayedValue = clamped;
        modifierText.text = clamped.ToString();

        bool warning = clamped <= 1;
        modifierText.color = warning ? moveLimitWarningColor : moveLimitNormalColor;

        if (!warning && modifierTextCached)
        {
            modifierText.rectTransform.localScale = modifierTextBaseScale;
        }
    }

    private static IEnumerator WaitUnscaled(float seconds)
    {
        float remaining = Mathf.Max(0f, seconds);
        while (remaining > 0f)
        {
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void SetDragArrowVisible(bool visible)
    {
        if (dragArrowRoot != null)
        {
            dragArrowRoot.SetActive(visible);
            return;
        }

        if (dragArrowRect != null)
        {
            dragArrowRect.gameObject.SetActive(visible);
        }
    }
}

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// First-time objective tutorial overlay. Presentation/orchestration only.
/// Toont na Special Mission Intro (specials) of na scene fade (Classic).
/// Markeert PlayerPrefs pas bij Got It.
/// </summary>
public class ObjectiveTutorialController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LevelObjectiveController objectiveController;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private ObjectiveTutorialLibrary tutorialLibrary;

    [Header("Overlay UI (scene refs — geen runtime create)")]
    [SerializeField] private GameObject tutorialOverlay;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Button gotItButton;

    [Header("Optional illustration placeholders")]
    [SerializeField] private Image primaryVehicleImage;
    [SerializeField] private Image secondaryVehicleImage;
    [SerializeField] private Image exitIcon;
    [SerializeField] private Image modifierIcon;
    [SerializeField] private TextMeshProUGUI modifierText;

    [Header("Tutorial Animation (optional)")]
    [SerializeField] private ObjectiveTutorialAnimation tutorialAnimation;

    public event Action OnObjectiveTutorialCompleted;

    private Coroutine showCoroutine;
    private int handledSessionId = -1;
    private int presentationGeneration;
    private bool tutorialUiActive;
    private bool isClosing;
    private bool gotItListenerBound;
    private LevelObjectiveType activeObjectiveType = LevelObjectiveType.Classic;

    public bool IsTutorialUiActive => tutorialUiActive;

    /// <summary>
    /// Bestaande ModifierIcon RectTransform (zelfde UI als library showModifier).
    /// </summary>
    public RectTransform ModifierRect =>
        modifierIcon != null ? modifierIcon.rectTransform : null;

    /// <summary>
    /// Bestaande PrimaryVehicleImage RectTransform.
    /// </summary>
    public RectTransform PrimaryVehicleRect =>
        primaryVehicleImage != null ? primaryVehicleImage.rectTransform : null;

    /// <summary>
    /// Bestaande SecondaryVehicleImage RectTransform (MultiTargetRescue).
    /// </summary>
    public RectTransform SecondaryVehicleRect =>
        secondaryVehicleImage != null ? secondaryVehicleImage.rectTransform : null;

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

        BindGotItListener();
        CloseTutorialPresentation();
    }

    private void OnEnable()
    {
        BindGotItListener();
    }

    private void OnDisable()
    {
        UnbindGotItListener();
        CloseTutorialPresentation();
    }

    private void OnDestroy()
    {
        UnbindGotItListener();
    }

    private void LateUpdate()
    {
        if (objectiveController == null || isClosing)
        {
            return;
        }

        bool waiting = objectiveController.IsWaitingForObjectiveTutorial;
        int sessionId = objectiveController.ObjectiveTutorialSessionId;

        // Session-wissel terwijl UI open/in-flight is: altijd eerst volledig teardown.
        if (sessionId != handledSessionId)
        {
            if (showCoroutine != null || tutorialUiActive || IsOverlayActive())
            {
                CloseTutorialPresentation();
            }
        }

        if (!waiting || sessionId == handledSessionId || showCoroutine != null || tutorialUiActive)
        {
            return;
        }

        LevelObjectiveType objectiveType = ResolveCurrentObjectiveType();
        if (ObjectiveTutorialPrefs.HasSeenTutorial(objectiveType))
        {
            LogTutorialLifecycle(
                "SkipShowAlreadySeen",
                objectiveType,
                sessionId,
                presentationGeneration);
            objectiveController.NotifyObjectiveTutorialCompleted();
            handledSessionId = sessionId;
            return;
        }

        handledSessionId = sessionId;
        int generation = presentationGeneration;
        LogTutorialLifecycle("ShowTutorial", objectiveType, sessionId, generation);
        showCoroutine = StartCoroutine(ShowTutorialWhenReady(sessionId, generation));
    }

    /// <summary>
    /// Authoritative Got It handler (code listener only — geen Inspector OnClick).
    /// </summary>
    public void OnGotItClicked()
    {
        bool overlayVisible = IsOverlayActive();
        if (isClosing || (!tutorialUiActive && !overlayVisible))
        {
            LogTutorialLifecycle(
                "GotItIgnored",
                activeObjectiveType,
                handledSessionId,
                presentationGeneration);
            return;
        }

        isClosing = true;

        LogTutorialLifecycle(
            "GotIt",
            activeObjectiveType,
            handledSessionId,
            presentationGeneration);

        // 1) Mark seen EERST zodat eligibility-checks niet opnieuw openen.
        ObjectiveTutorialPrefs.MarkTutorialSeen(activeObjectiveType);

        // 2) Invalideer in-flight show + stop presentation.
        presentationGeneration++;
        CloseTutorialPresentation();

        // 3) Release gate pas NA mark + close.
        if (objectiveController != null)
        {
            objectiveController.NotifyObjectiveTutorialCompleted();
        }
        else if (gameManager != null)
        {
            gameManager.SetObjectiveTutorialBlocked(false);
        }

        LogTutorialLifecycle(
            "NotifyObjectiveTutorialCompleted",
            activeObjectiveType,
            handledSessionId,
            presentationGeneration);

        OnObjectiveTutorialCompleted?.Invoke();
        isClosing = false;
    }

    private IEnumerator ShowTutorialWhenReady(int sessionId, int generation)
    {
        // 1) Scene fade klaar.
        while (SceneTransition.IsTransitioning)
        {
            if (!StillCurrentShow(sessionId, generation))
            {
                showCoroutine = null;
                yield break;
            }

            yield return null;
        }

        // 2) Special intro klaar (Classic: IsWaitingForSpecialIntro = false).
        while (objectiveController != null &&
               objectiveController.IsWaitingForSpecialIntro)
        {
            if (!StillCurrentShow(sessionId, generation))
            {
                showCoroutine = null;
                yield break;
            }

            yield return null;
        }

        if (!StillCurrentShow(sessionId, generation))
        {
            showCoroutine = null;
            yield break;
        }

        // Een frame: HUD/intro restore stabiel.
        yield return null;

        if (!StillCurrentShow(sessionId, generation))
        {
            showCoroutine = null;
            yield break;
        }

        LevelObjectiveType objectiveType = ResolveCurrentObjectiveType();
        activeObjectiveType = objectiveType;

        // Al gezien (race): gate vrijgeven zonder flash.
        if (ObjectiveTutorialPrefs.HasSeenTutorial(objectiveType))
        {
            objectiveController?.NotifyObjectiveTutorialCompleted();
            showCoroutine = null;
            yield break;
        }

        if (!StillCurrentShow(sessionId, generation) || tutorialUiActive)
        {
            showCoroutine = null;
            yield break;
        }

        TMP_Text resolvedModifierText = ResolveModifierText();
        Image modifierImage = ResolveModifierImage();
        Image primaryImage = ResolvePrimaryImage();
        Image secondaryImage = ResolveSecondaryImage();

        if (tutorialAnimation != null)
        {
            if (primaryImage != null)
            {
                tutorialAnimation.SetPrimaryVehicleRect(primaryImage.rectTransform);
            }

            if (secondaryImage != null)
            {
                tutorialAnimation.SetSecondaryVehicleRect(secondaryImage.rectTransform);
            }

            if (modifierImage != null)
            {
                tutorialAnimation.SetModifierRect(modifierImage.rectTransform);
            }

            tutorialAnimation.SetModifierText(resolvedModifierText);
        }

        // Ownership: overlay visibility alleen via SetOverlayVisible — niet via EnsureActive.
        tutorialUiActive = true;
        ApplyPresentation(objectiveType);
        LogModifierTextDebug(objectiveType);

        if (!StillCurrentShow(sessionId, generation))
        {
            CloseTutorialPresentation();
            showCoroutine = null;
            yield break;
        }

        SetOverlayVisible(true);

        if (gameManager != null)
        {
            gameManager.SetObjectiveTutorialBlocked(true);
        }

        if (tutorialAnimation != null)
        {
            tutorialAnimation.Play(objectiveType);
            if (!StillCurrentShow(sessionId, generation))
            {
                CloseTutorialPresentation();
                showCoroutine = null;
                yield break;
            }

            ApplyPresentation(objectiveType);
        }

        showCoroutine = null;
    }

    /// <summary>
    /// Definitieve presentation-close. Geen aparte alpha/SetActive paden.
    /// </summary>
    private void CloseTutorialPresentation()
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        tutorialAnimation?.StopAndReset();
        SetOverlayVisible(false);
        tutorialUiActive = false;

        LogTutorialLifecycle(
            "CloseTutorial",
            activeObjectiveType,
            handledSessionId,
            presentationGeneration);
    }

    private void BindGotItListener()
    {
        if (gotItButton == null)
        {
            return;
        }

        // Idempotent: voorkomt gestapelde listeners (domain reload / OnEnable).
        gotItButton.onClick.RemoveListener(OnGotItClicked);
        gotItButton.onClick.AddListener(OnGotItClicked);
        gotItListenerBound = true;
    }

    private void UnbindGotItListener()
    {
        if (gotItButton == null || !gotItListenerBound)
        {
            return;
        }

        gotItButton.onClick.RemoveListener(OnGotItClicked);
        gotItListenerBound = false;
    }

    private bool IsOverlayActive()
    {
        return tutorialOverlay != null && tutorialOverlay.activeInHierarchy;
    }

    private bool StillCurrentShow(int sessionId, int generation)
    {
        return !isClosing &&
               generation == presentationGeneration &&
               objectiveController != null &&
               objectiveController.IsWaitingForObjectiveTutorial &&
               objectiveController.ObjectiveTutorialSessionId == sessionId;
    }

    private void ApplyPresentation(LevelObjectiveType objectiveType)
    {
        ObjectiveTutorialEntry entry =
            tutorialLibrary != null ? tutorialLibrary.GetEntry(objectiveType) : null;

        string title = entry != null && !string.IsNullOrEmpty(entry.title)
            ? entry.title
            : objectiveType.ToString();
        string instruction = entry != null ? entry.instruction : string.Empty;

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (instructionText != null)
        {
            instructionText.text = instruction ?? string.Empty;
        }

        Sprite primarySprite = entry != null ? entry.primarySprite : null;
        Sprite secondarySprite = entry != null ? entry.secondarySprite : null;
        Sprite modifierSprite = entry != null ? entry.modifierSprite : null;
        bool showSecondary = entry != null && entry.showSecondary;
        bool showModifier = entry != null && entry.showModifier;
        string libraryModifierText = entry != null ? (entry.modifierText ?? string.Empty) : string.Empty;

        Image primaryImage = ResolvePrimaryImage();
        Image secondaryImage = ResolveSecondaryImage();
        Image modifierImage = ResolveModifierImage();
        TMP_Text resolvedModifierText = ResolveModifierText();

        ApplyOptionalImage(primaryImage, primarySprite, visible: true);
        ApplyOptionalImage(secondaryImage, secondarySprite, visible: showSecondary);

        bool showModifierIcon = showModifier && modifierSprite != null;
        bool showModifierLabel = showModifier && !string.IsNullOrWhiteSpace(libraryModifierText);

        // LimitedVehicle demo: lock-sprite toewijzen, maar start met text ON / icon OFF.
        // Animation toont lock pas na 2→1→LOCK.
        if (objectiveType == LevelObjectiveType.LimitedVehicle)
        {
            if (modifierImage != null && modifierSprite != null)
            {
                modifierImage.sprite = modifierSprite;
                modifierImage.preserveAspect = true;
                modifierImage.enabled = true;
            }

            showModifierIcon = false;
        }

        ApplyModifierIconVisibility(modifierImage, modifierSprite, showModifierIcon, resolvedModifierText);
        ApplyModifierTextVisibility(resolvedModifierText, libraryModifierText, showModifierLabel);

        if (tutorialAnimation != null)
        {
            if (primaryImage != null)
            {
                tutorialAnimation.SetPrimaryVehicleRect(primaryImage.rectTransform);
            }

            if (secondaryImage != null)
            {
                tutorialAnimation.SetSecondaryVehicleRect(secondaryImage.rectTransform);
            }

            // LimitedVehicle: icon-ref altijd doorgeven (ook als leaf start-hidden).
            if (modifierImage != null &&
                (showModifierIcon || objectiveType == LevelObjectiveType.LimitedVehicle))
            {
                tutorialAnimation.SetModifierRect(modifierImage.rectTransform);
            }

            tutorialAnimation.SetModifierText(resolvedModifierText);
        }
    }

    private TMP_Text ResolveModifierText()
    {
        if (modifierText != null)
        {
            return modifierText;
        }

        return tutorialAnimation != null ? tutorialAnimation.ResolveModifierText() : null;
    }

    private Image ResolvePrimaryImage()
    {
        if (primaryVehicleImage != null)
        {
            return primaryVehicleImage;
        }

        return tutorialAnimation != null ? tutorialAnimation.ResolvePrimaryImage() : null;
    }

    private Image ResolveSecondaryImage()
    {
        if (secondaryVehicleImage != null)
        {
            return secondaryVehicleImage;
        }

        return tutorialAnimation != null ? tutorialAnimation.ResolveSecondaryImage() : null;
    }

    private Image ResolveModifierImage()
    {
        if (modifierIcon != null)
        {
            return modifierIcon;
        }

        return tutorialAnimation != null ? tutorialAnimation.ResolveModifierImage() : null;
    }

    private void ApplyOptionalImage(Image image, Sprite sprite, bool visible)
    {
        if (image == null)
        {
            return;
        }

        if (visible)
        {
            if (sprite != null)
            {
                image.sprite = sprite;
            }

            image.enabled = true;
            image.preserveAspect = true;
            EnsureActiveUnderOverlay(image.gameObject);
            image.gameObject.SetActive(true);
            return;
        }

        image.gameObject.SetActive(false);
    }

    private void ApplyModifierIconVisibility(
        Image image,
        Sprite sprite,
        bool visible,
        TMP_Text relatedText)
    {
        if (image == null)
        {
            return;
        }

        if (visible)
        {
            if (sprite != null)
            {
                image.sprite = sprite;
            }

            image.enabled = true;
            image.preserveAspect = true;
            EnsureActiveUnderOverlay(image.gameObject);
            image.gameObject.SetActive(true);
            return;
        }

        image.enabled = false;
        bool textIsDescendant = relatedText != null &&
            relatedText.transform != image.transform &&
            relatedText.transform.IsChildOf(image.transform);

        if (textIsDescendant)
        {
            EnsureActiveUnderOverlay(image.gameObject);
            image.gameObject.SetActive(true);
            return;
        }

        image.gameObject.SetActive(false);
    }

    private void ApplyModifierTextVisibility(TMP_Text text, string content, bool visible)
    {
        if (text == null)
        {
            return;
        }

        if (!visible)
        {
            text.gameObject.SetActive(false);
            return;
        }

        text.text = content ?? string.Empty;
        text.enabled = true;
        EnsureActiveUnderOverlay(text.gameObject);
        text.gameObject.SetActive(true);
    }

    /// <summary>
    /// Activeert inactive parents onder de overlay, maar NOOIT ObjectiveTutorialOverlay zelf.
    /// Overlay visibility = uitsluitend SetOverlayVisible / CloseTutorialPresentation.
    /// </summary>
    private void EnsureActiveUnderOverlay(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        Transform current = target.transform.parent;
        while (current != null)
        {
            if (current.GetComponent<Canvas>() != null)
            {
                break;
            }

            if (tutorialOverlay != null && current.gameObject == tutorialOverlay)
            {
                break;
            }

            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
            }

            current = current.parent;
        }
    }

    private void LogModifierTextDebug(LevelObjectiveType objectiveType)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (objectiveType != LevelObjectiveType.MoveLimit)
        {
            return;
        }

        ObjectiveTutorialEntry entry =
            tutorialLibrary != null ? tutorialLibrary.GetEntry(objectiveType) : null;
        TMP_Text resolved = ResolveModifierText();
        TMP_Text animText = tutorialAnimation != null
            ? tutorialAnimation.ResolveModifierText()
            : null;

        bool sameObject = resolved != null && animText != null &&
            ReferenceEquals(resolved, animText);

        float alpha = -1f;
        float scale = -1f;
        if (resolved != null)
        {
            alpha = resolved.color.a;
            scale = resolved.rectTransform.localScale.x;
        }

        Debug.Log(
            "[ModifierTextDebug]\n" +
            "Objective=" + objectiveType + "\n" +
            "ControllerText=" + (modifierText != null ? modifierText.gameObject.name : "null") + "\n" +
            "AnimationText=" + (animText != null ? animText.gameObject.name : "null") + "\n" +
            "SameObject=" + sameObject + "\n" +
            "ActiveSelf=" + (resolved != null && resolved.gameObject.activeSelf) + "\n" +
            "ActiveInHierarchy=" + (resolved != null && resolved.gameObject.activeInHierarchy) + "\n" +
            "Text=" + (resolved != null ? resolved.text : "null") + "\n" +
            "Alpha=" + alpha + "\n" +
            "Scale=" + scale + "\n" +
            "ShowModifier=" + (entry != null && entry.showModifier) + "\n" +
            "LibraryModifierText=" + (entry != null ? (entry.modifierText ?? string.Empty) : "null")
        );
#endif
    }

    private void LogTutorialLifecycle(
        string phase,
        LevelObjectiveType objectiveType,
        int sessionId,
        int generation)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[ObjectiveTutorialLifecycle] " + phase +
            " objective=" + objectiveType +
            " sessionId=" + sessionId +
            " generation=" + generation +
            " tutorialUiActive=" + tutorialUiActive +
            " isClosing=" + isClosing +
            " overlayActive=" + IsOverlayActive()
        );
#endif
    }

    private LevelObjectiveType ResolveCurrentObjectiveType()
    {
        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        LevelData levelData = levelManager != null ? levelManager.CurrentLevelData : null;
        if (levelData == null)
        {
            return LevelObjectiveType.Classic;
        }

        return levelData.objectiveType;
    }

    private void SetOverlayVisible(bool visible)
    {
        if (tutorialOverlay != null)
        {
            tutorialOverlay.SetActive(visible);
        }
    }
}

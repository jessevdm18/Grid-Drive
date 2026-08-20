using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Feature tutorials: Coins / Skins.
/// Modal (Coins/Skins Step2) vs PointerOnly (Skins Step1) — presentation only.
/// </summary>
public class FeatureTutorialController : MonoBehaviour
{
    private enum FeatureTutorialPresentationMode
    {
        Modal,
        PointerOnly
    }

    [Header("Refs (optioneel per scene)")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private ObjectiveTutorialController objectiveTutorialController;
    [SerializeField] private ShopUIController shopUI;

    [Header("Modal UI (Coins / Skins)")]
    [SerializeField] private GameObject featureTutorialOverlay;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI secondaryText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button gotItButton;

    [Header("Shared pointer (Skins Step1)")]
    [SerializeField] private RectTransform arrow;

    [Header("Spotlight targets (bestaande UI)")]
    [SerializeField] private RectTransform coinCounterTarget;
    [SerializeField] private RectTransform shopButtonTarget;

    [Header("Coins copy")]
    [SerializeField] private string coinsTitle = "COINS";
    [SerializeField] private string coinsPrimary = "GET 3 STARS TO EARN COINS";
    [SerializeField] private string coinsSecondary = "USE COINS FOR HINTS AND SKINS";

    [Header("Skins (MainMenu two-step)")]
    [SerializeField] private string skinsTitle = "UNLOCK NEW SKINS";
    [SerializeField] private string skinsPrimary =
        "USE YOUR COINS TO BUY NEW VEHICLE SKINS.";
    [SerializeField] private string skinsSecondary =
        "EARN MORE COINS BY GETTING 3 STARS.";
    [Tooltip("MainMenu: enable Skins Step1 pointer + Step2 shop modal.")]
    [SerializeField] private bool enableSkinsTutorialOnThisScene = false;

    [Header("Modal timing")]
    [SerializeField, Min(0f)] private float modalShowDelaySeconds = 0.25f;
    [SerializeField, Min(0.1f)] private float modalPulsePeriodSeconds = 0.9f;

    [Header("Pointer Tutorials")]
    [SerializeField, Min(0.5f)] private float pointerDisplayDuration = 3.0f;
    [Tooltip("Offset vanaf target-center in arrow.parent local space. Links van knop: bijv. (-80, 0).")]
    [SerializeField] private Vector2 pointerTargetOffset = new Vector2(-80f, 0f);
    [SerializeField, Min(0f)] private float pointerHorizontalMoveDistance = 12f;
    [SerializeField, Min(0.1f)] private float pointerHorizontalMoveSpeed = 2f;
    [SerializeField] private bool pointerScalePulseEnabled = true;
    [SerializeField, Min(1f)] private float pointerPulseScale = 1.06f;

    private int presentationGeneration;
    private bool modalActive;
    private bool pointerActive;
    private bool isClosing;
    private bool gotItListenerBound;
    private bool targetClickBound;

    private bool pendingCoins;
    private bool pendingSkinsStep1;
    private bool pendingSkinsStep2;

    /// <summary>Max één Shop-pointer per MainMenu-visit.</summary>
    private bool skinsPointerShownThisVisit;

    /// <summary>Runtime spotlight override (buyable skin card during Step 2).</summary>
    private RectTransform skinsStep2Spotlight;

    private FeatureTutorialType activeType = FeatureTutorialType.Coins;
    private FeatureTutorialPresentationMode activeMode =
        FeatureTutorialPresentationMode.Modal;

    private Coroutine showCoroutine;
    private Coroutine pointerLifetimeCoroutine;
    private Coroutine pulseCoroutine;
    private Coroutine pointerPresentationCoroutine;

    private RectTransform pulseTarget;
    private Vector3 pulseBaseScale = Vector3.one;
    private bool pulseBaseCached;

    private Vector2 arrowBaseAnchoredPos;
    private Vector3 arrowBaseLocalScale = Vector3.one;
    private Quaternion arrowBaseLocalRotation = Quaternion.identity;
    private bool arrowTransformCached;
    private bool arrowSceneRotationCached;
    private Quaternion arrowSceneLocalRotation = Quaternion.identity;

    private Button boundTargetButton;
    private CanvasGroup overlayCanvasGroup;
    private bool loggedMissingOverlay;
    private bool loggedInactiveHierarchy;
    private bool loggedRefValidation;
    private bool winFinishedHandledThisSequence;
    private string lastPendingBlockReason;

    /// <summary>Editor/dev trace session voor één CompleteLevel→modal flow.</summary>
    public static int CoinsFtSessionId { get; private set; }

    /// <summary>True terwijl modal of pointer actief is (stacking).</summary>
    public bool IsTutorialUiActive => modalActive || pointerActive;

    /// <summary>
    /// True while a tutorial modal is queued, delayed, or visible.
    /// Used so Try-difficulty navigation waits for Coins FT after win.
    /// </summary>
    public bool IsBlockingModalPresentation =>
        modalActive || pendingCoins || showCoroutine != null;

    private void Awake()
    {
        ResolveRefs();
        EnsureControllerStaysAlive();
        ValidateCoinsModalRefs();
        BindGotItListener();
        SubscribeEvents();
        ForceHideAllPresentation(releaseGate: true);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[CoinsFT]\n" +
            "Stage=ControllerAwake\n" +
            "ControllerObject=" + name + "\n" +
            "ActiveSelf=" + gameObject.activeSelf + "\n" +
            "ActiveInHierarchy=" + gameObject.activeInHierarchy + "\n" +
            "OverlayRef=" + (featureTutorialOverlay != null
                ? featureTutorialOverlay.name
                : "null") + "\n" +
            "OverlayIsSelf=" + (featureTutorialOverlay == gameObject) + "\n" +
            "Parent=" + (transform.parent != null ? transform.parent.name : "null")
        );
#endif
    }

    private void OnEnable()
    {
        ResolveRefs();
        BindGotItListener();
        SubscribeEvents();
    }

    private void Start()
    {
        if (!enableSkinsTutorialOnThisScene)
        {
            return;
        }

        TryBeginSkinsOnboardingForThisVisit();
    }

    private void OnDisable()
    {
        // Niet unsubscriben als we alleen CanvasGroup-hide doen op gedeelde overlay —
        // OnDisable mag niet vuren. Als parent écht uit gaat: pending blijft via GameManager trigger.
        UnsubscribeEvents();
        UnbindGotItListener();
        UnbindTargetClick();
        ForceHideAllPresentation(releaseGate: true);
        // Coins komt terug via GameManager trigger.
        // Skins Step2 pending behouden niet over disable — visit is klaar.
        pendingSkinsStep1 = false;
        pendingSkinsStep2 = false;
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
        UnbindGotItListener();
        UnbindTargetClick();
    }

    private void LateUpdate()
    {
        if (isClosing)
        {
            return;
        }

        if (gameManager != null && !gameManager.IsLevelCompleted)
        {
            winFinishedHandledThisSequence = false;
        }

        // Win afgerond maar queue gemist (bijv. controller was inactive): alsnog oppakken.
        if (!modalActive &&
            !pointerActive &&
            !pendingCoins &&
            gameManager != null &&
            gameManager.HasPendingCoinsFeatureTutorialTrigger &&
            !FeatureTutorialPrefs.HasSeen(FeatureTutorialType.Coins))
        {
            TryQueueFeatureTutorial(FeatureTutorialType.Coins);
        }

        if (modalActive || pointerActive)
        {
            if (!IsActivePresentationStillAllowed())
            {
                HideActivePresentation(markSeen: false, releaseGate: true);
                return;
            }

            // Modal: bestaande arrow-rotation naar spotlight.
            // Pointer: plaats/bounce zit in pointerPresentationCoroutine.
            if (modalActive)
            {
                UpdateArrowTowardSpotlight();
            }

            return;
        }

        TryOpenNextPending();
    }

    /// <summary>
    /// Aangeroepen door UIManager na win-sequence (event + directe call).
    /// Vindt ook inactive controllers.
    /// </summary>
    public static void NotifyWinSequenceFinished()
    {
        FeatureTutorialController[] controllers =
            FindObjectsByType<FeatureTutorialController>(
                FindObjectsInactive.Include);

        if (controllers == null || controllers.Length == 0)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(
                "[CoinsFT] Stage=NoFeatureTutorialControllerInScene Session=" +
                CoinsFtSessionId
            );
#endif
            return;
        }

        for (int i = 0; i < controllers.Length; i++)
        {
            controllers[i]?.HandleWinSequenceFinished();
        }
    }

    /// <summary>Editor: toon Coins modal zonder coins/prefs te wijzigen (Got It markeert wel).</summary>
    public void EditorShowCoinsTutorialNow()
    {
        EnsureControllerStaysAlive();
        if (!isActiveAndEnabled)
        {
            gameObject.SetActive(true);
        }

        pendingCoins = true;
        if (CanOpen(FeatureTutorialType.Coins))
        {
            pendingCoins = false;
            BeginShow(FeatureTutorialType.Coins, FeatureTutorialPresentationMode.Modal);
        }
    }

    private void HandleWinSequenceFinished()
    {
        if (winFinishedHandledThisSequence)
        {
            return;
        }

        winFinishedHandledThisSequence = true;

        bool rewardGranted =
            gameManager != null &&
            (gameManager.LastThreeStarCoinRewardGranted ||
             gameManager.HasPendingCoinsFeatureTutorialTrigger);

        bool coinsSeen = FeatureTutorialPrefs.HasSeen(FeatureTutorialType.Coins);
        string block = GetPresentBlockReason(
            FeatureTutorialType.Coins,
            ignoreShowInFlight: false);
        bool canPresent = block == null;

        EnsureControllerStaysAlive();

        bool queued = false;
        if (rewardGranted && !coinsSeen)
        {
            queued = TryQueueFeatureTutorial(FeatureTutorialType.Coins);
            if (queued && gameManager != null)
            {
                gameManager.TryConsumeCoinsFeatureTutorialTrigger();
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[CoinsFT]\n" +
            "Stage=FeatureControllerReceivedWinFinished\n" +
            "Session=" + CoinsFtSessionId + "\n" +
            "ControllerObject=" + name + "\n" +
            "ControllerActive=" + gameObject.activeInHierarchy + "\n" +
            "ControllerEnabled=" + enabled + "\n" +
            "CoinsSeen=" + coinsSeen + "\n" +
            "RewardGranted=" + rewardGranted + "\n" +
            "FeatureTutorialOverlay=" +
            (featureTutorialOverlay != null ? featureTutorialOverlay.name : "null") + "\n" +
            "PendingFeature=" + (pendingCoins ? "Coins" : "None") + "\n" +
            "CurrentFeature=" + (modalActive || pointerActive
                ? activeType.ToString()
                : "None") + "\n" +
            "CanPresent=" + canPresent + "\n" +
            "BlockReason=" + (block ?? "None") + "\n" +
            "Queued=" + queued
        );

        if (!isActiveAndEnabled && !loggedInactiveHierarchy)
        {
            loggedInactiveHierarchy = true;
            Debug.LogError(
                "[CoinsFT] Controller inactive. Verplaats FeatureTutorialController " +
                "naar een altijd-actief object (niet child van inactive overlay)."
            );
        }
#endif

        if (pendingCoins)
        {
            TryOpenNextPending();
        }
    }

    /// <summary>Start een nieuwe CoinsFT trace-session (aangeroepen vanuit CompleteLevel).</summary>
    public static void BeginCoinsFtSession()
    {
        CoinsFtSessionId++;
    }

    public bool TryQueueFeatureTutorial(FeatureTutorialType type)
    {
        if (type == FeatureTutorialType.Undo ||
            type == FeatureTutorialType.Hint ||
            type == FeatureTutorialType.WatchAdForHint)
        {
            return false;
        }

        if (FeatureTutorialPrefs.HasSeen(type))
        {
            return false;
        }

        switch (type)
        {
            case FeatureTutorialType.Coins:
                pendingCoins = true;
                return true;
            case FeatureTutorialType.Skins:
                if (!IsSkinsEligible())
                {
                    return false;
                }

                // Queue Step 1 unless shop already open → Step 2.
                if (shopUI != null && shopUI.IsShopOpen)
                {
                    pendingSkinsStep2 = true;
                }
                else if (!skinsPointerShownThisVisit)
                {
                    pendingSkinsStep1 = true;
                }

                return true;
            default:
                return false;
        }
    }

    public void OnGotItClicked()
    {
        if (isClosing || !modalActive || activeMode != FeatureTutorialPresentationMode.Modal)
        {
            return;
        }

        isClosing = true;
        FeatureTutorialPrefs.MarkSeen(activeType);
        presentationGeneration++;
        HideActivePresentation(markSeen: false, releaseGate: true);
        isClosing = false;
    }

    private void TryOpenNextPending()
    {
        ClearSeenPendings();

        // Priority: Coins (modal) → Skins (Step2 modal / Step1 pointer)
        if (pendingCoins)
        {
            bool canPresent = CanOpen(FeatureTutorialType.Coins);

            if (canPresent)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log(
                    "[CoinsFT]\n" +
                    "Stage=CoinsQueued\n" +
                    "Session=" + CoinsFtSessionId + "\n" +
                    "CanPresent=true\n" +
                    "BlockReason=None"
                );
#endif
                lastPendingBlockReason = null;
                pendingCoins = false;
                BeginShow(
                    FeatureTutorialType.Coins,
                    FeatureTutorialPresentationMode.Modal);
                return;
            }

            LogCoinsFtPendingRecheck(
                FeatureTutorialType.Coins,
                ignoreShowInFlight: false);
        }

        // Skins Step 2 (shop modal) vóór Step 1 als beide pending.
        if (pendingSkinsStep2 &&
            IsSkinsEligible() &&
            shopUI != null &&
            shopUI.IsShopOpen &&
            CanOpen(FeatureTutorialType.Skins, ignoreShowInFlight: false))
        {
            pendingSkinsStep2 = false;
            BeginShow(
                FeatureTutorialType.Skins,
                FeatureTutorialPresentationMode.Modal);
            return;
        }

        if (pendingSkinsStep1 &&
            IsSkinsEligible() &&
            !skinsPointerShownThisVisit &&
            CanOpen(FeatureTutorialType.Skins, ignoreShowInFlight: false))
        {
            pendingSkinsStep1 = false;
            skinsPointerShownThisVisit = true;
            BeginShow(
                FeatureTutorialType.Skins,
                FeatureTutorialPresentationMode.PointerOnly);
            return;
        }

        if (enableSkinsTutorialOnThisScene &&
            IsSkinsEligible() &&
            !skinsPointerShownThisVisit &&
            !pendingSkinsStep1 &&
            !pendingSkinsStep2 &&
            !(shopUI != null && shopUI.IsShopOpen) &&
            IsPresentationClearForModal())
        {
            pendingSkinsStep1 = true;
        }
    }

    private void BeginShow(
        FeatureTutorialType type,
        FeatureTutorialPresentationMode mode)
    {
        int generation = presentationGeneration;
        showCoroutine = StartCoroutine(ShowAfterDelay(type, mode, generation));
    }

    private IEnumerator ShowAfterDelay(
        FeatureTutorialType type,
        FeatureTutorialPresentationMode mode,
        int generation)
    {
        float delay = mode == FeatureTutorialPresentationMode.Modal
            ? modalShowDelaySeconds
            : 0.05f;

        // BELANGRIJK: CanOpen(..., ignoreShowInFlight: true) —
        // showCoroutine IS deze routine; die mag zichzelf niet blokkeren.
        if (delay > 0f)
        {
            float elapsed = 0f;
            while (elapsed < delay)
            {
                if (generation != presentationGeneration)
                {
                    showCoroutine = null;
                    yield break;
                }

                if (!CanOpen(type, ignoreShowInFlight: true))
                {
                    LogCoinsFtPendingRecheck(type, ignoreShowInFlight: true);
                    Requeue(type);
                    showCoroutine = null;
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (generation != presentationGeneration ||
            !CanOpen(type, ignoreShowInFlight: true))
        {
            LogCoinsFtPendingRecheck(type, ignoreShowInFlight: true);
            Requeue(type);
            showCoroutine = null;
            yield break;
        }

        if (mode == FeatureTutorialPresentationMode.Modal)
        {
            OpenModal(type);
        }
        else
        {
            OpenPointer(type);
        }

        showCoroutine = null;
    }

    private void OpenModal(FeatureTutorialType type)
    {
        activeType = type;
        activeMode = FeatureTutorialPresentationMode.Modal;
        modalActive = true;
        pointerActive = false;

        if (featureTutorialOverlay == null && !loggedMissingOverlay)
        {
            loggedMissingOverlay = true;
            Debug.LogWarning(
                "FeatureTutorialController: featureTutorialOverlay is null — " +
                "Coins/Skins modal kan niet zichtbaar worden. Koppel de overlay in de Inspector."
            );
        }

        skinsStep2Spotlight = null;
        if (type == FeatureTutorialType.Skins)
        {
            PrepareSkinsStep2ShopContext();
        }

        ApplyModalCopy(type);
        SetModalOverlayVisible(true);

        // Boven WinPanel/Shop renderen (zelfde Canvas).
        if (featureTutorialOverlay != null)
        {
            featureTutorialOverlay.transform.SetAsLastSibling();
        }

        // MainMenu heeft geen board-input gate; Gameplay Coins wel.
        if (gameManager != null && type == FeatureTutorialType.Coins)
        {
            gameManager.SetFeatureTutorialBlocked(true);
        }

        RectTransform pulseTargetRect = ResolveSpotlightTarget(type);
        StartPulse(pulseTargetRect, modalPulsePeriodSeconds, 1.08f);
        UpdateArrowTowardSpotlight();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (type == FeatureTutorialType.Coins)
        {
            bool overlayActive = featureTutorialOverlay != null &&
                                 featureTutorialOverlay.activeInHierarchy;
            float alpha = overlayCanvasGroup != null ? overlayCanvasGroup.alpha : -1f;
            Debug.Log(
                "[CoinsFT]\n" +
                "Stage=CoinsModalOpened\n" +
                "Session=" + CoinsFtSessionId + "\n" +
                "Overlay=" + (featureTutorialOverlay != null
                    ? featureTutorialOverlay.name
                    : "null") + "\n" +
                "OverlayActiveInHierarchy=" + overlayActive + "\n" +
                "CanvasGroupAlpha=" + alpha + "\n" +
                "SiblingIndex=" + (featureTutorialOverlay != null
                    ? featureTutorialOverlay.transform.GetSiblingIndex().ToString()
                    : "n/a")
            );
        }
#endif
    }

    private void PrepareSkinsStep2ShopContext()
    {
        if (shopUI == null)
        {
            shopUI = FindAnyObjectByType<ShopUIController>();
        }

        if (shopUI == null)
        {
            return;
        }

        // Zorg dat skins-tab zichtbaar is vóór card-search.
        shopUI.ShowSkinsTab();
        skinsStep2Spotlight = shopUI.FindBuyableSkinSpotlightTarget();
    }

    private void OpenPointer(FeatureTutorialType type)
    {
        activeType = type;
        activeMode = FeatureTutorialPresentationMode.PointerOnly;
        pointerActive = true;
        modalActive = false;

        // Geen modal / Got It / input gate — alleen arrow (target-button niet animeren).
        SetModalOverlayVisible(false);
        SetPointerArrowVisible(true);

        RectTransform target = ResolveSpotlightTarget(type);
        BindTargetClick(target);
        PlacePointerArrowAtTarget(target);
        StartPointerArrowPresentation();

        if (pointerLifetimeCoroutine != null)
        {
            StopCoroutine(pointerLifetimeCoroutine);
        }

        int generation = presentationGeneration;
        pointerLifetimeCoroutine =
            StartCoroutine(PointerAutoHideRoutine(type, generation));
    }

    private IEnumerator PointerAutoHideRoutine(
        FeatureTutorialType type,
        int generation)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.5f, pointerDisplayDuration);

        while (elapsed < duration)
        {
            if (generation != presentationGeneration ||
                !pointerActive ||
                activeType != type)
            {
                pointerLifetimeCoroutine = null;
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        pointerLifetimeCoroutine = null;

        // Skins Step1 (enige pointer): GEEN MarkSeen — mag later opnieuw (andere visit).
        CompletePointer(markSeen: false);
    }

    private void OnTargetButtonClicked()
    {
        if (!pointerActive)
        {
            return;
        }

        if (activeType == FeatureTutorialType.Skins)
        {
            // Shop opent via bestaande button — Step 2 pending, nog niet seen.
            pendingSkinsStep2 = true;
            CompletePointer(markSeen: false);
            return;
        }

        CompletePointer(markSeen: false);
    }

    private void CompletePointer(bool markSeen)
    {
        if (!pointerActive)
        {
            return;
        }

        if (markSeen)
        {
            FeatureTutorialPrefs.MarkSeen(activeType);
        }

        presentationGeneration++;
        HideActivePresentation(markSeen: false, releaseGate: true);
    }

    private void HideActivePresentation(bool markSeen, bool releaseGate)
    {
        if (markSeen && (modalActive || pointerActive))
        {
            FeatureTutorialPrefs.MarkSeen(activeType);
        }

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        if (pointerLifetimeCoroutine != null)
        {
            StopCoroutine(pointerLifetimeCoroutine);
            pointerLifetimeCoroutine = null;
        }

        UnbindTargetClick();
        StopPointerArrowPresentation(restoreTransform: true);
        StopPulse(restoreScale: true);
        SetPointerArrowVisible(false);
        SetModalOverlayVisible(false);

        modalActive = false;
        pointerActive = false;
        skinsStep2Spotlight = null;

        if (releaseGate && gameManager != null)
        {
            gameManager.SetFeatureTutorialBlocked(false);
        }
    }

    private void ForceHideAllPresentation(bool releaseGate)
    {
        presentationGeneration++;
        HideActivePresentation(markSeen: false, releaseGate: releaseGate);
    }

    private void ApplyModalCopy(FeatureTutorialType type)
    {
        switch (type)
        {
            case FeatureTutorialType.Coins:
                SetText(titleText, coinsTitle, true);
                SetText(descriptionText, coinsPrimary, true);
                SetText(secondaryText, coinsSecondary, true);
                SetText(costText, null, false);
                break;

            case FeatureTutorialType.Skins:
                SetText(titleText, skinsTitle, true);
                SetText(descriptionText, skinsPrimary, true);
                SetText(secondaryText, skinsSecondary, true);
                SetText(costText, null, false);
                break;

            default:
                SetText(titleText, null, false);
                SetText(descriptionText, null, false);
                SetText(secondaryText, null, false);
                SetText(costText, null, false);
                break;
        }

        if (gotItButton != null)
        {
            gotItButton.gameObject.SetActive(true);
        }

        if (arrow != null)
        {
            arrow.gameObject.SetActive(true);
        }
    }

    private static void SetText(TextMeshProUGUI field, string content, bool visible)
    {
        if (field == null)
        {
            return;
        }

        if (!visible || string.IsNullOrEmpty(content))
        {
            field.gameObject.SetActive(false);
            return;
        }

        field.text = content;
        field.gameObject.SetActive(true);
    }

    private void SetModalOverlayVisible(bool visible)
    {
        if (featureTutorialOverlay == null)
        {
            if (visible && !loggedMissingOverlay)
            {
                loggedMissingOverlay = true;
                Debug.LogWarning(
                    "FeatureTutorialController: featureTutorialOverlay is null."
                );
            }

            return;
        }

        // Nooit de controller uitschakelen door overlay.SetActive(false).
        if (OverlaySharesHierarchyWithController())
        {
            if (!featureTutorialOverlay.activeSelf)
            {
                featureTutorialOverlay.SetActive(true);
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            EnsureOverlayCanvasGroup();
            if (overlayCanvasGroup != null)
            {
                overlayCanvasGroup.alpha = visible ? 1f : 0f;
                overlayCanvasGroup.blocksRaycasts = visible;
                overlayCanvasGroup.interactable = visible;
            }

            if (!visible)
            {
                HideModalChrome();
            }

            return;
        }

        featureTutorialOverlay.SetActive(visible);

        if (!visible)
        {
            return;
        }

        EnsureOverlayCanvasGroup();
        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.blocksRaycasts = true;
            overlayCanvasGroup.interactable = true;
            overlayCanvasGroup.alpha = 1f;
        }
    }

    private void SetPointerArrowVisible(bool visible)
    {
        if (arrow == null)
        {
            return;
        }

        if (visible)
        {
            EnsureActiveForPointer(arrow.gameObject);

            if (featureTutorialOverlay != null &&
                arrow.transform.IsChildOf(featureTutorialOverlay.transform))
            {
                if (!featureTutorialOverlay.activeSelf)
                {
                    featureTutorialOverlay.SetActive(true);
                }

                EnsureOverlayCanvasGroup();
                if (overlayCanvasGroup != null)
                {
                    // Pointer: geen dim/raycast-block.
                    overlayCanvasGroup.blocksRaycasts = false;
                    overlayCanvasGroup.interactable = false;
                    overlayCanvasGroup.alpha = 0f;
                }

                HideModalChrome();
            }

            arrow.gameObject.SetActive(true);
        }
        else
        {
            arrow.gameObject.SetActive(false);

            // Alleen losse overlay uitzetten — nooit gedeelde controller-hierarchy.
            if (featureTutorialOverlay != null &&
                !modalActive &&
                !OverlaySharesHierarchyWithController() &&
                overlayCanvasGroup != null &&
                overlayCanvasGroup.alpha <= 0.01f)
            {
                featureTutorialOverlay.SetActive(false);
                overlayCanvasGroup.alpha = 1f;
                overlayCanvasGroup.blocksRaycasts = true;
                overlayCanvasGroup.interactable = true;
            }
        }
    }

    private bool OverlaySharesHierarchyWithController()
    {
        if (featureTutorialOverlay == null)
        {
            return false;
        }

        Transform ot = featureTutorialOverlay.transform;
        return ot == transform ||
               transform.IsChildOf(ot) ||
               ot.IsChildOf(transform);
    }

    /// <summary>
    /// Houd controller alive als die op/onder de overlay hangt.
    /// </summary>
    private void EnsureControllerStaysAlive()
    {
        if (!OverlaySharesHierarchyWithController())
        {
            return;
        }

        if (featureTutorialOverlay != null && !featureTutorialOverlay.activeSelf)
        {
            featureTutorialOverlay.SetActive(true);
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        EnsureOverlayCanvasGroup();
        if (overlayCanvasGroup != null && !modalActive && !pointerActive)
        {
            overlayCanvasGroup.alpha = 0f;
            overlayCanvasGroup.blocksRaycasts = false;
            overlayCanvasGroup.interactable = false;
        }
    }

    private void HideModalChrome()
    {
        SetText(titleText, null, false);
        SetText(descriptionText, null, false);
        SetText(secondaryText, null, false);
        SetText(costText, null, false);

        if (gotItButton != null)
        {
            gotItButton.gameObject.SetActive(false);
        }
    }

    private void EnsureOverlayCanvasGroup()
    {
        if (featureTutorialOverlay == null)
        {
            return;
        }

        if (overlayCanvasGroup == null)
        {
            overlayCanvasGroup = featureTutorialOverlay.GetComponent<CanvasGroup>();
            if (overlayCanvasGroup == null)
            {
                overlayCanvasGroup = featureTutorialOverlay.AddComponent<CanvasGroup>();
            }
        }
    }

    private static void EnsureActiveForPointer(GameObject target)
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

            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
            }

            current = current.parent;
        }
    }

    private bool CanOpen(FeatureTutorialType type, bool ignoreShowInFlight = false)
    {
        return GetPresentBlockReason(type, ignoreShowInFlight) == null;
    }

    /// <summary>
    /// null = mag openen. Anders exacte block reason voor logs.
    /// </summary>
    private string GetPresentBlockReason(
        FeatureTutorialType type,
        bool ignoreShowInFlight)
    {
        if (FeatureTutorialPrefs.HasSeen(type))
        {
            return "AlreadySeen";
        }

        if (modalActive)
        {
            return "ModalActive";
        }

        if (pointerActive)
        {
            return "PointerActive";
        }

        if (isClosing)
        {
            return "IsClosing";
        }

        // showCoroutine is de in-flight ShowAfterDelay — mag zichzelf niet blokkeren.
        if (!ignoreShowInFlight && showCoroutine != null)
        {
            return "ShowCoroutineInFlight";
        }

        if (objectiveTutorialController != null &&
            objectiveTutorialController.IsTutorialUiActive)
        {
            return "ObjectiveTutorialUi";
        }

        if (gameManager != null)
        {
            if (gameManager.IsSpecialMissionIntroPlaying)
            {
                return "SpecialMissionIntro";
            }

            if (gameManager.IsObjectiveTutorialPlaying)
            {
                return "ObjectiveTutorialGate";
            }

            // Coins mag boven WinPanel; feature-gate van ONS mag open niet blokkeren
            // als we nog niet open zijn (gate wordt pas in OpenModal gezet).
            if (gameManager.IsFeatureTutorialPlaying &&
                type != FeatureTutorialType.Coins &&
                type != FeatureTutorialType.Skins)
            {
                return "FeatureTutorialGate";
            }

            if (gameManager.IsLevelFailed)
            {
                return "LevelFailed";
            }

            // Coins/Skins: post-win toegestaan. PointerOnly (Skins Step1 via BeginShow)
            // blokkeert tijdens win/exit — GetMode blijft Modal voor queue types.
            if (GetMode(type) == FeatureTutorialPresentationMode.PointerOnly)
            {
                if (gameManager.IsLevelCompleted)
                {
                    return "LevelCompleted(PointerOnly)";
                }

                if (gameManager.IsTargetExitInProgress)
                {
                    return "TargetExitInProgress(PointerOnly)";
                }
            }
        }

        return null;
    }

    private static FeatureTutorialPresentationMode GetMode(FeatureTutorialType type)
    {
        // Coins + Skins queue as Modal; Skins Step1 pointer set via BeginShow.
        // Parameter kept for call-site clarity / future PointerOnly queue types.
        _ = type;
        return FeatureTutorialPresentationMode.Modal;
    }

    private bool IsPresentationClearForModal()
    {
        // UI-busy check voor Skins auto-queue (WinPanel mag open blijven).
        if (modalActive || pointerActive || isClosing)
        {
            return false;
        }

        if (objectiveTutorialController != null &&
            objectiveTutorialController.IsTutorialUiActive)
        {
            return false;
        }

        if (gameManager != null)
        {
            if (gameManager.IsSpecialMissionIntroPlaying ||
                gameManager.IsObjectiveTutorialPlaying ||
                gameManager.IsLevelFailed)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsActivePresentationStillAllowed()
    {
        if (gameManager == null)
        {
            return true;
        }

        if (gameManager.IsLevelFailed)
        {
            return false;
        }

        if (activeMode == FeatureTutorialPresentationMode.Modal &&
            activeType == FeatureTutorialType.Coins &&
            !gameManager.IsLevelCompleted)
        {
            return false;
        }

        if (activeMode == FeatureTutorialPresentationMode.PointerOnly)
        {
            if (gameManager.IsLevelCompleted || gameManager.IsTargetExitInProgress)
            {
                return false;
            }

            if (gameManager.IsObjectiveTutorialPlaying ||
                gameManager.IsSpecialMissionIntroPlaying)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsSkinsEligible()
    {
        if (FeatureTutorialPrefs.HasSeen(FeatureTutorialType.Skins))
        {
            return false;
        }

        if (!FeatureTutorialPrefs.HasSeen(FeatureTutorialType.Coins))
        {
            return false;
        }

        if (!enableSkinsTutorialOnThisScene)
        {
            return false;
        }

        return shopButtonTarget != null ||
               (shopUI != null && shopUI.ShopButtonRect != null);
    }

    /// <summary>MainMenu visit: start Step1 of skip naar Step2 als shop al open.</summary>
    private void TryBeginSkinsOnboardingForThisVisit()
    {
        if (!IsSkinsEligible())
        {
            return;
        }

        ResolveShopTargetIfNeeded();

        if (shopUI != null && shopUI.IsShopOpen)
        {
            pendingSkinsStep2 = true;
            return;
        }

        if (!skinsPointerShownThisVisit)
        {
            pendingSkinsStep1 = true;
        }
    }

    private void ResolveShopTargetIfNeeded()
    {
        if (shopUI == null)
        {
            shopUI = FindAnyObjectByType<ShopUIController>();
        }

        if (shopButtonTarget == null && shopUI != null)
        {
            shopButtonTarget = shopUI.ShopButtonRect;
        }
    }

    private void OnShopOpened()
    {
        if (!enableSkinsTutorialOnThisScene || !IsSkinsEligible())
        {
            return;
        }

        // Stop Step1 pointer indien actief.
        if (pointerActive && activeType == FeatureTutorialType.Skins)
        {
            CompletePointer(markSeen: false);
        }

        pendingSkinsStep1 = false;
        pendingSkinsStep2 = true;

        // Zelfde frame of kort erna: Step 2 openen.
        TryOpenNextPending();
    }

    private RectTransform ResolveSpotlightTarget(FeatureTutorialType type)
    {
        switch (type)
        {
            case FeatureTutorialType.Coins:
                return coinCounterTarget;
            case FeatureTutorialType.Skins:
                // Step2: alleen buyable card (mag null = geen pulse).
                if (activeMode == FeatureTutorialPresentationMode.Modal)
                {
                    return skinsStep2Spotlight;
                }

                ResolveShopTargetIfNeeded();
                return shopButtonTarget;
            default:
                return null;
        }
    }

    private void StartPulse(RectTransform target, float periodSeconds, float peakMultiplier)
    {
        StopPulse(restoreScale: false);

        pulseTarget = target;
        if (pulseTarget == null)
        {
            return;
        }

        pulseBaseScale = pulseTarget.localScale;
        pulseBaseCached = true;
        pulseCoroutine = StartCoroutine(PulseRoutine(periodSeconds, peakMultiplier));
    }

    private void StopPulse(bool restoreScale)
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        if (restoreScale && pulseBaseCached && pulseTarget != null)
        {
            pulseTarget.localScale = pulseBaseScale;
        }

        pulseTarget = null;
        pulseBaseCached = false;
    }

    private IEnumerator PulseRoutine(float periodSeconds, float peakMultiplier)
    {
        if (pulseTarget == null || !pulseBaseCached)
        {
            pulseCoroutine = null;
            yield break;
        }

        Vector3 baseScale = pulseBaseScale;
        Vector3 peakScale = baseScale * peakMultiplier;
        float halfPeriod = Mathf.Max(0.05f, periodSeconds * 0.5f);

        while ((modalActive || pointerActive) && pulseTarget != null)
        {
            float t = 0f;
            while (t < halfPeriod && (modalActive || pointerActive) && pulseTarget != null)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / halfPeriod);
                pulseTarget.localScale = Vector3.LerpUnclamped(baseScale, peakScale, u);
                yield return null;
            }

            t = 0f;
            while (t < halfPeriod && (modalActive || pointerActive) && pulseTarget != null)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / halfPeriod);
                pulseTarget.localScale = Vector3.LerpUnclamped(peakScale, baseScale, u);
                yield return null;
            }

            if (pulseTarget != null)
            {
                pulseTarget.localScale = baseScale;
            }
        }

        pulseCoroutine = null;
    }

    /// <summary>
    /// Zet arrow links van target (target-center + pointerTargetOffset).
    /// Geen auto-rotation — sprite blijft naar rechts wijzen.
    /// </summary>
    private void PlacePointerArrowAtTarget(RectTransform target)
    {
        if (arrow == null || target == null)
        {
            return;
        }

        RectTransform arrowParent = arrow.parent as RectTransform;
        if (arrowParent == null)
        {
            return;
        }

        CacheArrowSceneRotationIfNeeded();
        arrow.localRotation = arrowSceneLocalRotation;

        Vector3 targetWorldCenter = target.TransformPoint(target.rect.center);
        Vector3 local = arrowParent.InverseTransformPoint(targetWorldCenter);
        Vector2 targetLocal = new Vector2(local.x, local.y);

        arrowBaseAnchoredPos = targetLocal + pointerTargetOffset;
        arrow.anchoredPosition = arrowBaseAnchoredPos;

        arrowBaseLocalScale = arrow.localScale;
        arrowBaseLocalRotation = arrow.localRotation;
        arrowTransformCached = true;
    }

    private void CacheArrowSceneRotationIfNeeded()
    {
        if (arrowSceneRotationCached || arrow == null)
        {
            return;
        }

        arrowSceneLocalRotation = arrow.localRotation;
        arrowSceneRotationCached = true;
    }

    private void StartPointerArrowPresentation()
    {
        // Stop lopende animatie ZONDER base-cache te wissen.
        if (pointerPresentationCoroutine != null)
        {
            StopCoroutine(pointerPresentationCoroutine);
            pointerPresentationCoroutine = null;
        }

        if (arrow == null || !arrowTransformCached)
        {
            return;
        }

        pointerPresentationCoroutine = StartCoroutine(PointerArrowPresentationRoutine());
    }

    private void StopPointerArrowPresentation(bool restoreTransform)
    {
        if (pointerPresentationCoroutine != null)
        {
            StopCoroutine(pointerPresentationCoroutine);
            pointerPresentationCoroutine = null;
        }

        if (restoreTransform && arrowTransformCached && arrow != null)
        {
            arrow.anchoredPosition = arrowBaseAnchoredPos;
            arrow.localScale = arrowBaseLocalScale;
            arrow.localRotation = arrowBaseLocalRotation;
            arrowTransformCached = false;
        }
    }

    private IEnumerator PointerArrowPresentationRoutine()
    {
        if (arrow == null || !arrowTransformCached)
        {
            pointerPresentationCoroutine = null;
            yield break;
        }

        Vector2 basePos = arrowBaseAnchoredPos;
        Vector3 baseScale = arrowBaseLocalScale;
        Quaternion baseRot = arrowBaseLocalRotation;
        float moveDist = Mathf.Max(0f, pointerHorizontalMoveDistance);
        float moveSpeed = Mathf.Max(0.1f, pointerHorizontalMoveSpeed);
        float pulsePeak = Mathf.Max(1f, pointerPulseScale);
        bool scalePulse = pointerScalePulseEnabled;

        // Rotatie vast houden (sprite wijst rechts).
        arrow.localRotation = baseRot;

        while (pointerActive && arrow != null && arrow.gameObject.activeInHierarchy)
        {
            float t = Time.unscaledTime * moveSpeed;
            float pulse01 = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f);

            // Alleen horizontaal naar rechts (sprite-richting).
            arrow.anchoredPosition =
                basePos + Vector2.right * (pulse01 * moveDist);

            if (scalePulse)
            {
                float scaleMul = Mathf.Lerp(1f, pulsePeak, pulse01);
                arrow.localScale = baseScale * scaleMul;
            }
            else
            {
                arrow.localScale = baseScale;
            }

            // Geen auto-rotation tijdens pointer.
            arrow.localRotation = baseRot;

            yield return null;
        }

        pointerPresentationCoroutine = null;
    }

    /// <summary>Modal-only: roteer bestaande arrow naar spotlight (geen herpositionering).</summary>
    private void UpdateArrowTowardSpotlight()
    {
        if (arrow == null || !arrow.gameObject.activeInHierarchy)
        {
            return;
        }

        RectTransform target = ResolveSpotlightTarget(activeType);
        if (target == null)
        {
            return;
        }

        Canvas canvas = arrow.GetComponentInParent<Canvas>();
        Camera eventCam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCam = canvas.worldCamera;
        }

        Vector2 targetScreen = RectTransformUtility.WorldToScreenPoint(
            eventCam,
            target.TransformPoint(target.rect.center));

        Vector2 arrowScreen = RectTransformUtility.WorldToScreenPoint(
            eventCam,
            arrow.position);

        Vector2 delta = targetScreen - arrowScreen;
        if (delta.sqrMagnitude < 0.001f)
        {
            return;
        }

        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        arrow.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void BindTargetClick(RectTransform target)
    {
        UnbindTargetClick();

        if (target == null)
        {
            return;
        }

        boundTargetButton = target.GetComponent<Button>();
        if (boundTargetButton == null)
        {
            boundTargetButton = target.GetComponentInParent<Button>();
        }

        if (boundTargetButton == null)
        {
            return;
        }

        boundTargetButton.onClick.AddListener(OnTargetButtonClicked);
        targetClickBound = true;
    }

    private void UnbindTargetClick()
    {
        if (!targetClickBound || boundTargetButton == null)
        {
            boundTargetButton = null;
            targetClickBound = false;
            return;
        }

        boundTargetButton.onClick.RemoveListener(OnTargetButtonClicked);
        boundTargetButton = null;
        targetClickBound = false;
    }

    private void OnWinSequenceFinished()
    {
        // Event-pad; UIManager roept ook NotifyWinSequenceFinished (direct).
        HandleWinSequenceFinished();
    }

    private void LogCoinsFtPendingRecheck(
        FeatureTutorialType type,
        bool ignoreShowInFlight)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (type != FeatureTutorialType.Coins)
        {
            return;
        }

        string block = GetPresentBlockReason(type, ignoreShowInFlight) ?? "None";
        if (block == lastPendingBlockReason)
        {
            return;
        }

        lastPendingBlockReason = block;
        Debug.Log(
            "[CoinsFT]\n" +
            "Stage=PendingRecheck\n" +
            "Session=" + CoinsFtSessionId + "\n" +
            "CanPresent=" + (block == "None") + "\n" +
            "BlockReason=" + block
        );
#endif
    }

    private void ValidateCoinsModalRefs()
    {
        if (loggedRefValidation)
        {
            return;
        }

        loggedRefValidation = true;
        System.Text.StringBuilder missing = new System.Text.StringBuilder();

        if (featureTutorialOverlay == null)
        {
            missing.Append("featureTutorialOverlay; ");
        }

        if (titleText == null)
        {
            missing.Append("titleText; ");
        }

        if (descriptionText == null)
        {
            missing.Append("descriptionText; ");
        }

        if (gotItButton == null)
        {
            missing.Append("gotItButton; ");
        }

        if (missing.Length > 0)
        {
            Debug.LogWarning(
                "[CoinsFT] Stage=MissingModalRefs Missing=" + missing
            );
        }

        if (featureTutorialOverlay == gameObject)
        {
            Debug.LogWarning(
                "[CoinsFT] Stage=HierarchyWarning " +
                "FeatureTutorialController staat OP FeatureTutorialOverlay (self-ref). " +
                "Verplaats de controller naar een altijd-actief sibling onder GameCanvas; " +
                "houd alleen de overlay als inactive/visible target."
            );
        }
    }

    private void Requeue(FeatureTutorialType type)
    {
        if (FeatureTutorialPrefs.HasSeen(type))
        {
            return;
        }

        switch (type)
        {
            case FeatureTutorialType.Coins:
                pendingCoins = true;
                break;
            case FeatureTutorialType.Skins:
                // Requeue Step2 if shop open, else Step1 (mits visit allow).
                if (shopUI != null && shopUI.IsShopOpen)
                {
                    pendingSkinsStep2 = true;
                }
                else if (!skinsPointerShownThisVisit)
                {
                    pendingSkinsStep1 = true;
                }

                break;
        }
    }

    private void ClearSeenPendings()
    {
        if (FeatureTutorialPrefs.HasSeen(FeatureTutorialType.Coins))
        {
            pendingCoins = false;
        }

        if (FeatureTutorialPrefs.HasSeen(FeatureTutorialType.Skins))
        {
            pendingSkinsStep1 = false;
            pendingSkinsStep2 = false;
        }
    }

    private void ClearAllPending()
    {
        pendingCoins = false;
        pendingSkinsStep1 = false;
        pendingSkinsStep2 = false;
    }

    private void BindGotItListener()
    {
        if (gotItButton == null || gotItListenerBound)
        {
            return;
        }

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

    private void SubscribeEvents()
    {
        if (uiManager != null)
        {
            uiManager.OnWinSequenceFinished -= OnWinSequenceFinished;
            uiManager.OnWinSequenceFinished += OnWinSequenceFinished;
        }

        if (shopUI == null && enableSkinsTutorialOnThisScene)
        {
            shopUI = FindAnyObjectByType<ShopUIController>();
        }

        if (shopUI != null)
        {
            shopUI.OnShopOpened -= OnShopOpened;
            shopUI.OnShopOpened += OnShopOpened;
        }
    }

    private void UnsubscribeEvents()
    {
        if (uiManager != null)
        {
            uiManager.OnWinSequenceFinished -= OnWinSequenceFinished;
        }

        if (shopUI != null)
        {
            shopUI.OnShopOpened -= OnShopOpened;
        }
    }

    private void ResolveRefs()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        if (saveManager == null)
        {
            saveManager = FindAnyObjectByType<SaveManager>();
        }

        if (uiManager == null)
        {
            uiManager = FindAnyObjectByType<UIManager>();
        }

        if (objectiveTutorialController == null)
        {
            objectiveTutorialController =
                FindAnyObjectByType<ObjectiveTutorialController>();
        }

        if (shopUI == null && enableSkinsTutorialOnThisScene)
        {
            shopUI = FindAnyObjectByType<ShopUIController>();
        }

        ResolveShopTargetIfNeeded();
    }

    /// <summary>Editor/dev: log Skins eligibility snapshot.</summary>
    public void EditorLogSkinsEligibility()
    {
        ResolveShopTargetIfNeeded();
        bool coinsSeen = FeatureTutorialPrefs.HasSeen(FeatureTutorialType.Coins);
        bool skinsSeen = FeatureTutorialPrefs.HasSeen(FeatureTutorialType.Skins);
        bool eligible = IsSkinsEligible();
        Debug.Log(
            "[SkinsFT]\n" +
            "Stage=Eligibility\n" +
            "CoinsSeen=" + coinsSeen + "\n" +
            "SkinsSeen=" + skinsSeen + "\n" +
            "EnableOnScene=" + enableSkinsTutorialOnThisScene + "\n" +
            "ShopTarget=" + (shopButtonTarget != null
                ? shopButtonTarget.name
                : "null") + "\n" +
            "ShopOpen=" + (shopUI != null && shopUI.IsShopOpen) + "\n" +
            "Eligible=" + eligible
        );
    }
}

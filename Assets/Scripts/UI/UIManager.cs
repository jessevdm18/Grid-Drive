using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Bestuurt het win-scherm van Grid Drive.
/// Koppel knoppen in de Inspector aan OnNextLevelButton / OnRestartButton.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Het panel dat verschijnt wanneer het level is gehaald.")]
    [SerializeField] private GameObject winPanel;

    [Header("Win Score")]
    [Tooltip("Alleen het aantal moves (cijfer). Label blijft buiten deze tekst.")]
    [SerializeField] private TMP_Text movesValueText;
    [Tooltip("ParCard/ValueText — alleen het par-getal, bijv. \"8\".")]
    [SerializeField] private TMP_Text parValueText;
    [SerializeField] private Image star1;
    [SerializeField] private Image star2;
    [SerializeField] private Image star3;
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private Sprite emptyStarSprite;

    [Header("Star Reveal")]
    [SerializeField] private float starRevealDelay = 0.18f;
    [SerializeField] private float starPopDuration = 0.18f;
    [SerializeField] private float starPopScale = 1.25f;

    [Header("Coin Reward Animation")]
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private RectTransform rewardTextRect;
    [SerializeField] private RectTransform coinTextRect;
    [SerializeField] private float rewardShowDuration = 0.4f;
    [SerializeField] private float rewardFlyDuration = 0.5f;
    [SerializeField] private float rewardStartScale = 0.6f;
    [SerializeField] private float rewardPopScale = 1.2f;

    [Header("Win Confetti")]
    [SerializeField] private UIWinConfetti winConfetti;

    [Header("Difficulty Unlock")]
    [Tooltip("Optional choice panel when Medium/Hard unlocks for the first time.")]
    [SerializeField] private DifficultyUnlockNotificationUI difficultyUnlockNotification;

    [Header("Win Sequence Gate")]
    [Tooltip("Optional. Disabled during unlock notice + star/coin reveal.")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;
    [Tooltip("Optional. When assigned, blocksRaycasts follows win-sequence interactable.")]
    [SerializeField] private CanvasGroup winPanelCanvasGroup;

    [Header("Referenties")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AdsManager adsManager;

    private AudioManager audioManager;

    // Voorkomt dubbele Next Level-acties (dubbele klik / dubbele ad-callback).
    private bool isHandlingNextLevel;

    // Diagnostic: how many Next clicks this WinPanel session.
    private int nextClickCountThisWin;

    // Lopende win-panel animaties (sterren + coin reward).
    private Coroutine winSequenceCoroutine;
    private Coroutine coinHudPopCoroutine;

    // Home-state van RewardText voor betrouwbare reset.
    private Vector2 rewardHomeAnchoredPos;
    private bool hasRewardHomePos;

    /// <summary>
    /// Fired wanneer sterren + eventuele coin-fly klaar zijn (WinPanel blijft open).
    /// Feature tutorials (Coins) kunnen hier veilig na openen.
    /// </summary>
    public event Action OnWinSequenceFinished;

    private void Awake()
    {
        audioManager = FindAnyObjectByType<AudioManager>();

        if (rewardTextRect != null)
        {
            rewardHomeAnchoredPos = rewardTextRect.anchoredPosition;
            hasRewardHomePos = true;
        }

        // RewardText start verborgen.
        ResetRewardVisual(hide: true);

        // Ensure no stale unlock raycast blocker from a previous Play Mode session.
        EnsureDifficultyUnlockFullyHidden();
    }

    private void Start()
    {
        // Win-scherm start verborgen.
        HideWinPanel();
    }

    /// <summary>
    /// Toont het win-scherm. Roep dit aan vanuit GameManager bij een win.
    /// </summary>
    public void ShowWinPanel()
    {
        UpdateWinScoreTexts();

        StopWinAnimations();
        PrepareStarsForReveal();
        ResetRewardVisual(hide: true);

        nextClickCountThisWin = 0;
        isHandlingNextLevel = false;

        // Always clear unlock UI — even when this completion has no unlock —
        // so a previous Stay/Try never leaves InputBlocker over Next.
        EnsureDifficultyUnlockFullyHidden();

        // Block Next/Restart until unlock (if any) + stars/coins + FT finish.
        SetWinPanelButtonsInteractable(false);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (winConfetti != null)
        {
            Debug.Log("UIManager calling UIWinConfetti.PlayConfetti");
            winConfetti.PlayConfetti();
        }
        else
        {
            Debug.LogWarning("UIManager: winConfetti is not assigned.");
        }

        int earnedStars = gameManager != null ? gameManager.LastEarnedStars : 0;
        int earnedCoins = gameManager != null ? gameManager.LastEarnedCoins : 0;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        bool grantedStart =
            gameManager != null && gameManager.LastThreeStarCoinRewardGranted;
        Debug.Log(
            "[CoinsFT]\n" +
            "Stage=WinSequenceStart\n" +
            "Session=" + FeatureTutorialController.CoinsFtSessionId + "\n" +
            "RewardGranted=" + grantedStart + "\n" +
            "EarnedCoins=" + earnedCoins
        );
#endif

        winSequenceCoroutine = StartCoroutine(WinSequenceRoutine(earnedStars, earnedCoins));
    }

    /// <summary>
    /// Verbergt het win-scherm en stopt alle win-animaties.
    /// </summary>
    public void HideWinPanel()
    {
        StopWinAnimations();
        ResetStarScales();
        ResetRewardVisual(hide: true);

        EnsureDifficultyUnlockFullyHidden();

        SetWinPanelButtonsInteractable(true);

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    private void StopWinAnimations()
    {
        if (winSequenceCoroutine != null)
        {
            StopCoroutine(winSequenceCoroutine);
            winSequenceCoroutine = null;
        }

        if (coinHudPopCoroutine != null)
        {
            StopCoroutine(coinHudPopCoroutine);
            coinHudPopCoroutine = null;
        }

        if (winConfetti != null)
        {
            winConfetti.StopConfetti();
        }

        if (coinTextRect != null)
        {
            coinTextRect.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Unlock choice (only if pending) → stars → coins → Coins FT →
    /// Stay/normal: enable Next / Try: navigate to first ordered unlocked tier.
    /// Completion/rewards are already persisted before this routine runs.
    /// </summary>
    private IEnumerator WinSequenceRoutine(int earnedStars, int earnedCoins)
    {
        // Local only — never carried across completions.
        LevelDifficulty? pendingDifficultyNavigation = null;

        bool unlockPending =
            gameManager != null &&
            gameManager.PendingDifficultyUnlockNotice.HasValue;

        LogNextButtonTrace("WinSequenceStart", unlockPending);

        // 1) Difficulty unlock choice ONLY when pending for this completion.
        if (gameManager != null &&
            gameManager.TryConsumePendingDifficultyUnlockNotice(out LevelDifficulty unlocked))
        {
            DifficultyUnlockChoice choice =
                DifficultyUnlockChoice.StayCurrentDifficulty;

            if (difficultyUnlockNotification != null)
            {
                yield return difficultyUnlockNotification.ShowChoiceAndWait(unlocked);
                choice = difficultyUnlockNotification.LastChoice;
            }
            else
            {
                Debug.LogWarning(
                    "UIManager: difficulty unlock pending but " +
                    "DifficultyUnlockNotificationUI is not assigned."
                );
            }

            // Mark seen only after the player actually chose.
            DifficultyUnlockNoticePrefs.MarkSeen(unlocked);

            if (choice == DifficultyUnlockChoice.TryUnlockedDifficulty)
            {
                pendingDifficultyNavigation = unlocked;
            }

            // Always clear blocker after choice (Stay or Try) before stars.
            EnsureDifficultyUnlockFullyHidden();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogDifficultyUnlockChoice(unlocked, choice, pendingDifficultyNavigation);
#endif
        }
        else
        {
            // Normal completion: unlock UI must not affect this flow.
            EnsureDifficultyUnlockFullyHidden();
        }

        LogNextButtonTrace("BeforeStars", unlockPending: false);

        // 2) Star reveal
        yield return RevealStarsRoutine(earnedStars);

        LogNextButtonTrace("AfterStars", unlockPending: false);

        // 3) Coin fly (only when coins granted)
        if (earnedCoins > 0)
        {
            if (CanPlayRewardAnimation())
            {
                yield return RewardFlyRoutine(earnedCoins);
            }
            else
            {
                audioManager?.PlayCoin();
            }
        }

        // 4) Fire Coins FT while Next stays non-interactable (no half-active state).
        LogNextButtonTrace("BeforeEnableButtons", unlockPending: false);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        bool granted = gameManager != null && gameManager.LastThreeStarCoinRewardGranted;
        int subscriberCount = OnWinSequenceFinished != null
            ? OnWinSequenceFinished.GetInvocationList().Length
            : 0;
        Debug.Log(
            "[CoinsFT]\n" +
            "Stage=WinSequenceFinishedEventFired\n" +
            "Session=" + FeatureTutorialController.CoinsFtSessionId + "\n" +
            "RewardGranted=" + granted + "\n" +
            "EventSubscribers=" + subscriberCount
        );
#endif

        OnWinSequenceFinished?.Invoke();
        FeatureTutorialController.NotifyWinSequenceFinished();

        yield return WaitUntilFeatureTutorialsIdle();

        // 5) Clear running flag BEFORE enabling buttons so Next click is never
        // rejected by WinSequenceRunning while visually interactable.
        winSequenceCoroutine = null;

        if (pendingDifficultyNavigation.HasValue)
        {
            LevelDifficulty target = pendingDifficultyNavigation.Value;
            pendingDifficultyNavigation = null;
            if (!TryNavigateToFirstLevelOfDifficulty(target))
            {
                SetWinPanelButtonsInteractable(true);
                LogNextButtonTrace("ButtonsEnabled", unlockPending: false);
            }
        }
        else
        {
            SetWinPanelButtonsInteractable(true);
            LogNextButtonTrace("ButtonsEnabled", unlockPending: false);
        }
    }

    private void EnsureDifficultyUnlockFullyHidden()
    {
        if (difficultyUnlockNotification != null)
        {
            difficultyUnlockNotification.EnsureFullyHidden();
        }
    }

    private IEnumerator WaitUntilFeatureTutorialsIdle()
    {
        // Allow subscribers / NotifyWinSequenceFinished to queue Coins FT.
        yield return null;

        FeatureTutorialController ft =
            FindAnyObjectByType<FeatureTutorialController>(
                FindObjectsInactive.Include);
        if (ft == null)
        {
            yield break;
        }

        // Coins FT uses a short show delay; wait through queue + delay + Got It.
        float elapsed = 0f;
        const float timeoutSeconds = 90f;
        while (ft.IsBlockingModalPresentation && elapsed < timeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Save first ordered db index for difficulty and reload Gameplay.
    /// Returns false on missing levels / unlock / managers (caller continues Stay flow).
    /// </summary>
    private bool TryNavigateToFirstLevelOfDifficulty(LevelDifficulty difficulty)
    {
        if (levelManager == null)
        {
            Debug.LogWarning(
                "[DifficultyUnlock] Cannot navigate: LevelManager missing."
            );
            return false;
        }

        LevelDatabase database = levelManager.LevelDatabase;
        if (database == null)
        {
            Debug.LogWarning(
                "[DifficultyUnlock] Cannot navigate: LevelDatabase missing."
            );
            return false;
        }

        SaveManager saveManager = FindAnyObjectByType<SaveManager>();
        if (saveManager == null)
        {
            Debug.LogWarning(
                "[DifficultyUnlock] Cannot navigate: SaveManager missing."
            );
            return false;
        }

        int dbIndex = LevelDifficultyOrder.GetFirstOrderedLevelIndex(
            database,
            difficulty
        );

        if (dbIndex < 0)
        {
            Debug.LogWarning(
                "[DifficultyUnlock] TRY " + difficulty +
                " failed: no levels in that tier. Continuing current WinSequence."
            );
            return false;
        }

        if (!saveManager.IsDifficultyUnlocked(
                difficulty,
                database,
                levelManager.DifficultyProgressionConfig))
        {
            Debug.LogWarning(
                "[DifficultyUnlock] TRY " + difficulty +
                " failed: difficulty not unlocked. Continuing current WinSequence."
            );
            return false;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        int displayNumber = LevelDifficultyOrder.GetDifficultyDisplayNumber(
            database,
            dbIndex
        );
        Debug.Log(
            "[DifficultyUnlock]\n" +
            "Unlocked=" + difficulty + "\n" +
            "Choice=Try\n" +
            "TargetDbIndex=" + dbIndex + "\n" +
            "TargetDisplayNumber=" + displayNumber + "\n" +
            "Navigating=Gameplay"
        );
#endif

        saveManager.SaveCurrentLevel(dbIndex);
        SceneTransition.LoadScene("Gameplay");
        return true;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void LogDifficultyUnlockChoice(
        LevelDifficulty unlocked,
        DifficultyUnlockChoice choice,
        LevelDifficulty? pendingNav)
    {
        if (choice == DifficultyUnlockChoice.StayCurrentDifficulty)
        {
            Debug.Log(
                "[DifficultyUnlock]\n" +
                "Unlocked=" + unlocked + "\n" +
                "Choice=Stay"
            );
            return;
        }

        LevelDatabase database =
            levelManager != null ? levelManager.LevelDatabase : null;
        int dbIndex = database != null
            ? LevelDifficultyOrder.GetFirstOrderedLevelIndex(database, unlocked)
            : -1;
        int displayNumber = database != null && dbIndex >= 0
            ? LevelDifficultyOrder.GetDifficultyDisplayNumber(database, dbIndex)
            : -1;

        Debug.Log(
            "[DifficultyUnlock]\n" +
            "Unlocked=" + unlocked + "\n" +
            "Choice=Try\n" +
            "TargetDbIndex=" + dbIndex + "\n" +
            "TargetDisplayNumber=" + displayNumber + "\n" +
            "PendingNav=" + (pendingNav.HasValue ? pendingNav.Value.ToString() : "none")
        );
    }
#endif

    /// <summary>
    /// Single owner for WinPanel Next/Restart interactable state.
    /// Buttons become interactable only when no unlock/FT blocker should eat clicks.
    /// </summary>
    private void SetWinPanelButtonsInteractable(bool enabled)
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = enabled;
        }

        if (restartButton != null)
        {
            restartButton.interactable = enabled;
        }

        if (winPanelCanvasGroup != null)
        {
            winPanelCanvasGroup.interactable = enabled;
            // Keep raycasts on so layout works; buttons themselves are gated.
            winPanelCanvasGroup.blocksRaycasts = true;
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void LogNextButtonTrace(string stage, bool unlockPending)
    {
        bool nextInteractable =
            nextLevelButton != null && nextLevelButton.interactable;
        bool blockerActive = false;
        bool blockerBlocks = false;

        if (difficultyUnlockNotification != null)
        {
            blockerActive = difficultyUnlockNotification.IsVisualRootActive();
            blockerBlocks = difficultyUnlockNotification.IsBlockerRaycastActive();
        }

        Debug.Log(
            "[NextButtonTrace]\n" +
            "Stage=" + stage + "\n" +
            "UnlockPending=" + unlockPending + "\n" +
            "NextInteractable=" + nextInteractable + "\n" +
            "BlockerActive=" + blockerActive + "\n" +
            "BlockerBlocksRaycasts=" + blockerBlocks + "\n" +
            "IsHandlingNext=" + isHandlingNextLevel + "\n" +
            "WinSequenceRunning=" + (winSequenceCoroutine != null)
        );
    }
#else
    private void LogNextButtonTrace(string stage, bool unlockPending)
    {
    }
#endif

    private bool CanPlayRewardAnimation()
    {
        return rewardText != null && rewardTextRect != null && coinTextRect != null;
    }

    // -------------------------------------------------------------------------
    // Score teksten
    // -------------------------------------------------------------------------

    private void UpdateWinScoreTexts()
    {
        int moves = gameManager != null ? gameManager.CurrentMoves : 0;

        if (movesValueText != null)
        {
            movesValueText.text = moves.ToString();
        }

        if (parValueText != null)
        {
            int par = 0;
            LevelData levelData = levelManager != null ? levelManager.CurrentLevelData : null;
            if (levelData != null)
            {
                par = levelData.minimumMoves;
            }

            parValueText.text = par.ToString();
        }
    }

    // -------------------------------------------------------------------------
    // Star reveal
    // -------------------------------------------------------------------------

    private void PrepareStarsForReveal()
    {
        SetStarImage(star1, false);
        SetStarImage(star2, false);
        SetStarImage(star3, false);
        ResetStarScales();
    }

    private void ResetStarScales()
    {
        SetStarScale(star1, Vector3.one);
        SetStarScale(star2, Vector3.one);
        SetStarScale(star3, Vector3.one);
    }

    private IEnumerator RevealStarsRoutine(int earnedStars)
    {
        earnedStars = Mathf.Clamp(earnedStars, 0, 3);
        Image[] stars = { star1, star2, star3 };

        for (int i = 0; i < earnedStars; i++)
        {
            yield return new WaitForSeconds(starRevealDelay);

            Image star = stars[i];
            if (star == null)
            {
                continue;
            }

            SetStarImage(star, true);
            yield return AnimateStarPop(star);
        }
    }

    private IEnumerator AnimateStarPop(Image star)
    {
        float duration = Mathf.Max(0.01f, starPopDuration);
        float half = duration * 0.5f;
        Transform t = star.transform;

        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float n = Mathf.Clamp01(elapsed / half);
            float eased = Mathf.SmoothStep(0f, 1f, n);
            t.localScale = Vector3.one * Mathf.Lerp(0.6f, starPopScale, eased);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float n = Mathf.Clamp01(elapsed / half);
            float eased = Mathf.SmoothStep(0f, 1f, n);
            t.localScale = Vector3.one * Mathf.Lerp(starPopScale, 1f, eased);
            yield return null;
        }

        t.localScale = Vector3.one;
    }

    private void SetStarImage(Image image, bool filled)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = filled ? filledStarSprite : emptyStarSprite;
    }

    private static void SetStarScale(Image image, Vector3 scale)
    {
        if (image != null)
        {
            image.transform.localScale = scale;
        }
    }

    // -------------------------------------------------------------------------
    // Coin reward fly (visueel only — geen AddCoins)
    // -------------------------------------------------------------------------

    private IEnumerator RewardFlyRoutine(int earnedCoins)
    {
        // Toon "+50" (of echte LastEarnedCoins).
        rewardText.gameObject.SetActive(true);
        rewardText.text = "+" + earnedCoins;
        SetRewardAlpha(1f);

        if (hasRewardHomePos)
        {
            rewardTextRect.anchoredPosition = rewardHomeAnchoredPos;
        }

        rewardTextRect.localScale = Vector3.one * rewardStartScale;

        // Pop: start → pop → 1.0
        yield return AnimateScale(
            rewardTextRect,
            rewardStartScale,
            rewardPopScale,
            rewardPopScale,
            1f,
            starPopDuration
        );

        // Kort zichtbaar houden.
        yield return new WaitForSeconds(Mathf.Max(0f, rewardShowDuration));

        // Vlieg naar CoinText (correcte UI-coördinaten).
        Vector2 startLocal = rewardTextRect.anchoredPosition;
        Vector2 endLocal = GetLocalPointInRewardParent(coinTextRect);

        float flyDuration = Mathf.Max(0.01f, rewardFlyDuration);
        float elapsed = 0f;
        Color baseColor = rewardText.color;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float n = Mathf.Clamp01(elapsed / flyDuration);
            float eased = Mathf.SmoothStep(0f, 1f, n);

            rewardTextRect.anchoredPosition = Vector2.Lerp(startLocal, endLocal, eased);
            rewardTextRect.localScale = Vector3.one * Mathf.Lerp(1f, 0.35f, eased);

            Color c = baseColor;
            c.a = Mathf.Lerp(1f, 0f, eased);
            rewardText.color = c;

            yield return null;
        }

        // Aankomst: verberg + reset reward, pop CoinText, speel coin-SFX.
        ResetRewardVisual(hide: true);
        audioManager?.PlayCoin();

        if (coinHudPopCoroutine != null)
        {
            StopCoroutine(coinHudPopCoroutine);
        }

        coinHudPopCoroutine = StartCoroutine(AnimateCoinHudPop());
    }

    /// <summary>
    /// Zet een world/UI RectTransform om naar local anchored space van rewardTextRect.parent.
    /// Werkt voor Overlay- en Camera-canvas.
    /// </summary>
    private Vector2 GetLocalPointInRewardParent(RectTransform target)
    {
        RectTransform parent = rewardTextRect.parent as RectTransform;
        if (parent == null || target == null)
        {
            return rewardTextRect.anchoredPosition;
        }

        Canvas canvas = rewardTextRect.GetComponentInParent<Canvas>();
        Camera eventCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = canvas.worldCamera;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, target.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            screenPoint,
            eventCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }

    private IEnumerator AnimateCoinHudPop()
    {
        if (coinTextRect == null)
        {
            coinHudPopCoroutine = null;
            yield break;
        }

        float duration = 0.16f;
        float half = duration * 0.5f;

        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float n = Mathf.Clamp01(elapsed / half);
            float eased = Mathf.SmoothStep(0f, 1f, n);
            coinTextRect.localScale = Vector3.one * Mathf.Lerp(1f, 1.15f, eased);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float n = Mathf.Clamp01(elapsed / half);
            float eased = Mathf.SmoothStep(0f, 1f, n);
            coinTextRect.localScale = Vector3.one * Mathf.Lerp(1.15f, 1f, eased);
            yield return null;
        }

        coinTextRect.localScale = Vector3.one;
        coinHudPopCoroutine = null;
    }

    private IEnumerator AnimateScale(
        RectTransform target,
        float fromA,
        float toA,
        float fromB,
        float toB,
        float totalDuration)
    {
        float duration = Mathf.Max(0.01f, totalDuration);
        float half = duration * 0.5f;

        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float n = Mathf.Clamp01(elapsed / half);
            float eased = Mathf.SmoothStep(0f, 1f, n);
            target.localScale = Vector3.one * Mathf.Lerp(fromA, toA, eased);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float n = Mathf.Clamp01(elapsed / half);
            float eased = Mathf.SmoothStep(0f, 1f, n);
            target.localScale = Vector3.one * Mathf.Lerp(fromB, toB, eased);
            yield return null;
        }

        target.localScale = Vector3.one * toB;
    }

    private void ResetRewardVisual(bool hide)
    {
        if (rewardTextRect != null)
        {
            if (hasRewardHomePos)
            {
                rewardTextRect.anchoredPosition = rewardHomeAnchoredPos;
            }

            rewardTextRect.localScale = Vector3.one;
        }

        SetRewardAlpha(1f);

        if (rewardText != null)
        {
            if (hide)
            {
                rewardText.gameObject.SetActive(false);
            }
        }
        else if (rewardTextRect != null && hide)
        {
            rewardTextRect.gameObject.SetActive(false);
        }
    }

    private void SetRewardAlpha(float alpha)
    {
        if (rewardText == null)
        {
            return;
        }

        Color c = rewardText.color;
        c.a = alpha;
        rewardText.color = c;
    }

    // -------------------------------------------------------------------------
    // Buttons
    // -------------------------------------------------------------------------

    /// <summary>
    /// Wordt aangeroepen door de "Next Level"-knop.
    /// Toont eventueel een interstitial vóór het volgende level.
    /// Invariant: if Next is interactable, only isHandlingNextLevel may ignore a click.
    /// </summary>
    public void OnNextLevelButton()
    {
        nextClickCountThisWin++;

        bool nextInteractable =
            nextLevelButton == null || nextLevelButton.interactable;

        string blockReason = null;
        if (isHandlingNextLevel)
        {
            blockReason = "AlreadyNavigating";
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        bool blockerBlocks = difficultyUnlockNotification != null &&
            difficultyUnlockNotification.IsBlockerRaycastActive();
        Debug.Log(
            "[NextButtonTrace]\n" +
            "Stage=NextClicked\n" +
            "ClickCount=" + nextClickCountThisWin + "\n" +
            "CanNavigate=" + (blockReason == null) + "\n" +
            "ReasonIfBlocked=" + (blockReason ?? "None") + "\n" +
            "NextInteractable=" + nextInteractable + "\n" +
            "BlockerBlocksRaycasts=" + blockerBlocks + "\n" +
            "IsHandlingNext=" + isHandlingNextLevel
        );
#endif

        if (blockReason != null)
        {
            return;
        }

        // Valid click: arm navigate guard only now (not earlier).
        isHandlingNextLevel = true;

        EnsureDifficultyUnlockFullyHidden();
        HideWinPanel();

        bool wantsInterstitial =
            gameManager != null &&
            gameManager.ShouldShowInterstitial();

        bool adReady =
            adsManager != null &&
            adsManager.IsInterstitialReady();

        if (wantsInterstitial && adReady)
        {
            gameManager.ResetInterstitialCounter();

            adsManager.ShowInterstitialAd(() =>
            {
                LoadNextLevelOnce();
            });

            return;
        }

        LoadNextLevelOnce();
    }

    private void LoadNextLevelOnce()
    {
        try
        {
            if (levelManager != null)
            {
                levelManager.LoadNextLevel();
            }
        }
        finally
        {
            // Scene may unload this object; if navigation stayed in-scene, allow retry.
            isHandlingNextLevel = false;
        }
    }

    /// <summary>
    /// Wordt aangeroepen door de "Restart"-knop.
    /// </summary>
    public void OnRestartButton()
    {
        if (restartButton != null && !restartButton.interactable)
        {
            return;
        }

        EnsureDifficultyUnlockFullyHidden();
        HideWinPanel();

        if (levelManager != null)
        {
            levelManager.RestartLevel();
        }
    }
}

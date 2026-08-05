using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Bestuurt het win-scherm van Rush Out.
/// Koppel knoppen in de Inspector aan OnNextLevelButton / OnRestartButton.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Het panel dat verschijnt wanneer het level is gehaald.")]
    [SerializeField] private GameObject winPanel;

    [Header("Win Score")]
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text parText;
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

    [Header("Referenties")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AdsManager adsManager;

    private AudioManager audioManager;

    // Voorkomt dubbele Next Level-acties (dubbele klik / dubbele ad-callback).
    private bool isHandlingNextLevel;

    // Lopende win-panel animaties (sterren + coin reward).
    private Coroutine winSequenceCoroutine;
    private Coroutine coinHudPopCoroutine;

    // Home-state van RewardText voor betrouwbare reset.
    private Vector2 rewardHomeAnchoredPos;
    private bool hasRewardHomePos;

    private void Awake()
    {
        audioManager = FindFirstObjectByType<AudioManager>();

        if (rewardTextRect != null)
        {
            rewardHomeAnchoredPos = rewardTextRect.anchoredPosition;
            hasRewardHomePos = true;
        }

        // RewardText start verborgen.
        ResetRewardVisual(hide: true);
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

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        int earnedStars = gameManager != null ? gameManager.LastEarnedStars : 0;
        int earnedCoins = gameManager != null ? gameManager.LastEarnedCoins : 0;
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

        if (coinTextRect != null)
        {
            coinTextRect.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Sterren reveal → daarna coin reward fly (alleen visueel).
    /// </summary>
    private IEnumerator WinSequenceRoutine(int earnedStars, int earnedCoins)
    {
        yield return RevealStarsRoutine(earnedStars);

        if (CanPlayRewardAnimation())
        {
            yield return RewardFlyRoutine(earnedCoins);
        }
        else
        {
            // Geen reward-UI: speel coin-SFX alsnog één keer (GameManager speelt hem niet meer direct).
            audioManager?.PlayCoin();
        }

        winSequenceCoroutine = null;
    }

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

        if (movesText != null)
        {
            movesText.text = "MOVES: " + moves;
        }

        if (parText != null)
        {
            int par = 0;
            LevelData levelData = levelManager != null ? levelManager.CurrentLevelData : null;
            if (levelData != null)
            {
                par = levelData.minimumMoves;
            }

            parText.text = par > 0 ? "PAR: " + par : "PAR: -";
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
    /// </summary>
    public void OnNextLevelButton()
    {
        if (isHandlingNextLevel)
        {
            return;
        }

        HideWinPanel();

        bool wantsInterstitial =
            gameManager != null &&
            gameManager.ShouldShowInterstitial();

        bool adReady =
            adsManager != null &&
            adsManager.IsInterstitialReady();

        if (wantsInterstitial && adReady)
        {
            isHandlingNextLevel = true;
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
        if (levelManager != null)
        {
            levelManager.LoadNextLevel();
        }

        isHandlingNextLevel = false;
    }

    /// <summary>
    /// Wordt aangeroepen door de "Restart"-knop.
    /// </summary>
    public void OnRestartButton()
    {
        HideWinPanel();

        if (levelManager != null)
        {
            levelManager.RestartLevel();
        }
    }
}

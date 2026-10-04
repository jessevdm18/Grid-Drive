using UnityEngine;
using TMPro;

/// <summary>
/// Toont coins, levelnummer, difficulty en moves tijdens Gameplay.
/// Level/Moves: alleen ValueText (cijfers); LabelText blijft "LEVEL" / "MOVES".
/// Global/normal move-limit card shares urgency bands with MoveLimitUrgency.
/// </summary>
public class GameplayUI : MonoBehaviour
{
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private TextMeshProUGUI coinText;

    /// <summary>Bestaande coin counter (spotlight target voor Coins feature tutorial).</summary>
    public RectTransform CoinCounterRect =>
        coinText != null ? coinText.rectTransform : null;

    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameManager gameManager;

    [Tooltip("LevelCard/ValueText — alleen het levelnummer, bijv. \"8\".")]
    [SerializeField] private TextMeshProUGUI levelValueText;

    [Tooltip("MovesCard/ValueText — alleen het aantal moves, bijv. \"0\".")]
    [SerializeField] private TextMeshProUGUI movesValueText;

    [Header("Difficulty Label")]
    [Tooltip("Optional. Shows EASY / MEDIUM / HARD from LevelData.difficulty.")]
    [SerializeField] private TextMeshProUGUI difficultyText;
    [Tooltip("Optional root to show/hide the difficulty label.")]
    [SerializeField] private GameObject difficultyLabelRoot;

    [Header("Global move-limit urgency (normal campaign)")]
    [SerializeField] private Color movesNormalColor = Color.white;
    [SerializeField] private Color movesWarningColor = new Color(1f, 0.78f, 0.15f, 1f);
    [SerializeField] private Color movesCriticalColor = new Color(1f, 0.28f, 0.22f, 1f);
    [SerializeField, Range(0.1f, 8f)] private float movesCriticalPulseSpeed = 2f;
    [SerializeField, Range(1f, 1.25f)] private float movesCriticalPulseScale = 1.08f;
    [SerializeField] private AudioManager audioManager;

    private bool movesPulseActive;
    private Vector3 movesBaseScale = Vector3.one;
    private bool movesScaleCached;
    private bool movesWarningSfxPlayed;
    private int lastMovesRemaining = int.MinValue;
    private bool urgencyActive;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        if (audioManager == null)
        {
            audioManager = AudioManager.Resolve();
        }

        CacheMovesScale();
        UpdateCoinText(coinManager != null ? coinManager.GetCoins() : 0);
        UpdateLevelText();

        // Use GameManager's authoritative formatting (includes "0 / limit" when configured).
        // Do NOT call UpdateMovesText(0) here — that overwrites a prior Configure refresh.
        if (gameManager != null)
        {
            gameManager.SyncMovesHud();
        }
        else
        {
            UpdateMovesText(0);
        }
    }

    private void Update()
    {
        if (!movesPulseActive || movesValueText == null || !urgencyActive)
        {
            return;
        }

        float wave =
            (Mathf.Sin(Time.unscaledTime * movesCriticalPulseSpeed * Mathf.PI * 2f) + 1f) *
            0.5f;
        float scaleMul = Mathf.Lerp(1f, movesCriticalPulseScale, wave);
        movesValueText.rectTransform.localScale = movesBaseScale * scaleMul;
    }

    private void OnEnable()
    {
        if (coinManager != null)
        {
            coinManager.OnCoinsChanged += UpdateCoinText;
        }
    }

    private void OnDisable()
    {
        if (coinManager != null)
        {
            coinManager.OnCoinsChanged -= UpdateCoinText;
        }
    }

    /// <summary>
    /// Gaat terug naar het hoofdmenu zonder progressie te wijzigen.
    /// </summary>
    public void OnBackToMenuButton()
    {
        if (DailyChallengeUiGuard.IsDailyActive)
        {
            DailyChallengeManager.EnsureInstance()?.NotifyAbandoned("gameplay_back");
        }

        SceneTransition.LoadScene("MainMenu");
    }

    /// <summary>
    /// Zet CoinText op alleen het aantal coins (icoon staat apart in de UI).
    /// </summary>
    private void UpdateCoinText(int amount)
    {
        if (coinText != null)
        {
            coinText.text = amount.ToString();
        }
    }

    /// <summary>
    /// Zet LevelCard/ValueText + difficulty label (LevelData.difficulty).
    /// Raakt LabelText ("LEVEL") niet aan.
    /// </summary>
    public void UpdateLevelText()
    {
        if (levelValueText != null && levelManager != null)
        {
            int displayNumber = levelManager.GetDisplayLevelNumber();
            levelValueText.text = displayNumber.ToString();
        }

        UpdateDifficultyLabel();
    }

    /// <summary>
    /// HUD difficulty from authoritative LevelData.difficulty (incl. V1 playtest).
    /// </summary>
    public void UpdateDifficultyLabel()
    {
        if (difficultyText == null && difficultyLabelRoot == null)
        {
            return;
        }

        if (levelManager == null)
        {
            SetDifficultyLabelVisible(false);
            return;
        }

        LevelData data = levelManager.CurrentLevelData;
        if (data == null)
        {
            SetDifficultyLabelVisible(false);
            return;
        }

        if (difficultyText != null)
        {
            difficultyText.text = GetDifficultyLabelCopy(data.difficulty);
        }

        SetDifficultyLabelVisible(true);
    }

    private static string GetDifficultyLabelCopy(LevelDifficulty difficulty)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Medium:
                return "MEDIUM";
            case LevelDifficulty.Hard:
                return "HARD";
            case LevelDifficulty.Easy:
            default:
                return "EASY";
        }
    }

    private void SetDifficultyLabelVisible(bool visible)
    {
        if (difficultyLabelRoot != null)
        {
            difficultyLabelRoot.SetActive(visible);
        }
    }

    /// <summary>
    /// Zet alleen MovesCard/ValueText op het aantal moves.
    /// Raakt LabelText ("MOVES") niet aan.
    /// Optional limit → "12 / 24" for global forgiving OR special MoveLimit maximum.
    /// </summary>
    public void UpdateMovesText(int moves)
    {
        UpdateMovesText(moves, showLimit: false, limit: 0);
    }

    /// <summary>
    /// Moves HUD with optional denominator (global forgiving or special authored limit).
    /// </summary>
    public void UpdateMovesText(int moves, int limit)
    {
        UpdateMovesText(moves, showLimit: true, limit: limit);
    }

    private void UpdateMovesText(int moves, bool showLimit, int limit)
    {
        if (movesValueText == null)
        {
            return;
        }

        if (showLimit && limit > 0)
        {
            movesValueText.text = moves.ToString() + " / " + limit.ToString();
            // Daily: no performance failure — suppress urgency on the moves card.
            bool daily =
                DailyChallengeGameplayPolicy.IsActive ||
                (levelManager != null && levelManager.IsDailyChallengeSession);
            // Urgency bands are for forgiving/global campaign limits only.
            // Special MoveLimit keeps its own MissionUI thresholds/SFX unchanged.
            bool globalLimitActive =
                gameManager != null && gameManager.EffectiveGlobalMoveLimit > 0;
            if (daily || !globalLimitActive)
            {
                ClearMovesUrgency();
            }
            else
            {
                ApplyMovesUrgency(Mathf.Max(0, limit - moves));
            }
        }
        else
        {
            movesValueText.text = moves.ToString();
            ClearMovesUrgency();
        }
    }

    private void ApplyMovesUrgency(int remaining)
    {
        urgencyActive = true;
        CacheMovesScale();

        // Reset one-shot SFX when remaining climbs (restart / undo recovery).
        if (remaining > MoveLimitUrgency.WarningRemaining)
        {
            movesWarningSfxPlayed = false;
        }

        MoveLimitUrgency.Band band = MoveLimitUrgency.Evaluate(remaining);
        movesValueText.color = MoveLimitUrgency.ColorFor(
            band,
            movesNormalColor,
            movesWarningColor,
            movesCriticalColor);

        movesPulseActive = band == MoveLimitUrgency.Band.Critical;
        if (!movesPulseActive)
        {
            movesValueText.rectTransform.localScale = movesBaseScale;
        }

        // One warning SFX when first entering warning band (not every frame / move).
        if (band != MoveLimitUrgency.Band.Normal &&
            !movesWarningSfxPlayed &&
            remaining <= MoveLimitUrgency.WarningRemaining &&
            (lastMovesRemaining == int.MinValue ||
             lastMovesRemaining > MoveLimitUrgency.WarningRemaining))
        {
            movesWarningSfxPlayed = true;
            if (audioManager == null)
            {
                audioManager = AudioManager.Resolve();
            }

            audioManager?.PlayMoveLimitWarning();
        }

        lastMovesRemaining = remaining;
    }

    private void ClearMovesUrgency()
    {
        urgencyActive = false;
        movesPulseActive = false;
        movesWarningSfxPlayed = false;
        lastMovesRemaining = int.MinValue;
        if (movesValueText == null)
        {
            return;
        }

        movesValueText.color = movesNormalColor;
        if (movesScaleCached)
        {
            movesValueText.rectTransform.localScale = movesBaseScale;
        }
    }

    private void CacheMovesScale()
    {
        if (movesScaleCached || movesValueText == null)
        {
            return;
        }

        movesBaseScale = movesValueText.rectTransform.localScale;
        if (movesBaseScale.sqrMagnitude < 0.0001f)
        {
            movesBaseScale = Vector3.one;
        }

        movesScaleCached = true;
    }
}

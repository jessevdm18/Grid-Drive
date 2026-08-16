using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private AdsManager adsManager;
    [SerializeField] private GameplayUI gameplayUI;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private HintManager hintManager;

    private AudioManager audioManager;
    private LevelObjectiveController levelObjectiveController;

    // Voorkomt dat CompleteLevel meerdere keren voor hetzelfde level draait.
    private bool levelCompleted;

    // Timed mission failure (geen rewards / unlock).
    private bool levelFailed;

    // Target-exit animatie gestart — fail mag niet meer winnen van win.
    private bool targetExitInProgress;

    // Telt voltooide levels sinds de laatste interstitial.
    private int completedLevelsSinceAd = 0;

    // Aantal geldige voertuig-moves in het huidige level.
    private int currentMoves = 0;

    public int CurrentMoves => currentMoves;
    public bool IsLevelCompleted => levelCompleted;
    public bool IsLevelFailed => levelFailed;
    public bool IsTargetExitInProgress => targetExitInProgress;

    /// <summary>
    /// False bij completed/failed — blokkeert nieuwe vehicle-input.
    /// </summary>
    public bool CanAcceptVehicleInput => !levelCompleted && !levelFailed;

    /// <summary>
    /// Coin-beloning bij level completion (één bron van waarheid voor UI + uitbetaling).
    /// </summary>
    public const int LevelCompleteCoinReward = 50;

    /// <summary>
    /// Sterren verdiend bij de laatste succesvolle CompleteLevel (1–3).
    /// </summary>
    public int LastEarnedStars { get; private set; }

    /// <summary>
    /// Coins verdiend bij de laatste CompleteLevel (voor WinPanel-animatie).
    /// </summary>
    public int LastEarnedCoins { get; private set; }

    private void Awake()
    {
        audioManager = FindFirstObjectByType<AudioManager>();

        if (gameplayUI == null)
        {
            gameplayUI = FindFirstObjectByType<GameplayUI>();
        }

        if (levelManager == null)
        {
            levelManager = FindFirstObjectByType<LevelManager>();
        }

        if (saveManager == null)
        {
            saveManager = FindFirstObjectByType<SaveManager>();
        }

        if (hintManager == null)
        {
            hintManager = FindFirstObjectByType<HintManager>();
        }

        if (levelObjectiveController == null)
        {
            levelObjectiveController = FindFirstObjectByType<LevelObjectiveController>();
        }
    }

    /// <summary>
    /// Registreert één move (één drag naar een andere gridpositie, of exit).
    /// </summary>
    public void RegisterMove()
    {
        currentMoves++;
        Debug.Log("Move registered. Total moves: " + currentMoves);

        if (gameplayUI != null)
        {
            gameplayUI.UpdateMovesText(currentMoves);
        }

        if (levelObjectiveController == null)
        {
            levelObjectiveController = FindFirstObjectByType<LevelObjectiveController>();
        }

        levelObjectiveController?.NotifyValidMove();
    }

    /// <summary>
    /// Reset de move-teller (restart / nieuw level).
    /// </summary>
    public void ResetMoves()
    {
        currentMoves = 0;

        if (gameplayUI != null)
        {
            gameplayUI.UpdateMovesText(currentMoves);
        }

        if (hintManager != null)
        {
            hintManager.ClearCurrentHint();
        }
    }

    /// <summary>
    /// Markeert dat de target-exit-animatie is gestart (vóór CompleteLevel).
    /// Voorkomt timed-fail tijdens wegrijden.
    /// </summary>
    public void NotifyTargetExitStarted()
    {
        if (levelFailed || levelCompleted)
        {
            return;
        }

        targetExitInProgress = true;
    }

    /// <summary>
    /// Wordt aangeroepen wanneer de doelauto succesvol via de exit ontsnapt.
    /// </summary>
    public void CompleteLevel()
    {
        if (levelCompleted)
        {
            return;
        }

        // Fail vóór exit-start blokkeert win. Exit-in-progress wint de race.
        if (levelFailed && !targetExitInProgress)
        {
            return;
        }

        levelCompleted = true;
        levelFailed = false;
        targetExitInProgress = true;

        if (levelObjectiveController == null)
        {
            levelObjectiveController = FindFirstObjectByType<LevelObjectiveController>();
        }

        levelObjectiveController?.NotifyLevelCompleted();

        if (hintManager != null)
        {
            hintManager.ClearCurrentHint();
        }

        Debug.Log("LEVEL COMPLETED!");

        LastEarnedStars = CalculateStars(out int parMoves);
        Debug.Log(
            "Level complete in " + currentMoves +
            " moves. Par = " + parMoves +
            ". Stars = " + LastEarnedStars
        );

        // Sterren opslaan: één keer per level (beschermd door levelCompleted).
        if (saveManager != null && levelManager != null)
        {
            int completedIndex = levelManager.CurrentLevelIndex;
            saveManager.SaveStarsForLevel(completedIndex, LastEarnedStars);

            // Unlock volgende level meteen bij completion (niet pas bij Next Level).
            int nextUnlock = completedIndex + 1;
            int maxIndex = Mathf.Max(0, levelManager.LevelCount - 1);
            nextUnlock = Mathf.Clamp(nextUnlock, 0, maxIndex);

            Debug.Log(
                "Completed level " + completedIndex +
                " -> unlocking level " + nextUnlock
            );

            saveManager.SaveUnlockedLevel(nextUnlock);
        }

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif

        // Beloning: één keer per level (beschermd door levelCompleted).
        // Coin-SFX speelt bij aankomst van de WinPanel reward-animatie (geen dubbel geluid).
        LastEarnedCoins = LevelCompleteCoinReward;
        if (coinManager != null)
        {
            coinManager.AddCoins(LastEarnedCoins);
        }

        audioManager?.PlayWin();

        // Interstitial-teller: één keer per level (beschermd door levelCompleted).
        completedLevelsSinceAd++;
        Debug.Log("Completed levels since ad: " + completedLevelsSinceAd);

        if (uiManager != null)
        {
            uiManager.ShowWinPanel();
        }
    }

    /// <summary>
    /// Special mission failed: geen win, unlock, stars of reward.
    /// </summary>
    public void FailLevel()
    {
        if (levelCompleted || levelFailed || targetExitInProgress)
        {
            return;
        }

        levelFailed = true;

        if (hintManager != null)
        {
            hintManager.ClearCurrentHint();
        }

        Debug.Log("LEVEL FAILED (special mission).");
    }

    /// <summary>
    /// 3★ ≤ par, 2★ ≤ par+2, anders 1★.
    /// </summary>
    private int CalculateStars(out int parMoves)
    {
        parMoves = 0;

        LevelData levelData = levelManager != null ? levelManager.CurrentLevelData : null;
        if (levelData != null)
        {
            parMoves = levelData.minimumMoves;
        }

        if (currentMoves <= parMoves)
        {
            return 3;
        }

        if (currentMoves <= parMoves + 2)
        {
            return 2;
        }

        return 1;
    }

    /// <summary>
    /// True als er al 3 levels voltooid zijn sinds de laatste interstitial.
    /// </summary>
    public bool ShouldShowInterstitial()
    {
        return completedLevelsSinceAd >= 3;
    }

    /// <summary>
    /// Zet de interstitial-teller terug op 0 (na een getoonde advertentie).
    /// </summary>
    public void ResetInterstitialCounter()
    {
        completedLevelsSinceAd = 0;
    }

    /// <summary>
    /// Reset de win-vlag zodat een nieuw/herstart level opnieuw gewonnen kan worden.
    /// Wordt aangeroepen bij level load/restart via VehicleController.Setup.
    /// </summary>
    public void ResetLevelCompleted()
    {
        levelCompleted = false;
        levelFailed = false;
        targetExitInProgress = false;
    }
}

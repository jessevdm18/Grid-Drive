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
    [SerializeField] private GameplayUndoManager undoManager;

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
    public bool IsSpecialMissionIntroPlaying => specialMissionIntroBlocked;
    public bool IsObjectiveTutorialPlaying => objectiveTutorialBlocked;
    public bool IsFeatureTutorialPlaying => featureTutorialBlocked;

    /// <summary>
    /// False bij completed/failed/special-intro/objective/feature tutorial — blokkeert vehicle-input.
    /// </summary>
    public bool CanAcceptVehicleInput =>
        !levelCompleted &&
        !levelFailed &&
        !specialMissionIntroBlocked &&
        !objectiveTutorialBlocked &&
        !featureTutorialBlocked;

    // Special mission label-intro: tijdelijke input-gate (naast fail/complete).
    private bool specialMissionIntroBlocked;

    // First-time objective tutorial: aparte gate naast special intro.
    private bool objectiveTutorialBlocked;

    // Feature tutorial (Undo/Hint/Coins/Skins): aparte gate naast objective tutorial.
    private bool featureTutorialBlocked;

    /// <summary>
    /// Sterren verdiend bij de laatste succesvolle CompleteLevel (1–3).
    /// </summary>
    public int LastEarnedStars { get; private set; }

    /// <summary>
    /// Coins verdiend bij de laatste CompleteLevel (voor WinPanel-animatie).
    /// 0 als geen 3★ of reward al eerder geclaimd.
    /// </summary>
    public int LastEarnedCoins { get; private set; }

    /// <summary>
    /// True als CompleteLevel zojuist de eenmalige 3★ coin reward heeft uitgekeerd.
    /// Blijft true tot de volgende CompleteLevel (niet gereset vóór win-sequence).
    /// </summary>
    public bool LastThreeStarCoinRewardGranted { get; private set; }

    /// <summary>
    /// Consumable trigger voor Coins feature tutorial (tot queue/consume).
    /// Los van LastThreeStarCoinRewardGranted zodat UI-timing niet fragiel is.
    /// </summary>
    private bool coinsFeatureTutorialTriggerPending;

    public bool HasPendingCoinsFeatureTutorialTrigger => coinsFeatureTutorialTriggerPending;

    /// <summary>
    /// Pending one-time Medium/Hard unlock toast after this completion (null if none).
    /// </summary>
    public LevelDifficulty? PendingDifficultyUnlockNotice { get; private set; }

    /// <summary>
    /// True als het huidige level bij 3★ nog een coin reward zou geven.
    /// Voor latere WinPanel-copy ("GET 3 STARS TO EARN COINS").
    /// </summary>
    public bool IsThreeStarCoinRewardAvailableForCurrentLevel
    {
        get
        {
            if (saveManager == null || levelManager == null)
            {
                return false;
            }

            return saveManager.IsThreeStarCoinRewardAvailable(levelManager.CurrentLevelIndex);
        }
    }

    /// <summary>
    /// Consumeert de pending Coins-tutorial trigger (één keer).
    /// </summary>
    public bool TryConsumeCoinsFeatureTutorialTrigger()
    {
        if (!coinsFeatureTutorialTriggerPending)
        {
            return false;
        }

        coinsFeatureTutorialTriggerPending = false;
        return true;
    }

    /// <summary>
    /// Consumeert pending difficulty-unlock toast (één keer).
    /// </summary>
    public bool TryConsumePendingDifficultyUnlockNotice(out LevelDifficulty difficulty)
    {
        if (!PendingDifficultyUnlockNotice.HasValue)
        {
            difficulty = LevelDifficulty.Easy;
            return false;
        }

        difficulty = PendingDifficultyUnlockNotice.Value;
        PendingDifficultyUnlockNotice = null;
        return true;
    }

    private void Awake()
    {
        audioManager = FindAnyObjectByType<AudioManager>();

        if (gameplayUI == null)
        {
            gameplayUI = FindAnyObjectByType<GameplayUI>();
        }

        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        if (saveManager == null)
        {
            saveManager = FindAnyObjectByType<SaveManager>();
        }

        if (hintManager == null)
        {
            hintManager = FindAnyObjectByType<HintManager>();
        }

        if (levelObjectiveController == null)
        {
            levelObjectiveController = FindAnyObjectByType<LevelObjectiveController>();
        }

        if (undoManager == null)
        {
            undoManager = FindAnyObjectByType<GameplayUndoManager>();
        }

        EnsureVehicleInputRouter();
    }

    private void EnsureVehicleInputRouter()
    {
        if (VehicleInputRouter.Instance != null ||
            GetComponent<VehicleInputRouter>() != null ||
            FindAnyObjectByType<VehicleInputRouter>() != null)
        {
            return;
        }

        gameObject.AddComponent<VehicleInputRouter>();
    }

    private void Start()
    {
        // Authoritative gameplay music — covers Splash→Gameplay fresh launch
        // (MainMenu MusicManager never loaded) and Menu→Gameplay.
        MusicManager.PlayGameplayMusic();

        if (saveManager != null && levelManager != null)
        {
            DifficultyUnlockNoticePrefs.MigrateExistingUnlocks(
                saveManager,
                levelManager.LevelDatabase,
                levelManager.DifficultyProgressionConfig
            );
        }
    }

    /// <summary>
    /// Registreert één move (één drag naar een andere gridpositie, of exit).
    /// vehicle: het voertuig dat de geldige move uitvoerde (voor NoTouchChallenge).
    /// </summary>
    public void RegisterMove(VehicleController vehicle)
    {
        currentMoves++;
        Debug.Log("Move registered. Total moves: " + currentMoves);

        if (hintManager != null)
        {
            hintManager.NotifyPlayerMove();
        }

        if (gameplayUI != null)
        {
            gameplayUI.UpdateMovesText(currentMoves);
        }

        if (levelObjectiveController == null)
        {
            levelObjectiveController = FindAnyObjectByType<LevelObjectiveController>();
        }

        levelObjectiveController?.NotifyValidMove(vehicle);
    }

    /// <summary>
    /// Draait één eerder geregistreerde geldige move terug (geen nieuwe move).
    /// </summary>
    public void UndoRegisteredMove()
    {
        currentMoves = Mathf.Max(0, currentMoves - 1);

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

        ClearUndoHistory();
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
        ClearUndoHistory();
    }

    /// <summary>
    /// Einde exit-animatie. Classic/Timed/MoveLimit → CompleteLevel.
    /// MultiTargetRescue → CompleteLevel alleen na laatste target.
    /// </summary>
    public void NotifyTargetExitFinished(VehicleController vehicle)
    {
        if (levelCompleted || levelFailed)
        {
            return;
        }

        if (levelObjectiveController == null)
        {
            levelObjectiveController = FindAnyObjectByType<LevelObjectiveController>();
        }

        if (levelObjectiveController != null &&
            levelObjectiveController.IsMultiTargetRescueLevel)
        {
            bool allRescued = levelObjectiveController.NotifyTargetRescued(vehicle);
            if (!allRescued)
            {
                // Tussentijdse rescue: gameplay gaat door.
                targetExitInProgress = false;
                return;
            }
        }

        CompleteLevel();
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
            levelObjectiveController = FindAnyObjectByType<LevelObjectiveController>();
        }

        levelObjectiveController?.NotifyLevelCompleted();

        if (hintManager != null)
        {
            hintManager.ClearCurrentHint();
        }

        ClearUndoHistory();

        Debug.Log("LEVEL COMPLETED!");

        LastEarnedStars = CalculateStars(out int parMoves);
        Debug.Log(
            "Level complete in " + currentMoves +
            " moves. Par = " + parMoves +
            ". Stars = " + LastEarnedStars
        );

        ReportLevelCompleteTelemetry(parMoves);

        LastThreeStarCoinRewardGranted = false;
        LastEarnedCoins = 0;
        coinsFeatureTutorialTriggerPending = false;
        PendingDifficultyUnlockNotice = null;

        FeatureTutorialController.BeginCoinsFtSession();

        // Reward VOORDAT stars worden opgeslagen — anders kan one-shot migration
        // net-opgeslagen 3★ als "legacy claimed" markeren zonder payout.
        if (saveManager != null && levelManager != null)
        {
            // Editor V1 candidate playtest: geen DB-index → geen progression/economy writes.
            if (levelManager.IsV1CandidatePlaytest)
            {
                Debug.Log(
                    "CompleteLevel: V1 candidate playtest — skipping stars/coins/unlock writes."
                );
            }
            else
            {
                int completedIndex = levelManager.CurrentLevelIndex;
                int previousBestStars = saveManager.GetStarsForLevel(completedIndex);
                bool claimedBefore = saveManager.HasClaimedThreeStarCoinReward(completedIndex);
                bool eligible = LastEarnedStars == 3 && !claimedBefore;

                LevelDatabase database = levelManager.LevelDatabase;
                DifficultyProgressionConfig progressionConfig =
                    levelManager.DifficultyProgressionConfig;

                bool mediumUnlockedBefore = saveManager.IsDifficultyUnlocked(
                    LevelDifficulty.Medium,
                    database,
                    progressionConfig
                );
                bool hardUnlockedBefore = saveManager.IsDifficultyUnlocked(
                    LevelDifficulty.Hard,
                    database,
                    progressionConfig
                );

                if (eligible)
                {
                    LevelDifficulty rewardDifficulty =
                        levelManager.CurrentDifficulty;
                    LevelData completedData = levelManager.CurrentLevelData;
                    if (completedData != null)
                    {
                        rewardDifficulty = completedData.difficulty;
                    }

                    LastEarnedCoins = ThreeStarCoinReward.GetReward(rewardDifficulty);
                    saveManager.MarkThreeStarCoinRewardClaimed(completedIndex);
                    LastThreeStarCoinRewardGranted = true;
                    coinsFeatureTutorialTriggerPending = true;

                    if (coinManager != null)
                    {
                        coinManager.AddCoins(LastEarnedCoins);
                        GameAnalytics.LogCoinReward(LastEarnedCoins, "three_star");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.Log(
                            "[CoinsFT]\n" +
                            "Stage=CoinsAdded\n" +
                            "Session=" + FeatureTutorialController.CoinsFtSessionId + "\n" +
                            "Amount=" + LastEarnedCoins + "\n" +
                            "BalanceAfter=" + coinManager.GetCoins()
                        );
#endif
                    }
                }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                LevelDifficulty logDifficulty = levelManager.CurrentDifficulty;
                LevelData logData = levelManager.CurrentLevelData;
                if (logData != null)
                {
                    logDifficulty = logData.difficulty;
                }

                Debug.Log(
                    "[ThreeStarReward]\n" +
                    "LevelIndex=" + completedIndex + "\n" +
                    "Difficulty=" + logDifficulty + "\n" +
                    "Stars=" + LastEarnedStars + "\n" +
                    "ClaimedBefore=" + claimedBefore + "\n" +
                    "Eligible=" + eligible + "\n" +
                    "Reward=" + (eligible ? LastEarnedCoins : 0)
                );

                Debug.Log(
                    "[CoinsFT]\n" +
                    "Stage=CompleteLevel\n" +
                    "Session=" + FeatureTutorialController.CoinsFtSessionId + "\n" +
                    "LevelIndex=" + completedIndex + "\n" +
                    "Difficulty=" + logDifficulty + "\n" +
                    "ResultStars=" + LastEarnedStars + "\n" +
                    "PreviousBest=" + previousBestStars + "\n" +
                    "ClaimedBefore=" + claimedBefore + "\n" +
                    "RewardGranted=" + LastThreeStarCoinRewardGranted + "\n" +
                    "LastEarnedCoins=" + LastEarnedCoins + "\n" +
                    "LastThreeStarCoinRewardGranted=" + LastThreeStarCoinRewardGranted
                );
#endif

                saveManager.SaveStarsForLevel(completedIndex, LastEarnedStars);

                // Difficulty unlock toast: only on unique first completion + false→true.
                if (previousBestStars <= 0)
                {
                    bool mediumUnlockedAfter = saveManager.IsDifficultyUnlocked(
                        LevelDifficulty.Medium,
                        database,
                        progressionConfig
                    );
                    bool hardUnlockedAfter = saveManager.IsDifficultyUnlocked(
                        LevelDifficulty.Hard,
                        database,
                        progressionConfig
                    );

                    if (!hardUnlockedBefore && hardUnlockedAfter)
                    {
                        GameAnalytics.LogDifficultyUnlocked(LevelDifficulty.Hard.ToString());
                        if (!DifficultyUnlockNoticePrefs.HasSeenHard())
                        {
                            PendingDifficultyUnlockNotice = LevelDifficulty.Hard;
                        }
                    }
                    else if (!mediumUnlockedBefore && mediumUnlockedAfter)
                    {
                        GameAnalytics.LogDifficultyUnlocked(LevelDifficulty.Medium.ToString());
                        if (!DifficultyUnlockNoticePrefs.HasSeenMedium())
                        {
                            PendingDifficultyUnlockNotice = LevelDifficulty.Medium;
                        }
                    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log(
                        "[DifficultyUnlock]\n" +
                        "UniqueCompletion=" + (previousBestStars <= 0) + "\n" +
                        "Medium " + mediumUnlockedBefore + "→" + mediumUnlockedAfter + "\n" +
                        "Hard " + hardUnlockedBefore + "→" + hardUnlockedAfter + "\n" +
                        "PendingNotice=" +
                        (PendingDifficultyUnlockNotice.HasValue
                            ? PendingDifficultyUnlockNotice.Value.ToString()
                            : "None")
                    );
#endif
                }

                int nextUnlock = completedIndex + 1;
                int maxIndex = Mathf.Max(0, levelManager.LevelCount - 1);
                nextUnlock = Mathf.Clamp(nextUnlock, 0, maxIndex);

                Debug.Log(
                    "Completed level " + completedIndex +
                    " -> unlocking level " + nextUnlock
                );

                saveManager.SaveUnlockedLevel(nextUnlock);
            }
        }

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif

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
    public void FailLevel(string failReason = "mission_failed")
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

        ClearUndoHistory();

        ReportLevelFailTelemetry(failReason);

        Debug.Log("LEVEL FAILED (special mission).");
    }

    private void ReportLevelCompleteTelemetry(int parMoves)
    {
        if (levelManager == null)
        {
            return;
        }

        LevelData data = levelManager.CurrentLevelData;
        string difficulty = data != null
            ? data.difficulty.ToString()
            : levelManager.CurrentDifficulty.ToString();
        string objective = data != null
            ? data.objectiveType.ToString()
            : "unknown";
        string asset = data != null ? data.name : null;

        GameAnalytics.LogLevelComplete(
            levelManager.GetDisplayLevelNumber(),
            difficulty,
            objective,
            currentMoves,
            parMoves,
            LastEarnedStars,
            asset
        );
    }

    private void ReportLevelFailTelemetry(string failReason)
    {
        if (levelManager == null)
        {
            return;
        }

        LevelData data = levelManager.CurrentLevelData;
        string difficulty = data != null
            ? data.difficulty.ToString()
            : levelManager.CurrentDifficulty.ToString();
        string objective = data != null
            ? data.objectiveType.ToString()
            : "unknown";
        int display = levelManager.GetDisplayLevelNumber();

        GameAnalytics.LogLevelFail(
            display,
            difficulty,
            objective,
            failReason,
            currentMoves
        );
        GameAnalytics.LogObjectiveFailed(
            display,
            difficulty,
            objective,
            failReason
        );
    }

    private void ClearUndoHistory()
    {
        if (undoManager == null)
        {
            undoManager = FindAnyObjectByType<GameplayUndoManager>();
        }

        undoManager?.ClearHistory();
    }

    /// <summary>
    /// 3★ ≤ par, 2★ ≤ par+2, anders 1★.
    /// Uses LevelMinMoves — never treats stored -1 as PAR.
    /// </summary>
    private int CalculateStars(out int parMoves)
    {
        LevelData levelData = levelManager != null ? levelManager.CurrentLevelData : null;
        int dbIndex = levelManager != null ? levelManager.CurrentLevelIndex : -1;
        LevelMinMoves.LogInvalidIfNeeded(levelData, dbIndex, "CompleteLevel/CalculateStars");
        return LevelMinMoves.CalculateStars(currentMoves, levelData, out parMoves);
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
    /// Special mission intro: blokkeer/deblokkeer vehicle-input.
    /// Vervangt Failed/Completed/Pause niet — werkt ernaast.
    /// </summary>
    public void SetSpecialMissionIntroBlocked(bool blocked)
    {
        specialMissionIntroBlocked = blocked;
    }

    /// <summary>
    /// First-time objective tutorial: blokkeer/deblokkeer vehicle-input.
    /// Aparte gate naast special intro.
    /// </summary>
    public void SetObjectiveTutorialBlocked(bool blocked)
    {
        objectiveTutorialBlocked = blocked;
    }

    /// <summary>
    /// Feature tutorial (Undo/Hint/Coins/Skins): blokkeer/deblokkeer vehicle-input.
    /// Aparte gate naast objective tutorial / special intro.
    /// </summary>
    public void SetFeatureTutorialBlocked(bool blocked)
    {
        featureTutorialBlocked = blocked;
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
        specialMissionIntroBlocked = false;
        objectiveTutorialBlocked = false;
        featureTutorialBlocked = false;
    }
}

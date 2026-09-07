using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Laadt levels via LevelDatabase en spawnt alle voertuigen.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Levels")]
    [Tooltip("Centrale database met alle LevelData-assets.")]
    [SerializeField] private LevelDatabase levelDatabase;

    [Tooltip("Welk level nu actief is (index in de database).")]
    [SerializeField] private int currentLevelIndex = 0;

    [Header("Prefabs & Referenties")]
    [Tooltip("Prefab van een voertuig (bijv. Car_Player).")]
    [SerializeField] private VehicleController vehiclePrefab;

    [SerializeField] private GridManager gridManager;

    [SerializeField] private GameManager gameManager;

    [SerializeField] private SaveManager saveManager;

    [Tooltip("Optioneel. Null = built-in unlock defaults (10 Easy / 10 Easy + 10 Medium).")]
    [SerializeField] private DifficultyProgressionConfig difficultyProgressionConfig;

    [SerializeField] private GameplayUI gameplayUI;

    [Tooltip("Onder dit Transform komen alle gespawnde voertuigen.")]
    [SerializeField] private Transform vehicleParent;

    [Tooltip("Visuele Exit in de scene (alleen Y volgt exitRow).")]
    [SerializeField] private Transform exitVisual;

    [Tooltip("Optionele visuele parkeerplaats (tiles + borders).")]
    [SerializeField] private ParkingGridVisual parkingGridVisual;

    [Tooltip("Past camera framing aan op gridgrootte.")]
    [SerializeField] private CameraFitter cameraFitter;

    [Tooltip("Optionele sprite-library voor automatische voertuig-visuals.")]
    [SerializeField] private VehicleSpriteLibrary vehicleSpriteLibrary;

    [Tooltip("Centrale special-mission target sprites (TimedAmbulance / FragileCargo).")]
    [SerializeField] private SpecialMissionSpriteLibrary specialMissionSpriteLibrary;

    [Tooltip("Timed/special objectives (optioneel).")]
    [SerializeField] private LevelObjectiveController levelObjectiveController;

    // Actieve library voor deze load (skin overlay of inspector-default).
    private VehicleSpriteLibrary activeVehicleSpriteLibrary;

    // Laatst gekozen auto-sprite (voorkomt twee dezelfde achter elkaar).
    private Sprite lastAutoAssignedSprite;

    // Stabiele spawn-order (zelfde volgorde als LevelData.vehicles).
    // HintManager / solver gebruiken deze lijst — nooit her-sorteren op positie.
    private readonly List<VehicleController> activeVehicles = new List<VehicleController>();

    // Editor V1 playtest: direct LevelData (niet via MainLevelDatabase index).
    private LevelData editorPlaytestLevel;

    private SkinManager skinManager;

    /// <summary>
    /// Zero-based index van het actieve level (0 = LEVEL 1).
    /// -1 tijdens Editor V1 candidate playtest (geen DB index).
    /// </summary>
    public int CurrentLevelIndex => currentLevelIndex;

    /// <summary>Alias: authoritative save identity index.</summary>
    public int CurrentDatabaseIndex => currentLevelIndex;

    /// <summary>
    /// Difficulty of the active LevelData, or Easy if none.
    /// </summary>
    public LevelDifficulty CurrentDifficulty
    {
        get
        {
            LevelData data = CurrentLevelData;
            return data != null ? data.difficulty : LevelDifficulty.Easy;
        }
    }

    /// <summary>
    /// Difficulty-local display number (1..N within tier). Matches LevelSelect.
    /// </summary>
    public int CurrentDifficultyDisplayNumber => GetDisplayLevelNumber();

    /// <summary>
    /// Editor-only: true wanneer Gameplay via V1 direct-candidate override is geladen.
    /// </summary>
    public bool IsV1CandidatePlaytest => editorPlaytestLevel != null;

    /// <summary>
    /// Actieve voertuigen in LevelData-spawnvolgorde (stabiele solver-indices).
    /// </summary>
    public IReadOnlyList<VehicleController> ActiveVehicles => activeVehicles;

    /// <summary>
    /// Aantal levels in de database.
    /// </summary>
    public int LevelCount => levelDatabase != null ? levelDatabase.LevelCount : 0;

    /// <summary>Authoritative LevelDatabase reference (unlock / progression).</summary>
    public LevelDatabase LevelDatabase => levelDatabase;

    /// <summary>Optional progression thresholds; null = built-in defaults.</summary>
    public DifficultyProgressionConfig DifficultyProgressionConfig =>
        difficultyProgressionConfig;

    /// <summary>
    /// LevelData van het actieve level, of null.
    /// </summary>
    public LevelData CurrentLevelData
    {
        get
        {
            if (editorPlaytestLevel != null)
            {
                return editorPlaytestLevel;
            }

            if (levelDatabase == null)
            {
                return null;
            }

            return levelDatabase.GetLevel(currentLevelIndex);
        }
    }

    /// <summary>
    /// UI display number matching LevelSelect local numbering within difficulty.
    /// Candidate playtest: LevelData.levelNumber fallback.
    /// </summary>
    public int GetDisplayLevelNumber()
    {
        if (editorPlaytestLevel != null)
        {
            return editorPlaytestLevel.levelNumber;
        }

        if (levelDatabase == null || currentLevelIndex < 0)
        {
            return 0;
        }

        return LevelDifficultyOrder.GetDifficultyDisplayNumber(levelDatabase, currentLevelIndex);
    }

    private void OnEnable()
    {
        ResolveSkinManager();
        if (skinManager != null)
        {
            skinManager.OnSkinSelected += OnSelectedSkinChanged;
        }
    }

    private void OnDisable()
    {
        if (skinManager != null)
        {
            skinManager.OnSkinSelected -= OnSelectedSkinChanged;
        }
    }

    private void Start()
    {
        int serializedSnapshot = currentLevelIndex;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[FreshStartTrace]\n" +
            "Stage=LevelManagerStart\n" +
            "SavedDbIndex=" +
            (saveManager != null
                ? saveManager.GetCurrentLevel().ToString()
                : "no-SaveManager") + "\n" +
            "HasCurrentLevelKey=" + SaveManager.HasCurrentLevelKey() + "\n" +
            "SerializedCurrentLevelIndex=" + serializedSnapshot + "\n" +
            "PendingPlaytestOverride=" + V1PlaytestOverride.HasPending +
            " (" + V1PlaytestOverride.GetPendingGuid() + ")\n" +
            "ActivePlaytestOverride=" + V1PlaytestOverride.HasActive +
            " (" + V1PlaytestOverride.GetActiveGuid() + ")\n" +
            "FirstLaunchCompletedKey=" + SaveManager.HasFirstLaunchCompletedKeyRaw()
        );
#endif

#if UNITY_EDITOR
        // Fresh / first-launch must never be hijacked by a leftover V1 playtest Pending GUID.
        if (!SaveManager.HasFirstLaunchCompletedKeyRaw())
        {
            if (V1PlaytestOverride.HasPending || V1PlaytestOverride.HasActive)
            {
                Debug.LogWarning(
                    "[FreshStartTrace] Clearing V1 playtest override during first-launch."
                );
                V1PlaytestOverride.Clear();
            }
        }
        else if (V1PlaytestOverride.TryConsumePendingLevel(out LevelData overrideLevel))
        {
            editorPlaytestLevel = overrideLevel;
            currentLevelIndex = -1;
            LoadLevelData(overrideLevel);
            MarkFirstLaunchDone();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                "[FreshStartTrace]\n" +
                "Stage=FinalLevelResolved\n" +
                "Source=V1Override\n" +
                "DbIndex=-1\n" +
                "Asset=" + overrideLevel.name + "\n" +
                "Difficulty=" + overrideLevel.difficulty + "\n" +
                "DisplayNumber=" + overrideLevel.levelNumber
            );
#endif
            return;
        }
#endif

        // Authoritative: SaveManager only. Serialized Inspector index is never used as source.
        // Safety net if Splash was skipped (Editor Play from Gameplay).
        LevelDatabaseContentVersion.ApplyIfNeeded();

        if (saveManager != null)
        {
            currentLevelIndex = saveManager.GetCurrentLevel();
        }
        else if (levelDatabase != null)
        {
            currentLevelIndex = SaveManager.ResolveFreshStartDatabaseIndex(levelDatabase);
            Debug.LogWarning(
                "LevelManager: no SaveManager — using ResolveFreshStartDatabaseIndex."
            );
        }
        else
        {
            currentLevelIndex = 0;
            Debug.LogError("LevelManager: no SaveManager and no LevelDatabase.");
        }

        if (levelDatabase != null && levelDatabase.LevelCount > 0)
        {
            if (currentLevelIndex < 0 || currentLevelIndex >= levelDatabase.LevelCount)
            {
                Debug.LogWarning(
                    "[LevelLoadTrace] CurrentLevel out of bounds (" +
                    currentLevelIndex + ") — falling back to first Easy."
                );
                currentLevelIndex = SaveManager.ResolveFreshStartDatabaseIndex(levelDatabase);
                if (saveManager != null)
                {
                    saveManager.SaveCurrentLevel(currentLevelIndex);
                }
            }
            else
            {
                currentLevelIndex = Mathf.Clamp(
                    currentLevelIndex,
                    0,
                    levelDatabase.LevelCount - 1
                );
            }

            // Stale save / mismatch: difficulty+order gate — never play a locked tier level.
            if (saveManager != null &&
                editorPlaytestLevel == null &&
                !saveManager.IsLevelUnlocked(
                    currentLevelIndex,
                    levelDatabase,
                    difficultyProgressionConfig))
            {
                int safeIndex = SaveManager.ResolveFreshStartDatabaseIndex(levelDatabase);
                Debug.LogWarning(
                    "[LevelLoadTrace] CurrentLevel dbIndex=" + currentLevelIndex +
                    " is locked — falling back to first Easy dbIndex=" + safeIndex
                );
                currentLevelIndex = safeIndex;
                saveManager.SaveCurrentLevel(currentLevelIndex);
            }
        }
        else
        {
            currentLevelIndex = 0;
        }

        LoadCurrentLevel();
        MarkFirstLaunchDone();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LevelData loaded = CurrentLevelData;
        Debug.Log(
            "[FreshStartTrace]\n" +
            "Stage=FinalLevelResolved\n" +
            "Source=SaveManager\n" +
            "DbIndex=" + currentLevelIndex + "\n" +
            "Asset=" + (loaded != null ? loaded.name : "?") + "\n" +
            "Difficulty=" + (loaded != null ? loaded.difficulty.ToString() : "?") + "\n" +
            "DisplayNumber=" + GetDisplayLevelNumber() + "\n" +
            "SerializedWasIgnored=" + serializedSnapshot
        );
#endif
    }

    private void MarkFirstLaunchDone()
    {
        // First-launch routing is klaar zodra Gameplay level succesvol start.
        // Classic tutorial blijft eigen seen-state houden.
        if (saveManager != null)
        {
            saveManager.MarkFirstLaunchCompleted();
        }
        else
        {
            SaveManager.MarkFirstLaunchCompletedStatic();
        }
    }

    /// <summary>
    /// Laadt het level op currentLevelIndex opnieuw.
    /// Wijzigt currentLevelIndex niet en raakt SaveManager niet aan.
    /// </summary>
    public void LoadCurrentLevel()
    {
        LoadLevel();
    }

    /// <summary>
    /// Laadt het volgende level binnen dezelfde difficulty (difficulty-local order).
    /// Geen auto-switch naar Medium/Hard. Geen volgend same-difficulty → LevelSelect.
    /// </summary>
    public void LoadNextLevel()
    {
        if (IsV1CandidatePlaytest)
        {
            Debug.Log(
                "LevelManager: V1 candidate playtest — Next Level disabled (no DB identity)."
            );
            SceneTransition.LoadScene("LevelSelect");
            return;
        }

        if (levelDatabase == null || levelDatabase.LevelCount == 0)
        {
            Debug.LogError("LevelManager: geen levels in LevelDatabase.");
            return;
        }

        int currentDb = currentLevelIndex;
        LevelDifficulty difficulty = CurrentDifficulty;
        int currentLocal = LevelDifficultyOrder.GetDifficultyDisplayNumber(
            levelDatabase,
            currentDb
        );
        List<int> ordered = LevelDifficultyOrder.GetOrderedLevelIndicesForDifficulty(
            levelDatabase,
            difficulty
        );

        int nextIndex = LevelDifficultyOrder.FindNextOrderedLevelIndexSameDifficulty(
            levelDatabase,
            currentDb
        );
        int nextLocal = nextIndex >= 0
            ? LevelDifficultyOrder.GetDifficultyDisplayNumber(levelDatabase, nextIndex)
            : 0;
        bool currentCompleted = saveManager != null && saveManager.IsLevelCompleted(currentDb);
        bool nextUnlocked = nextIndex >= 0 &&
            saveManager != null &&
            saveManager.IsLevelUnlocked(
                nextIndex,
                levelDatabase,
                difficultyProgressionConfig
            );

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[LevelTransition] NextIndexResolved=" + nextIndex +
            " CurrentDb=" + currentDb +
            " Difficulty=" + difficulty +
            " NextLocal=" + nextLocal +
            " NextUnlocked=" + nextUnlocked
        );
        Debug.Log(
            "[NextDifficultyLevel]\n" +
            "CurrentDb=" + currentDb + "\n" +
            "Difficulty=" + difficulty + "\n" +
            "CurrentLocal=" + currentLocal + "\n" +
            "OrderedCount=" + ordered.Count + "\n" +
            "NextDb=" + nextIndex + "\n" +
            "NextLocal=" + nextLocal + "\n" +
            "CurrentCompleted=" + currentCompleted + "\n" +
            "NextUnlocked=" + nextUnlocked
        );
#endif

        if (nextIndex < 0)
        {
            Debug.Log(
                "LevelManager: geen volgend level binnen difficulty " + difficulty +
                " — terug naar LevelSelect."
            );
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[LevelTransition] SceneLoadStarted Path=LevelSelect Reason=NoNextInTier");
#endif
            SceneTransition.LoadScene("LevelSelect");
            return;
        }

        // Alleen laden als difficulty unlocked + sequential unlock binnen ordered tier.
        if (saveManager != null && !nextUnlocked)
        {
            Debug.Log(
                "LevelManager: next same-difficulty index " + nextIndex +
                " (local " + nextLocal + ") is locked — terug naar LevelSelect."
            );
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[LevelTransition] SceneLoadStarted Path=LevelSelect Reason=NextLocked");
#endif
            SceneTransition.LoadScene("LevelSelect");
            return;
        }

        currentLevelIndex = nextIndex;

        Debug.Log("Loading next level index: " + currentLevelIndex);

        // Huidig level opslaan. Unlock gebeurt al in GameManager.CompleteLevel().
        if (saveManager != null)
        {
            saveManager.SaveCurrentLevel(currentLevelIndex);
            saveManager.SaveUnlockedLevel(currentLevelIndex);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LevelData nextData =
            levelDatabase != null ? levelDatabase.GetLevel(currentLevelIndex) : null;
        Debug.Log(
            "[LevelTransition] LevelDataLoaded=" +
            (nextData != null ? nextData.name : "null") +
            " DbIndex=" + currentLevelIndex
        );
#endif

        LoadLevel();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[LevelTransition] Complete");
#endif
    }

    /// <summary>
    /// Herlaadt hetzelfde level. Verhoogt de index NIET en leest/schrijft SaveManager NIET.
    /// </summary>
    public void RestartLevel()
    {
        LogLevelRestartTelemetry();

        if (editorPlaytestLevel != null)
        {
            Debug.Log("Restarting V1 playtest level: " + editorPlaytestLevel.name);
            LoadLevelData(editorPlaytestLevel);
            return;
        }

        // Bewaar de index lokaal — Restart mag currentLevelIndex nooit wijzigen.
        int indexToReload = currentLevelIndex;

        Debug.Log("Restarting level index: " + indexToReload);

        currentLevelIndex = indexToReload;
        LoadCurrentLevel();
    }

    private void LogLevelRestartTelemetry()
    {
        LevelData data = CurrentLevelData;
        string difficulty = data != null
            ? data.difficulty.ToString()
            : CurrentDifficulty.ToString();
        int movesUsed = gameManager != null ? gameManager.CurrentMoves : 0;
        GameAnalytics.LogLevelRestart(GetDisplayLevelNumber(), difficulty, movesUsed);
    }

    /// <summary>
    /// Laadt levelDatabase.GetLevel(currentLevelIndex):
    /// ruimt oude auto's/occupancy op en spawnt opnieuw.
    /// </summary>
    public void LoadLevel()
    {
        if (editorPlaytestLevel != null)
        {
            LoadLevelData(editorPlaytestLevel);
            return;
        }

        if (levelDatabase == null || levelDatabase.LevelCount == 0)
        {
            Debug.LogError("LevelManager: geen levels in LevelDatabase.");
            FirebaseManager.ReportNonFatal(
                "LevelManager: empty LevelDatabase (impossible current level lookup)"
            );
            return;
        }

        if (currentLevelIndex < 0 || currentLevelIndex >= levelDatabase.LevelCount)
        {
            Debug.LogError("LevelManager: currentLevelIndex buiten bereik: " + currentLevelIndex);
            FirebaseManager.ReportNonFatal(
                "LevelManager: currentLevelIndex out of range: " + currentLevelIndex
            );
            return;
        }

        LevelData levelData = levelDatabase.GetLevel(currentLevelIndex);
        if (levelData == null)
        {
            Debug.LogError("LevelManager: level op index " + currentLevelIndex + " is leeg.");
            FirebaseManager.ReportNonFatal(
                "LevelManager: null LevelData at index " + currentLevelIndex
            );
            return;
        }

        LoadLevelData(levelData);
    }

    /// <summary>
    /// Shared load pipeline for DB levels and Editor V1 candidate override.
    /// </summary>
    private void LoadLevelData(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("LevelManager: levelData is null.");
            return;
        }

        if (vehiclePrefab == null)
        {
            Debug.LogError("LevelManager: geen vehiclePrefab gekoppeld.");
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("LevelManager: geen GridManager gekoppeld.");
            return;
        }

        activeVehicleSpriteLibrary = ResolveActiveSpriteLibrary();

        // Hint tint eerst herstellen, daarna voertuigen vernietigen.
        if (gameManager != null)
        {
            gameManager.ResetMoves();
        }

        HintManager hintManager = FindAnyObjectByType<HintManager>();
        if (hintManager != null)
        {
            hintManager.ResetHintSession();
        }

        // Eerst oude gespawnde voertuigen + occupancy opruimen.
        ClearExistingVehicles();
        activeVehicles.Clear();
        lastAutoAssignedSprite = null;

        // Runtime gridgrootte uit LevelData (oude assets → 6x6).
        int width = levelData.ResolvedGridWidth;
        int height = levelData.ResolvedGridHeight;
        gridManager.Configure(width, height);
        Debug.Log("Loaded grid " + width + "x" + height);

        // Visuele parking (vervangt vaste ParkingLotVisual-sprite).
        if (parkingGridVisual != null)
        {
            Debug.Log("Building parking visual: " + width + "x" + height);
            parkingGridVisual.BuildGrid(width, height);
        }
        else
        {
            Debug.LogWarning("ParkingGridVisual is not assigned");
        }

        // Visuele exit op rechterrand + exitRow — VOOR camera fit,
        // zodat exit road/arrow in de visual bounds zitten.
        UpdateExitVisualPosition(levelData.exitRow);

        // Camera framing nadat parking + exit visuals klaar zijn.
        // ParkingGridVisual deactiveert oude tiles vóór Destroy (voorkomt stale 8x8 bounds).
        // CameraFitter past orthographicSize ABSOLUUT toe + end-of-frame refit.
        if (cameraFitter != null)
        {
            if (parkingGridVisual != null)
            {
                cameraFitter.SetBoardVisualRoot(parkingGridVisual.transform);
            }

            if (exitVisual != null)
            {
                cameraFitter.SetExitVisualRoot(exitVisual);
            }

            float cellSize = gridManager != null ? gridManager.CellSize : 1f;
            cameraFitter.FitToGrid(width, height, cellSize);
        }

        // Maak voor elk item in LevelData één voertuig (volgorde = solver indices).
        foreach (VehicleData data in levelData.vehicles)
        {
            VehicleController spawned = SpawnVehicle(data, levelData);
            if (spawned != null)
            {
                activeVehicles.Add(spawned);
            }
        }

        // TargetIndicator boven alle vehicle CarSprites (één keer na spawn).
        RefreshTargetIndicatorSorting();

        Debug.Log(
            "Level " + levelData.levelNumber +
            " (index " + currentLevelIndex +
            (editorPlaytestLevel != null ? ", V1 playtest override" : string.Empty) +
            ") geladen met " +
            levelData.vehicles.Count + " voertuigen."
        );

        if (gameplayUI != null)
        {
            gameplayUI.UpdateLevelText();
        }

        if (levelObjectiveController == null)
        {
            levelObjectiveController = FindAnyObjectByType<LevelObjectiveController>();
        }

        // Na spawn: Classic = no-op, TimedAmbulance = wacht op fade → start timer.
        levelObjectiveController?.BeginForLevel(levelData);

        ReportLevelLoadTelemetry(levelData);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[LevelLoadTrace]\n" +
            "DbIndex=" + currentLevelIndex + "\n" +
            "Asset=" + levelData.name + "\n" +
            "Difficulty=" + levelData.difficulty + "\n" +
            "DisplayNumber=" + GetDisplayLevelNumber() + "\n" +
            "Objective=" + levelData.objectiveType + "\n" +
            "MinMoves=" + levelData.minimumMoves + "\n" +
            "ContentVersion=" + LevelDatabaseContentVersion.Current + "\n" +
            "SavedContentVersion=" + LevelDatabaseContentVersion.GetSavedVersion() + "\n" +
            "V1Override=" + (editorPlaytestLevel != null)
        );
#endif
    }

    private void ReportLevelLoadTelemetry(LevelData levelData)
    {
        if (levelData == null)
        {
            return;
        }

        int displayNumber = GetDisplayLevelNumber();
        string difficulty = levelData.difficulty.ToString();
        string objective = levelData.objectiveType.ToString();
        int width = levelData.ResolvedGridWidth;
        int height = levelData.ResolvedGridHeight;
        string grid = width + "x" + height;

        FirebaseManager.SetLevelCrashContext(
            displayNumber,
            levelData.name,
            difficulty,
            objective,
            grid,
            levelData.minimumMoves
        );

        GameAnalytics.LogLevelStart(
            displayNumber,
            difficulty,
            objective,
            levelData.minimumMoves,
            width,
            height,
            levelData.name
        );

        // Serious production content issues → Crashlytics non-fatal (via shared validators).
        LevelMinMoves.LogInvalidIfNeeded(
            levelData,
            currentLevelIndex,
            "LevelManager.LoadLevelData"
        );

        string objectiveMissing = ObjectiveConfigValidation.ValidateSpecialObjective(levelData);
        if (!string.IsNullOrEmpty(objectiveMissing))
        {
            ObjectiveConfigValidation.LogValidation(
                levelData,
                objectiveMissing,
                currentLevelIndex
            );
        }
    }

    /// <summary>
    /// Zet actieve TargetIndicators boven de hoogste CarSprite sortingOrder in het level.
    /// Geen FindObjects — gebruikt ActiveVehicles. Roept geen vehicle sorting om.
    /// </summary>
    private void RefreshTargetIndicatorSorting()
    {
        int highestVehicleOrder = 0;
        bool foundVehicleSprite = false;

        for (int i = 0; i < activeVehicles.Count; i++)
        {
            VehicleController vehicle = activeVehicles[i];
            if (vehicle == null)
            {
                continue;
            }

            SpriteRenderer body = vehicle.VisualSpriteRenderer;
            if (body == null)
            {
                continue;
            }

            if (!foundVehicleSprite || body.sortingOrder > highestVehicleOrder)
            {
                highestVehicleOrder = body.sortingOrder;
                foundVehicleSprite = true;
            }
        }

        for (int i = 0; i < activeVehicles.Count; i++)
        {
            VehicleController vehicle = activeVehicles[i];
            if (vehicle == null)
            {
                continue;
            }

            vehicle.ApplyTargetIndicatorSorting(highestVehicleOrder);
        }
    }

    /// <summary>
    /// Verwijdert alle voertuigen onder vehicleParent en unregistreert ze bij GridManager.
    /// </summary>
    private void ClearExistingVehicles()
    {
        activeVehicles.Clear();

        if (vehicleParent != null)
        {
            // Loop achterstevoren zodat Destroy veilig is tijdens de loop.
            for (int i = vehicleParent.childCount - 1; i >= 0; i--)
            {
                Transform child = vehicleParent.GetChild(i);
                VehicleController vehicle = child.GetComponent<VehicleController>();

                if (vehicle != null)
                {
                    gridManager.UnregisterVehicle(vehicle);
                }

                Destroy(child.gameObject);
            }

            return;
        }

        // Fallback: zoek alle VehicleControllers in de scene.
        VehicleController[] vehicles = FindObjectsByType<VehicleController>();

        foreach (VehicleController vehicle in vehicles)
        {
            gridManager.UnregisterVehicle(vehicle);
            Destroy(vehicle.gameObject);
        }
    }

    /// <summary>
    /// Plaatst exitVisual op de rechterrand van het huidige grid, op exitRow.
    /// </summary>
    private void UpdateExitVisualPosition(int exitRow)
    {
        if (exitVisual == null || gridManager == null)
        {
            return;
        }

        // Eén cel rechts van de laatste kolom = buiten het speelveld.
        Vector3 exitWorld = gridManager.CellToWorld(
            new Vector2Int(gridManager.GridWidth, exitRow)
        );

        Vector3 position = exitVisual.position;
        position.x = exitWorld.x;
        position.y = exitWorld.y;
        exitVisual.position = position;

        Debug.Log(
            "Exit visual moved to row " + exitRow +
            " at world (" + exitWorld.x + ", " + exitWorld.y + ")"
        );
    }

    /// <summary>
    /// Instantieert één voertuig en vult alle data via Setup(...).
    /// </summary>
    private VehicleController SpawnVehicle(VehicleData data, LevelData levelData)
    {
        VehicleController vehicle = Instantiate(
            vehiclePrefab,
            vehicleParent
        );

        // Naam in de Hierarchy (handig bij debuggen).
        vehicle.gameObject.name = string.IsNullOrEmpty(data.vehicleName)
            ? "Vehicle"
            : data.vehicleName;

        // Vul alle velden via de publieke Setup-methode.
        vehicle.Setup(
            gridManager,
            gameManager,
            data.orientation,
            data.lengthInCells,
            data.gridPosition,
            data.canExitRight,
            levelData.exitRow,
            data.isProtectedVehicle,
            data.isFragileCargo,
            data.isLimitedVehicle
        );

        ApplyVehicleSprite(vehicle, ResolveVehicleSprite(data, levelData));

        // Sprite kan net gezet zijn — herbereken uniforme visual scale.
        vehicle.UpdateVisualSize();

        // Shadow moet de definitieve CarSprite (na library-assign) overnemen.
        VehicleShadow shadow = vehicle.GetComponentInChildren<VehicleShadow>(true);
        if (shadow != null)
        {
            shadow.UpdateShadow();
        }

        return vehicle;
    }

    /// <summary>
    /// Live visual-only refresh after SkinManager.SelectSkin (shop SELECT in Gameplay).
    /// Does not respawn vehicles, move pieces, or touch puzzle / mission / timer state.
    /// </summary>
    public void RefreshActiveVehicleSkinVisuals()
    {
        LevelData levelData = CurrentLevelData;
        if (levelData == null || levelData.vehicles == null)
        {
            return;
        }

        activeVehicleSpriteLibrary = ResolveActiveSpriteLibrary();
        lastAutoAssignedSprite = null;

        int count = Mathf.Min(activeVehicles.Count, levelData.vehicles.Count);
        for (int i = 0; i < count; i++)
        {
            VehicleController vehicle = activeVehicles[i];
            VehicleData data = levelData.vehicles[i];
            if (vehicle == null || data == null)
            {
                continue;
            }

            ApplyVehicleSprite(vehicle, ResolveVehicleSprite(data, levelData));
            vehicle.UpdateVisualSize();

            VehicleShadow shadow = vehicle.GetComponentInChildren<VehicleShadow>(true);
            if (shadow != null)
            {
                shadow.UpdateShadow();
            }
        }

        RefreshTargetIndicatorSorting();
    }

    private void OnSelectedSkinChanged(VehicleSkinData _)
    {
        RefreshActiveVehicleSkinVisuals();
    }

    private void ResolveSkinManager()
    {
        if (skinManager == null)
        {
            skinManager = GetComponent<SkinManager>();
        }

        if (skinManager == null)
        {
            skinManager = FindAnyObjectByType<SkinManager>();
        }
    }

    /// <summary>
    /// Selected skin library indien aanwezig, anders Inspector-default.
    /// </summary>
    private VehicleSpriteLibrary ResolveActiveSpriteLibrary()
    {
        ResolveSkinManager();
        if (skinManager != null)
        {
            VehicleSpriteLibrary fromSkin = skinManager.GetActiveSpriteLibrary();
            if (fromSkin != null)
            {
                return fromSkin;
            }
        }

        return vehicleSpriteLibrary;
    }

    /// <summary>
    /// Bepaalt welke sprite dit voertuig krijgt (alleen visueel).
    /// Prioriteit: target → expliciete vehicleSprite → library.
    /// </summary>
    private Sprite ResolveVehicleSprite(VehicleData data, LevelData levelData)
    {
        VehicleSpriteLibrary library = activeVehicleSpriteLibrary != null
            ? activeVehicleSpriteLibrary
            : vehicleSpriteLibrary;

        // Target car:
        // 1) LevelData.specialTargetSprite (explicit override)
        // 2) SpecialMissionSpriteLibrary (TimedAmbulance / FragileCargo)
        // 3) selected skin TargetCarSprite
        if (data.canExitRight)
        {
            Sprite specialTarget = ResolveSpecialTargetSprite(data, levelData);
            if (specialTarget != null)
            {
                return specialTarget;
            }

            if (library != null && library.TargetCarSprite != null)
            {
                return library.TargetCarSprite;
            }

            if (data.vehicleSprite != null)
            {
                return data.vehicleSprite;
            }

            Debug.LogWarning(
                "LevelManager: target car heeft geen targetCarSprite in VehicleSpriteLibrary."
            );
            return null;
        }

        // 2) Expliciet in LevelData gezet.
        if (data.vehicleSprite != null)
        {
            return data.vehicleSprite;
        }

        // 3) Automatisch uit library op orientation + length (selected skin).
        if (library == null)
        {
            return null;
        }

        Sprite chosen = library.GetSpriteForVehicle(
            data.orientation,
            data.lengthInCells,
            lastAutoAssignedSprite
        );

        if (chosen == null)
        {
            Debug.LogWarning(
                "LevelManager: geen geschikte sprite in VehicleSpriteLibrary voor " +
                data.orientation + " length " + data.lengthInCells +
                " (" + data.vehicleName + ")."
            );
            return null;
        }

        lastAutoAssignedSprite = chosen;
        return chosen;
    }

    /// <summary>
    /// TimedAmbulance / FragileCargo target visual.
    /// Priority: LevelData.specialTargetSprite → SpecialMissionSpriteLibrary → null.
    /// </summary>
    private Sprite ResolveSpecialTargetSprite(VehicleData data, LevelData levelData)
    {
        if (levelData == null || data == null)
        {
            return null;
        }

        LevelObjectiveType objective = levelData.objectiveType;

        bool usesSpecialTarget =
            objective == LevelObjectiveType.TimedAmbulance ||
            (objective == LevelObjectiveType.FragileCargo && data.isFragileCargo);

        if (!usesSpecialTarget)
        {
            return null;
        }

        // Priority 1: explicit per-level override.
        if (levelData.specialTargetSprite != null)
        {
            return levelData.specialTargetSprite;
        }

        // Priority 2: central special-mission library.
        if (specialMissionSpriteLibrary == null)
        {
            return null;
        }

        if (objective == LevelObjectiveType.TimedAmbulance)
        {
            return specialMissionSpriteLibrary.TimedAmbulanceTargetSprite;
        }

        if (objective == LevelObjectiveType.FragileCargo && data.isFragileCargo)
        {
            return specialMissionSpriteLibrary.FragileCargoTargetSprite;
        }

        return null;
    }

    /// <summary>
    /// Zet optioneel een custom sprite op CarSprite (of Visual als fallback).
    /// Null = laat de prefab-sprite zoals die is.
    /// </summary>
    private void ApplyVehicleSprite(VehicleController vehicle, Sprite vehicleSprite)
    {
        if (vehicleSprite == null)
        {
            return;
        }

        Transform visual = vehicle.transform.Find("VisualRoot");
        if (visual == null)
        {
            visual = vehicle.transform.Find("Visual");
        }

        if (visual == null)
        {
            return;
        }

        // Bij voorkeur CarSprite — niet TargetIndicator/HintDirection.
        Transform carSprite = visual.Find("CarSprite");
        SpriteRenderer spriteRenderer = carSprite != null
            ? carSprite.GetComponent<SpriteRenderer>()
            : visual.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sprite = vehicleSprite;
    }
}

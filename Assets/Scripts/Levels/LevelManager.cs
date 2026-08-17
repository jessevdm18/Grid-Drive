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

    /// <summary>
    /// Zero-based index van het actieve level (0 = LEVEL 1).
    /// </summary>
    public int CurrentLevelIndex => currentLevelIndex;

    /// <summary>
    /// Actieve voertuigen in LevelData-spawnvolgorde (stabiele solver-indices).
    /// </summary>
    public IReadOnlyList<VehicleController> ActiveVehicles => activeVehicles;

    /// <summary>
    /// Aantal levels in de database.
    /// </summary>
    public int LevelCount => levelDatabase != null ? levelDatabase.LevelCount : 0;

    /// <summary>
    /// LevelData van het actieve level, of null.
    /// </summary>
    public LevelData CurrentLevelData
    {
        get
        {
            if (levelDatabase == null)
            {
                return null;
            }

            return levelDatabase.GetLevel(currentLevelIndex);
        }
    }

    private void Start()
    {
        // Laad voortgang uit save (default = 0).
        if (saveManager != null)
        {
            currentLevelIndex = saveManager.GetCurrentLevel();
        }

        // Zorg dat de index altijd binnen de database valt.
        if (levelDatabase != null && levelDatabase.LevelCount > 0)
        {
            currentLevelIndex = Mathf.Clamp(
                currentLevelIndex,
                0,
                levelDatabase.LevelCount - 1
            );
        }
        else
        {
            currentLevelIndex = 0;
        }

        LoadCurrentLevel();
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
    /// Laadt het volgende level. Doet niets als je al op het laatste level bent.
    /// Alleen hier mag de index omhoog en progressie worden opgeslagen.
    /// </summary>
    public void LoadNextLevel()
    {
        if (levelDatabase == null || levelDatabase.LevelCount == 0)
        {
            Debug.LogError("LevelManager: geen levels in LevelDatabase.");
            return;
        }

        // Stop bij het laatste level — voorkom index-out-of-range.
        if (currentLevelIndex >= levelDatabase.LevelCount - 1)
        {
            Debug.Log("LevelManager: dit is het laatste level.");
            return;
        }

        currentLevelIndex++;

        Debug.Log("Loading next level index: " + currentLevelIndex);

        // Huidig level opslaan. Unlock gebeurt al in GameManager.CompleteLevel().
        if (saveManager != null)
        {
            saveManager.SaveCurrentLevel(currentLevelIndex);
            saveManager.SaveUnlockedLevel(currentLevelIndex);
        }

        LoadLevel();
    }

    /// <summary>
    /// Herlaadt hetzelfde level. Verhoogt de index NIET en leest/schrijft SaveManager NIET.
    /// </summary>
    public void RestartLevel()
    {
        // Bewaar de index lokaal — Restart mag currentLevelIndex nooit wijzigen.
        int indexToReload = currentLevelIndex;

        Debug.Log("Restarting level index: " + indexToReload);

        currentLevelIndex = indexToReload;
        LoadCurrentLevel();
    }

    /// <summary>
    /// Laadt levelDatabase.GetLevel(currentLevelIndex):
    /// ruimt oude auto's/occupancy op en spawnt opnieuw.
    /// </summary>
    public void LoadLevel()
    {
        if (levelDatabase == null || levelDatabase.LevelCount == 0)
        {
            Debug.LogError("LevelManager: geen levels in LevelDatabase.");
            return;
        }

        if (currentLevelIndex < 0 || currentLevelIndex >= levelDatabase.LevelCount)
        {
            Debug.LogError("LevelManager: currentLevelIndex buiten bereik: " + currentLevelIndex);
            return;
        }

        activeVehicleSpriteLibrary = ResolveActiveSpriteLibrary();

        LevelData levelData = levelDatabase.GetLevel(currentLevelIndex);

        if (levelData == null)
        {
            Debug.LogError("LevelManager: level op index " + currentLevelIndex + " is leeg.");
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

        // Hint tint eerst herstellen, daarna voertuigen vernietigen.
        if (gameManager != null)
        {
            gameManager.ResetMoves();
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

        Debug.Log(
            "Level " + levelData.levelNumber +
            " (index " + currentLevelIndex + ") geladen met " +
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
    /// Bepaalt welke sprite dit voertuig krijgt (alleen visueel).
    /// Prioriteit: target → expliciete vehicleSprite → library.
    /// </summary>
    /// <summary>
    /// Selected skin library indien aanwezig, anders Inspector-default.
    /// </summary>
    private VehicleSpriteLibrary ResolveActiveSpriteLibrary()
    {
        SkinManager skinManager = FindAnyObjectByType<SkinManager>();
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

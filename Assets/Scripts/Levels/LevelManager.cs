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

    /// <summary>
    /// Zero-based index van het actieve level (0 = LEVEL 1).
    /// </summary>
    public int CurrentLevelIndex => currentLevelIndex;

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

        // Progressie opslaan: huidige + unlocked level.
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

        // Eerst oude gespawnde voertuigen + occupancy opruimen.
        ClearExistingVehicles();

        // Visuele exit op de juiste rij zetten (next level + restart).
        UpdateExitVisualPosition(levelData.exitRow);

        // Maak voor elk item in LevelData één voertuig.
        foreach (VehicleData data in levelData.vehicles)
        {
            SpawnVehicle(data, levelData);
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
    }

    /// <summary>
    /// Verwijdert alle voertuigen onder vehicleParent en unregistreert ze bij GridManager.
    /// </summary>
    private void ClearExistingVehicles()
    {
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
        VehicleController[] vehicles = FindObjectsByType<VehicleController>(FindObjectsSortMode.None);

        foreach (VehicleController vehicle in vehicles)
        {
            gridManager.UnregisterVehicle(vehicle);
            Destroy(vehicle.gameObject);
        }
    }

    /// <summary>
    /// Zet alleen de Y van exitVisual op de wereld-Y van exitRow.
    /// X en Z blijven zoals in de scene (rechts buiten het grid).
    /// </summary>
    private void UpdateExitVisualPosition(int exitRow)
    {
        if (exitVisual == null || gridManager == null)
        {
            return;
        }

        Vector3 cellWorld = gridManager.CellToWorld(
            new Vector2Int(gridManager.GridWidth - 1, exitRow)
        );

        Vector3 position = exitVisual.position;
        position.y = cellWorld.y;
        exitVisual.position = position;

        Debug.Log(
            "Exit visual moved to row " + exitRow +
            " at world Y " + cellWorld.y
        );
    }

    /// <summary>
    /// Instantieert één voertuig en vult alle data via Setup(...).
    /// </summary>
    private void SpawnVehicle(VehicleData data, LevelData levelData)
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
            levelData.exitRow
        );

        ApplyVehicleSprite(vehicle, data.vehicleSprite);
    }

    /// <summary>
    /// Zet optioneel een custom sprite op het Visual-child.
    /// Null = laat de prefab-sprite zoals die is.
    /// </summary>
    private void ApplyVehicleSprite(VehicleController vehicle, Sprite vehicleSprite)
    {
        if (vehicleSprite == null)
        {
            return;
        }

        Transform visual = vehicle.transform.Find("Visual");
        if (visual == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer = visual.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sprite = vehicleSprite;
    }
}

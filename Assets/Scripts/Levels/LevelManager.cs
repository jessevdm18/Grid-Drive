using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Laadt LevelData ScriptableObjects en spawnt alle voertuigen.
/// Ondersteunt meerdere levels via een lijst.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Levels")]
    [Tooltip("Alle levels in volgorde (index 0 = eerste level).")]
    [SerializeField] private List<LevelData> levels = new List<LevelData>();

    [Tooltip("Welk level nu actief is (index in de levels-lijst).")]
    [SerializeField] private int currentLevelIndex = 0;

    [Header("Prefabs & Referenties")]
    [Tooltip("Prefab van een voertuig (bijv. Car_Player).")]
    [SerializeField] private VehicleController vehiclePrefab;

    [SerializeField] private GridManager gridManager;

    [SerializeField] private GameManager gameManager;

    [Tooltip("Onder dit Transform komen alle gespawnde voertuigen.")]
    [SerializeField] private Transform vehicleParent;

    private void Start()
    {
        LoadCurrentLevel();
    }

    /// <summary>
    /// Laadt het level op currentLevelIndex opnieuw.
    /// </summary>
    public void LoadCurrentLevel()
    {
        LoadLevel();
    }

    /// <summary>
    /// Laadt het volgende level. Doet niets als je al op het laatste level bent.
    /// </summary>
    public void LoadNextLevel()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("LevelManager: geen levels in de lijst.");
            return;
        }

        // Stop bij het laatste level — voorkom index-out-of-range.
        if (currentLevelIndex >= levels.Count - 1)
        {
            Debug.Log("LevelManager: dit is het laatste level.");
            return;
        }

        currentLevelIndex++;
        LoadLevel();
    }

    /// <summary>
    /// Laadt het huidige level opnieuw (zelfde index).
    /// </summary>
    public void RestartLevel()
    {
        LoadLevel();
    }

    /// <summary>
    /// Laadt levels[currentLevelIndex]: ruimt oude auto's/occupancy op en spawnt opnieuw.
    /// </summary>
    public void LoadLevel()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("LevelManager: geen levels in de lijst.");
            return;
        }

        if (currentLevelIndex < 0 || currentLevelIndex >= levels.Count)
        {
            Debug.LogError("LevelManager: currentLevelIndex buiten bereik: " + currentLevelIndex);
            return;
        }

        LevelData levelData = levels[currentLevelIndex];

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
    }
}

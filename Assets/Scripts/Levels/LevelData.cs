using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Difficulty-tier voor level-progressie / metadata (niet de solver-score).
/// </summary>
public enum LevelDifficulty
{
    Easy,
    Medium,
    Hard
}

[CreateAssetMenu(
    fileName = "LevelData",
    menuName = "RushOut/Level Data"
)]
public class LevelData : ScriptableObject
{
    public const int DefaultGridSize = 6;
    public const int MinGridSize = 4;
    public const int MaxGridSize = 10;

    [Header("Level Info")]
    public int levelNumber = 1;

    [Tooltip("Handmatige / generator difficulty-tier (Easy / Medium / Hard).")]
    public LevelDifficulty difficulty = LevelDifficulty.Medium;

    [Header("Grid")]
    [Tooltip("Breedte in cellen. Ontbrekende/oude assets (0) → 6.")]
    public int gridWidth = DefaultGridSize;

    [Tooltip("Hoogte in cellen. Ontbrekende/oude assets (0) → 6.")]
    public int gridHeight = DefaultGridSize;

    [Header("Exit")]
    public int exitRow = 2;

    [Header("Vehicles")]
    public List<VehicleData> vehicles = new List<VehicleData>();

    [Header("Solver Stats")]
    public int minimumMoves;
    public int statesExplored;
    public int difficultyScore;

    /// <summary>
    /// Effectieve breedte (oude assets zonder veld → 6).
    /// </summary>
    public int ResolvedGridWidth =>
        gridWidth > 0 ? gridWidth : DefaultGridSize;

    /// <summary>
    /// Effectieve hoogte (oude assets zonder veld → 6).
    /// </summary>
    public int ResolvedGridHeight =>
        gridHeight > 0 ? gridHeight : DefaultGridSize;

    private void OnValidate()
    {
        if (gridWidth <= 0)
        {
            gridWidth = DefaultGridSize;
        }

        if (gridHeight <= 0)
        {
            gridHeight = DefaultGridSize;
        }

        gridWidth = Mathf.Clamp(gridWidth, MinGridSize, MaxGridSize);
        gridHeight = Mathf.Clamp(gridHeight, MinGridSize, MaxGridSize);
        exitRow = Mathf.Clamp(exitRow, 0, Mathf.Max(0, gridHeight - 1));
    }
}

[System.Serializable]
public class VehicleData
{
    public string vehicleName;

    public VehicleController.VehicleOrientation orientation;

    public int lengthInCells = 2;

    public Vector2Int gridPosition;

    public bool canExitRight = false;

    [Tooltip("Optioneel. Leeg = behoud de standaard sprite van de Car_Player prefab.")]
    public Sprite vehicleSprite;
}

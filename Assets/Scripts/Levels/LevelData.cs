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
    [Header("Level Info")]
    public int levelNumber = 1;

    [Tooltip("Handmatige / generator difficulty-tier (Easy / Medium / Hard).")]
    public LevelDifficulty difficulty = LevelDifficulty.Medium;

    [Header("Exit")]
    public int exitRow = 2;

    [Header("Vehicles")]
    public List<VehicleData> vehicles = new List<VehicleData>();

    [Header("Solver Stats")]
    public int minimumMoves;
    public int statesExplored;
    public int difficultyScore;
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
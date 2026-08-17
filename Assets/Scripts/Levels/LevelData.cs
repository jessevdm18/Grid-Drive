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

/// <summary>
/// Runtime win-condition type. Classic = alleen target-exit.
/// </summary>
public enum LevelObjectiveType
{
    Classic = 0,
    TimedAmbulance = 1,
    MoveLimit = 2,
    MultiTargetRescue = 3,
    NoTouchChallenge = 4,
    FragileCargo = 5,
    LimitedVehicle = 6
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

    [Header("Objective")]
    [Tooltip("Classic / TimedAmbulance / MoveLimit / MultiTargetRescue / NoTouchChallenge / FragileCargo / LimitedVehicle.")]
    public LevelObjectiveType objectiveType = LevelObjectiveType.Classic;

    [Tooltip("Alleen voor TimedAmbulance. 0 = geen bruikbare timer.")]
    public float timeLimitSeconds = 0f;

    [Tooltip("Alleen voor MoveLimit. 0 = geen bruikbare move-limit.")]
    public int moveLimit = 0;

    [Tooltip("Alleen voor FragileCargo. Max geldige moves van de cargo-target. 0 = ongeldig.")]
    public int fragileCargoMoveLimit = 0;

    [Tooltip("Alleen voor LimitedVehicle. Max geldige moves van de limited blocker. 0 = ongeldig.")]
    public int limitedVehicleMoveLimit = 0;

    [Tooltip("Optioneel per-level override. TimedAmbulance / FragileCargo: null = SpecialMissionSpriteLibrary.")]
    public Sprite specialTargetSprite;

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

    [Header("Solution Structure (legacy / unused by Random generator)")]
    [Tooltip("Legacy ConstructiveHard metadata. Random generator writes uniqueVehiclesInSolution only.")]
    public int solutionComplexityScore;
    public int uniqueVehiclesInSolution;
    public int axisAlternations;
    public int vehicleRevisits;
    public int longestSingleAxisRun;
    public float linearChainSolutionRatio;
    public int secondaryBlockersUsed;
    public int forkDependencies;

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
        timeLimitSeconds = Mathf.Max(0f, timeLimitSeconds);
        fragileCargoMoveLimit = Mathf.Max(0, fragileCargoMoveLimit);
        limitedVehicleMoveLimit = Mathf.Max(0, limitedVehicleMoveLimit);
    }
}

[System.Serializable]
public class VehicleData
{
    public string vehicleName;

    public VehicleController.VehicleOrientation orientation;

    public int lengthInCells = 2; // Geldig: 2 (auto), 3 (bus), 4 (truck)

    public Vector2Int gridPosition;

    public bool canExitRight = false;

    [Tooltip("Alleen NoTouchChallenge: dit voertuig mag niet van gridpositie veranderen. Classic/andere objectives negeren dit.")]
    public bool isProtectedVehicle = false;

    [Tooltip("Alleen FragileCargo: dit is de cargo-target met eigen movebudget. Moet canExitRight zijn.")]
    public bool isFragileCargo = false;

    [Tooltip("Alleen LimitedVehicle: blocker met eigen movebudget. Mag geen target/protected/fragile zijn.")]
    public bool isLimitedVehicle = false;

    [Tooltip("Optioneel. Leeg = behoud de standaard sprite van de Car_Player prefab.")]
    public Sprite vehicleSprite;
}

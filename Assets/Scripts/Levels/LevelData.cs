using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "LevelData",
    menuName = "RushOut/Level Data"
)]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber = 1;

    [Header("Exit")]
    public int exitRow = 2;

    [Header("Vehicles")]
    public List<VehicleData> vehicles = new List<VehicleData>();
}

[System.Serializable]
public class VehicleData
{
    public string vehicleName;

    public VehicleController.VehicleOrientation orientation;

    public int lengthInCells = 2;

    public Vector2Int gridPosition;

    public bool canExitRight = false;
}
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Speelveld: variable rectangular grid, gecentreerd in world space.
/// Occupancy blijft een dictionary (geen vaste 6x6 array).
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 6;
    [SerializeField] private int gridHeight = 6;
    [SerializeField] private float cellSize = 1f;

    // Welke auto bezet welke cel?
    private Dictionary<Vector2Int, VehicleController> occupiedCells
        = new Dictionary<Vector2Int, VehicleController>();

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float CellSize => cellSize;

    /// <summary>
    /// Zet de runtime-gridgrootte (aangeroepen bij level-load) en wist occupancy.
    /// Grid blijft gecentreerd rond world origin.
    /// </summary>
    public void Configure(int width, int height)
    {
        gridWidth = Mathf.Max(1, width);
        gridHeight = Mathf.Max(1, height);
        occupiedCells.Clear();
    }

    /// <summary>
    /// Gridcel → wereldpositie van het CELMIDDEN.
    /// cellSize = 1: x - (gridWidth - 1) * 0.5f, y - (gridHeight - 1) * 0.5f
    /// </summary>
    public Vector3 CellToWorld(Vector2Int cell)
    {
        float worldX = (cell.x - (gridWidth - 1) * 0.5f) * cellSize;
        float worldY = (cell.y - (gridHeight - 1) * 0.5f) * cellSize;
        return new Vector3(worldX, worldY, 0f);
    }

    /// <summary>
    /// Wereldpositie → dichtstbijzijnde gridcel (zelfde centrering als CellToWorld).
    /// </summary>
    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        float x = worldPosition.x / cellSize + (gridWidth - 1) * 0.5f;
        float y = worldPosition.y / cellSize + (gridHeight - 1) * 0.5f;
        return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
    }

    public bool IsInsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 &&
               cell.x < gridWidth &&
               cell.y >= 0 &&
               cell.y < gridHeight;
    }

    public bool IsCellFree(Vector2Int cell, VehicleController requestingVehicle)
    {
        if (!IsInsideGrid(cell))
        {
            return false;
        }

        if (!occupiedCells.TryGetValue(cell, out VehicleController occupant))
        {
            return true;
        }

        // Een auto mag zijn eigen huidige cellen natuurlijk gebruiken.
        return occupant == requestingVehicle;
    }

    public bool AreCellsFree(
        List<Vector2Int> cells,
        VehicleController requestingVehicle)
    {
        foreach (Vector2Int cell in cells)
        {
            if (!IsCellFree(cell, requestingVehicle))
            {
                return false;
            }
        }

        return true;
    }

    public void RegisterVehicle(
        VehicleController vehicle,
        List<Vector2Int> cells)
    {
        UnregisterVehicle(vehicle);

        foreach (Vector2Int cell in cells)
        {
            occupiedCells[cell] = vehicle;
        }
    }

    public void UnregisterVehicle(VehicleController vehicle)
    {
        List<Vector2Int> cellsToRemove = new List<Vector2Int>();

        foreach (var pair in occupiedCells)
        {
            if (pair.Value == vehicle)
            {
                cellsToRemove.Add(pair.Key);
            }
        }

        foreach (Vector2Int cell in cellsToRemove)
        {
            occupiedCells.Remove(cell);
        }
    }
}

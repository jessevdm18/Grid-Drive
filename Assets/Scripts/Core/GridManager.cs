using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 6;
    [SerializeField] private int gridHeight = 6;
    [SerializeField] private float cellSize = 1f;

    // Onderste linker CELMIDDEN van ons 6x6 grid.
    // Bij een speelveld van -3 tot +3 zijn de celmiddens:
    // -2.5, -1.5, -0.5, 0.5, 1.5, 2.5
    [SerializeField] private Vector2 bottomLeftCellCenter = new Vector2(-2.5f, -2.5f);

    // Welke auto bezet welke cel?
    private Dictionary<Vector2Int, VehicleController> occupiedCells
        = new Dictionary<Vector2Int, VehicleController>();

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float CellSize => cellSize;

    // Zet een gridcel om naar de wereldpositie van het MIDDEN van die cel.
    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(
            bottomLeftCellCenter.x + cell.x * cellSize,
            bottomLeftCellCenter.y + cell.y * cellSize,
            0f
        );
    }

    // Controleert of een cel binnen het 6x6 grid ligt.
    public bool IsInsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 &&
               cell.x < gridWidth &&
               cell.y >= 0 &&
               cell.y < gridHeight;
    }

    // Controleert of één cel beschikbaar is.
    public bool IsCellFree(Vector2Int cell, VehicleController requestingVehicle)
    {
        if (!IsInsideGrid(cell))
            return false;

        if (!occupiedCells.TryGetValue(cell, out VehicleController occupant))
            return true;

        // Een auto mag zijn eigen huidige cellen natuurlijk gebruiken.
        return occupant == requestingVehicle;
    }

    // Controleert een lijst met cellen.
    public bool AreCellsFree(
        List<Vector2Int> cells,
        VehicleController requestingVehicle)
    {
        foreach (Vector2Int cell in cells)
        {
            if (!IsCellFree(cell, requestingVehicle))
                return false;
        }

        return true;
    }

    // Verwijdert alle oude registraties van deze auto
    // en registreert daarna zijn nieuwe cellen.
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
                cellsToRemove.Add(pair.Key);
        }

        foreach (Vector2Int cell in cellsToRemove)
        {
            occupiedCells.Remove(cell);
        }
    }
}
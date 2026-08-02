using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Beheert het speelveld-grid voor Rush Out.
/// Zet dit script op een leeg GameObject in je scene (bijv. "GridManager").
/// Gridcoördinaten lopen van 0 t/m gridWidth-1 en 0 t/m gridHeight-1.
/// Zet dit GameObject op het geometrische midden van het speelveld.
/// </summary>
public class GridManager : MonoBehaviour
{
    // Singleton: andere scripts kunnen GridManager.Instance gebruiken.
    public static GridManager Instance { get; private set; }

    [Header("Grid-instellingen")]
    [Tooltip("Aantal kolommen (breedte) van het grid.")]
    [SerializeField] private int gridWidth = 6;

    [Tooltip("Aantal rijen (hoogte) van het grid.")]
    [SerializeField] private int gridHeight = 6;

    [Tooltip("Grootte van één gridcel in wereld-eenheden.")]
    [SerializeField] private float cellSize = 1f;

    private void Awake()
    {
        // Zorg dat er maar één GridManager in de scene is.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // --- Publieke eigenschappen (alleen lezen) ---

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float CellSize => cellSize;

    /// <summary>
    /// De linksonderhoek van cel (0, 0) in wereldcoördinaten.
    /// Berekend vanuit het midden van dit GameObject, zodat het grid gecentreerd uitlijnt.
    /// </summary>
    public Vector2 GridOrigin => (Vector2)transform.position - new Vector2(gridWidth * cellSize * 0.5f, gridHeight * cellSize * 0.5f);

    /// <summary>
    /// Laagste grid-x-coördinaat (altijd 0).
    /// </summary>
    public int MinGridX => 0;

    /// <summary>
    /// Hoogste grid-x-coördinaat. Bij 6x6 is dit 5.
    /// </summary>
    public int MaxGridX => gridWidth - 1;

    /// <summary>
    /// Laagste grid-y-coördinaat (altijd 0).
    /// </summary>
    public int MinGridY => 0;

    /// <summary>
    /// Hoogste grid-y-coördinaat.
    /// </summary>
    public int MaxGridY => gridHeight - 1;

    // --- Positie-omrekening ---

    /// <summary>
    /// Zet een wereldpositie om naar een gridpositie.
    /// </summary>
    public Vector2Int WorldToGrid(Vector2 worldPosition)
    {
        Vector2 localPosition = worldPosition - GridOrigin;

        int x = Mathf.FloorToInt(localPosition.x / cellSize);
        int y = Mathf.FloorToInt(localPosition.y / cellSize);

        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Zet een gridpositie om naar het midden van die cel in wereldcoördinaten.
    /// </summary>
    public Vector2 GridToWorld(Vector2Int gridPosition)
    {
        float worldX = GridOrigin.x + (gridPosition.x + 0.5f) * cellSize;
        float worldY = GridOrigin.y + (gridPosition.y + 0.5f) * cellSize;

        return new Vector2(worldX, worldY);
    }

    /// <summary>
    /// Zet een gridpositie om naar de linksonderhoek van die cel.
    /// </summary>
    public Vector2 GridToWorldCorner(Vector2Int gridPosition)
    {
        return new Vector2(
            GridOrigin.x + gridPosition.x * cellSize,
            GridOrigin.y + gridPosition.y * cellSize
        );
    }

    // --- Grid-validatie ---

    /// <summary>
    /// Controleert of een gridpositie binnen het speelveld valt (0 t/m breedte/hoogte - 1).
    /// </summary>
    public bool IsInsideGrid(Vector2Int gridPosition)
    {
        return gridPosition.x >= MinGridX
            && gridPosition.x <= MaxGridX
            && gridPosition.y >= MinGridY
            && gridPosition.y <= MaxGridY;
    }

    // --- Bezetting ---
    // Per cel staat welk voertuig die cel bezet. Puur grid-logica — geen physics.

    // Snel opzoeken: welk voertuig bezet een cel?
    private Dictionary<Vector2Int, VehicleController> cellOccupancy = new Dictionary<Vector2Int, VehicleController>();

    // Snel vrijgeven: welke cellen hoorden bij een voertuig?
    private Dictionary<VehicleController, List<Vector2Int>> cellsByVehicle = new Dictionary<VehicleController, List<Vector2Int>>();

    /// <summary>
    /// Controleert of één gridcel vrij is voor het opgegeven voertuig.
    /// Het voertuig mag zijn eigen cellen gebruiken zonder zichzelf te blokkeren.
    /// </summary>
    public bool IsCellFree(Vector2Int cell, VehicleController requestingVehicle)
    {
        if (!IsInsideGrid(cell))
        {
            return false;
        }

        if (!cellOccupancy.TryGetValue(cell, out VehicleController occupant))
        {
            return true;
        }

        return occupant == requestingVehicle;
    }

    /// <summary>
    /// Controleert of alle opgegeven cellen vrij zijn voor het opgegeven voertuig.
    /// </summary>
    public bool AreCellsFree(List<Vector2Int> cells, VehicleController requestingVehicle)
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

    /// <summary>
    /// Registreert welke cellen een voertuig bezet.
    /// Geeft false terug als een cel al door een ander voertuig bezet is.
    /// </summary>
    public bool RegisterVehicle(VehicleController vehicle, List<Vector2Int> occupiedCells)
    {
        if (vehicle == null || occupiedCells == null)
        {
            return false;
        }

        if (!AreCellsFree(occupiedCells, vehicle))
        {
            return false;
        }

        // Verwijder oude registratie van dit voertuig (bij verplaatsen).
        UnregisterVehicle(vehicle);

        List<Vector2Int> registeredCells = new List<Vector2Int>();

        foreach (Vector2Int cell in occupiedCells)
        {
            cellOccupancy[cell] = vehicle;
            registeredCells.Add(cell);
        }

        cellsByVehicle[vehicle] = registeredCells;
        return true;
    }

    /// <summary>
    /// Geeft alle cellen vrij die door het opgegeven voertuig bezet waren.
    /// </summary>
    public void UnregisterVehicle(VehicleController vehicle)
    {
        if (vehicle == null || !cellsByVehicle.TryGetValue(vehicle, out List<Vector2Int> cells))
        {
            return;
        }

        foreach (Vector2Int cell in cells)
        {
            if (cellOccupancy.TryGetValue(cell, out VehicleController occupant) && occupant == vehicle)
            {
                cellOccupancy.Remove(cell);
            }
        }

        cellsByVehicle.Remove(vehicle);
    }

    /// <summary>
    /// Geeft alle gridcellen terug die een voertuig bezet bij een ankerpositie en afmeting.
    /// Anker = linksonder cel. Grootte bv. (2,1) voor 2x1 horizontaal, (1,3) voor 1x3 verticaal.
    /// </summary>
    public List<Vector2Int> GetCellsForArea(Vector2Int anchorPosition, Vector2Int sizeInCells)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        for (int x = 0; x < sizeInCells.x; x++)
        {
            for (int y = 0; y < sizeInCells.y; y++)
            {
                cells.Add(new Vector2Int(anchorPosition.x + x, anchorPosition.y + y));
            }
        }

        return cells;
    }

    /// <summary>
    /// Geeft de ankerpositie (linksonder cel) terug op basis van een wereldpositie en voertuiggrootte.
    /// </summary>
    public Vector2Int GetAnchorFromWorldPosition(Vector2 worldPosition, Vector2Int sizeInCells)
    {
        Vector2 origin = GridOrigin;
        Vector2 local = worldPosition - origin;

        int anchorX = Mathf.RoundToInt(local.x / cellSize - sizeInCells.x * 0.5f);
        int anchorY = Mathf.RoundToInt(local.y / cellSize - sizeInCells.y * 0.5f);

        return new Vector2Int(anchorX, anchorY);
    }

    /// <summary>
    /// Maakt alle cellen vrij. Handig bij het resetten van een level.
    /// </summary>
    public void ClearOccupiedCells()
    {
        cellOccupancy.Clear();
        cellsByVehicle.Clear();
    }

    // --- Debug-visualisatie in de Scene-view ---

    /// <summary>
    /// Tekent het grid als groene lijnen in de Scene-view (niet in het spel zelf).
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector2 bottomLeft = GridToWorldCorner(new Vector2Int(MinGridX, MinGridY));
        Vector2 topRight = GridToWorldCorner(new Vector2Int(MaxGridX + 1, MaxGridY + 1));

        // Verticale lijnen
        for (int i = 0; i <= gridWidth; i++)
        {
            float worldX = bottomLeft.x + i * cellSize;
            Gizmos.DrawLine(new Vector2(worldX, bottomLeft.y), new Vector2(worldX, topRight.y));
        }

        // Horizontale lijnen
        for (int i = 0; i <= gridHeight; i++)
        {
            float worldY = bottomLeft.y + i * cellSize;
            Gizmos.DrawLine(new Vector2(bottomLeft.x, worldY), new Vector2(topRight.x, worldY));
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Beheert het speelveld-grid voor Rush Out.
/// Zet dit script op een leeg GameObject in je scene (bijv. "GridManager").
/// Gridpositie (0, 0) is de centrale cel.
/// De positie van dit GameObject is het exacte geometrische midden van het grid.
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

    // Welke gridcellen momenteel bezet zijn (bijv. door een voertuig).
    private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

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
    /// Het exacte geometrische midden van het grid in wereldcoördinaten.
    /// Zet dit GameObject op (0, 0) — geen handmatige 0.5-offset nodig.
    /// </summary>
    public Vector2 GridOrigin => transform.position;

    /// <summary>
    /// Interne offset voor het centreren van een even-sized grid.
    /// Bij 6x6 is dit -0.5, zodat het grid symmetrisch rond de oorsprong ligt.
    /// </summary>
    private float GridCenterOffsetX => (MinGridX + MaxGridX) / 2f;
    private float GridCenterOffsetY => (MinGridY + MaxGridY) / 2f;

    /// <summary>
    /// Laagste grid-x-coördinaat. Bij een 6x6 grid is dit -3.
    /// </summary>
    public int MinGridX => -gridWidth / 2;

    /// <summary>
    /// Hoogste grid-x-coördinaat. Bij een 6x6 grid is dit 2.
    /// </summary>
    public int MaxGridX => (gridWidth - 1) / 2;

    /// <summary>
    /// Laagste grid-y-coördinaat.
    /// </summary>
    public int MinGridY => -gridHeight / 2;

    /// <summary>
    /// Hoogste grid-y-coördinaat.
    /// </summary>
    public int MaxGridY => (gridHeight - 1) / 2;

    // --- Positie-omrekening ---

    /// <summary>
    /// Zet een wereldpositie om naar een gridpositie.
    /// </summary>
    public Vector2Int WorldToGrid(Vector2 worldPosition)
    {
        Vector2 localPosition = worldPosition - GridOrigin;

        float gridX = localPosition.x / cellSize + GridCenterOffsetX;
        float gridY = localPosition.y / cellSize + GridCenterOffsetY;

        int x = Mathf.FloorToInt(gridX + 0.5f);
        int y = Mathf.FloorToInt(gridY + 0.5f);

        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Zet een gridpositie om naar het midden van die cel in wereldcoördinaten.
    /// </summary>
    public Vector2 GridToWorld(Vector2Int gridPosition)
    {
        float worldX = GridOrigin.x + (gridPosition.x - GridCenterOffsetX) * cellSize;
        float worldY = GridOrigin.y + (gridPosition.y - GridCenterOffsetY) * cellSize;

        return new Vector2(worldX, worldY);
    }

    /// <summary>
    /// Zet een gridpositie om naar het linksonder-punt van die cel.
    /// </summary>
    public Vector2 GridToWorldCorner(Vector2Int gridPosition)
    {
        float halfCell = cellSize * 0.5f;
        Vector2 cellCenter = GridToWorld(gridPosition);

        return new Vector2(cellCenter.x - halfCell, cellCenter.y - halfCell);
    }

    // --- Grid-validatie ---

    /// <summary>
    /// Controleert of een gridpositie binnen het speelveld valt.
    /// Bij 6x6: x en y lopen van -3 t/m 2.
    /// </summary>
    public bool IsInsideGrid(Vector2Int gridPosition)
    {
        return gridPosition.x >= MinGridX
            && gridPosition.x <= MaxGridX
            && gridPosition.y >= MinGridY
            && gridPosition.y <= MaxGridY;
    }

    // --- Bezetting ---

    /// <summary>
    /// Controleert of een gridcel bezet is.
    /// Retourneert ook true als de positie buiten het grid valt.
    /// </summary>
    public bool IsOccupied(Vector2Int gridPosition)
    {
        if (!IsInsideGrid(gridPosition))
        {
            return true;
        }

        return occupiedCells.Contains(gridPosition);
    }

    /// <summary>
    /// Markeert een cel als bezet of vrij.
    /// </summary>
    public void SetOccupied(Vector2Int gridPosition, bool occupied)
    {
        if (!IsInsideGrid(gridPosition))
        {
            return;
        }

        if (occupied)
        {
            occupiedCells.Add(gridPosition);
        }
        else
        {
            occupiedCells.Remove(gridPosition);
        }
    }

    /// <summary>
    /// Maakt alle cellen vrij. Handig bij het resetten van een level.
    /// </summary>
    public void ClearOccupiedCells()
    {
        occupiedCells.Clear();
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

        // Markeer het geometrische middelpunt van het grid
        Gizmos.color = Color.yellow;
        Vector2 center = GridOrigin;
        float crossSize = cellSize * 0.2f;
        Gizmos.DrawLine(center - Vector2.right * crossSize, center + Vector2.right * crossSize);
        Gizmos.DrawLine(center - Vector2.up * crossSize, center + Vector2.up * crossSize);
    }
}

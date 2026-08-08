using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bouwt de visuele parkeerplaats (asphalt + borders) op basis van gridWidth/Height.
/// Geen gameplay/colliders — alleen visuals. Wordt aangeroepen bij level load.
/// </summary>
public class ParkingGridVisual : MonoBehaviour
{
    [Header("Parents")]
    [Tooltip("Container voor asphalt tiles. Wordt leeggemaakt bij Rebuild.")]
    [SerializeField] private Transform tilesParent;

    [Tooltip("Container voor borders/corners. Wordt leeggemaakt bij Rebuild.")]
    [SerializeField] private Transform bordersParent;

    [Tooltip("Container voor interne gridlijnen. Wordt leeggemaakt bij Rebuild.")]
    [SerializeField] private Transform gridLinesParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject asphaltTilePrefab;
    [SerializeField] private GameObject borderTopPrefab;
    [SerializeField] private GameObject borderBottomPrefab;
    [SerializeField] private GameObject borderLeftPrefab;
    [SerializeField] private GameObject borderRightPrefab;
    [SerializeField] private GameObject cornerTLPrefab;
    [SerializeField] private GameObject cornerTRPrefab;
    [SerializeField] private GameObject cornerBLPrefab;
    [SerializeField] private GameObject cornerBRPrefab;
    [SerializeField] private GameObject gridLinePrefab;

    [Header("Layout")]
    [Tooltip("Moet overeenkomen met GridManager.CellSize (meestal 1).")]
    [SerializeField] private float tileSize = 1f;

    [Tooltip("Optioneel: zelfde CellToWorld als gameplay. Anders lokale centrering.")]
    [SerializeField] private GridManager gridManager;

    [Header("Sorting")]
    [Tooltip("Asphalt onder vehicles (vehicles ≈ 0).")]
    [SerializeField] private int tileSortingOrder = -10;

    [Tooltip("Borders iets boven asphalt.")]
    [SerializeField] private int borderSortingOrder = -9;

    [Tooltip("Gridlijnen boven asphalt, onder vehicles.")]
    [SerializeField] private int gridLineSortingOrder = -8;

    [Header("Corner Fine-tune")]
    [Tooltip("Kleine overlap van endpoint border-segmenten richting de corners.")]
    [SerializeField] private float borderCornerOverlap = 0.08f;

    [Tooltip("Extra lengte per recht border-segment t.o.v. tileSize, om naden te dichten.")]
    [SerializeField] private float borderSegmentOverlap = 0.03f;

    [Tooltip("Extra X-offset voor rechte Left borders (negatief = verder naar links).")]
    [SerializeField] private float leftBorderOffsetX = -0.08f;

    [Tooltip("Extra X-offset voor rechte Right borders (positief = verder naar rechts).")]
    [SerializeField] private float rightBorderOffsetX = 0.08f;

    [SerializeField] private Vector2 topLeftCornerOffset;
    [SerializeField] private Vector2 topRightCornerOffset;
    [SerializeField] private Vector2 bottomLeftCornerOffset;
    [SerializeField] private Vector2 bottomRightCornerOffset;

    // Voorkomt dat we per ongeluk in Edit Mode blijven hangen met runtime-objecten.
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private void Awake()
    {
        EnsureParents();
    }

    /// <summary>
    /// Bouwt (of herbouwt) het hele visuele grid.
    /// Oude tiles/borders worden eerst verwijderd — veilig bij restart / size-change.
    /// </summary>
    public void BuildGrid(int gridWidth, int gridHeight)
    {
        gridWidth = Mathf.Max(1, gridWidth);
        gridHeight = Mathf.Max(1, gridHeight);

        EnsureParents();
        ClearGeneratedVisuals();

        BuildAsphaltTiles(gridWidth, gridHeight);
        BuildBorders(gridWidth, gridHeight);
        BuildGridLines(gridWidth, gridHeight);
    }

    // -------------------------------------------------------------------------
    // Asphalt
    // -------------------------------------------------------------------------

    private void BuildAsphaltTiles(int gridWidth, int gridHeight)
    {
        if (asphaltTilePrefab == null)
        {
            Debug.LogWarning("ParkingGridVisual: asphaltTilePrefab ontbreekt.");
            return;
        }

        int created = 0;
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 worldPos = CellToWorld(new Vector2Int(x, y), gridWidth, gridHeight);
                GameObject tile = SpawnVisual(
                    asphaltTilePrefab,
                    tilesParent,
                    worldPos,
                    "Tile_" + x + "_" + y
                );
                ApplySorting(tile, tileSortingOrder);
                created++;
            }
        }

        Debug.Log("Created " + created + " asphalt tiles");
    }

    // -------------------------------------------------------------------------
    // Borders + corners
    // -------------------------------------------------------------------------

    private void BuildBorders(int gridWidth, int gridHeight)
    {
        // Outer cell centers.
        Vector3 bl = CellToWorld(new Vector2Int(0, 0), gridWidth, gridHeight);
        Vector3 br = CellToWorld(new Vector2Int(gridWidth - 1, 0), gridWidth, gridHeight);
        Vector3 tl = CellToWorld(new Vector2Int(0, gridHeight - 1), gridWidth, gridHeight);

        // 0.5 cell buiten de outer centers = tegen de speelveldrand.
        float half = tileSize * 0.5f;
        float bottomY = bl.y - half;
        float topY = tl.y + half;
        float leftX = bl.x - half;
        float rightX = br.x + half;

        // Top / bottom middensegmenten (hoeken apart).
        // Endpoint-segmenten schuiven subtiel richting de corners om naden te dichten.
        for (int x = 1; x < gridWidth - 1; x++)
        {
            float overlapX = 0f;
            if (x == 1)
            {
                overlapX -= borderCornerOverlap;
            }

            if (x == gridWidth - 2)
            {
                overlapX += borderCornerOverlap;
            }

            Vector3 cell = CellToWorld(new Vector2Int(x, gridHeight - 1), gridWidth, gridHeight);
            SpawnStraightBorder(borderTopPrefab, new Vector3(cell.x + overlapX, topY, 0f), "Border_Top_" + x, true);

            cell = CellToWorld(new Vector2Int(x, 0), gridWidth, gridHeight);
            SpawnStraightBorder(borderBottomPrefab, new Vector3(cell.x + overlapX, bottomY, 0f), "Border_Bottom_" + x, true);
        }

        // Left / right middensegmenten (hoeken apart).
        for (int y = 1; y < gridHeight - 1; y++)
        {
            float overlapY = 0f;
            if (y == gridHeight - 2)
            {
                overlapY += borderCornerOverlap;
            }

            if (y == 1)
            {
                overlapY -= borderCornerOverlap;
            }

            Vector3 cell = CellToWorld(new Vector2Int(0, y), gridWidth, gridHeight);
            SpawnStraightBorder(
                borderLeftPrefab,
                new Vector3(leftX + leftBorderOffsetX, cell.y + overlapY, 0f),
                "Border_Left_" + y,
                false);

            cell = CellToWorld(new Vector2Int(gridWidth - 1, y), gridWidth, gridHeight);
            SpawnStraightBorder(
                borderRightPrefab,
                new Vector3(rightX + rightBorderOffsetX, cell.y + overlapY, 0f),
                "Border_Right_" + y,
                false);
        }

        // Corners: zelfde X als first/last kolom + zelfde Y-offset als Top/Bottom borders.
        // Geen extra half-cell naar buiten t.o.v. de straight borders.
        float firstCellX = bl.x;
        float lastCellX = br.x;

        Vector3 cornerTL = new Vector3(firstCellX, topY, 0f) + (Vector3)topLeftCornerOffset;
        Vector3 cornerTR = new Vector3(lastCellX, topY, 0f) + (Vector3)topRightCornerOffset;
        Vector3 cornerBL = new Vector3(firstCellX, bottomY, 0f) + (Vector3)bottomLeftCornerOffset;
        Vector3 cornerBR = new Vector3(lastCellX, bottomY, 0f) + (Vector3)bottomRightCornerOffset;

        Debug.Log("Corner TL world pos: " + cornerTL);
        Debug.Log("Corner TR world pos: " + cornerTR);
        Debug.Log("Corner BL world pos: " + cornerBL);
        Debug.Log("Corner BR world pos: " + cornerBR);

        SpawnBorder(cornerTLPrefab, cornerTL, "Corner_TL");
        SpawnBorder(cornerTRPrefab, cornerTR, "Corner_TR");
        SpawnBorder(cornerBLPrefab, cornerBL, "Corner_BL");
        SpawnBorder(cornerBRPrefab, cornerBR, "Corner_BR");
    }

    private void SpawnBorder(GameObject prefab, Vector3 worldPos, string objectName)
    {
        if (prefab == null)
        {
            return;
        }

        GameObject instance = SpawnVisual(prefab, bordersParent, worldPos, objectName);
        ApplySorting(instance, borderSortingOrder);
    }

    /// <summary>
    /// Rechte border: schaal alleen de lengte-as naar tileSize + borderSegmentOverlap.
    /// Corners gebruiken SpawnBorder en blijven ongeschaald.
    /// </summary>
    private void SpawnStraightBorder(
        GameObject prefab,
        Vector3 worldPos,
        string objectName,
        bool horizontalLength)
    {
        if (prefab == null)
        {
            return;
        }

        GameObject instance = SpawnVisual(prefab, bordersParent, worldPos, objectName);
        ApplySorting(instance, borderSortingOrder);
        ApplyStraightBorderScale(instance, horizontalLength);
    }

    private void ApplyStraightBorderScale(GameObject instance, bool horizontalLength)
    {
        SpriteRenderer renderer = instance.GetComponentInChildren<SpriteRenderer>(true);
        if (renderer == null || renderer.sprite == null)
        {
            return;
        }

        float desiredLength = tileSize + borderSegmentOverlap;
        Vector3 scale = instance.transform.localScale;

        if (horizontalLength)
        {
            float nativeWidth = renderer.sprite.bounds.size.x;
            if (nativeWidth > 0.0001f)
            {
                scale.x = desiredLength / nativeWidth;
            }
        }
        else
        {
            float nativeHeight = renderer.sprite.bounds.size.y;
            if (nativeHeight > 0.0001f)
            {
                scale.y = desiredLength / nativeHeight;
            }
        }

        instance.transform.localScale = scale;
    }

    // -------------------------------------------------------------------------
    // Interne gridlijnen
    // -------------------------------------------------------------------------

    private void BuildGridLines(int gridWidth, int gridHeight)
    {
        if (gridLinePrefab == null)
        {
            Debug.LogWarning("ParkingGridVisual: gridLinePrefab ontbreekt.");
            return;
        }

        Vector3 bottomLeft = CellToWorld(new Vector2Int(0, 0), gridWidth, gridHeight);
        Vector3 bottomRight = CellToWorld(new Vector2Int(gridWidth - 1, 0), gridWidth, gridHeight);
        Vector3 topLeft = CellToWorld(new Vector2Int(0, gridHeight - 1), gridWidth, gridHeight);

        float centerX = (bottomLeft.x + bottomRight.x) * 0.5f;
        float centerY = (bottomLeft.y + topLeft.y) * 0.5f;

        int created = 0;

        // Horizontaal: tussen rijen (rotation Z = 0, lengte = volle gridWidth).
        float horizontalLength = gridWidth * tileSize;
        for (int y = 0; y < gridHeight - 1; y++)
        {
            Vector3 below = CellToWorld(new Vector2Int(0, y), gridWidth, gridHeight);
            Vector3 above = CellToWorld(new Vector2Int(0, y + 1), gridWidth, gridHeight);
            float lineY = (below.y + above.y) * 0.5f;

            SpawnGridLine(
                new Vector3(centerX, lineY, 0f),
                0f,
                horizontalLength,
                "GridLine_H_" + y
            );
            created++;
        }

        // Verticaal: tussen kolommen (rotation Z = 90, lengte = volle gridHeight).
        float verticalLength = gridHeight * tileSize;
        for (int x = 0; x < gridWidth - 1; x++)
        {
            Vector3 left = CellToWorld(new Vector2Int(x, 0), gridWidth, gridHeight);
            Vector3 right = CellToWorld(new Vector2Int(x + 1, 0), gridWidth, gridHeight);
            float lineX = (left.x + right.x) * 0.5f;

            SpawnGridLine(
                new Vector3(lineX, centerY, 0f),
                90f,
                verticalLength,
                "GridLine_V_" + x
            );
            created++;
        }

        Debug.Log("Created " + created + " grid lines");
    }

    private void SpawnGridLine(Vector3 worldPos, float rotationZ, float length, string objectName)
    {
        GameObject instance = Instantiate(gridLinePrefab, gridLinesParent);
        instance.name = objectName;
        instance.transform.SetPositionAndRotation(worldPos, Quaternion.Euler(0f, 0f, rotationZ));

        StripGameplayColliders(instance);
        ApplySorting(instance, gridLineSortingOrder);

        // Prefab is horizontaal: schaal alleen lokale X (lengterichting).
        float nativeLength = GetUnscaledSpriteWidth(instance);
        Vector3 scale = instance.transform.localScale;
        float scaleX = nativeLength > 0.0001f ? length / nativeLength : length;
        instance.transform.localScale = new Vector3(scaleX, scale.y, scale.z);

        spawnedObjects.Add(instance);
    }

    private static float GetUnscaledSpriteWidth(GameObject root)
    {
        SpriteRenderer renderer = root.GetComponentInChildren<SpriteRenderer>(true);
        if (renderer != null && renderer.sprite != null)
        {
            return renderer.sprite.bounds.size.x;
        }

        return 1f;
    }

    // -------------------------------------------------------------------------
    // Cell → world (zelfde centrering als GridManager)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Celcentrum in world space. Gebruikt GridManager als die geconfigureerd is,
    /// anders dezelfde formule met lokale tileSize / dimensions.
    /// </summary>
    private Vector3 CellToWorld(Vector2Int cell, int gridWidth, int gridHeight)
    {
        if (gridManager != null &&
            gridManager.GridWidth == gridWidth &&
            gridManager.GridHeight == gridHeight)
        {
            return gridManager.CellToWorld(cell);
        }

        // Fallback: x - (width-1)*0.5, y - (height-1)*0.5, * tileSize
        float worldX = (cell.x - (gridWidth - 1) * 0.5f) * tileSize;
        float worldY = (cell.y - (gridHeight - 1) * 0.5f) * tileSize;
        return new Vector3(worldX, worldY, 0f);
    }

    // -------------------------------------------------------------------------
    // Spawn / cleanup helpers
    // -------------------------------------------------------------------------

    private GameObject SpawnVisual(
        GameObject prefab,
        Transform parent,
        Vector3 worldPosition,
        string objectName)
    {
        GameObject instance = Instantiate(prefab, parent);
        instance.name = objectName;
        instance.transform.position = worldPosition;
        instance.transform.rotation = Quaternion.identity;

        // Visueel only — geen physics.
        StripGameplayColliders(instance);

        spawnedObjects.Add(instance);
        return instance;
    }

    private static void StripGameplayColliders(GameObject root)
    {
        Collider2D[] colliders = root.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Destroy(colliders[i]);
        }
    }

    private static void ApplySorting(GameObject root, int sortingOrder)
    {
        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingOrder = sortingOrder;
        }
    }

    private void ClearGeneratedVisuals()
    {
        // Tracked list (runtime).
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }

        spawnedObjects.Clear();

        // Extra safety: wis alle children onder de parents (ook na domain reload).
        ClearChildren(tilesParent);
        ClearChildren(bordersParent);
        ClearChildren(gridLinesParent);
    }

    private static void ClearChildren(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void EnsureParents()
    {
        if (tilesParent == null)
        {
            Transform existing = transform.Find("Tiles");
            if (existing != null)
            {
                tilesParent = existing;
            }
            else
            {
                GameObject tiles = new GameObject("Tiles");
                tiles.transform.SetParent(transform, false);
                tilesParent = tiles.transform;
            }
        }

        if (bordersParent == null)
        {
            Transform existing = transform.Find("Borders");
            if (existing != null)
            {
                bordersParent = existing;
            }
            else
            {
                GameObject borders = new GameObject("Borders");
                borders.transform.SetParent(transform, false);
                bordersParent = borders.transform;
            }
        }

        if (gridLinesParent == null)
        {
            Transform existing = transform.Find("GridLines");
            if (existing != null)
            {
                gridLinesParent = existing;
            }
            else
            {
                GameObject gridLines = new GameObject("GridLines");
                gridLines.transform.SetParent(transform, false);
                gridLinesParent = gridLines.transform;
            }
        }
    }
}

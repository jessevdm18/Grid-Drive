using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Bestuurt een voertuig in Rush Out met echte grid-based movement.
/// De logische gridpositie (Vector2Int) is de bron van waarheid — niet transform.position.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class VehicleController : MonoBehaviour
{
    /// <summary>
    /// Richting waarin het voertuig mag bewegen.
    /// </summary>
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    [Header("Grid")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private int gridWidth = 6;
    [SerializeField] private int gridHeight = 6;
    [SerializeField] private GridManager gridManager;

    [Header("Voertuig")]
    [Tooltip("Horizontaal = beweegt op X-as. Verticaal = beweegt op Y-as.")]
    [SerializeField] private Orientation orientation = Orientation.Horizontal;

    [Tooltip("Aantal cellen in beweegrichting (bijv. 2 of 3).")]
    [SerializeField] private int lengthInCells = 1;

    [Header("Beweging")]
    [Tooltip("Snelheid van visuele animatie tussen gridcellen.")]
    [SerializeField] private float smoothSpeed = 12f;

    // --- Logische gridstate (bron van waarheid) ---

    // Huidige gridpositie: linksonder cel van dit voertuig.
    private Vector2Int gridPosition;

    // Doelpositie tijdens slepen (altijd een geldige hele gridpositie).
    private Vector2Int targetGridPosition;

    // Gridpositie bij start van slepen.
    private Vector2Int dragStartGridPosition;

    // --- Runtime ---

    private bool isDragging;
    private Camera mainCamera;
    private Vector3 moveVelocity;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Start()
    {
        // Eenmalig: lees startpositie uit scene en snap visueel naar grid.
        gridPosition = WorldToGridAnchor(transform.position);
        gridPosition = ClampToBounds(gridPosition);
        targetGridPosition = gridPosition;

        SnapVisualToGrid();
        RegisterOccupancy();
    }

    private void Update()
    {
        // Visueel soepel bewegen naar de doel-gridpositie.
        AnimateToTarget();
    }

    private void OnMouseDown()
    {
        isDragging = true;
        dragStartGridPosition = gridPosition;
        targetGridPosition = gridPosition;

        // Geef eigen cellen vrij zodat dit voertuig zichzelf niet blokkeert.
        if (gridManager != null)
        {
            gridManager.UnregisterVehicle(this);
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
        {
            return;
        }

        // Muis/touch bepaalt alleen de gewenste gridpositie — geen vrije wereldbeweging.
        Vector2Int desiredGrid = GetTargetGridFromPointer(GetPointerWorldPosition());
        desiredGrid = ClampToBounds(desiredGrid);

        // Cel voor cel controleren zodat het voertuig niet door andere auto's springt.
        targetGridPosition = FindLastValidGridPosition(dragStartGridPosition, desiredGrid);
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // Logische positie bijwerken naar de laatste geldige gridpositie.
        gridPosition = targetGridPosition;
        RegisterOccupancy();
    }

    // --- Grid ↔ World ---

    /// <summary>
    /// Zet een grid-anker (linksonder cel) om naar de wereldpositie van het voertuigmiddelpunt.
    /// </summary>
    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        Vector2 origin = GetGridWorldOrigin();
        float worldX;
        float worldY;

        if (orientation == Orientation.Horizontal)
        {
            // Middelpunt over lengthInCells horizontale cellen.
            worldX = origin.x + (gridPos.x + lengthInCells * 0.5f) * cellSize;
            worldY = origin.y + (gridPos.y + 0.5f) * cellSize;
        }
        else
        {
            worldX = origin.x + (gridPos.x + 0.5f) * cellSize;
            worldY = origin.y + (gridPos.y + lengthInCells * 0.5f) * cellSize;
        }

        return new Vector3(worldX, worldY, transform.position.z);
    }

    /// <summary>
    /// Berekent het grid-anker (linksonder) vanuit een wereldpositie.
    /// Alleen gebruikt bij Start om voertuigen in de scene te initialiseren.
    /// </summary>
    private Vector2Int WorldToGridAnchor(Vector2 worldPosition)
    {
        Vector2 origin = GetGridWorldOrigin();

        if (orientation == Orientation.Horizontal)
        {
            int x = Mathf.RoundToInt((worldPosition.x - origin.x) / cellSize - lengthInCells * 0.5f);
            int y = Mathf.RoundToInt((worldPosition.y - origin.y) / cellSize - 0.5f);
            return new Vector2Int(x, y);
        }

        int anchorX = Mathf.RoundToInt((worldPosition.x - origin.x) / cellSize - 0.5f);
        int anchorY = Mathf.RoundToInt((worldPosition.y - origin.y) / cellSize - lengthInCells * 0.5f);
        return new Vector2Int(anchorX, anchorY);
    }

    /// <summary>
    /// Bepaalt welke gridpositie de speler bedoelt op basis van muis/touch.
    /// </summary>
    private Vector2Int GetTargetGridFromPointer(Vector2 pointerWorld)
    {
        Vector2 origin = GetGridWorldOrigin();

        if (orientation == Orientation.Horizontal)
        {
            int x = Mathf.RoundToInt((pointerWorld.x - origin.x) / cellSize - lengthInCells * 0.5f);
            return new Vector2Int(x, dragStartGridPosition.y);
        }

        int y = Mathf.RoundToInt((pointerWorld.y - origin.y) / cellSize - lengthInCells * 0.5f);
        return new Vector2Int(dragStartGridPosition.x, y);
    }

    /// <summary>
    /// Wereldpositie van de linksonderhoek van cel (0, 0).
    /// </summary>
    private Vector2 GetGridWorldOrigin()
    {
        if (gridManager != null)
        {
            return gridManager.GridOrigin;
        }

        return Vector2.zero;
    }

    // --- Boundaries & validatie ---

    /// <summary>
    /// Houdt het grid-anker binnen de grenzen van het 6x6 speelveld.
    /// </summary>
    private Vector2Int ClampToBounds(Vector2Int anchor)
    {
        if (orientation == Orientation.Horizontal)
        {
            int maxX = gridWidth - lengthInCells;
            int maxY = gridHeight - 1;
            anchor.x = Mathf.Clamp(anchor.x, 0, maxX);
            anchor.y = Mathf.Clamp(anchor.y, 0, maxY);
        }
        else
        {
            int maxX = gridWidth - 1;
            int maxY = gridHeight - lengthInCells;
            anchor.x = Mathf.Clamp(anchor.x, 0, maxX);
            anchor.y = Mathf.Clamp(anchor.y, 0, maxY);
        }

        return anchor;
    }

    /// <summary>
    /// Controleert of het voertuig op deze gridpositie past (grenzen + vrije cellen).
    /// </summary>
    private bool IsGridPositionValid(Vector2Int anchor)
    {
        if (!IsWithinBounds(anchor))
        {
            return false;
        }

        if (gridManager == null)
        {
            return true;
        }

        return gridManager.AreCellsFree(GetOccupiedCellsAt(anchor), this);
    }

    /// <summary>
    /// Controleert gridgrenzen zonder collider — puur op celcoördinaten.
    /// </summary>
    private bool IsWithinBounds(Vector2Int anchor)
    {
        if (orientation == Orientation.Horizontal)
        {
            return anchor.x >= 0
                && anchor.x + lengthInCells <= gridWidth
                && anchor.y >= 0
                && anchor.y + 1 <= gridHeight;
        }

        return anchor.x >= 0
            && anchor.x + 1 <= gridWidth
            && anchor.y >= 0
            && anchor.y + lengthInCells <= gridHeight;
    }

    /// <summary>
    /// Loopt cel voor cel van start naar doel en stopt bij het eerste obstakel.
    /// </summary>
    private Vector2Int FindLastValidGridPosition(Vector2Int start, Vector2Int target)
    {
        Vector2Int best = start;

        if (orientation == Orientation.Horizontal)
        {
            int direction = Mathf.Clamp(target.x - start.x, -1, 1);

            if (direction == 0)
            {
                return start;
            }

            for (int x = start.x; direction > 0 ? x <= target.x : x >= target.x; x += direction)
            {
                Vector2Int test = new Vector2Int(x, start.y);

                if (IsGridPositionValid(test))
                {
                    best = test;
                }
                else
                {
                    break;
                }
            }
        }
        else
        {
            int direction = Mathf.Clamp(target.y - start.y, -1, 1);

            if (direction == 0)
            {
                return start;
            }

            for (int y = start.y; direction > 0 ? y <= target.y : y >= target.y; y += direction)
            {
                Vector2Int test = new Vector2Int(start.x, y);

                if (IsGridPositionValid(test))
                {
                    best = test;
                }
                else
                {
                    break;
                }
            }
        }

        return best;
    }

    // --- Occupancy ---

    /// <summary>
    /// Grootte in cellen voor GridManager (horizontaal: 2x1, verticaal: 1x2, etc.).
    /// </summary>
    private Vector2Int GetOccupancySize()
    {
        if (orientation == Orientation.Horizontal)
        {
            return new Vector2Int(lengthInCells, 1);
        }

        return new Vector2Int(1, lengthInCells);
    }

    /// <summary>
    /// Geeft alle gridcellen die dit voertuig bezet op de opgegeven ankerpositie.
    /// </summary>
    private List<Vector2Int> GetOccupiedCellsAt(Vector2Int anchor)
    {
        return gridManager.GetCellsForArea(anchor, GetOccupancySize());
    }

    /// <summary>
    /// Registreert de huidige gridcellen als bezet bij GridManager.
    /// </summary>
    private void RegisterOccupancy()
    {
        if (gridManager == null)
        {
            return;
        }

        List<Vector2Int> cells = GetOccupiedCellsAt(gridPosition);

        if (!gridManager.RegisterVehicle(this, cells))
        {
            // Alleen terugvallen tijdens slepen als registratie mislukt.
            if (isDragging)
            {
                gridPosition = dragStartGridPosition;
                targetGridPosition = gridPosition;
                SnapVisualToGrid();
                gridManager.RegisterVehicle(this, GetOccupiedCellsAt(gridPosition));
            }
        }
    }

    // --- Visuele beweging ---

    /// <summary>
    /// Animeert transform.position soepel naar de doel-gridpositie.
    /// </summary>
    private void AnimateToTarget()
    {
        Vector3 targetWorld = GridToWorld(isDragging ? targetGridPosition : gridPosition);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetWorld,
            ref moveVelocity,
            1f / smoothSpeed
        );
    }

    /// <summary>
    /// Zet het voertuig direct op de gridpositie (geen animatie).
    /// </summary>
    private void SnapVisualToGrid()
    {
        transform.position = GridToWorld(gridPosition);
        moveVelocity = Vector3.zero;
    }

    /// <summary>
    /// Leest muis- of touchpositie en zet die om naar wereldcoördinaten.
    /// </summary>
    private Vector2 GetPointerWorldPosition()
    {
        Vector2 screenPosition = Pointer.current != null
            ? Pointer.current.position.ReadValue()
            : Vector2.zero;

        float depth = Mathf.Abs(transform.position.z - mainCamera.transform.position.z);
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));

        return new Vector2(worldPoint.x, worldPoint.y);
    }
}

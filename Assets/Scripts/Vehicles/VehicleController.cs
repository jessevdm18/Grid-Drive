using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleController : MonoBehaviour
{
    public enum VehicleOrientation
    {
        Horizontal,
        Vertical
    }

    [Header("Exit Settings")]
[SerializeField] private bool canExitRight = false;
[SerializeField] private int exitRow = 2;
[SerializeField] private GameManager gameManager;

    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Vehicle Settings")]
    [SerializeField] private VehicleOrientation orientation
        = VehicleOrientation.Horizontal;

    [SerializeField] private int lengthInCells = 2;

    [Header("Starting Grid Position")]
    [Tooltip("De onderste/linker gridcel die dit voertuig bezet.")]
    [SerializeField] private Vector2Int gridPosition;

    private Vector2Int dragStartGridPosition;
    private Vector3 dragStartMouseWorld;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        // Zorg dat we meteen exact op de juiste gridpositie staan.
        transform.position = GetWorldPosition(gridPosition);

        // Registreer onze bezette cellen.
        gridManager.RegisterVehicle(this, GetOccupiedCells(gridPosition));
    }

    private void OnMouseDown()
    {
        if (gridManager == null)
        {
            Debug.LogError("Geen GridManager gekoppeld aan " + name);
            return;
        }

        dragStartGridPosition = gridPosition;
        dragStartMouseWorld = GetMouseWorldPosition();
    }

private bool CanExitRight(Vector3 dragDifference)
{
    // Alleen auto's die expliciet toestemming hebben.
    if (!canExitRight)
        return false;

    // Alleen horizontale voertuigen.
    if (orientation != VehicleOrientation.Horizontal)
        return false;

    // Alleen op de rij waar de uitgang zit.
    if (gridPosition.y != exitRow)
        return false;

    // De auto moet al helemaal rechts staan.
    int rightMostValidX =
        gridManager.GridWidth - lengthInCells;

    if (gridPosition.x != rightMostValidX)
        return false;

    // De speler moet nog duidelijk verder naar rechts slepen.
    // Eén halve cel extra voelt op mobiel vrij natuurlijk.
    float requiredExtraDrag =
        gridManager.CellSize * 0.5f;

    return dragDifference.x > requiredExtraDrag;
}

    private void OnMouseDrag()
    {
        if (gridManager == null)
            return;

        Vector3 currentMouseWorld = GetMouseWorldPosition();

        Vector3 dragDifference =
            currentMouseWorld - dragStartMouseWorld;

            // Controleer of deze auto via de rechteruitgang
// van het bord mag rijden.
if (CanExitRight(dragDifference))
{
    ExitBoard();

    if (gameManager != null)
    {
        gameManager.CheckWinCondition();
    }

    return;
}

        Vector2Int wantedPosition = dragStartGridPosition;

        // De muis bepaalt alleen HOEVEEL HELE CELLEN
        // we vanaf de startpositie willen bewegen.
        if (orientation == VehicleOrientation.Horizontal)
        {
            int cellMovement =
                Mathf.RoundToInt(
                    dragDifference.x / gridManager.CellSize
                );

            wantedPosition.x += cellMovement;
        }
        else
        {
            int cellMovement =
                Mathf.RoundToInt(
                    dragDifference.y / gridManager.CellSize
                );

            wantedPosition.y += cellMovement;
        }

        // Zorg eerst dat de gewenste positie niet buiten het bord kan liggen.
        wantedPosition = ClampGridPosition(wantedPosition);

        // Zoek vanuit de oorspronkelijke positie cel voor cel
        // hoe ver we daadwerkelijk mogen bewegen.
        Vector2Int validPosition =
            FindFarthestValidPosition(
                dragStartGridPosition,
                wantedPosition
            );

        // Alleen als de logische positie verandert.
        if (validPosition != gridPosition)
        {
            gridPosition = validPosition;

            // NIEUWE occupancy registreren.
            gridManager.RegisterVehicle(
                this,
                GetOccupiedCells(gridPosition)
            );
        }

        // Belangrijk:
        // de auto staat ALTIJD exact op een gridpositie.
        // Dus geen halve vakken tijdens het slepen.
        transform.position = GetWorldPosition(gridPosition);
    }

    private void OnMouseUp()
    {
        // Voor de zekerheid exact snap naar het grid.
        transform.position = GetWorldPosition(gridPosition);

        gridManager.RegisterVehicle(
            this,
            GetOccupiedCells(gridPosition)
        );
    }

   private Vector3 GetMouseWorldPosition()
{
    Vector2 screenPosition;

    // Muis / editor
    if (Mouse.current != null)
    {
        screenPosition = Mouse.current.position.ReadValue();
    }
    // Touchscreen / mobiel
    else if (Touchscreen.current != null)
    {
        screenPosition =
            Touchscreen.current.primaryTouch.position.ReadValue();
    }
    else
    {
        return transform.position;
    }

    Vector3 worldPosition =
        mainCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, 0f)
        );

    worldPosition.z = 0f;

    return worldPosition;
}

    // Geeft alle cellen terug die het voertuig bezet.
    public List<Vector2Int> GetOccupiedCells(Vector2Int position)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        for (int i = 0; i < lengthInCells; i++)
        {
            if (orientation == VehicleOrientation.Horizontal)
            {
                cells.Add(
                    new Vector2Int(
                        position.x + i,
                        position.y
                    )
                );
            }
            else
            {
                cells.Add(
                    new Vector2Int(
                        position.x,
                        position.y + i
                    )
                );
            }
        }

        return cells;
    }

    // Houdt de LOGISCHE gridpositie binnen het speelveld.
    private Vector2Int ClampGridPosition(Vector2Int position)
    {
        if (orientation == VehicleOrientation.Horizontal)
        {
            position.x = Mathf.Clamp(
                position.x,
                0,
                gridManager.GridWidth - lengthInCells
            );

            position.y = Mathf.Clamp(
                position.y,
                0,
                gridManager.GridHeight - 1
            );
        }
        else
        {
            position.x = Mathf.Clamp(
                position.x,
                0,
                gridManager.GridWidth - 1
            );

            position.y = Mathf.Clamp(
                position.y,
                0,
                gridManager.GridHeight - lengthInCells
            );
        }

        return position;
    }

    // Controleert de route CEL VOOR CEL.
    // Hierdoor kan een auto niet door een andere auto heen springen.
    private Vector2Int FindFarthestValidPosition(
        Vector2Int from,
        Vector2Int target)
    {
        Vector2Int current = from;

        Vector2Int direction = Vector2Int.zero;

        if (orientation == VehicleOrientation.Horizontal)
        {
            if (target.x > from.x)
                direction = Vector2Int.right;
            else if (target.x < from.x)
                direction = Vector2Int.left;
        }
        else
        {
            if (target.y > from.y)
                direction = Vector2Int.up;
            else if (target.y < from.y)
                direction = Vector2Int.down;
        }

        if (direction == Vector2Int.zero)
            return current;

        while (current != target)
        {
            Vector2Int next = current + direction;

            next = ClampGridPosition(next);

            // Als clamp ons niet verder laat bewegen,
            // hebben we de rand bereikt.
            if (next == current)
                break;

            List<Vector2Int> nextCells =
                GetOccupiedCells(next);

            if (!gridManager.AreCellsFree(nextCells, this))
            {
                // Andere auto blokkeert.
                break;
            }

            current = next;
        }

        return current;
    }

    // Zet de LOGISCHE gridpositie om naar de visuele
    // middenpositie van de volledige auto.
    private Vector3 GetWorldPosition(Vector2Int position)
    {
        Vector3 firstCellCenter =
            gridManager.CellToWorld(position);

        float centerOffset =
            (lengthInCells - 1) *
            gridManager.CellSize *
            0.5f;

        if (orientation == VehicleOrientation.Horizontal)
        {
            return firstCellCenter +
                   new Vector3(centerOffset, 0f, -1f);
        }
        else
        {
            return firstCellCenter +
                   new Vector3(0f, centerOffset, -1f);
        }
    }

    public void ExitBoard()
{
    // Verwijder deze auto uit de bezette gridcellen.
    if (gridManager != null)
    {
        gridManager.UnregisterVehicle(this);
    }

    // Verberg de auto nadat hij het speelveld heeft verlaten.
    gameObject.SetActive(false);
}
}
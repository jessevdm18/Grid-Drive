using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Bestuurt een voertuig in Rush Out.
/// Voeg dit script toe aan een voertuig met een Collider2D.
/// Bij loslaten springt het voertuig naar het dichtstbijzijnde gridpunt.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class VehicleController : MonoBehaviour
{
    /// <summary>
    /// Bepaalt of het voertuig horizontaal (links/rechts) of verticaal (omhoog/omlaag) mag bewegen.
    /// </summary>
    public enum MovementAxis
    {
        Horizontal,
        Vertical
    }

    [Header("Grid")]
    [Tooltip("Grootte van één gridcel. Gebruikt om af te ronden bij loslaten.")]
    [SerializeField] private float cellSize = 1f;

    [Tooltip("Breedte van het speelveld in aantal cellen.")]
    [SerializeField] private int gridWidth = 6;

    [Tooltip("Hoogte van het speelveld in aantal cellen.")]
    [SerializeField] private int gridHeight = 6;

    [Header("Voertuiggrootte")]
    [Tooltip("Optioneel: lengte in cellen als fallback als er geen BoxCollider2D is.")]
    [SerializeField] private int vehicleLengthInCells = 1;

    [Tooltip("Optioneel: breedte in cellen als fallback als er geen BoxCollider2D is.")]
    [SerializeField] private int vehicleWidthInCells = 1;

    [Header("Beweging")]
    [Tooltip("De richting waarin dit voertuig mag schuiven.")]
    [SerializeField] private MovementAxis movementAxis = MovementAxis.Horizontal;

    // True zolang de speler het voertuig sleept.
    private bool isDragging;

    // Positie op de as die vast blijft staan tijdens het slepen.
    private float lockedAxisPosition;

    // Verschil tussen voertuig en cursor bij start van slepen (voorkomt springen).
    private Vector3 dragOffset;

    // Referentie naar de camera voor scherm-naar-wereld omrekening.
    private Camera mainCamera;

    // Collider van het voertuig — bepaalt de werkelijke afmetingen voor boundaries.
    private BoxCollider2D boxCollider;

    // Afstand van de pivot (transform.position) tot elke rand van de collider.
    // Blijft constant zolang het voertuig alleen verschuift, niet schaalt of roteert.
    private float boundsOffsetMinX;
    private float boundsOffsetMaxX;
    private float boundsOffsetMinY;
    private float boundsOffsetMaxY;

    private void Awake()
    {
        mainCamera = Camera.main;
        boxCollider = GetComponent<BoxCollider2D>();
        CacheColliderBoundsOffsets();
    }

    /// <summary>
    /// Slaat op hoe ver elke collider-rand van de pivot af ligt.
    /// Zo weten we bij elke positie precies waar de collider eindigt.
    /// </summary>
    private void CacheColliderBoundsOffsets()
    {
        if (boxCollider == null)
        {
            return;
        }

        Bounds bounds = boxCollider.bounds;

        boundsOffsetMinX = bounds.min.x - transform.position.x;
        boundsOffsetMaxX = bounds.max.x - transform.position.x;
        boundsOffsetMinY = bounds.min.y - transform.position.y;
        boundsOffsetMaxY = bounds.max.y - transform.position.y;
    }

    /// <summary>
    /// Wordt aangeroepen wanneer de speler op het voertuig klikt of tikt.
    /// Start het slepen en onthoud de offset t.o.v. de cursor.
    /// </summary>
    private void OnMouseDown()
    {
        isDragging = true;

        // Onthoud de positie op de as waar het voertuig niet mag bewegen.
        if (movementAxis == MovementAxis.Horizontal)
        {
            lockedAxisPosition = transform.position.y;
        }
        else
        {
            lockedAxisPosition = transform.position.x;
        }

        // Zorg dat het voertuig ook op de vaste as binnen het speelveld blijft.
        lockedAxisPosition = ClampLockedAxis(lockedAxisPosition);

        // Bereken offset zodat het voertuig niet naar de cursor springt.
        Vector3 pointerWorld = GetPointerWorldPosition();
        dragOffset = transform.position - pointerWorld;
    }

    /// <summary>
    /// Wordt elke frame aangeroepen zolang de speler sleept.
    /// Het voertuig volgt de cursor, maar alleen op de toegestane as.
    /// </summary>
    private void OnMouseDrag()
    {
        if (!isDragging)
        {
            return;
        }

        Vector3 pointerWorld = GetPointerWorldPosition() + dragOffset;

        // Beweeg alleen horizontaal of verticaal — niet beide tegelijk.
        Vector3 newPosition;

        if (movementAxis == MovementAxis.Horizontal)
        {
            newPosition = new Vector3(pointerWorld.x, lockedAxisPosition, transform.position.z);
        }
        else
        {
            newPosition = new Vector3(lockedAxisPosition, pointerWorld.y, transform.position.z);
        }

        // Houd het volledige voertuig binnen het speelveld tijdens het slepen.
        transform.position = ClampToGridBounds(newPosition);
    }

    /// <summary>
    /// Wordt aangeroepen wanneer de speler loslaat.
    /// Het voertuig springt naar het dichtstbijzijnde gridpunt.
    /// </summary>
    private void OnMouseUp()
    {
        isDragging = false;
        SnapToGrid();
    }

    /// <summary>
    /// Rondt de positie af naar het dichtstbijzijnde gridpunt op basis van cellSize.
    /// </summary>
    private void SnapToGrid()
    {
        Vector3 position = transform.position;

        if (movementAxis == MovementAxis.Horizontal)
        {
            position.x = SnapAxis(position.x);
            position.y = lockedAxisPosition;
        }
        else
        {
            position.y = SnapAxis(position.y);
            position.x = lockedAxisPosition;
        }

        transform.position = ClampToGridBounds(position);
    }

    /// <summary>
    /// Houdt het voertuig volledig binnen het speelveld.
    /// Gebruikt de BoxCollider2D om de werkelijke randen te berekenen.
    /// </summary>
    private Vector3 ClampToGridBounds(Vector3 position)
    {
        // Randen van het 6x6 speelveld in wereldcoördinaten (gecentreerd op 0,0).
        float fieldMinX = -gridWidth * cellSize * 0.5f;
        float fieldMaxX = gridWidth * cellSize * 0.5f;
        float fieldMinY = -gridHeight * cellSize * 0.5f;
        float fieldMaxY = gridHeight * cellSize * 0.5f;

        if (boxCollider != null)
        {
            // Min/max pivot-posities zodat de volledige collider binnen het veld past.
            // colliderMin = position.x + boundsOffsetMinX  >=  fieldMinX
            // colliderMax = position.x + boundsOffsetMaxX  <=  fieldMaxX
            float minPosX = fieldMinX - boundsOffsetMinX;
            float maxPosX = fieldMaxX - boundsOffsetMaxX;
            float minPosY = fieldMinY - boundsOffsetMinY;
            float maxPosY = fieldMaxY - boundsOffsetMaxY;

            position.x = Mathf.Clamp(position.x, minPosX, maxPosX);
            position.y = Mathf.Clamp(position.y, minPosY, maxPosY);
        }
        else
        {
            // Fallback: gebruik ingestelde cel-afmetingen als er geen collider is.
            float halfLength = vehicleLengthInCells * cellSize * 0.5f;
            float halfWidth = vehicleWidthInCells * cellSize * 0.5f;

            if (movementAxis == MovementAxis.Horizontal)
            {
                position.x = Mathf.Clamp(position.x, fieldMinX + halfLength, fieldMaxX - halfLength);
                position.y = Mathf.Clamp(position.y, fieldMinY + halfWidth, fieldMaxY - halfWidth);
            }
            else
            {
                position.y = Mathf.Clamp(position.y, fieldMinY + halfLength, fieldMaxY - halfLength);
                position.x = Mathf.Clamp(position.x, fieldMinX + halfWidth, fieldMaxX - halfWidth);
            }
        }

        return position;
    }

    /// <summary>
    /// Clampt de vaste as (waar het voertuig niet beweegt) binnen het speelveld.
    /// </summary>
    private float ClampLockedAxis(float axisValue)
    {
        Vector3 clamped = movementAxis == MovementAxis.Horizontal
            ? ClampToGridBounds(new Vector3(transform.position.x, axisValue, transform.position.z))
            : ClampToGridBounds(new Vector3(axisValue, transform.position.y, transform.position.z));

        return movementAxis == MovementAxis.Horizontal ? clamped.y : clamped.x;
    }

    /// <summary>
    /// Rondt één as af naar het dichtstbijzijnde veelvoud van cellSize.
    /// Voorbeeld: waarde 1.3 met cellSize 1 → wordt 1.0
    /// </summary>
    private float SnapAxis(float axisValue)
    {
        return Mathf.Round(axisValue / cellSize) * cellSize;
    }

    /// <summary>
    /// Leest de huidige muis- of touchpositie en zet die om naar wereldcoördinaten.
    /// </summary>
    private Vector3 GetPointerWorldPosition()
    {
        // Pointer.current werkt voor zowel muis als touchscreen.
        Vector2 screenPosition = Pointer.current != null
            ? Pointer.current.position.ReadValue()
            : Vector2.zero;

        // Z-afstand van camera naar voertuig (nodig voor ScreenToWorldPoint).
        float depth = Mathf.Abs(transform.position.z - mainCamera.transform.position.z);

        return mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
    }
}

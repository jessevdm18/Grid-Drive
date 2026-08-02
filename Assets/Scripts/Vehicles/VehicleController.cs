using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Bestuurt een voertuig in Rush Out.
/// Voeg dit script toe aan een voertuig met een Collider2D.
/// Bij loslaten springt het voertuig naar het dichtstbijzijnde gridpunt.
/// </summary>
[RequireComponent(typeof(Collider2D))]
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

    private void Awake()
    {
        mainCamera = Camera.main;
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
        if (movementAxis == MovementAxis.Horizontal)
        {
            transform.position = new Vector3(pointerWorld.x, lockedAxisPosition, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(lockedAxisPosition, pointerWorld.y, transform.position.z);
        }
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

        transform.position = position;
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

using System.Collections;
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

    [Header("Visual")]
    [Tooltip("Child met de SpriteRenderer (bijv. \"Visual\"). Root blijft logica/collider.")]
    [SerializeField] private Transform visualTransform;

    [Tooltip("Duur van de soepele visual-beweging tussen gridcellen (lager = sneller op mobiel).")]
    [SerializeField] private float moveAnimationDuration = 0.06f;

    [Header("Blocked Feedback")]
    [Tooltip("Hoe ver de Visual kort 'tikt' bij een geblokkeerde stap (world units).")]
    [SerializeField] private float blockedShakeDistance = 0.08f;

    [Tooltip("Totale duur van de blocked-tik (heen + terug).")]
    [SerializeField] private float blockedShakeDuration = 0.12f;

    [Tooltip("Minimale tijd tussen twee blocked-feedbacks (voorkomt spam).")]
    [SerializeField] private float blockedFeedbackCooldown = 0.15f;

    [Header("Exit Animation")]
    [Tooltip("Hoe lang de auto doet over wegrijden via de uitgang.")]
    [SerializeField] private float exitAnimationDuration = 0.4f;

    [Tooltip("Hoe ver naar rechts de auto rijdt tijdens de exit (world units).")]
    [SerializeField] private float exitDistance = 4f;

    [Header("Vehicle Settings")]
    [SerializeField] private VehicleOrientation orientation
        = VehicleOrientation.Horizontal;

    [SerializeField] private int lengthInCells = 2;

    [Header("Touch Feel")]
    [Tooltip("Hoeveel van een cel je moet slepen vóór de eerste gridstap (lager = responsiever).")]
    [SerializeField, Range(0.05f, 0.5f)]
    private float dragThreshold = 0.12f;

    [Header("Starting Grid Position")]
    [Tooltip("De onderste/linker gridcel die dit voertuig bezet.")]
    [SerializeField] private Vector2Int gridPosition;

    // Prefab-scale bij Awake = "1 cel" basis. Voorkomt dat we
    // lengthInCells stapelen op een al opgeschaalde prefab.
    private Vector3 baseScale = Vector3.one;

    private Vector2Int dragStartGridPosition;
    private Vector3 dragStartMouseWorld;

    private Camera mainCamera;

    // Voorkomt dubbele snap + occupancy als Setup() al heeft geïnitialiseerd.
    private bool isInitialized;

    // Lopende visual-animatie (gestopt bij een nieuwe gridstap).
    private Coroutine visualMoveCoroutine;

    // Korte 'tik' wanneer beweging geblokkeerd is.
    private Coroutine blockedShakeCoroutine;

    // Volgende moment waarop blocked-feedback mag starten.
    private float nextBlockedFeedbackTime;

    // True tijdens de exit-animatie — blokkeert verdere input/movement.
    private bool isExiting;

    private AudioManager audioManager;

    private void Awake()
    {
        // Bewaar de originele prefab-scale als 1x1-basis.
        // UpdateVisualSize gebruikt dit i.p.v. de huidige scale te vermenigvuldigen.
        baseScale = transform.localScale;

        // Optioneel: zoek child "Visual" als er niets is gekoppeld.
        if (visualTransform == null)
        {
            visualTransform = transform.Find("Visual");
        }
    }

    /// <summary>
    /// Wordt aangeroepen door LevelManager na Instantiate.
    /// Zet level-data en initialiseert precies één keer.
    /// </summary>
    public void Setup(
        GridManager newGridManager,
        GameManager newGameManager,
        VehicleOrientation newOrientation,
        int newLengthInCells,
        Vector2Int newGridPosition,
        bool newCanExitRight,
        int newExitRow)
    {
        gridManager = newGridManager;
        gameManager = newGameManager;
        orientation = newOrientation;
        lengthInCells = newLengthInCells;
        gridPosition = newGridPosition;
        canExitRight = newCanExitRight;
        exitRow = newExitRow;

        // Nieuw level / restart: win-vlag resetten zodat CompleteLevel opnieuw mag.
        if (gameManager != null)
        {
            gameManager.ResetLevelCompleted();
        }

        // Visuele grootte pas nadat orientation en length bekend zijn.
        UpdateVisualSize();
        ApplyVisualRotation();

        Initialize();
    }

    /// <summary>
    /// Draait alleen de Visual-child. Root blijft zonder rotatie (gameplay/collider).
    /// </summary>
    private void ApplyVisualRotation()
    {
        if (visualTransform == null)
        {
            return;
        }

        if (orientation == VehicleOrientation.Horizontal)
        {
            visualTransform.localRotation = Quaternion.identity;
        }
        else
        {
            visualTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }
    }

    /// <summary>
    /// Past de sprite/collider-schaal aan op orientation + lengthInCells.
    /// Prefab moet een 1x1-cel zijn (BoxCollider2D size ≈ 1x1).
    /// Scale wordt ABSOLUUT gezet vanaf baseScale — nooit opnieuw vermenigvuldigd.
    /// Daardoor schalen sprite én BoxCollider2D samen mee.
    /// </summary>
    private void UpdateVisualSize()
    {
        if (orientation == VehicleOrientation.Horizontal)
        {
            // Breedte = length, hoogte = 1 cel.
            transform.localScale = new Vector3(
                baseScale.x * lengthInCells,
                baseScale.y * 1f,
                baseScale.z * 1f
            );
        }
        else
        {
            // Breedte = 1 cel, hoogte = length.
            transform.localScale = new Vector3(
                baseScale.x * 1f,
                baseScale.y * lengthInCells,
                baseScale.z * 1f
            );
        }
    }

    private void Start()
    {
        // Alleen initialiseren als Setup() dit nog niet heeft gedaan
        // (bijv. handmatig geplaatste auto in de scene).
        if (!isInitialized)
        {
            Initialize();
        }
    }

    /// <summary>
    /// Snapt naar grid en registreert occupancy — maximaal één keer.
    /// </summary>
    private void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        mainCamera = Camera.main;
        audioManager = FindFirstObjectByType<AudioManager>();

        // Zorg dat we meteen exact op de juiste gridpositie staan.
        transform.position = GetWorldPosition(gridPosition);

        if (visualTransform != null)
        {
            visualTransform.localPosition = Vector3.zero;
        }

        // Registreer onze bezette cellen.
        gridManager.RegisterVehicle(this, GetOccupiedCells(gridPosition));

        isInitialized = true;
    }

    private void OnMouseDown()
    {
        if (isExiting)
        {
            return;
        }

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
    {
        return false;
    }

    // Alleen horizontale voertuigen.
    if (orientation != VehicleOrientation.Horizontal)
    {
        return false;
    }

    // Alleen op de rij waar de uitgang zit.
    if (gridPosition.y != exitRow)
    {
        return false;
    }

    // De auto moet al helemaal rechts staan.
    int rightMostValidX = gridManager.GridWidth - lengthInCells;

    if (gridPosition.x != rightMostValidX)
    {
        return false;
    }

    // De speler moet nog duidelijk verder naar rechts slepen.
    float requiredExtraDrag = gridManager.CellSize * 0.5f;

    return dragDifference.x > requiredExtraDrag;
}

    /// <summary>
    /// Enig toegestane exit-pad. Start exit-animatie alleen als CanExitRight true is.
    /// </summary>
    private bool TryExitRight(Vector3 dragDifference)
    {
        if (isExiting)
        {
            return true;
        }

        if (!CanExitRight(dragDifference))
        {
            return false;
        }

        Debug.Log(
            "ExitBoard: " + name +
            ", canExitRight=" + canExitRight +
            ", gridPosition=" + gridPosition +
            ", exitRow=" + exitRow
        );

        StartCoroutine(PlayExitAnimation());
        return true;
    }

    /// <summary>
    /// Rijdt soepel naar rechts uit het veld, daarna CompleteLevel + deactiveren.
    /// </summary>
    private IEnumerator PlayExitAnimation()
    {
        isExiting = true;

        // Stop movement/blocked-animaties; Visual zit vast op de root.
        StopAllVisualCoroutines();

        if (visualTransform != null)
        {
            visualTransform.localPosition = Vector3.zero;
        }

        // Vacate gridcellen meteen, zodat occupancy klopt tijdens wegrijden.
        if (gridManager != null)
        {
            gridManager.UnregisterVehicle(this);
        }

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.right * exitDistance;
        float duration = Mathf.Max(0.01f, exitAnimationDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease-in: start rustig, versnelt tijdens wegrijden.
            float eased = t * t;

            transform.position = Vector3.Lerp(startPosition, endPosition, eased);
            yield return null;
        }

        transform.position = endPosition;

        // Level pas voltooien als de auto zichtbaar is weggereden.
        if (gameManager != null)
        {
            gameManager.CompleteLevel();
        }

        // Zelfde eindresultaat als voorheen: auto van het bord.
        gameObject.SetActive(false);
    }

    private void OnMouseDrag()
    {
        if (isExiting || gridManager == null)
            return;

        Vector3 currentMouseWorld = GetMouseWorldPosition();

        Vector3 dragDifference =
            currentMouseWorld - dragStartMouseWorld;

        // Enig toegestane exit-pad: via CanExitRight → TryExitRight → ExitBoard.
        if (TryExitRight(dragDifference))
        {
            return;
        }

        Vector2Int wantedPosition = dragStartGridPosition;

        // Threshold i.p.v. RoundToInt: sneller reageren op touch,
        // maar nog steeds alleen hele gridstappen.
        int cellSteps = 0;

        if (orientation == VehicleOrientation.Horizontal)
        {
            cellSteps = GetDragCellSteps(dragDifference.x);
            wantedPosition.x += cellSteps;
        }
        else
        {
            cellSteps = GetDragCellSteps(dragDifference.y);
            wantedPosition.y += cellSteps;
        }

        // Ongeclampte wens — nodig om rand-blokkades te detecteren.
        Vector2Int unclampedWanted = wantedPosition;

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
            // Root springt direct; Visual glijdt soepel mee.
            MoveRootToGridPosition(validPosition, animateVisual: true);
            audioManager?.PlayMove();

            // NIEUWE occupancy registreren.
            gridManager.RegisterVehicle(
                this,
                GetOccupiedCells(gridPosition)
            );
        }
        else
        {
            // Root blijft op de logische gridpositie.
            transform.position = GetWorldPosition(gridPosition);

            // Speler wil verder, maar kan niet (obstakel of rand).
            if (IsBlockedMoveAttempt(unclampedWanted))
            {
                TryPlayBlockedFeedback(unclampedWanted);
            }
        }
    }

    /// <summary>
    /// True als de sleep verder wil dan de huidige gridpositie (geblokkeerde poging).
    /// </summary>
    private bool IsBlockedMoveAttempt(Vector2Int unclampedWanted)
    {
        if (orientation == VehicleOrientation.Horizontal)
        {
            return unclampedWanted.x != gridPosition.x;
        }

        return unclampedWanted.y != gridPosition.y;
    }

    /// <summary>
    /// Speelt een korte Visual-tik in de geblokkeerde richting (met cooldown).
    /// </summary>
    private void TryPlayBlockedFeedback(Vector2Int unclampedWanted)
    {
        if (visualTransform == null)
        {
            return;
        }

        // Geen spam tijdens vasthouden van de vinger.
        if (Time.time < nextBlockedFeedbackTime)
        {
            return;
        }

        // Niet storen tijdens een echte movement-animatie.
        if (visualMoveCoroutine != null)
        {
            return;
        }

        // Al een blocked-shake bezig.
        if (blockedShakeCoroutine != null)
        {
            return;
        }

        Vector3 localPeak = GetBlockedShakeLocalOffset(unclampedWanted);

        if (localPeak.sqrMagnitude < 0.0001f)
        {
            return;
        }

        nextBlockedFeedbackTime = Time.time + blockedFeedbackCooldown;
        audioManager?.PlayBlocked();
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
        blockedShakeCoroutine = StartCoroutine(AnimateBlockedShake(localPeak));
    }

    /// <summary>
    /// Local offset voor de Visual-tik, gecorrigeerd voor root-scale.
    /// </summary>
    private Vector3 GetBlockedShakeLocalOffset(Vector2Int unclampedWanted)
    {
        float dirX = 0f;
        float dirY = 0f;

        if (orientation == VehicleOrientation.Horizontal)
        {
            dirX = Mathf.Sign(unclampedWanted.x - gridPosition.x);
        }
        else
        {
            dirY = Mathf.Sign(unclampedWanted.y - gridPosition.y);
        }

        // World-afstand ≈ blockedShakeDistance, ondanks parent-scale.
        float scaleX = Mathf.Abs(transform.localScale.x);
        float scaleY = Mathf.Abs(transform.localScale.y);

        if (scaleX < 0.0001f) scaleX = 1f;
        if (scaleY < 0.0001f) scaleY = 1f;

        return new Vector3(
            dirX * blockedShakeDistance / scaleX,
            dirY * blockedShakeDistance / scaleY,
            0f
        );
    }

    /// <summary>
    /// Visual beweegt kort heen en weer; root blijft stilstaan.
    /// </summary>
    private IEnumerator AnimateBlockedShake(Vector3 localPeak)
    {
        Vector3 startLocal = visualTransform.localPosition;
        float halfDuration = Mathf.Max(0.01f, blockedShakeDuration * 0.5f);

        // Heen: huidige positie → peak.
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            visualTransform.localPosition = Vector3.Lerp(startLocal, localPeak, eased);
            yield return null;
        }

        // Terug: peak → zero.
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            visualTransform.localPosition = Vector3.Lerp(localPeak, Vector3.zero, eased);
            yield return null;
        }

        visualTransform.localPosition = Vector3.zero;
        blockedShakeCoroutine = null;
    }

    /// <summary>
    /// Zet de root direct op de gridpositie. Visual krijgt een offset en animeert naar zero.
    /// Logische stap wacht nooit op een vorige animatie.
    /// </summary>
    private void MoveRootToGridPosition(Vector2Int newGridPosition, bool animateVisual)
    {
        // Huidige world-positie van de visual (ook midden in een animatie).
        Vector3 visualWorldBefore = visualTransform != null
            ? visualTransform.position
            : transform.position;

        // Stop lopende visual-coroutines VOORDAT we opnieuw animeren
        // (geen wachtrij — altijd vanaf de actuele visual-positie).
        StopAllVisualCoroutines();

        gridPosition = newGridPosition;
        transform.position = GetWorldPosition(gridPosition);

        if (visualTransform == null)
        {
            return;
        }

        if (!animateVisual)
        {
            visualTransform.localPosition = Vector3.zero;
            return;
        }

        // Visual blijft visueel op de oude plek → localOffset t.o.v. nieuwe root.
        visualTransform.position = visualWorldBefore;
        StartVisualMoveAnimation();
    }

    /// <summary>
    /// Stopt movement- én blocked-animaties zonder Visual te teleporten.
    /// </summary>
    private void StopAllVisualCoroutines()
    {
        if (visualMoveCoroutine != null)
        {
            StopCoroutine(visualMoveCoroutine);
            visualMoveCoroutine = null;
        }

        if (blockedShakeCoroutine != null)
        {
            StopCoroutine(blockedShakeCoroutine);
            blockedShakeCoroutine = null;
        }
    }

    /// <summary>
    /// Start (of herstart) de SmoothStep-animatie van Visual naar localPosition zero.
    /// </summary>
    private void StartVisualMoveAnimation()
    {
        if (visualTransform == null)
        {
            return;
        }

        // Zekerheid: geen parallelle coroutines.
        StopAllVisualCoroutines();

        visualMoveCoroutine = StartCoroutine(AnimateVisualToZero());
    }

    /// <summary>
    /// Animeert Visual.localPosition van de huidige offset naar Vector3.zero.
    /// </summary>
    private IEnumerator AnimateVisualToZero()
    {
        Vector3 startLocal = visualTransform.localPosition;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, moveAnimationDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            visualTransform.localPosition = Vector3.Lerp(startLocal, Vector3.zero, eased);
            yield return null;
        }

        visualTransform.localPosition = Vector3.zero;
        visualMoveCoroutine = null;
    }

    /// <summary>
    /// Zet sleepafstand om naar hele gridstappen.
    /// Eerste stap na dragThreshold * cellSize; daarna elke volle cel extra.
    /// </summary>
    private int GetDragCellSteps(float axisDrag)
    {
        float distanceInCells = axisDrag / gridManager.CellSize;
        float absoluteDistance = Mathf.Abs(distanceInCells);

        if (absoluteDistance < dragThreshold)
        {
            return 0;
        }

        int steps = Mathf.FloorToInt(absoluteDistance - dragThreshold) + 1;
        return (int)Mathf.Sign(distanceInCells) * steps;
    }

    private void OnMouseUp()
    {
        if (isExiting)
        {
            return;
        }

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

    /// <summary>
    /// Read-only: kan dit voertuig minstens één geldige gridstap maken?
    /// Gebruikt bestaande occupancy-checks — verandert geen positie.
    /// </summary>
    public bool CanMakeAnyMove()
    {
        if (gridManager == null)
        {
            return false;
        }

        if (orientation == VehicleOrientation.Horizontal)
        {
            Vector2Int oneRight = ClampGridPosition(gridPosition + Vector2Int.right);
            Vector2Int oneLeft = ClampGridPosition(gridPosition + Vector2Int.left);

            if (FindFarthestValidPosition(gridPosition, oneRight) != gridPosition)
            {
                return true;
            }

            if (FindFarthestValidPosition(gridPosition, oneLeft) != gridPosition)
            {
                return true;
            }
        }
        else
        {
            Vector2Int oneUp = ClampGridPosition(gridPosition + Vector2Int.up);
            Vector2Int oneDown = ClampGridPosition(gridPosition + Vector2Int.down);

            if (FindFarthestValidPosition(gridPosition, oneUp) != gridPosition)
            {
                return true;
            }

            if (FindFarthestValidPosition(gridPosition, oneDown) != gridPosition)
            {
                return true;
            }
        }

        return false;
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
}
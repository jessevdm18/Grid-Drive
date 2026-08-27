using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Explicit Input System owner for vehicle drag.
/// Replaces legacy OnMouse* as the production path (Android + Editor).
/// </summary>
[DefaultExecutionOrder(-20)]
public class VehicleInputRouter : MonoBehaviour
{
    private enum PointerSource
    {
        None,
        Touch,
        Pointer,
        Mouse
    }

    public static VehicleInputRouter Instance { get; private set; }

    /// <summary>
    /// When true, VehicleController OnMouse* must no-op (router owns input).
    /// </summary>
    public static bool OwnsVehicleInput =>
        Instance != null && Instance.isActiveAndEnabled;

    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private GameManager gameManager;

    private VehicleController activeVehicle;
    private PointerSource activeSource = PointerSource.None;
    private bool isDragging;
    private bool loggedMeaningfulDrag;
    private Vector2 pressScreenPos;
    private Vector2 lastScreenPos;
    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>(8);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        ResolveRefs();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        CancelActiveDrag();
    }

    private void OnDisable()
    {
        CancelActiveDrag();
    }

    private void Update()
    {
        ResolveRefs();

        if (isDragging)
        {
            UpdateActiveDrag();
            return;
        }

        TryBeginDragFromPress();
    }

    private void ResolveRefs()
    {
        if (gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
    }

    private void TryBeginDragFromPress()
    {
        if (!TryGetPressBegan(out Vector2 screenPos, out PointerSource source))
        {
            return;
        }

        if (IsPointerOverUi(screenPos))
        {
            LogVehicleInput(
                "Down",
                source,
                screenPos,
                Vector3.zero,
                null,
                null,
                canAccept: false,
                note: "overUI");
            return;
        }

        if (gameManager != null && !gameManager.CanAcceptVehicleInput)
        {
            LogVehicleInput(
                "Down",
                source,
                screenPos,
                Vector3.zero,
                null,
                null,
                canAccept: false,
                note: "canAccept=false");
            return;
        }

        if (!TryScreenToWorld(screenPos, out Vector3 worldPos))
        {
            LogVehicleInput(
                "Down",
                source,
                screenPos,
                Vector3.zero,
                null,
                null,
                canAccept: false,
                note: "noCamera");
            return;
        }

        VehicleController hitVehicle = HitTestVehicle(worldPos, out Collider2D hitCollider);
        bool canAccept = hitVehicle != null && hitVehicle.CanAcceptDragInput();

        LogVehicleInput(
            "Down",
            source,
            screenPos,
            worldPos,
            hitCollider,
            hitVehicle,
            canAccept);

        if (!canAccept || hitVehicle == null)
        {
            return;
        }

        if (!hitVehicle.TryBeginDrag(worldPos))
        {
            return;
        }

        activeVehicle = hitVehicle;
        activeSource = source;
        isDragging = true;
        loggedMeaningfulDrag = false;
        pressScreenPos = screenPos;
        lastScreenPos = screenPos;
    }

    private void UpdateActiveDrag()
    {
        if (activeVehicle == null)
        {
            ClearDragState();
            return;
        }

        if (activeVehicle.isActiveAndEnabled == false ||
            !activeVehicle.IsDragSessionActive)
        {
            ClearDragState();
            return;
        }

        if (gameManager != null && !gameManager.CanAcceptVehicleInput)
        {
            activeVehicle.CancelDrag();
            LogVehicleInput(
                "Up",
                activeSource,
                pressScreenPos,
                Vector3.zero,
                null,
                activeVehicle,
                canAccept: false,
                note: "cancelledGate");
            ClearDragState();
            return;
        }

        bool stillHeld = TryGetSourceHeld(activeSource, out Vector2 screenPos);
        bool released = TryGetSourceReleased(activeSource, out Vector2 releasePos);

        if (released || !stillHeld)
        {
            Vector2 endScreen = released ? releasePos : lastScreenPos;
            Vector3 endWorld = Vector3.zero;
            if (TryScreenToWorld(endScreen, out endWorld))
            {
                activeVehicle.UpdateDrag(endWorld);
            }

            VehicleController ended = activeVehicle;
            ended.EndDrag();
            LogVehicleInput(
                "Up",
                activeSource,
                endScreen,
                endWorld,
                null,
                ended,
                canAccept: true);
            ClearDragState();
            return;
        }

        lastScreenPos = screenPos;

        if (!TryScreenToWorld(screenPos, out Vector3 worldPos))
        {
            return;
        }

        activeVehicle.UpdateDrag(worldPos);

        if (!loggedMeaningfulDrag &&
            (screenPos - pressScreenPos).sqrMagnitude > 16f)
        {
            loggedMeaningfulDrag = true;
            LogVehicleInput(
                "Drag",
                activeSource,
                screenPos,
                worldPos,
                null,
                activeVehicle,
                canAccept: true);
        }
    }

    private void CancelActiveDrag()
    {
        if (activeVehicle != null && activeVehicle.IsDragSessionActive)
        {
            activeVehicle.CancelDrag();
        }

        ClearDragState();
    }

    private void ClearDragState()
    {
        activeVehicle = null;
        activeSource = PointerSource.None;
        isDragging = false;
        loggedMeaningfulDrag = false;
    }

    /// <summary>
    /// Priority: active Touchscreen press → Pointer press → Mouse (Editor/desktop).
    /// </summary>
    private static bool TryGetPressBegan(out Vector2 screenPos, out PointerSource source)
    {
        screenPos = default;
        source = PointerSource.None;

        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            TouchControl primary = touchscreen.primaryTouch;
            if (primary.press.wasPressedThisFrame)
            {
                screenPos = primary.position.ReadValue();
                source = PointerSource.Touch;
                return true;
            }

            // Some OEMs miss wasPressedThisFrame; accept Began phase.
            if (primary.press.isPressed)
            {
                UnityEngine.InputSystem.TouchPhase phase = primary.phase.ReadValue();
                if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    screenPos = primary.position.ReadValue();
                    source = PointerSource.Touch;
                    return true;
                }
            }
        }

        Pointer pointer = Pointer.current;
        if (pointer != null &&
            pointer != Mouse.current &&
            pointer.press.wasPressedThisFrame)
        {
            screenPos = pointer.position.ReadValue();
            source = PointerSource.Pointer;
            return true;
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            // Prefer real touch when a touch is already down (never start mouse over touch).
            if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
            {
                return false;
            }

            screenPos = mouse.position.ReadValue();
            source = PointerSource.Mouse;
            return true;
        }

        return false;
    }

    private static bool TryGetSourceHeld(PointerSource source, out Vector2 screenPos)
    {
        screenPos = default;
        switch (source)
        {
            case PointerSource.Touch:
            {
                Touchscreen ts = Touchscreen.current;
                if (ts == null)
                {
                    return false;
                }

                TouchControl primary = ts.primaryTouch;
                if (!primary.press.isPressed)
                {
                    return false;
                }

                screenPos = primary.position.ReadValue();
                return true;
            }
            case PointerSource.Pointer:
            {
                Pointer pointer = Pointer.current;
                if (pointer == null || !pointer.press.isPressed)
                {
                    return false;
                }

                screenPos = pointer.position.ReadValue();
                return true;
            }
            case PointerSource.Mouse:
            {
                Mouse mouse = Mouse.current;
                if (mouse == null || !mouse.leftButton.isPressed)
                {
                    return false;
                }

                screenPos = mouse.position.ReadValue();
                return true;
            }
            default:
                return false;
        }
    }

    private static bool TryGetSourceReleased(PointerSource source, out Vector2 screenPos)
    {
        screenPos = default;
        switch (source)
        {
            case PointerSource.Touch:
            {
                Touchscreen ts = Touchscreen.current;
                if (ts == null)
                {
                    return false;
                }

                TouchControl primary = ts.primaryTouch;
                if (primary.press.wasReleasedThisFrame)
                {
                    screenPos = primary.position.ReadValue();
                    return true;
                }

                return false;
            }
            case PointerSource.Pointer:
            {
                Pointer pointer = Pointer.current;
                if (pointer != null && pointer.press.wasReleasedThisFrame)
                {
                    screenPos = pointer.position.ReadValue();
                    return true;
                }

                return false;
            }
            case PointerSource.Mouse:
            {
                Mouse mouse = Mouse.current;
                if (mouse != null && mouse.leftButton.wasReleasedThisFrame)
                {
                    screenPos = mouse.position.ReadValue();
                    return true;
                }

                return false;
            }
            default:
                return false;
        }
    }

    private bool TryScreenToWorld(Vector2 screenPos, out Vector3 worldPos)
    {
        worldPos = Vector3.zero;
        if (gameplayCamera == null)
        {
            return false;
        }

        worldPos = gameplayCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, 0f));
        worldPos.z = 0f;
        return true;
    }

    private static VehicleController HitTestVehicle(
        Vector3 worldPos,
        out Collider2D hitCollider)
    {
        hitCollider = Physics2D.OverlapPoint(worldPos);
        if (hitCollider == null)
        {
            return null;
        }

        VehicleController vehicle = hitCollider.GetComponent<VehicleController>();
        if (vehicle == null)
        {
            vehicle = hitCollider.GetComponentInParent<VehicleController>();
        }

        return vehicle;
    }

    private bool IsPointerOverUi(Vector2 screenPos)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            return false;
        }

        PointerEventData eventData = new PointerEventData(eventSystem)
        {
            position = screenPos
        };

        uiRaycastResults.Clear();
        eventSystem.RaycastAll(eventData, uiRaycastResults);
        return uiRaycastResults.Count > 0;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD || INTERNAL_DIAGNOSTICS
    private static void LogVehicleInput(
        string phase,
        PointerSource source,
        Vector2 screen,
        Vector3 world,
        Collider2D hitCollider,
        VehicleController vehicle,
        bool canAccept,
        string note = null)
    {
        string hitName = hitCollider != null ? hitCollider.name : "none";
        string vehicleName = vehicle != null ? vehicle.name : "none";
        string extra = string.IsNullOrEmpty(note) ? string.Empty : " note=" + note;

        Debug.Log(
            "[VehicleInput]\n" +
            "phase=" + phase + "\n" +
            "source=" + source + "\n" +
            "screen=" + screen.x.ToString("0.0") + "," + screen.y.ToString("0.0") + "\n" +
            "world=" + world.x.ToString("0.00") + "," + world.y.ToString("0.00") + "\n" +
            "hit=" + hitName + "\n" +
            "vehicle=" + vehicleName + "\n" +
            "canAccept=" + canAccept +
            extra);
    }
#else
    private static void LogVehicleInput(
        string phase,
        PointerSource source,
        Vector2 screen,
        Vector3 world,
        Collider2D hitCollider,
        VehicleController vehicle,
        bool canAccept,
        string note = null)
    {
    }
#endif
}

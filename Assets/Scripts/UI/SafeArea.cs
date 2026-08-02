using UnityEngine;

/// <summary>
/// Past de RectTransform aan zodat UI binnen Screen.safeArea blijft
/// (notch, statusbalk, home-indicator op Android/iOS).
/// Zet dit script op een UI-panel met RectTransform (vaak een child van Canvas).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform rectTransform;

    // Onthoud de laatste safe area om onnodige updates te vermijden.
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        ApplySafeArea();
    }

    private void Update()
    {
        // Alleen opnieuw toepassen als scherm of safe area echt veranderde.
        if (HasSafeAreaChanged())
        {
            ApplySafeArea();
        }
    }

    /// <summary>
    /// True als resolutie of safe area anders is dan de vorige keer.
    /// </summary>
    private bool HasSafeAreaChanged()
    {
        Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);

        return screenSize != lastScreenSize || Screen.safeArea != lastSafeArea;
    }

    /// <summary>
    /// Zet anchors zodat dit UI-object exact binnen de safe area valt.
    /// </summary>
    private void ApplySafeArea()
    {
        if (rectTransform == null)
        {
            return;
        }

        Rect safeArea = Screen.safeArea;

        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        // Reken safe area (pixels) om naar genormaliseerde anchors (0–1).
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}

using System;
using UnityEngine;

/// <summary>
/// Past de RectTransform aan zodat UI binnen Screen.safeArea blijft.
/// Screen.safeArea is in PIXELS; anchors zijn genormaliseerd (0–1).
/// Zet dit op een full-stretch UI-object onder de Canvas.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[DefaultExecutionOrder(-100)]
public class SafeArea : MonoBehaviour
{
    private RectTransform rectTransform;

    // Onthoud vorige waarden om onnodige updates te vermijden.
    private Rect lastSafeArea;
    private int lastScreenWidth;
    private int lastScreenHeight;

    /// <summary>Last applied Screen.safeArea in pixels (zero before first apply).</summary>
    public Rect LastAppliedSafeAreaPixels { get; private set; }

    /// <summary>Fired after anchors are written (Awake/Start/Update/ForceApply).</summary>
    public static event Action<SafeArea> OnApplied;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Apply before GameplayLayoutController (-50) Start/Awake consumers need final bounds.
        ApplySafeArea();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameplayLayoutController layout =
            GetComponent<GameplayLayoutController>() ??
            FindAnyObjectByType<GameplayLayoutController>();
        if (layout != null)
        {
            layout.LogUILayoutAudit("SafeArea.Awake");
        }
#endif
    }

    private void Start()
    {
        ApplySafeArea();
    }

    private void Update()
    {
        if (HasScreenOrSafeAreaChanged())
        {
            ApplySafeArea();
        }
    }

    /// <summary>
    /// Ensures safe-area anchors are current (layout systems may call before Start).
    /// </summary>
    public void ForceApply()
    {
        ApplySafeArea();
    }

    /// <summary>
    /// True als breedte, hoogte of safe area is veranderd.
    /// </summary>
    private bool HasScreenOrSafeAreaChanged()
    {
        return Screen.width != lastScreenWidth
            || Screen.height != lastScreenHeight
            || Screen.safeArea != lastSafeArea;
    }

    /// <summary>
    /// Zet anchors zodat dit object exact binnen de safe area valt.
    /// </summary>
    private void ApplySafeArea()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (rectTransform == null)
        {
            return;
        }

        // Nooit delen door 0 (kan kort voorkomen bij scene-start).
        if (Screen.width <= 0 || Screen.height <= 0)
        {
            Debug.LogWarning("SafeArea: Screen.width/height is 0 — skip.");
            return;
        }

        Rect safeArea = Screen.safeArea;

        // Pixels → genormaliseerde anchors (0–1).
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Dev-only: confirm safe-area insets on device.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "SafeArea apply\n" +
            "Screen.width = " + Screen.width + "\n" +
            "Screen.height = " + Screen.height + "\n" +
            "Screen.safeArea = " + safeArea + "\n" +
            "anchorMin = " + anchorMin + "\n" +
            "anchorMax = " + anchorMax
        );
#endif

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        lastSafeArea = safeArea;
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        LastAppliedSafeAreaPixels = safeArea;

        OnApplied?.Invoke(this);
    }
}

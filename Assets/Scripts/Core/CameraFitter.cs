using UnityEngine;

/// <summary>
/// Past de Orthographic Size van de camera aan zodat een vaste world-breedte
/// altijd in beeld blijft (belangrijk voor portrait Android-schermen).
/// </summary>
public class CameraFitter : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("De orthographic camera die aangepast wordt (meestal Main Camera).")]
    [SerializeField] private Camera targetCamera;

    [Header("Fit-instellingen")]
    [Tooltip("Hoeveel world-units horizontaal altijd zichtbaar moeten zijn.")]
    [SerializeField] private float targetWorldWidth = 8f;

    [Tooltip("Kleine schermen mogen niet smaller kijken dan deze size.")]
    [SerializeField] private float minOrthographicSize = 5f;

    // Onthoud de laatste aspectratio om onnodige updates te vermijden.
    private float lastAspect = -1f;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        FitCamera();
    }

    private void Update()
    {
        // Op sommige phones verandert de resolutie (cutouts, rotatie, safe area).
        if (targetCamera == null)
        {
            return;
        }

        float aspect = (float)Screen.width / Screen.height;

        if (!Mathf.Approximately(aspect, lastAspect))
        {
            FitCamera();
        }
    }

    /// <summary>
    /// Berekent Orthographic Size zodat targetWorldWidth volledig zichtbaar is.
    /// </summary>
    private void FitCamera()
    {
        if (targetCamera == null || !targetCamera.orthographic)
        {
            Debug.LogWarning("CameraFitter: targetCamera ontbreekt of is niet orthographic.");
            return;
        }

        float aspect = (float)Screen.width / Screen.height;
        lastAspect = aspect;

        // Zichtbare world-hoogte = 2 * orthographicSize
        // Zichtbare world-breedte = aspect * 2 * orthographicSize
        // We willen: aspect * 2 * size = targetWorldWidth
        // Dus: size = targetWorldWidth / (2 * aspect)
        float sizeForWidth = targetWorldWidth / (2f * aspect);

        // Nooit kleiner dan het minimum (voorkomt te strakke zoom op brede schermen).
        targetCamera.orthographicSize = Mathf.Max(sizeForWidth, minOrthographicSize);
    }
}

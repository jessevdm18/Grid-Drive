using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Subtiele press/pop-schaalanimatie voor UI Buttons.
/// Schaalt bij voorkeur child "Visual"; anders (bewust) de Button-root.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonPressAnimation : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Tooltip("Child die visueel schaalt (bijv. Visual). Leeg = Find(\"Visual\") of root.")]
    [SerializeField] private Transform visualTransform;

    [SerializeField] private float pressedScale = 0.94f;
    [SerializeField] private float pressDuration = 0.06f;
    [SerializeField] private float releaseDuration = 0.10f;
    [SerializeField] private float releaseOvershoot = 1.04f;

    private Button button;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    private void Awake()
    {
        button = GetComponent<Button>();
        ResolveVisualTransform();
        originalScale = visualTransform.localScale;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Helpt Inspector/prefab setup: koppel Visual-child als die bestaat.
        if (visualTransform == null)
        {
            Transform visualChild = transform.Find("Visual");
            if (visualChild != null)
            {
                visualTransform = visualChild;
            }
        }
    }
#endif

    /// <summary>
    /// 1) serialized ref
    /// 2) child genaamd "Visual"
    /// 3) root (hitbox schaalt mee — ondersteunde fallback, geen warning spam)
    /// </summary>
    private void ResolveVisualTransform()
    {
        if (visualTransform != null)
        {
            return;
        }

        Transform visualChild = transform.Find("Visual");
        if (visualChild != null)
        {
            visualTransform = visualChild;
            return;
        }

        visualTransform = transform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button == null || !button.interactable)
        {
            return;
        }

        StopScaleAnimation();
        scaleCoroutine = StartCoroutine(
            AnimateScale(visualTransform.localScale, originalScale * pressedScale, pressDuration)
        );
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button == null || !button.interactable)
        {
            return;
        }

        StopScaleAnimation();
        scaleCoroutine = StartCoroutine(AnimateReleasePop());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Vinger/muis verlaat knop tijdens press → soepel terug naar normaal.
        StopScaleAnimation();
        scaleCoroutine = StartCoroutine(
            AnimateScale(visualTransform.localScale, originalScale, releaseDuration)
        );
    }

    private void OnDisable()
    {
        StopScaleAnimation();
        if (visualTransform != null)
        {
            visualTransform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Release: overshoot → originalScale.
    /// </summary>
    private IEnumerator AnimateReleasePop()
    {
        Vector3 overshoot = originalScale * releaseOvershoot;
        float half = Mathf.Max(0.01f, releaseDuration) * 0.5f;

        yield return AnimateScale(visualTransform.localScale, overshoot, half);
        yield return AnimateScale(visualTransform.localScale, originalScale, half);

        scaleCoroutine = null;
    }

    private IEnumerator AnimateScale(Vector3 from, Vector3 to, float duration)
    {
        duration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            visualTransform.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }

        visualTransform.localScale = to;
    }

    private void StopScaleAnimation()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }
    }
}

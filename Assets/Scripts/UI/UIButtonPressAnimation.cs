using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Subtiele press/pop-schaalanimatie voor UI Buttons.
/// Schaalt bij voorkeur child "Visual"; anders een child — nooit de Button-root
/// (root-schaal krimpt de hitbox en annuleert PointerClick intermittent).
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonPressAnimation : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Tooltip("Child die visueel schaalt (bijv. Visual). Leeg = Find(\"Visual\") of eerste child.")]
    [SerializeField] private Transform visualTransform;

    [SerializeField] private float pressedScale = 0.94f;
    [SerializeField] private float pressDuration = 0.06f;
    [SerializeField] private float releaseDuration = 0.10f;
    [SerializeField] private float releaseOvershoot = 1.04f;

    private Button button;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private bool canAnimate;

    private void Awake()
    {
        button = GetComponent<Button>();
        ResolveVisualTransform();
        canAnimate = visualTransform != null && visualTransform != transform;
        if (canAnimate)
        {
            originalScale = visualTransform.localScale;
        }
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
    /// 1) serialized ref (if not the Button root)
    /// 2) child genaamd "Visual"
    /// 3) first child (label/icon)
    /// Never falls back to the Button root — scaling the hitbox cancels clicks.
    /// </summary>
    private void ResolveVisualTransform()
    {
        if (visualTransform != null && visualTransform != transform)
        {
            return;
        }

        // Serialized root is unsafe for press scale — ignore and resolve a child.
        visualTransform = null;

        Transform visualChild = transform.Find("Visual");
        if (visualChild != null)
        {
            visualTransform = visualChild;
            return;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child != null)
            {
                visualTransform = child;
                return;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canAnimate || button == null || !button.interactable)
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
        if (!canAnimate || button == null || !button.interactable)
        {
            return;
        }

        StopScaleAnimation();
        scaleCoroutine = StartCoroutine(AnimateReleasePop());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!canAnimate)
        {
            return;
        }

        // Vinger/muis verlaat knop tijdens press → soepel terug naar normaal.
        StopScaleAnimation();
        scaleCoroutine = StartCoroutine(
            AnimateScale(visualTransform.localScale, originalScale, releaseDuration)
        );
    }

    private void OnDisable()
    {
        StopScaleAnimation();
        if (canAnimate && visualTransform != null)
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

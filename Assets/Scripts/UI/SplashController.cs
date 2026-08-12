using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Eenvoudige branded splash: fade/scale in → hold → SceneTransition naar MainMenu.
/// Eigen fade-out is verwijderd om dubbele fade met SceneTransition te voorkomen.
/// </summary>
public class SplashController : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform logoTransform;
    [SerializeField] private Image logoImage;

    [Header("Timing (seconds, unscaled)")]
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float holdDuration = 0.80f;

    [Header("Logo Scale")]
    [SerializeField] private float startScale = 0.90f;
    [SerializeField] private float endScale = 1.00f;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = MainMenuSceneName;

    private bool hasStartedLoad;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (logoTransform == null && logoImage != null)
        {
            logoTransform = logoImage.rectTransform;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (logoTransform != null)
        {
            logoTransform.localScale = Vector3.one * startScale;
        }
    }

    private void Start()
    {
        StartCoroutine(PlaySplashSequence());
    }

    private IEnumerator PlaySplashSequence()
    {
        // Fade + scale in
        yield return Animate(
            fadeInDuration,
            t =>
            {
                float eased = SmoothStep(t);
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = eased;
                }

                if (logoTransform != null)
                {
                    float scale = Mathf.Lerp(startScale, endScale, eased);
                    logoTransform.localScale = Vector3.one * scale;
                }
            }
        );

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        if (logoTransform != null)
        {
            logoTransform.localScale = Vector3.one * endScale;
        }

        // Hold
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, holdDuration));

        // SceneTransition doet fade naar donker → MainMenu → fade in.
        LoadMainMenuOnce();
    }

    private IEnumerator Animate(float duration, System.Action<float> onProgress)
    {
        float safeDuration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;

        while (elapsed < safeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            onProgress?.Invoke(t);
            yield return null;
        }

        onProgress?.Invoke(1f);
    }

    private static float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private void LoadMainMenuOnce()
    {
        if (hasStartedLoad)
        {
            return;
        }

        hasStartedLoad = true;

        string sceneName = string.IsNullOrEmpty(nextSceneName)
            ? MainMenuSceneName
            : nextSceneName;

        SceneTransition.LoadScene(sceneName);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Branded splash: fade/scale in → hold → UMP resolve → Analytics consent → SceneTransition.
/// First-launch: Gameplay Level 1. Returning: MainMenu.
/// </summary>
public class SplashController : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";
    private const string GameplaySceneName = "Gameplay";
    private const float PrivacyResolveTimeoutSeconds = 20f;

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
    [Tooltip("Returning players. First-launch gaat naar Gameplay (negeert dit veld).")]
    [SerializeField] private string nextSceneName = MainMenuSceneName;

    [Tooltip("MainLevelDatabase — required for first-launch Easy Level 1 DB index.")]
    [SerializeField] private LevelDatabase levelDatabase;

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

        // Hold (UMP may finish in parallel during fade/hold).
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, holdDuration));

        // Startup order: UMP resolve/fail-soft → Analytics consent (if Unknown) → route.
        yield return WaitForPrivacyConsentResolved();
        yield return WaitForAnalyticsConsentGate();

        // First-launch: Splash → Gameplay (geen MainMenu-flash).
        // Returning: Splash → MainMenu.
        LoadNextSceneOnce();
    }

    private IEnumerator WaitForPrivacyConsentResolved()
    {
        PrivacyConsentManager.EnsureInstance();

        float elapsed = 0f;
        while (!PrivacyConsentManager.IsResolved &&
               elapsed < PrivacyResolveTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!PrivacyConsentManager.IsResolved)
        {
            Debug.LogWarning(
                "[Privacy] UMP resolve wait timed out — continuing Splash routing."
            );
        }
    }

    private IEnumerator WaitForAnalyticsConsentGate()
    {
        bool done = false;
        AnalyticsConsentPopup.RunGate(() => done = true);

        while (!done)
        {
            yield return null;
        }
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

    private void LoadNextSceneOnce()
    {
        if (hasStartedLoad)
        {
            return;
        }

        hasStartedLoad = true;

        // Index-based saves + DB reorder: wipe stale progression before routing.
        LevelDatabaseContentVersion.ApplyIfNeeded();

        if (SaveManager.ShouldRouteFirstLaunchToGameplay())
        {
            // Same contract as LevelSelect Easy button 1 (ordered Easy[0]).
            SaveManager.PrepareFirstLaunchGameplayLevelStatic(levelDatabase);
            SceneTransition.LoadScene(GameplaySceneName);
            return;
        }

        string sceneName = string.IsNullOrEmpty(nextSceneName)
            ? MainMenuSceneName
            : nextSceneName;

        SceneTransition.LoadScene(sceneName);
    }
}

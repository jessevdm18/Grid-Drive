using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Lichte scene-overgang: fade naar donker → LoadScene → fade in.
/// Maximaal één instance (DontDestroyOnLoad). Werkt met Time.timeScale = 0.
/// </summary>
public class SceneTransition : MonoBehaviour
{
    private const int OverlaySortingOrder = 32000;

    [SerializeField] private float fadeOutDuration = 0.30f;
    [SerializeField] private float fadeInDuration = 0.30f;
    [SerializeField] private Color overlayColor = new Color(0.07f, 0.11f, 0.18f, 1f);

    private static SceneTransition instance;
    private static bool isCreating;

    private CanvasGroup canvasGroup;
    private Image fadeImage;
    private bool isTransitioning;
    /// <summary>
    /// True after the destination scene has loaded and fade-in has started.
    /// A new LoadScene may interrupt so the first post-arrival click is not lost.
    /// </summary>
    private bool allowInterruptForNewRequest;
    private Coroutine runningTransition;

    /// <summary>
    /// True tijdens fade-out / load (and briefly fade-in until interrupt is armed).
    /// </summary>
    public static bool IsTransitioning =>
        instance != null && instance.isTransitioning;

    /// <summary>
    /// Start een fade-out → scene load → fade-in.
    /// Extra calls tijdens fade-out/load worden genegeerd.
    /// Calls tijdens fade-in underbreken de fade-in en starten een nieuwe transitie
    /// (zodat de eerste geldige klik na aankomst niet verloren gaat).
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneTransition: sceneName is leeg.");
            return;
        }

        if (!TryAuthorizeGameplaySceneEntry(sceneName))
        {
            LogNav("LoadScene rejected — gameplay entry not authorized. scene=" + sceneName);
            return;
        }

        SceneTransition transition = EnsureInstance();
        transition.BeginTransition(sceneName, fadeOutFirst: true);
    }

    /// <summary>
    /// Zero-lives gate before entering Gameplay. V1 pending/active playtest bypasses.
    /// Other scenes are never gated.
    /// </summary>
    private static bool TryAuthorizeGameplaySceneEntry(string sceneName)
    {
        if (!string.Equals(sceneName, "Gameplay", System.StringComparison.Ordinal))
        {
            return true;
        }

        // Explicit Editor V1 candidate session — do not block entry.
        if (V1PlaytestOverride.HasPending || V1PlaytestOverride.HasActive)
        {
            LivesManager bypassLog = LivesManager.EnsureInstance();
            bypassLog?.TryBeginLevelAttempt("scene_gameplay", bypassLivesGate: true);
            return true;
        }

        if (DailyChallengeContext.HasPending || DailyChallengeContext.IsActiveSession)
        {
            LivesManager bypassLog = LivesManager.EnsureInstance();
            bypassLog?.TryBeginLevelAttempt("scene_gameplay_daily", bypassLivesGate: true);
            return true;
        }

        LivesManager livesManager = LivesManager.EnsureInstance();
        if (livesManager == null)
        {
            Debug.LogWarning(
                "[Lives] SceneTransition: LivesManager missing — allowing Gameplay."
            );
            return true;
        }

        return livesManager.TryBeginLevelAttempt("scene_gameplay");
    }

    /// <summary>
    /// Laadt een scene met alleen fade-in (overlay start al dicht).
    /// Handig als de vorige scene zelf al naar donker is gefaded.
    /// </summary>
    public static void LoadSceneFadeInOnly(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneTransition: sceneName is leeg.");
            return;
        }

        if (!TryAuthorizeGameplaySceneEntry(sceneName))
        {
            return;
        }

        SceneTransition transition = EnsureInstance();
        transition.BeginTransition(sceneName, fadeOutFirst: false);
    }

    private static SceneTransition EnsureInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        SceneTransition existing = FindAnyObjectByType<SceneTransition>();
        if (existing != null)
        {
            instance = existing;
            return instance;
        }

        isCreating = true;
        GameObject root = new GameObject("SceneTransition");
        DontDestroyOnLoad(root);
        instance = root.AddComponent<SceneTransition>();
        instance.BuildOverlay();
        isCreating = false;
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (canvasGroup == null && !isCreating)
        {
            BuildOverlay();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        // Never leave a transparent full-screen raycast blocker after teardown.
        ClearRaycastBlock();
        isTransitioning = false;
        allowInterruptForNewRequest = false;
        runningTransition = null;
    }

    private void OnDisable()
    {
        // Always clear blockers if this object is disabled. A transparent overlay
        // that still blocksRaycasts eats MainMenu taps after LevelSelect→MainMenu.
        ClearRaycastBlock();
        if (isTransitioning)
        {
            LogNav("OnDisable while transitioning — forcing idle reset");
            isTransitioning = false;
            allowInterruptForNewRequest = false;
            runningTransition = null;
        }
    }

    private void BuildOverlay()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = OverlaySortingOrder;

        if (gameObject.GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1820f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (gameObject.GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Transform existingOverlay = transform.Find("FadeOverlay");
        GameObject overlayObject;
        if (existingOverlay != null)
        {
            overlayObject = existingOverlay.gameObject;
        }
        else
        {
            overlayObject = new GameObject(
                "FadeOverlay",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            overlayObject.transform.SetParent(transform, false);
        }

        RectTransform rect = overlayObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        fadeImage = overlayObject.GetComponent<Image>();
        fadeImage.color = overlayColor;
        // Idle: must not eat failure-UI / MainMenu taps under a transparent overlay.
        fadeImage.raycastTarget = false;
    }

    private void BeginTransition(string sceneName, bool fadeOutFirst)
    {
        if (isTransitioning)
        {
            if (!allowInterruptForNewRequest)
            {
                LogNav(
                    "LoadScene rejected — already transitioning (fade-out/load). scene=" +
                    sceneName + " frame=" + Time.frameCount);
                return;
            }

            LogNav(
                "LoadScene interrupt — replacing fade-in with new transition. scene=" +
                sceneName + " frame=" + Time.frameCount);
            StopActiveTransitionCoroutine();
            // Leave overlay as-is; new routine will fade out from current alpha.
            isTransitioning = false;
            allowInterruptForNewRequest = false;
        }

        if (canvasGroup == null)
        {
            BuildOverlay();
        }

        LogNav("LoadScene accepted scene=" + sceneName + " fadeOutFirst=" + fadeOutFirst);
        runningTransition = StartCoroutine(TransitionRoutine(sceneName, fadeOutFirst));
    }

    private void StopActiveTransitionCoroutine()
    {
        if (runningTransition != null)
        {
            StopCoroutine(runningTransition);
            runningTransition = null;
        }
    }

    private IEnumerator TransitionRoutine(string sceneName, bool fadeOutFirst)
    {
        isTransitioning = true;
        allowInterruptForNewRequest = false;
        SetRaycastBlock(true);
        LogNav("Fade start (out) scene=" + sceneName);

        if (fadeOutFirst)
        {
            float fromAlpha = canvasGroup != null ? canvasGroup.alpha : 0f;
            yield return Fade(fromAlpha, 1f, fadeOutDuration);
        }
        else
        {
            canvasGroup.alpha = 1f;
        }

        // Overlay blijft dicht tijdens de load (freeze zit achter zwart).
        canvasGroup.alpha = 1f;
        SetRaycastBlock(true);

        LogNav("Scene load begin scene=" + sceneName);
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        if (load == null)
        {
            Debug.LogError("SceneTransition: kon scene niet laden: " + sceneName);
            ForceIdle();
            runningTransition = null;
            yield break;
        }

        load.allowSceneActivation = true;
        while (!load.isDone)
        {
            yield return null;
        }

        // Eén frame wachten zodat de nieuwe scene klaar is onder de overlay.
        yield return null;
        LogNav("Scene load done scene=" + sceneName);

        // Destination scene is live. Stop eating clicks so the first MainMenu /
        // LevelSelect tap is not lost on the transparent FadeOverlay, and allow
        // that tap to interrupt this fade-in with a new outbound transition.
        ClearRaycastBlock();
        allowInterruptForNewRequest = true;
        LogNav("Fade-in start — raycasts cleared, interrupt armed");

        yield return Fade(1f, 0f, fadeInDuration);

        canvasGroup.alpha = 0f;
        ForceIdle();
        runningTransition = null;
        LogNav("Fade complete — transition idle");
    }

    private void ForceIdle()
    {
        ClearRaycastBlock();
        isTransitioning = false;
        allowInterruptForNewRequest = false;
    }

    private void SetRaycastBlock(bool block)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = block;
            canvasGroup.interactable = false;
        }

        if (fadeImage == null)
        {
            Transform overlay = transform.Find("FadeOverlay");
            if (overlay != null)
            {
                fadeImage = overlay.GetComponent<Image>();
            }
        }

        if (fadeImage != null)
        {
            fadeImage.raycastTarget = block;
        }
    }

    private void ClearRaycastBlock()
    {
        SetRaycastBlock(false);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float safeDuration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < safeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            float eased = t * t * (3f - 2f * t);
            canvasGroup.alpha = Mathf.Lerp(from, to, eased);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private static void LogNav(string message)
    {
        Debug.Log("[SceneTransition] " + message + " frame=" + Time.frameCount);
    }
}

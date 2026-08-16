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
    private bool isTransitioning;

    /// <summary>
    /// True tijdens fade-out / load / fade-in.
    /// </summary>
    public static bool IsTransitioning =>
        instance != null && instance.isTransitioning;

    /// <summary>
    /// Start een fade-out → scene load → fade-in.
    /// Extra calls tijdens een actieve transition worden genegeerd.
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneTransition: sceneName is leeg.");
            return;
        }

        SceneTransition transition = EnsureInstance();
        transition.BeginTransition(sceneName, fadeOutFirst: true);
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
            overlayObject = new GameObject("FadeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlayObject.transform.SetParent(transform, false);
        }

        RectTransform rect = overlayObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        Image image = overlayObject.GetComponent<Image>();
        image.color = overlayColor;
        image.raycastTarget = true;
    }

    private void BeginTransition(string sceneName, bool fadeOutFirst)
    {
        if (isTransitioning)
        {
            return;
        }

        if (canvasGroup == null)
        {
            BuildOverlay();
        }

        StartCoroutine(TransitionRoutine(sceneName, fadeOutFirst));
    }

    private IEnumerator TransitionRoutine(string sceneName, bool fadeOutFirst)
    {
        isTransitioning = true;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;

        if (fadeOutFirst)
        {
            yield return Fade(0f, 1f, fadeOutDuration);
        }
        else
        {
            canvasGroup.alpha = 1f;
        }

        // Overlay blijft dicht tijdens de load (freeze zit achter zwart).
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        if (load == null)
        {
            Debug.LogError("SceneTransition: kon scene niet laden: " + sceneName);
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            isTransitioning = false;
            yield break;
        }

        load.allowSceneActivation = true;
        while (!load.isDone)
        {
            yield return null;
        }

        // Eén frame wachten zodat de nieuwe scene klaar is onder de overlay.
        yield return null;

        yield return Fade(1f, 0f, fadeInDuration);

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        isTransitioning = false;
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
}

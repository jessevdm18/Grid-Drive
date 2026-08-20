using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Player choice when Medium/Hard unlocks for the first time.
/// Stay → continue WinSequence in current difficulty.
/// Try → after reward presentations, load first ordered level of unlocked tier.
/// Presenter stays always-active; <see cref="root"/> is the visual (may start inactive).
/// </summary>
public enum DifficultyUnlockChoice
{
    StayCurrentDifficulty = 0,
    TryUnlockedDifficulty = 1
}

public class DifficultyUnlockNotificationUI : MonoBehaviour
{
    [Header("Root")]
    [Tooltip("Visual popup root (may be inactive when hidden). Prefer a CHILD of this component.")]
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRect;
    [Tooltip("Fullscreen raycast blocker (often sibling of Panel). Must be hidden with the panel.")]
    [SerializeField] private GameObject inputBlocker;

    [Header("Text References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private bool showSubtitle = true;

    [Header("Choice Buttons")]
    [SerializeField] private Button stayButton;
    [SerializeField] private TMP_Text stayButtonText;
    [SerializeField] private Button tryButton;
    [SerializeField] private TMP_Text tryButtonText;

    [Header("Medium Copy")]
    [SerializeField] private string mediumTitle = "MEDIUM UNLOCKED!";
    [SerializeField] private string mediumSubtitle = "NEW CHALLENGES AVAILABLE";
    [SerializeField] private string mediumStayLabel = "KEEP PLAYING EASY";
    [SerializeField] private string mediumTryLabel = "TRY MEDIUM";

    [Header("Hard Copy")]
    [SerializeField] private string hardTitle = "HARD UNLOCKED!";
    [SerializeField] private string hardSubtitle =
        "THE TOUGHEST LEVELS ARE NOW AVAILABLE";
    [SerializeField] private string hardStayLabel = "KEEP PLAYING MEDIUM";
    [SerializeField] private string hardTryLabel = "TRY HARD";

    [Header("Timing (unscaled)")]
    [SerializeField] private float appearDuration = 0.2f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float popScale = 1.08f;

    private Coroutine playCoroutine;
    private bool isPlaying;
    private bool choiceMade;
    private DifficultyUnlockChoice pendingChoice;

    public bool IsPlaying => isPlaying;

    /// <summary>Result of the last completed ShowChoiceAndWait.</summary>
    public DifficultyUnlockChoice LastChoice { get; private set; }

    private void Awake()
    {
        TryAutoFindInputBlocker();
        WireButtons();
        EnsureFullyHidden();
    }

    private void OnEnable()
    {
        WireButtons();
    }

    private void OnDisable()
    {
        if (stayButton != null)
        {
            stayButton.onClick.RemoveListener(OnStayClicked);
        }

        if (tryButton != null)
        {
            tryButton.onClick.RemoveListener(OnTryClicked);
        }

        EnsureFullyHidden();
    }

    private void TryAutoFindInputBlocker()
    {
        if (inputBlocker != null)
        {
            return;
        }

        Transform searchRoot = transform;
        if (root != null && root.transform.parent != null)
        {
            searchRoot = root.transform.parent;
        }

        Transform found = searchRoot.Find("InputBlocker");
        if (found == null)
        {
            found = transform.Find("InputBlocker");
        }

        if (found != null)
        {
            inputBlocker = found.gameObject;
            return;
        }

        // Fallback: fullscreen Image sibling of Panel named differently.
        if (root == null || root.transform.parent == null)
        {
            return;
        }

        Transform parent = root.transform.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child == root.transform)
            {
                continue;
            }

            if (child.GetComponent<Image>() != null &&
                child.name.IndexOf("Block", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                inputBlocker = child.gameObject;
                return;
            }
        }
    }

    private void WireButtons()
    {
        if (stayButton != null)
        {
            stayButton.onClick.RemoveListener(OnStayClicked);
            stayButton.onClick.AddListener(OnStayClicked);
        }

        if (tryButton != null)
        {
            tryButton.onClick.RemoveListener(OnTryClicked);
            tryButton.onClick.AddListener(OnTryClicked);
        }
    }

    /// <summary>
    /// Appear → wait for Stay/Try → fade. Easy / unknown → no-op (Stay).
    /// </summary>
    public IEnumerator ShowChoiceAndWait(LevelDifficulty unlockedDifficulty)
    {
        LastChoice = DifficultyUnlockChoice.StayCurrentDifficulty;

        if (unlockedDifficulty != LevelDifficulty.Medium &&
            unlockedDifficulty != LevelDifficulty.Hard)
        {
            yield break;
        }

        EnsurePresenterCanRunCoroutines();
        ActivateVisualRoot();
        ApplyCopy(unlockedDifficulty);

        choiceMade = false;
        pendingChoice = DifficultyUnlockChoice.StayCurrentDifficulty;

        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        playCoroutine = StartCoroutine(ChoiceRoutine());
        yield return playCoroutine;
        playCoroutine = null;

        LastChoice = pendingChoice;
    }

    /// <summary>Legacy alias — waits for choice (no auto-hide).</summary>
    public IEnumerator Show(LevelDifficulty difficulty)
    {
        yield return ShowChoiceAndWait(difficulty);
    }

    public IEnumerator Play(LevelDifficulty difficulty)
    {
        yield return ShowChoiceAndWait(difficulty);
    }

    /// <summary>Editor: interactive choice panel (does not MarkSeen).</summary>
    public void PlayNow(LevelDifficulty difficulty)
    {
        EnsurePresenterCanRunCoroutines();
        StartCoroutine(EditorPlayChoice(difficulty));
    }

    private IEnumerator EditorPlayChoice(LevelDifficulty difficulty)
    {
        yield return ShowChoiceAndWait(difficulty);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[DifficultyUnlock]\n" +
            "Unlocked=" + difficulty + "\n" +
            "Choice=" +
            (LastChoice == DifficultyUnlockChoice.TryUnlockedDifficulty ? "Try" : "Stay") +
            "\n" +
            "(Editor preview — prefs not marked)"
        );
#endif
    }

    private void OnStayClicked()
    {
        if (!isPlaying || choiceMade)
        {
            return;
        }

        pendingChoice = DifficultyUnlockChoice.StayCurrentDifficulty;
        choiceMade = true;
    }

    private void OnTryClicked()
    {
        if (!isPlaying || choiceMade)
        {
            return;
        }

        pendingChoice = DifficultyUnlockChoice.TryUnlockedDifficulty;
        choiceMade = true;
    }

    private void EnsurePresenterCanRunCoroutines()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError(
                "DifficultyUnlockNotificationUI: presenter GameObject is inactive in hierarchy. " +
                "Move this component to an always-active parent and assign the popup Panel as Root."
            );
        }
    }

    private void ActivateVisualRoot()
    {
        if (inputBlocker != null)
        {
            inputBlocker.SetActive(true);
        }

        if (root != null)
        {
            root.SetActive(true);
        }
    }

    private IEnumerator ChoiceRoutine()
    {
        isPlaying = true;
        ActivateVisualRoot();
        SetChoiceButtonsInteractable(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            // Block clicks under the panel; interactable=true so Stay/Try receive clicks.
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        if (panelRect != null)
        {
            panelRect.localScale = Vector3.one * 0.92f;
        }

        float appear = Mathf.Max(0.01f, appearDuration);
        float t = 0f;
        while (t < appear)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / appear);
            float eased = u * u * (3f - 2f * u);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = eased;
            }

            if (panelRect != null)
            {
                float scale = Mathf.Lerp(0.92f, popScale, eased);
                panelRect.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        if (panelRect != null)
        {
            float settle = 0.12f;
            float s = 0f;
            while (s < settle)
            {
                s += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(s / settle);
                panelRect.localScale = Vector3.one * Mathf.Lerp(popScale, 1f, u);
                yield return null;
            }

            panelRect.localScale = Vector3.one;
        }

        // Wait until player picks Stay or Try.
        while (!choiceMade)
        {
            yield return null;
        }

        SetChoiceButtonsInteractable(false);

        float fade = Mathf.Max(0.01f, fadeDuration);
        t = 0f;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / fade);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - u;
            }

            yield return null;
        }

        HideImmediate();
        isPlaying = false;
    }

    private void SetChoiceButtonsInteractable(bool enabled)
    {
        if (stayButton != null)
        {
            stayButton.interactable = enabled;
        }

        if (tryButton != null)
        {
            tryButton.interactable = enabled;
        }
    }

    private void ApplyCopy(LevelDifficulty difficulty)
    {
        string title;
        string subtitle;
        string stayLabel;
        string tryLabel;

        if (difficulty == LevelDifficulty.Hard)
        {
            title = hardTitle;
            subtitle = hardSubtitle;
            stayLabel = hardStayLabel;
            tryLabel = hardTryLabel;
        }
        else
        {
            title = mediumTitle;
            subtitle = mediumSubtitle;
            stayLabel = mediumStayLabel;
            tryLabel = mediumTryLabel;
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (subtitleText != null)
        {
            subtitleText.gameObject.SetActive(showSubtitle);
            if (showSubtitle)
            {
                subtitleText.text = subtitle;
            }
        }

        if (stayButtonText != null)
        {
            stayButtonText.text = stayLabel;
        }

        if (tryButtonText != null)
        {
            tryButtonText.text = tryLabel;
        }
    }

    /// <summary>
    /// Alias used by UIManager — always clears raycast blockers even when not shown.
    /// </summary>
    public void HideImmediate()
    {
        EnsureFullyHidden();
    }

    /// <summary>
    /// Force-hide visual + raycast state. Safe to call every WinSequence start
    /// so a previous unlock never leaves a stale InputBlocker over Next.
    /// </summary>
    public void EnsureFullyHidden()
    {
        isPlaying = false;
        choiceMade = true;

        if (playCoroutine != null && gameObject.activeInHierarchy)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        SetChoiceButtonsInteractable(false);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (panelRect != null)
        {
            panelRect.localScale = Vector3.one;
        }

        // InputBlocker is often a sibling of Panel — hide explicitly.
        if (inputBlocker != null)
        {
            inputBlocker.SetActive(false);
        }

        if (root != null && root != gameObject)
        {
            root.SetActive(false);
        }
        else if (root == gameObject)
        {
            Debug.LogWarning(
                "DifficultyUnlockNotificationUI: Root is the same GameObject as the presenter. " +
                "Keep the component on an always-active parent and assign a child Panel as Root."
            );
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public bool IsBlockerRaycastActive()
    {
        if (inputBlocker != null &&
            inputBlocker.activeInHierarchy)
        {
            return true;
        }

        if (canvasGroup != null &&
            canvasGroup.blocksRaycasts &&
            canvasGroup.alpha > 0.01f)
        {
            return true;
        }

        if (canvasGroup != null && canvasGroup.blocksRaycasts)
        {
            // blocksRaycasts true with alpha 0 still eats clicks — treat as active leak.
            return true;
        }

        return false;
    }

    public bool IsVisualRootActive()
    {
        if (inputBlocker != null && inputBlocker.activeInHierarchy)
        {
            return true;
        }

        return root != null && root.activeInHierarchy;
    }
#endif
}

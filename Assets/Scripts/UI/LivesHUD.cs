using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentation-only lives indicator. Reads LivesManager; never owns regen math.
/// Visual hierarchy is authored in-scene — this script only drives text/visibility.
/// </summary>
public class LivesHUD : MonoBehaviour
{
    private const float TickSeconds = 1f;

    [Header("Authored refs (required)")]
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject countdownRoot;
    [SerializeField] private Image livesIcon;

    [Header("Edit Mode preview (replaced at runtime)")]
    [SerializeField] private string previewLivesText = "5 / 5";
    [SerializeField] private string previewCountdownText = "1:42:18";

    private LivesManager livesManager;
    private Coroutine tickRoutine;
    private bool subscribed;

    /// <summary>
    /// Finds an authored LivesHUD in the loaded scenes. Does not create visuals.
    /// </summary>
    public static LivesHUD FindInLoadedScenes()
    {
        return FindAnyObjectByType<LivesHUD>();
    }

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            ApplyPreviewText();
            return;
        }

        Subscribe(true);
        RefreshFromManager();
        StartTickIfNeeded();
    }

    private void OnDisable()
    {
        UnsubscribeOnly();
        StopTick();
    }

    private void OnDestroy()
    {
        UnsubscribeOnly();
        StopTick();
    }

    private void ResolveManager(bool allowCreate)
    {
        if (allowCreate)
        {
            livesManager = LivesManager.EnsureInstance();
        }
        else if (livesManager == null)
        {
            livesManager = LivesManager.Instance;
        }
    }

    private void Subscribe(bool bind)
    {
        if (bind)
        {
            ResolveManager(allowCreate: true);
        }
        else
        {
            ResolveManager(allowCreate: false);
        }

        if (livesManager == null)
        {
            return;
        }

        livesManager.OnLivesChanged -= OnLivesChanged;
        if (bind)
        {
            livesManager.OnLivesChanged += OnLivesChanged;
            subscribed = true;
        }
        else
        {
            subscribed = false;
        }
    }

    private void UnsubscribeOnly()
    {
        if (livesManager == null)
        {
            livesManager = LivesManager.Instance;
        }

        if (livesManager == null)
        {
            subscribed = false;
            return;
        }

        livesManager.OnLivesChanged -= OnLivesChanged;
        subscribed = false;
    }

    private void OnLivesChanged(int _)
    {
        RefreshFromManager();
        StartTickIfNeeded();
    }

    private void RefreshFromManager()
    {
        if (!Application.isPlaying)
        {
            ApplyPreviewText();
            return;
        }

        ResolveManager(allowCreate: true);
        if (livesManager == null)
        {
            return;
        }

        if (livesText != null)
        {
            livesText.text = LivesDisplayFormatting.FormatLivesFraction(
                livesManager.CurrentLives,
                LivesManager.MaxLives
            );
        }

        bool showCountdown = !livesManager.IsFull &&
                             livesManager.HasActiveNextLifeTimestamp;
        if (countdownRoot != null)
        {
            countdownRoot.SetActive(showCountdown);
        }

        if (countdownText != null)
        {
            if (showCountdown)
            {
                countdownText.text = LivesDisplayFormatting.FormatCountdown(
                    livesManager.TimeUntilNextLife
                );
                countdownText.gameObject.SetActive(true);
            }
            else
            {
                countdownText.text = string.Empty;
                if (countdownRoot == null)
                {
                    countdownText.gameObject.SetActive(false);
                }
            }
        }
    }

    private void StartTickIfNeeded()
    {
        StopTick();
        if (!Application.isPlaying || !isActiveAndEnabled)
        {
            return;
        }

        ResolveManager(allowCreate: true);
        if (livesManager == null || livesManager.IsFull)
        {
            return;
        }

        tickRoutine = StartCoroutine(CountdownTick());
    }

    private void StopTick()
    {
        if (tickRoutine != null)
        {
            StopCoroutine(tickRoutine);
            tickRoutine = null;
        }
    }

    private IEnumerator CountdownTick()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(TickSeconds);
        while (enabled && livesManager != null && !livesManager.IsFull)
        {
            yield return wait;

            if (livesManager == null)
            {
                yield break;
            }

            TimeSpan until = livesManager.TimeUntilNextLife;
            if (until <= TimeSpan.Zero && !livesManager.IsFull)
            {
                livesManager.RecalculateRegeneration();
                RefreshFromManager();
                if (livesManager.IsFull)
                {
                    yield break;
                }

                continue;
            }

            if (countdownText != null &&
                !livesManager.IsFull &&
                livesManager.HasActiveNextLifeTimestamp)
            {
                countdownText.text = LivesDisplayFormatting.FormatCountdown(until);
            }
        }
    }

    private void ApplyPreviewText()
    {
        if (livesText != null)
        {
            livesText.text = previewLivesText;
        }

        if (countdownText != null)
        {
            countdownText.text = previewCountdownText;
            countdownText.gameObject.SetActive(true);
        }

        if (countdownRoot != null)
        {
            countdownRoot.SetActive(true);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            ApplyPreviewText();
        }
    }

    [ContextMenu("Preview Lives HUD")]
    private void ContextPreviewHud()
    {
        ApplyPreviewText();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string DebugDescribeState()
    {
        ResolveManager(allowCreate: Application.isPlaying);
        return "livesText=" + (livesText != null ? livesText.text : "null") +
               " countdown=" + (countdownText != null ? countdownText.text : "null") +
               " subscribed=" + subscribed +
               " manager=" +
               (livesManager != null
                   ? livesManager.CurrentLives + "/" + LivesManager.MaxLives
                   : "null");
    }
#endif
}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Failure-panel button binding helpers: exclusive onClick + optional input diagnostics.
/// Clears persistent Inspector duplicates (e.g. PauseManager + MissionUI on the same MENU).
/// </summary>
public static class FailureUiButtonBinding
{
    /// <summary>
    /// Replaces all persistent + runtime onClick listeners with a single handler.
    /// </summary>
    public static void BindExclusive(Button button, UnityAction handler, string debugLabel)
    {
        if (button == null || handler == null)
        {
            return;
        }

        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(handler);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        FailureUiInputDiagnostics.EnsureAttached(button, debugLabel);
#endif
    }
}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
/// <summary>
/// Temporary diagnostics: distinguishes missed PointerClick vs rejected SceneTransition.
/// </summary>
[DisallowMultipleComponent]
public sealed class FailureUiInputDiagnostics : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler
{
    private string label = "Button";
    private Button button;

    public static void EnsureAttached(Button button, string debugLabel)
    {
        if (button == null)
        {
            return;
        }

        FailureUiInputDiagnostics probe =
            button.GetComponent<FailureUiInputDiagnostics>();
        if (probe == null)
        {
            probe = button.gameObject.AddComponent<FailureUiInputDiagnostics>();
        }

        probe.label = string.IsNullOrEmpty(debugLabel) ? button.name : debugLabel;
        probe.button = button;
        probe.HookClickListener();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Log("PointerDown", eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Log("PointerUp", eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Log("PointerClick", eventData);
    }

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        HookClickListener();
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }

    private void HookClickListener()
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(OnButtonClicked);
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        Log("Button.onClick", null);
    }

    private void Log(string phase, PointerEventData eventData)
    {
        GameObject selected =
            EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject
                : null;

        string pressAnim =
            GetComponent<UIButtonPressAnimation>() != null ? "yes" : "no";

        Debug.Log(
            "[FailureUiInput] " + phase +
            " label=" + label +
            " frame=" + Time.frameCount +
            " transitioning=" + SceneTransition.IsTransitioning +
            " selected=" + (selected != null ? selected.name : "null") +
            " pressAnim=" + pressAnim +
            " activePanels=" + DescribeActiveFailurePanels() +
            (eventData != null
                ? " pointerId=" + eventData.pointerId +
                  " eligible=" + eventData.eligibleForClick
                : string.Empty)
        );
    }

    private static string DescribeActiveFailurePanels()
    {
        string[] names =
        {
            "MissionFailedLimitPanel",
            "MissionFailedTimedPanel",
            "MissionFailedNoTouchPanel",
            "MissionFailedFragileCargoPanel",
            "OutOfLivesPanel",
            "PausePanel",
            "FeatureTutorialOverlay"
        };

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < names.Length; i++)
        {
            GameObject go = GameObject.Find(names[i]);
            if (go == null)
            {
                continue;
            }

            if (sb.Length > 0)
            {
                sb.Append(',');
            }

            sb.Append(names[i])
                .Append(go.activeInHierarchy ? ":ON" : ":off");
        }

        EventSystem[] eventSystems =
            Object.FindObjectsByType<EventSystem>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
        sb.Append(" eventSystems=").Append(eventSystems.Length);

        SceneTransition[] transitions =
            Object.FindObjectsByType<SceneTransition>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
        sb.Append(" sceneTransitions=").Append(transitions.Length);

        return sb.ToString();
    }
}
#endif

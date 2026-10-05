using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Authored Settings control for Daily Challenge notifications.
/// Runtime owns toggle state and interactivity; Inspector owns layout/visuals.
/// Never prompts at startup — only when the user enables the toggle.
/// </summary>
public class DailyNotificationSettingsUI : MonoBehaviour
{
    [Header("Roots (optional)")]
    [SerializeField] private GameObject sectionRoot;

    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI helperText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Toggle")]
    [SerializeField] private Toggle notificationsToggle;

    private bool syncingUi;
    private bool changeInFlight;

    private void Awake()
    {
        if (titleText != null)
        {
            titleText.text = "DAILY NOTIFICATIONS";
        }
    }

    private void OnEnable()
    {
        if (notificationsToggle != null)
        {
            notificationsToggle.onValueChanged.AddListener(OnToggleChanged);
        }

        DailyNotificationService.Bootstrap();
        RefreshFromService();
    }

    private void OnDisable()
    {
        if (notificationsToggle != null)
        {
            notificationsToggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }

    /// <summary>Call when Settings panel opens.</summary>
    public void RefreshFromSettingsOpen()
    {
        DailyNotificationService.Bootstrap();
        RefreshFromService();
    }

    private void RefreshFromService()
    {
        syncingUi = true;
        try
        {
            bool messagingOk = DailyNotificationService.IsMessagingSdkAvailable;
            bool editor = DailyNotificationService.IsEditorUnsupported;
            bool preferred = DailyNotificationService.PreferredEnabled;

            if (helperText != null)
            {
                helperText.gameObject.SetActive(editor);
                if (editor)
                {
                    helperText.text = "Push notifications unavailable in Editor";
                }
            }

            if (statusText != null)
            {
                statusText.text = string.Empty;
                statusText.gameObject.SetActive(false);
            }

            if (notificationsToggle != null)
            {
                notificationsToggle.interactable = CanInteract(editor, messagingOk);
                notificationsToggle.SetIsOnWithoutNotify(preferred && (editor || messagingOk));
            }
        }
        finally
        {
            syncingUi = false;
        }
    }

    private void OnToggleChanged(bool wantEnabled)
    {
        if (syncingUi || changeInFlight)
        {
            return;
        }

        _ = ApplyToggleAsync(wantEnabled);
    }

    private async Task ApplyToggleAsync(bool wantEnabled)
    {
        changeInFlight = true;
        if (notificationsToggle != null)
        {
            notificationsToggle.interactable = false;
        }

        try
        {
            bool enabled = await DailyNotificationService.SetEnabledFromSettingsAsync(wantEnabled);
            // Keep toggle honest if permission denied / SDK missing.
            syncingUi = true;
            if (notificationsToggle != null)
            {
                notificationsToggle.SetIsOnWithoutNotify(enabled);
            }

            syncingUi = false;
            RefreshFromService();
        }
        finally
        {
            changeInFlight = false;
            if (notificationsToggle != null)
            {
                notificationsToggle.interactable = CanInteract(
                    DailyNotificationService.IsEditorUnsupported,
                    DailyNotificationService.IsMessagingSdkAvailable);
            }
        }
    }

    private bool CanInteract(bool editor, bool messagingOk)
    {
        return !changeInFlight && !editor && messagingOk;
    }
}

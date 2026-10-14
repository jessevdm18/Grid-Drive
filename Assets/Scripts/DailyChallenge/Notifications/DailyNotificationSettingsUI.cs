using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Settings rows for Daily Challenge (FCM), Lives Full, and Free Coins.
/// Scene owns the rows. Runtime only updates toggle state and interactivity.
/// Never prompts at startup — only when the user turns a toggle on.
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

    [Header("Lives")]
    [SerializeField] private TextMeshProUGUI livesTitleText;
    [SerializeField] private Toggle livesToggle;

    [Header("Free coins")]
    [SerializeField] private TextMeshProUGUI freeCoinsTitleText;
    [SerializeField] private Toggle freeCoinsToggle;

    private bool syncingUi;
    private bool changeInFlight;

    private void OnEnable()
    {
        // Authored in the scene. Runtime does not rewrite or create this label.
        _ = livesTitleText;
        _ = freeCoinsTitleText;

        if (notificationsToggle != null)
        {
            notificationsToggle.onValueChanged.AddListener(OnDailyToggleChanged);
        }

        if (livesToggle != null)
        {
            livesToggle.onValueChanged.AddListener(OnLivesToggleChanged);
        }

        if (freeCoinsToggle != null)
        {
            freeCoinsToggle.onValueChanged.AddListener(OnFreeCoinsToggleChanged);
        }

        DailyNotificationService.Bootstrap();
        RefreshFromService();
    }

    private void OnDisable()
    {
        if (notificationsToggle != null)
        {
            notificationsToggle.onValueChanged.RemoveListener(OnDailyToggleChanged);
        }

        if (livesToggle != null)
        {
            livesToggle.onValueChanged.RemoveListener(OnLivesToggleChanged);
        }

        if (freeCoinsToggle != null)
        {
            freeCoinsToggle.onValueChanged.RemoveListener(OnFreeCoinsToggleChanged);
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
            bool dailyPreferred = DailyNotificationService.PreferredEnabled;

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
                notificationsToggle.interactable = CanUseDaily(editor, messagingOk);
                notificationsToggle.SetIsOnWithoutNotify(dailyPreferred && (editor || messagingOk));
            }

            if (livesToggle != null)
            {
                livesToggle.interactable = !changeInFlight;
                livesToggle.SetIsOnWithoutNotify(LivesNotificationService.PreferredEnabled);
            }

            if (freeCoinsToggle != null)
            {
                freeCoinsToggle.interactable = !changeInFlight;
                freeCoinsToggle.SetIsOnWithoutNotify(FreeCoinsNotificationService.PreferredEnabled);
            }
        }
        finally
        {
            syncingUi = false;
        }
    }

    private void OnDailyToggleChanged(bool wantEnabled)
    {
        if (syncingUi || changeInFlight)
        {
            return;
        }

        _ = ApplyDailyAsync(wantEnabled);
    }

    private void OnLivesToggleChanged(bool wantEnabled)
    {
        if (syncingUi || changeInFlight)
        {
            return;
        }

        _ = ApplyLivesAsync(wantEnabled);
    }

    private void OnFreeCoinsToggleChanged(bool wantEnabled)
    {
        if (syncingUi || changeInFlight)
        {
            return;
        }

        _ = ApplyFreeCoinsAsync(wantEnabled);
    }

    private async Task ApplyDailyAsync(bool wantEnabled)
    {
        changeInFlight = true;
        SetInteractable(false);
        try
        {
            NotificationOptInResult result = await DailyNotificationService.SetEnabledAsync(
                wantEnabled,
                DailyNotificationService.AnalyticsSourceSettings);
            if (result == NotificationOptInResult.NeedsSettings)
            {
                NotificationPromptCoordinator.ShowSettingsExplainer(RefreshFromService);
            }

            RefreshFromService();
        }
        finally
        {
            changeInFlight = false;
            RefreshFromService();
        }
    }

    private async Task ApplyLivesAsync(bool wantEnabled)
    {
        changeInFlight = true;
        SetInteractable(false);
        try
        {
            NotificationOptInResult result =
                await LivesNotificationService.SetEnabledFromUserAsync(wantEnabled);
            if (result == NotificationOptInResult.NeedsSettings)
            {
                NotificationPromptCoordinator.ShowSettingsExplainer(RefreshFromService);
            }

            RefreshFromService();
        }
        finally
        {
            changeInFlight = false;
            RefreshFromService();
        }
    }

    private async Task ApplyFreeCoinsAsync(bool wantEnabled)
    {
        changeInFlight = true;
        SetInteractable(false);
        try
        {
            NotificationOptInResult result =
                await FreeCoinsNotificationService.SetEnabledFromUserAsync(wantEnabled);
            if (result == NotificationOptInResult.NeedsSettings)
            {
                NotificationPromptCoordinator.ShowSettingsExplainer(RefreshFromService);
            }

            RefreshFromService();
        }
        finally
        {
            changeInFlight = false;
            RefreshFromService();
        }
    }

    private void SetInteractable(bool fallback)
    {
        if (notificationsToggle != null)
        {
            notificationsToggle.interactable = fallback && CanUseDaily(
                DailyNotificationService.IsEditorUnsupported,
                DailyNotificationService.IsMessagingSdkAvailable);
        }

        if (livesToggle != null)
        {
            livesToggle.interactable = fallback && !changeInFlight;
        }

        if (freeCoinsToggle != null)
        {
            freeCoinsToggle.interactable = fallback && !changeInFlight;
        }
    }

    private bool CanUseDaily(bool editor, bool messagingOk)
    {
        return !changeInFlight && !editor && messagingOk;
    }
}

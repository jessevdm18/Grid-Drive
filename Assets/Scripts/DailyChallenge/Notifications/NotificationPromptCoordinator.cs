using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One-time contextual opt-in for Daily (FCM) and Lives (local alarm).
/// Uses the shared Android POST_NOTIFICATIONS permission. Does not prompt at startup.
/// </summary>
public class NotificationPromptCoordinator : MonoBehaviour
{
    public const string DailyPromptShownKey = "RushOut_DailyNotificationPromptShown";
    public const string LivesPromptShownKey = "RushOut_LivesNotificationPromptShown";

    private const float LivesSettleSeconds = 0.6f;

    private enum PromptKind
    {
        None = 0,
        Daily = 1,
        Lives = 2,
        Settings = 3
    }

    private static NotificationPromptCoordinator instance;
    private static bool pendingLives;
    private static float livesQueuedUnscaled;

    private PromptKind showingKind;
    private bool choiceInFlight;
    private bool built;
    private bool markShownOnDisplay;
    private Action onFinished;

    private GameObject overlayRoot;
    private TextMeshProUGUI titleLabel;
    private TextMeshProUGUI bodyLabel;
    private TextMeshProUGUI declineLabel;
    private TextMeshProUGUI acceptLabel;
    private Button declineButton;
    private Button acceptButton;

    public static bool IsShowing =>
        instance != null && instance.showingKind != PromptKind.None;

    /// <summary>
    /// Daily result Continue. Shows the one-time Daily offer before leaving.
    /// Returns true when the prompt owns the continue callback.
    /// </summary>
    public static bool TryOfferDailyBeforeLeaving(Action then)
    {
        if (DailyNotificationService.PreferredEnabled)
        {
            MarkShown(DailyPromptShownKey);
            return false;
        }

        if (IsShown(DailyPromptShownKey) || IsShowing)
        {
            return false;
        }

        Ensure().Present(PromptKind.Daily, markShown: true, then);
        return true;
    }

    /// <summary>First genuine life loss below max. Waits until the fail or menu flow is idle.</summary>
    public static void NotifyGenuineLifeLost()
    {
        if (DailyChallengeGameplayPolicy.IsActive)
        {
            return;
        }

        if (LivesNotificationService.PreferredEnabled)
        {
            MarkShown(LivesPromptShownKey);
            return;
        }

        if (IsShown(LivesPromptShownKey) || pendingLives)
        {
            return;
        }

        LivesManager lives = LivesManager.Instance;
        if (lives != null && lives.IsFull)
        {
            return;
        }

        pendingLives = true;
        livesQueuedUnscaled = Time.unscaledTime;
        Ensure();
    }

    public static void ShowSettingsExplainer(Action then)
    {
        Ensure().Present(PromptKind.Settings, markShown: false, then);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static void ShowDailyPromptForDevelopment()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Notifications] Enter Play Mode to preview the Daily prompt.");
            return;
        }

        Ensure().Present(PromptKind.Daily, markShown: false, null);
    }

    public static void ShowLivesPromptForDevelopment()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Notifications] Enter Play Mode to preview the Lives prompt.");
            return;
        }

        Ensure().Present(PromptKind.Lives, markShown: false, null);
    }

    public static void ResetPromptFlags()
    {
        PlayerPrefs.DeleteKey(DailyPromptShownKey);
        PlayerPrefs.DeleteKey(LivesPromptShownKey);
        PlayerPrefs.Save();
        pendingLives = false;
        Debug.Log(
            "[Notifications] Reset prompt-shown flags only. Enabled preferences were not changed.");
    }
#endif

    private static NotificationPromptCoordinator Ensure()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject go = new GameObject("NotificationPromptCoordinator");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<NotificationPromptCoordinator>();
        return instance;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Update()
    {
        if (!pendingLives || showingKind != PromptKind.None)
        {
            return;
        }

        if (Time.unscaledTime - livesQueuedUnscaled < LivesSettleSeconds)
        {
            return;
        }

        if (IsBlocked())
        {
            return;
        }

        LivesManager lives = LivesManager.Instance;
        if (lives == null || lives.IsFull || lives.CurrentLives >= LivesManager.MaxLives)
        {
            pendingLives = false;
            return;
        }

        if (!IsLivesSurfaceReady())
        {
            return;
        }

        pendingLives = false;
        Present(PromptKind.Lives, markShown: true, null);
    }

    private static bool IsLivesSurfaceReady()
    {
        GameManager gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null && gameManager.IsLevelFailed)
        {
            return true;
        }

        return SceneManager.GetActiveScene().name == "MainMenu";
    }

    private static bool IsBlocked()
    {
        if (SceneTransition.IsTransitioning)
        {
            return true;
        }

        OutOfLivesUI outOfLives = OutOfLivesUI.FindInLoadedScenes();
        if (outOfLives != null && outOfLives.IsOpen)
        {
            return true;
        }

        DailyRewardUI reward = FindAnyObjectByType<DailyRewardUI>(FindObjectsInactive.Include);
        if (reward != null && reward.IsShowing)
        {
            return true;
        }

        if (IapService.Instance != null && IapService.Instance.IsPurchaseInProgress)
        {
            return true;
        }

        AdsManager ads = FindAnyObjectByType<AdsManager>();
        if (ads != null && ads.IsFullScreenAdActive)
        {
            return true;
        }

        if (AnalyticsConsentPopup.Instance != null && AnalyticsConsentPopup.Instance.IsVisible)
        {
            return true;
        }

        MainMenuSettingsUI settings = FindAnyObjectByType<MainMenuSettingsUI>();
        if (settings != null && settings.IsOpen)
        {
            return true;
        }

        return false;
    }

    private void Present(PromptKind kind, bool markShown, Action then)
    {
        if (showingKind != PromptKind.None && kind != PromptKind.Settings)
        {
            then?.Invoke();
            return;
        }

        EnsureUi();
        showingKind = kind;
        choiceInFlight = false;
        markShownOnDisplay = markShown;
        onFinished = then;
        ApplyCopy(kind);
        overlayRoot.SetActive(true);
        SetButtonsInteractable(true);

        if (markShown)
        {
            if (kind == PromptKind.Daily)
            {
                MarkShown(DailyPromptShownKey);
            }
            else if (kind == PromptKind.Lives)
            {
                MarkShown(LivesPromptShownKey);
            }
        }
    }

    private void ApplyCopy(PromptKind kind)
    {
        if (kind == PromptKind.Daily)
        {
            titleLabel.text = "DON'T MISS TOMORROW'S DAILY";
            bodyLabel.text = "Get a notification when the next Daily Challenge is ready.";
            declineLabel.text = "NOT NOW";
            acceptLabel.text = "NOTIFY ME";
            return;
        }

        if (kind == PromptKind.Lives)
        {
            titleLabel.text = "LIVES RECHARGING";
            bodyLabel.text = "Want a notification when all 5 lives are ready again?";
            declineLabel.text = "NOT NOW";
            acceptLabel.text = "NOTIFY ME";
            return;
        }

        titleLabel.text = "NOTIFICATIONS";
        bodyLabel.text = "Notifications are disabled for Rush Out. Enable them in Android Settings.";
        declineLabel.text = "CANCEL";
        acceptLabel.text = "OPEN SETTINGS";
    }

    private void OnDecline()
    {
        if (choiceInFlight)
        {
            return;
        }

        Close();
    }

    private void OnAccept()
    {
        if (choiceInFlight)
        {
            return;
        }

        if (showingKind == PromptKind.Settings)
        {
            AndroidNotificationPermission.OpenAppSettings();
            Close();
            return;
        }

        PromptKind kind = showingKind;
        choiceInFlight = true;
        SetButtonsInteractable(false);
        _ = AcceptAsync(kind);
    }

    private async Task AcceptAsync(PromptKind kind)
    {
        NotificationOptInResult result = NotificationOptInResult.Disabled;
        try
        {
            if (kind == PromptKind.Daily)
            {
                result = await DailyNotificationService.SetEnabledAsync(
                    true,
                    DailyNotificationService.AnalyticsSourcePrompt);
            }
            else if (kind == PromptKind.Lives)
            {
                result = await LivesNotificationService.SetEnabledFromUserAsync(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Notifications] Opt-in failed: " + ex.GetType().Name);
            result = NotificationOptInResult.Disabled;
        }

        choiceInFlight = false;
        if (result == NotificationOptInResult.NeedsSettings)
        {
            Present(PromptKind.Settings, markShown: false, onFinished);
            return;
        }

        Close();
    }

    private void Close()
    {
        showingKind = PromptKind.None;
        choiceInFlight = false;
        if (overlayRoot != null)
        {
            overlayRoot.SetActive(false);
        }

        Action finished = onFinished;
        onFinished = null;
        finished?.Invoke();
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (declineButton != null)
        {
            declineButton.interactable = interactable;
        }

        if (acceptButton != null)
        {
            acceptButton.interactable = interactable;
        }
    }

    private static bool IsShown(string key)
    {
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    private static void MarkShown(string key)
    {
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    private void EnsureUi()
    {
        if (built)
        {
            return;
        }

        built = true;
        TMP_FontAsset font = null;
        TextMeshProUGUI sample = FindAnyObjectByType<TextMeshProUGUI>();
        if (sample != null)
        {
            font = sample.font;
        }

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;
        gameObject.AddComponent<GraphicRaycaster>();
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        overlayRoot = CreateUi("Overlay", transform);
        RectTransform overlayRt = overlayRoot.GetComponent<RectTransform>();
        Stretch(overlayRt);
        Image dim = overlayRoot.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.72f);

        GameObject panel = CreateUi("Panel", overlayRoot.transform);
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(860f, 520f);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.10f, 0.14f, 0.20f, 0.98f);

        titleLabel = CreateLabel("Title", panel.transform, font, 52f, new Vector2(0f, 150f), new Vector2(780f, 140f));
        bodyLabel = CreateLabel("Body", panel.transform, font, 34f, new Vector2(0f, 10f), new Vector2(760f, 160f));

        declineButton = CreateButton("NotNow", panel.transform, font, out declineLabel, new Vector2(-190f, -170f));
        acceptButton = CreateButton("NotifyMe", panel.transform, font, out acceptLabel, new Vector2(190f, -170f));
        declineButton.onClick.AddListener(OnDecline);
        acceptButton.onClick.AddListener(OnAccept);
        overlayRoot.SetActive(false);
    }

    private static GameObject CreateUi(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static TextMeshProUGUI CreateLabel(
        string name,
        Transform parent,
        TMP_FontAsset font,
        float size,
        Vector2 position,
        Vector2 box)
    {
        GameObject go = CreateUi(name, parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        RectTransform rt = tmp.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = box;
        return tmp;
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        TMP_FontAsset font,
        out TextMeshProUGUI label,
        Vector2 position)
    {
        GameObject go = CreateUi(name, parent);
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.18f, 0.48f, 0.78f, 1f);
        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(340f, 110f);
        label = CreateLabel("Label", go.transform, font, 32f, Vector2.zero, new Vector2(320f, 90f));
        return button;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Knoppen voor het MainMenu-scherm.
/// Koppel in de Inspector elke knop aan de juiste methode.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private MainMenuSettingsUI settingsUI;
    [SerializeField] private ShopUIController shopUI;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private const int CrashlyticsTestTapCount = 7;
    private const float CrashlyticsTestTapWindowSeconds = 3f;

    private int settingsTapCount;
    private float settingsTapWindowStartUnscaled;
#endif

    private void Start()
    {
        // Safety if Splash was skipped (Editor Play from MainMenu).
        LevelDatabaseContentVersion.ApplyIfNeeded();

        // Authoritative menu music — works whether MusicManager came from this
        // scene or was bootstrapped earlier (e.g. fresh Splash→Gameplay→Menu).
        MusicManager.PlayMenuMusic();

        // Ensure Daily Challenge domain is alive for countdown / day rollover.
        DailyChallengeManager.EnsureInstance();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        AttachLevelsButtonNavProbe();
#endif
    }

    /// <summary>
    /// Start het spel → Gameplay-scene.
    /// </summary>
    public void OnPlayButton()
    {
        SceneTransition.LoadScene("Gameplay");
    }

    /// <summary>
    /// Open levelkeuze → LevelSelect-scene.
    /// </summary>
    public void OnLevelsButton()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[MainMenuNav] LevelsButton.onClick" +
            " frame=" + Time.frameCount +
            " transitioning=" + SceneTransition.IsTransitioning);
#endif
        SceneTransition.LoadScene("LevelSelect");
    }

    /// <summary>
    /// SettingsButton: opent het MainMenu settingspanel.
    /// Development builds: 7 taps within 3s also sends a Crashlytics non-fatal test.
    /// </summary>
    public void OnSettingsButton()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        RegisterSettingsTapForCrashlyticsTest();
#endif

        if (settingsUI == null)
        {
            settingsUI = FindAnyObjectByType<MainMenuSettingsUI>();
        }

        if (settingsUI != null)
        {
            settingsUI.OpenSettings();
        }
        else
        {
            Debug.LogWarning("MainMenuUI: MainMenuSettingsUI is not assigned.");
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void RegisterSettingsTapForCrashlyticsTest()
    {
        float now = Time.unscaledTime;
        if (settingsTapCount == 0 ||
            now - settingsTapWindowStartUnscaled > CrashlyticsTestTapWindowSeconds)
        {
            settingsTapCount = 0;
            settingsTapWindowStartUnscaled = now;
        }

        settingsTapCount++;
        if (settingsTapCount < CrashlyticsTestTapCount)
        {
            return;
        }

        settingsTapCount = 0;
        settingsTapWindowStartUnscaled = 0f;
        CrashlyticsDeviceTest.SendTestNonFatal();
    }

    private void AttachLevelsButtonNavProbe()
    {
        // Prefer the Inspector-wired Levels button via persistent onClick target search.
        Button[] buttons = FindObjectsByType<Button>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
            {
                continue;
            }

            if (button.name != "LevelsButton" && button.name != "Levels")
            {
                continue;
            }

            MenuNavInputDiagnostics.EnsureAttached(button, "MainMenu.Levels");
            return;
        }
    }
#endif

    /// <summary>
    /// ShopButton: opent het ShopPanel (geen scene load).
    /// </summary>
    public void OnShopButton()
    {
        if (shopUI == null)
        {
            shopUI = FindAnyObjectByType<ShopUIController>();
        }

        if (shopUI != null)
        {
            shopUI.OpenShop();
        }
        else
        {
            Debug.LogWarning("MainMenuUI: ShopUIController is not assigned.");
        }
    }
}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
/// <summary>
/// Dev-only: proves where a MainMenu navigation click is lost
/// (pointer vs onClick vs SceneTransition reject).
/// </summary>
[DisallowMultipleComponent]
public sealed class MenuNavInputDiagnostics : MonoBehaviour,
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

        MenuNavInputDiagnostics probe = button.GetComponent<MenuNavInputDiagnostics>();
        if (probe == null)
        {
            probe = button.gameObject.AddComponent<MenuNavInputDiagnostics>();
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
        CanvasGroup transitionGroup = null;
        Image fadeImage = null;
        SceneTransition[] transitions = Object.FindObjectsByType<SceneTransition>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        if (transitions != null && transitions.Length > 0 && transitions[0] != null)
        {
            transitionGroup = transitions[0].GetComponent<CanvasGroup>();
            Transform overlay = transitions[0].transform.Find("FadeOverlay");
            if (overlay != null)
            {
                fadeImage = overlay.GetComponent<Image>();
            }
        }

        EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        Debug.Log(
            "[MenuNavInput] " + phase +
            " label=" + label +
            " frame=" + Time.frameCount +
            " transitioning=" + SceneTransition.IsTransitioning +
            " fadeBlocks=" + (transitionGroup != null && transitionGroup.blocksRaycasts) +
            " fadeAlpha=" + (transitionGroup != null ? transitionGroup.alpha.ToString("0.00") : "n/a") +
            " fadeRaycastTarget=" + (fadeImage != null && fadeImage.raycastTarget) +
            " eventSystems=" + eventSystems.Length +
            " sceneTransitions=" + (transitions != null ? transitions.Length : 0) +
            (eventData != null
                ? " eligible=" + eventData.eligibleForClick
                : string.Empty)
        );
    }
}
#endif

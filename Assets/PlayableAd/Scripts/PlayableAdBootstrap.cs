using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Starts one authored level immediately and shows the store CTA after the exit.
/// Does not read or write campaign saves.
/// </summary>
[DefaultExecutionOrder(-500)]
public class PlayableAdBootstrap : MonoBehaviour
{
    [SerializeField] private LevelData level;
    [SerializeField] private Sprite logo;
    [SerializeField] private TMP_FontAsset font;

    private GameManager gameManager;
    private TextMeshProUGUI movesLabel;
    private GameObject winRoot;
    private bool winShown;

    private void Awake()
    {
        PlayableAdSession.Begin(level);
        EnsureEventSystem();
        BuildHud();
    }

    private void OnEnable()
    {
        PlayableAdSession.LevelCompleted += OnLevelCompleted;
    }

    private void OnDisable()
    {
        PlayableAdSession.LevelCompleted -= OnLevelCompleted;
    }

    private void Update()
    {
        if (movesLabel == null)
        {
            return;
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        int moves = gameManager != null ? gameManager.CurrentMoves : 0;
        movesLabel.text = "MOVES  " + moves;
    }

    private void OnLevelCompleted()
    {
        if (winShown)
        {
            return;
        }

        winShown = true;
        StartCoroutine(ShowWinAfterDelay());
    }

    private IEnumerator ShowWinAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.75f);
        if (winRoot != null)
        {
            winRoot.SetActive(true);
        }
    }

    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    private void BuildHud()
    {
        TMP_FontAsset useFont = font != null ? font : TMP_Settings.defaultFontAsset;

        GameObject canvasObject = new GameObject(
            "PlayableAdCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(320f, 480f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;

        CreateText(
            canvasRect,
            "Prompt",
            "GET THE RED CAR OUT",
            18f,
            FontStyles.Bold,
            useFont,
            new Vector2(0.08f, 1f),
            new Vector2(0.92f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -10f),
            new Vector2(0f, 28f),
            TextAlignmentOptions.Center);

        movesLabel = CreateText(
            canvasRect,
            "Moves",
            "MOVES  0",
            16f,
            FontStyles.Bold,
            useFont,
            new Vector2(0.08f, 1f),
            new Vector2(0.92f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -36f),
            new Vector2(0f, 22f),
            TextAlignmentOptions.Center);

        winRoot = CreateWinOverlay(canvasRect, useFont);
        winRoot.SetActive(false);
    }

    private GameObject CreateWinOverlay(RectTransform parent, TMP_FontAsset useFont)
    {
        GameObject root = new GameObject("WinOverlay", typeof(RectTransform), typeof(Image));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(parent, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        root.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.1f, 0.82f);

        if (logo != null)
        {
            GameObject logoObject = new GameObject("Logo", typeof(RectTransform), typeof(Image));
            RectTransform logoRect = logoObject.GetComponent<RectTransform>();
            logoRect.SetParent(rootRect, false);
            logoRect.anchorMin = new Vector2(0.5f, 1f);
            logoRect.anchorMax = new Vector2(0.5f, 1f);
            logoRect.pivot = new Vector2(0.5f, 1f);
            logoRect.anchoredPosition = new Vector2(0f, -28f);
            logoRect.sizeDelta = new Vector2(200f, 64f);
            Image logoImage = logoObject.GetComponent<Image>();
            logoImage.sprite = logo;
            logoImage.preserveAspect = true;
            logoImage.color = Color.white;
        }

        CreateText(
            rootRect,
            "Nice",
            "NICE!\nYOU ESCAPED",
            28f,
            FontStyles.Bold,
            useFont,
            new Vector2(0.08f, 0.5f),
            new Vector2(0.92f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 36f),
            new Vector2(0f, 80f),
            TextAlignmentOptions.Center);

        CreateText(
            rootRect,
            "Next",
            "CAN YOU SOLVE THE NEXT ONE?",
            13f,
            FontStyles.Normal,
            useFont,
            new Vector2(0.08f, 0.5f),
            new Vector2(0.92f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -16f),
            new Vector2(0f, 28f),
            TextAlignmentOptions.Center);

        GameObject buttonObject = new GameObject(
            "PlayButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.SetParent(rootRect, false);
        buttonRect.anchorMin = new Vector2(0.12f, 0f);
        buttonRect.anchorMax = new Vector2(0.88f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 28f);
        buttonRect.sizeDelta = new Vector2(0f, 48f);
        buttonObject.GetComponent<Image>().color = new Color(0.95f, 0.32f, 0.22f, 1f);
        buttonObject.GetComponent<Button>().onClick.AddListener(OpenStore);

        CreateText(
            buttonRect,
            "Label",
            "PLAY GRID DRIVE",
            16f,
            FontStyles.Bold,
            useFont,
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            TextAlignmentOptions.Center);

        return root;
    }

    private static void OpenStore()
    {
        Application.OpenURL(PlayableAdSession.StoreUrl);
    }

    private static TextMeshProUGUI CreateText(
        RectTransform parent,
        string name,
        string text,
        float size,
        FontStyles style,
        TMP_FontAsset useFont,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        if (useFont != null)
        {
            label.font = useFont;
        }

        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = alignment;
        label.color = Color.white;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.Normal;
        return label;
    }
}

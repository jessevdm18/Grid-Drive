#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Creates an authored Daily Notifications section under MainMenu Settings.
/// Never forces permission at startup.
/// </summary>
public static class DailyNotificationAuthoringMenu
{
    private const string MenuPath =
        "Rush Out/UI/Create Daily Notifications Section Under Settings";

    [MenuItem(MenuPath)]
    public static void CreateDailyNotificationsSection()
    {
        MainMenuSettingsUI settings = Object.FindAnyObjectByType<MainMenuSettingsUI>(
            FindObjectsInactive.Include);
        if (settings == null)
        {
            Debug.LogError(
                "[DailyNotify] Open MainMenu scene and ensure MainMenuSettingsUI exists.");
            return;
        }

        SerializedObject so = new SerializedObject(settings);
        SerializedProperty panelProp = so.FindProperty("settingsPanel");
        GameObject panel = panelProp != null
            ? panelProp.objectReferenceValue as GameObject
            : null;
        if (panel == null)
        {
            Debug.LogError("[DailyNotify] MainMenuSettingsUI.settingsPanel is not assigned.");
            return;
        }

        Transform existing = panel.transform.Find("DailyNotificationsSection");
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.LogWarning(
                "[DailyNotify] DailyNotificationsSection already exists — selected it.");
            return;
        }

        GameObject section = CreateUi("DailyNotificationsSection", panel.transform);
        RectTransform sectionRt = section.GetComponent<RectTransform>();
        sectionRt.anchorMin = new Vector2(0.5f, 0.5f);
        sectionRt.anchorMax = new Vector2(0.5f, 0.5f);
        sectionRt.pivot = new Vector2(0.5f, 0.5f);
        sectionRt.sizeDelta = new Vector2(560f, 110f);
        sectionRt.anchoredPosition = new Vector2(0f, -110f);

        TextMeshProUGUI title = CreateTmp(
            "Title",
            section.transform,
            "DAILY NOTIFICATIONS",
            40,
            FontStyles.Normal);
        title.alignment = TextAlignmentOptions.Center;
        PlaceCentered(title.rectTransform, new Vector2(0f, 20f), new Vector2(458.7f, 69.1f));

        TextMeshProUGUI helper = CreateTmp(
            "Helper",
            section.transform,
            "Push notifications unavailable in Editor",
            16,
            FontStyles.Normal);
        helper.color = new Color(0.85f, 0.85f, 0.85f, 0.75f);
        helper.alignment = TextAlignmentOptions.Center;
        PlaceCentered(helper.rectTransform, new Vector2(0f, -28f), new Vector2(458f, 24f));
        helper.gameObject.SetActive(false);

        TextMeshProUGUI status = CreateTmp(
            "Status",
            section.transform,
            string.Empty,
            22,
            FontStyles.Normal);
        status.gameObject.SetActive(false);

        GameObject toggleGo = CreateUi("NotificationsToggle", section.transform);
        Toggle toggle = toggleGo.AddComponent<Toggle>();
        Image bg = toggleGo.AddComponent<Image>();
        bg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        bg.type = Image.Type.Sliced;
        bg.color = Color.white;
        RectTransform toggleRt = toggleGo.GetComponent<RectTransform>();
        toggleRt.localScale = new Vector3(3.4f, 3.4f, 3.4f);
        PlaceCentered(toggleRt, new Vector2(202.8f, 20f), new Vector2(20f, 20f));

        GameObject checkGo = CreateUi("Checkmark", toggleGo.transform);
        Image check = checkGo.AddComponent<Image>();
        check.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
        check.color = Color.white;
        PlaceCentered(checkGo.GetComponent<RectTransform>(), Vector2.zero, new Vector2(20f, 20f));
        toggle.graphic = check;
        toggle.targetGraphic = bg;
        toggle.isOn = false;

        DailyNotificationSettingsUI ui = section.AddComponent<DailyNotificationSettingsUI>();
        SerializedObject uiSo = new SerializedObject(ui);
        uiSo.FindProperty("sectionRoot").objectReferenceValue = section;
        uiSo.FindProperty("titleText").objectReferenceValue = title;
        uiSo.FindProperty("helperText").objectReferenceValue = helper;
        uiSo.FindProperty("statusText").objectReferenceValue = status;
        uiSo.FindProperty("notificationsToggle").objectReferenceValue = toggle;
        uiSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedProperty notifyProp = so.FindProperty("dailyNotificationSettingsUI");
        if (notifyProp != null)
        {
            notifyProp.objectReferenceValue = ui;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = section;
        Debug.Log(
            "[DailyNotify] Created DailyNotificationsSection under Settings. " +
            "Default OFF — no startup permission prompt. " +
            "Import FirebaseMessaging.unitypackage 13.15.0 before device opt-in works.");
    }

    private static GameObject CreateUi(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static TextMeshProUGUI CreateTmp(
        string name,
        Transform parent,
        string text,
        float size,
        FontStyles style)
    {
        GameObject go = CreateUi(name, parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    private static void PlaceCentered(RectTransform rt, Vector2 anchoredPosition, Vector2 size)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = size;
    }
}
#endif

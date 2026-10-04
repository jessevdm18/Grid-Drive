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
        sectionRt.anchorMin = new Vector2(0.05f, 0.30f);
        sectionRt.anchorMax = new Vector2(0.95f, 0.48f);
        sectionRt.offsetMin = Vector2.zero;
        sectionRt.offsetMax = Vector2.zero;

        TextMeshProUGUI title = CreateTmp(
            "Title",
            section.transform,
            "DAILY NOTIFICATIONS",
            32,
            FontStyles.Bold);
        Place(title.rectTransform, 0f, 0.72f, 1f, 1f);

        TextMeshProUGUI helper = CreateTmp(
            "Helper",
            section.transform,
            "Get notified when a new Daily Challenge is ready.",
            20,
            FontStyles.Normal);
        Place(helper.rectTransform, 0f, 0.42f, 1f, 0.72f);

        TextMeshProUGUI status = CreateTmp(
            "Status",
            section.transform,
            "OFF",
            22,
            FontStyles.Normal);
        Place(status.rectTransform, 0.55f, 0.05f, 1f, 0.40f);

        GameObject toggleGo = CreateUi("NotificationsToggle", section.transform);
        Toggle toggle = toggleGo.AddComponent<Toggle>();
        Image bg = toggleGo.AddComponent<Image>();
        bg.color = new Color(0.25f, 0.25f, 0.28f, 1f);
        RectTransform toggleRt = toggleGo.GetComponent<RectTransform>();
        Place(toggleRt, 0.05f, 0.05f, 0.50f, 0.40f);

        GameObject checkGo = CreateUi("Checkmark", toggleGo.transform);
        Image check = checkGo.AddComponent<Image>();
        check.color = new Color(0.25f, 0.75f, 0.40f, 1f);
        Place(checkGo.GetComponent<RectTransform>(), 0.1f, 0.15f, 0.9f, 0.85f);
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

    private static void Place(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
    {
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
#endif

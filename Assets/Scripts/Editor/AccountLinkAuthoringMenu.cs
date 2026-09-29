#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Creates an authored ACCOUNT section under MainMenu Settings (Inspector-owned layout).
/// Does not force Google Play at startup.
/// </summary>
public static class AccountLinkAuthoringMenu
{
    private const string MenuPath =
        "Rush Out/UI/Create Account Link Section Under Settings";

    [MenuItem(MenuPath)]
    public static void CreateAccountLinkSection()
    {
        MainMenuSettingsUI settings = Object.FindAnyObjectByType<MainMenuSettingsUI>(
            FindObjectsInactive.Include);
        if (settings == null)
        {
            Debug.LogError(
                "[Account] Open MainMenu scene and ensure MainMenuSettingsUI exists.");
            return;
        }

        SerializedObject so = new SerializedObject(settings);
        SerializedProperty panelProp = so.FindProperty("settingsPanel");
        GameObject panel = panelProp != null
            ? panelProp.objectReferenceValue as GameObject
            : null;
        if (panel == null)
        {
            Debug.LogError("[Account] MainMenuSettingsUI.settingsPanel is not assigned.");
            return;
        }

        Transform existing = panel.transform.Find("AccountSection");
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.LogWarning("[Account] AccountSection already exists — selected it.");
            return;
        }

        GameObject section = CreateUi("AccountSection", panel.transform);
        RectTransform sectionRt = section.GetComponent<RectTransform>();
        sectionRt.anchorMin = new Vector2(0.05f, 0.08f);
        sectionRt.anchorMax = new Vector2(0.95f, 0.28f);
        sectionRt.offsetMin = Vector2.zero;
        sectionRt.offsetMax = Vector2.zero;

        TextMeshProUGUI title = CreateTmp("Title", section.transform, "ACCOUNT", 36, FontStyles.Bold);
        Place(title.rectTransform, 0f, 0.72f, 1f, 1f);

        TextMeshProUGUI status = CreateTmp(
            "Status",
            section.transform,
            "Playing as Guest",
            28,
            FontStyles.Normal);
        Place(status.rectTransform, 0f, 0.48f, 1f, 0.72f);

        TextMeshProUGUI helper = CreateTmp(
            "Helper",
            section.transform,
            "Link your account to protect your progress.",
            22,
            FontStyles.Normal);
        Place(helper.rectTransform, 0f, 0.28f, 1f, 0.48f);

        GameObject buttonGo = CreateUi("LinkGooglePlayButton", section.transform);
        Image img = buttonGo.AddComponent<Image>();
        img.color = new Color(0.18f, 0.55f, 0.28f, 1f);
        Button button = buttonGo.AddComponent<Button>();
        RectTransform buttonRt = buttonGo.GetComponent<RectTransform>();
        Place(buttonRt, 0.15f, 0f, 0.85f, 0.28f);

        TextMeshProUGUI buttonLabel = CreateTmp(
            "Label",
            buttonGo.transform,
            "LINK GOOGLE PLAY",
            26,
            FontStyles.Bold);
        Place(buttonLabel.rectTransform, 0f, 0f, 1f, 1f);

        AccountLinkUI linkUi = section.AddComponent<AccountLinkUI>();
        SerializedObject linkSo = new SerializedObject(linkUi);
        linkSo.FindProperty("accountRoot").objectReferenceValue = section;
        linkSo.FindProperty("titleText").objectReferenceValue = title;
        linkSo.FindProperty("statusText").objectReferenceValue = status;
        linkSo.FindProperty("helperText").objectReferenceValue = helper;
        linkSo.FindProperty("linkGooglePlayButton").objectReferenceValue = button;
        linkSo.FindProperty("linkButtonLabel").objectReferenceValue = buttonLabel;
        linkSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedProperty accountProp = so.FindProperty("accountLinkUI");
        if (accountProp != null)
        {
            accountProp.objectReferenceValue = linkUi;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = section;
        Debug.Log(
            "[Account] Created AccountSection under Settings. Adjust RectTransforms/styles in Inspector. " +
            "Google Play linking stays optional — no startup popup.");
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

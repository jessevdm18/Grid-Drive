#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One-time authoring migration: Pause RestartButton → RestartText + CostText + CoinIcon.
/// Scene becomes source of truth; PauseManager only updates the cost number.
/// </summary>
public static class PauseRestartCostRowMigrationMenu
{
    private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
    private const string CoinSpritePath = "Assets/Art/UI/Icons/Icon_Coin.png";

    [MenuItem("Rush Out/UI/Migrate Pause Restart Cost Row")]
    public static void Migrate()
    {
        Scene scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
        PauseManager pause = Object.FindAnyObjectByType<PauseManager>(FindObjectsInactive.Include);
        if (pause == null)
        {
            EditorUtility.DisplayDialog(
                "Pause Restart migration",
                "No PauseManager found in Gameplay.",
                "OK");
            return;
        }

        SerializedObject so = new SerializedObject(pause);
        Button restartButton = so.FindProperty("restartButton").objectReferenceValue as Button;
        if (restartButton == null)
        {
            EditorUtility.DisplayDialog(
                "Pause Restart migration",
                "PauseManager.restartButton is not assigned.",
                "OK");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(restartButton.gameObject, "Migrate Pause Restart Cost Row");
        Undo.RecordObject(pause, "Migrate Pause Restart Cost Row");

        TextMeshProUGUI restartText = EnsureRestartText(restartButton);
        TextMeshProUGUI costText = EnsureCostText(restartButton, restartText);
        Image coinIcon = EnsureCoinIcon(restartButton);

        so.FindProperty("restartButtonLabel").objectReferenceValue = restartText;
        so.FindProperty("restartCostLabel").objectReferenceValue = costText;
        so.FindProperty("restartCoinIcon").objectReferenceValue = coinIcon;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Seed authored cost value once (runtime will refresh from LivesEconomyConfig).
        costText.text = LivesEconomyConfig.RestartCoinCost.ToString();
        EditorUtility.SetDirty(restartText);
        EditorUtility.SetDirty(costText);
        EditorUtility.SetDirty(coinIcon);
        EditorUtility.SetDirty(restartButton.gameObject);
        EditorUtility.SetDirty(pause);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log(
            "[PauseRestartMigration] RestartButton now has RestartText + CostText + CoinIcon. " +
            "PauseManager wired. Cost=" + LivesEconomyConfig.RestartCoinCost);
        EditorUtility.DisplayDialog(
            "Pause Restart migration",
            "Done.\n\nRestartButton children:\n- RestartText (RESTART)\n- CostText (25)\n- CoinIcon\n\n" +
            "PauseManager refs wired. You can now Preview Pause and manually position them.",
            "OK");
    }

    private static TextMeshProUGUI EnsureRestartText(Button restartButton)
    {
        TextMeshProUGUI existing =
            FindChildTmp(restartButton.transform, "RestartText") ??
            FindChildTmp(restartButton.transform, "Text (TMP)") ??
            restartButton.GetComponentInChildren<TextMeshProUGUI>(true);

        if (existing == null)
        {
            GameObject go = new GameObject("RestartText", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create RestartText");
            go.layer = restartButton.gameObject.layer;
            go.transform.SetParent(restartButton.transform, false);
            existing = go.AddComponent<TextMeshProUGUI>();
            existing.fontSize = 40f;
            existing.alignment = TextAlignmentOptions.Center;
            existing.color = Color.white;
            existing.raycastTarget = false;
        }

        existing.gameObject.name = "RestartText";
        existing.text = "RESTART";
        existing.raycastTarget = false;

        RectTransform rt = existing.rectTransform;
        // Left / center of the button row — manually authorable afterwards.
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(-48f, 0f);
        rt.sizeDelta = new Vector2(160f, 50f);
        rt.localScale = Vector3.one;
        existing.alignment = TextAlignmentOptions.Center;
        existing.enableAutoSizing = false;

        return existing;
    }

    private static TextMeshProUGUI EnsureCostText(Button restartButton, TextMeshProUGUI restartText)
    {
        TextMeshProUGUI existing = FindChildTmp(restartButton.transform, "CostText");
        if (existing == null)
        {
            GameObject go = new GameObject("CostText", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create CostText");
            go.layer = restartButton.gameObject.layer;
            go.transform.SetParent(restartButton.transform, false);
            existing = go.AddComponent<TextMeshProUGUI>();

            if (restartText != null)
            {
                existing.font = restartText.font;
                existing.fontSharedMaterial = restartText.fontSharedMaterial;
                existing.fontSize = restartText.fontSize;
                existing.fontStyle = restartText.fontStyle;
                existing.color = restartText.color;
            }
            else
            {
                existing.fontSize = 40f;
                existing.color = Color.white;
            }

            existing.alignment = TextAlignmentOptions.Center;
            existing.raycastTarget = false;
        }

        existing.gameObject.name = "CostText";
        existing.raycastTarget = false;

        RectTransform rt = existing.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(42f, 0f);
        rt.sizeDelta = new Vector2(56f, 50f);
        rt.localScale = Vector3.one;
        existing.alignment = TextAlignmentOptions.Center;
        existing.enableAutoSizing = false;

        return existing;
    }

    private static Image EnsureCoinIcon(Button restartButton)
    {
        Transform existingTf = restartButton.transform.Find("CoinIcon");
        Image existing = existingTf != null ? existingTf.GetComponent<Image>() : null;
        if (existing == null)
        {
            GameObject go = new GameObject("CoinIcon", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create CoinIcon");
            go.layer = restartButton.gameObject.layer;
            go.transform.SetParent(restartButton.transform, false);
            existing = go.AddComponent<Image>();
        }

        existing.gameObject.name = "CoinIcon";
        existing.raycastTarget = false;
        existing.preserveAspect = true;

        Sprite coin = AssetDatabase.LoadAssetAtPath<Sprite>(CoinSpritePath);
        if (coin == null)
        {
            // Multi-sprite sheet fallback (Icon_Coin_0).
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(CoinSpritePath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite)
                {
                    coin = sprite;
                    break;
                }
            }
        }

        if (coin != null)
        {
            existing.sprite = coin;
        }
        else
        {
            Debug.LogWarning(
                "[PauseRestartMigration] Could not load coin sprite at " + CoinSpritePath);
        }

        RectTransform rt = existing.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(88f, 0f);
        rt.sizeDelta = new Vector2(40f, 40f);
        rt.localScale = Vector3.one;

        return existing;
    }

    private static TextMeshProUGUI FindChildTmp(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }

        Transform child = root.Find(name);
        if (child == null)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform c = root.GetChild(i);
                if (c != null && c.name == name)
                {
                    child = c;
                    break;
                }
            }
        }

        return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
    }
}
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Serializable RectTransform snapshot for authored gameplay layouts.
/// </summary>
[Serializable]
public struct RectTransformState
{
    [Tooltip("Stable key, e.g. TopHUD or NoTouchHudRoot/Mission Label")]
    public string key;

    [Tooltip("Parent key: SafeArea, TopHUD, TabletHUDPanel, TabletControlsRow, ...")]
    public string parentKey;

    public int siblingIndex;
    public Vector2 anchorMin;
    public Vector2 anchorMax;
    public Vector2 pivot;
    public Vector2 anchoredPosition;
    public Vector2 sizeDelta;
    public Vector3 localScale;
    public Vector3 localEulerAngles;

    public bool hasHorizontalLayout;
    public bool horizontalLayoutEnabled;
    public TextAnchor childAlignment;
    public float spacing;
    public bool childForceExpandWidth;
    public bool childForceExpandHeight;
    public bool childControlWidth;
    public bool childControlHeight;

    public static RectTransformState From(
        string key,
        string parentKey,
        RectTransform rt)
    {
        RectTransformState s = new RectTransformState
        {
            key = key,
            parentKey = parentKey ?? string.Empty,
            siblingIndex = rt != null ? rt.GetSiblingIndex() : 0,
            anchorMin = rt != null ? rt.anchorMin : Vector2.zero,
            anchorMax = rt != null ? rt.anchorMax : Vector2.one,
            pivot = rt != null ? rt.pivot : new Vector2(0.5f, 0.5f),
            anchoredPosition = rt != null ? rt.anchoredPosition : Vector2.zero,
            sizeDelta = rt != null ? rt.sizeDelta : Vector2.zero,
            localScale = rt != null ? rt.localScale : Vector3.one,
            localEulerAngles = rt != null ? rt.localEulerAngles : Vector3.zero
        };

        if (rt != null)
        {
            HorizontalLayoutGroup hlg = rt.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                s.hasHorizontalLayout = true;
                s.horizontalLayoutEnabled = hlg.enabled;
                s.childAlignment = hlg.childAlignment;
                s.spacing = hlg.spacing;
                s.childForceExpandWidth = hlg.childForceExpandWidth;
                s.childForceExpandHeight = hlg.childForceExpandHeight;
                s.childControlWidth = hlg.childControlWidth;
                s.childControlHeight = hlg.childControlHeight;
            }
        }

        return s;
    }

    /// <summary>
    /// Presentation only. Never SetActive — objective/gameplay controllers own visibility.
    /// </summary>
    public void ApplyTo(RectTransform rt)
    {
        if (rt == null)
        {
            return;
        }

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = sizeDelta;
        rt.localScale = localScale;
        rt.localEulerAngles = localEulerAngles;

        if (hasHorizontalLayout)
        {
            HorizontalLayoutGroup hlg = rt.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                hlg.enabled = horizontalLayoutEnabled;
                hlg.childAlignment = childAlignment;
                hlg.spacing = spacing;
                hlg.childForceExpandWidth = childForceExpandWidth;
                hlg.childForceExpandHeight = childForceExpandHeight;
                hlg.childControlWidth = childControlWidth;
                hlg.childControlHeight = childControlHeight;
            }
        }
    }
}

/// <summary>
/// One authored layout mode (Tall / Compact / Tablet).
/// When hasCapture is true, runtime uses these transforms instead of heuristics.
/// </summary>
[Serializable]
public class GameplayLayoutProfile
{
    public bool hasCapture;

    [Tooltip("Camera top reserved fraction for this mode (0-1 of view height).")]
    [Range(0f, 0.45f)]
    public float topReservedFraction = 0.16f;

    [Tooltip("Camera bottom reserved fraction for this mode.")]
    [Range(0f, 0.45f)]
    public float bottomReservedFraction = 0.16f;

    [Tooltip("Tablet only: left HUD width fraction.")]
    [Range(0.15f, 0.45f)]
    public float leftHudWidthFraction = 0.30f;

    public List<RectTransformState> entries = new List<RectTransformState>(32);

    public bool TryGet(string key, out RectTransformState state)
    {
        state = default;
        if (string.IsNullOrEmpty(key) || entries == null)
        {
            return false;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].key == key)
            {
                state = entries[i];
                return true;
            }
        }

        return false;
    }

    public void Set(RectTransformState state)
    {
        if (entries == null)
        {
            entries = new List<RectTransformState>(32);
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].key == state.key)
            {
                entries[i] = state;
                return;
            }
        }

        entries.Add(state);
    }
}

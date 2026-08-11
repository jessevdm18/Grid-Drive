using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CSS background-size: cover voor een UI Image.
/// Vult de container altijd volledig; crop is ok; sprite-aspect blijft behouden.
/// Schaal hangt alleen af van container + sprite aspect — niet van camera/grid.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class UIBackgroundCover : MonoBehaviour
{
    [Tooltip("Image die gecoverd wordt. Default: Image op dit object.")]
    [SerializeField] private Image image;

    [Tooltip("Container die gevuld moet worden. Default: parent RectTransform.")]
    [SerializeField] private RectTransform container;

    private RectTransform rectTransform;
    private Vector2 lastContainerSize;
    private Sprite lastSprite;

    private void Awake()
    {
        CacheRefs();
        ApplyCover();
    }

    private void OnEnable()
    {
        CacheRefs();
        ApplyCover();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyCover();
    }

#if UNITY_EDITOR
    private void Update()
    {
        // Editor preview bij resize / sprite swap zonder play mode.
        if (!Application.isPlaying)
        {
            ApplyCover();
        }
    }
#endif

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (NeedsRefresh())
        {
            ApplyCover();
        }
    }

    public void ApplyCover()
    {
        CacheRefs();

        if (rectTransform == null || container == null || image == null)
        {
            return;
        }

        Sprite sprite = image.sprite;
        if (sprite == null)
        {
            return;
        }

        Rect containerRect = container.rect;
        float containerWidth = containerRect.width;
        float containerHeight = containerRect.height;
        if (containerWidth <= 0.01f || containerHeight <= 0.01f)
        {
            return;
        }

        float spriteWidth = sprite.rect.width;
        float spriteHeight = sprite.rect.height;
        if (spriteWidth <= 0.01f || spriteHeight <= 0.01f)
        {
            return;
        }

        float containerAspect = containerWidth / containerHeight;
        float spriteAspect = spriteWidth / spriteHeight;

        float targetWidth;
        float targetHeight;

        if (containerAspect > spriteAspect)
        {
            // Container breder: breedte vult, hoogte loopt over.
            targetWidth = containerWidth;
            targetHeight = containerWidth / spriteAspect;
        }
        else
        {
            // Container taller/smaller: hoogte vult, breedte loopt over.
            targetHeight = containerHeight;
            targetWidth = containerHeight * spriteAspect;
        }

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
        rectTransform.localScale = Vector3.one;

        lastContainerSize = new Vector2(containerWidth, containerHeight);
        lastSprite = sprite;
    }

    private bool NeedsRefresh()
    {
        if (container == null || image == null)
        {
            return true;
        }

        Rect r = container.rect;
        if (!Mathf.Approximately(r.width, lastContainerSize.x) ||
            !Mathf.Approximately(r.height, lastContainerSize.y))
        {
            return true;
        }

        return image.sprite != lastSprite;
    }

    private void CacheRefs()
    {
        if (rectTransform == null)
        {
            rectTransform = transform as RectTransform;
        }

        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (container == null && transform.parent != null)
        {
            container = transform.parent as RectTransform;
        }

        if (container == null)
        {
            container = rectTransform;
        }
    }
}

using UnityEngine;

/// <summary>
/// Soft contact-shadow under a vehicle.
/// Copies sprite + flip from CarSprite, keeps a fixed world light offset
/// (shadowOffset is never mirrored when the car flips).
/// </summary>
public class VehicleShadow : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Usually the CarSprite SpriteRenderer.")]
    [SerializeField] private SpriteRenderer sourceSpriteRenderer;

    [Tooltip("SpriteRenderer on this Shadow object.")]
    [SerializeField] private SpriteRenderer shadowSpriteRenderer;

    [Header("Shadow Look")]
    [Tooltip("Fixed light-direction offset in Visual-local space. Not mirrored on flip.")]
    [SerializeField] private Vector2 shadowOffset = new Vector2(0.06f, -0.08f);

    [SerializeField, Range(0f, 1f)]
    private float shadowAlpha = 0.25f;

    [Tooltip("Uniform scale multiplier relative to CarSprite localScale.")]
    [SerializeField] private float shadowScale = 1.03f;

    [Tooltip("Added to the source sortingOrder (usually -1 = behind the car).")]
    [SerializeField] private int sortingOrderOffset = -1;

    private void Awake()
    {
        ResolveReferencesIfNeeded();
        UpdateShadow();
    }

    private void OnEnable()
    {
        ResolveReferencesIfNeeded();
        UpdateShadow();
    }

    /// <summary>
    /// Syncs the shadow to the current CarSprite (sprite, flip, rotation, scale).
    /// Call again after the car sprite or facing changes.
    /// </summary>
    public void UpdateShadow()
    {
        ResolveReferencesIfNeeded();

        if (sourceSpriteRenderer == null || shadowSpriteRenderer == null)
        {
            return;
        }

        Transform sourceTransform = sourceSpriteRenderer.transform;
        Transform shadowTransform = shadowSpriteRenderer.transform;

        // Same silhouette as the car (including mirrored facing).
        shadowSpriteRenderer.sprite = sourceSpriteRenderer.sprite;
        shadowSpriteRenderer.flipX = sourceSpriteRenderer.flipX;
        shadowSpriteRenderer.flipY = sourceSpriteRenderer.flipY;

        // Soft dark contact shadow.
        shadowSpriteRenderer.color = new Color(0f, 0f, 0f, shadowAlpha);

        // Draw just behind the car on the same sorting layer.
        shadowSpriteRenderer.sortingLayerID = sourceSpriteRenderer.sortingLayerID;
        shadowSpriteRenderer.sortingOrder =
            sourceSpriteRenderer.sortingOrder + sortingOrderOffset;

        // Fixed light direction — do NOT mirror shadowOffset when flipped.
        shadowTransform.localPosition = new Vector3(shadowOffset.x, shadowOffset.y, 0f);

        // Match car orientation (horizontal identity / vertical 90°).
        shadowTransform.localRotation = sourceTransform.localRotation;

        // Same uniform visual scale as CarSprite, slightly larger for a soft edge.
        float sourceScale = sourceTransform.localScale.x;
        float finalScale = sourceScale * shadowScale;
        shadowTransform.localScale = new Vector3(finalScale, finalScale, 1f);
    }

    /// <summary>
    /// Fills missing Inspector refs from sibling CarSprite / own SpriteRenderer.
    /// </summary>
    private void ResolveReferencesIfNeeded()
    {
        if (shadowSpriteRenderer == null)
        {
            shadowSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (sourceSpriteRenderer == null)
        {
            Transform parent = transform.parent;
            if (parent != null)
            {
                Transform carSprite = parent.Find("CarSprite");
                if (carSprite != null)
                {
                    sourceSpriteRenderer = carSprite.GetComponent<SpriteRenderer>();
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-confetti voor Screen Space - Overlay (WinPanel / ConfettiLayer).
/// Burst omhoog → afremmen → vallen. Alleen UI Images + coroutines.
/// </summary>
public class UIWinConfetti : MonoBehaviour
{
    [Header("Layer & Sprites")]
    [SerializeField] private RectTransform confettiLayer;
    [SerializeField] private List<Sprite> confettiSprites = new List<Sprite>();

    [Header("Burst")]
    [SerializeField] private int confettiCount = 50;
    [SerializeField] private Vector2 burstOrigin = new Vector2(0f, -80f);
    [SerializeField] private float spawnRadius = 40f;

    [SerializeField] private float minLaunchSpeed = 500f;
    [SerializeField] private float maxLaunchSpeed = 850f;
    [SerializeField] private float minLaunchAngle = 55f;
    [SerializeField] private float maxLaunchAngle = 125f;

    [SerializeField] private float gravity = 900f;

    [Header("Lifetime & Size")]
    [SerializeField] private float minLifetime = 1.6f;
    [SerializeField] private float maxLifetime = 2.5f;
    [SerializeField] private float minSize = 20f;
    [SerializeField] private float maxSize = 45f;

    private readonly List<GameObject> activePieces = new List<GameObject>();
    private readonly List<Coroutine> activeCoroutines = new List<Coroutine>();
    private Coroutine spawnRoutine;
    private int spriteLogBudget;

    /// <summary>
    /// Ruimt oude pieces op en speelt een nieuwe celebratory burst.
    /// </summary>
    public void PlayConfetti()
    {
        StopConfetti();

        if (confettiLayer == null)
        {
            Debug.LogWarning("UIWinConfetti: confettiLayer is not assigned.");
            return;
        }

        if (confettiSprites == null || confettiSprites.Count == 0)
        {
            Debug.LogWarning("No confetti sprites assigned");
            return;
        }

        Debug.Log("UIWinConfetti burst started | sprites=" + confettiSprites.Count);

        spriteLogBudget = 5;
        spawnRoutine = StartCoroutine(SpawnConfettiRoutine());
    }

    /// <summary>
    /// Stopt animaties en vernietigt gegenereerde pieces.
    /// ConfettiLayer blijft bestaan.
    /// </summary>
    public void StopConfetti()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        for (int i = 0; i < activeCoroutines.Count; i++)
        {
            if (activeCoroutines[i] != null)
            {
                StopCoroutine(activeCoroutines[i]);
            }
        }

        activeCoroutines.Clear();

        for (int i = 0; i < activePieces.Count; i++)
        {
            if (activePieces[i] != null)
            {
                Destroy(activePieces[i]);
            }
        }

        activePieces.Clear();

        if (confettiLayer != null)
        {
            for (int i = confettiLayer.childCount - 1; i >= 0; i--)
            {
                Destroy(confettiLayer.GetChild(i).gameObject);
            }
        }
    }

    private IEnumerator SpawnConfettiRoutine()
    {
        int count = Mathf.Max(0, confettiCount);
        for (int i = 0; i < count; i++)
        {
            SpawnPiece();

            // Lichte stagger zodat niet alles exact tegelijk start.
            if (i % 5 == 4)
            {
                yield return null;
            }
        }

        spawnRoutine = null;
    }

    private void SpawnPiece()
    {
        Sprite selectedSprite = confettiSprites[Random.Range(0, confettiSprites.Count)];
        if (selectedSprite == null)
        {
            return;
        }

        if (spriteLogBudget > 0)
        {
            Debug.Log("Confetti sprite: " + selectedSprite.name);
            spriteLogBudget--;
        }

        GameObject piece = new GameObject(
            "ConfettiPiece",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        RectTransform rt = piece.GetComponent<RectTransform>();
        rt.SetParent(confettiLayer, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // Uniform size + preserveAspect behoudt de sprite-verhouding.
        float size = Random.Range(minSize, maxSize);
        rt.sizeDelta = new Vector2(size, size);

        Vector2 spawnOffset = Random.insideUnitCircle * spawnRadius;
        Vector2 startPos = burstOrigin + spawnOffset;
        rt.anchoredPosition = startPos;
        rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        rt.localScale = Vector3.one;

        Image image = piece.GetComponent<Image>();
        image.sprite = selectedSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = Color.white;

        float launchSpeed = Random.Range(minLaunchSpeed, maxLaunchSpeed);
        float launchAngle = Random.Range(minLaunchAngle, maxLaunchAngle);
        float angleRadians = launchAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
        Vector2 velocity = direction * launchSpeed;

        float lifetime = Random.Range(minLifetime, maxLifetime);
        float rotationSpeed = Random.Range(-360f, 360f);
        float wobbleAmplitude = Random.Range(8f, 22f);
        float wobbleFrequency = Random.Range(1.5f, 3.5f);
        float wobblePhase = Random.Range(0f, Mathf.PI * 2f);

        activePieces.Add(piece);
        Coroutine routine = StartCoroutine(
            AnimateBurstPieceRoutine(
                rt,
                image,
                velocity,
                lifetime,
                rotationSpeed,
                wobbleAmplitude,
                wobbleFrequency,
                wobblePhase
            )
        );
        activeCoroutines.Add(routine);
    }

    private IEnumerator AnimateBurstPieceRoutine(
        RectTransform rt,
        Image image,
        Vector2 velocity,
        float lifetime,
        float rotationSpeed,
        float wobbleAmplitude,
        float wobbleFrequency,
        float wobblePhase)
    {
        float elapsed = 0f;
        Vector2 position = rt.anchoredPosition;
        float rotationZ = rt.localEulerAngles.z;
        Color color = image.color;

        while (elapsed < lifetime && rt != null)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;

            velocity.y -= gravity * dt;
            position += velocity * dt;

            float wobble =
                Mathf.Sin((elapsed * wobbleFrequency) + wobblePhase) * wobbleAmplitude;
            rt.anchoredPosition = new Vector2(position.x + wobble, position.y);

            rotationZ += rotationSpeed * dt;
            rt.localRotation = Quaternion.Euler(0f, 0f, rotationZ);

            // Fade in de laatste 25% van de lifetime.
            float u = Mathf.Clamp01(elapsed / lifetime);
            float alpha = u < 0.75f ? 1f : Mathf.Lerp(1f, 0f, (u - 0.75f) / 0.25f);
            color.a = alpha;
            image.color = color;

            yield return null;
        }

        if (rt != null)
        {
            activePieces.Remove(rt.gameObject);
            Destroy(rt.gameObject);
        }
    }

    private void OnDisable()
    {
        StopConfetti();
    }
}

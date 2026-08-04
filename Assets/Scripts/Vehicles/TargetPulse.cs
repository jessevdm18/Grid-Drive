using UnityEngine;

/// <summary>
/// Subtiele continue pulse op TargetIndicator (alleen localScale).
/// </summary>
public class TargetPulse : MonoBehaviour
{
    [SerializeField] private float pulseAmount = 0.08f;
    [SerializeField] private float pulseSpeed = 2f;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Sin gaat van -1 tot 1 → schaal van (1 - amount) tot (1 + amount).
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
    }
}

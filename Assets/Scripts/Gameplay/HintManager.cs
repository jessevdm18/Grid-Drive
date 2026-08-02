using System.Collections;
using UnityEngine;

/// <summary>
/// Eenvoudige hint: kost coins en highlight een voertuig dat kan bewegen.
/// </summary>
public class HintManager : MonoBehaviour
{
    [SerializeField] private CoinManager coinManager;

    [SerializeField] private int hintCost = 100;

    [Tooltip("Hoe lang de highlight zichtbaar blijft (seconden).")]
    [SerializeField] private float highlightDuration = 1.5f;

    [Tooltip("Kleur tijdens de hint-highlight.")]
    [SerializeField] private Color highlightColor = Color.yellow;

    // Voorkomt dat meerdere hints tegelijk lopen.
    private bool isHighlighting;

    /// <summary>
    /// Knop-callback: probeer een hint te kopen en te tonen.
    /// </summary>
    public void UseHint()
    {
        if (isHighlighting)
        {
            Debug.Log("HintManager: hint is al bezig.");
            return;
        }

        if (coinManager == null)
        {
            Debug.LogError("HintManager: geen CoinManager gekoppeld.");
            return;
        }

        // Probeer 100 coins af te trekken.
        if (!coinManager.SpendCoins(hintCost))
        {
            Debug.Log("Not enough coins");
            return;
        }

        VehicleController vehicle = FindMovableVehicle();

        if (vehicle == null)
        {
            Debug.Log("HintManager: geen voertuig gevonden dat kan bewegen.");
            // Coins zijn al afgeschreven — bij een latere versie kun je refunden.
            return;
        }

        StartCoroutine(HighlightVehicle(vehicle));
    }

    /// <summary>
    /// Zoekt de eerste actieve auto die minstens één geldige stap kan maken.
    /// </summary>
    private VehicleController FindMovableVehicle()
    {
        VehicleController[] vehicles = FindObjectsByType<VehicleController>(
            FindObjectsSortMode.None
        );

        foreach (VehicleController vehicle in vehicles)
        {
            if (vehicle != null && vehicle.gameObject.activeInHierarchy && vehicle.CanMakeAnyMove())
            {
                return vehicle;
            }
        }

        return null;
    }

    /// <summary>
    /// Verandert tijdelijk de SpriteRenderer-kleur en zet daarna terug.
    /// </summary>
    private IEnumerator HighlightVehicle(VehicleController vehicle)
    {
        isHighlighting = true;

        SpriteRenderer spriteRenderer = vehicle.GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning("HintManager: geen SpriteRenderer op " + vehicle.name);
            isHighlighting = false;
            yield break;
        }

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = highlightColor;

        yield return new WaitForSeconds(highlightDuration);

        // Auto kan ondertussen van het bord zijn (exit) — veilig terugzetten.
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        isHighlighting = false;
    }
}

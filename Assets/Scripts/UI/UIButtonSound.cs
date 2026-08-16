using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Speelt de standaard UI button-SFX bij PointerDown (niet bij onClick/release),
/// zodat het geluid synchroon voelt met de press-animatie.
/// Button.onClick blijft ongemoeid voor de echte actie.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("Uit zetten voor knoppen met eigen success-SFX (bijv. Hint).")]
    [SerializeField] private bool playDefaultClickSound = true;

    private Button button;
    private AudioManager audioManager;

    private void Awake()
    {
        button = GetComponent<Button>();
        audioManager = FindAnyObjectByType<AudioManager>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!playDefaultClickSound)
        {
            return;
        }

        if (!isActiveAndEnabled)
        {
            return;
        }

        if (button == null || !button.interactable)
        {
            return;
        }

        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        audioManager?.PlayButton();
    }
}

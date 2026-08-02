using UnityEngine;

public class ExitZone : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Iets heeft de Exit geraakt: " + other.gameObject.name);
        
        VehicleController vehicle =
            other.GetComponent<VehicleController>();

        if (vehicle == null)
            return;

        // Exit wordt uitsluitend afgehandeld via VehicleController.TryExitRight tijdens slepen.
        // Triggers mogen ExitBoard niet rechtstreeks aanroepen.
    }
}
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void CheckWinCondition()
    {
        VehicleController[] vehicles =
            FindObjectsByType<VehicleController>(
                FindObjectsSortMode.None
            );

        if (vehicles.Length == 0)
        {
            WinLevel();
        }
    }

    private void WinLevel()
    {
        Debug.Log("LEVEL COMPLETED!");
    }
}
using UnityEngine;

/// <summary>
/// Slaat levelprogressie op met PlayerPrefs.
/// Zet dit script op een GameObject in je scene (bijv. "SaveManager").
/// </summary>
public class SaveManager : MonoBehaviour
{
    // Duidelijke keys voor PlayerPrefs (geen typos tussen Save en Get).
    private const string UnlockedLevelKey = "RushOut_UnlockedLevel";
    private const string CurrentLevelKey = "RushOut_CurrentLevel";

    /// <summary>
    /// Slaat de hoogste unlocked level-index op.
    /// </summary>
    public void SaveUnlockedLevel(int levelIndex)
    {
        PlayerPrefs.SetInt(UnlockedLevelKey, levelIndex);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Slaat het laatst gespeelde level op.
    /// </summary>
    public void SaveCurrentLevel(int levelIndex)
    {
        PlayerPrefs.SetInt(CurrentLevelKey, levelIndex);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Geeft de hoogste unlocked level-index terug.
    /// Default = 0 (eerste level).
    /// </summary>
    public int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt(UnlockedLevelKey, 0);
    }

    /// <summary>
    /// Geeft het laatst gespeelde level terug.
    /// Default = 0 (eerste level).
    /// </summary>
    public int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt(CurrentLevelKey, 0);
    }

    /// <summary>
    /// Wis alle opgeslagen progressie.
    /// </summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(UnlockedLevelKey);
        PlayerPrefs.DeleteKey(CurrentLevelKey);
        PlayerPrefs.Save();
    }
}

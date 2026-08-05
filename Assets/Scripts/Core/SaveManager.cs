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
    private const string StarsKeyPrefix = "LevelStars_";
    private const string StarsMaxIndexKey = "LevelStars_MaxIndex";

    /// <summary>
    /// Slaat de hoogste unlocked level-index op.
    /// Zet progressie NOOIT terug — alleen een hogere index wordt bewaard.
    /// </summary>
    public void SaveUnlockedLevel(int levelIndex)
    {
        int current = GetUnlockedLevel();
        if (levelIndex <= current)
        {
            return;
        }

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
    /// Repareert unlocked progress op basis van bestaande sterren.
    /// Verwijdert geen PlayerPrefs; zet unlocked alleen omhoog indien nodig.
    /// </summary>
    public void RepairUnlockedProgress(int totalLevels)
    {
        if (totalLevels <= 0)
        {
            return;
        }

        int maxValidIndex = totalLevels - 1;
        int oldUnlocked = Mathf.Clamp(GetUnlockedLevel(), 0, maxValidIndex);

        // Hoogste level met sterren > 0 geldt als voltooid.
        int highestCompleted = -1;
        int starScanLimit = Mathf.Max(
            PlayerPrefs.GetInt(StarsMaxIndexKey, -1),
            maxValidIndex
        );

        for (int i = 0; i <= starScanLimit; i++)
        {
            if (i > maxValidIndex)
            {
                break;
            }

            if (GetStarsForLevel(i) > 0)
            {
                highestCompleted = i;
            }
        }

        // Voltooid level X ⇒ minstens X+1 unlocked (geclamped).
        int reconstructed = highestCompleted >= 0
            ? highestCompleted + 1
            : 0;
        reconstructed = Mathf.Clamp(reconstructed, 0, maxValidIndex);

        int repaired = Mathf.Max(oldUnlocked, reconstructed);
        repaired = Mathf.Clamp(repaired, 0, maxValidIndex);

        Debug.Log(
            "Progress repair | old unlocked=" + oldUnlocked +
            " | repaired unlocked=" + repaired
        );

        if (repaired > oldUnlocked)
        {
            PlayerPrefs.SetInt(UnlockedLevelKey, repaired);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Slaat sterren voor een level op. Bewaart alleen de hoogste score (0–3).
    /// </summary>
    public void SaveStarsForLevel(int levelIndex, int stars)
    {
        stars = Mathf.Clamp(stars, 0, 3);

        int existing = GetStarsForLevel(levelIndex);
        if (stars <= existing)
        {
            return;
        }

        PlayerPrefs.SetInt(StarsKeyPrefix + levelIndex, stars);

        // Bijhouden tot welk index we sterren hebben, voor ResetProgress.
        int maxIndex = PlayerPrefs.GetInt(StarsMaxIndexKey, -1);
        if (levelIndex > maxIndex)
        {
            PlayerPrefs.SetInt(StarsMaxIndexKey, levelIndex);
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Geeft opgeslagen sterren voor een level (default 0 als nooit gespeeld).
    /// </summary>
    public int GetStarsForLevel(int levelIndex)
    {
        return Mathf.Clamp(
            PlayerPrefs.GetInt(StarsKeyPrefix + levelIndex, 0),
            0,
            3
        );
    }

    /// <summary>
    /// Wis alle opgeslagen progressie.
    /// </summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(UnlockedLevelKey);
        PlayerPrefs.DeleteKey(CurrentLevelKey);

        // Verwijder alle LevelStars_X keys die we ooit hebben aangemaakt.
        int maxIndex = PlayerPrefs.GetInt(StarsMaxIndexKey, -1);
        for (int i = 0; i <= maxIndex; i++)
        {
            PlayerPrefs.DeleteKey(StarsKeyPrefix + i);
        }

        PlayerPrefs.DeleteKey(StarsMaxIndexKey);
        PlayerPrefs.Save();
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centrale lijst van alle LevelData-assets.
/// Maak aan via: Create → RushOut → Level Database.
/// </summary>
[CreateAssetMenu(
    fileName = "LevelDatabase",
    menuName = "RushOut/Level Database"
)]
public class LevelDatabase : ScriptableObject
{
    [Tooltip("Alle levels in volgorde (index 0 = eerste level).")]
    public List<LevelData> levels = new List<LevelData>();

    /// <summary>
    /// Aantal levels in de database.
    /// </summary>
    public int LevelCount => levels != null ? levels.Count : 0;

    /// <summary>
    /// Geeft het LevelData op de opgegeven index.
    /// Retourneert null als de index ongeldig is.
    /// </summary>
    public LevelData GetLevel(int index)
    {
        if (levels == null || index < 0 || index >= levels.Count)
        {
            return null;
        }

        return levels[index];
    }

    /// <summary>
    /// Levels met de gegeven difficulty, in database-volgorde (geen herschikking).
    /// </summary>
    public List<LevelData> GetLevelsByDifficulty(LevelDifficulty difficulty)
    {
        List<LevelData> result = new List<LevelData>();
        if (levels == null)
        {
            return result;
        }

        for (int i = 0; i < levels.Count; i++)
        {
            LevelData level = levels[i];
            if (level != null && level.difficulty == difficulty)
            {
                result.Add(level);
            }
        }

        return result;
    }

    /// <summary>
    /// Database-indices van levels met de gegeven difficulty, in database-volgorde.
    /// </summary>
    public List<int> GetLevelIndicesByDifficulty(LevelDifficulty difficulty)
    {
        List<int> result = new List<int>();
        if (levels == null)
        {
            return result;
        }

        for (int i = 0; i < levels.Count; i++)
        {
            LevelData level = levels[i];
            if (level != null && level.difficulty == difficulty)
            {
                result.Add(i);
            }
        }

        return result;
    }

    /// <summary>
    /// Volgende index ná <paramref name="fromIndex"/> met dezelfde difficulty, of -1.
    /// </summary>
    public int FindNextLevelIndexSameDifficulty(int fromIndex)
    {
        LevelData current = GetLevel(fromIndex);
        if (current == null || levels == null)
        {
            return -1;
        }

        LevelDifficulty tier = current.difficulty;
        for (int i = fromIndex + 1; i < levels.Count; i++)
        {
            LevelData level = levels[i];
            if (level != null && level.difficulty == tier)
            {
                return i;
            }
        }

        return -1;
    }
}

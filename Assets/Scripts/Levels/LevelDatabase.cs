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
}

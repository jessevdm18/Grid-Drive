using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Authoritative difficulty-local ordering for LevelSelect, display numbers, and Next Level.
/// Does NOT reorder MainLevelDatabase — view/progression order only.
/// Order: minimumMoves ASC → difficultyScore ASC → database index ASC.
/// </summary>
public static class LevelDifficultyOrder
{
    /// <summary>
    /// Database indices for <paramref name="difficulty"/> in progression order.
    /// </summary>
    public static List<int> GetOrderedLevelIndicesForDifficulty(
        LevelDatabase database,
        LevelDifficulty difficulty)
    {
        List<int> indices = new List<int>();
        if (database == null || database.levels == null)
        {
            return indices;
        }

        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level != null && level.difficulty == difficulty)
            {
                indices.Add(i);
            }
        }

        indices.Sort((a, b) => CompareIndices(database, a, b));
        return indices;
    }

    /// <summary>
    /// 1-based local display number within the level's difficulty tier.
    /// Returns 0 if index invalid / not found.
    /// </summary>
    public static int GetDifficultyDisplayNumber(LevelDatabase database, int databaseIndex)
    {
        if (database == null)
        {
            return 0;
        }

        LevelData level = database.GetLevel(databaseIndex);
        if (level == null)
        {
            return 0;
        }

        List<int> ordered = GetOrderedLevelIndicesForDifficulty(database, level.difficulty);
        for (int i = 0; i < ordered.Count; i++)
        {
            if (ordered[i] == databaseIndex)
            {
                return i + 1;
            }
        }

        return 0;
    }

    /// <summary>
    /// Database index for 1-based local display number within difficulty, or -1.
    /// </summary>
    public static int GetDatabaseIndexForDifficultyDisplayNumber(
        LevelDatabase database,
        LevelDifficulty difficulty,
        int displayNumber)
    {
        if (database == null || displayNumber <= 0)
        {
            return -1;
        }

        List<int> ordered = GetOrderedLevelIndicesForDifficulty(database, difficulty);
        int index = displayNumber - 1;
        if (index < 0 || index >= ordered.Count)
        {
            return -1;
        }

        return ordered[index];
    }

    /// <summary>
    /// Next database index in ordered same-difficulty list after <paramref name="fromIndex"/>, or -1.
    /// </summary>
    public static int FindNextOrderedLevelIndexSameDifficulty(
        LevelDatabase database,
        int fromIndex)
    {
        if (database == null)
        {
            return -1;
        }

        LevelData current = database.GetLevel(fromIndex);
        if (current == null)
        {
            return -1;
        }

        List<int> ordered = GetOrderedLevelIndicesForDifficulty(database, current.difficulty);
        for (int i = 0; i < ordered.Count; i++)
        {
            if (ordered[i] != fromIndex)
            {
                continue;
            }

            if (i + 1 < ordered.Count)
            {
                return ordered[i + 1];
            }

            return -1;
        }

        return -1;
    }

    public static int CompareIndices(LevelDatabase database, int indexA, int indexB)
    {
        LevelData a = database != null ? database.GetLevel(indexA) : null;
        LevelData b = database != null ? database.GetLevel(indexB) : null;
        return CompareLevels(a, b, indexA, indexB);
    }

    public static int CompareLevels(LevelData a, LevelData b, int indexA, int indexB)
    {
        int movesA = LevelMinMoves.SortKey(a != null ? a.minimumMoves : 0);
        int movesB = LevelMinMoves.SortKey(b != null ? b.minimumMoves : 0);
        int movesCmp = movesA.CompareTo(movesB);
        if (movesCmp != 0)
        {
            return movesCmp;
        }

        int scoreA = a != null ? a.difficultyScore : 0;
        int scoreB = b != null ? b.difficultyScore : 0;
        int scoreCmp = scoreA.CompareTo(scoreB);
        if (scoreCmp != 0)
        {
            return scoreCmp;
        }

        return indexA.CompareTo(indexB);
    }

    /// <summary>
    /// First database index in difficulty-local order, or -1 if none.
    /// </summary>
    public static int GetFirstOrderedLevelIndex(
        LevelDatabase database,
        LevelDifficulty difficulty)
    {
        List<int> ordered = GetOrderedLevelIndicesForDifficulty(database, difficulty);
        if (ordered == null || ordered.Count == 0)
        {
            return -1;
        }

        return ordered[0];
    }
}

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only: herschik MainLevelDatabase op eenvoudige difficulty-curve.
/// Preview schrijft niets. Apply herschikt alleen list-order + levelNumber.
/// Asset-bestandsnamen blijven ongemoeid. Runtime/save-architectuur ongewijzigd.
///
/// Ranking (makkelijk → moeilijk):
/// 1) board area (gridW * gridH) — 5x5 vóór 6x6
/// 2) minimumMoves (lager eerst; ontbrekend/≤0 achteraan)
/// 3) difficultyScore (tie-break; negatief achteraan)
/// 4) asset name
///
/// Bestaande LevelSolver "Analyze And Sort" blijft beschikbaar (re-solve + score).
/// </summary>
public static class MainLevelDatabaseDifficultyOrderUtility
{
    private const string DatabaseAssetFilter = "MainLevelDatabase t:LevelDatabase";

    public struct OrderRow
    {
        public int currentIndex;   // 0-based in huidige database
        public int proposedIndex;  // 0-based na sort
        public LevelData level;
        public string assetName;
        public int gridWidth;
        public int gridHeight;
        public int boardArea;
        public int minimumMoves;
        public int difficultyScore;
        public LevelObjectiveType objectiveType;
        public int currentLevelNumber;
        public int proposedLevelNumber;
    }

    /// <summary>
    /// Vergelijking voor oplopende difficulty. Herbruikbaar door Preview/Apply.
    /// </summary>
    public static int CompareDifficultyOrder(LevelData a, LevelData b)
    {
        bool aNull = a == null;
        bool bNull = b == null;
        if (aNull && bNull)
        {
            return 0;
        }

        if (aNull)
        {
            return 1;
        }

        if (bNull)
        {
            return -1;
        }

        int areaA = GetBoardArea(a);
        int areaB = GetBoardArea(b);
        int areaCmp = areaA.CompareTo(areaB);
        if (areaCmp != 0)
        {
            return areaCmp;
        }

        int movesA = NormalizeMovesForSort(a.minimumMoves);
        int movesB = NormalizeMovesForSort(b.minimumMoves);
        int movesCmp = movesA.CompareTo(movesB);
        if (movesCmp != 0)
        {
            return movesCmp;
        }

        bool aBadScore = a.difficultyScore < 0;
        bool bBadScore = b.difficultyScore < 0;
        if (aBadScore && !bBadScore)
        {
            return 1;
        }

        if (!aBadScore && bBadScore)
        {
            return -1;
        }

        if (!aBadScore && !bBadScore)
        {
            int scoreCmp = a.difficultyScore.CompareTo(b.difficultyScore);
            if (scoreCmp != 0)
            {
                return scoreCmp;
            }
        }

        string nameA = a.name ?? string.Empty;
        string nameB = b.name ?? string.Empty;
        return string.CompareOrdinal(nameA, nameB);
    }

    public static List<OrderRow> BuildPreview()
    {
        List<OrderRow> rows = new List<OrderRow>();
        LevelDatabase database = LoadMainLevelDatabase(logErrors: false);
        if (database == null || database.levels == null)
        {
            return rows;
        }

        List<(int index, LevelData level)> indexed = new List<(int, LevelData)>();
        for (int i = 0; i < database.levels.Count; i++)
        {
            indexed.Add((i, database.levels[i]));
        }

        indexed.Sort((x, y) => CompareDifficultyOrder(x.level, y.level));

        for (int proposed = 0; proposed < indexed.Count; proposed++)
        {
            int current = indexed[proposed].index;
            LevelData level = indexed[proposed].level;
            OrderRow row = new OrderRow
            {
                currentIndex = current,
                proposedIndex = proposed,
                level = level,
                assetName = level != null ? level.name : "(null)",
                gridWidth = level != null ? level.ResolvedGridWidth : 0,
                gridHeight = level != null ? level.ResolvedGridHeight : 0,
                boardArea = level != null ? GetBoardArea(level) : 0,
                minimumMoves = level != null ? level.minimumMoves : 0,
                difficultyScore = level != null ? level.difficultyScore : -1,
                objectiveType = level != null
                    ? level.objectiveType
                    : LevelObjectiveType.Classic,
                currentLevelNumber = level != null ? level.levelNumber : 0,
                proposedLevelNumber = proposed + 1
            };
            rows.Add(row);
        }

        return rows;
    }

    public static string FormatPreviewLog(List<OrderRow> rows)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== Preview Difficulty Order (geen writes) ===");
        sb.AppendLine(
            "Ranking: boardArea → minimumMoves → difficultyScore → name"
        );
        sb.AppendLine(
            "Current# | Proposed# | Asset | Grid | MinMoves | Score | Objective"
        );

        if (rows == null || rows.Count == 0)
        {
            sb.AppendLine("(geen levels)");
            return sb.ToString();
        }

        for (int i = 0; i < rows.Count; i++)
        {
            OrderRow row = rows[i];
            sb.AppendLine(
                "#" + (row.currentIndex + 1) +
                " → #" + (row.proposedIndex + 1) +
                " | " + row.assetName +
                " | " + row.gridWidth + "x" + row.gridHeight +
                " | " + row.minimumMoves +
                " | " + row.difficultyScore +
                " | " + row.objectiveType
            );
        }

        return sb.ToString();
    }

    /// <summary>
    /// Herschikt database.levels + zet levelNumber = index+1.
    /// Wijzigt geen asset-bestandsnamen, geen save-keys, geen solver.
    /// </summary>
    public static bool ApplyDifficultyOrder()
    {
        LevelDatabase database = LoadMainLevelDatabase(logErrors: true);
        if (database == null)
        {
            return false;
        }

        if (database.levels == null || database.levels.Count == 0)
        {
            Debug.LogWarning(
                "DifficultyOrder: MainLevelDatabase heeft geen levels."
            );
            return false;
        }

        List<OrderRow> preview = BuildPreview();
        Debug.Log(FormatPreviewLog(preview));

        Undo.RecordObject(database, "Apply Difficulty Order");
        for (int i = 0; i < database.levels.Count; i++)
        {
            if (database.levels[i] != null)
            {
                Undo.RecordObject(database.levels[i], "Apply Difficulty Order");
            }
        }

        database.levels.Sort(CompareDifficultyOrder);

        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level == null)
            {
                continue;
            }

            level.levelNumber = i + 1;
            EditorUtility.SetDirty(level);
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("=== Apply Difficulty Order ===");
        summary.AppendLine(
            "Herschikt " + database.levels.Count +
            " levels. Assetnamen ongewijzigd. levelNumber = 1..N."
        );
        summary.AppendLine(
            "LET OP: PlayerPrefs (Unlocked/Current/Stars) zijn INDEX-gebaseerd — " +
            "bestaande test-save koppelt nu aan andere content. Special objectives " +
            "blijven op assets tot Apply Special Mission Progression."
        );

        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level == null)
            {
                continue;
            }

            summary.AppendLine(
                "#" + (i + 1) +
                " levelNumber=" + level.levelNumber +
                " | " + level.name +
                " | " + level.ResolvedGridWidth + "x" + level.ResolvedGridHeight +
                " | moves=" + level.minimumMoves +
                " | obj=" + level.objectiveType
            );
        }

        Debug.Log(summary.ToString());
        return true;
    }

    private static int GetBoardArea(LevelData level)
    {
        return level.ResolvedGridWidth * level.ResolvedGridHeight;
    }

    private static int NormalizeMovesForSort(int minimumMoves)
    {
        return minimumMoves > 0 ? minimumMoves : int.MaxValue;
    }

    public static LevelDatabase LoadMainLevelDatabase(bool logErrors)
    {
        string[] guids = AssetDatabase.FindAssets(DatabaseAssetFilter);
        if (guids == null || guids.Length == 0)
        {
            if (logErrors)
            {
                Debug.LogError("DifficultyOrder: geen MainLevelDatabase gevonden.");
            }

            return null;
        }

        if (guids.Length > 1)
        {
            if (logErrors)
            {
                Debug.LogError(
                    "DifficultyOrder: meerdere MainLevelDatabase assets (" +
                    guids.Length + ")."
                );
            }

            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);
    }

    [MenuItem("RushOut/Preview Difficulty Order")]
    public static void MenuPreview()
    {
        List<OrderRow> rows = BuildPreview();
        Debug.Log(FormatPreviewLog(rows));
    }

    [MenuItem("RushOut/Apply Difficulty Order")]
    public static void MenuApply()
    {
        if (!EditorUtility.DisplayDialog(
                "Apply Difficulty Order",
                "Herschikt MainLevelDatabase:\n" +
                "• board area (5x5 vóór 6x6)\n" +
                "• daarna minimumMoves\n" +
                "• levelNumber = 1..N\n" +
                "• assetnamen blijven gelijk\n\n" +
                "Save/test progression is INDEX-gebaseerd en zal niet meer " +
                "matchen met dezelfde content.\n\n" +
                "Daarna: Apply Special Mission Progression.\n\nDoorgaan?",
                "Apply",
                "Cancel"))
        {
            return;
        }

        ApplyDifficultyOrder();
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only Rush Hour-achtige level solver + validator.
/// BFS-zoekwerk delegeert naar de gedeelde runtime RushOutSolver.
/// Menu:
///   RushOut → Validate Selected Level
///   RushOut → Validate All Levels In Database
///   RushOut → Analyze And Sort Levels By Difficulty
///   RushOut → Generated Levels → Validate All
///   RushOut → Generated Levels → Analyze And Sort By Difficulty
///   RushOut → Generated Levels → Rename By Difficulty
/// </summary>
public static class LevelSolver
{
    private const int MaxStates = 100000;
    private const string GeneratedLevelsFolder = "Assets/Data/GeneratedLevels";

    // -------------------------------------------------------------------------
    // Publieke resultaat-types
    // -------------------------------------------------------------------------

    public class Move
    {
        public string vehicleName;
        public Vector2Int fromPosition;
        public Vector2Int toPosition;
        public bool exitsBoard;

        public Move(string vehicleName, Vector2Int from, Vector2Int to, bool exitsBoard)
        {
            this.vehicleName = vehicleName;
            this.fromPosition = from;
            this.toPosition = to;
            this.exitsBoard = exitsBoard;
        }
    }

    public class SolverResult
    {
        public bool solvable;
        public bool searchLimitReached;
        public bool invalid;
        public string validationError;
        public int minimumMoves;
        public List<Move> solution = new List<Move>();
        public int statesExplored;
        public int levelNumber;
        public int difficultyScore;

        // Doorgestuurd van RushOutSolver hot-path profiling
        public int generatedMoves;
        public int visitedPrecheckRejects;
        public int occupancyBuildCount;
        public int childStatesCreated;
        public int childStatesEnqueued;
        public int discoveredStates;
        public int queuePeakSize;
        public string searchLimitReason;
        public double totalOccupancyBuildMs;
        public double totalMoveGenerationMs;
        public double totalVisitedMs;
        public double totalStateCopyMs;
    }

    // -------------------------------------------------------------------------
    // Menu — MainLevelDatabase
    // -------------------------------------------------------------------------

    [MenuItem("RushOut/Validate Selected Level")]
    public static void ValidateSelectedLevel()
    {
        LevelData levelData = Selection.activeObject as LevelData;
        if (levelData == null)
        {
            Debug.LogError(
                "LevelSolver: selecteer eerst een LevelData asset in het Project-venster."
            );
            return;
        }

        SolverResult result = Solve(levelData);
        ApplyMetadata(levelData, result);
        AssetDatabase.SaveAssets();

        LogSingleResult(result);
        Debug.Log(FormatMetadataLine(levelData));
    }

    [MenuItem("RushOut/Validate All Levels In Database")]
    public static void ValidateAllLevelsInDatabase()
    {
        LevelDatabase database = LoadMainLevelDatabase();
        if (database == null)
        {
            return;
        }

        StringBuilder summary = AnalyzeAllLevels(database, sortAndRenumber: false);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(summary.ToString());
    }

    [MenuItem("RushOut/Analyze And Sort Levels By Difficulty")]
    public static void AnalyzeAndSortLevelsByDifficulty()
    {
        LevelDatabase database = LoadMainLevelDatabase();
        if (database == null)
        {
            return;
        }

        StringBuilder summary = AnalyzeAllLevels(database, sortAndRenumber: true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(summary.ToString());
    }

    // -------------------------------------------------------------------------
    // Menu — Generated Levels (niet MainLevelDatabase)
    // -------------------------------------------------------------------------

    [MenuItem("RushOut/Generated Levels/Validate All")]
    public static void ValidateAllGeneratedLevels()
    {
        List<LevelData> levels = LoadGeneratedLevels();
        if (levels == null)
        {
            return;
        }

        StringBuilder summary = AnalyzeGeneratedLevels(levels, printSortedList: false);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(summary.ToString());
    }

    [MenuItem("RushOut/Generated Levels/Analyze And Sort By Difficulty")]
    public static void AnalyzeGeneratedLevelsByDifficulty()
    {
        List<LevelData> levels = LoadGeneratedLevels();
        if (levels == null)
        {
            return;
        }

        // Alleen analyseren + Console-lijst. Geen database-wijziging, geen rename.
        StringBuilder summary = AnalyzeGeneratedLevels(levels, printSortedList: true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(summary.ToString());
    }

    [MenuItem("RushOut/Generated Levels/Rename By Difficulty")]
    public static void RenameGeneratedLevelsByDifficulty()
    {
        List<LevelData> levels = LoadGeneratedLevels();
        if (levels == null)
        {
            return;
        }

        bool confirmed = EditorUtility.DisplayDialog(
            "Rename Generated Levels",
            "Dit hernoemt bestanden in " + GeneratedLevelsFolder +
            " op difficultyScore (makkelijk → moeilijk).\n\n" +
            "MainLevelDatabase wordt niet gewijzigd.\n\nDoorgaan?",
            "Rename",
            "Cancel"
        );

        if (!confirmed)
        {
            Debug.Log("LevelSolver: rename geannuleerd.");
            return;
        }

        // Eerst metadata verversen.
        AnalyzeGeneratedLevels(levels, printSortedList: false);

        List<LevelData> sorted = SortLevelsByDifficultyCopy(levels);
        RenameGeneratedLevelAssets(sorted);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "LevelSolver: " + sorted.Count +
            " generated levels hernoemd op difficultyScore → " + GeneratedLevelsFolder
        );
    }

    /// <summary>
    /// Analyseert alle levels, schrijft metadata, optioneel sorteren + hernummeren.
    /// </summary>
    private static StringBuilder AnalyzeAllLevels(LevelDatabase database, bool sortAndRenumber)
    {
        int valid = 0;
        int unsolvable = 0;
        int invalid = 0;
        int limitReached = 0;

        StringBuilder summary = new StringBuilder();
        summary.AppendLine(
            sortAndRenumber
                ? "=== LevelSolver: Analyze And Sort By Difficulty ==="
                : "=== LevelSolver: Validate All Levels ==="
        );

        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level == null)
            {
                invalid++;
                summary.AppendLine("[" + i + "] NULL entry → invalid");
                continue;
            }

            SolverResult result = Solve(level);
            ApplyMetadata(level, result);
            CountResult(result, ref valid, ref unsolvable, ref invalid, ref limitReached);
            AppendResultLine(summary, level, result);
        }

        if (sortAndRenumber)
        {
            SortDatabaseByDifficulty(database);
            RenumberLevels(database);
            summary.AppendLine("--- Sorted by difficultyScore (unsolvable/invalid last) ---");
            for (int i = 0; i < database.levels.Count; i++)
            {
                LevelData level = database.levels[i];
                if (level == null)
                {
                    continue;
                }

                summary.AppendLine(
                    "Order " + (i + 1) + " / levelNumber=" + level.levelNumber +
                    " → " + FormatMetadataLine(level)
                );
            }
        }

        AppendCountFooter(summary, valid, unsolvable, invalid, limitReached, database.levels.Count);
        return summary;
    }

    /// <summary>
    /// Analyseert GeneratedLevels-map. Raakt MainLevelDatabase niet aan.
    /// </summary>
    private static StringBuilder AnalyzeGeneratedLevels(List<LevelData> levels, bool printSortedList)
    {
        int valid = 0;
        int unsolvable = 0;
        int invalid = 0;
        int limitReached = 0;

        StringBuilder summary = new StringBuilder();
        summary.AppendLine(
            printSortedList
                ? "=== Generated Levels: Analyze And Sort By Difficulty ==="
                : "=== Generated Levels: Validate All ==="
        );
        summary.AppendLine("folder: " + GeneratedLevelsFolder);

        foreach (LevelData level in levels)
        {
            if (level == null)
            {
                invalid++;
                summary.AppendLine("NULL entry → invalid");
                continue;
            }

            SolverResult result = Solve(level);
            ApplyMetadata(level, result);
            CountResult(result, ref valid, ref unsolvable, ref invalid, ref limitReached);
            AppendResultLine(summary, level, result);
        }

        if (printSortedList)
        {
            List<LevelData> sorted = SortLevelsByDifficultyCopy(levels);
            summary.AppendLine();
            summary.AppendLine("Generated levels sorted by difficulty:");
            summary.AppendLine();

            int rank = 1;
            foreach (LevelData level in sorted)
            {
                if (level == null)
                {
                    continue;
                }

                string name = GetLevelAssetName(level);
                summary.AppendLine(
                    rank + ". " + name +
                    " | moves=" + level.minimumMoves +
                    " | states=" + level.statesExplored +
                    " | score=" + level.difficultyScore
                );
                rank++;
            }
        }

        AppendCountFooter(summary, valid, unsolvable, invalid, limitReached, levels.Count);
        return summary;
    }

    private static void CountResult(
        SolverResult result,
        ref int valid,
        ref int unsolvable,
        ref int invalid,
        ref int limitReached)
    {
        if (result.invalid)
        {
            invalid++;
        }
        else if (result.searchLimitReached)
        {
            limitReached++;
        }
        else if (result.solvable)
        {
            valid++;
        }
        else
        {
            unsolvable++;
        }
    }

    private static void AppendResultLine(StringBuilder summary, LevelData level, SolverResult result)
    {
        if (result.invalid)
        {
            summary.AppendLine(
                FormatMetadataLine(level) + " → INVALID: " + result.validationError
            );
        }
        else if (result.searchLimitReached)
        {
            summary.AppendLine(FormatMetadataLine(level) + " → Search limit reached");
        }
        else if (result.solvable)
        {
            summary.AppendLine(FormatMetadataLine(level) + " → solvable");
        }
        else
        {
            summary.AppendLine(FormatMetadataLine(level) + " → UNSOLVABLE");
        }
    }

    private static void AppendCountFooter(
        StringBuilder summary,
        int valid,
        int unsolvable,
        int invalid,
        int limitReached,
        int total)
    {
        summary.AppendLine("---");
        summary.AppendLine("total generated/listed: " + total);
        summary.AppendLine("valid (solvable): " + valid);
        summary.AppendLine("unsolvable: " + unsolvable);
        summary.AppendLine("invalid: " + invalid);
        summary.AppendLine("search limit reached: " + limitReached);
    }

    private static List<LevelData> LoadGeneratedLevels()
    {
        if (!AssetDatabase.IsValidFolder(GeneratedLevelsFolder))
        {
            Debug.LogWarning(
                "LevelSolver: map niet gevonden: " + GeneratedLevelsFolder +
                ". Genereer eerst levels via RushOut → Generate Levels."
            );
            return null;
        }

        string[] guids = AssetDatabase.FindAssets(
            "t:LevelData",
            new[] { GeneratedLevelsFolder }
        );

        if (guids == null || guids.Length == 0)
        {
            Debug.LogWarning(
                "LevelSolver: geen LevelData gevonden in " + GeneratedLevelsFolder
            );
            return null;
        }

        List<LevelData> levels = new List<LevelData>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (level != null)
            {
                levels.Add(level);
            }
        }

        // Stabiele startvolgorde op bestandsnaam.
        levels.Sort((a, b) =>
            string.Compare(GetLevelAssetName(a), GetLevelAssetName(b), StringComparison.Ordinal)
        );

        return levels;
    }

    private static List<LevelData> SortLevelsByDifficultyCopy(List<LevelData> levels)
    {
        List<LevelData> sorted = new List<LevelData>(levels);
        sorted.Sort(CompareDifficulty);
        return sorted;
    }

    private static int CompareDifficulty(LevelData a, LevelData b)
    {
        bool aBad = a == null || a.difficultyScore < 0;
        bool bBad = b == null || b.difficultyScore < 0;

        if (aBad && !bBad)
        {
            return 1;
        }

        if (!aBad && bBad)
        {
            return -1;
        }

        if (aBad && bBad)
        {
            return 0;
        }

        int scoreCompare = a.difficultyScore.CompareTo(b.difficultyScore);
        if (scoreCompare != 0)
        {
            return scoreCompare;
        }

        return a.minimumMoves.CompareTo(b.minimumMoves);
    }

    /// <summary>
    /// Hernoemt generated assets naar Generated_Level_001, _002, ... op difficulty.
    /// Twee-fasen rename voorkomt naamconflicten. Wijzigt MainLevelDatabase niet.
    /// </summary>
    private static void RenameGeneratedLevelAssets(List<LevelData> sortedByDifficulty)
    {
        // Fase 1: tijdelijke namen.
        for (int i = 0; i < sortedByDifficulty.Count; i++)
        {
            LevelData level = sortedByDifficulty[i];
            if (level == null)
            {
                continue;
            }

            string path = AssetDatabase.GetAssetPath(level);
            string error = AssetDatabase.RenameAsset(path, "_tmp_gen_" + i.ToString("000"));
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError("Rename (temp) mislukt voor " + path + ": " + error);
            }
        }

        AssetDatabase.SaveAssets();

        // Fase 2: definitieve namen + levelNumber.
        for (int i = 0; i < sortedByDifficulty.Count; i++)
        {
            LevelData level = sortedByDifficulty[i];
            if (level == null)
            {
                continue;
            }

            string path = AssetDatabase.GetAssetPath(level);
            string finalName = "Generated_Level_" + (i + 1).ToString("000");
            string error = AssetDatabase.RenameAsset(path, finalName);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError("Rename (final) mislukt voor " + path + ": " + error);
                continue;
            }

            level.levelNumber = i + 1;
            EditorUtility.SetDirty(level);
        }
    }

    private static string GetLevelAssetName(LevelData levelData)
    {
        string path = AssetDatabase.GetAssetPath(levelData);
        if (!string.IsNullOrEmpty(path))
        {
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }

        return levelData != null ? levelData.name : "null";
    }

    private static LevelDatabase LoadMainLevelDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("MainLevelDatabase t:LevelDatabase");

        if (guids == null || guids.Length == 0)
        {
            Debug.LogError("LevelSolver: geen MainLevelDatabase gevonden.");
            return null;
        }

        if (guids.Length > 1)
        {
            Debug.LogError(
                "LevelSolver: meerdere MainLevelDatabase assets gevonden (" +
                guids.Length + "). Gebruik precies één database."
            );
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        LevelDatabase database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);

        if (database == null || database.levels == null)
        {
            Debug.LogError("LevelSolver: MainLevelDatabase kon niet geladen worden.");
            return null;
        }

        return database;
    }

    // -------------------------------------------------------------------------
    // Metadata
    // -------------------------------------------------------------------------

    /// <summary>
    /// Schrijft solver-resultaat terug naar LevelData en markeert dirty.
    /// Never writes minimumMoves=-1 over a previously valid value on timeout/unsolvable
    /// (avoids corrupting PAR / Easy#1 ordering). Only successful solves update minMoves.
    /// </summary>
    public static void ApplyMetadata(LevelData levelData, SolverResult result)
    {
        if (levelData == null || result == null)
        {
            return;
        }

        int vehicleCount = levelData.vehicles != null ? levelData.vehicles.Count : 0;
        int previousMinMoves = levelData.minimumMoves;

        if (result.invalid)
        {
            // Layout/config invalid — do not invent a par; keep previous if valid.
            if (!LevelMinMoves.IsValid(previousMinMoves))
            {
                levelData.minimumMoves = -1;
            }

            levelData.statesExplored = 0;
            levelData.difficultyScore = -1;
            result.difficultyScore = -1;
        }
        else if (result.solvable &&
                 !result.searchLimitReached &&
                 LevelMinMoves.IsValid(result.minimumMoves))
        {
            levelData.minimumMoves = result.minimumMoves;
            levelData.statesExplored = result.statesExplored;
            levelData.difficultyScore = ComputeDifficultyScore(
                result.minimumMoves,
                result.statesExplored,
                vehicleCount
            );
            result.difficultyScore = levelData.difficultyScore;
        }
        else
        {
            // Unsolvable or search limit / zero-length solution: preserve existing PAR.
            levelData.statesExplored = result.statesExplored;
            if (!LevelMinMoves.IsValid(previousMinMoves))
            {
                levelData.minimumMoves = -1;
                levelData.difficultyScore = -1;
            }

            result.difficultyScore = levelData.difficultyScore;
            Debug.LogWarning(
                "LevelSolver.ApplyMetadata: did not overwrite minimumMoves for " +
                levelData.name + " (solvable=" + result.solvable +
                ", searchLimit=" + result.searchLimitReached +
                ", resultMoves=" + result.minimumMoves +
                ", kept=" + levelData.minimumMoves + ")."
            );
        }

        EditorUtility.SetDirty(levelData);
    }

    private static int ComputeDifficultyScore(
        int minimumMoves,
        int statesExplored,
        int vehicleCount)
    {
        return minimumMoves * 100
            + Mathf.RoundToInt(Mathf.Log10(statesExplored + 1) * 50f)
            + vehicleCount * 10;
    }

    /// <summary>
    /// Sorteert oplopend op difficultyScore. Negatieve scores (ongeldig/onoplosbaar) achteraan.
    /// Past geen asset-bestandsnamen aan.
    /// </summary>
    private static void SortDatabaseByDifficulty(LevelDatabase database)
    {
        database.levels.Sort((a, b) =>
        {
            bool aBad = a == null || a.difficultyScore < 0;
            bool bBad = b == null || b.difficultyScore < 0;

            if (aBad && !bBad)
            {
                return 1;
            }

            if (!aBad && bBad)
            {
                return -1;
            }

            if (aBad && bBad)
            {
                return 0;
            }

            int scoreCompare = a.difficultyScore.CompareTo(b.difficultyScore);
            if (scoreCompare != 0)
            {
                return scoreCompare;
            }

            // Tie-breaker: minder moves eerst.
            return a.minimumMoves.CompareTo(b.minimumMoves);
        });

        EditorUtility.SetDirty(database);
    }

    /// <summary>
    /// Zet levelNumber opnieuw op 1, 2, 3, ... volgens databasevolgorde.
    /// </summary>
    private static void RenumberLevels(LevelDatabase database)
    {
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
    }

    private static string FormatMetadataLine(LevelData levelData)
    {
        string assetName = levelData.name;
        string path = AssetDatabase.GetAssetPath(levelData);
        if (!string.IsNullOrEmpty(path))
        {
            assetName = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        return assetName +
               ": minimumMoves=" + levelData.minimumMoves +
               ", states=" + levelData.statesExplored +
               ", score=" + levelData.difficultyScore;
    }

    // -------------------------------------------------------------------------
    // Publieke solve-entry (delegeert BFS naar RushOutSolver)
    // -------------------------------------------------------------------------

    public static SolverResult Solve(LevelData levelData)
    {
        return Solve(levelData, MaxStates);
    }

    /// <summary>
    /// Solve met configureerbare BFS state-caps (explored + discovered).
    /// maxDiscoveredStates &lt;= 0 → gelijk aan maxStates.
    /// </summary>
    public static SolverResult Solve(LevelData levelData, int maxStates)
    {
        return Solve(levelData, maxStates, maxDiscoveredStates: maxStates);
    }

    public static SolverResult Solve(LevelData levelData, int maxStates, int maxDiscoveredStates)
    {
        SolverResult result = new SolverResult();
        result.levelNumber = levelData != null ? levelData.levelNumber : -1;

        string validationError = ValidateLevelData(levelData);
        if (validationError != null)
        {
            result.invalid = true;
            result.validationError = validationError;
            return result;
        }

        int cappedStates = Mathf.Max(1, maxStates);
        int cappedDiscovered = maxDiscoveredStates > 0
            ? maxDiscoveredStates
            : cappedStates;
        RushOutSolver.SolverResult core = RushOutSolver.SolveLevelData(
            levelData,
            cappedStates,
            cappedDiscovered
        );
        return MapCoreResult(core, result);
    }

    private static SolverResult MapCoreResult(
        RushOutSolver.SolverResult core,
        SolverResult result)
    {
        result.solvable = core.solvable;
        result.searchLimitReached = core.searchLimitReached;
        result.searchLimitReason = core.searchLimitReason;
        result.minimumMoves = core.minimumMoves;
        result.statesExplored = core.statesExplored;
        result.generatedMoves = core.generatedMoves;
        result.visitedPrecheckRejects = core.visitedPrecheckRejects;
        result.occupancyBuildCount = core.occupancyBuildCount;
        result.childStatesCreated = core.childStatesCreated;
        result.childStatesEnqueued = core.childStatesEnqueued;
        result.discoveredStates = core.discoveredStates;
        result.queuePeakSize = core.queuePeakSize;
        result.totalOccupancyBuildMs = core.totalOccupancyBuildMs;
        result.totalMoveGenerationMs = core.totalMoveGenerationMs;
        result.totalVisitedMs = core.totalVisitedMs;
        result.totalStateCopyMs = core.totalStateCopyMs;
        result.solution = new List<Move>();

        if (core.solution != null)
        {
            foreach (RushOutSolver.SolverMove move in core.solution)
            {
                result.solution.Add(new Move(
                    move.vehicleName,
                    move.fromPosition,
                    move.toPosition,
                    move.exitsBoard
                ));
            }
        }

        return result;
    }

    // -------------------------------------------------------------------------
    // Validatie (basisregels — Editor wrapper; BFS zit in RushOutSolver)
    // -------------------------------------------------------------------------

    public static string ValidateLevelData(LevelData levelData)
    {
        if (levelData == null)
        {
            return "LevelData is null.";
        }

        int gridWidth = levelData.ResolvedGridWidth;
        int gridHeight = levelData.ResolvedGridHeight;

        if (gridWidth < LevelData.MinGridSize || gridWidth > LevelData.MaxGridSize)
        {
            return "gridWidth moet tussen " + LevelData.MinGridSize +
                   " en " + LevelData.MaxGridSize +
                   " liggen (nu: " + gridWidth + ").";
        }

        if (gridHeight < LevelData.MinGridSize || gridHeight > LevelData.MaxGridSize)
        {
            return "gridHeight moet tussen " + LevelData.MinGridSize +
                   " en " + LevelData.MaxGridSize +
                   " liggen (nu: " + gridHeight + ").";
        }

        if (levelData.exitRow < 0 || levelData.exitRow >= gridHeight)
        {
            return "exitRow moet tussen 0 en " + (gridHeight - 1) +
                   " liggen (nu: " + levelData.exitRow + ").";
        }

        if (levelData.vehicles == null || levelData.vehicles.Count == 0)
        {
            return "geen voertuigen aanwezig.";
        }

        int targetCount = 0;
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

        for (int i = 0; i < levelData.vehicles.Count; i++)
        {
            VehicleData v = levelData.vehicles[i];
            string label = string.IsNullOrEmpty(v.vehicleName)
                ? ("voertuig index " + i)
                : v.vehicleName;

            if (v.lengthInCells < 2 || v.lengthInCells > 4)
            {
                return label + ": lengthInCells moet 2, 3 of 4 zijn.";
            }

            bool horizontal =
                v.orientation == VehicleController.VehicleOrientation.Horizontal;

            List<Vector2Int> cells = RushOutSolver.GetCells(
                v.gridPosition,
                horizontal,
                v.lengthInCells
            );

            foreach (Vector2Int cell in cells)
            {
                if (cell.x < 0 || cell.x >= gridWidth || cell.y < 0 || cell.y >= gridHeight)
                {
                    return label + ": staat (deels) buiten het " +
                           gridWidth + "x" + gridHeight + " grid.";
                }

                if (!occupied.Add(cell))
                {
                    return label + ": overlapt met een ander voertuig op " + cell + ".";
                }
            }

            if (v.canExitRight)
            {
                targetCount++;

                if (!horizontal)
                {
                    return label + ": target vehicle moet Horizontal zijn.";
                }

                if (v.gridPosition.y != levelData.exitRow)
                {
                    return label + ": target Y moet gelijk zijn aan exitRow.";
                }
            }
        }

        if (targetCount < 1)
        {
            return "minstens één voertuig moet canExitRight=true hebben (nu: " +
                   targetCount + ").";
        }

        if (levelData.objectiveType != LevelObjectiveType.MultiTargetRescue &&
            targetCount != 1)
        {
            return "Classic/special (niet MultiTarget): precies één canExitRight " +
                   "(nu: " + targetCount + ").";
        }

        return null;
    }

    // -------------------------------------------------------------------------
    // Logging
    // -------------------------------------------------------------------------

    private static void LogSingleResult(SolverResult result)
    {
        if (result.invalid)
        {
            Debug.LogError(
                "Level " + result.levelNumber +
                " is INVALID: " + result.validationError
            );
            return;
        }

        if (result.searchLimitReached)
        {
            Debug.LogWarning(
                "Level " + result.levelNumber +
                ": Search limit reached (" + (result.searchLimitReason ?? "unknown") +
                "). Explored " + result.statesExplored +
                " states, discovered " + result.discoveredStates + "."
            );
            return;
        }

        if (!result.solvable)
        {
            Debug.LogError("Level " + result.levelNumber + " is UNSOLVABLE");
            Debug.Log(
                "Level " + result.levelNumber +
                " explored " + result.statesExplored + " states."
            );
            return;
        }

        Debug.Log(
            "Level " + result.levelNumber +
            " solvable in " + result.minimumMoves +
            " moves. Explored " + result.statesExplored + " states."
        );
        Debug.Log(
            "Solver profile | occBuilds=" + result.occupancyBuildCount +
            " genMoves=" + result.generatedMoves +
            " precheckRejects=" + result.visitedPrecheckRejects +
            " childrenCreated=" + result.childStatesCreated +
            " childrenEnqueued=" + result.childStatesEnqueued +
            " discovered=" + result.discoveredStates +
            " queuePeak=" + result.queuePeakSize +
            " limitReason=" + (result.searchLimitReason ?? "-") +
            " | occMs=" + result.totalOccupancyBuildMs.ToString("0.00") +
            " moveMs=" + result.totalMoveGenerationMs.ToString("0.00") +
            " visMs=" + result.totalVisitedMs.ToString("0.00") +
            " copyMs=" + result.totalStateCopyMs.ToString("0.00")
        );

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Minimal solution for Level " + result.levelNumber + ":");

        int moveNumber = 1;
        foreach (Move move in result.solution)
        {
            if (move.exitsBoard)
            {
                sb.AppendLine("Final: " + move.vehicleName + " exits right");
            }
            else
            {
                sb.AppendLine(
                    "Move " + moveNumber + ": " + move.vehicleName +
                    " (" + move.fromPosition.x + "," + move.fromPosition.y + ") -> (" +
                    move.toPosition.x + "," + move.toPosition.y + ")"
                );
                moveNumber++;
            }
        }

        Debug.Log(sb.ToString());
    }
}

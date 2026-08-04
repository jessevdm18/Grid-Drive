using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only Rush Hour-achtige level solver + validator.
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
    private const int GridSize = 6;
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
    /// </summary>
    public static void ApplyMetadata(LevelData levelData, SolverResult result)
    {
        if (levelData == null || result == null)
        {
            return;
        }

        int vehicleCount = levelData.vehicles != null ? levelData.vehicles.Count : 0;

        if (result.invalid)
        {
            levelData.minimumMoves = -1;
            levelData.statesExplored = 0;
            levelData.difficultyScore = -1;
            result.difficultyScore = -1;
        }
        else if (result.solvable && !result.searchLimitReached)
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
            // Onoplosbaar of search limit.
            levelData.minimumMoves = -1;
            levelData.statesExplored = result.statesExplored;
            levelData.difficultyScore = -1;
            result.difficultyScore = -1;
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
    // Publieke solve-entry
    // -------------------------------------------------------------------------

    public static SolverResult Solve(LevelData levelData)
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

        // Bouw vaste voertuig-definities (veranderen niet tijdens search).
        SolverVehicle[] vehicles = BuildVehicles(levelData);
        int exitRow = levelData.exitRow;
        int targetIndex = FindTargetIndex(vehicles);

        // Startstate = alle startposities.
        PuzzleState start = new PuzzleState(ExtractPositions(levelData));

        return RunBfs(vehicles, exitRow, targetIndex, start, result);
    }

    // -------------------------------------------------------------------------
    // Validatie (basisregels, geen GameObjects)
    // -------------------------------------------------------------------------

    public static string ValidateLevelData(LevelData levelData)
    {
        if (levelData == null)
        {
            return "LevelData is null.";
        }

        if (levelData.exitRow < 0 || levelData.exitRow > 5)
        {
            return "exitRow moet tussen 0 en 5 liggen (nu: " + levelData.exitRow + ").";
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

            if (v.lengthInCells != 2 && v.lengthInCells != 3)
            {
                return label + ": lengthInCells moet 2 of 3 zijn.";
            }

            bool horizontal =
                v.orientation == VehicleController.VehicleOrientation.Horizontal;

            List<Vector2Int> cells = GetCells(v.gridPosition, horizontal, v.lengthInCells);
            foreach (Vector2Int cell in cells)
            {
                if (cell.x < 0 || cell.x >= GridSize || cell.y < 0 || cell.y >= GridSize)
                {
                    return label + ": staat (deels) buiten het 6x6 grid.";
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

        if (targetCount != 1)
        {
            return "precies één voertuig moet canExitRight=true hebben (nu: " +
                   targetCount + ").";
        }

        return null;
    }

    // -------------------------------------------------------------------------
    // BFS
    // -------------------------------------------------------------------------

    private static SolverResult RunBfs(
        SolverVehicle[] vehicles,
        int exitRow,
        int targetIndex,
        PuzzleState start,
        SolverResult result)
    {
        // Queue van states om te verkennen.
        Queue<PuzzleState> queue = new Queue<PuzzleState>();

        // Bezochte states (hashbaar via PuzzleState).
        HashSet<PuzzleState> visited = new HashSet<PuzzleState>();

        // Padreconstructie: state → (ouder-state, move die hierheen leidde).
        Dictionary<PuzzleState, BfsNode> cameFrom =
            new Dictionary<PuzzleState, BfsNode>();

        queue.Enqueue(start);
        visited.Add(start);
        cameFrom[start] = new BfsNode(null, null);

        int explored = 0;

        while (queue.Count > 0)
        {
            if (explored >= MaxStates)
            {
                result.searchLimitReached = true;
                result.statesExplored = explored;
                result.solvable = false;
                return result;
            }

            PuzzleState current = queue.Dequeue();
            explored++;

            // Genereer eerst de exit-move als die mogelijk is (winnende zet).
            Move exitMove = TryCreateExitMove(vehicles, exitRow, targetIndex, current);
            if (exitMove != null)
            {
                result.solvable = true;
                result.statesExplored = explored;
                result.solution = ReconstructPath(cameFrom, current, exitMove);
                result.minimumMoves = result.solution.Count;
                return result;
            }

            // Alle normale schuif-moves.
            List<MoveCandidate> candidates = GenerateMoveCandidates(vehicles, current);
            foreach (MoveCandidate candidate in candidates)
            {
                if (visited.Contains(candidate.nextState))
                {
                    continue;
                }

                visited.Add(candidate.nextState);
                cameFrom[candidate.nextState] = new BfsNode(current, candidate.move);
                queue.Enqueue(candidate.nextState);
            }
        }

        // Geen oplossing gevonden (volledig doorzocht).
        result.solvable = false;
        result.statesExplored = explored;
        return result;
    }

    private static List<Move> ReconstructPath(
        Dictionary<PuzzleState, BfsNode> cameFrom,
        PuzzleState endState,
        Move finalExitMove)
    {
        List<Move> path = new List<Move>();
        PuzzleState cursor = endState;

        while (cameFrom.TryGetValue(cursor, out BfsNode node) && node.parent != null)
        {
            path.Add(node.move);
            cursor = node.parent;
        }

        path.Reverse();
        path.Add(finalExitMove);
        return path;
    }

    // -------------------------------------------------------------------------
    // Move-generatie
    // -------------------------------------------------------------------------

    private static Move TryCreateExitMove(
        SolverVehicle[] vehicles,
        int exitRow,
        int targetIndex,
        PuzzleState state)
    {
        SolverVehicle target = vehicles[targetIndex];
        Vector2Int pos = state.positions[targetIndex];

        // Target moet op de exit-rij staan.
        if (pos.y != exitRow)
        {
            return null;
        }

        // Alle cellen rechts van de target tot de rand moeten vrij zijn.
        // Rightmost cel van de target = pos.x + length - 1.
        int rightMost = pos.x + target.length - 1;

        bool[,] occupied = BuildOccupancy(vehicles, state, ignoreIndex: targetIndex);

        for (int x = rightMost + 1; x < GridSize; x++)
        {
            if (occupied[x, pos.y])
            {
                return null;
            }
        }

        // Finale exit-move (positie blijft conceptueel de start; exitsBoard markeert win).
        return new Move(target.name, pos, pos, exitsBoard: true);
    }

    private static List<MoveCandidate> GenerateMoveCandidates(
        SolverVehicle[] vehicles,
        PuzzleState state)
    {
        List<MoveCandidate> candidates = new List<MoveCandidate>();

        for (int i = 0; i < vehicles.Length; i++)
        {
            SolverVehicle vehicle = vehicles[i];
            Vector2Int from = state.positions[i];
            bool[,] occupied = BuildOccupancy(vehicles, state, ignoreIndex: i);

            if (vehicle.horizontal)
            {
                // Links schuiven: elke bereikbare x is één aparte move.
                for (int steps = 1; ; steps++)
                {
                    int newX = from.x - steps;
                    if (newX < 0)
                    {
                        break;
                    }

                    // Nieuwe linker cel moet vrij zijn (stap voor stap).
                    int checkX = from.x - steps;
                    if (occupied[checkX, from.y])
                    {
                        break;
                    }

                    Vector2Int to = new Vector2Int(newX, from.y);
                    candidates.Add(CreateCandidate(vehicles, state, i, from, to, vehicle.name));
                }

                // Rechts schuiven.
                for (int steps = 1; ; steps++)
                {
                    int newX = from.x + steps;
                    int rightMost = newX + vehicle.length - 1;
                    if (rightMost >= GridSize)
                    {
                        break;
                    }

                    // Nieuwe rechter cel moet vrij zijn.
                    if (occupied[rightMost, from.y])
                    {
                        break;
                    }

                    Vector2Int to = new Vector2Int(newX, from.y);
                    candidates.Add(CreateCandidate(vehicles, state, i, from, to, vehicle.name));
                }
            }
            else
            {
                // Omlaag (y kleiner als bottom-left origin — hier y+ = omhoog in grid).
                // Vertical: gridPosition is onderste/linker cel; length gaat omhoog in y.
                for (int steps = 1; ; steps++)
                {
                    int newY = from.y - steps;
                    if (newY < 0)
                    {
                        break;
                    }

                    if (occupied[from.x, newY])
                    {
                        break;
                    }

                    Vector2Int to = new Vector2Int(from.x, newY);
                    candidates.Add(CreateCandidate(vehicles, state, i, from, to, vehicle.name));
                }

                for (int steps = 1; ; steps++)
                {
                    int newY = from.y + steps;
                    int topMost = newY + vehicle.length - 1;
                    if (topMost >= GridSize)
                    {
                        break;
                    }

                    if (occupied[from.x, topMost])
                    {
                        break;
                    }

                    Vector2Int to = new Vector2Int(from.x, newY);
                    candidates.Add(CreateCandidate(vehicles, state, i, from, to, vehicle.name));
                }
            }
        }

        return candidates;
    }

    private static MoveCandidate CreateCandidate(
        SolverVehicle[] vehicles,
        PuzzleState state,
        int vehicleIndex,
        Vector2Int from,
        Vector2Int to,
        string vehicleName)
    {
        PuzzleState next = state.WithMovedVehicle(vehicleIndex, to);
        Move move = new Move(vehicleName, from, to, exitsBoard: false);
        return new MoveCandidate(next, move);
    }

    // -------------------------------------------------------------------------
    // Occupancy helpers
    // -------------------------------------------------------------------------

    private static bool[,] BuildOccupancy(
        SolverVehicle[] vehicles,
        PuzzleState state,
        int ignoreIndex)
    {
        bool[,] occupied = new bool[GridSize, GridSize];

        for (int i = 0; i < vehicles.Length; i++)
        {
            if (i == ignoreIndex)
            {
                continue;
            }

            SolverVehicle v = vehicles[i];
            Vector2Int pos = state.positions[i];
            List<Vector2Int> cells = GetCells(pos, v.horizontal, v.length);

            foreach (Vector2Int cell in cells)
            {
                if (cell.x >= 0 && cell.x < GridSize && cell.y >= 0 && cell.y < GridSize)
                {
                    occupied[cell.x, cell.y] = true;
                }
            }
        }

        return occupied;
    }

    private static List<Vector2Int> GetCells(Vector2Int start, bool horizontal, int length)
    {
        List<Vector2Int> cells = new List<Vector2Int>(length);
        for (int i = 0; i < length; i++)
        {
            if (horizontal)
            {
                cells.Add(new Vector2Int(start.x + i, start.y));
            }
            else
            {
                cells.Add(new Vector2Int(start.x, start.y + i));
            }
        }

        return cells;
    }

    // -------------------------------------------------------------------------
    // LevelData → solver data
    // -------------------------------------------------------------------------

    private static SolverVehicle[] BuildVehicles(LevelData levelData)
    {
        SolverVehicle[] vehicles = new SolverVehicle[levelData.vehicles.Count];

        for (int i = 0; i < levelData.vehicles.Count; i++)
        {
            VehicleData v = levelData.vehicles[i];
            vehicles[i] = new SolverVehicle
            {
                name = string.IsNullOrEmpty(v.vehicleName) ? ("Vehicle_" + i) : v.vehicleName,
                horizontal = v.orientation == VehicleController.VehicleOrientation.Horizontal,
                length = v.lengthInCells,
                canExitRight = v.canExitRight
            };
        }

        return vehicles;
    }

    private static Vector2Int[] ExtractPositions(LevelData levelData)
    {
        Vector2Int[] positions = new Vector2Int[levelData.vehicles.Count];
        for (int i = 0; i < levelData.vehicles.Count; i++)
        {
            positions[i] = levelData.vehicles[i].gridPosition;
        }

        return positions;
    }

    private static int FindTargetIndex(SolverVehicle[] vehicles)
    {
        for (int i = 0; i < vehicles.Length; i++)
        {
            if (vehicles[i].canExitRight)
            {
                return i;
            }
        }

        return -1;
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
                ": Search limit reached (" + MaxStates +
                "). Explored " + result.statesExplored + " states."
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

    // -------------------------------------------------------------------------
    // Interne types
    // -------------------------------------------------------------------------

    private struct SolverVehicle
    {
        public string name;
        public bool horizontal;
        public int length;
        public bool canExitRight;
    }

    /// <summary>
    /// Immutable snapshot van alle voertuigposities.
    /// Hashable voor HashSet/Dictionary.
    /// </summary>
    private sealed class PuzzleState : IEquatable<PuzzleState>
    {
        public readonly Vector2Int[] positions;
        private readonly int cachedHash;

        public PuzzleState(Vector2Int[] positions)
        {
            this.positions = new Vector2Int[positions.Length];
            Array.Copy(positions, this.positions, positions.Length);
            cachedHash = ComputeHash(this.positions);
        }

        public PuzzleState WithMovedVehicle(int index, Vector2Int newPos)
        {
            Vector2Int[] copy = new Vector2Int[positions.Length];
            Array.Copy(positions, copy, positions.Length);
            copy[index] = newPos;
            return new PuzzleState(copy);
        }

        public bool Equals(PuzzleState other)
        {
            if (other == null || other.positions.Length != positions.Length)
            {
                return false;
            }

            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i] != other.positions[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PuzzleState);
        }

        public override int GetHashCode()
        {
            return cachedHash;
        }

        private static int ComputeHash(Vector2Int[] positions)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < positions.Length; i++)
                {
                    hash = hash * 31 + positions[i].x;
                    hash = hash * 31 + positions[i].y;
                }

                return hash;
            }
        }
    }

    private sealed class BfsNode
    {
        public readonly PuzzleState parent;
        public readonly Move move;

        public BfsNode(PuzzleState parent, Move move)
        {
            this.parent = parent;
            this.move = move;
        }
    }

    private sealed class MoveCandidate
    {
        public readonly PuzzleState nextState;
        public readonly Move move;

        public MoveCandidate(PuzzleState nextState, Move move)
        {
            this.nextState = nextState;
            this.move = move;
        }
    }
}

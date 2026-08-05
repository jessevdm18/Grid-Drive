using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only automatische level generator voor Rush Out.
/// Menu: RushOut → Generate Levels
///
/// Werkt puur met data (geen GameObjects). Gebruikt LevelSolver voor validatie.
/// Inclusief quality filters en Easy/Medium/Hard presets.
/// </summary>
public class LevelGeneratorWindow : EditorWindow
{
    public enum DifficultyPreset
    {
        Custom,
        Easy,
        Medium,
        Hard
    }

    private const int GridSize = 6;
    private const int TotalCells = GridSize * GridSize;
    private const string GeneratedRoot = "Assets/Data/GeneratedLevels";

    // --- Preset / batch ---
    private DifficultyPreset difficultyPreset = DifficultyPreset.Custom;
    private string batchName = "Custom_Batch_01";

    // --- Basisinstellingen ---
    private int numberOfLevels = 10;
    private int minVehicles = 4;
    private int maxVehicles = 10;
    private int minMinimumMoves = 3;
    private int maxMinimumMoves = 12;
    private int maxAttemptsPerLevel = 500;
    private int randomSeed = 12345;
    private string outputFolder = "Assets/Data/GeneratedLevels/Custom";

    // --- Quality filters ---
    private int minVehiclesUsedInSolution = 3;
    private int minDirectBlockers = 1;
    private float minBoardOccupancy = 0.25f;
    private float maxBoardOccupancy = 0.70f;
    private float minMovableRatio = 0.25f;
    private float maxMovableRatio = 0.85f;
    private float maxLongVehicleRatio = 0.40f;
    private float minSolutionVehicleRatio = 0.35f;

    private Vector2 scroll;

    [MenuItem("RushOut/Generate Levels")]
    public static void OpenWindow()
    {
        LevelGeneratorWindow window = GetWindow<LevelGeneratorWindow>("Generate Levels");
        window.minSize = new Vector2(440f, 560f);
        window.Show();
    }

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Rush Out — Level Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space(6f);

        // Preset bovenaan: vult waarden in, daarna nog steeds handmatig aanpasbaar.
        DifficultyPreset newPreset = (DifficultyPreset)EditorGUILayout.EnumPopup(
            "Difficulty Preset",
            difficultyPreset
        );
        if (newPreset != difficultyPreset)
        {
            difficultyPreset = newPreset;
            ApplyDifficultyPreset(difficultyPreset);
        }

        batchName = EditorGUILayout.TextField("Batch Name", batchName);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Basics", EditorStyles.boldLabel);
        numberOfLevels = EditorGUILayout.IntField("Number Of Levels To Generate", numberOfLevels);
        minVehicles = EditorGUILayout.IntField("Min Vehicles", minVehicles);
        maxVehicles = EditorGUILayout.IntField("Max Vehicles", maxVehicles);
        minMinimumMoves = EditorGUILayout.IntField("Min Minimum Moves", minMinimumMoves);
        maxMinimumMoves = EditorGUILayout.IntField("Max Minimum Moves", maxMinimumMoves);
        maxAttemptsPerLevel = EditorGUILayout.IntField("Max Attempts Per Level", maxAttemptsPerLevel);
        randomSeed = EditorGUILayout.IntField("Random Seed", randomSeed);

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Quality Filters", EditorStyles.boldLabel);
        minVehiclesUsedInSolution = EditorGUILayout.IntField(
            "Min Vehicles Used In Solution",
            minVehiclesUsedInSolution
        );
        minDirectBlockers = EditorGUILayout.IntField("Min Direct Blockers", minDirectBlockers);
        minBoardOccupancy = EditorGUILayout.Slider("Min Board Occupancy", minBoardOccupancy, 0f, 1f);
        maxBoardOccupancy = EditorGUILayout.Slider("Max Board Occupancy", maxBoardOccupancy, 0f, 1f);
        minMovableRatio = EditorGUILayout.Slider("Min Movable Ratio", minMovableRatio, 0f, 1f);
        maxMovableRatio = EditorGUILayout.Slider("Max Movable Ratio", maxMovableRatio, 0f, 1f);

        EditorGUILayout.Space(4f);
        EditorGUILayout.HelpBox(
            "Strengere 'almost solved'-check geldt alleen als Min Minimum Moves >= 4 " +
            "(tutorials met lagere min-moves blijven mogelijk).\n" +
            "Handmatige wijzigingen na een preset worden gewoon gebruikt.",
            MessageType.None
        );

        EditorGUILayout.Space(12f);
        EditorGUILayout.HelpBox(
            "Gegenereerde levels worden NIET aan MainLevelDatabase toegevoegd. " +
            "Review ze eerst in de output-map (inclusief Easy/Medium/Hard subfolders).",
            MessageType.Info
        );

        EditorGUILayout.Space(8f);
        if (GUILayout.Button("Generate Levels", GUILayout.Height(36f)))
        {
            GenerateLevels();
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Vult generator-instellingen + output folder + batch name voor een preset.
    /// </summary>
    private void ApplyDifficultyPreset(DifficultyPreset preset)
    {
        switch (preset)
        {
            case DifficultyPreset.Easy:
                minVehicles = 3;
                maxVehicles = 6;
                minMinimumMoves = 2;
                maxMinimumMoves = 5;
                minVehiclesUsedInSolution = 2;
                minDirectBlockers = 1;
                minBoardOccupancy = 0.20f;
                maxBoardOccupancy = 0.50f;
                minMovableRatio = 0.30f;
                maxMovableRatio = 0.90f;
                maxAttemptsPerLevel = 1000;
                batchName = "Easy_Batch_01";
                outputFolder = GeneratedRoot + "/Easy";
                break;

            case DifficultyPreset.Medium:
                minVehicles = 5;
                maxVehicles = 9;
                minMinimumMoves = 5;
                maxMinimumMoves = 9;
                minVehiclesUsedInSolution = 3;
                minDirectBlockers = 1;
                minBoardOccupancy = 0.30f;
                maxBoardOccupancy = 0.65f;
                minMovableRatio = 0.25f;
                maxMovableRatio = 0.80f;
                maxAttemptsPerLevel = 2000;
                batchName = "Medium_Batch_01";
                outputFolder = GeneratedRoot + "/Medium";
                break;

            case DifficultyPreset.Hard:
                minVehicles = 7;
                maxVehicles = 11;
                minMinimumMoves = 8;
                maxMinimumMoves = 14;
                minVehiclesUsedInSolution = 4;
                minDirectBlockers = 2;
                minBoardOccupancy = 0.40f;
                maxBoardOccupancy = 0.75f;
                minMovableRatio = 0.20f;
                maxMovableRatio = 0.70f;
                maxAttemptsPerLevel = 5000;
                batchName = "Hard_Batch_01";
                outputFolder = GeneratedRoot + "/Hard";
                break;

            case DifficultyPreset.Custom:
            default:
                batchName = "Custom_Batch_01";
                outputFolder = GeneratedRoot + "/Custom";
                break;
        }
    }

    /// <summary>
    /// LevelDifficulty-tier die op gegenereerde assets wordt gezet.
    /// Custom → Medium als neutrale default.
    /// </summary>
    private LevelDifficulty GetLevelDifficultyTier()
    {
        switch (difficultyPreset)
        {
            case DifficultyPreset.Easy:
                return LevelDifficulty.Easy;
            case DifficultyPreset.Hard:
                return LevelDifficulty.Hard;
            case DifficultyPreset.Medium:
            case DifficultyPreset.Custom:
            default:
                return LevelDifficulty.Medium;
        }
    }

    private static string SanitizeBatchName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Batch";
        }

        char[] chars = name.Trim().ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (!(char.IsLetterOrDigit(c) || c == '_' || c == '-'))
            {
                chars[i] = '_';
            }
        }

        return new string(chars);
    }

    // -------------------------------------------------------------------------
    // Generatie
    // -------------------------------------------------------------------------

    private void GenerateLevels()
    {
        // Clamp basisinstellingen.
        numberOfLevels = Mathf.Max(1, numberOfLevels);
        minVehicles = Mathf.Clamp(minVehicles, 2, 16);
        maxVehicles = Mathf.Clamp(maxVehicles, minVehicles, 16);
        minMinimumMoves = Mathf.Max(1, minMinimumMoves);
        maxMinimumMoves = Mathf.Max(minMinimumMoves, maxMinimumMoves);
        maxAttemptsPerLevel = Mathf.Max(1, maxAttemptsPerLevel);
        minVehiclesUsedInSolution = Mathf.Max(1, minVehiclesUsedInSolution);
        minDirectBlockers = Mathf.Max(0, minDirectBlockers);
        if (maxBoardOccupancy < minBoardOccupancy)
        {
            maxBoardOccupancy = minBoardOccupancy;
        }

        if (maxMovableRatio < minMovableRatio)
        {
            maxMovableRatio = minMovableRatio;
        }

        if (string.IsNullOrWhiteSpace(outputFolder))
        {
            Debug.LogError("LevelGenerator: Output Folder is leeg.");
            return;
        }

        EnsureFolderExists(outputFolder);

        string safeBatchName = SanitizeBatchName(batchName);
        LevelDifficulty tier = GetLevelDifficultyTier();

        System.Random rng = new System.Random(randomSeed);
        HashSet<string> usedHashes = new HashSet<string>();

        int accepted = 0;
        int totalAttempts = 0;
        int rejectedUnsolvable = 0;
        int rejectedTooEasy = 0;
        int rejectedTooHard = 0;
        int rejectedDuplicates = 0;
        int rejectedTrivial = 0;
        int rejectedInvalid = 0;
        int rejectedSearchLimit = 0;
        int rejectedPlacementFailed = 0;
        int rejectedLowSolutionParticipation = 0;
        int rejectedDensity = 0;
        int rejectedMovableRatio = 0;
        int rejectedRepetitiveSolution = 0;
        int rejectedOrientationLengthBalance = 0;
        int rejectedBlockers = 0;
        int rejectedAlmostSolved = 0;

        int nextFileIndex = FindNextFileIndex(outputFolder, safeBatchName);

        for (int levelIndex = 0; levelIndex < numberOfLevels; levelIndex++)
        {
            bool found = false;

            for (int attempt = 0; attempt < maxAttemptsPerLevel; attempt++)
            {
                totalAttempts++;

                CandidateLevel candidate = TryBuildCandidate(rng);
                if (candidate == null)
                {
                    rejectedPlacementFailed++;
                    continue;
                }

                // --- Pre-solver quality filters (layout) ---
                if (!PassesOrientationLengthBalance(candidate))
                {
                    rejectedOrientationLengthBalance++;
                    continue;
                }

                int directBlockers = CountDirectBlockers(candidate);
                if (directBlockers < minDirectBlockers)
                {
                    rejectedBlockers++;
                    continue;
                }

                if (IsTrivialExit(candidate))
                {
                    rejectedTrivial++;
                    continue;
                }

                float occupancy = CalculateBoardOccupancy(candidate);
                if (occupancy < minBoardOccupancy || occupancy > maxBoardOccupancy)
                {
                    rejectedDensity++;
                    continue;
                }

                float movableRatio = CalculateMovableRatio(candidate);
                if (movableRatio < minMovableRatio || movableRatio > maxMovableRatio)
                {
                    rejectedMovableRatio++;
                    continue;
                }

                string hash = BuildCanonicalHash(candidate);
                if (!usedHashes.Add(hash))
                {
                    rejectedDuplicates++;
                    continue;
                }

                // Tijdelijke LevelData voor de bestaande BFS-solver.
                LevelData tempLevel = ScriptableObject.CreateInstance<LevelData>();
                FillLevelData(tempLevel, candidate, levelNumber: 0);

                LevelSolver.SolverResult result = LevelSolver.Solve(tempLevel);

                if (result.invalid)
                {
                    rejectedInvalid++;
                    DestroyImmediate(tempLevel);
                    continue;
                }

                if (result.searchLimitReached)
                {
                    rejectedSearchLimit++;
                    DestroyImmediate(tempLevel);
                    continue;
                }

                if (!result.solvable)
                {
                    rejectedUnsolvable++;
                    DestroyImmediate(tempLevel);
                    continue;
                }

                if (result.minimumMoves < minMinimumMoves)
                {
                    rejectedTooEasy++;
                    DestroyImmediate(tempLevel);
                    continue;
                }

                if (result.minimumMoves > maxMinimumMoves)
                {
                    rejectedTooHard++;
                    DestroyImmediate(tempLevel);
                    continue;
                }

                int uniqueVehiclesMoved = CountUniqueVehiclesInSolution(result);
                if (uniqueVehiclesMoved < minVehiclesUsedInSolution)
                {
                    rejectedLowSolutionParticipation++;
                    DestroyImmediate(tempLevel);
                    continue;
                }

                // Strenger: bijna-opgelost (één blocker / weinig unieke moves)
                // alleen als we geen tutorial-achtige min-moves genereren.
                if (minMinimumMoves >= 4 &&
                    directBlockers <= 1 &&
                    uniqueVehiclesMoved <= 2)
                {
                    rejectedAlmostSolved++;
                    DestroyImmediate(tempLevel);
                    continue;
                }

                // Solution diversity: vermijd zeer repetitieve oplossingen.
                if (result.minimumMoves >= 5)
                {
                    float solutionVehicleRatio =
                        (float)uniqueVehiclesMoved / result.minimumMoves;
                    if (solutionVehicleRatio < minSolutionVehicleRatio)
                    {
                        rejectedRepetitiveSolution++;
                        DestroyImmediate(tempLevel);
                        continue;
                    }
                }

                // Geaccepteerd → opslaan.
                int batchNumber = accepted + 1;
                tempLevel.levelNumber = batchNumber;
                tempLevel.difficulty = tier;
                LevelSolver.ApplyMetadata(tempLevel, result);

                string fileName = safeBatchName + "_" + nextFileIndex.ToString("000") + ".asset";
                string assetPath = outputFolder.TrimEnd('/', '\\') + "/" + fileName;

                AssetDatabase.CreateAsset(tempLevel, assetPath);
                EditorUtility.SetDirty(tempLevel);

                accepted++;
                nextFileIndex++;
                found = true;

                Debug.Log(
                    "Accepted | tier=" + tier +
                    " | moves=" + tempLevel.minimumMoves +
                    " | uniqueVehicles=" + uniqueVehiclesMoved +
                    " | occupancy=" + occupancy.ToString("0.00") +
                    " | movableRatio=" + movableRatio.ToString("0.00") +
                    " | score=" + tempLevel.difficultyScore +
                    " | → " + assetPath
                );
                break;
            }

            if (!found)
            {
                Debug.LogWarning(
                    "LevelGenerator: kon level " + (levelIndex + 1) +
                    "/" + numberOfLevels +
                    " niet vinden binnen " + maxAttemptsPerLevel + " attempts."
                );
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("=== LevelGenerator klaar ===");
        summary.AppendLine("attempts: " + totalAttempts);
        summary.AppendLine("accepted: " + accepted);
        summary.AppendLine("rejected unsolvable: " + rejectedUnsolvable);
        summary.AppendLine("rejected too easy: " + rejectedTooEasy);
        summary.AppendLine("rejected too hard: " + rejectedTooHard);
        summary.AppendLine("rejected duplicate: " + rejectedDuplicates);
        summary.AppendLine("rejected low solution participation: " + rejectedLowSolutionParticipation);
        summary.AppendLine("rejected density: " + rejectedDensity);
        summary.AppendLine("rejected movable ratio: " + rejectedMovableRatio);
        summary.AppendLine("rejected repetitive solution: " + rejectedRepetitiveSolution);
        summary.AppendLine("rejected orientation/length balance: " + rejectedOrientationLengthBalance);
        summary.AppendLine("rejected blockers: " + rejectedBlockers);
        summary.AppendLine("rejected almost solved: " + rejectedAlmostSolved);
        summary.AppendLine("rejected trivial (open exit): " + rejectedTrivial);
        summary.AppendLine("rejected invalid: " + rejectedInvalid);
        summary.AppendLine("rejected search limit: " + rejectedSearchLimit);
        summary.AppendLine("rejected placement failed: " + rejectedPlacementFailed);
        summary.AppendLine("preset: " + difficultyPreset);
        summary.AppendLine("batch: " + safeBatchName);
        summary.AppendLine("tier: " + tier);
        summary.AppendLine("output: " + outputFolder);
        summary.AppendLine("(niet toegevoegd aan MainLevelDatabase)");

        Debug.Log(summary.ToString());
    }

    // -------------------------------------------------------------------------
    // Kandidaat bouwen
    // -------------------------------------------------------------------------

    private CandidateLevel TryBuildCandidate(System.Random rng)
    {
        int vehicleCount = rng.Next(minVehicles, maxVehicles + 1);

        CandidateLevel candidate = new CandidateLevel();

        // 1) Target car eerst (altijd Horizontal length 2).
        int exitRow = rng.Next(1, 5);
        int targetX = rng.Next(0, 3);

        CandidateVehicle target = new CandidateVehicle
        {
            name = "Target",
            horizontal = true,
            length = 2,
            position = new Vector2Int(targetX, exitRow),
            canExitRight = true
        };

        candidate.exitRow = exitRow;
        candidate.vehicles.Add(target);

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        Occupy(occupied, target);

        int nonTargetCount = vehicleCount - 1;
        int maxLength3 = Mathf.FloorToInt(nonTargetCount * maxLongVehicleRatio);
        int length3Placed = 0;

        int placeAttempts = 0;
        int maxPlaceAttempts = nonTargetCount * 50;

        while (candidate.vehicles.Count < vehicleCount && placeAttempts < maxPlaceAttempts)
        {
            placeAttempts++;

            bool horizontal = rng.Next(0, 2) == 0;
            int length = 2;
            if (length3Placed < maxLength3 && rng.NextDouble() < 0.45)
            {
                length = 3;
            }

            if (!TryFindRandomFreePosition(rng, occupied, horizontal, length, out Vector2Int pos))
            {
                continue;
            }

            CandidateVehicle vehicle = new CandidateVehicle
            {
                name = GetVehicleName(candidate.vehicles.Count - 1),
                horizontal = horizontal,
                length = length,
                position = pos,
                canExitRight = false
            };

            candidate.vehicles.Add(vehicle);
            Occupy(occupied, vehicle);

            if (length == 3)
            {
                length3Placed++;
            }
        }

        if (candidate.vehicles.Count < vehicleCount)
        {
            return null;
        }

        if (CountDirectBlockers(candidate) < minDirectBlockers)
        {
            return null;
        }

        return candidate;
    }

    private static bool TryFindRandomFreePosition(
        System.Random rng,
        HashSet<Vector2Int> occupied,
        bool horizontal,
        int length,
        out Vector2Int position)
    {
        position = Vector2Int.zero;
        List<Vector2Int> options = new List<Vector2Int>();

        if (horizontal)
        {
            int maxX = GridSize - length;
            for (int x = 0; x <= maxX; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (AreCellsFree(occupied, pos, true, length))
                    {
                        options.Add(pos);
                    }
                }
            }
        }
        else
        {
            int maxY = GridSize - length;
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y <= maxY; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (AreCellsFree(occupied, pos, false, length))
                    {
                        options.Add(pos);
                    }
                }
            }
        }

        if (options.Count == 0)
        {
            return false;
        }

        position = options[rng.Next(0, options.Count)];
        return true;
    }

    // -------------------------------------------------------------------------
    // Quality helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Unieke voertuignamen in de minimale BFS-oplossing (incl. exit-move).
    /// </summary>
    private static int CountUniqueVehiclesInSolution(LevelSolver.SolverResult result)
    {
        if (result.solution == null || result.solution.Count == 0)
        {
            return 0;
        }

        HashSet<string> unique = new HashSet<string>();
        foreach (LevelSolver.Move move in result.solution)
        {
            if (!string.IsNullOrEmpty(move.vehicleName))
            {
                unique.Add(move.vehicleName);
            }
        }

        return unique.Count;
    }

    /// <summary>
    /// Aantal unieke voertuigen dat minstens één cel op de directe exit-route bezet.
    /// </summary>
    private static int CountDirectBlockers(CandidateLevel candidate)
    {
        CandidateVehicle target = FindTarget(candidate);
        if (target == null)
        {
            return 0;
        }

        int rightMost = target.position.x + target.length - 1;
        HashSet<string> blockers = new HashSet<string>();

        for (int i = 0; i < candidate.vehicles.Count; i++)
        {
            CandidateVehicle v = candidate.vehicles[i];
            if (v.canExitRight)
            {
                continue;
            }

            foreach (Vector2Int cell in GetCells(v.position, v.horizontal, v.length))
            {
                if (cell.y == target.position.y && cell.x > rightMost && cell.x < GridSize)
                {
                    blockers.Add(v.name);
                    break;
                }
            }
        }

        return blockers.Count;
    }

    private static float CalculateBoardOccupancy(CandidateLevel candidate)
    {
        int occupiedCells = 0;
        foreach (CandidateVehicle v in candidate.vehicles)
        {
            occupiedCells += v.length;
        }

        return occupiedCells / (float)TotalCells;
    }

    /// <summary>
    /// Aandeel niet-target voertuigen dat minstens één geldige gridstap kan maken.
    /// </summary>
    private static float CalculateMovableRatio(CandidateLevel candidate)
    {
        int nonTarget = 0;
        int movable = 0;

        for (int i = 0; i < candidate.vehicles.Count; i++)
        {
            CandidateVehicle v = candidate.vehicles[i];
            if (v.canExitRight)
            {
                continue;
            }

            nonTarget++;
            if (CanMakeAnyMove(candidate, i))
            {
                movable++;
            }
        }

        if (nonTarget == 0)
        {
            return 0f;
        }

        return movable / (float)nonTarget;
    }

    /// <summary>
    /// True als voertuig index minstens één cel kan schuiven in de startstate.
    /// (Zelfde move-definitie als de solver: geldige gridstap zonder overlap.)
    /// </summary>
    private static bool CanMakeAnyMove(CandidateLevel candidate, int vehicleIndex)
    {
        CandidateVehicle v = candidate.vehicles[vehicleIndex];
        bool[,] occupied = BuildOccupancy(candidate, ignoreIndex: vehicleIndex);

        if (v.horizontal)
        {
            // Links 1 cel.
            if (v.position.x - 1 >= 0 && !occupied[v.position.x - 1, v.position.y])
            {
                return true;
            }

            // Rechts 1 cel.
            int rightMost = v.position.x + v.length;
            if (rightMost < GridSize && !occupied[rightMost, v.position.y])
            {
                return true;
            }
        }
        else
        {
            // Omlaag 1 cel (y-).
            if (v.position.y - 1 >= 0 && !occupied[v.position.x, v.position.y - 1])
            {
                return true;
            }

            // Omhoog 1 cel (y+).
            int topMost = v.position.y + v.length;
            if (topMost < GridSize && !occupied[v.position.x, topMost])
            {
                return true;
            }
        }

        return false;
    }

    private static bool[,] BuildOccupancy(CandidateLevel candidate, int ignoreIndex)
    {
        bool[,] occupied = new bool[GridSize, GridSize];
        for (int i = 0; i < candidate.vehicles.Count; i++)
        {
            if (i == ignoreIndex)
            {
                continue;
            }

            CandidateVehicle v = candidate.vehicles[i];
            foreach (Vector2Int cell in GetCells(v.position, v.horizontal, v.length))
            {
                if (cell.x >= 0 && cell.x < GridSize && cell.y >= 0 && cell.y < GridSize)
                {
                    occupied[cell.x, cell.y] = true;
                }
            }
        }

        return occupied;
    }

    /// <summary>
    /// Lengtebalans (max ~40% length-3 bij non-target) + oriëntatiebalans bij 6+ voertuigen.
    /// </summary>
    private static bool PassesOrientationLengthBalance(CandidateLevel candidate)
    {
        int nonTarget = 0;
        int length3 = 0;
        int horizontal = 0;
        int vertical = 0;

        foreach (CandidateVehicle v in candidate.vehicles)
        {
            if (v.canExitRight)
            {
                // Target moet length 2 blijven.
                if (v.length != 2 || !v.horizontal)
                {
                    return false;
                }

                horizontal++;
                continue;
            }

            nonTarget++;
            if (v.length == 3)
            {
                length3++;
            }

            if (v.horizontal)
            {
                horizontal++;
            }
            else
            {
                vertical++;
            }
        }

        if (nonTarget > 0)
        {
            float longRatio = length3 / (float)nonTarget;
            if (longRatio > 0.40f + 0.0001f)
            {
                return false;
            }
        }

        if (candidate.vehicles.Count >= 6)
        {
            if (horizontal < 2 || vertical < 2)
            {
                return false;
            }
        }

        return true;
    }

    // -------------------------------------------------------------------------
    // Trivialiteit / blockers
    // -------------------------------------------------------------------------

    private static bool IsTrivialExit(CandidateLevel candidate)
    {
        return CountDirectBlockers(candidate) == 0;
    }

    private static CandidateVehicle FindTarget(CandidateLevel candidate)
    {
        foreach (CandidateVehicle v in candidate.vehicles)
        {
            if (v.canExitRight)
            {
                return v;
            }
        }

        return candidate.vehicles.Count > 0 ? candidate.vehicles[0] : null;
    }

    // -------------------------------------------------------------------------
    // Canonical hash
    // -------------------------------------------------------------------------

    private static string BuildCanonicalHash(CandidateLevel candidate)
    {
        List<string> parts = new List<string>();
        parts.Add("exit=" + candidate.exitRow);

        foreach (CandidateVehicle v in candidate.vehicles)
        {
            parts.Add(
                (v.horizontal ? "H" : "V") +
                v.length +
                "@" + v.position.x + "," + v.position.y +
                (v.canExitRight ? "T" : "")
            );
        }

        parts.Sort(StringComparer.Ordinal);
        return string.Join("|", parts);
    }

    // -------------------------------------------------------------------------
    // LevelData / bestanden
    // -------------------------------------------------------------------------

    private static void FillLevelData(LevelData levelData, CandidateLevel candidate, int levelNumber)
    {
        levelData.levelNumber = levelNumber;
        levelData.exitRow = candidate.exitRow;
        levelData.vehicles = new List<VehicleData>();

        foreach (CandidateVehicle v in candidate.vehicles)
        {
            levelData.vehicles.Add(new VehicleData
            {
                vehicleName = v.name,
                orientation = v.horizontal
                    ? VehicleController.VehicleOrientation.Horizontal
                    : VehicleController.VehicleOrientation.Vertical,
                lengthInCells = v.length,
                gridPosition = v.position,
                canExitRight = v.canExitRight,
                vehicleSprite = null
            });
        }
    }

    private static string GetVehicleName(int blockerIndex)
    {
        if (blockerIndex < 26)
        {
            return ((char)('A' + blockerIndex)).ToString();
        }

        return "V" + blockerIndex;
    }

    private static void EnsureFolderExists(string folder)
    {
        folder = folder.Replace('\\', '/').TrimEnd('/');
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string[] parts = folder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private static int FindNextFileIndex(string folder, string batchName)
    {
        folder = folder.Replace('\\', '/').TrimEnd('/');
        int maxIndex = 0;
        string prefix = batchName + "_";

        if (!AssetDatabase.IsValidFolder(folder))
        {
            return 1;
        }

        string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { folder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path);
            if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string numberPart = name.Substring(prefix.Length);
            if (int.TryParse(numberPart, out int index))
            {
                maxIndex = Mathf.Max(maxIndex, index);
            }
        }

        return maxIndex + 1;
    }

    // -------------------------------------------------------------------------
    // Occupancy helpers
    // -------------------------------------------------------------------------

    private static void Occupy(HashSet<Vector2Int> occupied, CandidateVehicle vehicle)
    {
        foreach (Vector2Int cell in GetCells(vehicle.position, vehicle.horizontal, vehicle.length))
        {
            occupied.Add(cell);
        }
    }

    private static bool AreCellsFree(
        HashSet<Vector2Int> occupied,
        Vector2Int start,
        bool horizontal,
        int length)
    {
        foreach (Vector2Int cell in GetCells(start, horizontal, length))
        {
            if (cell.x < 0 || cell.x >= GridSize || cell.y < 0 || cell.y >= GridSize)
            {
                return false;
            }

            if (occupied.Contains(cell))
            {
                return false;
            }
        }

        return true;
    }

    private static List<Vector2Int> GetCells(Vector2Int start, bool horizontal, int length)
    {
        List<Vector2Int> cells = new List<Vector2Int>(length);
        for (int i = 0; i < length; i++)
        {
            cells.Add(horizontal
                ? new Vector2Int(start.x + i, start.y)
                : new Vector2Int(start.x, start.y + i));
        }

        return cells;
    }

    // -------------------------------------------------------------------------
    // Interne data-types
    // -------------------------------------------------------------------------

    private class CandidateLevel
    {
        public int exitRow;
        public List<CandidateVehicle> vehicles = new List<CandidateVehicle>();
    }

    private class CandidateVehicle
    {
        public string name;
        public bool horizontal;
        public int length;
        public Vector2Int position;
        public bool canExitRight;
    }
}

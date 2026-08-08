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

    private const string GeneratedRoot = "Assets/Data/GeneratedLevels";

    // --- Preset / batch ---
    private DifficultyPreset difficultyPreset = DifficultyPreset.Custom;
    private string batchName = "Custom_Batch_01";

    // --- Grid (default blijft 6x6) ---
    private int gridWidth = LevelData.DefaultGridSize;
    private int gridHeight = LevelData.DefaultGridSize;

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

    // Placement-diagnostics (tijdelijk, gereset per GenerateLevels).
    private int failTargetPlacement;
    private int failDirectBlockerPlacement;
    private int failOrdinaryVehiclePlacement;
    private int failCouldNotReachVehicleCount;
    private int failInvalidBounds;
    private int failOverlap;

    private const int BaselineBoardArea = 36; // 6x6
    private const int PlacementAttemptsPerVehicle = 40;

    private Vector2 scroll;

    private int BoardArea => gridWidth * gridHeight;

    private float AreaScale => BoardArea / (float)BaselineBoardArea;

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

        int prevW = gridWidth;
        int prevH = gridHeight;
        gridWidth = EditorGUILayout.IntSlider(
            "Grid Width",
            gridWidth,
            LevelData.MinGridSize,
            LevelData.MaxGridSize
        );
        gridHeight = EditorGUILayout.IntSlider(
            "Grid Height",
            gridHeight,
            LevelData.MinGridSize,
            LevelData.MaxGridSize
        );

        if ((gridWidth != prevW || gridHeight != prevH) &&
            difficultyPreset != DifficultyPreset.Custom)
        {
            ApplyDifficultyPreset(difficultyPreset);
        }

        DrawEffectiveGridSummary();

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
    /// Toont de actuele (geschaalde) generator-waarden voor dit board.
    /// </summary>
    private void DrawEffectiveGridSummary()
    {
        EditorGUILayout.HelpBox(
            "Grid: " + gridWidth + "x" + gridHeight +
            "\nEffective Vehicles: " + minVehicles + "-" + maxVehicles +
            "\nOccupancy: " + minBoardOccupancy.ToString("0.00") +
            "-" + maxBoardOccupancy.ToString("0.00") +
            "\nArea scale: " + AreaScale.ToString("0.00") +
            " (baseline 6x6 = 1.00)",
            MessageType.Info
        );
    }

    /// <summary>
    /// Vult generator-instellingen vanuit 6x6-baseline, daarna schalen op board area.
    /// </summary>
    private void ApplyDifficultyPreset(DifficultyPreset preset)
    {
        // Baseline = gevoel op 6x6; ScalePresetToCurrentGrid past aan op gridWidth/Height.
        int baseMinV;
        int baseMaxV;
        float baseMinOcc;
        float baseMaxOcc;
        int baseMinUsed;
        int baseMinBlockers;
        float baseMinMovable;
        float baseMaxMovable;
        int baseAttempts;

        switch (preset)
        {
            case DifficultyPreset.Easy:
                baseMinV = 3;
                baseMaxV = 6;
                minMinimumMoves = 2;
                maxMinimumMoves = 5;
                baseMinUsed = 2;
                baseMinBlockers = 1;
                baseMinOcc = 0.20f;
                baseMaxOcc = 0.50f;
                baseMinMovable = 0.30f;
                baseMaxMovable = 0.90f;
                baseAttempts = 1000;
                batchName = "Easy_Batch_01";
                outputFolder = GeneratedRoot + "/Easy";
                break;

            case DifficultyPreset.Medium:
                baseMinV = 5;
                baseMaxV = 9;
                minMinimumMoves = 5;
                maxMinimumMoves = 9;
                baseMinUsed = 3;
                baseMinBlockers = 1;
                baseMinOcc = 0.30f;
                baseMaxOcc = 0.65f;
                baseMinMovable = 0.25f;
                baseMaxMovable = 0.80f;
                baseAttempts = 2000;
                batchName = "Medium_Batch_01";
                outputFolder = GeneratedRoot + "/Medium";
                break;

            case DifficultyPreset.Hard:
                baseMinV = 7;
                baseMaxV = 11;
                minMinimumMoves = 8;
                maxMinimumMoves = 14;
                baseMinUsed = 4;
                baseMinBlockers = 2;
                baseMinOcc = 0.40f;
                baseMaxOcc = 0.75f;
                baseMinMovable = 0.20f;
                baseMaxMovable = 0.70f;
                baseAttempts = 5000;
                batchName = "Hard_Batch_01";
                outputFolder = GeneratedRoot + "/Hard";
                break;

            case DifficultyPreset.Custom:
            default:
                batchName = "Custom_Batch_01";
                outputFolder = GeneratedRoot + "/Custom";
                return;
        }

        ScalePresetToCurrentGrid(
            baseMinV,
            baseMaxV,
            baseMinOcc,
            baseMaxOcc,
            baseMinUsed,
            baseMinBlockers,
            baseMinMovable,
            baseMaxMovable,
            baseAttempts
        );
    }

    /// <summary>
    /// Schaalt 6x6-baseline naar huidige board area (boardArea / 36).
    /// 6x6 blijft ~onveranderd; 10x10 Hard ≈ 14-22 vehicles, occupancy ~0.25-0.50.
    /// </summary>
    private void ScalePresetToCurrentGrid(
        int baseMinVehicles,
        int baseMaxVehicles,
        float baseMinOccupancy,
        float baseMaxOccupancy,
        int baseMinVehiclesUsed,
        int baseMinDirectBlockers,
        float baseMinMovable,
        float baseMaxMovable,
        int baseAttempts)
    {
        float areaScale = Mathf.Max(0.25f, AreaScale);

        // Voertuigen: gedeeltelijk meegroeien met area (niet 1:1, anders te zwaar).
        float vehicleScale = Mathf.Lerp(1f, areaScale, 0.6f);
        minVehicles = Mathf.Clamp(Mathf.RoundToInt(baseMinVehicles * vehicleScale), 2, 40);
        maxVehicles = Mathf.Clamp(
            Mathf.RoundToInt(baseMaxVehicles * vehicleScale),
            minVehicles,
            40
        );

        // Density: grotere boards → lagere occupancy-targets (zelfde "volheid").
        float occupancyScale = Mathf.Lerp(1f, 1f / Mathf.Sqrt(areaScale), 0.85f);
        minBoardOccupancy = Mathf.Clamp(baseMinOccupancy * occupancyScale, 0.12f, 0.55f);
        maxBoardOccupancy = Mathf.Clamp(
            baseMaxOccupancy * occupancyScale,
            minBoardOccupancy + 0.08f,
            0.85f
        );

        // Oplossing-kwaliteit: licht meeschalen; op grote boards genoeg met 1 blocker.
        minVehiclesUsedInSolution = Mathf.Clamp(
            Mathf.RoundToInt(baseMinVehiclesUsed * Mathf.Lerp(1f, areaScale, 0.25f)),
            1,
            8
        );
        if (areaScale >= 2f && baseMinDirectBlockers > 1)
        {
            minDirectBlockers = 1;
        }
        else
        {
            minDirectBlockers = Mathf.Clamp(baseMinDirectBlockers, 0, 3);
        }

        minMovableRatio = Mathf.Clamp01(baseMinMovable);
        maxMovableRatio = Mathf.Clamp(baseMaxMovable, minMovableRatio, 1f);

        // Meer attempts op grotere boards (state space / placement).
        maxAttemptsPerLevel = Mathf.Clamp(
            Mathf.RoundToInt(baseAttempts * Mathf.Lerp(1f, areaScale, 0.5f)),
            baseAttempts,
            25000
        );
    }

    /// <summary>
    /// Aanbevolen vehicle-count range (zelfde schaalformule als presets).
    /// </summary>
    private static void GetScaledVehicleRange(
        float areaScale,
        int baseMin,
        int baseMax,
        out int scaledMin,
        out int scaledMax)
    {
        float vehicleScale = Mathf.Lerp(1f, Mathf.Max(0.25f, areaScale), 0.6f);
        scaledMin = Mathf.Clamp(Mathf.RoundToInt(baseMin * vehicleScale), 2, 40);
        scaledMax = Mathf.Clamp(Mathf.RoundToInt(baseMax * vehicleScale), scaledMin, 40);
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
        // Clamp basisinstellingen (vehicle-cap schaalt met board, niet vast op 16).
        numberOfLevels = Mathf.Max(1, numberOfLevels);
        gridWidth = Mathf.Clamp(gridWidth, LevelData.MinGridSize, LevelData.MaxGridSize);
        gridHeight = Mathf.Clamp(gridHeight, LevelData.MinGridSize, LevelData.MaxGridSize);
        int vehicleCap = Mathf.Clamp(Mathf.RoundToInt(16f * Mathf.Max(1f, AreaScale)), 16, 40);
        minVehicles = Mathf.Clamp(minVehicles, 2, vehicleCap);
        maxVehicles = Mathf.Clamp(maxVehicles, minVehicles, vehicleCap);
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

        ResetPlacementDiagnostics();

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

                CandidateLevel candidate = TryBuildCandidate(rng, out PlacementFailReason failReason);
                if (candidate == null)
                {
                    rejectedPlacementFailed++;
                    RecordPlacementFailure(failReason);
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
        summary.AppendLine("  · target placement failed: " + failTargetPlacement);
        summary.AppendLine("  · direct blocker placement failed: " + failDirectBlockerPlacement);
        summary.AppendLine("  · ordinary vehicle placement failed: " + failOrdinaryVehiclePlacement);
        summary.AppendLine("  · could not reach vehicle count: " + failCouldNotReachVehicleCount);
        summary.AppendLine("  · invalid bounds: " + failInvalidBounds);
        summary.AppendLine("  · overlap: " + failOverlap);
        summary.AppendLine("grid: " + gridWidth + "x" + gridHeight);
        summary.AppendLine("vehicles: " + minVehicles + "-" + maxVehicles);
        summary.AppendLine(
            "occupancy targets: " +
            minBoardOccupancy.ToString("0.00") + "-" + maxBoardOccupancy.ToString("0.00")
        );
        summary.AppendLine("area scale: " + AreaScale.ToString("0.00"));
        summary.AppendLine("preset: " + difficultyPreset);
        summary.AppendLine("batch: " + safeBatchName);
        summary.AppendLine("tier: " + tier);
        summary.AppendLine("output: " + outputFolder);
        summary.AppendLine("(niet toegevoegd aan MainLevelDatabase)");

        Debug.Log(summary.ToString());
    }

    // -------------------------------------------------------------------------
    // Kandidaat bouwen (grid-size-aware)
    // -------------------------------------------------------------------------

    private enum PlacementFailReason
    {
        None,
        TargetPlacementFailed,
        DirectBlockerPlacementFailed,
        OrdinaryVehiclePlacementFailed,
        CouldNotReachVehicleCount,
        InvalidBounds,
        Overlap
    }

    private void ResetPlacementDiagnostics()
    {
        failTargetPlacement = 0;
        failDirectBlockerPlacement = 0;
        failOrdinaryVehiclePlacement = 0;
        failCouldNotReachVehicleCount = 0;
        failInvalidBounds = 0;
        failOverlap = 0;
    }

    private void RecordPlacementFailure(PlacementFailReason reason)
    {
        switch (reason)
        {
            case PlacementFailReason.TargetPlacementFailed:
                failTargetPlacement++;
                break;
            case PlacementFailReason.DirectBlockerPlacementFailed:
                failDirectBlockerPlacement++;
                break;
            case PlacementFailReason.OrdinaryVehiclePlacementFailed:
                failOrdinaryVehiclePlacement++;
                break;
            case PlacementFailReason.CouldNotReachVehicleCount:
                failCouldNotReachVehicleCount++;
                break;
            case PlacementFailReason.InvalidBounds:
                failInvalidBounds++;
                break;
            case PlacementFailReason.Overlap:
                failOverlap++;
                break;
        }
    }

    private CandidateLevel TryBuildCandidate(
        System.Random rng,
        out PlacementFailReason failReason)
    {
        failReason = PlacementFailReason.None;
        int vehicleCount = rng.Next(minVehicles, maxVehicles + 1);

        CandidateLevel candidate = new CandidateLevel
        {
            gridWidth = gridWidth,
            gridHeight = gridHeight
        };

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

        // 1) Target eerst (Horizontal length 2).
        if (!TryPlaceTarget(rng, candidate, occupied, out failReason))
        {
            return null;
        }

        int nonTargetBudget = vehicleCount - 1;
        int maxLength3 = Mathf.FloorToInt(nonTargetBudget * maxLongVehicleRatio);
        int length3Placed = 0;

        // 2) Vereiste direct blockers op exit-pad (niet aan kans overlaten).
        for (int b = 0; b < minDirectBlockers; b++)
        {
            if (!TryPlaceDirectBlocker(
                    rng,
                    candidate,
                    occupied,
                    ref length3Placed,
                    maxLength3,
                    out failReason))
            {
                if (failReason == PlacementFailReason.None)
                {
                    failReason = PlacementFailReason.DirectBlockerPlacementFailed;
                }

                return null;
            }
        }

        // 3) Overige voertuigen — meerdere attempts per slot (oriëntatie + length).
        while (candidate.vehicles.Count < vehicleCount)
        {
            if (!TryPlaceOrdinaryVehicle(
                    rng,
                    candidate,
                    occupied,
                    ref length3Placed,
                    maxLength3,
                    out PlacementFailReason ordinaryFail))
            {
                // Geen enkele vrije configuratie meer → stop met duidelijke reden.
                failReason = candidate.vehicles.Count < vehicleCount
                    ? PlacementFailReason.CouldNotReachVehicleCount
                    : ordinaryFail;

                if (ordinaryFail == PlacementFailReason.OrdinaryVehiclePlacementFailed ||
                    ordinaryFail == PlacementFailReason.None)
                {
                    failReason = PlacementFailReason.CouldNotReachVehicleCount;
                }

                return null;
            }
        }

        if (candidate.vehicles.Count < vehicleCount)
        {
            failReason = PlacementFailReason.CouldNotReachVehicleCount;
            return null;
        }

        // Safety: blockers zouden er nu moeten zijn.
        if (CountDirectBlockers(candidate) < minDirectBlockers)
        {
            failReason = PlacementFailReason.DirectBlockerPlacementFailed;
            return null;
        }

        failReason = PlacementFailReason.None;
        return candidate;
    }

    private bool TryPlaceTarget(
        System.Random rng,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        out PlacementFailReason failReason)
    {
        failReason = PlacementFailReason.None;

        if (gridWidth < 2 || gridHeight < 1)
        {
            failReason = PlacementFailReason.InvalidBounds;
            return false;
        }

        for (int attempt = 0; attempt < PlacementAttemptsPerVehicle; attempt++)
        {
            int exitRow = rng.Next(0, gridHeight);

            // Ruimte rechts houden zodat direct blockers op het exit-pad passen.
            int minPathCells = Mathf.Max(2, minDirectBlockers);
            int maxTargetX = Mathf.Max(0, gridWidth - 2 - minPathCells);
            int targetX = rng.Next(0, maxTargetX + 1);
            Vector2Int pos = new Vector2Int(targetX, exitRow);

            if (!AreCellsFree(occupied, pos, horizontal: true, length: 2))
            {
                failReason = PlacementFailReason.Overlap;
                continue;
            }

            if (!IsPlacementInsideGrid(pos, horizontal: true, length: 2, gridWidth, gridHeight))
            {
                failReason = PlacementFailReason.InvalidBounds;
                continue;
            }

            CandidateVehicle target = new CandidateVehicle
            {
                name = "Target",
                horizontal = true,
                length = 2,
                position = pos,
                canExitRight = true
            };

            candidate.exitRow = exitRow;
            candidate.vehicles.Add(target);
            Occupy(occupied, target);
            failReason = PlacementFailReason.None;
            return true;
        }

        failReason = PlacementFailReason.TargetPlacementFailed;
        return false;
    }

    /// <summary>
    /// Plaatst een voertuig dat minstens één cel op het exit-pad van de target bezet.
    /// </summary>
    private bool TryPlaceDirectBlocker(
        System.Random rng,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        ref int length3Placed,
        int maxLength3,
        out PlacementFailReason failReason)
    {
        failReason = PlacementFailReason.None;
        CandidateVehicle target = FindTarget(candidate);
        if (target == null)
        {
            failReason = PlacementFailReason.TargetPlacementFailed;
            return false;
        }

        List<PlacementOption> options = CollectDirectBlockerOptions(
            occupied,
            target,
            length3Placed,
            maxLength3
        );

        if (options.Count == 0)
        {
            failReason = PlacementFailReason.DirectBlockerPlacementFailed;
            return false;
        }

        // Shuffle-achtig: kies random optie.
        PlacementOption chosen = options[rng.Next(0, options.Count)];
        if (!AreCellsFree(occupied, chosen.position, chosen.horizontal, chosen.length))
        {
            failReason = PlacementFailReason.Overlap;
            return false;
        }

        CandidateVehicle vehicle = new CandidateVehicle
        {
            name = GetVehicleName(candidate.vehicles.Count - 1),
            horizontal = chosen.horizontal,
            length = chosen.length,
            position = chosen.position,
            canExitRight = false
        };

        candidate.vehicles.Add(vehicle);
        Occupy(occupied, vehicle);
        if (chosen.length == 3)
        {
            length3Placed++;
        }

        return true;
    }

    private List<PlacementOption> CollectDirectBlockerOptions(
        HashSet<Vector2Int> occupied,
        CandidateVehicle target,
        int length3Placed,
        int maxLength3)
    {
        List<PlacementOption> options = new List<PlacementOption>();
        int rightMost = target.position.x + target.length - 1;
        int exitRow = target.position.y;

        bool allowLength3 = length3Placed < maxLength3;
        int[] lengths = allowLength3 ? new[] { 2, 3 } : new[] { 2 };

        foreach (int length in lengths)
        {
            // Horizontaal op exit-row, rechts van target.
            int maxX = gridWidth - length;
            for (int x = 0; x <= maxX; x++)
            {
                Vector2Int pos = new Vector2Int(x, exitRow);
                if (!AreCellsFree(occupied, pos, true, length))
                {
                    continue;
                }

                if (!PlacementBlocksExitPath(pos, true, length, rightMost, exitRow))
                {
                    continue;
                }

                options.Add(new PlacementOption(true, length, pos));
            }

            // Verticaal: kolom op exit-pad, overlapt exitRow.
            int maxY = gridHeight - length;
            for (int x = rightMost + 1; x < gridWidth; x++)
            {
                for (int y = 0; y <= maxY; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (!AreCellsFree(occupied, pos, false, length))
                    {
                        continue;
                    }

                    if (!PlacementBlocksExitPath(pos, false, length, rightMost, exitRow))
                    {
                        continue;
                    }

                    options.Add(new PlacementOption(false, length, pos));
                }
            }
        }

        return options;
    }

    private static bool PlacementBlocksExitPath(
        Vector2Int pos,
        bool horizontal,
        int length,
        int targetRightMost,
        int exitRow)
    {
        foreach (Vector2Int cell in GetCells(pos, horizontal, length))
        {
            if (cell.y == exitRow && cell.x > targetRightMost)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryPlaceOrdinaryVehicle(
        System.Random rng,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        ref int length3Placed,
        int maxLength3,
        out PlacementFailReason failReason)
    {
        failReason = PlacementFailReason.None;

        for (int attempt = 0; attempt < PlacementAttemptsPerVehicle; attempt++)
        {
            bool preferHorizontal = rng.Next(0, 2) == 0;
            int length = 2;
            if (length3Placed < maxLength3 && rng.NextDouble() < 0.45)
            {
                length = 3;
            }

            // Probeer gekozen oriëntatie, daarna de andere.
            bool[] orientations = preferHorizontal
                ? new[] { true, false }
                : new[] { false, true };

            foreach (bool horizontal in orientations)
            {
                if (!TryFindRandomFreePosition(
                        rng,
                        occupied,
                        horizontal,
                        length,
                        gridWidth,
                        gridHeight,
                        out Vector2Int pos))
                {
                    continue;
                }

                if (!IsPlacementInsideGrid(pos, horizontal, length, gridWidth, gridHeight))
                {
                    failReason = PlacementFailReason.InvalidBounds;
                    continue;
                }

                if (!AreCellsFree(occupied, pos, horizontal, length))
                {
                    failReason = PlacementFailReason.Overlap;
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

                failReason = PlacementFailReason.None;
                return true;
            }

            // Length-2 fallback als length-3 geen plek had.
            if (length == 3)
            {
                foreach (bool horizontal in orientations)
                {
                    if (!TryFindRandomFreePosition(
                            rng,
                            occupied,
                            horizontal,
                            2,
                            gridWidth,
                            gridHeight,
                            out Vector2Int pos))
                    {
                        continue;
                    }

                    CandidateVehicle vehicle = new CandidateVehicle
                    {
                        name = GetVehicleName(candidate.vehicles.Count - 1),
                        horizontal = horizontal,
                        length = 2,
                        position = pos,
                        canExitRight = false
                    };

                    candidate.vehicles.Add(vehicle);
                    Occupy(occupied, vehicle);
                    failReason = PlacementFailReason.None;
                    return true;
                }
            }
        }

        failReason = PlacementFailReason.OrdinaryVehiclePlacementFailed;
        return false;
    }

    private static bool IsPlacementInsideGrid(
        Vector2Int start,
        bool horizontal,
        int length,
        int width,
        int height)
    {
        foreach (Vector2Int cell in GetCells(start, horizontal, length))
        {
            if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
            {
                return false;
            }
        }

        return true;
    }

    private readonly struct PlacementOption
    {
        public readonly bool horizontal;
        public readonly int length;
        public readonly Vector2Int position;

        public PlacementOption(bool horizontal, int length, Vector2Int position)
        {
            this.horizontal = horizontal;
            this.length = length;
            this.position = position;
        }
    }

    private static bool TryFindRandomFreePosition(
        System.Random rng,
        HashSet<Vector2Int> occupied,
        bool horizontal,
        int length,
        int gridWidth,
        int gridHeight,
        out Vector2Int position)
    {
        position = Vector2Int.zero;
        List<Vector2Int> options = new List<Vector2Int>();

        if (horizontal)
        {
            int maxX = gridWidth - length;
            if (maxX < 0)
            {
                return false;
            }

            for (int x = 0; x <= maxX; x++)
            {
                for (int y = 0; y < gridHeight; y++)
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
            int maxY = gridHeight - length;
            if (maxY < 0)
            {
                return false;
            }

            for (int x = 0; x < gridWidth; x++)
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
                if (cell.y == target.position.y && cell.x > rightMost && cell.x < candidate.gridWidth)
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

        return occupiedCells / (float)(candidate.gridWidth * candidate.gridHeight);
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
            if (rightMost < candidate.gridWidth && !occupied[rightMost, v.position.y])
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
            if (topMost < candidate.gridHeight && !occupied[v.position.x, topMost])
            {
                return true;
            }
        }

        return false;
    }

    private static bool[,] BuildOccupancy(CandidateLevel candidate, int ignoreIndex)
    {
        bool[,] occupied = new bool[candidate.gridWidth, candidate.gridHeight];
        for (int i = 0; i < candidate.vehicles.Count; i++)
        {
            if (i == ignoreIndex)
            {
                continue;
            }

            CandidateVehicle v = candidate.vehicles[i];
            foreach (Vector2Int cell in GetCells(v.position, v.horizontal, v.length))
            {
                if (cell.x >= 0 && cell.x < candidate.gridWidth && cell.y >= 0 && cell.y < candidate.gridHeight)
                {
                    occupied[cell.x, cell.y] = true;
                }
            }
        }

        return occupied;
    }

    /// <summary>
    /// Lengtebalans (max ~40% length-3 bij non-target) + oriëntatiebalans
    /// wanneer er genoeg voertuigen zijn (schaalt licht met board area).
    /// </summary>
    private bool PassesOrientationLengthBalance(CandidateLevel candidate)
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
            // Blijft ~40%; iets soepeler op hele grote boards zodat density haalbaar blijft.
            float maxLong = Mathf.Lerp(0.40f, 0.45f, Mathf.Clamp01(AreaScale - 1f));
            if (longRatio > maxLong + 0.0001f)
            {
                return false;
            }
        }

        // Oriëntatiebalans: baseline bij 6 voertuigen op 6x6; schaal mee.
        int orientationGate = Mathf.Max(6, Mathf.RoundToInt(6f * Mathf.Lerp(1f, AreaScale, 0.35f)));
        if (candidate.vehicles.Count >= orientationGate)
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
        levelData.gridWidth = candidate.gridWidth;
        levelData.gridHeight = candidate.gridHeight;
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
        public int gridWidth = LevelData.DefaultGridSize;
        public int gridHeight = LevelData.DefaultGridSize;
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

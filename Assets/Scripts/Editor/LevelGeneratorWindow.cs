using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

    public enum GenerationStrategy
    {
        Random,
        ConstructiveHard
    }

    private const string GeneratedRoot = "Assets/Data/GeneratedLevels";

    // --- Preset / batch ---
    private DifficultyPreset difficultyPreset = DifficultyPreset.Custom;
    private GenerationStrategy generationStrategy = GenerationStrategy.Random;
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

    // --- Constructive Hard ---
    private int minBlockingChainLength = 2;
    private int maxBlockingChainLength = 4;
    private float constructiveMinMovableRatio = 0.10f;
    private float constructiveMaxMovableRatio = 0.95f;
    private int minInitialBranching = 4;
    private int maxInitialBranching = 18;
    private int maxFillerTravelDistance = 2;
    private bool useConstructiveHardAutoTuning = false;
    private float minChainMoveEfficiency = 0.75f;
    private float minChainParticipation = 0.70f;
    private int chainOnlySolverMaxStates = 8000;
    [SerializeField] private int minSecondaryBlockers = 0;
    [SerializeField] private int maxSecondaryBlockers = 2;
    [SerializeField] private bool enableForkDependencies = false;

    // Solution structure quality (post-solver)
    [SerializeField] private int minAxisAlternations = 0;
    [SerializeField] private int maxSingleAxisRun = 99;
    [SerializeField] private int minVehicleRevisits = 0;
    [SerializeField] private float maxLinearChainSolutionRatio = 1f;
    [SerializeField] private int minSecondaryBlockersUsed = 0;
    [SerializeField] private int minForkDependencies = 0;
    [SerializeField] private int minSolutionComplexityScore = 0;

    // Effective runtime values (generation only; ResolveEffectiveSettings).
    private int effectiveMinVehicles;
    private int effectiveMaxVehicles;
    private int effectiveMinMoves;
    private int effectiveMaxMoves;
    private int effectiveMinChain;
    private int effectiveMaxChain;
    private int effectiveMinBranching;
    private int effectiveMaxBranching;
    private int effectiveMaxFillerTravel;
    private int effectiveMinSecondary;
    private int effectiveMaxSecondary;
    private int effectiveMinAxisAlternations;
    private int effectiveMaxSingleAxisRun;
    private int effectiveMinVehicleRevisits;
    private float effectiveMaxLinearChainSolutionRatio;
    private int effectiveMinSecondaryBlockersUsed;
    private int effectiveMinForkDependencies;
    private int effectiveMinVehiclesUsedInSolution;
    private int effectiveMinSolutionComplexityScore;
    private int effectivePreSolverMoveFloor;
    private float effectiveMinOccupancy;
    private float effectiveMaxOccupancy;

    // --- Performance / safety ---
    private int placementAttemptsPerVehicle = 40;
    private int maxPlacementIterationsPerCandidate = 800;
    private int maxSolverStatesPerCandidate = 50000;
    private float maxGenerationSecondsPerLevel = 30f;
    private bool cancelGeneration;

    // --- Quality filters ---
    private int minVehiclesUsedInSolution = 3;
    private int minDirectBlockers = 1;
    private float minBoardOccupancy = 0.25f;
    private float maxBoardOccupancy = 0.70f;
    private float minMovableRatio = 0.25f;
    private float maxMovableRatio = 0.85f;
    private float maxLongVehicleRatio = 0.40f;
    private float minSolutionVehicleRatio = 0.35f;

    // Length-4 trucks (filler / ordinary only; ConstructiveHard chain blijft 2/3).
    private bool allowLength4Vehicles = false;
    private int maxLength4Vehicles = 2;

    // Placement-diagnostics (tijdelijk, gereset per GenerateLevels).
    private int failTargetPlacement;
    private int failDirectBlockerPlacement;
    private int failOrdinaryVehiclePlacement;
    private int failCouldNotReachVehicleCount;
    private int failInvalidBounds;
    private int failOverlap;
    private int failChainCreation;
    private int failChainNoTargetBlocker;
    private int failChainNoDependencyCell;
    private int failChainNoValidOrientation;
    private int failChainPlacementExhausted;
    private int failChainBacktrackExhausted;
    private readonly int[] chainNodePlacedCounts = new int[8];
    private int sumDeepestChainNode;
    private int deepestChainNodeSamples;
    private int lastDeepestChainNode;

    // Broken-chain validation diagnostics
    [SerializeField] private bool verboseChainDiagnostics;
    private int brokenChainVerboseLogged;
    private int brokenChainPreviousCriticalCellNotBlocked;
    private int brokenChainRequiredMoveDoesNotClear;
    private int brokenChainDependencyDoesNotBlockRequiredMove;
    private int brokenChainAlternativeDirectionCreatesShortcut;
    private int brokenChainInvalidDependencyReference;
    private int brokenChainRequiredDisplacementInvalid;
    private int brokenChainOutOfBoundsClearance;
    private int brokenChainOther;

    // Constructive adaptatie binnen één GenerateLevels-run.
    private int constructiveChainBias;
    private bool constructiveSparseFiller;

    // Solution-floor construction diagnostics (gereset per GenerateLevels).
    private int chainExtensionsAttempted;
    private int chainExtensionsSuccessful;
    // Secondary metrics (clear definitions)
    private int secondaryCreationAttempts;
    private int secondaryCandidatePlacements;
    private int secondaryCommitted;
    private int secondaryRejectedAfterPlacement;
    private int secondaryUsedInOptimalSolution;
    private int secondaryBypassed;
    private int forkCreationAttempted;
    private int forkCreationSuccessful;
    private int secondaryVerboseLogged;
    private int forkVerboseLogged;
    private int secondaryNoEligibleChainNode;
    private int secondaryNoRequiredClearanceCell;
    private int secondaryNoPlacementCandidate;
    private int secondaryCannotMove;
    private int secondaryDoesNotBlock;
    private int secondaryDoesNotUnlock;
    private int secondaryOverlap;
    private int secondaryOutOfBounds;
    private int secondaryWouldBreakChain;
    private int secondaryCreatesShortcut;
    private int secondaryWouldMakeUnsolvable;
    private int secondaryOther;
    private int forkNoEligibleNode;
    private int forkNotEnoughClearanceCells;
    private int forkPlacementFailed;
    private int forkOverlap;
    private int forkOutOfBounds;
    private int forkWouldBreakChain;
    private int forkOther;
    private long sumBranchingBeforeFillers;
    private int branchingBeforeFillersSamples;
    private long sumBranchingAfterFillers;
    private int branchingAfterFillersSamples;
    private long sumVehiclesBeforeFillers;
    private int vehiclesBeforeFillersSamples;
    private long sumInitialChainLength;
    private int initialChainLengthSamples;
    private long sumExtendedChainLength;
    private int extendedChainLengthSamples;
    private long sumChainOnlyMovesBeforeExtension;
    private int chainOnlyMovesBeforeExtensionSamples;
    private long sumChainOnlyMovesAfterExtension;
    private int chainOnlyMovesAfterExtensionSamples;

    private const int BaselineBoardArea = 36; // 6x6

    private Vector2 scroll;

    private int BoardArea => gridWidth * gridHeight;

    private float AreaScale => BoardArea / (float)BaselineBoardArea;

    [MenuItem("RushOut/Generate Levels")]
    public static void OpenWindow()
    {
        LevelGeneratorWindow window = GetWindow<LevelGeneratorWindow>("Generate Levels");
        window.minSize = new Vector2(440f, 640f);
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
        EditorGUILayout.LabelField("Generation Strategy", EditorStyles.boldLabel);
        generationStrategy = (GenerationStrategy)EditorGUILayout.EnumPopup(
            "Strategy",
            generationStrategy
        );

        if (generationStrategy == GenerationStrategy.Random)
        {
            EditorGUILayout.HelpBox(
                "Actieve pipeline: Random → cheap filters → solver → minimumMoves → basic quality.\n" +
                "ConstructiveHard blijft beschikbaar via Strategy, maar wordt niet auto-geselecteerd.",
                MessageType.Info
            );
        }
        else
        {
            // ConstructiveHard UI alleen zichtbaar bij expliciete keuze (fase 1: niet verwijderd).
            minBlockingChainLength = EditorGUILayout.IntField(
                "Min Blocking Chain Length",
                minBlockingChainLength
            );
            maxBlockingChainLength = EditorGUILayout.IntField(
                "Max Blocking Chain Length",
                maxBlockingChainLength
            );
            constructiveMinMovableRatio = EditorGUILayout.Slider(
                "Constructive Min Movable Ratio",
                constructiveMinMovableRatio,
                0f,
                1f
            );
            constructiveMaxMovableRatio = EditorGUILayout.Slider(
                "Constructive Max Movable Ratio",
                constructiveMaxMovableRatio,
                0f,
                1f
            );
            minInitialBranching = EditorGUILayout.IntField(
                "Min Initial Branching",
                minInitialBranching
            );
            maxInitialBranching = EditorGUILayout.IntField(
                "Max Initial Branching",
                maxInitialBranching
            );
            maxFillerTravelDistance = EditorGUILayout.IntField(
                "Max Filler Travel Distance",
                maxFillerTravelDistance
            );
            useConstructiveHardAutoTuning = EditorGUILayout.Toggle(
                "Use ConstructiveHard Auto Tuning",
                useConstructiveHardAutoTuning
            );
            verboseChainDiagnostics = EditorGUILayout.Toggle(
                "Verbose Chain Diagnostics",
                verboseChainDiagnostics
            );
            minChainMoveEfficiency = EditorGUILayout.Slider(
                "Min Chain Move Efficiency",
                minChainMoveEfficiency,
                0.5f,
                1f
            );
            minChainParticipation = EditorGUILayout.Slider(
                "Min Chain Participation",
                minChainParticipation,
                0f,
                1f
            );
            chainOnlySolverMaxStates = EditorGUILayout.IntField(
                "Chain-Only Solver Max States",
                chainOnlySolverMaxStates
            );
            minSecondaryBlockers = EditorGUILayout.IntField(
                "Min Secondary Blockers",
                minSecondaryBlockers
            );
            maxSecondaryBlockers = EditorGUILayout.IntField(
                "Max Secondary Blockers",
                maxSecondaryBlockers
            );
            enableForkDependencies = EditorGUILayout.Toggle(
                "Enable Fork Dependencies",
                enableForkDependencies
            );

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Solution Structure Quality", EditorStyles.boldLabel);
            minAxisAlternations = EditorGUILayout.IntField(
                "Min Axis Alternations",
                minAxisAlternations
            );
            maxSingleAxisRun = EditorGUILayout.IntField(
                "Max Single Axis Run",
                maxSingleAxisRun
            );
            minVehicleRevisits = EditorGUILayout.IntField(
                "Min Vehicle Revisits",
                minVehicleRevisits
            );
            maxLinearChainSolutionRatio = EditorGUILayout.Slider(
                "Max Linear Chain Solution Ratio",
                maxLinearChainSolutionRatio,
                0.3f,
                1f
            );
            minSecondaryBlockersUsed = EditorGUILayout.IntField(
                "Min Secondary Blockers Used",
                minSecondaryBlockersUsed
            );
            minForkDependencies = EditorGUILayout.IntField(
                "Min Fork Dependencies",
                minForkDependencies
            );
            minSolutionComplexityScore = EditorGUILayout.IntField(
                "Min Solution Complexity Score",
                minSolutionComplexityScore
            );

            EditorGUILayout.HelpBox(
                "ConstructiveHard (legacy): chain / secondary / fork / complexity gates.\n" +
                "Voor normale levelgeneratie: kies Strategy = Random.",
                MessageType.Warning
            );
        }

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

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Performance / Safety", EditorStyles.boldLabel);
        placementAttemptsPerVehicle = EditorGUILayout.IntField(
            "Placement Attempts Per Vehicle",
            placementAttemptsPerVehicle
        );
        maxPlacementIterationsPerCandidate = EditorGUILayout.IntField(
            "Max Placement Iterations / Candidate",
            maxPlacementIterationsPerCandidate
        );
        maxSolverStatesPerCandidate = EditorGUILayout.IntField(
            "Max Solver States / Candidate",
            maxSolverStatesPerCandidate
        );
        maxGenerationSecondsPerLevel = EditorGUILayout.FloatField(
            "Max Generation Seconds / Level",
            maxGenerationSecondsPerLevel
        );

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
        EditorGUILayout.LabelField("Vehicle Lengths", EditorStyles.boldLabel);
        allowLength4Vehicles = EditorGUILayout.Toggle(
            "Allow Length 4 Vehicles",
            allowLength4Vehicles
        );
        using (new EditorGUI.DisabledScope(!allowLength4Vehicles))
        {
            maxLength4Vehicles = EditorGUILayout.IntField(
                "Max Length 4 Vehicles",
                maxLength4Vehicles
            );
        }

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
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Levels", GUILayout.Height(36f)))
        {
            cancelGeneration = false;
            GenerateLevels();
        }

        GUI.enabled = true;
        if (GUILayout.Button("STOP / CANCEL", GUILayout.Height(36f), GUILayout.Width(140f)))
        {
            cancelGeneration = true;
            Debug.LogWarning("LevelGenerator: cancel requested.");
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Toont de actuele (geschaalde) generator-waarden voor dit board.
    /// </summary>
    private void DrawEffectiveGridSummary()
    {
        ResolveEffectiveSettings();

        if (generationStrategy == GenerationStrategy.Random)
        {
            EditorGUILayout.HelpBox(
                "Grid: " + gridWidth + "x" + gridHeight +
                "\nStrategy: Random" +
                "\nVehicles: " + effectiveMinVehicles + "-" + effectiveMaxVehicles +
                "\nMoves: " + effectiveMinMoves + "-" + effectiveMaxMoves +
                "\nOccupancy: " +
                effectiveMinOccupancy.ToString("0.00") + "-" +
                effectiveMaxOccupancy.ToString("0.00") +
                "\nMovable: " +
                GetEffectiveMinMovableRatio().ToString("0.00") + "-" +
                GetEffectiveMaxMovableRatio().ToString("0.00") +
                "\nLength 4: " +
                (allowLength4Vehicles
                    ? ("on (max " + maxLength4Vehicles + ")")
                    : "off") +
                "\nArea scale: " + AreaScale.ToString("0.00") +
                " (baseline 6x6 = 1.00)",
                MessageType.Info
            );
            return;
        }

        string effectiveLabel = "Effective ConstructiveHard:";

        EditorGUILayout.HelpBox(
            "Grid: " + gridWidth + "x" + gridHeight +
            "\nStrategy: " + generationStrategy +
            "\nConfigured:" +
            "\n  Chain: " + minBlockingChainLength + "-" + maxBlockingChainLength +
            "\n  Vehicles: " + minVehicles + "-" + maxVehicles +
            "\n  Moves: " + minMinimumMoves + "-" + maxMinimumMoves +
            "\n  Branching: " + minInitialBranching + "-" + maxInitialBranching +
            "\n" + effectiveLabel +
            "\n  Chain: " + effectiveMinChain + "-" + effectiveMaxChain +
            "\n  Vehicles: " + effectiveMinVehicles + "-" + effectiveMaxVehicles +
            "\n  Moves: " + effectiveMinMoves + "-" + effectiveMaxMoves +
            "\n  Branching: " + effectiveMinBranching + "-" + effectiveMaxBranching +
            "\n  Secondary: try " + effectiveMinSecondary + "-" + effectiveMaxSecondary +
            " (optional enrichment)" +
            "\n  Pre-solver move floor: " + effectivePreSolverMoveFloor +
            "\n  Post-solver complexity ≥ " + effectiveMinSolutionComplexityScore +
            "\n  Movable: " +
            GetEffectiveMinMovableRatio().ToString("0.00") + "-" +
            GetEffectiveMaxMovableRatio().ToString("0.00") +
            "\n  Participation: " + minChainParticipation.ToString("0.00") +
            "\n  Efficiency: " + minChainMoveEfficiency.ToString("0.00") +
            "\nOccupancy: " + minBoardOccupancy.ToString("0.00") +
            "-" + maxBoardOccupancy.ToString("0.00") +
            "\nArea scale: " + AreaScale.ToString("0.00") +
            " (baseline 6x6 = 1.00)",
            MessageType.Info
        );
    }

    /// <summary>
    /// Berekent effective* runtime-waarden voor generatie (auto-tuning optioneel).
    /// </summary>
    private void ResolveEffectiveSettings()
    {
        effectiveMinVehicles = minVehicles;
        effectiveMaxVehicles = maxVehicles;
        effectiveMinMoves = minMinimumMoves;
        effectiveMaxMoves = maxMinimumMoves;
        effectiveMinChain = minBlockingChainLength;
        effectiveMaxChain = maxBlockingChainLength;
        effectiveMinBranching = minInitialBranching;
        effectiveMaxBranching = maxInitialBranching;
        effectiveMaxFillerTravel = maxFillerTravelDistance;
        effectiveMinSecondary = minSecondaryBlockers;
        effectiveMaxSecondary = maxSecondaryBlockers;
        effectiveMinAxisAlternations = minAxisAlternations;
        effectiveMaxSingleAxisRun = maxSingleAxisRun;
        effectiveMinVehicleRevisits = minVehicleRevisits;
        effectiveMaxLinearChainSolutionRatio = maxLinearChainSolutionRatio;
        effectiveMinSecondaryBlockersUsed = minSecondaryBlockersUsed;
        effectiveMinForkDependencies = minForkDependencies;
        effectiveMinVehiclesUsedInSolution = minVehiclesUsedInSolution;
        effectiveMinSolutionComplexityScore = minSolutionComplexityScore;
        effectivePreSolverMoveFloor = Mathf.Max(2, effectiveMinMoves - 2);
        effectiveMinOccupancy = minBoardOccupancy;
        effectiveMaxOccupancy = maxBoardOccupancy;

        // Fase 1 Random: geen ConstructiveHard accept-gates / auto-tuning.
        if (generationStrategy != GenerationStrategy.ConstructiveHard)
        {
            effectiveMinSolutionComplexityScore = 0;
            effectiveMinAxisAlternations = 0;
            effectiveMinVehicleRevisits = 0;
            effectiveMaxSingleAxisRun = 99;
            effectiveMaxLinearChainSolutionRatio = 1f;
            effectiveMinSecondaryBlockersUsed = 0;
            effectiveMinForkDependencies = 0;
            effectivePreSolverMoveFloor = 0;
            return;
        }

        if (!useConstructiveHardAutoTuning)
        {
            return;
        }

        int minSide = Mathf.Min(gridWidth, gridHeight);
        if (minSide < 8)
        {
            return;
        }

        if (minSide >= 10)
        {
            // ≈ chainLength+1 moves → chain 6–7; secondary/fork optioneel; score post-solver.
            effectiveMinChain = 6;
            effectiveMaxChain = 7;
            effectiveMinVehicles = 12;
            effectiveMaxVehicles = 17;
            effectiveMinMoves = 8;
            effectiveMaxMoves = 14;
            effectiveMinBranching = 5;
            effectiveMaxBranching = 12;
            effectiveMaxFillerTravel = 1;
            // Secondary creation is enrichment — not a hard pre-solver gate.
            effectiveMinSecondary = 0;
            effectiveMaxSecondary = 2;
            effectiveMinAxisAlternations = 3;
            effectiveMaxSingleAxisRun = 3;
            effectiveMinVehicleRevisits = 1;
            effectiveMaxLinearChainSolutionRatio = 0.70f;
            effectiveMinSecondaryBlockersUsed = 0;
            effectiveMinForkDependencies = 0;
            effectiveMinVehiclesUsedInSolution = minVehiclesUsedInSolution;
            effectiveMinSolutionComplexityScore = Mathf.Max(minSolutionComplexityScore, 5);
            effectivePreSolverMoveFloor = Mathf.Max(2, effectiveMinMoves - 2);
            effectiveMinOccupancy = 0.20f;
            effectiveMaxOccupancy = 0.42f;
        }
        else
        {
            effectiveMinChain = 3;
            effectiveMaxChain = 4;
            effectiveMinVehicles = Mathf.Clamp(minVehicles, 10, 18);
            effectiveMaxVehicles = Mathf.Clamp(maxVehicles, effectiveMinVehicles, 18);
            effectiveMinMoves = 8;
            effectiveMaxMoves = 14;
            effectiveMinBranching = 4;
            effectiveMaxBranching = 16;
            effectiveMaxFillerTravel = 2;
            effectiveMinSecondary = 0;
            effectiveMaxSecondary = 2;
            effectiveMinAxisAlternations = 2;
            effectiveMaxSingleAxisRun = 4;
            effectiveMinVehicleRevisits = 0;
            effectiveMaxLinearChainSolutionRatio = 0.80f;
            effectiveMinSecondaryBlockersUsed = 0;
            effectiveMinForkDependencies = 0;
            effectiveMinVehiclesUsedInSolution = minVehiclesUsedInSolution;
            effectiveMinSolutionComplexityScore = Mathf.Max(minSolutionComplexityScore, 4);
            effectivePreSolverMoveFloor = Mathf.Max(2, effectiveMinMoves - 2);
            effectiveMinOccupancy = minBoardOccupancy;
            effectiveMaxOccupancy = maxBoardOccupancy;
        }

        minChainMoveEfficiency = 0.50f;
        minChainParticipation = 0.60f;
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
                minMinimumMoves = 3;
                maxMinimumMoves = 6;
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
                minMinimumMoves = 6;
                maxMinimumMoves = 10;
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
                minMinimumMoves = 10;
                maxMinimumMoves = 16;
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
        ApplyGenerationStrategyDefaults(preset);
        ApplyLength4DefaultsForGrid();
    }

    /// <summary>
    /// Length-4 defaults voor Random-pipeline op 6×6–8×8 (fase 1).
    /// 6→ uit; 7→ max 1; 8+→ max 2. Gebruikt max(side) zodat 8×6 ook trucks kan krijgen.
    /// </summary>
    private void ApplyLength4DefaultsForGrid()
    {
        int size = Mathf.Max(gridWidth, gridHeight);
        if (size >= 8)
        {
            allowLength4Vehicles = true;
            maxLength4Vehicles = 2;
        }
        else if (size >= 7)
        {
            allowLength4Vehicles = true;
            maxLength4Vehicles = 1;
        }
        else
        {
            allowLength4Vehicles = false;
            maxLength4Vehicles = 1;
        }
    }

    /// <summary>
    /// Fase 1: altijd Random als actieve default. ConstructiveHard alleen handmatig.
    /// Geen auto-switch meer naar ConstructiveHard op Hard/8×8.
    /// </summary>
    private void ApplyGenerationStrategyDefaults(DifficultyPreset preset)
    {
        if (preset == DifficultyPreset.Easy ||
            preset == DifficultyPreset.Medium ||
            preset == DifficultyPreset.Hard)
        {
            generationStrategy = GenerationStrategy.Random;
            useConstructiveHardAutoTuning = false;
        }
    }

    private bool UsesConstructiveRelaxedMovable =>
        generationStrategy == GenerationStrategy.ConstructiveHard &&
        Mathf.Min(gridWidth, gridHeight) >= 8;

    private bool UsesConstructiveBranchingFilter =>
        generationStrategy == GenerationStrategy.ConstructiveHard;

    private float GetEffectiveMinMovableRatio()
    {
        return UsesConstructiveRelaxedMovable
            ? constructiveMinMovableRatio
            : minMovableRatio;
    }

    private float GetEffectiveMaxMovableRatio()
    {
        float max = UsesConstructiveRelaxedMovable
            ? constructiveMaxMovableRatio
            : maxMovableRatio;
        return Mathf.Max(max, GetEffectiveMinMovableRatio());
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

        // Meer attempts op grotere boards (state space / placement), maar HARD capped.
        maxAttemptsPerLevel = Mathf.Clamp(
            Mathf.RoundToInt(baseAttempts * Mathf.Lerp(1f, areaScale, 0.5f)),
            baseAttempts,
            2000
        );

        // 10x10 defaults: strengere safety-limieten.
        if (areaScale >= 2.5f)
        {
            maxGenerationSecondsPerLevel = 30f;
            maxSolverStatesPerCandidate = 50000;
        }
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
        cancelGeneration = false;

        // Clamp basisinstellingen (vehicle-cap schaalt met board, niet vast op 16).
        numberOfLevels = Mathf.Max(1, numberOfLevels);
        gridWidth = Mathf.Clamp(gridWidth, LevelData.MinGridSize, LevelData.MaxGridSize);
        gridHeight = Mathf.Clamp(gridHeight, LevelData.MinGridSize, LevelData.MaxGridSize);
        int vehicleCap = Mathf.Clamp(Mathf.RoundToInt(16f * Mathf.Max(1f, AreaScale)), 16, 40);
        minVehicles = Mathf.Clamp(minVehicles, 2, vehicleCap);
        maxVehicles = Mathf.Clamp(maxVehicles, minVehicles, vehicleCap);
        minMinimumMoves = Mathf.Max(1, minMinimumMoves);
        maxMinimumMoves = Mathf.Max(minMinimumMoves, maxMinimumMoves);
        maxAttemptsPerLevel = Mathf.Clamp(maxAttemptsPerLevel, 1, 5000);
        placementAttemptsPerVehicle = Mathf.Clamp(placementAttemptsPerVehicle, 1, 200);
        maxPlacementIterationsPerCandidate = Mathf.Clamp(maxPlacementIterationsPerCandidate, 1, 5000);
        maxSolverStatesPerCandidate = Mathf.Clamp(maxSolverStatesPerCandidate, 1000, 200000);
        maxGenerationSecondsPerLevel = Mathf.Max(1f, maxGenerationSecondsPerLevel);
        minVehiclesUsedInSolution = Mathf.Max(1, minVehiclesUsedInSolution);
        minDirectBlockers = Mathf.Max(0, minDirectBlockers);
        minBlockingChainLength = Mathf.Clamp(minBlockingChainLength, 1, 12);
        maxBlockingChainLength = Mathf.Clamp(
            maxBlockingChainLength,
            minBlockingChainLength,
            12
        );
        constructiveMinMovableRatio = Mathf.Clamp01(constructiveMinMovableRatio);
        constructiveMaxMovableRatio = Mathf.Clamp(
            constructiveMaxMovableRatio,
            constructiveMinMovableRatio,
            1f
        );
        minInitialBranching = Mathf.Max(0, minInitialBranching);
        maxInitialBranching = Mathf.Max(minInitialBranching, maxInitialBranching);
        maxFillerTravelDistance = Mathf.Clamp(maxFillerTravelDistance, 1, 8);
        maxLength4Vehicles = Mathf.Clamp(maxLength4Vehicles, 0, 8);
        minChainMoveEfficiency = Mathf.Clamp(minChainMoveEfficiency, 0.5f, 1f);
        minChainParticipation = Mathf.Clamp01(minChainParticipation);
        chainOnlySolverMaxStates = Mathf.Clamp(chainOnlySolverMaxStates, 500, 100000);
        minSecondaryBlockers = Mathf.Clamp(minSecondaryBlockers, 0, 6);
        maxSecondaryBlockers = Mathf.Clamp(
            maxSecondaryBlockers,
            minSecondaryBlockers,
            6
        );
        minAxisAlternations = Mathf.Clamp(minAxisAlternations, 0, 40);
        maxSingleAxisRun = Mathf.Clamp(maxSingleAxisRun, 1, 40);
        minVehicleRevisits = Mathf.Clamp(minVehicleRevisits, 0, 20);
        maxLinearChainSolutionRatio = Mathf.Clamp(maxLinearChainSolutionRatio, 0.3f, 1f);
        minSecondaryBlockersUsed = Mathf.Clamp(minSecondaryBlockersUsed, 0, 6);
        minForkDependencies = Mathf.Clamp(minForkDependencies, 0, 6);
        minSolutionComplexityScore = Mathf.Clamp(minSolutionComplexityScore, 0, 20);
        if (maxBoardOccupancy < minBoardOccupancy)
        {
            maxBoardOccupancy = minBoardOccupancy;
        }

        if (maxMovableRatio < minMovableRatio)
        {
            maxMovableRatio = minMovableRatio;
        }

        ResetPlacementDiagnostics();
        constructiveChainBias = 0;
        constructiveSparseFiller = false;

        ResolveEffectiveSettings();

        int constructiveAttempts = 0;
        int chainCreationFailed = 0;
        int rejectedBrokenChain = 0;
        int rejectedChainShortcut = 0;
        int rejectedLowChainParticipation = 0;
        int sumChainLength = 0;
        int chainLengthSamples = 0;
        int sumPlannedChainLength = 0;
        int plannedChainLengthSamples = 0;
        int sumValidChainLength = 0;
        int validChainLengthSamples = 0;
        long sumChainParticipationSolved = 0;
        int chainParticipationSolvedSamples = 0;
        long sumChainParticipationTooEasy = 0;
        int chainParticipationTooEasySamples = 0;
        long sumInitialMovableVehicles = 0;
        int initialMovableVehicleSamples = 0;
        long sumDestinationMoveCount = 0;
        int destinationMoveCountSamples = 0;
        long sumChainOnlyMinimumMoves = 0;
        int chainOnlyMinimumMovesSamples = 0;
        long sumFinalMinimumMoves = 0;
        int finalMinimumMovesSamples = 0;
        int solvedConstructiveCandidates = 0;
        int rejectedConstructiveTooEasy = 0;
        int rejectedConstructiveSearchLimit = 0;
        int rejectedExcessiveBranching = 0;
        int rejectedConstructedSolutionFloor = 0;
        int rejectedLinearSolution = 0;
        int rejectedLowAxisAlternation = 0;
        int rejectedNoVehicleRevisit = 0;
        int rejectedNoSecondaryBlocker = 0;
        int rejectedInsufficientDependencyComplexity = 0;
        int rejectedLowComplexityScore = 0;
        int postSolverRejects = 0;
        long sumShortcutChainOnlyMoves = 0;
        long sumShortcutChainLength = 0;
        int shortcutRejectSamples = 0;
        long sumAcceptanceComplexityScore = 0;
        int acceptanceComplexitySamples = 0;
        long sumAxisAlternations = 0;
        long sumVehicleRevisits = 0;
        long sumLongestSingleAxisRun = 0;
        long sumLinearChainRatio = 0;
        long sumSecondaryUsedSolved = 0;
        long sumForkDependenciesSolved = 0;
        int structureMetricSamples = 0;

        long sumInitialBranching = 0;
        int initialBranchingSamples = 0;
        long sumSearchLimitBranching = 0;
        long sumSearchLimitVehicles = 0;
        long sumSearchLimitChain = 0;
        int searchLimitConstructiveSamples = 0;
        long sumTooEasyMoves = 0;
        int tooEasyConstructiveSamples = 0;

        if (string.IsNullOrWhiteSpace(outputFolder))
        {
            Debug.LogError("LevelGenerator: Output Folder is leeg.");
            return;
        }

        EnsureFolderExists(outputFolder);

        string safeBatchName = SanitizeBatchName(batchName);
        LevelDifficulty tier = GetLevelDifficultyTier();

        System.Random rng = new System.Random(randomSeed);

        int existingDuplicateReports;
        int generatedAssetsScanned;
        int databaseLevelsScanned;
        HashSet<string> existingLevelKeys = LevelCanonicalKey.CollectExistingLevelKeys(
            out int existingLevelKeysLoaded,
            out generatedAssetsScanned,
            out databaseLevelsScanned,
            out existingDuplicateReports
        );
        HashSet<string> generatedKeysThisRun = new HashSet<string>(StringComparer.Ordinal);
        int rejectedDuplicatesThisRun = 0;
        int rejectedDuplicatesAgainstExisting = 0;

        int accepted = 0;
        long sumAcceptedMinimumMoves = 0;
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
        int solverInvocations = 0;
        int rejectedBeforeSolverCount = 0;
        long sumStatesExplored = 0;
        long sumOccupancyBuilds = 0;
        long sumGeneratedMoves = 0;
        long sumPrecheckRejects = 0;
        long sumChildStates = 0;
        long sumChildEnqueued = 0;
        long sumDiscovered = 0;
        int maxQueuePeak = 0;
        int exploredLimitHits = 0;
        int discoveredLimitHits = 0;
        double sumOccBuildMs = 0;
        double sumMoveGenMs = 0;
        double sumVisitedMs = 0;
        double sumStateCopyMs = 0;

        // Profiling accumulators (ms)
        double sumPlacementMs = 0;
        double sumSolverMs = 0;
        double sumTotalCandidateMs = 0;
        double sumDensityMs = 0;
        double sumMovableMs = 0;
        double sumDuplicateMs = 0;
        double sumReconstructMs = 0;
        double sumRepetitiveMs = 0;
        double sumOtherFilterMs = 0;
        double maxSolverMs = 0;
        double maxCandidateMs = 0;
        int profiledCandidates = 0;

        int nextFileIndex = FindNextFileIndex(outputFolder, safeBatchName);
        Stopwatch batchWatch = Stopwatch.StartNew();

        Debug.Log(
            "LevelGenerator START | grid=" + gridWidth + "x" + gridHeight +
            " | strategy=" + generationStrategy +
            " | chain=" + effectiveMinChain + "-" + effectiveMaxChain +
            " (configured " + minBlockingChainLength + "-" + maxBlockingChainLength + ")" +
            " | branching=" + effectiveMinBranching + "-" + effectiveMaxBranching +
            " | moves=" + effectiveMinMoves + "-" + effectiveMaxMoves +
            " | movable=" + GetEffectiveMinMovableRatio().ToString("0.00") +
            "-" + GetEffectiveMaxMovableRatio().ToString("0.00") +
            " | levels=" + numberOfLevels +
            " | maxAttempts/level=" + maxAttemptsPerLevel +
            " | maxSolverStates=" + maxSolverStatesPerCandidate +
            " | timeout/level=" + maxGenerationSecondsPerLevel + "s"
        );

        try
        {
            for (int levelIndex = 0; levelIndex < numberOfLevels; levelIndex++)
            {
                if (cancelGeneration)
                {
                    Debug.LogWarning("LevelGenerator: cancelled before level " + (levelIndex + 1));
                    break;
                }

                bool found = false;
                double levelStartSeconds = batchWatch.Elapsed.TotalSeconds;
                int levelAttemptStart = totalAttempts;

                for (int attempt = 0; attempt < maxAttemptsPerLevel; attempt++)
                {
                    if (cancelGeneration)
                    {
                        Debug.LogWarning("LevelGenerator: cancel requested.");
                        break;
                    }

                    double levelElapsed = batchWatch.Elapsed.TotalSeconds - levelStartSeconds;
                    if (levelElapsed >= maxGenerationSecondsPerLevel)
                    {
                        Debug.LogWarning(
                            "Generation timeout after " +
                            levelElapsed.ToString("0.0") +
                            " seconds (level " + (levelIndex + 1) + "/" + numberOfLevels + ")"
                        );
                        break;
                    }

                    totalAttempts++;
                    profiledCandidates++;

                    if (totalAttempts % 25 == 0)
                    {
                        bool cancel = EditorUtility.DisplayCancelableProgressBar(
                            "Generating Levels",
                            "Level " + (levelIndex + 1) + "/" + numberOfLevels +
                            " | attempt " + totalAttempts,
                            Mathf.Clamp01(
                                (levelIndex + (attempt + 1f) / maxAttemptsPerLevel) /
                                Mathf.Max(1, numberOfLevels)
                            )
                        );
                        if (cancel)
                        {
                            cancelGeneration = true;
                            break;
                        }

                        Repaint();
                    }

                    Stopwatch candidateWatch = Stopwatch.StartNew();
                    Stopwatch phaseWatch = Stopwatch.StartNew();

                    CandidateLevel candidate = TryBuildCandidate(rng, out PlacementFailReason failReason);
                    if (generationStrategy == GenerationStrategy.ConstructiveHard)
                    {
                        constructiveAttempts++;
                        sumPlannedChainLength += lastConstructivePlannedChainLength;
                        plannedChainLengthSamples++;

                        if (candidate == null)
                        {
                            deepestChainNodeSamples++;
                            sumDeepestChainNode += lastDeepestChainNode;

                            switch (failReason)
                            {
                                case PlacementFailReason.ChainCreationFailed:
                                    chainCreationFailed++;
                                    break;
                                case PlacementFailReason.ChainBroken:
                                    rejectedBrokenChain++;
                                    break;
                                case PlacementFailReason.ChainShortcut:
                                    rejectedChainShortcut++;
                                    sumShortcutChainOnlyMoves += lastShortcutChainOnlyMoves;
                                    sumShortcutChainLength += lastShortcutChainLength;
                                    shortcutRejectSamples++;
                                    break;
                                case PlacementFailReason.ConstructedSolutionFloor:
                                    rejectedConstructedSolutionFloor++;
                                    break;
                                case PlacementFailReason.InsufficientDependencyComplexity:
                                    // Legacy — should no longer fire as hard pre-solver gate.
                                    rejectedInsufficientDependencyComplexity++;
                                    break;
                            }
                        }
                        else
                        {
                            deepestChainNodeSamples++;
                            sumDeepestChainNode += lastDeepestChainNode;
                            RecordConstructiveStructureMetrics(candidate);
                        }

                        if (candidate != null && candidate.usedConstructive)
                        {
                            sumValidChainLength += candidate.chainLength;
                            validChainLengthSamples++;
                            sumChainLength += candidate.chainLength;
                            chainLengthSamples++;
                        }
                    }

                    phaseWatch.Stop();
                    double placementMs = phaseWatch.Elapsed.TotalMilliseconds;
                    sumPlacementMs += placementMs;

                    if (candidate == null)
                    {
                        rejectedPlacementFailed++;
                        rejectedBeforeSolverCount++;
                        RecordPlacementFailure(failReason);
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        MaybeLogAttemptProgress(
                            totalAttempts,
                            maxAttemptsPerLevel,
                            sumPlacementMs,
                            sumSolverMs,
                            maxSolverMs,
                            sumTotalCandidateMs,
                            profiledCandidates,
                            rejectedPlacementFailed,
                            rejectedDensity,
                            rejectedUnsolvable,
                            rejectedSearchLimit
                        );
                        continue;
                    }

                    // --- Pre-solver quality filters (CHEAP FIRST — never call BFS early) ---
                    // Random order: trivial → density → movable → blockers → balance → duplicate → SOLVER
                    phaseWatch.Restart();
                    if (IsTrivialExit(candidate))
                    {
                        rejectedTrivial++;
                        rejectedBeforeSolverCount++;
                        sumOtherFilterMs += phaseWatch.Elapsed.TotalMilliseconds;
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    phaseWatch.Restart();
                    float occupancy = CalculateBoardOccupancy(candidate);
                    if (occupancy < effectiveMinOccupancy || occupancy > effectiveMaxOccupancy)
                    {
                        rejectedDensity++;
                        rejectedBeforeSolverCount++;
                        sumDensityMs += phaseWatch.Elapsed.TotalMilliseconds;
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        MaybeLogAttemptProgress(
                            totalAttempts,
                            maxAttemptsPerLevel,
                            sumPlacementMs,
                            sumSolverMs,
                            maxSolverMs,
                            sumTotalCandidateMs,
                            profiledCandidates,
                            rejectedPlacementFailed,
                            rejectedDensity,
                            rejectedUnsolvable,
                            rejectedSearchLimit
                        );
                        continue;
                    }

                    sumDensityMs += phaseWatch.Elapsed.TotalMilliseconds;

                    phaseWatch.Restart();
                    float movableRatio = CalculateMovableRatio(candidate);
                    float effectiveMinMovable = GetEffectiveMinMovableRatio();
                    float effectiveMaxMovable = GetEffectiveMaxMovableRatio();
                    if (movableRatio < effectiveMinMovable || movableRatio > effectiveMaxMovable)
                    {
                        rejectedMovableRatio++;
                        rejectedBeforeSolverCount++;
                        sumMovableMs += phaseWatch.Elapsed.TotalMilliseconds;
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    sumMovableMs += phaseWatch.Elapsed.TotalMilliseconds;

                    phaseWatch.Restart();
                    int directBlockers = CountDirectBlockers(candidate);
                    if (directBlockers < minDirectBlockers)
                    {
                        rejectedBlockers++;
                        rejectedBeforeSolverCount++;
                        sumOtherFilterMs += phaseWatch.Elapsed.TotalMilliseconds;
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    phaseWatch.Restart();
                    if (!PassesOrientationLengthBalance(candidate))
                    {
                        rejectedOrientationLengthBalance++;
                        rejectedBeforeSolverCount++;
                        sumOtherFilterMs += phaseWatch.Elapsed.TotalMilliseconds;
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    // Goedkope branching-estimate vóór BFS (alleen ConstructiveHard-gate).
                    phaseWatch.Restart();
                    int initialBranching = CountInitialBranching(candidate);
                    int initialMovable = CountInitialMovableVehicles(candidate);
                    candidate.initialBranching = initialBranching;
                    candidate.initialDestinationMoveCount = initialBranching;
                    candidate.initialMovableVehicleCount = initialMovable;
                    sumInitialBranching += initialBranching;
                    initialBranchingSamples++;
                    sumInitialMovableVehicles += initialMovable;
                    initialMovableVehicleSamples++;
                    sumDestinationMoveCount += initialBranching;
                    destinationMoveCountSamples++;
                    if (UsesConstructiveBranchingFilter &&
                        (initialBranching < effectiveMinBranching ||
                         initialBranching > effectiveMaxBranching))
                    {
                        rejectedExcessiveBranching++;
                        rejectedBeforeSolverCount++;
                        sumOtherFilterMs += phaseWatch.Elapsed.TotalMilliseconds;
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    sumOtherFilterMs += phaseWatch.Elapsed.TotalMilliseconds;

                    // Almost-solved (uniqueVehiclesMoved) vereist BFS — blijft post-solver.
                    // Geen pre-solver almost-solved zonder betekenis van de filter te wijzigen.

                    phaseWatch.Restart();
                    string layoutKey = BuildCanonicalHash(candidate);
                    if (!generatedKeysThisRun.Add(layoutKey))
                    {
                        rejectedDuplicates++;
                        rejectedDuplicatesThisRun++;
                        rejectedBeforeSolverCount++;
                        sumDuplicateMs += phaseWatch.Elapsed.TotalMilliseconds;
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    if (existingLevelKeys.Contains(layoutKey))
                    {
                        rejectedDuplicates++;
                        rejectedDuplicatesAgainstExisting++;
                        rejectedBeforeSolverCount++;
                        sumDuplicateMs += phaseWatch.Elapsed.TotalMilliseconds;
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    sumDuplicateMs += phaseWatch.Elapsed.TotalMilliseconds;

                    // --- PAS NU BFS (duurste stap) ---
                    LevelData tempLevel = ScriptableObject.CreateInstance<LevelData>();
                    FillLevelData(tempLevel, candidate, levelNumber: 0);

                    solverInvocations++;
                    phaseWatch.Restart();
                    LevelSolver.SolverResult result = LevelSolver.Solve(
                        tempLevel,
                        maxSolverStatesPerCandidate
                    );
                    phaseWatch.Stop();
                    double solverMs = phaseWatch.Elapsed.TotalMilliseconds;
                    sumSolverMs += solverMs;
                    if (solverMs > maxSolverMs)
                    {
                        maxSolverMs = solverMs;
                    }

                    sumStatesExplored += result.statesExplored;
                    sumOccupancyBuilds += result.occupancyBuildCount;
                    sumGeneratedMoves += result.generatedMoves;
                    sumPrecheckRejects += result.visitedPrecheckRejects;
                    sumChildStates += result.childStatesCreated;
                    sumChildEnqueued += result.childStatesEnqueued;
                    sumDiscovered += result.discoveredStates;
                    if (result.queuePeakSize > maxQueuePeak)
                    {
                        maxQueuePeak = result.queuePeakSize;
                    }

                    if (result.searchLimitReached)
                    {
                        if (result.searchLimitReason == RushOutSolver.SearchLimitDiscovered)
                        {
                            discoveredLimitHits++;
                        }
                        else
                        {
                            exploredLimitHits++;
                        }
                    }

                    sumOccBuildMs += result.totalOccupancyBuildMs;
                    sumMoveGenMs += result.totalMoveGenerationMs;
                    sumVisitedMs += result.totalVisitedMs;
                    sumStateCopyMs += result.totalStateCopyMs;

                    if (result.invalid)
                    {
                        rejectedInvalid++;
                        DestroyImmediate(tempLevel);
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    if (result.searchLimitReached)
                    {
                        rejectedSearchLimit++;
                        if (candidate.usedConstructive)
                        {
                            rejectedConstructiveSearchLimit++;
                            constructiveSparseFiller = true;
                            searchLimitConstructiveSamples++;
                            sumSearchLimitBranching += candidate.initialBranching;
                            sumSearchLimitVehicles += candidate.vehicles.Count;
                            sumSearchLimitChain += candidate.chainLength;
                            Debug.Log(
                                "Search-limit constructive | chain=" + candidate.chainLength +
                                " | vehicles=" + candidate.vehicles.Count +
                                " | branching=" + candidate.initialBranching +
                                " | reason=" + (result.searchLimitReason ?? "-")
                            );
                        }

                        DestroyImmediate(tempLevel);
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        MaybeLogAttemptProgress(
                            totalAttempts,
                            maxAttemptsPerLevel,
                            sumPlacementMs,
                            sumSolverMs,
                            maxSolverMs,
                            sumTotalCandidateMs,
                            profiledCandidates,
                            rejectedPlacementFailed,
                            rejectedDensity,
                            rejectedUnsolvable,
                            rejectedSearchLimit
                        );
                        continue;
                    }

                    if (!result.solvable)
                    {
                        rejectedUnsolvable++;
                        DestroyImmediate(tempLevel);
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    if (candidate.usedConstructive)
                    {
                        solvedConstructiveCandidates++;
                        sumFinalMinimumMoves += result.minimumMoves;
                        finalMinimumMovesSamples++;
                        if (candidate.chainOnlyMinimumMoves > 0)
                        {
                            sumChainOnlyMinimumMoves += candidate.chainOnlyMinimumMoves;
                            chainOnlyMinimumMovesSamples++;
                        }

                        int secondaryUsed = CountSecondaryVehiclesInSolution(result, candidate);
                        secondaryUsedInOptimalSolution += secondaryUsed;
                        candidate.secondaryBlockersUsed = secondaryUsed;
                        if (candidate.secondaryVehicleNames != null &&
                            candidate.secondaryVehicleNames.Count > 0 &&
                            secondaryUsed == 0)
                        {
                            secondaryBypassed++;
                        }
                    }

                    // --- Post-solver quality filters ---
                    phaseWatch.Restart();
                    if (result.minimumMoves < effectiveMinMoves)
                    {
                        rejectedTooEasy++;
                        postSolverRejects++;
                        if (candidate.usedConstructive)
                        {
                            rejectedConstructiveTooEasy++;
                            constructiveChainBias = Mathf.Min(constructiveChainBias + 1, 1);
                            tooEasyConstructiveSamples++;
                            sumTooEasyMoves += result.minimumMoves;
                            int chainUsed = CountChainVehiclesInSolution(result, candidate);
                            float participation = candidate.chainLength > 0
                                ? (float)chainUsed / candidate.chainLength
                                : 0f;
                            sumChainParticipationTooEasy += (long)(participation * 1000);
                            chainParticipationTooEasySamples++;
                            Debug.Log(
                                "Too-easy constructive | moves=" + result.minimumMoves +
                                " | chain=" + candidate.chainLength +
                                " | chainUsedInSolution=" + chainUsed +
                                " | participation=" + participation.ToString("0.00") +
                                " | vehicles=" + candidate.vehicles.Count +
                                " | branching=" + candidate.initialBranching
                            );
                        }

                        DestroyImmediate(tempLevel);
                        sumReconstructMs += phaseWatch.Elapsed.TotalMilliseconds;
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    if (result.minimumMoves > effectiveMaxMoves)
                    {
                        rejectedTooHard++;
                        postSolverRejects++;
                        DestroyImmediate(tempLevel);
                        sumReconstructMs += phaseWatch.Elapsed.TotalMilliseconds;
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    int uniqueVehiclesMoved = CountUniqueVehiclesInSolution(result);
                    sumReconstructMs += phaseWatch.Elapsed.TotalMilliseconds;

                    SolutionComplexityMetrics structure = AnalyzeSolutionComplexity(
                        result,
                        candidate);
                    candidate.secondaryBlockersUsed = structure.secondaryBlockersUsed;
                    candidate.secondaryParticipationRatio =
                        candidate.secondaryVehicleNames != null &&
                        candidate.secondaryVehicleNames.Count > 0
                            ? (float)structure.secondaryBlockersUsed /
                              candidate.secondaryVehicleNames.Count
                            : 0f;

                    if (candidate.usedConstructive && candidate.dependencies != null &&
                        candidate.dependencies.Count > 0)
                    {
                        int chainUsed = CountChainVehiclesInSolution(result, candidate);
                        candidate.chainParticipationRatio = candidate.chainLength > 0
                            ? (float)chainUsed / candidate.chainLength
                            : 0f;
                        if (candidate.chainParticipationRatio < minChainParticipation)
                        {
                            rejectedLowChainParticipation++;
                            postSolverRejects++;
                            DestroyImmediate(tempLevel);
                            sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                            continue;
                        }

                        sumChainParticipationSolved += (long)(candidate.chainParticipationRatio * 1000);
                        chainParticipationSolvedSamples++;
                    }

                    if (uniqueVehiclesMoved < effectiveMinVehiclesUsedInSolution)
                    {
                        rejectedLowSolutionParticipation++;
                        postSolverRejects++;
                        DestroyImmediate(tempLevel);
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    phaseWatch.Restart();
                    int acceptanceScore = ComputeAcceptanceComplexityScore(structure);
                    structure.acceptanceComplexityScore = acceptanceScore;

                    // Fase 1: structure/complexity accept-gates alleen voor ConstructiveHard.
                    // Random gebruikt minimumMoves + unique vehicles + almost-solved.
                    if (generationStrategy == GenerationStrategy.ConstructiveHard)
                    {
                        StructureRejectReason structureReject = EvaluateSolutionStructure(
                            structure,
                            candidate);
                        acceptanceScore = ComputeAcceptanceComplexityScore(structure);
                        structure.acceptanceComplexityScore = acceptanceScore;
                        sumAxisAlternations += structure.axisAlternations;
                        sumVehicleRevisits += structure.vehicleRevisits;
                        sumLongestSingleAxisRun += structure.longestSingleAxisRun;
                        sumLinearChainRatio += (long)(structure.linearChainSolutionRatio * 1000);
                        sumSecondaryUsedSolved += structure.secondaryBlockersUsed;
                        sumForkDependenciesSolved += structure.forkDependencies;
                        sumAcceptanceComplexityScore += acceptanceScore;
                        acceptanceComplexitySamples++;
                        structureMetricSamples++;

                        if (structureReject != StructureRejectReason.None)
                        {
                            postSolverRejects++;
                            switch (structureReject)
                            {
                                case StructureRejectReason.RepetitiveRatio:
                                    rejectedRepetitiveSolution++;
                                    break;
                                case StructureRejectReason.LowComplexityScore:
                                    rejectedLowComplexityScore++;
                                    if (structure.linearChainSolutionRatio >
                                        effectiveMaxLinearChainSolutionRatio + 0.0001f)
                                    {
                                        rejectedLinearSolution++;
                                    }

                                    if (structure.axisAlternations < effectiveMinAxisAlternations ||
                                        structure.longestSingleAxisRun > effectiveMaxSingleAxisRun)
                                    {
                                        rejectedLowAxisAlternation++;
                                    }

                                    if (structure.vehicleRevisits < effectiveMinVehicleRevisits)
                                    {
                                        rejectedNoVehicleRevisit++;
                                    }

                                    if (structure.secondaryBlockersUsed < 1 &&
                                        effectiveMinSolutionComplexityScore > 0)
                                    {
                                        rejectedNoSecondaryBlocker++;
                                    }

                                    break;
                            }

                            sumRepetitiveMs += phaseWatch.Elapsed.TotalMilliseconds;
                            DestroyImmediate(tempLevel);
                            sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                            continue;
                        }
                    }

                    // Almost-solved: needs uniqueVehiclesMoved from solution (betekenis ongewijzigd).
                    if (minMinimumMoves >= 4 &&
                        directBlockers <= 1 &&
                        uniqueVehiclesMoved <= 2)
                    {
                        rejectedAlmostSolved++;
                        postSolverRejects++;
                        sumOtherFilterMs += phaseWatch.Elapsed.TotalMilliseconds;
                        DestroyImmediate(tempLevel);
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    sumRepetitiveMs += phaseWatch.Elapsed.TotalMilliseconds;

                    // Geaccepteerd → opslaan.
                    int batchNumber = accepted + 1;
                    tempLevel.levelNumber = batchNumber;
                    tempLevel.difficulty = tier;
                    LevelSolver.ApplyMetadata(tempLevel, result);
                    WriteSolutionStructureToLevel(tempLevel, structure, candidate, result);

                    string fileName = safeBatchName + "_" + nextFileIndex.ToString("000") + ".asset";
                    string assetPath = outputFolder.TrimEnd('/', '\\') + "/" + fileName;

                    AssetDatabase.CreateAsset(tempLevel, assetPath);
                    EditorUtility.SetDirty(tempLevel);

                    accepted++;
                    sumAcceptedMinimumMoves += result.minimumMoves;
                    existingLevelKeys.Add(layoutKey);
                    nextFileIndex++;
                    found = true;

                    candidateWatch.Stop();
                    double candidateMs = candidateWatch.Elapsed.TotalMilliseconds;
                    sumTotalCandidateMs += candidateMs;
                    if (candidateMs > maxCandidateMs)
                    {
                        maxCandidateMs = candidateMs;
                    }

                    Debug.Log(
                        candidate.usedConstructive
                            ? ("Accepted ConstructiveHard" +
                               "\ngrid=" + gridWidth + "x" + gridHeight +
                               "\nmoves=" + tempLevel.minimumMoves +
                               "\nuniqueVehicles=" + structure.uniqueVehiclesMoved +
                               "\naxisAlternations=" + structure.axisAlternations +
                               "\nvehicleRevisits=" + structure.vehicleRevisits +
                               "\nlongestSingleAxisRun=" + structure.longestSingleAxisRun +
                               "\nlinearChainRatio=" +
                               structure.linearChainSolutionRatio.ToString("0.00") +
                               "\nsecondaryUsed=" + structure.secondaryBlockersUsed +
                               "\nforkUsed=" + structure.forkDependencies +
                               "\ncomplexityScore=" + acceptanceScore +
                               "\nchain=" + candidate.chainLength +
                               "\nvehicles=" + candidate.vehicles.Count +
                               "\n→ " + assetPath)
                            : ("Accepted | tier=" + tier +
                               " | moves=" + tempLevel.minimumMoves +
                               " | uniqueVehicles=" + uniqueVehiclesMoved +
                               " | occupancy=" + occupancy.ToString("0.00") +
                               " | movableRatio=" + movableRatio.ToString("0.00") +
                               " | score=" + tempLevel.difficultyScore +
                               " | candidateMs=" + candidateMs.ToString("0.0") +
                               " | solverMs=" + solverMs.ToString("0.0") +
                               " | → " + assetPath)
                    );
                    break;
                }

                if (cancelGeneration)
                {
                    break;
                }

                if (!found)
                {
                    int levelAttempts = totalAttempts - levelAttemptStart;
                    Debug.LogWarning(
                        "LevelGenerator: kon level " + (levelIndex + 1) +
                        "/" + numberOfLevels +
                        " niet vinden (attempts this level≈" + levelAttempts +
                        ", maxAttempts=" + maxAttemptsPerLevel +
                        ", timeout=" + maxGenerationSecondsPerLevel + "s)."
                    );
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        batchWatch.Stop();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        double avgPlacement = profiledCandidates > 0 ? sumPlacementMs / profiledCandidates : 0;
        double avgSolver = profiledCandidates > 0 ? sumSolverMs / profiledCandidates : 0;
        double avgTotal = profiledCandidates > 0 ? sumTotalCandidateMs / profiledCandidates : 0;
        double avgSolverPerInvocation =
            solverInvocations > 0 ? sumSolverMs / solverInvocations : 0;
        double solverInvocationRatio =
            totalAttempts > 0 ? (double)solverInvocations / totalAttempts : 0;
        double acceptanceRate =
            totalAttempts > 0 ? (double)accepted / totalAttempts : 0;
        double averageMinimumMovesAccepted =
            accepted > 0 ? (double)sumAcceptedMinimumMoves / accepted : 0;

        if (generationStrategy == GenerationStrategy.Random)
        {
            StringBuilder randomSummary = new StringBuilder();
            randomSummary.AppendLine("=== LevelGenerator klaar (Random) ===");
            randomSummary.AppendLine(
                "total elapsed seconds: " + batchWatch.Elapsed.TotalSeconds.ToString("0.00")
            );
            randomSummary.AppendLine("attempts: " + totalAttempts);
            randomSummary.AppendLine("accepted: " + accepted);
            randomSummary.AppendLine(
                "acceptanceRate: " + acceptanceRate.ToString("0.000") +
                " (" + accepted + "/" + totalAttempts + ")"
            );
            randomSummary.AppendLine(
                "averageMinimumMovesAccepted: " +
                (accepted > 0 ? averageMinimumMovesAccepted.ToString("0.00") : "n/a")
            );
            randomSummary.AppendLine("solverInvocations: " + solverInvocations);
            randomSummary.AppendLine(
                "solverInvocationRatio: " + solverInvocationRatio.ToString("0.000") +
                " (" + solverInvocations + "/" + totalAttempts + ")"
            );
            randomSummary.AppendLine(
                "average solver ms: " + avgSolverPerInvocation.ToString("0.00")
            );
            randomSummary.AppendLine("maximum solver ms: " + maxSolverMs.ToString("0.00"));
            randomSummary.AppendLine("");
            randomSummary.AppendLine("--- rejected ---");
            randomSummary.AppendLine("placement failed: " + rejectedPlacementFailed);
            randomSummary.AppendLine("density: " + rejectedDensity);
            randomSummary.AppendLine("movable ratio: " + rejectedMovableRatio);
            randomSummary.AppendLine("blockers: " + rejectedBlockers);
            randomSummary.AppendLine("trivial: " + rejectedTrivial);
            randomSummary.AppendLine("duplicate: " + rejectedDuplicates);
            randomSummary.AppendLine(
                "  · duplicates rejected this run: " + rejectedDuplicatesThisRun
            );
            randomSummary.AppendLine(
                "  · duplicates rejected against existing assets: " +
                rejectedDuplicatesAgainstExisting
            );
            randomSummary.AppendLine("unsolvable: " + rejectedUnsolvable);
            randomSummary.AppendLine("search limit: " + rejectedSearchLimit);
            randomSummary.AppendLine("too easy: " + rejectedTooEasy);
            randomSummary.AppendLine("too hard: " + rejectedTooHard);
            randomSummary.AppendLine(
                "low solution participation: " + rejectedLowSolutionParticipation
            );
            randomSummary.AppendLine("almost solved: " + rejectedAlmostSolved);
            randomSummary.AppendLine(
                "orientation/length balance: " + rejectedOrientationLengthBalance
            );
            randomSummary.AppendLine("invalid: " + rejectedInvalid);
            if (accepted == 0)
            {
                string topReject = FindTopRandomRejectReason(
                    rejectedPlacementFailed,
                    rejectedDensity,
                    rejectedMovableRatio,
                    rejectedBlockers,
                    rejectedTrivial,
                    rejectedDuplicates,
                    rejectedUnsolvable,
                    rejectedSearchLimit,
                    rejectedTooEasy,
                    rejectedTooHard,
                    rejectedLowSolutionParticipation,
                    rejectedAlmostSolved,
                    rejectedOrientationLengthBalance,
                    rejectedInvalid
                );
                randomSummary.AppendLine("");
                randomSummary.AppendLine("TOP REJECT REASON (accepted=0): " + topReject);
            }

            randomSummary.AppendLine("");
            randomSummary.AppendLine("--- batch ---");
            randomSummary.AppendLine(
                "existing level keys loaded: " + existingLevelKeysLoaded +
                " (generated scanned=" + generatedAssetsScanned +
                ", database scanned=" + databaseLevelsScanned +
                ", existing duplicate reports=" + existingDuplicateReports + ")"
            );
            randomSummary.AppendLine("grid: " + gridWidth + "x" + gridHeight);
            randomSummary.AppendLine(
                "vehicle count range: " + effectiveMinVehicles + "-" + effectiveMaxVehicles
            );
            randomSummary.AppendLine(
                "length 4: " +
                (allowLength4Vehicles
                    ? ("enabled, max " + maxLength4Vehicles)
                    : "disabled")
            );
            randomSummary.AppendLine(
                "minimumMoves range: " + effectiveMinMoves + "-" + effectiveMaxMoves
            );
            randomSummary.AppendLine("preset: " + difficultyPreset);
            randomSummary.AppendLine("output folder: " + outputFolder);
            randomSummary.AppendLine("(niet toegevoegd aan MainLevelDatabase)");
            Debug.Log(randomSummary.ToString());
            cancelGeneration = false;
            return;
        }

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("=== LevelGenerator klaar ===");
        summary.AppendLine("total elapsed seconds: " + batchWatch.Elapsed.TotalSeconds.ToString("0.00"));
        summary.AppendLine("attempts: " + totalAttempts);
        summary.AppendLine("accepted: " + accepted);
        summary.AppendLine(
            "acceptanceRate: " + acceptanceRate.ToString("0.000") +
            " (" + accepted + "/" + totalAttempts + ")"
        );
        summary.AppendLine("solverInvocations: " + solverInvocations);
        summary.AppendLine(
            "solverInvocationRatio: " + solverInvocationRatio.ToString("0.000") +
            " (" + solverInvocations + "/" + totalAttempts + ")"
        );
        summary.AppendLine("candidates rejected before solver: " + rejectedBeforeSolverCount);
        summary.AppendLine("pre-solver rejects: " + rejectedBeforeSolverCount);
        summary.AppendLine("post-solver rejects: " + postSolverRejects);
        summary.AppendLine("secondary creation attempts: " + secondaryCreationAttempts);
        summary.AppendLine("secondary candidate placements: " + secondaryCandidatePlacements);
        summary.AppendLine("secondary committed: " + secondaryCommitted);
        summary.AppendLine(
            "secondary rejected after placement: " + secondaryRejectedAfterPlacement
        );
        summary.AppendLine(
            "secondary used in optimal solution: " + secondaryUsedInOptimalSolution
        );
        summary.AppendLine("secondary bypassed: " + secondaryBypassed);
        summary.AppendLine(
            "secondary commit ratio: " +
            (secondaryCreationAttempts > 0
                ? ((double)secondaryCommitted / secondaryCreationAttempts).ToString("0.000")
                : "0")
        );
        summary.AppendLine("  · no eligible chain node: " + secondaryNoEligibleChainNode);
        summary.AppendLine("  · no clearance path: " + secondaryNoRequiredClearanceCell);
        summary.AppendLine(
            "  · no valid placement through blocked cell: " + secondaryNoPlacementCandidate
        );
        summary.AppendLine("  · secondary cannot move: " + secondaryCannotMove);
        summary.AppendLine(
            "  · does not block required displacement: " + secondaryDoesNotBlock
        );
        summary.AppendLine(
            "  · moving secondary does not unlock: " + secondaryDoesNotUnlock
        );
        summary.AppendLine("  · overlap: " + secondaryOverlap);
        summary.AppendLine("  · out of bounds: " + secondaryOutOfBounds);
        summary.AppendLine("  · would make unsolvable: " + secondaryWouldMakeUnsolvable);
        summary.AppendLine("  · other: " + secondaryOther);
        summary.AppendLine("fork creation attempted: " + forkCreationAttempted);
        summary.AppendLine("fork creation successful: " + forkCreationSuccessful);
        summary.AppendLine(
            "fork success ratio: " +
            (forkCreationAttempted > 0
                ? ((double)forkCreationSuccessful / forkCreationAttempted).ToString("0.000")
                : "0")
        );
        summary.AppendLine("  · no eligible node: " + forkNoEligibleNode);
        summary.AppendLine("  · not enough clearance cells: " + forkNotEnoughClearanceCells);
        summary.AppendLine("  · placement failed: " + forkPlacementFailed);
        summary.AppendLine("  · overlap: " + forkOverlap);
        summary.AppendLine("  · out of bounds: " + forkOutOfBounds);
        summary.AppendLine("  · would break chain: " + forkWouldBreakChain);
        summary.AppendLine("  · other: " + forkOther);
        summary.AppendLine(
            "average branching before fillers: " +
            (branchingBeforeFillersSamples > 0
                ? ((double)sumBranchingBeforeFillers / branchingBeforeFillersSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average branching after fillers: " +
            (branchingAfterFillersSamples > 0
                ? ((double)sumBranchingAfterFillers / branchingAfterFillersSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average vehicle count before fillers: " +
            (vehiclesBeforeFillersSamples > 0
                ? ((double)sumVehiclesBeforeFillers / vehiclesBeforeFillersSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average solution complexity score: " +
            (acceptanceComplexitySamples > 0
                ? ((double)sumAcceptanceComplexityScore / acceptanceComplexitySamples)
                    .ToString("0.00")
                : "0")
        );
        summary.AppendLine("rejected low complexity score: " + rejectedLowComplexityScore);
        summary.AppendLine(
            "average chain-only moves of shortcut rejects: " +
            (shortcutRejectSamples > 0
                ? ((double)sumShortcutChainOnlyMoves / shortcutRejectSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average chain length of shortcut rejects: " +
            (shortcutRejectSamples > 0
                ? ((double)sumShortcutChainLength / shortcutRejectSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine("average ms per candidate: " + avgTotal.ToString("0.00"));
        summary.AppendLine("average placement ms: " + avgPlacement.ToString("0.00"));
        summary.AppendLine("average solver ms (all attempts): " + avgSolver.ToString("0.00"));
        summary.AppendLine(
            "average solver ms per invocation: " + avgSolverPerInvocation.ToString("0.00")
        );
        summary.AppendLine("maximum solver ms: " + maxSolverMs.ToString("0.00"));
        summary.AppendLine("slowest candidate ms: " + maxCandidateMs.ToString("0.00"));
        summary.AppendLine("total placement time ms: " + sumPlacementMs.ToString("0.0"));
        summary.AppendLine("total solver time ms: " + sumSolverMs.ToString("0.0"));
        summary.AppendLine("--- solver hot-path ---");
        summary.AppendLine("statesExplored (sum): " + sumStatesExplored);
        summary.AppendLine("occupancyBuildCount (sum): " + sumOccupancyBuilds);
        summary.AppendLine(
            "occupancyBuilds≈statesExplored: " +
            (sumOccupancyBuilds == sumStatesExplored ? "YES" : "NO")
        );
        summary.AppendLine("generatedMoves (sum): " + sumGeneratedMoves);
        summary.AppendLine("visitedPrecheckRejects (sum): " + sumPrecheckRejects);
        summary.AppendLine("childStatesCreated (sum): " + sumChildStates);
        summary.AppendLine("childStatesEnqueued (sum): " + sumChildEnqueued);
        summary.AppendLine("discoveredStates (sum): " + sumDiscovered);
        summary.AppendLine("queuePeakSize (max): " + maxQueuePeak);
        summary.AppendLine("searchLimit explored hits: " + exploredLimitHits);
        summary.AppendLine("searchLimit discovered hits: " + discoveredLimitHits);
        summary.AppendLine("totalOccupancyBuildMs: " + sumOccBuildMs.ToString("0.00"));
        summary.AppendLine("totalMoveGenerationMs: " + sumMoveGenMs.ToString("0.00"));
        summary.AppendLine("totalVisitedMs: " + sumVisitedMs.ToString("0.00"));
        summary.AppendLine("totalStateCopyMs: " + sumStateCopyMs.ToString("0.00"));
        summary.AppendLine("total density filter ms: " + sumDensityMs.ToString("0.0"));
        summary.AppendLine("total movable filter ms: " + sumMovableMs.ToString("0.0"));
        summary.AppendLine("total duplicate filter ms: " + sumDuplicateMs.ToString("0.0"));
        summary.AppendLine("total reconstruct/quality ms: " + sumReconstructMs.ToString("0.0"));
        summary.AppendLine("total repetitive filter ms: " + sumRepetitiveMs.ToString("0.0"));
        summary.AppendLine("total other filter ms: " + sumOtherFilterMs.ToString("0.0"));
        summary.AppendLine("maxSolverStatesPerCandidate: " + maxSolverStatesPerCandidate);
        summary.AppendLine("maxGenerationSecondsPerLevel: " + maxGenerationSecondsPerLevel);
        summary.AppendLine("cancelled: " + cancelGeneration);
        summary.AppendLine("rejected unsolvable: " + rejectedUnsolvable);
        summary.AppendLine("rejected too easy: " + rejectedTooEasy);
        summary.AppendLine("rejected too hard: " + rejectedTooHard);
        summary.AppendLine("rejected duplicate: " + rejectedDuplicates);
        summary.AppendLine(
            "  · duplicates rejected this run: " + rejectedDuplicatesThisRun
        );
        summary.AppendLine(
            "  · duplicates rejected against existing assets: " +
            rejectedDuplicatesAgainstExisting
        );
        summary.AppendLine(
            "existing level keys loaded: " + existingLevelKeysLoaded
        );
        summary.AppendLine("rejected low solution participation: " + rejectedLowSolutionParticipation);
        summary.AppendLine("rejected density: " + rejectedDensity);
        summary.AppendLine("rejected movable ratio: " + rejectedMovableRatio);
        summary.AppendLine("rejected excessive branching: " + rejectedExcessiveBranching);
        summary.AppendLine("rejected repetitive solution: " + rejectedRepetitiveSolution);
        summary.AppendLine("rejected linear solution: " + rejectedLinearSolution);
        summary.AppendLine("rejected low axis alternation: " + rejectedLowAxisAlternation);
        summary.AppendLine("rejected no vehicle revisit: " + rejectedNoVehicleRevisit);
        summary.AppendLine("rejected no secondary blocker: " + rejectedNoSecondaryBlocker);
        summary.AppendLine(
            "rejected insufficient dependency complexity: " +
            rejectedInsufficientDependencyComplexity
        );
        summary.AppendLine(
            "average axis alternations (solved): " +
            (structureMetricSamples > 0
                ? ((double)sumAxisAlternations / structureMetricSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average vehicle revisits (solved): " +
            (structureMetricSamples > 0
                ? ((double)sumVehicleRevisits / structureMetricSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average longest single-axis run (solved): " +
            (structureMetricSamples > 0
                ? ((double)sumLongestSingleAxisRun / structureMetricSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average linear chain ratio (solved): " +
            (structureMetricSamples > 0
                ? ((double)sumLinearChainRatio / structureMetricSamples / 1000.0).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average secondary used (solved): " +
            (structureMetricSamples > 0
                ? ((double)sumSecondaryUsedSolved / structureMetricSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average fork dependencies (solved): " +
            (structureMetricSamples > 0
                ? ((double)sumForkDependenciesSolved / structureMetricSamples).ToString("0.00")
                : "0")
        );
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
        summary.AppendLine("  · chain creation failed: " + failChainCreation);
        summary.AppendLine("    · no target blocker: " + failChainNoTargetBlocker);
        summary.AppendLine("    · no dependency cell: " + failChainNoDependencyCell);
        summary.AppendLine("    · no valid orientation: " + failChainNoValidOrientation);
        summary.AppendLine("    · placement exhausted: " + failChainPlacementExhausted);
        summary.AppendLine("    · backtrack exhausted: " + failChainBacktrackExhausted);
        for (int i = 0; i < chainNodePlacedCounts.Length; i++)
        {
            summary.AppendLine("    · chainNode" + i + "Placed: " + chainNodePlacedCounts[i]);
        }

        summary.AppendLine(
            "    · average deepest chain node: " +
            (deepestChainNodeSamples > 0
                ? ((double)sumDeepestChainNode / deepestChainNodeSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine("rejected broken chain: " + rejectedBrokenChain);
        summary.AppendLine(
            "  · previous cell not blocked: " + brokenChainPreviousCriticalCellNotBlocked
        );
        summary.AppendLine(
            "  · required move does not clear: " + brokenChainRequiredMoveDoesNotClear
        );
        summary.AppendLine(
            "  · dependency does not block: " + brokenChainDependencyDoesNotBlockRequiredMove
        );
        summary.AppendLine(
            "  · alternative shortcut: " + brokenChainAlternativeDirectionCreatesShortcut
        );
        summary.AppendLine(
            "  · invalid dependency reference: " + brokenChainInvalidDependencyReference
        );
        summary.AppendLine(
            "  · invalid displacement: " + brokenChainRequiredDisplacementInvalid
        );
        summary.AppendLine(
            "  · out of bounds: " + brokenChainOutOfBoundsClearance
        );
        summary.AppendLine("  · other: " + brokenChainOther);
        summary.AppendLine("rejected chain shortcut: " + rejectedChainShortcut);
        summary.AppendLine("rejected low chain participation: " + rejectedLowChainParticipation);
        summary.AppendLine("strategy: " + generationStrategy);
        summary.AppendLine(
            "effective movable ratio: " +
            GetEffectiveMinMovableRatio().ToString("0.00") + "-" +
            GetEffectiveMaxMovableRatio().ToString("0.00")
        );
        summary.AppendLine(
            "initial branching range: " + effectiveMinBranching + "-" + effectiveMaxBranching +
            " (configured " + minInitialBranching + "-" + maxInitialBranching + ")"
        );
        summary.AppendLine(
            "average initial branching: " +
            (initialBranchingSamples > 0
                ? ((double)sumInitialBranching / initialBranchingSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average branching of search-limit candidates: " +
            (searchLimitConstructiveSamples > 0
                ? ((double)sumSearchLimitBranching / searchLimitConstructiveSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average vehicles of search-limit candidates: " +
            (searchLimitConstructiveSamples > 0
                ? ((double)sumSearchLimitVehicles / searchLimitConstructiveSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average chain length of search-limit candidates: " +
            (searchLimitConstructiveSamples > 0
                ? ((double)sumSearchLimitChain / searchLimitConstructiveSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average minimumMoves of solved-too-easy candidates: " +
            (tooEasyConstructiveSamples > 0
                ? ((double)sumTooEasyMoves / tooEasyConstructiveSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine("constructive attempts: " + constructiveAttempts);
        summary.AppendLine("chain creation failed: " + chainCreationFailed);
        summary.AppendLine(
            "average planned chain length: " +
            (plannedChainLengthSamples > 0
                ? ((double)sumPlannedChainLength / plannedChainLengthSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average valid chain length: " +
            (validChainLengthSamples > 0
                ? ((double)sumValidChainLength / validChainLengthSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average initial chain length: " +
            (initialChainLengthSamples > 0
                ? ((double)sumInitialChainLength / initialChainLengthSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average extended chain length: " +
            (extendedChainLengthSamples > 0
                ? ((double)sumExtendedChainLength / extendedChainLengthSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine("chain extensions attempted: " + chainExtensionsAttempted);
        summary.AppendLine("chain extensions successful: " + chainExtensionsSuccessful);
        summary.AppendLine("secondary committed: " + secondaryCommitted);
        summary.AppendLine(
            "secondary used in optimal solution: " + secondaryUsedInOptimalSolution
        );
        summary.AppendLine(
            "average chain-only moves before extension: " +
            (chainOnlyMovesBeforeExtensionSamples > 0
                ? ((double)sumChainOnlyMovesBeforeExtension /
                   chainOnlyMovesBeforeExtensionSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average chain-only moves after extension: " +
            (chainOnlyMovesAfterExtensionSamples > 0
                ? ((double)sumChainOnlyMovesAfterExtension /
                   chainOnlyMovesAfterExtensionSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average chain length (built): " +
            (chainLengthSamples > 0
                ? ((double)sumChainLength / chainLengthSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average chain participation (solved): " +
            (chainParticipationSolvedSamples > 0
                ? ((double)sumChainParticipationSolved / chainParticipationSolvedSamples / 1000.0
                ).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average chain participation (too-easy): " +
            (chainParticipationTooEasySamples > 0
                ? ((double)sumChainParticipationTooEasy / chainParticipationTooEasySamples / 1000.0
                ).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average movable vehicle count: " +
            (initialMovableVehicleSamples > 0
                ? ((double)sumInitialMovableVehicles / initialMovableVehicleSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average destination move count: " +
            (destinationMoveCountSamples > 0
                ? ((double)sumDestinationMoveCount / destinationMoveCountSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average chain-only minimumMoves: " +
            (chainOnlyMinimumMovesSamples > 0
                ? ((double)sumChainOnlyMinimumMoves / chainOnlyMinimumMovesSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine(
            "average final minimumMoves (solved constructive): " +
            (finalMinimumMovesSamples > 0
                ? ((double)sumFinalMinimumMoves / finalMinimumMovesSamples).ToString("0.00")
                : "0")
        );
        summary.AppendLine("solved constructive candidates: " + solvedConstructiveCandidates);
        summary.AppendLine("rejected constructed solution floor: " + rejectedConstructedSolutionFloor);
        summary.AppendLine("rejected constructive too easy: " + rejectedConstructiveTooEasy);
        summary.AppendLine("rejected constructive search limit: " + rejectedConstructiveSearchLimit);
        summary.AppendLine(
            "blocking chain range: " + effectiveMinChain + "-" + effectiveMaxChain +
            " (configured " + minBlockingChainLength + "-" + maxBlockingChainLength + ")"
        );
        summary.AppendLine(
            "secondary blockers range: " + effectiveMinSecondary + "-" + effectiveMaxSecondary +
            " (configured " + minSecondaryBlockers + "-" + maxSecondaryBlockers +
            ", optional enrichment)"
        );
        summary.AppendLine(
            "pre-solver move floor: " + effectivePreSolverMoveFloor +
            " (full solver min moves: " + effectiveMinMoves + ")"
        );
        summary.AppendLine(
            "min solution complexity score: " + effectiveMinSolutionComplexityScore
        );
        summary.AppendLine("max filler travel distance: " + effectiveMaxFillerTravel);
        summary.AppendLine("grid: " + gridWidth + "x" + gridHeight);
        summary.AppendLine(
            "vehicles: " + effectiveMinVehicles + "-" + effectiveMaxVehicles +
            " (configured " + minVehicles + "-" + maxVehicles + ")"
        );
        summary.AppendLine(
            "moves: " + effectiveMinMoves + "-" + effectiveMaxMoves +
            " (configured " + minMinimumMoves + "-" + maxMinimumMoves + ")"
        );
        summary.AppendLine(
            "occupancy targets: " +
            effectiveMinOccupancy.ToString("0.00") + "-" +
            effectiveMaxOccupancy.ToString("0.00") +
            " (configured " + minBoardOccupancy.ToString("0.00") + "-" +
            maxBoardOccupancy.ToString("0.00") + ")"
        );
        summary.AppendLine("area scale: " + AreaScale.ToString("0.00"));
        summary.AppendLine("preset: " + difficultyPreset);
        summary.AppendLine("batch: " + safeBatchName);
        summary.AppendLine("tier: " + tier);
        summary.AppendLine("output: " + outputFolder);
        summary.AppendLine("(niet toegevoegd aan MainLevelDatabase)");
        summary.AppendLine("--- AUDIT NOTES ---");
        summary.AppendLine(
            "Visited: HashSet<BoardState> met structurele Equals (hash collisions veilig)."
        );
        summary.AppendLine(
            "Occupancy: één reusable int[] per BFS, 1 BuildOccupancy per dequeued state."
        );
        summary.AppendLine(
            "Moves: Zobrist pre-check vóór state-copy; discover mark bij enqueue; " +
            "maxDiscoveredStates=maxSolverStates."
        );
        summary.AppendLine(
            "ConstructiveHard: pre-solver chain + soft floor; secondary/fork optional enrichment; " +
            "post-solver composite complexity score ≥ " + effectiveMinSolutionComplexityScore +
            "; participation ≥ " + minChainParticipation.ToString("0.00") +
            ", efficiency ≥ " + minChainMoveEfficiency.ToString("0.00") +
            "; auto tuning optioneel."
        );

        Debug.Log(summary.ToString());
        cancelGeneration = false;
    }

    private static string FindTopRandomRejectReason(
        int rejectedPlacementFailed,
        int rejectedDensity,
        int rejectedMovableRatio,
        int rejectedBlockers,
        int rejectedTrivial,
        int rejectedDuplicates,
        int rejectedUnsolvable,
        int rejectedSearchLimit,
        int rejectedTooEasy,
        int rejectedTooHard,
        int rejectedLowSolutionParticipation,
        int rejectedAlmostSolved,
        int rejectedOrientationLengthBalance,
        int rejectedInvalid)
    {
        string topName = "none";
        int topCount = -1;

        CompareTop(ref topName, ref topCount, "placement failed", rejectedPlacementFailed);
        CompareTop(ref topName, ref topCount, "density", rejectedDensity);
        CompareTop(ref topName, ref topCount, "movable ratio", rejectedMovableRatio);
        CompareTop(ref topName, ref topCount, "blockers", rejectedBlockers);
        CompareTop(ref topName, ref topCount, "trivial", rejectedTrivial);
        CompareTop(ref topName, ref topCount, "duplicate", rejectedDuplicates);
        CompareTop(ref topName, ref topCount, "unsolvable", rejectedUnsolvable);
        CompareTop(ref topName, ref topCount, "search limit", rejectedSearchLimit);
        CompareTop(ref topName, ref topCount, "too easy", rejectedTooEasy);
        CompareTop(ref topName, ref topCount, "too hard", rejectedTooHard);
        CompareTop(
            ref topName,
            ref topCount,
            "low solution participation",
            rejectedLowSolutionParticipation
        );
        CompareTop(ref topName, ref topCount, "almost solved", rejectedAlmostSolved);
        CompareTop(
            ref topName,
            ref topCount,
            "orientation/length balance",
            rejectedOrientationLengthBalance
        );
        CompareTop(ref topName, ref topCount, "invalid", rejectedInvalid);

        return topCount <= 0 ? "none" : topName + " (" + topCount + ")";
    }

    private static void CompareTop(
        ref string topName,
        ref int topCount,
        string name,
        int count)
    {
        if (count > topCount)
        {
            topCount = count;
            topName = name;
        }
    }

    private void MaybeLogAttemptProgress(
        int attempt,
        int maxAttempts,
        double sumPlacementMs,
        double sumSolverMs,
        double maxSolverMs,
        double sumTotalCandidateMs,
        int profiledCandidates,
        int rejectedPlacement,
        int rejectedDensity,
        int rejectedUnsolvable,
        int rejectedSearchLimit)
    {
        if (attempt <= 0 || attempt % 250 != 0)
        {
            return;
        }

        double avgPlacement = profiledCandidates > 0 ? sumPlacementMs / profiledCandidates : 0;
        double avgSolver = profiledCandidates > 0 ? sumSolverMs / profiledCandidates : 0;
        double avgTotal = profiledCandidates > 0 ? sumTotalCandidateMs / profiledCandidates : 0;

        Debug.Log(
            "Attempt " + attempt + " / " + maxAttempts +
            "\nAverage placement ms: " + avgPlacement.ToString("0.00") +
            "\nAverage solver ms: " + avgSolver.ToString("0.00") +
            "\nMaximum solver ms: " + maxSolverMs.ToString("0.00") +
            "\nAverage total candidate ms: " + avgTotal.ToString("0.00") +
            "\nRejected placement: " + rejectedPlacement +
            "\nRejected density: " + rejectedDensity +
            "\nRejected unsolvable: " + rejectedUnsolvable +
            "\nRejected search limit: " + rejectedSearchLimit
        );

        // Laat de Editor repainten / cancelbar updaten.
        bool cancel = EditorUtility.DisplayCancelableProgressBar(
            "Generating Levels",
            "Attempt " + attempt + " / " + maxAttempts,
            Mathf.Clamp01((float)attempt / Mathf.Max(1, maxAttempts))
        );
        if (cancel)
        {
            cancelGeneration = true;
        }

        Repaint();
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
        Overlap,
        ChainCreationFailed,
        ChainBroken,
        ChainShortcut,
        ConstructedSolutionFloor,
        InsufficientDependencyComplexity
    }

    private void ResetPlacementDiagnostics()
    {
        failTargetPlacement = 0;
        failDirectBlockerPlacement = 0;
        failOrdinaryVehiclePlacement = 0;
        failCouldNotReachVehicleCount = 0;
        failInvalidBounds = 0;
        failOverlap = 0;
        failChainCreation = 0;
        failChainNoTargetBlocker = 0;
        failChainNoDependencyCell = 0;
        failChainNoValidOrientation = 0;
        failChainPlacementExhausted = 0;
        failChainBacktrackExhausted = 0;
        brokenChainVerboseLogged = 0;
        brokenChainPreviousCriticalCellNotBlocked = 0;
        brokenChainRequiredMoveDoesNotClear = 0;
        brokenChainDependencyDoesNotBlockRequiredMove = 0;
        brokenChainAlternativeDirectionCreatesShortcut = 0;
        brokenChainInvalidDependencyReference = 0;
        brokenChainRequiredDisplacementInvalid = 0;
        brokenChainOutOfBoundsClearance = 0;
        brokenChainOther = 0;
        chainExtensionsAttempted = 0;
        chainExtensionsSuccessful = 0;
        secondaryCreationAttempts = 0;
        secondaryCandidatePlacements = 0;
        secondaryCommitted = 0;
        secondaryRejectedAfterPlacement = 0;
        secondaryUsedInOptimalSolution = 0;
        secondaryBypassed = 0;
        forkCreationAttempted = 0;
        forkCreationSuccessful = 0;
        secondaryVerboseLogged = 0;
        forkVerboseLogged = 0;
        secondaryNoEligibleChainNode = 0;
        secondaryNoRequiredClearanceCell = 0;
        secondaryNoPlacementCandidate = 0;
        secondaryCannotMove = 0;
        secondaryDoesNotBlock = 0;
        secondaryDoesNotUnlock = 0;
        secondaryOverlap = 0;
        secondaryOutOfBounds = 0;
        secondaryWouldBreakChain = 0;
        secondaryCreatesShortcut = 0;
        secondaryWouldMakeUnsolvable = 0;
        secondaryOther = 0;
        forkNoEligibleNode = 0;
        forkNotEnoughClearanceCells = 0;
        forkPlacementFailed = 0;
        forkOverlap = 0;
        forkOutOfBounds = 0;
        forkWouldBreakChain = 0;
        forkOther = 0;
        sumBranchingBeforeFillers = 0;
        branchingBeforeFillersSamples = 0;
        sumBranchingAfterFillers = 0;
        branchingAfterFillersSamples = 0;
        sumVehiclesBeforeFillers = 0;
        vehiclesBeforeFillersSamples = 0;
        sumInitialChainLength = 0;
        initialChainLengthSamples = 0;
        sumExtendedChainLength = 0;
        extendedChainLengthSamples = 0;
        sumChainOnlyMovesBeforeExtension = 0;
        chainOnlyMovesBeforeExtensionSamples = 0;
        sumChainOnlyMovesAfterExtension = 0;
        chainOnlyMovesAfterExtensionSamples = 0;
        for (int i = 0; i < chainNodePlacedCounts.Length; i++)
        {
            chainNodePlacedCounts[i] = 0;
        }

        sumDeepestChainNode = 0;
        deepestChainNodeSamples = 0;
        lastDeepestChainNode = 0;
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
            case PlacementFailReason.ChainCreationFailed:
                failChainCreation++;
                break;
            case PlacementFailReason.ChainBroken:
            case PlacementFailReason.ChainShortcut:
            case PlacementFailReason.ConstructedSolutionFloor:
            case PlacementFailReason.InsufficientDependencyComplexity:
                break;
        }
    }

    private CandidateLevel TryBuildCandidate(
        System.Random rng,
        out PlacementFailReason failReason)
    {
        if (generationStrategy == GenerationStrategy.ConstructiveHard)
        {
            return TryBuildConstructiveHardCandidate(rng, out failReason);
        }

        return TryBuildRandomCandidate(rng, out failReason);
    }

    private CandidateLevel TryBuildRandomCandidate(
        System.Random rng,
        out PlacementFailReason failReason)
    {
        failReason = PlacementFailReason.None;
        int vehicleCount = rng.Next(effectiveMinVehicles, effectiveMaxVehicles + 1);

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
        int placementIterations = 0;
        while (candidate.vehicles.Count < vehicleCount)
        {
            placementIterations++;
            if (placementIterations > maxPlacementIterationsPerCandidate)
            {
                failReason = PlacementFailReason.CouldNotReachVehicleCount;
                return null;
            }

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

    // -------------------------------------------------------------------------
    // Constructive Hard — real chain dependencies
    // -------------------------------------------------------------------------

    private sealed class ChainDependency
    {
        public int vehicleIndex;
        public int blockedVehicleIndex = -1; // -1 = target
        public int requiredMoveDirection; // +1/-1 along vehicle axis
        public int requiredDisplacement;
        public Vector2Int blockedCriticalCell;
        public Vector2Int nextDependencyCell;
        /// <summary>
        /// Exacte critical cells die tijdens construction vrij moeten worden (gedeeld met validator).
        /// </summary>
        public List<Vector2Int> previousCriticalCells = new List<Vector2Int>();
        public List<Vector2Int> requiredClearanceCells = new List<Vector2Int>();
        public int dependencyVehicleIndex = -1;
        /// <summary>Secondary die een tweede clearance-cell blokkeert (fork).</summary>
        public int forkSecondaryVehicleIndex = -1;
        public CandidateVehicle vehicle;
    }

    private enum BrokenChainReason
    {
        None,
        PreviousCriticalCellNotBlocked,
        RequiredMoveDoesNotClear,
        DependencyDoesNotBlockRequiredMove,
        AlternativeDirectionCreatesShortcut,
        InvalidDependencyReference,
        RequiredDisplacementInvalid,
        OutOfBoundsClearance,
        Other
    }

    private enum ChainBuildFailReason
    {
        None,
        NoTargetBlocker,
        NoDependencyCell,
        NoValidOrientation,
        PlacementExhausted,
        BacktrackExhausted
    }

    private const int ChainLocalAttemptsPerNode = 40;
    private const int ChainTotalPlacementBudget = 400;

    private ChainBuildFailReason lastChainBuildFailReason = ChainBuildFailReason.None;

    private CandidateLevel TryBuildConstructiveHardCandidate(
        System.Random rng,
        out PlacementFailReason failReason)
    {
        failReason = PlacementFailReason.None;
        lastChainBuildFailReason = ChainBuildFailReason.None;
        lastDeepestChainNode = 0;

        int chainLen = PickConstructiveChainLength(rng);
        lastConstructivePlannedChainLength = chainLen;
        // Soft pre-solver floor: full solver still enforces effectiveMinMoves.
        int preSolverMoveFloor = effectivePreSolverMoveFloor > 0
            ? effectivePreSolverMoveFloor
            : Mathf.Max(2, effectiveMinMoves - 2);
        int targetConstructedMinMoves = preSolverMoveFloor;

        int vehicleCount = rng.Next(effectiveMinVehicles, effectiveMaxVehicles + 1);
        // Reserve room for max chain + secondary before fillers.
        vehicleCount = Mathf.Max(
            vehicleCount,
            1 + effectiveMaxChain + effectiveMaxSecondary
        );

        if (constructiveSparseFiller)
        {
            int sparseMax = Mathf.Max(
                1 + chainLen + effectiveMaxSecondary,
                effectiveMinVehicles + Mathf.Max(1, (effectiveMaxVehicles - effectiveMinVehicles) / 3)
            );
            vehicleCount = Mathf.Min(vehicleCount, sparseMax);
            vehicleCount = Mathf.Max(vehicleCount, 1 + chainLen);
        }

        CandidateLevel candidate = new CandidateLevel
        {
            gridWidth = gridWidth,
            gridHeight = gridHeight,
            usedConstructive = true,
            chainLength = chainLen,
            chainVehicleNames = new List<string>(),
            dependencies = new List<ChainDependency>(),
            secondaryVehicleNames = new List<string>(),
            secondaryVehicleIndices = new List<int>(),
            targetConstructedMinMoves = targetConstructedMinMoves,
            forkDependencyCount = 0
        };

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        int nonTargetBudget = vehicleCount - 1;
        int maxLength3 = Mathf.FloorToInt(nonTargetBudget * maxLongVehicleRatio);
        int length3Placed = 0;

        if (!TryPlaceTargetForChain(rng, candidate, occupied, chainLen, out failReason))
        {
            return null;
        }

        List<ChainDependency> dependencies = candidate.dependencies;

        if (!BuildChainWithBacktracking(
                rng,
                candidate,
                occupied,
                dependencies,
                chainLen,
                ref length3Placed,
                maxLength3,
                out ChainBuildFailReason buildFail))
        {
            lastChainBuildFailReason = buildFail;
            RecordChainBuildFailure(buildFail);
            failReason = PlacementFailReason.ChainCreationFailed;
            return null;
        }

        RefreshChainVehicleNames(candidate, dependencies);
        candidate.initialChainLength = dependencies.Count;
        candidate.chainLength = dependencies.Count;

        if (!ValidateChainGeometrically(candidate, dependencies, out BrokenChainReason brokenReason))
        {
            RecordBrokenChainReason(brokenReason);
            failReason = PlacementFailReason.ChainBroken;
            return null;
        }

        int chainOnlyMinMoves;
        if (!ChainOnlySolvePrecheck(candidate, dependencies.Count, out chainOnlyMinMoves))
        {
            lastShortcutChainOnlyMoves = chainOnlyMinMoves;
            lastShortcutChainLength = dependencies.Count;
            failReason = PlacementFailReason.ChainShortcut;
            return null;
        }

        candidate.chainOnlyMovesBeforeExtension = chainOnlyMinMoves;
        candidate.chainOnlyMinimumMoves = chainOnlyMinMoves;

        // --- Chain extension toward soft pre-solver floor (hard cap = effectiveMaxChain) ---
        while (chainOnlyMinMoves < targetConstructedMinMoves &&
               dependencies.Count < effectiveMaxChain)
        {
            chainExtensionsAttempted++;
            if (!TryExtendChainByOneNode(
                    rng,
                    candidate,
                    occupied,
                    dependencies,
                    ref length3Placed,
                    maxLength3))
            {
                break;
            }

            if (!ValidateChainGeometrically(
                    candidate,
                    dependencies,
                    out BrokenChainReason extendBroken))
            {
                RemoveLastChainNode(candidate, occupied, dependencies, ref length3Placed);
                RecordBrokenChainReason(extendBroken);
                break;
            }

            int extendedMoves;
            if (!ChainOnlySolvePrecheck(candidate, dependencies.Count, out extendedMoves))
            {
                RemoveLastChainNode(candidate, occupied, dependencies, ref length3Placed);
                break;
            }

            chainExtensionsSuccessful++;
            chainOnlyMinMoves = extendedMoves;
            candidate.chainOnlyMinimumMoves = chainOnlyMinMoves;
            candidate.chainLength = dependencies.Count;
            RefreshChainVehicleNames(candidate, dependencies);
        }

        candidate.extendedChainLength = dependencies.Count;

        // --- Optional enrichment: secondary blockers (never hard-reject if they fail) ---
        TryEnrichWithSecondaryAndFork(
            rng,
            candidate,
            occupied,
            dependencies,
            ref length3Placed,
            maxLength3,
            targetConstructedMinMoves,
            ref chainOnlyMinMoves);

        candidate.chainOnlyMinimumMoves = chainOnlyMinMoves;
        candidate.chainOnlyMovesAfterExtension = chainOnlyMinMoves;

        if (chainOnlyMinMoves < targetConstructedMinMoves)
        {
            RecordConstructiveStructureMetrics(candidate);
            failReason = PlacementFailReason.ConstructedSolutionFloor;
            return null;
        }

        candidate.vehicleCountBeforeFillers = candidate.vehicles.Count;
        candidate.branchingBeforeFillers = CountInitialBranching(candidate);
        sumVehiclesBeforeFillers += candidate.vehicleCountBeforeFillers;
        vehiclesBeforeFillersSamples++;
        sumBranchingBeforeFillers += candidate.branchingBeforeFillers;
        branchingBeforeFillersSamples++;

        // Fillers pas NA chain (+ optionele secondary).
        vehicleCount = Mathf.Max(vehicleCount, candidate.vehicles.Count);
        HashSet<Vector2Int> reserved = BuildChainCorridorReservations(dependencies, candidate);

        int placementIterations = 0;
        while (candidate.vehicles.Count < vehicleCount)
        {
            placementIterations++;
            if (placementIterations > maxPlacementIterationsPerCandidate)
            {
                failReason = PlacementFailReason.CouldNotReachVehicleCount;
                return null;
            }

            if (!TryPlaceFillerVehicle(
                    rng,
                    candidate,
                    occupied,
                    reserved,
                    ref length3Placed,
                    maxLength3,
                    out PlacementFailReason fillerFail))
            {
                failReason = PlacementFailReason.CouldNotReachVehicleCount;
                if (fillerFail != PlacementFailReason.None)
                {
                    failReason = fillerFail == PlacementFailReason.OrdinaryVehiclePlacementFailed
                        ? PlacementFailReason.CouldNotReachVehicleCount
                        : fillerFail;
                }

                return null;
            }
        }

        if (CountDirectBlockers(candidate) < Mathf.Max(1, minDirectBlockers))
        {
            failReason = PlacementFailReason.ChainCreationFailed;
            return null;
        }

        candidate.branchingAfterFillers = CountInitialBranching(candidate);
        sumBranchingAfterFillers += candidate.branchingAfterFillers;
        branchingAfterFillersSamples++;

        failReason = PlacementFailReason.None;
        return candidate;
    }

    private void RecordConstructiveStructureMetrics(CandidateLevel candidate)
    {
        if (candidate == null)
        {
            return;
        }

        if (candidate.initialChainLength > 0)
        {
            sumInitialChainLength += candidate.initialChainLength;
            initialChainLengthSamples++;
        }

        int extended = candidate.extendedChainLength > 0
            ? candidate.extendedChainLength
            : candidate.chainLength;
        if (extended > 0)
        {
            sumExtendedChainLength += extended;
            extendedChainLengthSamples++;
        }

        if (candidate.chainOnlyMovesBeforeExtension > 0)
        {
            sumChainOnlyMovesBeforeExtension += candidate.chainOnlyMovesBeforeExtension;
            chainOnlyMovesBeforeExtensionSamples++;
        }

        if (candidate.chainOnlyMovesAfterExtension > 0)
        {
            sumChainOnlyMovesAfterExtension += candidate.chainOnlyMovesAfterExtension;
            chainOnlyMovesAfterExtensionSamples++;
        }
        else if (candidate.chainOnlyMinimumMoves > 0)
        {
            sumChainOnlyMovesAfterExtension += candidate.chainOnlyMinimumMoves;
            chainOnlyMovesAfterExtensionSamples++;
        }
    }

    private static void RefreshChainVehicleNames(
        CandidateLevel candidate,
        List<ChainDependency> dependencies)
    {
        candidate.chainVehicleNames.Clear();
        for (int i = 0; i < dependencies.Count; i++)
        {
            candidate.chainVehicleNames.Add(dependencies[i].vehicle.name);
        }
    }

    /// <summary>
    /// Optionele enrichment: één mandatory secondary blocker.
    /// Forks alleen wanneer enableForkDependencies.
    /// Geen dure chain-only solve per placement — alleen lokale A/B geometrie.
    /// </summary>
    private void TryEnrichWithSecondaryAndFork(
        System.Random rng,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        List<ChainDependency> dependencies,
        ref int length3Placed,
        int maxLength3,
        int preSolverMoveFloor,
        ref int chainOnlyMinMoves)
    {
        if (candidate.secondaryVehicleNames.Count == 0 &&
            effectiveMaxSecondary > 0)
        {
            secondaryCreationAttempts++;
            if (TryCommitMandatorySecondary(
                    rng,
                    candidate,
                    occupied,
                    dependencies,
                    ref length3Placed,
                    maxLength3,
                    out SecondaryFailReason secondaryFail))
            {
                secondaryCommitted++;
                // Lokale A/B-check is genoeg; chainOnly moves mogen gelijk blijven.
            }
            else
            {
                RecordSecondaryFail(secondaryFail);
            }
        }

        if (enableForkDependencies && candidate.forkDependencyCount == 0)
        {
            forkCreationAttempted++;
            int secondariesBeforeFork = candidate.secondaryVehicleNames.Count;
            if (TryPlaceSimpleFork(
                    rng,
                    candidate,
                    occupied,
                    dependencies,
                    ref length3Placed,
                    maxLength3,
                    out ForkFailReason forkFail))
            {
                forkCreationSuccessful++;
            }
            else
            {
                RecordForkFail(forkFail);
                while (candidate.secondaryVehicleNames.Count > secondariesBeforeFork)
                {
                    RemoveLastSecondaryBlocker(candidate, occupied, ref length3Placed);
                }

                candidate.forkDependencyCount = 0;
            }
        }
    }

    private enum SecondaryFailReason
    {
        None,
        NoEligibleChainNode,
        NoRequiredClearanceCell,
        NoPlacementCandidate,
        SecondaryCannotMove,
        DoesNotBlockRequiredDisplacement,
        MovingSecondaryDoesNotUnlock,
        Overlap,
        OutOfBounds,
        WouldBreakChain,
        CreatesShortcut,
        WouldMakeUnsolvable,
        Other
    }

    private enum ForkFailReason
    {
        None,
        NoEligibleNode,
        NotEnoughClearanceCells,
        PlacementFailed,
        Overlap,
        OutOfBounds,
        WouldBreakChain,
        Other
    }

    private void RecordSecondaryFail(SecondaryFailReason reason)
    {
        switch (reason)
        {
            case SecondaryFailReason.NoEligibleChainNode:
                secondaryNoEligibleChainNode++;
                break;
            case SecondaryFailReason.NoRequiredClearanceCell:
                secondaryNoRequiredClearanceCell++;
                break;
            case SecondaryFailReason.NoPlacementCandidate:
                secondaryNoPlacementCandidate++;
                break;
            case SecondaryFailReason.SecondaryCannotMove:
                secondaryCannotMove++;
                break;
            case SecondaryFailReason.DoesNotBlockRequiredDisplacement:
                secondaryDoesNotBlock++;
                break;
            case SecondaryFailReason.MovingSecondaryDoesNotUnlock:
                secondaryDoesNotUnlock++;
                break;
            case SecondaryFailReason.Overlap:
                secondaryOverlap++;
                break;
            case SecondaryFailReason.OutOfBounds:
                secondaryOutOfBounds++;
                break;
            case SecondaryFailReason.WouldBreakChain:
                secondaryWouldBreakChain++;
                break;
            case SecondaryFailReason.CreatesShortcut:
                secondaryCreatesShortcut++;
                break;
            case SecondaryFailReason.WouldMakeUnsolvable:
                secondaryWouldMakeUnsolvable++;
                break;
            default:
                secondaryOther++;
                break;
        }
    }

    private void RecordForkFail(ForkFailReason reason)
    {
        switch (reason)
        {
            case ForkFailReason.NoEligibleNode:
                forkNoEligibleNode++;
                break;
            case ForkFailReason.NotEnoughClearanceCells:
                forkNotEnoughClearanceCells++;
                break;
            case ForkFailReason.PlacementFailed:
                forkPlacementFailed++;
                break;
            case ForkFailReason.Overlap:
                forkOverlap++;
                break;
            case ForkFailReason.OutOfBounds:
                forkOutOfBounds++;
                break;
            case ForkFailReason.WouldBreakChain:
                forkWouldBreakChain++;
                break;
            default:
                forkOther++;
                break;
        }
    }

    private void MaybeLogSecondaryFailure(
        CandidateLevel candidate,
        SecondaryFailReason secondaryFail,
        string detail,
        ChainDependency node = null,
        PlacementOption? chosen = null,
        Vector2Int? blockCell = null,
        List<PlacementOption> attempted = null)
    {
        if (!verboseChainDiagnostics || secondaryVerboseLogged >= 3)
        {
            return;
        }

        secondaryVerboseLogged++;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("SECONDARY CREATION FAILURE");
        sb.AppendLine("reason: " + secondaryFail + (detail != null ? " | " + detail : ""));
        if (node != null && node.vehicle != null)
        {
            CandidateVehicle v = node.vehicle;
            sb.AppendLine(
                "chain node idx=" + node.vehicleIndex +
                " " + (v.horizontal ? "H" : "V") +
                " at " + v.position +
                " len=" + v.length
            );
            sb.AppendLine(
                "requiredDir=" + node.requiredMoveDirection +
                " requiredDisp=" + node.requiredDisplacement
            );
            sb.Append("clearance=");
            List<Vector2Int> cells = GetClearanceCellsForDep(node);
            for (int i = 0; i < cells.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }

                sb.Append(cells[i]);
            }

            sb.AppendLine();
        }

        if (blockCell.HasValue)
        {
            sb.AppendLine("blockedCell: " + blockCell.Value);
        }

        if (chosen.HasValue)
        {
            PlacementOption p = chosen.Value;
            sb.AppendLine(
                "chosen: " + (p.horizontal ? "H" : "V") +
                " len=" + p.length + " at " + p.position
            );
        }

        if (attempted != null && attempted.Count > 0)
        {
            sb.AppendLine("attempted placements (" + attempted.Count + "):");
            int show = Mathf.Min(attempted.Count, 12);
            for (int i = 0; i < show; i++)
            {
                PlacementOption p = attempted[i];
                sb.AppendLine(
                    "  " + (p.horizontal ? "H" : "V") +
                    " len=" + p.length + " at " + p.position
                );
            }
        }

        Debug.LogWarning(sb.ToString());
    }

    private void MaybeLogForkFailure(ForkFailReason reason, string detail)
    {
        if (!verboseChainDiagnostics || forkVerboseLogged >= 5)
        {
            return;
        }

        forkVerboseLogged++;
        Debug.LogWarning("FORK CREATION FAILURE | " + reason + " | " + detail);
    }

    /// <summary>
    /// Één mandatory secondary: S blokkeert een latere clearance cell van mid-chain C
    /// (disp ≥ 2, pad momenteel vrij). Test A/B verplicht.
    /// </summary>
    private bool TryCommitMandatorySecondary(
        System.Random rng,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        List<ChainDependency> dependencies,
        ref int length3Placed,
        int maxLength3,
        out SecondaryFailReason failReason)
    {
        failReason = SecondaryFailReason.NoEligibleChainNode;
        List<int> eligible = BuildMandatorySecondaryEligibleNodes(dependencies, occupied);
        if (eligible.Count == 0)
        {
            failReason = SecondaryFailReason.NoEligibleChainNode;
            MaybeLogSecondaryFailure(
                candidate,
                failReason,
                "no mid-chain node with disp>=2 and currently clear required path");
            return false;
        }

        // Prefer middle nodes first.
        eligible.Sort((a, b) =>
        {
            int mid = (dependencies.Count - 1) / 2;
            return Mathf.Abs(a - mid).CompareTo(Mathf.Abs(b - mid));
        });

        int nodesToTry = Mathf.Min(5, eligible.Count);
        SecondaryFailReason lastFail = SecondaryFailReason.NoPlacementCandidate;

        for (int n = 0; n < nodesToTry; n++)
        {
            ChainDependency dep = dependencies[eligible[n]];
            List<Vector2Int> cells = GetClearanceCellsForDep(dep);
            List<Vector2Int> freeCells = new List<Vector2Int>();
            // Laatste required clearance cell eerst.
            for (int c = cells.Count - 1; c >= 0; c--)
            {
                Vector2Int cell = cells[c];
                if (!occupied.Contains(cell) &&
                    cell.x >= 0 && cell.y >= 0 &&
                    cell.x < gridWidth && cell.y < gridHeight)
                {
                    freeCells.Add(cell);
                }
            }

            if (freeCells.Count == 0)
            {
                lastFail = SecondaryFailReason.NoRequiredClearanceCell;
                continue;
            }

            foreach (Vector2Int blockCell in freeCells)
            {
                List<PlacementOption> derived = DerivePlacementsCoveringBlockedCell(
                    blockCell,
                    perpendicularToHorizontal: dep.vehicle.horizontal,
                    allowLength3: length3Placed < maxLength3);

                if (derived.Count == 0)
                {
                    lastFail = SecondaryFailReason.NoPlacementCandidate;
                    continue;
                }

                ShuffleOptions(derived, rng);
                List<PlacementOption> attemptedLog = verboseChainDiagnostics
                    ? new List<PlacementOption>()
                    : null;

                foreach (PlacementOption chosen in derived)
                {
                    attemptedLog?.Add(chosen);

                    if (!IsPlacementInsideGrid(
                            chosen.position,
                            chosen.horizontal,
                            chosen.length,
                            gridWidth,
                            gridHeight))
                    {
                        lastFail = SecondaryFailReason.OutOfBounds;
                        continue;
                    }

                    if (!AreCellsFree(
                            occupied,
                            chosen.position,
                            chosen.horizontal,
                            chosen.length))
                    {
                        lastFail = SecondaryFailReason.Overlap;
                        continue;
                    }

                    if (!VehicleOccupiesCell(
                            chosen.position,
                            chosen.horizontal,
                            chosen.length,
                            blockCell))
                    {
                        continue;
                    }

                    secondaryCandidatePlacements++;

                    CandidateVehicle vehicle = new CandidateVehicle
                    {
                        name = GetVehicleName(candidate.vehicles.Count - 1),
                        horizontal = chosen.horizontal,
                        length = chosen.length,
                        position = chosen.position,
                        canExitRight = false
                    };

                    Occupy(occupied, vehicle);

                    // Test A: C cannot complete full required displacement with S present.
                    if (CanCompleteRequiredDisplacement(dep, occupied))
                    {
                        Unoccupy(occupied, vehicle);
                        secondaryRejectedAfterPlacement++;
                        lastFail = SecondaryFailReason.DoesNotBlockRequiredDisplacement;
                        continue;
                    }

                    // S must have a tight escape.
                    if (!SecondaryHasTightEscape(vehicle, occupied))
                    {
                        Unoccupy(occupied, vehicle);
                        secondaryRejectedAfterPlacement++;
                        lastFail = SecondaryFailReason.SecondaryCannotMove;
                        continue;
                    }

                    // Test B: after one escape move of S, C can complete.
                    if (!SecondaryMoveUnlocksDisplacement(dep, vehicle, occupied))
                    {
                        Unoccupy(occupied, vehicle);
                        secondaryRejectedAfterPlacement++;
                        lastFail = SecondaryFailReason.MovingSecondaryDoesNotUnlock;
                        MaybeLogSecondaryFailure(
                            candidate,
                            lastFail,
                            "Test B failed",
                            dep,
                            chosen,
                            blockCell,
                            attemptedLog);
                        continue;
                    }

                    // Commit — NOT added to main chain dependency list.
                    candidate.vehicles.Add(vehicle);
                    if (chosen.length == 3)
                    {
                        length3Placed++;
                    }

                    candidate.secondaryVehicleNames.Add(vehicle.name);
                    candidate.secondaryVehicleIndices.Add(candidate.vehicles.Count - 1);
                    failReason = SecondaryFailReason.None;
                    return true;
                }

                MaybeLogSecondaryFailure(
                    candidate,
                    lastFail,
                    "no valid placement for blockedCell",
                    dep,
                    null,
                    blockCell,
                    attemptedLog);
            }
        }

        failReason = lastFail;
        return false;
    }

    private List<int> BuildMandatorySecondaryEligibleNodes(
        List<ChainDependency> dependencies,
        HashSet<Vector2Int> occupied)
    {
        List<int> eligible = new List<int>();
        if (dependencies == null || dependencies.Count < 2)
        {
            // Need room to skip first; still allow last-only chains of length 1+.
        }

        for (int d = 0; d < dependencies.Count; d++)
        {
            // Skip first blocker (direct target blocker).
            if (d == 0)
            {
                continue;
            }

            ChainDependency dep = dependencies[d];
            if (dep.requiredDisplacement < 2 || dep.vehicle == null)
            {
                continue;
            }

            // Pad moet ZONDER secondary al volledig clear zijn, anders kan Test B
            // niet slagen (next-chain blocker zit nog op het pad).
            if (!CanCompleteRequiredDisplacement(dep, occupied))
            {
                continue;
            }

            eligible.Add(d);
        }

        // Fallback: laatste node met clear path (disp >= 1), als mid-nodes ontbreken.
        if (eligible.Count == 0 && dependencies.Count > 0)
        {
            int last = dependencies.Count - 1;
            ChainDependency dep = dependencies[last];
            if (dep.requiredDisplacement >= 1 &&
                dep.vehicle != null &&
                CanCompleteRequiredDisplacement(dep, occupied))
            {
                eligible.Add(last);
            }
        }

        return eligible;
    }

    /// <summary>
    /// Alle length-2/(3) starts loodrecht op C die blockedCell bezetten.
    /// </summary>
    private List<PlacementOption> DerivePlacementsCoveringBlockedCell(
        Vector2Int blockedCell,
        bool perpendicularToHorizontal,
        bool allowLength3)
    {
        // C horizontal → S vertical; C vertical → S horizontal.
        bool sHorizontal = !perpendicularToHorizontal;
        List<PlacementOption> options = new List<PlacementOption>();

        void AddLength(int length)
        {
            if (sHorizontal)
            {
                for (int offset = 0; offset < length; offset++)
                {
                    int startX = blockedCell.x - offset;
                    Vector2Int start = new Vector2Int(startX, blockedCell.y);
                    if (startX < 0 || startX + length > gridWidth)
                    {
                        continue;
                    }

                    if (blockedCell.y < 0 || blockedCell.y >= gridHeight)
                    {
                        continue;
                    }

                    options.Add(new PlacementOption(true, length, start));
                }
            }
            else
            {
                for (int offset = 0; offset < length; offset++)
                {
                    int startY = blockedCell.y - offset;
                    Vector2Int start = new Vector2Int(blockedCell.x, startY);
                    if (startY < 0 || startY + length > gridHeight)
                    {
                        continue;
                    }

                    if (blockedCell.x < 0 || blockedCell.x >= gridWidth)
                    {
                        continue;
                    }

                    options.Add(new PlacementOption(false, length, start));
                }
            }
        }

        AddLength(2);
        if (allowLength3)
        {
            AddLength(3);
        }

        return options;
    }

    /// <summary>
    /// Test B: na één geldige escape-stap van S kan C zijn required displacement voltooien.
    /// </summary>
    private bool SecondaryMoveUnlocksDisplacement(
        ChainDependency dep,
        CandidateVehicle secondary,
        HashSet<Vector2Int> occupiedWithSecondary)
    {
        foreach (int dir in new[] { +1, -1 })
        {
            if (!EscapeHasClearStep(
                    secondary.position,
                    secondary.horizontal,
                    secondary.length,
                    dir,
                    occupiedWithSecondary))
            {
                continue;
            }

            // Simuleer één stap van S.
            HashSet<Vector2Int> sim = new HashSet<Vector2Int>(occupiedWithSecondary);
            foreach (Vector2Int cell in GetOccupiedCells(secondary))
            {
                sim.Remove(cell);
            }

            Vector2Int moved = ShiftPosition(
                secondary.position,
                secondary.horizontal,
                secondary.length,
                dir);
            foreach (Vector2Int cell in GetOccupiedCells(
                         moved,
                         secondary.horizontal,
                         secondary.length))
            {
                sim.Add(cell);
            }

            if (CanCompleteRequiredDisplacement(dep, sim))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Fork: twee secondaries op verschillende clearance cells, of één secondary
    /// naast een bestaande next-chain dependency, op C met disp ≥ 2.
    /// </summary>
    private bool TryPlaceSimpleFork(
        System.Random rng,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        List<ChainDependency> dependencies,
        ref int length3Placed,
        int maxLength3,
        out ForkFailReason failReason)
    {
        failReason = ForkFailReason.NoEligibleNode;

        List<int> eligible = new List<int>();
        for (int d = 0; d < dependencies.Count; d++)
        {
            if (dependencies[d].requiredDisplacement >= 2)
            {
                eligible.Add(d);
            }
        }

        if (eligible.Count == 0)
        {
            failReason = ForkFailReason.NoEligibleNode;
            MaybeLogForkFailure(failReason, "no disp>=2 node");
            return false;
        }

        ShuffleInts(eligible, rng);

        for (int e = 0; e < eligible.Count; e++)
        {
            ChainDependency dep = dependencies[eligible[e]];
            List<Vector2Int> cells = GetClearanceCellsForDep(dep);
            List<Vector2Int> freeCells = new List<Vector2Int>();
            for (int c = 0; c < cells.Count; c++)
            {
                Vector2Int cell = cells[c];
                if (!occupied.Contains(cell) &&
                    cell.x >= 0 && cell.y >= 0 &&
                    cell.x < gridWidth && cell.y < gridHeight)
                {
                    freeCells.Add(cell);
                }
            }

            bool nextChainBlocksLast =
                dep.dependencyVehicleIndex >= 0 &&
                occupied.Contains(dep.nextDependencyCell);

            // Path A: one free cell + existing next-chain blocker = fork.
            if (freeCells.Count >= 1 && nextChainBlocksLast)
            {
                if (candidate.secondaryVehicleNames.Count >= effectiveMaxSecondary)
                {
                    failReason = ForkFailReason.PlacementFailed;
                    continue;
                }

                if (TryPlaceSecondaryOnCell(
                        rng,
                        candidate,
                        occupied,
                        dep,
                        freeCells[freeCells.Count - 1],
                        ref length3Placed,
                        maxLength3,
                        out _))
                {
                    if (!CanCompleteRequiredDisplacement(dep, occupied))
                    {
                        candidate.forkDependencyCount++;
                        dep.forkSecondaryVehicleIndex =
                            candidate.secondaryVehicleIndices[
                                candidate.secondaryVehicleIndices.Count - 1];
                        secondaryCommitted++;
                        failReason = ForkFailReason.None;
                        return true;
                    }

                    RemoveLastSecondaryBlocker(candidate, occupied, ref length3Placed);
                    failReason = ForkFailReason.WouldBreakChain;
                }
                else
                {
                    failReason = ForkFailReason.PlacementFailed;
                }

                continue;
            }

            // Path B: two free clearance cells → two secondaries.
            if (freeCells.Count < 2)
            {
                failReason = ForkFailReason.NotEnoughClearanceCells;
                continue;
            }

            if (candidate.secondaryVehicleNames.Count + 2 > effectiveMaxSecondary &&
                candidate.secondaryVehicleNames.Count > 0)
            {
                // Not enough secondary budget for a two-blocker fork.
                failReason = ForkFailReason.PlacementFailed;
                continue;
            }

            if (candidate.secondaryVehicleNames.Count + 2 > Mathf.Max(effectiveMaxSecondary, 2))
            {
                failReason = ForkFailReason.PlacementFailed;
                continue;
            }

            Vector2Int cellB = freeCells[0];
            Vector2Int cellD = freeCells[freeCells.Count - 1];
            if (cellB == cellD)
            {
                failReason = ForkFailReason.NotEnoughClearanceCells;
                continue;
            }

            if (!TryPlaceSecondaryOnCell(
                    rng,
                    candidate,
                    occupied,
                    dep,
                    cellB,
                    ref length3Placed,
                    maxLength3,
                    out _))
            {
                failReason = ForkFailReason.PlacementFailed;
                continue;
            }

            if (!TryPlaceSecondaryOnCell(
                    rng,
                    candidate,
                    occupied,
                    dep,
                    cellD,
                    ref length3Placed,
                    maxLength3,
                    out _))
            {
                RemoveLastSecondaryBlocker(candidate, occupied, ref length3Placed);
                failReason = ForkFailReason.PlacementFailed;
                continue;
            }

            if (CanCompleteRequiredDisplacement(dep, occupied))
            {
                RemoveLastSecondaryBlocker(candidate, occupied, ref length3Placed);
                RemoveLastSecondaryBlocker(candidate, occupied, ref length3Placed);
                failReason = ForkFailReason.WouldBreakChain;
                continue;
            }

            candidate.forkDependencyCount++;
            dep.forkSecondaryVehicleIndex =
                candidate.secondaryVehicleIndices[candidate.secondaryVehicleIndices.Count - 1];
            secondaryCommitted += 2;
            failReason = ForkFailReason.None;
            return true;
        }

        MaybeLogForkFailure(failReason, "no fork placement found");
        return false;
    }

    private bool TryPlaceSecondaryOnCell(
        System.Random rng,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        ChainDependency host,
        Vector2Int blockCell,
        ref int length3Placed,
        int maxLength3,
        out PlacementOption placed)
    {
        placed = default;
        bool preferHorizontal = !host.vehicle.horizontal;
        bool[] orientations = { preferHorizontal, !preferHorizontal };
        bool allowLength3 = length3Placed < maxLength3;
        int[] lengths = allowLength3 ? new[] { 2, 3 } : new[] { 2 };

        foreach (int length in lengths)
        {
            foreach (bool horizontal in orientations)
            {
                List<PlacementOption> placements =
                    CollectPlacementsCoveringCell(blockCell, horizontal, length, occupied);
                ShuffleOptions(placements, rng);
                foreach (PlacementOption chosen in placements)
                {
                    if (!AreCellsFree(
                            occupied,
                            chosen.position,
                            chosen.horizontal,
                            chosen.length))
                    {
                        continue;
                    }

                    CandidateVehicle vehicle = new CandidateVehicle
                    {
                        name = GetVehicleName(candidate.vehicles.Count - 1),
                        horizontal = chosen.horizontal,
                        length = chosen.length,
                        position = chosen.position,
                        canExitRight = false
                    };

                    if (!VehicleOccupiesCell(
                            vehicle.position,
                            vehicle.horizontal,
                            vehicle.length,
                            blockCell))
                    {
                        continue;
                    }

                    Occupy(occupied, vehicle);
                    if (!SecondaryHasTightEscape(vehicle, occupied))
                    {
                        Unoccupy(occupied, vehicle);
                        continue;
                    }

                    candidate.vehicles.Add(vehicle);
                    if (chosen.length == 3)
                    {
                        length3Placed++;
                    }

                    candidate.secondaryVehicleNames.Add(vehicle.name);
                    candidate.secondaryVehicleIndices.Add(candidate.vehicles.Count - 1);
                    placed = chosen;
                    return true;
                }
            }
        }

        return false;
    }

    private static List<Vector2Int> GetClearanceCellsForDep(ChainDependency dep)
    {
        if (dep.requiredClearanceCells != null && dep.requiredClearanceCells.Count > 0)
        {
            return dep.requiredClearanceCells;
        }

        return CellsEnteredAlongMoveIgnoringOccupancyStatic(
            dep.vehicle.position,
            dep.vehicle.horizontal,
            dep.vehicle.length,
            dep.requiredMoveDirection,
            dep.requiredDisplacement);
    }

    private static List<Vector2Int> CellsEnteredAlongMoveIgnoringOccupancyStatic(
        Vector2Int pos,
        bool horizontal,
        int length,
        int direction,
        int maxSteps)
    {
        List<Vector2Int> entered = new List<Vector2Int>(maxSteps);
        for (int step = 1; step <= maxSteps; step++)
        {
            if (horizontal)
            {
                entered.Add(direction > 0
                    ? new Vector2Int(pos.x + length - 1 + step, pos.y)
                    : new Vector2Int(pos.x - step, pos.y));
            }
            else
            {
                entered.Add(direction > 0
                    ? new Vector2Int(pos.x, pos.y + length - 1 + step)
                    : new Vector2Int(pos.x, pos.y - step));
            }
        }

        return entered;
    }

    private bool CanCompleteRequiredDisplacement(
        ChainDependency dep,
        HashSet<Vector2Int> occupied)
    {
        CandidateVehicle v = dep.vehicle;
        HashSet<Vector2Int> previousCritical = new HashSet<Vector2Int>();
        if (dep.previousCriticalCells != null && dep.previousCriticalCells.Count > 0)
        {
            for (int i = 0; i < dep.previousCriticalCells.Count; i++)
            {
                previousCritical.Add(dep.previousCriticalCells[i]);
            }
        }
        else
        {
            previousCritical.Add(dep.blockedCriticalCell);
        }

        Vector2Int moved = v.position;
        HashSet<Vector2Int> sim = new HashSet<Vector2Int>(occupied);
        foreach (Vector2Int cell in GetOccupiedCells(v))
        {
            sim.Remove(cell);
        }

        for (int step = 1; step <= dep.requiredDisplacement; step++)
        {
            if (!EscapeHasClearStep(moved, v.horizontal, v.length, dep.requiredMoveDirection, sim))
            {
                return false;
            }

            foreach (Vector2Int cell in GetOccupiedCells(moved, v.horizontal, v.length))
            {
                sim.Remove(cell);
            }

            moved = ShiftPosition(moved, v.horizontal, v.length, dep.requiredMoveDirection);
            foreach (Vector2Int cell in GetOccupiedCells(moved, v.horizontal, v.length))
            {
                sim.Add(cell);
            }
        }

        return !VehicleOccupiesAnyCell(moved, v.horizontal, v.length, previousCritical);
    }

    private bool SecondaryHasTightEscape(
        CandidateVehicle vehicle,
        HashSet<Vector2Int> occupied)
    {
        HashSet<Vector2Int> sim = new HashSet<Vector2Int>(occupied);
        bool plus = EscapeHasClearStep(
            vehicle.position,
            vehicle.horizontal,
            vehicle.length,
            +1,
            sim);
        bool minus = EscapeHasClearStep(
            vehicle.position,
            vehicle.horizontal,
            vehicle.length,
            -1,
            sim);
        if (!plus && !minus)
        {
            return false;
        }

        int dest = 0;
        int maxTravel = 0;
        foreach (int dir in new[] { +1, -1 })
        {
            Vector2Int moved = vehicle.position;
            HashSet<Vector2Int> local = new HashSet<Vector2Int>(occupied);
            foreach (Vector2Int cell in GetOccupiedCells(vehicle))
            {
                local.Remove(cell);
            }

            int travel = 0;
            for (int step = 1; step <= 2; step++)
            {
                if (!EscapeHasClearStep(moved, vehicle.horizontal, vehicle.length, dir, local))
                {
                    break;
                }

                foreach (Vector2Int cell in GetOccupiedCells(moved, vehicle.horizontal, vehicle.length))
                {
                    local.Remove(cell);
                }

                moved = ShiftPosition(moved, vehicle.horizontal, vehicle.length, dir);
                foreach (Vector2Int cell in GetOccupiedCells(moved, vehicle.horizontal, vehicle.length))
                {
                    local.Add(cell);
                }

                travel = step;
                dest++;
            }

            maxTravel = Mathf.Max(maxTravel, travel);
        }

        return dest >= 1 && maxTravel >= 1 && maxTravel <= 2;
    }

    private static void ShuffleInts(List<int> values, System.Random rng)
    {
        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            int tmp = values[i];
            values[i] = values[j];
            values[j] = tmp;
        }
    }

    private void RecordChainBuildFailure(ChainBuildFailReason reason)
    {
        switch (reason)
        {
            case ChainBuildFailReason.NoTargetBlocker:
                failChainNoTargetBlocker++;
                break;
            case ChainBuildFailReason.NoDependencyCell:
                failChainNoDependencyCell++;
                break;
            case ChainBuildFailReason.NoValidOrientation:
                failChainNoValidOrientation++;
                break;
            case ChainBuildFailReason.PlacementExhausted:
                failChainPlacementExhausted++;
                break;
            case ChainBuildFailReason.BacktrackExhausted:
                failChainBacktrackExhausted++;
                break;
        }
    }

    private bool TryExtendChainByOneNode(
        System.Random rng,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        List<ChainDependency> dependencies,
        ref int length3Placed,
        int maxLength3)
    {
        if (dependencies.Count == 0 || dependencies.Count >= effectiveMaxChain)
        {
            return false;
        }

        int nodeIndex = dependencies.Count;
        if (!TryPlaceChainNodeAt(
                rng,
                nodeIndex,
                candidate,
                occupied,
                dependencies,
                ref length3Placed,
                maxLength3,
                out _))
        {
            return false;
        }

        if (nodeIndex < chainNodePlacedCounts.Length)
        {
            chainNodePlacedCounts[nodeIndex]++;
        }

        dependencies[nodeIndex - 1].dependencyVehicleIndex =
            dependencies[nodeIndex].vehicleIndex;
        lastDeepestChainNode = Mathf.Max(lastDeepestChainNode, nodeIndex);
        return true;
    }

    private void RemoveLastSecondaryBlocker(
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        ref int length3Placed)
    {
        if (candidate.secondaryVehicleNames == null ||
            candidate.secondaryVehicleNames.Count == 0)
        {
            return;
        }

        int lastIdx = candidate.vehicles.Count - 1;
        if (lastIdx < 0)
        {
            return;
        }

        CandidateVehicle removed = candidate.vehicles[lastIdx];
        string expectedName =
            candidate.secondaryVehicleNames[candidate.secondaryVehicleNames.Count - 1];
        if (removed.name != expectedName)
        {
            // Zoek op naam als volgorde afwijkt.
            lastIdx = -1;
            for (int i = candidate.vehicles.Count - 1; i >= 0; i--)
            {
                if (candidate.vehicles[i].name == expectedName)
                {
                    lastIdx = i;
                    removed = candidate.vehicles[i];
                    break;
                }
            }

            if (lastIdx < 0)
            {
                candidate.secondaryVehicleNames.RemoveAt(candidate.secondaryVehicleNames.Count - 1);
                if (candidate.secondaryVehicleIndices.Count > 0)
                {
                    candidate.secondaryVehicleIndices.RemoveAt(
                        candidate.secondaryVehicleIndices.Count - 1);
                }

                return;
            }
        }

        Unoccupy(occupied, removed);
        candidate.vehicles.RemoveAt(lastIdx);
        if (removed.length == 3)
        {
            length3Placed = Mathf.Max(0, length3Placed - 1);
        }

        candidate.secondaryVehicleNames.RemoveAt(candidate.secondaryVehicleNames.Count - 1);
        if (candidate.secondaryVehicleIndices.Count > 0)
        {
            candidate.secondaryVehicleIndices.RemoveAt(candidate.secondaryVehicleIndices.Count - 1);
        }

        if (candidate.dependencies != null)
        {
            for (int i = 0; i < candidate.dependencies.Count; i++)
            {
                ChainDependency dep = candidate.dependencies[i];
                if (dep.forkSecondaryVehicleIndex == lastIdx)
                {
                    dep.forkSecondaryVehicleIndex = -1;
                    candidate.forkDependencyCount = Mathf.Max(0, candidate.forkDependencyCount - 1);
                }
                else if (dep.forkSecondaryVehicleIndex > lastIdx)
                {
                    dep.forkSecondaryVehicleIndex--;
                }

                if (dep.vehicleIndex > lastIdx)
                {
                    dep.vehicleIndex--;
                }

                if (dep.blockedVehicleIndex > lastIdx)
                {
                    dep.blockedVehicleIndex--;
                }

                if (dep.dependencyVehicleIndex > lastIdx)
                {
                    dep.dependencyVehicleIndex--;
                }
            }
        }

        for (int i = 0; i < candidate.secondaryVehicleIndices.Count; i++)
        {
            if (candidate.secondaryVehicleIndices[i] > lastIdx)
            {
                candidate.secondaryVehicleIndices[i]--;
            }
        }
    }

    /// <summary>
    /// Uniforme chain-lengte binnen effective [min, max]. Bias verschuift ondergrens hoogstens +1.
    /// </summary>
    private int PickConstructiveChainLength(System.Random rng)
    {
        int min = effectiveMinChain;
        int max = effectiveMaxChain;
        if (max < min)
        {
            return min;
        }

        int lo = min;
        if (constructiveChainBias > 0)
        {
            lo = Mathf.Min(min + 1, max);
        }

        return rng.Next(lo, max + 1);
    }

    private bool TryPlaceTargetForChain(
        System.Random rng,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        int chainLen,
        out PlacementFailReason failReason)
    {
        failReason = PlacementFailReason.None;

        for (int attempt = 0; attempt < placementAttemptsPerVehicle; attempt++)
        {
            // Prefer exit rows near top/bottom zodat vertical blockers one-way clears krijgen.
            int exitRow;
            if (gridHeight >= 6)
            {
                if (rng.Next(0, 2) == 0)
                {
                    exitRow = rng.Next(1, 3);
                }
                else
                {
                    exitRow = rng.Next(gridHeight - 3, gridHeight - 1);
                }
            }
            else
            {
                exitRow = gridHeight >= 4
                    ? rng.Next(1, gridHeight - 1)
                    : rng.Next(0, gridHeight);
            }

            int minPathCells = chainLen <= 4
                ? Mathf.Max(chainLen, 2)
                : Mathf.Max(chainLen + 1, 3);
            int maxTargetX = Mathf.Max(0, gridWidth - 2 - minPathCells);
            // Liever links zodat er genoeg exit-pad rechts is.
            int targetX = maxTargetX <= 1
                ? rng.Next(0, maxTargetX + 1)
                : rng.Next(0, Mathf.Max(1, maxTargetX / 2 + 1));

            Vector2Int pos = new Vector2Int(targetX, exitRow);
            if (!AreCellsFree(occupied, pos, true, 2) ||
                !IsPlacementInsideGrid(pos, true, 2, gridWidth, gridHeight))
            {
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
            return true;
        }

        failReason = PlacementFailReason.TargetPlacementFailed;
        return false;
    }

    // Last planned chain length from constructive builder (for metrics on failed builds).
    private int lastConstructivePlannedChainLength;
    private int lastShortcutChainOnlyMoves;
    private int lastShortcutChainLength;

    private bool BuildChainWithBacktracking(
        System.Random rng,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        List<ChainDependency> dependencies,
        int desiredLength,
        ref int length3Placed,
        int maxLength3,
        out ChainBuildFailReason failReason)
    {
        failReason = ChainBuildFailReason.None;
        lastDeepestChainNode = 0;
        int totalAttempts = 0;

        return BuildChainNodeRecursive(
            rng,
            candidate,
            occupied,
            dependencies,
            desiredLength,
            nodeIndex: 0,
            ref length3Placed,
            maxLength3,
            ref totalAttempts,
            ref failReason);
    }

    private bool BuildChainNodeRecursive(
        System.Random rng,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        List<ChainDependency> dependencies,
        int desiredLength,
        int nodeIndex,
        ref int length3Placed,
        int maxLength3,
        ref int totalAttempts,
        ref ChainBuildFailReason failReason)
    {
        if (nodeIndex >= desiredLength)
        {
            return true;
        }

        lastDeepestChainNode = Mathf.Max(lastDeepestChainNode, nodeIndex);

        ChainBuildFailReason lastLocalFail = ChainBuildFailReason.PlacementExhausted;

        for (int attempt = 0; attempt < ChainLocalAttemptsPerNode; attempt++)
        {
            if (totalAttempts >= ChainTotalPlacementBudget)
            {
                failReason = ChainBuildFailReason.BacktrackExhausted;
                return false;
            }

            totalAttempts++;

            if (!TryPlaceChainNodeAt(
                    rng,
                    nodeIndex,
                    candidate,
                    occupied,
                    dependencies,
                    ref length3Placed,
                    maxLength3,
                    out ChainBuildFailReason localFail))
            {
                lastLocalFail = localFail;
                continue;
            }

            if (nodeIndex < chainNodePlacedCounts.Length)
            {
                chainNodePlacedCounts[nodeIndex]++;
            }

            if (nodeIndex > 0)
            {
                dependencies[nodeIndex - 1].dependencyVehicleIndex =
                    dependencies[nodeIndex].vehicleIndex;
            }

            if (BuildChainNodeRecursive(
                    rng,
                    candidate,
                    occupied,
                    dependencies,
                    desiredLength,
                    nodeIndex + 1,
                    ref length3Placed,
                    maxLength3,
                    ref totalAttempts,
                    ref failReason))
            {
                return true;
            }

            RemoveLastChainNode(candidate, occupied, dependencies, ref length3Placed);
        }

        failReason = nodeIndex == 0 ? lastLocalFail : ChainBuildFailReason.PlacementExhausted;
        return false;
    }

    private void RemoveLastChainNode(
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        List<ChainDependency> dependencies,
        ref int length3Placed)
    {
        if (dependencies.Count == 0)
        {
            return;
        }

        ChainDependency removed = dependencies[dependencies.Count - 1];
        dependencies.RemoveAt(dependencies.Count - 1);

        if (dependencies.Count > 0)
        {
            dependencies[dependencies.Count - 1].dependencyVehicleIndex = -1;
        }

        candidate.vehicles.RemoveAt(removed.vehicleIndex);
        Unoccupy(occupied, removed.vehicle);
        if (removed.vehicle.length == 3)
        {
            length3Placed = Mathf.Max(0, length3Placed - 1);
        }

        for (int i = 0; i < dependencies.Count; i++)
        {
            ChainDependency dep = dependencies[i];
            if (dep.vehicleIndex > removed.vehicleIndex)
            {
                dep.vehicleIndex--;
            }

            if (dep.blockedVehicleIndex > removed.vehicleIndex)
            {
                dep.blockedVehicleIndex--;
            }

            if (dep.dependencyVehicleIndex > removed.vehicleIndex)
            {
                dep.dependencyVehicleIndex--;
            }
        }
    }

    private bool TryPlaceChainNodeAt(
        System.Random rng,
        int nodeIndex,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        List<ChainDependency> dependencies,
        ref int length3Placed,
        int maxLength3,
        out ChainBuildFailReason failReason)
    {
        failReason = ChainBuildFailReason.None;
        CandidateVehicle target = FindTarget(candidate);
        if (target == null)
        {
            failReason = ChainBuildFailReason.NoTargetBlocker;
            return false;
        }

        bool allowLength3 = length3Placed < maxLength3;
        int[] lengths = allowLength3 ? new[] { 2, 3 } : new[] { 2 };

        if (nodeIndex == 0)
        {
            return TryPlaceFirstChainNodeByConstruction(
                rng,
                target,
                candidate,
                occupied,
                dependencies,
                lengths,
                ref length3Placed,
                out failReason);
        }

        return TryPlaceNextChainNodeByConstruction(
            rng,
            nodeIndex,
            candidate,
            occupied,
            dependencies,
            lengths,
            ref length3Placed,
            out failReason);
    }

    private bool TryPlaceFirstChainNodeByConstruction(
        System.Random rng,
        CandidateVehicle target,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        List<ChainDependency> dependencies,
        int[] lengths,
        ref int length3Placed,
        out ChainBuildFailReason failReason)
    {
        failReason = ChainBuildFailReason.NoTargetBlocker;
        List<Vector2Int> corridorCells = GetExitCorridorCells(target, candidate);
        if (corridorCells.Count == 0)
        {
            return false;
        }

        ShuffleCells(corridorCells, rng);
        int targetRightMost = target.position.x + target.length - 1;
        int exitRow = target.position.y;
        HashSet<Vector2Int> exitCorridor = BuildExitCorridorSet(exitRow, targetRightMost);

        // Vertical first (preferred), then horizontal on exit row as fallback.
        bool[] orientations = { false, true };

        foreach (Vector2Int corridorCell in corridorCells)
        {
            foreach (int length in lengths)
            {
                foreach (bool horizontal in orientations)
                {
                    List<PlacementOption> placements =
                        CollectPlacementsCoveringCell(corridorCell, horizontal, length, occupied);
                    PreferOneWayClearPlacements(
                        placements,
                        exitCorridor,
                        occupied);

                    foreach (PlacementOption chosen in placements)
                    {
                        List<DisplacementOption> options = ComputeClearExitDisplacement(
                            chosen.position,
                            chosen.horizontal,
                            chosen.length,
                            exitRow,
                            targetRightMost,
                            occupied);

                        if (options.Count == 0)
                        {
                            failReason = ChainBuildFailReason.NoValidOrientation;
                            continue;
                        }

                        // Alleen richtingen waar tegenovergestelde NIET volledig kan clearen.
                        List<DisplacementOption> oneWay = FilterOneWayDisplacements(
                            chosen.position,
                            chosen.horizontal,
                            chosen.length,
                            exitCorridor,
                            options,
                            occupied);
                        if (oneWay.Count == 0)
                        {
                            failReason = ChainBuildFailReason.NoValidOrientation;
                            continue;
                        }

                        ShuffleDisplacementOptions(oneWay, rng);
                        foreach (DisplacementOption option in oneWay)
                        {
                            if (!CommitChainNode(
                                    candidate,
                                    occupied,
                                    dependencies,
                                    blockedVehicleIndex: -1,
                                    chosen,
                                    option,
                                    corridorCell,
                                    ref length3Placed))
                            {
                                failReason = ChainBuildFailReason.NoValidOrientation;
                                continue;
                            }

                            failReason = ChainBuildFailReason.None;
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Sorteert placements zodat posities met een one-way clear (edge-hulp) eerst komen.
    /// </summary>
    private void PreferOneWayClearPlacements(
        List<PlacementOption> placements,
        HashSet<Vector2Int> mustClearCells,
        HashSet<Vector2Int> occupied)
    {
        placements.Sort((a, b) =>
        {
            int scoreA = CountOneWayClearOptions(a, mustClearCells, occupied);
            int scoreB = CountOneWayClearOptions(b, mustClearCells, occupied);
            return scoreB.CompareTo(scoreA);
        });
    }

    private int CountOneWayClearOptions(
        PlacementOption chosen,
        HashSet<Vector2Int> mustClearCells,
        HashSet<Vector2Int> occupied)
    {
        List<DisplacementOption> options = ComputeClearCellsDisplacement(
            chosen.position,
            chosen.horizontal,
            chosen.length,
            mustClearCells,
            occupied);
        return FilterOneWayDisplacements(
            chosen.position,
            chosen.horizontal,
            chosen.length,
            mustClearCells,
            options,
            occupied).Count;
    }

    private List<DisplacementOption> FilterOneWayDisplacements(
        Vector2Int pos,
        bool horizontal,
        int length,
        HashSet<Vector2Int> mustClearCells,
        List<DisplacementOption> options,
        HashSet<Vector2Int> occupied)
    {
        List<DisplacementOption> oneWay = new List<DisplacementOption>();
        for (int i = 0; i < options.Count; i++)
        {
            DisplacementOption option = options[i];
            int opposite = -option.direction;
            int? oppositeDisp = TryComputeClearDisplacement(
                pos,
                horizontal,
                length,
                mustClearCells,
                opposite,
                occupied);
            if (oppositeDisp.HasValue)
            {
                continue;
            }

            oneWay.Add(option);
        }

        return oneWay;
    }

    private bool TryPlaceNextChainNodeByConstruction(
        System.Random rng,
        int nodeIndex,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        List<ChainDependency> dependencies,
        int[] lengths,
        ref int length3Placed,
        out ChainBuildFailReason failReason)
    {
        failReason = ChainBuildFailReason.NoDependencyCell;
        ChainDependency prev = dependencies[nodeIndex - 1];
        Vector2Int blockedCell = prev.nextDependencyCell;
        if (blockedCell.x < 0 || blockedCell.y < 0)
        {
            return false;
        }

        bool preferHorizontal = !prev.vehicle.horizontal;
        bool[] orientations = preferHorizontal
            ? new[] { true, false }
            : new[] { false, true };

        foreach (int length in lengths)
        {
            foreach (bool horizontal in orientations)
            {
                List<PlacementOption> placements =
                    CollectPlacementsCoveringCell(blockedCell, horizontal, length, occupied);
                ShuffleOptions(placements, rng);

                foreach (PlacementOption chosen in placements)
                {
                    List<DisplacementOption> options = ComputeClearCellDisplacement(
                        chosen.position,
                        chosen.horizontal,
                        chosen.length,
                        blockedCell,
                        occupied);

                    if (options.Count == 0)
                    {
                        failReason = ChainBuildFailReason.NoValidOrientation;
                        continue;
                    }

                    ShuffleDisplacementOptions(options, rng);
                    foreach (DisplacementOption option in options)
                    {
                        HashSet<Vector2Int> critical = new HashSet<Vector2Int> { blockedCell };
                        List<DisplacementOption> oneWay = FilterOneWayDisplacements(
                            chosen.position,
                            chosen.horizontal,
                            chosen.length,
                            critical,
                            new List<DisplacementOption> { option },
                            occupied);
                        if (oneWay.Count == 0)
                        {
                            // Tegenovergestelde kan ook clearen → shortcut-risico; skip.
                            failReason = ChainBuildFailReason.NoValidOrientation;
                            continue;
                        }

                        if (!CommitChainNode(
                                candidate,
                                occupied,
                                dependencies,
                                prev.vehicleIndex,
                                chosen,
                                option,
                                blockedCell,
                                ref length3Placed))
                        {
                            failReason = ChainBuildFailReason.NoValidOrientation;
                            continue;
                        }

                        failReason = ChainBuildFailReason.None;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool CommitChainNode(
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        List<ChainDependency> dependencies,
        int blockedVehicleIndex,
        PlacementOption chosen,
        DisplacementOption option,
        Vector2Int blockedCriticalCell,
        ref int length3Placed)
    {
        List<Vector2Int> entered = CellsEnteredAlongMove(
            chosen.position,
            chosen.horizontal,
            chosen.length,
            option.direction,
            option.displacement,
            occupied);

        if (entered.Count < option.displacement)
        {
            return false;
        }

        Vector2Int nextCell = entered[entered.Count - 1];

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

        List<Vector2Int> previousCritical = new List<Vector2Int>();
        if (blockedVehicleIndex < 0)
        {
            CandidateVehicle target = FindTarget(candidate);
            if (target != null)
            {
                previousCritical.AddRange(GetExitCorridorCells(target, candidate));
            }
        }
        else
        {
            previousCritical.Add(blockedCriticalCell);
        }

        ChainDependency dep = new ChainDependency
        {
            vehicleIndex = candidate.vehicles.Count - 1,
            blockedVehicleIndex = blockedVehicleIndex,
            requiredMoveDirection = option.direction,
            requiredDisplacement = option.displacement,
            blockedCriticalCell = blockedCriticalCell,
            nextDependencyCell = nextCell,
            vehicle = vehicle
        };
        dep.previousCriticalCells.Clear();
        dep.previousCriticalCells.AddRange(previousCritical);
        dep.requiredClearanceCells.Clear();
        dep.requiredClearanceCells.AddRange(entered);
        dependencies.Add(dep);
        return true;
    }

    private readonly struct DisplacementOption
    {
        public readonly int direction;
        public readonly int displacement;

        public DisplacementOption(int direction, int displacement)
        {
            this.direction = direction;
            this.displacement = displacement;
        }
    }

    private List<Vector2Int> GetExitCorridorCells(CandidateVehicle target, CandidateLevel candidate)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        int rightMost = target.position.x + target.length - 1;
        int exitRow = target.position.y;
        for (int x = rightMost + 1; x < candidate.gridWidth; x++)
        {
            cells.Add(new Vector2Int(x, exitRow));
        }

        return cells;
    }

    private HashSet<Vector2Int> BuildExitCorridorSet(int exitRow, int targetRightMost)
    {
        HashSet<Vector2Int> cells = new HashSet<Vector2Int>();
        for (int x = targetRightMost + 1; x < gridWidth; x++)
        {
            cells.Add(new Vector2Int(x, exitRow));
        }

        return cells;
    }

    private List<DisplacementOption> ComputeClearExitDisplacement(
        Vector2Int pos,
        bool horizontal,
        int length,
        int exitRow,
        int targetRightMost,
        HashSet<Vector2Int> occupied)
    {
        HashSet<Vector2Int> exitCorridor = BuildExitCorridorSet(exitRow, targetRightMost);
        return ComputeClearCellsDisplacement(pos, horizontal, length, exitCorridor, occupied);
    }

    private List<DisplacementOption> ComputeClearCellDisplacement(
        Vector2Int pos,
        bool horizontal,
        int length,
        Vector2Int criticalCell,
        HashSet<Vector2Int> occupied)
    {
        HashSet<Vector2Int> critical = new HashSet<Vector2Int> { criticalCell };
        return ComputeClearCellsDisplacement(pos, horizontal, length, critical, occupied);
    }

    private List<DisplacementOption> ComputeClearCellsDisplacement(
        Vector2Int pos,
        bool horizontal,
        int length,
        HashSet<Vector2Int> mustClearCells,
        HashSet<Vector2Int> occupied)
    {
        List<DisplacementOption> options = new List<DisplacementOption>();
        foreach (int direction in new[] { +1, -1 })
        {
            int? displacement = TryComputeClearDisplacement(
                pos,
                horizontal,
                length,
                mustClearCells,
                direction,
                occupied);
            if (displacement.HasValue && displacement.Value > 0)
            {
                options.Add(new DisplacementOption(direction, displacement.Value));
            }
        }

        return options;
    }

    private int? TryComputeClearDisplacement(
        Vector2Int pos,
        bool horizontal,
        int length,
        HashSet<Vector2Int> mustClearCells,
        int direction,
        HashSet<Vector2Int> occupied)
    {
        Vector2Int moved = pos;
        HashSet<Vector2Int> simOccupied = new HashSet<Vector2Int>(occupied);
        foreach (Vector2Int cell in GetOccupiedCells(pos, horizontal, length))
        {
            simOccupied.Remove(cell);
        }

        for (int step = 1; step <= gridWidth + gridHeight; step++)
        {
            if (!EscapeHasClearStep(moved, horizontal, length, direction, simOccupied))
            {
                return null;
            }

            foreach (Vector2Int cell in GetOccupiedCells(moved, horizontal, length))
            {
                simOccupied.Remove(cell);
            }

            moved = ShiftPosition(moved, horizontal, length, direction);

            foreach (Vector2Int cell in GetOccupiedCells(moved, horizontal, length))
            {
                simOccupied.Add(cell);
            }

            if (!VehicleOccupiesAnyCell(moved, horizontal, length, mustClearCells))
            {
                return step;
            }
        }

        return null;
    }

    private static bool VehicleOccupiesAnyCell(
        Vector2Int pos,
        bool horizontal,
        int length,
        HashSet<Vector2Int> cells)
    {
        foreach (Vector2Int cell in GetOccupiedCells(pos, horizontal, length))
        {
            if (cells.Contains(cell))
            {
                return true;
            }
        }

        return false;
    }

    private Vector2Int GetLeadingEdgeCellAfterSteps(
        Vector2Int pos,
        bool horizontal,
        int length,
        int direction,
        int steps)
    {
        if (horizontal)
        {
            return direction > 0
                ? new Vector2Int(pos.x + length - 1 + steps, pos.y)
                : new Vector2Int(pos.x - steps, pos.y);
        }

        return direction > 0
            ? new Vector2Int(pos.x, pos.y + length - 1 + steps)
            : new Vector2Int(pos.x, pos.y - steps);
    }

    private List<Vector2Int> CellsEnteredAlongMove(
        Vector2Int pos,
        bool horizontal,
        int length,
        int direction,
        int maxSteps,
        HashSet<Vector2Int> occupied)
    {
        List<Vector2Int> entered = new List<Vector2Int>(maxSteps);
        Vector2Int moved = pos;
        HashSet<Vector2Int> simOccupied = new HashSet<Vector2Int>(occupied);
        foreach (Vector2Int cell in GetOccupiedCells(pos, horizontal, length))
        {
            simOccupied.Remove(cell);
        }

        for (int step = 1; step <= maxSteps; step++)
        {
            if (!EscapeHasClearStep(moved, horizontal, length, direction, simOccupied))
            {
                break;
            }

            entered.Add(GetLeadingEdgeCellAfterSteps(moved, horizontal, length, direction, 1));

            foreach (Vector2Int cell in GetOccupiedCells(moved, horizontal, length))
            {
                simOccupied.Remove(cell);
            }

            moved = ShiftPosition(moved, horizontal, length, direction);
            foreach (Vector2Int cell in GetOccupiedCells(moved, horizontal, length))
            {
                simOccupied.Add(cell);
            }
        }

        return entered;
    }

    private List<PlacementOption> CollectPlacementsCoveringCell(
        Vector2Int cell,
        bool horizontal,
        int length,
        HashSet<Vector2Int> occupied)
    {
        List<PlacementOption> options = new List<PlacementOption>();
        if (horizontal)
        {
            for (int x = cell.x - length + 1; x <= cell.x; x++)
            {
                if (x < 0 || x + length > gridWidth)
                {
                    continue;
                }

                Vector2Int pos = new Vector2Int(x, cell.y);
                if (AreCellsFree(occupied, pos, true, length))
                {
                    options.Add(new PlacementOption(true, length, pos));
                }
            }
        }
        else
        {
            for (int y = cell.y - length + 1; y <= cell.y; y++)
            {
                if (y < 0 || y + length > gridHeight)
                {
                    continue;
                }

                Vector2Int pos = new Vector2Int(cell.x, y);
                if (AreCellsFree(occupied, pos, false, length))
                {
                    options.Add(new PlacementOption(false, length, pos));
                }
            }
        }

        return options;
    }

    private static void ShuffleCells(List<Vector2Int> cells, System.Random rng)
    {
        for (int i = cells.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            Vector2Int tmp = cells[i];
            cells[i] = cells[j];
            cells[j] = tmp;
        }
    }

    private static void ShuffleDisplacementOptions(List<DisplacementOption> options, System.Random rng)
    {
        for (int i = options.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            DisplacementOption tmp = options[i];
            options[i] = options[j];
            options[j] = tmp;
        }
    }

    private static Vector2Int ShiftPosition(
        Vector2Int pos,
        bool horizontal,
        int length,
        int direction)
    {
        if (horizontal)
        {
            return new Vector2Int(pos.x + direction, pos.y);
        }

        return new Vector2Int(pos.x, pos.y + direction);
    }

    private bool ValidateChainGeometrically(
        CandidateLevel candidate,
        List<ChainDependency> dependencies,
        out BrokenChainReason failReason)
    {
        failReason = BrokenChainReason.None;

        if (dependencies == null || dependencies.Count == 0)
        {
            failReason = BrokenChainReason.Other;
            return false;
        }

        CandidateVehicle target = FindTarget(candidate);
        if (target == null)
        {
            failReason = BrokenChainReason.Other;
            return false;
        }

        if (CanTargetExitImmediately(candidate))
        {
            failReason = BrokenChainReason.PreviousCriticalCellNotBlocked;
            MaybeLogBrokenChainExample(candidate, dependencies, -1, failReason, null);
            return false;
        }

        StringBuilder nodeDump = verboseChainDiagnostics ? new StringBuilder() : null;

        for (int i = 0; i < dependencies.Count; i++)
        {
            ChainDependency dep = dependencies[i];

            if (dep.requiredDisplacement <= 0)
            {
                failReason = BrokenChainReason.RequiredDisplacementInvalid;
                MaybeLogBrokenChainExample(candidate, dependencies, i, failReason, null);
                return false;
            }

            if (dep.vehicleIndex < 0 ||
                dep.vehicleIndex >= candidate.vehicles.Count ||
                dep.vehicle == null)
            {
                failReason = BrokenChainReason.InvalidDependencyReference;
                MaybeLogBrokenChainExample(candidate, dependencies, i, failReason, null);
                return false;
            }

            if (dep.dependencyVehicleIndex >= 0)
            {
                if (dep.dependencyVehicleIndex >= candidate.vehicles.Count)
                {
                    failReason = BrokenChainReason.InvalidDependencyReference;
                    MaybeLogBrokenChainExample(candidate, dependencies, i, failReason, null);
                    return false;
                }

                if (i + 1 < dependencies.Count)
                {
                    ChainDependency next = dependencies[i + 1];
                    if (next.vehicleIndex != dep.dependencyVehicleIndex ||
                        !VehicleOccupiesCell(
                            next.vehicle.position,
                            next.vehicle.horizontal,
                            next.vehicle.length,
                            dep.nextDependencyCell))
                    {
                        failReason = BrokenChainReason.InvalidDependencyReference;
                        MaybeLogBrokenChainExample(candidate, dependencies, i, failReason, null);
                        return false;
                    }
                }
            }

            CandidateVehicle v = dep.vehicle;
            HashSet<Vector2Int> previousCritical = GetPreviousCriticalCells(dep, candidate);

            bool blocksPrevious = OccupiesAnyCritical(v, previousCritical);
            bool requiredClears = RequiredMoveClearsCriticalGeometrically(dep, previousCritical);
            bool requiredInBounds = RequiredDisplacementStaysInBounds(dep);
            bool dependencyBlocks = dep.dependencyVehicleIndex < 0 ||
                DependencyBlocksRequiredDisplacement(dep, candidate);
            // Occupancy-aware — zelfde semantiek als FilterOneWayDisplacements tijdens construction.
            bool altShortcut = AlternativeDirectionFullyClearsCritical(
                dep,
                previousCritical,
                candidate);

            if (nodeDump != null)
            {
                AppendNodeDiagnostic(
                    nodeDump,
                    i,
                    dep,
                    blocksPrevious,
                    requiredClears,
                    dependencyBlocks,
                    altShortcut,
                    requiredInBounds);
            }

            if (!blocksPrevious)
            {
                failReason = BrokenChainReason.PreviousCriticalCellNotBlocked;
                MaybeLogBrokenChainExample(candidate, dependencies, i, failReason, nodeDump);
                return false;
            }

            if (!requiredInBounds)
            {
                failReason = BrokenChainReason.OutOfBoundsClearance;
                MaybeLogBrokenChainExample(candidate, dependencies, i, failReason, nodeDump);
                return false;
            }

            if (!requiredClears)
            {
                failReason = BrokenChainReason.RequiredMoveDoesNotClear;
                MaybeLogBrokenChainExample(candidate, dependencies, i, failReason, nodeDump);
                return false;
            }

            if (dep.dependencyVehicleIndex >= 0 && !dependencyBlocks)
            {
                failReason = BrokenChainReason.DependencyDoesNotBlockRequiredMove;
                MaybeLogBrokenChainExample(candidate, dependencies, i, failReason, nodeDump);
                return false;
            }

            if (altShortcut)
            {
                failReason = BrokenChainReason.AlternativeDirectionCreatesShortcut;
                MaybeLogBrokenChainExample(candidate, dependencies, i, failReason, nodeDump);
                return false;
            }
        }

        return true;
    }

    private void RecordBrokenChainReason(BrokenChainReason reason)
    {
        switch (reason)
        {
            case BrokenChainReason.PreviousCriticalCellNotBlocked:
                brokenChainPreviousCriticalCellNotBlocked++;
                break;
            case BrokenChainReason.RequiredMoveDoesNotClear:
                brokenChainRequiredMoveDoesNotClear++;
                break;
            case BrokenChainReason.DependencyDoesNotBlockRequiredMove:
                brokenChainDependencyDoesNotBlockRequiredMove++;
                break;
            case BrokenChainReason.AlternativeDirectionCreatesShortcut:
                brokenChainAlternativeDirectionCreatesShortcut++;
                break;
            case BrokenChainReason.InvalidDependencyReference:
                brokenChainInvalidDependencyReference++;
                break;
            case BrokenChainReason.RequiredDisplacementInvalid:
                brokenChainRequiredDisplacementInvalid++;
                break;
            case BrokenChainReason.OutOfBoundsClearance:
                brokenChainOutOfBoundsClearance++;
                break;
            default:
                brokenChainOther++;
                break;
        }
    }

    private void MaybeLogBrokenChainExample(
        CandidateLevel candidate,
        List<ChainDependency> dependencies,
        int failNodeIndex,
        BrokenChainReason reason,
        StringBuilder nodeDump)
    {
        if (!verboseChainDiagnostics || brokenChainVerboseLogged >= 5)
        {
            return;
        }

        brokenChainVerboseLogged++;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("BROKEN CHAIN EXAMPLE");
        sb.AppendLine("Planned length: " + (dependencies != null ? dependencies.Count : 0));
        sb.AppendLine("REJECT REASON: " + FormatBrokenChainReason(reason));
        if (failNodeIndex >= 0)
        {
            sb.AppendLine("Failed at node: " + failNodeIndex);
        }

        if (nodeDump != null && nodeDump.Length > 0)
        {
            sb.Append(nodeDump);
        }
        else if (dependencies != null)
        {
            for (int i = 0; i < dependencies.Count; i++)
            {
                AppendNodeDiagnostic(sb, i, dependencies[i], false, false, false, false, false);
            }
        }

        Debug.LogWarning(sb.ToString());
    }

    private static string FormatBrokenChainReason(BrokenChainReason reason)
    {
        switch (reason)
        {
            case BrokenChainReason.PreviousCriticalCellNotBlocked:
                return "previous critical cell not blocked";
            case BrokenChainReason.RequiredMoveDoesNotClear:
                return "required move does not clear";
            case BrokenChainReason.DependencyDoesNotBlockRequiredMove:
                return "dependency does not block required move";
            case BrokenChainReason.AlternativeDirectionCreatesShortcut:
                return "alternative direction creates shortcut";
            case BrokenChainReason.InvalidDependencyReference:
                return "invalid dependency reference";
            case BrokenChainReason.RequiredDisplacementInvalid:
                return "required displacement invalid";
            case BrokenChainReason.OutOfBoundsClearance:
                return "out of bounds clearance";
            default:
                return "other";
        }
    }

    private void AppendNodeDiagnostic(
        StringBuilder sb,
        int nodeIndex,
        ChainDependency dep,
        bool blocksPrevious,
        bool requiredClears,
        bool dependencyBlocks,
        bool altShortcut,
        bool requiredInBounds)
    {
        CandidateVehicle v = dep.vehicle;
        string orient = v != null && v.horizontal ? "horizontal" : "vertical";
        sb.AppendLine();
        sb.AppendLine("Node " + nodeIndex + ":");
        sb.AppendLine(
            "  Vehicle idx=" + dep.vehicleIndex +
            " " + orient +
            " at (" + (v != null ? v.position.x + "," + v.position.y : "?") +
            "), len=" + (v != null ? v.length : 0)
        );
        sb.AppendLine(
            "  previousCriticalCell=" + dep.blockedCriticalCell +
            " | requiredDir=" + dep.requiredMoveDirection +
            " | requiredDisp=" + dep.requiredDisplacement
        );
        if (dep.previousCriticalCells != null && dep.previousCriticalCells.Count > 0)
        {
            sb.Append("  previousCriticalCells=");
            for (int c = 0; c < dep.previousCriticalCells.Count; c++)
            {
                if (c > 0)
                {
                    sb.Append(",");
                }

                sb.Append(dep.previousCriticalCells[c]);
            }

            sb.AppendLine();
        }

        sb.AppendLine(
            "  dependencyVehicleIndex=" + dep.dependencyVehicleIndex +
            " | nextDependencyCell=" + dep.nextDependencyCell
        );
        sb.AppendLine("  A BlocksPreviousCriticalCell=" + blocksPrevious);
        sb.AppendLine("  B RequiredMoveClearsCriticalCell=" + requiredClears);
        sb.AppendLine("  C DependencyActuallyBlocksRequiredMove=" + dependencyBlocks);
        sb.AppendLine("  D AlternativeDirectionClearsCriticalCell=" + altShortcut);
        sb.AppendLine("  E RequiredDisplacementInBounds=" + requiredInBounds);
    }

    private HashSet<Vector2Int> GetPreviousCriticalCells(
        ChainDependency dep,
        CandidateLevel candidate)
    {
        if (dep.previousCriticalCells != null && dep.previousCriticalCells.Count > 0)
        {
            return new HashSet<Vector2Int>(dep.previousCriticalCells);
        }

        // Fallback voor oude nodes zonder opgeslagen set.
        if (dep.blockedVehicleIndex == -1)
        {
            CandidateVehicle target = FindTarget(candidate);
            return target == null
                ? new HashSet<Vector2Int>()
                : new HashSet<Vector2Int>(GetExitCorridorCells(target, candidate));
        }

        return new HashSet<Vector2Int> { dep.blockedCriticalCell };
    }

    private static bool OccupiesAnyCritical(CandidateVehicle v, HashSet<Vector2Int> critical)
    {
        foreach (Vector2Int cell in GetOccupiedCells(v))
        {
            if (critical.Contains(cell))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// B: geometrische clear — eindpose na requiredDisplacement bezet previousCritical niet.
    /// Pad hoeft hier NIET vrij te zijn (dat is check C).
    /// </summary>
    private bool RequiredMoveClearsCriticalGeometrically(
        ChainDependency dep,
        HashSet<Vector2Int> previousCritical)
    {
        CandidateVehicle v = dep.vehicle;
        Vector2Int finalPos = ShiftPositionBy(
            v.position,
            v.horizontal,
            dep.requiredMoveDirection,
            dep.requiredDisplacement);

        if (!IsPlacementInsideGrid(finalPos, v.horizontal, v.length, gridWidth, gridHeight))
        {
            return false;
        }

        foreach (Vector2Int cell in GetOccupiedCells(finalPos, v.horizontal, v.length))
        {
            if (previousCritical.Contains(cell))
            {
                return false;
            }
        }

        return true;
    }

    private bool RequiredDisplacementStaysInBounds(ChainDependency dep)
    {
        CandidateVehicle v = dep.vehicle;
        Vector2Int finalPos = ShiftPositionBy(
            v.position,
            v.horizontal,
            dep.requiredMoveDirection,
            dep.requiredDisplacement);
        return IsPlacementInsideGrid(finalPos, v.horizontal, v.length, gridWidth, gridHeight);
    }

    private static Vector2Int ShiftPositionBy(
        Vector2Int pos,
        bool horizontal,
        int direction,
        int steps)
    {
        if (horizontal)
        {
            return new Vector2Int(pos.x + direction * steps, pos.y);
        }

        return new Vector2Int(pos.x, pos.y + direction * steps);
    }

    /// <summary>
    /// C: dependency blokkeert als het minstens één cell bezet die nodig is voor
    /// de volledige requiredDisplacement (niet alleen de eerste angrenzende cell).
    /// </summary>
    private bool DependencyBlocksRequiredDisplacement(
        ChainDependency dep,
        CandidateLevel candidate)
    {
        if (dep.dependencyVehicleIndex < 0 ||
            dep.dependencyVehicleIndex >= candidate.vehicles.Count)
        {
            return false;
        }

        CandidateVehicle blocker = candidate.vehicles[dep.dependencyVehicleIndex];
        HashSet<Vector2Int> blockerCells = new HashSet<Vector2Int>(GetOccupiedCells(blocker));

        // Cells die betreden moeten worden tijdens de required move.
        List<Vector2Int> needed = dep.requiredClearanceCells;
        if (needed == null || needed.Count == 0)
        {
            needed = CellsEnteredAlongMoveIgnoringOccupancy(
                dep.vehicle.position,
                dep.vehicle.horizontal,
                dep.vehicle.length,
                dep.requiredMoveDirection,
                dep.requiredDisplacement);
        }

        for (int i = 0; i < needed.Count; i++)
        {
            if (blockerCells.Contains(needed[i]))
            {
                return true;
            }
        }

        // Ook: nextDependencyCell moet door dependency bezet zijn.
        if (blockerCells.Contains(dep.nextDependencyCell))
        {
            return true;
        }

        return false;
    }

    private List<Vector2Int> CellsEnteredAlongMoveIgnoringOccupancy(
        Vector2Int pos,
        bool horizontal,
        int length,
        int direction,
        int maxSteps)
    {
        List<Vector2Int> entered = new List<Vector2Int>(maxSteps);
        for (int step = 1; step <= maxSteps; step++)
        {
            entered.Add(GetLeadingEdgeCellAfterSteps(pos, horizontal, length, direction, step));
        }

        return entered;
    }

    /// <summary>
    /// D: shortcut alleen als tegengestelde richting previousCritical echt volledig
    /// vrijmaakt (pad begaanbaar + critical volledig clear). Occupancy-aware, identiek
    /// aan construction FilterOneWayDisplacements / TryComputeClearDisplacement.
    /// Een partial move die critical niet vrijmaakt telt NIET als shortcut.
    /// </summary>
    private bool AlternativeDirectionFullyClearsCritical(
        ChainDependency dep,
        HashSet<Vector2Int> previousCritical,
        CandidateLevel candidate)
    {
        int opposite = -dep.requiredMoveDirection;
        CandidateVehicle v = dep.vehicle;
        HashSet<Vector2Int> occupied = BuildOccupiedSet(candidate);
        int? disp = TryComputeClearDisplacement(
            v.position,
            v.horizontal,
            v.length,
            previousCritical,
            opposite,
            occupied);
        return disp.HasValue && disp.Value > 0;
    }

    private static bool VehicleOccupiesCell(
        Vector2Int pos,
        bool horizontal,
        int length,
        Vector2Int cell)
    {
        foreach (Vector2Int vCell in GetOccupiedCells(pos, horizontal, length))
        {
            if (vCell == cell)
            {
                return true;
            }
        }

        return false;
    }

    private HashSet<Vector2Int> BuildOccupiedSet(CandidateLevel candidate)
    {
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        for (int i = 0; i < candidate.vehicles.Count; i++)
        {
            Occupy(occupied, candidate.vehicles[i]);
        }

        return occupied;
    }

    private bool CanTargetExitImmediately(CandidateLevel candidate)
    {
        CandidateVehicle target = FindTarget(candidate);
        if (target == null || !target.canExitRight)
        {
            return false;
        }

        int targetIndex = -1;
        for (int i = 0; i < candidate.vehicles.Count; i++)
        {
            if (candidate.vehicles[i].canExitRight)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex < 0)
        {
            return false;
        }

        bool[,] occupied = BuildOccupancy(candidate, ignoreIndex: targetIndex);
        for (int steps = 1; ; steps++)
        {
            int rightMost = target.position.x + target.length - 1 + steps;
            if (rightMost >= candidate.gridWidth)
            {
                return true;
            }

            if (occupied[rightMost, target.position.y])
            {
                return false;
            }
        }
    }

    private bool ChainOnlySolvePrecheck(
        CandidateLevel candidate,
        int chainLen,
        out int chainOnlyMinMoves)
    {
        chainOnlyMinMoves = 0;
        LevelData tempLevel = ScriptableObject.CreateInstance<LevelData>();
        tempLevel.gridWidth = candidate.gridWidth;
        tempLevel.gridHeight = candidate.gridHeight;

        CandidateLevel chainOnly = new CandidateLevel
        {
            gridWidth = candidate.gridWidth,
            gridHeight = candidate.gridHeight,
            exitRow = candidate.exitRow,
            vehicles = new List<CandidateVehicle>()
        };

        HashSet<int> chainIndices = new HashSet<int>();
        for (int i = 0; i < candidate.vehicles.Count; i++)
        {
            if (candidate.vehicles[i].canExitRight)
            {
                chainIndices.Add(i);
                break;
            }
        }

        if (candidate.dependencies != null)
        {
            for (int i = 0; i < candidate.dependencies.Count; i++)
            {
                chainIndices.Add(candidate.dependencies[i].vehicleIndex);
            }
        }

        if (candidate.secondaryVehicleIndices != null)
        {
            for (int i = 0; i < candidate.secondaryVehicleIndices.Count; i++)
            {
                chainIndices.Add(candidate.secondaryVehicleIndices[i]);
            }
        }
        else if (candidate.secondaryVehicleNames != null &&
                 candidate.secondaryVehicleNames.Count > 0)
        {
            HashSet<string> secondaryNames = new HashSet<string>(candidate.secondaryVehicleNames);
            for (int i = 0; i < candidate.vehicles.Count; i++)
            {
                if (secondaryNames.Contains(candidate.vehicles[i].name))
                {
                    chainIndices.Add(i);
                }
            }
        }

        foreach (int idx in chainIndices)
        {
            if (idx >= 0 && idx < candidate.vehicles.Count)
            {
                chainOnly.vehicles.Add(CloneVehicle(candidate.vehicles[idx]));
            }
        }

        FillLevelData(tempLevel, chainOnly, levelNumber: 0);
        LevelSolver.SolverResult result = LevelSolver.Solve(tempLevel, chainOnlySolverMaxStates);
        DestroyImmediate(tempLevel);

        if (result.invalid || !result.solvable || result.searchLimitReached)
        {
            return false;
        }

        chainOnlyMinMoves = result.minimumMoves;
        int requiredMin = Mathf.CeilToInt(chainLen * minChainMoveEfficiency);
        return result.minimumMoves >= requiredMin;
    }

    private static CandidateVehicle CloneVehicle(CandidateVehicle source)
    {
        return new CandidateVehicle
        {
            name = source.name,
            horizontal = source.horizontal,
            length = source.length,
            position = source.position,
            canExitRight = source.canExitRight
        };
    }

    private bool TryPlaceFillerVehicle(
        System.Random rng,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        HashSet<Vector2Int> reserved,
        ref int length3Placed,
        int maxLength3,
        out PlacementFailReason failReason)
    {
        failReason = PlacementFailReason.None;

        for (int attempt = 0; attempt < placementAttemptsPerVehicle; attempt++)
        {
            bool preferHorizontal = rng.Next(0, 2) == 0;
            int length = PickNonTargetVehicleLength(
                rng,
                candidate,
                length3Placed,
                maxLength3,
                constructiveSparseFiller ? 0.20 : 0.35
            );

            bool[] orientations = preferHorizontal
                ? new[] { true, false }
                : new[] { false, true };

            foreach (bool horizontal in orientations)
            {
                if (TryPickControlledFillerPosition(
                        rng,
                        candidate,
                        occupied,
                        reserved,
                        horizontal,
                        length,
                        out Vector2Int pos))
                {
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
                    if (length >= 3)
                    {
                        length3Placed++;
                    }

                    int branching = CountInitialBranching(candidate);
                    if (branching > effectiveMaxBranching)
                    {
                        candidate.vehicles.RemoveAt(candidate.vehicles.Count - 1);
                        Unoccupy(occupied, vehicle);
                        if (length >= 3)
                        {
                            length3Placed--;
                        }

                        continue;
                    }

                    return true;
                }
            }

            if (length >= 3)
            {
                foreach (bool horizontal in orientations)
                {
                    if (TryPickControlledFillerPosition(
                            rng,
                            candidate,
                            occupied,
                            reserved,
                            horizontal,
                            2,
                            out Vector2Int pos))
                    {
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

                        int branching = CountInitialBranching(candidate);
                        if (branching > effectiveMaxBranching)
                        {
                            candidate.vehicles.RemoveAt(candidate.vehicles.Count - 1);
                            Unoccupy(occupied, vehicle);
                            continue;
                        }

                        return true;
                    }
                }
            }
        }

        failReason = PlacementFailReason.OrdinaryVehiclePlacementFailed;
        return false;
    }

    /// <summary>
    /// Kiest filler-positie met beperkte travel distance (1–effectiveMaxFillerTravel
    /// bestemmingen), niet door lange lege corridors.
    /// </summary>
    private bool TryPickControlledFillerPosition(
        System.Random rng,
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        HashSet<Vector2Int> reserved,
        bool horizontal,
        int length,
        out Vector2Int position)
    {
        position = Vector2Int.zero;
        List<Vector2Int> tight = new List<Vector2Int>();
        List<Vector2Int> ok = new List<Vector2Int>();
        List<Vector2Int> fallback = new List<Vector2Int>();

        int maxX = horizontal ? gridWidth - length : gridWidth - 1;
        int maxY = horizontal ? gridHeight - 1 : gridHeight - length;
        if (maxX < 0 || maxY < 0)
        {
            return false;
        }

        if (horizontal)
        {
            for (int x = 0; x <= maxX; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    ConsiderFillerCandidate(
                        candidate,
                        occupied,
                        reserved,
                        new Vector2Int(x, y),
                        true,
                        length,
                        tight,
                        ok,
                        fallback
                    );
                }
            }
        }
        else
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y <= maxY; y++)
                {
                    ConsiderFillerCandidate(
                        candidate,
                        occupied,
                        reserved,
                        new Vector2Int(x, y),
                        false,
                        length,
                        tight,
                        ok,
                        fallback
                    );
                }
            }
        }

        List<Vector2Int> pool = tight.Count > 0
            ? tight
            : (ok.Count > 0 ? ok : (constructiveSparseFiller ? ok : fallback));

        if (pool.Count == 0)
        {
            // Sparse: geen reserved fallback. Anders laatste redmiddel.
            if (!constructiveSparseFiller && fallback.Count > 0)
            {
                pool = fallback;
            }
            else
            {
                return false;
            }
        }

        position = pool[rng.Next(0, pool.Count)];
        return true;
    }

    private void ConsiderFillerCandidate(
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        HashSet<Vector2Int> reserved,
        Vector2Int pos,
        bool horizontal,
        int length,
        List<Vector2Int> tight,
        List<Vector2Int> ok,
        List<Vector2Int> fallback)
    {
        if (!AreCellsFree(occupied, pos, horizontal, length))
        {
            return;
        }

        bool touchesReserved = TouchesReserved(pos, horizontal, length, reserved);
        int destinations = CountHypotheticalDestinations(
            candidate,
            occupied,
            pos,
            horizontal,
            length
        );
        int maxTravel = CountHypotheticalMaxTravel(
            occupied,
            pos,
            horizontal,
            length
        );

        if (maxTravel > effectiveMaxFillerTravel || destinations > effectiveMaxFillerTravel * 2)
        {
            return;
        }

        if (touchesReserved)
        {
            fallback.Add(pos);
            return;
        }

        if (destinations >= 1 && destinations <= 2 && maxTravel <= effectiveMaxFillerTravel)
        {
            tight.Add(pos);
        }
        else if (destinations >= 1 && maxTravel <= effectiveMaxFillerTravel)
        {
            ok.Add(pos);
        }
        else if (destinations == 0 && maxTravel == 0)
        {
            // Volledig vastgezet filler: ok voor density, beperkt branching.
            ok.Add(pos);
        }
    }

    private static int CountHypotheticalDestinations(
        CandidateLevel candidate,
        HashSet<Vector2Int> occupied,
        Vector2Int pos,
        bool horizontal,
        int length)
    {
        int count = 0;
        int gridW = candidate.gridWidth;
        int gridH = candidate.gridHeight;

        if (horizontal)
        {
            for (int steps = 1; ; steps++)
            {
                int checkX = pos.x - steps;
                if (checkX < 0 || occupied.Contains(new Vector2Int(checkX, pos.y)))
                {
                    break;
                }

                count++;
            }

            for (int steps = 1; ; steps++)
            {
                int checkX = pos.x + length - 1 + steps;
                if (checkX >= gridW || occupied.Contains(new Vector2Int(checkX, pos.y)))
                {
                    break;
                }

                count++;
            }
        }
        else
        {
            for (int steps = 1; ; steps++)
            {
                int checkY = pos.y - steps;
                if (checkY < 0 || occupied.Contains(new Vector2Int(pos.x, checkY)))
                {
                    break;
                }

                count++;
            }

            for (int steps = 1; ; steps++)
            {
                int checkY = pos.y + length - 1 + steps;
                if (checkY >= gridH || occupied.Contains(new Vector2Int(pos.x, checkY)))
                {
                    break;
                }

                count++;
            }
        }

        return count;
    }

    private int CountHypotheticalMaxTravel(
        HashSet<Vector2Int> occupied,
        Vector2Int pos,
        bool horizontal,
        int length)
    {
        int best = 0;
        if (horizontal)
        {
            int left = 0;
            for (int steps = 1; ; steps++)
            {
                int x = pos.x - steps;
                if (x < 0 || occupied.Contains(new Vector2Int(x, pos.y)))
                {
                    break;
                }

                left = steps;
            }

            int right = 0;
            for (int steps = 1; ; steps++)
            {
                int x = pos.x + length - 1 + steps;
                if (x >= gridWidth || occupied.Contains(new Vector2Int(x, pos.y)))
                {
                    break;
                }

                right = steps;
            }

            best = Mathf.Max(left, right);
        }
        else
        {
            int down = 0;
            for (int steps = 1; ; steps++)
            {
                int y = pos.y - steps;
                if (y < 0 || occupied.Contains(new Vector2Int(pos.x, y)))
                {
                    break;
                }

                down = steps;
            }

            int up = 0;
            for (int steps = 1; ; steps++)
            {
                int y = pos.y + length - 1 + steps;
                if (y >= gridHeight || occupied.Contains(new Vector2Int(pos.x, y)))
                {
                    break;
                }

                up = steps;
            }

            best = Mathf.Max(down, up);
        }

        return best;
    }

    private HashSet<Vector2Int> BuildChainCorridorReservations(
        List<ChainDependency> dependencies,
        CandidateLevel candidate)
    {
        HashSet<Vector2Int> reserved = new HashSet<Vector2Int>();

        for (int i = 0; i < dependencies.Count; i++)
        {
            ChainDependency dep = dependencies[i];
            List<Vector2Int> corridor = CollectEscapeCorridorCells(
                dep.vehicle.position,
                dep.vehicle.horizontal,
                dep.vehicle.length,
                dep.requiredMoveDirection,
                maxSteps: gridWidth + gridHeight
            );
            for (int c = 0; c < corridor.Count; c++)
            {
                reserved.Add(corridor[c]);
            }

            if (dep.requiredClearanceCells != null)
            {
                for (int c = 0; c < dep.requiredClearanceCells.Count; c++)
                {
                    reserved.Add(dep.requiredClearanceCells[c]);
                }
            }
        }

        CandidateVehicle target = FindTarget(candidate);
        if (target != null)
        {
            int rightMost = target.position.x + target.length - 1;
            for (int x = rightMost + 1; x < gridWidth; x++)
            {
                reserved.Add(new Vector2Int(x, target.position.y));
            }
        }

        return reserved;
    }

    private bool TryFindRandomFreePositionAvoiding(
        System.Random rng,
        HashSet<Vector2Int> occupied,
        HashSet<Vector2Int> reserved,
        bool horizontal,
        int length,
        out Vector2Int position)
    {
        position = Vector2Int.zero;
        List<Vector2Int> options = new List<Vector2Int>();
        List<Vector2Int> fallback = new List<Vector2Int>();

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
                    if (!AreCellsFree(occupied, pos, true, length))
                    {
                        continue;
                    }

                    if (TouchesReserved(pos, true, length, reserved))
                    {
                        fallback.Add(pos);
                    }
                    else
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
                    if (!AreCellsFree(occupied, pos, false, length))
                    {
                        continue;
                    }

                    if (TouchesReserved(pos, false, length, reserved))
                    {
                        fallback.Add(pos);
                    }
                    else
                    {
                        options.Add(pos);
                    }
                }
            }
        }

        List<Vector2Int> pool = options.Count > 0 ? options : fallback;
        if (pool.Count == 0)
        {
            return false;
        }

        // Sparse: vermijd fallback (reserved) helemaal als er alternatieven zijn.
        if (constructiveSparseFiller && options.Count > 0)
        {
            pool = options;
        }

        position = pool[rng.Next(0, pool.Count)];
        return true;
    }

    private static bool TouchesReserved(
        Vector2Int pos,
        bool horizontal,
        int length,
        HashSet<Vector2Int> reserved)
    {
        if (reserved == null || reserved.Count == 0)
        {
            return false;
        }

        foreach (Vector2Int cell in GetCells(pos, horizontal, length))
        {
            if (reserved.Contains(cell))
            {
                return true;
            }
        }

        return false;
    }

    private List<Vector2Int> CollectEscapeCorridorCells(
        Vector2Int pos,
        bool horizontal,
        int length,
        int escapeSign,
        int maxSteps)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        if (escapeSign == 0)
        {
            return cells;
        }

        if (horizontal)
        {
            if (escapeSign > 0)
            {
                int start = pos.x + length;
                for (int s = 0; s < maxSteps; s++)
                {
                    int x = start + s;
                    if (x >= gridWidth)
                    {
                        break;
                    }

                    cells.Add(new Vector2Int(x, pos.y));
                }
            }
            else
            {
                for (int s = 1; s <= maxSteps; s++)
                {
                    int x = pos.x - s;
                    if (x < 0)
                    {
                        break;
                    }

                    cells.Add(new Vector2Int(x, pos.y));
                }
            }
        }
        else
        {
            if (escapeSign > 0)
            {
                int start = pos.y + length;
                for (int s = 0; s < maxSteps; s++)
                {
                    int y = start + s;
                    if (y >= gridHeight)
                    {
                        break;
                    }

                    cells.Add(new Vector2Int(pos.x, y));
                }
            }
            else
            {
                for (int s = 1; s <= maxSteps; s++)
                {
                    int y = pos.y - s;
                    if (y < 0)
                    {
                        break;
                    }

                    cells.Add(new Vector2Int(pos.x, y));
                }
            }
        }

        return cells;
    }

    private bool TryPickEscapeSign(
        Vector2Int pos,
        bool horizontal,
        int length,
        HashSet<Vector2Int> occupied,
        out int escapeSign)
    {
        bool plus = EscapeHasClearStep(pos, horizontal, length, +1, occupied);
        bool minus = EscapeHasClearStep(pos, horizontal, length, -1, occupied);
        if (!plus && !minus)
        {
            escapeSign = 0;
            return false;
        }

        if (plus && !minus)
        {
            escapeSign = +1;
            return true;
        }

        if (!plus && minus)
        {
            escapeSign = -1;
            return true;
        }

        // Beide kanten open: kies de kortere corridor (makkelijker te blokkeren / sneller keten).
        int plusLen = CollectEscapeCorridorCells(pos, horizontal, length, +1, gridWidth + gridHeight).Count;
        int minusLen = CollectEscapeCorridorCells(pos, horizontal, length, -1, gridWidth + gridHeight).Count;
        escapeSign = plusLen <= minusLen ? +1 : -1;
        return true;
    }

    private bool EscapeHasClearStep(
        Vector2Int pos,
        bool horizontal,
        int length,
        int escapeSign,
        HashSet<Vector2Int> occupied)
    {
        if (escapeSign == 0)
        {
            return false;
        }

        Vector2Int check;
        if (horizontal)
        {
            check = escapeSign > 0
                ? new Vector2Int(pos.x + length, pos.y)
                : new Vector2Int(pos.x - 1, pos.y);
        }
        else
        {
            check = escapeSign > 0
                ? new Vector2Int(pos.x, pos.y + length)
                : new Vector2Int(pos.x, pos.y - 1);
        }

        if (check.x < 0 || check.x >= gridWidth || check.y < 0 || check.y >= gridHeight)
        {
            return false;
        }

        if (occupied == null)
        {
            return true;
        }

        return !occupied.Contains(check);
    }

    private static bool BlocksOppositeEscape(
        CandidateVehicle prev,
        int blockedEscapeSign,
        PlacementOption candidate)
    {
        int opposite = -blockedEscapeSign;
        Vector2Int first;
        if (prev.horizontal)
        {
            first = opposite > 0
                ? new Vector2Int(prev.position.x + prev.length, prev.position.y)
                : new Vector2Int(prev.position.x - 1, prev.position.y);
        }
        else
        {
            first = opposite > 0
                ? new Vector2Int(prev.position.x, prev.position.y + prev.length)
                : new Vector2Int(prev.position.x, prev.position.y - 1);
        }

        foreach (Vector2Int cell in GetCells(candidate.position, candidate.horizontal, candidate.length))
        {
            if (cell == first)
            {
                return true;
            }
        }

        return false;
    }

    private static bool EscapeCorridorHitsVehicle(
        Vector2Int pos,
        bool horizontal,
        int length,
        int escapeSign,
        CandidateVehicle other)
    {
        HashSet<Vector2Int> otherCells = new HashSet<Vector2Int>(
            GetCells(other.position, other.horizontal, other.length)
        );

        // Check first few escape cells.
        for (int s = 1; s <= 4; s++)
        {
            Vector2Int cell;
            if (horizontal)
            {
                cell = escapeSign > 0
                    ? new Vector2Int(pos.x + length - 1 + s, pos.y)
                    : new Vector2Int(pos.x - s, pos.y);
            }
            else
            {
                cell = escapeSign > 0
                    ? new Vector2Int(pos.x, pos.y + length - 1 + s)
                    : new Vector2Int(pos.x, pos.y - s);
            }

            if (otherCells.Contains(cell))
            {
                return true;
            }
        }

        return false;
    }

    private static void ShuffleOptions(List<PlacementOption> options, System.Random rng)
    {
        for (int i = options.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            PlacementOption tmp = options[i];
            options[i] = options[j];
            options[j] = tmp;
        }
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

        for (int attempt = 0; attempt < placementAttemptsPerVehicle; attempt++)
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

        for (int attempt = 0; attempt < placementAttemptsPerVehicle; attempt++)
        {
            bool preferHorizontal = rng.Next(0, 2) == 0;
            int length = PickNonTargetVehicleLength(
                rng,
                candidate,
                length3Placed,
                maxLength3,
                0.45
            );

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
                if (length >= 3)
                {
                    length3Placed++;
                }

                failReason = PlacementFailReason.None;
                return true;
            }

            // Length-2 fallback als langere length geen plek had.
            if (length >= 3)
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

    /// <summary>
    /// Kiest length 2/3/4 voor non-target vehicles.
    /// Length 4 alleen als allowLength4Vehicles + maxLength4Vehicles + long-budget.
    /// </summary>
    private int PickNonTargetVehicleLength(
        System.Random rng,
        CandidateLevel candidate,
        int length3Placed,
        int maxLength3,
        double longVehicleChance)
    {
        if (length3Placed >= maxLength3 || rng.NextDouble() >= longVehicleChance)
        {
            return 2;
        }

        bool canLength4 =
            allowLength4Vehicles &&
            maxLength4Vehicles > 0 &&
            CountNonTargetLength(candidate, 4) < maxLength4Vehicles &&
            Mathf.Min(gridWidth, gridHeight) >= 4;

        // Onder de long picks: soms length 4, anders length 3.
        if (canLength4 && rng.NextDouble() < 0.40)
        {
            return 4;
        }

        return 3;
    }

    private static int CountNonTargetLength(CandidateLevel candidate, int length)
    {
        int count = 0;
        for (int i = 0; i < candidate.vehicles.Count; i++)
        {
            CandidateVehicle v = candidate.vehicles[i];
            if (!v.canExitRight && v.length == length)
            {
                count++;
            }
        }

        return count;
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
    private enum StructureRejectReason
    {
        None,
        RepetitiveRatio,
        LowComplexityScore
    }

    private sealed class SolutionComplexityMetrics
    {
        public int moveCount;
        public int uniqueVehiclesMoved;
        public int horizontalMoves;
        public int verticalMoves;
        public int axisAlternations;
        public int vehicleRevisits;
        public int directionChanges;
        public int longestSingleAxisRun;
        public int longestLinearDependencyRun;
        public float uniqueVehicleRatio;
        public float linearChainSolutionRatio;
        public int secondaryBlockersUsed;
        public int forkDependencies;
        public int acceptanceComplexityScore;
    }

    private SolutionComplexityMetrics AnalyzeSolutionComplexity(
        LevelSolver.SolverResult result,
        CandidateLevel candidate)
    {
        SolutionComplexityMetrics m = new SolutionComplexityMetrics();
        if (result == null || result.solution == null || result.solution.Count == 0)
        {
            return m;
        }

        Dictionary<string, bool> orientationByName = new Dictionary<string, bool>();
        for (int i = 0; i < candidate.vehicles.Count; i++)
        {
            CandidateVehicle v = candidate.vehicles[i];
            if (!string.IsNullOrEmpty(v.name))
            {
                orientationByName[v.name] = v.horizontal;
            }
        }

        List<LevelSolver.Move> solution = result.solution;
        m.moveCount = solution.Count;
        m.forkDependencies = candidate.forkDependencyCount;
        m.secondaryBlockersUsed = CountSecondaryVehiclesInSolution(result, candidate);

        HashSet<string> unique = new HashSet<string>();
        Dictionary<string, int> lastIndex = new Dictionary<string, int>();
        Dictionary<string, Vector2Int> lastDelta = new Dictionary<string, Vector2Int>();

        bool? prevHorizontal = null;
        int currentAxisRun = 0;

        for (int i = 0; i < solution.Count; i++)
        {
            LevelSolver.Move move = solution[i];
            string name = move.vehicleName ?? string.Empty;
            unique.Add(name);

            bool horizontal = true;
            if (orientationByName.TryGetValue(name, out bool h))
            {
                horizontal = h;
            }
            else
            {
                Vector2Int delta = move.toPosition - move.fromPosition;
                horizontal = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y);
            }

            if (horizontal)
            {
                m.horizontalMoves++;
            }
            else
            {
                m.verticalMoves++;
            }

            if (prevHorizontal.HasValue)
            {
                if (prevHorizontal.Value != horizontal)
                {
                    m.axisAlternations++;
                    currentAxisRun = 1;
                }
                else
                {
                    currentAxisRun++;
                }
            }
            else
            {
                currentAxisRun = 1;
            }

            m.longestSingleAxisRun = Mathf.Max(m.longestSingleAxisRun, currentAxisRun);
            prevHorizontal = horizontal;

            if (lastIndex.TryGetValue(name, out int prevIdx))
            {
                bool otherBetween = false;
                for (int j = prevIdx + 1; j < i; j++)
                {
                    if (solution[j].vehicleName != name)
                    {
                        otherBetween = true;
                        break;
                    }
                }

                if (otherBetween)
                {
                    m.vehicleRevisits++;
                }

                Vector2Int delta = move.toPosition - move.fromPosition;
                if (lastDelta.TryGetValue(name, out Vector2Int prevDelta))
                {
                    // Tegengestelde richting op dezelfde as.
                    if ((prevDelta.x != 0 && delta.x != 0 && prevDelta.x * delta.x < 0) ||
                        (prevDelta.y != 0 && delta.y != 0 && prevDelta.y * delta.y < 0))
                    {
                        m.directionChanges++;
                    }
                }

                lastDelta[name] = delta;
            }
            else
            {
                lastDelta[name] = move.toPosition - move.fromPosition;
            }

            lastIndex[name] = i;
        }

        m.uniqueVehiclesMoved = unique.Count;
        m.uniqueVehicleRatio = m.moveCount > 0
            ? m.uniqueVehiclesMoved / (float)m.moveCount
            : 0f;

        ComputeLinearChainMetrics(result, candidate, m);
        return m;
    }

    private static void ComputeLinearChainMetrics(
        LevelSolver.SolverResult result,
        CandidateLevel candidate,
        SolutionComplexityMetrics m)
    {
        m.linearChainSolutionRatio = 0f;
        m.longestLinearDependencyRun = 0;
        if (candidate.dependencies == null || candidate.dependencies.Count == 0 ||
            result.solution == null)
        {
            return;
        }

        // Verwachte pure reverse MAIN-chain: last dep → … → first dep → Target
        List<string> expected = new List<string>(candidate.dependencies.Count + 1);
        for (int i = candidate.dependencies.Count - 1; i >= 0; i--)
        {
            expected.Add(candidate.dependencies[i].vehicle.name);
        }

        expected.Add("Target");

        HashSet<string> expectedSet = new HashSet<string>(expected);
        HashSet<string> secondaryNames = candidate.secondaryVehicleNames != null
            ? new HashSet<string>(candidate.secondaryVehicleNames)
            : new HashSet<string>();

        // Include secondary vehicles in the relevant sequence so a mandatory S
        // before/during the reverse chain lowers linearChainSolutionRatio.
        List<string> relevant = new List<string>();
        foreach (LevelSolver.Move move in result.solution)
        {
            if (string.IsNullOrEmpty(move.vehicleName))
            {
                continue;
            }

            bool isMainOrTarget = expectedSet.Contains(move.vehicleName);
            bool isSecondary = secondaryNames.Contains(move.vehicleName);
            if (!isMainOrTarget && !isSecondary)
            {
                continue;
            }

            if (!relevant.Contains(move.vehicleName))
            {
                relevant.Add(move.vehicleName);
            }
        }

        int matched = 0;
        int n = Mathf.Min(relevant.Count, expected.Count);
        for (int i = 0; i < n; i++)
        {
            if (relevant[i] == expected[i])
            {
                matched++;
            }
            else
            {
                break;
            }
        }

        m.longestLinearDependencyRun = matched;
        m.linearChainSolutionRatio = expected.Count > 0
            ? matched / (float)expected.Count
            : 0f;
    }

    private StructureRejectReason EvaluateSolutionStructure(
        SolutionComplexityMetrics m,
        CandidateLevel candidate)
    {
        if (m.moveCount >= 5 && m.uniqueVehicleRatio < minSolutionVehicleRatio)
        {
            return StructureRejectReason.RepetitiveRatio;
        }

        int score = ComputeAcceptanceComplexityScore(m);
        m.acceptanceComplexityScore = score;
        if (score < effectiveMinSolutionComplexityScore)
        {
            return StructureRejectReason.LowComplexityScore;
        }

        return StructureRejectReason.None;
    }

    /// <summary>
    /// Composite post-solver interest score. Multiple paths to the threshold.
    /// </summary>
    private int ComputeAcceptanceComplexityScore(SolutionComplexityMetrics m)
    {
        int score = 0;
        if (m.axisAlternations >= 3)
        {
            score += 2;
        }

        if (m.vehicleRevisits >= 1)
        {
            score += 2;
        }

        if (m.secondaryBlockersUsed >= 1)
        {
            score += 2;
        }

        if (m.forkDependencies >= 1)
        {
            score += 2;
        }

        if (m.linearChainSolutionRatio <= effectiveMaxLinearChainSolutionRatio + 0.0001f)
        {
            score += 2;
        }

        if (m.longestSingleAxisRun <= Mathf.Max(1, effectiveMaxSingleAxisRun))
        {
            score += 1;
        }

        if (m.uniqueVehiclesMoved >= 6)
        {
            score += 1;
        }

        return score;
    }

    private void WriteSolutionStructureToLevel(
        LevelData level,
        SolutionComplexityMetrics m,
        CandidateLevel candidate,
        LevelSolver.SolverResult result)
    {
        level.uniqueVehiclesInSolution = m.uniqueVehiclesMoved;
        level.axisAlternations = m.axisAlternations;
        level.vehicleRevisits = m.vehicleRevisits;
        level.longestSingleAxisRun = m.longestSingleAxisRun;
        level.linearChainSolutionRatio = m.linearChainSolutionRatio;
        level.secondaryBlockersUsed = m.secondaryBlockersUsed;
        level.forkDependencies = m.forkDependencies;

        int acceptance = m.acceptanceComplexityScore > 0
            ? m.acceptanceComplexityScore
            : ComputeAcceptanceComplexityScore(m);
        level.solutionComplexityScore = acceptance;

        // Lichte boost bovenop bestaande difficultyScore.
        if (level.difficultyScore >= 0)
        {
            level.difficultyScore += acceptance * 10;
        }

        result.difficultyScore = level.difficultyScore;
    }

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
    /// Aantal geldige vehicle-destination moves vanuit de startstate (branching estimate).
    /// </summary>
    private static int CountInitialBranching(CandidateLevel candidate)
    {
        int total = 0;
        for (int i = 0; i < candidate.vehicles.Count; i++)
        {
            total += CountDestinationsForVehicle(candidate, i);
        }

        return total;
    }

    /// <summary>
    /// Aantal niet-target voertuigen dat minstens één geldige gridstap kan maken.
    /// </summary>
    private static int CountInitialMovableVehicles(CandidateLevel candidate)
    {
        int movable = 0;
        for (int i = 0; i < candidate.vehicles.Count; i++)
        {
            CandidateVehicle v = candidate.vehicles[i];
            if (v.canExitRight)
            {
                continue;
            }

            if (CanMakeAnyMove(candidate, i))
            {
                movable++;
            }
        }

        return movable;
    }

    private static int CountDestinationsForVehicle(CandidateLevel candidate, int vehicleIndex)
    {
        CandidateVehicle v = candidate.vehicles[vehicleIndex];
        bool[,] occupied = BuildOccupancy(candidate, ignoreIndex: vehicleIndex);
        int count = 0;

        if (v.horizontal)
        {
            for (int steps = 1; ; steps++)
            {
                int newX = v.position.x - steps;
                if (newX < 0 || occupied[v.position.x - steps, v.position.y])
                {
                    break;
                }

                count++;
            }

            for (int steps = 1; ; steps++)
            {
                int rightMost = v.position.x + v.length - 1 + steps;
                if (rightMost >= candidate.gridWidth || occupied[rightMost, v.position.y])
                {
                    break;
                }

                count++;
            }
        }
        else
        {
            for (int steps = 1; ; steps++)
            {
                int newY = v.position.y - steps;
                if (newY < 0 || occupied[v.position.x, newY])
                {
                    break;
                }

                count++;
            }

            for (int steps = 1; ; steps++)
            {
                int topMost = v.position.y + v.length - 1 + steps;
                if (topMost >= candidate.gridHeight || occupied[v.position.x, topMost])
                {
                    break;
                }

                count++;
            }
        }

        return count;
    }

    private static int CountChainVehiclesInSolution(
        LevelSolver.SolverResult result,
        CandidateLevel candidate)
    {
        if (result.solution == null ||
            candidate.chainVehicleNames == null ||
            candidate.chainVehicleNames.Count == 0)
        {
            return 0;
        }

        HashSet<string> chainNames = new HashSet<string>(candidate.chainVehicleNames);
        HashSet<string> used = new HashSet<string>();
        foreach (LevelSolver.Move move in result.solution)
        {
            if (!string.IsNullOrEmpty(move.vehicleName) && chainNames.Contains(move.vehicleName))
            {
                used.Add(move.vehicleName);
            }
        }

        return used.Count;
    }

    private static int CountSecondaryVehiclesInSolution(
        LevelSolver.SolverResult result,
        CandidateLevel candidate)
    {
        if (result.solution == null ||
            candidate.secondaryVehicleNames == null ||
            candidate.secondaryVehicleNames.Count == 0)
        {
            return 0;
        }

        HashSet<string> secondaryNames = new HashSet<string>(candidate.secondaryVehicleNames);
        HashSet<string> used = new HashSet<string>();
        foreach (LevelSolver.Move move in result.solution)
        {
            if (!string.IsNullOrEmpty(move.vehicleName) &&
                secondaryNames.Contains(move.vehicleName))
            {
                used.Add(move.vehicleName);
            }
        }

        return used.Count;
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
    /// Lengtebalans (max ~40% long vehicles length≥3 bij non-target) + oriëntatiebalans
    /// wanneer er genoeg voertuigen zijn (schaalt licht met board area).
    /// </summary>
    private bool PassesOrientationLengthBalance(CandidateLevel candidate)
    {
        int nonTarget = 0;
        int lengthLong = 0;
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
            if (v.length >= 3)
            {
                lengthLong++;
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
            float longRatio = lengthLong / (float)nonTarget;
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
    // Canonical hash (gameplay layout only — shared format with LevelCanonicalKey)
    // -------------------------------------------------------------------------

    private static string BuildCanonicalHash(CandidateLevel candidate)
    {
        int count = candidate.vehicles != null ? candidate.vehicles.Count : 0;
        bool[] canExitRight = new bool[count];
        int[] xs = new int[count];
        int[] ys = new int[count];
        bool[] horizontal = new bool[count];
        int[] lengths = new int[count];

        for (int i = 0; i < count; i++)
        {
            CandidateVehicle v = candidate.vehicles[i];
            canExitRight[i] = v.canExitRight;
            xs[i] = v.position.x;
            ys[i] = v.position.y;
            horizontal[i] = v.horizontal;
            lengths[i] = v.length;
        }

        return LevelCanonicalKey.BuildCanonicalLevelKey(
            candidate.gridWidth,
            candidate.gridHeight,
            candidate.exitRow,
            canExitRight,
            xs,
            ys,
            horizontal,
            lengths
        );
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
        foreach (Vector2Int cell in GetOccupiedCells(vehicle))
        {
            occupied.Add(cell);
        }
    }

    private static void Unoccupy(HashSet<Vector2Int> occupied, CandidateVehicle vehicle)
    {
        foreach (Vector2Int cell in GetOccupiedCells(vehicle))
        {
            occupied.Remove(cell);
        }
    }

    /// <summary>
    /// Gedeelde occupied-cell helper. gridPosition = minimum/left-bottom origin;
    /// horizontal: (x..x+len-1, y); vertical: (x, y..y+len-1).
    /// </summary>
    private static List<Vector2Int> GetOccupiedCells(CandidateVehicle vehicle)
    {
        return GetOccupiedCells(vehicle.position, vehicle.horizontal, vehicle.length);
    }

    private static List<Vector2Int> GetOccupiedCells(
        Vector2Int start,
        bool horizontal,
        int length)
    {
        return GetCells(start, horizontal, length);
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
        public bool usedConstructive;
        public int chainLength;
        public int initialBranching;
        public int initialMovableVehicleCount;
        public int initialDestinationMoveCount;
        public float chainParticipationRatio;
        public int chainOnlyMinimumMoves;
        public int targetConstructedMinMoves;
        public int initialChainLength;
        public int extendedChainLength;
        public int chainOnlyMovesBeforeExtension;
        public int chainOnlyMovesAfterExtension;
        public float secondaryParticipationRatio;
        public int secondaryBlockersUsed;
        public int forkDependencyCount;
        public int vehicleCountBeforeFillers;
        public int branchingBeforeFillers;
        public int branchingAfterFillers;
        public List<string> chainVehicleNames = new List<string>();
        public List<string> secondaryVehicleNames = new List<string>();
        public List<int> secondaryVehicleIndices = new List<int>();
        public List<ChainDependency> dependencies = new List<ChainDependency>();
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

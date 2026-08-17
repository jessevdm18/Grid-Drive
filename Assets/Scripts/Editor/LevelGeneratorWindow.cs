using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Editor-only Random level generator voor Grid Drive.
/// Menu: RushOut → Generate Levels
///
/// Pipeline: Random placement → cheap filters → RushOutSolver →
/// minimumMoves / basic quality → duplicate check → accept.
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
    private const int BaselineBoardArea = 36; // 6x6

    // --- Preset / batch ---
    private DifficultyPreset difficultyPreset = DifficultyPreset.Custom;
    private string batchName = "Custom_Batch_01";
    private string outputFolder = "Assets/Data/GeneratedLevels/Custom";

    // --- Grid ---
    private int gridWidth = LevelData.DefaultGridSize;
    private int gridHeight = LevelData.DefaultGridSize;

    // --- Basics ---
    private int numberOfLevels = 10;
    private int minVehicles = 4;
    private int maxVehicles = 10;
    private int minMinimumMoves = 3;
    private int maxMinimumMoves = 12;
    private int maxAttemptsPerLevel = 500;
    private int randomSeed = 12345;

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

    // Length-4 trucks
    private bool allowLength4Vehicles = false;
    private int maxLength4Vehicles = 2;

    // Effective runtime values
    private int effectiveMinVehicles;
    private int effectiveMaxVehicles;
    private int effectiveMinMoves;
    private int effectiveMaxMoves;
    private int effectiveMinVehiclesUsedInSolution;
    private float effectiveMinOccupancy;
    private float effectiveMaxOccupancy;

    // Placement diagnostics (reset per GenerateLevels)
    private int failTargetPlacement;
    private int failDirectBlockerPlacement;
    private int failOrdinaryVehiclePlacement;
    private int failCouldNotReachVehicleCount;
    private int failInvalidBounds;
    private int failOverlap;

    private Vector2 scroll;

    // --- Special Mission Progression (EditorPrefs) ---
    private SpecialMissionProgressionUtility.Settings specialMissionSettings =
        SpecialMissionProgressionUtility.Settings.CreateDefaults();

    private bool specialMissionSettingsLoaded;
    private Vector2 specialMissionPreviewScroll;

    // --- Difficulty order preview ---
    private List<MainLevelDatabaseDifficultyOrderUtility.OrderRow> difficultyOrderPreview;
    private Vector2 difficultyOrderPreviewScroll;

    // --- MultiTarget suitability ---
    private List<MultiTargetSuitabilityAnalyzer.Result> multiTargetResults;
    private Vector2 multiTargetResultsScroll;
    private bool multiTargetShowOnlyGood;

    // --- Special Mission suitability (NoTouch / FragileCargo / LimitedVehicle) ---
    private List<SpecialMissionSuitabilityAnalyzer.Result> specialMissionSuitabilityResults;
    private Vector2 specialMissionSuitabilityScroll;
    private bool specialMissionSuitabilityShowOnlyGood;
    private int specialMissionSuitabilityTab; // 0 NoTouch, 1 Fragile, 2 Limited


    private int BoardArea => gridWidth * gridHeight;
    private float AreaScale => BoardArea / (float)BaselineBoardArea;

    #region Menu / UI

    [MenuItem("RushOut/Generate Levels")]
    public static void OpenWindow()
    {
        LevelGeneratorWindow window = GetWindow<LevelGeneratorWindow>("Generate Levels");
        window.minSize = new Vector2(700f, 700f);
        window.Show();
    }

    private void OnEnable()
    {
        minSize = new Vector2(700f, 700f);

        specialMissionSettings =
            SpecialMissionProgressionUtility.Settings.LoadFromEditorPrefs();
        specialMissionSettingsLoaded = true;
    }

    private void OnDisable()
    {
        if (specialMissionSettingsLoaded)
        {
            specialMissionSettings.SaveToEditorPrefs();
        }
    }

    private void OnGUI()
    {
        if (!specialMissionSettingsLoaded)
        {
            specialMissionSettings =
                SpecialMissionProgressionUtility.Settings.LoadFromEditorPrefs();
            specialMissionSettingsLoaded = true;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Grid Drive — Level Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space(6f);

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
        EditorGUILayout.HelpBox(
            "Pipeline: Random placement → cheap filters → solver → minimumMoves → " +
            "basic quality → duplicate check → accept.",
            MessageType.Info
        );

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
            "Almost-solved guard geldt alleen als Min Minimum Moves >= 4.\n" +
            "Handmatige wijzigingen na een preset worden gewoon gebruikt.\n" +
            "Duplicate detection: deze run + GeneratedLevels + MainLevelDatabase.",
            MessageType.None
        );

        EditorGUILayout.Space(12f);
        EditorGUILayout.HelpBox(
            "Gegenereerde levels worden NIET aan MainLevelDatabase toegevoegd. " +
            "Review ze eerst in de output-map.",
            MessageType.Info
        );

        DrawSpecialMissionProgressionSection();

        EditorGUILayout.Space(8f);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Levels", GUILayout.Height(36f)))
        {
            cancelGeneration = false;
            GenerateLevels();
        }

        if (GUILayout.Button("STOP / CANCEL", GUILayout.Height(36f), GUILayout.Width(140f)))
        {
            cancelGeneration = true;
            Debug.LogWarning("LevelGenerator: cancel requested.");
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    private void DrawSpecialMissionProgressionSection()
    {
        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("SPECIAL MISSION PROGRESSION", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Editor-only balancing. Preview herberekent live; LevelData wordt pas " +
            "gewijzigd bij Apply. Waarden blijven bewaard via EditorPrefs.\n" +
            "Regel: level % 10 == 0 → special (10/30 Ambulance, 20/40 MoveLimit).",
            MessageType.None
        );

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Ambulance", EditorStyles.boldLabel);
        specialMissionSettings.ambulanceSecondsPerMoveStart = EditorGUILayout.FloatField(
            "Start Seconds Per Move",
            specialMissionSettings.ambulanceSecondsPerMoveStart
        );
        specialMissionSettings.ambulanceSecondsPerMoveReduction = EditorGUILayout.FloatField(
            "Seconds Per Move Reduction",
            specialMissionSettings.ambulanceSecondsPerMoveReduction
        );
        specialMissionSettings.ambulanceSecondsPerMoveMin = EditorGUILayout.FloatField(
            "Minimum Seconds Per Move",
            specialMissionSettings.ambulanceSecondsPerMoveMin
        );
        specialMissionSettings.ambulanceBufferStart = EditorGUILayout.FloatField(
            "Start Buffer Seconds",
            specialMissionSettings.ambulanceBufferStart
        );
        specialMissionSettings.ambulanceBufferReduction = EditorGUILayout.FloatField(
            "Buffer Reduction",
            specialMissionSettings.ambulanceBufferReduction
        );
        specialMissionSettings.ambulanceBufferMin = EditorGUILayout.FloatField(
            "Minimum Buffer Seconds",
            specialMissionSettings.ambulanceBufferMin
        );
        specialMissionSettings.ambulanceTimeLimitMin = EditorGUILayout.FloatField(
            "Minimum Total Time",
            specialMissionSettings.ambulanceTimeLimitMin
        );

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Move Limit", EditorStyles.boldLabel);
        specialMissionSettings.moveLimitExtraStart = EditorGUILayout.IntField(
            "Start Extra Moves",
            specialMissionSettings.moveLimitExtraStart
        );
        specialMissionSettings.moveLimitExtraReduction = EditorGUILayout.IntField(
            "Extra Moves Reduction",
            specialMissionSettings.moveLimitExtraReduction
        );
        specialMissionSettings.moveLimitExtraMin = EditorGUILayout.IntField(
            "Minimum Extra Moves",
            specialMissionSettings.moveLimitExtraMin
        );

        if (EditorGUI.EndChangeCheck())
        {
            specialMissionSettings.SaveToEditorPrefs();
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Preview (read-only)", EditorStyles.boldLabel);
        DrawSpecialMissionPreview();

        EditorGUILayout.Space(6f);
        if (GUILayout.Button("Apply Special Mission Progression", GUILayout.Height(32f)))
        {
            if (EditorUtility.DisplayDialog(
                    "Apply Special Mission Progression",
                    "Schrijft objectiveType / timeLimitSeconds / moveLimit op alle levels " +
                    "in MainLevelDatabase met de huidige balancing-waarden.\n\n" +
                    "Handmatige overrides op die velden worden overschreven. Doorgaan?",
                    "Apply",
                    "Cancel"))
            {
                specialMissionSettings.SaveToEditorPrefs();
                SpecialMissionProgressionUtility.ApplyToMainLevelDatabase(
                    specialMissionSettings
                );
            }
        }

        if (GUILayout.Button("Reset Balancing To Defaults", GUILayout.Height(22f)))
        {
            if (EditorUtility.DisplayDialog(
                    "Reset Balancing",
                    "Zet Special Mission formula-waarden terug naar defaults?\n" +
                    "(Wijzigt nog geen LevelData.)",
                    "Reset",
                    "Cancel"))
            {
                specialMissionSettings =
                    SpecialMissionProgressionUtility.Settings.CreateDefaults();
                specialMissionSettings.SaveToEditorPrefs();
            }
        }

        DrawDifficultyOrderSection();
        DrawMultiTargetSuitabilitySection();
        DrawSpecialMissionSuitabilitySection();
    }

    private void DrawDifficultyOrderSection()
    {
        EditorGUILayout.Space(14f);
        EditorGUILayout.LabelField("DIFFICULTY ORDER (MainLevelDatabase)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Editor-only. Ranking: board area → minimumMoves → difficultyScore.\n" +
            "5x5 komt vóór 6x6. Preview wijzigt niets. Apply herschikt list + levelNumber " +
            "(geen asset rename).\n" +
            "Workflow: Accept levels → Preview → Apply Difficulty Order → " +
            "Apply Special Mission Progression.\n" +
            "Save (Unlocked/Stars) is INDEX-gebaseerd — test-progressie matcht content niet meer na Apply.\n" +
            "Alternatief (re-solve + difficultyScore): RushOut → Analyze And Sort Levels By Difficulty.",
            MessageType.None
        );

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Preview Difficulty Order", GUILayout.Height(28f)))
        {
            List<MainLevelDatabaseDifficultyOrderUtility.OrderRow> rows =
                MainLevelDatabaseDifficultyOrderUtility.BuildPreview();
            difficultyOrderPreview = rows;
            Debug.Log(MainLevelDatabaseDifficultyOrderUtility.FormatPreviewLog(rows));
        }

        if (GUILayout.Button("Apply Difficulty Order", GUILayout.Height(28f)))
        {
            if (EditorUtility.DisplayDialog(
                    "Apply Difficulty Order",
                    "Herschikt MainLevelDatabase (area → minMoves) en zet levelNumber = 1..N.\n\n" +
                    "Assetnamen blijven gelijk. Save/test-progressie is index-gebaseerd.\n" +
                    "Daarna Special Mission Progression opnieuw toepassen.\n\nDoorgaan?",
                    "Apply",
                    "Cancel"))
            {
                if (MainLevelDatabaseDifficultyOrderUtility.ApplyDifficultyOrder())
                {
                    difficultyOrderPreview =
                        MainLevelDatabaseDifficultyOrderUtility.BuildPreview();
                }
            }
        }

        EditorGUILayout.EndHorizontal();

        DrawDifficultyOrderPreview();
    }

    private void DrawDifficultyOrderPreview()
    {
        if (difficultyOrderPreview == null || difficultyOrderPreview.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Klik Preview Difficulty Order om de voorgestelde volgorde te zien.",
                MessageType.Info
            );
            return;
        }

        EditorGUILayout.LabelField("Preview (read-only)", EditorStyles.boldLabel);
        difficultyOrderPreviewScroll = EditorGUILayout.BeginScrollView(
            difficultyOrderPreviewScroll,
            GUILayout.Height(Mathf.Clamp(position.height * 0.22f, 180f, 280f))
        );

        for (int i = 0; i < difficultyOrderPreview.Count; i++)
        {
            MainLevelDatabaseDifficultyOrderUtility.OrderRow row = difficultyOrderPreview[i];
            string moved = row.currentIndex == row.proposedIndex
                ? "(same)"
                : "#" + (row.currentIndex + 1) + " → #" + (row.proposedIndex + 1);

            EditorGUILayout.HelpBox(
                moved + " | " + row.assetName + "\n" +
                row.gridWidth + "x" + row.gridHeight +
                " | minMoves " + row.minimumMoves +
                " | score " + row.difficultyScore +
                " | " + row.objectiveType +
                " | levelNumber " + row.currentLevelNumber +
                " → " + row.proposedLevelNumber,
                MessageType.None
            );
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawMultiTargetSuitabilitySection()
    {
        EditorGUILayout.Space(14f);
        EditorGUILayout.LabelField("MULTI TARGET SUITABILITY", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Editor-only, read-only heuristiek (geen solver).\n" +
            "Zoekt length-2 horizontals op exitRow als tweede target-kandidaat.\n" +
            "Immediate exit na first rescue = zware penalty. Wijzigt geen LevelData.",
            MessageType.None
        );

        multiTargetShowOnlyGood = EditorGUILayout.Toggle(
            "Show Only Good Candidates",
            multiTargetShowOnlyGood
        );

        if (GUILayout.Button("Analyze MultiTarget Candidates", GUILayout.Height(28f)))
        {
            multiTargetResults = MultiTargetSuitabilityAnalyzer.AnalyzeMainLevelDatabase();
            Debug.Log(
                MultiTargetSuitabilityAnalyzer.FormatResultsLog(
                    multiTargetResults,
                    multiTargetShowOnlyGood
                )
            );
        }

        DrawMultiTargetSuitabilityResults();
    }

    private void DrawMultiTargetSuitabilityResults()
    {
        if (multiTargetResults == null || multiTargetResults.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Klik Analyze MultiTarget Candidates voor resultaten.",
                MessageType.Info
            );
            return;
        }

        // Vaste Height i.p.v. MaxHeight: nested scroll inklappen voorkómen.
        // Groeit mee met window, min 280 / max 520.
        float resultsHeight = Mathf.Clamp(position.height * 0.48f, 280f, 520f);

        multiTargetResultsScroll = EditorGUILayout.BeginScrollView(
            multiTargetResultsScroll,
            GUILayout.Height(resultsHeight)
        );

        int shown = 0;
        for (int i = 0; i < multiTargetResults.Count; i++)
        {
            MultiTargetSuitabilityAnalyzer.Result r = multiTargetResults[i];
            if (multiTargetShowOnlyGood &&
                r.category != MultiTargetSuitabilityAnalyzer.Category.Good)
            {
                continue;
            }

            shown++;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField(
                "Level " + r.levelNumber + " — " + r.category +
                " — Score " + r.score,
                EditorStyles.boldLabel
            );
            EditorGUILayout.LabelField(
                r.assetName + "  |  " +
                r.gridWidth + "x" + r.gridHeight +
                "  |  exitRow " + r.exitRow +
                "  |  minMoves " + r.minimumMoves
            );

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Second: " + r.bestCandidateName + " @ " + r.bestCandidatePos
            );
            EditorGUILayout.LabelField(
                "Blockers: " + r.blockersAfterFirstRescue +
                "    Immediate Exit: " + (r.immediateExit ? "Yes" : "No")
            );
            EditorGUILayout.LabelField(
                "Primary: " + r.primaryTargetName + " @ " + r.primaryTargetPos
            );

            if (r.reasons != null && r.reasons.Count > 0)
            {
                EditorGUILayout.Space(4f);
                for (int j = 0; j < r.reasons.Count; j++)
                {
                    EditorGUILayout.LabelField(r.reasons[j]);
                }
            }

            EditorGUILayout.Space(4f);
            using (new EditorGUI.DisabledScope(r.level == null))
            {
                if (GUILayout.Button("Select Level", GUILayout.Width(120f)))
                {
                    Selection.activeObject = r.level;
                    EditorGUIUtility.PingObject(r.level);
                }
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6f);
        }

        if (shown == 0)
        {
            EditorGUILayout.HelpBox(
                "Geen GOOD kandidaten — zet filter uit of analyseer opnieuw.",
                MessageType.Info
            );
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawSpecialMissionSuitabilitySection()
    {
        EditorGUILayout.Space(14f);
        EditorGUILayout.LabelField("SPECIAL MISSION SUITABILITY", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Editor-only, read-only. Analyze NoTouch / FragileCargo / LimitedVehicle.\n" +
            "One RushOutSolver solve per level (solution path heuristics — not a guarantee).\n" +
            "Wijzigt geen LevelData / flags / objectiveType / database.",
            MessageType.None
        );

        specialMissionSuitabilityTab = GUILayout.Toolbar(
            specialMissionSuitabilityTab,
            new[] { "NO TOUCH", "FRAGILE CARGO", "LIMITED VEHICLE" }
        );

        specialMissionSuitabilityShowOnlyGood = EditorGUILayout.Toggle(
            "Show Only Good Candidates",
            specialMissionSuitabilityShowOnlyGood
        );

        if (GUILayout.Button("Analyze Special Mission Candidates", GUILayout.Height(28f)))
        {
            specialMissionSuitabilityResults =
                SpecialMissionSuitabilityAnalyzer.AnalyzeMainLevelDatabase();
            Debug.Log(
                SpecialMissionSuitabilityAnalyzer.FormatResultsLog(
                    specialMissionSuitabilityResults,
                    filterKind: null,
                    onlyGood: false
                )
            );
        }

        DrawSpecialMissionSuitabilityResults();
    }

    private void DrawSpecialMissionSuitabilityResults()
    {
        if (specialMissionSuitabilityResults == null ||
            specialMissionSuitabilityResults.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Klik Analyze Special Mission Candidates voor resultaten.",
                MessageType.Info
            );
            return;
        }

        SpecialMissionSuitabilityAnalyzer.MissionKind filterKind =
            (SpecialMissionSuitabilityAnalyzer.MissionKind)specialMissionSuitabilityTab;

        float resultsHeight = Mathf.Clamp(position.height * 0.48f, 280f, 520f);
        specialMissionSuitabilityScroll = EditorGUILayout.BeginScrollView(
            specialMissionSuitabilityScroll,
            GUILayout.Height(resultsHeight)
        );

        int shown = 0;
        for (int i = 0; i < specialMissionSuitabilityResults.Count; i++)
        {
            SpecialMissionSuitabilityAnalyzer.Result r = specialMissionSuitabilityResults[i];
            if (r.missionKind != filterKind)
            {
                continue;
            }

            if (specialMissionSuitabilityShowOnlyGood &&
                r.category != SpecialMissionSuitabilityAnalyzer.Category.Good)
            {
                continue;
            }

            shown++;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField(
                "LEVEL " + r.levelNumber + " — " +
                SpecialMissionSuitabilityAnalyzer.MissionKindLabel(r.missionKind) +
                " — " + r.category + " — " + r.score,
                EditorStyles.boldLabel
            );
            EditorGUILayout.LabelField(
                r.assetName + "  |  " +
                r.gridWidth + "x" + r.gridHeight +
                "  |  exitRow " + r.exitRow +
                "  |  minMoves " + r.minimumMoves +
                " (stored " + r.storedMinimumMoves + ")"
            );

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Recommended Vehicle: " + r.vehicleName
            );
            EditorGUILayout.LabelField(
                "@ " + r.vehiclePos +
                ", " + r.vehicleOrientation +
                ", Length " + r.vehicleLength
            );
            EditorGUILayout.LabelField(
                "Solution moves: " + r.vehicleMovesInSolution +
                "    First: " + FormatSolutionMoveIndex(r.firstMoveIndex) +
                "    Last: " + FormatSolutionMoveIndex(r.lastMoveIndex)
            );

            if (r.missionKind == SpecialMissionSuitabilityAnalyzer.MissionKind.FragileCargo)
            {
                EditorGUILayout.LabelField(
                    "Target moves in solution: " + r.targetMovesInSolution +
                    "    Recommended Cargo Limit: " + r.recommendedCargoLimit
                );
            }

            if (r.missionKind == SpecialMissionSuitabilityAnalyzer.MissionKind.LimitedVehicle)
            {
                EditorGUILayout.LabelField(
                    "Recommended Limited Limit: " + r.recommendedLimitedLimit
                );
            }

            EditorGUILayout.LabelField(
                "Best match on level: " +
                SpecialMissionSuitabilityAnalyzer.MissionKindLabel(r.bestMatchKind) +
                " (" + r.bestMatchScore + ")"
            );

            if (r.reasons != null && r.reasons.Count > 0)
            {
                EditorGUILayout.Space(4f);
                for (int j = 0; j < r.reasons.Count; j++)
                {
                    EditorGUILayout.LabelField(r.reasons[j]);
                }
            }

            EditorGUILayout.Space(4f);
            using (new EditorGUI.DisabledScope(r.level == null))
            {
                if (GUILayout.Button("Select Level", GUILayout.Width(120f)))
                {
                    Selection.activeObject = r.level;
                    EditorGUIUtility.PingObject(r.level);
                }
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6f);
        }

        if (shown == 0)
        {
            EditorGUILayout.HelpBox(
                "Geen resultaten voor deze filter — zet Show Only Good uit of analyseer opnieuw.",
                MessageType.Info
            );
        }

        EditorGUILayout.EndScrollView();
    }

    private static string FormatSolutionMoveIndex(int index)
    {
        return index < 0 ? "-" : (index + 1).ToString();
    }

    private void DrawSpecialMissionPreview()
    {
        List<SpecialMissionProgressionUtility.AssignmentResult> preview =
            SpecialMissionProgressionUtility.BuildSpecialPreview(specialMissionSettings);

        if (preview == null || preview.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Geen special levels (levelNumber % 10 == 0) in MainLevelDatabase.",
                MessageType.Info
            );
            return;
        }

        specialMissionPreviewScroll = EditorGUILayout.BeginScrollView(
            specialMissionPreviewScroll,
            GUILayout.Height(Mathf.Clamp(position.height * 0.18f, 140f, 220f))
        );

        for (int i = 0; i < preview.Count; i++)
        {
            SpecialMissionProgressionUtility.AssignmentResult row = preview[i];
            string body;

            if (row.objectiveType == LevelObjectiveType.TimedAmbulance)
            {
                body =
                    "Level " + row.levelNumber + "\n" +
                    "Timed Ambulance\n" +
                    "Minimum Moves: " + row.minimumMoves + "\n" +
                    "Calculated Time: " + row.timeLimitSeconds.ToString("0.#") + " sec";
            }
            else
            {
                body =
                    "Level " + row.levelNumber + "\n" +
                    "Move Limit\n" +
                    "Minimum Moves: " + row.minimumMoves + "\n" +
                    "Calculated Limit: " + row.moveLimit + " moves";
            }

            EditorGUILayout.HelpBox(body, MessageType.None);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawEffectiveGridSummary()
    {
        ResolveEffectiveSettings();

        EditorGUILayout.HelpBox(
            "Grid: " + gridWidth + "x" + gridHeight +
            "\nVehicles: " + effectiveMinVehicles + "-" + effectiveMaxVehicles +
            "\nMoves: " + effectiveMinMoves + "-" + effectiveMaxMoves +
            "\nOccupancy: " +
            effectiveMinOccupancy.ToString("0.00") + "-" +
            effectiveMaxOccupancy.ToString("0.00") +
            "\nMovable: " +
            minMovableRatio.ToString("0.00") + "-" +
            maxMovableRatio.ToString("0.00") +
            "\nLength 4: " +
            (allowLength4Vehicles
                ? ("on (max " + maxLength4Vehicles + ")")
                : "off") +
            "\nArea scale: " + AreaScale.ToString("0.00") +
            " (baseline 6x6 = 1.00)",
            MessageType.Info
        );
    }

    #endregion

    #region Presets

    private void ResolveEffectiveSettings()
    {
        effectiveMinVehicles = minVehicles;
        effectiveMaxVehicles = maxVehicles;
        effectiveMinMoves = minMinimumMoves;
        effectiveMaxMoves = maxMinimumMoves;
        effectiveMinVehiclesUsedInSolution = minVehiclesUsedInSolution;
        effectiveMinOccupancy = minBoardOccupancy;
        effectiveMaxOccupancy = maxBoardOccupancy;
    }

    private void ApplyDifficultyPreset(DifficultyPreset preset)
    {
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
        ApplyLength4DefaultsForGrid();
    }

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

        float vehicleScale = Mathf.Lerp(1f, areaScale, 0.6f);
        minVehicles = Mathf.Clamp(Mathf.RoundToInt(baseMinVehicles * vehicleScale), 2, 40);
        maxVehicles = Mathf.Clamp(
            Mathf.RoundToInt(baseMaxVehicles * vehicleScale),
            minVehicles,
            40
        );

        float occupancyScale = Mathf.Lerp(1f, 1f / Mathf.Sqrt(areaScale), 0.85f);
        minBoardOccupancy = Mathf.Clamp(baseMinOccupancy * occupancyScale, 0.12f, 0.55f);
        maxBoardOccupancy = Mathf.Clamp(
            baseMaxOccupancy * occupancyScale,
            minBoardOccupancy + 0.08f,
            0.85f
        );

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

        maxAttemptsPerLevel = Mathf.Clamp(
            Mathf.RoundToInt(baseAttempts * Mathf.Lerp(1f, areaScale, 0.5f)),
            baseAttempts,
            2000
        );

        if (areaScale >= 2.5f)
        {
            maxGenerationSecondsPerLevel = 30f;
            maxSolverStatesPerCandidate = 50000;
        }
    }

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

    #endregion

    #region Generation

    private void GenerateLevels()
    {
        cancelGeneration = false;

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
        maxLength4Vehicles = Mathf.Clamp(maxLength4Vehicles, 0, 8);

        if (maxBoardOccupancy < minBoardOccupancy)
        {
            maxBoardOccupancy = minBoardOccupancy;
        }

        if (maxMovableRatio < minMovableRatio)
        {
            maxMovableRatio = minMovableRatio;
        }

        ResetPlacementDiagnostics();
        ResolveEffectiveSettings();

        if (string.IsNullOrWhiteSpace(outputFolder))
        {
            Debug.LogError("LevelGenerator: Output Folder is leeg.");
            return;
        }

        EnsureFolderExists(outputFolder);

        string safeBatchName = SanitizeBatchName(batchName);
        LevelDifficulty tier = GetLevelDifficultyTier();
        System.Random rng = new System.Random(randomSeed);

        HashSet<string> existingLevelKeys = LevelCanonicalKey.CollectExistingLevelKeys(
            out int existingLevelKeysLoaded,
            out int generatedAssetsScanned,
            out int databaseLevelsScanned,
            out int existingDuplicateReports
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
        int rejectedOrientationLengthBalance = 0;
        int rejectedBlockers = 0;
        int rejectedAlmostSolved = 0;
        int solverInvocations = 0;
        int rejectedBeforeSolverCount = 0;

        double sumSolverMs = 0;
        double maxSolverMs = 0;
        double sumPlacementMs = 0;
        double sumTotalCandidateMs = 0;
        double sumDensityMs = 0;
        double sumMovableMs = 0;
        double sumDuplicateMs = 0;
        double sumOtherFilterMs = 0;
        int profiledCandidates = 0;

        int nextFileIndex = FindNextFileIndex(outputFolder, safeBatchName);
        Stopwatch batchWatch = Stopwatch.StartNew();

        Debug.Log(
            "LevelGenerator START | Random | grid=" + gridWidth + "x" + gridHeight +
            " | levels=" + numberOfLevels +
            " | vehicles=" + effectiveMinVehicles + "-" + effectiveMaxVehicles +
            " | moves=" + effectiveMinMoves + "-" + effectiveMaxMoves +
            " | existingKeys=" + existingLevelKeysLoaded
        );

        try
        {
            for (int levelIndex = 0; levelIndex < numberOfLevels; levelIndex++)
            {
                if (cancelGeneration)
                {
                    break;
                }

                bool found = false;
                double levelStartSeconds = batchWatch.Elapsed.TotalSeconds;
                int levelAttemptStart = totalAttempts;

                for (int attempt = 0; attempt < maxAttemptsPerLevel; attempt++)
                {
                    if (cancelGeneration)
                    {
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

                    CandidateLevel candidate = TryBuildRandomCandidate(rng, out PlacementFailReason failReason);
                    phaseWatch.Stop();
                    sumPlacementMs += phaseWatch.Elapsed.TotalMilliseconds;

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

                    // --- Pre-solver quality filters ---
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
                        continue;
                    }

                    sumDensityMs += phaseWatch.Elapsed.TotalMilliseconds;

                    phaseWatch.Restart();
                    float movableRatio = CalculateMovableRatio(candidate);
                    if (movableRatio < minMovableRatio || movableRatio > maxMovableRatio)
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

                    sumOtherFilterMs += phaseWatch.Elapsed.TotalMilliseconds;

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

                    // --- Solver ---
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
                        DestroyImmediate(tempLevel);
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    if (!result.solvable)
                    {
                        rejectedUnsolvable++;
                        DestroyImmediate(tempLevel);
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    // --- Post-solver quality ---
                    if (result.minimumMoves < effectiveMinMoves)
                    {
                        rejectedTooEasy++;
                        DestroyImmediate(tempLevel);
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    if (result.minimumMoves > effectiveMaxMoves)
                    {
                        rejectedTooHard++;
                        DestroyImmediate(tempLevel);
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    int uniqueVehiclesMoved = CountUniqueVehiclesInSolution(result);
                    if (uniqueVehiclesMoved < effectiveMinVehiclesUsedInSolution)
                    {
                        rejectedLowSolutionParticipation++;
                        DestroyImmediate(tempLevel);
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    if (minMinimumMoves >= 4 &&
                        directBlockers <= 1 &&
                        uniqueVehiclesMoved <= 2)
                    {
                        rejectedAlmostSolved++;
                        DestroyImmediate(tempLevel);
                        sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;
                        continue;
                    }

                    // Accept
                    tempLevel.levelNumber = accepted + 1;
                    tempLevel.difficulty = tier;
                    LevelSolver.ApplyMetadata(tempLevel, result);
                    tempLevel.uniqueVehiclesInSolution = uniqueVehiclesMoved;

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
                    sumTotalCandidateMs += candidateWatch.Elapsed.TotalMilliseconds;

                    Debug.Log(
                        "Accepted | tier=" + tier +
                        " | moves=" + tempLevel.minimumMoves +
                        " | uniqueVehicles=" + uniqueVehiclesMoved +
                        " | occupancy=" + occupancy.ToString("0.00") +
                        " | movableRatio=" + movableRatio.ToString("0.00") +
                        " | score=" + tempLevel.difficultyScore +
                        " | solverMs=" + solverMs.ToString("0.0") +
                        " | → " + assetPath
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

        double avgSolverPerInvocation =
            solverInvocations > 0 ? sumSolverMs / solverInvocations : 0;
        double solverInvocationRatio =
            totalAttempts > 0 ? (double)solverInvocations / totalAttempts : 0;
        double acceptanceRate =
            totalAttempts > 0 ? (double)accepted / totalAttempts : 0;
        double averageMinimumMovesAccepted =
            accepted > 0 ? (double)sumAcceptedMinimumMoves / accepted : 0;

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("=== LevelGenerator klaar ===");
        summary.AppendLine(
            "total elapsed seconds: " + batchWatch.Elapsed.TotalSeconds.ToString("0.00")
        );
        summary.AppendLine("attempts: " + totalAttempts);
        summary.AppendLine("accepted: " + accepted);
        summary.AppendLine(
            "acceptanceRate: " + acceptanceRate.ToString("0.000") +
            " (" + accepted + "/" + totalAttempts + ")"
        );
        summary.AppendLine(
            "averageMinimumMovesAccepted: " +
            (accepted > 0 ? averageMinimumMovesAccepted.ToString("0.00") : "n/a")
        );
        summary.AppendLine("solverInvocations: " + solverInvocations);
        summary.AppendLine(
            "solverInvocationRatio: " + solverInvocationRatio.ToString("0.000") +
            " (" + solverInvocations + "/" + totalAttempts + ")"
        );
        summary.AppendLine("average solver ms: " + avgSolverPerInvocation.ToString("0.00"));
        summary.AppendLine("maximum solver ms: " + maxSolverMs.ToString("0.00"));
        summary.AppendLine("");
        summary.AppendLine("--- rejected ---");
        summary.AppendLine("placement failed: " + rejectedPlacementFailed);
        summary.AppendLine("density: " + rejectedDensity);
        summary.AppendLine("movable ratio: " + rejectedMovableRatio);
        summary.AppendLine("blockers: " + rejectedBlockers);
        summary.AppendLine("trivial/open exit: " + rejectedTrivial);
        summary.AppendLine("duplicate: " + rejectedDuplicates);
        summary.AppendLine("  · duplicates this run: " + rejectedDuplicatesThisRun);
        summary.AppendLine(
            "  · duplicates against existing assets: " + rejectedDuplicatesAgainstExisting
        );
        summary.AppendLine("unsolvable: " + rejectedUnsolvable);
        summary.AppendLine("search limit: " + rejectedSearchLimit);
        summary.AppendLine("too easy: " + rejectedTooEasy);
        summary.AppendLine("too hard: " + rejectedTooHard);
        summary.AppendLine(
            "low solution participation: " + rejectedLowSolutionParticipation
        );
        summary.AppendLine("almost solved: " + rejectedAlmostSolved);
        summary.AppendLine(
            "orientation/length balance: " + rejectedOrientationLengthBalance
        );
        summary.AppendLine("invalid: " + rejectedInvalid);
        summary.AppendLine("pre-solver rejects: " + rejectedBeforeSolverCount);

        if (accepted == 0)
        {
            string topReject = FindTopRejectReason(
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
            summary.AppendLine("");
            summary.AppendLine("TOP REJECT REASON (accepted=0): " + topReject);
        }

        summary.AppendLine("");
        summary.AppendLine("--- batch ---");
        summary.AppendLine(
            "existing level keys loaded: " + existingLevelKeysLoaded +
            " (generated scanned=" + generatedAssetsScanned +
            ", database scanned=" + databaseLevelsScanned +
            ", existing duplicate reports=" + existingDuplicateReports + ")"
        );
        summary.AppendLine("grid: " + gridWidth + "x" + gridHeight);
        summary.AppendLine(
            "vehicles: " + effectiveMinVehicles + "-" + effectiveMaxVehicles
        );
        summary.AppendLine(
            "length4: " +
            (allowLength4Vehicles
                ? ("enabled, max " + maxLength4Vehicles)
                : "disabled")
        );
        summary.AppendLine(
            "minimumMoves range: " + effectiveMinMoves + "-" + effectiveMaxMoves
        );
        summary.AppendLine(
            "occupancy: " +
            effectiveMinOccupancy.ToString("0.00") + "-" +
            effectiveMaxOccupancy.ToString("0.00")
        );
        summary.AppendLine("preset: " + difficultyPreset);
        summary.AppendLine("batch: " + safeBatchName);
        summary.AppendLine("tier: " + tier);
        summary.AppendLine("output: " + outputFolder);
        summary.AppendLine("(niet toegevoegd aan MainLevelDatabase)");

        Debug.Log(summary.ToString());
        cancelGeneration = false;
    }

    private static string FindTopRejectReason(
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
            "\nRejected placement/density/unsolvable/searchLimit: " +
            rejectedPlacement + "/" + rejectedDensity + "/" +
            rejectedUnsolvable + "/" + rejectedSearchLimit
        );
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

    #endregion

    #region Random placement

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

        if (!TryPlaceTarget(rng, candidate, occupied, out failReason))
        {
            return null;
        }

        int nonTargetBudget = vehicleCount - 1;
        int maxLength3 = Mathf.FloorToInt(nonTargetBudget * maxLongVehicleRatio);
        int length3Placed = 0;

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

        for (int attempt = 0; attempt < placementAttemptsPerVehicle; attempt++)
        {
            int exitRow = rng.Next(0, gridHeight);
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
        if (chosen.length >= 3)
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

    #endregion

    #region Quality helpers

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
                if (cell.y == target.position.y &&
                    cell.x > rightMost &&
                    cell.x < candidate.gridWidth)
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

    private static bool CanMakeAnyMove(CandidateLevel candidate, int vehicleIndex)
    {
        CandidateVehicle v = candidate.vehicles[vehicleIndex];
        bool[,] occupied = BuildOccupancy(candidate, ignoreIndex: vehicleIndex);

        if (v.horizontal)
        {
            if (v.position.x - 1 >= 0 && !occupied[v.position.x - 1, v.position.y])
            {
                return true;
            }

            int rightMost = v.position.x + v.length;
            if (rightMost < candidate.gridWidth && !occupied[rightMost, v.position.y])
            {
                return true;
            }
        }
        else
        {
            if (v.position.y - 1 >= 0 && !occupied[v.position.x, v.position.y - 1])
            {
                return true;
            }

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
                if (cell.x >= 0 &&
                    cell.x < candidate.gridWidth &&
                    cell.y >= 0 &&
                    cell.y < candidate.gridHeight)
                {
                    occupied[cell.x, cell.y] = true;
                }
            }
        }

        return occupied;
    }

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
            float maxLong = Mathf.Lerp(0.40f, 0.45f, Mathf.Clamp01(AreaScale - 1f));
            if (longRatio > maxLong + 0.0001f)
            {
                return false;
            }
        }

        int orientationGate = Mathf.Max(
            6,
            Mathf.RoundToInt(6f * Mathf.Lerp(1f, AreaScale, 0.35f))
        );
        if (candidate.vehicles.Count >= orientationGate)
        {
            if (horizontal < 2 || vertical < 2)
            {
                return false;
            }
        }

        return true;
    }

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

    #endregion

    #region Canonical key / files / occupancy

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

    private static void Occupy(HashSet<Vector2Int> occupied, CandidateVehicle vehicle)
    {
        foreach (Vector2Int cell in GetOccupiedCells(vehicle))
        {
            occupied.Add(cell);
        }
    }

    private static List<Vector2Int> GetOccupiedCells(CandidateVehicle vehicle)
    {
        return GetCells(vehicle.position, vehicle.horizontal, vehicle.length);
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

    #endregion

    #region Types

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

    #endregion
}

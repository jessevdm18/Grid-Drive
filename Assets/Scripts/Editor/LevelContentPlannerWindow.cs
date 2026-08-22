using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only: handmatige Easy/Medium/Hard + objective assignment op MainLevelDatabase.
/// Planned state tot APPLY. Geen database reorder, geen levelNumber/save-identity wijzigingen.
/// </summary>
public class LevelContentPlannerWindow : EditorWindow
{
    private const string DatabaseFilter = "MainLevelDatabase t:LevelDatabase";
    private const float MinWidth = 520f;
    private const float MinHeight = 400f;

    private enum DifficultyFilter
    {
        All = 0,
        Easy = 1,
        Medium = 2,
        Hard = 3
    }

    private enum ObjectiveFilter
    {
        All = 0,
        Classic = 1,
        TimedAmbulance = 2,
        MoveLimit = 3,
        MultiTargetRescue = 4,
        NoTouchChallenge = 5,
        FragileCargo = 6,
        LimitedVehicle = 7
    }

    private enum SortMode
    {
        DbIndex = 0,
        MinimumMoves = 1,
        DifficultyScore = 2,
        GridArea = 3,
        Difficulty = 4,
        Objective = 5
    }

    private enum CurationViewFilter
    {
        MainDatabase = 0,
        Final = 1,
        Reserves = 2,
        Rejected = 3,
        NeedsReview = 4,
        AllCandidates = 5,
        ManualKeep = 6,
        Maybe = 7,
        Unreviewed = 8
    }

    private enum PlayabilityFilter
    {
        All = 0,
        Valid = 1,
        Invalid = 2,
        Timeout = 3,
        Unsolvable = 4,
        ObjectiveInvalid = 5,
        ConfigInvalid = 6
    }

    private sealed class RowState
    {
        public int dbIndex;
        public LevelData level;
        public LevelDifficulty plannedDifficulty;
        public LevelObjectiveType plannedObjective;
        public float plannedTimeLimit;
        public int plannedMoveLimit;
        public int plannedFragileLimit;
        public int plannedLimitedLimit;
        public bool selected;
        public V1CurationStatus curationStatus = V1CurationStatus.Unassigned;
        public bool needsReview;
        public string reviewReason = string.Empty;
        public float effectiveScore;
    }

    private struct SuitabilityScores
    {
        public bool hasData;
        public int noTouchScore;
        public SpecialMissionSuitabilityAnalyzer.Category noTouchCategory;
        public int fragileScore;
        public SpecialMissionSuitabilityAnalyzer.Category fragileCategory;
        public int limitedScore;
        public SpecialMissionSuitabilityAnalyzer.Category limitedCategory;
        public int multiTargetScore;
        public MultiTargetSuitabilityAnalyzer.Category multiTargetCategory;

        public int noTouchVehicleIndex;
        public string noTouchLabel;
        public int fragileVehicleIndex;
        public string fragileLabel;
        public int recommendedCargoLimit;
        public int fragileRequiredMoves;
        public int limitedVehicleIndex;
        public string limitedLabel;
        public int recommendedLimitedLimit;
        public int limitedRequiredMoves;
        public int multiPrimaryIndex;
        public int multiCandidateIndex;
        public string multiPrimaryLabel;
        public string multiCandidateLabel;

        public static SuitabilityScores CreateEmpty()
        {
            return new SuitabilityScores
            {
                hasData = true,
                noTouchVehicleIndex = -1,
                noTouchLabel = string.Empty,
                fragileVehicleIndex = -1,
                fragileLabel = string.Empty,
                recommendedCargoLimit = 0,
                fragileRequiredMoves = 0,
                limitedVehicleIndex = -1,
                limitedLabel = string.Empty,
                recommendedLimitedLimit = 0,
                limitedRequiredMoves = 0,
                multiPrimaryIndex = -1,
                multiCandidateIndex = -1,
                multiPrimaryLabel = string.Empty,
                multiCandidateLabel = string.Empty
            };
        }
    }

    private LevelDatabase database;
    private DifficultyProgressionConfig progressionConfig;
    private readonly List<RowState> rows = new List<RowState>();
    private readonly Dictionary<int, SuitabilityScores> suitabilityByDbIndex =
        new Dictionary<int, SuitabilityScores>();
    private readonly Dictionary<int, ProductionLevelPlayability.Report> playabilityByDbIndex =
        new Dictionary<int, ProductionLevelPlayability.Report>();
    private readonly Dictionary<int, ProductionLevelPlayability.DeepTimeoutResult> deepTimeoutByDbIndex =
        new Dictionary<int, ProductionLevelPlayability.DeepTimeoutResult>();
    private readonly Dictionary<int, bool> hintPathPassByDbIndex = new Dictionary<int, bool>();
    private PlayabilityFilter playabilityFilter = PlayabilityFilter.All;

    private Vector2 mainScrollPosition;
    private Vector2 tableHorizontalScroll;

    private DifficultyFilter difficultyFilter = DifficultyFilter.All;
    private ObjectiveFilter objectiveFilter = ObjectiveFilter.All;
    private int gridSizeFilterIndex;
    private readonly List<string> gridSizeLabels = new List<string> { "All" };
    private readonly List<Vector2Int> gridSizeValues = new List<Vector2Int>();
    private string searchText = string.Empty;
    private SortMode sortMode = SortMode.Difficulty;
    private bool sortAscending = true;
    private CurationViewFilter curationViewFilter = CurationViewFilter.MainDatabase;
    private V1CurationState curationState;

    private BulkDifficultyChoice bulkDifficulty = BulkDifficultyChoice.Easy;
    private BulkObjectiveChoice bulkObjective = BulkObjectiveChoice.Classic;

    private enum BulkDifficultyChoice
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    private enum BulkObjectiveChoice
    {
        Classic = 0,
        TimedAmbulance = 1,
        MoveLimit = 2,
        MultiTargetRescue = 3,
        NoTouchChallenge = 4,
        FragileCargo = 5,
        LimitedVehicle = 6
    }

    [MenuItem("RushOut/Level Content Planner")]
    public static void OpenWindow()
    {
        LevelContentPlannerWindow window = GetWindow<LevelContentPlannerWindow>(
            "Level Content Planner"
        );
        window.minSize = new Vector2(MinWidth, MinHeight);
        window.Show();
    }

    private void OnEnable()
    {
        minSize = new Vector2(MinWidth, MinHeight);
        ReloadFromDatabase();
    }

    private void OnDisable()
    {
        int dirty = CountDirtyRows();
        if (dirty > 0)
        {
            Debug.LogWarning(
                "Level Content Planner closed with " + dirty +
                " planned change(s) not applied."
            );
        }
    }

    private void OnGUI()
    {
        if (database == null && curationViewFilter == CurationViewFilter.MainDatabase)
        {
            EditorGUILayout.HelpBox(
                "MainLevelDatabase niet gevonden. Maak/hernoem asset of refresh.",
                MessageType.Error
            );
            if (GUILayout.Button("Reload", GUILayout.Height(28f)))
            {
                ReloadFromDatabase();
            }

            return;
        }

        // One vertical root scroll for the full planner (toolbar + levels + side panel + footer).
        // Levels table uses a height-locked scroll so nested vertical scrolls do not fight.
        mainScrollPosition = EditorGUILayout.BeginScrollView(mainScrollPosition);

        DrawHeader();
        EditorGUILayout.Space(4f);
        DrawCurationToolbar();
        EditorGUILayout.Space(4f);
        DrawToolbar();
        EditorGUILayout.Space(4f);
        DrawFiltersAndSort();
        EditorGUILayout.Space(4f);
        DrawBulkActions();
        EditorGUILayout.Space(4f);

        EditorGUILayout.BeginHorizontal();
        DrawTable();
        DrawSidePanel();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4f);
        DrawFooter();

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Level Content Planner", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "PRE-RELEASE DATABASE TOOL\n" +
            "Do not reorder/remove database entries after public release while save identity " +
            "is index-based.",
            MessageType.Warning
        );
        EditorGUILayout.HelpBox(
            "Planner does not reorder MainLevelDatabase. Existing save identities remain stable.\n" +
            "Planned Difficulty / Objective are window-state until APPLY.\n" +
            "After assigning Timed/MoveLimit, run Special Mission Progression if desired (not auto).",
            MessageType.Info
        );

        int dirty = CountDirtyRows();
        if (dirty > 0)
        {
            EditorGUILayout.HelpBox(
                dirty + " planned change(s) not applied.",
                MessageType.Warning
            );
        }
    }

    private void DrawCurationToolbar()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("V1 AUTO CURATION", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Candidates → Auto Curate → Final 150 + reserves → playtest → Reject → Replace.\n" +
            "Does not delete assets. MainLevelDatabase changes only via Apply Final Set.",
            MessageType.None
        );

        if (curationState == null)
        {
            curationState = V1CurationState.LoadOrCreate();
        }

        EditorGUILayout.LabelField(
            "Final " +
            (curationState.CountFinalDifficulty(LevelDifficulty.Easy) +
             curationState.CountFinalDifficulty(LevelDifficulty.Medium) +
             curationState.CountFinalDifficulty(LevelDifficulty.Hard)) +
            "/" + V1ReleaseContentPlan.TargetTotal +
            " | Easy " + curationState.CountFinalDifficulty(LevelDifficulty.Easy) + "/50" +
            " | Medium " + curationState.CountFinalDifficulty(LevelDifficulty.Medium) + "/50" +
            " | Hard " + curationState.CountFinalDifficulty(LevelDifficulty.Hard) + "/50" +
            " | Reserve " + curationState.CountStatus(V1CurationStatus.Reserve) +
            " | Rejected " + curationState.CountStatus(V1CurationStatus.Rejected),
            EditorStyles.miniLabel
        );

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto Curate V1", GUILayout.Height(28f)))
        {
            string report = V1AutoCurator.AutoCurate();
            curationState = V1CurationState.LoadOrCreate();
            curationViewFilter = CurationViewFilter.Final;
            ReloadFromCuration();
            EditorUtility.DisplayDialog("Auto Curate V1", TrimDialog(report), "OK");
        }

        if (GUILayout.Button("Replace Rejected", GUILayout.Height(28f)))
        {
            string report = V1AutoCurator.ReplaceRejected();
            curationState = V1CurationState.LoadOrCreate();
            ReloadFromCuration();
            EditorUtility.DisplayDialog("Replace Rejected", TrimDialog(report), "OK");
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reject Selected", GUILayout.Height(24f)))
        {
            RejectSelectedRows();
        }

        if (GUILayout.Button("Manual Keep Selected", GUILayout.Height(24f)))
        {
            ManualKeepSelectedRows();
        }

        if (GUILayout.Button("Reload Curation View", GUILayout.Height(24f)))
        {
            curationState = V1CurationState.LoadOrCreate();
            if (curationViewFilter == CurationViewFilter.MainDatabase)
            {
                ReloadFromDatabase();
            }
            else
            {
                ReloadFromCuration();
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("DANGEROUS — DATABASE WRITE", EditorStyles.boldLabel);
        GUI.backgroundColor = new Color(0.95f, 0.7f, 0.45f);
        if (GUILayout.Button("APPLY FINAL SET → DB", GUILayout.Height(28f)))
        {
            string report = V1AutoCurator.ApplyFinalToMainLevelDatabase();
            curationViewFilter = CurationViewFilter.MainDatabase;
            ReloadFromDatabase();
            EditorUtility.DisplayDialog("Apply Final Set", TrimDialog(report), "OK");
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();
    }

    private static string TrimDialog(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "(empty)";
        }

        return text.Length <= 1200 ? text : text.Substring(0, 1200) + "\n…(see Console)";
    }

    private void DrawToolbar()
    {
        // CHANGES / APPLY first — always visible above filters, never clipped off the right.
        DrawChangesApplySection();
        EditorGUILayout.Space(4f);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("DATABASE / ANALYSIS", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reload Database", GUILayout.Height(24f)))
        {
            if (CountDirtyRows() > 0)
            {
                bool ok = EditorUtility.DisplayDialog(
                    "Reload Database",
                    "Reload discards unapplied planned changes. Continue?",
                    "Reload",
                    "Cancel"
                );
                if (ok)
                {
                    curationViewFilter = CurationViewFilter.MainDatabase;
                    ReloadFromDatabase();
                }
            }
            else
            {
                curationViewFilter = CurationViewFilter.MainDatabase;
                ReloadFromDatabase();
            }
        }

        if (GUILayout.Button("Analyze Suitability", GUILayout.Height(24f)))
        {
            RunSuitabilityAnalyze();
        }

        if (GUILayout.Button("Validate All Playable Levels", GUILayout.Height(24f)))
        {
            RunPlayabilityValidation();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Deep Validate Timeouts", GUILayout.Height(24f)))
        {
            RunDeepValidateTimeouts();
        }

        if (GUILayout.Button("Verify Hint Paths", GUILayout.Height(24f)))
        {
            RunHintPathVerification();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2f);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("LEVEL DATA", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Recalculate Min Moves", GUILayout.Height(24f)))
        {
            ProductionLevelPlayability.MenuRecalculateMinMoves();
        }

        if (GUILayout.Button("Auto Classify Difficulty", GUILayout.Height(24f)))
        {
            AutoClassifyDifficultyByMinMoves();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField(
            "Planned difficulty only — " + LevelMinMovesDifficulty.RangesSummary,
            EditorStyles.miniLabel
        );
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2f);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("SPECIALS", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = suitabilityByDbIndex.Count > 0;
        if (GUILayout.Button("Auto Suggest Specials", GUILayout.Height(24f)))
        {
            AutoSuggestSpecials();
        }

        GUI.enabled = true;
        if (GUILayout.Button("Auto Tune Special Parameters", GUILayout.Height(24f)))
        {
            AutoTuneSpecialParameters();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Special Progression Report", GUILayout.Height(24f)))
        {
            LogSpecialProgressionReport();
        }

        if (GUILayout.Button("Reset Special Difficulty Defaults", GUILayout.Height(24f)))
        {
            SpecialObjectiveDifficultyConfig.MenuResetDefaults();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawChangesApplySection()
    {
        int dirty = CountDirtyRows();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("CHANGES", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            dirty + " planned change(s)",
            dirty > 0 ? EditorStyles.boldLabel : EditorStyles.miniLabel
        );

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Revert Planned", GUILayout.Height(28f)))
        {
            RevertPlannedChanges();
        }

        GUI.enabled = dirty > 0;
        GUI.backgroundColor = new Color(0.55f, 0.9f, 0.55f);
        if (GUILayout.Button("APPLY PLANNED ASSIGNMENTS", GUILayout.Height(28f)))
        {
            TryApplyPlannedAssignments();
        }

        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        if (dirty == 0)
        {
            EditorGUILayout.LabelField(
                "Apply disabled — no planned changes.",
                EditorStyles.miniLabel
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Apply writes Difficulty / Objective / special limits to LevelData assets.",
                MessageType.Warning
            );
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawFiltersAndSort()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Filters / Sort (view only — no asset writes)", EditorStyles.boldLabel);

        CurationViewFilter newView = (CurationViewFilter)EditorGUILayout.EnumPopup(
            "View Source",
            curationViewFilter
        );
        if (newView != curationViewFilter)
        {
            curationViewFilter = newView;
            if (curationViewFilter == CurationViewFilter.MainDatabase)
            {
                ReloadFromDatabase();
            }
            else
            {
                ReloadFromCuration();
            }
        }

        EditorGUILayout.BeginHorizontal();
        difficultyFilter = (DifficultyFilter)EditorGUILayout.EnumPopup(
            "Difficulty",
            difficultyFilter,
            GUILayout.Width(280f)
        );
        objectiveFilter = (ObjectiveFilter)EditorGUILayout.EnumPopup(
            "Objective",
            objectiveFilter,
            GUILayout.Width(320f)
        );
        EditorGUILayout.EndHorizontal();

        playabilityFilter = (PlayabilityFilter)EditorGUILayout.EnumPopup(
            "Playability",
            playabilityFilter
        );
        if (playabilityByDbIndex.Count == 0 && playabilityFilter != PlayabilityFilter.All)
        {
            EditorGUILayout.HelpBox(
                "Run VALIDATE ALL PLAYABLE LEVELS to populate playability statuses.",
                MessageType.Info
            );
        }

        EditorGUILayout.BeginHorizontal();
        if (gridSizeLabels.Count > 0)
        {
            gridSizeFilterIndex = EditorGUILayout.Popup(
                "Grid Size",
                Mathf.Clamp(gridSizeFilterIndex, 0, gridSizeLabels.Count - 1),
                gridSizeLabels.ToArray(),
                GUILayout.Width(280f)
            );
        }

        searchText = EditorGUILayout.TextField("Search", searchText);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        sortMode = (SortMode)EditorGUILayout.EnumPopup("Sort By", sortMode, GUILayout.Width(280f));
        sortAscending = EditorGUILayout.ToggleLeft("Ascending", sortAscending, GUILayout.Width(100f));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawBulkActions()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Bulk (planned state only)", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All Visible", GUILayout.Width(140f)))
        {
            SetVisibleSelection(true);
        }

        if (GUILayout.Button("Clear Selection", GUILayout.Width(130f)))
        {
            SetVisibleSelection(false);
        }

        int selectedCount = CountSelectedRows();
        EditorGUILayout.LabelField("Selected: " + selectedCount, GUILayout.Width(110f));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        bulkDifficulty = (BulkDifficultyChoice)EditorGUILayout.EnumPopup(
            "Set Difficulty",
            bulkDifficulty,
            GUILayout.Width(280f)
        );
        GUI.enabled = selectedCount > 0;
        if (GUILayout.Button("Apply To Selection", GUILayout.Width(140f)))
        {
            ApplyBulkDifficulty();
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        bulkObjective = (BulkObjectiveChoice)EditorGUILayout.EnumPopup(
            "Set Objective",
            bulkObjective,
            GUILayout.Width(280f)
        );
        GUI.enabled = selectedCount > 0;
        if (GUILayout.Button("Apply To Selection", GUILayout.Width(140f)))
        {
            ApplyBulkObjective();
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        if (curationViewFilter == CurationViewFilter.MainDatabase && database != null)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("DANGEROUS — DATABASE REMOVE", EditorStyles.boldLabel);
            GUI.enabled = selectedCount > 0;
            GUI.backgroundColor = new Color(0.95f, 0.55f, 0.5f);
            if (GUILayout.Button("REMOVE SELECTED FROM MAIN DATABASE", GUILayout.Height(28f)))
            {
                RemoveSelectedFromMainDatabase();
            }

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawTable()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);

        List<RowState> visible = BuildVisibleSortedRows();
        float rowHeight = EditorGUIUtility.singleLineHeight + 6f;
        float headerHeight = 24f;
        float contentWidth = 1680f;
        float contentHeight = headerHeight + visible.Count * rowHeight + 8f;
        // Horizontal scrollbar lane so full row height stays visible inside root vertical scroll.
        float scrollViewHeight = contentHeight + GUI.skin.horizontalScrollbar.fixedHeight + 4f;

        // Height-locked scroll: full row block stays visible; root scroll handles vertical.
        // Horizontal scrollbar appears when columns exceed window width.
        Vector2 tableScroll = EditorGUILayout.BeginScrollView(
            new Vector2(tableHorizontalScroll.x, 0f),
            GUILayout.Height(scrollViewHeight)
        );
        tableHorizontalScroll = new Vector2(tableScroll.x, 0f);

        Rect contentRect = GUILayoutUtility.GetRect(
            contentWidth,
            contentHeight,
            GUILayout.ExpandWidth(true)
        );

        float x = contentRect.x;
        float y = contentRect.y;

        DrawTableHeader(ref x, y);
        y += headerHeight;

        for (int i = 0; i < visible.Count; i++)
        {
            DrawTableRow(visible[i], contentRect.x, y, i);
            y += rowHeight;
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawTableHeader(ref float x, float y)
    {
        float h = 22f;
        GUI.Label(new Rect(x, y, 28f, h), "", EditorStyles.boldLabel);
        x += 28f;
        GUI.Label(new Rect(x, y, 44f, h), "DB#", EditorStyles.boldLabel);
        x += 44f;
        GUI.Label(new Rect(x, y, 44f, h), "Disp#", EditorStyles.boldLabel);
        x += 44f;
        GUI.Label(new Rect(x, y, 150f, h), "Asset", EditorStyles.boldLabel);
        x += 150f;
        GUI.Label(new Rect(x, y, 54f, h), "Grid", EditorStyles.boldLabel);
        x += 54f;
        GUI.Label(new Rect(x, y, 40f, h), "Min", EditorStyles.boldLabel);
        x += 40f;
        GUI.Label(new Rect(x, y, 48f, h), "Score", EditorStyles.boldLabel);
        x += 48f;
        GUI.Label(new Rect(x, y, 70f, h), "Difficulty", EditorStyles.boldLabel);
        x += 70f;
        GUI.Label(new Rect(x, y, 86f, h), "Planned Diff", EditorStyles.boldLabel);
        x += 86f;
        GUI.Label(new Rect(x, y, 100f, h), "Objective", EditorStyles.boldLabel);
        x += 100f;
        GUI.Label(new Rect(x, y, 120f, h), "Planned Obj", EditorStyles.boldLabel);
        x += 120f;
        GUI.Label(new Rect(x, y, 72f, h), "NoTouch", EditorStyles.boldLabel);
        x += 72f;
        GUI.Label(new Rect(x, y, 72f, h), "Fragile", EditorStyles.boldLabel);
        x += 72f;
        GUI.Label(new Rect(x, y, 72f, h), "Limited", EditorStyles.boldLabel);
        x += 72f;
        GUI.Label(new Rect(x, y, 72f, h), "MultiT", EditorStyles.boldLabel);
        x += 72f;
        GUI.Label(new Rect(x, y, 220f, h), "Warnings", EditorStyles.boldLabel);
        x += 220f;
        GUI.Label(new Rect(x, y, 70f, h), "Select", EditorStyles.boldLabel);
    }

    private void DrawTableRow(RowState row, float startX, float y, int visualIndex)
    {
        LevelData level = row.level;
        if (level == null)
        {
            return;
        }

        bool dirty = IsRowDirty(row);
        Rect bg = new Rect(startX, y, 1680f, EditorGUIUtility.singleLineHeight + 4f);
        if (dirty)
        {
            EditorGUI.DrawRect(bg, new Color(1f, 0.92f, 0.55f, 0.35f));
        }
        else if (visualIndex % 2 == 1)
        {
            EditorGUI.DrawRect(bg, new Color(0f, 0f, 0f, 0.06f));
        }

        float x = startX;
        float h = EditorGUIUtility.singleLineHeight;
        float padY = y + 2f;

        row.selected = GUI.Toggle(new Rect(x, padY, 24f, h), row.selected, GUIContent.none);
        x += 28f;

        GUI.Label(new Rect(x, padY, 44f, h), row.dbIndex.ToString());
        x += 44f;
        GUI.Label(new Rect(x, padY, 44f, h), BuildDisplayNumberLabel(row));
        x += 44f;
        GUI.Label(new Rect(x, padY, 150f, h), level.name);
        x += 150f;
        GUI.Label(
            new Rect(x, padY, 54f, h),
            level.ResolvedGridWidth + "x" + level.ResolvedGridHeight
        );
        x += 54f;
        GUI.Label(new Rect(x, padY, 40f, h), level.minimumMoves.ToString());
        x += 40f;
        GUI.Label(new Rect(x, padY, 48f, h), level.difficultyScore.ToString());
        x += 48f;
        GUI.Label(new Rect(x, padY, 70f, h), level.difficulty.ToString());
        x += 70f;

        EditorGUI.BeginChangeCheck();
        LevelDifficulty newDiff = (LevelDifficulty)EditorGUI.EnumPopup(
            new Rect(x, padY, 82f, h),
            row.plannedDifficulty
        );
        if (EditorGUI.EndChangeCheck())
        {
            row.plannedDifficulty = newDiff;
        }

        x += 86f;
        GUI.Label(new Rect(x, padY, 100f, h), ShortObjective(level.objectiveType));
        x += 100f;

        EditorGUI.BeginChangeCheck();
        LevelObjectiveType newObj = (LevelObjectiveType)EditorGUI.EnumPopup(
            new Rect(x, padY, 116f, h),
            row.plannedObjective
        );
        if (EditorGUI.EndChangeCheck())
        {
            row.plannedObjective = newObj;
            InitializePlannedLimitsForObjective(row);
        }

        x += 120f;

        SuitabilityScores scores;
        suitabilityByDbIndex.TryGetValue(row.dbIndex, out scores);
        DrawSuitabilityCell(ref x, padY, h, scores, 0);
        DrawSuitabilityCell(ref x, padY, h, scores, 1);
        DrawSuitabilityCell(ref x, padY, h, scores, 2);
        DrawSuitabilityCell(ref x, padY, h, scores, 3);

        string warning = BuildRowWarning(row);
        if (row.needsReview)
        {
            string review = string.IsNullOrEmpty(row.reviewReason)
                ? "Needs Review"
                : row.reviewReason;
            warning = string.IsNullOrEmpty(warning) ? review : warning + " | " + review;
        }

        if (curationViewFilter != CurationViewFilter.MainDatabase)
        {
            string statusTag = row.curationStatus.ToString();
            warning = string.IsNullOrEmpty(warning)
                ? statusTag
                : statusTag + " | " + warning;
        }

        Color prev = GUI.color;
        if (!string.IsNullOrEmpty(warning))
        {
            GUI.color = new Color(1f, 0.55f, 0.2f);
        }

        GUI.Label(new Rect(x, padY, 220f, h), new GUIContent(warning, warning));
        GUI.color = prev;
        x += 220f;

        if (GUI.Button(new Rect(x, padY, 54f, h), "Ping"))
        {
            Selection.activeObject = level;
            EditorGUIUtility.PingObject(level);
        }
    }

    private void DrawSuitabilityCell(
        ref float x,
        float y,
        float h,
        SuitabilityScores scores,
        int kind)
    {
        string text = "-";
        string tooltip = "Not analyzed.";
        Color color = Color.gray;

        if (scores.hasData)
        {
            int score;
            string cat;
            switch (kind)
            {
                case 0:
                    score = scores.noTouchScore;
                    cat = ShortCategory(scores.noTouchCategory);
                    color = CategoryColor(scores.noTouchCategory);
                    tooltip = "Protected candidate: " +
                        DescribeRecommendation(scores.noTouchLabel, scores.noTouchVehicleIndex);
                    break;
                case 1:
                    score = scores.fragileScore;
                    cat = ShortCategory(scores.fragileCategory);
                    color = CategoryColor(scores.fragileCategory);
                    tooltip = "Cargo candidate: " +
                        DescribeRecommendation(scores.fragileLabel, scores.fragileVehicleIndex) +
                        " | recommended limit " + scores.recommendedCargoLimit;
                    break;
                case 2:
                    score = scores.limitedScore;
                    cat = ShortCategory(scores.limitedCategory);
                    color = CategoryColor(scores.limitedCategory);
                    tooltip = "Limited candidate: " +
                        DescribeRecommendation(scores.limitedLabel, scores.limitedVehicleIndex) +
                        " | recommended limit " + scores.recommendedLimitedLimit;
                    break;
                default:
                    score = scores.multiTargetScore;
                    cat = ShortCategory(scores.multiTargetCategory);
                    color = CategoryColor(scores.multiTargetCategory);
                    tooltip = "Targets: " +
                        DescribeRecommendation(scores.multiPrimaryLabel, scores.multiPrimaryIndex) +
                        " + " +
                        DescribeRecommendation(
                            scores.multiCandidateLabel,
                            scores.multiCandidateIndex
                        );
                    break;
            }

            text = score + " " + cat;
        }

        Color prev = GUI.color;
        GUI.color = color;
        GUI.Label(new Rect(x, y, 72f, h), new GUIContent(text, tooltip));
        GUI.color = prev;
        x += 72f;
    }

    private void DrawSidePanel()
    {
        // Natural height — follows root vertical scroll (no nested ExpandHeight scroll).
        EditorGUILayout.BeginVertical("box", GUILayout.Width(280f));
        EditorGUILayout.LabelField("Planned Balance", EditorStyles.boldLabel);

        int easy = 0;
        int medium = 0;
        int hard = 0;
        int[] objectives = new int[7];
        int[,] matrix = new int[3, 7];

        for (int i = 0; i < rows.Count; i++)
        {
            RowState row = rows[i];
            if (row.level == null)
            {
                continue;
            }

            int d = (int)row.plannedDifficulty;
            int o = (int)row.plannedObjective;
            if (d >= 0 && d <= 2)
            {
                if (d == 0)
                {
                    easy++;
                }
                else if (d == 1)
                {
                    medium++;
                }
                else
                {
                    hard++;
                }
            }

            if (o >= 0 && o <= 6)
            {
                objectives[o]++;
                if (d >= 0 && d <= 2)
                {
                    matrix[d, o]++;
                }
            }
        }

        EditorGUILayout.LabelField("MIN MOVES RANGES", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(LevelMinMovesDifficulty.RangesSummary, EditorStyles.miniLabel);
        EditorGUILayout.Space(6f);

        EditorGUILayout.LabelField("DATABASE", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Total levels: " + (database != null ? database.LevelCount : rows.Count),
            EditorStyles.miniLabel
        );
        EditorGUILayout.LabelField("Rows in view source: " + rows.Count, EditorStyles.miniLabel);
        EditorGUILayout.Space(6f);

        EditorGUILayout.LabelField("DIFFICULTY", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Easy: " + easy);
        EditorGUILayout.LabelField("Medium: " + medium);
        EditorGUILayout.LabelField("Hard: " + hard);

        DrawTargetShortageLabel("Easy", easy, V1ReleaseContentPlan.TargetEasy);
        DrawTargetShortageLabel("Medium", medium, V1ReleaseContentPlan.TargetMedium);
        DrawTargetShortageLabel("Hard", hard, V1ReleaseContentPlan.TargetHard);
        EditorGUILayout.Space(6f);

        DrawSelectedRowSetupPanel();

        EditorGUILayout.Space(6f);
        DrawSpecialDifficultyPanel();

        EditorGUILayout.LabelField("OBJECTIVES", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Classic: " + objectives[0]);
        EditorGUILayout.LabelField("Timed: " + objectives[1]);
        EditorGUILayout.LabelField("MoveLimit: " + objectives[2]);
        EditorGUILayout.LabelField("MultiTarget: " + objectives[3]);
        EditorGUILayout.LabelField("NoTouch: " + objectives[4]);
        EditorGUILayout.LabelField("Fragile: " + objectives[5]);
        EditorGUILayout.LabelField("Limited: " + objectives[6]);
        EditorGUILayout.Space(6f);

        DrawMatrixBlock("Easy", matrix, 0);
        DrawMatrixBlock("Medium", matrix, 1);
        DrawMatrixBlock("Hard", matrix, 2);

        EditorGUILayout.Space(8f);
        DrawGridDistributionReport();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Progression Warnings", EditorStyles.boldLabel);
        List<string> progressionWarnings = BuildProgressionWarnings(easy, medium, hard);
        if (progressionWarnings.Count == 0)
        {
            EditorGUILayout.LabelField("None", EditorStyles.miniLabel);
        }
        else
        {
            for (int i = 0; i < progressionWarnings.Count; i++)
            {
                EditorGUILayout.HelpBox(progressionWarnings[i], MessageType.Warning);
            }
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("V1 Content Validation", EditorStyles.boldLabel);
        List<string> v1Warnings = BuildV1ContentWarnings(easy, medium, hard);
        if (v1Warnings.Count == 0)
        {
            EditorGUILayout.LabelField("V1 difficulty counts OK (50/50/50).", EditorStyles.miniLabel);
        }
        else
        {
            for (int i = 0; i < v1Warnings.Count; i++)
            {
                EditorGUILayout.HelpBox(v1Warnings[i], MessageType.Warning);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private static void DrawTargetShortageLabel(string label, int planned, int target)
    {
        if (planned >= target)
        {
            return;
        }

        EditorGUILayout.LabelField(
            "  " + label + " shortage: " + (target - planned) + " (target " + target + ")",
            EditorStyles.miniLabel
        );
    }

    /// <summary>
    /// Auto setup preview + editable planned limits for the first selected row.
    /// </summary>
    private void DrawSelectedRowSetupPanel()
    {
        RowState row = GetSelectedRowForPanel();
        if (row == null || row.level == null)
        {
            return;
        }

        EditorGUILayout.LabelField("AUTO SETUP PREVIEW", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "DB#" + row.dbIndex + " " + row.level.name,
            EditorStyles.miniLabel
        );
        EditorGUILayout.HelpBox(BuildAutoSetupPreview(row), MessageType.None);

        switch (row.plannedObjective)
        {
            case LevelObjectiveType.TimedAmbulance:
                row.plannedTimeLimit = EditorGUILayout.FloatField(
                    "Time Limit",
                    row.plannedTimeLimit
                );
                break;

            case LevelObjectiveType.MoveLimit:
                row.plannedMoveLimit = EditorGUILayout.IntField(
                    "Move Limit",
                    row.plannedMoveLimit
                );
                break;

            case LevelObjectiveType.FragileCargo:
                row.plannedFragileLimit = EditorGUILayout.IntField(
                    "Fragile Limit",
                    row.plannedFragileLimit
                );
                break;

            case LevelObjectiveType.LimitedVehicle:
                row.plannedLimitedLimit = EditorGUILayout.IntField(
                    "Limited Limit",
                    row.plannedLimitedLimit
                );
                break;
        }

        EditorGUILayout.Space(8f);
    }

    /// <summary>
    /// SPECIAL DIFFICULTY: current vs recommended + strictness for selected special.
    /// </summary>
    private void DrawSpecialDifficultyPanel()
    {
        EditorGUILayout.LabelField("SPECIAL DIFFICULTY", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Recommendations use LevelDifficulty + minimumMoves (Editor only).",
            EditorStyles.miniLabel
        );

        RowState row = GetSelectedRowForPanel();
        if (row == null || row.level == null)
        {
            EditorGUILayout.LabelField("Select a row to inspect.", EditorStyles.miniLabel);
            EditorGUILayout.Space(6f);
            return;
        }

        if (row.plannedObjective == LevelObjectiveType.Classic)
        {
            EditorGUILayout.LabelField("Classic — no special parameter.", EditorStyles.miniLabel);
            EditorGUILayout.Space(6f);
            return;
        }

        SpecialObjectiveDifficultyConfig.Settings settings =
            SpecialObjectiveDifficultyConfig.Settings.Load();
        float progress = SpecialObjectiveDifficultyRecommender.GetLocalProgress01(
            database,
            row.dbIndex,
            row.plannedDifficulty
        );
        string local = BuildDisplayNumberLabel(row);

        EditorGUILayout.LabelField(
            row.plannedDifficulty + " #" + local +
            "  MinMoves: " + row.level.minimumMoves +
            "  LocalProgress: " + progress.ToString("0%") ,
            EditorStyles.miniLabel
        );

        SuitabilityScores scores;
        bool hasScores = suitabilityByDbIndex.TryGetValue(row.dbIndex, out scores) &&
            scores.hasData;

        switch (row.plannedObjective)
        {
            case LevelObjectiveType.TimedAmbulance:
            {
                var rec = SpecialObjectiveDifficultyRecommender.RecommendTimed(
                    row.level,
                    row.plannedDifficulty,
                    progress,
                    settings
                );
                float current = row.plannedTimeLimit > 0f
                    ? row.plannedTimeLimit
                    : row.level.timeLimitSeconds;
                var strict = SpecialObjectiveDifficultyRecommender.EvaluateStrictness(
                    current,
                    rec.recommendedSeconds
                );
                EditorGUILayout.LabelField(
                    "Current Time: " + current.ToString("0.#") + "s",
                    EditorStyles.miniLabel
                );
                EditorGUILayout.LabelField(
                    "Recommended: " + rec.recommendedSeconds.ToString("0.#") + "s" +
                    "  (" + rec.secondsPerMoveUsed.ToString("0.00") + "s/move + " +
                    rec.bufferUsed.ToString("0.#") + ")",
                    EditorStyles.miniLabel
                );
                EditorGUILayout.LabelField(
                    "→ " + SpecialObjectiveDifficultyRecommender.StrictnessLabel(strict),
                    EditorStyles.boldLabel
                );
                break;
            }

            case LevelObjectiveType.MoveLimit:
            {
                var rec = SpecialObjectiveDifficultyRecommender.RecommendMoveLimit(
                    row.level,
                    row.plannedDifficulty,
                    progress,
                    settings
                );
                int current = row.plannedMoveLimit > 0
                    ? row.plannedMoveLimit
                    : row.level.moveLimit;
                var strict = SpecialObjectiveDifficultyRecommender.EvaluateStrictnessInt(
                    current,
                    rec.recommended,
                    higherIsEasier: true
                );
                EditorGUILayout.LabelField("Current Limit: " + current, EditorStyles.miniLabel);
                EditorGUILayout.LabelField(
                    "Recommended: " + rec.recommended +
                    "  (minMoves+" + (rec.recommended - row.level.minimumMoves) + ")",
                    EditorStyles.miniLabel
                );
                EditorGUILayout.LabelField(
                    "→ " + SpecialObjectiveDifficultyRecommender.StrictnessLabel(strict),
                    EditorStyles.boldLabel
                );
                break;
            }

            case LevelObjectiveType.FragileCargo:
            {
                int required = hasScores ? scores.fragileRequiredMoves : 0;
                if (required <= 0)
                {
                    EditorGUILayout.LabelField(
                        "Run Analyze Suitability for required cargo moves.",
                        EditorStyles.miniLabel
                    );
                    break;
                }

                var rec = SpecialObjectiveDifficultyRecommender.RecommendFragile(
                    required,
                    row.level,
                    row.plannedDifficulty,
                    settings
                );
                int current = row.plannedFragileLimit > 0
                    ? row.plannedFragileLimit
                    : row.level.fragileCargoMoveLimit;
                var strict = SpecialObjectiveDifficultyRecommender.EvaluateStrictnessInt(
                    current,
                    rec.recommended,
                    higherIsEasier: true
                );
                EditorGUILayout.LabelField(
                    "Required cargo moves: " + required,
                    EditorStyles.miniLabel
                );
                EditorGUILayout.LabelField("Current Limit: " + current, EditorStyles.miniLabel);
                EditorGUILayout.LabelField(
                    "Recommended: " + rec.recommended,
                    EditorStyles.miniLabel
                );
                EditorGUILayout.LabelField(
                    "→ " + SpecialObjectiveDifficultyRecommender.StrictnessLabel(strict),
                    EditorStyles.boldLabel
                );
                break;
            }

            case LevelObjectiveType.LimitedVehicle:
            {
                int required = hasScores ? scores.limitedRequiredMoves : 0;
                if (required <= 0)
                {
                    EditorGUILayout.LabelField(
                        "Run Analyze Suitability for required limited moves.",
                        EditorStyles.miniLabel
                    );
                    break;
                }

                var rec = SpecialObjectiveDifficultyRecommender.RecommendLimited(
                    required,
                    row.level,
                    row.plannedDifficulty,
                    settings
                );
                int current = row.plannedLimitedLimit > 0
                    ? row.plannedLimitedLimit
                    : row.level.limitedVehicleMoveLimit;
                var strict = SpecialObjectiveDifficultyRecommender.EvaluateStrictnessInt(
                    current,
                    rec.recommended,
                    higherIsEasier: true
                );
                EditorGUILayout.LabelField(
                    "Required limited moves: " + required,
                    EditorStyles.miniLabel
                );
                EditorGUILayout.LabelField("Current Limit: " + current, EditorStyles.miniLabel);
                EditorGUILayout.LabelField(
                    "Recommended: " + rec.recommended,
                    EditorStyles.miniLabel
                );
                EditorGUILayout.LabelField(
                    "→ " + SpecialObjectiveDifficultyRecommender.StrictnessLabel(strict),
                    EditorStyles.boldLabel
                );
                break;
            }

            case LevelObjectiveType.NoTouchChallenge:
                EditorGUILayout.LabelField(
                    "No numeric limit. Harder tiers: protected vehicle with " +
                    "stronger temptation but solvable without moving it.",
                    EditorStyles.wordWrappedMiniLabel
                );
                if (hasScores && scores.noTouchVehicleIndex >= 0)
                {
                    EditorGUILayout.LabelField(
                        "Analyzer pick: " + scores.noTouchLabel +
                        " (score " + scores.noTouchScore + ")",
                        EditorStyles.miniLabel
                    );
                }

                break;

            case LevelObjectiveType.MultiTargetRescue:
                EditorGUILayout.LabelField(
                    "No numeric limit. Progression via minMoves / blockers / " +
                    "target involvement (analyzer).",
                    EditorStyles.wordWrappedMiniLabel
                );
                if (hasScores)
                {
                    EditorGUILayout.LabelField(
                        "Pair: " + scores.multiPrimaryLabel + " + " +
                        scores.multiCandidateLabel +
                        " (score " + scores.multiTargetScore + ")",
                        EditorStyles.miniLabel
                    );
                }

                break;
        }

        EditorGUILayout.Space(6f);
    }

    /// <summary>
    /// First selected row in view order, or null.
    /// </summary>
    private RowState GetSelectedRowForPanel()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].selected && rows[i].level != null)
            {
                return rows[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Human-readable description of what APPLY will configure for this row.
    /// </summary>
    private string BuildAutoSetupPreview(RowState row)
    {
        if (row == null || row.level == null)
        {
            return "(no level)";
        }

        SuitabilityScores scores;
        bool hasScores = suitabilityByDbIndex.TryGetValue(row.dbIndex, out scores) &&
            scores.hasData;

        switch (row.plannedObjective)
        {
            case LevelObjectiveType.NoTouchChallenge:
                if (!hasScores || scores.noTouchVehicleIndex < 0)
                {
                    return "NoTouch: Protected = ANALYZE REQUIRED";
                }

                return "NoTouch: Protected = " +
                    DescribeRecommendation(scores.noTouchLabel, scores.noTouchVehicleIndex);

            case LevelObjectiveType.FragileCargo:
                if (!hasScores || scores.fragileVehicleIndex < 0)
                {
                    return "Fragile: Cargo = ANALYZE REQUIRED";
                }

                return "Fragile: Cargo = " +
                    DescribeRecommendation(scores.fragileLabel, scores.fragileVehicleIndex) +
                    "\nLimit = " + ResolveFragileLimit(row, scores);

            case LevelObjectiveType.LimitedVehicle:
                if (!hasScores || scores.limitedVehicleIndex < 0)
                {
                    return "Limited: Limited = ANALYZE REQUIRED";
                }

                return "Limited: Limited = " +
                    DescribeRecommendation(scores.limitedLabel, scores.limitedVehicleIndex) +
                    "\nLimit = " + ResolveLimitedLimit(row, scores);

            case LevelObjectiveType.MultiTargetRescue:
                if (!hasScores ||
                    scores.multiPrimaryIndex < 0 ||
                    scores.multiCandidateIndex < 0)
                {
                    return "MultiTarget: Targets = ANALYZE REQUIRED";
                }

                return "MultiTarget: Targets = " +
                    DescribeRecommendation(scores.multiPrimaryLabel, scores.multiPrimaryIndex) +
                    " + " +
                    DescribeRecommendation(
                        scores.multiCandidateLabel,
                        scores.multiCandidateIndex
                    );

            case LevelObjectiveType.TimedAmbulance:
                return "Timed: time limit = " +
                    row.plannedTimeLimit.ToString("0.#") + " sec" +
                    (row.plannedTimeLimit <= 0f ? " (invalid)" : string.Empty);

            case LevelObjectiveType.MoveLimit:
                return "MoveLimit: move limit = " + row.plannedMoveLimit +
                    (row.plannedMoveLimit <= 0 ? " (invalid)" : string.Empty);

            default:
                return "Classic: clear special vehicle flags and special limits.";
        }
    }

    private static string DescribeRecommendation(string label, int vehicleIndex)
    {
        if (vehicleIndex < 0)
        {
            return "ANALYZE REQUIRED";
        }

        string name = string.IsNullOrEmpty(label) ? "vehicle" : label;
        return name + " [" + vehicleIndex + "]";
    }

    private static int ResolveFragileLimit(RowState row, SuitabilityScores scores)
    {
        if (row.plannedFragileLimit > 0)
        {
            return row.plannedFragileLimit;
        }

        return scores.recommendedCargoLimit;
    }

    private static int ResolveLimitedLimit(RowState row, SuitabilityScores scores)
    {
        if (row.plannedLimitedLimit > 0)
        {
            return row.plannedLimitedLimit;
        }

        return scores.recommendedLimitedLimit;
    }

    private string BuildDisplayNumberLabel(RowState row)
    {
        if (database == null ||
            row.dbIndex < 0 ||
            curationViewFilter != CurationViewFilter.MainDatabase)
        {
            return "-";
        }

        int display = LevelDifficultyOrder.GetDifficultyDisplayNumber(database, row.dbIndex);
        return display > 0 ? display.ToString() : "-";
    }

    private void DrawGridDistributionReport()
    {
        EditorGUILayout.LabelField("GRID DISTRIBUTION (planned)", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "            Easy Med Hard Tot",
            EditorStyles.miniLabel
        );

        int[,] counts = BuildPlannedGridCounts();

        for (int i = 0; i < V1ReleaseContentPlan.ExactGrids.Length; i++)
        {
            Vector2Int grid = V1ReleaseContentPlan.ExactGrids[i];
            int easyC = counts[i, 0];
            int medC = counts[i, 1];
            int hardC = counts[i, 2];
            int total = easyC + medC + hardC;

            int tEasy = V1ReleaseContentPlan.GetTarget(LevelDifficulty.Easy, grid.x, grid.y);
            int tMed = V1ReleaseContentPlan.GetTarget(LevelDifficulty.Medium, grid.x, grid.y);
            int tHard = V1ReleaseContentPlan.GetTarget(LevelDifficulty.Hard, grid.x, grid.y);

            string line =
                grid.x + "x" + grid.y + ": " +
                easyC + "/" + tEasy + "  " +
                medC + "/" + tMed + "  " +
                hardC + "/" + tHard + "  (" + total + ")";
            EditorGUILayout.LabelField(line, EditorStyles.miniLabel);
        }

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Grouped pairs vs target", EditorStyles.boldLabel);
        DrawGroupedPairLine("5x6 / 6x5", new Vector2Int(5, 6), new Vector2Int(6, 5), counts);
        DrawGroupedPairLine("6x7 / 7x6", new Vector2Int(6, 7), new Vector2Int(7, 6), counts);
        DrawGroupedPairLine("7x8 / 8x7", new Vector2Int(7, 8), new Vector2Int(8, 7), counts);
    }

    private void DrawGroupedPairLine(
        string label,
        Vector2Int a,
        Vector2Int b,
        int[,] counts)
    {
        int ia = IndexOfExactGrid(a);
        int ib = IndexOfExactGrid(b);
        int easy = SafeGridCount(counts, ia, 0) + SafeGridCount(counts, ib, 0);
        int med = SafeGridCount(counts, ia, 1) + SafeGridCount(counts, ib, 1);
        int hard = SafeGridCount(counts, ia, 2) + SafeGridCount(counts, ib, 2);

        EditorGUILayout.LabelField(
            label + ": E " + easy + "/" +
            V1ReleaseContentPlan.GetGroupedTarget(label, LevelDifficulty.Easy) +
            "  M " + med + "/" +
            V1ReleaseContentPlan.GetGroupedTarget(label, LevelDifficulty.Medium) +
            "  H " + hard + "/" +
            V1ReleaseContentPlan.GetGroupedTarget(label, LevelDifficulty.Hard),
            EditorStyles.miniLabel
        );
    }

    private static int SafeGridCount(int[,] counts, int gridIndex, int difficultyIndex)
    {
        if (gridIndex < 0)
        {
            return 0;
        }

        return counts[gridIndex, difficultyIndex];
    }

    private static int IndexOfExactGrid(Vector2Int grid)
    {
        for (int i = 0; i < V1ReleaseContentPlan.ExactGrids.Length; i++)
        {
            if (V1ReleaseContentPlan.ExactGrids[i] == grid)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// [gridIndex, difficulty 0..2] planned counts for ExactGrids.
    /// </summary>
    private int[,] BuildPlannedGridCounts()
    {
        int[,] counts = new int[V1ReleaseContentPlan.ExactGrids.Length, 3];
        for (int i = 0; i < rows.Count; i++)
        {
            RowState row = rows[i];
            if (row.level == null)
            {
                continue;
            }

            int gi = IndexOfExactGrid(
                new Vector2Int(row.level.ResolvedGridWidth, row.level.ResolvedGridHeight)
            );
            if (gi < 0)
            {
                continue;
            }

            int d = (int)row.plannedDifficulty;
            if (d < 0 || d > 2)
            {
                continue;
            }

            counts[gi, d]++;
        }

        return counts;
    }

    private List<string> BuildV1ContentWarnings(int easy, int medium, int hard)
    {
        List<string> warnings = new List<string>();

        if (easy != V1ReleaseContentPlan.TargetEasy)
        {
            warnings.Add(
                "V1 Easy planned count " + easy +
                " (target " + V1ReleaseContentPlan.TargetEasy + ")."
            );
        }

        if (medium != V1ReleaseContentPlan.TargetMedium)
        {
            warnings.Add(
                "V1 Medium planned count " + medium +
                " (target " + V1ReleaseContentPlan.TargetMedium + ")."
            );
        }

        if (hard != V1ReleaseContentPlan.TargetHard)
        {
            warnings.Add(
                "V1 Hard planned count " + hard +
                " (target " + V1ReleaseContentPlan.TargetHard + ")."
            );
        }

        for (int i = 0; i < rows.Count; i++)
        {
            RowState row = rows[i];
            if (row.level == null)
            {
                continue;
            }

            int w = row.level.ResolvedGridWidth;
            int h = row.level.ResolvedGridHeight;
            if (!V1ReleaseContentPlan.IsAllowedDimension(w, h))
            {
                warnings.Add(
                    "DB#" + row.dbIndex + " " + row.level.name +
                    " grid " + w + "x" + h + " outside V1 rules."
                );
            }
        }

        // Soft distribution drift warnings (not blocking).
        int[,] counts = BuildPlannedGridCounts();
        for (int i = 0; i < V1ReleaseContentPlan.ExactGrids.Length; i++)
        {
            Vector2Int grid = V1ReleaseContentPlan.ExactGrids[i];
            for (int d = 0; d < 3; d++)
            {
                LevelDifficulty difficulty = (LevelDifficulty)d;
                int current = counts[i, d];
                int target = V1ReleaseContentPlan.GetTarget(difficulty, grid.x, grid.y);
                if (target == 0 && current == 0)
                {
                    continue;
                }

                if (current != target)
                {
                    warnings.Add(
                        grid.x + "x" + grid.y + " " + difficulty +
                        ": " + current + " / target " + target
                    );
                }
            }
        }

        // Cap spam: if too many, keep first few + summary.
        const int maxShown = 12;
        if (warnings.Count > maxShown)
        {
            int omitted = warnings.Count - maxShown;
            warnings = warnings.GetRange(0, maxShown);
            warnings.Add("… +" + omitted + " more V1 distribution warnings.");
        }

        return warnings;
    }

    private void DrawMatrixBlock(string title, int[,] matrix, int difficultyIndex)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "  Classic " + matrix[difficultyIndex, 0] +
            " | Timed " + matrix[difficultyIndex, 1] +
            " | Move " + matrix[difficultyIndex, 2]
        );
        EditorGUILayout.LabelField(
            "  MT " + matrix[difficultyIndex, 3] +
            " | NT " + matrix[difficultyIndex, 4] +
            " | FC " + matrix[difficultyIndex, 5] +
            " | LV " + matrix[difficultyIndex, 6]
        );
    }

    private void DrawFooter()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(
            "Rows: " + rows.Count +
            " | Visible: " + BuildVisibleSortedRows().Count +
            " | Dirty: " + CountDirtyRows() +
            " | Suitability cache: " +
            (suitabilityByDbIndex.Count > 0 ? "ready" : "not analyzed"),
            EditorStyles.miniLabel
        );
        EditorGUILayout.EndHorizontal();
    }

    // -------------------------------------------------------------------------
    // Data load / filters
    // -------------------------------------------------------------------------

    private void ReloadFromDatabase()
    {
        database = LoadMainLevelDatabase();
        progressionConfig = LoadProgressionConfig();
        rows.Clear();
        suitabilityByDbIndex.Clear();
        RebuildGridSizeFilterOptions();

        if (database == null || database.levels == null)
        {
            return;
        }

        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level == null)
            {
                continue;
            }

            rows.Add(new RowState
            {
                dbIndex = i,
                level = level,
                plannedDifficulty = level.difficulty,
                plannedObjective = level.objectiveType,
                plannedTimeLimit = level.timeLimitSeconds,
                plannedMoveLimit = level.moveLimit,
                plannedFragileLimit = level.fragileCargoMoveLimit,
                plannedLimitedLimit = level.limitedVehicleMoveLimit,
                selected = false
            });
        }

        RebuildGridSizeFilterOptions();
        Repaint();
    }

    private void ReloadFromCuration()
    {
        if (curationState == null)
        {
            curationState = V1CurationState.LoadOrCreate();
        }

        // Keep database ref for apply/unlock APIs.
        if (database == null)
        {
            database = LoadMainLevelDatabase();
        }

        progressionConfig = LoadProgressionConfig();
        rows.Clear();
        suitabilityByDbIndex.Clear();

        for (int i = 0; i < curationState.entries.Count; i++)
        {
            V1CurationEntry entry = curationState.entries[i];
            if (entry == null || entry.level == null)
            {
                continue;
            }

            bool include = false;
            switch (curationViewFilter)
            {
                case CurationViewFilter.Final:
                    include = V1CurationState.IsFinalStatus(entry.status);
                    break;
                case CurationViewFilter.Reserves:
                    include = entry.status == V1CurationStatus.Reserve;
                    break;
                case CurationViewFilter.Rejected:
                    include = entry.status == V1CurationStatus.Rejected;
                    break;
                case CurationViewFilter.NeedsReview:
                    include = entry.needsReview &&
                              V1CurationState.IsFinalStatus(entry.status);
                    break;
                case CurationViewFilter.ManualKeep:
                    include = entry.status == V1CurationStatus.ManualKeep;
                    break;
                case CurationViewFilter.Maybe:
                    include = entry.status == V1CurationStatus.Maybe;
                    break;
                case CurationViewFilter.Unreviewed:
                    include = V1CurationState.IsUnreviewedFinal(entry.status);
                    break;
                case CurationViewFilter.AllCandidates:
                    include = true;
                    break;
                default:
                    include = false;
                    break;
            }

            if (!include)
            {
                continue;
            }

            rows.Add(new RowState
            {
                dbIndex = i,
                level = entry.level,
                plannedDifficulty = entry.assignedDifficulty,
                plannedObjective = entry.level.objectiveType,
                plannedTimeLimit = entry.level.timeLimitSeconds,
                plannedMoveLimit = entry.level.moveLimit,
                plannedFragileLimit = entry.level.fragileCargoMoveLimit,
                plannedLimitedLimit = entry.level.limitedVehicleMoveLimit,
                selected = false,
                curationStatus = entry.status,
                needsReview = entry.needsReview,
                reviewReason = entry.reviewReason ?? string.Empty,
                effectiveScore = entry.effectiveScore
            });
        }

        RebuildGridSizeFilterOptions();
        Repaint();
    }

    private void RejectSelectedRows()
    {
        int count = 0;
        for (int i = 0; i < rows.Count; i++)
        {
            if (!rows[i].selected || rows[i].level == null)
            {
                continue;
            }

            if (V1AutoCurator.RejectLevel(rows[i].level))
            {
                count++;
            }
        }

        curationState = V1CurationState.LoadOrCreate();
        ReloadFromCuration();
        Debug.Log("Rejected " + count + " selected level(s).");
    }

    private void ManualKeepSelectedRows()
    {
        int count = 0;
        for (int i = 0; i < rows.Count; i++)
        {
            if (!rows[i].selected || rows[i].level == null)
            {
                continue;
            }

            if (V1AutoCurator.MarkManualKeep(rows[i].level))
            {
                count++;
            }
        }

        curationState = V1CurationState.LoadOrCreate();
        ReloadFromCuration();
        Debug.Log("ManualKeep " + count + " selected level(s).");
    }

    private void RebuildGridSizeFilterOptions()
    {
        gridSizeLabels.Clear();
        gridSizeValues.Clear();
        gridSizeLabels.Add("All");

        HashSet<string> seen = new HashSet<string>();
        for (int i = 0; i < rows.Count; i++)
        {
            LevelData level = rows[i].level;
            if (level == null)
            {
                continue;
            }

            string key = level.ResolvedGridWidth + "x" + level.ResolvedGridHeight;
            if (seen.Add(key))
            {
                gridSizeLabels.Add(key);
                gridSizeValues.Add(
                    new Vector2Int(level.ResolvedGridWidth, level.ResolvedGridHeight)
                );
            }
        }

        gridSizeFilterIndex = Mathf.Clamp(gridSizeFilterIndex, 0, gridSizeLabels.Count - 1);
    }

    private List<RowState> BuildVisibleSortedRows()
    {
        List<RowState> visible = new List<RowState>();
        for (int i = 0; i < rows.Count; i++)
        {
            if (PassesFilters(rows[i]))
            {
                visible.Add(rows[i]);
            }
        }

        visible.Sort(CompareRows);
        return visible;
    }

    private bool PassesFilters(RowState row)
    {
        LevelData level = row.level;
        if (level == null)
        {
            return false;
        }

        if (difficultyFilter != DifficultyFilter.All)
        {
            LevelDifficulty wanted = (LevelDifficulty)((int)difficultyFilter - 1);
            if (row.plannedDifficulty != wanted)
            {
                return false;
            }
        }

        if (objectiveFilter != ObjectiveFilter.All)
        {
            LevelObjectiveType wanted = (LevelObjectiveType)((int)objectiveFilter - 1);
            if (row.plannedObjective != wanted)
            {
                return false;
            }
        }

        if (gridSizeFilterIndex > 0 && gridSizeFilterIndex <= gridSizeValues.Count)
        {
            Vector2Int size = gridSizeValues[gridSizeFilterIndex - 1];
            if (level.ResolvedGridWidth != size.x || level.ResolvedGridHeight != size.y)
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            string q = searchText.Trim();
            bool nameMatch = level.name != null &&
                level.name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
            bool numberMatch = level.levelNumber.ToString().IndexOf(
                    q,
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;
            bool indexMatch = row.dbIndex.ToString() == q;
            if (!nameMatch && !numberMatch && !indexMatch)
            {
                return false;
            }
        }

        if (playabilityFilter != PlayabilityFilter.All)
        {
            if (!playabilityByDbIndex.TryGetValue(row.dbIndex, out ProductionLevelPlayability.Report report))
            {
                return false;
            }

            switch (playabilityFilter)
            {
                case PlayabilityFilter.Valid:
                    if (report.status != ProductionLevelPlayability.Status.Valid)
                    {
                        return false;
                    }

                    break;
                case PlayabilityFilter.Invalid:
                    if (report.status == ProductionLevelPlayability.Status.Valid)
                    {
                        return false;
                    }

                    break;
                case PlayabilityFilter.Timeout:
                    if (report.status != ProductionLevelPlayability.Status.SolverTimeout)
                    {
                        return false;
                    }

                    break;
                case PlayabilityFilter.Unsolvable:
                    if (report.status != ProductionLevelPlayability.Status.Unsolvable)
                    {
                        return false;
                    }

                    break;
                case PlayabilityFilter.ObjectiveInvalid:
                    if (report.status != ProductionLevelPlayability.Status.ObjectiveInvalid)
                    {
                        return false;
                    }

                    break;
                case PlayabilityFilter.ConfigInvalid:
                    if (report.status != ProductionLevelPlayability.Status.ConfigInvalid)
                    {
                        return false;
                    }

                    break;
            }
        }

        return true;
    }

    private int CompareRows(RowState a, RowState b)
    {
        int cmp = 0;
        LevelData la = a.level;
        LevelData lb = b.level;

        switch (sortMode)
        {
            case SortMode.MinimumMoves:
                cmp = (la != null ? la.minimumMoves : 0).CompareTo(
                    lb != null ? lb.minimumMoves : 0
                );
                break;
            case SortMode.DifficultyScore:
                cmp = (la != null ? la.difficultyScore : 0).CompareTo(
                    lb != null ? lb.difficultyScore : 0
                );
                break;
            case SortMode.GridArea:
                int areaA = la != null
                    ? la.ResolvedGridWidth * la.ResolvedGridHeight
                    : 0;
                int areaB = lb != null
                    ? lb.ResolvedGridWidth * lb.ResolvedGridHeight
                    : 0;
                cmp = areaA.CompareTo(areaB);
                break;
            case SortMode.Difficulty:
                cmp = a.plannedDifficulty.CompareTo(b.plannedDifficulty);
                if (cmp == 0)
                {
                    cmp = LevelDifficultyOrder.CompareLevels(la, lb, a.dbIndex, b.dbIndex);
                }

                break;
            case SortMode.Objective:
                cmp = a.plannedObjective.CompareTo(b.plannedObjective);
                break;
            case SortMode.DbIndex:
            default:
                cmp = a.dbIndex.CompareTo(b.dbIndex);
                break;
        }

        if (cmp == 0)
        {
            cmp = a.dbIndex.CompareTo(b.dbIndex);
        }

        return sortAscending ? cmp : -cmp;
    }

    // -------------------------------------------------------------------------
    // Suitability
    // -------------------------------------------------------------------------

    private void RunSuitabilityAnalyze()
    {
        suitabilityByDbIndex.Clear();

        List<SpecialMissionSuitabilityAnalyzer.Result> specialResults =
            SpecialMissionSuitabilityAnalyzer.AnalyzeMainLevelDatabase();
        List<MultiTargetSuitabilityAnalyzer.Result> multiResults =
            MultiTargetSuitabilityAnalyzer.AnalyzeMainLevelDatabase();

        Dictionary<LevelData, int> indexByLevel = new Dictionary<LevelData, int>();
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].level != null && !indexByLevel.ContainsKey(rows[i].level))
            {
                indexByLevel.Add(rows[i].level, rows[i].dbIndex);
            }
        }

        for (int i = 0; i < specialResults.Count; i++)
        {
            SpecialMissionSuitabilityAnalyzer.Result r = specialResults[i];
            if (r.level == null || !indexByLevel.TryGetValue(r.level, out int dbIndex))
            {
                continue;
            }

            SuitabilityScores scores;
            if (!suitabilityByDbIndex.TryGetValue(dbIndex, out scores))
            {
                scores = SuitabilityScores.CreateEmpty();
            }

            scores.hasData = true;
            switch (r.missionKind)
            {
                case SpecialMissionSuitabilityAnalyzer.MissionKind.NoTouch:
                    scores.noTouchScore = r.score;
                    scores.noTouchCategory = r.category;
                    scores.noTouchVehicleIndex = r.vehicleIndex;
                    scores.noTouchLabel = r.vehicleName;
                    break;
                case SpecialMissionSuitabilityAnalyzer.MissionKind.FragileCargo:
                    scores.fragileScore = r.score;
                    scores.fragileCategory = r.category;
                    scores.fragileVehicleIndex = r.vehicleIndex;
                    scores.fragileLabel = r.vehicleName;
                    scores.fragileRequiredMoves = r.targetMovesInSolution;
                    scores.recommendedCargoLimit = r.recommendedCargoLimit;
                    break;
                case SpecialMissionSuitabilityAnalyzer.MissionKind.LimitedVehicle:
                    scores.limitedScore = r.score;
                    scores.limitedCategory = r.category;
                    scores.limitedVehicleIndex = r.vehicleIndex;
                    scores.limitedLabel = r.vehicleName;
                    scores.limitedRequiredMoves = r.vehicleMovesInSolution;
                    scores.recommendedLimitedLimit = r.recommendedLimitedLimit;
                    break;
            }

            suitabilityByDbIndex[dbIndex] = scores;
        }

        for (int i = 0; i < multiResults.Count; i++)
        {
            MultiTargetSuitabilityAnalyzer.Result r = multiResults[i];
            if (r.level == null || !indexByLevel.TryGetValue(r.level, out int dbIndex))
            {
                continue;
            }

            SuitabilityScores scores;
            if (!suitabilityByDbIndex.TryGetValue(dbIndex, out scores))
            {
                scores = SuitabilityScores.CreateEmpty();
            }

            scores.hasData = true;
            scores.multiTargetScore = r.score;
            scores.multiTargetCategory = r.category;
            scores.multiPrimaryIndex = r.primaryTargetIndex;
            scores.multiCandidateIndex = r.bestCandidateIndex;
            scores.multiPrimaryLabel = r.primaryTargetName;
            scores.multiCandidateLabel = r.bestCandidateName;
            suitabilityByDbIndex[dbIndex] = scores;
        }

        Debug.Log(
            "Level Content Planner: suitability cached for " +
            suitabilityByDbIndex.Count + " levels."
        );
        Repaint();
    }

    private void RunPlayabilityValidation()
    {
        playabilityByDbIndex.Clear();
        deepTimeoutByDbIndex.Clear();
        hintPathPassByDbIndex.Clear();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== VALIDATE ALL PLAYABLE LEVELS ===");
        sb.AppendLine(
            "Normal budget states=" + ProductionLevelPlayability.DefaultMaxStates +
            " — TIMEOUT ≠ VALID/UNSOLVABLE."
        );
        int[] counts = new int[6];

        for (int i = 0; i < rows.Count; i++)
        {
            RowState row = rows[i];
            if (row.level == null)
            {
                continue;
            }

            ProductionLevelPlayability.Report report =
                ProductionLevelPlayability.Validate(row.level, row.dbIndex);
            playabilityByDbIndex[row.dbIndex] = report;
            counts[(int)report.status]++;
            if (report.status != ProductionLevelPlayability.Status.Valid)
            {
                sb.AppendLine(ProductionLevelPlayability.FormatReportLine(report));
            }
        }

        sb.AppendLine();
        sb.AppendLine(
            "Valid=" + counts[(int)ProductionLevelPlayability.Status.Valid] +
            " Unsolvable=" + counts[(int)ProductionLevelPlayability.Status.Unsolvable] +
            " ObjectiveInvalid=" +
            counts[(int)ProductionLevelPlayability.Status.ObjectiveInvalid] +
            " Timeout=" + counts[(int)ProductionLevelPlayability.Status.SolverTimeout] +
            " ConfigInvalid=" +
            counts[(int)ProductionLevelPlayability.Status.ConfigInvalid]
        );
        sb.AppendLine(
            "Use DEEP VALIDATE TIMEOUT LEVELS for the " +
            counts[(int)ProductionLevelPlayability.Status.SolverTimeout] +
            " timeout(s). No LevelData auto-modified."
        );
        Debug.Log(sb.ToString());
        Repaint();
    }

    private void RunDeepValidateTimeouts()
    {
        if (database == null)
        {
            EditorUtility.DisplayDialog(
                "Deep Validate Timeout Levels",
                "No MainLevelDatabase loaded.",
                "OK"
            );
            return;
        }

        List<ProductionLevelPlayability.Report> priors =
            new List<ProductionLevelPlayability.Report>();
        foreach (KeyValuePair<int, ProductionLevelPlayability.Report> pair in
                 playabilityByDbIndex)
        {
            if (pair.Value.status == ProductionLevelPlayability.Status.SolverTimeout ||
                pair.Value.status == ProductionLevelPlayability.Status.Unknown)
            {
                priors.Add(pair.Value);
            }
        }

        if (priors.Count == 0)
        {
            bool scan = EditorUtility.DisplayDialog(
                "Deep Validate Timeout Levels",
                "No TIMEOUT results cached.\n\n" +
                "Run a normal scan first to find timeouts, then deep-validate only those?",
                "Scan + Deep Validate",
                "Cancel"
            );
            if (!scan)
            {
                return;
            }

            RunPlayabilityValidation();
            foreach (KeyValuePair<int, ProductionLevelPlayability.Report> pair in
                     playabilityByDbIndex)
            {
                if (pair.Value.status == ProductionLevelPlayability.Status.SolverTimeout ||
                    pair.Value.status == ProductionLevelPlayability.Status.Unknown)
                {
                    priors.Add(pair.Value);
                }
            }

            if (priors.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Deep Validate Timeout Levels",
                    "No TIMEOUT levels under normal budget.",
                    "OK"
                );
                return;
            }
        }

        bool proceed = EditorUtility.DisplayDialog(
            "Deep Validate Timeout Levels",
            "Deep-validate " + priors.Count + " TIMEOUT level(s)\n" +
            "with " + ProductionLevelPlayability.DeepMaxStates + " states " +
            "(normal " + ProductionLevelPlayability.DefaultMaxStates + " × " +
            ProductionLevelPlayability.DeepMaxStatesMultiplier + ").\n\n" +
            "No LevelData writes. Continue?",
            "Deep Validate",
            "Cancel"
        );
        if (!proceed)
        {
            return;
        }

        deepTimeoutByDbIndex.Clear();
        List<ProductionLevelPlayability.DeepTimeoutResult> results =
            ProductionLevelPlayability.DeepValidateTimeoutLevels(
                database,
                priors,
                ProductionLevelPlayability.DeepMaxStates
            );

        for (int i = 0; i < results.Count; i++)
        {
            ProductionLevelPlayability.DeepTimeoutResult entry = results[i];
            int dbIndex = entry.deepReport.dbIndex;
            deepTimeoutByDbIndex[dbIndex] = entry;
            playabilityByDbIndex[dbIndex] = entry.deepReport;
            if (entry.deepReport.status == ProductionLevelPlayability.Status.Valid)
            {
                hintPathPassByDbIndex[dbIndex] = entry.hintPathPass;
            }

            if (entry.releaseBlocker)
            {
                for (int r = 0; r < rows.Count; r++)
                {
                    if (rows[r].dbIndex != dbIndex)
                    {
                        continue;
                    }

                    rows[r].needsReview = true;
                    rows[r].reviewReason = entry.recommendedAction;
                    break;
                }
            }
        }

        string summary = ProductionLevelPlayability.BuildDeepValidationSummary(results);
        Debug.Log(summary);
        EditorUtility.DisplayDialog(
            "Deep Validate Timeout Levels",
            "Finished " + results.Count +
            " deep check(s).\nSee Console for Release Safe / Blockers.",
            "OK"
        );
        Repaint();
    }

    private void RunHintPathVerification()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== VERIFY HINT PATHS ===");
        int pass = 0;
        int fail = 0;
        for (int i = 0; i < rows.Count; i++)
        {
            RowState row = rows[i];
            if (row.level == null)
            {
                continue;
            }

            bool ok = ProductionLevelPlayability.VerifyHintPath(row.level, out string detail);
            if (ok)
            {
                pass++;
            }
            else
            {
                fail++;
                sb.AppendLine(
                    "FAIL DB#" + row.dbIndex + " " + row.level.name + " | " + detail
                );
            }
        }

        sb.AppendLine("Pass=" + pass + " Fail=" + fail);
        Debug.Log(sb.ToString());
    }

    private void AutoSuggestSpecials()
    {
        if (suitabilityByDbIndex.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Auto Suggest Specials",
                "Run Analyze Suitability first.",
                "OK"
            );
            return;
        }

        // Alleen Classic planned rows als kandidaten; top Good per soort.
        SuggestForObjective(
            LevelObjectiveType.NoTouchChallenge,
            s => s.noTouchScore,
            s => s.noTouchCategory
        );
        SuggestForObjective(
            LevelObjectiveType.FragileCargo,
            s => s.fragileScore,
            s => s.fragileCategory
        );
        SuggestForObjective(
            LevelObjectiveType.LimitedVehicle,
            s => s.limitedScore,
            s => s.limitedCategory
        );
        SuggestForObjective(
            LevelObjectiveType.MultiTargetRescue,
            s => s.multiTargetScore,
            s => ToSpecialCategory(s.multiTargetCategory)
        );

        Repaint();
    }

    private void SuggestForObjective(
        LevelObjectiveType objective,
        Func<SuitabilityScores, int> scoreSelector,
        Func<SuitabilityScores, SpecialMissionSuitabilityAnalyzer.Category> categorySelector)
    {
        const int maxSuggestions = 4;
        List<(RowState row, int score)> candidates = new List<(RowState, int)>();

        for (int i = 0; i < rows.Count; i++)
        {
            RowState row = rows[i];
            if (row.level == null)
            {
                continue;
            }

            if (row.plannedObjective != LevelObjectiveType.Classic)
            {
                continue;
            }

            if (!suitabilityByDbIndex.TryGetValue(row.dbIndex, out SuitabilityScores scores) ||
                !scores.hasData)
            {
                continue;
            }

            SpecialMissionSuitabilityAnalyzer.Category cat = categorySelector(scores);
            if (cat != SpecialMissionSuitabilityAnalyzer.Category.Good)
            {
                continue;
            }

            candidates.Add((row, scoreSelector(scores)));
        }

        candidates.Sort((a, b) => b.score.CompareTo(a.score));
        int applied = 0;
        for (int i = 0; i < candidates.Count && applied < maxSuggestions; i++)
        {
            candidates[i].row.plannedObjective = objective;
            InitializePlannedLimitsForObjective(candidates[i].row);
            applied++;
        }
    }

    private static SpecialMissionSuitabilityAnalyzer.Category ToSpecialCategory(
        MultiTargetSuitabilityAnalyzer.Category category)
    {
        switch (category)
        {
            case MultiTargetSuitabilityAnalyzer.Category.Good:
                return SpecialMissionSuitabilityAnalyzer.Category.Good;
            case MultiTargetSuitabilityAnalyzer.Category.Maybe:
                return SpecialMissionSuitabilityAnalyzer.Category.Maybe;
            case MultiTargetSuitabilityAnalyzer.Category.Poor:
                return SpecialMissionSuitabilityAnalyzer.Category.Poor;
            default:
                return SpecialMissionSuitabilityAnalyzer.Category.NotApplicable;
        }
    }

    // -------------------------------------------------------------------------
    // Bulk / apply / revert
    // -------------------------------------------------------------------------

    private void SetVisibleSelection(bool selected)
    {
        List<RowState> visible = BuildVisibleSortedRows();
        for (int i = 0; i < visible.Count; i++)
        {
            visible[i].selected = selected;
        }
    }

    private void ApplyBulkDifficulty()
    {
        LevelDifficulty difficulty = (LevelDifficulty)(int)bulkDifficulty;
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].selected)
            {
                rows[i].plannedDifficulty = difficulty;
            }
        }
    }

    private void ApplyBulkObjective()
    {
        LevelObjectiveType objective = (LevelObjectiveType)(int)bulkObjective;
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].selected)
            {
                rows[i].plannedObjective = objective;
                InitializePlannedLimitsForObjective(rows[i]);
            }
        }
    }

    /// <summary>
    /// Planned-difficulty only, from minimumMoves ranges. No asset writes.
    /// </summary>
    private void AutoClassifyDifficultyByMinMoves()
    {
        int easy = 0;
        int medium = 0;
        int hard = 0;
        int invalid = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            RowState row = rows[i];
            if (row.level == null)
            {
                continue;
            }

            LevelDifficulty classified;
            if (!LevelMinMovesDifficulty.TryGetDifficultyForMinimumMoves(
                    row.level.minimumMoves,
                    out classified))
            {
                invalid++;
                continue;
            }

            row.plannedDifficulty = classified;
            switch (classified)
            {
                case LevelDifficulty.Easy:
                    easy++;
                    break;
                case LevelDifficulty.Medium:
                    medium++;
                    break;
                default:
                    hard++;
                    break;
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Planned difficulty set from minimumMoves.");
        sb.AppendLine(LevelMinMovesDifficulty.RangesSummary);
        sb.AppendLine();
        sb.AppendLine("Easy: " + easy);
        sb.AppendLine("Medium: " + medium);
        sb.AppendLine("Hard: " + hard);
        sb.AppendLine("Invalid (minimumMoves <= 0, skipped): " + invalid);
        sb.AppendLine();
        sb.AppendLine("Planned state only — no asset writes until APPLY.");

        EditorUtility.DisplayDialog(
            "Auto Classify Difficulty By Min Moves",
            sb.ToString(),
            "OK"
        );
        Repaint();
    }

    /// <summary>
    /// Removes selected level references from MainLevelDatabase. Assets are not deleted.
    /// </summary>
    private void RemoveSelectedFromMainDatabase()
    {
        if (database == null || database.levels == null)
        {
            EditorUtility.DisplayDialog(
                "Remove From Main Database",
                "MainLevelDatabase not loaded.",
                "OK"
            );
            return;
        }

        List<LevelData> toRemove = new List<LevelData>();
        for (int i = 0; i < rows.Count; i++)
        {
            RowState row = rows[i];
            if (row.selected && row.level != null && !toRemove.Contains(row.level))
            {
                toRemove.Add(row.level);
            }
        }

        if (toRemove.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Remove From Main Database",
                "No levels selected.",
                "OK"
            );
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Remove " + toRemove.Count + " selected level(s) from MainLevelDatabase?");
        sb.AppendLine();
        sb.AppendLine("Removing levels changes database-index identities for entries after them.");
        sb.AppendLine("This is PRE-RELEASE ONLY and may invalidate local test progression.");
        sb.AppendLine();
        sb.AppendLine("Level assets are NOT deleted — only database references.");

        bool confirm = EditorUtility.DisplayDialog(
            "Remove Selected From Main Database",
            sb.ToString(),
            "Remove",
            "Cancel"
        );
        if (!confirm)
        {
            return;
        }

        Undo.RecordObject(database, "Remove Levels From Main Database");

        int removed = 0;
        for (int i = database.levels.Count - 1; i >= 0; i--)
        {
            LevelData level = database.levels[i];
            if (level == null || !toRemove.Contains(level))
            {
                continue;
            }

            database.levels.RemoveAt(i);
            removed++;
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        ReloadFromDatabase();

        Debug.LogWarning(
            "Level Content Planner: removed " + removed +
            " level reference(s) from MainLevelDatabase. Database-index identities shifted."
        );
        Repaint();
    }

    /// <summary>
    /// Seeds planned limits from difficulty-aware recommendations. No asset writes.
    /// Only fills empty/invalid planned fields.
    /// </summary>
    private void InitializePlannedLimitsForObjective(RowState row)
    {
        if (row == null || row.level == null)
        {
            return;
        }

        SpecialObjectiveDifficultyConfig.Settings settings =
            SpecialObjectiveDifficultyConfig.Settings.Load();
        float progress = SpecialObjectiveDifficultyRecommender.GetLocalProgress01(
            database,
            row.dbIndex,
            row.plannedDifficulty
        );

        SuitabilityScores scores;
        bool hasScores = suitabilityByDbIndex.TryGetValue(row.dbIndex, out scores) &&
            scores.hasData;

        switch (row.plannedObjective)
        {
            case LevelObjectiveType.TimedAmbulance:
                if (row.plannedTimeLimit <= 0f)
                {
                    row.plannedTimeLimit =
                        SpecialObjectiveDifficultyRecommender.RecommendTimed(
                            row.level,
                            row.plannedDifficulty,
                            progress,
                            settings
                        ).recommendedSeconds;
                }

                break;

            case LevelObjectiveType.MoveLimit:
                if (row.plannedMoveLimit <= 0)
                {
                    row.plannedMoveLimit =
                        SpecialObjectiveDifficultyRecommender.RecommendMoveLimit(
                            row.level,
                            row.plannedDifficulty,
                            progress,
                            settings
                        ).recommended;
                }

                break;

            case LevelObjectiveType.FragileCargo:
                if (row.plannedFragileLimit <= 0)
                {
                    int required = hasScores ? scores.fragileRequiredMoves : 0;
                    if (required <= 0 && hasScores && scores.recommendedCargoLimit > 0)
                    {
                        required = Mathf.Max(1, scores.recommendedCargoLimit - 1);
                    }

                    if (required > 0)
                    {
                        row.plannedFragileLimit =
                            SpecialObjectiveDifficultyRecommender.RecommendFragile(
                                required,
                                row.level,
                                row.plannedDifficulty,
                                settings
                            ).recommended;
                    }
                    else if (hasScores && scores.recommendedCargoLimit > 0)
                    {
                        row.plannedFragileLimit = scores.recommendedCargoLimit;
                    }
                }

                break;

            case LevelObjectiveType.LimitedVehicle:
                if (row.plannedLimitedLimit <= 0)
                {
                    int required = hasScores ? scores.limitedRequiredMoves : 0;
                    if (required <= 0 && hasScores && scores.recommendedLimitedLimit > 0)
                    {
                        required = scores.recommendedLimitedLimit;
                    }

                    if (required > 0)
                    {
                        row.plannedLimitedLimit =
                            SpecialObjectiveDifficultyRecommender.RecommendLimited(
                                required,
                                row.level,
                                row.plannedDifficulty,
                                settings
                            ).recommended;
                    }
                    else if (hasScores && scores.recommendedLimitedLimit > 0)
                    {
                        row.plannedLimitedLimit = scores.recommendedLimitedLimit;
                    }
                }

                break;
        }
    }

    /// <summary>
    /// Force-overwrite planned numeric special parameters with recommendations.
    /// Planned only — APPLY required to write assets.
    /// </summary>
    private void AutoTuneSpecialParameters()
    {
        SpecialObjectiveDifficultyConfig.Settings settings =
            SpecialObjectiveDifficultyConfig.Settings.Load();
        settings.Save();

        int tuned = 0;
        StringBuilder preview = new StringBuilder();
        preview.AppendLine("AUTO TUNE SPECIAL PARAMETERS (planned only)");
        preview.AppendLine();

        for (int i = 0; i < rows.Count; i++)
        {
            RowState row = rows[i];
            if (row.level == null)
            {
                continue;
            }

            if (row.plannedObjective == LevelObjectiveType.Classic)
            {
                continue;
            }

            float progress = SpecialObjectiveDifficultyRecommender.GetLocalProgress01(
                database,
                row.dbIndex,
                row.plannedDifficulty
            );
            SuitabilityScores scores;
            bool hasScores = suitabilityByDbIndex.TryGetValue(row.dbIndex, out scores) &&
                scores.hasData;

            string local = BuildDisplayNumberLabel(row);
            string header =
                row.plannedDifficulty + " #" + local +
                " [" + ShortObjective(row.plannedObjective) + "] " +
                row.level.name;

            switch (row.plannedObjective)
            {
                case LevelObjectiveType.TimedAmbulance:
                {
                    float before = row.plannedTimeLimit > 0f
                        ? row.plannedTimeLimit
                        : row.level.timeLimitSeconds;
                    float after = SpecialObjectiveDifficultyRecommender.RecommendTimed(
                        row.level,
                        row.plannedDifficulty,
                        progress,
                        settings
                    ).recommendedSeconds;
                    row.plannedTimeLimit = after;
                    preview.AppendLine(
                        header + "\n  Timed: " + before.ToString("0.#") +
                        "s → " + after.ToString("0.#") + "s"
                    );
                    tuned++;
                    break;
                }

                case LevelObjectiveType.MoveLimit:
                {
                    int before = row.plannedMoveLimit > 0
                        ? row.plannedMoveLimit
                        : row.level.moveLimit;
                    int after = SpecialObjectiveDifficultyRecommender.RecommendMoveLimit(
                        row.level,
                        row.plannedDifficulty,
                        progress,
                        settings
                    ).recommended;
                    row.plannedMoveLimit = after;
                    preview.AppendLine(
                        header + "\n  MoveLimit: " + before + " → " + after
                    );
                    tuned++;
                    break;
                }

                case LevelObjectiveType.FragileCargo:
                {
                    int required = hasScores ? scores.fragileRequiredMoves : 0;
                    if (required <= 0)
                    {
                        preview.AppendLine(
                            header + "\n  Fragile: skipped (Analyze Suitability first)"
                        );
                        break;
                    }

                    int before = row.plannedFragileLimit > 0
                        ? row.plannedFragileLimit
                        : row.level.fragileCargoMoveLimit;
                    int after = SpecialObjectiveDifficultyRecommender.RecommendFragile(
                        required,
                        row.level,
                        row.plannedDifficulty,
                        settings
                    ).recommended;
                    row.plannedFragileLimit = after;
                    preview.AppendLine(
                        header + "\n  Fragile: " + before + " → " + after +
                        " (required=" + required + ")"
                    );
                    tuned++;
                    break;
                }

                case LevelObjectiveType.LimitedVehicle:
                {
                    int required = hasScores ? scores.limitedRequiredMoves : 0;
                    if (required <= 0)
                    {
                        preview.AppendLine(
                            header + "\n  Limited: skipped (Analyze Suitability first)"
                        );
                        break;
                    }

                    int before = row.plannedLimitedLimit > 0
                        ? row.plannedLimitedLimit
                        : row.level.limitedVehicleMoveLimit;
                    int after = SpecialObjectiveDifficultyRecommender.RecommendLimited(
                        required,
                        row.level,
                        row.plannedDifficulty,
                        settings
                    ).recommended;
                    row.plannedLimitedLimit = after;
                    preview.AppendLine(
                        header + "\n  Limited: " + before + " → " + after +
                        " (required=" + required + ")"
                    );
                    tuned++;
                    break;
                }

                case LevelObjectiveType.NoTouchChallenge:
                    if (hasScores && scores.noTouchVehicleIndex >= 0)
                    {
                        preview.AppendLine(
                            header + "\n  NoTouch: keep analyzer pick → " +
                            scores.noTouchLabel + " (no numeric tune)"
                        );
                    }
                    else
                    {
                        preview.AppendLine(
                            header + "\n  NoTouch: Analyze Suitability for vehicle pick"
                        );
                    }

                    break;

                case LevelObjectiveType.MultiTargetRescue:
                    if (hasScores &&
                        scores.multiPrimaryIndex >= 0 &&
                        scores.multiCandidateIndex >= 0)
                    {
                        preview.AppendLine(
                            header + "\n  MultiTarget: " +
                            scores.multiPrimaryLabel + " + " +
                            scores.multiCandidateLabel + " (no numeric tune)"
                        );
                    }
                    else
                    {
                        preview.AppendLine(
                            header + "\n  MultiTarget: Analyze Suitability for pair"
                        );
                    }

                    break;
            }
        }

        preview.AppendLine();
        preview.AppendLine("Tuned numeric rows: " + tuned);
        preview.AppendLine("APPLY PLANNED ASSIGNMENTS to write LevelData.");
        Debug.Log(preview.ToString());
        EditorUtility.DisplayDialog(
            "AUTO TUNE SPECIAL PARAMETERS",
            "Updated planned values for " + tuned + " specials.\n\n" +
            "See Console for before → after preview.\n" +
            "Nothing written until APPLY.",
            "OK"
        );
        Repaint();
    }

    private void LogSpecialProgressionReport()
    {
        List<LevelData> levels = new List<LevelData>();
        Dictionary<LevelData, int> indexByLevel = new Dictionary<LevelData, int>();
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].level == null)
            {
                continue;
            }

            levels.Add(rows[i].level);
            if (!indexByLevel.ContainsKey(rows[i].level))
            {
                indexByLevel.Add(rows[i].level, rows[i].dbIndex);
            }
        }

        string report = SpecialObjectiveDifficultyRecommender.BuildProgressionReport(
            database,
            levels,
            level =>
            {
                int idx;
                return indexByLevel.TryGetValue(level, out idx) ? idx : -1;
            },
            dbIndex =>
            {
                SuitabilityScores scores;
                if (suitabilityByDbIndex.TryGetValue(dbIndex, out scores) && scores.hasData)
                {
                    return scores.fragileRequiredMoves;
                }

                return 0;
            },
            dbIndex =>
            {
                SuitabilityScores scores;
                if (suitabilityByDbIndex.TryGetValue(dbIndex, out scores) && scores.hasData)
                {
                    return scores.limitedRequiredMoves;
                }

                return 0;
            }
        );

        Debug.Log(report);
    }

    private void RevertPlannedChanges()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            RowState row = rows[i];
            if (row.level == null)
            {
                continue;
            }

            row.plannedDifficulty = row.level.difficulty;
            row.plannedObjective = row.level.objectiveType;
            row.plannedTimeLimit = row.level.timeLimitSeconds;
            row.plannedMoveLimit = row.level.moveLimit;
            row.plannedFragileLimit = row.level.fragileCargoMoveLimit;
            row.plannedLimitedLimit = row.level.limitedVehicleMoveLimit;
        }

        Repaint();
    }

    private void TryApplyPlannedAssignments()
    {
        List<RowState> dirty = new List<RowState>();
        for (int i = 0; i < rows.Count; i++)
        {
            if (IsRowDirty(rows[i]))
            {
                dirty.Add(rows[i]);
            }
        }

        if (dirty.Count == 0)
        {
            EditorUtility.DisplayDialog("Apply", "No planned changes.", "OK");
            return;
        }

        string summary = BuildApplyConfirmationSummary(dirty);
        bool confirm = EditorUtility.DisplayDialog(
            "Apply Level Content Plan",
            summary,
            "Apply",
            "Cancel"
        );
        if (!confirm)
        {
            return;
        }

        int applied = 0;
        int specialConfigured = 0;
        int specialSkipped = 0;

        for (int i = 0; i < dirty.Count; i++)
        {
            RowState row = dirty[i];
            LevelData level = row.level;
            if (level == null)
            {
                continue;
            }

            Undo.RecordObject(level, "Apply Level Content Plan");

            LevelObjectiveType oldObjective = level.objectiveType;
            level.difficulty = row.plannedDifficulty;
            level.objectiveType = row.plannedObjective;

            bool objectiveChanged = oldObjective != row.plannedObjective;
            if (objectiveChanged || AreSpecialLimitsDirty(row))
            {
                if (ApplySpecialConfiguration(row, level))
                {
                    specialConfigured++;
                }
                else
                {
                    specialSkipped++;
                }
            }

            EditorUtility.SetDirty(level);
            SyncPlannedLimitsFromLevel(row);
            applied++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            "Level Content Planner: applied assignments to " + applied + " levels" +
            " | special auto-config " + specialConfigured +
            " | special skipped " + specialSkipped + "."
        );
        Repaint();
    }

    /// <summary>
    /// Applies objective-specific vehicle flags / limits. False when a recommendation was
    /// missing and no vehicle flags were invented.
    /// </summary>
    private bool ApplySpecialConfiguration(RowState row, LevelData level)
    {
        SuitabilityScores scores;
        bool hasScores = suitabilityByDbIndex.TryGetValue(row.dbIndex, out scores) &&
            scores.hasData;
        string error;

        switch (row.plannedObjective)
        {
            case LevelObjectiveType.Classic:
                LevelSpecialConfigEditor.ApplyClassicCleanup(level);
                return true;

            case LevelObjectiveType.TimedAmbulance:
                LevelSpecialConfigEditor.ApplyTimedAmbulance(level, row.plannedTimeLimit);
                if (row.plannedTimeLimit > 0f)
                {
                    return true;
                }

                Debug.LogWarning(
                    "Level Content Planner: DB#" + row.dbIndex + " " + level.name +
                    " Timed objective applied without a positive time limit."
                );
                return false;

            case LevelObjectiveType.MoveLimit:
                LevelSpecialConfigEditor.ApplyMoveLimit(level, row.plannedMoveLimit);
                if (row.plannedMoveLimit > 0)
                {
                    return true;
                }

                Debug.LogWarning(
                    "Level Content Planner: DB#" + row.dbIndex + " " + level.name +
                    " MoveLimit objective applied without a positive move limit."
                );
                return false;

            case LevelObjectiveType.NoTouchChallenge:
                if (!hasScores || scores.noTouchVehicleIndex < 0)
                {
                    Debug.LogWarning(
                        "Level Content Planner: DB#" + row.dbIndex + " " + level.name +
                        " NoTouch needs Analyze Suitability — objective set, " +
                        "vehicle flags untouched."
                    );
                    return false;
                }

                if (!LevelSpecialConfigEditor.TryApplyNoTouch(
                        level,
                        scores.noTouchVehicleIndex,
                        out error))
                {
                    Debug.LogWarning(
                        "Level Content Planner: DB#" + row.dbIndex + " " + level.name +
                        " NoTouch auto-config failed (" + error + ")."
                    );
                    return false;
                }

                return true;

            case LevelObjectiveType.FragileCargo:
                if (!hasScores || scores.fragileVehicleIndex < 0)
                {
                    Debug.LogWarning(
                        "Level Content Planner: DB#" + row.dbIndex + " " + level.name +
                        " FragileCargo needs Analyze Suitability — objective set, " +
                        "vehicle flags untouched."
                    );
                    return false;
                }

                int fragileLimit = ResolveFragileLimit(row, scores);
                if (!LevelSpecialConfigEditor.TryApplyFragileCargo(
                        level,
                        scores.fragileVehicleIndex,
                        fragileLimit,
                        out error))
                {
                    Debug.LogWarning(
                        "Level Content Planner: DB#" + row.dbIndex + " " + level.name +
                        " FragileCargo auto-config failed (" + error + ")."
                    );
                    return false;
                }

                row.plannedFragileLimit = fragileLimit;
                return true;

            case LevelObjectiveType.LimitedVehicle:
                if (!hasScores || scores.limitedVehicleIndex < 0)
                {
                    Debug.LogWarning(
                        "Level Content Planner: DB#" + row.dbIndex + " " + level.name +
                        " LimitedVehicle needs Analyze Suitability — objective set, " +
                        "vehicle flags untouched."
                    );
                    return false;
                }

                int limitedLimit = ResolveLimitedLimit(row, scores);
                if (!LevelSpecialConfigEditor.TryApplyLimitedVehicle(
                        level,
                        scores.limitedVehicleIndex,
                        limitedLimit,
                        out error))
                {
                    Debug.LogWarning(
                        "Level Content Planner: DB#" + row.dbIndex + " " + level.name +
                        " LimitedVehicle auto-config failed (" + error + ")."
                    );
                    return false;
                }

                row.plannedLimitedLimit = limitedLimit;
                return true;

            case LevelObjectiveType.MultiTargetRescue:
                if (!hasScores ||
                    scores.multiPrimaryIndex < 0 ||
                    scores.multiCandidateIndex < 0)
                {
                    Debug.LogWarning(
                        "Level Content Planner: DB#" + row.dbIndex + " " + level.name +
                        " MultiTarget needs Analyze Suitability — objective set, " +
                        "targets untouched."
                    );
                    return false;
                }

                if (!LevelSpecialConfigEditor.TryApplyMultiTarget(
                        level,
                        scores.multiPrimaryIndex,
                        scores.multiCandidateIndex,
                        out error))
                {
                    Debug.LogWarning(
                        "Level Content Planner: DB#" + row.dbIndex + " " + level.name +
                        " MultiTarget auto-config failed (" + error + ")."
                    );
                    return false;
                }

                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Special config helpers clear limits they do not own; mirror the asset back into
    /// planned state so applied rows stop reporting as dirty.
    /// </summary>
    private static void SyncPlannedLimitsFromLevel(RowState row)
    {
        if (row == null || row.level == null)
        {
            return;
        }

        row.plannedTimeLimit = row.level.timeLimitSeconds;
        row.plannedMoveLimit = row.level.moveLimit;
        row.plannedFragileLimit = row.level.fragileCargoMoveLimit;
        row.plannedLimitedLimit = row.level.limitedVehicleMoveLimit;
    }

    private static bool AreSpecialLimitsDirty(RowState row)
    {
        if (row == null || row.level == null)
        {
            return false;
        }

        return !Mathf.Approximately(row.plannedTimeLimit, row.level.timeLimitSeconds)
            || row.plannedMoveLimit != row.level.moveLimit
            || row.plannedFragileLimit != row.level.fragileCargoMoveLimit
            || row.plannedLimitedLimit != row.level.limitedVehicleMoveLimit;
    }

    private string BuildApplyConfirmationSummary(List<RowState> dirty)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Apply changes to " + dirty.Count + " levels?");
        sb.AppendLine();
        sb.AppendLine("Difficulty changes:");

        Dictionary<string, int> diffChanges = new Dictionary<string, int>();
        Dictionary<string, int> objChanges = new Dictionary<string, int>();
        int autoConfigReady = 0;
        int autoConfigBlocked = 0;
        int limitOnlyChanges = 0;

        for (int i = 0; i < dirty.Count; i++)
        {
            RowState row = dirty[i];
            if (row.level == null)
            {
                continue;
            }

            if (RequiresVehicleAutoConfiguration(row.plannedObjective))
            {
                if (HasVehicleRecommendation(row))
                {
                    autoConfigReady++;
                }
                else
                {
                    autoConfigBlocked++;
                }
            }
            else if (row.plannedObjective == row.level.objectiveType &&
                     AreSpecialLimitsDirty(row))
            {
                limitOnlyChanges++;
            }

            if (row.plannedDifficulty != row.level.difficulty)
            {
                string key = row.level.difficulty + " → " + row.plannedDifficulty;
                diffChanges.TryGetValue(key, out int count);
                diffChanges[key] = count + 1;
            }

            if (row.plannedObjective != row.level.objectiveType)
            {
                string key = ShortObjective(row.level.objectiveType) +
                    " → " +
                    ShortObjective(row.plannedObjective);
                objChanges.TryGetValue(key, out int count);
                objChanges[key] = count + 1;
            }
        }

        if (diffChanges.Count == 0)
        {
            sb.AppendLine("  (none)");
        }
        else
        {
            foreach (KeyValuePair<string, int> pair in diffChanges)
            {
                sb.AppendLine("  " + pair.Key + ": " + pair.Value);
            }
        }

        sb.AppendLine();
        sb.AppendLine("Objective changes:");
        if (objChanges.Count == 0)
        {
            sb.AppendLine("  (none)");
        }
        else
        {
            foreach (KeyValuePair<string, int> pair in objChanges)
            {
                sb.AppendLine("  " + pair.Key + ": " + pair.Value);
            }
        }

        sb.AppendLine();
        sb.AppendLine("Special vehicle auto-config:");
        sb.AppendLine("  Ready (from suitability cache): " + autoConfigReady);
        sb.AppendLine("  Blocked (ANALYZE REQUIRED, objective only): " + autoConfigBlocked);
        sb.AppendLine("  Limit-only updates: " + limitOnlyChanges);

        sb.AppendLine();
        sb.AppendLine("Special parameter changes:");
        int paramLines = 0;
        for (int i = 0; i < dirty.Count; i++)
        {
            RowState row = dirty[i];
            if (row.level == null || !AreSpecialLimitsDirty(row))
            {
                continue;
            }

            string name = row.level.name;
            if (!Mathf.Approximately(row.plannedTimeLimit, row.level.timeLimitSeconds) &&
                row.plannedObjective == LevelObjectiveType.TimedAmbulance)
            {
                sb.AppendLine(
                    "  Timed " + name + ": " +
                    row.level.timeLimitSeconds.ToString("0.#") + "s → " +
                    row.plannedTimeLimit.ToString("0.#") + "s"
                );
                paramLines++;
            }

            if (row.plannedMoveLimit != row.level.moveLimit &&
                row.plannedObjective == LevelObjectiveType.MoveLimit)
            {
                sb.AppendLine(
                    "  MoveLimit " + name + ": " +
                    row.level.moveLimit + " → " + row.plannedMoveLimit
                );
                paramLines++;
            }

            if (row.plannedFragileLimit != row.level.fragileCargoMoveLimit &&
                row.plannedObjective == LevelObjectiveType.FragileCargo)
            {
                sb.AppendLine(
                    "  Fragile " + name + ": " +
                    row.level.fragileCargoMoveLimit + " → " + row.plannedFragileLimit
                );
                paramLines++;
            }

            if (row.plannedLimitedLimit != row.level.limitedVehicleMoveLimit &&
                row.plannedObjective == LevelObjectiveType.LimitedVehicle)
            {
                sb.AppendLine(
                    "  Limited " + name + ": " +
                    row.level.limitedVehicleMoveLimit + " → " + row.plannedLimitedLimit
                );
                paramLines++;
            }
        }

        if (paramLines == 0)
        {
            sb.AppendLine("  (none)");
        }

        sb.AppendLine();
        sb.AppendLine("Database order and levelNumber will NOT change.");
        return sb.ToString();
    }

    private static bool RequiresVehicleAutoConfiguration(LevelObjectiveType objective)
    {
        return objective == LevelObjectiveType.NoTouchChallenge
            || objective == LevelObjectiveType.FragileCargo
            || objective == LevelObjectiveType.LimitedVehicle
            || objective == LevelObjectiveType.MultiTargetRescue;
    }

    /// <summary>
    /// True when the suitability cache holds a usable vehicle recommendation for
    /// the planned objective.
    /// </summary>
    private bool HasVehicleRecommendation(RowState row)
    {
        SuitabilityScores scores;
        if (row == null ||
            !suitabilityByDbIndex.TryGetValue(row.dbIndex, out scores) ||
            !scores.hasData)
        {
            return false;
        }

        switch (row.plannedObjective)
        {
            case LevelObjectiveType.NoTouchChallenge:
                return scores.noTouchVehicleIndex >= 0;
            case LevelObjectiveType.FragileCargo:
                return scores.fragileVehicleIndex >= 0;
            case LevelObjectiveType.LimitedVehicle:
                return scores.limitedVehicleIndex >= 0;
            case LevelObjectiveType.MultiTargetRescue:
                return scores.multiPrimaryIndex >= 0 && scores.multiCandidateIndex >= 0;
            default:
                return false;
        }
    }

    // -------------------------------------------------------------------------
    // Validation helpers
    // -------------------------------------------------------------------------

    private List<string> BuildProgressionWarnings(int easy, int medium, int hard)
    {
        List<string> warnings = new List<string>();
        int mediumNeed = GetMediumRequiredEasy();
        int hardNeedEasy = GetHardRequiredEasy();
        int hardNeedMedium = GetHardRequiredMedium();

        if (easy == 0)
        {
            warnings.Add("Easy tier has 0 levels.");
        }
        else if (easy < mediumNeed)
        {
            warnings.Add(
                "Fewer than " + mediumNeed + " Easy levels (" + easy +
                ") — Medium unlock may be impossible."
            );
        }

        if (medium == 0)
        {
            warnings.Add("Medium tier has 0 levels.");
        }
        else if (medium < hardNeedMedium && hard > 0)
        {
            warnings.Add(
                "Hard exists but Medium count (" + medium +
                ") < Hard requirement (" + hardNeedMedium + ")."
            );
        }

        if (hard > 0 && easy < hardNeedEasy)
        {
            warnings.Add(
                "Hard exists but Easy count (" + easy +
                ") < Hard Easy-requirement (" + hardNeedEasy + ")."
            );
        }

        if (hard == 0)
        {
            warnings.Add("Hard tier has 0 levels.");
        }

        return warnings;
    }

    private string BuildRowWarning(RowState row)
    {
        LevelData level = row.level;
        if (level == null)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();

        string mismatch = LevelMinMovesDifficulty.GetMismatchWarning(
            row.plannedDifficulty,
            level.minimumMoves
        );
        if (!string.IsNullOrEmpty(mismatch))
        {
            parts.Add(mismatch);
        }

        if (RequiresVehicleAutoConfiguration(row.plannedObjective))
        {
            SuitabilityScores scores;
            bool hasScores = suitabilityByDbIndex.TryGetValue(row.dbIndex, out scores) &&
                scores.hasData;
            if (!hasScores)
            {
                parts.Add("ANALYZE REQUIRED");
            }
            else if (IsLowSuitability(row, scores))
            {
                parts.Add("LOW SUITABILITY");
            }
        }

        string config = BuildObjectiveConfigWarning(row, level);
        if (!string.IsNullOrEmpty(config))
        {
            // Apply rewrites the flags for dirty rows, so a stale-config warning is noise.
            parts.Add(
                IsRowDirty(row) && WillApplyConfigureObjective(row)
                    ? "will auto-configure on Apply"
                    : config
            );
        }

        return string.Join(" | ", parts.ToArray());
    }

    /// <summary>
    /// True when the planned objective has no usable recommendation or a Poor score.
    /// Apply is still allowed.
    /// </summary>
    private bool IsLowSuitability(RowState row, SuitabilityScores scores)
    {
        if (!HasVehicleRecommendation(row))
        {
            return true;
        }

        switch (row.plannedObjective)
        {
            case LevelObjectiveType.NoTouchChallenge:
                return scores.noTouchCategory != SpecialMissionSuitabilityAnalyzer.Category.Good &&
                    scores.noTouchCategory != SpecialMissionSuitabilityAnalyzer.Category.Maybe;
            case LevelObjectiveType.FragileCargo:
                return scores.fragileCategory != SpecialMissionSuitabilityAnalyzer.Category.Good &&
                    scores.fragileCategory != SpecialMissionSuitabilityAnalyzer.Category.Maybe;
            case LevelObjectiveType.LimitedVehicle:
                return scores.limitedCategory != SpecialMissionSuitabilityAnalyzer.Category.Good &&
                    scores.limitedCategory != SpecialMissionSuitabilityAnalyzer.Category.Maybe;
            case LevelObjectiveType.MultiTargetRescue:
                return scores.multiTargetCategory != MultiTargetSuitabilityAnalyzer.Category.Good &&
                    scores.multiTargetCategory != MultiTargetSuitabilityAnalyzer.Category.Maybe;
            default:
                return false;
        }
    }

    /// <summary>
    /// True when APPLY has everything it needs to configure the planned objective,
    /// so current LevelData flags are about to be rewritten.
    /// </summary>
    private bool WillApplyConfigureObjective(RowState row)
    {
        switch (row.plannedObjective)
        {
            case LevelObjectiveType.Classic:
                return true;
            case LevelObjectiveType.TimedAmbulance:
                return row.plannedTimeLimit > 0f;
            case LevelObjectiveType.MoveLimit:
                return row.plannedMoveLimit > 0;
            default:
                return HasVehicleRecommendation(row);
        }
    }

    /// <summary>
    /// Incomplete-configuration warning based on planned limits + current LevelData flags.
    /// </summary>
    private static string BuildObjectiveConfigWarning(RowState row, LevelData level)
    {
        switch (row.plannedObjective)
        {
            case LevelObjectiveType.TimedAmbulance:
                if (row.plannedTimeLimit <= 0f)
                {
                    return "Timed: timeLimitSeconds <= 0";
                }

                if (CountExitTargets(level) < 1)
                {
                    return "Timed: no canExitRight target";
                }

                break;

            case LevelObjectiveType.MoveLimit:
                if (row.plannedMoveLimit <= 0)
                {
                    return "MoveLimit: moveLimit <= 0";
                }

                break;

            case LevelObjectiveType.MultiTargetRescue:
                int targets = CountExitTargets(level);
                if (targets < 2)
                {
                    return "MultiTarget: <2 exit targets (" + targets + ")";
                }

                break;

            case LevelObjectiveType.NoTouchChallenge:
                int protectedCount = CountFlag(level, v => v.isProtectedVehicle);
                if (protectedCount == 0)
                {
                    return "NoTouch: no protected vehicle";
                }

                if (protectedCount > 1)
                {
                    return "NoTouch: >1 protected vehicle";
                }

                if (CountExitTargets(level) < 1)
                {
                    return "NoTouch: no normal target";
                }

                break;

            case LevelObjectiveType.FragileCargo:
                int fragile = CountFlag(level, v => v.isFragileCargo);
                if (fragile == 0)
                {
                    return "Fragile: no isFragileCargo";
                }

                if (row.plannedFragileLimit <= 0)
                {
                    return "Fragile: fragileCargoMoveLimit <= 0";
                }

                break;

            case LevelObjectiveType.LimitedVehicle:
                int limited = CountFlag(level, v => v.isLimitedVehicle);
                if (limited == 0)
                {
                    return "Limited: no isLimitedVehicle";
                }

                if (row.plannedLimitedLimit <= 0)
                {
                    return "Limited: limitedVehicleMoveLimit <= 0";
                }

                break;
        }

        return string.Empty;
    }

    private static int CountExitTargets(LevelData level)
    {
        if (level.vehicles == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < level.vehicles.Count; i++)
        {
            VehicleData v = level.vehicles[i];
            if (v != null && v.canExitRight)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountFlag(LevelData level, Func<VehicleData, bool> predicate)
    {
        if (level.vehicles == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < level.vehicles.Count; i++)
        {
            VehicleData v = level.vehicles[i];
            if (v != null && predicate(v))
            {
                count++;
            }
        }

        return count;
    }

    // -------------------------------------------------------------------------
    // Misc helpers
    // -------------------------------------------------------------------------

    private static bool IsRowDirty(RowState row)
    {
        if (row == null || row.level == null)
        {
            return false;
        }

        return row.plannedDifficulty != row.level.difficulty
            || row.plannedObjective != row.level.objectiveType
            || AreSpecialLimitsDirty(row);
    }

    private int CountDirtyRows()
    {
        int count = 0;
        for (int i = 0; i < rows.Count; i++)
        {
            if (IsRowDirty(rows[i]))
            {
                count++;
            }
        }

        return count;
    }

    private int CountSelectedRows()
    {
        int count = 0;
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].selected)
            {
                count++;
            }
        }

        return count;
    }

    private int GetMediumRequiredEasy()
    {
        return progressionConfig != null
            ? progressionConfig.mediumRequiredEasy
            : DifficultyProgressionConfig.DefaultMediumRequiredEasy;
    }

    private int GetHardRequiredEasy()
    {
        return progressionConfig != null
            ? progressionConfig.hardRequiredEasy
            : DifficultyProgressionConfig.DefaultHardRequiredEasy;
    }

    private int GetHardRequiredMedium()
    {
        return progressionConfig != null
            ? progressionConfig.hardRequiredMedium
            : DifficultyProgressionConfig.DefaultHardRequiredMedium;
    }

    private static string ShortObjective(LevelObjectiveType type)
    {
        switch (type)
        {
            case LevelObjectiveType.TimedAmbulance:
                return "Timed";
            case LevelObjectiveType.MoveLimit:
                return "MoveLimit";
            case LevelObjectiveType.MultiTargetRescue:
                return "MultiTarget";
            case LevelObjectiveType.NoTouchChallenge:
                return "NoTouch";
            case LevelObjectiveType.FragileCargo:
                return "Fragile";
            case LevelObjectiveType.LimitedVehicle:
                return "Limited";
            default:
                return "Classic";
        }
    }

    private static string ShortCategory(SpecialMissionSuitabilityAnalyzer.Category category)
    {
        switch (category)
        {
            case SpecialMissionSuitabilityAnalyzer.Category.Good:
                return "Good";
            case SpecialMissionSuitabilityAnalyzer.Category.Maybe:
                return "Maybe";
            case SpecialMissionSuitabilityAnalyzer.Category.Poor:
                return "Poor";
            default:
                return "n/a";
        }
    }

    private static string ShortCategory(MultiTargetSuitabilityAnalyzer.Category category)
    {
        switch (category)
        {
            case MultiTargetSuitabilityAnalyzer.Category.Good:
                return "Good";
            case MultiTargetSuitabilityAnalyzer.Category.Maybe:
                return "Maybe";
            case MultiTargetSuitabilityAnalyzer.Category.Poor:
                return "Poor";
            default:
                return "n/a";
        }
    }

    private static Color CategoryColor(SpecialMissionSuitabilityAnalyzer.Category category)
    {
        switch (category)
        {
            case SpecialMissionSuitabilityAnalyzer.Category.Good:
                return new Color(0.2f, 0.75f, 0.35f);
            case SpecialMissionSuitabilityAnalyzer.Category.Maybe:
                return new Color(0.95f, 0.7f, 0.15f);
            case SpecialMissionSuitabilityAnalyzer.Category.Poor:
                return new Color(0.9f, 0.35f, 0.3f);
            default:
                return Color.gray;
        }
    }

    private static Color CategoryColor(MultiTargetSuitabilityAnalyzer.Category category)
    {
        return CategoryColor(ToSpecialCategory(category));
    }

    private static LevelDatabase LoadMainLevelDatabase()
    {
        string[] guids = AssetDatabase.FindAssets(DatabaseFilter);
        if (guids == null || guids.Length == 0)
        {
            Debug.LogError("Level Content Planner: MainLevelDatabase niet gevonden.");
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);
    }

    private static DifficultyProgressionConfig LoadProgressionConfig()
    {
        string[] guids = AssetDatabase.FindAssets("t:DifficultyProgressionConfig");
        if (guids == null || guids.Length == 0)
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<DifficultyProgressionConfig>(path);
    }
}

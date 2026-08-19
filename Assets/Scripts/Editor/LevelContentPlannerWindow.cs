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
    private const float MinWidth = 1000f;
    private const float MinHeight = 650f;

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

    private sealed class RowState
    {
        public int dbIndex;
        public LevelData level;
        public LevelDifficulty plannedDifficulty;
        public LevelObjectiveType plannedObjective;
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
    }

    private LevelDatabase database;
    private DifficultyProgressionConfig progressionConfig;
    private readonly List<RowState> rows = new List<RowState>();
    private readonly Dictionary<int, SuitabilityScores> suitabilityByDbIndex =
        new Dictionary<int, SuitabilityScores>();

    private Vector2 tableScroll;
    private Vector2 summaryScroll;

    private DifficultyFilter difficultyFilter = DifficultyFilter.All;
    private ObjectiveFilter objectiveFilter = ObjectiveFilter.All;
    private int gridSizeFilterIndex;
    private readonly List<string> gridSizeLabels = new List<string> { "All" };
    private readonly List<Vector2Int> gridSizeValues = new List<Vector2Int>();
    private string searchText = string.Empty;
    private SortMode sortMode = SortMode.DbIndex;
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
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Level Content Planner", EditorStyles.boldLabel);
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

        if (GUILayout.Button("Apply Final Set → DB", GUILayout.Height(28f)))
        {
            string report = V1AutoCurator.ApplyFinalToMainLevelDatabase();
            curationViewFilter = CurationViewFilter.MainDatabase;
            ReloadFromDatabase();
            EditorUtility.DisplayDialog("Apply Final Set", TrimDialog(report), "OK");
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
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Reload Database", GUILayout.Height(26f), GUILayout.Width(140f)))
        {
            if (CountDirtyRows() > 0)
            {
                bool ok = EditorUtility.DisplayDialog(
                    "Reload Database",
                    "Reload discards unapplied planned changes. Continue?",
                    "Reload",
                    "Cancel"
                );
                if (!ok)
                {
                    EditorGUILayout.EndHorizontal();
                    return;
                }
            }

            curationViewFilter = CurationViewFilter.MainDatabase;
            ReloadFromDatabase();
        }

        if (GUILayout.Button("Analyze Suitability", GUILayout.Height(26f), GUILayout.Width(160f)))
        {
            RunSuitabilityAnalyze();
        }

        GUI.enabled = suitabilityByDbIndex.Count > 0;
        if (GUILayout.Button("Auto Suggest Specials", GUILayout.Height(26f), GUILayout.Width(170f)))
        {
            AutoSuggestSpecials();
        }

        GUI.enabled = true;

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Revert Planned", GUILayout.Height(26f), GUILayout.Width(130f)))
        {
            RevertPlannedChanges();
        }

        GUI.enabled = CountDirtyRows() > 0;
        GUI.backgroundColor = new Color(0.55f, 0.9f, 0.55f);
        if (GUILayout.Button("APPLY PLANNED ASSIGNMENTS", GUILayout.Height(26f), GUILayout.Width(220f)))
        {
            TryApplyPlannedAssignments();
        }

        GUI.backgroundColor = Color.white;
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
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

        tableScroll = EditorGUILayout.BeginScrollView(
            tableScroll,
            true,
            true,
            GUILayout.ExpandHeight(true)
        );

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
        GUI.Label(new Rect(x, padY, 44f, h), level.levelNumber.ToString());
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

        GUI.Label(new Rect(x, padY, 220f, h), warning);
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
                    break;
                case 1:
                    score = scores.fragileScore;
                    cat = ShortCategory(scores.fragileCategory);
                    color = CategoryColor(scores.fragileCategory);
                    break;
                case 2:
                    score = scores.limitedScore;
                    cat = ShortCategory(scores.limitedCategory);
                    color = CategoryColor(scores.limitedCategory);
                    break;
                default:
                    score = scores.multiTargetScore;
                    cat = ShortCategory(scores.multiTargetCategory);
                    color = CategoryColor(scores.multiTargetCategory);
                    break;
            }

            text = score + " " + cat;
        }

        Color prev = GUI.color;
        GUI.color = color;
        GUI.Label(new Rect(x, y, 72f, h), text);
        GUI.color = prev;
        x += 72f;
    }

    private void DrawSidePanel()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Width(280f));
        EditorGUILayout.LabelField("Planned Balance", EditorStyles.boldLabel);

        summaryScroll = EditorGUILayout.BeginScrollView(summaryScroll, GUILayout.ExpandHeight(true));

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

        EditorGUILayout.LabelField("DIFFICULTY", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Easy: " + easy);
        EditorGUILayout.LabelField("Medium: " + medium);
        EditorGUILayout.LabelField("Hard: " + hard);
        EditorGUILayout.Space(6f);

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

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
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
                scores = new SuitabilityScores { hasData = true };
            }

            scores.hasData = true;
            switch (r.missionKind)
            {
                case SpecialMissionSuitabilityAnalyzer.MissionKind.NoTouch:
                    scores.noTouchScore = r.score;
                    scores.noTouchCategory = r.category;
                    break;
                case SpecialMissionSuitabilityAnalyzer.MissionKind.FragileCargo:
                    scores.fragileScore = r.score;
                    scores.fragileCategory = r.category;
                    break;
                case SpecialMissionSuitabilityAnalyzer.MissionKind.LimitedVehicle:
                    scores.limitedScore = r.score;
                    scores.limitedCategory = r.category;
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
                scores = new SuitabilityScores { hasData = true };
            }

            scores.hasData = true;
            scores.multiTargetScore = r.score;
            scores.multiTargetCategory = r.category;
            suitabilityByDbIndex[dbIndex] = scores;
        }

        Debug.Log(
            "Level Content Planner: suitability cached for " +
            suitabilityByDbIndex.Count + " levels."
        );
        Repaint();
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
            }
        }
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
        for (int i = 0; i < dirty.Count; i++)
        {
            RowState row = dirty[i];
            LevelData level = row.level;
            if (level == null)
            {
                continue;
            }

            Undo.RecordObject(level, "Apply Level Content Plan");
            level.difficulty = row.plannedDifficulty;
            level.objectiveType = row.plannedObjective;
            EditorUtility.SetDirty(level);
            applied++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Level Content Planner: applied assignments to " + applied + " levels.");
        Repaint();
    }

    private string BuildApplyConfirmationSummary(List<RowState> dirty)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Apply changes to " + dirty.Count + " levels?");
        sb.AppendLine();
        sb.AppendLine("Difficulty changes:");

        Dictionary<string, int> diffChanges = new Dictionary<string, int>();
        Dictionary<string, int> objChanges = new Dictionary<string, int>();

        for (int i = 0; i < dirty.Count; i++)
        {
            RowState row = dirty[i];
            if (row.level == null)
            {
                continue;
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
        sb.AppendLine("Database order and levelNumber will NOT change.");
        return sb.ToString();
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

        switch (row.plannedObjective)
        {
            case LevelObjectiveType.TimedAmbulance:
                if (level.timeLimitSeconds <= 0f)
                {
                    return "Timed: timeLimitSeconds <= 0";
                }

                if (CountExitTargets(level) < 1)
                {
                    return "Timed: no canExitRight target";
                }

                break;

            case LevelObjectiveType.MoveLimit:
                if (level.moveLimit <= 0)
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

                if (level.fragileCargoMoveLimit <= 0)
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

                if (level.limitedVehicleMoveLimit <= 0)
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
            || row.plannedObjective != row.level.objectiveType;
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

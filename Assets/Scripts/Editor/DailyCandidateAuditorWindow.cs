using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor-only scan of LevelData assets for Daily candidate quality.
/// Does not modify levels. Solver minimumMoves is authoritative.
/// </summary>
public class DailyCandidateAuditorWindow : EditorWindow
{
    public const int MinimumMovesMin = 8;
    public const int MinimumMovesMax = 13;
    public const string GeneratedDailyFolder = "Assets/Data/GeneratedLevels/Daily";
    private const float SimilarOverlap = 0.8f;
    private const string TestedGuidsPref = "RushOut_DailyGeneratedPreviewTestedGuids";
    private const string ReviewDecisionsPref = "RushOut_DailyGeneratedReviewDecisions";

    private enum DailyReviewStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    private enum ReviewListFilter
    {
        All = 0,
        Pending,
        Approved,
        Rejected
    }

    private enum ExportMode
    {
        AppendApproved = 0,
        ReplacePool = 1
    }

    private enum PoolPresence
    {
        NotExported = 0,
        InPool,
        Mismatch
    }

    private enum AuditSource
    {
        AllEligibleLevels = 0,
        GeneratedDailyOnly = 1
    }

    private static readonly string[] ExportModeLabels =
    {
        "Append Approved",
        "Replace Pool"
    };

    private static readonly string[] SourceLabels =
    {
        "All Eligible Levels",
        "Generated Daily Only"
    };

    private enum AuditReason
    {
        Eligible = 0,
        WrongObjective,
        InvalidVehicles,
        CannotExitRight,
        Unsolvable,
        MinimumMovesTooLow,
        MinimumMovesTooHigh,
        Duplicate
    }

    private sealed class AuditRow
    {
        public AuditReason reason;
        public string guid;
        public string path;
        public string assetName;
        public int gridWidth;
        public int gridHeight;
        public int exitRow;
        public int solverMoves;
        public int authoredMoves;
        public int vehicleCount;
        public int databaseIndex;
        public string fingerprint;
        public string detail;
        public string similarTo;
        public LevelData asset;
    }

    private readonly List<AuditRow> rows = new List<AuditRow>();
    private readonly int[] reasonCounts = new int[8];
    private Vector2 scrollPosition;
    private AuditSource source = AuditSource.AllEligibleLevels;
    private AuditReason listFilter = AuditReason.Eligible;
    private bool showAllStatuses;
    private bool audited;
    private int similarWarnings;
    private string[] generatedDailyGuids = System.Array.Empty<string>();
    private HashSet<string> testedGuids;
    private Dictionary<string, DailyReviewStatus> reviewDecisions;
    private int reviewIndex;
    private ReviewListFilter reviewListFilter = ReviewListFilter.Pending;
    private ExportMode exportMode = ExportMode.AppendApproved;
    private bool membershipLoaded;
    private PoolMembership membership;
    private string status = "Run an audit to classify LevelData assets. Nothing is modified.";

    private void OnEnable()
    {
        RefreshGeneratedDailyGuids();
        EnsureTestedLoaded();
        EnsureReviewLoaded();
    }

    [MenuItem("Rush Out/Daily/Audit Daily Candidates")]
    public static void Open()
    {
        DailyCandidateAuditorWindow window = GetWindow<DailyCandidateAuditorWindow>(
            "Daily Candidate Audit");
        window.minSize = new Vector2(720f, 420f);
        window.Show();
    }

    private void OnGUI()
    {
        membershipLoaded = false;
        EditorGUILayout.LabelField("Daily candidate audit", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawAuditorBody();
        EditorGUILayout.EndScrollView();
    }

    private void DrawAuditorBody()
    {
        DrawGeneratedReviewCounts();
        EditorGUILayout.HelpBox(
            "Classic boards only. RushOutSolver minimumMoves must be " +
            MinimumMovesMin + "–" + MinimumMovesMax +
            ". Exact layout duplicates are rejected. Near-duplicates are listed, not rejected. " +
            "Generated Daily Only scans " + GeneratedDailyFolder +
            " and does not include campaign levels, MainLevelDatabase, or DailyChallengeConfig. " +
            "This window does not write LevelData.",
            MessageType.Info);

        EditorGUI.BeginChangeCheck();
        int sourceIndex = EditorGUILayout.Popup("Source", (int)source, SourceLabels);
        if (EditorGUI.EndChangeCheck())
        {
            source = (AuditSource)sourceIndex;
            showAllStatuses = source == AuditSource.GeneratedDailyOnly;
            if (source == AuditSource.GeneratedDailyOnly)
            {
                reviewListFilter = ReviewListFilter.Pending;
                reviewIndex = 0;
            }

            ClearAuditResults();
            status = source == AuditSource.GeneratedDailyOnly
                ? "Source set to Generated Daily Only. Run Audit to scan " + GeneratedDailyFolder + "."
                : "Source set to All Eligible Levels. Run Audit to scan every LevelData asset.";
            RefreshGeneratedDailyGuids();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Run Audit", GUILayout.Height(28f), GUILayout.Width(140f)))
        {
            RunAudit();
        }

        using (new EditorGUI.DisabledScope(!audited || CountReason(AuditReason.Eligible) == 0))
        {
            if (GUILayout.Button("Export Eligible JSON", GUILayout.Height(28f), GUILayout.Width(180f)))
            {
                ExportEligible();
            }
        }

        EditorGUILayout.EndHorizontal();
        DrawPreviewActions();
        if (source == AuditSource.GeneratedDailyOnly)
        {
            DrawDailyReview();
        }

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField(status, EditorStyles.wordWrappedLabel);

        if (!audited)
        {
            return;
        }

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Totals", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Scanned", rows.Count.ToString());
        EditorGUILayout.LabelField("Eligible (" + MinimumMovesMin + "–" + MinimumMovesMax + ")",
            CountReason(AuditReason.Eligible).ToString());
        EditorGUILayout.LabelField("Similar warnings (still eligible)", similarWarnings.ToString());

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Rejections", EditorStyles.boldLabel);
        DrawCount(AuditReason.WrongObjective);
        DrawCount(AuditReason.InvalidVehicles);
        DrawCount(AuditReason.CannotExitRight);
        DrawCount(AuditReason.Unsolvable);
        DrawCount(AuditReason.MinimumMovesTooLow);
        DrawCount(AuditReason.MinimumMovesTooHigh);
        DrawCount(AuditReason.Duplicate);

        EditorGUILayout.Space(6f);
        if (source == AuditSource.GeneratedDailyOnly)
        {
            showAllStatuses = EditorGUILayout.Toggle("Show all statuses", showAllStatuses);
        }

        if (!showAllStatuses)
        {
            listFilter = (AuditReason)EditorGUILayout.EnumPopup("List", listFilter);
        }

        for (int i = 0; i < rows.Count; i++)
        {
            AuditRow row = rows[i];
            if (!IsRowVisible(row))
            {
                continue;
            }

            DrawRow(row);
        }
    }

    private void DrawCount(AuditReason reason)
    {
        EditorGUILayout.LabelField(reason.ToString(), CountReason(reason).ToString());
    }

    private int CountReason(AuditReason reason)
    {
        return reasonCounts[(int)reason];
    }

    private void DrawRow(AuditRow row)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(row.assetName, EditorStyles.boldLabel);
        string statusText = row.reason.ToString();
        if (!string.IsNullOrEmpty(row.detail))
        {
            statusText += " — " + row.detail;
        }

        EditorGUILayout.LabelField("Status", statusText);
        EditorGUILayout.LabelField(
            "Grid " + row.gridWidth + "x" + row.gridHeight +
            "    solver minimumMoves=" + row.solverMoves +
            "    vehicles=" + row.vehicleCount +
            "    authoredMoves=" + row.authoredMoves);
        if (source != AuditSource.GeneratedDailyOnly)
        {
            EditorGUILayout.LabelField("dbIndex", row.databaseIndex.ToString());
        }

        EditorGUILayout.LabelField("GUID " + row.guid);
        EditorGUILayout.LabelField(row.path, EditorStyles.miniLabel);
        if (!string.IsNullOrEmpty(row.fingerprint))
        {
            EditorGUILayout.LabelField("Fingerprint", row.fingerprint, EditorStyles.miniLabel);
        }

        if (!string.IsNullOrEmpty(row.similarTo))
        {
            EditorGUILayout.LabelField(
                "Near-duplicate warning: similar to " + row.similarTo,
                EditorStyles.miniLabel);
        }

        if (IsTested(row.guid))
        {
            EditorGUILayout.LabelField("Tested in local Daily preview");
        }

        if (source == AuditSource.GeneratedDailyOnly)
        {
            EditorGUILayout.LabelField("Review", GetReviewStatus(row.guid).ToString());
            if (GetReviewStatus(row.guid) == DailyReviewStatus.Approved)
            {
                EditorGUILayout.LabelField(FormatPresence(GetPoolPresence(row.guid, row.assetName)));
            }
        }

        if (GUILayout.Button("Select asset", GUILayout.Width(120f)))
        {
            Selection.activeObject = row.asset;
            EditorGUIUtility.PingObject(row.asset);
        }

        EditorGUILayout.EndVertical();
    }

    private void RunAudit()
    {
        rows.Clear();
        similarWarnings = 0;
        for (int i = 0; i < reasonCounts.Length; i++)
        {
            reasonCounts[i] = 0;
        }

        if (source == AuditSource.GeneratedDailyOnly &&
            !AssetDatabase.IsValidFolder(GeneratedDailyFolder))
        {
            audited = false;
            status = "Folder not found: " + GeneratedDailyFolder + ". No assets were modified.";
            RefreshGeneratedDailyGuids();
            return;
        }

        List<string> paths = CollectLevelPaths(source);
        Dictionary<string, int> databaseIndexByPath = source == AuditSource.GeneratedDailyOnly
            ? new Dictionary<string, int>(System.StringComparer.Ordinal)
            : LoadDatabaseIndices();
        Dictionary<string, string> firstPathByFingerprint =
            new Dictionary<string, string>(System.StringComparer.Ordinal);

        bool cancelled = false;
        try
        {
            for (int i = 0; i < paths.Count; i++)
            {
                string path = paths[i];
                if (EditorUtility.DisplayCancelableProgressBar(
                        "Daily candidate audit",
                        path,
                        paths.Count == 0 ? 1f : (float)i / paths.Count))
                {
                    cancelled = true;
                    break;
                }

                LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (level == null)
                {
                    continue;
                }

                bool dirtyBefore = EditorUtility.IsDirty(level);
                AuditRow row = Classify(
                    level,
                    path,
                    databaseIndexByPath,
                    firstPathByFingerprint);
                if (!dirtyBefore && EditorUtility.IsDirty(level))
                {
                    EditorUtility.ClearDirty(level);
                }

                rows.Add(row);
                reasonCounts[(int)row.reason]++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        if (cancelled)
        {
            rows.Clear();
            for (int i = 0; i < reasonCounts.Length; i++)
            {
                reasonCounts[i] = 0;
            }

            audited = false;
            status = "Audit cancelled. No assets were modified.";
            return;
        }

        FlagSimilarEligible();
        audited = true;
        RefreshGeneratedDailyGuids();
        status = source == AuditSource.GeneratedDailyOnly
            ? "Audited " + rows.Count + " generated Daily assets in " + GeneratedDailyFolder +
              ". Eligible " + CountReason(AuditReason.Eligible) +
              ". Campaign, database, config, and backend were not scanned. No assets were modified."
            : "Audited " + rows.Count + " LevelData assets. Eligible " +
              CountReason(AuditReason.Eligible) +
              ". No assets were modified.";
        Repaint();
    }

    private void ClearAuditResults()
    {
        rows.Clear();
        similarWarnings = 0;
        audited = false;
        for (int i = 0; i < reasonCounts.Length; i++)
        {
            reasonCounts[i] = 0;
        }
    }

    private bool IsRowVisible(AuditRow row)
    {
        if (!showAllStatuses && row.reason != listFilter)
        {
            return false;
        }

        if (source == AuditSource.GeneratedDailyOnly &&
            !MatchesReviewFilter(row.guid))
        {
            return false;
        }

        return true;
    }

    private bool MatchesReviewFilter(string guid)
    {
        if (reviewListFilter == ReviewListFilter.All)
        {
            return true;
        }

        DailyReviewStatus reviewStatus = GetReviewStatus(guid);
        switch (reviewListFilter)
        {
            case ReviewListFilter.Pending:
                return reviewStatus == DailyReviewStatus.Pending;
            case ReviewListFilter.Approved:
                return reviewStatus == DailyReviewStatus.Approved;
            case ReviewListFilter.Rejected:
                return reviewStatus == DailyReviewStatus.Rejected;
            default:
                return true;
        }
    }

    private static List<string> CollectLevelPaths(AuditSource scanSource)
    {
        string[] guids;
        if (scanSource == AuditSource.GeneratedDailyOnly)
        {
            if (!AssetDatabase.IsValidFolder(GeneratedDailyFolder))
            {
                return new List<string>();
            }

            guids = AssetDatabase.FindAssets("t:LevelData", new[] { GeneratedDailyFolder });
        }
        else
        {
            guids = AssetDatabase.FindAssets("t:LevelData");
        }

        List<string> paths = new List<string>(guids.Length);
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            if (scanSource == AuditSource.GeneratedDailyOnly && !IsUnderGeneratedDaily(path))
            {
                continue;
            }

            paths.Add(path);
        }

        paths.Sort(System.StringComparer.Ordinal);
        return paths;
    }

    private static bool IsUnderGeneratedDaily(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        string normalized = path.Replace('\\', '/');
        return normalized.StartsWith(
            GeneratedDailyFolder + "/",
            System.StringComparison.OrdinalIgnoreCase);
    }

    private void RefreshGeneratedDailyGuids()
    {
        if (!AssetDatabase.IsValidFolder(GeneratedDailyFolder))
        {
            generatedDailyGuids = System.Array.Empty<string>();
            return;
        }

        generatedDailyGuids = AssetDatabase.FindAssets(
            "t:LevelData",
            new[] { GeneratedDailyFolder });
        if (generatedDailyGuids == null)
        {
            generatedDailyGuids = System.Array.Empty<string>();
        }
    }

    private static AuditRow Classify(
        LevelData level,
        string path,
        Dictionary<string, int> databaseIndexByPath,
        Dictionary<string, string> firstPathByFingerprint)
    {
        int dbIndex;
        if (!databaseIndexByPath.TryGetValue(path, out dbIndex))
        {
            dbIndex = -1;
        }

        AuditRow row = new AuditRow
        {
            guid = AssetDatabase.AssetPathToGUID(path),
            path = path,
            assetName = level.name,
            gridWidth = level.ResolvedGridWidth,
            gridHeight = level.ResolvedGridHeight,
            exitRow = level.exitRow,
            authoredMoves = level.minimumMoves,
            vehicleCount = CountVehicles(level),
            databaseIndex = dbIndex,
            fingerprint = LevelCanonicalKey.BuildCanonicalLevelKey(level),
            solverMoves = -1,
            asset = level
        };

        if (level.objectiveType != LevelObjectiveType.Classic)
        {
            row.reason = AuditReason.WrongObjective;
            row.detail = level.objectiveType.ToString();
            RememberFingerprint(row, firstPathByFingerprint);
            return row;
        }

        if (HasNullVehicle(level))
        {
            row.reason = AuditReason.InvalidVehicles;
            row.detail = "Null vehicle entry.";
            RememberFingerprint(row, firstPathByFingerprint);
            return row;
        }

        string validationError = LevelSolver.ValidateLevelData(level);
        if (validationError != null)
        {
            row.reason = IsExitRejection(validationError)
                ? AuditReason.CannotExitRight
                : AuditReason.InvalidVehicles;
            row.detail = validationError;
            RememberFingerprint(row, firstPathByFingerprint);
            return row;
        }

        LevelSolver.SolverResult solved = LevelSolver.Solve(level);
        if (solved.invalid)
        {
            row.reason = IsExitRejection(solved.validationError)
                ? AuditReason.CannotExitRight
                : AuditReason.InvalidVehicles;
            row.detail = solved.validationError;
            RememberFingerprint(row, firstPathByFingerprint);
            return row;
        }

        if (solved.searchLimitReached || !solved.solvable)
        {
            row.reason = AuditReason.Unsolvable;
            row.detail = solved.searchLimitReached
                ? "Search limit: " + (solved.searchLimitReason ?? "unknown")
                : "No solution.";
            RememberFingerprint(row, firstPathByFingerprint);
            return row;
        }

        row.solverMoves = solved.minimumMoves;
        if (solved.minimumMoves < MinimumMovesMin)
        {
            row.reason = AuditReason.MinimumMovesTooLow;
            RememberFingerprint(row, firstPathByFingerprint);
            return row;
        }

        if (solved.minimumMoves > MinimumMovesMax)
        {
            row.reason = AuditReason.MinimumMovesTooHigh;
            RememberFingerprint(row, firstPathByFingerprint);
            return row;
        }

        string firstPath;
        if (!string.IsNullOrEmpty(row.fingerprint) &&
            firstPathByFingerprint.TryGetValue(row.fingerprint, out firstPath))
        {
            row.reason = AuditReason.Duplicate;
            row.detail = "Same board as " + firstPath;
            return row;
        }

        row.reason = AuditReason.Eligible;
        if (row.authoredMoves != row.solverMoves)
        {
            row.detail = "Authored minimumMoves " + row.authoredMoves +
                         " differs from solver " + row.solverMoves + ".";
        }

        RememberFingerprint(row, firstPathByFingerprint);
        return row;
    }

    private static void RememberFingerprint(
        AuditRow row,
        Dictionary<string, string> firstPathByFingerprint)
    {
        if (string.IsNullOrEmpty(row.fingerprint) ||
            firstPathByFingerprint.ContainsKey(row.fingerprint))
        {
            return;
        }

        firstPathByFingerprint[row.fingerprint] = row.path;
    }

    private void FlagSimilarEligible()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            AuditRow candidate = rows[i];
            if (candidate.reason != AuditReason.Eligible)
            {
                continue;
            }

            for (int j = 0; j < i; j++)
            {
                if (!IsNearDuplicate(candidate, rows[j]))
                {
                    continue;
                }

                candidate.similarTo = rows[j].path;
                similarWarnings++;
                break;
            }
        }
    }

    /// <summary>
    /// Same grid, exit row, and vehicle count, with at least 80% occupied-cell overlap.
    /// Exact canonical duplicates return false so callers can reject those separately.
    /// </summary>
    public static bool IsNearDuplicateBoard(LevelData candidate, LevelData other)
    {
        if (candidate == null || other == null)
        {
            return false;
        }

        if (candidate.ResolvedGridWidth != other.ResolvedGridWidth ||
            candidate.ResolvedGridHeight != other.ResolvedGridHeight ||
            candidate.exitRow != other.exitRow)
        {
            return false;
        }

        if (CountVehicles(candidate) != CountVehicles(other))
        {
            return false;
        }

        string candidateKey = LevelCanonicalKey.BuildCanonicalLevelKey(candidate);
        string otherKey = LevelCanonicalKey.BuildCanonicalLevelKey(other);
        if (string.IsNullOrEmpty(candidateKey) || candidateKey == otherKey)
        {
            return false;
        }

        return PlacementOverlap(candidate, other) >= SimilarOverlap;
    }

    private static bool IsNearDuplicate(AuditRow candidate, AuditRow earlier)
    {
        return IsNearDuplicateBoard(candidate.asset, earlier.asset);
    }

    private static float PlacementOverlap(LevelData a, LevelData b)
    {
        HashSet<Vector2Int> cellsA = OccupiedCells(a);
        HashSet<Vector2Int> cellsB = OccupiedCells(b);
        if (cellsA.Count == 0 || cellsB.Count == 0)
        {
            return 0f;
        }

        int intersection = 0;
        foreach (Vector2Int cell in cellsA)
        {
            if (cellsB.Contains(cell))
            {
                intersection++;
            }
        }

        int denom = Mathf.Min(cellsA.Count, cellsB.Count);
        return intersection / (float)denom;
    }

    private static HashSet<Vector2Int> OccupiedCells(LevelData level)
    {
        HashSet<Vector2Int> cells = new HashSet<Vector2Int>();
        if (level == null || level.vehicles == null)
        {
            return cells;
        }

        for (int i = 0; i < level.vehicles.Count; i++)
        {
            VehicleData vehicle = level.vehicles[i];
            if (vehicle == null)
            {
                continue;
            }

            bool horizontal =
                vehicle.orientation == VehicleController.VehicleOrientation.Horizontal;
            List<Vector2Int> covered = RushOutSolver.GetCells(
                vehicle.gridPosition,
                horizontal,
                vehicle.lengthInCells);
            for (int c = 0; c < covered.Count; c++)
            {
                cells.Add(covered[c]);
            }
        }

        return cells;
    }

    private static Dictionary<string, int> LoadDatabaseIndices()
    {
        Dictionary<string, int> indexByPath = new Dictionary<string, int>(
            System.StringComparer.Ordinal);
        string[] dbGuids = AssetDatabase.FindAssets("MainLevelDatabase t:LevelDatabase");
        if (dbGuids == null || dbGuids.Length == 0)
        {
            return indexByPath;
        }

        string dbPath = AssetDatabase.GUIDToAssetPath(dbGuids[0]);
        LevelDatabase database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(dbPath);
        if (database == null || database.levels == null)
        {
            return indexByPath;
        }

        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level == null)
            {
                continue;
            }

            string path = AssetDatabase.GetAssetPath(level);
            if (string.IsNullOrEmpty(path) || indexByPath.ContainsKey(path))
            {
                continue;
            }

            indexByPath.Add(path, i);
        }

        return indexByPath;
    }

    private void ExportEligible()
    {
        string path = EditorUtility.SaveFilePanel(
            "Export eligible Daily candidates",
            Application.dataPath,
            "daily-candidate-audit.json",
            "json");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("{\n");
        sb.Append("  \"minimumMovesMin\": ").Append(MinimumMovesMin).Append(",\n");
        sb.Append("  \"minimumMovesMax\": ").Append(MinimumMovesMax).Append(",\n");
        sb.Append("  \"eligible\": [\n");
        bool first = true;
        for (int i = 0; i < rows.Count; i++)
        {
            AuditRow row = rows[i];
            if (row.reason != AuditReason.Eligible)
            {
                continue;
            }

            if (!first)
            {
                sb.Append(",\n");
            }

            first = false;
            sb.Append("    {\n");
            sb.Append("      \"guid\": \"").Append(JsonEscape(row.guid)).Append("\",\n");
            sb.Append("      \"path\": \"").Append(JsonEscape(row.path)).Append("\",\n");
            sb.Append("      \"gridWidth\": ").Append(row.gridWidth).Append(",\n");
            sb.Append("      \"gridHeight\": ").Append(row.gridHeight).Append(",\n");
            sb.Append("      \"minimumMoves\": ").Append(row.solverMoves).Append(",\n");
            sb.Append("      \"authoredMinimumMoves\": ").Append(row.authoredMoves).Append(",\n");
            sb.Append("      \"vehicleCount\": ").Append(row.vehicleCount).Append(",\n");
            sb.Append("      \"databaseIndex\": ").Append(row.databaseIndex).Append(",\n");
            sb.Append("      \"fingerprint\": \"").Append(JsonEscape(row.fingerprint)).Append("\",\n");
            sb.Append("      \"similarTo\": \"").Append(JsonEscape(row.similarTo)).Append("\"\n");
            sb.Append("    }");
        }

        sb.Append("\n  ]\n}\n");
        File.WriteAllText(path, sb.ToString());
        status = "Exported eligible candidates to " + path;
    }

    private static int CountVehicles(LevelData level)
    {
        if (level.vehicles == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < level.vehicles.Count; i++)
        {
            if (level.vehicles[i] != null)
            {
                count++;
            }
        }

        return count;
    }

    private static bool HasNullVehicle(LevelData level)
    {
        if (level.vehicles == null)
        {
            return false;
        }

        for (int i = 0; i < level.vehicles.Count; i++)
        {
            if (level.vehicles[i] == null)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsExitRejection(string validationError)
    {
        if (string.IsNullOrEmpty(validationError))
        {
            return false;
        }

        return validationError.IndexOf("canExitRight", System.StringComparison.Ordinal) >= 0 ||
               validationError.IndexOf("target", System.StringComparison.Ordinal) >= 0;
    }

    private static string JsonEscape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private void DrawGeneratedReviewCounts()
    {
        CountReview(out int pending, out int approved, out int rejected);
        int total = generatedDailyGuids.Length;
        EditorGUILayout.LabelField(
            "Total: " + total +
            "    Pending: " + pending +
            "    Approved: " + approved +
            "    Rejected: " + rejected +
            "    Tested: " + CountTestedGenerated() + " / " + total);
        CountExportPresence(out int notExported, out int inPool, out int mismatch, out string catalogError);
        if (!string.IsNullOrEmpty(catalogError))
        {
            EditorGUILayout.LabelField("Backend catalog", catalogError);
        }
        else
        {
            EditorGUILayout.LabelField(
                "Approved not exported: " + notExported +
                "    Approved already in pool: " + inPool +
                "    Export mismatch: " + mismatch);
        }
    }

    private void DrawDailyReview()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Daily review", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "The queue defaults to Pending. Accept and Reject leave that queue and open the next Pending level. " +
            "Approved and Rejected stay available in their filters. Reset Decision returns a level to Pending.",
            MessageType.Info);
        EditorGUI.BeginChangeCheck();
        reviewListFilter = (ReviewListFilter)EditorGUILayout.EnumPopup(
            "Review Status",
            reviewListFilter);
        if (EditorGUI.EndChangeCheck())
        {
            reviewIndex = 0;
        }

        exportMode = (ExportMode)EditorGUILayout.Popup(
            "Export Mode",
            (int)exportMode,
            ExportModeLabels);
        DrawExportPreview();
        string exportButton = exportMode == ExportMode.AppendApproved
            ? "ADD APPROVED TO DAILY POOL"
            : "REPLACE DAILY POOL WITH APPROVED";
        if (GUILayout.Button(exportButton, GUILayout.Height(28f)))
        {
            ExportApprovedDailyLevels();
        }

        EditorGUILayout.HelpBox(
            "Same one-candidate review as RushOut → Generated Levels → Review. " +
            "Accept marks this Daily candidate Approved for a later export. " +
            "Reject marks it Rejected. Nothing is moved, renamed, deleted, or published. " +
            "Reset Decision returns it to Pending.",
            MessageType.Info);

        List<LevelData> order = BuildGeneratedPlayOrder();
        if (order.Count == 0)
        {
            EditorGUILayout.HelpBox(
                generatedDailyGuids.Length == 0
                    ? "No generated Daily levels in " + GeneratedDailyFolder + "."
                    : "No candidates with review status " + reviewListFilter + ".",
                MessageType.Info);
            return;
        }

        reviewIndex = Mathf.Clamp(reviewIndex, 0, order.Count - 1);
        LevelData level = order[reviewIndex];
        if (level == null)
        {
            EditorGUILayout.HelpBox("Current candidate is missing. Refresh the source.", MessageType.Warning);
            return;
        }

        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(level));
        DailyReviewStatus reviewStatus = GetReviewStatus(guid);
        int vehicleCount = level.vehicles != null ? level.vehicles.Count : 0;

        EditorGUILayout.LabelField(
            "Level " + (reviewIndex + 1) + " / " + order.Count,
            EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Asset", level.name);
        EditorGUILayout.LabelField("Review", reviewStatus.ToString());
        if (reviewStatus == DailyReviewStatus.Approved)
        {
            EditorGUILayout.LabelField(FormatPresence(GetPoolPresence(guid, level.name)));
        }
        EditorGUILayout.LabelField("minimumMoves", level.minimumMoves.ToString());
        EditorGUILayout.LabelField("Vehicles", vehicleCount.ToString());
        EditorGUILayout.LabelField(
            "Grid",
            level.ResolvedGridWidth + "x" + level.ResolvedGridHeight);

        float previewSize = Mathf.Min(position.width - 40f, 360f);
        Rect previewRect = GUILayoutUtility.GetRect(
            previewSize,
            previewSize,
            GUILayout.ExpandWidth(false));
        previewRect.x = (position.width - previewSize) * 0.5f;
        GeneratedLevelReviewWindow.DrawReviewGridPreview(previewRect, level);

        EditorGUILayout.Space(8f);
        EditorGUILayout.BeginHorizontal();
        using (new EditorGUI.DisabledScope(reviewIndex <= 0))
        {
            if (GUILayout.Button("Previous", GUILayout.Height(28f)))
            {
                reviewIndex--;
                Repaint();
            }
        }

        using (new EditorGUI.DisabledScope(reviewIndex >= order.Count - 1))
        {
            if (GUILayout.Button("Next", GUILayout.Height(28f)))
            {
                reviewIndex++;
                Repaint();
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4f);
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.45f, 0.85f, 0.45f);
        if (GUILayout.Button("Accept", GUILayout.Height(32f)))
        {
            ApplyReviewAndAdvance(level, order, DailyReviewStatus.Approved);
        }

        GUI.backgroundColor = new Color(0.95f, 0.45f, 0.45f);
        if (GUILayout.Button("Reject", GUILayout.Height(32f)))
        {
            ApplyReviewAndAdvance(level, order, DailyReviewStatus.Rejected);
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4f);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Play as Daily", GUILayout.Height(28f)))
        {
            Selection.activeObject = level;
            PlaySelectedAsDaily(level);
        }

        if (GUILayout.Button("Open Asset", GUILayout.Height(28f)))
        {
            Selection.activeObject = level;
            EditorGUIUtility.PingObject(level);
        }

        using (new EditorGUI.DisabledScope(reviewStatus == DailyReviewStatus.Pending))
        {
            if (GUILayout.Button("Reset Decision", GUILayout.Height(28f)))
            {
                ApplyReviewAndAdvance(level, order, DailyReviewStatus.Pending);
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void ApplyReviewAndAdvance(
        LevelData level,
        List<LevelData> orderBefore,
        DailyReviewStatus nextStatus)
    {
        int indexBefore = reviewIndex;
        if (orderBefore != null && level != null)
        {
            for (int i = 0; i < orderBefore.Count; i++)
            {
                if (orderBefore[i] == level)
                {
                    indexBefore = i;
                    break;
                }
            }
        }

        SetReviewStatus(level, nextStatus);
        List<LevelData> orderAfter = BuildGeneratedPlayOrder();
        int stillAt = -1;
        for (int i = 0; i < orderAfter.Count; i++)
        {
            if (orderAfter[i] == level)
            {
                stillAt = i;
                break;
            }
        }

        if (stillAt >= 0)
        {
            reviewIndex = stillAt < orderAfter.Count - 1 ? stillAt + 1 : stillAt;
        }
        else if (orderAfter.Count == 0)
        {
            reviewIndex = 0;
        }
        else
        {
            reviewIndex = Mathf.Clamp(indexBefore, 0, orderAfter.Count - 1);
        }

        Repaint();
    }

    private void CountReview(out int pending, out int approved, out int rejected)
    {
        pending = 0;
        approved = 0;
        rejected = 0;
        for (int i = 0; i < generatedDailyGuids.Length; i++)
        {
            switch (GetReviewStatus(generatedDailyGuids[i]))
            {
                case DailyReviewStatus.Approved:
                    approved++;
                    break;
                case DailyReviewStatus.Rejected:
                    rejected++;
                    break;
                default:
                    pending++;
                    break;
            }
        }
    }

    private void EnsureReviewLoaded()
    {
        if (reviewDecisions != null)
        {
            return;
        }

        reviewDecisions = new Dictionary<string, DailyReviewStatus>(System.StringComparer.Ordinal);
        string raw = EditorPrefs.GetString(ReviewDecisionsPref, string.Empty);
        if (string.IsNullOrEmpty(raw))
        {
            return;
        }

        string[] parts = raw.Split('|');
        for (int i = 0; i < parts.Length; i++)
        {
            int split = parts[i].IndexOf(':');
            if (split <= 0 || split >= parts[i].Length - 1)
            {
                continue;
            }

            string guid = parts[i].Substring(0, split);
            int value;
            if (!int.TryParse(parts[i].Substring(split + 1), out value))
            {
                continue;
            }

            if (value == (int)DailyReviewStatus.Approved ||
                value == (int)DailyReviewStatus.Rejected)
            {
                reviewDecisions[guid] = (DailyReviewStatus)value;
            }
        }
    }

    private DailyReviewStatus GetReviewStatus(string guid)
    {
        EnsureReviewLoaded();
        DailyReviewStatus statusValue;
        if (string.IsNullOrEmpty(guid) ||
            !reviewDecisions.TryGetValue(guid, out statusValue))
        {
            return DailyReviewStatus.Pending;
        }

        return statusValue;
    }

    private void SetReviewStatus(LevelData level, DailyReviewStatus reviewStatus)
    {
        if (level == null)
        {
            return;
        }

        string path = AssetDatabase.GetAssetPath(level);
        if (!IsUnderGeneratedDaily(path))
        {
            return;
        }

        string guid = AssetDatabase.AssetPathToGUID(path);
        if (string.IsNullOrEmpty(guid))
        {
            return;
        }

        EnsureReviewLoaded();
        if (reviewStatus == DailyReviewStatus.Pending)
        {
            reviewDecisions.Remove(guid);
        }
        else
        {
            reviewDecisions[guid] = reviewStatus;
        }

        SaveReviewDecisions();
        Repaint();
    }

    private void SaveReviewDecisions()
    {
        if (reviewDecisions == null || reviewDecisions.Count == 0)
        {
            EditorPrefs.DeleteKey(ReviewDecisionsPref);
            return;
        }

        StringBuilder builder = new StringBuilder();
        foreach (KeyValuePair<string, DailyReviewStatus> pair in reviewDecisions)
        {
            if (builder.Length > 0)
            {
                builder.Append('|');
            }

            builder.Append(pair.Key);
            builder.Append(':');
            builder.Append((int)pair.Value);
        }

        EditorPrefs.SetString(ReviewDecisionsPref, builder.ToString());
    }

    private void DrawPreviewActions()
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Editor preview", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Select a LevelData asset in the Project window. " +
            "Play Selected as Daily is a local Editor preview: Daily HUD, timer, and rules. " +
            "It does not start an attempt, submit a score, or change today's Daily challenge. " +
            "Next and Previous walk only " + GeneratedDailyFolder + " in the current list order.",
            MessageType.Info);

        LevelData selected = Selection.activeObject as LevelData;
        using (new EditorGUI.DisabledScope(selected == null))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ping Selected Asset", GUILayout.Height(24f)))
            {
                EditorGUIUtility.PingObject(selected);
            }

            if (GUILayout.Button("Play Selected Normally", GUILayout.Height(24f)))
            {
                PlaySelectedNormally(selected);
            }

            if (GUILayout.Button("Play Selected as Daily", GUILayout.Height(24f)))
            {
                PlaySelectedAsDaily(selected);
            }

            EditorGUILayout.EndHorizontal();
        }

        bool hasGenerated = generatedDailyGuids.Length > 0;
        using (new EditorGUI.DisabledScope(!hasGenerated))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play Previous Generated Daily", GUILayout.Height(24f)))
            {
                PlayAdjacentGenerated(-1);
            }

            if (GUILayout.Button("Play Next Generated Daily", GUILayout.Height(24f)))
            {
                PlayAdjacentGenerated(1);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.LabelField(
            "Selected",
            selected != null ? selected.name : "none");
    }

    private void PlayAdjacentGenerated(int direction)
    {
        List<LevelData> order = BuildGeneratedPlayOrder();
        if (order.Count == 0)
        {
            Debug.LogWarning(
                "[DailyPreview] No generated Daily levels in " + GeneratedDailyFolder + ".");
            return;
        }

        int index = -1;
        LevelData selected = Selection.activeObject as LevelData;
        if (selected != null)
        {
            for (int i = 0; i < order.Count; i++)
            {
                if (order[i] == selected)
                {
                    index = i;
                    break;
                }
            }
        }

        int next = index < 0
            ? (direction > 0 ? 0 : order.Count - 1)
            : index + direction;
        if (next < 0 || next >= order.Count)
        {
            Debug.LogWarning(
                "[DailyPreview] " +
                (direction < 0 ? "Already at the first" : "Already at the last") +
                " generated Daily level in the current list.");
            return;
        }

        LevelData level = order[next];
        Selection.activeObject = level;
        EditorGUIUtility.PingObject(level);
        PlaySelectedAsDaily(level);
    }

    private List<LevelData> BuildGeneratedPlayOrder()
    {
        List<LevelData> order = new List<LevelData>();
        if (audited && source == AuditSource.GeneratedDailyOnly)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                AuditRow row = rows[i];
                if (row.asset == null || !IsUnderGeneratedDaily(row.path) || !IsRowVisible(row))
                {
                    continue;
                }

                order.Add(row.asset);
            }

            return order;
        }

        List<string> paths = CollectLevelPaths(AuditSource.GeneratedDailyOnly);
        for (int i = 0; i < paths.Count; i++)
        {
            LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(paths[i]);
            if (level != null)
            {
                order.Add(level);
            }
        }

        if (source == AuditSource.GeneratedDailyOnly)
        {
            ApplyReviewStatusFilter(order);
        }

        return order;
    }

    private void ApplyReviewStatusFilter(List<LevelData> order)
    {
        if (reviewListFilter == ReviewListFilter.All || order == null)
        {
            return;
        }

        for (int i = order.Count - 1; i >= 0; i--)
        {
            string path = AssetDatabase.GetAssetPath(order[i]);
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (!MatchesReviewFilter(guid))
            {
                order.RemoveAt(i);
            }
        }
    }

    private static void PlaySelectedNormally(LevelData level)
    {
        DailyChallengeEditorPreview.Clear();
        string error = V1PlaytestSession.LoadLevelForPlaytest(level, out _);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogWarning("[DailyPreview] " + error);
        }
    }

    private void PlaySelectedAsDaily(LevelData level)
    {
        if (level == null)
        {
            return;
        }

        V1PlaytestOverride.Clear();
        DailyChallengeEditorPreview.SetPending(level);
        string error = OpenGameplayScene();
        if (string.IsNullOrEmpty(error))
        {
            MarkTested(level);
            return;
        }

        DailyChallengeEditorPreview.Clear();
        Debug.LogWarning("[DailyPreview] " + error);
    }

    private void EnsureTestedLoaded()
    {
        if (testedGuids != null)
        {
            return;
        }

        testedGuids = new HashSet<string>(System.StringComparer.Ordinal);
        string raw = EditorPrefs.GetString(TestedGuidsPref, string.Empty);
        if (string.IsNullOrEmpty(raw))
        {
            return;
        }

        string[] parts = raw.Split('|');
        for (int i = 0; i < parts.Length; i++)
        {
            if (!string.IsNullOrEmpty(parts[i]))
            {
                testedGuids.Add(parts[i]);
            }
        }
    }

    private bool IsTested(string guid)
    {
        EnsureTestedLoaded();
        return !string.IsNullOrEmpty(guid) && testedGuids.Contains(guid);
    }

    private int CountTestedGenerated()
    {
        EnsureTestedLoaded();
        int tested = 0;
        for (int i = 0; i < generatedDailyGuids.Length; i++)
        {
            if (testedGuids.Contains(generatedDailyGuids[i]))
            {
                tested++;
            }
        }

        return tested;
    }

    private void MarkTested(LevelData level)
    {
        if (level == null)
        {
            return;
        }

        string path = AssetDatabase.GetAssetPath(level);
        if (!IsUnderGeneratedDaily(path))
        {
            return;
        }

        string guid = AssetDatabase.AssetPathToGUID(path);
        if (string.IsNullOrEmpty(guid))
        {
            return;
        }

        EnsureTestedLoaded();
        if (!testedGuids.Add(guid))
        {
            return;
        }

        EditorPrefs.SetString(TestedGuidsPref, string.Join("|", testedGuids));
        RefreshGeneratedDailyGuids();
        Repaint();
    }

    private static string OpenGameplayScene()
    {
        string scenePath = FindGameplayScenePath();
        if (EditorApplication.isPlaying)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                SceneManager.LoadScene(V1PlaytestSession.GameplaySceneName);
            }
            else
            {
                SceneManager.LoadScene(scenePath);
            }

            return string.Empty;
        }

        if (string.IsNullOrEmpty(scenePath))
        {
            return "Gameplay scene not found.";
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return "Scene load cancelled.";
        }

        EditorSceneManager.OpenScene(scenePath);
        EditorApplication.isPlaying = true;
        return string.Empty;
    }

    private static string FindGameplayScenePath()
    {
        string[] guids = AssetDatabase.FindAssets(V1PlaytestSession.GameplaySceneName + " t:Scene");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (path.EndsWith("/" + V1PlaytestSession.GameplaySceneName + ".unity"))
            {
                return path;
            }
        }

        return guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : string.Empty;
    }

    private sealed class ApprovedExportCandidate
    {
        public LevelData level;
        public string path;
        public string guid;
        public string fingerprint;
        public int solverMoves;
        public int gridWidth;
        public int gridHeight;
    }

    private void ExportApprovedDailyLevels()
    {
        List<string> paths = CollectLevelPaths(AuditSource.GeneratedDailyOnly);
        List<ApprovedExportCandidate> approved = new List<ApprovedExportCandidate>();
        for (int i = 0; i < paths.Count; i++)
        {
            string guid = AssetDatabase.AssetPathToGUID(paths[i]);
            if (GetReviewStatus(guid) != DailyReviewStatus.Approved)
            {
                continue;
            }

            LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(paths[i]);
            if (level == null)
            {
                Debug.LogError(
                    "[DailyExport] Aborted. Missing Approved asset " + paths[i] +
                    ". Config and JSON were not changed.");
                return;
            }

            approved.Add(new ApprovedExportCandidate
            {
                level = level,
                path = paths[i],
                guid = guid
            });
        }

        if (approved.Count == 0)
        {
            Debug.LogError(
                "[DailyExport] Aborted. No Approved levels in " + GeneratedDailyFolder +
                ". Config and JSON were not changed.");
            return;
        }

        DailyChallengeConfig config = DailyChallengeConfig.LoadDefault();
        if (config == null)
        {
            Debug.LogError(
                "[DailyExport] Aborted. DailyChallengeConfig was not found. JSON was not changed.");
            return;
        }

        PoolMembership current = LoadMembership(config);
        List<string> collisions = new List<string>();
        if (exportMode == ExportMode.AppendApproved &&
            !string.IsNullOrEmpty(current.backendError))
        {
            Debug.LogError(
                "[DailyExport] Aborted. Backend catalog could not be read: " +
                current.backendError + " Config and JSON were not changed.");
            return;
        }

        if (exportMode == ExportMode.AppendApproved)
        {
            string[] catalogPaths = GetDailyCatalogPaths();
            for (int p = 1; p < catalogPaths.Length; p++)
            {
                CatalogDocument extra;
                string extraError;
                if (!TryReadCatalog(catalogPaths[p], out extra, out extraError))
                {
                    Debug.LogError(
                        "[DailyExport] Aborted. " + extraError +
                        " Config and JSON were not changed.");
                    return;
                }

                for (int i = 0; i < approved.Count; i++)
                {
                    if (FindCatalogMatch(extra.rows, approved[i].guid, approved[i].level.name) ==
                        CatalogMatch.Collision)
                    {
                        collisions.Add(
                            approved[i].path +
                            " — levelId " + approved[i].level.name +
                            " is already in " + catalogPaths[p] + " with a different assetGuid.");
                    }
                }
            }
        }

        List<ApprovedExportCandidate> toValidate = new List<ApprovedExportCandidate>();
        int addLocal = 0;
        int addBackend = 0;
        for (int i = 0; i < approved.Count; i++)
        {
            ApprovedExportCandidate candidate = approved[i];
            string collision = DescribeIdentityCollision(current, candidate.guid, candidate.level.name);
            if (!string.IsNullOrEmpty(collision))
            {
                collisions.Add(candidate.path + " — " + collision);
                continue;
            }

            bool inLocal = IsInLocalPool(current, candidate.guid, candidate.level.name);
            bool inBackend = IsInBackend(current, candidate.guid, candidate.level.name);
            if (exportMode == ExportMode.ReplacePool)
            {
                toValidate.Add(candidate);
            }
            else if (!inLocal || !inBackend)
            {
                toValidate.Add(candidate);
                if (!inLocal)
                {
                    addLocal++;
                }

                if (!inBackend)
                {
                    addBackend++;
                }
            }
        }

        if (collisions.Count > 0)
        {
            Debug.LogError(
                "[DailyExport] Aborted. Identity collision. Config and JSON were not changed.\n" +
                string.Join("\n", collisions));
            return;
        }

        if (exportMode == ExportMode.AppendApproved && toValidate.Count == 0)
        {
            Debug.Log(
                "[DailyExport] Nothing to add. Every Approved level is already in the local pool and backend catalog.");
            return;
        }

        List<string> failures = new List<string>();
        HashSet<string> seenFingerprints = new HashSet<string>(System.StringComparer.Ordinal);
        HashSet<string> seenNames = new HashSet<string>(System.StringComparer.Ordinal);
        if (exportMode == ExportMode.AppendApproved)
        {
            for (int i = 0; i < current.poolFingerprints.Count; i++)
            {
                if (!string.IsNullOrEmpty(current.poolFingerprints[i]))
                {
                    seenFingerprints.Add(current.poolFingerprints[i]);
                }
            }

            for (int i = 0; i < current.backend.Count; i++)
            {
                if (!string.IsNullOrEmpty(current.backend[i].fingerprint))
                {
                    seenFingerprints.Add(current.backend[i].fingerprint);
                }
            }
        }

        try
        {
            for (int i = 0; i < toValidate.Count; i++)
            {
                ApprovedExportCandidate pending = toValidate[i];
                if (EditorUtility.DisplayCancelableProgressBar(
                        "Export Approved Daily Levels",
                        pending.path,
                        toValidate.Count == 0 ? 1f : (float)i / toValidate.Count))
                {
                    Debug.LogWarning("[DailyExport] Cancelled. Config and JSON were not changed.");
                    return;
                }

                if (exportMode == ExportMode.AppendApproved)
                {
                    ForgetOwnFingerprint(current, pending, seenFingerprints);
                }

                string failure = TryValidateApprovedCandidate(
                    pending.path,
                    pending.guid,
                    seenFingerprints,
                    seenNames,
                    out ApprovedExportCandidate validated);
                if (failure != null)
                {
                    failures.Add(failure);
                    continue;
                }

                pending.level = validated.level;
                pending.fingerprint = validated.fingerprint;
                pending.solverMoves = validated.solverMoves;
                pending.gridWidth = validated.gridWidth;
                pending.gridHeight = validated.gridHeight;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        if (failures.Count > 0)
        {
            Debug.LogError(
                "[DailyExport] Aborted. " + failures.Count +
                " level(s) failed validation. Config and JSON were not changed.\n" +
                string.Join("\n", failures));
            return;
        }

        string oldNames = FormatPoolNames(config);
        int oldLocal = config.PoolCount;
        int oldBackend = current.backendCount;
        bool append = exportMode == ExportMode.AppendApproved;
        string dialog = append
            ? "Add Approved levels to the existing Daily pool.\n\n" +
              "Existing local pool: " + oldLocal + "\n" +
              "Existing backend catalog: " + oldBackend + "\n" +
              "New approved to add: " + toValidate.Count + "\n" +
              "Result local pool: " + (oldLocal + addLocal) + "\n" +
              "Result backend catalog: " + (oldBackend + addBackend) + "\n\n" +
              "Existing entries stay. Pending and Rejected are not added.\n" +
              "Today's Firestore challenge is not changed. Firebase is not deployed."
            : "REPLACE the Daily pool with the current Approved set.\n\n" +
              "Existing local pool: " + oldLocal + "\n" +
              oldNames + "\n\n" +
              "Existing backend catalog: " + oldBackend + "\n" +
              "Replacement approved count: " + approved.Count + "\n\n" +
              "Every current pool entry and catalog entry that is not in this Approved set will be removed.\n" +
              "Today's Firestore challenge is not changed. Firebase is not deployed.";
        bool confirmed = EditorUtility.DisplayDialog(
            append ? "Add Approved To Daily Pool" : "Replace Daily Pool With Approved",
            dialog,
            append ? "Add to pool" : "Replace pool",
            "Cancel");
        if (!confirmed)
        {
            Debug.Log("[DailyExport] Cancelled. Config and JSON were not changed.");
            return;
        }

        string[] jsonPaths = GetDailyCatalogPaths();
        string backupStamp = System.DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var backups = new List<string>();
        var writtenJson = new List<string>();
        if (append)
        {
            for (int i = 0; i < jsonPaths.Length; i++)
            {
                CatalogDocument document;
                string readError;
                if (!TryReadCatalog(jsonPaths[i], out document, out readError))
                {
                    Debug.LogError(
                        "[DailyExport] Aborted before writing. " + readError +
                        " Config and JSON were not changed.");
                    return;
                }

                List<ApprovedExportCandidate> missing = new List<ApprovedExportCandidate>();
                for (int c = 0; c < toValidate.Count; c++)
                {
                    if (FindCatalogMatch(document.rows, toValidate[c].guid, toValidate[c].level.name) !=
                        CatalogMatch.Same)
                    {
                        missing.Add(toValidate[c]);
                    }
                }

                string updated = SpliceCatalogEntries(document, missing);
                writtenJson.Add(updated);
            }
        }
        else
        {
            string json = BuildApprovedCatalogJson(config, approved);
            for (int i = 0; i < jsonPaths.Length; i++)
            {
                writtenJson.Add(json);
            }
        }

        for (int i = 0; i < jsonPaths.Length; i++)
        {
            if (!File.Exists(jsonPaths[i]))
            {
                continue;
            }

            string backupPath = jsonPaths[i] + ".bak-" + backupStamp;
            File.Copy(jsonPaths[i], backupPath, false);
            backups.Add(backupPath);
        }

        for (int i = 0; i < jsonPaths.Length; i++)
        {
            string directory = Path.GetDirectoryName(jsonPaths[i]);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(jsonPaths[i], writtenJson[i]);
        }

        if (append)
        {
            AppendDailyPool(config, toValidate);
        }
        else
        {
            ReplaceDailyPool(config, approved);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        membershipLoaded = false;

        Debug.Log(
            "[DailyExport] " + (append ? "Appended" : "Replaced") + " Daily pool.\n" +
            "Old local pool count: " + oldLocal +
            "\nOld local pool: " + oldNames +
            "\nOld backend catalog count: " + oldBackend +
            "\nNew local pool count: " + config.PoolCount +
            "\nNew backend catalog count: " + (append ? oldBackend + addBackend : approved.Count) +
            "\nAdded levelIds: " + FormatExportNames(toValidate) +
            "\nConfig: Assets/Resources/DailyChallengeConfig.asset" +
            "\nJSON:\n" + string.Join("\n", jsonPaths) +
            "\nBackups:\n" + (backups.Count == 0 ? "(none, file was missing)" : string.Join("\n", backups)) +
            "\nFirebase was not deployed. Today's Firestore challenge was not modified.");
    }

    private static string TryValidateApprovedCandidate(
        string path,
        string guid,
        HashSet<string> seenFingerprints,
        HashSet<string> seenNames,
        out ApprovedExportCandidate candidate)
    {
        candidate = null;
        LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        if (level == null)
        {
            return path + " — missing LevelData asset.";
        }

        if (!IsUnderGeneratedDaily(path))
        {
            return path + " — not under " + GeneratedDailyFolder + ".";
        }

        if (level.objectiveType != LevelObjectiveType.Classic)
        {
            return path + " — objective is " + level.objectiveType + ", not Classic.";
        }

        string validationError = LevelSolver.ValidateLevelData(level);
        if (!string.IsNullOrEmpty(validationError))
        {
            return path + " — " + validationError;
        }

        LevelSolver.SolverResult solved = LevelSolver.Solve(level);
        if (solved.invalid)
        {
            return path + " — " + (solved.validationError ?? "invalid");
        }

        if (solved.searchLimitReached || !solved.solvable)
        {
            return path + " — " +
                   (solved.searchLimitReached
                       ? "search limit: " + (solved.searchLimitReason ?? "unknown")
                       : "unsolvable");
        }

        if (solved.minimumMoves < MinimumMovesMin || solved.minimumMoves > MinimumMovesMax)
        {
            return path + " — solver minimumMoves " + solved.minimumMoves +
                   " is outside " + MinimumMovesMin + "–" + MinimumMovesMax + ".";
        }

        if (level.minimumMoves != solved.minimumMoves)
        {
            return path + " — stored minimumMoves " + level.minimumMoves +
                   " does not match solver " + solved.minimumMoves + ".";
        }

        string fingerprint = LevelCanonicalKey.BuildCanonicalLevelKey(level);
        if (string.IsNullOrEmpty(fingerprint))
        {
            return path + " — fingerprint is empty.";
        }

        if (!seenFingerprints.Add(fingerprint))
        {
            return path + " — exact duplicate fingerprint " + fingerprint + ".";
        }

        if (string.IsNullOrEmpty(level.name) || !seenNames.Add(level.name))
        {
            return path + " — duplicate or empty levelId/asset name.";
        }

        if (string.IsNullOrEmpty(guid))
        {
            return path + " — missing asset GUID.";
        }

        candidate = new ApprovedExportCandidate
        {
            level = level,
            path = path,
            guid = guid,
            fingerprint = fingerprint,
            solverMoves = solved.minimumMoves,
            gridWidth = level.ResolvedGridWidth,
            gridHeight = level.ResolvedGridHeight
        };
        return null;
    }

    private static void ReplaceDailyPool(
        DailyChallengeConfig config,
        List<ApprovedExportCandidate> approved)
    {
        SerializedObject serialized = new SerializedObject(config);
        SerializedProperty pool = serialized.FindProperty("dailyLevelPool");
        pool.ClearArray();
        for (int i = 0; i < approved.Count; i++)
        {
            pool.InsertArrayElementAtIndex(i);
            pool.GetArrayElementAtIndex(i).objectReferenceValue = approved[i].level;
        }

        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(config);
    }

    private static void AppendDailyPool(
        DailyChallengeConfig config,
        List<ApprovedExportCandidate> additions)
    {
        SerializedObject serialized = new SerializedObject(config);
        SerializedProperty pool = serialized.FindProperty("dailyLevelPool");
        for (int i = 0; i < additions.Count; i++)
        {
            bool present = false;
            for (int p = 0; p < pool.arraySize; p++)
            {
                LevelData existing = pool.GetArrayElementAtIndex(p).objectReferenceValue as LevelData;
                if (existing == null)
                {
                    continue;
                }

                if (existing == additions[i].level || existing.name == additions[i].level.name)
                {
                    present = true;
                    break;
                }

                string existingGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(existing));
                if (existingGuid == additions[i].guid)
                {
                    present = true;
                    break;
                }
            }

            if (present)
            {
                continue;
            }

            int index = pool.arraySize;
            pool.InsertArrayElementAtIndex(index);
            pool.GetArrayElementAtIndex(index).objectReferenceValue = additions[i].level;
        }

        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(config);
    }

    private void DrawExportPreview()
    {
        DailyChallengeConfig config = DailyChallengeConfig.LoadDefault();
        PoolMembership current = LoadMembership(config);
        int localCount = config != null ? config.PoolCount : 0;
        int backendCount = current.backendCount;
        int newLevels = 0;
        int addLocal = 0;
        int addBackend = 0;
        if (string.IsNullOrEmpty(current.backendError))
        {
            for (int i = 0; i < generatedDailyGuids.Length; i++)
            {
                string guid = generatedDailyGuids[i];
                if (GetReviewStatus(guid) != DailyReviewStatus.Approved)
                {
                    continue;
                }

                string path = AssetDatabase.GUIDToAssetPath(guid);
                string levelId = Path.GetFileNameWithoutExtension(path);
                if (!string.IsNullOrEmpty(DescribeIdentityCollision(current, guid, levelId)))
                {
                    continue;
                }

                bool inLocal = IsInLocalPool(current, guid, levelId);
                bool inBackend = IsInBackend(current, guid, levelId);
                if (exportMode == ExportMode.ReplacePool)
                {
                    continue;
                }

                if (!inLocal || !inBackend)
                {
                    newLevels++;
                }

                if (!inLocal)
                {
                    addLocal++;
                }

                if (!inBackend)
                {
                    addBackend++;
                }
            }
        }

        if (exportMode == ExportMode.AppendApproved)
        {
            EditorGUILayout.LabelField(
                "Existing local pool: " + localCount +
                "    Existing backend catalog: " + backendCount);
            EditorGUILayout.LabelField(
                "New approved to add: " + newLevels +
                "    Result local pool: " + (localCount + addLocal) +
                "    Result backend catalog: " + (backendCount + addBackend));
        }
        else
        {
            CountReview(out _, out int approvedCount, out _);
            EditorGUILayout.LabelField(
                "Existing local pool: " + localCount +
                "    Existing backend catalog: " + backendCount +
                "    Replacement approved count: " + approvedCount);
        }
    }

    private void CountExportPresence(
        out int notExported,
        out int inPool,
        out int mismatch,
        out string catalogError)
    {
        notExported = 0;
        inPool = 0;
        mismatch = 0;
        PoolMembership current = LoadMembership(DailyChallengeConfig.LoadDefault());
        catalogError = current.backendError;
        if (!string.IsNullOrEmpty(catalogError))
        {
            return;
        }

        for (int i = 0; i < generatedDailyGuids.Length; i++)
        {
            string guid = generatedDailyGuids[i];
            if (GetReviewStatus(guid) != DailyReviewStatus.Approved)
            {
                continue;
            }

            string levelId = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid));
            switch (GetPoolPresence(guid, levelId))
            {
                case PoolPresence.InPool:
                    inPool++;
                    break;
                case PoolPresence.Mismatch:
                    mismatch++;
                    break;
                default:
                    notExported++;
                    break;
            }
        }
    }

    private PoolPresence GetPoolPresence(string guid, string levelId)
    {
        PoolMembership current = LoadMembership(DailyChallengeConfig.LoadDefault());
        if (!string.IsNullOrEmpty(current.backendError))
        {
            return PoolPresence.Mismatch;
        }

        if (!string.IsNullOrEmpty(DescribeIdentityCollision(current, guid, levelId)))
        {
            return PoolPresence.Mismatch;
        }

        bool inLocal = IsInLocalPool(current, guid, levelId);
        bool inBackend = IsInBackend(current, guid, levelId);
        if (inLocal && inBackend)
        {
            return PoolPresence.InPool;
        }

        if (inLocal || inBackend)
        {
            return PoolPresence.Mismatch;
        }

        return PoolPresence.NotExported;
    }

    private static string FormatPresence(PoolPresence presence)
    {
        switch (presence)
        {
            case PoolPresence.InPool:
                return "Approved — Already In Pool";
            case PoolPresence.Mismatch:
                return "Export Mismatch";
            default:
                return "Approved — Not Exported";
        }
    }

    private PoolMembership LoadMembership(DailyChallengeConfig config)
    {
        if (membershipLoaded && membership != null)
        {
            return membership;
        }

        membership = new PoolMembership();
        membershipLoaded = true;
        if (config != null)
        {
            membership.localCount = config.PoolCount;
            for (int i = 0; i < config.PoolCount; i++)
            {
                LevelData level = config.GetLevelAt(i);
                if (level == null)
                {
                    membership.poolGuids.Add(string.Empty);
                    membership.poolNames.Add(string.Empty);
                    membership.poolFingerprints.Add(string.Empty);
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(level);
                membership.poolGuids.Add(AssetDatabase.AssetPathToGUID(path));
                membership.poolNames.Add(level.name);
                membership.poolFingerprints.Add(LevelCanonicalKey.BuildCanonicalLevelKey(level));
            }
        }

        string[] jsonPaths = GetDailyCatalogPaths();
        CatalogDocument document;
        string error;
        if (!TryReadCatalog(jsonPaths[0], out document, out error))
        {
            membership.backendError = error;
            return membership;
        }

        membership.backend = document.rows;
        membership.backendCount = document.rows.Count;
        return membership;
    }

    private static bool IsInLocalPool(PoolMembership current, string guid, string levelId)
    {
        for (int i = 0; i < current.poolGuids.Count; i++)
        {
            if (!string.IsNullOrEmpty(guid) && current.poolGuids[i] == guid)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(levelId) && current.poolNames[i] == levelId)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInBackend(PoolMembership current, string guid, string levelId)
    {
        return FindCatalogMatch(current.backend, guid, levelId) == CatalogMatch.Same;
    }

    private static string DescribeIdentityCollision(PoolMembership current, string guid, string levelId)
    {
        for (int i = 0; i < current.poolGuids.Count; i++)
        {
            bool sameGuid = !string.IsNullOrEmpty(guid) && current.poolGuids[i] == guid;
            bool sameName = !string.IsNullOrEmpty(levelId) && current.poolNames[i] == levelId;
            if (sameName && !sameGuid && !string.IsNullOrEmpty(current.poolGuids[i]))
            {
                return "levelId " + levelId + " is already in the local pool with assetGuid " +
                       current.poolGuids[i] + ".";
            }
        }

        if (FindCatalogMatch(current.backend, guid, levelId) == CatalogMatch.Collision)
        {
            return "levelId " + levelId +
                   " is already in the backend catalog with a different assetGuid.";
        }

        return null;
    }

    private static void ForgetOwnFingerprint(
        PoolMembership current,
        ApprovedExportCandidate candidate,
        HashSet<string> seenFingerprints)
    {
        for (int i = 0; i < current.poolGuids.Count; i++)
        {
            if (current.poolGuids[i] == candidate.guid &&
                !string.IsNullOrEmpty(current.poolFingerprints[i]))
            {
                seenFingerprints.Remove(current.poolFingerprints[i]);
            }
        }

        for (int i = 0; i < current.backend.Count; i++)
        {
            CatalogRow row = current.backend[i];
            bool same = row.assetGuid == candidate.guid ||
                        (row.levelId == candidate.level.name &&
                         (string.IsNullOrEmpty(row.assetGuid) || row.assetGuid == candidate.guid));
            if (same && !string.IsNullOrEmpty(row.fingerprint))
            {
                seenFingerprints.Remove(row.fingerprint);
            }
        }
    }

    private enum CatalogMatch
    {
        None = 0,
        Same,
        Collision
    }

    private sealed class CatalogRow
    {
        public string levelId;
        public string assetGuid;
        public string fingerprint;
    }

    private sealed class CatalogDocument
    {
        public string text;
        public int arrayEnd;
        public readonly List<CatalogRow> rows = new List<CatalogRow>();
    }

    private sealed class PoolMembership
    {
        public readonly List<string> poolGuids = new List<string>();
        public readonly List<string> poolNames = new List<string>();
        public readonly List<string> poolFingerprints = new List<string>();
        public List<CatalogRow> backend = new List<CatalogRow>();
        public int localCount;
        public int backendCount;
        public string backendError;
    }

    private static CatalogMatch FindCatalogMatch(List<CatalogRow> rows, string guid, string levelId)
    {
        CatalogRow byGuid = null;
        CatalogRow byId = null;
        if (rows == null)
        {
            return CatalogMatch.None;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            CatalogRow row = rows[i];
            if (!string.IsNullOrEmpty(guid) && row.assetGuid == guid)
            {
                byGuid = row;
            }

            if (!string.IsNullOrEmpty(levelId) && row.levelId == levelId)
            {
                byId = row;
            }
        }

        if (byId != null && !string.IsNullOrEmpty(byId.assetGuid) && byId.assetGuid != guid)
        {
            return CatalogMatch.Collision;
        }

        if (byGuid != null && byId != null && !string.Equals(byGuid.levelId, byId.levelId, System.StringComparison.Ordinal))
        {
            return CatalogMatch.Collision;
        }

        if (byGuid != null || byId != null)
        {
            return CatalogMatch.Same;
        }

        return CatalogMatch.None;
    }

    private static bool TryReadCatalog(string path, out CatalogDocument document, out string error)
    {
        document = null;
        error = null;
        if (!File.Exists(path))
        {
            error = "Missing catalog " + path;
            return false;
        }

        string text = File.ReadAllText(path);
        int levelsKey = text.IndexOf("\"levels\"", System.StringComparison.Ordinal);
        if (levelsKey < 0)
        {
            error = "Catalog has no levels array: " + path;
            return false;
        }

        int arrayStart = text.IndexOf('[', levelsKey);
        if (arrayStart < 0)
        {
            error = "Catalog levels array is missing: " + path;
            return false;
        }

        int arrayEnd = FindMatching(text, arrayStart, '[', ']');
        if (arrayEnd < 0)
        {
            error = "Catalog levels array is not closed: " + path;
            return false;
        }

        document = new CatalogDocument
        {
            text = text,
            arrayEnd = arrayEnd
        };
        int cursor = arrayStart + 1;
        while (cursor < arrayEnd)
        {
            int objectStart = text.IndexOf('{', cursor);
            if (objectStart < 0 || objectStart > arrayEnd)
            {
                break;
            }

            int objectEnd = FindMatching(text, objectStart, '{', '}');
            if (objectEnd < 0 || objectEnd > arrayEnd)
            {
                error = "Catalog level object is not closed: " + path;
                document = null;
                return false;
            }

            string raw = text.Substring(objectStart, objectEnd - objectStart + 1);
            document.rows.Add(new CatalogRow
            {
                levelId = ExtractJsonString(raw, "levelId"),
                assetGuid = ExtractJsonString(raw, "assetGuid"),
                fingerprint = ExtractJsonString(raw, "fingerprint")
            });
            cursor = objectEnd + 1;
        }

        return true;
    }

    private static string SpliceCatalogEntries(
        CatalogDocument document,
        List<ApprovedExportCandidate> additions)
    {
        if (additions == null || additions.Count == 0)
        {
            return document.text;
        }

        StringBuilder inserted = new StringBuilder();
        bool arrayEmpty = true;
        for (int i = document.arrayEnd - 1; i > 0; i--)
        {
            char c = document.text[i];
            if (char.IsWhiteSpace(c))
            {
                continue;
            }

            arrayEmpty = c == '[';
            break;
        }

        for (int i = 0; i < additions.Count; i++)
        {
            if (!arrayEmpty || i > 0)
            {
                inserted.Append(",\n");
            }
            else
            {
                inserted.Append("\n");
            }

            ApprovedExportCandidate entry = additions[i];
            inserted.Append("    { ");
            inserted.Append("\"levelId\": \"").Append(JsonEscape(entry.level.name)).Append("\", ");
            inserted.Append("\"levelVersion\": \"").Append(JsonEscape(entry.level.name)).Append("\", ");
            inserted.Append("\"minimumMoves\": ").Append(entry.solverMoves).Append(", ");
            inserted.Append("\"assetGuid\": \"").Append(JsonEscape(entry.guid)).Append("\", ");
            inserted.Append("\"gridWidth\": ").Append(entry.gridWidth).Append(", ");
            inserted.Append("\"gridHeight\": ").Append(entry.gridHeight).Append(", ");
            inserted.Append("\"fingerprint\": \"").Append(JsonEscape(entry.fingerprint)).Append("\", ");
            inserted.Append("\"enabled\": true }");
        }

        if (!arrayEmpty)
        {
            inserted.Append('\n');
        }

        return document.text.Substring(0, document.arrayEnd) +
               inserted +
               document.text.Substring(document.arrayEnd);
    }

    private static int FindMatching(string text, int openIndex, char open, char close)
    {
        int depth = 0;
        bool inString = false;
        bool escaped = false;
        for (int i = openIndex; i < text.Length; i++)
        {
            char c = text[i];
            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (c == '\\')
                {
                    escaped = true;
                }
                else if (c == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (c == '"')
            {
                inString = true;
                continue;
            }

            if (c == open)
            {
                depth++;
            }
            else if (c == close)
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static string ExtractJsonString(string json, string key)
    {
        string token = "\"" + key + "\"";
        int keyIndex = json.IndexOf(token, System.StringComparison.Ordinal);
        if (keyIndex < 0)
        {
            return string.Empty;
        }

        int colon = json.IndexOf(':', keyIndex + token.Length);
        if (colon < 0)
        {
            return string.Empty;
        }

        int start = json.IndexOf('"', colon + 1);
        if (start < 0)
        {
            return string.Empty;
        }

        StringBuilder value = new StringBuilder();
        bool escaped = false;
        for (int i = start + 1; i < json.Length; i++)
        {
            char c = json[i];
            if (escaped)
            {
                value.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (c == '"')
            {
                break;
            }

            value.Append(c);
        }

        return value.ToString();
    }

    private static string BuildApprovedCatalogJson(
        DailyChallengeConfig config,
        List<ApprovedExportCandidate> approved)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{\n");
        sb.Append("  \"scoreVersion\": ").Append(DailyChallengeScoreVersion.Current).Append(",\n");
        sb.Append("  \"catalogVersion\": 1,\n");
        sb.Append("  \"scoring\": {\n");
        sb.Append("    \"baseMoveScore\": ").Append(config.BaseMoveScore).Append(",\n");
        sb.Append("    \"movePenaltyPerExtraMove\": ").Append(config.MovePenaltyPerExtraMove).Append(",\n");
        sb.Append("    \"minimumMoveScore\": ").Append(config.MinimumMoveScore).Append(",\n");
        sb.Append("    \"timeBonusMax\": ").Append(config.TimeBonusMax).Append(",\n");
        sb.Append("    \"timeReferenceSeconds\": ")
            .Append(config.TimeReferenceSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Append("\n");
        sb.Append("  },\n");
        sb.Append("  \"validation\": {\n");
        sb.Append("    \"minMoves\": 1,\n");
        sb.Append("    \"maxMoves\": 5000,\n");
        sb.Append("    \"minCompletionTimeMs\": 500,\n");
        sb.Append("    \"maxCompletionTimeMs\": 21600000\n");
        sb.Append("  },\n");
        sb.Append("  \"levels\": [\n");
        for (int i = 0; i < approved.Count; i++)
        {
            ApprovedExportCandidate entry = approved[i];
            if (i > 0)
            {
                sb.Append(",\n");
            }

            sb.Append("    { ");
            sb.Append("\"levelId\": \"").Append(JsonEscape(entry.level.name)).Append("\", ");
            sb.Append("\"levelVersion\": \"").Append(JsonEscape(entry.level.name)).Append("\", ");
            sb.Append("\"minimumMoves\": ").Append(entry.solverMoves).Append(", ");
            sb.Append("\"assetGuid\": \"").Append(JsonEscape(entry.guid)).Append("\", ");
            sb.Append("\"gridWidth\": ").Append(entry.gridWidth).Append(", ");
            sb.Append("\"gridHeight\": ").Append(entry.gridHeight).Append(", ");
            sb.Append("\"fingerprint\": \"").Append(JsonEscape(entry.fingerprint)).Append("\", ");
            sb.Append("\"enabled\": true }");
        }

        sb.Append("\n  ],\n");
        sb.Append(
            "  \"notes\": \"Approved generated Daily candidates only. " +
            "levelId matches the Unity asset name. assetGuid is the durable identity. " +
            "Deploy functions/config/daily-levels.json.\"\n");
        sb.Append("}\n");
        return sb.ToString();
    }

    private static string[] GetDailyCatalogPaths()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        return new[]
        {
            Path.Combine(
                projectRoot, "Backend", "DailyChallenge", "functions", "config", "daily-levels.json"),
            Path.Combine(
                projectRoot, "Backend", "DailyChallenge", "config", "daily-levels.json")
        };
    }

    private static string FormatPoolNames(DailyChallengeConfig config)
    {
        if (config == null || config.PoolCount == 0)
        {
            return "(empty)";
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < config.PoolCount; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            LevelData level = config.GetLevelAt(i);
            sb.Append(level != null ? level.name : "null");
        }

        return sb.ToString();
    }

    private static string FormatExportNames(List<ApprovedExportCandidate> approved)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < approved.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(approved[i].level.name);
        }

        return sb.ToString();
    }
}

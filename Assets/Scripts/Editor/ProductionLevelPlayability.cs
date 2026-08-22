using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Production playability: same RushOutSolver + objective constraints as gameplay hints.
/// Editor validation marks statuses; never auto-writes LevelData.
/// </summary>
public static class ProductionLevelPlayability
{
    public const int DefaultMaxStates = 200000;

    /// <summary>Deep pass only — does not change normal validation defaults.</summary>
    public const int DeepMaxStatesMultiplier = 10;

    public const int DeepMaxStates = DefaultMaxStates * DeepMaxStatesMultiplier;

    public enum Status
    {
        Valid = 0,
        Unsolvable = 1,
        ObjectiveInvalid = 2,
        SolverTimeout = 3,
        ConfigInvalid = 4,
        Unknown = 5
    }

    public struct Report
    {
        public LevelData level;
        public int dbIndex;
        public Status status;
        public string detail;
        public int minimumMoves;
        public int solutionLength;
        public int statesExplored;
        public int discoveredStates;
        public int fragileMovesUsed;
        public int limitedMovesUsed;
        public bool classicSolvable;
        public bool objectiveSolvable;
        public RushOutSolver.SolverResult solverResult;
        public double elapsedSeconds;
        public int vehicleCount;
        public int gridWidth;
        public int gridHeight;
        public int targetCount;
        public float branchingEstimate;
        public string searchLimitKind;
        public bool usedDeepBudget;
    }

    public struct DeepTimeoutResult
    {
        public Report normalReport;
        public Report deepReport;
        public bool hintPathPass;
        public string hintPathDetail;
        public bool releaseSafe;
        public bool releaseBlocker;
        public string recommendedAction;
        public string summaryLine;
    }

    /// <summary>
    /// Build SolveConstraints from LevelData special objective fields.
    /// </summary>
    public static RushOutSolver.SolveConstraints BuildConstraints(LevelData level)
    {
        return ObjectiveSolveConstraints.FromLevelData(level);
    }

    public static Report Validate(
        LevelData level,
        int dbIndex = -1,
        int maxStates = DefaultMaxStates)
    {
        Report report = new Report
        {
            level = level,
            dbIndex = dbIndex,
            status = Status.Unknown,
            detail = string.Empty,
            minimumMoves = -1,
            solutionLength = 0,
            statesExplored = 0,
            usedDeepBudget = maxStates > DefaultMaxStates
        };

        if (level == null)
        {
            report.status = Status.ConfigInvalid;
            report.detail = "LevelDataNull";
            return report;
        }

        FillLevelDiagnostics(ref report, level);

        string configError = ObjectiveConfigValidation.ValidateSpecialObjective(level);
        if (configError != null)
        {
            report.status = Status.ConfigInvalid;
            report.detail = configError;
            return report;
        }

        // Layout sanity (MultiTarget allowed to have 2+ targets).
        string layoutError = ValidateLayoutForPlayability(level);
        if (layoutError != null)
        {
            report.status = Status.ConfigInvalid;
            report.detail = layoutError;
            return report;
        }

        RushOutSolver.SolveConstraints constraints = BuildConstraints(level);
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        RushOutSolver.SolverResult result = RushOutSolver.SolveLevelData(
            level,
            maxStates,
            -1,
            constraints
        );
        sw.Stop();
        report.elapsedSeconds = sw.Elapsed.TotalSeconds;
        report.solverResult = result;
        report.statesExplored = result.statesExplored;
        report.discoveredStates = result.discoveredStates;
        report.fragileMovesUsed = result.fragileMovesUsed;
        report.limitedMovesUsed = result.limitedMovesUsed;
        report.branchingEstimate = EstimateBranching(result);
        report.searchLimitKind = result.searchLimitReached
            ? (result.searchLimitReason ?? "search limit")
            : string.Empty;

        if (result.searchLimitReached)
        {
            report.status = Status.SolverTimeout;
            report.detail = result.searchLimitReason ?? "search limit";
            report.objectiveSolvable = false;
            return report;
        }

        if (!result.solvable ||
            result.solution == null ||
            result.solution.Count == 0)
        {
            // Distinguish classic-only vs objective failure when objective is special.
            if (level.objectiveType != LevelObjectiveType.Classic &&
                level.objectiveType != LevelObjectiveType.TimedAmbulance)
            {
                RushOutSolver.SolverResult classic = RushOutSolver.SolveLevelData(
                    level,
                    maxStates,
                    -1,
                    RushOutSolver.SolveConstraints.Classic()
                );
                report.classicSolvable = classic.solvable && !classic.searchLimitReached;
                if (report.classicSolvable)
                {
                    report.status = Status.ObjectiveInvalid;
                    report.detail = "Classic solvable but objective constraints fail";
                    report.minimumMoves = classic.minimumMoves;
                    return report;
                }
            }

            report.status = Status.Unsolvable;
            report.detail = "No complete solution";
            report.objectiveSolvable = false;
            return report;
        }

        report.classicSolvable = true;
        report.objectiveSolvable = true;
        report.minimumMoves = result.minimumMoves;
        report.solutionLength = result.solution.Count;
        report.status = Status.Valid;
        report.detail = "OK";
        return report;
    }

    private static void FillLevelDiagnostics(ref Report report, LevelData level)
    {
        report.gridWidth = level.ResolvedGridWidth;
        report.gridHeight = level.ResolvedGridHeight;
        report.vehicleCount = level.vehicles != null ? level.vehicles.Count : 0;
        report.targetCount = 0;
        if (level.vehicles == null)
        {
            return;
        }

        for (int i = 0; i < level.vehicles.Count; i++)
        {
            if (level.vehicles[i] != null && level.vehicles[i].canExitRight)
            {
                report.targetCount++;
            }
        }
    }

    private static float EstimateBranching(RushOutSolver.SolverResult result)
    {
        if (result == null || result.statesExplored <= 0)
        {
            return 0f;
        }

        return result.generatedMoves / (float)result.statesExplored;
    }

    /// <summary>
    /// Replay hint path: always take first solution move from current state until win or loop.
    /// </summary>
    public static bool VerifyHintPath(
        LevelData level,
        out string detail,
        int maxStates = DefaultMaxStates,
        int maxSteps = 256)
    {
        detail = string.Empty;
        if (level == null)
        {
            detail = "null level";
            return false;
        }

        RushOutSolver.BuildFromLevelData(
            level,
            out List<RushOutSolver.VehicleDefinition> vehicles,
            out RushOutSolver.BoardState state
        );
        RushOutSolver.SolveConstraints constraints = BuildConstraints(level);
        HashSet<int> seen = new HashSet<int>();
        int steps = 0;

        while (steps < maxSteps)
        {
            int hash = state.GetHashCode();
            if (seen.Contains(hash))
            {
                detail =
                    "HintLoopDetected at step " + steps +
                    " (repeated board hash=" + hash +
                    "; earlier occurrence in this hint-follow sequence)";
                return false;
            }

            seen.Add(hash);

            RushOutSolver.SolverResult result = RushOutSolver.Solve(
                vehicles,
                state,
                level.exitRow,
                level.ResolvedGridWidth,
                level.ResolvedGridHeight,
                maxStates,
                -1,
                constraints
            );

            if (result.searchLimitReached)
            {
                detail = "SOLVER TIMEOUT at step " + steps;
                return false;
            }

            if (!result.solvable || result.solution == null || result.solution.Count == 0)
            {
                detail = "Unsolvable at step " + steps;
                return false;
            }

            // Already won: only exit move(s) left / empty board targets.
            if (IsWinSolution(result))
            {
                // Apply remaining exit-only moves if any non-exit left — first move may be exit.
            }

            RushOutSolver.SolverMove move = result.solution[0];
            if (move.exitsBoard)
            {
                // Single-target: exit ends. Multi: apply exit and continue.
                if (!constraints.multiTarget)
                {
                    detail = "Reached win in " + (steps + 1) + " hint steps";
                    return true;
                }

                state = state.WithVehicleExited(move.vehicleIndex);
                if (AllTargetsExited(vehicles, state, constraints))
                {
                    detail = "Reached multi-target win in " + (steps + 1) + " hint steps";
                    return true;
                }

                steps++;
                continue;
            }

            state = state.WithMovedVehicle(
                move.vehicleIndex,
                move.toPosition,
                constraints.fragileVehicleIndex,
                constraints.limitedVehicleIndex
            );
            steps++;
        }

        detail = "Exceeded maxSteps " + maxSteps;
        return false;
    }

    public static string FormatReportLine(Report r)
    {
        string name = r.level != null ? r.level.name : "?";
        return "DB#" + r.dbIndex + " " + name +
            " | " + r.status +
            " | moves=" + r.minimumMoves +
            " | solLen=" + r.solutionLength +
            " | explored=" + r.statesExplored +
            " | fragileUsed=" + r.fragileMovesUsed +
            " | limitedUsed=" + r.limitedMovesUsed +
            " | " + r.detail;
    }

    /// <summary>
    /// Deep-validate only levels that timed out under the normal budget.
    /// Does not write LevelData. TIMEOUT remains TIMEOUT until proven otherwise.
    /// </summary>
    public static List<DeepTimeoutResult> DeepValidateTimeoutLevels(
        LevelDatabase database,
        IList<Report> priorReports,
        int deepMaxStates = DeepMaxStates)
    {
        List<DeepTimeoutResult> results = new List<DeepTimeoutResult>();
        if (database == null || database.levels == null)
        {
            return results;
        }

        List<Report> timeouts = new List<Report>();
        if (priorReports != null)
        {
            for (int i = 0; i < priorReports.Count; i++)
            {
                if (priorReports[i].status == Status.SolverTimeout ||
                    priorReports[i].status == Status.Unknown)
                {
                    timeouts.Add(priorReports[i]);
                }
            }
        }

        if (timeouts.Count == 0)
        {
            // Discover timeouts with normal budget first.
            for (int i = 0; i < database.levels.Count; i++)
            {
                Report normal = Validate(database.levels[i], i, DefaultMaxStates);
                if (normal.status == Status.SolverTimeout || normal.status == Status.Unknown)
                {
                    timeouts.Add(normal);
                }
            }
        }

        for (int i = 0; i < timeouts.Count; i++)
        {
            Report normal = timeouts[i];
            LevelData level = normal.level;
            if (level == null &&
                normal.dbIndex >= 0 &&
                normal.dbIndex < database.levels.Count)
            {
                level = database.levels[normal.dbIndex];
            }

            if (level == null)
            {
                continue;
            }

            int dbIndex = normal.dbIndex >= 0 ? normal.dbIndex : i;
            Report deep = Validate(level, dbIndex, deepMaxStates);

            DeepTimeoutResult entry = new DeepTimeoutResult
            {
                normalReport = normal,
                deepReport = deep,
                hintPathPass = false,
                hintPathDetail = string.Empty,
                releaseSafe = false,
                releaseBlocker = false,
                recommendedAction = string.Empty
            };

            if (deep.status == Status.Valid)
            {
                bool hintOk = VerifyHintPath(
                    level,
                    out string hintDetail,
                    deepMaxStates
                );
                entry.hintPathPass = hintOk;
                entry.hintPathDetail = hintDetail;
                entry.releaseSafe = hintOk;
                entry.releaseBlocker = !hintOk;
                entry.recommendedAction = hintOk
                    ? "Keep (release-safe). Write minMoves only via Recalculate Min Moves."
                    : "Needs Review — VALID solve but HintPath FAIL. Reject/Replace.";
            }
            else if (deep.status == Status.Unsolvable ||
                     deep.status == Status.ObjectiveInvalid ||
                     deep.status == Status.ConfigInvalid)
            {
                entry.releaseBlocker = true;
                entry.recommendedAction =
                    "Needs Review — Recommended Action = Reject/Replace";
            }
            else
            {
                // Still TIMEOUT / UNKNOWN after deep budget.
                entry.releaseBlocker = true;
                entry.recommendedAction =
                    "RELEASE BLOCKER — UNPROVEN SOLVABILITY — Reject/Replace";
            }

            entry.summaryLine = FormatDeepTimeoutLine(database, entry);
            results.Add(entry);
        }

        return results;
    }

    public static string FormatDeepTimeoutLine(
        LevelDatabase database,
        DeepTimeoutResult entry)
    {
        Report n = entry.normalReport;
        Report d = entry.deepReport;
        LevelData level = d.level != null ? d.level : n.level;
        string asset = level != null ? level.name : "?";
        int dbIndex = d.dbIndex >= 0 ? d.dbIndex : n.dbIndex;
        LevelDifficulty difficulty = level != null ? level.difficulty : LevelDifficulty.Easy;
        int local = database != null
            ? LevelDifficultyOrder.GetDifficultyDisplayNumber(database, dbIndex)
            : 0;

        StringBuilder sb = new StringBuilder(512);
        sb.AppendLine("----");
        sb.AppendLine("Asset=" + asset);
        sb.AppendLine("DBIndex=" + dbIndex);
        sb.AppendLine("Difficulty=" + difficulty);
        sb.AppendLine("LocalNumber=" + local);
        sb.AppendLine(
            "Objective=" + (level != null ? level.objectiveType.ToString() : "?")
        );
        sb.AppendLine("NormalStatus=TIMEOUT");
        sb.AppendLine("DeepStatus=" + d.status);
        sb.AppendLine("StatesExplored=" + d.statesExplored);
        sb.AppendLine("DiscoveredStates=" + d.discoveredStates);
        sb.AppendLine("ElapsedSeconds=" + d.elapsedSeconds.ToString("0.00"));
        sb.AppendLine(
            "MinimumPlayerMoves=" +
            (d.status == Status.Valid ? d.minimumMoves.ToString() : "n/a")
        );
        sb.AppendLine(
            "Grid=" + d.gridWidth + "x" + d.gridHeight +
            " Vehicles=" + d.vehicleCount +
            " Targets=" + d.targetCount
        );
        sb.AppendLine(
            "BranchingEstimate=" + d.branchingEstimate.ToString("0.00") +
            " SearchLimitKind=" +
            (string.IsNullOrEmpty(d.searchLimitKind) ? "-" : d.searchLimitKind)
        );
        sb.AppendLine(
            "DeepBudgetStates=" + DeepMaxStates +
            " (normal=" + DefaultMaxStates + ")"
        );
        if (d.status == Status.Valid)
        {
            sb.AppendLine(
                "HintPath=" + (entry.hintPathPass ? "PASS" : "FAIL") +
                (string.IsNullOrEmpty(entry.hintPathDetail)
                    ? string.Empty
                    : " | " + entry.hintPathDetail)
            );
        }

        sb.AppendLine(
            "ReleaseSafe=" + entry.releaseSafe +
            " ReleaseBlocker=" + entry.releaseBlocker
        );
        sb.AppendLine("RecommendedAction=" + entry.recommendedAction);
        return sb.ToString();
    }

    public static string BuildDeepValidationSummary(List<DeepTimeoutResult> results)
    {
        StringBuilder sb = new StringBuilder(2048);
        sb.AppendLine("=== DEEP VALIDATE TIMEOUT LEVELS ===");
        sb.AppendLine(
            "Normal budget states=" + DefaultMaxStates +
            " | Deep budget states=" + DeepMaxStates +
            " (x" + DeepMaxStatesMultiplier + ")"
        );
        sb.AppendLine("TIMEOUT ≠ VALID/UNSOLVABLE until proven.");
        sb.AppendLine();

        int releaseSafe = 0;
        int blocked = 0;
        for (int i = 0; i < results.Count; i++)
        {
            sb.AppendLine(results[i].summaryLine);
            if (results[i].releaseSafe)
            {
                releaseSafe++;
            }

            if (results[i].releaseBlocker)
            {
                blocked++;
            }
        }

        sb.AppendLine();
        sb.AppendLine("=== RELEASE VALIDATION RULE ===");
        sb.AppendLine("Release Safe: Playability VALID + HintPath PASS");
        sb.AppendLine(
            "Blocked: UNSOLVABLE | OBJECTIVE_INVALID | CONFIG_INVALID | DEEP TIMEOUT"
        );
        sb.AppendLine(
            "TimeoutCount=" + results.Count +
            " ReleaseSafe=" + releaseSafe +
            " ReleaseBlockers=" + blocked
        );
        sb.AppendLine("No LevelData writes — use Recalculate Min Moves for VALID pars.");
        return sb.ToString();
    }

    public struct MinMovesRecalcRow
    {
        public int dbIndex;
        public string assetName;
        public Status status;
        public int oldMinMoves;
        public int newMinMoves;
        public string detail;
    }

    /// <summary>
    /// Preview-only pass: solve all levels, return old→new without writing.
    /// </summary>
    public static List<MinMovesRecalcRow> BuildMinMovesRecalcPreview(
        LevelDatabase database,
        int maxStates = DefaultMaxStates)
    {
        List<MinMovesRecalcRow> rows = new List<MinMovesRecalcRow>();
        if (database == null || database.levels == null)
        {
            return rows;
        }

        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            MinMovesRecalcRow row = new MinMovesRecalcRow
            {
                dbIndex = i,
                assetName = level != null ? level.name : "null",
                oldMinMoves = level != null ? level.minimumMoves : 0,
                newMinMoves = level != null ? level.minimumMoves : 0,
                status = Status.Unknown,
                detail = string.Empty
            };

            if (level == null)
            {
                row.status = Status.ConfigInvalid;
                row.detail = "null level";
                rows.Add(row);
                continue;
            }

            Report report = Validate(level, i, maxStates);
            row.status = report.status;
            row.detail = report.detail;

            if (report.status == Status.Valid &&
                LevelMinMoves.IsValid(report.minimumMoves))
            {
                row.newMinMoves = report.minimumMoves;
            }

            rows.Add(row);
        }

        return rows;
    }

    /// <summary>
    /// Writes new minMoves only for VALID rows. Returns how many assets updated.
    /// </summary>
    public static int ApplyMinMovesRecalc(List<MinMovesRecalcRow> preview)
    {
        if (preview == null)
        {
            return 0;
        }

        LevelDatabase database = LoadMainDb();
        if (database == null || database.levels == null)
        {
            return 0;
        }

        int written = 0;
        for (int i = 0; i < preview.Count; i++)
        {
            MinMovesRecalcRow row = preview[i];
            if (row.status != Status.Valid || !LevelMinMoves.IsValid(row.newMinMoves))
            {
                continue;
            }

            if (row.dbIndex < 0 || row.dbIndex >= database.levels.Count)
            {
                continue;
            }

            LevelData level = database.levels[row.dbIndex];
            if (level == null)
            {
                continue;
            }

            if (level.minimumMoves == row.newMinMoves)
            {
                continue;
            }

            Undo.RecordObject(level, "Recalculate Min Moves");
            level.minimumMoves = row.newMinMoves;
            // Refresh difficultyScore when we have states from a fresh validate.
            Report fresh = Validate(level, row.dbIndex);
            if (fresh.status == Status.Valid && fresh.statesExplored > 0)
            {
                int vehicleCount = level.vehicles != null ? level.vehicles.Count : 0;
                level.statesExplored = fresh.statesExplored;
                level.difficultyScore =
                    row.newMinMoves * 100 +
                    Mathf.RoundToInt(Mathf.Log10(fresh.statesExplored + 1) * 50f) +
                    vehicleCount * 10;
            }

            EditorUtility.SetDirty(level);
            written++;
        }

        if (written > 0)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }

        return written;
    }

#if UNITY_EDITOR
    [MenuItem("RushOut/Recalculate Min Moves For All Playable Levels")]
    public static void MenuRecalculateMinMoves()
    {
        LevelDatabase database = LoadMainDb();
        if (database == null)
        {
            return;
        }

        List<MinMovesRecalcRow> preview = BuildMinMovesRecalcPreview(database);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== RECALCULATE MIN MOVES (preview) ===");
        int willWrite = 0;
        int errors = 0;
        int timeouts = 0;
        int unchanged = 0;

        for (int i = 0; i < preview.Count; i++)
        {
            MinMovesRecalcRow row = preview[i];
            if (row.status == Status.Valid && LevelMinMoves.IsValid(row.newMinMoves))
            {
                if (row.oldMinMoves != row.newMinMoves)
                {
                    willWrite++;
                    sb.AppendLine(
                        "WRITE DB#" + row.dbIndex + " " + row.assetName +
                        ": " + row.oldMinMoves + " → " + row.newMinMoves
                    );
                }
                else
                {
                    unchanged++;
                }
            }
            else if (row.status == Status.SolverTimeout || row.status == Status.Unknown)
            {
                timeouts++;
                sb.AppendLine(
                    "TIMEOUT/UNKNOWN DB#" + row.dbIndex + " " + row.assetName +
                    " (kept " + row.oldMinMoves + ") | " + row.detail
                );
            }
            else
            {
                errors++;
                sb.AppendLine(
                    "ERROR DB#" + row.dbIndex + " " + row.assetName +
                    " status=" + row.status +
                    " (kept " + row.oldMinMoves + ") | " + row.detail
                );
            }
        }

        sb.AppendLine();
        sb.AppendLine(
            "Will write=" + willWrite +
            " unchanged=" + unchanged +
            " errors=" + errors +
            " timeout/unknown=" + timeouts
        );
        Debug.Log(sb.ToString());

        bool ok = EditorUtility.DisplayDialog(
            "Recalculate Min Moves",
            "Preview logged to Console.\n\n" +
            "Write " + willWrite + " LevelData.minimumMoves update(s)?\n" +
            "Errors (not overwritten): " + errors + "\n" +
            "Timeouts (not overwritten): " + timeouts + "\n" +
            "Unchanged: " + unchanged,
            "Write",
            "Cancel"
        );
        if (!ok)
        {
            return;
        }

        int written = ApplyMinMovesRecalc(preview);
        EditorUtility.DisplayDialog(
            "Recalculate Min Moves",
            "Wrote minimumMoves on " + written + " level asset(s).",
            "OK"
        );
    }

    [MenuItem("RushOut/Validate Playable Levels (Production)")]
    public static void MenuValidateAll()
    {
        LevelDatabase database = LoadMainDb();
        if (database == null)
        {
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== PRODUCTION PLAYABILITY VALIDATION ===");
        int[] counts = new int[6];
        for (int i = 0; i < database.levels.Count; i++)
        {
            Report r = Validate(database.levels[i], i);
            counts[(int)r.status]++;
            if (r.status != Status.Valid)
            {
                sb.AppendLine(FormatReportLine(r));
            }
        }

        sb.AppendLine();
        sb.AppendLine(
            "Valid=" + counts[(int)Status.Valid] +
            " Unsolvable=" + counts[(int)Status.Unsolvable] +
            " ObjectiveInvalid=" + counts[(int)Status.ObjectiveInvalid] +
            " Timeout=" + counts[(int)Status.SolverTimeout] +
            " ConfigInvalid=" + counts[(int)Status.ConfigInvalid]
        );
        Debug.Log(sb.ToString());
    }

    [MenuItem("RushOut/Deep Validate Timeout Levels")]
    public static void MenuDeepValidateTimeouts()
    {
        LevelDatabase database = LoadMainDb();
        if (database == null)
        {
            return;
        }

        // Discover normal timeouts then deep-validate only those.
        List<Report> priors = new List<Report>();
        for (int i = 0; i < database.levels.Count; i++)
        {
            Report r = Validate(database.levels[i], i, DefaultMaxStates);
            if (r.status == Status.SolverTimeout || r.status == Status.Unknown)
            {
                priors.Add(r);
            }
        }

        if (priors.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Deep Validate Timeout Levels",
                "No TIMEOUT/UNKNOWN levels under normal budget (" +
                DefaultMaxStates + " states).",
                "OK"
            );
            return;
        }

        bool proceed = EditorUtility.DisplayDialog(
            "Deep Validate Timeout Levels",
            "Found " + priors.Count + " TIMEOUT level(s).\n\n" +
            "Deep budget: " + DeepMaxStates + " states " +
            "(normal " + DefaultMaxStates + " × " + DeepMaxStatesMultiplier + ").\n" +
            "This may take several minutes. No LevelData writes.\n\nContinue?",
            "Deep Validate",
            "Cancel"
        );
        if (!proceed)
        {
            return;
        }

        List<DeepTimeoutResult> results = DeepValidateTimeoutLevels(
            database,
            priors,
            DeepMaxStates
        );
        string summary = BuildDeepValidationSummary(results);
        Debug.Log(summary);
        EditorUtility.DisplayDialog(
            "Deep Validate Timeout Levels",
            "Done. " + results.Count + " timeout level(s) rechecked.\n" +
            "See Console for per-level DeepStatus / ReleaseSafe / Blockers.",
            "OK"
        );
    }

    [MenuItem("RushOut/Audit Level_033 Playability")]
    public static void MenuAuditLevel033()
    {
        AuditLevelAssetByName("Level_033");
    }

    /// <summary>
    /// Console report: Validate + VerifyHintPath for one LevelData asset name.
    /// </summary>
    public static void AuditLevelAssetByName(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            Debug.LogError("AuditLevelAssetByName: empty name.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets(assetName + " t:LevelData");
        LevelData level = null;
        string path = null;
        for (int i = 0; i < guids.Length; i++)
        {
            path = AssetDatabase.GUIDToAssetPath(guids[i]);
            LevelData candidate = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (candidate != null && candidate.name == assetName)
            {
                level = candidate;
                break;
            }
        }

        if (level == null)
        {
            Debug.LogError("Level asset not found: " + assetName);
            return;
        }

        int dbIndex = -1;
        LevelDatabase database = LoadMainDb();
        if (database != null && database.levels != null)
        {
            for (int i = 0; i < database.levels.Count; i++)
            {
                if (database.levels[i] == level)
                {
                    dbIndex = i;
                    break;
                }
            }
        }

        Report report = Validate(level, dbIndex >= 0 ? dbIndex : 0);
        bool hintOk = VerifyHintPath(level, out string hintDetail, DefaultMaxStates, 256);

        StringBuilder sb = new StringBuilder(1024);
        sb.AppendLine("=== LEVEL AUDIT: " + assetName + " ===");
        sb.AppendLine("Path: " + path);
        sb.AppendLine("DB index: " + (dbIndex >= 0 ? dbIndex.ToString() : "(not in MainLevelDatabase)"));
        sb.AppendLine("Objective: " + level.objectiveType);
        sb.AppendLine("Difficulty: " + level.difficulty);
        sb.AppendLine("Grid: " + level.ResolvedGridWidth + "x" + level.ResolvedGridHeight);
        sb.AppendLine("Asset minimumMoves: " + level.minimumMoves);
        sb.AppendLine("Playability status: " + report.status);
        sb.AppendLine("Solvable (classic): " + report.classicSolvable);
        sb.AppendLine("Solvable (objective): " + report.objectiveSolvable);
        sb.AppendLine("Reported minimum player moves: " + report.minimumMoves);
        sb.AppendLine("Objective valid: " +
            (report.status != Status.ObjectiveInvalid && report.status != Status.ConfigInvalid));
        sb.AppendLine("Verify Hint Path: " + (hintOk ? "PASS" : "FAIL"));
        sb.AppendLine("Hint path detail: " +
            (string.IsNullOrEmpty(hintDetail) ? "(none)" : hintDetail));
        sb.AppendLine("Detail: " + report.detail);
        Debug.Log(sb.ToString());
    }

    [MenuItem("RushOut/Verify Hint Paths (All Levels)")]
    public static void MenuVerifyHintPaths()
    {
        LevelDatabase database = LoadMainDb();
        if (database == null)
        {
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== VERIFY HINT PATHS ===");
        int pass = 0;
        int fail = 0;
        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level == null)
            {
                continue;
            }

            bool ok = VerifyHintPath(level, out string detail);
            if (ok)
            {
                pass++;
            }
            else
            {
                fail++;
                sb.AppendLine("FAIL DB#" + i + " " + level.name + " | " + detail);
            }
        }

        sb.AppendLine("Pass=" + pass + " Fail=" + fail);
        Debug.Log(sb.ToString());
    }

    [MenuItem("RushOut/Report Regression Levels (Hard11/22/24 Med42)")]
    public static void MenuReportNamedLevels()
    {
        LevelDatabase database = LoadMainDb();
        if (database == null)
        {
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== REGRESSION LEVEL REPORT ===");
        AppendLocal(sb, database, LevelDifficulty.Hard, 11);
        AppendLocal(sb, database, LevelDifficulty.Hard, 22);
        AppendLocal(sb, database, LevelDifficulty.Hard, 24);
        AppendLocal(sb, database, LevelDifficulty.Medium, 42);
        Debug.Log(sb.ToString());
    }

    private static void AppendLocal(
        StringBuilder sb,
        LevelDatabase database,
        LevelDifficulty difficulty,
        int localNumber)
    {
        int dbIndex = LevelDifficultyOrder.GetDatabaseIndexForDifficultyDisplayNumber(
            database,
            difficulty,
            localNumber
        );
        if (dbIndex < 0 || dbIndex >= database.levels.Count)
        {
            sb.AppendLine(difficulty + " #" + localNumber + " → NOT FOUND");
            return;
        }

        LevelData level = database.levels[dbIndex];
        Report r = Validate(level, dbIndex);
        bool hintOk = VerifyHintPath(level, out string hintDetail);
        sb.AppendLine(
            difficulty + " #" + localNumber +
            " | asset=" + (level != null ? level.name : "?") +
            " | dbIndex=" + dbIndex +
            " | objective=" + (level != null ? level.objectiveType.ToString() : "?") +
            " | status=" + r.status +
            " | minMoves=" + r.minimumMoves +
            " | classicSolvable=" + r.classicSolvable +
            " | objectiveSolvable=" + r.objectiveSolvable +
            " | hintPath=" + (hintOk ? "OK" : hintDetail) +
            " | " + r.detail
        );
    }

    private static LevelDatabase LoadMainDb()
    {
        string[] guids = AssetDatabase.FindAssets("MainLevelDatabase t:LevelDatabase");
        if (guids == null || guids.Length == 0)
        {
            Debug.LogError("MainLevelDatabase not found.");
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<LevelDatabase>(
            AssetDatabase.GUIDToAssetPath(guids[0])
        );
    }
#endif

    private static string ValidateLayoutForPlayability(LevelData level)
    {
        if (level.vehicles == null || level.vehicles.Count == 0)
        {
            return "no vehicles";
        }

        int targets = 0;
        for (int i = 0; i < level.vehicles.Count; i++)
        {
            if (level.vehicles[i] != null && level.vehicles[i].canExitRight)
            {
                targets++;
            }
        }

        if (level.objectiveType == LevelObjectiveType.MultiTargetRescue)
        {
            if (targets < 2)
            {
                return "MultiTarget needs >=2 canExitRight";
            }
        }
        else if (targets < 1)
        {
            return "needs >=1 canExitRight";
        }

        return null;
    }

    private static bool IsWinSolution(RushOutSolver.SolverResult result)
    {
        return result != null &&
            result.solvable &&
            result.solution != null &&
            result.solution.Count > 0;
    }

    private static bool AllTargetsExited(
        List<RushOutSolver.VehicleDefinition> vehicles,
        RushOutSolver.BoardState state,
        RushOutSolver.SolveConstraints constraints)
    {
        int[] targets = RushOutSolver.ResolveTargetIndices(vehicles, constraints);
        for (int i = 0; i < targets.Length; i++)
        {
            if (!state.IsExited(targets[i]))
            {
                return false;
            }
        }

        return true;
    }
}

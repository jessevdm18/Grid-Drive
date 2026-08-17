using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only, read-only: heuristische kandidaten voor NoTouch / FragileCargo / LimitedVehicle.
/// Één RushOutSolver.SolveLevelData per level (solution path analyse). Wijzigt geen assets.
/// </summary>
public static class SpecialMissionSuitabilityAnalyzer
{
    private const string DatabaseAssetFilter = "MainLevelDatabase t:LevelDatabase";

    public const int ScoreGoodMin = 80;
    public const int ScoreMaybeMin = 55;

    public enum MissionKind
    {
        NoTouch = 0,
        FragileCargo = 1,
        LimitedVehicle = 2
    }

    public enum Category
    {
        NotApplicable = 0,
        Poor = 1,
        Maybe = 2,
        Good = 3
    }

    public struct Result
    {
        public LevelData level;
        public string assetName;
        public int levelNumber;
        public int gridWidth;
        public int gridHeight;
        public int exitRow;
        public int minimumMoves;
        public int storedMinimumMoves;
        public bool solvable;

        public MissionKind missionKind;
        public int score;
        public Category category;
        public List<string> reasons;

        public int vehicleIndex;
        public string vehicleName;
        public Vector2Int vehiclePos;
        public VehicleController.VehicleOrientation vehicleOrientation;
        public int vehicleLength;

        public int vehicleMovesInSolution;
        public int firstMoveIndex;
        public int lastMoveIndex;

        public int targetMovesInSolution;
        public int recommendedCargoLimit;
        public int recommendedLimitedLimit;

        public MissionKind bestMatchKind;
        public int bestMatchScore;
    }

    private struct VehicleSolutionStats
    {
        public int moveCount;
        public int firstIndex;
        public int lastIndex;
    }

    public static List<Result> AnalyzeMainLevelDatabase()
    {
        List<Result> results = new List<Result>();
        LevelDatabase database = LoadMainLevelDatabase();
        if (database == null || database.levels == null)
        {
            return results;
        }

        try
        {
            int total = database.levels.Count;
            for (int i = 0; i < total; i++)
            {
                LevelData level = database.levels[i];
                float progress = total > 0 ? (i + 1) / (float)total : 1f;
                EditorUtility.DisplayProgressBar(
                    "Special Mission Suitability",
                    level != null
                        ? ("Level " + level.levelNumber + " — " + level.name)
                        : ("Index " + i),
                    progress
                );

                if (level == null)
                {
                    continue;
                }

                List<Result> levelResults = AnalyzeLevel(level);
                results.AddRange(levelResults);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        results.Sort((a, b) =>
        {
            int missionCmp = ((int)a.missionKind).CompareTo((int)b.missionKind);
            if (missionCmp != 0)
            {
                return missionCmp;
            }

            int scoreCmp = b.score.CompareTo(a.score);
            if (scoreCmp != 0)
            {
                return scoreCmp;
            }

            return a.levelNumber.CompareTo(b.levelNumber);
        });

        return results;
    }

    public static List<Result> AnalyzeLevel(LevelData level)
    {
        List<Result> results = new List<Result>();
        if (level == null || level.vehicles == null || level.vehicles.Count == 0)
        {
            return results;
        }

        RushOutSolver.SolverResult solve = RushOutSolver.SolveLevelData(level);
        VehicleSolutionStats[] stats = BuildVehicleStats(level.vehicles.Count, solve);

        int bestScore = -1;
        MissionKind bestKind = MissionKind.NoTouch;

        Result noTouch = AnalyzeNoTouch(level, solve, stats);
        Result fragile = AnalyzeFragileCargo(level, solve, stats);
        Result limited = AnalyzeLimitedVehicle(level, solve, stats);

        ConsiderBest(noTouch, ref bestScore, ref bestKind);
        ConsiderBest(fragile, ref bestScore, ref bestKind);
        ConsiderBest(limited, ref bestScore, ref bestKind);

        noTouch.bestMatchKind = bestKind;
        noTouch.bestMatchScore = Mathf.Max(0, bestScore);
        fragile.bestMatchKind = bestKind;
        fragile.bestMatchScore = Mathf.Max(0, bestScore);
        limited.bestMatchKind = bestKind;
        limited.bestMatchScore = Mathf.Max(0, bestScore);

        results.Add(noTouch);
        results.Add(fragile);
        results.Add(limited);
        return results;
    }

    private static void ConsiderBest(Result r, ref int bestScore, ref MissionKind bestKind)
    {
        if (r.category == Category.NotApplicable)
        {
            return;
        }

        if (r.score > bestScore)
        {
            bestScore = r.score;
            bestKind = r.missionKind;
        }
    }

    private static Result CreateBaseResult(LevelData level, MissionKind kind, RushOutSolver.SolverResult solve)
    {
        return new Result
        {
            level = level,
            assetName = level != null ? level.name : "(null)",
            levelNumber = level != null ? level.levelNumber : 0,
            gridWidth = level != null ? level.ResolvedGridWidth : 0,
            gridHeight = level != null ? level.ResolvedGridHeight : 0,
            exitRow = level != null ? level.exitRow : 0,
            minimumMoves = solve != null && solve.solvable
                ? solve.minimumMoves
                : (level != null ? level.minimumMoves : 0),
            storedMinimumMoves = level != null ? level.minimumMoves : 0,
            solvable = solve != null && solve.solvable,
            missionKind = kind,
            score = 0,
            category = Category.NotApplicable,
            reasons = new List<string>(),
            vehicleIndex = -1,
            vehicleName = "-",
            vehiclePos = new Vector2Int(-1, -1),
            vehicleOrientation = VehicleController.VehicleOrientation.Horizontal,
            vehicleLength = 0,
            vehicleMovesInSolution = 0,
            firstMoveIndex = -1,
            lastMoveIndex = -1,
            targetMovesInSolution = 0,
            recommendedCargoLimit = 0,
            recommendedLimitedLimit = 0,
            bestMatchKind = MissionKind.NoTouch,
            bestMatchScore = 0
        };
    }

    private static VehicleSolutionStats[] BuildVehicleStats(
        int vehicleCount,
        RushOutSolver.SolverResult solve)
    {
        VehicleSolutionStats[] stats = new VehicleSolutionStats[vehicleCount];
        for (int i = 0; i < vehicleCount; i++)
        {
            stats[i].firstIndex = -1;
            stats[i].lastIndex = -1;
        }

        if (solve == null || !solve.solvable || solve.solution == null)
        {
            return stats;
        }

        for (int s = 0; s < solve.solution.Count; s++)
        {
            RushOutSolver.SolverMove move = solve.solution[s];
            int vi = move.vehicleIndex;
            if (vi < 0 || vi >= vehicleCount)
            {
                continue;
            }

            stats[vi].moveCount++;
            if (stats[vi].firstIndex < 0)
            {
                stats[vi].firstIndex = s;
            }

            stats[vi].lastIndex = s;
        }

        return stats;
    }

    // -------------------------------------------------------------------------
    // NoTouch
    // -------------------------------------------------------------------------

    private static Result AnalyzeNoTouch(
        LevelData level,
        RushOutSolver.SolverResult solve,
        VehicleSolutionStats[] stats)
    {
        Result result = CreateBaseResult(level, MissionKind.NoTouch, solve);

        if (!solve.solvable || solve.solution == null)
        {
            result.reasons.Add("Not solvable / no solution path");
            return result;
        }

        int bestScore = -1;
        Result best = result;

        for (int i = 0; i < level.vehicles.Count; i++)
        {
            VehicleData v = level.vehicles[i];
            if (v == null || v.canExitRight)
            {
                continue;
            }

            Result scored = ScoreNoTouchVehicle(level, solve, stats, i, result);
            if (scored.score > bestScore)
            {
                bestScore = scored.score;
                best = scored;
            }
        }

        if (bestScore < 0)
        {
            result.reasons.Add("No non-target vehicles");
            return result;
        }

        return best;
    }

    private static Result ScoreNoTouchVehicle(
        LevelData level,
        RushOutSolver.SolverResult solve,
        VehicleSolutionStats[] stats,
        int vehicleIndex,
        Result template)
    {
        Result result = template;
        result.reasons = new List<string>();

        VehicleData v = level.vehicles[vehicleIndex];
        FillVehicleFields(ref result, v, vehicleIndex, stats);

        int score = 30;
        result.reasons.Add("+ Valid non-target candidate");

        int moves = stats[vehicleIndex].moveCount;
        if (moves == 0)
        {
            score += 35;
            result.reasons.Add("+ Not used in optimal solution");
        }
        else if (moves == 1)
        {
            score -= 20;
            result.reasons.Add("- Moved once in optimal solution");
        }
        else
        {
            score -= 45;
            result.reasons.Add("- Moved " + moves + " times in optimal solution");
        }

        int gw = level.ResolvedGridWidth;
        int gh = level.ResolvedGridHeight;
        int exitRow = level.exitRow;

        int rowDist = Mathf.Abs(v.gridPosition.y - exitRow);
        if (rowDist <= 1)
        {
            score += 10;
            result.reasons.Add("+ Near exit row");
        }

        bool onEdge = IsOnBoardEdge(v, gw, gh);
        bool central = IsBoardCentral(v, gw, gh);
        if (central)
        {
            score += 10;
            result.reasons.Add("+ Central / relevant board position");
        }
        else if (onEdge && rowDist > 2)
        {
            score -= 15;
            result.reasons.Add("- Edge-isolated / likely irrelevant");
        }

        if (v.lengthInCells >= 3)
        {
            score += 5;
            result.reasons.Add("+ Longer blocker (visually tempting)");
        }

        if (solve.minimumMoves >= 10)
        {
            score += 5;
            result.reasons.Add("+ Higher minimumMoves puzzle");
        }
        else if (solve.minimumMoves <= 4)
        {
            score -= 8;
            result.reasons.Add("- Very short puzzle");
        }

        result.score = Mathf.Clamp(score, 0, 100);
        result.category = Categorize(result.score);
        return result;
    }

    // -------------------------------------------------------------------------
    // FragileCargo
    // -------------------------------------------------------------------------

    private static Result AnalyzeFragileCargo(
        LevelData level,
        RushOutSolver.SolverResult solve,
        VehicleSolutionStats[] stats)
    {
        Result result = CreateBaseResult(level, MissionKind.FragileCargo, solve);

        if (!solve.solvable || solve.solution == null)
        {
            result.reasons.Add("Not solvable / no solution path");
            return result;
        }

        int targetIndex = FindPrimaryTargetIndex(level.vehicles);
        if (targetIndex < 0)
        {
            result.reasons.Add("No canExitRight target");
            return result;
        }

        VehicleData target = level.vehicles[targetIndex];
        FillVehicleFields(ref result, target, targetIndex, stats);

        int targetMoves = stats[targetIndex].moveCount;
        result.targetMovesInSolution = targetMoves;
        result.vehicleMovesInSolution = targetMoves;

        int recommended = RecommendCargoLimit(targetMoves);
        result.recommendedCargoLimit = recommended;

        int score = 25;
        result.reasons.Add("+ Target is cargo candidate (canExitRight)");

        if (targetMoves <= 1)
        {
            score -= 30;
            result.reasons.Add("- Target only needs 1 move (exit)");
        }
        else if (targetMoves == 2)
        {
            score += 15;
            result.reasons.Add("+ Target requires 2 moves");
        }
        else if (targetMoves == 3)
        {
            score += 35;
            result.reasons.Add("+ Target requires 3 moves");
        }
        else
        {
            score += 40;
            result.reasons.Add("+ Target requires " + targetMoves + " moves");
        }

        int rightMost = target.gridPosition.x + target.lengthInCells - 1;
        int gapToExit = (level.ResolvedGridWidth - 1) - rightMost;
        if (gapToExit >= 3)
        {
            score += 12;
            result.reasons.Add("+ Target starts far from exit");
        }
        else if (gapToExit >= 2)
        {
            score += 6;
            result.reasons.Add("+ Target moderate distance to exit");
        }
        else
        {
            score -= 12;
            result.reasons.Add("- Target already near exit");
        }

        if (solve.minimumMoves >= 12)
        {
            score += 10;
            result.reasons.Add("+ High minimumMoves (" + solve.minimumMoves + ")");
        }
        else if (solve.minimumMoves >= 8)
        {
            score += 5;
            result.reasons.Add("+ Solid minimumMoves (" + solve.minimumMoves + ")");
        }
        else if (solve.minimumMoves <= 4)
        {
            score -= 10;
            result.reasons.Add("- Trivial minimumMoves");
        }

        int nonTargetMoves = CountNonTargetMoves(solve, targetIndex);
        if (nonTargetMoves >= 6)
        {
            score += 8;
            result.reasons.Add("+ Several blocker moves before/around exit");
        }
        else if (nonTargetMoves <= 2)
        {
            score -= 6;
            result.reasons.Add("- Few blocker moves in solution");
        }

        if (recommended > 0)
        {
            result.reasons.Add("+ Recommended cargo limit: " + recommended);
        }

        result.score = Mathf.Clamp(score, 0, 100);
        result.category = Categorize(result.score);
        return result;
    }

    private static int RecommendCargoLimit(int targetMoves)
    {
        if (targetMoves <= 1)
        {
            return 0;
        }

        if (targetMoves == 2)
        {
            return 3;
        }

        if (targetMoves == 3)
        {
            return 4;
        }

        return Mathf.Clamp(targetMoves + 1, 4, 12);
    }

    private static int CountNonTargetMoves(RushOutSolver.SolverResult solve, int targetIndex)
    {
        if (solve == null || solve.solution == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < solve.solution.Count; i++)
        {
            if (solve.solution[i].vehicleIndex != targetIndex)
            {
                count++;
            }
        }

        return count;
    }

    // -------------------------------------------------------------------------
    // LimitedVehicle
    // -------------------------------------------------------------------------

    private static Result AnalyzeLimitedVehicle(
        LevelData level,
        RushOutSolver.SolverResult solve,
        VehicleSolutionStats[] stats)
    {
        Result result = CreateBaseResult(level, MissionKind.LimitedVehicle, solve);

        if (!solve.solvable || solve.solution == null)
        {
            result.reasons.Add("Not solvable / no solution path");
            return result;
        }

        int bestScore = -1;
        Result best = result;

        for (int i = 0; i < level.vehicles.Count; i++)
        {
            VehicleData v = level.vehicles[i];
            if (v == null || v.canExitRight)
            {
                continue;
            }

            Result scored = ScoreLimitedVehicle(level, solve, stats, i, result);
            if (scored.score > bestScore)
            {
                bestScore = scored.score;
                best = scored;
            }
        }

        if (bestScore < 0)
        {
            result.reasons.Add("No non-target vehicles");
            return result;
        }

        return best;
    }

    private static Result ScoreLimitedVehicle(
        LevelData level,
        RushOutSolver.SolverResult solve,
        VehicleSolutionStats[] stats,
        int vehicleIndex,
        Result template)
    {
        Result result = template;
        result.reasons = new List<string>();

        VehicleData v = level.vehicles[vehicleIndex];
        FillVehicleFields(ref result, v, vehicleIndex, stats);

        int moves = stats[vehicleIndex].moveCount;
        int first = stats[vehicleIndex].firstIndex;
        int last = stats[vehicleIndex].lastIndex;
        int spread = (first >= 0 && last >= first) ? (last - first) : 0;

        int recommended = RecommendLimitedLimit(moves);
        result.recommendedLimitedLimit = recommended;

        int score = 20;
        result.reasons.Add("+ Valid non-target limited candidate");

        if (moves == 0)
        {
            score -= 35;
            result.reasons.Add("- Never moved in solution (constraint does nothing)");
        }
        else if (moves == 1)
        {
            score += 18;
            result.reasons.Add("+ Exactly 1 required move");
        }
        else if (moves == 2)
        {
            score += 40;
            result.reasons.Add("+ Exactly 2 required moves");
        }
        else if (moves == 3)
        {
            score += 28;
            result.reasons.Add("+ Exactly 3 required moves");
        }
        else
        {
            score -= 20;
            result.reasons.Add("- Moved " + moves + " times (risky for v1)");
        }

        if (moves >= 2 && spread >= 5)
        {
            score += 15;
            result.reasons.Add(
                "+ Moves spread across solution (first " +
                (first + 1) + ", last " + (last + 1) + ")"
            );
        }
        else if (moves >= 2 && spread <= 1)
        {
            score -= 8;
            result.reasons.Add("- Moves clustered back-to-back");
        }
        else if (moves >= 2 && spread >= 3)
        {
            score += 8;
            result.reasons.Add("+ Moderate move spread");
        }

        int gw = level.ResolvedGridWidth;
        int gh = level.ResolvedGridHeight;
        if (IsBoardCentral(v, gw, gh))
        {
            score += 8;
            result.reasons.Add("+ Central blocker");
        }

        int rowDist = Mathf.Abs(v.gridPosition.y - level.exitRow);
        if (rowDist <= 1)
        {
            score += 8;
            result.reasons.Add("+ Near exit / target route");
        }

        if (v.lengthInCells >= 3)
        {
            score += 6;
            result.reasons.Add("+ Length-" + v.lengthInCells + " blocker");
        }

        if (IsOnBoardEdge(v, gw, gh) && rowDist > 2 && moves <= 1)
        {
            score -= 10;
            result.reasons.Add("- Edge / less relevant");
        }

        if (recommended > 0)
        {
            result.reasons.Add("+ Recommended limited limit: " + recommended);
        }

        result.score = Mathf.Clamp(score, 0, 100);
        result.category = Categorize(result.score);
        return result;
    }

    private static int RecommendLimitedLimit(int movesInSolution)
    {
        if (movesInSolution <= 0)
        {
            return 0;
        }

        if (movesInSolution >= 4)
        {
            return 3;
        }

        return movesInSolution;
    }

    // -------------------------------------------------------------------------
    // Shared helpers
    // -------------------------------------------------------------------------

    private static void FillVehicleFields(
        ref Result result,
        VehicleData v,
        int index,
        VehicleSolutionStats[] stats)
    {
        result.vehicleIndex = index;
        result.vehicleName = VehicleLabel(v, index);
        result.vehiclePos = v.gridPosition;
        result.vehicleOrientation = v.orientation;
        result.vehicleLength = v.lengthInCells;
        result.vehicleMovesInSolution = stats[index].moveCount;
        result.firstMoveIndex = stats[index].firstIndex;
        result.lastMoveIndex = stats[index].lastIndex;
    }

    private static Category Categorize(int score)
    {
        if (score >= ScoreGoodMin)
        {
            return Category.Good;
        }

        if (score >= ScoreMaybeMin)
        {
            return Category.Maybe;
        }

        return Category.Poor;
    }

    private static int FindPrimaryTargetIndex(List<VehicleData> vehicles)
    {
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (vehicles[i] != null && vehicles[i].canExitRight)
            {
                return i;
            }
        }

        return -1;
    }

    private static bool IsOnBoardEdge(VehicleData v, int gw, int gh)
    {
        if (v.orientation == VehicleController.VehicleOrientation.Horizontal)
        {
            int right = v.gridPosition.x + v.lengthInCells - 1;
            return v.gridPosition.x <= 0 ||
                   right >= gw - 1 ||
                   v.gridPosition.y <= 0 ||
                   v.gridPosition.y >= gh - 1;
        }

        int top = v.gridPosition.y + v.lengthInCells - 1;
        return v.gridPosition.x <= 0 ||
               v.gridPosition.x >= gw - 1 ||
               v.gridPosition.y <= 0 ||
               top >= gh - 1;
    }

    private static bool IsBoardCentral(VehicleData v, int gw, int gh)
    {
        float cx = gw * 0.5f;
        float cy = gh * 0.5f;
        float vx = v.gridPosition.x + (v.lengthInCells - 1) * 0.5f;
        float vy = v.gridPosition.y;
        if (v.orientation == VehicleController.VehicleOrientation.Vertical)
        {
            vx = v.gridPosition.x;
            vy = v.gridPosition.y + (v.lengthInCells - 1) * 0.5f;
        }

        float dx = Mathf.Abs(vx - (cx - 0.5f));
        float dy = Mathf.Abs(vy - (cy - 0.5f));
        return dx <= gw * 0.35f && dy <= gh * 0.35f;
    }

    private static string VehicleLabel(VehicleData v, int index)
    {
        if (v == null)
        {
            return "vehicle[" + index + "]";
        }

        if (!string.IsNullOrEmpty(v.vehicleName))
        {
            return v.vehicleName + " [" + index + "]";
        }

        return "vehicle[" + index + "]";
    }

    public static string MissionKindLabel(MissionKind kind)
    {
        switch (kind)
        {
            case MissionKind.NoTouch:
                return "NO TOUCH";
            case MissionKind.FragileCargo:
                return "FRAGILE CARGO";
            case MissionKind.LimitedVehicle:
                return "LIMITED VEHICLE";
            default:
                return kind.ToString();
        }
    }

    public static string FormatResultsLog(
        List<Result> results,
        MissionKind? filterKind,
        bool onlyGood)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== Special Mission Suitability (read-only) ===");

        int shown = 0;
        for (int i = 0; i < results.Count; i++)
        {
            Result r = results[i];
            if (filterKind.HasValue && r.missionKind != filterKind.Value)
            {
                continue;
            }

            if (onlyGood && r.category != Category.Good)
            {
                continue;
            }

            shown++;
            sb.AppendLine();
            sb.AppendLine(
                "LEVEL " + r.levelNumber + " — " + MissionKindLabel(r.missionKind) +
                " — " + r.category + " — " + r.score
            );
            sb.AppendLine(
                "  " + r.assetName + " | " + r.gridWidth + "x" + r.gridHeight +
                " | minMoves " + r.minimumMoves +
                " (stored " + r.storedMinimumMoves + ")"
            );
            sb.AppendLine(
                "  Vehicle: " + r.vehicleName +
                " @ " + r.vehiclePos +
                ", " + r.vehicleOrientation +
                ", Length " + r.vehicleLength
            );
            sb.AppendLine(
                "  Moves in solution: " + r.vehicleMovesInSolution +
                " | first " + FormatMoveIndex(r.firstMoveIndex) +
                " | last " + FormatMoveIndex(r.lastMoveIndex)
            );

            if (r.missionKind == MissionKind.FragileCargo)
            {
                sb.AppendLine(
                    "  Target moves: " + r.targetMovesInSolution +
                    " | Recommended cargo limit: " + r.recommendedCargoLimit
                );
            }

            if (r.missionKind == MissionKind.LimitedVehicle)
            {
                sb.AppendLine(
                    "  Recommended limited limit: " + r.recommendedLimitedLimit
                );
            }

            sb.AppendLine(
                "  Best match on level: " + MissionKindLabel(r.bestMatchKind) +
                " (" + r.bestMatchScore + ")"
            );

            if (r.reasons != null)
            {
                for (int j = 0; j < r.reasons.Count; j++)
                {
                    sb.AppendLine("  " + r.reasons[j]);
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("Shown: " + shown + " / " + results.Count);
        return sb.ToString();
    }

    private static string FormatMoveIndex(int index)
    {
        return index < 0 ? "-" : (index + 1).ToString();
    }

    private static LevelDatabase LoadMainLevelDatabase()
    {
        string[] guids = AssetDatabase.FindAssets(DatabaseAssetFilter);
        if (guids == null || guids.Length == 0)
        {
            Debug.LogError("SpecialMissionSuitability: geen MainLevelDatabase gevonden.");
            return null;
        }

        if (guids.Length > 1)
        {
            Debug.LogError(
                "SpecialMissionSuitability: meerdere MainLevelDatabase assets."
            );
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);
    }

    [MenuItem("RushOut/Analyze Special Mission Candidates")]
    public static void MenuAnalyze()
    {
        List<Result> results = AnalyzeMainLevelDatabase();
        Debug.Log(FormatResultsLog(results, filterKind: null, onlyGood: false));
    }
}

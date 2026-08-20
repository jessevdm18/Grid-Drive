using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only, read-only: heuristische MultiTargetRescue-kandidaten in MainLevelDatabase.
/// Wijzigt geen LevelData / canExitRight / objectiveType. Geen solver-calls.
/// </summary>
public static class MultiTargetSuitabilityAnalyzer
{
    private const string DatabaseAssetFilter = "MainLevelDatabase t:LevelDatabase";

    public const int ScoreGoodMin = 80;
    public const int ScoreMaybeMin = 55;

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
        public string primaryTargetName;
        public Vector2Int primaryTargetPos;
        public int primaryTargetIndex;
        public string bestCandidateName;
        public Vector2Int bestCandidatePos;
        public int bestCandidateIndex;
        public int blockersAfterFirstRescue;
        public bool immediateExit;
        public int score;
        public Category category;
        public List<string> reasons;
    }

    public static List<Result> AnalyzeMainLevelDatabase()
    {
        List<Result> results = new List<Result>();
        LevelDatabase database = LoadMainLevelDatabase();
        if (database == null || database.levels == null)
        {
            return results;
        }

        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level == null)
            {
                continue;
            }

            results.Add(AnalyzeLevel(level));
        }

        results.Sort((a, b) =>
        {
            int scoreCmp = b.score.CompareTo(a.score);
            if (scoreCmp != 0)
            {
                return scoreCmp;
            }

            return a.levelNumber.CompareTo(b.levelNumber);
        });

        return results;
    }

    public static Result AnalyzeLevel(LevelData level)
    {
        Result result = new Result
        {
            level = level,
            assetName = level != null ? level.name : "(null)",
            levelNumber = level != null ? level.levelNumber : 0,
            gridWidth = level != null ? level.ResolvedGridWidth : 0,
            gridHeight = level != null ? level.ResolvedGridHeight : 0,
            exitRow = level != null ? level.exitRow : 0,
            minimumMoves = level != null ? level.minimumMoves : 0,
            primaryTargetName = "-",
            primaryTargetPos = new Vector2Int(-1, -1),
            primaryTargetIndex = -1,
            bestCandidateName = "-",
            bestCandidatePos = new Vector2Int(-1, -1),
            bestCandidateIndex = -1,
            blockersAfterFirstRescue = 0,
            immediateExit = false,
            score = 0,
            category = Category.NotApplicable,
            reasons = new List<string>()
        };

        if (level == null || level.vehicles == null || level.vehicles.Count == 0)
        {
            result.reasons.Add("Empty / null level");
            return result;
        }

        int gw = level.ResolvedGridWidth;
        int gh = level.ResolvedGridHeight;
        int exitRow = level.exitRow;

        int primaryIndex = FindPrimaryTargetIndex(level.vehicles);
        if (primaryIndex < 0)
        {
            result.reasons.Add("No primary canExitRight target");
            return result;
        }

        VehicleData primary = level.vehicles[primaryIndex];
        result.primaryTargetName = VehicleLabel(primary, primaryIndex);
        result.primaryTargetPos = primary.gridPosition;
        result.primaryTargetIndex = primaryIndex;

        if (primary.orientation != VehicleController.VehicleOrientation.Horizontal ||
            primary.gridPosition.y != exitRow)
        {
            result.reasons.Add(
                "Primary target not Horizontal on exitRow — MultiTarget v1 layout invalid"
            );
            return result;
        }

        List<int> candidates = FindSecondTargetCandidateIndices(
            level.vehicles,
            primaryIndex,
            exitRow
        );

        if (candidates.Count == 0)
        {
            result.reasons.Add(
                "No second horizontal length-2 vehicle on exit row"
            );
            return result;
        }

        int bestScore = -1;
        Result bestPartial = result;

        for (int c = 0; c < candidates.Count; c++)
        {
            int candidateIndex = candidates[c];
            Result scored = ScoreCandidate(
                level,
                gw,
                gh,
                exitRow,
                primaryIndex,
                candidateIndex,
                result
            );

            if (scored.score > bestScore)
            {
                bestScore = scored.score;
                bestPartial = scored;
            }
        }

        return bestPartial;
    }

    private static Result ScoreCandidate(
        LevelData level,
        int gw,
        int gh,
        int exitRow,
        int primaryIndex,
        int candidateIndex,
        Result template)
    {
        Result result = template;
        result.reasons = new List<string>();

        VehicleData primary = level.vehicles[primaryIndex];
        VehicleData candidate = level.vehicles[candidateIndex];

        result.bestCandidateName = VehicleLabel(candidate, candidateIndex);
        result.bestCandidatePos = candidate.gridPosition;
        result.bestCandidateIndex = candidateIndex;
        result.primaryTargetIndex = primaryIndex;

        int[,] occupancy = BuildOccupancy(level.vehicles, gw, gh);
        ClearVehicle(occupancy, primary, gw, gh);

        int blockers = CountRouteBlockers(
            occupancy,
            candidate,
            gw,
            out List<int> blockerIndices
        );
        result.blockersAfterFirstRescue = blockers;

        bool immediate = blockers == 0 && CanSlideFullyRight(occupancy, candidate, gw);
        result.immediateExit = immediate;

        int score = 40; // base for having a valid second candidate

        // --- Immediate exit penalty ---
        if (immediate)
        {
            score -= 45;
            result.reasons.Add(
                "- Second target becomes immediately escapable after first rescue"
            );
        }
        else
        {
            result.reasons.Add("+ Not immediately escapable after first rescue");
        }

        // --- Blocker count ---
        if (blockers == 0 && !immediate)
        {
            score -= 10;
            result.reasons.Add("- No blockers on exit path (edge case)");
        }
        else if (blockers == 1)
        {
            score += 8;
            result.reasons.Add("+ 1 blocker after first rescue");
        }
        else if (blockers == 2)
        {
            score += 18;
            result.reasons.Add("+ 2 independent blockers");
        }
        else if (blockers >= 3)
        {
            score += 26;
            result.reasons.Add("+ " + blockers + " independent blockers");
        }

        // --- Blocker quality ---
        int qualityPoints = 0;
        for (int i = 0; i < blockerIndices.Count; i++)
        {
            int bi = blockerIndices[i];
            if (bi < 0 || bi >= level.vehicles.Count)
            {
                continue;
            }

            VehicleData blocker = level.vehicles[bi];
            if (blocker == null)
            {
                continue;
            }

            if (blocker.lengthInCells >= 3)
            {
                qualityPoints += 1;
            }

            int freeMoves = CountFreeAxisCells(occupancy, blocker, gw, gh);
            if (freeMoves <= 1)
            {
                qualityPoints += 2;
            }
            else if (freeMoves <= 2)
            {
                qualityPoints += 1;
            }

            if (blocker.orientation == VehicleController.VehicleOrientation.Vertical)
            {
                qualityPoints += 1;
            }
        }

        qualityPoints = Mathf.Min(12, qualityPoints);
        score += qualityPoints;
        if (qualityPoints > 0)
        {
            result.reasons.Add("+ Blocker quality +" + qualityPoints);
        }

        // --- Distance to exit (cells from candidate right edge to board edge) ---
        int candidateRight = candidate.gridPosition.x + candidate.lengthInCells - 1;
        int gapToEdge = (gw - 1) - candidateRight;
        if (gapToEdge >= 4)
        {
            score += 12;
            result.reasons.Add("+ Candidate far from exit");
        }
        else if (gapToEdge >= 2)
        {
            score += 6;
            result.reasons.Add("+ Candidate moderate distance to exit");
        }
        else
        {
            score -= 6;
            result.reasons.Add("- Candidate already near exit");
        }

        // --- Spacing vs primary ---
        int primaryLeft = primary.gridPosition.x;
        int candidateRightEdge = candidateRight;
        int gapBetween = primaryLeft - candidateRightEdge - 1;
        if (gapBetween <= 0)
        {
            score -= 10;
            result.reasons.Add("- Targets relatively close / adjacent");
        }
        else if (gapBetween == 1)
        {
            score -= 4;
            result.reasons.Add("- Targets relatively close");
        }
        else if (gapBetween >= 2)
        {
            score += 6;
            result.reasons.Add("+ Spacing between targets");
        }

        // --- Candidate must be left of primary for typical flow ---
        if (candidate.gridPosition.x >= primary.gridPosition.x)
        {
            score -= 8;
            result.reasons.Add("- Candidate not left of primary target");
        }

        // --- Base minimumMoves (light) ---
        int moves = level.minimumMoves;
        if (moves <= 0)
        {
            score -= 4;
            result.reasons.Add("- Missing minimumMoves");
        }
        else if (moves <= 4)
        {
            score -= 8;
            result.reasons.Add("- Base level very easy (low minMoves)");
        }
        else if (moves <= 8)
        {
            score += 2;
            result.reasons.Add("+ Base minMoves modest");
        }
        else if (moves <= 14)
        {
            score += 8;
            result.reasons.Add("+ Base minMoves solid");
        }
        else
        {
            score += 10;
            result.reasons.Add("+ Base minMoves high");
        }

        result.score = Mathf.Clamp(score, 0, 100);
        result.category = Categorize(result.score, immediate);
        return result;
    }

    private static Category Categorize(int score, bool immediate)
    {
        if (immediate && score < ScoreMaybeMin)
        {
            return Category.Poor;
        }

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

    /// <summary>
    /// v1 preference: Horizontal length-2 on exitRow, not primary, left of exit edge.
    /// </summary>
    private static List<int> FindSecondTargetCandidateIndices(
        List<VehicleData> vehicles,
        int primaryIndex,
        int exitRow)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < vehicles.Count; i++)
        {
            if (i == primaryIndex)
            {
                continue;
            }

            VehicleData v = vehicles[i];
            if (v == null)
            {
                continue;
            }

            if (v.orientation != VehicleController.VehicleOrientation.Horizontal)
            {
                continue;
            }

            if (v.lengthInCells != 2)
            {
                continue;
            }

            if (v.gridPosition.y != exitRow)
            {
                continue;
            }

            list.Add(i);
        }

        return list;
    }

    private static int[,] BuildOccupancy(List<VehicleData> vehicles, int gw, int gh)
    {
        int[,] occ = new int[gw, gh];
        for (int x = 0; x < gw; x++)
        {
            for (int y = 0; y < gh; y++)
            {
                occ[x, y] = -1;
            }
        }

        for (int i = 0; i < vehicles.Count; i++)
        {
            VehicleData v = vehicles[i];
            if (v == null)
            {
                continue;
            }

            StampVehicle(occ, v, i, gw, gh);
        }

        return occ;
    }

    private static void StampVehicle(
        int[,] occ,
        VehicleData v,
        int index,
        int gw,
        int gh)
    {
        bool horizontal = v.orientation == VehicleController.VehicleOrientation.Horizontal;
        for (int k = 0; k < v.lengthInCells; k++)
        {
            int x = v.gridPosition.x + (horizontal ? k : 0);
            int y = v.gridPosition.y + (horizontal ? 0 : k);
            if (x >= 0 && x < gw && y >= 0 && y < gh)
            {
                occ[x, y] = index;
            }
        }
    }

    private static void ClearVehicle(int[,] occ, VehicleData v, int gw, int gh)
    {
        bool horizontal = v.orientation == VehicleController.VehicleOrientation.Horizontal;
        for (int k = 0; k < v.lengthInCells; k++)
        {
            int x = v.gridPosition.x + (horizontal ? k : 0);
            int y = v.gridPosition.y + (horizontal ? 0 : k);
            if (x >= 0 && x < gw && y >= 0 && y < gh)
            {
                occ[x, y] = -1;
            }
        }
    }

    /// <summary>
    /// Distinct vehicle indices blocking cells to the right of candidate until board edge.
    /// </summary>
    private static int CountRouteBlockers(
        int[,] occ,
        VehicleData candidate,
        int gw,
        out List<int> blockerIndices)
    {
        HashSet<int> set = new HashSet<int>();
        int y = candidate.gridPosition.y;
        int startX = candidate.gridPosition.x + candidate.lengthInCells;

        for (int x = startX; x < gw; x++)
        {
            int id = occ[x, y];
            if (id >= 0)
            {
                set.Add(id);
            }
        }

        blockerIndices = new List<int>(set);
        return set.Count;
    }

    private static bool CanSlideFullyRight(int[,] occ, VehicleData candidate, int gw)
    {
        int y = candidate.gridPosition.y;
        int startX = candidate.gridPosition.x + candidate.lengthInCells;
        for (int x = startX; x < gw; x++)
        {
            if (occ[x, y] >= 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Free cells along vehicle axis before hitting another vehicle / edge.
    /// </summary>
    private static int CountFreeAxisCells(int[,] occ, VehicleData v, int gw, int gh)
    {
        bool horizontal = v.orientation == VehicleController.VehicleOrientation.Horizontal;
        int free = 0;

        if (horizontal)
        {
            // Left
            for (int x = v.gridPosition.x - 1; x >= 0; x--)
            {
                if (occ[x, v.gridPosition.y] >= 0)
                {
                    break;
                }

                free++;
            }

            // Right
            int right = v.gridPosition.x + v.lengthInCells;
            for (int x = right; x < gw; x++)
            {
                if (occ[x, v.gridPosition.y] >= 0)
                {
                    break;
                }

                free++;
            }
        }
        else
        {
            for (int y = v.gridPosition.y - 1; y >= 0; y--)
            {
                if (occ[v.gridPosition.x, y] >= 0)
                {
                    break;
                }

                free++;
            }

            int bottom = v.gridPosition.y + v.lengthInCells;
            for (int y = bottom; y < gh; y++)
            {
                if (occ[v.gridPosition.x, y] >= 0)
                {
                    break;
                }

                free++;
            }
        }

        return free;
    }

    private static string VehicleLabel(VehicleData v, int index)
    {
        if (v != null && !string.IsNullOrEmpty(v.vehicleName))
        {
            return v.vehicleName;
        }

        return "Vehicle " + index;
    }

    public static string FormatResultsLog(List<Result> results, bool onlyGood)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== MultiTarget Suitability (read-only) ===");
        sb.AppendLine(
            "Heuristics only — no solver. Length-2 horizontal on exitRow."
        );

        int shown = 0;
        for (int i = 0; i < results.Count; i++)
        {
            Result r = results[i];
            if (onlyGood && r.category != Category.Good)
            {
                continue;
            }

            shown++;
            sb.AppendLine();
            sb.AppendLine(
                "Level " + r.levelNumber + " (" + r.assetName + ") | " +
                r.gridWidth + "x" + r.gridHeight +
                " | exitRow " + r.exitRow +
                " | minMoves " + r.minimumMoves
            );
            sb.AppendLine(
                "Primary: " + r.primaryTargetName + " @ " + r.primaryTargetPos
            );
            sb.AppendLine(
                "Best second: " + r.bestCandidateName + " @ " + r.bestCandidatePos
            );
            sb.AppendLine(
                "Blockers after first rescue: " + r.blockersAfterFirstRescue +
                " | Immediate exit: " + (r.immediateExit ? "Yes" : "No")
            );
            sb.AppendLine(
                "Score: " + r.score + " | " + r.category
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

    private static LevelDatabase LoadMainLevelDatabase()
    {
        string[] guids = AssetDatabase.FindAssets(DatabaseAssetFilter);
        if (guids == null || guids.Length == 0)
        {
            Debug.LogError("MultiTargetSuitability: geen MainLevelDatabase gevonden.");
            return null;
        }

        if (guids.Length > 1)
        {
            Debug.LogError(
                "MultiTargetSuitability: meerdere MainLevelDatabase assets."
            );
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);
    }

    [MenuItem("RushOut/Analyze MultiTarget Candidates")]
    public static void MenuAnalyze()
    {
        List<Result> results = AnalyzeMainLevelDatabase();
        Debug.Log(FormatResultsLog(results, onlyGood: false));
    }
}

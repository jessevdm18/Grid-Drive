using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Read-only deep validation for Hard local #20 / Level_096 (MultiTarget).
/// Menu: RushOut → Solver → Deep Validate Level_096 (Hard 20)
/// Does NOT write LevelData.
/// </summary>
public static class Level096DeepValidate
{
    private const string AssetName = "Level_096";
    private const int NormalMaxStates = ProductionLevelPlayability.DefaultMaxStates;
    private const int DeepMaxStates = ProductionLevelPlayability.DeepMaxStates; // 2_000_000

    [MenuItem("RushOut/Solver/Deep Validate Level_096 (Hard 20)", priority = 57)]
    public static void Run()
    {
        LevelDatabase database = LoadMainDb();
        if (database == null)
        {
            return;
        }

        int dbIndex = LevelDifficultyOrder.GetDatabaseIndexForDifficultyDisplayNumber(
            database,
            LevelDifficulty.Hard,
            20
        );

        StringBuilder sb = new StringBuilder(48 * 1024);
        sb.AppendLine("=== DEEP VALIDATE Level_096 / Hard local #20 ===");
        sb.AppendLine("No LevelData writes.");
        sb.AppendLine();

        LevelData level = null;
        if (dbIndex >= 0 && dbIndex < database.levels.Count)
        {
            level = database.levels[dbIndex];
        }

        if (level == null || level.name != AssetName)
        {
            // Fallback: find by asset name.
            level = FindLevelByName(database, AssetName, out dbIndex);
        }

        if (level == null)
        {
            sb.AppendLine("FAIL: " + AssetName + " not found.");
            Finish(sb);
            return;
        }

        int local = LevelDifficultyOrder.GetDifficultyDisplayNumber(database, dbIndex);
        List<int> targets = CollectTargets(level);

        sb.AppendLine("Asset=" + level.name);
        sb.AppendLine("DbIndex=" + dbIndex);
        sb.AppendLine("Difficulty=" + level.difficulty);
        sb.AppendLine("LocalNumber=" + local);
        sb.AppendLine("Objective=" + level.objectiveType);
        sb.AppendLine("StoredMinMoves=" + level.minimumMoves);
        sb.AppendLine(
            "Grid=" + level.ResolvedGridWidth + "x" + level.ResolvedGridHeight +
            " exitRow=" + level.exitRow
        );
        sb.AppendLine("TargetCount=" + targets.Count);
        for (int i = 0; i < targets.Count; i++)
        {
            int ti = targets[i];
            VehicleData v = level.vehicles[ti];
            sb.AppendLine(
                "  Target " + (char)('A' + i) +
                " idx=" + ti +
                " name=" + (v != null ? v.vehicleName : "?") +
                " pos=" + (v != null ? v.gridPosition.ToString() : "?")
            );
        }

        RushOutSolver.SolveConstraints constraints =
            ObjectiveSolveConstraints.FromLevelData(level);

        sb.AppendLine();
        sb.AppendLine("--- 1) NORMAL SOLVE (budget " + NormalMaxStates + ") ---");
        AppendSolveBlock(sb, level, constraints, NormalMaxStates, "NORMAL");

        sb.AppendLine();
        sb.AppendLine("--- 2) DEEP SOLVE (budget " + DeepMaxStates + ") ---");
        SolveSnapshot deep = RunSolve(level, constraints, DeepMaxStates);
        AppendSolveSnapshot(sb, deep, "DEEP");

        sb.AppendLine();
        sb.AppendLine("--- 3) RESULT ---");
        string status = ClassifyStatus(deep);
        sb.AppendLine("Status=" + status);
        sb.AppendLine("StatesExplored=" + deep.result.statesExplored);
        sb.AppendLine("DiscoveredStates=" + deep.result.discoveredStates);
        sb.AppendLine("ElapsedSeconds=" + deep.elapsedSeconds.ToString("0.000"));
        sb.AppendLine(
            "PlayerMoves=" +
            (deep.result.solvable ? deep.result.minimumMoves.ToString() : "n/a")
        );

        bool releaseSafe = false;
        string hintDetail = string.Empty;

        if (status == "VALID")
        {
            sb.AppendLine();
            sb.AppendLine("--- 4) PAR CHECK ---");
            int recomputed = deep.result.minimumMoves;
            sb.AppendLine("StoredMinMoves=" + level.minimumMoves);
            sb.AppendLine("RecomputedPlayerMoves=" + recomputed);
            sb.AppendLine(
                "PAR20=" +
                (level.minimumMoves == 20 && recomputed == 20
                    ? "YES"
                    : "NO (stored=" + level.minimumMoves +
                      " recomputed=" + recomputed + ")")
            );
            if (recomputed != level.minimumMoves)
            {
                sb.AppendLine(
                    "NOTE: metadata stale preview " + level.minimumMoves +
                    "→" + recomputed + " — NO auto write."
                );
            }

            sb.AppendLine();
            sb.AppendLine("--- OPTIMAL PATH ---");
            AppendOptimalPath(sb, deep.result, targets, level);

            sb.AppendLine();
            sb.AppendLine("--- 5) VERIFY HINT PATH ---");
            // Use deep budget so hint-path steps are not false-TIMEOUT on this heavy level.
            bool hintOk = ProductionLevelPlayability.VerifyHintPath(
                level,
                out hintDetail,
                DeepMaxStates,
                512
            );
            sb.AppendLine("HintPath=" + (hintOk ? "PASS" : "FAIL"));
            sb.AppendLine("HintPathDetail=" + hintDetail);
            releaseSafe = hintOk;
            sb.AppendLine(
                "ReleaseSafe=" +
                (releaseSafe ? "YES (VALID + HintPath PASS)" : "NO")
            );
        }
        else if (status == "TIMEOUT")
        {
            sb.AppendLine();
            sb.AppendLine("--- 6) STILL UNPROVEN AT 2M ---");
            sb.AppendLine("RELEASE BLOCKER / UNPROVEN");
            sb.AppendLine(
                "Advice: remove/replace Level_096 for v1. " +
                "Do NOT raise global solver budget for all levels."
            );
        }
        else if (status == "UNSOLVABLE")
        {
            sb.AppendLine();
            sb.AppendLine("--- 7) UNSOLVABLE ---");
            sb.AppendLine("Reject/replace Level_096. No Classic fallback for release.");
        }

        sb.AppendLine();
        sb.AppendLine("--- RELEASE RECOMMENDATION ---");
        if (status == "VALID" && releaseSafe)
        {
            sb.AppendLine(
                recomputedParLine(level, deep) +
                " Keep if PAR metadata updated when needed; HintPath PASS."
            );
        }
        else if (status == "VALID" && !releaseSafe)
        {
            sb.AppendLine(
                "VALID solve but HintPath FAIL — Needs Review / not release-safe. " +
                hintDetail
            );
        }
        else if (status == "TIMEOUT")
        {
            sb.AppendLine("UNPROVEN at 2M — REPLACE for v1 release.");
        }
        else
        {
            sb.AppendLine("UNSOLVABLE — REPLACE for v1 release.");
        }

        sb.AppendLine();
        sb.AppendLine("No LevelData / database writes performed.");
        Finish(sb);
    }

    private static string recomputedParLine(LevelData level, SolveSnapshot deep)
    {
        return "VALID. RecomputedPlayerMoves=" + deep.result.minimumMoves +
            " Stored=" + level.minimumMoves + ".";
    }

    private static string ClassifyStatus(SolveSnapshot snap)
    {
        if (snap.result.searchLimitReached)
        {
            return "TIMEOUT";
        }

        if (snap.result.solvable &&
            snap.result.solution != null &&
            snap.result.solution.Count > 0)
        {
            return "VALID";
        }

        return "UNSOLVABLE";
    }

    private static void AppendSolveBlock(
        StringBuilder sb,
        LevelData level,
        RushOutSolver.SolveConstraints constraints,
        int maxStates,
        string label)
    {
        SolveSnapshot snap = RunSolve(level, constraints, maxStates);
        AppendSolveSnapshot(sb, snap, label);
    }

    private static SolveSnapshot RunSolve(
        LevelData level,
        RushOutSolver.SolveConstraints constraints,
        int maxStates)
    {
        // maxDiscovered mirrors maxStates (solver default when -1).
        Stopwatch sw = Stopwatch.StartNew();
        RushOutSolver.SolverResult result = RushOutSolver.SolveLevelData(
            level,
            maxStates,
            maxStates,
            constraints
        );
        sw.Stop();
        return new SolveSnapshot
        {
            result = result,
            elapsedSeconds = sw.Elapsed.TotalSeconds,
            budgetStates = maxStates
        };
    }

    private static void AppendSolveSnapshot(
        StringBuilder sb,
        SolveSnapshot snap,
        string label)
    {
        RushOutSolver.SolverResult r = snap.result;
        bool exploredLimit =
            r.searchLimitReached &&
            r.searchLimitReason == RushOutSolver.SearchLimitExplored;
        bool discoveredLimit =
            r.searchLimitReached &&
            r.searchLimitReason == RushOutSolver.SearchLimitDiscovered;

        sb.AppendLine("[" + label + "]");
        sb.AppendLine("BudgetMaxStates=" + snap.budgetStates);
        sb.AppendLine("solvable=" + r.solvable);
        sb.AppendLine("searchLimitReached=" + r.searchLimitReached);
        sb.AppendLine(
            "searchLimitReason=" +
            (string.IsNullOrEmpty(r.searchLimitReason) ? "(none)" : r.searchLimitReason)
        );
        sb.AppendLine("StateLimitReached=" + exploredLimit);
        sb.AppendLine("DiscoveredLimitReached=" + discoveredLimit);
        sb.AppendLine("QueueLimitReached=" + discoveredLimit + " (same cap as discovered)");
        sb.AppendLine("TimeLimitReached=False (solver has no wall-clock limit)");
        sb.AppendLine("Cancellation=False");
        sb.AppendLine(
            "OtherSearchLimit=" +
            (r.searchLimitReached && !exploredLimit && !discoveredLimit
                ? (r.searchLimitReason ?? "unknown")
                : "False")
        );
        sb.AppendLine("StatesExplored=" + r.statesExplored);
        sb.AppendLine("DiscoveredStates=" + r.discoveredStates);
        sb.AppendLine("QueuePeakSize=" + r.queuePeakSize);
        sb.AppendLine("ElapsedSeconds=" + snap.elapsedSeconds.ToString("0.000"));
        if (r.solvable)
        {
            sb.AppendLine("PlayerMoves=" + r.minimumMoves);
            sb.AppendLine(
                "CountPlayerMovesCheck=" +
                RushOutSolver.CountPlayerMoves(r.solution, multiTarget: true)
            );
        }
    }

    private static void AppendOptimalPath(
        StringBuilder sb,
        RushOutSolver.SolverResult result,
        List<int> targetIndices,
        LevelData level)
    {
        int playerStep = 0;
        int[] lastPlayer = new int[targetIndices.Count];
        int[] autoAfter = new int[targetIndices.Count];
        for (int i = 0; i < targetIndices.Count; i++)
        {
            lastPlayer[i] = -1;
            autoAfter[i] = -1;
        }

        HashSet<int> exitedTargets = new HashSet<int>();

        for (int i = 0; i < result.solution.Count; i++)
        {
            RushOutSolver.SolverMove move = result.solution[i];
            bool counts = RushOutSolver.IsPlayerHintMove(move, multiTarget: true);
            bool zeroExit = move.exitsBoard && move.fromPosition == move.toPosition;
            int slot = IndexOf(targetIndices, move.vehicleIndex);

            if (counts)
            {
                playerStep++;
                if (slot >= 0)
                {
                    lastPlayer[slot] = playerStep;
                }
            }
            else if (zeroExit && slot >= 0)
            {
                autoAfter[slot] = playerStep;
            }

            if (move.exitsBoard && targetIndices.Contains(move.vehicleIndex))
            {
                exitedTargets.Add(move.vehicleIndex);
            }

            string tag = counts
                ? ("PLAYER #" + playerStep)
                : (zeroExit ? "AUTO_EXIT +0" : "OTHER");

            sb.AppendLine(
                "Step " + (i + 1) + " [" + tag + "] " +
                move.vehicleName + " idx=" + move.vehicleIndex +
                " (" + move.fromPosition.x + "," + move.fromPosition.y + ")→(" +
                move.toPosition.x + "," + move.toPosition.y + ")" +
                " exitsBoard=" + move.exitsBoard
            );
        }

        sb.AppendLine();
        for (int i = 0; i < targetIndices.Count; i++)
        {
            int ti = targetIndices[i];
            string name = level.vehicles[ti] != null
                ? level.vehicles[ti].vehicleName
                : ("idx" + ti);
            sb.AppendLine(
                "Target " + (char)('A' + i) + " (" + name + "): " +
                "final player move toward exit = step " +
                (lastPlayer[i] >= 0 ? lastPlayer[i].ToString() : "NONE") +
                "; automatic exit = +0" +
                (autoAfter[i] >= 0
                    ? " (after player step " + autoAfter[i] + ")"
                    : "")
            );
        }

        sb.AppendLine(
            "BothTargetsExitedInPath=" +
            (exitedTargets.Count >= targetIndices.Count)
        );
    }

    private static int IndexOf(List<int> list, int value)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == value)
            {
                return i;
            }
        }

        return -1;
    }

    private static List<int> CollectTargets(LevelData level)
    {
        List<int> list = new List<int>();
        if (level == null || level.vehicles == null)
        {
            return list;
        }

        for (int i = 0; i < level.vehicles.Count; i++)
        {
            if (level.vehicles[i] != null && level.vehicles[i].canExitRight)
            {
                list.Add(i);
            }
        }

        return list;
    }

    private static LevelData FindLevelByName(
        LevelDatabase database,
        string name,
        out int dbIndex)
    {
        dbIndex = -1;
        if (database == null || database.levels == null)
        {
            return null;
        }

        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level != null && level.name == name)
            {
                dbIndex = i;
                return level;
            }
        }

        return null;
    }

    private static void Finish(StringBuilder sb)
    {
        string text = sb.ToString();
        Debug.Log(text);
        string outPath = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "RushOut_Level096_DeepValidate.txt"
        );
        System.IO.File.WriteAllText(outPath, text);
        Debug.Log("Deep validate written to: " + outPath);
        EditorUtility.DisplayDialog(
            "Deep Validate Level_096",
            "Done. See Console and:\n" + outPath + "\n\nNo LevelData writes.",
            "OK"
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

    private struct SolveSnapshot
    {
        public RushOutSolver.SolverResult result;
        public double elapsedSeconds;
        public int budgetStates;
    }
}

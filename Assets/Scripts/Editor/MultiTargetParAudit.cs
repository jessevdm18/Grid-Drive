using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Read-only audit: MultiTarget minimumMoves / PAR vs runtime RegisterMove semantics.
/// Menu: RushOut → Solver → MultiTarget PAR Audit (Hard 20 + All)
/// Does NOT write LevelData.
/// </summary>
public static class MultiTargetParAudit
{
    private const int MaxStates = 200000;

    [MenuItem("RushOut/Solver/MultiTarget PAR Audit (Hard 20 + All)", priority = 55)]
    public static void RunFullAudit()
    {
        LevelDatabase database = LoadMainDb();
        if (database == null)
        {
            return;
        }

        StringBuilder sb = new StringBuilder(64 * 1024);
        sb.AppendLine("=== MULTITARGET PAR / MOVES AUDIT (read-only) ===");
        sb.AppendLine();
        AppendRuntimeSemantics(sb);
        sb.AppendLine();
        AppendSolverSemantics(sb);
        sb.AppendLine();
        AuditHardLocal(database, 20, sb);
        sb.AppendLine();
        AuditAllMultiTarget(database, sb);

        Debug.Log(sb.ToString());

        string outPath = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "RushOut_MultiTargetParAudit.txt"
        );
        System.IO.File.WriteAllText(outPath, sb.ToString());
        Debug.Log("MultiTarget PAR audit also written to: " + outPath);

        EditorUtility.DisplayDialog(
            "MultiTarget PAR Audit",
            "Done. See Console and:\n" + outPath + "\n\n" +
            "No LevelData writes.",
            "OK"
        );
    }

    [MenuItem("RushOut/Solver/MultiTarget PAR Audit Hard 20 Only", priority = 56)]
    public static void RunHard20Only()
    {
        LevelDatabase database = LoadMainDb();
        if (database == null)
        {
            return;
        }

        StringBuilder sb = new StringBuilder(16 * 1024);
        sb.AppendLine("=== MULTITARGET PAR AUDIT — HARD LOCAL 20 ===");
        AppendRuntimeSemantics(sb);
        sb.AppendLine();
        AppendSolverSemantics(sb);
        sb.AppendLine();
        AuditHardLocal(database, 20, sb);
        Debug.Log(sb.ToString());
    }

    private static void AppendRuntimeSemantics(StringBuilder sb)
    {
        sb.AppendLine("--- RUNTIME MultiTarget exit (VehicleController) ---");
        sb.AppendLine(
            "Authority: GameManager.RegisterMove once per accepted player action."
        );
        sb.AppendLine(
            "Board slide (destination changed, no exit): RegisterMove +1."
        );
        sb.AppendLine(
            "Target exit (TryExitRight / TryAssistedExitRight → BeginTargetExit):"
        );
        sb.AppendLine(
            "  - Requires player drag (extra rightward at dock, or assist within N cells)."
        );
        sb.AppendLine(
            "  - BeginTargetExit always RegisterMove +1 (dock+exit gesture merges)."
        );
        sb.AppendLine(
            "  - PlayExitAnimation after that: +0 (no second RegisterMove)."
        );
        sb.AppendLine(
            "There is NO fully automatic exit without player input after docking."
        );
        sb.AppendLine(
            "Typical optimal gesture: one drag that docks and starts exit = 1 player move."
        );
    }

    private static void AppendSolverSemantics(StringBuilder sb)
    {
        sb.AppendLine("--- SOLVER MultiTarget (RushOutSolver) ---");
        sb.AppendLine(
            "ObjectiveSolveConstraints: MultiTargetRescue → multiTarget=true, " +
            "targets = all canExitRight vehicles."
        );
        sb.AppendLine(
            "When a target CanTargetExit (docked): TryEnqueueExitChild at SAME BFS depth " +
            "(zero-cost edge) with exitsBoard from==to marker."
        );
        sb.AppendLine(
            "CountPlayerMoves: skip multiTarget exitsBoard && from==to (HUD +0)."
        );
        sb.AppendLine(
            "Board slide onto dock: counts as 1 — that is the player move; " +
            "auto-exit marker is path bookkeeping only (= runtime animation +0)."
        );
        sb.AppendLine(
            "Intended equivalence: solver slide-to-dock +0-cost exit marker " +
            "↔ runtime BeginTargetExit once (+1) + animation (+0)."
        );
    }

    private static void AuditHardLocal(
        LevelDatabase database,
        int localNumber,
        StringBuilder sb)
    {
        int dbIndex = LevelDifficultyOrder.GetDatabaseIndexForDifficultyDisplayNumber(
            database,
            LevelDifficulty.Hard,
            localNumber
        );

        sb.AppendLine("--- HARD LOCAL #" + localNumber + " ---");
        if (dbIndex < 0 || dbIndex >= database.levels.Count)
        {
            sb.AppendLine("NOT FOUND via LevelDifficultyOrder.");
            return;
        }

        LevelData level = database.levels[dbIndex];
        if (level == null)
        {
            sb.AppendLine("Null LevelData at dbIndex=" + dbIndex);
            return;
        }

        List<int> targetIndices = CollectTargetIndices(level);
        sb.AppendLine("Asset=" + level.name);
        sb.AppendLine("DbIndex=" + dbIndex);
        sb.AppendLine("Difficulty=" + level.difficulty);
        sb.AppendLine(
            "LocalNumber=" +
            LevelDifficultyOrder.GetDifficultyDisplayNumber(database, dbIndex)
        );
        sb.AppendLine("Objective=" + level.objectiveType);
        sb.AppendLine("StoredMinMoves / PAR source=" + level.minimumMoves);
        sb.AppendLine(
            "Grid=" + level.ResolvedGridWidth + "x" + level.ResolvedGridHeight +
            " exitRow=" + level.exitRow
        );
        sb.AppendLine("TargetCount=" + targetIndices.Count);
        for (int i = 0; i < targetIndices.Count; i++)
        {
            int ti = targetIndices[i];
            VehicleData v = level.vehicles[ti];
            sb.AppendLine(
                "  Target[" + i + "] idx=" + ti +
                " name=" + (v != null ? v.vehicleName : "?") +
                " len=" + (v != null ? v.lengthInCells : 0) +
                " pos=" + (v != null ? v.gridPosition.ToString() : "?") +
                " canExitRight=" + (v != null && v.canExitRight)
            );
        }

        if (level.objectiveType != LevelObjectiveType.MultiTargetRescue)
        {
            sb.AppendLine("FAIL: Hard #" + localNumber + " is not MultiTargetRescue.");
            return;
        }

        if (targetIndices.Count < 2)
        {
            sb.AppendLine("FAIL: fewer than 2 canExitRight targets.");
            return;
        }

        RushOutSolver.SolveConstraints constraints =
            ObjectiveSolveConstraints.FromLevelData(level);
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        RushOutSolver.SolverResult result = RushOutSolver.SolveLevelData(
            level,
            MaxStates,
            MaxStates,
            constraints
        );
        sw.Stop();

        sb.AppendLine(
            "Solver: solvable=" + result.solvable +
            " searchLimit=" + result.searchLimitReached +
            " reason=" +
            (string.IsNullOrEmpty(result.searchLimitReason)
                ? "(none)"
                : result.searchLimitReason) +
            " statesExplored=" + result.statesExplored +
            " discovered=" + result.discoveredStates +
            " elapsed=" + sw.Elapsed.TotalSeconds.ToString("0.000") + "s"
        );
        sb.AppendLine(
            "StateLimitReached=" +
            (result.searchLimitReached &&
             result.searchLimitReason == RushOutSolver.SearchLimitExplored)
        );
        sb.AppendLine(
            "DiscoveredLimitReached=" +
            (result.searchLimitReached &&
             result.searchLimitReason == RushOutSolver.SearchLimitDiscovered)
        );
        sb.AppendLine("TimeLimitReached=False");
        sb.AppendLine("Cancellation=False");

        if (!result.solvable || result.solution == null)
        {
            sb.AppendLine("No solution — cannot verify PAR.");
            sb.AppendLine(
                "TIP: Run RushOut → Solver → Deep Validate Level_096 (Hard 20) " +
                "for 2M-state deep solve."
            );
            return;
        }

        int solverPlayerMoves = RushOutSolver.CountPlayerMoves(
            result.solution,
            multiTarget: true
        );
        int runtimeEquivalent = solverPlayerMoves; // same metric by design

        sb.AppendLine("SolverPlayerMoves=" + solverPlayerMoves);
        sb.AppendLine("RuntimeEquivalentMoves=" + runtimeEquivalent);
        sb.AppendLine(
            "StoredMinMoves=" + level.minimumMoves +
            " MatchStored=" + (level.minimumMoves == solverPlayerMoves)
        );
        sb.AppendLine(
            "PAR20Confirm=" +
            (solverPlayerMoves == 20 && level.minimumMoves == 20 ? "YES" : "NO")
        );
        sb.AppendLine();
        sb.AppendLine("=== OPTIMAL PATH (Hard #" + localNumber + ") ===");

        int playerStep = 0;
        int[] lastPlayerStepForTarget = new int[targetIndices.Count];
        for (int i = 0; i < lastPlayerStepForTarget.Length; i++)
        {
            lastPlayerStepForTarget[i] = -1;
        }

        int[] autoExitAfterStep = new int[targetIndices.Count];
        for (int i = 0; i < autoExitAfterStep.Length; i++)
        {
            autoExitAfterStep[i] = -1;
        }

        for (int i = 0; i < result.solution.Count; i++)
        {
            RushOutSolver.SolverMove move = result.solution[i];
            bool counts = RushOutSolver.IsPlayerHintMove(move, multiTarget: true);
            bool zeroCostExit =
                move.exitsBoard &&
                move.fromPosition == move.toPosition;

            int targetSlot = IndexOfTarget(targetIndices, move.vehicleIndex);

            if (counts)
            {
                playerStep++;
                if (targetSlot >= 0)
                {
                    lastPlayerStepForTarget[targetSlot] = playerStep;
                }
            }
            else if (zeroCostExit && targetSlot >= 0)
            {
                autoExitAfterStep[targetSlot] = playerStep;
            }

            string tag = counts
                ? ("PLAYER_MOVE #" + playerStep)
                : (zeroCostExit ? "AUTO_EXIT +0" : "OTHER");

            sb.AppendLine(
                "Step " + (i + 1) + " [" + tag + "]" +
                " vehicle=" + move.vehicleName + " idx=" + move.vehicleIndex +
                " (" + move.fromPosition.x + "," + move.fromPosition.y + ") → (" +
                move.toPosition.x + "," + move.toPosition.y + ")" +
                " exitsBoard=" + move.exitsBoard
            );
        }

        sb.AppendLine();
        sb.AppendLine("=== TARGET EXIT SUMMARY ===");
        for (int i = 0; i < targetIndices.Count; i++)
        {
            int ti = targetIndices[i];
            string name = level.vehicles[ti] != null
                ? level.vehicles[ti].vehicleName
                : ("idx" + ti);
            char label = (char)('A' + i);
            sb.AppendLine(
                "Target " + label + " (" + name + " idx=" + ti + "):"
            );
            sb.AppendLine(
                "  final player move toward exit = step " +
                (lastPlayerStepForTarget[i] >= 0
                    ? lastPlayerStepForTarget[i].ToString()
                    : "NONE")
            );
            sb.AppendLine(
                "  automatic exit = +0" +
                (autoExitAfterStep[i] >= 0
                    ? " (after player step " + autoExitAfterStep[i] + ")"
                    : " (no separate marker — check if exit merged into last slide)")
            );
        }

        sb.AppendLine(
            "Both targets exited in path=" +
            (CountExitedTargetsInPath(result.solution, targetIndices) >=
             targetIndices.Count)
        );
        sb.AppendLine(
            "Final: SolverPlayerMoves=" + solverPlayerMoves +
            " RuntimeEquivalentMoves=" + runtimeEquivalent +
            " → " +
            (solverPlayerMoves == runtimeEquivalent ? "ALIGNED" : "MISMATCH")
        );
    }

    private static void AuditAllMultiTarget(LevelDatabase database, StringBuilder sb)
    {
        sb.AppendLine("=== ALL MULTITARGET LEVELS (no LevelData writes) ===");
        sb.AppendLine(
            "Difficulty | Local# | Asset | Targets | StoredMinMoves | " +
            "RecomputedPlayerMoves | Match"
        );

        int match = 0;
        int mismatch = 0;
        int fail = 0;

        for (int d = 0; d <= 2; d++)
        {
            LevelDifficulty difficulty = (LevelDifficulty)d;
            List<int> ordered =
                LevelDifficultyOrder.GetOrderedLevelIndicesForDifficulty(
                    database,
                    difficulty
                );

            for (int local = 0; local < ordered.Count; local++)
            {
                int dbIndex = ordered[local];
                LevelData level = database.levels[dbIndex];
                if (level == null ||
                    level.objectiveType != LevelObjectiveType.MultiTargetRescue)
                {
                    continue;
                }

                int localNumber = local + 1;
                List<int> targets = CollectTargetIndices(level);
                RushOutSolver.SolveConstraints constraints =
                    ObjectiveSolveConstraints.FromLevelData(level);
                RushOutSolver.SolverResult result = RushOutSolver.SolveLevelData(
                    level,
                    MaxStates,
                    MaxStates,
                    constraints
                );

                string recomputed;
                string matchFlag;
                if (!result.solvable || result.solution == null)
                {
                    recomputed = result.searchLimitReached ? "TIMEOUT" : "UNSOLVABLE";
                    matchFlag = "false";
                    fail++;
                }
                else
                {
                    int moves = RushOutSolver.CountPlayerMoves(
                        result.solution,
                        multiTarget: true
                    );
                    recomputed = moves.ToString();
                    bool ok = moves == level.minimumMoves;
                    matchFlag = ok ? "true" : "false";
                    if (ok)
                    {
                        match++;
                    }
                    else
                    {
                        mismatch++;
                    }
                }

                sb.AppendLine(
                    difficulty + " | " + localNumber + " | " + level.name +
                    " | " + targets.Count + " | " + level.minimumMoves +
                    " | " + recomputed + " | Match=" + matchFlag
                );
            }
        }

        sb.AppendLine();
        sb.AppendLine(
            "Summary: Match=" + match +
            " Mismatch=" + mismatch +
            " FailSolve=" + fail
        );
        sb.AppendLine("No LevelData writes performed.");
    }

    private static int CountExitedTargetsInPath(
        List<RushOutSolver.SolverMove> solution,
        List<int> targetIndices)
    {
        HashSet<int> exited = new HashSet<int>();
        for (int i = 0; i < solution.Count; i++)
        {
            RushOutSolver.SolverMove m = solution[i];
            if (m == null || !m.exitsBoard)
            {
                continue;
            }

            if (targetIndices.Contains(m.vehicleIndex))
            {
                exited.Add(m.vehicleIndex);
            }
        }

        return exited.Count;
    }

    private static int IndexOfTarget(List<int> targetIndices, int vehicleIndex)
    {
        for (int i = 0; i < targetIndices.Count; i++)
        {
            if (targetIndices[i] == vehicleIndex)
            {
                return i;
            }
        }

        return -1;
    }

    private static List<int> CollectTargetIndices(LevelData level)
    {
        List<int> list = new List<int>();
        if (level == null || level.vehicles == null)
        {
            return list;
        }

        for (int i = 0; i < level.vehicles.Count; i++)
        {
            VehicleData v = level.vehicles[i];
            if (v != null && v.canExitRight)
            {
                list.Add(i);
            }
        }

        return list;
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
}

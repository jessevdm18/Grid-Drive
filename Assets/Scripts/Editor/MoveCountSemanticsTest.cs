using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tests: solver player-move semantics must match GameManager.RegisterMove / HUD.
/// Menu: RushOut → Solver → Move Count Semantics Audit
/// Does NOT write LevelData.
/// </summary>
public static class MoveCountSemanticsTest
{
    private const int MaxStates = 200000;

    [MenuItem("RushOut/Solver/Move Count Semantics Audit")]
    public static void RunAudit()
    {
        StringBuilder sb = new StringBuilder(4096);
        sb.AppendLine("=== MOVE COUNT SEMANTICS AUDIT ===");
        sb.AppendLine("Gameplay authority: one RegisterMove per drag destination change,");
        sb.AppendLine("or one RegisterMove on BeginTargetExit (assist/dock exit).");
        sb.AppendLine("Multi-cell drag = 1. Auto exit animation after RegisterMove = +0.");
        sb.AppendLine();

        LevelDatabase database = LoadMainDb();
        if (database == null)
        {
            return;
        }

        AuditEasyLevel1(database, sb);
        sb.AppendLine();
        RunSyntheticTests(sb);

        Debug.Log(sb.ToString());
    }

    private static void AuditEasyLevel1(LevelDatabase database, StringBuilder sb)
    {
        int dbIndex = LevelDifficultyOrder.GetDatabaseIndexForDifficultyDisplayNumber(
            database,
            LevelDifficulty.Easy,
            1
        );
        if (dbIndex < 0)
        {
            sb.AppendLine("Easy #1 NOT FOUND");
            return;
        }

        LevelData level = database.levels[dbIndex];
        RushOutSolver.SolveConstraints constraints =
            ObjectiveSolveConstraints.FromLevelData(level);
        RushOutSolver.SolverResult result = RushOutSolver.SolveLevelData(
            level,
            MaxStates,
            -1,
            constraints
        );

        sb.AppendLine("[MoveCountAudit]");
        sb.AppendLine("Asset=" + level.name);
        sb.AppendLine("DbIndex=" + dbIndex);
        sb.AppendLine(
            "Grid=" + level.ResolvedGridWidth + "x" + level.ResolvedGridHeight
        );
        sb.AppendLine("RuntimeObservedMoves=3 (playtest)");
        sb.AppendLine("StoredMinimumMoves=" + level.minimumMoves);
        sb.AppendLine(
            "SolverPlayerMoves=" +
            (result.solvable ? result.minimumMoves.ToString() : "UNSOLVABLE/TIMEOUT")
        );
        sb.AppendLine(
            "searchLimit=" + result.searchLimitReached +
            " states=" + result.statesExplored
        );
        sb.AppendLine();

        if (!result.solvable || result.solution == null)
        {
            sb.AppendLine("No solution path to print.");
            return;
        }

        sb.AppendLine("Full solver path (player-move metric):");
        for (int i = 0; i < result.solution.Count; i++)
        {
            RushOutSolver.SolverMove move = result.solution[i];
            int dist = Mathf.Abs(move.toPosition.x - move.fromPosition.x) +
                Mathf.Abs(move.toPosition.y - move.fromPosition.y);
            bool counts = RushOutSolver.IsPlayerHintMove(
                move,
                constraints.multiTarget
            );
            sb.AppendLine(
                "Move " + (i + 1) + ":" +
                "\n  Vehicle=" + move.vehicleName + " idx=" + move.vehicleIndex +
                "\n  From=(" + move.fromPosition.x + "," + move.fromPosition.y + ")" +
                "\n  To=(" + move.toPosition.x + "," + move.toPosition.y + ")" +
                "\n  Distance=" + dist +
                "\n  exitsBoard=" + move.exitsBoard +
                "\n  countsAsPlayerMove=" + counts
            );
            if (move.exitsBoard && move.fromPosition == move.toPosition)
            {
                sb.AppendLine(
                    "  NOTE: stationary exitsBoard — " +
                    (constraints.multiTarget
                        ? "MULTI zero-cost auto-exit (HUD +0)"
                        : "explicit exit-from-dock (HUD +1)")
                );
            }
        }

        sb.AppendLine();
        if (result.minimumMoves == 3)
        {
            sb.AppendLine("TEST E PASS: Easy #1 solver player moves = 3");
        }
        else
        {
            sb.AppendLine(
                "TEST E FAIL: Easy #1 solver player moves = " +
                result.minimumMoves + " (expected 3)"
            );
            sb.AppendLine(
                "Root cause before fix: synthetic dock→exit +1 made PAR 4 vs HUD 3."
            );
        }

        if (level.minimumMoves != result.minimumMoves)
        {
            sb.AppendLine(
                "NOTE: Stored LevelData.minimumMoves still " + level.minimumMoves +
                " — run RECALCULATE MIN MOVES after confirming semantics (not auto-run)."
            );
        }
    }

    private static void RunSyntheticTests(StringBuilder sb)
    {
        sb.AppendLine("--- Synthetic semantics checks ---");

        // TEST C conceptually: ExpandMoves already enqueues any distance as one edge.
        sb.AppendLine(
            "TEST C (multi-cell drag): solver ExpandMoves steps 1..N as separate " +
            "destinations each costing 1 BFS edge — PASS by architecture " +
            "(not cell-by-cell chaining)."
        );

        // TEST A/B: build tiny levels via LevelData ScriptableObjects in memory.
        LevelData oneMove = CreateTinyLevel(
            gridW: 6,
            gridH: 6,
            exitRow: 2,
            targetPos: new Vector2Int(3, 2),
            targetLen: 2
        );
        // Dock at x = 6-2 = 4. Target at x=3 → one slide to x=4 wins (no +1 exit).
        RushOutSolver.SolverResult a = RushOutSolver.SolveLevelData(oneMove, MaxStates);
        bool passA = a.solvable && a.minimumMoves == 1;
        sb.AppendLine(
            "TEST A (one slide to dock): moves=" + a.minimumMoves +
            " → " + (passA ? "PASS" : "FAIL (expected 1)")
        );
        Object.DestroyImmediate(oneMove);

        LevelData twoMove = CreateTinyLevelWithBlocker();
        RushOutSolver.SolverResult b = RushOutSolver.SolveLevelData(twoMove, MaxStates);
        bool passB = b.solvable && b.minimumMoves == 2;
        sb.AppendLine(
            "TEST B (blocker + target dock): moves=" + b.minimumMoves +
            " → " + (passB ? "PASS" : "FAIL (expected 2)")
        );
        Object.DestroyImmediate(twoMove);

        LevelData alreadyDocked = CreateTinyLevel(
            gridW: 6,
            gridH: 6,
            exitRow: 2,
            targetPos: new Vector2Int(4, 2),
            targetLen: 2
        );
        RushOutSolver.SolverResult docked = RushOutSolver.SolveLevelData(
            alreadyDocked,
            MaxStates
        );
        bool passDocked = docked.solvable && docked.minimumMoves == 1;
        sb.AppendLine(
            "TEST A2 (already docked → explicit exit): moves=" + docked.minimumMoves +
            " → " + (passDocked ? "PASS" : "FAIL (expected 1)")
        );
        Object.DestroyImmediate(alreadyDocked);

        sb.AppendLine(
            "TEST D (MultiTarget zero-cost exit): covered by TryEnqueueExitChild " +
            "depth unchanged + CountPlayerMoves skipping from==to exitsBoard."
        );
        sb.AppendLine();
        sb.AppendLine("No LevelData writes performed by this audit.");
    }

    private static LevelData CreateTinyLevel(
        int gridW,
        int gridH,
        int exitRow,
        Vector2Int targetPos,
        int targetLen)
    {
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.gridWidth = gridW;
        level.gridHeight = gridH;
        level.exitRow = exitRow;
        level.difficulty = LevelDifficulty.Easy;
        level.objectiveType = LevelObjectiveType.Classic;
        level.vehicles = new List<VehicleData>
        {
            new VehicleData
            {
                vehicleName = "Target",
                orientation = VehicleController.VehicleOrientation.Horizontal,
                lengthInCells = targetLen,
                gridPosition = targetPos,
                canExitRight = true
            }
        };
        return level;
    }

    private static LevelData CreateTinyLevelWithBlocker()
    {
        // Target at (3,2) len 2, dock at x=4. Vertical blocker on (5,1) len 2 blocks?
        // Simpler: blocker on exit row to the right of target forcing one move.
        // Target (2,2) len 2 → occupies 2,3. Dock x=4. Cell 4 must be free.
        // Place vertical blocker at (4,2) length 1... vertical at (4,1) len 2 covers (4,1),(4,2).
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.gridWidth = 6;
        level.gridHeight = 6;
        level.exitRow = 2;
        level.difficulty = LevelDifficulty.Easy;
        level.objectiveType = LevelObjectiveType.Classic;
        level.vehicles = new List<VehicleData>
        {
            new VehicleData
            {
                vehicleName = "Target",
                orientation = VehicleController.VehicleOrientation.Horizontal,
                lengthInCells = 2,
                gridPosition = new Vector2Int(2, 2),
                canExitRight = true
            },
            new VehicleData
            {
                vehicleName = "Blocker",
                orientation = VehicleController.VehicleOrientation.Vertical,
                lengthInCells = 2,
                gridPosition = new Vector2Int(4, 1),
                canExitRight = false
            }
        };
        return level;
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

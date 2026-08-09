using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Regressietest voor RushOutSolver: 6x6 database-levels + één 10x10 kandidaat.
/// Controleert solvable, minimumMoves, solution-replay en final exit.
/// Menu: RushOut → Solver → Regression Test
/// </summary>
public static class SolverRegressionTest
{
    private const int MaxStates = 100000;

    [MenuItem("RushOut/Solver/Regression Test")]
    public static void RunRegressionTest()
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("=== RushOutSolver Regression Test ===");

        int pass = 0;
        int fail = 0;
        int skipped = 0;

        LevelDatabase database = LoadMainLevelDatabase();
        if (database == null || database.levels == null)
        {
            Debug.LogError("SolverRegressionTest: MainLevelDatabase niet gevonden.");
            return;
        }

        report.AppendLine("--- 6x6 / database levels ---");
        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level == null)
            {
                skipped++;
                report.AppendLine("[" + i + "] NULL → skip");
                continue;
            }

            string name = level.name;
            int w = level.ResolvedGridWidth;
            int h = level.ResolvedGridHeight;

            string validationError = LevelSolver.ValidateLevelData(level);
            if (validationError != null)
            {
                fail++;
                report.AppendLine(name + " FAIL invalid: " + validationError);
                continue;
            }

            Stopwatch sw = Stopwatch.StartNew();
            RushOutSolver.SolverResult result = RushOutSolver.SolveLevelData(level, MaxStates);
            sw.Stop();

            if (!CheckOccupancyInvariant(result, report, name))
            {
                fail++;
                continue;
            }

            if (!CheckChildCopyInvariant(result, report, name))
            {
                fail++;
                continue;
            }

            if (result.searchLimitReached)
            {
                // Als asset eerder ook geen geldige solve had, niet als regressie markeren.
                if (level.minimumMoves < 0)
                {
                    skipped++;
                    report.AppendLine(
                        name + " SKIP search limit (asset had no prior solve) | " +
                        FormatProfile(result, sw.Elapsed.TotalMilliseconds)
                    );
                    continue;
                }

                fail++;
                report.AppendLine(
                    name + " FAIL search limit (expected moves=" + level.minimumMoves + ") | " +
                    FormatProfile(result, sw.Elapsed.TotalMilliseconds)
                );
                continue;
            }

            if (!result.solvable)
            {
                if (level.minimumMoves < 0)
                {
                    skipped++;
                    report.AppendLine(name + " SKIP unsolvable (matches prior metadata)");
                    continue;
                }

                fail++;
                report.AppendLine(
                    name + " FAIL unsolvable (expected moves=" + level.minimumMoves + ")"
                );
                continue;
            }

            // Vergelijk met opgeslagen metadata indien aanwezig.
            if (level.minimumMoves > 0 && result.minimumMoves != level.minimumMoves)
            {
                fail++;
                report.AppendLine(
                    name + " FAIL minimumMoves mismatch: got " + result.minimumMoves +
                    ", expected " + level.minimumMoves + " | " +
                    FormatProfile(result, sw.Elapsed.TotalMilliseconds)
                );
                continue;
            }

            string replayError = ValidateSolutionReplay(level, result);
            if (replayError != null)
            {
                fail++;
                report.AppendLine(name + " FAIL solution: " + replayError);
                continue;
            }

            pass++;
            report.AppendLine(
                name + " PASS " + w + "x" + h +
                " moves=" + result.minimumMoves +
                " | " + FormatProfile(result, sw.Elapsed.TotalMilliseconds)
            );
        }

        report.AppendLine();
        report.AppendLine("--- 10x10 candidate ---");
        LevelData tenByTen = BuildSimple10x10Candidate();
        {
            Stopwatch sw = Stopwatch.StartNew();
            RushOutSolver.SolverResult result = RushOutSolver.SolveLevelData(tenByTen, MaxStates);
            sw.Stop();

            if (!CheckOccupancyInvariant(result, report, "10x10_candidate"))
            {
                fail++;
            }
            else if (!CheckChildCopyInvariant(result, report, "10x10_candidate"))
            {
                fail++;
            }
            else if (result.searchLimitReached)
            {
                fail++;
                report.AppendLine(
                    "10x10_candidate FAIL search limit | " +
                    FormatProfile(result, sw.Elapsed.TotalMilliseconds)
                );
            }
            else if (!result.solvable)
            {
                fail++;
                report.AppendLine("10x10_candidate FAIL unsolvable");
            }
            else
            {
                string replayError = ValidateSolutionReplay(tenByTen, result);
                if (replayError != null)
                {
                    fail++;
                    report.AppendLine("10x10_candidate FAIL solution: " + replayError);
                }
                else if (result.minimumMoves < 1)
                {
                    fail++;
                    report.AppendLine("10x10_candidate FAIL expected at least 1 move");
                }
                else
                {
                    pass++;
                    report.AppendLine(
                        "10x10_candidate PASS moves=" + result.minimumMoves +
                        " | " + FormatProfile(result, sw.Elapsed.TotalMilliseconds)
                    );
                }
            }

            UnityEngine.Object.DestroyImmediate(tenByTen);
        }

        report.AppendLine();
        report.AppendLine("--- summary ---");
        report.AppendLine("pass: " + pass);
        report.AppendLine("fail: " + fail);
        report.AppendLine("skipped: " + skipped);

        if (fail == 0)
        {
            Debug.Log(report.ToString());
        }
        else
        {
            Debug.LogError(report.ToString());
        }
    }

    private static bool CheckOccupancyInvariant(
        RushOutSolver.SolverResult result,
        StringBuilder report,
        string name)
    {
        // occupancyBuildCount ≈ statesExplored (max 1 per dequeued state)
        if (result.occupancyBuildCount != result.statesExplored)
        {
            report.AppendLine(
                name + " FAIL occupancyBuildCount=" + result.occupancyBuildCount +
                " != statesExplored=" + result.statesExplored
            );
            return false;
        }

        return true;
    }

    private static bool CheckChildCopyInvariant(
        RushOutSolver.SolverResult result,
        StringBuilder report,
        string name)
    {
        // Child states only for newly discovered (start telt niet).
        int expectedChildren = Math.Max(0, result.discoveredStates - 1);
        if (result.childStatesCreated != expectedChildren ||
            result.childStatesEnqueued != expectedChildren)
        {
            report.AppendLine(
                name + " FAIL child copy invariant: created=" + result.childStatesCreated +
                " enqueued=" + result.childStatesEnqueued +
                " expected=" + expectedChildren +
                " (discovered=" + result.discoveredStates + ")"
            );
            return false;
        }

        if (result.childStatesCreated > result.generatedMoves)
        {
            report.AppendLine(
                name + " FAIL childStatesCreated > generatedMoves"
            );
            return false;
        }

        return true;
    }

    private static string FormatProfile(RushOutSolver.SolverResult r, double wallMs)
    {
        return "wallMs=" + wallMs.ToString("0.0") +
               " states=" + r.statesExplored +
               " occBuilds=" + r.occupancyBuildCount +
               " genMoves=" + r.generatedMoves +
               " precheckRejects=" + r.visitedPrecheckRejects +
               " created=" + r.childStatesCreated +
               " enqueued=" + r.childStatesEnqueued +
               " discovered=" + r.discoveredStates +
               " queuePeak=" + r.queuePeakSize +
               " limit=" + (r.searchLimitReason ?? "-") +
               " occMs=" + r.totalOccupancyBuildMs.ToString("0.00") +
               " moveMs=" + r.totalMoveGenerationMs.ToString("0.00") +
               " visMs=" + r.totalVisitedMs.ToString("0.00") +
               " copyMs=" + r.totalStateCopyMs.ToString("0.00");
    }

    /// <summary>
    /// Speelt de solution af op een occupancy-grid en controleert finale exit.
    /// </summary>
    private static string ValidateSolutionReplay(LevelData level, RushOutSolver.SolverResult result)
    {
        if (result.solution == null || result.solution.Count == 0)
        {
            return "empty solution";
        }

        int w = level.ResolvedGridWidth;
        int h = level.ResolvedGridHeight;
        int vehicleCount = level.vehicles.Count;
        Vector2Int[] positions = new Vector2Int[vehicleCount];
        bool[] isHorizontal = new bool[vehicleCount];
        int[] lengths = new int[vehicleCount];
        int targetIndex = -1;

        for (int i = 0; i < vehicleCount; i++)
        {
            VehicleData v = level.vehicles[i];
            positions[i] = v.gridPosition;
            isHorizontal[i] = v.orientation == VehicleController.VehicleOrientation.Horizontal;
            lengths[i] = v.lengthInCells;
            if (v.canExitRight)
            {
                targetIndex = i;
            }
        }

        if (targetIndex < 0)
        {
            return "no target";
        }

        RushOutSolver.SolverMove last = null;
        for (int m = 0; m < result.solution.Count; m++)
        {
            RushOutSolver.SolverMove move = result.solution[m];
            last = move;

            if (move.exitsBoard)
            {
                if (m != result.solution.Count - 1)
                {
                    return "exit move not last";
                }

                if (move.vehicleIndex != targetIndex)
                {
                    return "exit move not target";
                }

                if (positions[targetIndex].y != level.exitRow)
                {
                    return "target not on exitRow at exit";
                }

                // Pad rechts moet vrij zijn.
                int[] occ = BuildOcc(positions, isHorizontal, lengths, w, h);
                int rightMost = positions[targetIndex].x + lengths[targetIndex] - 1;
                int rowBase = positions[targetIndex].y * w;
                for (int x = rightMost + 1; x < w; x++)
                {
                    if (occ[rowBase + x] >= 0)
                    {
                        return "exit path blocked at x=" + x;
                    }
                }

                return null;
            }

            if (move.vehicleIndex < 0 || move.vehicleIndex >= vehicleCount)
            {
                return "bad vehicleIndex at move " + m;
            }

            if (positions[move.vehicleIndex] != move.fromPosition)
            {
                return "from mismatch at move " + m;
            }

            if (!IsLegalSlide(
                    positions,
                    isHorizontal,
                    lengths,
                    move.vehicleIndex,
                    move.toPosition,
                    w,
                    h))
            {
                return "illegal slide at move " + m;
            }

            positions[move.vehicleIndex] = move.toPosition;
        }

        if (last == null || !last.exitsBoard)
        {
            return "solution missing final exit move";
        }

        return null;
    }

    private static bool IsLegalSlide(
        Vector2Int[] positions,
        bool[] isHorizontal,
        int[] lengths,
        int vehicleIndex,
        Vector2Int to,
        int gridWidth,
        int gridHeight)
    {
        Vector2Int from = positions[vehicleIndex];
        int length = lengths[vehicleIndex];

        if (isHorizontal[vehicleIndex])
        {
            if (to.y != from.y)
            {
                return false;
            }

            int delta = to.x - from.x;
            if (delta == 0)
            {
                return false;
            }

            int[] occ = BuildOcc(positions, isHorizontal, lengths, gridWidth, gridHeight);
            // Clear self
            ClearVehicle(occ, from, true, length, gridWidth, gridHeight);

            int step = delta > 0 ? 1 : -1;
            int absDelta = Math.Abs(delta);
            for (int s = 1; s <= absDelta; s++)
            {
                int newPosX = from.x + step * s;
                int checkX = delta > 0 ? (newPosX + length - 1) : newPosX;
                if (checkX < 0 || checkX >= gridWidth)
                {
                    return false;
                }

                if (occ[from.y * gridWidth + checkX] >= 0)
                {
                    return false;
                }
            }

            return true;
        }
        else
        {
            if (to.x != from.x)
            {
                return false;
            }

            int delta = to.y - from.y;
            if (delta == 0)
            {
                return false;
            }

            int[] occ = BuildOcc(positions, isHorizontal, lengths, gridWidth, gridHeight);
            ClearVehicle(occ, from, false, length, gridWidth, gridHeight);

            int step = delta > 0 ? 1 : -1;
            int absDelta = Math.Abs(delta);
            for (int s = 1; s <= absDelta; s++)
            {
                int newY = from.y + step * s;
                int checkY = delta > 0 ? (newY + length - 1) : newY;
                if (checkY < 0 || checkY >= gridHeight)
                {
                    return false;
                }

                if (occ[checkY * gridWidth + from.x] >= 0)
                {
                    return false;
                }
            }

            return true;
        }
    }

    private static int[] BuildOcc(
        Vector2Int[] positions,
        bool[] isHorizontal,
        int[] lengths,
        int gridWidth,
        int gridHeight)
    {
        int[] occ = new int[gridWidth * gridHeight];
        for (int i = 0; i < occ.Length; i++)
        {
            occ[i] = -1;
        }

        for (int i = 0; i < positions.Length; i++)
        {
            Vector2Int pos = positions[i];
            if (isHorizontal[i])
            {
                for (int c = 0; c < lengths[i]; c++)
                {
                    int x = pos.x + c;
                    if (x >= 0 && x < gridWidth && pos.y >= 0 && pos.y < gridHeight)
                    {
                        occ[pos.y * gridWidth + x] = i;
                    }
                }
            }
            else
            {
                for (int c = 0; c < lengths[i]; c++)
                {
                    int y = pos.y + c;
                    if (pos.x >= 0 && pos.x < gridWidth && y >= 0 && y < gridHeight)
                    {
                        occ[y * gridWidth + pos.x] = i;
                    }
                }
            }
        }

        return occ;
    }

    private static void ClearVehicle(
        int[] occ,
        Vector2Int pos,
        bool horizontal,
        int length,
        int gridWidth,
        int gridHeight)
    {
        if (horizontal)
        {
            for (int c = 0; c < length; c++)
            {
                int x = pos.x + c;
                if (x >= 0 && x < gridWidth && pos.y >= 0 && pos.y < gridHeight)
                {
                    occ[pos.y * gridWidth + x] = -1;
                }
            }
        }
        else
        {
            for (int c = 0; c < length; c++)
            {
                int y = pos.y + c;
                if (pos.x >= 0 && pos.x < gridWidth && y >= 0 && y < gridHeight)
                {
                    occ[y * gridWidth + pos.x] = -1;
                }
            }
        }
    }

    /// <summary>
    /// Eenvoudige oplosbare 10x10: target geblokkeerd door één verticale auto.
    /// </summary>
    private static LevelData BuildSimple10x10Candidate()
    {
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.gridWidth = 10;
        level.gridHeight = 10;
        level.exitRow = 4;
        level.levelNumber = 999;
        level.vehicles = new List<VehicleData>
        {
            new VehicleData
            {
                vehicleName = "Target",
                orientation = VehicleController.VehicleOrientation.Horizontal,
                lengthInCells = 2,
                canExitRight = true,
                gridPosition = new Vector2Int(3, 4)
            },
            new VehicleData
            {
                vehicleName = "Blocker",
                orientation = VehicleController.VehicleOrientation.Vertical,
                lengthInCells = 2,
                canExitRight = false,
                gridPosition = new Vector2Int(7, 3)
            },
            new VehicleData
            {
                vehicleName = "Parked",
                orientation = VehicleController.VehicleOrientation.Horizontal,
                lengthInCells = 3,
                canExitRight = false,
                gridPosition = new Vector2Int(1, 1)
            }
        };
        return level;
    }

    private static LevelDatabase LoadMainLevelDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("MainLevelDatabase t:LevelDatabase");
        if (guids == null || guids.Length == 0)
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);
    }
}

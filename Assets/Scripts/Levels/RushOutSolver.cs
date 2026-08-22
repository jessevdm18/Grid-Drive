using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Runtime-safe Rush Hour BFS-solver voor Grid Drive.
/// Werkt uitsluitend op data (geen Editor, geen GameObjects).
/// Move-definitie = PLAYER MOVE (zelfde als GameManager.RegisterMove):
/// één voertuig naar een andere geldige gridpositie = 1 move (multi-cell drag = 1);
/// target die dockt wint zonder extra synthetische exit-move (gameplay assist/exit merge).
/// Multi-target auto-exit vanaf dock = 0 player moves.
/// </summary>
public static class RushOutSolver
{
    public const int DefaultGridSize = 6;
    public const int DefaultMaxStates = 100000;

    public const string SearchLimitExplored = "explored limit";
    public const string SearchLimitDiscovered = "discovered limit";

    private const int MaxTrackedVehicles = 32;
    private const int CounterStates = 256;

    // -------------------------------------------------------------------------
    // Datastructuren
    // -------------------------------------------------------------------------

    public class VehicleDefinition
    {
        public string name;
        public VehicleController.VehicleOrientation orientation;
        public int lengthInCells;
        public bool canExitRight;

        public bool IsHorizontal =>
            orientation == VehicleController.VehicleOrientation.Horizontal;
    }

    /// <summary>
    /// Actuele gridposities per voertuig-index (immutable / hashable).
    /// Equality is structureel — hash collisions worden altijd met Equals gecorrigeerd.
    /// Constraint fields: exitedMask bits, fragile/limited move counters.
    /// </summary>
    public sealed class BoardState : IEquatable<BoardState>
    {
        public readonly Vector2Int[] positions;
        public readonly int exitedMask;
        public readonly byte fragileUsed;
        public readonly byte limitedUsed;
        private readonly int cachedHash;

        public BoardState(Vector2Int[] positions)
            : this(positions, 0, 0, 0)
        {
        }

        public BoardState(IList<Vector2Int> positions)
            : this(ToArray(positions), 0, 0, 0)
        {
        }

        public BoardState(
            Vector2Int[] positions,
            int exitedMask,
            byte fragileUsed,
            byte limitedUsed)
        {
            this.positions = new Vector2Int[positions.Length];
            Array.Copy(positions, this.positions, positions.Length);
            this.exitedMask = exitedMask;
            this.fragileUsed = fragileUsed;
            this.limitedUsed = limitedUsed;
            cachedHash = ComputeHash(this.positions, exitedMask, fragileUsed, limitedUsed);
        }

        /// <summary>
        /// Neemt ownership van de array (geen extra copy). Alleen voor interne move-generatie.
        /// </summary>
        private BoardState(
            Vector2Int[] ownedPositions,
            int exitedMask,
            byte fragileUsed,
            byte limitedUsed,
            bool _)
        {
            positions = ownedPositions;
            this.exitedMask = exitedMask;
            this.fragileUsed = fragileUsed;
            this.limitedUsed = limitedUsed;
            cachedHash = ComputeHash(positions, exitedMask, fragileUsed, limitedUsed);
        }

        public bool IsExited(int index)
        {
            return index >= 0 && index < 32 && (exitedMask & (1 << index)) != 0;
        }

        public BoardState WithMovedVehicle(
            int index,
            Vector2Int newPos,
            int fragileIndex,
            int limitedIndex)
        {
            Vector2Int[] copy = new Vector2Int[positions.Length];
            Array.Copy(positions, copy, positions.Length);
            copy[index] = newPos;
            byte nextFragile = fragileUsed;
            byte nextLimited = limitedUsed;
            if (index == fragileIndex && fragileIndex >= 0)
            {
                nextFragile = (byte)Mathf.Min(255, fragileUsed + 1);
            }

            if (index == limitedIndex && limitedIndex >= 0)
            {
                nextLimited = (byte)Mathf.Min(255, limitedUsed + 1);
            }

            return new BoardState(copy, exitedMask, nextFragile, nextLimited, true);
        }

        public BoardState WithVehicleExited(int index)
        {
            Vector2Int[] copy = new Vector2Int[positions.Length];
            Array.Copy(positions, copy, positions.Length);
            int mask = exitedMask | (1 << index);
            return new BoardState(copy, mask, fragileUsed, limitedUsed, true);
        }

        public bool Equals(BoardState other)
        {
            if (other == null || other.positions.Length != positions.Length)
            {
                return false;
            }

            if (exitedMask != other.exitedMask ||
                fragileUsed != other.fragileUsed ||
                limitedUsed != other.limitedUsed)
            {
                return false;
            }

            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i] != other.positions[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BoardState);
        }

        public override int GetHashCode()
        {
            return cachedHash;
        }

        private static Vector2Int[] ToArray(IList<Vector2Int> list)
        {
            Vector2Int[] arr = new Vector2Int[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                arr[i] = list[i];
            }

            return arr;
        }

        private static int ComputeHash(
            Vector2Int[] positions,
            int exitedMask,
            byte fragileUsed,
            byte limitedUsed)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + exitedMask;
                hash = hash * 31 + fragileUsed;
                hash = hash * 31 + limitedUsed;
                for (int i = 0; i < positions.Length; i++)
                {
                    hash = hash * 31 + positions[i].x;
                    hash = hash * 31 + positions[i].y;
                }

                return hash;
            }
        }
    }

    public class SolverMove
    {
        public int vehicleIndex;
        public string vehicleName;
        public Vector2Int fromPosition;
        public Vector2Int toPosition;
        public bool exitsBoard;

        public SolverMove(
            int vehicleIndex,
            string vehicleName,
            Vector2Int from,
            Vector2Int to,
            bool exitsBoard)
        {
            this.vehicleIndex = vehicleIndex;
            this.vehicleName = vehicleName;
            this.fromPosition = from;
            this.toPosition = to;
            this.exitsBoard = exitsBoard;
        }
    }

    public class SolverResult
    {
        public bool solvable;
        public int minimumMoves;
        public List<SolverMove> solution = new List<SolverMove>();
        public int statesExplored;
        public bool searchLimitReached;
        public string searchLimitReason;

        // Hot-path profiling
        public int generatedMoves;
        public int visitedPrecheckRejects;
        public int occupancyBuildCount;
        public int childStatesCreated;
        public int childStatesEnqueued;
        public int discoveredStates;
        public int queuePeakSize;
        public double totalOccupancyBuildMs;
        public double totalMoveGenerationMs;
        public double totalVisitedMs;
        public double totalStateCopyMs;

        /// <summary>Fragile cargo moves used along the found solution (0 if N/A).</summary>
        public int fragileMovesUsed;

        /// <summary>Limited vehicle moves used along the found solution (0 if N/A).</summary>
        public int limitedMovesUsed;
    }

    /// <summary>
    /// Optional production constraints. Null / default = Classic BFS (single target).
    /// Exit requires docking at the right edge (matches gameplay CanPerformExitRight).
    /// </summary>
    public sealed class SolveConstraints
    {
        public int protectedVehicleIndex = -1;
        public int fragileVehicleIndex = -1;
        public int fragileMoveLimit = -1;
        public int limitedVehicleIndex = -1;
        public int limitedMoveLimit = -1;
        public int maxSolutionMoves = -1;
        public bool multiTarget;
        public int[] targetIndices;

        /// <summary>When true (default), target must sit at gridWidth-length to exit.</summary>
        public bool requireDockedExit = true;

        public static SolveConstraints Classic()
        {
            return new SolveConstraints();
        }
    }

    // -------------------------------------------------------------------------
    // Publieke API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Lost een puzzel op vanaf een willekeurige actuele board-state.
    /// maxDiscoveredStates &lt;= 0 → gelijk aan maxStates (voorkomt enorme queues).
    /// </summary>
    public static SolverResult Solve(
        List<VehicleDefinition> vehicles,
        BoardState startState,
        int exitRow,
        int gridWidth,
        int gridHeight,
        int maxStates = DefaultMaxStates,
        int maxDiscoveredStates = -1)
    {
        return Solve(
            vehicles,
            startState,
            exitRow,
            gridWidth,
            gridHeight,
            maxStates,
            maxDiscoveredStates,
            null
        );
    }

    public static SolverResult Solve(
        List<VehicleDefinition> vehicles,
        BoardState startState,
        int exitRow,
        int gridWidth,
        int gridHeight,
        int maxStates,
        int maxDiscoveredStates,
        SolveConstraints constraints)
    {
        SolverResult result = new SolverResult();

        if (vehicles == null || vehicles.Count == 0 || startState == null)
        {
            result.solvable = false;
            return result;
        }

        if (startState.positions.Length != vehicles.Count)
        {
            result.solvable = false;
            return result;
        }

        int width = gridWidth > 0 ? gridWidth : DefaultGridSize;
        int height = gridHeight > 0 ? gridHeight : DefaultGridSize;
        int exploredCap = Mathf.Max(1, maxStates);
        int discoveredCap = maxDiscoveredStates > 0
            ? maxDiscoveredStates
            : exploredCap;

        SolveConstraints c = constraints ?? SolveConstraints.Classic();
        int[] targets = ResolveTargetIndices(vehicles, c);
        if (targets == null || targets.Length == 0)
        {
            result.solvable = false;
            return result;
        }

        return RunBfs(
            vehicles,
            exitRow,
            targets,
            startState,
            width,
            height,
            result,
            exploredCap,
            Mathf.Max(1, discoveredCap),
            c
        );
    }

    /// <summary>
    /// Convenience: bouw VehicleDefinition + BoardState vanuit LevelData-startposities.
    /// </summary>
    public static SolverResult SolveLevelData(
        LevelData levelData,
        int maxStates = DefaultMaxStates,
        int maxDiscoveredStates = -1)
    {
        return SolveLevelData(levelData, maxStates, maxDiscoveredStates, null);
    }

    public static SolverResult SolveLevelData(
        LevelData levelData,
        int maxStates,
        int maxDiscoveredStates,
        SolveConstraints constraints)
    {
        if (levelData == null || levelData.vehicles == null)
        {
            return new SolverResult { solvable = false };
        }

        BuildFromLevelData(levelData, out List<VehicleDefinition> vehicles, out BoardState start);
        return Solve(
            vehicles,
            start,
            levelData.exitRow,
            levelData.ResolvedGridWidth,
            levelData.ResolvedGridHeight,
            maxStates,
            maxDiscoveredStates,
            constraints
        );
    }

    public static void BuildFromLevelData(
        LevelData levelData,
        out List<VehicleDefinition> vehicles,
        out BoardState startState)
    {
        vehicles = new List<VehicleDefinition>(levelData.vehicles.Count);
        Vector2Int[] positions = new Vector2Int[levelData.vehicles.Count];

        for (int i = 0; i < levelData.vehicles.Count; i++)
        {
            VehicleData v = levelData.vehicles[i];
            vehicles.Add(new VehicleDefinition
            {
                name = string.IsNullOrEmpty(v.vehicleName) ? ("Vehicle_" + i) : v.vehicleName,
                orientation = v.orientation,
                lengthInCells = v.lengthInCells,
                canExitRight = v.canExitRight
            });
            positions[i] = v.gridPosition;
        }

        startState = new BoardState(positions);
    }

    /// <summary>
    /// Bepaalt welke voertuigen het bord moeten verlaten.
    /// Expliciete targetIndices &gt; multiTarget (alle canExitRight) &gt; eerste canExitRight.
    /// </summary>
    public static int[] ResolveTargetIndices(
        List<VehicleDefinition> vehicles,
        SolveConstraints constraints)
    {
        if (vehicles == null || vehicles.Count == 0)
        {
            return new int[0];
        }

        SolveConstraints c = constraints ?? SolveConstraints.Classic();

        if (c.targetIndices != null && c.targetIndices.Length > 0)
        {
            List<int> explicitTargets = new List<int>(c.targetIndices.Length);
            for (int i = 0; i < c.targetIndices.Length; i++)
            {
                int index = c.targetIndices[i];
                if (index < 0 || index >= vehicles.Count || index >= MaxTrackedVehicles)
                {
                    continue;
                }

                if (!explicitTargets.Contains(index))
                {
                    explicitTargets.Add(index);
                }
            }

            if (explicitTargets.Count > 0)
            {
                return explicitTargets.ToArray();
            }
        }

        if (c.multiTarget)
        {
            List<int> all = new List<int>();
            int limit = Mathf.Min(vehicles.Count, MaxTrackedVehicles);
            for (int i = 0; i < limit; i++)
            {
                if (vehicles[i] != null && vehicles[i].canExitRight)
                {
                    all.Add(i);
                }
            }

            if (all.Count > 0)
            {
                return all.ToArray();
            }
        }

        int first = FindTargetIndex(vehicles);
        return first >= 0 ? new[] { first } : new int[0];
    }

    // -------------------------------------------------------------------------
    // BFS
    // -------------------------------------------------------------------------

    private static SolverResult RunBfs(
        List<VehicleDefinition> vehicles,
        int exitRow,
        int[] targetIndices,
        BoardState start,
        int gridWidth,
        int gridHeight,
        SolverResult result,
        int maxStates,
        int maxDiscoveredStates,
        SolveConstraints constraints)
    {
        SolveConstraints c = constraints ?? SolveConstraints.Classic();

        int vehicleCount = vehicles.Count;
        int cellCount = gridWidth * gridHeight;

        bool[] isHorizontal = new bool[vehicleCount];
        int[] lengths = new int[vehicleCount];
        string[] names = new string[vehicleCount];
        for (int i = 0; i < vehicleCount; i++)
        {
            VehicleDefinition v = vehicles[i];
            isHorizontal[i] = v.IsHorizontal;
            lengths[i] = v.lengthInCells;
            names[i] = v.name;
        }

        bool multiMode = targetIndices.Length > 1;
        int targetMask = 0;
        for (int i = 0; i < targetIndices.Length; i++)
        {
            int ti = targetIndices[i];
            if (ti >= 0 && ti < MaxTrackedVehicles)
            {
                targetMask |= 1 << ti;
            }
        }

        BfsContext ctx = new BfsContext
        {
            isHorizontal = isHorizontal,
            lengths = lengths,
            names = names,
            vehicleCount = vehicleCount,
            gridWidth = gridWidth,
            gridHeight = gridHeight,
            cellCount = cellCount,
            occupancy = new int[cellCount],
            zobrist = new ZobristTables(vehicleCount, cellCount),
            queue = new Queue<QueueItem>(),
            discoveredSingle = new Dictionary<ulong, BoardState>(),
            discoveredMulti = new Dictionary<ulong, List<BoardState>>(),
            cameFrom = new Dictionary<BoardState, ParentLink>(),
            result = result,
            constraints = c,
            maxDiscoveredStates = maxDiscoveredStates,
            // Een niet-uitgereden single target kost altijd nog minimaal de exit-move.
            minRemainingMoves = multiMode ? 0 : 1
        };

        ulong startHash = ComputeZobristHash(ctx.zobrist, start, gridWidth);
        ctx.queue.Enqueue(new QueueItem(start, startHash, 0));
        AddDiscovered(ctx.discoveredSingle, ctx.discoveredMulti, startHash, start);
        ctx.cameFrom[start] = ParentLink.Root;
        ctx.discoveredCount = 1;
        result.discoveredStates = 1;
        result.queuePeakSize = 1;

        int explored = 0;
        long occTicks = 0;
        long moveTicks = 0;
        long tickFreq = Stopwatch.Frequency;

        while (ctx.queue.Count > 0)
        {
            if (explored >= maxStates)
            {
                result.searchLimitReached = true;
                result.searchLimitReason = SearchLimitExplored;
                FinishProfiling(
                    result,
                    explored,
                    ctx.discoveredCount,
                    occTicks,
                    moveTicks,
                    ctx.visitedTicks,
                    ctx.copyTicks,
                    tickFreq
                );
                result.solvable = false;
                return result;
            }

            QueueItem currentItem = ctx.queue.Dequeue();
            BoardState current = currentItem.state;
            ulong currentHash = currentItem.hash;
            int depth = currentItem.depth;
            explored++;

            if (multiMode && targetMask != 0 && (current.exitedMask & targetMask) == targetMask)
            {
                FinishProfiling(
                    result,
                    explored,
                    ctx.discoveredCount,
                    occTicks,
                    moveTicks,
                    ctx.visitedTicks,
                    ctx.copyTicks,
                    tickFreq
                );
                return FinishSolution(
                    result,
                    ReconstructPathFromLinks(ctx.cameFrom, current, names),
                    current,
                    c
                );
            }

            long t0 = Stopwatch.GetTimestamp();
            BuildOccupancyFlat(
                ctx.occupancy,
                isHorizontal,
                lengths,
                current.positions,
                current.exitedMask,
                vehicleCount,
                gridWidth,
                gridHeight
            );
            result.occupancyBuildCount++;
            occTicks += Stopwatch.GetTimestamp() - t0;

            bool hitDiscoveredLimit = false;

            if (!multiMode)
            {
                int ti = targetIndices[0];
                if (CanTargetExit(
                        current,
                        ti,
                        lengths[ti],
                        isHorizontal[ti],
                        exitRow,
                        gridWidth,
                        ctx.occupancy,
                        c.requireDockedExit))
                {
                    FinishProfiling(
                        result,
                        explored,
                        ctx.discoveredCount,
                        occTicks,
                        moveTicks,
                        ctx.visitedTicks,
                        ctx.copyTicks,
                        tickFreq
                    );
                    // Player-move semantics: arriving at dock (or already docked) matches
                    // gameplay assist/exit gesture. Do NOT add a second synthetic exit move.
                    return FinishSolution(
                        result,
                        BuildSingleTargetWinningPath(
                            ctx.cameFrom,
                            current,
                            names,
                            ti
                        ),
                        current,
                        c
                    );
                }
            }
            else
            {
                for (int t = 0; t < targetIndices.Length; t++)
                {
                    int ti = targetIndices[t];
                    if (current.IsExited(ti))
                    {
                        continue;
                    }

                    if (!CanTargetExit(
                            current,
                            ti,
                            lengths[ti],
                            isHorizontal[ti],
                            exitRow,
                            gridWidth,
                            ctx.occupancy,
                            c.requireDockedExit))
                    {
                        continue;
                    }

                    if (!TryEnqueueExitChild(ctx, current, currentHash, depth, ti))
                    {
                        hitDiscoveredLimit = true;
                        break;
                    }
                }
            }

            if (!hitDiscoveredLimit)
            {
                long visBefore = ctx.visitedTicks;
                long copyBefore = ctx.copyTicks;
                t0 = Stopwatch.GetTimestamp();
                hitDiscoveredLimit = !ExpandMoves(ctx, current, currentHash, depth);
                long expandElapsed = Stopwatch.GetTimestamp() - t0;
                moveTicks += expandElapsed
                    - (ctx.visitedTicks - visBefore)
                    - (ctx.copyTicks - copyBefore);
            }

            if (ctx.queue.Count > result.queuePeakSize)
            {
                result.queuePeakSize = ctx.queue.Count;
            }

            if (hitDiscoveredLimit)
            {
                result.searchLimitReached = true;
                result.searchLimitReason = SearchLimitDiscovered;
                FinishProfiling(
                    result,
                    explored,
                    ctx.discoveredCount,
                    occTicks,
                    moveTicks,
                    ctx.visitedTicks,
                    ctx.copyTicks,
                    tickFreq
                );
                result.solvable = false;
                return result;
            }
        }

        result.solvable = false;
        FinishProfiling(
            result,
            explored,
            ctx.discoveredCount,
            occTicks,
            moveTicks,
            ctx.visitedTicks,
            ctx.copyTicks,
            tickFreq
        );
        return result;
    }

    /// <summary>
    /// BFS levert de kortste PLAYER-MOVE oplossing (zelfde metric als HUD RegisterMove).
    /// Automatic/synthetic exit markers tellen niet mee; zie CountPlayerMoves.
    /// </summary>
    private static SolverResult FinishSolution(
        SolverResult result,
        List<SolverMove> solution,
        BoardState endState,
        SolveConstraints constraints)
    {
        int playerMoves = CountPlayerMoves(solution, constraints != null && constraints.multiTarget);
        if (constraints.maxSolutionMoves >= 0 && playerMoves > constraints.maxSolutionMoves)
        {
            result.solvable = false;
            result.solution = new List<SolverMove>();
            result.minimumMoves = 0;
            return result;
        }

        result.solvable = true;
        result.solution = solution ?? new List<SolverMove>();
        result.minimumMoves = playerMoves;
        result.fragileMovesUsed = endState.fragileUsed;
        result.limitedMovesUsed = endState.limitedUsed;
        return result;
    }

    /// <summary>
    /// Counts gameplay-equivalent player moves.
    /// Multi-target from==to exitsBoard links are zero-cost auto-exit (HUD +0).
    /// Single-target from==to exit is the explicit exit gesture when already docked (HUD +1).
    /// Board slides (including the slide that docks/wins) always count as 1.
    /// </summary>
    public static int CountPlayerMoves(List<SolverMove> solution, bool multiTarget)
    {
        if (solution == null || solution.Count == 0)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < solution.Count; i++)
        {
            SolverMove move = solution[i];
            if (move == null)
            {
                continue;
            }

            if (move.exitsBoard && move.fromPosition == move.toPosition)
            {
                if (multiTarget)
                {
                    // Zero-cost: target already docked; auto-exit does not RegisterMove again.
                    continue;
                }

                // Single-target explicit exit-from-dock (puzzle starts docked, or only exit left).
                count++;
                continue;
            }

            count++;
        }

        return count;
    }

    /// <summary>
    /// True when this solution entry should be shown/executed as a player hint action.
    /// </summary>
    public static bool IsPlayerHintMove(SolverMove move, bool multiTarget)
    {
        if (move == null)
        {
            return false;
        }

        if (move.exitsBoard && move.fromPosition == move.toPosition && multiTarget)
        {
            return false;
        }

        return true;
    }

    private static void FinishProfiling(
        SolverResult result,
        int explored,
        int discoveredCount,
        long occTicks,
        long moveTicks,
        long visitedTicks,
        long copyTicks,
        long tickFreq)
    {
        result.statesExplored = explored;
        result.discoveredStates = discoveredCount;
        double inv = 1000.0 / tickFreq;
        result.totalOccupancyBuildMs = occTicks * inv;
        result.totalMoveGenerationMs = moveTicks * inv;
        result.totalVisitedMs = visitedTicks * inv;
        result.totalStateCopyMs = copyTicks * inv;
    }

    /// <summary>
    /// Returns false als discovered-limit is geraakt (solve moet stoppen).
    /// </summary>
    private static bool ExpandMoves(
        BfsContext ctx,
        BoardState state,
        ulong stateHash,
        int depth)
    {
        Vector2Int[] positions = state.positions;
        int[] occupancy = ctx.occupancy;
        int gridWidth = ctx.gridWidth;
        int gridHeight = ctx.gridHeight;
        int protectedIndex = ctx.constraints.protectedVehicleIndex;

        for (int i = 0; i < ctx.vehicleCount; i++)
        {
            if (i == protectedIndex || state.IsExited(i))
            {
                continue;
            }

            Vector2Int from = positions[i];
            int length = ctx.lengths[i];

            if (ctx.isHorizontal[i])
            {
                for (int steps = 1; ; steps++)
                {
                    int newX = from.x - steps;
                    if (newX < 0)
                    {
                        break;
                    }

                    int checkIdx = from.y * gridWidth + newX;
                    if (occupancy[checkIdx] >= 0)
                    {
                        break;
                    }

                    if (!TryEnqueueChild(
                            ctx,
                            state,
                            stateHash,
                            depth,
                            i,
                            from,
                            new Vector2Int(newX, from.y)))
                    {
                        return false;
                    }
                }

                for (int steps = 1; ; steps++)
                {
                    int newX = from.x + steps;
                    int rightMost = newX + length - 1;
                    if (rightMost >= gridWidth)
                    {
                        break;
                    }

                    int checkIdx = from.y * gridWidth + rightMost;
                    if (occupancy[checkIdx] >= 0)
                    {
                        break;
                    }

                    if (!TryEnqueueChild(
                            ctx,
                            state,
                            stateHash,
                            depth,
                            i,
                            from,
                            new Vector2Int(newX, from.y)))
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int steps = 1; ; steps++)
                {
                    int newY = from.y - steps;
                    if (newY < 0)
                    {
                        break;
                    }

                    int checkIdx = newY * gridWidth + from.x;
                    if (occupancy[checkIdx] >= 0)
                    {
                        break;
                    }

                    if (!TryEnqueueChild(
                            ctx,
                            state,
                            stateHash,
                            depth,
                            i,
                            from,
                            new Vector2Int(from.x, newY)))
                    {
                        return false;
                    }
                }

                for (int steps = 1; ; steps++)
                {
                    int newY = from.y + steps;
                    int topMost = newY + length - 1;
                    if (topMost >= gridHeight)
                    {
                        break;
                    }

                    int checkIdx = topMost * gridWidth + from.x;
                    if (occupancy[checkIdx] >= 0)
                    {
                        break;
                    }

                    if (!TryEnqueueChild(
                            ctx,
                            state,
                            stateHash,
                            depth,
                            i,
                            from,
                            new Vector2Int(from.x, newY)))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Returns false bij discovered-limit. Duplicate / constraint-reject → true (doorgaan).
    /// Discover-markering gebeurt hier bij enqueue, niet bij dequeue.
    /// </summary>
    private static bool TryEnqueueChild(
        BfsContext ctx,
        BoardState state,
        ulong stateHash,
        int depth,
        int vehicleIndex,
        Vector2Int from,
        Vector2Int to)
    {
        SolveConstraints c = ctx.constraints;
        SolverResult result = ctx.result;
        result.generatedMoves++;

        if (!ctx.WithinMoveBudget(depth + 1))
        {
            return true;
        }

        if (vehicleIndex == c.fragileVehicleIndex &&
            c.fragileVehicleIndex >= 0 &&
            c.fragileMoveLimit >= 0 &&
            state.fragileUsed + 1 > c.fragileMoveLimit)
        {
            return true;
        }

        if (vehicleIndex == c.limitedVehicleIndex &&
            c.limitedVehicleIndex >= 0 &&
            c.limitedMoveLimit >= 0 &&
            state.limitedUsed + 1 > c.limitedMoveLimit)
        {
            return true;
        }

        long tCopy = Stopwatch.GetTimestamp();
        BoardState next = state.WithMovedVehicle(
            vehicleIndex,
            to,
            c.fragileVehicleIndex,
            c.limitedVehicleIndex
        );
        ctx.copyTicks += Stopwatch.GetTimestamp() - tCopy;
        result.childStatesCreated++;

        ulong childHash = HashAfterMove(
            stateHash,
            ctx.zobrist,
            vehicleIndex,
            from,
            to,
            ctx.gridWidth,
            state,
            next
        );

        long tVis = Stopwatch.GetTimestamp();
        bool discovered = IsDiscovered(
            ctx.discoveredSingle,
            ctx.discoveredMulti,
            childHash,
            next
        );
        ctx.visitedTicks += Stopwatch.GetTimestamp() - tVis;

        if (discovered)
        {
            result.visitedPrecheckRejects++;
            return true;
        }

        if (ctx.discoveredCount >= ctx.maxDiscoveredStates)
        {
            return false;
        }

        AddDiscovered(ctx.discoveredSingle, ctx.discoveredMulti, childHash, next);
        ctx.discoveredCount++;
        ctx.cameFrom[next] = new ParentLink(state, vehicleIndex, from, to, false);
        ctx.queue.Enqueue(new QueueItem(next, childHash, depth + 1));
        result.childStatesEnqueued++;
        return true;
    }

    /// <summary>
    /// Multi-target: docked target auto-exits at ZERO player-move cost (matches HUD).
    /// Same BFS depth; path records exitsBoard from==to for reconstruction only.
    /// </summary>
    private static bool TryEnqueueExitChild(
        BfsContext ctx,
        BoardState state,
        ulong stateHash,
        int depth,
        int targetIndex)
    {
        SolverResult result = ctx.result;
        result.generatedMoves++;

        // Zero-cost: do not consume move budget / depth.
        if (!ctx.WithinMoveBudget(depth))
        {
            return true;
        }

        long tCopy = Stopwatch.GetTimestamp();
        BoardState next = state.WithVehicleExited(targetIndex);
        ctx.copyTicks += Stopwatch.GetTimestamp() - tCopy;
        result.childStatesCreated++;

        ulong childHash = stateHash ^ ctx.zobrist.exited[targetIndex];

        long tVis = Stopwatch.GetTimestamp();
        bool discovered = IsDiscovered(
            ctx.discoveredSingle,
            ctx.discoveredMulti,
            childHash,
            next
        );
        ctx.visitedTicks += Stopwatch.GetTimestamp() - tVis;

        if (discovered)
        {
            result.visitedPrecheckRejects++;
            return true;
        }

        if (ctx.discoveredCount >= ctx.maxDiscoveredStates)
        {
            return false;
        }

        Vector2Int pos = state.positions[targetIndex];
        AddDiscovered(ctx.discoveredSingle, ctx.discoveredMulti, childHash, next);
        ctx.discoveredCount++;
        ctx.cameFrom[next] = new ParentLink(state, targetIndex, pos, pos, true);
        ctx.queue.Enqueue(new QueueItem(next, childHash, depth));
        result.childStatesEnqueued++;
        return true;
    }

    private static bool IsDiscovered(
        Dictionary<ulong, BoardState> single,
        Dictionary<ulong, List<BoardState>> multi,
        ulong hash,
        BoardState candidate)
    {
        if (multi.TryGetValue(hash, out List<BoardState> list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (candidate.Equals(list[i]))
                {
                    return true;
                }
            }

            return false;
        }

        if (single.TryGetValue(hash, out BoardState existing))
        {
            return candidate.Equals(existing);
        }

        return false;
    }

    private static void AddDiscovered(
        Dictionary<ulong, BoardState> single,
        Dictionary<ulong, List<BoardState>> multi,
        ulong hash,
        BoardState state)
    {
        if (multi.TryGetValue(hash, out List<BoardState> list))
        {
            list.Add(state);
            return;
        }

        if (single.TryGetValue(hash, out BoardState existing))
        {
            single.Remove(hash);
            multi[hash] = new List<BoardState>(2) { existing, state };
            return;
        }

        single[hash] = state;
    }

    // -------------------------------------------------------------------------
    // Zobrist hashing (incremental, collision-safe via structurele match)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Aparte tabellen voor posities, exit-vlaggen en de constraint-tellers, zodat
    /// twee states met dezelfde posities maar andere constraint-historie niet botsen.
    /// </summary>
    private sealed class ZobristTables
    {
        public readonly ulong[] cells;
        public readonly ulong[] exited;
        public readonly ulong[] fragile;
        public readonly ulong[] limited;
        public readonly int cellCount;

        public ZobristTables(int vehicleCount, int cellCount)
        {
            this.cellCount = cellCount;

            // Deterministische PRNG — zelfde seed → zelfde hashes tussen runs.
            ulong seed = 0xC0FFEE5EEDUL;
            cells = new ulong[Mathf.Max(1, vehicleCount * cellCount)];
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = SplitMix64(ref seed);
            }

            exited = new ulong[Mathf.Max(1, vehicleCount)];
            for (int i = 0; i < exited.Length; i++)
            {
                exited[i] = SplitMix64(ref seed);
            }

            fragile = new ulong[CounterStates];
            limited = new ulong[CounterStates];
            for (int i = 0; i < CounterStates; i++)
            {
                fragile[i] = SplitMix64(ref seed);
            }

            for (int i = 0; i < CounterStates; i++)
            {
                limited[i] = SplitMix64(ref seed);
            }
        }
    }

    private static ulong SplitMix64(ref ulong state)
    {
        unchecked
        {
            state += 0x9E3779B97F4A7C15UL;
            ulong z = state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }

    private static ulong ComputeZobristHash(
        ZobristTables zobrist,
        BoardState state,
        int gridWidth)
    {
        ulong hash = 0;
        Vector2Int[] positions = state.positions;
        int cellCount = zobrist.cellCount;

        for (int i = 0; i < positions.Length; i++)
        {
            int cell = positions[i].y * gridWidth + positions[i].x;
            if (cell >= 0 && cell < cellCount)
            {
                hash ^= zobrist.cells[i * cellCount + cell];
            }

            if (state.IsExited(i) && i < zobrist.exited.Length)
            {
                hash ^= zobrist.exited[i];
            }
        }

        hash ^= zobrist.fragile[state.fragileUsed];
        hash ^= zobrist.limited[state.limitedUsed];
        return hash;
    }

    private static ulong HashAfterMove(
        ulong parentHash,
        ZobristTables zobrist,
        int vehicleIndex,
        Vector2Int from,
        Vector2Int to,
        int gridWidth,
        BoardState parent,
        BoardState child)
    {
        int cellCount = zobrist.cellCount;
        int baseIndex = vehicleIndex * cellCount;
        int oldCell = from.y * gridWidth + from.x;
        int newCell = to.y * gridWidth + to.x;

        ulong hash = parentHash
            ^ zobrist.cells[baseIndex + oldCell]
            ^ zobrist.cells[baseIndex + newCell];

        if (child.fragileUsed != parent.fragileUsed)
        {
            hash ^= zobrist.fragile[parent.fragileUsed] ^ zobrist.fragile[child.fragileUsed];
        }

        if (child.limitedUsed != parent.limitedUsed)
        {
            hash ^= zobrist.limited[parent.limitedUsed] ^ zobrist.limited[child.limitedUsed];
        }

        return hash;
    }

    /// <summary>
    /// Single-target winning path in PLAYER MOVES.
    /// Board path to docked target — no synthetic extra exit.
    /// If already docked at start (empty path), one explicit exit gesture.
    /// </summary>
    private static List<SolverMove> BuildSingleTargetWinningPath(
        Dictionary<BoardState, ParentLink> cameFrom,
        BoardState endState,
        string[] names,
        int targetIndex)
    {
        List<SolverMove> path = ReconstructPathFromLinks(cameFrom, endState, names);
        if (path.Count == 0)
        {
            Vector2Int pos = endState.positions[targetIndex];
            path.Add(new SolverMove(
                targetIndex,
                names[targetIndex],
                pos,
                pos,
                exitsBoard: true
            ));
        }

        return path;
    }

    /// <summary>
    /// Legacy helper — prefer BuildSingleTargetWinningPath for player-move semantics.
    /// </summary>
    private static List<SolverMove> ReconstructPath(
        Dictionary<BoardState, ParentLink> cameFrom,
        BoardState endState,
        string[] names,
        int targetIndex,
        Vector2Int targetPos)
    {
        return BuildSingleTargetWinningPath(cameFrom, endState, names, targetIndex);
    }

    /// <summary>
    /// Loopt de ParentLink-keten terug; exit-links worden als exitsBoard-move uitgegeven.
    /// </summary>
    private static List<SolverMove> ReconstructPathFromLinks(
        Dictionary<BoardState, ParentLink> cameFrom,
        BoardState endState,
        string[] names)
    {
        List<SolverMove> path = new List<SolverMove>();
        BoardState cursor = endState;

        while (cameFrom.TryGetValue(cursor, out ParentLink link) && link.parent != null)
        {
            path.Add(new SolverMove(
                link.vehicleIndex,
                names[link.vehicleIndex],
                link.from,
                link.to,
                link.exitsBoard
            ));
            cursor = link.parent;
        }

        path.Reverse();
        return path;
    }

    // -------------------------------------------------------------------------
    // Occupancy + exit
    // -------------------------------------------------------------------------

    private static void BuildOccupancyFlat(
        int[] occupancy,
        bool[] isHorizontal,
        int[] lengths,
        Vector2Int[] positions,
        int exitedMask,
        int vehicleCount,
        int gridWidth,
        int gridHeight)
    {
        for (int i = 0; i < occupancy.Length; i++)
        {
            occupancy[i] = -1;
        }

        for (int i = 0; i < vehicleCount; i++)
        {
            if (i < MaxTrackedVehicles && (exitedMask & (1 << i)) != 0)
            {
                continue;
            }

            Vector2Int pos = positions[i];
            int length = lengths[i];

            if (isHorizontal[i])
            {
                int rowBase = pos.y * gridWidth;
                for (int c = 0; c < length; c++)
                {
                    int x = pos.x + c;
                    if (x >= 0 && x < gridWidth && pos.y >= 0 && pos.y < gridHeight)
                    {
                        occupancy[rowBase + x] = i;
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
                        occupancy[y * gridWidth + pos.x] = i;
                    }
                }
            }
        }
    }

    /// <summary>
    /// requireDocked = gameplay-regel (VehicleController.CanPerformExitRight): horizontaal,
    /// op exitRow en volledig tegen de rechterrand. Anders de oude "vrij pad naar rechts"-regel.
    /// </summary>
    private static bool CanTargetExit(
        BoardState state,
        int targetIndex,
        int targetLength,
        bool targetIsHorizontal,
        int exitRow,
        int gridWidth,
        int[] occupancy,
        bool requireDocked)
    {
        if (state.IsExited(targetIndex))
        {
            return false;
        }

        Vector2Int pos = state.positions[targetIndex];
        if (pos.y != exitRow)
        {
            return false;
        }

        if (requireDocked)
        {
            if (!targetIsHorizontal)
            {
                return false;
            }

            return pos.x == gridWidth - targetLength;
        }

        int rightMost = pos.x + targetLength - 1;
        int rowBase = pos.y * gridWidth;
        for (int x = rightMost + 1; x < gridWidth; x++)
        {
            if (occupancy[rowBase + x] >= 0)
            {
                return false;
            }
        }

        return true;
    }

    public static List<Vector2Int> GetCells(Vector2Int start, bool horizontal, int length)
    {
        List<Vector2Int> cells = new List<Vector2Int>(length);
        for (int i = 0; i < length; i++)
        {
            if (horizontal)
            {
                cells.Add(new Vector2Int(start.x + i, start.y));
            }
            else
            {
                cells.Add(new Vector2Int(start.x, start.y + i));
            }
        }

        return cells;
    }

    private static int FindTargetIndex(List<VehicleDefinition> vehicles)
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

    // -------------------------------------------------------------------------
    // Interne BFS-helpers
    // -------------------------------------------------------------------------

    private sealed class BfsContext
    {
        public bool[] isHorizontal;
        public int[] lengths;
        public string[] names;
        public int vehicleCount;
        public int gridWidth;
        public int gridHeight;
        public int cellCount;
        public int[] occupancy;
        public ZobristTables zobrist;
        public Queue<QueueItem> queue;
        public Dictionary<ulong, BoardState> discoveredSingle;
        public Dictionary<ulong, List<BoardState>> discoveredMulti;
        public Dictionary<BoardState, ParentLink> cameFrom;
        public SolverResult result;
        public SolveConstraints constraints;
        public int discoveredCount;
        public int maxDiscoveredStates;
        public long visitedTicks;
        public long copyTicks;

        /// <summary>Extra moves die na een state minimaal nog nodig zijn (single target: de exit).</summary>
        public int minRemainingMoves;

        public bool WithinMoveBudget(int childDepth)
        {
            int max = constraints.maxSolutionMoves;
            return max < 0 || childDepth + minRemainingMoves <= max;
        }
    }

    private readonly struct QueueItem
    {
        public readonly BoardState state;
        public readonly ulong hash;
        public readonly int depth;

        public QueueItem(BoardState state, ulong hash, int depth)
        {
            this.state = state;
            this.hash = hash;
            this.depth = depth;
        }
    }

    private readonly struct ParentLink
    {
        public readonly BoardState parent;
        public readonly int vehicleIndex;
        public readonly Vector2Int from;
        public readonly Vector2Int to;
        public readonly bool exitsBoard;

        public static readonly ParentLink Root =
            new ParentLink(null, -1, default, default, false);

        public ParentLink(
            BoardState parent,
            int vehicleIndex,
            Vector2Int from,
            Vector2Int to,
            bool exitsBoard)
        {
            this.parent = parent;
            this.vehicleIndex = vehicleIndex;
            this.from = from;
            this.to = to;
            this.exitsBoard = exitsBoard;
        }
    }
}

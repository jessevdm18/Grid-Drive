using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Runtime-safe Rush Hour BFS-solver voor Grid Drive.
/// Werkt uitsluitend op data (geen Editor, geen GameObjects).
/// Move-definitie: één voertuig naar een andere geldige gridpositie = 1 move;
/// target exit = finale move.
/// Gridgrootte is variabel (per level), geen vaste 6x6.
/// </summary>
public static class RushOutSolver
{
    public const int DefaultGridSize = 6;
    public const int DefaultMaxStates = 100000;

    public const string SearchLimitExplored = "explored limit";
    public const string SearchLimitDiscovered = "discovered limit";

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
    /// </summary>
    public sealed class BoardState : IEquatable<BoardState>
    {
        public readonly Vector2Int[] positions;
        private readonly int cachedHash;

        public BoardState(Vector2Int[] positions)
        {
            this.positions = new Vector2Int[positions.Length];
            Array.Copy(positions, this.positions, positions.Length);
            cachedHash = ComputeHash(this.positions);
        }

        public BoardState(IList<Vector2Int> positions)
            : this(ToArray(positions))
        {
        }

        /// <summary>
        /// Neemt ownership van de array (geen extra copy). Alleen voor interne move-generatie.
        /// </summary>
        private BoardState(Vector2Int[] ownedPositions, bool _)
        {
            positions = ownedPositions;
            cachedHash = ComputeHash(positions);
        }

        public BoardState WithMovedVehicle(int index, Vector2Int newPos)
        {
            Vector2Int[] copy = new Vector2Int[positions.Length];
            Array.Copy(positions, copy, positions.Length);
            copy[index] = newPos;
            return new BoardState(copy, true);
        }

        public bool Equals(BoardState other)
        {
            if (other == null || other.positions.Length != positions.Length)
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

        private static int ComputeHash(Vector2Int[] positions)
        {
            unchecked
            {
                int hash = 17;
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

        int targetIndex = FindTargetIndex(vehicles);
        if (targetIndex < 0)
        {
            result.solvable = false;
            return result;
        }

        return RunBfs(
            vehicles,
            exitRow,
            targetIndex,
            startState,
            width,
            height,
            result,
            exploredCap,
            Mathf.Max(1, discoveredCap)
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
            maxDiscoveredStates
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

    // -------------------------------------------------------------------------
    // BFS
    // -------------------------------------------------------------------------

    private static SolverResult RunBfs(
        List<VehicleDefinition> vehicles,
        int exitRow,
        int targetIndex,
        BoardState start,
        int gridWidth,
        int gridHeight,
        SolverResult result,
        int maxStates,
        int maxDiscoveredStates)
    {
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

        ulong[] zobrist = CreateZobristTable(vehicleCount, cellCount);
        ulong startHash = ComputeZobristHash(zobrist, start.positions, gridWidth, cellCount);

        int[] occupancy = new int[cellCount];

        Queue<QueueItem> queue = new Queue<QueueItem>();
        // Zobrist-buckets: snelle pre-check; structurele match bij hash-hit (collision-safe).
        Dictionary<ulong, BoardState> discoveredSingle =
            new Dictionary<ulong, BoardState>();
        Dictionary<ulong, List<BoardState>> discoveredMulti =
            new Dictionary<ulong, List<BoardState>>();
        Dictionary<BoardState, ParentLink> cameFrom = new Dictionary<BoardState, ParentLink>();

        queue.Enqueue(new QueueItem(start, startHash));
        AddDiscovered(discoveredSingle, discoveredMulti, startHash, start);
        cameFrom[start] = ParentLink.Root;
        int discoveredCount = 1;
        result.discoveredStates = 1;
        result.queuePeakSize = 1;

        int explored = 0;
        long occTicks = 0;
        long moveTicks = 0;
        long visitedTicks = 0;
        long copyTicks = 0;
        long tickFreq = Stopwatch.Frequency;

        while (queue.Count > 0)
        {
            if (explored >= maxStates)
            {
                result.searchLimitReached = true;
                result.searchLimitReason = SearchLimitExplored;
                FinishProfiling(
                    result,
                    explored,
                    discoveredCount,
                    occTicks,
                    moveTicks,
                    visitedTicks,
                    copyTicks,
                    tickFreq
                );
                result.solvable = false;
                return result;
            }

            QueueItem currentItem = queue.Dequeue();
            BoardState current = currentItem.state;
            ulong currentHash = currentItem.hash;
            explored++;

            long t0 = Stopwatch.GetTimestamp();
            BuildOccupancyFlat(
                occupancy,
                isHorizontal,
                lengths,
                current.positions,
                vehicleCount,
                gridWidth,
                gridHeight
            );
            result.occupancyBuildCount++;
            occTicks += Stopwatch.GetTimestamp() - t0;

            if (CanTargetExit(current, targetIndex, lengths[targetIndex], exitRow, gridWidth, occupancy))
            {
                result.solvable = true;
                FinishProfiling(
                    result,
                    explored,
                    discoveredCount,
                    occTicks,
                    moveTicks,
                    visitedTicks,
                    copyTicks,
                    tickFreq
                );
                result.solution = ReconstructPath(
                    cameFrom,
                    current,
                    names,
                    targetIndex,
                    current.positions[targetIndex]
                );
                result.minimumMoves = result.solution.Count;
                return result;
            }

            long visBefore = visitedTicks;
            long copyBefore = copyTicks;
            t0 = Stopwatch.GetTimestamp();
            bool hitDiscoveredLimit = !ExpandMoves(
                current,
                currentHash,
                isHorizontal,
                lengths,
                vehicleCount,
                gridWidth,
                gridHeight,
                cellCount,
                occupancy,
                zobrist,
                queue,
                discoveredSingle,
                discoveredMulti,
                cameFrom,
                result,
                ref discoveredCount,
                maxDiscoveredStates,
                ref visitedTicks,
                ref copyTicks
            );
            long expandElapsed = Stopwatch.GetTimestamp() - t0;
            moveTicks += expandElapsed
                - (visitedTicks - visBefore)
                - (copyTicks - copyBefore);

            if (queue.Count > result.queuePeakSize)
            {
                result.queuePeakSize = queue.Count;
            }

            if (hitDiscoveredLimit)
            {
                result.searchLimitReached = true;
                result.searchLimitReason = SearchLimitDiscovered;
                FinishProfiling(
                    result,
                    explored,
                    discoveredCount,
                    occTicks,
                    moveTicks,
                    visitedTicks,
                    copyTicks,
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
            discoveredCount,
            occTicks,
            moveTicks,
            visitedTicks,
            copyTicks,
            tickFreq
        );
        return result;
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
        BoardState state,
        ulong stateHash,
        bool[] isHorizontal,
        int[] lengths,
        int vehicleCount,
        int gridWidth,
        int gridHeight,
        int cellCount,
        int[] occupancy,
        ulong[] zobrist,
        Queue<QueueItem> queue,
        Dictionary<ulong, BoardState> discoveredSingle,
        Dictionary<ulong, List<BoardState>> discoveredMulti,
        Dictionary<BoardState, ParentLink> cameFrom,
        SolverResult result,
        ref int discoveredCount,
        int maxDiscoveredStates,
        ref long visitedTicks,
        ref long copyTicks)
    {
        Vector2Int[] positions = state.positions;

        for (int i = 0; i < vehicleCount; i++)
        {
            Vector2Int from = positions[i];
            int length = lengths[i];

            if (isHorizontal[i])
            {
                for (int steps = 1; ; steps++)
                {
                    int newX = from.x - steps;
                    if (newX < 0)
                    {
                        break;
                    }

                    int checkIdx = from.y * gridWidth + (from.x - steps);
                    if (occupancy[checkIdx] >= 0)
                    {
                        break;
                    }

                    if (!TryEnqueueChild(
                            state,
                            stateHash,
                            i,
                            from,
                            new Vector2Int(newX, from.y),
                            gridWidth,
                            cellCount,
                            zobrist,
                            queue,
                            discoveredSingle,
                            discoveredMulti,
                            cameFrom,
                            result,
                            ref discoveredCount,
                            maxDiscoveredStates,
                            ref visitedTicks,
                            ref copyTicks))
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
                            state,
                            stateHash,
                            i,
                            from,
                            new Vector2Int(newX, from.y),
                            gridWidth,
                            cellCount,
                            zobrist,
                            queue,
                            discoveredSingle,
                            discoveredMulti,
                            cameFrom,
                            result,
                            ref discoveredCount,
                            maxDiscoveredStates,
                            ref visitedTicks,
                            ref copyTicks))
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
                            state,
                            stateHash,
                            i,
                            from,
                            new Vector2Int(from.x, newY),
                            gridWidth,
                            cellCount,
                            zobrist,
                            queue,
                            discoveredSingle,
                            discoveredMulti,
                            cameFrom,
                            result,
                            ref discoveredCount,
                            maxDiscoveredStates,
                            ref visitedTicks,
                            ref copyTicks))
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
                            state,
                            stateHash,
                            i,
                            from,
                            new Vector2Int(from.x, newY),
                            gridWidth,
                            cellCount,
                            zobrist,
                            queue,
                            discoveredSingle,
                            discoveredMulti,
                            cameFrom,
                            result,
                            ref discoveredCount,
                            maxDiscoveredStates,
                            ref visitedTicks,
                            ref copyTicks))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Returns false bij discovered-limit. Duplicate → true (doorgaan).
    /// Discover-markering gebeurt hier bij enqueue, niet bij dequeue.
    /// </summary>
    private static bool TryEnqueueChild(
        BoardState state,
        ulong stateHash,
        int vehicleIndex,
        Vector2Int from,
        Vector2Int to,
        int gridWidth,
        int cellCount,
        ulong[] zobrist,
        Queue<QueueItem> queue,
        Dictionary<ulong, BoardState> discoveredSingle,
        Dictionary<ulong, List<BoardState>> discoveredMulti,
        Dictionary<BoardState, ParentLink> cameFrom,
        SolverResult result,
        ref int discoveredCount,
        int maxDiscoveredStates,
        ref long visitedTicks,
        ref long copyTicks)
    {
        result.generatedMoves++;

        ulong childHash = HashAfterMove(
            stateHash,
            zobrist,
            vehicleIndex,
            from,
            to,
            gridWidth,
            cellCount
        );

        long tVis = Stopwatch.GetTimestamp();
        if (IsDiscoveredMatch(
                discoveredSingle,
                discoveredMulti,
                childHash,
                state,
                vehicleIndex,
                to))
        {
            visitedTicks += Stopwatch.GetTimestamp() - tVis;
            result.visitedPrecheckRejects++;
            return true;
        }

        visitedTicks += Stopwatch.GetTimestamp() - tVis;

        if (discoveredCount >= maxDiscoveredStates)
        {
            return false;
        }

        long tCopy = Stopwatch.GetTimestamp();
        BoardState next = state.WithMovedVehicle(vehicleIndex, to);
        copyTicks += Stopwatch.GetTimestamp() - tCopy;
        result.childStatesCreated++;

        AddDiscovered(discoveredSingle, discoveredMulti, childHash, next);
        discoveredCount++;
        cameFrom[next] = new ParentLink(state, vehicleIndex, from, to);
        queue.Enqueue(new QueueItem(next, childHash));
        result.childStatesEnqueued++;
        return true;
    }

    private static bool IsDiscoveredMatch(
        Dictionary<ulong, BoardState> single,
        Dictionary<ulong, List<BoardState>> multi,
        ulong hash,
        BoardState parent,
        int vehicleIndex,
        Vector2Int to)
    {
        if (multi.TryGetValue(hash, out List<BoardState> list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (MatchesMovedState(list[i], parent, vehicleIndex, to))
                {
                    return true;
                }
            }

            return false;
        }

        if (single.TryGetValue(hash, out BoardState existing))
        {
            return MatchesMovedState(existing, parent, vehicleIndex, to);
        }

        return false;
    }

    /// <summary>
    /// Structurele check zonder child-array: existing == parent met vehicle op 'to'.
    /// </summary>
    private static bool MatchesMovedState(
        BoardState existing,
        BoardState parent,
        int vehicleIndex,
        Vector2Int to)
    {
        Vector2Int[] a = existing.positions;
        Vector2Int[] b = parent.positions;
        if (a.Length != b.Length)
        {
            return false;
        }

        for (int i = 0; i < a.Length; i++)
        {
            Vector2Int expected = i == vehicleIndex ? to : b[i];
            if (a[i] != expected)
            {
                return false;
            }
        }

        return true;
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

    private static ulong[] CreateZobristTable(int vehicleCount, int cellCount)
    {
        // Deterministische PRNG — zelfde seed → zelfde hashes tussen runs.
        ulong seed = 0xC0FFEE5EEDUL;
        ulong[] table = new ulong[vehicleCount * cellCount];
        for (int i = 0; i < table.Length; i++)
        {
            table[i] = SplitMix64(ref seed);
        }

        return table;
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
        ulong[] zobrist,
        Vector2Int[] positions,
        int gridWidth,
        int cellCount)
    {
        ulong hash = 0;
        for (int i = 0; i < positions.Length; i++)
        {
            int cell = positions[i].y * gridWidth + positions[i].x;
            hash ^= zobrist[i * cellCount + cell];
        }

        return hash;
    }

    private static ulong HashAfterMove(
        ulong parentHash,
        ulong[] zobrist,
        int vehicleIndex,
        Vector2Int from,
        Vector2Int to,
        int gridWidth,
        int cellCount)
    {
        int baseIndex = vehicleIndex * cellCount;
        int oldCell = from.y * gridWidth + from.x;
        int newCell = to.y * gridWidth + to.x;
        return parentHash
            ^ zobrist[baseIndex + oldCell]
            ^ zobrist[baseIndex + newCell];
    }

    private static List<SolverMove> ReconstructPath(
        Dictionary<BoardState, ParentLink> cameFrom,
        BoardState endState,
        string[] names,
        int targetIndex,
        Vector2Int targetPos)
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
                exitsBoard: false
            ));
            cursor = link.parent;
        }

        path.Reverse();
        path.Add(new SolverMove(
            targetIndex,
            names[targetIndex],
            targetPos,
            targetPos,
            exitsBoard: true
        ));
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

    private static bool CanTargetExit(
        BoardState state,
        int targetIndex,
        int targetLength,
        int exitRow,
        int gridWidth,
        int[] occupancy)
    {
        Vector2Int pos = state.positions[targetIndex];
        if (pos.y != exitRow)
        {
            return false;
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
            if (vehicles[i].canExitRight)
            {
                return i;
            }
        }

        return -1;
    }

    // -------------------------------------------------------------------------
    // Interne BFS-helpers
    // -------------------------------------------------------------------------

    private readonly struct QueueItem
    {
        public readonly BoardState state;
        public readonly ulong hash;

        public QueueItem(BoardState state, ulong hash)
        {
            this.state = state;
            this.hash = hash;
        }
    }

    private readonly struct ParentLink
    {
        public readonly BoardState parent;
        public readonly int vehicleIndex;
        public readonly Vector2Int from;
        public readonly Vector2Int to;

        public static readonly ParentLink Root = new ParentLink(null, -1, default, default);

        public ParentLink(BoardState parent, int vehicleIndex, Vector2Int from, Vector2Int to)
        {
            this.parent = parent;
            this.vehicleIndex = vehicleIndex;
            this.from = from;
            this.to = to;
        }
    }
}

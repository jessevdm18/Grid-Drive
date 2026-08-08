using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime-safe Rush Hour BFS-solver voor Rush Out.
/// Werkt uitsluitend op data (geen Editor, geen GameObjects).
/// Move-definitie: één voertuig naar een andere geldige gridpositie = 1 move;
/// target exit = finale move.
/// Gridgrootte is variabel (per level), geen vaste 6x6.
/// </summary>
public static class RushOutSolver
{
    public const int DefaultGridSize = 6;
    public const int DefaultMaxStates = 100000;

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

        public BoardState WithMovedVehicle(int index, Vector2Int newPos)
        {
            Vector2Int[] copy = new Vector2Int[positions.Length];
            Array.Copy(positions, copy, positions.Length);
            copy[index] = newPos;
            return new BoardState(copy);
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
    }

    // -------------------------------------------------------------------------
    // Publieke API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Lost een puzzel op vanaf een willekeurige actuele board-state.
    /// </summary>
    public static SolverResult Solve(
        List<VehicleDefinition> vehicles,
        BoardState startState,
        int exitRow,
        int gridWidth,
        int gridHeight,
        int maxStates = DefaultMaxStates)
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
            maxStates
        );
    }

    /// <summary>
    /// Convenience: bouw VehicleDefinition + BoardState vanuit LevelData-startposities.
    /// </summary>
    public static SolverResult SolveLevelData(LevelData levelData, int maxStates = DefaultMaxStates)
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
            maxStates
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
        int maxStates)
    {
        Queue<BoardState> queue = new Queue<BoardState>();
        HashSet<BoardState> visited = new HashSet<BoardState>();
        Dictionary<BoardState, BfsNode> cameFrom = new Dictionary<BoardState, BfsNode>();

        queue.Enqueue(start);
        visited.Add(start);
        cameFrom[start] = new BfsNode(null, null);

        int explored = 0;

        while (queue.Count > 0)
        {
            if (explored >= maxStates)
            {
                result.searchLimitReached = true;
                result.statesExplored = explored;
                result.solvable = false;
                return result;
            }

            BoardState current = queue.Dequeue();
            explored++;

            SolverMove exitMove = TryCreateExitMove(
                vehicles,
                exitRow,
                targetIndex,
                current,
                gridWidth,
                gridHeight
            );
            if (exitMove != null)
            {
                result.solvable = true;
                result.statesExplored = explored;
                result.solution = ReconstructPath(cameFrom, current, exitMove);
                result.minimumMoves = result.solution.Count;
                return result;
            }

            List<MoveCandidate> candidates = GenerateMoveCandidates(
                vehicles,
                current,
                gridWidth,
                gridHeight
            );
            foreach (MoveCandidate candidate in candidates)
            {
                if (visited.Contains(candidate.nextState))
                {
                    continue;
                }

                visited.Add(candidate.nextState);
                cameFrom[candidate.nextState] = new BfsNode(current, candidate.move);
                queue.Enqueue(candidate.nextState);
            }
        }

        result.solvable = false;
        result.statesExplored = explored;
        return result;
    }

    private static List<SolverMove> ReconstructPath(
        Dictionary<BoardState, BfsNode> cameFrom,
        BoardState endState,
        SolverMove finalExitMove)
    {
        List<SolverMove> path = new List<SolverMove>();
        BoardState cursor = endState;

        while (cameFrom.TryGetValue(cursor, out BfsNode node) && node.parent != null)
        {
            path.Add(node.move);
            cursor = node.parent;
        }

        path.Reverse();
        path.Add(finalExitMove);
        return path;
    }

    // -------------------------------------------------------------------------
    // Move-generatie
    // -------------------------------------------------------------------------

    private static SolverMove TryCreateExitMove(
        List<VehicleDefinition> vehicles,
        int exitRow,
        int targetIndex,
        BoardState state,
        int gridWidth,
        int gridHeight)
    {
        VehicleDefinition target = vehicles[targetIndex];
        Vector2Int pos = state.positions[targetIndex];

        if (pos.y != exitRow)
        {
            return null;
        }

        int rightMost = pos.x + target.lengthInCells - 1;
        bool[,] occupied = BuildOccupancy(
            vehicles,
            state,
            ignoreIndex: targetIndex,
            gridWidth,
            gridHeight
        );

        for (int x = rightMost + 1; x < gridWidth; x++)
        {
            if (occupied[x, pos.y])
            {
                return null;
            }
        }

        return new SolverMove(
            targetIndex,
            target.name,
            pos,
            pos,
            exitsBoard: true
        );
    }

    private static List<MoveCandidate> GenerateMoveCandidates(
        List<VehicleDefinition> vehicles,
        BoardState state,
        int gridWidth,
        int gridHeight)
    {
        List<MoveCandidate> candidates = new List<MoveCandidate>();

        for (int i = 0; i < vehicles.Count; i++)
        {
            VehicleDefinition vehicle = vehicles[i];
            Vector2Int from = state.positions[i];
            bool[,] occupied = BuildOccupancy(
                vehicles,
                state,
                ignoreIndex: i,
                gridWidth,
                gridHeight
            );

            if (vehicle.IsHorizontal)
            {
                for (int steps = 1; ; steps++)
                {
                    int newX = from.x - steps;
                    if (newX < 0)
                    {
                        break;
                    }

                    int checkX = from.x - steps;
                    if (occupied[checkX, from.y])
                    {
                        break;
                    }

                    Vector2Int to = new Vector2Int(newX, from.y);
                    candidates.Add(CreateCandidate(state, i, from, to, vehicle.name));
                }

                for (int steps = 1; ; steps++)
                {
                    int newX = from.x + steps;
                    int rightMost = newX + vehicle.lengthInCells - 1;
                    if (rightMost >= gridWidth)
                    {
                        break;
                    }

                    if (occupied[rightMost, from.y])
                    {
                        break;
                    }

                    Vector2Int to = new Vector2Int(newX, from.y);
                    candidates.Add(CreateCandidate(state, i, from, to, vehicle.name));
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

                    if (occupied[from.x, newY])
                    {
                        break;
                    }

                    Vector2Int to = new Vector2Int(from.x, newY);
                    candidates.Add(CreateCandidate(state, i, from, to, vehicle.name));
                }

                for (int steps = 1; ; steps++)
                {
                    int newY = from.y + steps;
                    int topMost = newY + vehicle.lengthInCells - 1;
                    if (topMost >= gridHeight)
                    {
                        break;
                    }

                    if (occupied[from.x, topMost])
                    {
                        break;
                    }

                    Vector2Int to = new Vector2Int(from.x, newY);
                    candidates.Add(CreateCandidate(state, i, from, to, vehicle.name));
                }
            }
        }

        return candidates;
    }

    private static MoveCandidate CreateCandidate(
        BoardState state,
        int vehicleIndex,
        Vector2Int from,
        Vector2Int to,
        string vehicleName)
    {
        BoardState next = state.WithMovedVehicle(vehicleIndex, to);
        SolverMove move = new SolverMove(vehicleIndex, vehicleName, from, to, exitsBoard: false);
        return new MoveCandidate(next, move);
    }

    // -------------------------------------------------------------------------
    // Occupancy
    // -------------------------------------------------------------------------

    private static bool[,] BuildOccupancy(
        List<VehicleDefinition> vehicles,
        BoardState state,
        int ignoreIndex,
        int gridWidth,
        int gridHeight)
    {
        bool[,] occupied = new bool[gridWidth, gridHeight];

        for (int i = 0; i < vehicles.Count; i++)
        {
            if (i == ignoreIndex)
            {
                continue;
            }

            VehicleDefinition v = vehicles[i];
            Vector2Int pos = state.positions[i];
            List<Vector2Int> cells = GetCells(pos, v.IsHorizontal, v.lengthInCells);

            foreach (Vector2Int cell in cells)
            {
                if (cell.x >= 0 && cell.x < gridWidth && cell.y >= 0 && cell.y < gridHeight)
                {
                    occupied[cell.x, cell.y] = true;
                }
            }
        }

        return occupied;
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

    private sealed class BfsNode
    {
        public readonly BoardState parent;
        public readonly SolverMove move;

        public BfsNode(BoardState parent, SolverMove move)
        {
            this.parent = parent;
            this.move = move;
        }
    }

    private sealed class MoveCandidate
    {
        public readonly BoardState nextState;
        public readonly SolverMove move;

        public MoveCandidate(BoardState nextState, SolverMove move)
        {
            this.nextState = nextState;
            this.move = move;
        }
    }
}

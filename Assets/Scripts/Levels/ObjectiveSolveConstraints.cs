using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds RushOutSolver.SolveConstraints from LevelData for gameplay hints + validation.
/// Runtime-safe (no UnityEditor).
/// </summary>
public static class ObjectiveSolveConstraints
{
    public static RushOutSolver.SolveConstraints FromLevelData(LevelData level)
    {
        RushOutSolver.SolveConstraints c = RushOutSolver.SolveConstraints.Classic();
        c.requireDockedExit = true;

        if (level == null || level.vehicles == null)
        {
            return c;
        }

        switch (level.objectiveType)
        {
            case LevelObjectiveType.MultiTargetRescue:
                c.multiTarget = true;
                break;

            case LevelObjectiveType.NoTouchChallenge:
                c.protectedVehicleIndex = FindFlag(level, v => v.isProtectedVehicle);
                break;

            case LevelObjectiveType.FragileCargo:
                c.fragileVehicleIndex = FindFlag(level, v => v.isFragileCargo);
                c.fragileMoveLimit = Mathf.Max(0, level.fragileCargoMoveLimit);
                break;

            case LevelObjectiveType.LimitedVehicle:
                c.limitedVehicleIndex = FindFlag(level, v => v.isLimitedVehicle);
                c.limitedMoveLimit = Mathf.Max(0, level.limitedVehicleMoveLimit);
                break;

            case LevelObjectiveType.MoveLimit:
                c.maxSolutionMoves = Mathf.Max(0, level.moveLimit);
                break;
        }

        return c;
    }

    /// <summary>
    /// Remap LevelData-based constraints onto the active runtime vehicle list
    /// (exited cars already removed). Applies remaining move budgets when possible.
    /// </summary>
    public static RushOutSolver.SolveConstraints ForRuntimeHint(
        LevelData level,
        IList<VehicleController> runtimeVehicles)
    {
        RushOutSolver.SolveConstraints source = FromLevelData(level);
        RushOutSolver.SolveConstraints c = RushOutSolver.SolveConstraints.Classic();
        c.requireDockedExit = true;
        c.multiTarget = source.multiTarget;

        if (runtimeVehicles == null || runtimeVehicles.Count == 0)
        {
            return c;
        }

        LevelObjectiveController objectives =
            Object.FindFirstObjectByType<LevelObjectiveController>();

        if (source.protectedVehicleIndex >= 0)
        {
            c.protectedVehicleIndex = IndexOfController(
                runtimeVehicles,
                v => v != null && v.IsProtectedVehicle
            );
        }

        if (source.fragileVehicleIndex >= 0)
        {
            c.fragileVehicleIndex = IndexOfMatching(
                runtimeVehicles,
                objectives != null ? objectives.FragileCargoVehicle : null,
                v => v != null && v.IsFragileCargo
            );
            int remaining = objectives != null
                ? objectives.CargoMovesRemaining
                : source.fragileMoveLimit;
            c.fragileMoveLimit = Mathf.Max(0, remaining);
        }

        if (source.limitedVehicleIndex >= 0)
        {
            c.limitedVehicleIndex = IndexOfMatching(
                runtimeVehicles,
                objectives != null ? objectives.LimitedVehicle : null,
                v => v != null && v.IsLimitedVehicle
            );
            int remaining = objectives != null
                ? objectives.LimitedVehicleMovesRemaining
                : source.limitedMoveLimit;
            c.limitedMoveLimit = Mathf.Max(0, remaining);
        }

        if (source.maxSolutionMoves >= 0)
        {
            int remaining = objectives != null
                ? objectives.MovesRemaining
                : source.maxSolutionMoves;
            c.maxSolutionMoves = Mathf.Max(0, remaining);
        }

        return c;
    }

    private static int IndexOfMatching(
        IList<VehicleController> list,
        VehicleController preferred,
        System.Predicate<VehicleController> fallback)
    {
        if (preferred != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == preferred)
                {
                    return i;
                }
            }
        }

        return IndexOfController(list, fallback);
    }

    private static int IndexOfController(
        IList<VehicleController> list,
        System.Predicate<VehicleController> pred)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (pred(list[i]))
            {
                return i;
            }
        }

        return -1;
    }

    private static int FindFlag(LevelData level, System.Predicate<VehicleData> pred)
    {
        for (int i = 0; i < level.vehicles.Count; i++)
        {
            if (level.vehicles[i] != null && pred(level.vehicles[i]))
            {
                return i;
            }
        }

        return -1;
    }
}

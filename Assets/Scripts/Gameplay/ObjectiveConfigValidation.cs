using UnityEngine;

/// <summary>
/// Shared special-objective config checks. Invalid configs must not crash;
/// callers fall back to Classic presentation/gameplay.
/// </summary>
public static class ObjectiveConfigValidation
{
    /// <summary>
    /// Returns null when valid; otherwise a short Missing=... reason.
    /// </summary>
    public static string ValidateSpecialObjective(LevelData levelData)
    {
        if (levelData == null)
        {
            return "LevelDataNull";
        }

        switch (levelData.objectiveType)
        {
            case LevelObjectiveType.Classic:
                return null;

            case LevelObjectiveType.TimedAmbulance:
                if (levelData.timeLimitSeconds <= 0f)
                {
                    return "timeLimitSeconds<=0";
                }

                if (CountExitTargets(levelData) < 1)
                {
                    return "no canExitRight target";
                }

                return null;

            case LevelObjectiveType.MoveLimit:
                if (levelData.moveLimit <= 0)
                {
                    return "moveLimit<=0";
                }

                return null;

            case LevelObjectiveType.MultiTargetRescue:
                if (CountExitTargets(levelData) < 2)
                {
                    return "exitTargets<2";
                }

                return null;

            case LevelObjectiveType.NoTouchChallenge:
                if (CountFlag(levelData, v => v.isProtectedVehicle) < 1)
                {
                    return "no isProtectedVehicle";
                }

                if (CountExitTargets(levelData) < 1)
                {
                    return "no canExitRight target";
                }

                return null;

            case LevelObjectiveType.FragileCargo:
                if (levelData.fragileCargoMoveLimit <= 0)
                {
                    return "fragileCargoMoveLimit<=0";
                }

                int cargoCount = CountFlag(levelData, v => v.isFragileCargo);
                if (cargoCount < 1)
                {
                    return "no isFragileCargo vehicle";
                }

                if (cargoCount > 1)
                {
                    return "multiple isFragileCargo vehicles";
                }

                if (!HasFragileCargoExitTarget(levelData))
                {
                    return "fragile cargo not canExitRight";
                }

                return null;

            case LevelObjectiveType.LimitedVehicle:
                if (levelData.limitedVehicleMoveLimit <= 0)
                {
                    return "limitedVehicleMoveLimit<=0";
                }

                if (CountFlag(levelData, v => v.isLimitedVehicle) < 1)
                {
                    return "no isLimitedVehicle";
                }

                return null;

            default:
                return "unknownObjectiveType";
        }
    }

    public static void LogValidation(
        LevelData levelData,
        string missing,
        int databaseIndex = -1)
    {
        if (string.IsNullOrEmpty(missing))
        {
            return;
        }

        string asset = levelData != null ? levelData.name : "?";
        string objective = levelData != null
            ? levelData.objectiveType.ToString()
            : "?";

        Debug.LogError(
            "[ObjectiveValidation]\n" +
            "DbIndex=" + databaseIndex + "\n" +
            "Level=" + asset + "\n" +
            "Objective=" + objective + "\n" +
            "Missing=" + missing + "\n" +
            "Fallback=Classic"
        );
    }

    private static int CountExitTargets(LevelData levelData)
    {
        return CountFlag(levelData, v => v.canExitRight);
    }

    private static bool HasFragileCargoExitTarget(LevelData levelData)
    {
        if (levelData == null || levelData.vehicles == null)
        {
            return false;
        }

        for (int i = 0; i < levelData.vehicles.Count; i++)
        {
            VehicleData v = levelData.vehicles[i];
            if (v != null && v.isFragileCargo && v.canExitRight)
            {
                return true;
            }
        }

        return false;
    }

    private static int CountFlag(
        LevelData levelData,
        System.Func<VehicleData, bool> predicate)
    {
        if (levelData == null || levelData.vehicles == null || predicate == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < levelData.vehicles.Count; i++)
        {
            VehicleData v = levelData.vehicles[i];
            if (v != null && predicate(v))
            {
                count++;
            }
        }

        return count;
    }
}

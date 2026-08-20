using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only: clear / apply special objective vehicle flags and limits on LevelData.
/// Does not change objectiveType or difficulty — caller writes those.
/// </summary>
public static class LevelSpecialConfigEditor
{
    /// <summary>
    /// Clears objective-specific flags that are safely reconfigurable.
    /// Preserves primary canExitRight targets (does not strip exit flags).
    /// </summary>
    public static void ClearSpecialConfiguration(LevelData level)
    {
        if (level == null || level.vehicles == null)
        {
            return;
        }

        for (int i = 0; i < level.vehicles.Count; i++)
        {
            VehicleData v = level.vehicles[i];
            if (v == null)
            {
                continue;
            }

            v.isProtectedVehicle = false;
            v.isFragileCargo = false;
            v.isLimitedVehicle = false;
        }

        // Limits are objective-specific; zero until re-applied.
        level.fragileCargoMoveLimit = 0;
        level.limitedVehicleMoveLimit = 0;
        // timeLimitSeconds / moveLimit left for Timed/MoveLimit assignment to set.
    }

    public static bool TryApplyNoTouch(LevelData level, int protectedVehicleIndex, out string error)
    {
        error = string.Empty;
        if (level == null || level.vehicles == null)
        {
            error = "null level";
            return false;
        }

        if (protectedVehicleIndex < 0 || protectedVehicleIndex >= level.vehicles.Count)
        {
            error = "invalid protected vehicle index";
            return false;
        }

        VehicleData chosen = level.vehicles[protectedVehicleIndex];
        if (chosen == null)
        {
            error = "null vehicle";
            return false;
        }

        if (chosen.canExitRight)
        {
            error = "protected vehicle must be non-target";
            return false;
        }

        ClearSpecialConfiguration(level);
        for (int i = 0; i < level.vehicles.Count; i++)
        {
            VehicleData v = level.vehicles[i];
            if (v == null)
            {
                continue;
            }

            v.isProtectedVehicle = i == protectedVehicleIndex;
        }

        return true;
    }

    public static bool TryApplyFragileCargo(
        LevelData level,
        int cargoTargetIndex,
        int cargoLimit,
        out string error)
    {
        error = string.Empty;
        if (level == null || level.vehicles == null)
        {
            error = "null level";
            return false;
        }

        if (cargoTargetIndex < 0 || cargoTargetIndex >= level.vehicles.Count)
        {
            error = "invalid cargo target index";
            return false;
        }

        VehicleData cargo = level.vehicles[cargoTargetIndex];
        if (cargo == null || !cargo.canExitRight)
        {
            error = "fragile cargo must be canExitRight target";
            return false;
        }

        if (cargoLimit <= 0)
        {
            error = "fragileCargoMoveLimit must be > 0";
            return false;
        }

        ClearSpecialConfiguration(level);
        for (int i = 0; i < level.vehicles.Count; i++)
        {
            VehicleData v = level.vehicles[i];
            if (v == null)
            {
                continue;
            }

            v.isFragileCargo = i == cargoTargetIndex;
        }

        level.fragileCargoMoveLimit = cargoLimit;
        return true;
    }

    public static bool TryApplyLimitedVehicle(
        LevelData level,
        int limitedVehicleIndex,
        int limitedLimit,
        out string error)
    {
        error = string.Empty;
        if (level == null || level.vehicles == null)
        {
            error = "null level";
            return false;
        }

        if (limitedVehicleIndex < 0 || limitedVehicleIndex >= level.vehicles.Count)
        {
            error = "invalid limited vehicle index";
            return false;
        }

        VehicleData blocker = level.vehicles[limitedVehicleIndex];
        if (blocker == null || blocker.canExitRight)
        {
            error = "limited vehicle must be non-target blocker";
            return false;
        }

        if (limitedLimit <= 0)
        {
            error = "limitedVehicleMoveLimit must be > 0";
            return false;
        }

        ClearSpecialConfiguration(level);
        for (int i = 0; i < level.vehicles.Count; i++)
        {
            VehicleData v = level.vehicles[i];
            if (v == null)
            {
                continue;
            }

            v.isLimitedVehicle = i == limitedVehicleIndex;
            if (i == limitedVehicleIndex)
            {
                v.canExitRight = false;
            }
        }

        level.limitedVehicleMoveLimit = limitedLimit;
        return true;
    }

    public static bool TryApplyMultiTarget(
        LevelData level,
        int primaryIndex,
        int secondTargetIndex,
        out string error)
    {
        error = string.Empty;
        if (level == null || level.vehicles == null)
        {
            error = "null level";
            return false;
        }

        if (primaryIndex < 0 || primaryIndex >= level.vehicles.Count ||
            secondTargetIndex < 0 || secondTargetIndex >= level.vehicles.Count ||
            primaryIndex == secondTargetIndex)
        {
            error = "invalid multi-target indices";
            return false;
        }

        VehicleData primary = level.vehicles[primaryIndex];
        VehicleData second = level.vehicles[secondTargetIndex];
        if (primary == null || second == null)
        {
            error = "null vehicle";
            return false;
        }

        ClearSpecialConfiguration(level);

        // Ensure both are exit targets; leave other vehicles' canExitRight unchanged
        // except we only ADD second target — do not strip unrelated targets.
        primary.canExitRight = true;
        second.canExitRight = true;
        return true;
    }

    public static void ApplyTimedAmbulance(LevelData level, float timeLimitSeconds)
    {
        if (level == null)
        {
            return;
        }

        ClearSpecialConfiguration(level);
        if (timeLimitSeconds > 0f)
        {
            level.timeLimitSeconds = timeLimitSeconds;
        }
    }

    public static void ApplyMoveLimit(LevelData level, int moveLimit)
    {
        if (level == null)
        {
            return;
        }

        ClearSpecialConfiguration(level);
        if (moveLimit > 0)
        {
            level.moveLimit = moveLimit;
        }
    }

    public static void ApplyClassicCleanup(LevelData level)
    {
        if (level == null)
        {
            return;
        }

        ClearSpecialConfiguration(level);
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central Daily Challenge level eligibility.
/// Only normal/standard (Classic) puzzles are allowed — never special objectives.
/// </summary>
public static class DailyChallengeLevelEligibility
{
    public enum ExclusionReason
    {
        None = 0,
        NullLevel,
        NotClassicObjective,
        InvalidMinimumMoves,
        NoVehicles,
        NoExitTarget
    }

    /// <summary>
    /// Authoritative normal Daily level: <see cref="LevelObjectiveType.Classic"/> only.
    /// Unknown / special objective types are never eligible.
    /// </summary>
    public static bool IsEligible(LevelData level)
    {
        return TryEvaluate(level, out _);
    }

    public static bool TryEvaluate(LevelData level, out ExclusionReason reason)
    {
        if (level == null)
        {
            reason = ExclusionReason.NullLevel;
            return false;
        }

        if (level.objectiveType != LevelObjectiveType.Classic)
        {
            reason = ExclusionReason.NotClassicObjective;
            return false;
        }

        if (level.minimumMoves <= 0)
        {
            reason = ExclusionReason.InvalidMinimumMoves;
            return false;
        }

        if (level.vehicles == null || level.vehicles.Count == 0)
        {
            reason = ExclusionReason.NoVehicles;
            return false;
        }

        bool hasExitTarget = false;
        for (int i = 0; i < level.vehicles.Count; i++)
        {
            VehicleData v = level.vehicles[i];
            if (v != null && v.canExitRight)
            {
                hasExitTarget = true;
                break;
            }
        }

        if (!hasExitTarget)
        {
            reason = ExclusionReason.NoExitTarget;
            return false;
        }

        reason = ExclusionReason.None;
        return true;
    }

    public static string FormatReason(LevelData level, ExclusionReason reason)
    {
        string name = level != null ? level.name : "null";
        switch (reason)
        {
            case ExclusionReason.None:
                return name + " PASS — Normal";
            case ExclusionReason.NullLevel:
                return "null EXCLUDED — NullLevel";
            case ExclusionReason.NotClassicObjective:
                return name + " EXCLUDED — " +
                       (level != null ? level.objectiveType.ToString() : "Unknown");
            case ExclusionReason.InvalidMinimumMoves:
                return name + " EXCLUDED — minimumMoves<=0";
            case ExclusionReason.NoVehicles:
                return name + " EXCLUDED — NoVehicles";
            case ExclusionReason.NoExitTarget:
                return name + " EXCLUDED — NoExitTarget";
            default:
                return name + " EXCLUDED — " + reason;
        }
    }

    /// <summary>
    /// Builds eligible levels in candidate-pool order (stable).
    /// Also returns matching candidate pool indices.
    /// </summary>
    public static void CollectEligible(
        IReadOnlyList<LevelData> candidatePool,
        List<LevelData> eligibleOut,
        List<int> candidateIndicesOut)
    {
        eligibleOut.Clear();
        candidateIndicesOut.Clear();
        if (candidatePool == null)
        {
            return;
        }

        for (int i = 0; i < candidatePool.Count; i++)
        {
            LevelData level = candidatePool[i];
            if (IsEligible(level))
            {
                eligibleOut.Add(level);
                candidateIndicesOut.Add(i);
            }
        }
    }
}

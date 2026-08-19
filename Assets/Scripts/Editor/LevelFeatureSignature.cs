using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Editor-only feature vector for diversity ranking (not stored on LevelData).
/// </summary>
public struct LevelFeatureSignature
{
    public int gridWidth;
    public int gridHeight;
    public int vehicleCount;
    public int horizontalCount;
    public int verticalCount;
    public int length2Count;
    public int length3PlusCount;
    public int targetStartX;
    public int targetStartY;
    public int exitRow;
    public int targetDistanceToExit;
    public int corridorBlockers;
    public int minimumMoves;
    public int difficultyScore;
    public int solutionLength;
    public int distinctVehiclesMoved;
    public int targetMovesInSolution;
    public bool valid;

    public int GridArea => gridWidth * gridHeight;

    public static LevelFeatureSignature Extract(LevelData level)
    {
        LevelFeatureSignature s = new LevelFeatureSignature
        {
            valid = false,
            targetStartX = -1,
            targetStartY = -1,
            targetDistanceToExit = -1,
            targetMovesInSolution = -1
        };

        if (level == null)
        {
            return s;
        }

        s.gridWidth = level.ResolvedGridWidth;
        s.gridHeight = level.ResolvedGridHeight;
        s.exitRow = level.exitRow;
        s.minimumMoves = level.minimumMoves;
        s.difficultyScore = level.difficultyScore;
        s.solutionLength = level.minimumMoves > 0 ? level.minimumMoves : 0;
        s.distinctVehiclesMoved = Mathf.Max(0, level.uniqueVehiclesInSolution);

        if (level.vehicles == null || level.vehicles.Count == 0)
        {
            return s;
        }

        s.vehicleCount = level.vehicles.Count;
        VehicleData target = null;

        for (int i = 0; i < level.vehicles.Count; i++)
        {
            VehicleData v = level.vehicles[i];
            if (v == null)
            {
                continue;
            }

            bool horizontal =
                v.orientation == VehicleController.VehicleOrientation.Horizontal;
            if (horizontal)
            {
                s.horizontalCount++;
            }
            else
            {
                s.verticalCount++;
            }

            if (v.lengthInCells <= 2)
            {
                s.length2Count++;
            }
            else
            {
                s.length3PlusCount++;
            }

            if (v.canExitRight && target == null)
            {
                target = v;
            }
        }

        if (target != null)
        {
            s.targetStartX = target.gridPosition.x;
            s.targetStartY = target.gridPosition.y;
            int rightMost = target.gridPosition.x + Mathf.Max(1, target.lengthInCells) - 1;
            s.targetDistanceToExit = Mathf.Max(0, s.gridWidth - 1 - rightMost);
            s.corridorBlockers = CountCorridorBlockers(level, target, s.gridWidth);
        }

        s.valid = s.minimumMoves > 0 && s.difficultyScore >= 0 && s.vehicleCount > 0;
        return s;
    }

    private static int CountCorridorBlockers(
        LevelData level,
        VehicleData target,
        int gridWidth)
    {
        if (level.vehicles == null || target == null)
        {
            return 0;
        }

        int row = target.gridPosition.y;
        int rightMost = target.gridPosition.x + Mathf.Max(1, target.lengthInCells) - 1;
        int blockers = 0;

        for (int i = 0; i < level.vehicles.Count; i++)
        {
            VehicleData v = level.vehicles[i];
            if (v == null || v == target || v.canExitRight)
            {
                continue;
            }

            bool horizontal =
                v.orientation == VehicleController.VehicleOrientation.Horizontal;
            int len = Mathf.Max(1, v.lengthInCells);

            if (horizontal)
            {
                if (v.gridPosition.y != row)
                {
                    continue;
                }

                int vx0 = v.gridPosition.x;
                int vx1 = vx0 + len - 1;
                if (vx1 > rightMost && vx0 < gridWidth)
                {
                    blockers++;
                }
            }
            else
            {
                int x = v.gridPosition.x;
                if (x <= rightMost)
                {
                    continue;
                }

                int y0 = v.gridPosition.y;
                int y1 = y0 + len - 1;
                if (row >= y0 && row <= y1)
                {
                    blockers++;
                }
            }
        }

        return blockers;
    }
}

/// <summary>
/// Weighted similarity in [0,1]. 1 = near-identical feature profile.
/// </summary>
public static class LevelDiversityMetrics
{
    public static float Similarity(LevelFeatureSignature a, LevelFeatureSignature b)
    {
        if (!a.valid || !b.valid)
        {
            return 0f;
        }

        float sum = 0f;
        float weightSum = 0f;

        Accumulate(ref sum, ref weightSum, 1.2f, NormDiff(a.gridWidth, b.gridWidth, 5f, 8f));
        Accumulate(ref sum, ref weightSum, 1.2f, NormDiff(a.gridHeight, b.gridHeight, 5f, 8f));
        Accumulate(ref sum, ref weightSum, 1.4f, NormDiff(a.vehicleCount, b.vehicleCount, 2f, 20f));
        Accumulate(
            ref sum,
            ref weightSum,
            1.0f,
            NormDiff(a.horizontalCount, b.horizontalCount, 0f, 16f)
        );
        Accumulate(
            ref sum,
            ref weightSum,
            1.0f,
            NormDiff(a.verticalCount, b.verticalCount, 0f, 16f)
        );
        Accumulate(ref sum, ref weightSum, 0.8f, NormDiff(a.length2Count, b.length2Count, 0f, 16f));
        Accumulate(
            ref sum,
            ref weightSum,
            0.8f,
            NormDiff(a.length3PlusCount, b.length3PlusCount, 0f, 12f)
        );
        Accumulate(ref sum, ref weightSum, 1.5f, NormDiff(a.minimumMoves, b.minimumMoves, 1f, 40f));
        Accumulate(
            ref sum,
            ref weightSum,
            1.5f,
            NormDiff(a.difficultyScore, b.difficultyScore, 0f, 5000f)
        );
        Accumulate(
            ref sum,
            ref weightSum,
            1.2f,
            NormDiff(a.targetDistanceToExit, b.targetDistanceToExit, 0f, 8f)
        );
        Accumulate(
            ref sum,
            ref weightSum,
            1.3f,
            NormDiff(a.corridorBlockers, b.corridorBlockers, 0f, 8f)
        );
        Accumulate(
            ref sum,
            ref weightSum,
            1.1f,
            NormDiff(a.distinctVehiclesMoved, b.distinctVehiclesMoved, 0f, 16f)
        );
        Accumulate(ref sum, ref weightSum, 0.7f, NormDiff(a.exitRow, b.exitRow, 0f, 8f));
        Accumulate(ref sum, ref weightSum, 0.6f, NormDiff(a.targetStartX, b.targetStartX, 0f, 8f));
        Accumulate(ref sum, ref weightSum, 0.6f, NormDiff(a.targetStartY, b.targetStartY, 0f, 8f));

        if (weightSum <= 0f)
        {
            return 0f;
        }

        // sum is weighted average of diffs (0=same). Similarity = 1 - avgDiff.
        float avgDiff = sum / weightSum;
        return Mathf.Clamp01(1f - avgDiff);
    }

    public static float Distance(LevelFeatureSignature a, LevelFeatureSignature b)
    {
        return 1f - Similarity(a, b);
    }

    private static void Accumulate(ref float sum, ref float weightSum, float weight, float diff)
    {
        sum += weight * diff;
        weightSum += weight;
    }

    private static float NormDiff(int a, int b, float min, float max)
    {
        float range = Mathf.Max(0.0001f, max - min);
        return Mathf.Clamp01(Mathf.Abs(a - b) / range);
    }
}

using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Editor-only V1 release content targets (150 levels: 50/50/50).
/// Grid size is a content-planning target — NOT automatic difficulty.
/// </summary>
public static class V1ReleaseContentPlan
{
    public const int MinDimension = 5;
    public const int MaxDimension = 8;
    public const int MaxAspectDelta = 1;
    public const int TargetEasy = 50;
    public const int TargetMedium = 50;
    public const int TargetHard = 50;
    public const int TargetTotal = 150;
    public const int DefaultCandidateMultiplier = 3;

    /// <summary>
    /// One distribution group. Orientations length 1 = square; 2 = mixed pair.
    /// Targets are content-planning counts (later assigned in Level Content Planner).
    /// </summary>
    public struct DistributionGroup
    {
        public string label;
        public Vector2Int[] orientations;
        public int easyTarget;
        public int mediumTarget;
        public int hardTarget;
    }

    /// <summary>
    /// One concrete generation job (single WxH + planning bucket + candidate count).
    /// </summary>
    public struct GenerationJob
    {
        public int width;
        public int height;
        public LevelDifficulty planningBucket;
        public int candidateCount;
        public string groupLabel;
    }

    public static readonly DistributionGroup[] Groups =
    {
        new DistributionGroup
        {
            label = "5x5",
            orientations = new[] { new Vector2Int(5, 5) },
            easyTarget = 12,
            mediumTarget = 3,
            hardTarget = 0
        },
        new DistributionGroup
        {
            label = "5x6 / 6x5",
            orientations = new[] { new Vector2Int(5, 6), new Vector2Int(6, 5) },
            easyTarget = 12,
            mediumTarget = 7,
            hardTarget = 2
        },
        new DistributionGroup
        {
            label = "6x6",
            orientations = new[] { new Vector2Int(6, 6) },
            easyTarget = 12,
            mediumTarget = 10,
            hardTarget = 5
        },
        new DistributionGroup
        {
            label = "6x7 / 7x6",
            orientations = new[] { new Vector2Int(6, 7), new Vector2Int(7, 6) },
            easyTarget = 8,
            mediumTarget = 10,
            hardTarget = 8
        },
        new DistributionGroup
        {
            label = "7x7",
            orientations = new[] { new Vector2Int(7, 7) },
            easyTarget = 4,
            mediumTarget = 10,
            hardTarget = 10
        },
        new DistributionGroup
        {
            label = "7x8 / 8x7",
            orientations = new[] { new Vector2Int(7, 8), new Vector2Int(8, 7) },
            easyTarget = 2,
            mediumTarget = 7,
            hardTarget = 12
        },
        new DistributionGroup
        {
            label = "8x8",
            orientations = new[] { new Vector2Int(8, 8) },
            easyTarget = 0,
            mediumTarget = 3,
            hardTarget = 13
        }
    };

    /// <summary>
    /// Exact WxH keys used in planner distribution reports (canonical order).
    /// </summary>
    public static readonly Vector2Int[] ExactGrids =
    {
        new Vector2Int(5, 5),
        new Vector2Int(5, 6),
        new Vector2Int(6, 5),
        new Vector2Int(6, 6),
        new Vector2Int(6, 7),
        new Vector2Int(7, 6),
        new Vector2Int(7, 7),
        new Vector2Int(7, 8),
        new Vector2Int(8, 7),
        new Vector2Int(8, 8)
    };

    public static bool IsAllowedDimension(int width, int height)
    {
        if (width < MinDimension || width > MaxDimension)
        {
            return false;
        }

        if (height < MinDimension || height > MaxDimension)
        {
            return false;
        }

        return Mathf.Abs(width - height) <= MaxAspectDelta;
    }

    /// <summary>
    /// Split odd totals with difference at most 1 (A gets the extra when odd).
    /// </summary>
    public static void SplitOrientationTargets(int total, out int countA, out int countB)
    {
        if (total <= 0)
        {
            countA = 0;
            countB = 0;
            return;
        }

        countA = (total + 1) / 2;
        countB = total / 2;
    }

    public static int GetTarget(LevelDifficulty difficulty, int width, int height)
    {
        for (int g = 0; g < Groups.Length; g++)
        {
            DistributionGroup group = Groups[g];
            int groupTarget = GetGroupTarget(group, difficulty);
            if (groupTarget <= 0)
            {
                continue;
            }

            if (group.orientations == null || group.orientations.Length == 0)
            {
                continue;
            }

            if (group.orientations.Length == 1)
            {
                if (group.orientations[0].x == width && group.orientations[0].y == height)
                {
                    return groupTarget;
                }

                continue;
            }

            // Mixed pair: split targets across orientations.
            bool isA = group.orientations[0].x == width && group.orientations[0].y == height;
            bool isB = group.orientations[1].x == width && group.orientations[1].y == height;
            if (!isA && !isB)
            {
                continue;
            }

            SplitOrientationTargets(groupTarget, out int countA, out int countB);
            return isA ? countA : countB;
        }

        return 0;
    }

    public static int GetGroupedTarget(string groupLabel, LevelDifficulty difficulty)
    {
        for (int i = 0; i < Groups.Length; i++)
        {
            if (Groups[i].label == groupLabel)
            {
                return GetGroupTarget(Groups[i], difficulty);
            }
        }

        return 0;
    }

    private static int GetGroupTarget(DistributionGroup group, LevelDifficulty difficulty)
    {
        switch (difficulty)
        {
            case LevelDifficulty.Easy:
                return group.easyTarget;
            case LevelDifficulty.Medium:
                return group.mediumTarget;
            case LevelDifficulty.Hard:
                return group.hardTarget;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Expand the V1 table into concrete generation jobs with oversampling + orientation balance.
    /// Does not touch MainLevelDatabase.
    /// </summary>
    public static List<GenerationJob> BuildGenerationJobs(int candidateMultiplier)
    {
        int multiplier = Mathf.Max(1, candidateMultiplier);
        List<GenerationJob> jobs = new List<GenerationJob>();

        for (int g = 0; g < Groups.Length; g++)
        {
            DistributionGroup group = Groups[g];
            AppendJobsForBucket(jobs, group, LevelDifficulty.Easy, group.easyTarget, multiplier);
            AppendJobsForBucket(jobs, group, LevelDifficulty.Medium, group.mediumTarget, multiplier);
            AppendJobsForBucket(jobs, group, LevelDifficulty.Hard, group.hardTarget, multiplier);
        }

        return jobs;
    }

    private static void AppendJobsForBucket(
        List<GenerationJob> jobs,
        DistributionGroup group,
        LevelDifficulty bucket,
        int targetCount,
        int multiplier)
    {
        if (targetCount <= 0 || group.orientations == null || group.orientations.Length == 0)
        {
            return;
        }

        if (group.orientations.Length == 1)
        {
            Vector2Int size = group.orientations[0];
            jobs.Add(new GenerationJob
            {
                width = size.x,
                height = size.y,
                planningBucket = bucket,
                candidateCount = targetCount * multiplier,
                groupLabel = group.label
            });
            return;
        }

        SplitOrientationTargets(targetCount, out int countA, out int countB);
        Vector2Int a = group.orientations[0];
        Vector2Int b = group.orientations[1];

        if (countA > 0)
        {
            jobs.Add(new GenerationJob
            {
                width = a.x,
                height = a.y,
                planningBucket = bucket,
                candidateCount = countA * multiplier,
                groupLabel = group.label
            });
        }

        if (countB > 0)
        {
            jobs.Add(new GenerationJob
            {
                width = b.x,
                height = b.y,
                planningBucket = bucket,
                candidateCount = countB * multiplier,
                groupLabel = group.label
            });
        }
    }

    public static string FormatDistributionTable()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("V1 TARGET DISTRIBUTION (content planning — not auto difficulty)");
        sb.AppendLine("GRID             EASY   MEDIUM   HARD");
        for (int i = 0; i < Groups.Length; i++)
        {
            DistributionGroup g = Groups[i];
            sb.AppendLine(
                g.label.PadRight(16) +
                g.easyTarget.ToString().PadLeft(5) +
                g.mediumTarget.ToString().PadLeft(8) +
                g.hardTarget.ToString().PadLeft(7)
            );
        }

        sb.AppendLine(
            "TOTAL".PadRight(16) +
            TargetEasy.ToString().PadLeft(5) +
            TargetMedium.ToString().PadLeft(8) +
            TargetHard.ToString().PadLeft(7)
        );
        return sb.ToString();
    }

    public static int SumTargetCandidates(int candidateMultiplier)
    {
        List<GenerationJob> jobs = BuildGenerationJobs(candidateMultiplier);
        int sum = 0;
        for (int i = 0; i < jobs.Count; i++)
        {
            sum += jobs[i].candidateCount;
        }

        return sum;
    }
}

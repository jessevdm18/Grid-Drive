using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Authored Daily Challenge settings: level pool, score V1 tuning, Resources path.
/// </summary>
[CreateAssetMenu(
    fileName = "DailyChallengeConfig",
    menuName = "RushOut/Daily Challenge Config")]
public class DailyChallengeConfig : ScriptableObject
{
    public const string ResourcesPath = "DailyChallengeConfig";

    [Tooltip("Authored solvable LevelData pool. Same UTC day always picks the same entry.")]
    [SerializeField] private List<LevelData> dailyLevelPool = new List<LevelData>();

    [Header("Score V1 — Move efficiency (primary)")]
    [SerializeField] private int baseMoveScore = 100000;
    [SerializeField] private int movePenaltyPerExtraMove = 2500;
    [SerializeField] private int minimumMoveScore = 10000;

    [Header("Score V1 — Time bonus (secondary, capped)")]
    [SerializeField] private int timeBonusMax = 20000;
    [SerializeField] private float timeReferenceSeconds = 60f;

    public IReadOnlyList<LevelData> DailyLevelPool => dailyLevelPool;

    public int PoolCount => dailyLevelPool != null ? dailyLevelPool.Count : 0;

    public int BaseMoveScore => baseMoveScore;
    public int MovePenaltyPerExtraMove => movePenaltyPerExtraMove;
    public int MinimumMoveScore => minimumMoveScore;
    public int TimeBonusMax => timeBonusMax;
    public float TimeReferenceSeconds => timeReferenceSeconds;

    public LevelData GetLevelAt(int index)
    {
        if (dailyLevelPool == null || dailyLevelPool.Count == 0)
        {
            return null;
        }

        if (index < 0 || index >= dailyLevelPool.Count)
        {
            return null;
        }

        return dailyLevelPool[index];
    }

    public int GetStableIndexForDay(string utcDayId)
    {
        if (PoolCount <= 0)
        {
            return -1;
        }

        uint hash = DailyChallengeHash.StableHash32(utcDayId ?? string.Empty);
        return (int)(hash % (uint)PoolCount);
    }

    public LevelData SelectLevelForDay(string utcDayId)
    {
        int index = GetStableIndexForDay(utcDayId);
        return index >= 0 ? GetLevelAt(index) : null;
    }

    public static DailyChallengeConfig LoadDefault()
    {
        return Resources.Load<DailyChallengeConfig>(ResourcesPath);
    }
}

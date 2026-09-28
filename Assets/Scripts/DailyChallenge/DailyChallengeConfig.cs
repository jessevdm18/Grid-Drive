using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Authored Daily Challenge settings: candidate level pool, score V1 tuning.
/// Selection always filters through <see cref="DailyChallengeLevelEligibility"/>.
/// </summary>
[CreateAssetMenu(
    fileName = "DailyChallengeConfig",
    menuName = "RushOut/Daily Challenge Config")]
public class DailyChallengeConfig : ScriptableObject
{
    public const string ResourcesPath = "DailyChallengeConfig";

    [Tooltip("Candidate LevelData pool. Special/objective levels are filtered out at selection.")]
    [SerializeField] private List<LevelData> dailyLevelPool = new List<LevelData>();

    [Header("Score V1 — Move efficiency (primary)")]
    [SerializeField] private int baseMoveScore = 100000;
    [SerializeField] private int movePenaltyPerExtraMove = 2500;
    [SerializeField] private int minimumMoveScore = 10000;

    [Header("Score V1 — Time bonus (secondary, capped)")]
    [SerializeField] private int timeBonusMax = 20000;
    [SerializeField] private float timeReferenceSeconds = 60f;

    [Header("Leaderboard — Simulated (Phase 3)")]
    [Tooltip("Number of deterministic placeholder players per Daily day.")]
    [SerializeField] private int simulatedLeaderboardPlayerCount = 30;

    [Tooltip("DEV/Editor: show SIM markers on simulated rows. Production presentation hides them.")]
    [SerializeField] private bool showSimulatedLeaderboardMarkers = false;

    [Tooltip("Fill leaderboard with simulated rows until this many total entries.")]
    [SerializeField] private int minimumLeaderboardPopulation = 30;

    [Header("Backend (Phase 4.1)")]
    [Tooltip("SimulatedLocal = PlayerPrefs authority. Online = Firebase REST authority.")]
    [SerializeField] private DailyChallengeBackendMode backendMode =
        DailyChallengeBackendMode.SimulatedLocal;

    public IReadOnlyList<LevelData> DailyLevelPool => dailyLevelPool;

    public int PoolCount => dailyLevelPool != null ? dailyLevelPool.Count : 0;

    public int BaseMoveScore => baseMoveScore;
    public int MovePenaltyPerExtraMove => movePenaltyPerExtraMove;
    public int MinimumMoveScore => minimumMoveScore;
    public int TimeBonusMax => timeBonusMax;
    public float TimeReferenceSeconds => timeReferenceSeconds;
    public int SimulatedLeaderboardPlayerCount => simulatedLeaderboardPlayerCount;
    public bool ShowSimulatedLeaderboardMarkers => showSimulatedLeaderboardMarkers;
    public int MinimumLeaderboardPopulation => minimumLeaderboardPopulation;
    public DailyChallengeBackendMode BackendMode => backendMode;

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

    public int CountEligibleLevels()
    {
        if (dailyLevelPool == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < dailyLevelPool.Count; i++)
        {
            if (DailyChallengeLevelEligibility.IsEligible(dailyLevelPool[i]))
            {
                count++;
            }
        }

        return count;
    }

    public bool HasEligibleLevels => CountEligibleLevels() > 0;

    /// <summary>
    /// Deterministic Daily selection from eligible Classic levels only.
    /// Returns false when no eligible levels exist.
    /// candidatePoolIndex is the index in the authored candidate list.
    /// </summary>
    public bool TrySelectEligibleLevelForDay(
        string utcDayId,
        out LevelData level,
        out int candidatePoolIndex)
    {
        level = null;
        candidatePoolIndex = -1;

        List<LevelData> eligible = new List<LevelData>();
        List<int> indices = new List<int>();
        DailyChallengeLevelEligibility.CollectEligible(dailyLevelPool, eligible, indices);

        if (eligible.Count == 0)
        {
            return false;
        }

        uint hash = DailyChallengeHash.StableHash32(utcDayId ?? string.Empty);
        int eligibleIndex = (int)(hash % (uint)eligible.Count);
        level = eligible[eligibleIndex];
        candidatePoolIndex = indices[eligibleIndex];
        return level != null;
    }

    /// <summary>Legacy helper — prefers eligible selection; may return null.</summary>
    public LevelData SelectLevelForDay(string utcDayId)
    {
        TrySelectEligibleLevelForDay(utcDayId, out LevelData level, out _);
        return level;
    }

    public static DailyChallengeConfig LoadDefault()
    {
        return Resources.Load<DailyChallengeConfig>(ResourcesPath);
    }
}

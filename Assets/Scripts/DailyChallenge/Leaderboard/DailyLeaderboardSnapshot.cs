using System;
using System.Collections.Generic;

/// <summary>
/// Immutable-style snapshot consumed by UI. Provider builds once per day/level/result key.
/// </summary>
[Serializable]
public class DailyLeaderboardSnapshot
{
    public string DayId;
    public string LevelAssetName;
    public int MinimumMoves;
    public List<DailyLeaderboardEntry> Entries = new List<DailyLeaderboardEntry>();
    public bool HasCurrentPlayer;
    public int CurrentPlayerRank;
    public DailyLeaderboardEntry CurrentPlayerEntry;
    public string StatusMessage;
}

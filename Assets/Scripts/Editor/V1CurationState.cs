using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum V1CurationStatus
{
    Unassigned = 0,
    AutoSelectedFinal = 1,
    Reserve = 2,
    Rejected = 3,
    ManualKeep = 4,
    Maybe = 5
}

public enum V1RejectReason
{
    None = 0,
    TooEasy = 1,
    TooHard = 2,
    Trivial = 3,
    Confusing = 4,
    TooSimilar = 5,
    BadLayout = 6,
    Other = 7
}

[Serializable]
public class V1CurationEntry
{
    public LevelData level;
    public string assetGuid;
    public V1CurationStatus status = V1CurationStatus.Unassigned;
    public LevelDifficulty assignedDifficulty = LevelDifficulty.Easy;
    public float baseQuality;
    public float difficultyFit;
    public float trivialityPenalty;
    public float nearDuplicatePenalty;
    public float effectiveScore;
    public bool needsReview;
    public string reviewReason = string.Empty;
    public string suggestedDifficulty = string.Empty;

    [Tooltip("Editor-only playtest note.")]
    public string playtestNote = string.Empty;

    public V1RejectReason rejectReason = V1RejectReason.None;
}

/// <summary>
/// Editor-only curation metadata. Never touches PlayerPrefs / save identity.
/// </summary>
[CreateAssetMenu(
    fileName = "V1CurationState",
    menuName = "RushOut/V1 Curation State"
)]
public class V1CurationState : ScriptableObject
{
    public const string DefaultAssetPath = "Assets/Data/Editor/V1CurationState.asset";
    public const int DefaultReservePerDifficulty = 10;
    public const float HighSimilarityThreshold = 0.82f;

    public List<V1CurationEntry> entries = new List<V1CurationEntry>();
    public string lastCurateUtc = string.Empty;
    public string lastReport = string.Empty;

    public static V1CurationState LoadOrCreate()
    {
        V1CurationState state = AssetDatabase.LoadAssetAtPath<V1CurationState>(DefaultAssetPath);
        if (state != null)
        {
            return state;
        }

        string folder = "Assets/Data/Editor";
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
        {
            AssetDatabase.CreateFolder("Assets", "Data");
        }

        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Editor");
        }

        state = CreateInstance<V1CurationState>();
        AssetDatabase.CreateAsset(state, DefaultAssetPath);
        AssetDatabase.SaveAssets();
        return state;
    }

    public V1CurationEntry FindByLevel(LevelData level)
    {
        if (level == null)
        {
            return null;
        }

        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(level));
        for (int i = 0; i < entries.Count; i++)
        {
            V1CurationEntry e = entries[i];
            if (e == null)
            {
                continue;
            }

            if (e.level == level)
            {
                return e;
            }

            if (!string.IsNullOrEmpty(guid) && e.assetGuid == guid)
            {
                return e;
            }
        }

        return null;
    }

    public int CountStatus(V1CurationStatus status)
    {
        int count = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i] != null && entries[i].status == status)
            {
                count++;
            }
        }

        return count;
    }

    public int CountFinalDifficulty(LevelDifficulty difficulty)
    {
        int count = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            V1CurationEntry e = entries[i];
            if (e == null)
            {
                continue;
            }

            if (IsFinalStatus(e.status) && e.assignedDifficulty == difficulty)
            {
                count++;
            }
        }

        return count;
    }

    public List<V1CurationEntry> GetFinalEntries()
    {
        List<V1CurationEntry> result = new List<V1CurationEntry>();
        for (int i = 0; i < entries.Count; i++)
        {
            V1CurationEntry e = entries[i];
            if (e == null || e.level == null)
            {
                continue;
            }

            if (IsFinalStatus(e.status))
            {
                result.Add(e);
            }
        }

        return result;
    }

    public static bool IsFinalStatus(V1CurationStatus status)
    {
        return status == V1CurationStatus.AutoSelectedFinal
            || status == V1CurationStatus.ManualKeep
            || status == V1CurationStatus.Maybe;
    }

    public static bool IsReviewedStatus(V1CurationStatus status)
    {
        return status == V1CurationStatus.ManualKeep
            || status == V1CurationStatus.Maybe
            || status == V1CurationStatus.Rejected;
    }

    public static bool IsUnreviewedFinal(V1CurationStatus status)
    {
        return status == V1CurationStatus.AutoSelectedFinal;
    }

    public int CountReviewedFinals()
    {
        int count = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            V1CurationEntry e = entries[i];
            if (e == null)
            {
                continue;
            }

            // Keep/Maybe still in final pool; Rejected may have left the final set.
            if (e.status == V1CurationStatus.ManualKeep ||
                e.status == V1CurationStatus.Maybe ||
                e.status == V1CurationStatus.Rejected)
            {
                count++;
            }
        }

        return count;
    }

    public int CountUnreviewedFinals()
    {
        return CountStatus(V1CurationStatus.AutoSelectedFinal);
    }

    public int CountReviewedForDifficulty(LevelDifficulty difficulty)
    {
        int count = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            V1CurationEntry e = entries[i];
            if (e == null || e.assignedDifficulty != difficulty)
            {
                continue;
            }

            if (IsReviewedStatus(e.status))
            {
                count++;
            }
        }

        return count;
    }

    public void MarkDirtyAndSave()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
}

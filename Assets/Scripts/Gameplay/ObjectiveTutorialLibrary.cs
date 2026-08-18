using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Eén presentation-entry per objectiveType voor first-time tutorials.
/// </summary>
[Serializable]
public class ObjectiveTutorialEntry
{
    public LevelObjectiveType objectiveType = LevelObjectiveType.Classic;

    public string title = "OBJECTIVE";

    [TextArea(2, 4)]
    public string instruction = string.Empty;

    public Sprite primarySprite;
    public Sprite secondarySprite;
    public Sprite modifierSprite;

    public bool showSecondary;
    public bool showModifier;

    public string modifierText = string.Empty;
}

/// <summary>
/// Library met tutorial-teksten/sprites per objective. Geen Resources.Load.
/// </summary>
[CreateAssetMenu(
    fileName = "ObjectiveTutorialLibrary",
    menuName = "RushOut/Objective Tutorial Library"
)]
public class ObjectiveTutorialLibrary : ScriptableObject
{
    [SerializeField] private List<ObjectiveTutorialEntry> entries = new List<ObjectiveTutorialEntry>();

    public IReadOnlyList<ObjectiveTutorialEntry> Entries => entries;

    public ObjectiveTutorialEntry GetEntry(LevelObjectiveType objectiveType)
    {
        if (entries == null)
        {
            return null;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            ObjectiveTutorialEntry entry = entries[i];
            if (entry != null && entry.objectiveType == objectiveType)
            {
                return entry;
            }
        }

        return null;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Vult ontbrekende default-teksten (Editor). Overschrijft bestaande entries niet.
    /// </summary>
    [ContextMenu("Fill Missing Default Entries")]
    private void FillMissingDefaultEntries()
    {
        EnsureDefault(LevelObjectiveType.Classic, "ESCAPE",
            "Move the target car to the exit.");
        EnsureDefault(LevelObjectiveType.TimedAmbulance, "AMBULANCE RESCUE",
            "Get the ambulance to the exit before time runs out.");
        EnsureDefault(LevelObjectiveType.MoveLimit, "MOVE LIMIT",
            "Reach the exit before you run out of moves.");
        EnsureDefault(LevelObjectiveType.MultiTargetRescue, "RESCUE",
            "Get every rescue vehicle to the exit.");
        EnsureDefault(LevelObjectiveType.NoTouchChallenge, "DON'T MOVE",
            "Reach the exit without moving the protected vehicle.");
        EnsureDefault(LevelObjectiveType.FragileCargo, "FRAGILE CARGO",
            "Get the cargo vehicle out before using all of its moves.");
        EnsureDefault(LevelObjectiveType.LimitedVehicle, "LIMITED VEHICLE",
            "This vehicle can only move a limited number of times.");

        UnityEditor.EditorUtility.SetDirty(this);
    }

    private void EnsureDefault(LevelObjectiveType type, string title, string instruction)
    {
        if (GetEntry(type) != null)
        {
            return;
        }

        if (entries == null)
        {
            entries = new List<ObjectiveTutorialEntry>();
        }

        entries.Add(new ObjectiveTutorialEntry
        {
            objectiveType = type,
            title = title,
            instruction = instruction
        });
    }
#endif
}

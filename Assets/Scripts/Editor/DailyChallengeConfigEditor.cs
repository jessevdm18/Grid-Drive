#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector QoL for DailyChallengeConfig — warns when special levels are in the pool.
/// Does not auto-delete authored references.
/// </summary>
[CustomEditor(typeof(DailyChallengeConfig))]
public class DailyChallengeConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DailyChallengeConfig config = (DailyChallengeConfig)target;
        if (config == null)
        {
            return;
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Daily Pool Eligibility", EditorStyles.boldLabel);

        int total = config.PoolCount;
        int eligible = config.CountEligibleLevels();
        int special = 0;
        int invalid = 0;

        for (int i = 0; i < total; i++)
        {
            LevelData level = config.GetLevelAt(i);
            if (DailyChallengeLevelEligibility.IsEligible(level))
            {
                continue;
            }

            if (level != null && level.objectiveType != LevelObjectiveType.Classic)
            {
                special++;
            }
            else
            {
                invalid++;
            }
        }

        if (eligible <= 0)
        {
            EditorGUILayout.HelpBox(
                "No eligible Classic levels. Daily Challenge cannot start. " +
                "Add normal/standard puzzles (objectiveType = Classic, minimumMoves > 0).",
                MessageType.Error);
        }
        else if (special > 0 || invalid > 0)
        {
            EditorGUILayout.HelpBox(
                eligible + "/" + total + " eligible Classic levels. " +
                special + " special/objective and " + invalid +
                " invalid entries will be ignored at selection time.",
                MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox(
                eligible + "/" + total + " eligible Classic levels. Pool looks good.",
                MessageType.Info);
        }

        if (GUILayout.Button("Validate Daily Level Pool (Console)"))
        {
            DailyChallengeTestingMenu.ValidateDailyLevelPool();
        }
    }
}
#endif

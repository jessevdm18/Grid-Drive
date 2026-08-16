using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only: Special Mission progression op LevelData in MainLevelDatabase.
/// Runtime leest alleen LevelData.objectiveType — geen modulo in gameplay.
/// Balancing-parameters via EditorPrefs (niet PlayerPrefs).
/// </summary>
public static class SpecialMissionProgressionUtility
{
    private const string DatabaseAssetFilter = "MainLevelDatabase t:LevelDatabase";
    private const string PrefsPrefix = "GridDrive.SpecialMission.";

    public const float AmbulanceFallbackSeconds = 40f;
    public const int MoveLimitFallbackMoves = 12;

    /// <summary>
    /// Editor-only balancing. Defaults match de oorspronkelijke progression.
    /// </summary>
    public struct Settings
    {
        public float ambulanceSecondsPerMoveStart;
        public float ambulanceSecondsPerMoveReduction;
        public float ambulanceSecondsPerMoveMin;
        public float ambulanceBufferStart;
        public float ambulanceBufferReduction;
        public float ambulanceBufferMin;
        public float ambulanceTimeLimitMin;

        public int moveLimitExtraStart;
        public int moveLimitExtraReduction;
        public int moveLimitExtraMin;

        public static Settings CreateDefaults()
        {
            return new Settings
            {
                ambulanceSecondsPerMoveStart = 3.0f,
                ambulanceSecondsPerMoveReduction = 0.15f,
                ambulanceSecondsPerMoveMin = 2.1f,
                ambulanceBufferStart = 15f,
                ambulanceBufferReduction = 1f,
                ambulanceBufferMin = 9f,
                ambulanceTimeLimitMin = 20f,
                moveLimitExtraStart = 5,
                moveLimitExtraReduction = 1,
                moveLimitExtraMin = 2
            };
        }

        public static Settings LoadFromEditorPrefs()
        {
            Settings defaults = CreateDefaults();
            Settings loaded = defaults;

            loaded.ambulanceSecondsPerMoveStart = EditorPrefs.GetFloat(
                PrefsPrefix + "AmbSecPerMoveStart",
                defaults.ambulanceSecondsPerMoveStart
            );
            loaded.ambulanceSecondsPerMoveReduction = EditorPrefs.GetFloat(
                PrefsPrefix + "AmbSecPerMoveReduction",
                defaults.ambulanceSecondsPerMoveReduction
            );
            loaded.ambulanceSecondsPerMoveMin = EditorPrefs.GetFloat(
                PrefsPrefix + "AmbSecPerMoveMin",
                defaults.ambulanceSecondsPerMoveMin
            );
            loaded.ambulanceBufferStart = EditorPrefs.GetFloat(
                PrefsPrefix + "AmbBufferStart",
                defaults.ambulanceBufferStart
            );
            loaded.ambulanceBufferReduction = EditorPrefs.GetFloat(
                PrefsPrefix + "AmbBufferReduction",
                defaults.ambulanceBufferReduction
            );
            loaded.ambulanceBufferMin = EditorPrefs.GetFloat(
                PrefsPrefix + "AmbBufferMin",
                defaults.ambulanceBufferMin
            );
            loaded.ambulanceTimeLimitMin = EditorPrefs.GetFloat(
                PrefsPrefix + "AmbTimeLimitMin",
                defaults.ambulanceTimeLimitMin
            );
            loaded.moveLimitExtraStart = EditorPrefs.GetInt(
                PrefsPrefix + "MlExtraStart",
                defaults.moveLimitExtraStart
            );
            loaded.moveLimitExtraReduction = EditorPrefs.GetInt(
                PrefsPrefix + "MlExtraReduction",
                defaults.moveLimitExtraReduction
            );
            loaded.moveLimitExtraMin = EditorPrefs.GetInt(
                PrefsPrefix + "MlExtraMin",
                defaults.moveLimitExtraMin
            );

            return loaded;
        }

        public void SaveToEditorPrefs()
        {
            EditorPrefs.SetFloat(PrefsPrefix + "AmbSecPerMoveStart", ambulanceSecondsPerMoveStart);
            EditorPrefs.SetFloat(
                PrefsPrefix + "AmbSecPerMoveReduction",
                ambulanceSecondsPerMoveReduction
            );
            EditorPrefs.SetFloat(PrefsPrefix + "AmbSecPerMoveMin", ambulanceSecondsPerMoveMin);
            EditorPrefs.SetFloat(PrefsPrefix + "AmbBufferStart", ambulanceBufferStart);
            EditorPrefs.SetFloat(PrefsPrefix + "AmbBufferReduction", ambulanceBufferReduction);
            EditorPrefs.SetFloat(PrefsPrefix + "AmbBufferMin", ambulanceBufferMin);
            EditorPrefs.SetFloat(PrefsPrefix + "AmbTimeLimitMin", ambulanceTimeLimitMin);
            EditorPrefs.SetInt(PrefsPrefix + "MlExtraStart", moveLimitExtraStart);
            EditorPrefs.SetInt(PrefsPrefix + "MlExtraReduction", moveLimitExtraReduction);
            EditorPrefs.SetInt(PrefsPrefix + "MlExtraMin", moveLimitExtraMin);
        }
    }

    public struct AssignmentResult
    {
        public int levelNumber;
        public LevelObjectiveType objectiveType;
        public int minimumMoves;
        public float timeLimitSeconds;
        public int moveLimit;
        public string note;
    }

    /// <summary>
    /// specialIndex = levelNumber / 10. Odd → Ambulance, even → MoveLimit.
    /// Non-multiples of 10 → Classic.
    /// </summary>
    public static LevelObjectiveType ResolveObjectiveType(int levelNumber)
    {
        if (levelNumber <= 0 || levelNumber % 10 != 0)
        {
            return LevelObjectiveType.Classic;
        }

        int specialIndex = levelNumber / 10;
        return (specialIndex % 2 == 1)
            ? LevelObjectiveType.TimedAmbulance
            : LevelObjectiveType.MoveLimit;
    }

    /// <summary>
    /// 0-based index within Ambulance specials only (10→0, 30→1, 50→2).
    /// </summary>
    public static int GetAmbulanceProgressIndex(int levelNumber)
    {
        if (levelNumber <= 0 || levelNumber % 10 != 0)
        {
            return -1;
        }

        int specialIndex = levelNumber / 10;
        if (specialIndex % 2 == 0)
        {
            return -1;
        }

        return (specialIndex - 1) / 2;
    }

    /// <summary>
    /// 0-based index within MoveLimit specials only (20→0, 40→1, 60→2).
    /// </summary>
    public static int GetMoveLimitProgressIndex(int levelNumber)
    {
        if (levelNumber <= 0 || levelNumber % 10 != 0)
        {
            return -1;
        }

        int specialIndex = levelNumber / 10;
        if (specialIndex % 2 == 1)
        {
            return -1;
        }

        return (specialIndex / 2) - 1;
    }

    public static float ComputeAmbulanceTimeLimit(
        int minimumMoves,
        int ambulanceIndex,
        Settings settings)
    {
        if (minimumMoves <= 0)
        {
            return AmbulanceFallbackSeconds;
        }

        int index = Mathf.Max(0, ambulanceIndex);
        float secondsPerMove = Mathf.Max(
            settings.ambulanceSecondsPerMoveMin,
            settings.ambulanceSecondsPerMoveStart -
            settings.ambulanceSecondsPerMoveReduction * index
        );
        float buffer = Mathf.Max(
            settings.ambulanceBufferMin,
            settings.ambulanceBufferStart -
            settings.ambulanceBufferReduction * index
        );

        float timeLimit = minimumMoves * secondsPerMove + buffer;
        return Mathf.Max(settings.ambulanceTimeLimitMin, timeLimit);
    }

    public static int ComputeMoveLimit(
        int minimumMoves,
        int moveLimitIndex,
        Settings settings)
    {
        if (minimumMoves <= 0)
        {
            return MoveLimitFallbackMoves;
        }

        int index = Mathf.Max(0, moveLimitIndex);
        int extra = Mathf.Max(
            settings.moveLimitExtraMin,
            settings.moveLimitExtraStart -
            settings.moveLimitExtraReduction * index
        );
        return minimumMoves + extra;
    }

    public static AssignmentResult BuildAssignment(LevelData level, Settings settings)
    {
        AssignmentResult result = new AssignmentResult
        {
            levelNumber = level != null ? level.levelNumber : 0,
            objectiveType = LevelObjectiveType.Classic,
            minimumMoves = level != null ? level.minimumMoves : 0,
            timeLimitSeconds = 0f,
            moveLimit = 0,
            note = null
        };

        if (level == null)
        {
            result.note = "null level";
            return result;
        }

        LevelObjectiveType type = ResolveObjectiveType(level.levelNumber);
        result.objectiveType = type;

        if (type == LevelObjectiveType.Classic)
        {
            result.note = "Classic reset";
            return result;
        }

        if (type == LevelObjectiveType.TimedAmbulance)
        {
            int ambIndex = GetAmbulanceProgressIndex(level.levelNumber);
            result.timeLimitSeconds = ComputeAmbulanceTimeLimit(
                level.minimumMoves,
                ambIndex,
                settings
            );
            if (level.minimumMoves <= 0)
            {
                result.note = "fallback time (minimumMoves missing)";
            }
            else
            {
                result.note = "ambulanceIndex=" + ambIndex;
            }

            return result;
        }

        int mlIndex = GetMoveLimitProgressIndex(level.levelNumber);
        result.moveLimit = ComputeMoveLimit(level.minimumMoves, mlIndex, settings);
        if (level.minimumMoves <= 0)
        {
            result.note = "fallback moves (minimumMoves missing)";
        }
        else
        {
            result.note = "moveLimitIndex=" + mlIndex;
        }

        return result;
    }

    public static void ApplyToLevel(LevelData level, AssignmentResult assignment)
    {
        if (level == null)
        {
            return;
        }

        level.objectiveType = assignment.objectiveType;
        level.timeLimitSeconds = assignment.timeLimitSeconds;
        level.moveLimit = assignment.moveLimit;
        EditorUtility.SetDirty(level);
    }

    /// <summary>
    /// Preview: special levels in MainLevelDatabase (geen writes).
    /// </summary>
    public static List<AssignmentResult> BuildSpecialPreview(Settings settings)
    {
        List<AssignmentResult> results = new List<AssignmentResult>();
        LevelDatabase database = LoadMainLevelDatabase(logErrors: false);
        if (database == null || database.levels == null)
        {
            return results;
        }

        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level == null)
            {
                continue;
            }

            AssignmentResult assignment = BuildAssignment(level, settings);
            if (assignment.objectiveType == LevelObjectiveType.Classic)
            {
                continue;
            }

            results.Add(assignment);
        }

        results.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));
        return results;
    }

    /// <summary>
    /// Loopt MainLevelDatabase; wijzigt alleen objective-fields.
    /// </summary>
    public static void ApplyToMainLevelDatabase(Settings settings)
    {
        settings.SaveToEditorPrefs();

        LevelDatabase database = LoadMainLevelDatabase(logErrors: true);
        if (database == null)
        {
            return;
        }

        if (database.levels == null || database.levels.Count == 0)
        {
            Debug.LogWarning(
                "SpecialMissionProgression: MainLevelDatabase heeft geen levels."
            );
            return;
        }

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("=== Special Mission Progression Applied ===");

        int classicCount = 0;
        int ambulanceCount = 0;
        int moveLimitCount = 0;
        int skippedNull = 0;

        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level == null)
            {
                skippedNull++;
                continue;
            }

            AssignmentResult assignment = BuildAssignment(level, settings);
            ApplyToLevel(level, assignment);

            switch (assignment.objectiveType)
            {
                case LevelObjectiveType.TimedAmbulance:
                    ambulanceCount++;
                    summary.AppendLine(
                        "Level " + assignment.levelNumber +
                        " → TimedAmbulance | minMoves " + assignment.minimumMoves +
                        " | time " + assignment.timeLimitSeconds.ToString("0.#") + " sec" +
                        " (" + assignment.note + ")"
                    );
                    break;

                case LevelObjectiveType.MoveLimit:
                    moveLimitCount++;
                    summary.AppendLine(
                        "Level " + assignment.levelNumber +
                        " → MoveLimit | minMoves " + assignment.minimumMoves +
                        " | limit " + assignment.moveLimit +
                        " (" + assignment.note + ")"
                    );
                    break;

                default:
                    classicCount++;
                    break;
            }
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();

        summary.AppendLine(
            "Classic=" + classicCount +
            " | TimedAmbulance=" + ambulanceCount +
            " | MoveLimit=" + moveLimitCount +
            " | nullSkipped=" + skippedNull
        );

        Debug.Log(summary.ToString());
    }

    /// <summary>
    /// Menu Apply: gebruikt laatst opgeslagen EditorPrefs-waarden.
    /// </summary>
    public static void ApplyToMainLevelDatabase()
    {
        ApplyToMainLevelDatabase(Settings.LoadFromEditorPrefs());
    }

    public static LevelDatabase LoadMainLevelDatabase(bool logErrors)
    {
        string[] guids = AssetDatabase.FindAssets(DatabaseAssetFilter);
        if (guids == null || guids.Length == 0)
        {
            if (logErrors)
            {
                Debug.LogError("SpecialMissionProgression: geen MainLevelDatabase gevonden.");
            }

            return null;
        }

        if (guids.Length > 1)
        {
            if (logErrors)
            {
                Debug.LogError(
                    "SpecialMissionProgression: meerdere MainLevelDatabase assets (" +
                    guids.Length + "). Verwacht precies één."
                );
            }

            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        LevelDatabase database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);
        if (database == null && logErrors)
        {
            Debug.LogError(
                "SpecialMissionProgression: kon MainLevelDatabase niet laden: " + path
            );
        }

        return database;
    }

    [MenuItem("RushOut/Apply Special Mission Progression")]
    public static void MenuApply()
    {
        if (!EditorUtility.DisplayDialog(
                "Apply Special Mission Progression",
                "Schrijft objectiveType / timeLimitSeconds / moveLimit op alle levels " +
                "in MainLevelDatabase op basis van levelNumber + minimumMoves.\n\n" +
                "Gebruikt de laatst opgeslagen Editor balancing-waarden " +
                "(RushOut → Generate Levels).\n\n" +
                "Handmatige Inspector-overrides op die velden worden overschreven.\n\n" +
                "Doorgaan?",
                "Apply",
                "Cancel"))
        {
            return;
        }

        ApplyToMainLevelDatabase();
    }
}

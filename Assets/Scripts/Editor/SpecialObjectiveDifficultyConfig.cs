using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only central knobs for special-objective difficulty recommendations.
/// Defaults: generous TimedAmbulance (Hard 17 moves → 55s). Stored in EditorPrefs.
/// Runtime gameplay never reads this — only planned LevelData values after Apply.
/// </summary>
public static class SpecialObjectiveDifficultyConfig
{
    private const string PrefsPrefix = "RushOut.SpecialDifficulty.";
    private const string VersionKey = PrefsPrefix + "ConfigVersion";

    /// <summary>
    /// Bump when defaults change so stale EditorPrefs can migrate once.
    /// </summary>
    public const int CurrentConfigVersion = 2;

    public struct Settings
    {
        // TimedAmbulance: recommended = minMoves * secPerMove + buffer, then clamp
        public float easySecondsPerMove;
        public float mediumSecondsPerMove;
        public float hardSecondsPerMove;
        public float easyTimedBuffer;
        public float mediumTimedBuffer;
        public float hardTimedBuffer;
        public float easyTimedMin;
        public float easyTimedMax;
        public float mediumTimedMin;
        public float mediumTimedMax;
        public float hardTimedMin;
        public float hardTimedMax;

        // MoveLimit: recommended = minMoves + margin (floor: minMoves + 2)
        public int easyMoveLimitMargin;
        public int mediumMoveLimitMargin;
        public int hardMoveLimitMargin;

        // FragileCargo: required + margin (never below required)
        public int easyFragileMargin;
        public int mediumFragileMargin;
        public int hardFragileMargin;

        // LimitedVehicle: same margin model as Fragile
        public int easyLimitedMargin;
        public int mediumLimitedMargin;
        public int hardLimitedMargin;

        // Late-local nudge: at end of tier, max this fraction stricter (e.g. 0.05 = 5%)
        public float localProgressNudge;

        public static Settings CreateDefaults()
        {
            return new Settings
            {
                easySecondsPerMove = 3.5f,
                mediumSecondsPerMove = 3.25f,
                hardSecondsPerMove = 3.0f,
                easyTimedBuffer = 5f,
                mediumTimedBuffer = 4f,
                hardTimedBuffer = 4f,
                easyTimedMin = 15f,
                easyTimedMax = 70f,
                mediumTimedMin = 20f,
                mediumTimedMax = 70f,
                hardTimedMin = 25f,
                hardTimedMax = 70f,
                easyMoveLimitMargin = 4,
                mediumMoveLimitMargin = 3,
                hardMoveLimitMargin = 2,
                easyFragileMargin = 2,
                mediumFragileMargin = 1,
                hardFragileMargin = 0,
                easyLimitedMargin = 2,
                mediumLimitedMargin = 1,
                hardLimitedMargin = 0,
                localProgressNudge = 0.05f
            };
        }

        public static Settings Load()
        {
            MigrateIfNeeded();
            Settings d = CreateDefaults();
            Settings s = d;
            s.easySecondsPerMove = EditorPrefs.GetFloat(PrefsPrefix + "ESec", d.easySecondsPerMove);
            s.mediumSecondsPerMove = EditorPrefs.GetFloat(PrefsPrefix + "MSec", d.mediumSecondsPerMove);
            s.hardSecondsPerMove = EditorPrefs.GetFloat(PrefsPrefix + "HSec", d.hardSecondsPerMove);
            s.easyTimedBuffer = EditorPrefs.GetFloat(PrefsPrefix + "EBuf", d.easyTimedBuffer);
            s.mediumTimedBuffer = EditorPrefs.GetFloat(PrefsPrefix + "MBuf", d.mediumTimedBuffer);
            s.hardTimedBuffer = EditorPrefs.GetFloat(PrefsPrefix + "HBuf", d.hardTimedBuffer);
            s.easyTimedMin = EditorPrefs.GetFloat(PrefsPrefix + "ETmin", d.easyTimedMin);
            s.easyTimedMax = EditorPrefs.GetFloat(PrefsPrefix + "ETmax", d.easyTimedMax);
            s.mediumTimedMin = EditorPrefs.GetFloat(PrefsPrefix + "MTmin", d.mediumTimedMin);
            s.mediumTimedMax = EditorPrefs.GetFloat(PrefsPrefix + "MTmax", d.mediumTimedMax);
            s.hardTimedMin = EditorPrefs.GetFloat(PrefsPrefix + "HTmin", d.hardTimedMin);
            s.hardTimedMax = EditorPrefs.GetFloat(PrefsPrefix + "HTmax", d.hardTimedMax);
            s.easyMoveLimitMargin = EditorPrefs.GetInt(PrefsPrefix + "EMl", d.easyMoveLimitMargin);
            s.mediumMoveLimitMargin = EditorPrefs.GetInt(PrefsPrefix + "MMl", d.mediumMoveLimitMargin);
            s.hardMoveLimitMargin = EditorPrefs.GetInt(PrefsPrefix + "HMl", d.hardMoveLimitMargin);
            s.easyFragileMargin = EditorPrefs.GetInt(PrefsPrefix + "EFr", d.easyFragileMargin);
            s.mediumFragileMargin = EditorPrefs.GetInt(PrefsPrefix + "MFr", d.mediumFragileMargin);
            s.hardFragileMargin = EditorPrefs.GetInt(PrefsPrefix + "HFr", d.hardFragileMargin);
            s.easyLimitedMargin = EditorPrefs.GetInt(PrefsPrefix + "ELi", d.easyLimitedMargin);
            s.mediumLimitedMargin = EditorPrefs.GetInt(PrefsPrefix + "MLi", d.mediumLimitedMargin);
            s.hardLimitedMargin = EditorPrefs.GetInt(PrefsPrefix + "HLi", d.hardLimitedMargin);
            s.localProgressNudge = EditorPrefs.GetFloat(PrefsPrefix + "Nudge", d.localProgressNudge);
            return s;
        }

        public void Save()
        {
            EditorPrefs.SetFloat(PrefsPrefix + "ESec", easySecondsPerMove);
            EditorPrefs.SetFloat(PrefsPrefix + "MSec", mediumSecondsPerMove);
            EditorPrefs.SetFloat(PrefsPrefix + "HSec", hardSecondsPerMove);
            EditorPrefs.SetFloat(PrefsPrefix + "EBuf", easyTimedBuffer);
            EditorPrefs.SetFloat(PrefsPrefix + "MBuf", mediumTimedBuffer);
            EditorPrefs.SetFloat(PrefsPrefix + "HBuf", hardTimedBuffer);
            EditorPrefs.SetFloat(PrefsPrefix + "ETmin", easyTimedMin);
            EditorPrefs.SetFloat(PrefsPrefix + "ETmax", easyTimedMax);
            EditorPrefs.SetFloat(PrefsPrefix + "MTmin", mediumTimedMin);
            EditorPrefs.SetFloat(PrefsPrefix + "MTmax", mediumTimedMax);
            EditorPrefs.SetFloat(PrefsPrefix + "HTmin", hardTimedMin);
            EditorPrefs.SetFloat(PrefsPrefix + "HTmax", hardTimedMax);
            EditorPrefs.SetInt(PrefsPrefix + "EMl", easyMoveLimitMargin);
            EditorPrefs.SetInt(PrefsPrefix + "MMl", mediumMoveLimitMargin);
            EditorPrefs.SetInt(PrefsPrefix + "HMl", hardMoveLimitMargin);
            EditorPrefs.SetInt(PrefsPrefix + "EFr", easyFragileMargin);
            EditorPrefs.SetInt(PrefsPrefix + "MFr", mediumFragileMargin);
            EditorPrefs.SetInt(PrefsPrefix + "HFr", hardFragileMargin);
            EditorPrefs.SetInt(PrefsPrefix + "ELi", easyLimitedMargin);
            EditorPrefs.SetInt(PrefsPrefix + "MLi", mediumLimitedMargin);
            EditorPrefs.SetInt(PrefsPrefix + "HLi", hardLimitedMargin);
            EditorPrefs.SetFloat(PrefsPrefix + "Nudge", localProgressNudge);
            EditorPrefs.SetInt(VersionKey, CurrentConfigVersion);
        }

        /// <summary>
        /// Overwrites EditorPrefs with CreateDefaults(). Editor-only.
        /// </summary>
        public static Settings ResetToDefaults()
        {
            Settings defaults = CreateDefaults();
            defaults.Save();
            Debug.Log(
                "SpecialObjectiveDifficultyConfig: reset to defaults " +
                "(Timed Hard 17 moves → 55s, clamps max 70, nudge 5%)."
            );
            return defaults;
        }

        private static void MigrateIfNeeded()
        {
            int stored = EditorPrefs.GetInt(VersionKey, 0);
            if (stored >= CurrentConfigVersion)
            {
                return;
            }

            ResetToDefaults();
        }
    }

    [MenuItem("RushOut/Reset Special Difficulty Defaults")]
    public static void MenuResetDefaults()
    {
        if (!EditorUtility.DisplayDialog(
                "Reset Special Difficulty Defaults",
                "Overwrite EditorPrefs for Special Objective Difficulty recommendations " +
                "with the current code defaults?\n\n" +
                "Timed: Easy 3.5m+5, Medium 3.25m+4, Hard 3.0m+4 (Hard 17→55s).\n" +
                "Clamps up to 70s. Late nudge max 5%.\n" +
                "MoveLimit floor minMoves+2. Fragile/Limited +2/+1/+0.\n\n" +
                "Does not write LevelData until you Auto Tune + Apply in the planner.",
                "Reset",
                "Cancel"))
        {
            return;
        }

        Settings.ResetToDefaults();
        EditorUtility.DisplayDialog(
            "Reset Special Difficulty Defaults",
            "Defaults restored to EditorPrefs.\n" +
            "Open Level Content Planner and use AUTO TUNE / SPECIAL DIFFICULTY preview.",
            "OK"
        );
    }
}

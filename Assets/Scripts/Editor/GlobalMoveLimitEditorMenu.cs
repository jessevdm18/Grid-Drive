using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor validation for forgiving global move-limit formula (Phase 3).
/// </summary>
public static class GlobalMoveLimitEditorMenu
{
    private const string MenuPath =
        "RushOut/Testing/Lives/Validate Global Move Limits";

    [MenuItem(MenuPath, priority = 220)]
    private static void ValidateGlobalMoveLimits()
    {
        int pass = 0;
        int fail = 0;
        const int total = 9;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[MoveLimitValidation] === FORMULA A–I ===");

        void Check(string id, int minMoves, LevelDifficulty diff, int expected)
        {
            int actual = GlobalMoveLimitUtility.ComputeForgivingGlobalLimit(
                minMoves,
                diff
            );
            bool ok = actual == expected;
            if (ok)
            {
                pass++;
                string line =
                    "[MoveLimitValidation] " + id + " PASS " +
                    diff + " min=" + minMoves + " → " + actual;
                sb.AppendLine(line);
                Debug.Log(line);
            }
            else
            {
                fail++;
                string line =
                    "[MoveLimitValidation] " + id + " FAIL: expected " +
                    expected + ", actual " + actual +
                    " (" + diff + " min=" + minMoves + ")";
                sb.AppendLine(line);
                Debug.LogError(line);
            }
        }

        Check("A", 3, LevelDifficulty.Easy, 9);
        Check("B", 7, LevelDifficulty.Easy, 16);
        Check("C", 9, LevelDifficulty.Easy, 19);
        Check("D", 10, LevelDifficulty.Medium, 19);
        Check("E", 13, LevelDifficulty.Medium, 24);
        Check("F", 15, LevelDifficulty.Medium, 27);
        Check("G", 16, LevelDifficulty.Hard, 27);
        Check("H", 18, LevelDifficulty.Hard, 30);
        Check("I", 30, LevelDifficulty.Hard, 48);

        // Extra safety: special MoveLimit mode, invalid data
        LevelData fakeSpecial = ScriptableObject.CreateInstance<LevelData>();
        fakeSpecial.objectiveType = LevelObjectiveType.MoveLimit;
        fakeSpecial.moveLimit = 16;
        fakeSpecial.minimumMoves = 8;
        fakeSpecial.difficulty = LevelDifficulty.Easy;
        int special = GlobalMoveLimitUtility.GetEffectiveMoveLimit(
            fakeSpecial,
            out GlobalMoveLimitUtility.LimitMode specialMode
        );
        bool specialOk =
            special == 16 &&
            specialMode == GlobalMoveLimitUtility.LimitMode.SpecialMoveLimit &&
            !GlobalMoveLimitUtility.IsGlobalFailureCheckActive(fakeSpecial);
        Object.DestroyImmediate(fakeSpecial);

        LevelData fakeInvalid = ScriptableObject.CreateInstance<LevelData>();
        fakeInvalid.objectiveType = LevelObjectiveType.Classic;
        fakeInvalid.minimumMoves = 0;
        fakeInvalid.difficulty = LevelDifficulty.Easy;
        int invalidLimit = GlobalMoveLimitUtility.GetEffectiveMoveLimit(
            fakeInvalid,
            out GlobalMoveLimitUtility.LimitMode invalidMode
        );
        bool invalidOk =
            invalidLimit == 0 &&
            invalidMode == GlobalMoveLimitUtility.LimitMode.None &&
            !GlobalMoveLimitUtility.IsGlobalFailureCheckActive(fakeInvalid);
        Object.DestroyImmediate(fakeInvalid);

        sb.AppendLine(
            specialOk
                ? "[MoveLimitValidation] SpecialMoveLimit mode PASS"
                : "[MoveLimitValidation] SpecialMoveLimit mode FAIL"
        );
        sb.AppendLine(
            invalidOk
                ? "[MoveLimitValidation] Invalid minMoves mode PASS"
                : "[MoveLimitValidation] Invalid minMoves mode FAIL"
        );
        if (!specialOk || !invalidOk)
        {
            fail++;
        }

        string result =
            "[MoveLimitValidation] RESULT: " + pass + "/" + total +
            " formula PASS" +
            (fail > 0 ? " (extra/fails=" + fail + ")" : string.Empty);
        sb.AppendLine(result);
        Debug.Log(sb.ToString());
        EditorUtility.DisplayDialog(
            "Global Move Limit Validation",
            pass + "/" + total + " formula cases PASS" +
            (specialOk && invalidOk
                ? "\nSpecial + invalid modes OK"
                : "\nSee Console for mode checks") +
            "\n\nRuntime J–V remain manual Play Mode checks.",
            "OK"
        );
    }
}

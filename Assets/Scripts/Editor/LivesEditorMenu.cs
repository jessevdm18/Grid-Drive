using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor/DEV tools for Phase 1 LivesManager (prefs + regeneration).
/// A–O validation runs in Edit Mode via an ephemeral host (no Play Mode required).
/// Mutating runtime menus require Play Mode.
/// </summary>
public static class LivesEditorMenu
{
    private const string MenuRoot = "RushOut/Testing/Lives/";
    private const int PrefsScanLimit = 512;

    [MenuItem(MenuRoot + "Log Lives State", priority = 200)]
    private static void LogLivesState()
    {
        if (!RequirePlayMode("Log Lives State"))
        {
            return;
        }

        LivesManager manager = LivesManager.EnsureInstance();
        if (manager == null)
        {
            Debug.LogWarning("[Lives] Log Lives State — EnsureInstance returned null.");
            return;
        }

        manager.RecalculateRegeneration();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[Lives] === STATE ===");
        sb.AppendLine("CurrentLives=" + manager.CurrentLives + "/" + LivesManager.MaxLives);
        sb.AppendLine("HasLives=" + manager.HasLives);
        sb.AppendLine("IsFull=" + manager.IsFull);
        sb.AppendLine("HasActiveTimer=" + manager.HasActiveNextLifeTimestamp);
        sb.AppendLine("TimeUntilNextLife=" + manager.TimeUntilNextLife);
        sb.AppendLine(
            "NextLifeUtc=" +
            (manager.NextLifeUtc.HasValue
                ? manager.NextLifeUtc.Value.ToString("u")
                : "(none)")
        );
        sb.AppendLine(
            "PrefsLives=" +
            (PlayerPrefs.HasKey(LivesManager.LivesPrefsKey)
                ? PlayerPrefs.GetInt(LivesManager.LivesPrefsKey).ToString()
                : "(missing)")
        );
        sb.AppendLine(
            "PrefsNextTicks=" +
            (PlayerPrefs.HasKey(LivesManager.NextLifeUtcTicksPrefsKey)
                ? PlayerPrefs.GetString(LivesManager.NextLifeUtcTicksPrefsKey)
                : "(missing)")
        );
        Debug.Log(sb.ToString());
    }

    [MenuItem(MenuRoot + "Remove 1 Life", priority = 201)]
    private static void RemoveOneLife()
    {
        if (!RequirePlayMode("Remove 1 Life"))
        {
            return;
        }

        LivesManager manager = LivesManager.EnsureInstance();
        if (manager == null)
        {
            Debug.LogWarning("[Lives] Remove 1 Life — EnsureInstance returned null.");
            return;
        }

        bool ok = manager.TryRemoveLife();
        Debug.Log(
            "[Lives] Remove 1 Life → " + (ok ? "OK" : "FAILED") +
            " now=" + manager.CurrentLives + "/" + LivesManager.MaxLives
        );
    }

    [MenuItem(MenuRoot + "Add 1 Life", priority = 202)]
    private static void AddOneLife()
    {
        if (!RequirePlayMode("Add 1 Life"))
        {
            return;
        }

        LivesManager manager = LivesManager.EnsureInstance();
        if (manager == null)
        {
            Debug.LogWarning("[Lives] Add 1 Life — EnsureInstance returned null.");
            return;
        }

        bool ok = manager.AddLife(1);
        Debug.Log(
            "[Lives] Add 1 Life → " + (ok ? "OK" : "FAILED") +
            " now=" + manager.CurrentLives + "/" + LivesManager.MaxLives
        );
    }

    [MenuItem(MenuRoot + "Force Next Life Due Now", priority = 203)]
    private static void ForceNextLifeDueNow()
    {
        if (!RequirePlayMode("Force Next Life Due Now"))
        {
            return;
        }

        LivesManager manager = LivesManager.EnsureInstance();
        if (manager == null)
        {
            Debug.LogWarning("[Lives] Force Next Life Due Now — EnsureInstance returned null.");
            return;
        }

        manager.DebugForceNextLifeDueNow();
        manager.RecalculateRegeneration();
        Debug.Log(
            "[Lives] After force-due + recalc: lives=" + manager.CurrentLives +
            "/" + LivesManager.MaxLives +
            " until=" + manager.TimeUntilNextLife
        );
    }

    [MenuItem(MenuRoot + "Validate Phase 2 Gate+Consume (simulation)", priority = 211)]
    private static void ValidatePhase2GateAndConsume()
    {
        int pass = 0;
        int fail = 0;
        const int total = 8;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[LivesValidation] === PHASE 2 GATE+CONSUME ===");

        void Check(string id, bool ok, string expected, string actual)
        {
            if (ok)
            {
                pass++;
                string line = "[LivesValidation] " + id + " PASS " + actual;
                sb.AppendLine(line);
                Debug.Log(line);
            }
            else
            {
                fail++;
                string line =
                    "[LivesValidation] " + id + " FAIL: expected " + expected +
                    ", actual " + actual;
                sb.AppendLine(line);
                Debug.LogError(line);
            }
        }

        PrefsSnapshot snapshot = PrefsSnapshot.Capture();
        LivesManager manager = null;
        try
        {
            LivesManager.DeleteAllPersistedPrefs();
            PlayerPrefs.Save();
            manager = LivesManager.CreateEphemeralEditorValidationHost();
            if (manager == null)
            {
                Debug.LogError(
                    "[LivesValidation] SETUP FAIL: ephemeral LivesManager null"
                );
                return;
            }

            manager.ReinitializeToFreshDefaults();

            // P2-A: start check at 5 — allowed, no consume
            bool begin5 = manager.TryBeginLevelAttempt("test_start");
            Check(
                "P2-A",
                begin5 && manager.CurrentLives == 5,
                "allow at 5/5 unchanged",
                "ok=" + begin5 + " lives=" + manager.CurrentLives
            );

            // P2-B: simulate FailLevel consume
            manager.TryRemoveLife();
            Check(
                "P2-B",
                manager.CurrentLives == 4,
                "4 after one consume",
                "lives=" + manager.CurrentLives
            );

            // P2-C: begin still allowed at 4, no further consume
            bool begin4 = manager.TryBeginLevelAttempt("test_retry");
            Check(
                "P2-C",
                begin4 && manager.CurrentLives == 4,
                "retry allowed, still 4",
                "ok=" + begin4 + " lives=" + manager.CurrentLives
            );

            // Drain to 0
            while (manager.CurrentLives > 0)
            {
                manager.TryRemoveLife();
            }

            Check(
                "P2-M",
                manager.CurrentLives == 0,
                "0 lives",
                "lives=" + manager.CurrentLives
            );

            // P2-N: retry blocked at 0, still 0
            bool begin0 = manager.TryBeginLevelAttempt("restart");
            Check(
                "P2-N",
                !begin0 && manager.CurrentLives == 0,
                "blocked at 0",
                "ok=" + begin0 + " lives=" + manager.CurrentLives
            );

            // P2-O: scene entry source blocked
            bool beginScene = manager.TryBeginLevelAttempt("scene_gameplay");
            Check(
                "P2-O",
                !beginScene && manager.CurrentLives == 0,
                "scene_gameplay blocked",
                "ok=" + beginScene
            );

            // P2-P: next blocked
            bool beginNext = manager.TryBeginLevelAttempt("next");
            Check(
                "P2-P",
                !beginNext && manager.CurrentLives == 0,
                "next blocked",
                "ok=" + beginNext
            );

            // P2-R: V1 bypass allowed at 0 without consume
            bool beginV1 = manager.TryBeginLevelAttempt("v1", bypassForV1Playtest: true);
            Check(
                "P2-R",
                beginV1 && manager.CurrentLives == 0,
                "V1 bypass allow, lives unchanged",
                "ok=" + beginV1 + " lives=" + manager.CurrentLives
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("[LivesValidation] SETUP FAIL: " + ex);
        }
        finally
        {
            LivesManager.DestroyEphemeralEditorValidationHost(manager);
            snapshot.Restore();
        }

        string result =
            "[LivesValidation] RESULT: " + pass + "/" + total + " PASS" +
            (fail > 0 ? " (" + fail + " FAIL)" : string.Empty);
        sb.AppendLine(result);
        Debug.Log(sb.ToString());
        EditorUtility.DisplayDialog(
            "Lives Phase 2 Validation",
            pass + "/" + total + " PASS" +
            (fail > 0 ? "\nSee Console." : string.Empty) +
            "\n\nPlay Mode A–S (FailLevel/Restart/Undo/Win) remain manual.",
            "OK"
        );
    }

    [MenuItem(MenuRoot + "Validate Regen Cases A–O (simulation)", priority = 210)]
    private static void ValidateRegenCases()
    {
        bool confirm = EditorUtility.DisplayDialog(
            "Lives Validation A–O",
            "Runs simulated regeneration checks in Edit Mode.\n\n" +
            "PlayerPrefs are snapshotted and restored in finally " +
            "(including after failures).\n\nContinue?",
            "RUN VALIDATION",
            "CANCEL"
        );
        if (!confirm)
        {
            return;
        }

        int pass = 0;
        int fail = 0;
        const int totalCases = 15;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[LivesValidation] === A–O START (Edit Mode) ===");

        void Check(string id, bool ok, string expected, string actual)
        {
            if (ok)
            {
                pass++;
                string line = "[LivesValidation] " + id + " PASS " + actual;
                sb.AppendLine(line);
                Debug.Log(line);
            }
            else
            {
                fail++;
                string line =
                    "[LivesValidation] " + id + " FAIL: expected " + expected +
                    ", actual " + actual;
                sb.AppendLine(line);
                Debug.LogError(line);
            }
        }

        PrefsSnapshot snapshot = PrefsSnapshot.Capture();
        LivesManager manager = null;

        try
        {
            LivesManager.DeleteAllPersistedPrefs();
            PlayerPrefs.Save();

            manager = LivesManager.CreateEphemeralEditorValidationHost();
            if (manager == null)
            {
                Debug.LogError(
                    "[LivesValidation] SETUP FAIL: " +
                    "CreateEphemeralEditorValidationHost returned null"
                );
                return;
            }

            manager.ReinitializeToFreshDefaults();

            DateTime t0 = new DateTime(2030, 6, 1, 12, 0, 0, DateTimeKind.Utc);

            // A
            Check(
                "A",
                manager.CurrentLives == 5 && !manager.HasActiveNextLifeTimestamp,
                "5/5 no timer",
                "lives=" + manager.CurrentLives +
                " timer=" + manager.HasActiveNextLifeTimestamp
            );

            // B
            bool removedB = manager.TryRemoveLife(t0);
            Check(
                "B",
                removedB &&
                manager.CurrentLives == 4 &&
                DueEquals(manager, t0.AddHours(2)),
                "4/5 due T+2h",
                "lives=" + manager.CurrentLives + " due=" + FormatDue(manager)
            );

            // C
            bool removedC = manager.TryRemoveLife(t0.AddMinutes(30));
            Check(
                "C",
                removedC &&
                manager.CurrentLives == 3 &&
                DueEquals(manager, t0.AddHours(2)),
                "3/5 due still T+2h",
                "lives=" + manager.CurrentLives + " due=" + FormatDue(manager)
            );

            // D
            manager.RecalculateRegeneration(t0.AddHours(2), "ValD");
            Check(
                "D",
                manager.CurrentLives == 4 && DueEquals(manager, t0.AddHours(4)),
                "4/5 due T+4h",
                "lives=" + manager.CurrentLives + " due=" + FormatDue(manager)
            );

            // E
            manager.RecalculateRegeneration(t0.AddHours(4), "ValE");
            Check(
                "E",
                manager.CurrentLives == 5 && !manager.HasActiveNextLifeTimestamp,
                "5/5 timer cleared",
                "lives=" + manager.CurrentLives +
                " timer=" + manager.HasActiveNextLifeTimestamp
            );

            // F
            manager.EditorSetStateForTesting(1, t0.AddHours(2));
            manager.RecalculateRegeneration(t0.AddHours(9), "ValF");
            Check(
                "F",
                manager.CurrentLives == 5 && !manager.HasActiveNextLifeTimestamp,
                "5/5 timer cleared",
                "lives=" + manager.CurrentLives +
                " timer=" + manager.HasActiveNextLifeTimestamp
            );

            // G
            manager.EditorSetStateForTesting(1, t0.AddHours(2));
            manager.RecalculateRegeneration(t0.AddHours(5), "ValG");
            Check(
                "G",
                manager.CurrentLives == 3 && DueEquals(manager, t0.AddHours(6)),
                "3/5 due T+6h",
                "lives=" + manager.CurrentLives + " due=" + FormatDue(manager)
            );

            // H
            manager.EditorSetStateForTesting(0, t0.AddHours(2));
            manager.RecalculateRegeneration(t0.AddHours(2), "ValH");
            Check(
                "H",
                manager.CurrentLives == 1 && DueEquals(manager, t0.AddHours(4)),
                "1/5 due T+4h",
                "lives=" + manager.CurrentLives + " due=" + FormatDue(manager)
            );

            // I
            manager.EditorSetStateForTesting(2, t0.AddHours(2));
            bool addedI = manager.AddLife(1, t0.AddMinutes(10));
            Check(
                "I",
                addedI &&
                manager.CurrentLives == 3 &&
                DueEquals(manager, t0.AddHours(2)),
                "3/5 due preserved T+2h",
                "lives=" + manager.CurrentLives + " due=" + FormatDue(manager)
            );

            // J
            manager.EditorSetStateForTesting(4, t0.AddHours(2));
            bool addedJ = manager.AddLife(1, t0);
            Check(
                "J",
                addedJ &&
                manager.CurrentLives == 5 &&
                !manager.HasActiveNextLifeTimestamp,
                "5/5 timer cleared",
                "lives=" + manager.CurrentLives +
                " timer=" + manager.HasActiveNextLifeTimestamp
            );

            // K — load clamp only (no UtcNow regen grant)
            PlayerPrefs.SetInt(LivesManager.LivesPrefsKey, 99);
            PlayerPrefs.DeleteKey(LivesManager.NextLifeUtcTicksPrefsKey);
            PlayerPrefs.Save();
            manager.EditorLoadFromPrefsOnly();
            bool kHigh = manager.CurrentLives == 5;

            PlayerPrefs.SetInt(LivesManager.LivesPrefsKey, -3);
            PlayerPrefs.SetString(
                LivesManager.NextLifeUtcTicksPrefsKey,
                t0.AddHours(2).Ticks.ToString()
            );
            PlayerPrefs.Save();
            manager.EditorLoadFromPrefsOnly();
            bool kLow = manager.CurrentLives == 0;
            Check(
                "K",
                kHigh && kLow,
                "clamp 99→5 and -3→0",
                "highClamp=" + kHigh + " lowClamp=" + kLow +
                " livesNow=" + manager.CurrentLives
            );

            // L
            manager.EditorSetStateForTesting(2, null);
            manager.RecalculateRegeneration(t0, "ValL");
            Check(
                "L",
                manager.CurrentLives == 2 && DueEquals(manager, t0.AddHours(2)),
                "2/5 due T+2h (no free lives)",
                "lives=" + manager.CurrentLives + " due=" + FormatDue(manager)
            );

            // M
            manager.EditorSetStateForTesting(2, t0.AddHours(2));
            manager.RecalculateRegeneration(t0.AddHours(2), "ValM1");
            int afterFirst = manager.CurrentLives;
            manager.RecalculateRegeneration(t0.AddHours(2), "ValM2");
            Check(
                "M",
                afterFirst == 3 && manager.CurrentLives == 3 &&
                DueEquals(manager, t0.AddHours(4)),
                "3/5 no duplicate grant",
                "afterFirst=" + afterFirst +
                " lives=" + manager.CurrentLives +
                " due=" + FormatDue(manager)
            );

            // N
            manager.EditorSetStateForTesting(3, t0.AddHours(2));
            int livesBeforeN = manager.CurrentLives;
            long ticksBeforeN = ReadTicksPref();
            SaveManager.ResetLevelProgressForContentVersionMismatch();
            int livesAfterN = PlayerPrefs.GetInt(LivesManager.LivesPrefsKey, -999);
            long ticksAfterN = ReadTicksPref();
            Check(
                "N",
                livesAfterN == livesBeforeN && ticksAfterN == ticksBeforeN,
                "lives+ticks preserved",
                "lives=" + livesAfterN + " ticksMatch=" + (ticksAfterN == ticksBeforeN)
            );

            // O
            manager.EditorSetStateForTesting(2, t0.AddHours(2));
            SaveManager.EditorResetAllPlayerProgressPrefs();
            bool keysCleared =
                !PlayerPrefs.HasKey(LivesManager.LivesPrefsKey) &&
                !PlayerPrefs.HasKey(LivesManager.NextLifeUtcTicksPrefsKey);
            manager.ReinitializeToFreshDefaults();
            Check(
                "O",
                keysCleared &&
                manager.CurrentLives == 5 &&
                !manager.HasActiveNextLifeTimestamp &&
                !PlayerPrefs.HasKey(LivesManager.NextLifeUtcTicksPrefsKey),
                "keys cleared then 5/5 no timer",
                "keysCleared=" + keysCleared +
                " lives=" + manager.CurrentLives +
                " timer=" + manager.HasActiveNextLifeTimestamp
            );
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[LivesValidation] SETUP FAIL: unhandled exception — " + ex
            );
        }
        finally
        {
            LivesManager.DestroyEphemeralEditorValidationHost(manager);
            snapshot.Restore();

            // Sweep any leftover validation hosts (should be none).
            LivesManager[] leftovers =
                UnityEngine.Object.FindObjectsByType<LivesManager>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );
            for (int i = 0; i < leftovers.Length; i++)
            {
                LivesManager leftover = leftovers[i];
                if (leftover == null)
                {
                    continue;
                }

                if (leftover.gameObject != null &&
                    leftover.gameObject.name == "LivesManager_EditorValidation")
                {
                    LivesManager.DestroyEphemeralEditorValidationHost(leftover);
                }
            }
        }

        string resultLine =
            "[LivesValidation] RESULT: " + pass + "/" + totalCases + " PASS" +
            (fail > 0 ? " (" + fail + " FAIL)" : string.Empty);
        sb.AppendLine(resultLine);
        if (fail == 0 && pass == totalCases)
        {
            Debug.Log(sb.ToString());
        }
        else
        {
            Debug.LogWarning(sb.ToString());
        }

        EditorUtility.DisplayDialog(
            "Lives Validation",
            pass + "/" + totalCases + " PASS" +
            (fail > 0 ? "\n" + fail + " FAIL — see Console." : string.Empty),
            "OK"
        );
    }

    private static bool RequirePlayMode(string commandName)
    {
        if (Application.isPlaying)
        {
            return true;
        }

        Debug.LogWarning(
            "[Lives] " + commandName +
            " requires Play Mode (runtime LivesManager singleton). " +
            "Use Validate Regen Cases A–O for Edit Mode simulation."
        );
        return false;
    }

    /// <summary>
    /// Snapshot of every PlayerPrefs key the validator / SaveManager resets may touch.
    /// </summary>
    private struct PrefsSnapshot
    {
        private Dictionary<string, string> stringKeys;
        private Dictionary<string, int> intKeys;
        private HashSet<string> presentKeys;

        public static PrefsSnapshot Capture()
        {
            PrefsSnapshot s = new PrefsSnapshot
            {
                stringKeys = new Dictionary<string, string>(),
                intKeys = new Dictionary<string, int>(),
                presentKeys = new HashSet<string>()
            };

            CaptureInt(s, LivesManager.LivesPrefsKey);
            CaptureString(s, LivesManager.NextLifeUtcTicksPrefsKey);
            CaptureInt(s, SaveManager.CoinsPrefsKey);
            CaptureInt(s, "RushOut_UnlockedLevel");
            CaptureInt(s, "RushOut_CurrentLevel");
            CaptureInt(s, "RushOut_FirstLaunchCompleted");
            CaptureInt(s, "RushOut_LastSelectedDifficulty");
            CaptureInt(s, LevelDatabaseContentVersion.PrefsKey);
            CaptureInt(s, "RushOut_ThreeStarRewardMigrationCompleted");
            CaptureInt(s, "RushOut_ThreeStarCoinClaimMigrationV1");
            CaptureInt(s, "LevelStars_MaxIndex");
            CaptureInt(s, "RushOut_ThreeStarCoinRewardClaimed_MaxIndex");

            // Match SaveManager scan width so N/O cannot orphan unrestored keys.
            for (int i = 0; i < PrefsScanLimit; i++)
            {
                CaptureInt(s, "LevelStars_" + i);
                CaptureInt(s, "RushOut_ThreeStarCoinRewardClaimed_" + i);
            }

            return s;
        }

        public void Restore()
        {
            // Clear keys this validator may have created/changed, then rewrite snapshot.
            string[] alwaysClear =
            {
                LivesManager.LivesPrefsKey,
                LivesManager.NextLifeUtcTicksPrefsKey,
                SaveManager.CoinsPrefsKey,
                "RushOut_UnlockedLevel",
                "RushOut_CurrentLevel",
                "RushOut_FirstLaunchCompleted",
                "RushOut_LastSelectedDifficulty",
                LevelDatabaseContentVersion.PrefsKey,
                "RushOut_ThreeStarRewardMigrationCompleted",
                "RushOut_ThreeStarCoinClaimMigrationV1",
                "LevelStars_MaxIndex",
                "RushOut_ThreeStarCoinRewardClaimed_MaxIndex"
            };

            for (int i = 0; i < alwaysClear.Length; i++)
            {
                PlayerPrefs.DeleteKey(alwaysClear[i]);
            }

            for (int i = 0; i < PrefsScanLimit; i++)
            {
                PlayerPrefs.DeleteKey("LevelStars_" + i);
                PlayerPrefs.DeleteKey("RushOut_ThreeStarCoinRewardClaimed_" + i);
            }

            foreach (KeyValuePair<string, int> pair in intKeys)
            {
                PlayerPrefs.SetInt(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<string, string> pair in stringKeys)
            {
                PlayerPrefs.SetString(pair.Key, pair.Value);
            }

            PlayerPrefs.Save();
        }

        private static void CaptureInt(PrefsSnapshot s, string key)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                return;
            }

            s.presentKeys.Add(key);
            s.intKeys[key] = PlayerPrefs.GetInt(key);
        }

        private static void CaptureString(PrefsSnapshot s, string key)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                return;
            }

            s.presentKeys.Add(key);
            s.stringKeys[key] = PlayerPrefs.GetString(key);
        }
    }

    private static bool DueEquals(LivesManager manager, DateTime expected)
    {
        if (manager == null || !manager.NextLifeUtc.HasValue)
        {
            return false;
        }

        return Math.Abs(
                   manager.NextLifeUtc.Value.Ticks -
                   DateTime.SpecifyKind(expected, DateTimeKind.Utc).Ticks
               ) < TimeSpan.FromSeconds(1).Ticks;
    }

    private static string FormatDue(LivesManager manager)
    {
        if (manager == null || !manager.NextLifeUtc.HasValue)
        {
            return "(none)";
        }

        return manager.NextLifeUtc.Value.ToString("u");
    }

    private static long ReadTicksPref()
    {
        if (!PlayerPrefs.HasKey(LivesManager.NextLifeUtcTicksPrefsKey))
        {
            return 0L;
        }

        long.TryParse(
            PlayerPrefs.GetString(LivesManager.NextLifeUtcTicksPrefsKey, "0"),
            out long ticks
        );
        return ticks;
    }
}

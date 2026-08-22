using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Safe deep minMoves audit for suspect levels only (TIMEOUT / mismatch / invalid).
/// Preview never writes. Apply writes only VALID + HintPath PASS + moves &gt; 0.
/// Menu: RushOut → Solver → Deep Min Moves (Suspects) …
/// </summary>
public static class DeepMinMovesSuspectAudit
{
    private const int NormalBudget = ProductionLevelPlayability.DefaultMaxStates;
    private const int DeepBudget = ProductionLevelPlayability.DeepMaxStates; // 2_000_000

    public sealed class SuspectRow
    {
        public int dbIndex;
        public string assetName;
        public LevelDifficulty difficulty;
        public int localNumber;
        public LevelObjectiveType objective;
        public int storedMinMoves;
        public int deepPlayerMoves;
        public ProductionLevelPlayability.Status normalStatus;
        public ProductionLevelPlayability.Status deepStatus;
        public string deepDetail;
        public bool hintPathPass;
        public string hintPathDetail;
        public string suspectReason;
        public bool changeNeeded;
        public bool applyEligible;
        public string difficultyWarning;
    }

    private static List<SuspectRow> lastPreview = new List<SuspectRow>();

    [MenuItem("RushOut/Solver/Deep Min Moves (Suspects) — Preview", priority = 60)]
    public static void MenuPreview()
    {
        LevelDatabase database = LoadMainDb();
        if (database == null)
        {
            return;
        }

        lastPreview = BuildSuspectDeepPreview(database);
        StringBuilder sb = FormatPreviewReport(lastPreview);
        Debug.Log(sb.ToString());

        string outPath = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "RushOut_DeepMinMovesSuspects_Preview.txt"
        );
        System.IO.File.WriteAllText(outPath, sb.ToString());

        int eligible = CountApplyEligible(lastPreview);
        int timeouts = CountDeepTimeout(lastPreview);
        int changes = CountChangeNeeded(lastPreview);

        EditorUtility.DisplayDialog(
            "Deep Min Moves — Preview",
            "Suspects deep-audited: " + lastPreview.Count + "\n" +
            "ChangeNeeded: " + changes + "\n" +
            "Apply-eligible (VALID+HintPath): " + eligible + "\n" +
            "Still TIMEOUT: " + timeouts + "\n\n" +
            "No LevelData writes.\n" +
            "See Console / " + outPath + "\n\n" +
            "Next: RushOut → Solver → APPLY VERIFIED DEEP MIN MOVES",
            "OK"
        );
    }

    [MenuItem("RushOut/Solver/APPLY VERIFIED DEEP MIN MOVES", priority = 61)]
    public static void MenuApply()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Apply Deep Min Moves",
                "Exit Play Mode before writing LevelData.",
                "OK"
            );
            return;
        }

        if (lastPreview == null || lastPreview.Count == 0)
        {
            bool run = EditorUtility.DisplayDialog(
                "Apply Deep Min Moves",
                "No cached preview.\n\nRun deep preview now?",
                "Preview now",
                "Cancel"
            );
            if (!run)
            {
                return;
            }

            MenuPreview();
            if (lastPreview == null || lastPreview.Count == 0)
            {
                return;
            }
        }

        int eligible = CountApplyEligible(lastPreview);
        int changeEligible = 0;
        for (int i = 0; i < lastPreview.Count; i++)
        {
            if (lastPreview[i].applyEligible && lastPreview[i].changeNeeded)
            {
                changeEligible++;
            }
        }

        StringBuilder previewLines = new StringBuilder();
        for (int i = 0; i < lastPreview.Count; i++)
        {
            SuspectRow row = lastPreview[i];
            if (!row.applyEligible || !row.changeNeeded)
            {
                continue;
            }

            previewLines.AppendLine(
                row.assetName + ": " + row.storedMinMoves + " → " + row.deepPlayerMoves
            );
        }

        bool ok = EditorUtility.DisplayDialog(
            "APPLY VERIFIED DEEP MIN MOVES",
            "Write verified deep minimumMoves?\n\n" +
            "Eligible rows: " + eligible + "\n" +
            "Will change: " + changeEligible + "\n\n" +
            previewLines +
            (changeEligible == 0 ? "(nothing to write)\n\n" : "\n") +
            "TIMEOUT / UNSOLVABLE rows are skipped.\n" +
            "Difficulty assets are NOT retagged.\n" +
            "Special Timed/MoveLimit params are NOT auto-tuned.",
            "APPLY",
            "CANCEL"
        );
        if (!ok)
        {
            return;
        }

        int written = ApplyVerifiedDeepMinMoves(lastPreview);
        StringBuilder after = new StringBuilder();
        after.AppendLine("=== APPLY VERIFIED DEEP MIN MOVES ===");
        after.AppendLine("Wrote=" + written);
        after.AppendLine();
        after.AppendLine(
            "WARNING: Re-run Special Auto Tune recommendations " +
            "(Timed/MoveLimit use minMoves)."
        );
        after.AppendLine(
            "Ordering: LevelSelect / Gameplay / Next / Planner Disp# " +
            "use LevelDifficultyOrder (minMoves ASC) automatically."
        );
        after.AppendLine("No layout/objective/gameplay rule changes.");
        Debug.Log(after.ToString());

        EditorUtility.DisplayDialog(
            "Apply Deep Min Moves",
            "Wrote minimumMoves on " + written + " LevelData asset(s).\n\n" +
            "Re-run Special Auto Tune recommendations if needed.\n" +
            "Local Hard/Easy/Medium order updates via LevelDifficultyOrder.",
            "OK"
        );
    }

    /// <summary>
    /// Normal-pass filter → deep solve only suspects. No writes.
    /// </summary>
    public static List<SuspectRow> BuildSuspectDeepPreview(LevelDatabase database)
    {
        List<SuspectRow> rows = new List<SuspectRow>();
        if (database == null || database.levels == null)
        {
            return rows;
        }

        // Progress: normal scan first.
        List<int> suspectIndices = new List<int>();
        Dictionary<int, string> reasonByIndex = new Dictionary<int, string>();
        Dictionary<int, ProductionLevelPlayability.Report> normalByIndex =
            new Dictionary<int, ProductionLevelPlayability.Report>();

        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level == null)
            {
                continue;
            }

            EditorUtility.DisplayProgressBar(
                "Deep Min Moves — Normal scan",
                level.name + " (" + (i + 1) + "/" + database.levels.Count + ")",
                (i + 1) / (float)database.levels.Count
            );

            ProductionLevelPlayability.Report normal =
                ProductionLevelPlayability.Validate(level, i, NormalBudget);
            normalByIndex[i] = normal;

            string reason;
            if (IsSuspect(level, normal, out reason))
            {
                suspectIndices.Add(i);
                reasonByIndex[i] = reason;
            }
        }

        EditorUtility.ClearProgressBar();

        for (int s = 0; s < suspectIndices.Count; s++)
        {
            int dbIndex = suspectIndices[s];
            LevelData level = database.levels[dbIndex];
            EditorUtility.DisplayProgressBar(
                "Deep Min Moves — Deep solve",
                level.name + " (" + (s + 1) + "/" + suspectIndices.Count + ")",
                (s + 1) / (float)Mathf.Max(1, suspectIndices.Count)
            );

            SuspectRow row = BuildDeepRow(
                database,
                level,
                dbIndex,
                normalByIndex[dbIndex],
                reasonByIndex[dbIndex]
            );
            rows.Add(row);
        }

        EditorUtility.ClearProgressBar();
        rows.Sort((a, b) => a.dbIndex.CompareTo(b.dbIndex));
        return rows;
    }

    private static bool IsSuspect(
        LevelData level,
        ProductionLevelPlayability.Report normal,
        out string reason)
    {
        reason = string.Empty;

        if (normal.status == ProductionLevelPlayability.Status.SolverTimeout ||
            (normal.solverResult != null && normal.solverResult.searchLimitReached) ||
            (!string.IsNullOrEmpty(normal.searchLimitKind)))
        {
            reason = "TIMEOUT / searchLimitReached";
            return true;
        }

        if (!LevelMinMoves.IsValid(level.minimumMoves))
        {
            reason = "invalid/stale stored minimumMoves";
            return true;
        }

        if (normal.status == ProductionLevelPlayability.Status.Valid &&
            LevelMinMoves.IsValid(normal.minimumMoves) &&
            normal.minimumMoves != level.minimumMoves)
        {
            reason =
                "mismatch stored=" + level.minimumMoves +
                " verified=" + normal.minimumMoves;
            return true;
        }

        // Known deep mismatch case even if normal timed out already caught above.
        if (level.name == "Level_096")
        {
            reason = "known deep mismatch candidate (Hard #20)";
            return true;
        }

        return false;
    }

    private static SuspectRow BuildDeepRow(
        LevelDatabase database,
        LevelData level,
        int dbIndex,
        ProductionLevelPlayability.Report normal,
        string suspectReason)
    {
        SuspectRow row = new SuspectRow
        {
            dbIndex = dbIndex,
            assetName = level.name,
            difficulty = level.difficulty,
            localNumber = LevelDifficultyOrder.GetDifficultyDisplayNumber(database, dbIndex),
            objective = level.objectiveType,
            storedMinMoves = level.minimumMoves,
            deepPlayerMoves = -1,
            normalStatus = normal.status,
            deepStatus = ProductionLevelPlayability.Status.Unknown,
            deepDetail = string.Empty,
            hintPathPass = false,
            hintPathDetail = string.Empty,
            suspectReason = suspectReason,
            changeNeeded = false,
            applyEligible = false,
            difficultyWarning = string.Empty
        };

        ProductionLevelPlayability.Report deep =
            ProductionLevelPlayability.Validate(level, dbIndex, DeepBudget);
        row.deepStatus = deep.status;
        row.deepDetail = deep.detail;

        if (deep.status == ProductionLevelPlayability.Status.Valid &&
            LevelMinMoves.IsValid(deep.minimumMoves))
        {
            row.deepPlayerMoves = deep.minimumMoves;
            bool hintOk = ProductionLevelPlayability.VerifyHintPath(
                level,
                out string hintDetail,
                DeepBudget,
                512
            );
            row.hintPathPass = hintOk;
            row.hintPathDetail = hintDetail;
            row.applyEligible = hintOk && row.deepPlayerMoves > 0;
            row.changeNeeded = row.applyEligible &&
                row.deepPlayerMoves != row.storedMinMoves;

            row.difficultyWarning = LevelMinMovesDifficulty.GetMismatchWarning(
                level.difficulty,
                row.deepPlayerMoves
            );
        }
        else
        {
            row.hintPathPass = false;
            row.hintPathDetail = "skipped (deep not VALID)";
            row.applyEligible = false;
            row.changeNeeded = false;
        }

        return row;
    }

    public static int ApplyVerifiedDeepMinMoves(List<SuspectRow> preview)
    {
        if (preview == null || preview.Count == 0)
        {
            return 0;
        }

        LevelDatabase database = LoadMainDb();
        if (database == null || database.levels == null)
        {
            return 0;
        }

        int written = 0;
        for (int i = 0; i < preview.Count; i++)
        {
            SuspectRow row = preview[i];
            if (!row.applyEligible ||
                !row.changeNeeded ||
                row.deepStatus != ProductionLevelPlayability.Status.Valid ||
                !row.hintPathPass ||
                row.deepPlayerMoves <= 0)
            {
                continue;
            }

            if (row.dbIndex < 0 || row.dbIndex >= database.levels.Count)
            {
                continue;
            }

            LevelData level = database.levels[row.dbIndex];
            if (level == null)
            {
                continue;
            }

            if (level.minimumMoves == row.deepPlayerMoves)
            {
                continue;
            }

            Undo.RecordObject(level, "Apply Verified Deep Min Moves");
            level.minimumMoves = row.deepPlayerMoves;

            // Refresh score metadata from a fresh deep validate (no objective/layout change).
            ProductionLevelPlayability.Report fresh =
                ProductionLevelPlayability.Validate(level, row.dbIndex, DeepBudget);
            if (fresh.status == ProductionLevelPlayability.Status.Valid &&
                fresh.statesExplored > 0)
            {
                int vehicleCount = level.vehicles != null ? level.vehicles.Count : 0;
                level.statesExplored = fresh.statesExplored;
                level.difficultyScore =
                    row.deepPlayerMoves * 100 +
                    Mathf.RoundToInt(Mathf.Log10(fresh.statesExplored + 1) * 50f) +
                    vehicleCount * 10;
            }

            EditorUtility.SetDirty(level);
            written++;

            // Keep cache in sync for re-apply safety.
            row.storedMinMoves = row.deepPlayerMoves;
            row.changeNeeded = false;
            row.localNumber = LevelDifficultyOrder.GetDifficultyDisplayNumber(
                database,
                row.dbIndex
            );
        }

        if (written > 0)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }

        return written;
    }

    private static StringBuilder FormatPreviewReport(List<SuspectRow> rows)
    {
        StringBuilder sb = new StringBuilder(32 * 1024);
        sb.AppendLine("=== DEEP MIN MOVES — SUSPECT PREVIEW (no writes) ===");
        sb.AppendLine("NormalBudget=" + NormalBudget + " DeepBudget=" + DeepBudget);
        sb.AppendLine(
            "Asset | Diff | Local# | Obj | Stored | DeepMoves | DeepStatus | " +
            "HintPath | ChangeNeeded | SuspectReason"
        );

        int change = 0;
        int eligible = 0;
        int deepValid = 0;
        int deepTimeout = 0;
        int deepUnsolvable = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            SuspectRow r = rows[i];
            if (r.changeNeeded)
            {
                change++;
            }

            if (r.applyEligible)
            {
                eligible++;
            }

            if (r.deepStatus == ProductionLevelPlayability.Status.Valid)
            {
                deepValid++;
            }
            else if (r.deepStatus == ProductionLevelPlayability.Status.SolverTimeout)
            {
                deepTimeout++;
            }
            else if (r.deepStatus == ProductionLevelPlayability.Status.Unsolvable)
            {
                deepUnsolvable++;
            }

            string deepMoves = r.deepPlayerMoves > 0 ? r.deepPlayerMoves.ToString() : "n/a";
            sb.AppendLine(
                r.assetName + " | " + r.difficulty + " | " + r.localNumber +
                " | " + r.objective + " | " + r.storedMinMoves + " | " + deepMoves +
                " | " + r.deepStatus + " | " +
                (r.hintPathPass ? "PASS" : "FAIL") + " | " +
                r.changeNeeded + " | " + r.suspectReason
            );

            if (r.assetName == "Level_096")
            {
                sb.AppendLine(
                    "  >> Level_096 preview: " + r.storedMinMoves + " → " +
                    (r.deepPlayerMoves > 0 ? r.deepPlayerMoves.ToString() : "n/a") +
                    " (no write yet)"
                );
            }

            if (!string.IsNullOrEmpty(r.difficultyWarning))
            {
                sb.AppendLine("  DIFFICULTY WARNING: " + r.difficultyWarning);
            }

            if (r.deepStatus == ProductionLevelPlayability.Status.Valid &&
                !r.hintPathPass)
            {
                sb.AppendLine("  HintPathDetail=" + r.hintPathDetail);
            }
        }

        sb.AppendLine();
        sb.AppendLine("Suspects=" + rows.Count);
        sb.AppendLine("DeepValid=" + deepValid);
        sb.AppendLine("DeepTimeout=" + deepTimeout);
        sb.AppendLine("DeepUnsolvable=" + deepUnsolvable);
        sb.AppendLine("ChangeNeeded=" + change);
        sb.AppendLine("ApplyEligible=" + eligible);
        sb.AppendLine();
        sb.AppendLine(
            "WARNING after apply: Re-run Special Auto Tune recommendations " +
            "(Timed/MoveLimit use minMoves). Params are NOT auto-changed."
        );
        sb.AppendLine(
            "Ordering impact: LevelDifficultyOrder resorts by minMoves automatically " +
            "(LevelSelect, Gameplay display #, Next Level, Planner Disp#)."
        );
        sb.AppendLine("No layout / gameplay / objective writes.");
        return sb;
    }

    private static int CountApplyEligible(List<SuspectRow> rows)
    {
        int n = 0;
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].applyEligible)
            {
                n++;
            }
        }

        return n;
    }

    private static int CountChangeNeeded(List<SuspectRow> rows)
    {
        int n = 0;
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].changeNeeded)
            {
                n++;
            }
        }

        return n;
    }

    private static int CountDeepTimeout(List<SuspectRow> rows)
    {
        int n = 0;
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].deepStatus == ProductionLevelPlayability.Status.SolverTimeout)
            {
                n++;
            }
        }

        return n;
    }

    private static LevelDatabase LoadMainDb()
    {
        string[] guids = AssetDatabase.FindAssets("MainLevelDatabase t:LevelDatabase");
        if (guids == null || guids.Length == 0)
        {
            Debug.LogError("MainLevelDatabase not found.");
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<LevelDatabase>(
            AssetDatabase.GUIDToAssetPath(guids[0])
        );
    }
}

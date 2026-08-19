using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only V1 auto-curation: greedy diversity-aware selection → Final 150 + Reserves.
/// Never deletes assets. Never silently mutates MainLevelDatabase.
/// </summary>
public static class V1AutoCurator
{
    public const float NearDuplicateSoftThreshold = 0.72f;
    public const float NearDuplicateHardThreshold = 0.90f;
    public const float SimilarityPenaltyWeight = 0.55f;
    public const float GridQuotaBonus = 0.08f;
    public const float OrientationBalanceBonus = 0.04f;
    public const float MinAcceptBaseQuality = 0.22f;

    private sealed class Candidate
    {
        public LevelData level;
        public string assetPath;
        public string guid;
        public LevelFeatureSignature signature;
        public V1LevelQualityScorer.ScoreBreakdown score;
        public bool manualKeep;
        public V1CurationStatus previousStatus;
    }

    public static string AutoCurate(
        int reservePerDifficulty = V1CurationState.DefaultReservePerDifficulty,
        bool writeDifficultyOnFinals = true)
    {
        V1CurationState state = V1CurationState.LoadOrCreate();
        List<Candidate> pool = CollectCandidates(state, out int scanned);
        if (pool.Count == 0)
        {
            return "Auto Curate failed: no valid LevelData candidates under GeneratedLevels.";
        }

        // Preserve ManualKeep / Maybe / Rejected intent.
        HashSet<string> rejectedGuids = new HashSet<string>();
        Dictionary<string, LevelDifficulty> manualKeepDifficulty =
            new Dictionary<string, LevelDifficulty>();
        Dictionary<string, LevelDifficulty> maybeDifficulty =
            new Dictionary<string, LevelDifficulty>();

        for (int i = 0; i < state.entries.Count; i++)
        {
            V1CurationEntry e = state.entries[i];
            if (e == null || string.IsNullOrEmpty(e.assetGuid))
            {
                continue;
            }

            if (e.status == V1CurationStatus.Rejected)
            {
                rejectedGuids.Add(e.assetGuid);
            }
            else if (e.status == V1CurationStatus.ManualKeep)
            {
                manualKeepDifficulty[e.assetGuid] = e.assignedDifficulty;
            }
            else if (e.status == V1CurationStatus.Maybe)
            {
                maybeDifficulty[e.assetGuid] = e.assignedDifficulty;
            }
        }

        List<Candidate> eligible = new List<Candidate>();
        for (int i = 0; i < pool.Count; i++)
        {
            Candidate c = pool[i];
            if (rejectedGuids.Contains(c.guid))
            {
                continue;
            }

            if (manualKeepDifficulty.TryGetValue(c.guid, out LevelDifficulty keepDiff))
            {
                c.manualKeep = true;
                c.score.suggestedDifficulty = keepDiff;
            }
            else if (maybeDifficulty.TryGetValue(c.guid, out LevelDifficulty maybeDiff))
            {
                c.manualKeep = true; // seed into finals; status restored as Maybe below
                c.score.suggestedDifficulty = maybeDiff;
                c.previousStatus = V1CurationStatus.Maybe;
            }

            eligible.Add(c);
        }

        Dictionary<LevelDifficulty, List<Candidate>> selectedFinal =
            new Dictionary<LevelDifficulty, List<Candidate>>
            {
                { LevelDifficulty.Easy, new List<Candidate>() },
                { LevelDifficulty.Medium, new List<Candidate>() },
                { LevelDifficulty.Hard, new List<Candidate>() }
            };
        Dictionary<LevelDifficulty, List<Candidate>> selectedReserve =
            new Dictionary<LevelDifficulty, List<Candidate>>
            {
                { LevelDifficulty.Easy, new List<Candidate>() },
                { LevelDifficulty.Medium, new List<Candidate>() },
                { LevelDifficulty.Hard, new List<Candidate>() }
            };

        StringBuilder report = new StringBuilder();
        report.AppendLine("=== V1 Auto Curate ===");
        report.AppendLine("Scanned candidates: " + scanned);
        report.AppendLine("Eligible (excl. rejected): " + eligible.Count);

        // Seed ManualKeep into finals first.
        for (int i = 0; i < eligible.Count; i++)
        {
            Candidate c = eligible[i];
            if (!c.manualKeep)
            {
                continue;
            }

            LevelDifficulty d = c.score.suggestedDifficulty;
            if (selectedFinal[d].Count < TargetFor(d))
            {
                selectedFinal[d].Add(c);
            }
        }

        foreach (LevelDifficulty difficulty in new[]
                 {
                     LevelDifficulty.Easy,
                     LevelDifficulty.Medium,
                     LevelDifficulty.Hard
                 })
        {
            SelectForDifficulty(
                eligible,
                difficulty,
                selectedFinal,
                selectedReserve,
                TargetFor(difficulty),
                Mathf.Max(0, reservePerDifficulty),
                report
            );
        }

        // Rebuild state entries.
        HashSet<string> used = new HashSet<string>();
        List<V1CurationEntry> nextEntries = new List<V1CurationEntry>();

        void AddSelected(List<Candidate> list, V1CurationStatus status)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Candidate c = list[i];
                if (!used.Add(c.guid))
                {
                    continue;
                }

                V1CurationStatus resolvedStatus = status;
                if (status == V1CurationStatus.AutoSelectedFinal)
                {
                    if (c.previousStatus == V1CurationStatus.Maybe)
                    {
                        resolvedStatus = V1CurationStatus.Maybe;
                    }
                    else if (c.manualKeep)
                    {
                        resolvedStatus = V1CurationStatus.ManualKeep;
                    }
                }

                V1CurationEntry entry = BuildEntry(c, resolvedStatus, list, selectedFinal);
                // Preserve notes/reasons from prior curation when possible.
                V1CurationEntry prior = state.FindByLevel(c.level);
                if (prior != null)
                {
                    entry.playtestNote = prior.playtestNote;
                    entry.rejectReason = prior.rejectReason;
                }

                nextEntries.Add(entry);

                if (writeDifficultyOnFinals &&
                    (resolvedStatus == V1CurationStatus.AutoSelectedFinal ||
                     resolvedStatus == V1CurationStatus.ManualKeep ||
                     resolvedStatus == V1CurationStatus.Maybe))
                {
                    Undo.RecordObject(c.level, "V1 Auto Curate Difficulty");
                    c.level.difficulty = entry.assignedDifficulty;
                    EditorUtility.SetDirty(c.level);
                }
            }
        }

        AddSelected(selectedFinal[LevelDifficulty.Easy], V1CurationStatus.AutoSelectedFinal);
        AddSelected(selectedFinal[LevelDifficulty.Medium], V1CurationStatus.AutoSelectedFinal);
        AddSelected(selectedFinal[LevelDifficulty.Hard], V1CurationStatus.AutoSelectedFinal);
        // ManualKeep status already applied inside AddSelected.

        AddSelected(selectedReserve[LevelDifficulty.Easy], V1CurationStatus.Reserve);
        AddSelected(selectedReserve[LevelDifficulty.Medium], V1CurationStatus.Reserve);
        AddSelected(selectedReserve[LevelDifficulty.Hard], V1CurationStatus.Reserve);

        // Keep rejected entries.
        for (int i = 0; i < state.entries.Count; i++)
        {
            V1CurationEntry old = state.entries[i];
            if (old == null || old.status != V1CurationStatus.Rejected)
            {
                continue;
            }

            if (string.IsNullOrEmpty(old.assetGuid) || used.Contains(old.assetGuid))
            {
                continue;
            }

            used.Add(old.assetGuid);
            nextEntries.Add(old);
        }

        // Remaining eligible → Unassigned (not deleted).
        for (int i = 0; i < eligible.Count; i++)
        {
            Candidate c = eligible[i];
            if (used.Contains(c.guid))
            {
                continue;
            }

            V1CurationEntry entry = BuildEntry(
                c,
                V1CurationStatus.Unassigned,
                selectedFinal[c.score.suggestedDifficulty],
                selectedFinal
            );
            nextEntries.Add(entry);
            used.Add(c.guid);
        }

        MarkNeedsReview(nextEntries, selectedFinal, report);

        state.entries = nextEntries;
        state.lastCurateUtc = DateTime.UtcNow.ToString("u");
        AppendQuotaReport(report, selectedFinal, selectedReserve);
        state.lastReport = report.ToString();
        state.MarkDirtyAndSave();
        AssetDatabase.SaveAssets();

        Debug.Log(state.lastReport);
        return state.lastReport;
    }

    private static void SelectForDifficulty(
        List<Candidate> eligible,
        LevelDifficulty difficulty,
        Dictionary<LevelDifficulty, List<Candidate>> selectedFinal,
        Dictionary<LevelDifficulty, List<Candidate>> selectedReserve,
        int finalTarget,
        int reserveTarget,
        StringBuilder report)
    {
        List<Candidate> already = selectedFinal[difficulty];
        List<Candidate> pool = new List<Candidate>();
        for (int i = 0; i < eligible.Count; i++)
        {
            Candidate c = eligible[i];
            if (c.manualKeep && already.Contains(c))
            {
                continue;
            }

            if (IsUsedInAny(c, selectedFinal) || IsUsedInAny(c, selectedReserve))
            {
                continue;
            }

            // Prefer matching suggested tier; allow soft neighbors only if shortage.
            if (c.score.suggestedDifficulty == difficulty)
            {
                pool.Add(c);
            }
        }

        // Soft fill from neighbors if short.
        if (pool.Count + already.Count < finalTarget + reserveTarget)
        {
            for (int i = 0; i < eligible.Count; i++)
            {
                Candidate c = eligible[i];
                if (pool.Contains(c) || already.Contains(c))
                {
                    continue;
                }

                if (IsUsedInAny(c, selectedFinal) || IsUsedInAny(c, selectedReserve))
                {
                    continue;
                }

                if (Mathf.Abs((int)c.score.suggestedDifficulty - (int)difficulty) == 1 &&
                    c.score.baseQuality >= MinAcceptBaseQuality)
                {
                    pool.Add(c);
                }
            }
        }

        pool.Sort(CompareCandidatesStable);

        while (already.Count < finalTarget)
        {
            Candidate best = PickNext(pool, already, difficulty, true);
            if (best == null)
            {
                report.AppendLine(
                    "SHORTAGE Final " + difficulty + ": have " + already.Count +
                    " / target " + finalTarget
                );
                break;
            }

            if (best.score.baseQuality < MinAcceptBaseQuality &&
                !best.manualKeep)
            {
                pool.Remove(best);
                continue;
            }

            already.Add(best);
            pool.Remove(best);
        }

        List<Candidate> reserves = selectedReserve[difficulty];
        while (reserves.Count < reserveTarget)
        {
            Candidate best = PickNext(pool, already, difficulty, false);
            if (best == null)
            {
                report.AppendLine(
                    "SHORTAGE Reserve " + difficulty + ": have " + reserves.Count +
                    " / target " + reserveTarget
                );
                break;
            }

            reserves.Add(best);
            pool.Remove(best);
        }
    }

    private static Candidate PickNext(
        List<Candidate> pool,
        List<Candidate> alreadySelected,
        LevelDifficulty difficulty,
        bool forFinal)
    {
        Candidate best = null;
        float bestScore = float.NegativeInfinity;
        string bestTie = null;

        int[,] gridNeed = BuildRemainingGridNeed(alreadySelected, difficulty);

        for (int i = 0; i < pool.Count; i++)
        {
            Candidate c = pool[i];
            float nearDup = MaxSimilarityTo(c.signature, alreadySelected);
            if (nearDup >= NearDuplicateHardThreshold)
            {
                continue;
            }

            float fit = V1LevelQualityScorer.DifficultyFit(c.signature, difficulty);
            float effective =
                c.score.baseQuality * 0.65f +
                fit * 0.25f -
                nearDup * SimilarityPenaltyWeight;

            if (forFinal)
            {
                effective += GridNeedBonus(c.signature, gridNeed);
                effective += OrientationBonus(c.signature, alreadySelected);
            }

            string tie = c.guid ?? c.assetPath ?? string.Empty;
            if (effective > bestScore + 0.00001f ||
                (Mathf.Abs(effective - bestScore) <= 0.00001f &&
                 (bestTie == null || string.CompareOrdinal(tie, bestTie) < 0)))
            {
                bestScore = effective;
                best = c;
                bestTie = tie;
            }
        }

        if (best != null)
        {
            best.score.notes = "nearDupMax=" + MaxSimilarityTo(best.signature, alreadySelected)
                .ToString("0.00");
        }

        return best;
    }

    private static float MaxSimilarityTo(
        LevelFeatureSignature signature,
        List<Candidate> selected)
    {
        float max = 0f;
        for (int i = 0; i < selected.Count; i++)
        {
            float sim = LevelDiversityMetrics.Similarity(signature, selected[i].signature);
            if (sim > max)
            {
                max = sim;
            }
        }

        return max;
    }

    private static int[,] BuildRemainingGridNeed(
        List<Candidate> already,
        LevelDifficulty difficulty)
    {
        // [ExactGrids index] remaining target count
        int n = V1ReleaseContentPlan.ExactGrids.Length;
        int[] have = new int[n];
        for (int i = 0; i < already.Count; i++)
        {
            int idx = IndexOfGrid(
                already[i].signature.gridWidth,
                already[i].signature.gridHeight
            );
            if (idx >= 0)
            {
                have[idx]++;
            }
        }

        int[,] need = new int[n, 1];
        for (int i = 0; i < n; i++)
        {
            Vector2Int g = V1ReleaseContentPlan.ExactGrids[i];
            int target = V1ReleaseContentPlan.GetTarget(difficulty, g.x, g.y);
            need[i, 0] = Mathf.Max(0, target - have[i]);
        }

        return need;
    }

    private static float GridNeedBonus(LevelFeatureSignature s, int[,] need)
    {
        int idx = IndexOfGrid(s.gridWidth, s.gridHeight);
        if (idx < 0)
        {
            return 0f;
        }

        return need[idx, 0] > 0 ? GridQuotaBonus : 0f;
    }

    private static float OrientationBonus(
        LevelFeatureSignature s,
        List<Candidate> already)
    {
        // Soft: prefer underrepresented orientation in paired groups.
        bool isWide = s.gridWidth > s.gridHeight;
        bool isTall = s.gridHeight > s.gridWidth;
        if (!isWide && !isTall)
        {
            return 0f;
        }

        int wide = 0;
        int tall = 0;
        for (int i = 0; i < already.Count; i++)
        {
            LevelFeatureSignature o = already[i].signature;
            if (o.gridWidth > o.gridHeight)
            {
                wide++;
            }
            else if (o.gridHeight > o.gridWidth)
            {
                tall++;
            }
        }

        if (isWide && wide <= tall)
        {
            return OrientationBalanceBonus;
        }

        if (isTall && tall <= wide)
        {
            return OrientationBalanceBonus;
        }

        return 0f;
    }

    private static int IndexOfGrid(int w, int h)
    {
        for (int i = 0; i < V1ReleaseContentPlan.ExactGrids.Length; i++)
        {
            Vector2Int g = V1ReleaseContentPlan.ExactGrids[i];
            if (g.x == w && g.y == h)
            {
                return i;
            }
        }

        return -1;
    }

    private static bool IsUsedInAny(
        Candidate c,
        Dictionary<LevelDifficulty, List<Candidate>> map)
    {
        foreach (KeyValuePair<LevelDifficulty, List<Candidate>> pair in map)
        {
            if (pair.Value.Contains(c))
            {
                return true;
            }
        }

        return false;
    }

    private static int TargetFor(LevelDifficulty d)
    {
        switch (d)
        {
            case LevelDifficulty.Easy:
                return V1ReleaseContentPlan.TargetEasy;
            case LevelDifficulty.Medium:
                return V1ReleaseContentPlan.TargetMedium;
            default:
                return V1ReleaseContentPlan.TargetHard;
        }
    }

    private static int CompareCandidatesStable(Candidate a, Candidate b)
    {
        int q = b.score.baseQuality.CompareTo(a.score.baseQuality);
        if (q != 0)
        {
            return q;
        }

        int s = b.signature.difficultyScore.CompareTo(a.signature.difficultyScore);
        if (s != 0)
        {
            return s;
        }

        return string.CompareOrdinal(a.guid, b.guid);
    }

    private static V1CurationEntry BuildEntry(
        Candidate c,
        V1CurationStatus status,
        List<Candidate> peersForDup,
        Dictionary<LevelDifficulty, List<Candidate>> allFinals)
    {
        float nearDup = 0f;
        if (status == V1CurationStatus.AutoSelectedFinal ||
            status == V1CurationStatus.ManualKeep ||
            status == V1CurationStatus.Maybe ||
            status == V1CurationStatus.Reserve)
        {
            // Compare against all finals of same difficulty.
            List<Candidate> sameTier = allFinals[c.score.suggestedDifficulty];
            nearDup = MaxSimilarityTo(c.signature, sameTier);
        }

        float effective =
            c.score.baseQuality -
            nearDup * SimilarityPenaltyWeight;

        return new V1CurationEntry
        {
            level = c.level,
            assetGuid = c.guid,
            status = status,
            assignedDifficulty = c.score.suggestedDifficulty,
            baseQuality = c.score.baseQuality,
            difficultyFit = c.score.difficultyFit,
            trivialityPenalty = c.score.trivialityPenalty,
            nearDuplicatePenalty = nearDup,
            effectiveScore = effective,
            needsReview = false,
            reviewReason = string.Empty,
            suggestedDifficulty = c.score.suggestedDifficulty.ToString()
        };
    }

    private static void MarkNeedsReview(
        List<V1CurationEntry> entries,
        Dictionary<LevelDifficulty, List<Candidate>> finals,
        StringBuilder report)
    {
        Dictionary<string, LevelFeatureSignature> sigByGuid =
            new Dictionary<string, LevelFeatureSignature>();

        foreach (KeyValuePair<LevelDifficulty, List<Candidate>> pair in finals)
        {
            for (int i = 0; i < pair.Value.Count; i++)
            {
                Candidate c = pair.Value[i];
                sigByGuid[c.guid] = c.signature;
            }
        }

        for (int i = 0; i < entries.Count; i++)
        {
            V1CurationEntry e = entries[i];
            if (e == null || e.level == null)
            {
                continue;
            }

            if (e.status != V1CurationStatus.AutoSelectedFinal &&
                e.status != V1CurationStatus.ManualKeep &&
                e.status != V1CurationStatus.Maybe)
            {
                continue;
            }

            List<string> reasons = new List<string>();
            if (e.nearDuplicatePenalty >= V1CurationState.HighSimilarityThreshold)
            {
                reasons.Add("high similarity " + e.nearDuplicatePenalty.ToString("0.00"));
            }

            if (e.difficultyFit < 0.6f)
            {
                reasons.Add("borderline difficulty fit");
            }

            if (e.trivialityPenalty >= 0.35f)
            {
                reasons.Add("triviality risk");
            }

            if (e.level.minimumMoves <= 0 || e.level.difficultyScore < 0)
            {
                reasons.Add("validation warning");
            }

            if (reasons.Count > 0)
            {
                e.needsReview = true;
                e.reviewReason = string.Join("; ", reasons);
            }
        }

        // Closest-pair warnings per difficulty.
        foreach (KeyValuePair<LevelDifficulty, List<Candidate>> pair in finals)
        {
            List<Candidate> list = pair.Value;
            float bestSim = 0f;
            string aName = "-";
            string bName = "-";
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    float sim = LevelDiversityMetrics.Similarity(
                        list[i].signature,
                        list[j].signature
                    );
                    if (sim > bestSim)
                    {
                        bestSim = sim;
                        aName = list[i].level != null ? list[i].level.name : "?";
                        bName = list[j].level != null ? list[j].level.name : "?";
                    }
                }
            }

            report.AppendLine(
                pair.Key + " closest pair similarity=" + bestSim.ToString("0.00") +
                " (" + aName + " vs " + bName + ")"
            );
        }
    }

    private static void AppendQuotaReport(
        StringBuilder report,
        Dictionary<LevelDifficulty, List<Candidate>> finals,
        Dictionary<LevelDifficulty, List<Candidate>> reserves)
    {
        report.AppendLine();
        report.AppendLine("FINAL COUNTS:");
        report.AppendLine(
            "Easy " + finals[LevelDifficulty.Easy].Count + "/" + V1ReleaseContentPlan.TargetEasy
        );
        report.AppendLine(
            "Medium " + finals[LevelDifficulty.Medium].Count + "/" +
            V1ReleaseContentPlan.TargetMedium
        );
        report.AppendLine(
            "Hard " + finals[LevelDifficulty.Hard].Count + "/" + V1ReleaseContentPlan.TargetHard
        );
        report.AppendLine(
            "RESERVES: E" + reserves[LevelDifficulty.Easy].Count +
            " M" + reserves[LevelDifficulty.Medium].Count +
            " H" + reserves[LevelDifficulty.Hard].Count
        );

        report.AppendLine();
        report.AppendLine("GRID DRIFT (final vs target):");
        foreach (LevelDifficulty d in new[]
                 {
                     LevelDifficulty.Easy, LevelDifficulty.Medium, LevelDifficulty.Hard
                 })
        {
            for (int i = 0; i < V1ReleaseContentPlan.ExactGrids.Length; i++)
            {
                Vector2Int g = V1ReleaseContentPlan.ExactGrids[i];
                int target = V1ReleaseContentPlan.GetTarget(d, g.x, g.y);
                int have = 0;
                List<Candidate> list = finals[d];
                for (int c = 0; c < list.Count; c++)
                {
                    if (list[c].signature.gridWidth == g.x &&
                        list[c].signature.gridHeight == g.y)
                    {
                        have++;
                    }
                }

                if (target == 0 && have == 0)
                {
                    continue;
                }

                if (have != target)
                {
                    report.AppendLine(
                        d + " " + g.x + "x" + g.y + ": " + have + " / target " + target
                    );
                }
            }
        }
    }

    private static List<Candidate> CollectCandidates(
        V1CurationState state,
        out int scanned)
    {
        scanned = 0;
        List<Candidate> result = new List<Candidate>();
        string[] guids = AssetDatabase.FindAssets(
            "t:LevelData",
            new[] { LevelCanonicalKey.GeneratedLevelsRoot }
        );
        if (guids == null)
        {
            return result;
        }

        HashSet<string> exactKeys = new HashSet<string>(StringComparer.Ordinal);

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            scanned++;
            if (level == null)
            {
                continue;
            }

            if (!V1ReleaseContentPlan.IsAllowedDimension(
                    level.ResolvedGridWidth,
                    level.ResolvedGridHeight))
            {
                continue;
            }

            LevelFeatureSignature sig = LevelFeatureSignature.Extract(level);
            if (!sig.valid)
            {
                continue;
            }

            string layoutKey = LevelCanonicalKey.BuildCanonicalLevelKey(level);
            if (!string.IsNullOrEmpty(layoutKey) && !exactKeys.Add(layoutKey))
            {
                // Exact duplicate layout: skip later copies.
                continue;
            }

            V1LevelQualityScorer.ScoreBreakdown score = V1LevelQualityScorer.Evaluate(level, sig);
            result.Add(new Candidate
            {
                level = level,
                assetPath = path,
                guid = guids[i],
                signature = sig,
                score = score,
                manualKeep = false,
                previousStatus = V1CurationStatus.Unassigned
            });
        }

        return result;
    }

    /// <summary>
    /// Replace rejected finals with best matching reserves. No asset deletes.
    /// </summary>
    public static string ReplaceRejected()
    {
        V1CurationState state = V1CurationState.LoadOrCreate();
        List<V1CurationEntry> rejectedFinals = new List<V1CurationEntry>();
        List<V1CurationEntry> reserves = new List<V1CurationEntry>();
        List<V1CurationEntry> activeFinals = new List<V1CurationEntry>();

        for (int i = 0; i < state.entries.Count; i++)
        {
            V1CurationEntry e = state.entries[i];
            if (e == null)
            {
                continue;
            }

            if (e.status == V1CurationStatus.Rejected && e.level != null)
            {
                // Only replace if it was previously a final (heuristic: assignedDifficulty set).
                rejectedFinals.Add(e);
            }
            else if (e.status == V1CurationStatus.Reserve)
            {
                reserves.Add(e);
            }
            else if (e.status == V1CurationStatus.AutoSelectedFinal ||
                     e.status == V1CurationStatus.ManualKeep ||
                     e.status == V1CurationStatus.Maybe)
            {
                activeFinals.Add(e);
            }
        }

        // Re-identify rejected that need replacement: user rejects a final → status Rejected.
        // We treat all Rejected with a level as needing a reserve replacement if finals short.
        int replaced = 0;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== Replace Rejected ===");

        foreach (LevelDifficulty difficulty in new[]
                 {
                     LevelDifficulty.Easy, LevelDifficulty.Medium, LevelDifficulty.Hard
                 })
        {
            int need = TargetFor(difficulty) - CountFinal(activeFinals, difficulty);
            if (need <= 0)
            {
                continue;
            }

            List<V1CurationEntry> reservePool = new List<V1CurationEntry>();
            for (int i = 0; i < reserves.Count; i++)
            {
                if (reserves[i].assignedDifficulty == difficulty &&
                    reserves[i].status == V1CurationStatus.Reserve)
                {
                    reservePool.Add(reserves[i]);
                }
            }

            reservePool.Sort((a, b) =>
            {
                int q = b.effectiveScore.CompareTo(a.effectiveScore);
                if (q != 0)
                {
                    return q;
                }

                return string.CompareOrdinal(a.assetGuid, b.assetGuid);
            });

            for (int n = 0; n < need && reservePool.Count > 0; n++)
            {
                V1CurationEntry pick = PickBestReserve(reservePool, activeFinals, difficulty);
                if (pick == null)
                {
                    break;
                }

                pick.status = V1CurationStatus.AutoSelectedFinal;
                if (pick.level != null)
                {
                    Undo.RecordObject(pick.level, "V1 Replace Rejected");
                    pick.level.difficulty = difficulty;
                    pick.assignedDifficulty = difficulty;
                    EditorUtility.SetDirty(pick.level);
                }

                activeFinals.Add(pick);
                reservePool.Remove(pick);
                reserves.Remove(pick);
                replaced++;
                sb.AppendLine(
                    "Promoted reserve → Final " + difficulty + ": " +
                    (pick.level != null ? pick.level.name : pick.assetGuid)
                );
            }

            if (need > 0 && CountFinal(activeFinals, difficulty) < TargetFor(difficulty))
            {
                sb.AppendLine(
                    "WARNING: still short on " + difficulty + " after replace (" +
                    CountFinal(activeFinals, difficulty) + "/" + TargetFor(difficulty) + ")"
                );
            }
        }

        state.MarkDirtyAndSave();
        AssetDatabase.SaveAssets();
        sb.AppendLine("Replaced/promoted: " + replaced);
        Debug.Log(sb.ToString());
        return sb.ToString();
    }

    private static int CountFinal(List<V1CurationEntry> finals, LevelDifficulty d)
    {
        int c = 0;
        for (int i = 0; i < finals.Count; i++)
        {
            if (finals[i].assignedDifficulty == d)
            {
                c++;
            }
        }

        return c;
    }

    private static V1CurationEntry PickBestReserve(
        List<V1CurationEntry> reservePool,
        List<V1CurationEntry> activeFinals,
        LevelDifficulty difficulty)
    {
        V1CurationEntry best = null;
        float bestScore = float.NegativeInfinity;
        string bestTie = null;

        List<LevelFeatureSignature> finalSigs = new List<LevelFeatureSignature>();
        for (int i = 0; i < activeFinals.Count; i++)
        {
            if (activeFinals[i].level != null)
            {
                finalSigs.Add(LevelFeatureSignature.Extract(activeFinals[i].level));
            }
        }

        for (int i = 0; i < reservePool.Count; i++)
        {
            V1CurationEntry e = reservePool[i];
            if (e.level == null)
            {
                continue;
            }

            LevelFeatureSignature sig = LevelFeatureSignature.Extract(e.level);
            float maxSim = 0f;
            for (int s = 0; s < finalSigs.Count; s++)
            {
                maxSim = Mathf.Max(maxSim, LevelDiversityMetrics.Similarity(sig, finalSigs[s]));
            }

            if (maxSim >= NearDuplicateHardThreshold)
            {
                continue;
            }

            float gridBonus = 0f;
            int have = 0;
            int target = V1ReleaseContentPlan.GetTarget(
                difficulty,
                sig.gridWidth,
                sig.gridHeight
            );
            for (int f = 0; f < activeFinals.Count; f++)
            {
                if (activeFinals[f].assignedDifficulty != difficulty ||
                    activeFinals[f].level == null)
                {
                    continue;
                }

                if (activeFinals[f].level.ResolvedGridWidth == sig.gridWidth &&
                    activeFinals[f].level.ResolvedGridHeight == sig.gridHeight)
                {
                    have++;
                }
            }

            if (have < target)
            {
                gridBonus = GridQuotaBonus;
            }

            float effective = e.baseQuality - maxSim * SimilarityPenaltyWeight + gridBonus;
            string tie = e.assetGuid ?? string.Empty;
            if (effective > bestScore + 0.00001f ||
                (Mathf.Abs(effective - bestScore) <= 0.00001f &&
                 (bestTie == null || string.CompareOrdinal(tie, bestTie) < 0)))
            {
                best = e;
                bestScore = effective;
                bestTie = tie;
            }
        }

        return best;
    }

    public static string ApplyFinalToMainLevelDatabase()
    {
        V1CurationState state = V1CurationState.LoadOrCreate();
        List<V1CurationEntry> finals = state.GetFinalEntries();
        if (finals.Count == 0)
        {
            return "No final entries to apply.";
        }

        finals.Sort((a, b) =>
        {
            int d = a.assignedDifficulty.CompareTo(b.assignedDifficulty);
            if (d != 0)
            {
                return d;
            }

            LevelData la = a.level;
            LevelData lb = b.level;
            int areaA = la != null ? la.ResolvedGridWidth * la.ResolvedGridHeight : 0;
            int areaB = lb != null ? lb.ResolvedGridWidth * lb.ResolvedGridHeight : 0;
            int areaCmp = areaA.CompareTo(areaB);
            if (areaCmp != 0)
            {
                return areaCmp;
            }

            int moves = (la != null ? la.minimumMoves : 0).CompareTo(
                lb != null ? lb.minimumMoves : 0
            );
            if (moves != 0)
            {
                return moves;
            }

            return string.CompareOrdinal(a.assetGuid, b.assetGuid);
        });

        string[] dbGuids = AssetDatabase.FindAssets("MainLevelDatabase t:LevelDatabase");
        if (dbGuids == null || dbGuids.Length == 0)
        {
            return "MainLevelDatabase not found.";
        }

        string dbPath = AssetDatabase.GUIDToAssetPath(dbGuids[0]);
        LevelDatabase database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(dbPath);
        if (database == null)
        {
            return "Failed to load MainLevelDatabase.";
        }

        bool ok = EditorUtility.DisplayDialog(
            "Apply Final Set To MainLevelDatabase",
            "This REPLACES MainLevelDatabase.levels with " + finals.Count +
            " curated Final/ManualKeep levels.\n\n" +
            "Order: Easy → Medium → Hard, then area → minMoves.\n" +
            "WARNING: Save identity is database-index based. " +
            "Existing player progress indices will no longer match old content.\n\n" +
            "PRE-RELEASE only. Continue?",
            "Replace Database List",
            "Cancel"
        );
        if (!ok)
        {
            return "Apply cancelled.";
        }

        Undo.RecordObject(database, "Apply V1 Final Set");
        database.levels = new List<LevelData>();
        for (int i = 0; i < finals.Count; i++)
        {
            LevelData level = finals[i].level;
            if (level == null)
            {
                continue;
            }

            Undo.RecordObject(level, "Apply V1 Final Set LevelNumber");
            level.levelNumber = database.levels.Count + 1;
            level.difficulty = finals[i].assignedDifficulty;
            EditorUtility.SetDirty(level);
            database.levels.Add(level);
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();

        string msg =
            "Applied " + database.levels.Count +
            " finals to MainLevelDatabase (order Easy/Medium/Hard).\n" +
            "Save indices now refer to this new list.";
        Debug.Log(msg);
        return msg;
    }

    public static bool RejectLevel(LevelData level, V1RejectReason reason = V1RejectReason.None)
    {
        if (level == null)
        {
            return false;
        }

        V1CurationState state = V1CurationState.LoadOrCreate();
        V1CurationEntry entry = state.FindByLevel(level);
        if (entry == null)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(level));
            entry = new V1CurationEntry
            {
                level = level,
                assetGuid = guid,
                assignedDifficulty = level.difficulty,
                status = V1CurationStatus.Rejected,
                rejectReason = reason
            };
            state.entries.Add(entry);
        }
        else
        {
            entry.status = V1CurationStatus.Rejected;
            entry.rejectReason = reason;
        }

        state.MarkDirtyAndSave();
        return true;
    }

    public static bool MarkManualKeep(LevelData level)
    {
        if (level == null)
        {
            return false;
        }

        V1CurationState state = V1CurationState.LoadOrCreate();
        V1CurationEntry entry = state.FindByLevel(level);
        if (entry == null)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(level));
            entry = new V1CurationEntry
            {
                level = level,
                assetGuid = guid,
                assignedDifficulty = level.difficulty,
                status = V1CurationStatus.ManualKeep
            };
            state.entries.Add(entry);
        }
        else
        {
            entry.status = V1CurationStatus.ManualKeep;
            entry.assignedDifficulty = level.difficulty;
        }

        state.MarkDirtyAndSave();
        return true;
    }

    public static bool MarkMaybe(LevelData level)
    {
        if (level == null)
        {
            return false;
        }

        V1CurationState state = V1CurationState.LoadOrCreate();
        V1CurationEntry entry = state.FindByLevel(level);
        if (entry == null)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(level));
            entry = new V1CurationEntry
            {
                level = level,
                assetGuid = guid,
                assignedDifficulty = level.difficulty,
                status = V1CurationStatus.Maybe
            };
            state.entries.Add(entry);
        }
        else
        {
            entry.status = V1CurationStatus.Maybe;
            entry.assignedDifficulty = level.difficulty;
        }

        state.MarkDirtyAndSave();
        return true;
    }

    public static bool SetPlaytestNote(LevelData level, string note)
    {
        if (level == null)
        {
            return false;
        }

        V1CurationState state = V1CurationState.LoadOrCreate();
        V1CurationEntry entry = state.FindByLevel(level);
        if (entry == null)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(level));
            entry = new V1CurationEntry
            {
                level = level,
                assetGuid = guid,
                assignedDifficulty = level.difficulty,
                playtestNote = note ?? string.Empty
            };
            state.entries.Add(entry);
        }
        else
        {
            entry.playtestNote = note ?? string.Empty;
        }

        state.MarkDirtyAndSave();
        return true;
    }
}

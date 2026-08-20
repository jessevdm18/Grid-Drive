using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Compact Editor-only playtest toolbar for V1 Final curation.
/// Menu: RushOut → Levels → V1 Playtest
/// </summary>
public class V1PlaytestWindow : EditorWindow
{
    private enum DifficultyPlayFilter
    {
        All = 0,
        Easy = 1,
        Medium = 2,
        Hard = 3
    }

    private V1CurationState state;
    private Vector2 scroll;
    private bool autoNextAfterRating = true;
    private bool wrapPlaytest;
    private DifficultyPlayFilter difficultyFilter = DifficultyPlayFilter.All;
    private V1RejectReason pendingRejectReason = V1RejectReason.None;
    private string noteDraft = string.Empty;
    private string statusMessage = string.Empty;
    private V1PlaytestSession.ResolvedLevel cachedResolved;
    private double nextRefreshTime;
    private string lastNoteLevelGuid = string.Empty;

    [MenuItem("RushOut/Levels/V1 Playtest")]
    public static void OpenWindow()
    {
        V1PlaytestWindow window = GetWindow<V1PlaytestWindow>("V1 Playtest");
        window.minSize = new Vector2(320f, 420f);
        window.Show();
    }

    private void OnEnable()
    {
        minSize = new Vector2(320f, 420f);
        state = V1CurationState.LoadOrCreate();
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        RefreshResolved(force: true);
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    }

    private void OnPlayModeChanged(PlayModeStateChange change)
    {
        RefreshResolved(force: true);
        Repaint();
    }

    private void OnFocus()
    {
        RefreshResolved(force: true);
    }

    private void OnGUI()
    {
        if (state == null)
        {
            state = V1CurationState.LoadOrCreate();
        }

        if (EditorApplication.timeSinceStartup > nextRefreshTime)
        {
            RefreshResolved(force: false);
            nextRefreshTime = EditorApplication.timeSinceStartup + 0.5;
        }

        HandleShortcuts();

        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("V1 PLAYTEST", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "KEEP / REJECT / MAYBE / NEXT — Editor-only.\n" +
            "Loads LevelData via direct override (Main DB not required).\n" +
            "Shortcuts when this window is focused:\n" +
            "K Keep · R Reject · M Maybe · N Next Unreviewed",
            MessageType.Info
        );

        DrawCounts();
        EditorGUILayout.Space(6f);
        DrawCurrentLevel();
        EditorGUILayout.Space(6f);
        DrawOptions();
        EditorGUILayout.Space(6f);
        DrawActions();
        EditorGUILayout.Space(6f);
        DrawUtility();

        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage, MessageType.None);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawCounts()
    {
        int finalCount =
            state.CountFinalDifficulty(LevelDifficulty.Easy) +
            state.CountFinalDifficulty(LevelDifficulty.Medium) +
            state.CountFinalDifficulty(LevelDifficulty.Hard);
        int keep = state.CountStatus(V1CurationStatus.ManualKeep);
        int maybe = state.CountStatus(V1CurationStatus.Maybe);
        int rejected = state.CountStatus(V1CurationStatus.Rejected);
        int unreviewed = state.CountUnreviewedFinals();
        int reserves = state.CountStatus(V1CurationStatus.Reserve);
        int reviewed = state.CountReviewedFinals();

        EditorGUILayout.LabelField(
            "Final: " + finalCount +
            "  Keep: " + keep +
            "  Maybe: " + maybe +
            "  Rejected: " + rejected,
            EditorStyles.miniLabel
        );
        EditorGUILayout.LabelField(
            "Unreviewed: " + unreviewed +
            "  Reserves: " + reserves,
            EditorStyles.miniLabel
        );
        EditorGUILayout.LabelField(
            "Easy reviewed: " + state.CountReviewedForDifficulty(LevelDifficulty.Easy) + "/50  " +
            "Med: " + state.CountReviewedForDifficulty(LevelDifficulty.Medium) + "/50  " +
            "Hard: " + state.CountReviewedForDifficulty(LevelDifficulty.Hard) + "/50",
            EditorStyles.miniLabel
        );

        float progress = V1ReleaseContentPlan.TargetTotal > 0
            ? Mathf.Clamp01(reviewed / (float)V1ReleaseContentPlan.TargetTotal)
            : 0f;
        EditorGUI.ProgressBar(
            EditorGUILayout.GetControlRect(false, 18f),
            progress,
            "Reviewed Final: " + reviewed + " / " + V1ReleaseContentPlan.TargetTotal
        );
    }

    private void DrawCurrentLevel()
    {
        EditorGUILayout.LabelField("CURRENT PLAYTEST LEVEL", EditorStyles.boldLabel);

        if (!cachedResolved.ok || cachedResolved.level == null)
        {
            EditorGUILayout.HelpBox(
                string.IsNullOrEmpty(cachedResolved.message)
                    ? "No active Gameplay level"
                    : cachedResolved.message,
                MessageType.Warning
            );
            return;
        }

        LevelData level = cachedResolved.level;
        V1CurationEntry entry = cachedResolved.curationEntry;

        EditorGUILayout.LabelField(level.name, EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Source: " +
            (cachedResolved.isDirectOverride
                ? "DIRECT CANDIDATE OVERRIDE"
                : "MAIN DATABASE")
        );
        EditorGUILayout.LabelField(
            "DB Index: " +
            (cachedResolved.dbIndex >= 0
                ? cachedResolved.dbIndex.ToString()
                : "Not in Main DB")
        );
        string displayLabel = level.levelNumber.ToString();
        if (cachedResolved.dbIndex >= 0)
        {
            LevelDatabase mainDb = V1PlaytestSession.LoadMainDatabase();
            int local = LevelDifficultyOrder.GetDifficultyDisplayNumber(
                mainDb,
                cachedResolved.dbIndex
            );
            if (local > 0)
            {
                displayLabel = local.ToString();
            }
        }

        EditorGUILayout.LabelField("Display #: " + displayLabel);
        EditorGUILayout.LabelField(
            "Difficulty: " +
            (entry != null ? entry.assignedDifficulty.ToString() : level.difficulty.ToString())
        );
        EditorGUILayout.LabelField("Objective: " + level.objectiveType);
        EditorGUILayout.LabelField(
            "Grid: " + level.ResolvedGridWidth + "x" + level.ResolvedGridHeight
        );
        EditorGUILayout.LabelField("Min Moves: " + level.minimumMoves);
        EditorGUILayout.LabelField("Score: " + level.difficultyScore);

        if (entry == null)
        {
            EditorGUILayout.HelpBox(
                "Not in V1CurationState — rating will create an entry.",
                MessageType.Warning
            );
        }
        else
        {
            EditorGUILayout.LabelField("Status: " + entry.status);
            if (entry.rejectReason != V1RejectReason.None)
            {
                EditorGUILayout.LabelField("Reject Reason: " + entry.rejectReason);
            }
        }

        EditorGUI.BeginChangeCheck();
        noteDraft = EditorGUILayout.TextField("Playtest Note", noteDraft);
        if (EditorGUI.EndChangeCheck() && level != null)
        {
            V1AutoCurator.SetPlaytestNote(level, noteDraft);
            state = V1CurationState.LoadOrCreate();
            RefreshResolved(force: true);
        }

        pendingRejectReason = (V1RejectReason)EditorGUILayout.EnumPopup(
            "Reject Reason",
            pendingRejectReason
        );
    }

    private void DrawOptions()
    {
        EditorGUILayout.LabelField("OPTIONS", EditorStyles.boldLabel);
        autoNextAfterRating = EditorGUILayout.ToggleLeft(
            "Auto Next After Rating",
            autoNextAfterRating
        );
        wrapPlaytest = EditorGUILayout.ToggleLeft("Wrap Playtest", wrapPlaytest);
        difficultyFilter = (DifficultyPlayFilter)EditorGUILayout.EnumPopup(
            "Playtest Difficulty",
            difficultyFilter
        );
    }

    private void DrawActions()
    {
        EditorGUILayout.LabelField("ACTIONS", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.55f, 0.9f, 0.55f);
        if (GUILayout.Button("KEEP (K)", GUILayout.Height(32f)))
        {
            RateKeep();
        }

        GUI.backgroundColor = new Color(1f, 0.55f, 0.45f);
        if (GUILayout.Button("REJECT (R)", GUILayout.Height(32f)))
        {
            RateReject();
        }

        GUI.backgroundColor = new Color(1f, 0.85f, 0.4f);
        if (GUILayout.Button("MAYBE (M)", GUILayout.Height(32f)))
        {
            RateMaybe();
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("NEXT UNREVIEWED (N)", GUILayout.Height(28f)))
        {
            GoNextUnreviewed();
        }

        if (GUILayout.Button("NEXT FINAL", GUILayout.Height(28f)))
        {
            GoNextFinal(unreviewedOnly: false);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("PREVIOUS", GUILayout.Height(24f)))
        {
            GoPreviousFinal();
        }

        if (GUILayout.Button("RESTART CURRENT", GUILayout.Height(24f)))
        {
            V1PlaytestSession.RestartCurrentIfPlaying();
            statusMessage = "Restart requested.";
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawUtility()
    {
        EditorGUILayout.LabelField("UTILITY", EditorStyles.boldLabel);

        if (GUILayout.Button("REPLACE REJECTED NOW", GUILayout.Height(26f)))
        {
            string report = V1AutoCurator.ReplaceRejected();
            state = V1CurationState.LoadOrCreate();
            RefreshResolved(force: true);
            statusMessage = "Replace Rejected done. See Console for details.";
            Debug.Log(report);
        }

        if (GUILayout.Button("OPEN IN CONTENT PLANNER / PING", GUILayout.Height(24f)))
        {
            if (cachedResolved.level != null)
            {
                Selection.activeObject = cachedResolved.level;
                EditorGUIUtility.PingObject(cachedResolved.level);
                LevelContentPlannerWindow.OpenWindow();
            }
        }

        if (GUILayout.Button("CLEAR V1 PLAYTEST OVERRIDE", GUILayout.Height(24f)))
        {
            V1PlaytestOverride.Clear();
            statusMessage = "V1 Playtest Override cleared.";
            RefreshResolved(force: true);
            Repaint();
        }
    }

    private void HandleShortcuts()
    {
        Event e = Event.current;
        if (e == null || e.type != EventType.KeyDown)
        {
            return;
        }

        // Only when this window has focus.
        if (EditorWindow.focusedWindow != this)
        {
            return;
        }

        if (e.keyCode == KeyCode.K)
        {
            RateKeep();
            e.Use();
        }
        else if (e.keyCode == KeyCode.R)
        {
            RateReject();
            e.Use();
        }
        else if (e.keyCode == KeyCode.M)
        {
            RateMaybe();
            e.Use();
        }
        else if (e.keyCode == KeyCode.N)
        {
            GoNextUnreviewed();
            e.Use();
        }
    }

    private void RateKeep()
    {
        if (!RequireCurrentLevel(out LevelData level))
        {
            return;
        }

        V1AutoCurator.SetPlaytestNote(level, noteDraft);
        V1AutoCurator.MarkManualKeep(level);
        state = V1CurationState.LoadOrCreate();
        statusMessage = "KEEP → ManualKeep: " + level.name;
        AfterRating();
    }

    private void RateReject()
    {
        if (!RequireCurrentLevel(out LevelData level))
        {
            return;
        }

        V1AutoCurator.SetPlaytestNote(level, noteDraft);
        V1AutoCurator.RejectLevel(level, pendingRejectReason);
        state = V1CurationState.LoadOrCreate();
        statusMessage = "REJECT: " + level.name +
            (pendingRejectReason != V1RejectReason.None
                ? " (" + pendingRejectReason + ")"
                : string.Empty);
        AfterRating();
    }

    private void RateMaybe()
    {
        if (!RequireCurrentLevel(out LevelData level))
        {
            return;
        }

        V1AutoCurator.SetPlaytestNote(level, noteDraft);
        V1AutoCurator.MarkMaybe(level);
        state = V1CurationState.LoadOrCreate();
        statusMessage = "MAYBE: " + level.name;
        AfterRating();
    }

    private void AfterRating()
    {
        RefreshResolved(force: true);
        Repaint();
        if (autoNextAfterRating)
        {
            GoNextUnreviewed();
        }
    }

    private bool RequireCurrentLevel(out LevelData level)
    {
        RefreshResolved(force: true);
        level = cachedResolved.level;
        if (!cachedResolved.ok || level == null)
        {
            statusMessage = "No current level to rate.";
            return false;
        }

        return true;
    }

    private LevelDifficulty? GetDifficultyFilter()
    {
        switch (difficultyFilter)
        {
            case DifficultyPlayFilter.Easy:
                return LevelDifficulty.Easy;
            case DifficultyPlayFilter.Medium:
                return LevelDifficulty.Medium;
            case DifficultyPlayFilter.Hard:
                return LevelDifficulty.Hard;
            default:
                return null;
        }
    }

    private void GoNextUnreviewed()
    {
        GoNextFinal(unreviewedOnly: true);
    }

    private void GoNextFinal(bool unreviewedOnly)
    {
        state = V1CurationState.LoadOrCreate();
        LevelDifficulty? filter = GetDifficultyFilter();
        List<V1CurationEntry> list = unreviewedOnly
            ? V1PlaytestSession.GetOrderedUnreviewedFinals(state, filter)
            : V1PlaytestSession.GetOrderedFinals(state, filter);

        if (list.Count == 0)
        {
            statusMessage = unreviewedOnly
                ? "No unreviewed Final levels left" + FilterSuffix() + "."
                : "No Final levels" + FilterSuffix() + ".";
            return;
        }

        int currentPos = IndexInList(list, cachedResolved.level);
        int nextPos = currentPos + 1;
        if (nextPos >= list.Count)
        {
            if (!wrapPlaytest)
            {
                statusMessage = "End of Final Set" + FilterSuffix() + ".";
                return;
            }

            nextPos = 0;
        }

        // If current not in list, start at 0.
        if (currentPos < 0)
        {
            nextPos = 0;
        }

        LoadEntry(list[nextPos]);
    }

    private void GoPreviousFinal()
    {
        state = V1CurationState.LoadOrCreate();
        List<V1CurationEntry> list = V1PlaytestSession.GetOrderedFinals(
            state,
            GetDifficultyFilter()
        );
        if (list.Count == 0)
        {
            statusMessage = "No Final levels.";
            return;
        }

        int currentPos = IndexInList(list, cachedResolved.level);
        int prevPos = currentPos <= 0 ? (wrapPlaytest ? list.Count - 1 : 0) : currentPos - 1;
        if (currentPos <= 0 && !wrapPlaytest)
        {
            statusMessage = "Start of Final Set.";
            LoadEntry(list[0]);
            return;
        }

        LoadEntry(list[prevPos]);
    }

    private static int IndexInList(List<V1CurationEntry> list, LevelData level)
    {
        if (level == null)
        {
            return -1;
        }

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null && list[i].level == level)
            {
                return i;
            }
        }

        return -1;
    }

    private void LoadEntry(V1CurationEntry entry)
    {
        if (entry == null || entry.level == null)
        {
            statusMessage = "Invalid curation entry.";
            return;
        }

        string error = V1PlaytestSession.LoadLevelForPlaytest(entry.level, out int dbIndex);
        if (!string.IsNullOrEmpty(error))
        {
            statusMessage = error;
            return;
        }

        noteDraft = entry.playtestNote ?? string.Empty;
        statusMessage = dbIndex >= 0
            ? "Loaded " + entry.level.name + " (override, also in DB " + dbIndex + ")"
            : "Loaded " + entry.level.name + " (DIRECT OVERRIDE — not in Main DB)";
        RefreshResolved(force: true);
        Repaint();
    }

    private string FilterSuffix()
    {
        return difficultyFilter == DifficultyPlayFilter.All
            ? string.Empty
            : " [" + difficultyFilter + "]";
    }

    private void RefreshResolved(bool force)
    {
        cachedResolved = V1PlaytestSession.ResolveCurrentLevel();
        string guid = string.Empty;
        if (cachedResolved.level != null)
        {
            guid = AssetDatabase.AssetPathToGUID(
                AssetDatabase.GetAssetPath(cachedResolved.level)
            );
        }

        if (force || guid != lastNoteLevelGuid)
        {
            lastNoteLevelGuid = guid;
            noteDraft = cachedResolved.curationEntry != null
                ? (cachedResolved.curationEntry.playtestNote ?? string.Empty)
                : string.Empty;
        }
    }
}

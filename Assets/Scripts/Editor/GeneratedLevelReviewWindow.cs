using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only reviewtool voor gegenereerde levels.
/// Menu: RushOut â†’ Generated Levels â†’ Review
///
/// Accepteer â†’ Assets/Data/Levels + MainLevelDatabase
/// Afwijzen â†’ asset verwijderen
/// </summary>
public class GeneratedLevelReviewWindow : EditorWindow
{
    private const string GeneratedFolder = "Assets/Data/GeneratedLevels";
    private const string ApprovedFolder = "Assets/Data/Levels";

    private readonly List<LevelData> reviewList = new List<LevelData>();
    private int currentIndex;
    private Vector2 vehicleListScroll;
    private string databaseStatusMessage = "";
    private bool canAcceptToDatabase = true;

    // Canonical layout key → assetnamen (GeneratedLevels + MainLevelDatabase).
    private readonly Dictionary<string, List<string>> layoutKeyOwners =
        new Dictionary<string, List<string>>(StringComparer.Ordinal);

    // Preview-kleuren
    private static readonly Color CellColor = new Color(0.18f, 0.18f, 0.20f);
    private static readonly Color GridLineColor = new Color(0.35f, 0.35f, 0.38f);
    private static readonly Color TargetColor = new Color(0.92f, 0.28f, 0.22f);
    private static readonly Color ExitColor = new Color(1f, 0.85f, 0.2f);
    private static readonly Color[] VehiclePalette =
    {
        new Color(0.25f, 0.55f, 0.95f),
        new Color(0.20f, 0.75f, 0.55f),
        new Color(0.70f, 0.45f, 0.95f),
        new Color(0.95f, 0.65f, 0.20f),
        new Color(0.35f, 0.80f, 0.90f),
        new Color(0.85f, 0.40f, 0.65f),
        new Color(0.55f, 0.70f, 0.30f),
        new Color(0.60f, 0.60f, 0.90f),
    };

    [MenuItem("RushOut/Generated Levels/Review")]
    public static void OpenWindow()
    {
        GeneratedLevelReviewWindow window = GetWindow<GeneratedLevelReviewWindow>(
            "Generated Level Review"
        );
        window.minSize = new Vector2(520f, 640f);
        window.Show();
        window.RefreshGeneratedLevels();
    }

    private void OnEnable()
    {
        RefreshGeneratedLevels();
    }

    private void OnGUI()
    {
        DrawToolbar();
        EditorGUILayout.Space(6f);

        if (reviewList.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Geen generated levels gevonden in:\n" + GeneratedFolder +
                "\n\nGenereer eerst levels via RushOut â†’ Generate Levels.",
                MessageType.Info
            );
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, reviewList.Count - 1);
        LevelData level = reviewList[currentIndex];

        if (level == null)
        {
            EditorGUILayout.HelpBox("Huidig level is null. Druk op Refresh.", MessageType.Warning);
            return;
        }

        DrawHeader(level);
        EditorGUILayout.Space(8f);

        // Vierkante preview zo breed mogelijk.
        float previewSize = Mathf.Min(position.width - 40f, 360f);
        Rect previewRect = GUILayoutUtility.GetRect(
            previewSize,
            previewSize,
            GUILayout.ExpandWidth(false)
        );
        previewRect.x = (position.width - previewSize) * 0.5f;
        DrawGridPreview(previewRect, level);

        EditorGUILayout.Space(8f);
        DrawVehicleList(level);
        EditorGUILayout.Space(10f);
        DrawActionButtons(level);

        if (!string.IsNullOrEmpty(databaseStatusMessage))
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.HelpBox(
                databaseStatusMessage,
                canAcceptToDatabase ? MessageType.None : MessageType.Error
            );
        }
    }

    // -------------------------------------------------------------------------
    // UI-secties
    // -------------------------------------------------------------------------

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Refresh Generated Levels", EditorStyles.toolbarButton))
        {
            RefreshGeneratedLevels();
        }

        GUILayout.FlexibleSpace();
        GUILayout.Label(
            reviewList.Count + " candidate(s)",
            EditorStyles.miniLabel
        );

        EditorGUILayout.EndHorizontal();
    }

    private void DrawHeader(LevelData level)
    {
        string assetName = GetAssetName(level);
        int vehicleCount = level.vehicles != null ? level.vehicles.Count : 0;

        EditorGUILayout.LabelField(
            "Level " + (currentIndex + 1) + " / " + reviewList.Count,
            EditorStyles.boldLabel
        );
        EditorGUILayout.LabelField("Asset", assetName);
        EditorGUILayout.LabelField("minimumMoves", level.minimumMoves.ToString());
        EditorGUILayout.LabelField("statesExplored", level.statesExplored.ToString());
        EditorGUILayout.LabelField("difficultyScore", level.difficultyScore.ToString());
        EditorGUILayout.LabelField(
            "solutionComplexityScore",
            level.solutionComplexityScore.ToString()
        );
        EditorGUILayout.LabelField(
            "uniqueVehicles",
            level.uniqueVehiclesInSolution.ToString()
        );
        EditorGUILayout.LabelField("axisAlternations", level.axisAlternations.ToString());
        EditorGUILayout.LabelField("vehicleRevisits", level.vehicleRevisits.ToString());
        EditorGUILayout.LabelField(
            "longestSingleAxisRun",
            level.longestSingleAxisRun.ToString()
        );
        EditorGUILayout.LabelField(
            "linearChainRatio",
            level.linearChainSolutionRatio.ToString("0.00")
        );
        EditorGUILayout.LabelField(
            "secondaryBlockersUsed",
            level.secondaryBlockersUsed.ToString()
        );
        EditorGUILayout.LabelField("forkDependencies", level.forkDependencies.ToString());
        EditorGUILayout.LabelField("Vehicles", vehicleCount.ToString());
        EditorGUILayout.LabelField(
            "Grid",
            level.ResolvedGridWidth + "x" + level.ResolvedGridHeight
        );
        EditorGUILayout.LabelField("exitRow", level.exitRow.ToString());

        DrawDuplicateWarning(level);
    }

    private void DrawDuplicateWarning(LevelData level)
    {
        string key = LevelCanonicalKey.BuildCanonicalLevelKey(level);
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        List<string> owners;
        if (!layoutKeyOwners.TryGetValue(key, out owners) || owners == null || owners.Count <= 1)
        {
            return;
        }

        string currentName = GetAssetName(level);
        List<string> others = new List<string>();
        for (int i = 0; i < owners.Count; i++)
        {
            if (!string.Equals(owners[i], currentName, StringComparison.Ordinal))
            {
                others.Add(owners[i]);
            }
        }

        if (others.Count == 0)
        {
            return;
        }

        string preview = others[0];
        if (others.Count > 1)
        {
            preview += " (+" + (others.Count - 1) + " more)";
        }

        EditorGUILayout.HelpBox(
            "Duplicate gameplay layout vs existing level(s): " + preview,
            MessageType.Warning
        );
    }

    private void DrawVehicleList(LevelData level)
    {
        EditorGUILayout.LabelField("Vehicles", EditorStyles.boldLabel);

        vehicleListScroll = EditorGUILayout.BeginScrollView(
            vehicleListScroll,
            GUILayout.MinHeight(100f),
            GUILayout.MaxHeight(180f)
        );

        if (level.vehicles != null)
        {
            foreach (VehicleData vehicle in level.vehicles)
            {
                string line =
                    vehicle.vehicleName +
                    " | " + vehicle.orientation +
                    " | len=" + vehicle.lengthInCells +
                    " | pos=(" + vehicle.gridPosition.x + "," + vehicle.gridPosition.y + ")" +
                    " | exit=" + vehicle.canExitRight;

                EditorGUILayout.LabelField(line, EditorStyles.miniLabel);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawActionButtons(LevelData level)
    {
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = currentIndex > 0;
        if (GUILayout.Button("Previous", GUILayout.Height(28f)))
        {
            currentIndex--;
            Repaint();
        }

        GUI.enabled = currentIndex < reviewList.Count - 1;
        if (GUILayout.Button("Next", GUILayout.Height(28f)))
        {
            currentIndex++;
            Repaint();
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4f);
        EditorGUILayout.BeginHorizontal();

        // Accept alleen als database beschikbaar is.
        UpdateDatabaseStatus();
        GUI.enabled = canAcceptToDatabase;
        GUI.backgroundColor = new Color(0.45f, 0.85f, 0.45f);
        if (GUILayout.Button("Accept", GUILayout.Height(32f)))
        {
            AcceptCurrentLevel(level);
        }

        GUI.enabled = true;
        GUI.backgroundColor = new Color(0.95f, 0.45f, 0.45f);
        if (GUILayout.Button("Reject", GUILayout.Height(32f)))
        {
            RejectCurrentLevel(level);
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4f);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Re-run Solver", GUILayout.Height(28f)))
        {
            RerunSolver(level);
        }

        if (GUILayout.Button("Open Asset", GUILayout.Height(28f)))
        {
            OpenAsset(level);
        }

        EditorGUILayout.EndHorizontal();
    }

    // -------------------------------------------------------------------------
    // Grid preview (Editor GUI only)
    // -------------------------------------------------------------------------

    private void DrawGridPreview(Rect area, LevelData level)
    {
        int gridWidth = level.ResolvedGridWidth;
        int gridHeight = level.ResolvedGridHeight;

        EditorGUI.DrawRect(area, new Color(0.12f, 0.12f, 0.14f));

        float cell = Mathf.Min(area.width / gridWidth, area.height / gridHeight);
        float gridPixelW = cell * gridWidth;
        float gridPixelH = cell * gridHeight;
        Rect gridArea = new Rect(
            area.x + (area.width - gridPixelW) * 0.5f,
            area.y + (area.height - gridPixelH) * 0.5f,
            gridPixelW,
            gridPixelH
        );

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int drawY = gridHeight - 1 - y;
                Rect cellRect = new Rect(
                    gridArea.x + x * cell,
                    gridArea.y + drawY * cell,
                    cell,
                    cell
                );
                EditorGUI.DrawRect(Inset(cellRect, 1f), CellColor);
            }
        }

        if (level.vehicles != null)
        {
            int colorIndex = 0;
            foreach (VehicleData vehicle in level.vehicles)
            {
                Color color = vehicle.canExitRight
                    ? TargetColor
                    : VehiclePalette[colorIndex % VehiclePalette.Length];

                if (!vehicle.canExitRight)
                {
                    colorIndex++;
                }

                DrawVehicleRect(gridArea, cell, gridHeight, vehicle, color);
            }
        }

        DrawExitMarker(gridArea, cell, gridHeight, level.exitRow);

        Handles.BeginGUI();
        Handles.color = GridLineColor;
        Handles.DrawSolidRectangleWithOutline(
            gridArea,
            Color.clear,
            GridLineColor
        );
        Handles.EndGUI();
    }

    private void DrawVehicleRect(
        Rect area,
        float cell,
        int gridHeight,
        VehicleData vehicle,
        Color color)
    {
        int x = vehicle.gridPosition.x;
        int y = vehicle.gridPosition.y;
        bool horizontal =
            vehicle.orientation == VehicleController.VehicleOrientation.Horizontal;
        int length = Mathf.Max(1, vehicle.lengthInCells);

        float px;
        float py;
        float w;
        float h;

        if (horizontal)
        {
            px = area.x + x * cell;
            py = area.y + (gridHeight - 1 - y) * cell;
            w = length * cell;
            h = cell;
        }
        else
        {
            px = area.x + x * cell;
            int topY = y + length - 1;
            py = area.y + (gridHeight - 1 - topY) * cell;
            w = cell;
            h = length * cell;
        }

        Rect vehicleRect = Inset(new Rect(px, py, w, h), 3f);
        EditorGUI.DrawRect(vehicleRect, color);

        string label = string.IsNullOrEmpty(vehicle.vehicleName)
            ? "?"
            : vehicle.vehicleName.Substring(0, 1);
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            fontSize = 11
        };
        GUI.Label(vehicleRect, label, style);
    }

    private void DrawExitMarker(Rect area, float cell, int gridHeight, int exitRow)
    {
        exitRow = Mathf.Clamp(exitRow, 0, gridHeight - 1);
        float markerWidth = cell * 0.35f;

        Rect exitRect = new Rect(
            area.xMax - markerWidth,
            area.y + (gridHeight - 1 - exitRow) * cell + 4f,
            markerWidth,
            cell - 8f
        );

        EditorGUI.DrawRect(exitRect, ExitColor);

        GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.black },
            fontSize = 9
        };
        GUI.Label(exitRect, "E", style);
    }
    private static Rect Inset(Rect rect, float inset)
    {
        return new Rect(
            rect.x + inset,
            rect.y + inset,
            Mathf.Max(0f, rect.width - inset * 2f),
            Mathf.Max(0f, rect.height - inset * 2f)
        );
    }

    // -------------------------------------------------------------------------
    // Acties
    // -------------------------------------------------------------------------

    private void RefreshGeneratedLevels()
    {
        reviewList.Clear();
        layoutKeyOwners.Clear();
        currentIndex = 0;
        UpdateDatabaseStatus();

        if (!AssetDatabase.IsValidFolder(GeneratedFolder))
        {
            RebuildLayoutKeyOwners();
            Repaint();
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { GeneratedFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (level != null)
            {
                reviewList.Add(level);
            }
        }

        reviewList.Sort(CompareByDifficulty);
        RebuildLayoutKeyOwners();
        Repaint();
    }

    private void RebuildLayoutKeyOwners()
    {
        layoutKeyOwners.Clear();

        for (int i = 0; i < reviewList.Count; i++)
        {
            LevelData level = reviewList[i];
            if (level == null)
            {
                continue;
            }

            RegisterLayoutOwner(level, GetAssetName(level));
        }

        LevelDatabase database = TryLoadMainLevelDatabase(out _);
        if (database == null || database.levels == null)
        {
            return;
        }

        HashSet<LevelData> seen = new HashSet<LevelData>();
        for (int i = 0; i < database.levels.Count; i++)
        {
            LevelData level = database.levels[i];
            if (level == null || !seen.Add(level))
            {
                continue;
            }

            string label = GetAssetName(level);
            if (string.IsNullOrEmpty(label))
            {
                label = "MainLevelDatabase[" + i + "]";
            }

            RegisterLayoutOwner(level, label);
        }
    }

    private void RegisterLayoutOwner(LevelData level, string ownerLabel)
    {
        string key = LevelCanonicalKey.BuildCanonicalLevelKey(level);
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        List<string> owners;
        if (!layoutKeyOwners.TryGetValue(key, out owners))
        {
            owners = new List<string>();
            layoutKeyOwners[key] = owners;
        }

        if (!owners.Contains(ownerLabel))
        {
            owners.Add(ownerLabel);
        }
    }

    private static int CompareByDifficulty(LevelData a, LevelData b)
    {
        bool aBad = a == null || a.difficultyScore < 0;
        bool bBad = b == null || b.difficultyScore < 0;

        if (aBad && !bBad) return 1;
        if (!aBad && bBad) return -1;
        if (aBad && bBad) return 0;

        int score = a.difficultyScore.CompareTo(b.difficultyScore);
        if (score != 0) return score;
        return a.minimumMoves.CompareTo(b.minimumMoves);
    }

    private void AcceptCurrentLevel(LevelData level)
    {
        UpdateDatabaseStatus();
        if (!canAcceptToDatabase)
        {
            Debug.LogError("Accept geblokkeerd: " + databaseStatusMessage);
            return;
        }

        LevelDatabase database = TryLoadMainLevelDatabase(out string error);
        if (database == null)
        {
            databaseStatusMessage = error;
            canAcceptToDatabase = false;
            Repaint();
            return;
        }

        EnsureFolderExists(ApprovedFolder);

        string sourcePath = AssetDatabase.GetAssetPath(level);
        if (string.IsNullOrEmpty(sourcePath))
        {
            Debug.LogError("Accept: asset-pad ontbreekt.");
            return;
        }

        // Volgend levelNumber = 1 + hoogste bestaande in database Ã©n in Levels-map.
        int nextLevelNumber = GetNextApprovedLevelNumber(database);
        string desiredFileName = "Level_" + nextLevelNumber.ToString("000") + ".asset";
        string destinationPath = GetUniqueAssetPath(ApprovedFolder, desiredFileName);

        string moveError = AssetDatabase.MoveAsset(sourcePath, destinationPath);
        if (!string.IsNullOrEmpty(moveError))
        {
            Debug.LogError("Accept: verplaatsen mislukt â€” " + moveError);
            return;
        }

        LevelData moved = AssetDatabase.LoadAssetAtPath<LevelData>(destinationPath);
        if (moved == null)
        {
            Debug.LogError("Accept: kon verplaatst asset niet laden: " + destinationPath);
            return;
        }

        // Uniek levelNumber garanderen t.o.v. database.
        moved.levelNumber = nextLevelNumber;
        EditorUtility.SetDirty(moved);

        if (database.levels == null)
        {
            database.levels = new List<LevelData>();
        }

        // Geen duplicate references / levelNumbers.
        database.levels.RemoveAll(l => l == null || l == moved || l.levelNumber == moved.levelNumber);
        database.levels.Add(moved);
        database.levels.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));
        EditorUtility.SetDirty(database);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "Accepted " + GetAssetName(moved) +
            " as levelNumber=" + moved.levelNumber +
            " â†’ " + destinationPath
        );

        // Uit reviewlijst + door naar volgende kandidaat.
        int removeAt = currentIndex;
        reviewList.RemoveAt(removeAt);
        if (currentIndex >= reviewList.Count)
        {
            currentIndex = Mathf.Max(0, reviewList.Count - 1);
        }

        Repaint();
    }

    private void RejectCurrentLevel(LevelData level)
    {
        string assetName = GetAssetName(level);
        bool confirmed = EditorUtility.DisplayDialog(
            "Reject Generated Level",
            "Verwijder generated level \"" + assetName + "\" permanent?\n\nDit kan niet ongedaan worden gemaakt.",
            "Delete",
            "Cancel"
        );

        if (!confirmed)
        {
            return;
        }

        string path = AssetDatabase.GetAssetPath(level);
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log("Rejected & deleted: " + assetName);

        int removeAt = currentIndex;
        reviewList.RemoveAt(removeAt);
        if (currentIndex >= reviewList.Count)
        {
            currentIndex = Mathf.Max(0, reviewList.Count - 1);
        }

        Repaint();
    }

    private void RerunSolver(LevelData level)
    {
        LevelSolver.SolverResult result = LevelSolver.Solve(level);
        LevelSolver.ApplyMetadata(level, result);
        AssetDatabase.SaveAssets();

        Debug.Log(
            "Re-ran solver on " + GetAssetName(level) +
            " â†’ moves=" + level.minimumMoves +
            ", states=" + level.statesExplored +
            ", score=" + level.difficultyScore
        );

        // Houd sort-volgorde bij na score-wijziging.
        LevelData current = level;
        reviewList.Sort(CompareByDifficulty);
        currentIndex = Mathf.Max(0, reviewList.IndexOf(current));
        Repaint();
    }

    private static void OpenAsset(LevelData level)
    {
        Selection.activeObject = level;
        EditorGUIUtility.PingObject(level);
    }

    // -------------------------------------------------------------------------
    // Database / bestanden
    // -------------------------------------------------------------------------

    private void UpdateDatabaseStatus()
    {
        LevelDatabase database = TryLoadMainLevelDatabase(out string error);
        if (database == null)
        {
            canAcceptToDatabase = false;
            databaseStatusMessage = error + "\nAccept is geblokkeerd. Review blijft beschikbaar.";
        }
        else
        {
            canAcceptToDatabase = true;
            databaseStatusMessage = "";
        }
    }

    private static LevelDatabase TryLoadMainLevelDatabase(out string error)
    {
        error = null;
        string[] guids = AssetDatabase.FindAssets("MainLevelDatabase t:LevelDatabase");

        if (guids == null || guids.Length == 0)
        {
            error = "Geen MainLevelDatabase gevonden.";
            return null;
        }

        if (guids.Length > 1)
        {
            error = "Meerdere MainLevelDatabase assets gevonden (" + guids.Length +
                    "). Gebruik precies Ã©Ã©n database.";
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        LevelDatabase database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);
        if (database == null)
        {
            error = "MainLevelDatabase kon niet geladen worden: " + path;
            return null;
        }

        return database;
    }

    /// <summary>
    /// Hoogste levelNumber in database + Levels-map, plus 1.
    /// </summary>
    private static int GetNextApprovedLevelNumber(LevelDatabase database)
    {
        int highest = 0;

        if (database.levels != null)
        {
            foreach (LevelData level in database.levels)
            {
                if (level != null)
                {
                    highest = Mathf.Max(highest, level.levelNumber);
                }
            }
        }

        if (AssetDatabase.IsValidFolder(ApprovedFolder))
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { ApprovedFolder });
            foreach (string guid in guids)
            {
                LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(
                    AssetDatabase.GUIDToAssetPath(guid)
                );
                if (level != null)
                {
                    highest = Mathf.Max(highest, level.levelNumber);
                }
            }
        }

        return highest + 1;
    }

    private static string GetUniqueAssetPath(string folder, string fileName)
    {
        folder = folder.TrimEnd('/', '\\');
        string path = folder + "/" + fileName;

        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) == null)
        {
            return path;
        }

        string baseName = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        int suffix = 2;

        while (true)
        {
            string candidate = folder + "/" + baseName + "_" + suffix + extension;
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(candidate) == null)
            {
                return candidate;
            }

            suffix++;
        }
    }

    private static void EnsureFolderExists(string folder)
    {
        folder = folder.Replace('\\', '/').TrimEnd('/');
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string[] parts = folder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private static string GetAssetName(LevelData level)
    {
        string path = AssetDatabase.GetAssetPath(level);
        if (!string.IsNullOrEmpty(path))
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        return level != null ? level.name : "null";
    }
}

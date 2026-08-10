using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-tool: JSON-levels → LevelData ScriptableObjects.
/// Menu: RushOut → Import Levels From JSON
/// </summary>
public static class LevelJsonImporter
{
    private const string OutputFolder = "Assets/Data/Levels";

    private static int ResolveJsonGridSize(int value)
    {
        return value > 0 ? value : LevelData.DefaultGridSize;
    }

    [MenuItem("RushOut/Import Levels From JSON")]
    public static void ImportLevelsFromJson()
    {
        string jsonPath = EditorUtility.OpenFilePanel(
            "Select Levels JSON",
            Application.dataPath,
            "json"
        );

        if (string.IsNullOrEmpty(jsonPath))
        {
            Debug.Log("LevelJsonImporter: import geannuleerd.");
            return;
        }

        string jsonText = File.ReadAllText(jsonPath);
        if (string.IsNullOrWhiteSpace(jsonText))
        {
            Debug.LogError("LevelJsonImporter: JSON-bestand is leeg.");
            return;
        }

        // JsonUtility kan geen top-level array lezen — wrap in een object.
        string wrappedJson = "{\"levels\":" + jsonText + "}";
        JsonLevelList levelList = JsonUtility.FromJson<JsonLevelList>(wrappedJson);

        if (levelList == null || levelList.levels == null || levelList.levels.Length == 0)
        {
            Debug.LogError("LevelJsonImporter: geen levels gevonden in JSON.");
            return;
        }

        EnsureOutputFolderExists();

        int importedCount = 0;
        List<LevelData> importedLevels = new List<LevelData>();

        foreach (JsonLevel jsonLevel in levelList.levels)
        {
            string validationError = ValidateLevel(jsonLevel);
            if (validationError != null)
            {
                int levelLabel = jsonLevel != null ? jsonLevel.level : -1;
                Debug.LogError(
                    "LevelJsonImporter: level " + levelLabel +
                    " overgeslagen — " + validationError
                );
                continue;
            }

            LevelData levelData = CreateOrUpdateLevelAsset(jsonLevel);
            if (levelData != null)
            {
                importedLevels.Add(levelData);
                importedCount++;
            }
        }

        UpdateMainLevelDatabase(importedLevels, importedCount);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Maakt Assets/Data/Levels aan als die nog niet bestaat.
    /// </summary>
    private static void EnsureOutputFolderExists()
    {
        if (AssetDatabase.IsValidFolder(OutputFolder))
        {
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Data"))
        {
            AssetDatabase.CreateFolder("Assets", "Data");
        }

        AssetDatabase.CreateFolder("Assets/Data", "Levels");
    }

    /// <summary>
    /// Maakt een nieuw LevelData-asset of werkt een bestaand asset bij.
    /// </summary>
    private static LevelData CreateOrUpdateLevelAsset(JsonLevel jsonLevel)
    {
        string assetName = "Level_" + jsonLevel.level.ToString("000") + ".asset";
        string assetPath = OutputFolder + "/" + assetName;

        LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
        bool isNew = levelData == null;

        if (isNew)
        {
            levelData = ScriptableObject.CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(levelData, assetPath);
        }

        levelData.levelNumber = jsonLevel.level;
        levelData.exitRow = jsonLevel.exitRow;
        levelData.gridWidth = ResolveJsonGridSize(jsonLevel.gridWidth);
        levelData.gridHeight = ResolveJsonGridSize(jsonLevel.gridHeight);
        levelData.vehicles = BuildVehicleList(jsonLevel.vehicles);

        EditorUtility.SetDirty(levelData);

        Debug.Log(
            "LevelJsonImporter: " + (isNew ? "aangemaakt" : "bijgewerkt") +
            " → " + assetPath
        );

        return levelData;
    }

    /// <summary>
    /// Voegt geïmporteerde levels toe aan MainLevelDatabase (geen duplicates, gesorteerd).
    /// Verwijdert geen bestaande entries.
    /// </summary>
    private static void UpdateMainLevelDatabase(
        List<LevelData> importedLevels,
        int importedCount)
    {
        string[] guids = AssetDatabase.FindAssets("MainLevelDatabase t:LevelDatabase");

        if (guids == null || guids.Length == 0)
        {
            Debug.LogWarning(
                "LevelJsonImporter: geen MainLevelDatabase gevonden. " +
                "Level-assets zijn wel geïmporteerd, maar de database is niet bijgewerkt."
            );
            Debug.Log(
                "Imported " + importedCount +
                " levels. MainLevelDatabase was not updated."
            );
            return;
        }

        if (guids.Length > 1)
        {
            Debug.LogWarning(
                "LevelJsonImporter: meerdere MainLevelDatabase assets gevonden (" +
                guids.Length + "). Database niet automatisch bijgewerkt. " +
                "Zorg dat er precies één MainLevelDatabase bestaat."
            );
            Debug.Log(
                "Imported " + importedCount +
                " levels. MainLevelDatabase was not updated (multiple found)."
            );
            return;
        }

        string databasePath = AssetDatabase.GUIDToAssetPath(guids[0]);
        LevelDatabase database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(databasePath);

        if (database == null)
        {
            Debug.LogWarning(
                "LevelJsonImporter: MainLevelDatabase kon niet geladen worden op: " +
                databasePath
            );
            return;
        }

        if (database.levels == null)
        {
            database.levels = new List<LevelData>();
        }

        // Voeg alleen toe als dit asset nog niet in de lijst staat.
        foreach (LevelData imported in importedLevels)
        {
            if (imported == null)
            {
                continue;
            }

            if (!database.levels.Contains(imported))
            {
                database.levels.Add(imported);
            }
        }

        // Nulls weg, daarna maximaal één entry per levelNumber.
        database.levels.RemoveAll(level => level == null);
        DeduplicateLevelsByNumber(database);

        database.levels.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));

        EditorUtility.SetDirty(database);

        Debug.Log(
            "Imported " + importedCount +
            " levels and updated MainLevelDatabase. Total levels: " +
            database.levels.Count
        );
    }

    /// <summary>
    /// Houdt per levelNumber precies één LevelData.
    /// Voorkeur: asset onder Assets/Data/Levels/.
    /// Verwijdert alleen database-references, geen asset-bestanden.
    /// </summary>
    private static void DeduplicateLevelsByNumber(LevelDatabase database)
    {
        // Groepeer indices per levelNumber.
        Dictionary<int, List<LevelData>> byNumber = new Dictionary<int, List<LevelData>>();

        foreach (LevelData level in database.levels)
        {
            int number = level.levelNumber;
            if (!byNumber.ContainsKey(number))
            {
                byNumber[number] = new List<LevelData>();
            }

            byNumber[number].Add(level);
        }

        List<LevelData> cleaned = new List<LevelData>();

        foreach (KeyValuePair<int, List<LevelData>> pair in byNumber)
        {
            List<LevelData> candidates = pair.Value;

            if (candidates.Count == 1)
            {
                cleaned.Add(candidates[0]);
                continue;
            }

            // Kies voorkeur: pad onder Assets/Data/Levels/
            LevelData preferred = null;
            foreach (LevelData candidate in candidates)
            {
                string path = AssetDatabase.GetAssetPath(candidate);
                if (!string.IsNullOrEmpty(path) &&
                    path.StartsWith(OutputFolder + "/", StringComparison.OrdinalIgnoreCase))
                {
                    preferred = candidate;
                    break;
                }
            }

            // Geen import-map asset? Behoud de eerste in de lijst.
            if (preferred == null)
            {
                preferred = candidates[0];
            }

            cleaned.Add(preferred);

            // Log elke verwijderde duplicate-reference.
            foreach (LevelData candidate in candidates)
            {
                if (candidate == preferred)
                {
                    continue;
                }

                string removedPath = AssetDatabase.GetAssetPath(candidate);
                Debug.Log(
                    "LevelJsonImporter: duplicate levelNumber " + pair.Key +
                    " verwijderd uit MainLevelDatabase → " +
                    (string.IsNullOrEmpty(removedPath) ? candidate.name : removedPath) +
                    " (behouden: " + AssetDatabase.GetAssetPath(preferred) + ")"
                );
            }
        }

        database.levels = cleaned;
    }

    private static List<VehicleData> BuildVehicleList(JsonVehicle[] jsonVehicles)
    {
        List<VehicleData> vehicles = new List<VehicleData>();

        foreach (JsonVehicle jsonVehicle in jsonVehicles)
        {
            VehicleData vehicle = new VehicleData();
            vehicle.vehicleName = jsonVehicle.name;
            vehicle.orientation = ParseOrientation(jsonVehicle.orientation);
            vehicle.lengthInCells = jsonVehicle.lengthInCells;
            vehicle.gridPosition = new Vector2Int(
                jsonVehicle.gridPosition.X,
                jsonVehicle.gridPosition.Y
            );
            vehicle.canExitRight = jsonVehicle.canExitRight;
            // Sprites komen niet uit JSON — blijft null.
            vehicle.vehicleSprite = null;

            vehicles.Add(vehicle);
        }

        return vehicles;
    }

    private static VehicleController.VehicleOrientation ParseOrientation(string orientation)
    {
        if (string.Equals(orientation, "Vertical", StringComparison.OrdinalIgnoreCase))
        {
            return VehicleController.VehicleOrientation.Vertical;
        }

        return VehicleController.VehicleOrientation.Horizontal;
    }

    /// <summary>
    /// Valideert één level. Geeft null terug als alles ok is, anders een foutreden.
    /// </summary>
    private static string ValidateLevel(JsonLevel jsonLevel)
    {
        if (jsonLevel == null)
        {
            return "level-data is null.";
        }

        int gridWidth = ResolveJsonGridSize(jsonLevel.gridWidth);
        int gridHeight = ResolveJsonGridSize(jsonLevel.gridHeight);

        if (gridWidth < LevelData.MinGridSize || gridWidth > LevelData.MaxGridSize)
        {
            return "gridWidth moet tussen " + LevelData.MinGridSize +
                   " en " + LevelData.MaxGridSize +
                   " liggen (nu: " + gridWidth + ").";
        }

        if (gridHeight < LevelData.MinGridSize || gridHeight > LevelData.MaxGridSize)
        {
            return "gridHeight moet tussen " + LevelData.MinGridSize +
                   " en " + LevelData.MaxGridSize +
                   " liggen (nu: " + gridHeight + ").";
        }

        if (jsonLevel.exitRow < 0 || jsonLevel.exitRow >= gridHeight)
        {
            return "exitRow moet tussen 0 en " + (gridHeight - 1) +
                   " liggen (nu: " + jsonLevel.exitRow + ").";
        }

        if (jsonLevel.vehicles == null || jsonLevel.vehicles.Length == 0)
        {
            return "geen voertuigen aanwezig.";
        }

        int targetCount = 0;
        HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

        for (int i = 0; i < jsonLevel.vehicles.Length; i++)
        {
            JsonVehicle vehicle = jsonLevel.vehicles[i];
            string vehicleLabel = string.IsNullOrEmpty(vehicle.name)
                ? ("voertuig index " + i)
                : vehicle.name;

            if (vehicle.gridPosition == null)
            {
                return vehicleLabel + ": gridPosition ontbreekt.";
            }

            if (!IsValidOrientation(vehicle.orientation))
            {
                return vehicleLabel + ": orientation moet Horizontal of Vertical zijn (nu: \"" +
                       vehicle.orientation + "\").";
            }

            if (vehicle.lengthInCells < 2 || vehicle.lengthInCells > 4)
            {
                return vehicleLabel + ": lengthInCells moet 2, 3 of 4 zijn (nu: " +
                       vehicle.lengthInCells + ").";
            }

            List<Vector2Int> cells = GetOccupiedCells(vehicle);
            foreach (Vector2Int cell in cells)
            {
                if (cell.x < 0 || cell.x >= gridWidth || cell.y < 0 || cell.y >= gridHeight)
                {
                    return vehicleLabel + ": staat (deels) buiten het " +
                           gridWidth + "x" + gridHeight + " grid op cel " + cell + ".";
                }

                if (!occupiedCells.Add(cell))
                {
                    return vehicleLabel + ": overlapt met een ander voertuig op cel " + cell + ".";
                }
            }

            if (vehicle.canExitRight)
            {
                targetCount++;

                if (!string.Equals(vehicle.orientation, "Horizontal", StringComparison.OrdinalIgnoreCase))
                {
                    return vehicleLabel + ": target vehicle moet Horizontal zijn.";
                }

                if (vehicle.gridPosition.Y != jsonLevel.exitRow)
                {
                    return vehicleLabel + ": target Y (" + vehicle.gridPosition.Y +
                           ") moet gelijk zijn aan exitRow (" + jsonLevel.exitRow + ").";
                }
            }
        }

        if (targetCount != 1)
        {
            return "precies één voertuig moet canExitRight=true hebben (nu: " + targetCount + ").";
        }

        return null;
    }

    private static bool IsValidOrientation(string orientation)
    {
        return string.Equals(orientation, "Horizontal", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(orientation, "Vertical", StringComparison.OrdinalIgnoreCase);
    }

    private static List<Vector2Int> GetOccupiedCells(JsonVehicle vehicle)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        bool horizontal = string.Equals(
            vehicle.orientation,
            "Horizontal",
            StringComparison.OrdinalIgnoreCase
        );

        for (int i = 0; i < vehicle.lengthInCells; i++)
        {
            if (horizontal)
            {
                cells.Add(new Vector2Int(
                    vehicle.gridPosition.X + i,
                    vehicle.gridPosition.Y
                ));
            }
            else
            {
                cells.Add(new Vector2Int(
                    vehicle.gridPosition.X,
                    vehicle.gridPosition.Y + i
                ));
            }
        }

        return cells;
    }

    // -------------------------------------------------------------------------
    // JSON helper classes (veldnamen matchen de JSON-keys)
    // -------------------------------------------------------------------------

    [Serializable]
    private class JsonLevelList
    {
        public JsonLevel[] levels;
    }

    [Serializable]
    private class JsonLevel
    {
        public int level;
        public int exitRow;
        // Optioneel — ontbreekt of 0 → default 6.
        public int gridWidth;
        public int gridHeight;
        public JsonVehicle[] vehicles;
        // estimatedMinimumMoves / difficultyNote worden bewust genegeerd.
    }

    [Serializable]
    private class JsonVehicle
    {
        public string name;
        public string orientation;
        public int lengthInCells;
        public JsonGridPosition gridPosition;
        public bool canExitRight;
    }

    [Serializable]
    private class JsonGridPosition
    {
        public int X;
        public int Y;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Stabiele gameplay-layout key voor duplicate-detectie.
/// Negeert visuals, assetnamen en difficulty-metadata.
/// </summary>
public static class LevelCanonicalKey
{
    public const string GeneratedLevelsRoot = "Assets/Data/GeneratedLevels";

    /// <summary>
    /// Canonical key voor een LevelData-asset (alleen gameplay-layout).
    /// </summary>
    public static string BuildCanonicalLevelKey(LevelData level)
    {
        if (level == null)
        {
            return string.Empty;
        }

        int gridWidth = level.ResolvedGridWidth;
        int gridHeight = level.ResolvedGridHeight;
        int exitRow = level.exitRow;
        List<VehicleEntry> entries = new List<VehicleEntry>();

        if (level.vehicles != null)
        {
            for (int i = 0; i < level.vehicles.Count; i++)
            {
                VehicleData v = level.vehicles[i];
                if (v == null)
                {
                    continue;
                }

                entries.Add(new VehicleEntry(
                    v.canExitRight,
                    v.gridPosition.x,
                    v.gridPosition.y,
                    v.orientation == VehicleController.VehicleOrientation.Horizontal,
                    v.lengthInCells
                ));
            }
        }

        return BuildFromEntries(gridWidth, gridHeight, exitRow, entries);
    }

    /// <summary>
    /// Zelfde key-formaat vanuit generator-candidate velden (zonder visuals).
    /// </summary>
    public static string BuildCanonicalLevelKey(
        int gridWidth,
        int gridHeight,
        int exitRow,
        IList<bool> canExitRight,
        IList<int> x,
        IList<int> y,
        IList<bool> horizontal,
        IList<int> length)
    {
        int count = canExitRight != null ? canExitRight.Count : 0;
        List<VehicleEntry> entries = new List<VehicleEntry>(count);
        for (int i = 0; i < count; i++)
        {
            entries.Add(new VehicleEntry(
                canExitRight[i],
                x[i],
                y[i],
                horizontal[i],
                length[i]
            ));
        }

        return BuildFromEntries(gridWidth, gridHeight, exitRow, entries);
    }

    /// <summary>
    /// Scant GeneratedLevels (+ subfolders) en MainLevelDatabase één keer.
    /// Logt bestaande duplicates zonder ze te verwijderen.
    /// </summary>
    public static HashSet<string> CollectExistingLevelKeys(
        out int keysLoaded,
        out int generatedAssetsScanned,
        out int databaseLevelsScanned,
        out int existingDuplicateReports)
    {
        HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
        keysLoaded = 0;
        generatedAssetsScanned = 0;
        databaseLevelsScanned = 0;
        existingDuplicateReports = 0;

        Dictionary<string, string> firstPathByKey = new Dictionary<string, string>(StringComparer.Ordinal);

        if (AssetDatabase.IsValidFolder(GeneratedLevelsRoot))
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { GeneratedLevelsRoot });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (level == null)
                {
                    continue;
                }

                generatedAssetsScanned++;
                AddExistingKey(keys, firstPathByKey, level, path, ref existingDuplicateReports);
            }
        }

        LevelDatabase database = TryLoadMainLevelDatabase();
        if (database != null && database.levels != null)
        {
            HashSet<LevelData> seenRefs = new HashSet<LevelData>();
            for (int i = 0; i < database.levels.Count; i++)
            {
                LevelData level = database.levels[i];
                if (level == null || !seenRefs.Add(level))
                {
                    continue;
                }

                databaseLevelsScanned++;
                string path = AssetDatabase.GetAssetPath(level);
                if (string.IsNullOrEmpty(path))
                {
                    path = "MainLevelDatabase[" + i + "]";
                }

                AddExistingKey(keys, firstPathByKey, level, path, ref existingDuplicateReports);
            }
        }

        keysLoaded = keys.Count;
        return keys;
    }

    private static void AddExistingKey(
        HashSet<string> keys,
        Dictionary<string, string> firstPathByKey,
        LevelData level,
        string path,
        ref int existingDuplicateReports)
    {
        string key = BuildCanonicalLevelKey(level);
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (!keys.Add(key))
        {
            existingDuplicateReports++;
            string firstPath;
            firstPathByKey.TryGetValue(key, out firstPath);
            Debug.LogWarning(
                "LevelCanonicalKey: bestaande duplicate layout gevonden (niet verwijderd).\n" +
                "  first: " + (firstPath ?? "?") + "\n" +
                "  also:  " + path
            );
            return;
        }

        firstPathByKey[key] = path;
    }

    private static LevelDatabase TryLoadMainLevelDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("MainLevelDatabase t:LevelDatabase");
        if (guids == null || guids.Length != 1)
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);
    }

    private static string BuildFromEntries(
        int gridWidth,
        int gridHeight,
        int exitRow,
        List<VehicleEntry> entries)
    {
        entries.Sort(CompareEntries);

        StringBuilder sb = new StringBuilder(64 + entries.Count * 24);
        sb.Append("gw=").Append(gridWidth);
        sb.Append("|gh=").Append(gridHeight);
        sb.Append("|exit=").Append(exitRow);

        for (int i = 0; i < entries.Count; i++)
        {
            VehicleEntry e = entries[i];
            sb.Append('|');
            sb.Append(e.isTarget ? 'T' : 'N');
            sb.Append(':');
            sb.Append(e.horizontal ? 'H' : 'V');
            sb.Append(e.length);
            sb.Append('@');
            sb.Append(e.x);
            sb.Append(',');
            sb.Append(e.y);
        }

        return sb.ToString();
    }

    private static int CompareEntries(VehicleEntry a, VehicleEntry b)
    {
        // 1) target eerst
        if (a.isTarget != b.isTarget)
        {
            return a.isTarget ? -1 : 1;
        }

        // 2) x
        int cmp = a.x.CompareTo(b.x);
        if (cmp != 0)
        {
            return cmp;
        }

        // 3) y
        cmp = a.y.CompareTo(b.y);
        if (cmp != 0)
        {
            return cmp;
        }

        // 4) orientation (Horizontal vóór Vertical)
        cmp = a.horizontal.CompareTo(b.horizontal);
        if (cmp != 0)
        {
            // horizontal=true moet vóór false → reverse bool compare
            return -cmp;
        }

        // 5) length
        return a.length.CompareTo(b.length);
    }

    private readonly struct VehicleEntry
    {
        public readonly bool isTarget;
        public readonly int x;
        public readonly int y;
        public readonly bool horizontal;
        public readonly int length;

        public VehicleEntry(bool isTarget, int x, int y, bool horizontal, int length)
        {
            this.isTarget = isTarget;
            this.x = x;
            this.y = y;
            this.horizontal = horizontal;
            this.length = length;
        }
    }
}

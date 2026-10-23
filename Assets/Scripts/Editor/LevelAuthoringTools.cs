using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor checks for a manually authored LevelData asset.
/// Does not generate, rewrite, or insert levels.
/// </summary>
public static class LevelAuthoringTools
{
    [MenuItem("RushOut/Levels/Validate Selected Level")]
    public static void ValidateSelected()
    {
        LevelData level = Selection.activeObject as LevelData;
        if (level == null)
        {
            Debug.LogError("[Levels] Select a LevelData asset first.");
            return;
        }

        var report = new StringBuilder();
        int errors = 0;
        int warnings = 0;
        report.AppendLine("[Levels] Validate " + level.name);

        int width = level.ResolvedGridWidth;
        int height = level.ResolvedGridHeight;
        if (width < LevelData.MinGridSize || width > LevelData.MaxGridSize ||
            height < LevelData.MinGridSize || height > LevelData.MaxGridSize)
        {
            errors++;
            report.AppendLine("ERROR invalid grid " + width + "x" + height);
        }

        if (level.exitRow < 0 || level.exitRow >= height)
        {
            errors++;
            report.AppendLine("ERROR exit row " + level.exitRow + " is outside the grid");
        }

        if (level.vehicles == null || level.vehicles.Count == 0)
        {
            errors++;
            report.AppendLine("ERROR no vehicles");
        }
        else
        {
            int targets = 0;
            var occupied = new HashSet<Vector2Int>();
            for (int i = 0; i < level.vehicles.Count; i++)
            {
                VehicleData vehicle = level.vehicles[i];
                if (vehicle == null)
                {
                    errors++;
                    report.AppendLine("ERROR vehicle " + i + " is missing");
                    continue;
                }

                if (vehicle.canExitRight)
                {
                    targets++;
                }

                if (vehicle.orientation != VehicleController.VehicleOrientation.Horizontal &&
                    vehicle.orientation != VehicleController.VehicleOrientation.Vertical)
                {
                    errors++;
                    report.AppendLine("ERROR vehicle " + i + " has an invalid orientation");
                }

                if (vehicle.lengthInCells < 2 || vehicle.lengthInCells > 4)
                {
                    errors++;
                    report.AppendLine(
                        "ERROR vehicle " + i + " length " + vehicle.lengthInCells +
                        " is not 2, 3, or 4");
                }

                bool horizontal =
                    vehicle.orientation == VehicleController.VehicleOrientation.Horizontal;
                for (int step = 0; step < vehicle.lengthInCells; step++)
                {
                    int x = vehicle.gridPosition.x + (horizontal ? step : 0);
                    int y = vehicle.gridPosition.y + (horizontal ? 0 : step);
                    if (x < 0 || y < 0 || x >= width || y >= height)
                    {
                        errors++;
                        report.AppendLine("ERROR vehicle " + i + " extends outside the grid");
                        break;
                    }

                    var cell = new Vector2Int(x, y);
                    if (!occupied.Add(cell))
                    {
                        errors++;
                        report.AppendLine(
                            "ERROR vehicle " + i + " overlaps another vehicle at " + cell);
                    }
                }

                if (vehicle.canExitRight && !horizontal)
                {
                    errors++;
                    report.AppendLine(
                        "ERROR vehicle " + i +
                        " is marked as the exit target but is not horizontal");
                }

                if (vehicle.canExitRight && vehicle.gridPosition.y != level.exitRow)
                {
                    errors++;
                    report.AppendLine(
                        "ERROR target vehicle " + i +
                        " is not on exit row " + level.exitRow);
                }
            }

            if (targets == 0)
            {
                errors++;
                report.AppendLine("ERROR no target vehicle (canExitRight)");
            }
            else if (targets > 1 &&
                     level.objectiveType != LevelObjectiveType.MultiTargetRescue)
            {
                errors++;
                report.AppendLine("ERROR multiple target vehicles on a single-target level");
            }
        }

        LevelDatabase database = FindMainDatabase();
        int databaseIndex = IndexOf(database, level);
        if (database == null)
        {
            warnings++;
            report.AppendLine("WARNING MainLevelDatabase was not found");
        }
        else if (databaseIndex < 0)
        {
            warnings++;
            report.AppendLine("WARNING this level is not in MainLevelDatabase");
        }
        else
        {
            report.AppendLine("database index " + databaseIndex);
            for (int i = 0; i < database.levels.Count; i++)
            {
                LevelData other = database.levels[i];
                if (other == null || other == level || i == databaseIndex)
                {
                    continue;
                }

                if (other.name == level.name)
                {
                    errors++;
                    report.AppendLine("ERROR duplicate asset name also at database index " + i);
                }
            }
        }

        if (!LevelMinMoves.IsValid(level))
        {
            errors++;
            report.AppendLine("ERROR minimumMoves " + level.minimumMoves + " is not a valid par");
        }

        LevelDifficulty expected = ExpectedDifficulty(level.minimumMoves);
        if (LevelMinMoves.IsValid(level) && level.difficulty != expected)
        {
            warnings++;
            report.AppendLine(
                "WARNING difficulty is " + level.difficulty +
                " but minimumMoves " + level.minimumMoves +
                " falls in " + expected);
        }

        if (level.objectiveType == LevelObjectiveType.TimedAmbulance &&
            level.timeLimitSeconds <= 0f)
        {
            warnings++;
            report.AppendLine("WARNING timed objective has no time limit");
        }

        if (level.objectiveType != LevelObjectiveType.Classic &&
            level.specialTargetSprite == null)
        {
            warnings++;
            report.AppendLine("WARNING special objective has no specialTargetSprite");
        }

        RushOutSolver.SolverResult solved = RushOutSolver.SolveLevelData(
            level,
            RushOutSolver.DefaultMaxStates,
            -1,
            ObjectiveSolveConstraints.FromLevelData(level));
        if (solved == null || !solved.solvable)
        {
            errors++;
            report.AppendLine("ERROR solver did not find a solution");
        }
        else
        {
            report.AppendLine("solver minimumMoves " + solved.minimumMoves);
            if (level.minimumMoves != solved.minimumMoves)
            {
                warnings++;
                report.AppendLine(
                    "WARNING stored minimumMoves " + level.minimumMoves +
                    " does not match the solver. The asset was not changed.");
            }
        }

        report.AppendLine(errors + " errors, " + warnings + " warnings");
        if (errors > 0)
        {
            Debug.LogError(report.ToString());
        }
        else if (warnings > 0)
        {
            Debug.LogWarning(report.ToString());
        }
        else
        {
            Debug.Log(report.ToString());
        }
    }

    [MenuItem("RushOut/Levels/Preview Level Placement")]
    public static void PreviewPlacement()
    {
        LevelData level = Selection.activeObject as LevelData;
        if (level == null)
        {
            Debug.LogError("[Levels] Select a LevelData asset first.");
            return;
        }

        LevelDatabase database = FindMainDatabase();
        if (database == null || database.levels == null)
        {
            Debug.LogError("[Levels] MainLevelDatabase was not found.");
            return;
        }

        int existing = IndexOf(database, level);
        var preview = new List<LevelData>();
        for (int i = 0; i < database.levels.Count; i++)
        {
            if (database.levels[i] != null)
            {
                preview.Add(database.levels[i]);
            }
        }

        if (existing < 0)
        {
            preview.Add(level);
        }

        var same = new List<LevelData>();
        for (int i = 0; i < preview.Count; i++)
        {
            if (preview[i].difficulty == level.difficulty)
            {
                same.Add(preview[i]);
            }
        }

        int appendedIndex = database.levels.Count;
        same.Sort((a, b) => LevelDifficultyOrder.CompareLevels(
            a,
            b,
            TieIndex(database, a, level, existing, appendedIndex),
            TieIndex(database, b, level, existing, appendedIndex)));

        int place = 0;
        for (int i = 0; i < same.Count; i++)
        {
            if (same[i] == level)
            {
                place = i;
                break;
            }
        }

        var report = new StringBuilder();
        report.AppendLine("[Levels] Placement preview only. Database was not changed.");
        report.AppendLine("asset " + level.name);
        report.AppendLine(
            "grid " + level.ResolvedGridWidth + "x" + level.ResolvedGridHeight);
        report.AppendLine("minimumMoves " + level.minimumMoves);
        report.AppendLine("difficulty " + level.difficulty);
        report.AppendLine("difficultyScore " + level.difficultyScore);
        report.AppendLine(
            existing >= 0
                ? "database index " + existing
                : "not currently in MainLevelDatabase");
        report.AppendLine(
            "player-facing position if ordered with current rules: " +
            level.difficulty + " #" + (place + 1));

        int from = Mathf.Max(0, place - 2);
        int to = Mathf.Min(same.Count - 1, place + 2);
        for (int i = from; i <= to; i++)
        {
            LevelData neighbor = same[i];
            string marker = neighbor == level ? " NEW SELECTED LEVEL" : " " + neighbor.name;
            report.AppendLine(
                level.difficulty + " #" + (i + 1) + marker +
                " minimumMoves " + neighbor.minimumMoves +
                " difficultyScore " + neighbor.difficultyScore);
        }

        Debug.Log(report.ToString());
    }

    private static LevelDatabase FindMainDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("t:LevelDatabase MainLevelDatabase");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            LevelDatabase database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);
            if (database != null && database.name == "MainLevelDatabase")
            {
                return database;
            }
        }

        return null;
    }

    private static int TieIndex(
        LevelDatabase database,
        LevelData candidate,
        LevelData selected,
        int selectedIndex,
        int appendedIndex)
    {
        if (candidate == selected)
        {
            return selectedIndex >= 0 ? selectedIndex : appendedIndex;
        }

        int index = IndexOf(database, candidate);
        return index >= 0 ? index : appendedIndex;
    }

    private static int IndexOf(LevelDatabase database, LevelData level)
    {
        if (database == null || database.levels == null || level == null)
        {
            return -1;
        }

        for (int i = 0; i < database.levels.Count; i++)
        {
            if (database.levels[i] == level)
            {
                return i;
            }
        }

        return -1;
    }

    private static LevelDifficulty ExpectedDifficulty(int minimumMoves)
    {
        if (minimumMoves <= 9)
        {
            return LevelDifficulty.Easy;
        }

        if (minimumMoves <= 15)
        {
            return LevelDifficulty.Medium;
        }

        return LevelDifficulty.Hard;
    }
}

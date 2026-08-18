using UnityEngine;

/// <summary>
/// PlayerPrefs helpers voor first-time objective tutorials.
/// Keys: RushOut_TutorialSeen_&lt;ObjectiveType&gt; (0 = niet gezien, 1 = gezien).
/// </summary>
public static class ObjectiveTutorialPrefs
{
    private const string KeyPrefix = "RushOut_TutorialSeen_";

    public static string KeyFor(LevelObjectiveType objectiveType)
    {
        return KeyPrefix + objectiveType.ToString();
    }

    public static bool HasSeenTutorial(LevelObjectiveType objectiveType)
    {
        return PlayerPrefs.GetInt(KeyFor(objectiveType), 0) == 1;
    }

    /// <summary>
    /// Markeert tutorial als gezien. Alleen aanroepen na Got It.
    /// </summary>
    public static void MarkTutorialSeen(LevelObjectiveType objectiveType)
    {
        PlayerPrefs.SetInt(KeyFor(objectiveType), 1);
        PlayerPrefs.Save();
    }

    public static void ResetTutorial(LevelObjectiveType objectiveType)
    {
        PlayerPrefs.DeleteKey(KeyFor(objectiveType));
        PlayerPrefs.Save();
    }

    public static void ResetAllTutorials()
    {
        foreach (LevelObjectiveType type in System.Enum.GetValues(typeof(LevelObjectiveType)))
        {
            PlayerPrefs.DeleteKey(KeyFor(type));
        }

        PlayerPrefs.Save();
    }
}

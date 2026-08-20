using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only: reset first-time objective + feature tutorial PlayerPrefs keys.
/// </summary>
public static class ObjectiveTutorialEditorMenu
{
    private const string MenuRoot = "RushOut/Tutorials/";
    private const string TestingRoot = "RushOut/Testing/";

    [MenuItem(MenuRoot + "Reset All Objective Tutorials", priority = 100)]
    private static void ResetAllObjectiveTutorials()
    {
        ObjectiveTutorialPrefs.ResetAllTutorials();
        Debug.Log(
            "[ObjectiveTutorial] Alle RushOut_TutorialSeen_* keys gereset. " +
            "First-time tutorials verschijnen opnieuw."
        );
    }

    [MenuItem(MenuRoot + "Reset Classic Tutorial", priority = 200)]
    private static void ResetClassic() => ResetOne(LevelObjectiveType.Classic);

    [MenuItem(MenuRoot + "Reset TimedAmbulance Tutorial", priority = 201)]
    private static void ResetTimed() => ResetOne(LevelObjectiveType.TimedAmbulance);

    [MenuItem(MenuRoot + "Reset MoveLimit Tutorial", priority = 202)]
    private static void ResetMoveLimit() => ResetOne(LevelObjectiveType.MoveLimit);

    [MenuItem(MenuRoot + "Reset MultiTargetRescue Tutorial", priority = 203)]
    private static void ResetMultiTarget() => ResetOne(LevelObjectiveType.MultiTargetRescue);

    [MenuItem(MenuRoot + "Reset NoTouchChallenge Tutorial", priority = 204)]
    private static void ResetNoTouch() => ResetOne(LevelObjectiveType.NoTouchChallenge);

    [MenuItem(MenuRoot + "Reset FragileCargo Tutorial", priority = 205)]
    private static void ResetFragile() => ResetOne(LevelObjectiveType.FragileCargo);

    [MenuItem(MenuRoot + "Reset LimitedVehicle Tutorial", priority = 206)]
    private static void ResetLimited() => ResetOne(LevelObjectiveType.LimitedVehicle);

    [MenuItem(MenuRoot + "Reset Feature Tutorials", priority = 300)]
    private static void ResetAllFeatureTutorials()
    {
        FeatureTutorialPrefs.ResetAll();
        Debug.Log(
            "[FeatureTutorial] Alle RushOut_FeatureTutorial_* keys gereset."
        );
    }

    [MenuItem(MenuRoot + "Reset Coins Tutorial", priority = 301)]
    private static void ResetCoins()
    {
        FeatureTutorialPrefs.Reset(FeatureTutorialType.Coins);
        string key = FeatureTutorialPrefs.KeyFor(FeatureTutorialType.Coins);
        Debug.Log(
            "[CoinsFT]\n" +
            "Stage=ResetCoinsTutorial\n" +
            "Key=" + key + "\n" +
            "HasSeenAfterReset=" + FeatureTutorialPrefs.HasSeen(FeatureTutorialType.Coins)
        );
    }

    [MenuItem(MenuRoot + "Reset Skins Tutorial", priority = 304)]
    private static void ResetSkins() => ResetFeature(FeatureTutorialType.Skins);

    [MenuItem(MenuRoot + "Show Skins Tutorial Eligibility", priority = 351)]
    private static void ShowSkinsEligibility()
    {
        if (!Application.isPlaying)
        {
            bool coinsSeen = FeatureTutorialPrefs.HasSeen(FeatureTutorialType.Coins);
            bool skinsSeen = FeatureTutorialPrefs.HasSeen(FeatureTutorialType.Skins);
            Debug.Log(
                "[SkinsFT]\n" +
                "Stage=Eligibility\n" +
                "CoinsSeen=" + coinsSeen + "\n" +
                "SkinsSeen=" + skinsSeen + "\n" +
                "ShopTarget=(requires Play Mode)\n" +
                "Eligible=" + (coinsSeen && !skinsSeen) + " (prefs only; scene flags need Play Mode)"
            );
            return;
        }

        FeatureTutorialController controller =
            Object.FindAnyObjectByType<FeatureTutorialController>(
                FindObjectsInactive.Include);

        if (controller == null)
        {
            Debug.LogError("[SkinsFT] Geen FeatureTutorialController in scene.");
            return;
        }

        controller.EditorLogSkinsEligibility();
    }

    [MenuItem(MenuRoot + "Show Coins Tutorial Now", priority = 350)]
    private static void ShowCoinsTutorialNow()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "[FeatureTutorial] Show Coins Tutorial Now vereist Play Mode."
            );
            return;
        }

        FeatureTutorialController controller =
            Object.FindAnyObjectByType<FeatureTutorialController>(
                FindObjectsInactive.Include);

        if (controller == null)
        {
            Debug.LogError(
                "[FeatureTutorial] Geen FeatureTutorialController in scene."
            );
            return;
        }

        controller.EditorShowCoinsTutorialNow();
        Debug.Log(
            "[FeatureTutorial] Show Coins Tutorial Now — presentation only " +
            "(geen coins). Got It markeert wel seen."
        );
    }

    [MenuItem(TestingRoot + "Reset Current Level Three-Star Coin Claim", priority = 400)]
    private static void ResetCurrentLevelThreeStarClaim()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "[ThreeStarReward] Reset Current Level Claim vereist Play Mode " +
                "(huidige LevelManager index)."
            );
            return;
        }

        LevelManager levelManager = Object.FindAnyObjectByType<LevelManager>();
        SaveManager saveManager = Object.FindAnyObjectByType<SaveManager>();

        if (levelManager == null || saveManager == null)
        {
            Debug.LogError(
                "[ThreeStarReward] LevelManager of SaveManager ontbreekt."
            );
            return;
        }

        int index = levelManager.CurrentLevelIndex;
        string key = "RushOut_ThreeStarCoinRewardClaimed_" + index;
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();

        Debug.Log(
            "[ThreeStarReward] Cleared claim for LevelIndex=" + index + "\n" +
            "Key=" + key + "\n" +
            "HasClaimedNow=" + saveManager.HasClaimedThreeStarCoinReward(index) + "\n" +
            "Note: HasClaimed kan migration opnieuw toepassen als stars al 3 zijn " +
            "en migration-key ontbreekt — check Eligible via CompleteLevel log."
        );
    }

    private static void ResetOne(LevelObjectiveType type)
    {
        ObjectiveTutorialPrefs.ResetTutorial(type);
        Debug.Log(
            "[ObjectiveTutorial] Reset: " + ObjectiveTutorialPrefs.KeyFor(type)
        );
    }

    private static void ResetFeature(FeatureTutorialType type)
    {
        FeatureTutorialPrefs.Reset(type);
        Debug.Log("[FeatureTutorial] Reset: " + FeatureTutorialPrefs.KeyFor(type));
    }
}

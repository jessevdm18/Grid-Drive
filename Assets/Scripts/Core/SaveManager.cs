using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slaat levelprogressie op met PlayerPrefs.
/// Zet dit script op een GameObject in je scene (bijv. "SaveManager").
/// </summary>
public class SaveManager : MonoBehaviour
{
    // Duidelijke keys voor PlayerPrefs (geen typos tussen Save en Get).
    private const string UnlockedLevelKey = "RushOut_UnlockedLevel";
    private const string CurrentLevelKey = "RushOut_CurrentLevel";
    private const string StarsKeyPrefix = "LevelStars_";
    private const string StarsMaxIndexKey = "LevelStars_MaxIndex";
    private const string ThreeStarCoinClaimedPrefix = "RushOut_ThreeStarCoinRewardClaimed_";
    private const string ThreeStarCoinClaimedMaxIndexKey =
        "RushOut_ThreeStarCoinRewardClaimed_MaxIndex";
    /// <summary>Eenmalige legacy migration (alleen levels die al 3★ hadden vóór deze feature).</summary>
    private const string ThreeStarRewardMigrationCompletedKey =
        "RushOut_ThreeStarRewardMigrationCompleted";
    /// <summary>Legacy key — alleen gelezen om migratie niet opnieuw te draaien.</summary>
    private const string ThreeStarCoinClaimMigrationLegacyKey =
        "RushOut_ThreeStarCoinClaimMigrationV1";
    private const string FirstLaunchCompletedKey = "RushOut_FirstLaunchCompleted";
    private const string CoinsKey = "RushOut_Coins";

    /// <summary>Same key CoinManager uses — exposed for editor fresh-reset validation.</summary>
    public const string CoinsPrefsKey = "RushOut_Coins";
    /// <summary>Laatst gekozen LevelSelect difficulty-tab (int = LevelDifficulty).</summary>
    private const string LastSelectedDifficultyKey = "RushOut_LastSelectedDifficulty";

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only: één-shot force first-launch route (Splash → first Easy ordered level).
    /// </summary>
    public const string EditorForceFirstLaunchKey = "RushOut_EditorForceFirstLaunch";
#endif

    /// <summary>
    /// Slaat de hoogste unlocked level-index op.
    /// Zet progressie NOOIT terug — alleen een hogere index wordt bewaard.
    /// </summary>
    public void SaveUnlockedLevel(int levelIndex)
    {
        int current = GetUnlockedLevel();
        if (levelIndex <= current)
        {
            return;
        }

        PlayerPrefs.SetInt(UnlockedLevelKey, levelIndex);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Slaat het laatst gespeelde level op.
    /// </summary>
    public void SaveCurrentLevel(int levelIndex)
    {
        PlayerPrefs.SetInt(CurrentLevelKey, levelIndex);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Geeft de hoogste unlocked level-index terug.
    /// Default = 0 (eerste level).
    /// </summary>
    public int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt(UnlockedLevelKey, 0);
    }

    /// <summary>
    /// Geeft het laatst gespeelde level terug.
    /// Missing key → fresh Easy Level 1 DB index (not hardcoded 0).
    /// </summary>
    public int GetCurrentLevel()
    {
        if (!PlayerPrefs.HasKey(CurrentLevelKey))
        {
            return ResolveFreshStartDatabaseIndex(null);
        }

        return PlayerPrefs.GetInt(CurrentLevelKey, 0);
    }

    /// <summary>
    /// True when RushOut_CurrentLevel has been written.
    /// </summary>
    public static bool HasCurrentLevelKey()
    {
        return PlayerPrefs.HasKey(CurrentLevelKey);
    }

    /// <summary>
    /// Raw FirstLaunchCompleted without migration side-effects.
    /// </summary>
    public static bool HasFirstLaunchCompletedKeyRaw()
    {
        return PlayerPrefs.GetInt(FirstLaunchCompletedKey, 0) == 1;
    }

    /// <summary>
    /// Slaat de laatst gekozen LevelSelect difficulty-tab op.
    /// </summary>
    public void SaveLastSelectedDifficulty(LevelDifficulty difficulty)
    {
        PlayerPrefs.SetInt(LastSelectedDifficultyKey, (int)difficulty);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Laatst gekozen LevelSelect difficulty. Default = Easy.
    /// Ongeldige waarden → Easy.
    /// </summary>
    public LevelDifficulty GetLastSelectedDifficulty()
    {
        int raw = PlayerPrefs.GetInt(
            LastSelectedDifficultyKey,
            (int)LevelDifficulty.Easy
        );

        if (raw < (int)LevelDifficulty.Easy || raw > (int)LevelDifficulty.Hard)
        {
            return LevelDifficulty.Easy;
        }

        return (LevelDifficulty)raw;
    }

    /// <summary>
    /// True als first-launch routing al afgerond is (of gemigreerd voor bestaande installs).
    /// </summary>
    public bool HasCompletedFirstLaunch()
    {
        ResolveFirstLaunchMigration();
        return PlayerPrefs.GetInt(FirstLaunchCompletedKey, 0) == 1;
    }

    /// <summary>
    /// Markeert first-launch als afgerond (idempotent).
    /// </summary>
    public void MarkFirstLaunchCompleted()
    {
        MarkFirstLaunchCompletedStatic();
    }

    /// <summary>
    /// Splash/startup: true → direct Gameplay Easy Level 1 (ordered), geen MainMenu.
    /// Roept migratie aan voor bestaande spelers zonder FirstLaunchCompleted-key.
    /// </summary>
    public static bool ShouldRouteFirstLaunchToGameplay()
    {
        ResolveFirstLaunchMigrationStatic();

#if UNITY_EDITOR
        if (PlayerPrefs.GetInt(EditorForceFirstLaunchKey, 0) == 1)
        {
            PlayerPrefs.DeleteKey(EditorForceFirstLaunchKey);
            PrepareFirstLaunchGameplayLevelStatic(null);
            return true;
        }
#endif

        return PlayerPrefs.GetInt(FirstLaunchCompletedKey, 0) != 1;
    }

    /// <summary>
    /// Instance-wrapper: migratie voor bestaande progress vóór first-launch checks.
    /// </summary>
    public void ResolveFirstLaunchMigration()
    {
        ResolveFirstLaunchMigrationStatic();
    }

    /// <summary>
    /// Sets current level to the first Easy level in difficulty-local order
    /// (same as LevelSelect Easy button 1). Not hardcoded DB index 0.
    /// Clears Editor V1 playtest override so it cannot hijack fresh start.
    /// </summary>
    public static void PrepareFirstLaunchGameplayLevelStatic(LevelDatabase database = null)
    {
#if UNITY_EDITOR
        V1PlaytestOverride.Clear();
#endif
        int freshIndex = ResolveFreshStartDatabaseIndex(database);
        PlayerPrefs.SetInt(CurrentLevelKey, freshIndex);
        PlayerPrefs.Save();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LevelData asset = database != null ? database.GetLevel(freshIndex) : null;
#if UNITY_EDITOR
        if (asset == null)
        {
            LevelDatabase resolved = database != null ? database : EditorLoadMainLevelDatabase();
            asset = resolved != null ? resolved.GetLevel(freshIndex) : null;
        }
#endif
        Debug.Log(
            "[FreshStartTrace]\n" +
            "Stage=PrepareFirstLaunch\n" +
            "FirstEasyDbIndex=" + freshIndex + "\n" +
            "Asset=" + (asset != null ? asset.name : "?") + "\n" +
            "DisplayNumber=1\n" +
            "V1OverrideCleared=true"
        );
        Debug.Log(
            "[FreshStartTrace]\n" +
            "Stage=SaveCurrentLevel\n" +
            "DbIndex=" + freshIndex
        );
#endif
    }

    /// <summary>
    /// Authoritative fresh-start DB index = first ordered Easy level.
    /// Falls back to 0 only if database/Easy list unavailable.
    /// </summary>
    public static int ResolveFreshStartDatabaseIndex(LevelDatabase database)
    {
        LevelDatabase resolved = database;
#if UNITY_EDITOR
        if (resolved == null)
        {
            resolved = EditorLoadMainLevelDatabase();
        }
#endif
        if (resolved == null)
        {
            Debug.LogWarning(
                "SaveManager: no LevelDatabase for fresh-start index — fallback DB 0."
            );
            return 0;
        }

        int firstEasy = LevelDifficultyOrder.GetFirstOrderedLevelIndex(
            resolved,
            LevelDifficulty.Easy
        );
        if (firstEasy < 0)
        {
            Debug.LogWarning(
                "SaveManager: no Easy levels in database — fallback DB 0."
            );
            return 0;
        }

        return firstEasy;
    }

#if UNITY_EDITOR
    private static LevelDatabase EditorLoadMainLevelDatabase()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("MainLevelDatabase t:LevelDatabase");
        if (guids == null || guids.Length == 0)
        {
            return null;
        }

        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
        return UnityEditor.AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);
    }
#endif

    /// <summary>
    /// Zet FirstLaunchCompleted=1. Statisch zodat Splash/LevelManager geen race hebben.
    /// </summary>
    public static void MarkFirstLaunchCompletedStatic()
    {
        if (PlayerPrefs.GetInt(FirstLaunchCompletedKey, 0) == 1)
        {
            return;
        }

        PlayerPrefs.SetInt(FirstLaunchCompletedKey, 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Bestaande installs zonder FirstLaunchCompleted-key → returning player.
    /// </summary>
    public static void ResolveFirstLaunchMigrationStatic()
    {
        if (PlayerPrefs.GetInt(FirstLaunchCompletedKey, 0) == 1)
        {
            return;
        }

        if (HasExistingProgressEvidenceStatic())
        {
            MarkFirstLaunchCompletedStatic();
        }
    }

    /// <summary>
    /// Bewijs dat iemand al eerder gespeeld/geüpdatet heeft (niet alleen missing first-launch key).
    /// </summary>
    public static bool HasExistingProgressEvidenceStatic()
    {
        if (PlayerPrefs.HasKey(UnlockedLevelKey))
        {
            return true;
        }

        if (PlayerPrefs.HasKey(CurrentLevelKey))
        {
            return true;
        }

        if (PlayerPrefs.HasKey(StarsMaxIndexKey))
        {
            int maxIndex = PlayerPrefs.GetInt(StarsMaxIndexKey, -1);
            for (int i = 0; i <= maxIndex; i++)
            {
                if (PlayerPrefs.GetInt(StarsKeyPrefix + i, 0) > 0)
                {
                    return true;
                }
            }
        }

        // CoinManager schrijft pas bij wijziging — key aanwezig ⇒ eerdere sessie.
        if (PlayerPrefs.HasKey(CoinsKey))
        {
            return true;
        }

        return false;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor: alleen FirstLaunchCompleted wissen (migratie kan returning herstellen).
    /// </summary>
    public static void EditorResetFirstLaunchCompletedKey()
    {
        PlayerPrefs.DeleteKey(FirstLaunchCompletedKey);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Editor: forceer één volgende Splash→Gameplay Level 1 zonder progress te wissen.
    /// </summary>
    public static void EditorArmSimulateFreshFirstLaunch()
    {
        PlayerPrefs.SetInt(EditorForceFirstLaunchKey, 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Editor-only: wipe all player progression prefs to simulate a brand-new install.
    /// Does not touch EditorPrefs, curation, or project assets.
    /// Coins key is deleted (not written) so first-launch migration stays clean.
    /// </summary>
    public static void EditorResetAllPlayerProgressPrefs()
    {
        PlayerPrefs.DeleteKey(UnlockedLevelKey);
        PlayerPrefs.DeleteKey(CurrentLevelKey);
        PlayerPrefs.DeleteKey(LastSelectedDifficultyKey);
        PlayerPrefs.DeleteKey(FirstLaunchCompletedKey);
        PlayerPrefs.DeleteKey(EditorForceFirstLaunchKey);
        PlayerPrefs.DeleteKey(CoinsKey);

        // Wide scan so orphaned stars/claims without max-index are also cleared.
        const int scanLimit = 512;
        int maxStars = Mathf.Max(PlayerPrefs.GetInt(StarsMaxIndexKey, -1), scanLimit - 1);
        for (int i = 0; i <= maxStars; i++)
        {
            PlayerPrefs.DeleteKey(StarsKeyPrefix + i);
        }

        PlayerPrefs.DeleteKey(StarsMaxIndexKey);

        int maxClaimed = Mathf.Max(
            PlayerPrefs.GetInt(ThreeStarCoinClaimedMaxIndexKey, -1),
            scanLimit - 1
        );
        for (int i = 0; i <= maxClaimed; i++)
        {
            PlayerPrefs.DeleteKey(ThreeStarCoinClaimedPrefix + i);
        }

        PlayerPrefs.DeleteKey(ThreeStarCoinClaimedMaxIndexKey);
        PlayerPrefs.DeleteKey(ThreeStarRewardMigrationCompletedKey);
        PlayerPrefs.DeleteKey(ThreeStarCoinClaimMigrationLegacyKey);
        PlayerPrefs.Save();
    }

    /// <summary>Editor validation helpers (defaults match fresh install).</summary>
    public static int EditorGetCurrentLevelOrDefault()
    {
        return PlayerPrefs.GetInt(CurrentLevelKey, 0);
    }

    public static bool EditorHasFirstLaunchCompletedKey()
    {
        return PlayerPrefs.GetInt(FirstLaunchCompletedKey, 0) == 1;
    }

    public static bool EditorHasCoinsKey()
    {
        return PlayerPrefs.HasKey(CoinsKey);
    }

    public static int EditorCountStarsWithValue()
    {
        int count = 0;
        int maxIndex = PlayerPrefs.GetInt(StarsMaxIndexKey, -1);
        for (int i = 0; i <= maxIndex; i++)
        {
            if (PlayerPrefs.GetInt(StarsKeyPrefix + i, 0) > 0)
            {
                count++;
            }
        }

        return count;
    }

    public static int EditorCountThreeStarClaims()
    {
        int count = 0;
        int maxIndex = PlayerPrefs.GetInt(ThreeStarCoinClaimedMaxIndexKey, -1);
        for (int i = 0; i <= maxIndex; i++)
        {
            if (PlayerPrefs.GetInt(ThreeStarCoinClaimedPrefix + i, 0) == 1)
            {
                count++;
            }
        }

        return count;
    }
#endif

    /// <summary>
    /// Repareert unlocked progress op basis van bestaande sterren.
    /// Verwijdert geen PlayerPrefs; zet unlocked alleen omhoog indien nodig.
    /// </summary>
    public void RepairUnlockedProgress(int totalLevels)
    {
        if (totalLevels <= 0)
        {
            return;
        }

        int maxValidIndex = totalLevels - 1;
        int oldUnlocked = Mathf.Clamp(GetUnlockedLevel(), 0, maxValidIndex);

        // Hoogste level met sterren > 0 geldt als voltooid.
        int highestCompleted = -1;
        int starScanLimit = Mathf.Max(
            PlayerPrefs.GetInt(StarsMaxIndexKey, -1),
            maxValidIndex
        );

        for (int i = 0; i <= starScanLimit; i++)
        {
            if (i > maxValidIndex)
            {
                break;
            }

            if (GetStarsForLevel(i) > 0)
            {
                highestCompleted = i;
            }
        }

        // Voltooid level X ⇒ minstens X+1 unlocked (geclamped).
        int reconstructed = highestCompleted >= 0
            ? highestCompleted + 1
            : 0;
        reconstructed = Mathf.Clamp(reconstructed, 0, maxValidIndex);

        int repaired = Mathf.Max(oldUnlocked, reconstructed);
        repaired = Mathf.Clamp(repaired, 0, maxValidIndex);

        Debug.Log(
            "Progress repair | old unlocked=" + oldUnlocked +
            " | repaired unlocked=" + repaired
        );

        if (repaired > oldUnlocked)
        {
            PlayerPrefs.SetInt(UnlockedLevelKey, repaired);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Slaat sterren voor een level op. Bewaart alleen de hoogste score (0–3).
    /// </summary>
    public void SaveStarsForLevel(int levelIndex, int stars)
    {
        stars = Mathf.Clamp(stars, 0, 3);

        int existing = GetStarsForLevel(levelIndex);
        if (stars <= existing)
        {
            return;
        }

        PlayerPrefs.SetInt(StarsKeyPrefix + levelIndex, stars);

        // Bijhouden tot welk index we sterren hebben, voor ResetProgress.
        int maxIndex = PlayerPrefs.GetInt(StarsMaxIndexKey, -1);
        if (levelIndex > maxIndex)
        {
            PlayerPrefs.SetInt(StarsMaxIndexKey, levelIndex);
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Geeft opgeslagen sterren voor een level (default 0 als nooit gespeeld).
    /// </summary>
    public int GetStarsForLevel(int levelIndex)
    {
        return Mathf.Clamp(
            PlayerPrefs.GetInt(StarsKeyPrefix + levelIndex, 0),
            0,
            3
        );
    }

    /// <summary>
    /// Authoritative completion: ooit voltooid als best stars &gt; 0.
    /// </summary>
    public bool IsLevelCompleted(int levelIndex)
    {
        return GetStarsForLevel(levelIndex) > 0;
    }

    /// <summary>
    /// Unieke completed levels met de huidige <see cref="LevelData.difficulty"/> metadata.
    /// Geen sticky counter — volgt retags van difficulty op LevelData.
    /// </summary>
    public int GetCompletedCount(LevelDifficulty difficulty, LevelDatabase database)
    {
        if (database == null)
        {
            return 0;
        }

        int count = 0;
        int levelCount = database.LevelCount;
        for (int i = 0; i < levelCount; i++)
        {
            LevelData level = database.GetLevel(i);
            if (level == null)
            {
                continue;
            }

            if (level.difficulty == difficulty && IsLevelCompleted(i))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Of de speler deze difficulty mag kiezen (geen UI-logic).
    /// Null config → built-in defaults (10 / 10+10).
    /// </summary>
    public bool IsDifficultyUnlocked(
        LevelDifficulty difficulty,
        LevelDatabase database,
        DifficultyProgressionConfig config)
    {
        int easyCompleted = GetCompletedCount(LevelDifficulty.Easy, database);
        int mediumCompleted = GetCompletedCount(LevelDifficulty.Medium, database);

        if (config != null)
        {
            return config.IsDifficultyUnlocked(
                difficulty,
                easyCompleted,
                mediumCompleted
            );
        }

        return DifficultyProgressionConfig.IsDifficultyUnlockedWithDefaults(
            difficulty,
            easyCompleted,
            mediumCompleted
        );
    }

    /// <summary>
    /// Speelbaar level: difficulty unlocked + eerste in difficulty-local order,
    /// of vorige item in die same ordered list completed
    /// (of dit level zelf al completed — behoudt toegang na retag / replay).
    /// Order: minimumMoves → difficultyScore → dbIndex (LevelDifficultyOrder).
    /// </summary>
    public bool IsLevelUnlocked(
        int levelIndex,
        LevelDatabase database,
        DifficultyProgressionConfig config)
    {
        if (database == null)
        {
            return false;
        }

        LevelData level = database.GetLevel(levelIndex);
        if (level == null)
        {
            return false;
        }

        if (!IsDifficultyUnlocked(level.difficulty, database, config))
        {
            return false;
        }

        // Al voltooid → altijd speelbaar binnen unlocked difficulty.
        if (IsLevelCompleted(levelIndex))
        {
            return true;
        }

        List<int> ordered = LevelDifficultyOrder.GetOrderedLevelIndicesForDifficulty(
            database,
            level.difficulty
        );

        int position = -1;
        for (int i = 0; i < ordered.Count; i++)
        {
            if (ordered[i] == levelIndex)
            {
                position = i;
                break;
            }
        }

        if (position < 0)
        {
            return false;
        }

        // Eerste in difficulty-local progression → unlocked als difficulty unlocked.
        if (position == 0)
        {
            return true;
        }

        // Later level: vorige in dezelfde ordered list moet completed zijn.
        return IsLevelCompleted(ordered[position - 1]);
    }

    /// <summary>
    /// True als dit level zijn eenmalige 3-ster coin reward al heeft uitgekeerd.
    /// Zelfde index-semantics als LevelStars_*.
    /// </summary>
    public bool HasClaimedThreeStarCoinReward(int levelIndex)
    {
        MigrateExistingThreeStarClaimsIfNeeded();
        return PlayerPrefs.GetInt(ThreeStarCoinClaimedPrefix + levelIndex, 0) == 1;
    }

    /// <summary>
    /// Markeert de eenmalige 3-ster coin reward als uitbetaald (idempotent).
    /// </summary>
    public void MarkThreeStarCoinRewardClaimed(int levelIndex)
    {
        MigrateExistingThreeStarClaimsIfNeeded();

        if (PlayerPrefs.GetInt(ThreeStarCoinClaimedPrefix + levelIndex, 0) == 1)
        {
            return;
        }

        PlayerPrefs.SetInt(ThreeStarCoinClaimedPrefix + levelIndex, 1);

        int maxIndex = PlayerPrefs.GetInt(ThreeStarCoinClaimedMaxIndexKey, -1);
        if (levelIndex > maxIndex)
        {
            PlayerPrefs.SetInt(ThreeStarCoinClaimedMaxIndexKey, levelIndex);
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// True als de speler ooit minstens één 3-ster coin reward heeft ontvangen.
    /// </summary>
    public bool HasClaimedAnyThreeStarCoinReward()
    {
        MigrateExistingThreeStarClaimsIfNeeded();

        int maxIndex = PlayerPrefs.GetInt(ThreeStarCoinClaimedMaxIndexKey, -1);
        for (int i = 0; i <= maxIndex; i++)
        {
            if (PlayerPrefs.GetInt(ThreeStarCoinClaimedPrefix + i, 0) == 1)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// True als een 3-ster finish op dit level nog een coin reward zou geven.
    /// </summary>
    public bool IsThreeStarCoinRewardAvailable(int levelIndex)
    {
        return !HasClaimedThreeStarCoinReward(levelIndex);
    }

    /// <summary>
    /// Oude installs: levels die AL 3★ hadden vóór deze feature → claimed.
    /// Eenmalig. Roept nooit opnieuw aan na completion van nieuwe 3★ saves.
    /// Los van FirstLaunch-migratie.
    /// </summary>
    private void MigrateExistingThreeStarClaimsIfNeeded()
    {
        if (PlayerPrefs.GetInt(ThreeStarRewardMigrationCompletedKey, 0) == 1)
        {
            return;
        }

        // Eerdere versie al gedraaid → niet opnieuw (zou net-uitbetaalde 3★ kunnen claimen).
        if (PlayerPrefs.GetInt(ThreeStarCoinClaimMigrationLegacyKey, 0) == 1)
        {
            PlayerPrefs.SetInt(ThreeStarRewardMigrationCompletedKey, 1);
            PlayerPrefs.Save();
            return;
        }

        int maxStarsIndex = PlayerPrefs.GetInt(StarsMaxIndexKey, -1);
        for (int i = 0; i <= maxStarsIndex; i++)
        {
            // Alleen dit level: bestaande best stars >= 3.
            if (GetStarsForLevel(i) < 3)
            {
                continue;
            }

            if (PlayerPrefs.GetInt(ThreeStarCoinClaimedPrefix + i, 0) == 1)
            {
                continue;
            }

            PlayerPrefs.SetInt(ThreeStarCoinClaimedPrefix + i, 1);
            int claimedMax = PlayerPrefs.GetInt(ThreeStarCoinClaimedMaxIndexKey, -1);
            if (i > claimedMax)
            {
                PlayerPrefs.SetInt(ThreeStarCoinClaimedMaxIndexKey, i);
            }
        }

        PlayerPrefs.SetInt(ThreeStarRewardMigrationCompletedKey, 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Wis alle opgeslagen progressie.
    /// </summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(UnlockedLevelKey);
        PlayerPrefs.DeleteKey(CurrentLevelKey);
        PlayerPrefs.DeleteKey(LastSelectedDifficultyKey);

        // Verwijder alle LevelStars_X keys die we ooit hebben aangemaakt.
        int maxIndex = PlayerPrefs.GetInt(StarsMaxIndexKey, -1);
        for (int i = 0; i <= maxIndex; i++)
        {
            PlayerPrefs.DeleteKey(StarsKeyPrefix + i);
        }

        PlayerPrefs.DeleteKey(StarsMaxIndexKey);

        int claimedMax = PlayerPrefs.GetInt(ThreeStarCoinClaimedMaxIndexKey, -1);
        for (int i = 0; i <= claimedMax; i++)
        {
            PlayerPrefs.DeleteKey(ThreeStarCoinClaimedPrefix + i);
        }

        PlayerPrefs.DeleteKey(ThreeStarCoinClaimedMaxIndexKey);
        PlayerPrefs.DeleteKey(ThreeStarRewardMigrationCompletedKey);
        PlayerPrefs.DeleteKey(ThreeStarCoinClaimMigrationLegacyKey);
        PlayerPrefs.Save();
    }
}

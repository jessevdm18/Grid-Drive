using System;
using UnityEngine;

/// <summary>
/// Authoritative lives + offline regeneration (PlayerPrefs economy state).
/// DDOL singleton — available across Splash / MainMenu / LevelSelect / Gameplay.
/// Phase 1–2: persistence, offline regen, FailLevel consumption + attempt gate
/// (no move limits / shop / ads / out-of-lives UI yet).
/// </summary>
public class LivesManager : MonoBehaviour
{
    public const int MaxLives = 5;
    public const int StartingLives = 5;

    /// <summary>2-hour regeneration interval.</summary>
    public static readonly TimeSpan RegenInterval = TimeSpan.FromHours(2);

    public const string LivesPrefsKey = "RushOut_Lives";
    public const string NextLifeUtcTicksPrefsKey = "RushOut_NextLifeUtcTicks";

    /// <summary>Sentinel: no active regeneration countdown.</summary>
    private const long NoNextLifeTicks = 0L;

    public static LivesManager Instance { get; private set; }

    /// <summary>UI / systems: current lives after clamp (0..MaxLives).</summary>
    public event Action<int> OnLivesChanged;

    /// <summary>
    /// Fired when a genuine gameplay attempt is blocked because lives == 0.
    /// Parameter = source (e.g. restart, next, scene_gameplay). Phase 5/6 UI hook.
    /// </summary>
    public event Action<string> OnLevelAttemptBlocked;

    private int lives;
    private long nextLifeUtcTicks;
    private bool resumeRecalcQueued;

#if UNITY_EDITOR
    /// <summary>
    /// When true, Awake skips DDOL/singleton claim so Edit Mode validation hosts work
    /// (Unity does not reliably run Awake for AddComponent outside Play Mode).
    /// </summary>
    private static bool creatingEphemeralEditorValidationHost;
#endif

    public int CurrentLives => lives;
    public int MaxLivesCount => MaxLives;
    public bool HasLives => lives > 0;
    public bool IsFull => lives >= MaxLives;

    /// <summary>
    /// Time until the next life is due. Zero when full or no active timer.
    /// Never negative.
    /// </summary>
    public TimeSpan TimeUntilNextLife
    {
        get
        {
            if (IsFull || !HasActiveNextLifeTimestamp)
            {
                return TimeSpan.Zero;
            }

            TimeSpan remaining = GetNextLifeUtc() - DateTime.UtcNow;
            return remaining <= TimeSpan.Zero ? TimeSpan.Zero : remaining;
        }
    }

    /// <summary>True when a next-life UTC timestamp is stored and lives are below max.</summary>
    public bool HasActiveNextLifeTimestamp =>
        !IsFull && nextLifeUtcTicks > NoNextLifeTicks;

    /// <summary>UTC instant of next life, or null when no active timer.</summary>
    public DateTime? NextLifeUtc
    {
        get
        {
            if (!HasActiveNextLifeTimestamp)
            {
                return null;
            }

            return GetNextLifeUtc();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapAfterSceneLoad()
    {
        EnsureInstance();
    }

    /// <summary>Returns the DDOL instance, creating one if needed.</summary>
    public static LivesManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        LivesManager existing = FindAnyObjectByType<LivesManager>();
        if (existing != null)
        {
            Instance = existing;
            return Instance;
        }

        GameObject go = new GameObject("LivesManager");
        go.AddComponent<LivesManager>();
        return Instance;
    }

    /// <summary>
    /// Deletes lives prefs (fresh install). Does not touch coins/progression.
    /// Safe to call from Editor full-reset outside Play Mode.
    /// </summary>
    public static void DeleteAllPersistedPrefs()
    {
        PlayerPrefs.DeleteKey(LivesPrefsKey);
        PlayerPrefs.DeleteKey(NextLifeUtcTicksPrefsKey);
    }

    private void Awake()
    {
#if UNITY_EDITOR
        if (creatingEphemeralEditorValidationHost)
        {
            // Ephemeral Edit Mode / validation host — no DDOL, no Instance claim.
            return;
        }
#endif

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadFromPrefs();
        RecalculateRegeneration(DateTime.UtcNow, "Awake");
        LogState("Initialized");
    }

#if UNITY_EDITOR
    /// <summary>
    /// Creates a HideAndDontSave host for A–O Edit Mode validation.
    /// Does not use RuntimeInitializeOnLoadMethod or claim <see cref="Instance"/>.
    /// Caller must <see cref="DestroyEphemeralEditorValidationHost"/>.
    /// </summary>
    public static LivesManager CreateEphemeralEditorValidationHost()
    {
        creatingEphemeralEditorValidationHost = true;
        GameObject go = null;
        LivesManager host;
        try
        {
            go = new GameObject("LivesManager_EditorValidation");
            go.hideFlags = HideFlags.HideAndDontSave;
            host = go.AddComponent<LivesManager>();
        }
        finally
        {
            creatingEphemeralEditorValidationHost = false;
        }

        if (host == null)
        {
            if (go != null)
            {
                UnityEngine.Object.DestroyImmediate(go);
            }

            return null;
        }

        // Edit Mode: Awake usually did not run. Play Mode: Awake returned early via flag.
        host.lives = StartingLives;
        host.ClearNextLifeTimestamp();
        return host;
    }

    /// <summary>Destroys an ephemeral validation host; never leaves it in the scene.</summary>
    public static void DestroyEphemeralEditorValidationHost(LivesManager host)
    {
        if (host == null)
        {
            return;
        }

        if (Instance == host)
        {
            Instance = null;
        }

        GameObject go = host.gameObject;
        if (go != null)
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    /// <summary>Load prefs only (no UtcNow regen) — for clamp tests in Edit Mode.</summary>
    public void EditorLoadFromPrefsOnly()
    {
        LoadFromPrefs();
    }
#endif

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            return;
        }

        QueueResumeRecalculation();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            return;
        }

        QueueResumeRecalculation();
    }

    private void QueueResumeRecalculation()
    {
        if (resumeRecalcQueued)
        {
            return;
        }

        resumeRecalcQueued = true;
        RecalculateRegeneration(DateTime.UtcNow, "Resume");
        resumeRecalcQueued = false;
    }

    /// <summary>
    /// True when a real player attempt may begin (lives &gt; 0 after regen).
    /// Does not consume a life. V1 playtest callers should use
    /// <see cref="TryBeginLevelAttempt"/> with bypass instead.
    /// </summary>
    public bool CanStartLevelAttempt()
    {
        RecalculateRegeneration(DateTime.UtcNow, "CanStart");
        return HasLives;
    }

    /// <summary>
    /// Central zero-lives gate. Does NOT consume a life on success.
    /// Returns false when CurrentLives == 0 (unless <paramref name="bypassForV1Playtest"/>).
    /// </summary>
    public bool TryBeginLevelAttempt(string source, bool bypassForV1Playtest = false)
    {
        if (bypassForV1Playtest)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                "[Lives] V1 playtest bypass: attempt allowed | source=" +
                (string.IsNullOrEmpty(source) ? "unknown" : source)
            );
#endif
            return true;
        }

        RecalculateRegeneration(DateTime.UtcNow, "TryBegin");

        if (HasLives)
        {
            return true;
        }

        string src = string.IsNullOrEmpty(source) ? "unknown" : source;
        Debug.Log(
            "[Lives] Level attempt blocked: 0/" + MaxLives +
            " | source=" + src
        );
        OnLevelAttemptBlocked?.Invoke(src);
        return false;
    }

    /// <summary>
    /// Removes exactly one life. False if already 0.
    /// Full → below full starts a new timer; already regenerating preserves it.
    /// </summary>
    public bool TryRemoveLife()
    {
        return TryRemoveLife(DateTime.UtcNow);
    }

    /// <summary>Testable overload: uses <paramref name="utcNow"/> for regen + timer start.</summary>
    public bool TryRemoveLife(DateTime utcNow)
    {
        RecalculateRegeneration(utcNow, "PreRemove");

        if (lives <= 0)
        {
            LogDev("TryRemoveLife failed — already 0");
            return false;
        }

        bool wasFull = lives >= MaxLives;
        lives--;

        if (wasFull)
        {
            SetNextLifeUtc(utcNow + RegenInterval);
            LogDev(
                "TryRemoveLife → " + lives + "/" + MaxLives +
                " — started regen due " + FormatUtc(GetNextLifeUtc())
            );
        }
        else
        {
            EnsureValidTimerWhileBelowMax(utcNow);
            LogDev(
                "TryRemoveLife → " + lives + "/" + MaxLives +
                " — preserved regen due " +
                (HasActiveNextLifeTimestamp
                    ? FormatUtc(GetNextLifeUtc())
                    : "(none)")
            );
        }

        PersistAndNotify();
        return true;
    }

    /// <summary>
    /// Adds lives, clamped to MaxLives. False if amount &lt;= 0 or already full.
    /// Reaching max clears the timer; staying below preserves an active timer.
    /// </summary>
    public bool AddLife(int amount = 1)
    {
        return AddLife(amount, DateTime.UtcNow);
    }

    /// <summary>Testable overload: uses <paramref name="utcNow"/> for regen + timer recovery.</summary>
    public bool AddLife(int amount, DateTime utcNow)
    {
        if (amount <= 0)
        {
            return false;
        }

        RecalculateRegeneration(utcNow, "PreAdd");

        if (lives >= MaxLives)
        {
            ClearNextLifeTimestamp();
            LogDev("AddLife skipped — already full");
            return false;
        }

        int before = lives;
        lives = Mathf.Min(MaxLives, lives + amount);

        if (lives >= MaxLives)
        {
            ClearNextLifeTimestamp();
            LogDev(
                "AddLife " + before + "→" + lives +
                " — reached full, timer cleared"
            );
        }
        else
        {
            EnsureValidTimerWhileBelowMax(utcNow);
            LogDev(
                "AddLife " + before + "→" + lives +
                " — regen due " + FormatUtc(GetNextLifeUtc())
            );
        }

        PersistAndNotify();
        return true;
    }

    /// <summary>Recalculates offline regeneration using DateTime.UtcNow.</summary>
    public void RecalculateRegeneration()
    {
        RecalculateRegeneration(DateTime.UtcNow, "Manual");
    }

    /// <summary>
    /// Core regen. Advances from the STORED next-life timestamp (not UtcNow+interval).
    /// Idempotent for the same UTC instant — no duplicate grants.
    /// </summary>
    public void RecalculateRegeneration(DateTime utcNow, string reason)
    {
        int livesBefore = lives;
        long ticksBefore = nextLifeUtcTicks;

        lives = Mathf.Clamp(lives, 0, MaxLives);

        if (lives >= MaxLives)
        {
            if (nextLifeUtcTicks != NoNextLifeTicks)
            {
                ClearNextLifeTimestamp();
                PersistAndNotify();
                LogDev("[" + reason + "] Full — timer cleared");
            }
            else if (lives != livesBefore)
            {
                PersistAndNotify();
            }

            return;
        }

        // Below max with invalid/missing schedule → start interval from now (no free lives).
        if (!HasActiveNextLifeTimestamp)
        {
            SetNextLifeUtc(utcNow + RegenInterval);
            PersistAndNotify();
            LogDev(
                "[" + reason + "] Missing/invalid timestamp at " + lives +
                "/" + MaxLives + " — started interval due " + FormatUtc(GetNextLifeUtc())
            );
            return;
        }

        DateTime due = GetNextLifeUtc();
        if (utcNow < due)
        {
            if (lives != livesBefore || nextLifeUtcTicks != ticksBefore)
            {
                PersistAndNotify();
            }

            return;
        }

        int granted = 0;
        while (lives < MaxLives && utcNow >= due)
        {
            lives++;
            granted++;
            due = due.Add(RegenInterval);
        }

        if (lives >= MaxLives)
        {
            ClearNextLifeTimestamp();
            LogDev(
                "[" + reason + "] Offline regen +" + granted +
                " → " + lives + "/" + MaxLives + " — timer cleared"
            );
        }
        else
        {
            SetNextLifeUtc(due);
            LogDev(
                "[" + reason + "] Offline regen +" + granted +
                " → " + lives + "/" + MaxLives +
                " — next due " + FormatUtc(due)
            );
        }

        if (lives != livesBefore || nextLifeUtcTicks != ticksBefore)
        {
            PersistAndNotify();
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>Editor/DEV: set next-life due to UtcNow (does not change the clock).</summary>
    public void DebugForceNextLifeDueNow()
    {
        if (lives >= MaxLives)
        {
            LogDev("ForceNextLifeDueNow ignored — already full");
            return;
        }

        SetNextLifeUtc(DateTime.UtcNow);
        PersistAndNotify();
        LogDev("ForceNextLifeDueNow — due set to now");
    }

    /// <summary>Editor/DEV: reload prefs into this instance (after external pref edits).</summary>
    public void DebugReloadFromPrefs()
    {
        LoadFromPrefs();
        RecalculateRegeneration(DateTime.UtcNow, "DebugReload");
        LogState("DebugReloadFromPrefs");
    }

    /// <summary>
    /// Editor/DEV: inject exact lives + optional next-due without granting from UtcNow.
    /// Does not run offline regen — caller may RecalculateRegeneration(simulatedUtc).
    /// </summary>
    public void EditorSetStateForTesting(int desiredLives, DateTime? nextDueUtc)
    {
        lives = Mathf.Clamp(desiredLives, 0, MaxLives);
        if (lives >= MaxLives || !nextDueUtc.HasValue)
        {
            ClearNextLifeTimestamp();
        }
        else
        {
            SetNextLifeUtc(DateTime.SpecifyKind(nextDueUtc.Value, DateTimeKind.Utc));
        }

        PersistAndNotify();
        LogDev(
            "EditorSetState lives=" + lives +
            (HasActiveNextLifeTimestamp
                ? " due=" + FormatUtc(GetNextLifeUtc())
                : " due=(none)")
        );
    }
#endif

    /// <summary>
    /// After FULL RESET prefs wipe: restore in-memory state to fresh 5/5 if instance alive.
    /// </summary>
    public void ReinitializeToFreshDefaults()
    {
        lives = StartingLives;
        ClearNextLifeTimestamp();
        PersistAndNotify();
        LogState("ReinitializeToFreshDefaults");
    }

    private void LoadFromPrefs()
    {
        if (!PlayerPrefs.HasKey(LivesPrefsKey))
        {
            lives = StartingLives;
            ClearNextLifeTimestamp();
            SavePrefs();
            LogDev("LoadFromPrefs — missing key → fresh " + StartingLives + "/" + MaxLives);
            return;
        }

        lives = Mathf.Clamp(PlayerPrefs.GetInt(LivesPrefsKey, StartingLives), 0, MaxLives);

        string rawTicks = PlayerPrefs.GetString(NextLifeUtcTicksPrefsKey, string.Empty);
        if (string.IsNullOrEmpty(rawTicks) ||
            !long.TryParse(rawTicks, out long parsed) ||
            parsed <= NoNextLifeTicks)
        {
            nextLifeUtcTicks = NoNextLifeTicks;
        }
        else
        {
            // Reject absurdly malformed ticks (outside DateTime range).
            try
            {
                _ = new DateTime(parsed, DateTimeKind.Utc);
                nextLifeUtcTicks = parsed;
            }
            catch (ArgumentOutOfRangeException)
            {
                nextLifeUtcTicks = NoNextLifeTicks;
                LogDev("LoadFromPrefs — malformed ticks discarded");
            }
        }

        if (lives >= MaxLives)
        {
            nextLifeUtcTicks = NoNextLifeTicks;
        }

        LogDev(
            "LoadFromPrefs lives=" + lives +
            (HasActiveNextLifeTimestamp
                ? " due=" + FormatUtc(GetNextLifeUtc())
                : " (no timer)")
        );
    }

    private void EnsureValidTimerWhileBelowMax(DateTime utcNow)
    {
        if (lives >= MaxLives)
        {
            ClearNextLifeTimestamp();
            return;
        }

        if (!HasActiveNextLifeTimestamp)
        {
            SetNextLifeUtc(utcNow + RegenInterval);
        }
    }

    private void SetNextLifeUtc(DateTime utc)
    {
        nextLifeUtcTicks = DateTime.SpecifyKind(utc, DateTimeKind.Utc).Ticks;
    }

    private void ClearNextLifeTimestamp()
    {
        nextLifeUtcTicks = NoNextLifeTicks;
    }

    private DateTime GetNextLifeUtc()
    {
        return new DateTime(nextLifeUtcTicks, DateTimeKind.Utc);
    }

    private void PersistAndNotify()
    {
        SavePrefs();
        OnLivesChanged?.Invoke(lives);
    }

    private void SavePrefs()
    {
        PlayerPrefs.SetInt(LivesPrefsKey, lives);
        if (nextLifeUtcTicks == NoNextLifeTicks)
        {
            PlayerPrefs.DeleteKey(NextLifeUtcTicksPrefsKey);
        }
        else
        {
            PlayerPrefs.SetString(NextLifeUtcTicksPrefsKey, nextLifeUtcTicks.ToString());
        }

        PlayerPrefs.Save();
    }

    private void LogState(string label)
    {
        LogDev(
            label + " lives=" + lives + "/" + MaxLives +
            " full=" + IsFull +
            " untilNext=" + TimeUntilNextLife +
            (HasActiveNextLifeTimestamp
                ? " dueUtc=" + FormatUtc(GetNextLifeUtc())
                : " dueUtc=(none)")
        );
    }

    private static string FormatUtc(DateTime utc)
    {
        return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("u");
    }

    private static void LogDev(string message)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[Lives] " + message);
#endif
    }
}

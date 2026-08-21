using UnityEngine;

/// <summary>
/// Persistente achtergrondmuziek (DontDestroyOnLoad).
/// Scene-AudioManagers blijven verantwoordelijk voor SFX.
/// Bootstrap via <see cref="EnsureInstance"/> zodat Splash→Gameplay
/// (zonder MainMenu) alsnog muziek krijgt.
/// </summary>
public class MusicManager : MonoBehaviour
{
    private const string MusicEnabledKey = "MusicEnabled";
    private const string DefaultResourcesClipPath = "Music/Music_Main";

    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;

    [Header("Tracks (optional — Resources fallback if empty)")]
    [SerializeField] private AudioClip menuMusicClip;
    [SerializeField] private AudioClip gameplayMusicClip;

    [SerializeField, Range(0f, 1f)]
    private float musicVolume = 0.4f;

    private AudioClip activeClip;
    private bool bootstrappedAtRuntime;

    private static bool creatingRuntimeInstance;

    public bool MusicEnabled { get; private set; } = true;

    /// <summary>
    /// Returns existing DDOL instance, or creates one with Resources fallback clip.
    /// </summary>
    public static MusicManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        MusicManager existing = FindAnyObjectByType<MusicManager>();
        if (existing != null)
        {
            return existing;
        }

        GameObject go = new GameObject("MusicManager");
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;

        creatingRuntimeInstance = true;
        go.AddComponent<MusicManager>();
        creatingRuntimeInstance = false;

        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        bootstrappedAtRuntime = creatingRuntimeInstance;

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        EnsureDefaultClipAssigned();
        LoadMusicSetting();
        ApplyMusicSetting();

        // Scene-placed MainMenu MusicManager: start immediately (returning players).
        // Runtime bootstrap waits for PlayMenuMusic / PlayGameplayMusic.
        if (!bootstrappedAtRuntime)
        {
            EnsureMusicPlaying();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>Authoritative menu music entry (MainMenu).</summary>
    public static void PlayMenuMusic()
    {
        MusicManager manager = EnsureInstance();
        if (manager == null)
        {
            return;
        }

        manager.PlayTrack(manager.ResolveMenuClip(), "Menu");
    }

    /// <summary>Authoritative gameplay music entry (Gameplay).</summary>
    public static void PlayGameplayMusic()
    {
        MusicManager manager = EnsureInstance();
        if (manager == null)
        {
            return;
        }

        manager.PlayTrack(manager.ResolveGameplayClip(), "Gameplay");
    }

    public void ToggleMusic()
    {
        SetMusicEnabled(!MusicEnabled);
    }

    public void SetMusicEnabled(bool enabled)
    {
        MusicEnabled = enabled;
        PlayerPrefs.SetInt(MusicEnabledKey, MusicEnabled ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicSetting();

        if (MusicEnabled)
        {
            EnsureMusicPlaying();
        }
    }

    private void PlayTrack(AudioClip clip, string context)
    {
        if (musicSource == null)
        {
            Debug.LogWarning("MusicManager: musicSource ontbreekt (" + context + ").");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning(
                "MusicManager: geen clip voor " + context +
                " (assign menu/gameplay clip or Resources/" +
                DefaultResourcesClipPath + ")."
            );
            return;
        }

        // Same track already playing — do not restart (next level / restart).
        if (musicSource.isPlaying && activeClip == clip)
        {
            ApplyMusicSetting();
            return;
        }

        if (musicSource.clip != clip)
        {
            if (musicSource.isPlaying)
            {
                musicSource.Stop();
            }

            musicSource.clip = clip;
        }

        activeClip = clip;
        musicSource.loop = true;
        ApplyMusicSetting();

        if (!MusicEnabled)
        {
            if (musicSource.isPlaying)
            {
                musicSource.Stop();
            }

            return;
        }

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[Music]\n" +
            "Context=" + context + "\n" +
            "Clip=" + clip.name + "\n" +
            "Playing=" + musicSource.isPlaying + "\n" +
            "Enabled=" + MusicEnabled + "\n" +
            "Bootstrapped=" + bootstrappedAtRuntime
        );
#endif
    }

    private AudioClip ResolveMenuClip()
    {
        if (menuMusicClip != null)
        {
            return menuMusicClip;
        }

        return ResolveFallbackClip();
    }

    private AudioClip ResolveGameplayClip()
    {
        if (gameplayMusicClip != null)
        {
            return gameplayMusicClip;
        }

        // Prefer menu clip if only one is wired on the scene instance.
        if (menuMusicClip != null)
        {
            return menuMusicClip;
        }

        return ResolveFallbackClip();
    }

    private AudioClip ResolveFallbackClip()
    {
        if (musicSource != null && musicSource.clip != null)
        {
            return musicSource.clip;
        }

        return Resources.Load<AudioClip>(DefaultResourcesClipPath);
    }

    private void EnsureDefaultClipAssigned()
    {
        if (musicSource == null)
        {
            return;
        }

        if (musicSource.clip != null)
        {
            activeClip = musicSource.clip;
            return;
        }

        AudioClip fallback = ResolveFallbackClip();
        if (fallback != null)
        {
            musicSource.clip = fallback;
            activeClip = fallback;
        }
    }

    private void LoadMusicSetting()
    {
        MusicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;
    }

    private void ApplyMusicSetting()
    {
        if (musicSource == null)
        {
            return;
        }

        musicSource.volume = Mathf.Clamp01(musicVolume);
        musicSource.mute = !MusicEnabled;
    }

    private void EnsureMusicPlaying()
    {
        if (musicSource == null)
        {
            return;
        }

        if (musicSource.clip == null)
        {
            EnsureDefaultClipAssigned();
        }

        if (musicSource.clip == null)
        {
            return;
        }

        musicSource.loop = true;
        activeClip = musicSource.clip;

        if (MusicEnabled && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
}

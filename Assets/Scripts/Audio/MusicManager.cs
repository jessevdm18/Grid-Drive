using UnityEngine;

/// <summary>
/// Persistente achtergrondmuziek (DontDestroyOnLoad).
/// Scene-AudioManagers blijven verantwoordelijk voor SFX.
/// </summary>
public class MusicManager : MonoBehaviour
{
    private const string MusicEnabledKey = "MusicEnabled";

    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;

    [SerializeField, Range(0f, 1f)]
    private float musicVolume = 0.4f;

    public bool MusicEnabled { get; private set; } = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadMusicSetting();
        ApplyMusicSetting();
        EnsureMusicPlaying();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
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
        if (musicSource == null || musicSource.clip == null)
        {
            return;
        }

        musicSource.loop = true;

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    private const string MusicEnabledKey = "MusicEnabled";
    private const string SfxEnabledKey = "SfxEnabled";

    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip blockedClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip buttonClip;

    [SerializeField, Range(0f, 1f)]
    private float sfxVolume = 1f;

    [Tooltip("AudioSource die de achtergrondmuziek speelt (apart van SFX).")]
    [SerializeField] private AudioSource musicSource;

    private AudioSource audioSource;

    public bool MusicEnabled { get; private set; } = true;
    public bool SfxEnabled { get; private set; } = true;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Optioneel: tweede AudioSource op dit object met loop = muziek.
        if (musicSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] != audioSource && sources[i].loop)
                {
                    musicSource = sources[i];
                    break;
                }
            }
        }

        LoadAudioSettings();
        ApplyMusicSetting();
    }

    /// <summary>
    /// Zet muziek aan/uit en bewaart de keuze.
    /// </summary>
    public void ToggleMusic()
    {
        MusicEnabled = !MusicEnabled;
        PlayerPrefs.SetInt(MusicEnabledKey, MusicEnabled ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicSetting();
    }

    /// <summary>
    /// Zet SFX aan/uit en bewaart de keuze.
    /// </summary>
    public void ToggleSfx()
    {
        SfxEnabled = !SfxEnabled;
        PlayerPrefs.SetInt(SfxEnabledKey, SfxEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        // Default true bij eerste installatie (key ontbreekt → 1).
        MusicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;
        SfxEnabled = PlayerPrefs.GetInt(SfxEnabledKey, 1) == 1;
    }

    private void ApplyMusicSetting()
    {
        if (musicSource == null)
        {
            return;
        }

        // mute behoudt volume/clip; muziek speelt stil verder of hoorbaar.
        musicSource.mute = !MusicEnabled;
    }

    public void PlayMove()
    {
        PlaySfx(moveClip);
    }

    public void PlayBlocked()
    {
        PlaySfx(blockedClip);
    }

    public void PlayWin()
    {
        PlaySfx(winClip);
    }

    public void PlayCoin()
    {
        PlaySfx(coinClip);
    }

    public void PlayButton()
    {
        PlaySfx(buttonClip);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (!SfxEnabled || clip == null || audioSource == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, sfxVolume);
    }
}

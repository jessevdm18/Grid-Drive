using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    private const string MusicEnabledKey = "MusicEnabled";
    private const string SfxEnabledKey = "SfxEnabled";

    [Header("SFX Clips")]
    [SerializeField] private AudioClip buttonClip;
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip blockedClip;
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip vehicleExitClip;
    [SerializeField] private AudioClip coinSpendClip;
    [SerializeField] private AudioClip hintClip;
    [SerializeField] private AudioClip skinSelectClip;
    [SerializeField] private AudioClip panelOpenClip;
    [SerializeField] private AudioClip panelCloseClip;
    [SerializeField] private AudioClip insufficientCoinsClip;
    [SerializeField] private AudioClip ambulanceMissionStartClip;
    [SerializeField] private AudioClip countdownBeepClip;
    [SerializeField] private AudioClip timedMissionFailedClip;
    [SerializeField] private AudioClip moveLimitWarningClip;
    [SerializeField] private AudioClip moveLimitFailedClip;

    [Header("SFX Volumes")]
    [SerializeField, Range(0f, 1f)] private float buttonVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float moveVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float blockedVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float coinVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float winVolume = 0.9f;
    [SerializeField, Range(0f, 1f)] private float upgradeVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float vehicleExitVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float coinSpendVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float hintVolume = 0.75f;
    [SerializeField, Range(0f, 1f)] private float skinSelectVolume = 0.65f;
    [SerializeField, Range(0f, 1f)] private float panelOpenVolume = 0.55f;
    [SerializeField, Range(0f, 1f)] private float panelCloseVolume = 0.50f;
    [SerializeField, Range(0f, 1f)] private float insufficientCoinsVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float ambulanceMissionStartVolume = 0.65f;
    [SerializeField, Range(0f, 1f)] private float countdownBeepVolume = 0.55f;
    [SerializeField, Range(0f, 1f)] private float timedMissionFailedVolume = 0.75f;
    [SerializeField, Range(0f, 1f)] private float moveLimitWarningVolume = 0.55f;
    [SerializeField, Range(0f, 1f)] private float moveLimitFailedVolume = 0.75f;

    [Header("SFX Pitch (repeating gameplay)")]
    [SerializeField, Range(0.8f, 1.2f)] private float movePitchMin = 0.96f;
    [SerializeField, Range(0.8f, 1.2f)] private float movePitchMax = 1.04f;
    [SerializeField, Range(0.8f, 1.2f)] private float blockedPitchMin = 0.98f;
    [SerializeField, Range(0.8f, 1.2f)] private float blockedPitchMax = 1.02f;

    [Header("Global")]
    [SerializeField, Range(0f, 1f)]
    private float sfxVolume = 1f;

    [Header("Music")]
    [SerializeField, Range(0f, 1f)]
    private float musicVolume = 0.4f;

    [Tooltip("Apart van SFX. Loop + clip in Inspector; optioneel Play On Awake.")]
    [SerializeField] private AudioSource musicSource;

    private AudioSource audioSource;

    /// <summary>
    /// Alleen voor pitch-variabele gameplay OneShots (move/blocked).
    /// Nodig omdat AudioSource.pitch alle PlayOneShot-voices op die source deelt.
    /// </summary>
    private AudioSource pitchedSfxSource;

    public bool MusicEnabled { get; private set; } = true;
    public bool SfxEnabled { get; private set; } = true;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.pitch = 1f;
        }

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

        EnsurePitchedSfxSource();

        LoadAudioSettings();
        ApplyMusicSetting();
        // Persistent muziek leeft in MusicManager — hier geen Play().
    }

    /// <summary>
    /// Zet muziek aan/uit en bewaart de keuze.
    /// Stuurt door naar MusicManager indien aanwezig.
    /// </summary>
    public void ToggleMusic()
    {
        bool nextEnabled = !MusicEnabled;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetMusicEnabled(nextEnabled);
            MusicEnabled = MusicManager.Instance.MusicEnabled;
            return;
        }

        MusicEnabled = nextEnabled;
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

        musicSource.volume = Mathf.Clamp01(musicVolume);
        // Alleen relevant als een lokale musicSource nog gekoppeld is (dev fallback).
        musicSource.mute = !MusicEnabled;
    }

    public void PlayButton()
    {
        PlaySfx(buttonClip, buttonVolume);
    }

    public void PlayMove()
    {
        PlaySfx(moveClip, moveVolume, movePitchMin, movePitchMax);
    }

    public void PlayBlocked()
    {
        PlaySfx(blockedClip, blockedVolume, blockedPitchMin, blockedPitchMax);
    }

    public void PlayCoin()
    {
        PlaySfx(coinClip, coinVolume);
    }

    public void PlayWin()
    {
        PlaySfx(winClip, winVolume);
    }

    /// <summary>
    /// Succesvolle aankoop/upgrade (shop). Niet voor button-press of skin-select.
    /// </summary>
    public void PlayUpgrade()
    {
        PlaySfx(upgradeClip, upgradeVolume);
    }

    /// <summary>
    /// Definitieve vehicle exit van het bord. Pitch vast 1.0.
    /// </summary>
    public void PlayVehicleExit()
    {
        PlaySfx(vehicleExitClip, vehicleExitVolume);
    }

    /// <summary>
    /// Succesvolle coin-uitgave (gameplay, bijv. hint). Pitch vast 1.0.
    /// Niet automatisch vanuit CoinManager — callers kiezen wanneer.
    /// </summary>
    public void PlayCoinSpend()
    {
        PlaySfx(coinSpendClip, coinSpendVolume);
    }

    /// <summary>
    /// Succesvolle hint-presentatie aan de speler. Pitch vast 1.0.
    /// </summary>
    public void PlayHint()
    {
        PlaySfx(hintClip, hintVolume);
    }

    /// <summary>
    /// Succesvolle skin-selectie (selected skin daadwerkelijk gewijzigd). Pitch vast 1.0.
    /// </summary>
    public void PlaySkinSelect()
    {
        PlaySfx(skinSelectClip, skinSelectVolume);
    }

    /// <summary>
    /// Panel van dicht → open. Pitch vast 1.0.
    /// </summary>
    public void PlayPanelOpen()
    {
        PlaySfx(panelOpenClip, panelOpenVolume);
    }

    /// <summary>
    /// Panel van open → dicht. Pitch vast 1.0.
    /// </summary>
    public void PlayPanelClose()
    {
        PlaySfx(panelCloseClip, panelCloseVolume);
    }

    /// <summary>
    /// Betaalde actie geweigerd door te weinig coins (bijv. hint). Pitch vast 1.0.
    /// </summary>
    public void PlayInsufficientCoins()
    {
        PlaySfx(insufficientCoinsClip, insufficientCoinsVolume);
    }

    /// <summary>
    /// TimedAmbulance: één keer bij gameplay-ready start (Running).
    /// </summary>
    public void PlayAmbulanceMissionStart()
    {
        PlaySfx(ambulanceMissionStartClip, ambulanceMissionStartVolume);
    }

    /// <summary>
    /// TimedAmbulance countdown 3/2/1. Optionele vaste pitch (pitched SFX source).
    /// </summary>
    public void PlayCountdownBeep(float pitch = 1f)
    {
        float p = Mathf.Clamp(pitch, 0.5f, 2f);
        PlaySfx(countdownBeepClip, countdownBeepVolume, p, p);
    }

    /// <summary>
    /// TimedAmbulance Time's Up / mission failed. Pitch vast 1.0.
    /// </summary>
    public void PlayTimedMissionFailed()
    {
        PlaySfx(timedMissionFailedClip, timedMissionFailedVolume);
    }

    /// <summary>
    /// MoveLimit: één keer wanneer remaining de warning-drempel bereikt.
    /// </summary>
    public void PlayMoveLimitWarning()
    {
        PlaySfx(moveLimitWarningClip, moveLimitWarningVolume);
    }

    /// <summary>
    /// MoveLimit: Out of Moves / mission failed. Pitch vast 1.0.
    /// </summary>
    public void PlayMoveLimitFailed()
    {
        PlaySfx(moveLimitFailedClip, moveLimitFailedVolume);
    }

    private void PlaySfx(
        AudioClip clip,
        float volumeScale,
        float pitchMin = 1f,
        float pitchMax = 1f)
    {
        if (!SfxEnabled || clip == null || audioSource == null)
        {
            return;
        }

        float volume = Mathf.Clamp01(sfxVolume * Mathf.Clamp01(volumeScale));
        bool usePitchVariation = !Mathf.Approximately(pitchMin, 1f) ||
                                 !Mathf.Approximately(pitchMax, 1f);

        if (!usePitchVariation)
        {
            // Vaste pitch 1.0 op de hoofd-SFX source — UI/reward blijven consistent.
            if (!Mathf.Approximately(audioSource.pitch, 1f))
            {
                audioSource.pitch = 1f;
            }

            audioSource.PlayOneShot(clip, volume);
            return;
        }

        AudioSource source = pitchedSfxSource != null ? pitchedSfxSource : audioSource;
        source.pitch = ResolvePitch(pitchMin, pitchMax);
        source.PlayOneShot(clip, volume);
        // Pitch niet terugzetten naar 1: dat zou overlappende OneShots op deze source
        // mid-play terugtrekken. Alleen move/blocked gebruiken deze source.
    }

    private static float ResolvePitch(float pitchMin, float pitchMax)
    {
        float min = pitchMin;
        float max = pitchMax;

        if (min > max)
        {
            float swap = min;
            min = max;
            max = swap;
        }

        if (Mathf.Approximately(min, max))
        {
            return min;
        }

        return Random.Range(min, max);
    }

    private void EnsurePitchedSfxSource()
    {
        // Zelfde GameObject, geen scene-YAML: isoleert pitch van UI/reward OneShots.
        pitchedSfxSource = gameObject.AddComponent<AudioSource>();
        pitchedSfxSource.playOnAwake = false;
        pitchedSfxSource.loop = false;
        pitchedSfxSource.mute = false;
        pitchedSfxSource.spatialBlend = 0f;
        pitchedSfxSource.priority = audioSource != null ? audioSource.priority : 128;
        pitchedSfxSource.volume = 1f;
        pitchedSfxSource.pitch = 1f;
        pitchedSfxSource.outputAudioMixerGroup =
            audioSource != null ? audioSource.outputAudioMixerGroup : null;
    }
}

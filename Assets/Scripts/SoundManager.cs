using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Manages game audio including music and sound effects volume and mute states.
/// Uses an AudioMixer for separate control of music and SFX channels.
/// Persists settings using PlayerPrefs.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Mixer Group Names")]
    [SerializeField] private string masterVolumeParameter = "MasterVolume";
    [SerializeField] private string musicVolumeParameter = "MusicVolume";
    [SerializeField] private string sfxVolumeParameter = "SFXVolume";

    // PlayerPrefs keys
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MASTER_MUTE_KEY = "MasterMute";
    private const string MUSIC_MUTE_KEY = "MusicMute";
    private const string SFX_MUTE_KEY = "SFXMute";

    // Default values
    private const float DEFAULT_VOLUME = 0.75f;
    private const bool DEFAULT_MUTE = false;

    // Current state
    private float masterVolume;
    private float musicVolume;
    private float sfxVolume;
    private bool masterMuted;
    private bool musicMuted;
    private bool sfxMuted;

    // Events for UI updates
    public event System.Action<float> OnMasterVolumeChanged;
    public event System.Action<float> OnMusicVolumeChanged;
    public event System.Action<float> OnSFXVolumeChanged;
    public event System.Action<bool> OnMasterMuteChanged;
    public event System.Action<bool> OnMusicMuteChanged;
    public event System.Action<bool> OnSFXMuteChanged;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved settings
        LoadSettings();
    }

    /// <summary>
    /// Loads audio settings from PlayerPrefs and applies them
    /// </summary>
    private void LoadSettings()
    {
        // Load volumes (0-1 range)
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, DEFAULT_VOLUME);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_VOLUME);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME);

        // Load mute states
        masterMuted = PlayerPrefs.GetInt(MASTER_MUTE_KEY, DEFAULT_MUTE ? 1 : 0) == 1;
        musicMuted = PlayerPrefs.GetInt(MUSIC_MUTE_KEY, DEFAULT_MUTE ? 1 : 0) == 1;
        sfxMuted = PlayerPrefs.GetInt(SFX_MUTE_KEY, DEFAULT_MUTE ? 1 : 0) == 1;

        // Apply settings to mixer
        ApplyMasterVolume();
        ApplyMusicVolume();
        ApplySFXVolume();
    }

    /// <summary>
    /// Sets the master volume (0-1 range)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
        PlayerPrefs.Save();

        ApplyMasterVolume();
        OnMasterVolumeChanged?.Invoke(masterVolume);
    }

    /// <summary>
    /// Sets the music volume (0-1 range)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.Save();

        ApplyMusicVolume();
        OnMusicVolumeChanged?.Invoke(musicVolume);
    }

    /// <summary>
    /// Sets the SFX volume (0-1 range)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();

        ApplySFXVolume();
        OnSFXVolumeChanged?.Invoke(sfxVolume);
    }

    /// <summary>
    /// Toggles master mute state
    /// </summary>
    public void ToggleMasterMute()
    {
        SetMasterMute(!masterMuted);
    }

    /// <summary>
    /// Sets master mute state
    /// </summary>
    public void SetMasterMute(bool mute)
    {
        masterMuted = mute;
        PlayerPrefs.SetInt(MASTER_MUTE_KEY, masterMuted ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMasterVolume();
        OnMasterMuteChanged?.Invoke(masterMuted);
    }

    /// <summary>
    /// Toggles music mute state
    /// </summary>
    public void ToggleMusicMute()
    {
        SetMusicMute(!musicMuted);
    }

    /// <summary>
    /// Sets music mute state
    /// </summary>
    public void SetMusicMute(bool mute)
    {
        musicMuted = mute;
        PlayerPrefs.SetInt(MUSIC_MUTE_KEY, musicMuted ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMusicVolume();
        OnMusicMuteChanged?.Invoke(musicMuted);
    }

    /// <summary>
    /// Toggles SFX mute state
    /// </summary>
    public void ToggleSFXMute()
    {
        SetSFXMute(!sfxMuted);
    }

    /// <summary>
    /// Sets SFX mute state
    /// </summary>
    public void SetSFXMute(bool mute)
    {
        sfxMuted = mute;
        PlayerPrefs.SetInt(SFX_MUTE_KEY, sfxMuted ? 1 : 0);
        PlayerPrefs.Save();

        ApplySFXVolume();
        OnSFXMuteChanged?.Invoke(sfxMuted);
    }

    /// <summary>
    /// Applies current master volume to the audio mixer
    /// </summary>
    private void ApplyMasterVolume()
    {
        if (audioMixer == null) return;

        // Convert linear volume (0-1) to decibels (-80 to 0)
        // When muted, set to -80dB (effectively silent)
        float db = masterMuted ? -80f : LinearToDecibel(masterVolume);
        audioMixer.SetFloat(masterVolumeParameter, db);
    }

    /// <summary>
    /// Applies current music volume to the audio mixer
    /// </summary>
    private void ApplyMusicVolume()
    {
        if (audioMixer == null) return;

        // Convert linear volume (0-1) to decibels (-80 to 0)
        // When muted, set to -80dB (effectively silent)
        float db = musicMuted ? -80f : LinearToDecibel(musicVolume);
        audioMixer.SetFloat(musicVolumeParameter, db);
    }

    /// <summary>
    /// Applies current SFX volume to the audio mixer
    /// </summary>
    private void ApplySFXVolume()
    {
        if (audioMixer == null) return;

        // Convert linear volume (0-1) to decibels (-80 to 0)
        // When muted, set to -80dB (effectively silent)
        float db = sfxMuted ? -80f : LinearToDecibel(sfxVolume);
        audioMixer.SetFloat(sfxVolumeParameter, db);
    }

    /// <summary>
    /// Converts linear volume (0-1) to decibels (-80 to 0)
    /// </summary>
    private float LinearToDecibel(float linear)
    {
        // Clamp to avoid log(0)
        linear = Mathf.Clamp(linear, 0.0001f, 1f);

        // Convert to decibels: dB = 20 * log10(linear)
        float db = 20f * Mathf.Log10(linear);

        // Clamp to reasonable range
        return Mathf.Clamp(db, -80f, 0f);
    }

    /// <summary>
    /// Gets current master volume (0-1)
    /// </summary>
    public float GetMasterVolume() => masterVolume;

    /// <summary>
    /// Gets current music volume (0-1)
    /// </summary>
    public float GetMusicVolume() => musicVolume;

    /// <summary>
    /// Gets current SFX volume (0-1)
    /// </summary>
    public float GetSFXVolume() => sfxVolume;

    /// <summary>
    /// Gets current master mute state
    /// </summary>
    public bool IsMasterMuted() => masterMuted;

    /// <summary>
    /// Gets current music mute state
    /// </summary>
    public bool IsMusicMuted() => musicMuted;

    /// <summary>
    /// Gets current SFX mute state
    /// </summary>
    public bool IsSFXMuted() => sfxMuted;

    /// <summary>
    /// Resets all audio settings to defaults
    /// </summary>
    public void ResetToDefaults()
    {
        SetMasterVolume(DEFAULT_VOLUME);
        SetMusicVolume(DEFAULT_VOLUME);
        SetSFXVolume(DEFAULT_VOLUME);
        SetMasterMute(DEFAULT_MUTE);
        SetMusicMute(DEFAULT_MUTE);
        SetSFXMute(DEFAULT_MUTE);
    }
}

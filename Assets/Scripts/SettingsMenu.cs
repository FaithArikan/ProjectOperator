using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Handles opening and closing the settings panel in the menu scene
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeSettingsButton;

    [Header("Audio Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void Start()
    {
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.AddListener(CloseSettings);
        }

        // Subscribe to audio controls
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Initialize audio UI with current values
        InitializeAudioUI();

        // Subscribe to SoundManager events
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.OnMasterVolumeChanged += UpdateMasterVolumeUI;
            SoundManager.Instance.OnMusicVolumeChanged += UpdateMusicVolumeUI;
            SoundManager.Instance.OnSFXVolumeChanged += UpdateSFXVolumeUI;
        }

        // Hide settings panel initially
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    [Header("Input Settings")]
    [SerializeField] private bool enableInput = true;

    private void Update()
    {
        if (enableInput && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleSettings();
        }
    }

    /// <summary>
    /// Opens the settings panel
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Closes the settings panel
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles the settings panel open/closed
    /// </summary>
    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    // Audio Control Methods

    /// <summary>
    /// Initializes audio UI elements with current SoundManager values
    /// </summary>
    private void InitializeAudioUI()
    {
        if (SoundManager.Instance == null) return;

        // Set slider values without triggering callbacks
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(SoundManager.Instance.GetMasterVolume());
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(SoundManager.Instance.GetMusicVolume());
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(SoundManager.Instance.GetSFXVolume());
        }
    }

    /// <summary>
    /// Called when master volume slider changes
    /// </summary>
    private void OnMasterVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(value);
        }
    }

    /// <summary>
    /// Called when music volume slider changes
    /// </summary>
    private void OnMusicVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMusicVolume(value);
        }
    }

    /// <summary>
    /// Called when SFX volume slider changes
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(value);
        }
    }

    /// <summary>
    /// Called when master mute button is clicked
    /// </summary>
    private void OnMasterMuteToggled()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ToggleMasterMute();
        }
    }

    /// <summary>
    /// Called when music mute button is clicked
    /// </summary>
    private void OnMusicMuteToggled()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ToggleMusicMute();
        }
    }

    /// <summary>
    /// Called when SFX mute button is clicked
    /// </summary>
    private void OnSFXMuteToggled()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ToggleSFXMute();
        }
    }

    /// <summary>
    /// Updates master volume slider UI (called by SoundManager event)
    /// </summary>
    private void UpdateMasterVolumeUI(float volume)
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(volume);
        }
    }

    /// <summary>
    /// Updates music volume slider UI (called by SoundManager event)
    /// </summary>
    private void UpdateMusicVolumeUI(float volume)
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(volume);
        }
    }

    /// <summary>
    /// Updates SFX volume slider UI (called by SoundManager event)
    /// </summary>
    private void UpdateSFXVolumeUI(float volume)
    {
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(volume);
        }
    }

}
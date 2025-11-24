using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Initialize audio UI with current values
        InitializeAudioUI();

        // Subscribe to SoundManager events
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.OnMasterVolumeChanged += UpdateMasterVolumeUI;
            SoundManager.Instance.OnMusicVolumeChanged += UpdateMusicVolumeUI;
            SoundManager.Instance.OnSFXVolumeChanged += UpdateSFXVolumeUI;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from SoundManager events to prevent memory leaks
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.OnMasterVolumeChanged -= UpdateMasterVolumeUI;
            SoundManager.Instance.OnMusicVolumeChanged -= UpdateMusicVolumeUI;
            SoundManager.Instance.OnSFXVolumeChanged -= UpdateSFXVolumeUI;
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            // Ensure settings are closed when opening pause menu initially
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }
    }

    public void ResumeGame()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }

    public void OpenSettings()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Ensure time is running
        if (SceneController.Instance != null)
        {
            SceneController.Instance.TransitionToMenuSceneAsync().Forget();
        }
        else
        {
            // Fallback if SceneController is missing
            UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Audio Control Methods

    public void InitializeAudioUI()
    {
        if (SoundManager.Instance == null) return;

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

    public void OnMasterVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(value);
        }
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMusicVolume(value);
        }
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(value);
        }
    }

    private void UpdateMasterVolumeUI(float volume)
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(volume);
        }
    }

    private void UpdateMusicVolumeUI(float volume)
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(volume);
        }
    }

    private void UpdateSFXVolumeUI(float volume)
    {
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(volume);
        }
    }
}
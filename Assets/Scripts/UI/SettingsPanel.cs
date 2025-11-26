using UnityEngine;
using UnityEngine.UI;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Manages the settings panel UI for game options
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        public static SettingsPanel Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Toggle _fadeTransitionsToggle;
        [SerializeField] private Button _backButton;

        private void Awake()
        {
            Instance = this;

            // Set initial toggle state from ScreenFadeManager
            if (_fadeTransitionsToggle != null && ScreenFadeManager.Instance != null)
            {
                _fadeTransitionsToggle.isOn = ScreenFadeManager.Instance.enableFadeTransitions;
                _fadeTransitionsToggle.onValueChanged.AddListener(OnFadeTransitionsToggleChanged);
            }

            // Add back button listener
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackButtonClicked);
            }
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (_fadeTransitionsToggle != null)
            {
                _fadeTransitionsToggle.onValueChanged.RemoveListener(OnFadeTransitionsToggleChanged);
            }

            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(OnBackButtonClicked);
            }
        }

        /// <summary>
        /// Called when fade transitions toggle is changed
        /// </summary>
        private void OnFadeTransitionsToggleChanged(bool value)
        {
            if (ScreenFadeManager.Instance != null)
            {
                ScreenFadeManager.Instance.enableFadeTransitions = value;
                Debug.Log($"[SettingsPanel] Fade transitions {(value ? "enabled" : "disabled")}");
            }
        }

        /// <summary>
        /// Called when back button is clicked
        /// </summary>
        private void OnBackButtonClicked()
        {
            // Hide this settings panel
            gameObject.SetActive(false);
        }
    }
}

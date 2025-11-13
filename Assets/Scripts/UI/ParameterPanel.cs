using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using NeuralWaveBureau.Data;
using NeuralWaveBureau.AI;
using System.Collections.Generic;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Advanced parameter tweaking panel for fine-tuning AI settings and neural profiles.
    /// Allows real-time adjustment of individual parameters with save/load functionality.
    /// </summary>
    public class ParameterPanel : MonoBehaviour
    {
        public static ParameterPanel Instance;
        [Header("UI References")]
        [SerializeField]
        private GameObject _panelContainer;

        [SerializeField]
        private Button _toggleButton;

        [SerializeField]
        private Button _saveButton;

        [SerializeField]
        private Button _loadButton;

        [SerializeField]
        private Button _resetButton;

        [SerializeField]
        private Transform _sliderContainer;

        [SerializeField]
        private GameObject _sliderPrefab; // Prefab with Slider + Label

        [Header("Settings")]
        [SerializeField]
        private bool _startCollapsed = true;

        [SerializeField]
        private float _animationDuration = 0.5f;

        // State
        private bool _isExpanded = false;
        private NeuralProfile _activeProfile;
        private AISettings _aiSettings;
        private AIManager _aiManager;

        // Dynamic sliders
        private Dictionary<string, Slider> _parameterSliders = new Dictionary<string, Slider>();
        private Dictionary<string, TextMeshProUGUI> _parameterLabels = new Dictionary<string, TextMeshProUGUI>();

        // Original values for reset
        private Dictionary<string, float> _originalValues = new Dictionary<string, float>();

        public bool IsExpanded => _isExpanded;

        private void Awake()
        {
            Instance = this;

            // Setup buttons
            if (_toggleButton != null)
            {
                _toggleButton.onClick.AddListener(TogglePanel);
            }

            if (_saveButton != null)
            {
                _saveButton.onClick.AddListener(SaveParameters);
            }

            if (_loadButton != null)
            {
                _loadButton.onClick.AddListener(LoadParameters);
            }

            if (_resetButton != null)
            {
                _resetButton.onClick.AddListener(ResetToDefaults);
            }

            // Initial state
            if (_startCollapsed)
            {
                _isExpanded = false;
                if (_panelContainer != null)
                {
                    _panelContainer.SetActive(false);
                }
            }
        }

        private void Start()
        {
            _aiManager = AIManager.Instance;
            if (_aiManager != null)
            {
                _aiSettings = _aiManager.Settings;
            }
        }

        /// <summary>
        /// Sets the active profile and builds parameter UI
        /// </summary>
        public void SetActiveProfile(NeuralProfile profile)
        {
            _activeProfile = profile;
            RebuildParameterUI();
        }

        /// <summary>
        /// Rebuilds all parameter sliders based on current profile and settings
        /// </summary>
        private void RebuildParameterUI()
        {
            // Clear existing sliders
            ClearSliders();

            if (_activeProfile == null || _aiSettings == null)
                return;

            // Add band target sliders
            for (int i = 0; i < NeuralProfile.BandCount; i++)
            {
                string bandName = NeuralProfile.BandNames[i];
                CreateParameterSlider(
                    $"target_{i}",
                    $"{bandName} Target",
                    0f, 1f,
                    _activeProfile.BandTargets[i],
                    (value) => { _activeProfile.BandTargets[i] = value; }
                );
            }

            // Add band tolerance sliders
            for (int i = 0; i < NeuralProfile.BandCount; i++)
            {
                string bandName = NeuralProfile.BandNames[i];
                CreateParameterSlider(
                    $"tolerance_{i}",
                    $"{bandName} Tolerance",
                    0.01f, 0.5f,
                    _activeProfile.BandTolerance[i],
                    (value) => { _activeProfile.BandTolerance[i] = value; }
                );
            }

            // Add instability settings
            CreateParameterSlider(
                "baseline_instability",
                "Baseline Instability",
                0f, 1f,
                _activeProfile.baselineInstability,
                (value) => { _activeProfile.baselineInstability = value; }
            );

            CreateParameterSlider(
                "instability_rate",
                "Instability Rate",
                0f, 2f,
                _activeProfile.instabilityRate,
                (value) => { _activeProfile.instabilityRate = value; }
            );

            // Add AI settings
            CreateParameterSlider(
                "success_threshold",
                "Success Threshold",
                0f, 1f,
                _aiSettings.successThreshold,
                (value) => { _aiSettings.successThreshold = value; }
            );

            CreateParameterSlider(
                "overload_threshold",
                "Overload Threshold",
                0f, 1f,
                _aiSettings.overloadThreshold,
                (value) => { _aiSettings.overloadThreshold = value; }
            );

            CreateParameterSlider(
                "instability_fail",
                "Instability Fail Threshold",
                0f, 1f,
                _aiSettings.instabilityFailThreshold,
                (value) => { _aiSettings.instabilityFailThreshold = value; }
            );

            CreateParameterSlider(
                "smoothing_tau",
                "Smoothing Tau",
                0.05f, 2f,
                _aiSettings.smoothingTau,
                (value) => { _aiSettings.smoothingTau = value; }
            );
        }

        /// <summary>
        /// Creates a parameter slider with label
        /// </summary>
        private void CreateParameterSlider(string id, string labelText, float min, float max, float currentValue, System.Action<float> onValueChanged)
        {
            if (_sliderPrefab == null || _sliderContainer == null)
                return;

            // Instantiate slider
            GameObject sliderObj = Instantiate(_sliderPrefab, _sliderContainer);
            sliderObj.name = $"Slider_{id}";

            // Get components
            Slider slider = sliderObj.GetComponentInChildren<Slider>();
            TextMeshProUGUI label = sliderObj.GetComponentInChildren<TextMeshProUGUI>();

            if (slider == null || label == null)
            {
                Debug.LogWarning($"[ParameterPanel] Slider prefab missing Slider or TextMeshProUGUI component!");
                Destroy(sliderObj);
                return;
            }

            // Configure slider
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = currentValue;
            slider.onValueChanged.AddListener((value) =>
            {
                onValueChanged?.Invoke(value);
                UpdateSliderLabel(id, value);
            });

            // Configure label
            label.text = $"{labelText}: {currentValue:F3}";

            // Store references
            _parameterSliders[id] = slider;
            _parameterLabels[id] = label;
            _originalValues[id] = currentValue;

            // Animate in
            sliderObj.transform.localScale = Vector3.zero;
            sliderObj.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Updates a slider's label
        /// </summary>
        private void UpdateSliderLabel(string id, float value)
        {
            if (_parameterLabels.ContainsKey(id))
            {
                string labelText = _parameterLabels[id].text;
                int colonIndex = labelText.IndexOf(':');
                if (colonIndex > 0)
                {
                    string baseName = labelText.Substring(0, colonIndex);
                    _parameterLabels[id].text = $"{baseName}: {value:F3}";
                }
            }
        }

        /// <summary>
        /// Clears all dynamic sliders
        /// </summary>
        private void ClearSliders()
        {
            _parameterSliders.Clear();
            _parameterLabels.Clear();
            _originalValues.Clear();

            if (_sliderContainer != null)
            {
                foreach (Transform child in _sliderContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Toggles panel visibility
        /// </summary>
        public void TogglePanel()
        {
            if (_isExpanded)
            {
                CollapsePanel();
            }
            else
            {
                ExpandPanel();
            }
        }

        /// <summary>
        /// Expands the panel
        /// </summary>
        public void ExpandPanel()
        {
            _isExpanded = true;

            if (_panelContainer != null)
            {
                _panelContainer.SetActive(true);
                _panelContainer.transform.localScale = new Vector3(1f, 0f, 1f);
                _panelContainer.transform.DOScaleY(1f, _animationDuration).SetEase(Ease.OutCubic);
            }

            // Animate button
            if (_toggleButton != null)
            {
                _toggleButton.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.3f);
            }
        }

        /// <summary>
        /// Collapses the panel
        /// </summary>
        public void CollapsePanel()
        {
            _isExpanded = false;

            if (_panelContainer != null)
            {
                _panelContainer.transform.DOScaleY(0f, _animationDuration)
                    .SetEase(Ease.InCubic)
                    .OnComplete(() => _panelContainer.SetActive(false));
            }

            // Animate button
            if (_toggleButton != null)
            {
                _toggleButton.transform.DOPunchRotation(new Vector3(0, 0, -10f), 0.3f);
            }
        }

        /// <summary>
        /// Saves current parameters (to ScriptableObject)
        /// </summary>
        private void SaveParameters()
        {
            if (_activeProfile != null)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(_activeProfile);
#endif
                Debug.Log("[ParameterPanel] Parameters saved to NeuralProfile");
            }

            if (_aiSettings != null)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(_aiSettings);
#endif
                Debug.Log("[ParameterPanel] Parameters saved to AISettings");
            }

            // Animate save button
            if (_saveButton != null)
            {
                _saveButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
            }
        }

        /// <summary>
        /// Loads parameters (refreshes from ScriptableObject)
        /// </summary>
        private void LoadParameters()
        {
            RebuildParameterUI();

            // Animate load button
            if (_loadButton != null)
            {
                _loadButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
            }

            Debug.Log("[ParameterPanel] Parameters loaded");
        }

        /// <summary>
        /// Resets all parameters to original values
        /// </summary>
        private void ResetToDefaults()
        {
            foreach (var kvp in _originalValues)
            {
                string id = kvp.Key;
                float originalValue = kvp.Value;

                if (_parameterSliders.ContainsKey(id))
                {
                    _parameterSliders[id].value = originalValue;
                }
            }

            // Animate reset button
            if (_resetButton != null)
            {
                _resetButton.transform.DOPunchRotation(new Vector3(0, 0, 360f), 0.5f);
            }

            Debug.Log("[ParameterPanel] Parameters reset to defaults");
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (_toggleButton != null)
            {
                _toggleButton.onClick.RemoveListener(TogglePanel);
            }

            if (_saveButton != null)
            {
                _saveButton.onClick.RemoveListener(SaveParameters);
            }

            if (_loadButton != null)
            {
                _loadButton.onClick.RemoveListener(LoadParameters);
            }

            if (_resetButton != null)
            {
                _resetButton.onClick.RemoveListener(ResetToDefaults);
            }
        }
    }
}

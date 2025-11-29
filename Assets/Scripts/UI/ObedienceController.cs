using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using NeuralWaveBureau.Data;
using NeuralWaveBureau.AI;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Controls citizen obedience through a single slider that affects multiple AI parameters.
    /// Higher obedience = easier to stabilize citizens (increased tolerance, reduced instability, lower thresholds)
    /// </summary>
    public class ObedienceController : MonoBehaviour
    {
        public static ObedienceController Instance;
        [Header("UI Components")]
        [SerializeField]
        private Slider _obedienceSlider;

        [SerializeField]
        private TextMeshProUGUI _obedienceLabel;

        [SerializeField]
        private TextMeshProUGUI _percentageText;

        [SerializeField]
        private Image _sliderFillImage;

        [Header("Obedience Settings")]
        [SerializeField]
        [Range(0f, 100f)]
        private float _currentObedience = 0f;

        [Header("Parameter Ranges")]
        [Tooltip("How much to multiply base tolerance (1.0 = no change, 2.0 = double)")]
        [SerializeField]
        private Vector2 _toleranceMultiplierRange = new(0.5f, 2.5f);

        [Tooltip("How much to multiply instability rate (1.0 = no change, 0.5 = half speed)")]
        [SerializeField]
        private Vector2 _instabilityRateMultiplierRange = new(2f, 0.3f);

        [Tooltip("How much to adjust success threshold (-0.2 to +0.2)")]
        [SerializeField]
        private Vector2 _successThresholdAdjustRange = new(0.1f, -0.2f);

        [Header("Visual Feedback")]
        [SerializeField]
        private Gradient _obedienceColorGradient;

        [SerializeField]
        private bool _animateChanges = true;

        // References
        private AIManager _aiManager;
        private NeuralProfile _activeProfile;
        private AISettings _aiSettings;

        // Original values (to restore)
        private float[] _originalTolerances;
        private float _originalInstabilityRate;
        private float _originalSuccessThreshold;

        // Events
        public System.Action<float> OnObedienceChanged;

        public float CurrentObedience => _currentObedience;

        [Header("Dynamic Obedience")]
        [SerializeField]
        private bool _enableDynamicObedience = true;

        [SerializeField]
        [Tooltip("How fast obedience rises when performing well")]
        private float _obedienceRiseRate = 12f;

        [SerializeField]
        [Tooltip("How fast obedience falls when performing poorly")]
        private float _obedienceFallRate = 2f;

        private void Awake()
        {
            Instance = this;

            // Setup slider
            if (_obedienceSlider != null)
            {
                _obedienceSlider.minValue = 0f;
                _obedienceSlider.maxValue = 100f;
                _obedienceSlider.value = _currentObedience;
                _obedienceSlider.onValueChanged.AddListener(OnSliderValueChanged);
            }
        }

        private void Start()
        {
            _aiManager = AIManager.Instance;
            if (_aiManager != null)
            {
                _aiSettings = _aiManager.Settings;
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Sets the active citizen profile to modify
        /// </summary>
        public void SetActiveProfile(NeuralProfile profile)
        {
            _activeProfile = profile;

            // Store original values
            if (profile != null)
            {
                _originalTolerances = (float[])profile.BandTolerance.Clone();
                _originalInstabilityRate = profile.instabilityRate;
            }

            if (_aiSettings != null)
            {
                _originalSuccessThreshold = _aiSettings.successThreshold;
            }

            // Also update StimulationTimeController if it exists
            if (StimulationTimeController.Instance != null)
            {
                StimulationTimeController.Instance.SetActiveProfile(profile);
            }

            // Update obedience value and slider when citizen arrives at station
            if (profile != null)
            {
                // Set obedience to starting value for new citizen (using profile-specific value)
                _currentObedience = profile.startingObedience;

                // Set slider to current obedience value instantly
                if (_obedienceSlider != null)
                {
                    _obedienceSlider.value = _currentObedience;
                }

                // Update visuals and apply parameters
                UpdateVisuals();
                ApplyObedienceToParameters();
            }
        }

        /// <summary>
        /// Sets obedience value directly (0-100)
        /// </summary>
        /// <summary>
        /// Updates obedience based on citizen evaluation performance
        /// </summary>
        public void UpdateDynamicObedience(float evaluationScore, float deltaTime)
        {
            if (!_enableDynamicObedience)
                return;

            // Don't update if no active citizen
            if (_activeProfile == null)
                return;

            // Target obedience is directly proportional to score (0-100)
            // Added 10% boost to make it feel more rewarding
            float targetObedience = Mathf.Clamp(evaluationScore * 110f, 0f, 100f);

            // Determine rate based on whether we need to rise or fall
            // We fall slowly (forgiving) and rise quickly (rewarding)
            float rate = (targetObedience > _currentObedience) ? _obedienceRiseRate : _obedienceFallRate;

            // Move towards target
            if (Mathf.Abs(targetObedience - _currentObedience) > 0.01f)
            {
                float newValue = Mathf.MoveTowards(_currentObedience, targetObedience, rate * deltaTime);
                SetObedience(newValue, false); // Don't animate every frame, just set value
            }
        }

        /// <summary>
        /// Sets obedience value directly (0-100)
        /// </summary>
        public void SetObedience(float value, bool animate = true)
        {
            float clampedValue = Mathf.Clamp(value, 0f, 100f);

            if (animate && _animateChanges)
            {
                DOTween.To(() => _currentObedience, x => _currentObedience = x, clampedValue, 0.5f)
                    .SetEase(Ease.OutCubic)
                    .OnUpdate(() =>
                    {
                        if (_obedienceSlider != null)
                        {
                            _obedienceSlider.value = _currentObedience;
                        }
                        ApplyObedienceToParameters();
                        UpdateVisuals();
                    });
            }
            else
            {
                _currentObedience = clampedValue;
                if (_obedienceSlider != null)
                {
                    _obedienceSlider.value = _currentObedience;
                }
                ApplyObedienceToParameters();
                UpdateVisuals();
            }
        }

        /// <summary>
        /// Called when slider value changes
        /// </summary>
        private void OnSliderValueChanged(float value)
        {
            // Don't process slider changes if no active citizen
            if (_activeProfile == null)
                return;

            _currentObedience = value;
            ApplyObedienceToParameters();
            UpdateVisuals();
            OnObedienceChanged?.Invoke(_currentObedience);

            // Animate slider handle
            if (_animateChanges && _obedienceSlider != null)
            {
                _obedienceSlider.targetGraphic?.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
            }
        }

        /// <summary>
        /// Applies current obedience level to AI parameters
        /// </summary>
        private void ApplyObedienceToParameters()
        {
            if (_activeProfile == null || _aiSettings == null)
                return;

            // Calculate normalized obedience (0..1)
            float normalizedObedience = _currentObedience / 100f;

            /* 
            // Tolerance multiplier removed to decouple evaluation from obedience
            float toleranceMultiplier = Mathf.Lerp(
                _toleranceMultiplierRange.x,
                _toleranceMultiplierRange.y,
                normalizedObedience
            );

            for (int i = 0; i < _activeProfile.BandTolerance.Length; i++)
            {
                _activeProfile.BandTolerance[i] = _originalTolerances[i] * toleranceMultiplier;
            }
            */

            // Apply instability rate multiplier
            float instabilityMultiplier = Mathf.Lerp(
                _instabilityRateMultiplierRange.x,
                _instabilityRateMultiplierRange.y,
                normalizedObedience
            );

            // Apply to active citizen's state machine
            if (_aiManager != null)
            {
                var activeCitizen = _aiManager.ActiveCitizen;
                if (activeCitizen != null)
                {
                    activeCitizen.SetObedienceMultiplier(instabilityMultiplier);
                }
            }

            /*
            // Success threshold adjustment removed to decouple evaluation from obedience
            float thresholdAdjust = Mathf.Lerp(
                _successThresholdAdjustRange.x,
                _successThresholdAdjustRange.y,
                normalizedObedience
            );
            _aiSettings.successThreshold = Mathf.Clamp01(_originalSuccessThreshold + thresholdAdjust);
            */
        }

        /// <summary>
        /// Updates visual feedback
        /// </summary>
        private void UpdateVisuals()
        {
            // Don't update visuals if no active citizen
            if (_activeProfile == null)
                return;

            // Update percentage text
            if (_percentageText != null)
            {
                _percentageText.text = $"{_currentObedience:F0}%";
            }

            // Update label
            if (_obedienceLabel != null)
            {
                string label = GetObedienceLabel(_currentObedience);
                _obedienceLabel.text = $"Obedience: {label}";
            }
        }

        /// <summary>
        /// Gets a descriptive label for the obedience level
        /// </summary>
        private string GetObedienceLabel(float obedience)
        {
            if (obedience >= 90f) return "COMPLIANT";
            if (obedience >= 75f) return "COOPERATIVE";
            if (obedience >= 60f) return "STABLE";
            if (obedience >= 40f) return "NEUTRAL";
            if (obedience >= 25f) return "RESISTANT";
            if (obedience >= 10f) return "DEFIANT";
            return "REBELLIOUS";
        }

        /// <summary>
        /// Resets to default obedience (50%)
        /// </summary>
        public void ResetToDefault()
        {
            SetObedience(50f, true);

            // Pulse feedback
            if (_animateChanges)
            {
                transform.DOPunchScale(Vector3.one * 0.05f, 0.3f);
            }
        }

        /// <summary>
        /// Restores original AI parameters
        /// </summary>
        public void RestoreOriginalParameters()
        {
            if (_activeProfile != null && _originalTolerances != null)
            {
                for (int i = 0; i < _activeProfile.BandTolerance.Length; i++)
                {
                    _activeProfile.BandTolerance[i] = _originalTolerances[i];
                }
            }

            if (_aiSettings != null)
            {
                _aiSettings.successThreshold = _originalSuccessThreshold;
            }
        }

        /// <summary>
        /// Animates the obedience panel in
        /// </summary>
        public void AnimateIn(float duration = 0.5f)
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, duration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Animates the obedience panel out
        /// </summary>
        public void AnimateOut(float duration = 0.3f)
        {
            transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);
        }

        private void OnDestroy()
        {
            if (_obedienceSlider != null)
            {
                _obedienceSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }
        }
    }
}

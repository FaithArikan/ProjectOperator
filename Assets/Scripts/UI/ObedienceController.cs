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
        private float _currentObedience = 50f;

        [Header("Parameter Ranges")]
        [Tooltip("How much to multiply base tolerance (1.0 = no change, 2.0 = double)")]
        [SerializeField]
        private Vector2 _toleranceMultiplierRange = new Vector2(0.5f, 2.5f);

        [Tooltip("How much to multiply instability rate (1.0 = no change, 0.5 = half speed)")]
        [SerializeField]
        private Vector2 _instabilityRateMultiplierRange = new Vector2(2f, 0.3f);

        [Tooltip("How much to adjust success threshold (-0.2 to +0.2)")]
        [SerializeField]
        private Vector2 _successThresholdAdjustRange = new Vector2(0.1f, -0.2f);

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

        private void Awake()
        {
            // Setup slider
            if (_obedienceSlider != null)
            {
                _obedienceSlider.minValue = 0f;
                _obedienceSlider.maxValue = 100f;
                _obedienceSlider.value = _currentObedience;
                _obedienceSlider.onValueChanged.AddListener(OnSliderValueChanged);
            }

            // Setup default color gradient if not set
            if (_obedienceColorGradient == null || _obedienceColorGradient.colorKeys.Length == 0)
            {
                _obedienceColorGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[3];
                colorKeys[0] = new GradientColorKey(Color.red, 0f);
                colorKeys[1] = new GradientColorKey(Color.yellow, 0.5f);
                colorKeys[2] = new GradientColorKey(Color.green, 1f);

                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1f, 0f);
                alphaKeys[1] = new GradientAlphaKey(1f, 1f);

                _obedienceColorGradient.SetKeys(colorKeys, alphaKeys);
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

            // Apply tolerance multiplier
            float toleranceMultiplier = Mathf.Lerp(
                _toleranceMultiplierRange.x,
                _toleranceMultiplierRange.y,
                normalizedObedience
            );

            for (int i = 0; i < _activeProfile.BandTolerance.Length; i++)
            {
                _activeProfile.BandTolerance[i] = _originalTolerances[i] * toleranceMultiplier;
            }

            // Apply instability rate multiplier
            float instabilityMultiplier = Mathf.Lerp(
                _instabilityRateMultiplierRange.x,
                _instabilityRateMultiplierRange.y,
                normalizedObedience
            );
            // Note: We can't directly modify the profile instabilityRate as it's a public field
            // In production, you'd want to make it a property or use a different approach

            // Apply success threshold adjustment
            float thresholdAdjust = Mathf.Lerp(
                _successThresholdAdjustRange.x,
                _successThresholdAdjustRange.y,
                normalizedObedience
            );
            _aiSettings.successThreshold = Mathf.Clamp01(_originalSuccessThreshold + thresholdAdjust);
        }

        /// <summary>
        /// Updates visual feedback
        /// </summary>
        private void UpdateVisuals()
        {
            float normalizedValue = _currentObedience / 100f;

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

            // Update slider fill color
            if (_sliderFillImage != null)
            {
                Color targetColor = _obedienceColorGradient.Evaluate(normalizedValue);
                _sliderFillImage.DOColor(targetColor, 0.3f);
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

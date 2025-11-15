using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using NeuralWaveBureau.Data;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Controls the minimum stimulation time required for citizen stabilization via a slider.
    /// Allows player to adjust how long they need to maintain the target eval score.
    /// </summary>
    public class StimulationTimeController : MonoBehaviour
    {
        public static StimulationTimeController Instance;

        [Header("UI Components")]
        [SerializeField]
        private Slider _timeSlider;

        [SerializeField]
        private TextMeshProUGUI _timeLabel;

        [SerializeField]
        private TextMeshProUGUI _valueText;

        [SerializeField]
        private Image _sliderFillImage;

        [Header("Time Settings")]
        [Tooltip("Minimum time in seconds")]
        [SerializeField]
        [Range(0.5f, 20f)]
        private float _minTime = 0.5f;

        [Tooltip("Maximum time in seconds")]
        [SerializeField]
        [Range(0.5f, 20f)]
        private float _maxTime = 10f;

        [Tooltip("Current stimulation time in seconds")]
        [SerializeField]
        [Range(0.5f, 20f)]
        private float _currentTime = 5f;

        [Header("Visual Feedback")]
        [SerializeField]
        private Gradient _timeColorGradient;

        [SerializeField]
        private bool _animateChanges = true;

        // References
        private NeuralProfile _activeProfile;

        // Events
        public System.Action<float> OnTimeChanged;

        public float CurrentTime => _currentTime;

        private void Awake()
        {
            Instance = this;

            // Setup slider
            if (_timeSlider != null)
            {
                _timeSlider.minValue = _minTime;
                _timeSlider.maxValue = _maxTime;
                _timeSlider.value = _currentTime;
                _timeSlider.onValueChanged.AddListener(OnSliderValueChanged);
            }

            // Setup default color gradient if not set
            if (_timeColorGradient == null || _timeColorGradient.colorKeys.Length == 0)
            {
                _timeColorGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[3];
                colorKeys[0] = new GradientColorKey(Color.green, 0f);
                colorKeys[1] = new GradientColorKey(Color.yellow, 0.5f);
                colorKeys[2] = new GradientColorKey(Color.red, 1f);

                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1f, 0f);
                alphaKeys[1] = new GradientAlphaKey(1f, 1f);

                _timeColorGradient.SetKeys(colorKeys, alphaKeys);
            }
        }

        private void Start()
        {
            UpdateVisuals();
        }

        /// <summary>
        /// Sets the active citizen profile to modify
        /// </summary>
        public void SetActiveProfile(NeuralProfile profile)
        {
            _activeProfile = profile;

            // Apply current time to profile
            if (_activeProfile != null)
            {
                _activeProfile.minStimulationTime = _currentTime;
            }
        }

        /// <summary>
        /// Sets stimulation time value directly (in seconds)
        /// </summary>
        public void SetTime(float value, bool animate = true)
        {
            float clampedValue = Mathf.Clamp(value, _minTime, _maxTime);

            if (animate && _animateChanges)
            {
                DOTween.To(() => _currentTime, x => _currentTime = x, clampedValue, 0.5f)
                    .SetEase(Ease.OutCubic)
                    .OnUpdate(() =>
                    {
                        if (_timeSlider != null)
                        {
                            _timeSlider.value = _currentTime;
                        }
                        ApplyTimeToProfile();
                        UpdateVisuals();
                    });
            }
            else
            {
                _currentTime = clampedValue;
                if (_timeSlider != null)
                {
                    _timeSlider.value = _currentTime;
                }
                ApplyTimeToProfile();
                UpdateVisuals();
            }
        }

        /// <summary>
        /// Called when slider value changes
        /// </summary>
        private void OnSliderValueChanged(float value)
        {
            _currentTime = value;
            ApplyTimeToProfile();
            UpdateVisuals();
            OnTimeChanged?.Invoke(_currentTime);

            // Animate slider handle
            if (_animateChanges && _timeSlider != null)
            {
                _timeSlider.targetGraphic?.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
            }
        }

        /// <summary>
        /// Applies current time to the active profile
        /// </summary>
        private void ApplyTimeToProfile()
        {
            if (_activeProfile != null)
            {
                _activeProfile.minStimulationTime = _currentTime;
            }
        }

        /// <summary>
        /// Updates visual feedback
        /// </summary>
        private void UpdateVisuals()
        {
            float normalizedValue = Mathf.InverseLerp(_minTime, _maxTime, _currentTime);

            // Update value text
            if (_valueText != null)
            {
                _valueText.text = $"{_currentTime:F1}s";
            }

            // Update label
            if (_timeLabel != null)
            {
                string label = GetTimeLabel(_currentTime);
                _timeLabel.text = $"Hold Time: {label}";
            }

            // Update slider fill color
            if (_sliderFillImage != null)
            {
                Color targetColor = _timeColorGradient.Evaluate(normalizedValue);
                if (_animateChanges)
                {
                    _sliderFillImage.DOColor(targetColor, 0.3f);
                }
                else
                {
                    _sliderFillImage.color = targetColor;
                }
            }
        }

        /// <summary>
        /// Gets a descriptive label for the time value
        /// </summary>
        private string GetTimeLabel(float time)
        {
            if (time <= 1f) return "INSTANT";
            if (time <= 2f) return "QUICK";
            if (time <= 3f) return "FAST";
            if (time <= 5f) return "NORMAL";
            if (time <= 7f) return "MODERATE";
            if (time <= 10f) return "LONG";
            return "VERY LONG";
        }

        /// <summary>
        /// Resets to default time (5 seconds)
        /// </summary>
        public void ResetToDefault()
        {
            SetTime(5f, true);

            // Pulse feedback
            if (_animateChanges)
            {
                transform.DOPunchScale(Vector3.one * 0.05f, 0.3f);
            }
        }

        /// <summary>
        /// Animates the panel in
        /// </summary>
        public void AnimateIn(float duration = 0.5f)
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, duration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Animates the panel out
        /// </summary>
        public void AnimateOut(float duration = 0.3f)
        {
            transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);
        }

        private void OnDestroy()
        {
            if (_timeSlider != null)
            {
                _timeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }
        }
    }
}

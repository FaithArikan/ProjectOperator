using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// A 2D UI radio knob controller that rotates based on circular drag motion.
    /// Click and drag around the knob center to rotate it like a real radio dial.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class RadioKnobController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Knob Settings")]
        [SerializeField]
        private RectTransform _knobGraphic; // The visual knob image that rotates

        [SerializeField]
        [Range(0f, 1f)]
        private float _currentValue = 0.5f;

        [SerializeField]
        private float _minRotation = -135f; // Rotation angle at value 0

        [SerializeField]
        private float _maxRotation = 135f; // Rotation angle at value 1

        [SerializeField]
        [Range(0.1f, 3f)]
        private float _sensitivity = 1f; // Rotation sensitivity multiplier

        [Header("Visual Feedback")]
        [SerializeField]
        private Image _knobImage;

        [SerializeField]
        private Image _backgroundImage; // Optional background ring

        [SerializeField]
        private Image _indicatorImage; // Optional indicator dot/line on knob

        [SerializeField]
        private TextMeshProUGUI _valueText; // Optional text showing current value

        [SerializeField]
        private TextMeshProUGUI _labelText; // Optional label text

        [Header("Selection Outline")]
        [SerializeField]
        private Image _selectionOutlineImage; // Image-based outline for selection (show/hide)

        [Header("Audio")]
        [SerializeField]
        private AudioSource _audioSource;

        [SerializeField]
        private AudioClip _clickSound;

        [SerializeField]
        private AudioClip _tickSound; // Sound for each "tick" while rotating

        [SerializeField]
        [Range(0f, 0.1f)]
        private float _tickInterval = 0.05f; // Value change between ticks

        // Components
        private RectTransform _rectTransform;
        private Canvas _canvas;

        // State
        private bool _isActive = false;
        private bool _isDragging = false;
        private float _lastAngle;
        private float _lastTickValue;

        // Events
        public System.Action<float> OnValueChanged;
        public System.Action OnKnobActivated;
        public System.Action OnKnobDeactivated;
        public System.Action OnUserInteracted;

        public float Value
        {
            get => _currentValue;
            set => SetValue(value, true);
        }

        public bool IsActive => _isActive;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            if (_knobGraphic == null)
            {
                _knobGraphic = _rectTransform;
            }

            if (_knobImage == null)
            {
                _knobImage = GetComponent<Image>();
            }

            // Find parent canvas for coordinate conversion
            _canvas = GetComponentInParent<Canvas>();

            _lastTickValue = _currentValue;
        }

        private void Start()
        {
            // Set initial rotation
            UpdateKnobRotation();
            UpdateValueText();
            UpdateKnobVisuals();
        }

        /// <summary>
        /// Sets the knob value programmatically
        /// </summary>
        public void SetValue(float value, bool animate = true)
        {
            float clampedValue = Mathf.Clamp01(value);

            if (Mathf.Approximately(_currentValue, clampedValue))
                return;

            if (animate)
            {
                DOTween.To(() => _currentValue, x =>
                {
                    _currentValue = x;
                    UpdateKnobRotation();
                    UpdateValueText();
                    OnValueChanged?.Invoke(_currentValue);
                }, clampedValue, 0.3f).SetEase(Ease.OutCubic);
            }
            else
            {
                _currentValue = clampedValue;
                UpdateKnobRotation();
                UpdateValueText();
                OnValueChanged?.Invoke(_currentValue);
            }
        }

        /// <summary>
        /// Sets the label text
        /// </summary>
        public void SetLabel(string label)
        {
            if (_labelText != null)
            {
                _labelText.text = label;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = true;

            // Calculate initial angle from knob center to pointer
            _lastAngle = GetAngleFromCenter(eventData.position);

            // Activate knob
            if (!_isActive)
            {
                Activate();
            }

            // Play click sound
            PlaySound(_clickSound);

            // Visual press feedback
            if (_knobGraphic != null)
            {
                _knobGraphic.DOScale(0.95f, 0.1f).SetEase(Ease.OutCubic);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;

            // Visual release feedback
            if (_knobGraphic != null)
            {
                _knobGraphic.DOScale(1f, 0.15f).SetEase(Ease.OutBack);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;

            // Calculate current angle from knob center
            float currentAngle = GetAngleFromCenter(eventData.position);

            // Calculate angle delta (handle wrap-around)
            float angleDelta = Mathf.DeltaAngle(_lastAngle, currentAngle);

            // Convert angle delta to value delta
            float rotationRange = _maxRotation - _minRotation;
            float valueDelta = (angleDelta / rotationRange) * _sensitivity;

            // Update value
            float newValue = Mathf.Clamp01(_currentValue + valueDelta);

            if (Mathf.Abs(newValue - _currentValue) > 0.0001f)
            {
                _currentValue = newValue;
                UpdateKnobRotation();
                UpdateValueText();
                OnValueChanged?.Invoke(_currentValue);
                OnUserInteracted?.Invoke();

                // Play tick sound at intervals
                if (Mathf.Abs(_currentValue - _lastTickValue) >= _tickInterval)
                {
                    PlaySound(_tickSound, 0.3f);
                    _lastTickValue = _currentValue;
                }
            }

            _lastAngle = currentAngle;
        }

        /// <summary>
        /// Calculates angle from knob center to screen position
        /// </summary>
        private float GetAngleFromCenter(Vector2 screenPosition)
        {
            // Convert screen position to local position relative to knob center
            Vector2 localPoint;

            if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform,
                    screenPosition,
                    _canvas.worldCamera,
                    out localPoint
                );
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform,
                    screenPosition,
                    null,
                    out localPoint
                );
            }

            // Calculate angle (0 = right, 90 = up, 180/-180 = left, -90 = down)
            float angle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;

            return angle;
        }

        /// <summary>
        /// Activates this knob (shows it's selected)
        /// </summary>
        public void Activate()
        {
            _isActive = true;
            UpdateKnobVisuals();
            OnKnobActivated?.Invoke();

            // Pulse animation
            if (_knobGraphic != null)
            {
                _knobGraphic.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5);
            }
        }

        /// <summary>
        /// Deactivates this knob
        /// </summary>
        public void Deactivate()
        {
            _isActive = false;
            _isDragging = false;
            UpdateKnobVisuals();
            OnKnobDeactivated?.Invoke();
        }

        /// <summary>
        /// Updates the knob rotation based on current value
        /// </summary>
        private void UpdateKnobRotation()
        {
            if (_knobGraphic == null)
                return;

            // Map value (0-1) to rotation angle
            float rotation = Mathf.Lerp(_minRotation, _maxRotation, _currentValue);

            // Apply rotation (negative because Unity UI rotates counter-clockwise for positive Z)
            _knobGraphic.localRotation = Quaternion.Euler(0f, 0f, -rotation);
        }

        /// <summary>
        /// Updates the value text display
        /// </summary>
        private void UpdateValueText()
        {
            if (_valueText != null)
            {
                _valueText.text = $"{_currentValue:F2}";
            }
        }

        /// <summary>
        /// Updates knob outline visibility based on selection state
        /// </summary>
        private void UpdateKnobVisuals()
        {
            // Show/hide the outline image based on active state
            if (_selectionOutlineImage != null)
            {
                _selectionOutlineImage.enabled = _isActive;
            }
        }

        /// <summary>
        /// Plays a sound effect
        /// </summary>
        private void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip, volume);
            }
        }

        /// <summary>
        /// Resets knob to default value
        /// </summary>
        public void ResetToDefault(float defaultValue = 0.5f)
        {
            SetValue(defaultValue, true);
            _lastTickValue = defaultValue;
        }

        private void OnDestroy()
        {
            DOTween.Kill(_knobGraphic);
            DOTween.Kill(_knobImage);
            DOTween.Kill(_indicatorImage);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Update rotation in editor when value changes
            if (_knobGraphic != null)
            {
                float rotation = Mathf.Lerp(_minRotation, _maxRotation, _currentValue);
                _knobGraphic.localRotation = Quaternion.Euler(0f, 0f, -rotation);
            }
        }
#endif
    }
}

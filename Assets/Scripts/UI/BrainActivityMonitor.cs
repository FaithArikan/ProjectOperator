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
    /// Main controller for the Brain Activity Monitor UI system.
    /// Manages waveform displays, obedience control, and CRT effects.
    /// Coordinates all UI components and handles data flow from AIManager.
    /// </summary>
    public class BrainActivityMonitor : MonoBehaviour
    {
        [Header("Core Components")]
        [SerializeField]
        private CRTScreenEffect _crtEffect;

        [Header("Waveform Displays")]
        [SerializeField]
        private List<WaveformDisplay> _waveformDisplays = new List<WaveformDisplay>();

        [Header("Status Display")]
        [SerializeField]
        private TextMeshProUGUI _citizenNameText;

        [SerializeField]
        private TextMeshProUGUI _stateText;

        [SerializeField]
        private TextMeshProUGUI _evaluationScoreText;

        [SerializeField]
        private TextMeshProUGUI _instabilityText;

        [SerializeField]
        private Image _statusIndicator;

        [SerializeField]
        private Slider _instabilityBar;

        [Header("Control Buttons")]
        [SerializeField]
        private Button _powerButton;

        [SerializeField]
        private Button _startStimulationButton;

        [SerializeField]
        private Button _stopStimulationButton;

        [SerializeField]
        private Button _resetButton;

        [Header("Settings")]
        [SerializeField]
        private int _historyBufferSize = 120;

        [SerializeField]
        private float _updateRate = 30f; // Hz

        [SerializeField]
        private bool _autoStartOnCitizenActive = true;

        [SerializeField]
        private bool _showDebugInfo = true;

        [Header("Colors")]
        [SerializeField]
        private Color _idleColor = Color.gray;

        [SerializeField]
        private Color _stimulatedColor = Color.cyan;

        [SerializeField]
        private Color _stabilizedColor = Color.green;

        [SerializeField]
        private Color _agitatedColor = Color.yellow;

        [SerializeField]
        private Color _criticalColor = Color.red;

        // System components
        private AIManager _aiManager;
        private CitizenController _activeCitizen;
        private DataBufferManager _bufferManager;

        // State
        private bool _isPoweredOn = false;
        private bool _isMonitoring = false;
        private float _updateTimer = 0f;
        private Tween _alertAnimation;

        // Band colors (Delta, Theta, Alpha, Beta, Gamma)
        private static readonly Color[] BandColors = new Color[]
        {
            new Color(1f, 0.2f, 0.2f),      // Delta - Red
            new Color(1f, 0.6f, 0.2f),      // Theta - Orange
            new Color(0.2f, 1f, 0.2f),      // Alpha - Green
            new Color(0.2f, 0.6f, 1f),      // Beta - Blue
            new Color(0.8f, 0.2f, 1f)       // Gamma - Purple
        };

        private void Awake()
        {
            // Initialize buffer manager
            _bufferManager = new DataBufferManager(NeuralProfile.BandCount, _historyBufferSize);

            // Setup button listeners
            if (_powerButton != null)
            {
                _powerButton.onClick.AddListener(TogglePower);
            }

            if (_startStimulationButton != null)
            {
                _startStimulationButton.onClick.AddListener(StartMonitoring);
            }

            if (_stopStimulationButton != null)
            {
                _stopStimulationButton.onClick.AddListener(StopMonitoring);
            }

            if (_resetButton != null)
            {
                _resetButton.onClick.AddListener(ResetMonitor);
            }

            // Initialize waveform displays
            InitializeWaveformDisplays();
        }

        private void Start()
        {
            _aiManager = AIManager.Instance;

            // Start powered off
            if (_crtEffect != null)
            {
                gameObject.SetActive(true);
                _isPoweredOn = false;
            }

            UpdateButtonStates();
        }

        private void Update()
        {
            if (!_isPoweredOn || !_isMonitoring)
                return;

            // Update at specified rate
            _updateTimer += Time.deltaTime;
            float updateInterval = 1f / _updateRate;

            if (_updateTimer >= updateInterval)
            {
                _updateTimer = 0f;
                UpdateMonitorData();
            }

            // Update status display
            UpdateStatusDisplay();
        }

        /// <summary>
        /// Initializes waveform displays with buffer manager
        /// </summary>
        private void InitializeWaveformDisplays()
        {
            for (int i = 0; i < _waveformDisplays.Count && i < NeuralProfile.BandCount; i++)
            {
                if (_waveformDisplays[i] != null)
                {
                    _waveformDisplays[i].Initialize(_bufferManager, i);
                    _waveformDisplays[i].BandIndex = i;
                    _waveformDisplays[i].WaveColor = BandColors[i];
                }
            }
        }

        /// <summary>
        /// Sets the active citizen to monitor
        /// </summary>
        public void SetActiveCitizen(CitizenController citizen)
        {
            // Unsubscribe from previous citizen
            if (_activeCitizen != null)
            {
                _activeCitizen.OnStabilized -= OnCitizenStabilized;
                _activeCitizen.OnCriticalFailure -= OnCitizenCriticalFailure;
            }

            _activeCitizen = citizen;

            if (_activeCitizen != null)
            {
                // Subscribe to events
                _activeCitizen.OnStabilized += OnCitizenStabilized;
                _activeCitizen.OnCriticalFailure += OnCitizenCriticalFailure;

                // Update UI
                if (_citizenNameText != null)
                {
                    _citizenNameText.text = _activeCitizen.Profile.displayName;
                }

                // Set profile in controllers
                ObedienceController.Instance.SetActiveProfile(_activeCitizen.Profile);

                // Update waveform targets
                UpdateWaveformTargets();

                // Auto-start if enabled
                if (_autoStartOnCitizenActive && _isPoweredOn)
                {
                    StartMonitoring();
                }
            }
        }

        /// <summary>
        /// Updates waveform display targets from active profile
        /// </summary>
        private void UpdateWaveformTargets()
        {
            if (_activeCitizen == null)
                return;

            NeuralProfile profile = _activeCitizen.Profile;

            for (int i = 0; i < _waveformDisplays.Count && i < NeuralProfile.BandCount; i++)
            {
                if (_waveformDisplays[i] != null)
                {
                    _waveformDisplays[i].SetTarget(
                        profile.BandTargets[i],
                        profile.BandTolerance[i]
                    );
                }
            }
        }

        /// <summary>
        /// Updates monitor data from AIManager
        /// </summary>
        private void UpdateMonitorData()
        {
            if (_aiManager == null || _activeCitizen == null)
                return;

            // Get current wave sample
            WaveSample sample = _aiManager.CurrentWaveSample;

            // Add to buffer
            _bufferManager.AddSample(sample.bandValues);
        }

        /// <summary>
        /// Updates status display texts
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (_activeCitizen == null)
                return;

            // Update state
            if (_stateText != null)
            {
                _stateText.text = $"STATE: {_activeCitizen.CurrentState}";

                // Color code state
                Color stateColor = GetStateColor(_activeCitizen.CurrentState);
                _stateText.color = stateColor;

                if (_statusIndicator != null)
                {
                    _statusIndicator.color = stateColor;
                }
            }

            // Update evaluation score
            if (_evaluationScoreText != null)
            {
                float score = _activeCitizen.EvaluationScore;
                _evaluationScoreText.text = $"EVAL: {score:F3}";

                // Color code based on threshold
                if (_aiManager != null && _aiManager.Settings != null)
                {
                    if (score >= _aiManager.Settings.successThreshold)
                    {
                        _evaluationScoreText.color = _stabilizedColor;
                    }
                    else if (score <= _aiManager.Settings.overloadThreshold)
                    {
                        _evaluationScoreText.color = _criticalColor;
                    }
                    else
                    {
                        _evaluationScoreText.color = Color.white;
                    }
                }
            }

            // Update instability
            if (_instabilityText != null)
            {
                float instability = _activeCitizen.Instability;
                _instabilityText.text = $"INSTABILITY: {instability:F2}";
                _instabilityText.color = Color.Lerp(_stabilizedColor, _criticalColor, instability);
            }

            // Update instability bar
            if (_instabilityBar != null)
            {
                _instabilityBar.value = _activeCitizen.Instability;
                if (_instabilityBar.fillRect != null)
                {
                    Image fillImage = _instabilityBar.fillRect.GetComponent<Image>();
                    if (fillImage != null)
                    {
                        fillImage.color = Color.Lerp(_stabilizedColor, _criticalColor, _activeCitizen.Instability);
                    }
                }
            }
        }

        /// <summary>
        /// Gets color for citizen state
        /// </summary>
        private Color GetStateColor(CitizenState state)
        {
            switch (state)
            {
                case CitizenState.Idle:
                    return _idleColor;
                case CitizenState.BeingStimulated:
                    return _stimulatedColor;
                case CitizenState.Stabilized:
                    return _stabilizedColor;
                case CitizenState.Agitated:
                    return _agitatedColor;
                case CitizenState.CriticalFailure:
                    return _criticalColor;
                case CitizenState.Recovering:
                    return Color.Lerp(_agitatedColor, _stabilizedColor, 0.5f);
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Toggles monitor power
        /// </summary>
        public void TogglePower()
        {
            if (_isPoweredOn)
            {
                PowerOff();
            }
            else
            {
                PowerOn();
            }
        }

        /// <summary>
        /// Powers on the monitor
        /// </summary>
        public void PowerOn()
        {
            if (_isPoweredOn)
                return;

            _isPoweredOn = true;

            // Move camera to monitor view
            CameraManager.Instance.MoveToMonitorView();

            // CRT power on effect
            if (_crtEffect != null)
            {
                _crtEffect.PowerOn(1.5f);
            }

            // Animate waveforms in
            foreach (var waveform in _waveformDisplays)
            {
                if (waveform != null)
                {
                    waveform.AnimateIn(0.8f);
                }
            }

            // Animate obedience controller
            ObedienceController.Instance.AnimateIn(1f);

            UpdateButtonStates();

            // Button feedback
            if (_powerButton != null)
            {
                UITweenAnimations.ButtonPress(_powerButton.transform);
            }
        }

        /// <summary>
        /// Powers off the monitor
        /// </summary>
        public void PowerOff()
        {
            if (!_isPoweredOn)
                return;

            // Stop monitoring
            StopMonitoring();

            _isPoweredOn = false;

            // Move camera back to room view
            CameraManager.Instance.MoveToRoomView();

            // CRT power off effect
            if (_crtEffect != null)
            {
                _crtEffect.PowerOff(0.8f);
            }

            UpdateButtonStates();

            // Button feedback
            if (_powerButton != null)
            {
                UITweenAnimations.ButtonPress(_powerButton.transform);
            }
        }

        /// <summary>
        /// Starts monitoring the active citizen
        /// </summary>
        public void StartMonitoring()
        {
            if (!_isPoweredOn || _activeCitizen == null || _isMonitoring)
                return;

            _isMonitoring = true;

            // Start stimulation on citizen
            if (_aiManager != null)
            {
                _aiManager.StartStimulation(_activeCitizen);
            }

            UpdateButtonStates();

            // Button feedback
            if (_startStimulationButton != null)
            {
                UITweenAnimations.ButtonPress(_startStimulationButton.transform);
            }

            // Glitch effect on start
            if (_crtEffect != null)
            {
                _crtEffect.TriggerGlitch(0.2f, 1.5f);
            }
        }

        /// <summary>
        /// Stops monitoring
        /// </summary>
        public void StopMonitoring()
        {
            if (!_isMonitoring)
                return;

            _isMonitoring = false;

            // Stop stimulation
            if (_aiManager != null)
            {
                _aiManager.StopStimulation();
            }

            // Stop alert animation if running
            _alertAnimation?.Kill();

            UpdateButtonStates();

            // Button feedback
            if (_stopStimulationButton != null)
            {
                UITweenAnimations.ButtonPress(_stopStimulationButton.transform);
            }
        }

        /// <summary>
        /// Resets the monitor (clears data, resets citizen)
        /// </summary>
        public void ResetMonitor()
        {
            StopMonitoring();

            // Clear buffer
            _bufferManager.Clear();

            // Reset citizen
            if (_activeCitizen != null)
            {
                _activeCitizen.Reset();
            }

            // Reset obedience
            ObedienceController.Instance.ResetToDefault();

            // Button feedback
            if (_resetButton != null)
            {
                UITweenAnimations.ButtonPress(_resetButton.transform);
                _resetButton.transform.DOPunchRotation(new Vector3(0, 0, 360f), 0.5f);
            }

            // Glitch effect
            if (_crtEffect != null)
            {
                _crtEffect.TriggerGlitch(0.3f, 2f);
            }
        }

        /// <summary>
        /// Updates button interactable states
        /// </summary>
        private void UpdateButtonStates()
        {
            if (_startStimulationButton != null)
            {
                _startStimulationButton.interactable = _isPoweredOn && !_isMonitoring && _activeCitizen != null;
            }

            if (_stopStimulationButton != null)
            {
                _stopStimulationButton.interactable = _isMonitoring;
            }

            if (_resetButton != null)
            {
                _resetButton.interactable = _isPoweredOn;
            }
        }

        /// <summary>
        /// Called when citizen reaches stabilized state
        /// </summary>
        private void OnCitizenStabilized(CitizenController citizen)
        {
            // Success feedback
            if (_crtEffect != null)
            {
                UITweenAnimations.ColorFlash(_statusIndicator, _stabilizedColor, 0.5f);
            }

            // Pulse waveforms
            foreach (var waveform in _waveformDisplays)
            {
                if (waveform != null)
                {
                    waveform.Pulse(1.1f, 0.4f);
                }
            }
        }

        /// <summary>
        /// Called when citizen reaches critical failure
        /// </summary>
        private void OnCitizenCriticalFailure(CitizenController citizen)
        {
            // Critical alert feedback
            if (_crtEffect != null)
            {
                _crtEffect.ShowStatic(1f, 0.8f);
            }

            // Start continuous alert animation
            if (_statusIndicator != null)
            {
                _alertAnimation = UITweenAnimations.CriticalAlert(
                    _statusIndicator.transform,
                    _criticalColor,
                    _statusIndicator
                );
            }

            // Shake UI
            transform.DOShakePosition(0.5f, 5f, 30);
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (_powerButton != null)
            {
                _powerButton.onClick.RemoveListener(TogglePower);
            }

            if (_startStimulationButton != null)
            {
                _startStimulationButton.onClick.RemoveListener(StartMonitoring);
            }

            if (_stopStimulationButton != null)
            {
                _stopStimulationButton.onClick.RemoveListener(StopMonitoring);
            }

            if (_resetButton != null)
            {
                _resetButton.onClick.RemoveListener(ResetMonitor);
            }

            // Unsubscribe from citizen events
            if (_activeCitizen != null)
            {
                _activeCitizen.OnStabilized -= OnCitizenStabilized;
                _activeCitizen.OnCriticalFailure -= OnCitizenCriticalFailure;
            }

            // Kill any running animations
            _alertAnimation?.Kill();
        }
    }
}

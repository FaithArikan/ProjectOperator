using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using NeuralWaveBureau.Data;
using NeuralWaveBureau.AI;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Main controller for the Brain Activity Monitor UI system.
    /// Manages waveform displays, obedience control.
    /// Coordinates all UI components and handles data flow from AIManager.
    /// </summary>
    public class BrainActivityMonitor : MonoBehaviour
    {
        public static BrainActivityMonitor Instance { get; private set; }

        [Header("Core Components")]
        [SerializeField]
        private Canvas mainComputerCanvas;

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

        [Header("Settings")]
        [SerializeField]
        private int _historyBufferSize = 120;

        [SerializeField]
        private float _updateRate = 30f; // Hz

        [SerializeField]
        private bool _showDebugInfo = true;

        [SerializeField]
        [Tooltip("If true, monitoring will automatically start when the monitor powers on")]
        private bool _autoStartMonitoring = false;

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
        [SerializeField]
        private MonitoringStation _monitoringStation;

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
            Instance = this;

            // Initialize buffer manager
            _bufferManager = new DataBufferManager(NeuralProfile.BandCount, _historyBufferSize);

            // Initialize waveform displays
            InitializeWaveformDisplays();
        }

        private void Start()
        {
            _aiManager = AIManager.Instance;

            // Start powered off - initialize screen to off state
            _isPoweredOn = false;

            // Deactivate the main screen panel - it's like the computer is off
            if (mainComputerCanvas != null)
            {
                mainComputerCanvas.gameObject.SetActive(false);
                Debug.Log("[BrainActivityMonitor] Main screen panel deactivated - Computer OFF");
            }

            // Deactivate all waveforms at start
            foreach (var waveform in _waveformDisplays)
            {
                if (waveform != null)
                {
                    waveform.gameObject.SetActive(false);
                }
            }

            // Make sure camera starts in room view (CameraManager already does this, but ensure it)
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.MoveToRoomView();
            }
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
        /// Powers on the monitor - like turning on a computer
        /// </summary>
        public void PowerOn()
        {
            if (_isPoweredOn)
                return;

            _isPoweredOn = true;

            // Activate the main screen panel - Computer powers on!
            if (mainComputerCanvas != null)
            {
                mainComputerCanvas.gameObject.SetActive(true);
                Debug.Log("[BrainActivityMonitor] Main screen panel activated - Computer ON");
            }

            // Auto-find and set citizen if not already set
            if (_activeCitizen == null)
            {
                var citizen = FindFirstObjectByType<CitizenController>();
                if (citizen != null)
                {
                    SetActiveCitizen(citizen);
                    Debug.Log($"[BrainActivityMonitor] Auto-assigned citizen: {citizen.CitizenId}");
                }
                else
                {
                    Debug.LogWarning("[BrainActivityMonitor] No citizen found in scene. Create a GameObject with CitizenController.");
                }
            }

            // Move camera to monitor view
            CameraManager.Instance.MoveToMonitorView();

            // Button feedback
            if (_powerButton != null)
            {
                UITweenAnimations.ButtonPress(_powerButton.transform);
            }

            // Check if there's a citizen at the monitoring station and resume monitoring
            bool shouldStartMonitoring = false;

            if (_activeCitizen != null)
            {
                // If there's an active citizen, start monitoring them
                shouldStartMonitoring = true;
                Debug.Log("[BrainActivityMonitor] Monitor powered ON - Resuming monitoring of active citizen");
            }
            else if (_autoStartMonitoring)
            {
                // Only auto-start if setting is enabled and no citizen yet
                shouldStartMonitoring = true;
                Debug.Log("[BrainActivityMonitor] Monitor powered ON - Auto-starting monitoring");
            }

            if (shouldStartMonitoring)
            {
                StartMonitoring();
            }
            else
            {
                Debug.Log("[BrainActivityMonitor] Monitor powered ON - Ready for manual monitoring start");
            }
        }

        /// <summary>
        /// Powers off the monitor - like shutting down a computer
        /// </summary>
        public void PowerOff()
        {
            if (!_isPoweredOn)
                return;

            // Stop monitoring
            StopMonitoring();

            _isPoweredOn = false;

            if (mainComputerCanvas != null)
            {
                mainComputerCanvas.gameObject.SetActive(false);
                Debug.Log("[BrainActivityMonitor] Main screen panel deactivated - Computer OFF");
            }

            if (CameraManager.Instance.CurrentView == CameraView.Monitor)
            {
                // Move camera back to room view
                CameraManager.Instance.MoveToRoomView();
            }


            // Button feedback
            if (_powerButton != null)
            {
                UITweenAnimations.ButtonPress(_powerButton.transform);
            }
        }

        /// <summary>
        /// Starts monitoring the active citizen - Shows waveforms and starts evaluation
        /// </summary>
        public void StartMonitoring()
        {
            Debug.Log("[BrainActivityMonitor] StartMonitoring() called");
            if (_isMonitoring)
                return;

            // Must be powered on to start monitoring
            if (!_isPoweredOn)
            {
                Debug.LogWarning("[BrainActivityMonitor] Cannot start monitoring - Computer is not powered on!");
                return;
            }

            // Check if we have a citizen to monitor
            if (_activeCitizen == null)
            {
                Debug.LogWarning("[BrainActivityMonitor] Cannot start monitoring - No citizen assigned!");
                return;
            }

            _isMonitoring = true;

            // Activate and animate waveforms in - Waveforms appear on screen!
            foreach (var waveform in _waveformDisplays)
            {
                if (waveform != null)
                {
                    waveform.gameObject.SetActive(true);
                    waveform.AnimateIn(0.8f);
                }
            }

            // Animate obedience controller
            if (ObedienceController.Instance != null)
            {
                ObedienceController.Instance.AnimateIn(1f);
            }

            // Initialize wave sample with citizen's target values to prevent instant instability
            if (_aiManager != null)
            {
                // Set initial wave to match the citizen's target profile
                float[] initialWave = new float[NeuralProfile.BandCount];
                for (int i = 0; i < NeuralProfile.BandCount; i++)
                {
                    initialWave[i] = _activeCitizen.Profile.BandTargets[i];
                }
                _aiManager.SetWaveSample(initialWave);

                // Start stimulation on citizen - Evaluation starts!
                _aiManager.StartStimulation(_activeCitizen);
            }

            Debug.Log("[BrainActivityMonitor] Monitoring STARTED - Waveforms visible, evaluation running");
        }

        /// <summary>
        /// Stops monitoring
        /// </summary>
        public void StopMonitoring()
        {
            if (!_isMonitoring)
                return;

            _isMonitoring = false;

            // Animate waveforms out and deactivate them
            foreach (var waveform in _waveformDisplays)
            {
                if (waveform != null)
                {
                    waveform.AnimateOut(0.3f, () =>
                    {
                        waveform.gameObject.SetActive(false);
                    });
                }
            }

            // Stop stimulation
            if (_aiManager != null)
            {
                _aiManager.StopStimulation();
            }

            // Stop alert animation if running
            _alertAnimation?.Kill();
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
        }

        /// <summary>
        /// Called when citizen reaches stabilized state
        /// </summary>
        private void OnCitizenStabilized(CitizenController citizen)
        {
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
    }
}

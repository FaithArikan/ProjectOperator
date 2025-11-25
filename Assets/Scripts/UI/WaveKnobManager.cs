using UnityEngine;
using System.Collections.Generic;
using NeuralWaveBureau.AI;
using NeuralWaveBureau.Data;
using DG.Tweening;
using TMPro;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Manages a set of radio knobs for controlling brain wave bands.
    /// Each knob controls one band (Delta, Theta, Alpha, Beta, Gamma).
    /// </summary>
    public class WaveKnobManager : MonoBehaviour
    {
        public static WaveKnobManager Instance { get; private set; }

        [Header("Knobs")]
        [SerializeField]
        private List<RadioKnobController> _waveKnobs = new List<RadioKnobController>();

        [Header("Labels")]
        [SerializeField]
        private List<TextMeshProUGUI> _bandLabels = new List<TextMeshProUGUI>();

        [Header("Settings")]
        [SerializeField]
        private bool _updateInRealTime = true;

        [SerializeField]
        [Range(0.01f, 0.5f)]
        private float _updateInterval = 0.05f; // How often to send wave updates

        [SerializeField]
        private bool _deactivateOthersOnSelect = true; // Only one knob active at a time

        // Current wave values
        private float[] _currentWaveValues = new float[5];
        private float _updateTimer = 0f;
        private AIManager _aiManager;
        private int _activeKnobIndex = -1;

        private void Awake()
        {
            Instance = this;

            // Initialize wave values to middle
            for (int i = 0; i < 5; i++)
            {
                _currentWaveValues[i] = 0.5f;
            }
        }

        private void Start()
        {
            _aiManager = AIManager.Instance;

            // Setup knobs
            SetupKnobs();

            // Initial wave sample
            SendWaveSample();

            // Auto-select first knob on start
            if (_waveKnobs.Count > 0 && _waveKnobs[0] != null)
            {
                _waveKnobs[0].Activate();
                _activeKnobIndex = 0;
            }
        }

        private void Update()
        {
            if (!_updateInRealTime)
                return;

            _updateTimer += Time.deltaTime;
            if (_updateTimer >= _updateInterval)
            {
                _updateTimer = 0f;
                SendWaveSample();
            }
        }

        /// <summary>
        /// Sets up all knobs with colors and event handlers
        /// </summary>
        private void SetupKnobs()
        {
            for (int i = 0; i < _waveKnobs.Count && i < NeuralProfile.BandCount; i++)
            {
                if (_waveKnobs[i] == null)
                    continue;

                int bandIndex = i; // Capture for closure

                // Set initial value
                _waveKnobs[i].SetValue(_currentWaveValues[i], false);

                // Subscribe to value changes
                _waveKnobs[i].OnValueChanged += (value) => OnKnobValueChanged(bandIndex, value);

                // Subscribe to activation
                _waveKnobs[i].OnKnobActivated += () => OnKnobActivated(bandIndex);

                // Subscribe to user interaction
                _waveKnobs[i].OnUserInteracted += () =>
                {
                    if (_aiManager != null) _aiManager.NotifyUserInteraction();
                };

                // Set label
                if (i < _bandLabels.Count && _bandLabels[i] != null)
                {
                    _bandLabels[i].text = NeuralProfile.BandNames[i];
                }
            }
        }

        /// <summary>
        /// Called when a knob value changes
        /// </summary>
        private void OnKnobValueChanged(int bandIndex, float value)
        {
            if (bandIndex < 0 || bandIndex >= _currentWaveValues.Length)
                return;

            _currentWaveValues[bandIndex] = value;

            // Send immediate update if not using real-time updates
            if (!_updateInRealTime)
            {
                SendWaveSample();
            }
        }

        /// <summary>
        /// Called when a knob is activated
        /// </summary>
        private void OnKnobActivated(int bandIndex)
        {
            if (_deactivateOthersOnSelect)
            {
                // Deactivate all other knobs
                for (int i = 0; i < _waveKnobs.Count; i++)
                {
                    if (i != bandIndex && _waveKnobs[i] != null)
                    {
                        _waveKnobs[i].Deactivate();
                    }
                }
            }

            _activeKnobIndex = bandIndex;
        }

        /// <summary>
        /// Sends current wave values to AIManager
        /// </summary>
        private void SendWaveSample()
        {
            if (_aiManager == null)
                return;

            _aiManager.SetWaveSample(_currentWaveValues);
        }

        /// <summary>
        /// Sets all knobs to specific values
        /// </summary>
        public void SetAllValues(float[] values, bool animate = true)
        {
            if (values == null)
                return;

            for (int i = 0; i < _waveKnobs.Count && i < values.Length; i++)
            {
                if (_waveKnobs[i] != null)
                {
                    _waveKnobs[i].SetValue(values[i], animate);
                    _currentWaveValues[i] = values[i];
                }
            }

            SendWaveSample();
        }

        /// <summary>
        /// Sets a specific band value
        /// </summary>
        public void SetBandValue(int bandIndex, float value, bool animate = true)
        {
            if (bandIndex < 0 || bandIndex >= _waveKnobs.Count)
                return;

            if (_waveKnobs[bandIndex] != null)
            {
                _waveKnobs[bandIndex].SetValue(value, animate);
                _currentWaveValues[bandIndex] = value;
            }

            SendWaveSample();
        }

        /// <summary>
        /// Resets all knobs to default (0.5)
        /// </summary>
        public void ResetAllKnobs()
        {
            for (int i = 0; i < _waveKnobs.Count; i++)
            {
                if (_waveKnobs[i] != null)
                {
                    _waveKnobs[i].ResetToDefault(0.5f);
                    _currentWaveValues[i] = 0.5f;
                }
            }

            SendWaveSample();

            // Pulse feedback
            transform.DOPunchScale(Vector3.one * 0.05f, 0.3f);
        }

        /// <summary>
        /// Sets values to match a target profile
        /// </summary>
        public void MatchProfile(NeuralProfile profile)
        {
            if (profile == null)
                return;

            SetAllValues(profile.BandTargets, true);
        }

        /// <summary>
        /// Gets current wave values
        /// </summary>
        public float[] GetCurrentValues()
        {
            return (float[])_currentWaveValues.Clone();
        }

        /// <summary>
        /// Gets value for a specific band
        /// </summary>
        public float GetBandValue(int bandIndex)
        {
            if (bandIndex < 0 || bandIndex >= _currentWaveValues.Length)
                return 0f;

            return _currentWaveValues[bandIndex];
        }

        /// <summary>
        /// Animates all knobs in
        /// </summary>
        public void AnimateIn(float duration = 0.5f)
        {
            for (int i = 0; i < _waveKnobs.Count; i++)
            {
                if (_waveKnobs[i] != null)
                {
                    float delay = i * 0.1f;
                    _waveKnobs[i].transform.localScale = Vector3.zero;
                    _waveKnobs[i].transform.DOScale(Vector3.one, duration)
                        .SetDelay(delay)
                        .SetEase(Ease.OutBack);
                }
            }
        }

        /// <summary>
        /// Animates all knobs out
        /// </summary>
        public void AnimateOut(float duration = 0.3f)
        {
            for (int i = 0; i < _waveKnobs.Count; i++)
            {
                if (_waveKnobs[i] != null)
                {
                    _waveKnobs[i].transform.DOScale(Vector3.zero, duration)
                        .SetEase(Ease.InBack);
                }
            }
        }

        /// <summary>
        /// Deactivates all knobs
        /// </summary>
        public void DeactivateAllKnobs()
        {
            foreach (var knob in _waveKnobs)
            {
                if (knob != null)
                {
                    knob.Deactivate();
                }
            }

            _activeKnobIndex = -1;
        }

        private void OnDestroy()
        {
            // Unsubscribe from all events
            foreach (var knob in _waveKnobs)
            {
                if (knob != null)
                {
                    knob.OnValueChanged = null;
                    knob.OnKnobActivated = null;
                }
            }
        }
    }
}

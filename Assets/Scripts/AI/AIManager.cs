using UnityEngine;
using System.Collections.Generic;
using NeuralWaveBureau.Data;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Central coordinator for the AI system. Manages global wave input and feedback for the active citizen.
    /// Singleton pattern for easy access throughout the game.
    /// </summary>
    public class AIManager : MonoBehaviour
    {
        // Singleton instance
        public static AIManager Instance;

        [Header("Settings")]
        [SerializeField]
        private AISettings _aiSettings;

        // Current wave sample being broadcast to all active citizens
        private WaveSample _currentWaveSample;

        // Currently active citizen (being stimulated)
        private CitizenController _activeCitizen;

        // Feedback manager reference
        private FeedbackManager _feedbackManager;

        public AISettings Settings => _aiSettings;
        public CitizenController ActiveCitizen => _activeCitizen;
        public WaveSample CurrentWaveSample => _currentWaveSample;
        public bool HasUserInteracted { get; private set; }

        private void Awake()
        {
            Instance = this;

            // Validate settings
            if (_aiSettings == null)
            {
                Debug.LogError("[AIManager] No AI Settings assigned!");
                return;
            }

            // Initialize wave sample
            _currentWaveSample = WaveSample.Zero();

            // Get feedback manager
            _feedbackManager = GetComponent<FeedbackManager>();
        }

        /// <summary>
        /// Sets the current wave sample that will be evaluated by active citizens
        /// </summary>
        public void SetWaveSample(float[] bandValues)
        {
            _currentWaveSample = new WaveSample(Time.time, bandValues);
            _currentWaveSample.Validate();

            // Update active citizen
            if (_activeCitizen != null && _activeCitizen.IsActive)
            {
                _activeCitizen.UpdateWaveSample(_currentWaveSample);

                // Update feedback based on evaluation
                UpdateFeedback();
            }
        }

        /// <summary>
        /// Sets wave sample with individual band values
        /// </summary>
        public void SetWaveSample(float delta, float theta, float alpha, float beta, float gamma)
        {
            SetWaveSample(new float[] { delta, theta, alpha, beta, gamma });
        }

        /// <summary>
        /// Starts stimulation on a citizen
        /// </summary>
        public bool StartStimulation(CitizenController citizen)
        {
            // Stop current active citizen
            if (_activeCitizen != null && _activeCitizen != citizen)
            {
                StopStimulation();
            }

            _activeCitizen = citizen;
            HasUserInteracted = false;
            _activeCitizen.StartStimulation();

            // Subscribe to citizen events for feedback
            _activeCitizen.OnStabilized += HandleCitizenStabilized;
            _activeCitizen.OnCriticalFailure += HandleCitizenCriticalFailure;
            _activeCitizen.OnRecovered += HandleCitizenRecovered;

            if (_aiSettings.enableVerboseLogging)
            {
                Debug.Log($"[AIManager] Started stimulation on {citizen.CitizenId}");
            }

            return true;
        }

        /// <summary>
        /// Stops stimulation on the active citizen
        /// </summary>
        public void StopStimulation()
        {
            if (_activeCitizen != null)
            {
                // Unsubscribe from events
                _activeCitizen.OnStabilized -= HandleCitizenStabilized;
                _activeCitizen.OnCriticalFailure -= HandleCitizenCriticalFailure;
                _activeCitizen.OnRecovered -= HandleCitizenRecovered;

                _activeCitizen.StopStimulation();
                _activeCitizen = null;

                if (_aiSettings.enableVerboseLogging)
                {
                    Debug.Log("[AIManager] Stopped stimulation");
                }
            }
        }

        /// <summary>
        /// Updates feedback manager based on current evaluation
        /// </summary>
        private void UpdateFeedback()
        {
            if (_feedbackManager == null || _activeCitizen == null)
                return;

            float score = _activeCitizen.EvaluationScore;
            float instability = _activeCitizen.Instability;

            // Update ambient pulse based on score
            _feedbackManager.SetAmbientPulse(score);

            // Check for alarm condition
            if (instability >= _aiSettings.instabilityFailThreshold * 0.8f)
            {
                // Warning zone
                _feedbackManager.SetAlarmLevel(instability);
            }
        }

        // Event handlers
        private void HandleCitizenStabilized(CitizenController citizen)
        {
            if (_aiSettings.enableVerboseLogging)
            {
                Debug.Log($"[AIManager] Citizen stabilized: {citizen.CitizenId}");
            }

            _feedbackManager?.PlaySuccessFeedback();
        }

        private void HandleCitizenCriticalFailure(CitizenController citizen)
        {
            if (_aiSettings.enableVerboseLogging)
            {
                Debug.Log($"[AIManager] Citizen critical failure: {citizen.CitizenId}");
            }

            _feedbackManager?.TriggerAlarm();
        }

        private void HandleCitizenRecovered(CitizenController citizen)
        {
            if (_aiSettings.enableVerboseLogging)
            {
                Debug.Log($"[AIManager] Citizen recovered: {citizen.CitizenId}");
            }
        }

        public void NotifyUserInteraction()
        {
            HasUserInteracted = true;
        }
    }
}

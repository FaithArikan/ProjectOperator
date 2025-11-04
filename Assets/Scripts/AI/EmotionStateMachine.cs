using UnityEngine;
using NeuralWaveBureau.Data;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Citizen emotional/behavioral states
    /// </summary>
    public enum CitizenState
    {
        Idle,              // Default state, waiting for stimulation
        BeingStimulated,   // Wave input is active
        Stabilized,        // Successfully matched to target wave
        Agitated,          // Incorrect wave causing distress
        CriticalFailure,   // Instability exceeded threshold
        Recovering         // Returning to normal after agitation
    }

    /// <summary>
    /// Finite state machine managing citizen emotional states and transitions.
    /// Drives behavior based on wave evaluation and instability levels.
    /// </summary>
    public class EmotionStateMachine
    {
        // Current state
        private CitizenState _currentState;

        // Time spent in current state
        private float _stateTime;

        // Current instability level (0..1)
        private float _instability;

        // Reference to profile and settings
        private NeuralProfile _profile;
        private AISettings _settings;

        // Delegate for state change events
        public delegate void StateChangedDelegate(CitizenState oldState, CitizenState newState);
        public event StateChangedDelegate OnStateChanged;

        public CitizenState CurrentState => _currentState;
        public float StateTime => _stateTime;
        public float Instability => _instability;

        public EmotionStateMachine(NeuralProfile profile, AISettings settings)
        {
            _profile = profile;
            _settings = settings;
            _currentState = CitizenState.Idle;
            _stateTime = 0f;
            _instability = profile.baselineInstability;
        }

        /// <summary>
        /// Updates the state machine with current evaluation score
        /// </summary>
        /// <param name="evaluationScore">Current wave evaluation score (0..1)</param>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float evaluationScore, float deltaTime)
        {
            _stateTime += deltaTime;

            // Update instability based on evaluation score
            UpdateInstability(evaluationScore, deltaTime);

            // Process state transitions
            ProcessStateTransitions(evaluationScore);
        }

        /// <summary>
        /// Updates instability meter based on wave evaluation
        /// </summary>
        private void UpdateInstability(float score, float deltaTime)
        {
            // If score is below overload threshold, build instability
            if (score <= _settings.overloadThreshold)
            {
                float delta = (_settings.overloadThreshold - score) * _profile.instabilityRate * deltaTime;
                _instability += delta;
            }
            // If score is good and not in critical state, recover instability
            else if (score >= _settings.successThreshold && _currentState != CitizenState.CriticalFailure)
            {
                _instability -= _settings.instabilityRecoveryRate * deltaTime;
            }

            // Clamp instability to valid range
            _instability = Mathf.Clamp01(_instability);
        }

        /// <summary>
        /// Processes state transitions based on current conditions
        /// </summary>
        private void ProcessStateTransitions(float score)
        {
            CitizenState previousState = _currentState;

            switch (_currentState)
            {
                case CitizenState.Idle:
                    // Start stimulation when wave input begins (handled externally via StartStimulation)
                    break;

                case CitizenState.BeingStimulated:
                    // Check for critical failure
                    if (_instability >= _settings.instabilityFailThreshold)
                    {
                        TransitionTo(CitizenState.CriticalFailure);
                    }
                    // Check for stabilization (good score for minimum time)
                    else if (score >= _settings.successThreshold && _stateTime >= _profile.minStimulationTime)
                    {
                        TransitionTo(CitizenState.Stabilized);
                    }
                    // Check for agitation (bad score)
                    else if (score <= _settings.overloadThreshold)
                    {
                        TransitionTo(CitizenState.Agitated);
                    }
                    break;

                case CitizenState.Stabilized:
                    // Can return to stimulated if score drops
                    if (score < _settings.successThreshold)
                    {
                        TransitionTo(CitizenState.BeingStimulated);
                    }
                    break;

                case CitizenState.Agitated:
                    // Check for critical failure
                    if (_instability >= _settings.instabilityFailThreshold)
                    {
                        TransitionTo(CitizenState.CriticalFailure);
                    }
                    // Check for recovery (score improves)
                    else if (score >= _settings.successThreshold)
                    {
                        TransitionTo(CitizenState.Recovering);
                    }
                    break;

                case CitizenState.CriticalFailure:
                    // Stay in critical failure (game logic may reset manually)
                    break;

                case CitizenState.Recovering:
                    // After recovery time, return to idle or stabilized
                    if (_stateTime >= _profile.recoveryTime)
                    {
                        if (score >= _settings.successThreshold)
                        {
                            TransitionTo(CitizenState.Stabilized);
                        }
                        else
                        {
                            TransitionTo(CitizenState.Idle);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Manually starts wave stimulation (call when player begins wave input)
        /// </summary>
        public void StartStimulation()
        {
            if (_currentState == CitizenState.Idle)
            {
                TransitionTo(CitizenState.BeingStimulated);
            }
        }

        /// <summary>
        /// Manually stops stimulation and returns to idle
        /// </summary>
        public void StopStimulation()
        {
            if (_currentState == CitizenState.BeingStimulated || _currentState == CitizenState.Agitated)
            {
                TransitionTo(CitizenState.Idle);
            }
        }

        /// <summary>
        /// Resets the state machine to idle
        /// </summary>
        public void Reset()
        {
            TransitionTo(CitizenState.Idle);
            _instability = _profile.baselineInstability;
        }

        /// <summary>
        /// Transitions to a new state
        /// </summary>
        private void TransitionTo(CitizenState newState)
        {
            if (_currentState == newState)
                return;

            CitizenState oldState = _currentState;
            _currentState = newState;
            _stateTime = 0f;

            // Invoke state change event
            OnStateChanged?.Invoke(oldState, newState);

            // Log state transition if verbose logging enabled
            if (_settings.enableVerboseLogging)
            {
                Debug.Log($"[EmotionStateMachine] Transition: {oldState} -> {newState} (Instability: {_instability:F2})");
            }
        }

        /// <summary>
        /// Gets normalized agitation level for animator (0..1)
        /// </summary>
        public float GetAgitationLevel()
        {
            switch (_currentState)
            {
                case CitizenState.Idle:
                    return 0f;
                case CitizenState.BeingStimulated:
                    return _instability * 0.5f;
                case CitizenState.Stabilized:
                    return 0f;
                case CitizenState.Agitated:
                    return Mathf.Clamp01(_instability);
                case CitizenState.CriticalFailure:
                    return 1f;
                case CitizenState.Recovering:
                    return Mathf.Lerp(0.5f, 0f, _stateTime / _profile.recoveryTime);
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Gets normalized composure level for animator (0..1)
        /// </summary>
        public float GetComposureLevel()
        {
            return 1f - GetAgitationLevel();
        }
    }
}

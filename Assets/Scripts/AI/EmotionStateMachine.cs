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
        Stimulated,   // Wave input is active
        Stabilized,        // Successfully matched to target wave
        Agitated,          // Incorrect wave causing distress
        Critical_Failure,   // Instability exceeded threshold
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

        // Grace period tracking
        private float _timeSinceStimulationStart;
        private const float GRACE_PERIOD_DURATION = 10f; // 30 seconds of safe time
        private const float GRACE_PERIOD_FADE_DURATION = 5f; // 15 seconds fade-out

        // Obedience-based instability multiplier
        private float _obedienceInstabilityMultiplier = 1f; // Default = no change

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
            _instability = 0f; // All citizens start at 0 instability
            _timeSinceStimulationStart = 0f;
        }

        /// <summary>
        /// Updates the state machine with current evaluation score
        /// </summary>
        /// <param name="evaluationScore">Current wave evaluation score (0..1)</param>
        /// <param name="obedienceLevel">Current citizen obedience level (0..100)</param>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float evaluationScore, float obedienceLevel, float deltaTime)
        {
            _stateTime += deltaTime;
            _timeSinceStimulationStart += deltaTime; // Track total stimulation time

            // Convert obedience to bonus multiplier (0-100 -> 0.5-1.5)
            // Low obedience (0%) = 0.5x multiplier (harder)
            // High obedience (100%) = 1.5x multiplier (easier)
            float normalizedObedience = obedienceLevel / 100f;
            float obedienceMultiplier = Mathf.Lerp(0.5f, 1.5f, normalizedObedience);

            // Apply obedience bonus to wave score
            // This means: good wave matching + high obedience = excellent score
            // Good wave matching + low obedience = okay score
            // Bad wave matching + high obedience = poor score (still need to match waves)
            float finalScore = Mathf.Clamp01(evaluationScore * obedienceMultiplier);

            // Update instability based on combined score
            UpdateInstability(finalScore, deltaTime);

            // Process state transitions using combined score
            ProcessStateTransitions(finalScore);
        }

        /// <summary>
        /// Updates instability meter based on obedience evaluation
        /// </summary>
        private void UpdateInstability(float score, float deltaTime)
        {
            // Calculate grace period multiplier (0 = no instability, 1 = full instability)
            float gracePeriodMultiplier = CalculateGracePeriodMultiplier();

            // Add small hysteresis buffer (2% buffer on each side)
            const float HYSTERESIS_BUFFER = 0.02f;

            // Building instability (with hysteresis)
            if (score <= _settings.overloadThreshold - HYSTERESIS_BUFFER)
            {
                // Apply BOTH profile instability rate AND obedience multiplier
                float effectiveInstabilityRate = _profile.instabilityRate * _obedienceInstabilityMultiplier;
                float baseDelta = (_settings.overloadThreshold - score) * effectiveInstabilityRate * deltaTime;

                // Apply grace period multiplier to slow/prevent buildup early
                float delta = baseDelta * gracePeriodMultiplier;

                _instability += delta;
            }
            // Recovering instability (with hysteresis)
            else if (score >= _settings.successThreshold + HYSTERESIS_BUFFER && _currentState != CitizenState.Critical_Failure)
            {
                // Recovery is NOT affected by grace period - players can always recover
                _instability -= _settings.instabilityRecoveryRate * deltaTime;
            }
            // Neutral zone: no change

            // Clamp instability to valid range [0, 1]
            _instability = Mathf.Clamp01(_instability);
        }

        /// <summary>
        /// Calculates grace period multiplier based on time since stimulation started
        /// </summary>
        private float CalculateGracePeriodMultiplier()
        {
            // During grace period (0-30 seconds): no instability builds (multiplier = 0)
            if (_timeSinceStimulationStart < GRACE_PERIOD_DURATION)
            {
                return 0f;
            }

            // During fade period (30-45 seconds): gradually increase from 0 to 1
            if (_timeSinceStimulationStart < GRACE_PERIOD_DURATION + GRACE_PERIOD_FADE_DURATION)
            {
                float fadeProgress = (_timeSinceStimulationStart - GRACE_PERIOD_DURATION) / GRACE_PERIOD_FADE_DURATION;
                // Use smooth curve for gradual increase
                return Mathf.SmoothStep(0f, 1f, fadeProgress);
            }

            // After grace period + fade (45+ seconds): full instability (multiplier = 1)
            return 1f;
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

                case CitizenState.Stimulated:
                    // Check for critical failure
                    if (_instability >= _settings.instabilityFailThreshold)
                    {
                        TransitionTo(CitizenState.Critical_Failure);
                    }
                    // Check for stabilization (good score for minimum time)
                    else if (score >= _settings.successThreshold && _stateTime >= _profile.minStimulationTime)
                    {
                        TransitionTo(CitizenState.Stabilized);
                    }
                    // Reset timer if score drops below threshold (restart the 5-second countdown)
                    else if (score < _settings.successThreshold && _stateTime > 0.1f)
                    {
                        _stateTime = 0f; // Reset timer - player must maintain score for full duration
                    }
                    // Check for agitation (bad score) - only if significantly below threshold
                    else if (score <= _settings.overloadThreshold && _stateTime > 1f)
                    {
                        TransitionTo(CitizenState.Agitated);
                    }
                    break;

                case CitizenState.Stabilized:
                    // Can return to stimulated if score drops significantly (with hysteresis)
                    // Only transition back if score is really bad (below 0.3)
                    if (score < 0.3f)
                    {
                        TransitionTo(CitizenState.Stimulated);
                    }
                    break;

                case CitizenState.Agitated:
                    // Check for critical failure
                    if (_instability >= _settings.instabilityFailThreshold)
                    {
                        TransitionTo(CitizenState.Critical_Failure);
                    }
                    // Check for recovery (score improves)
                    else if (score >= _settings.successThreshold)
                    {
                        TransitionTo(CitizenState.Recovering);
                    }
                    break;

                case CitizenState.Critical_Failure:
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
                TransitionTo(CitizenState.Stimulated);
                _timeSinceStimulationStart = 0f; // Reset grace period timer
            }
        }

        /// <summary>
        /// Manually stops stimulation and returns to idle
        /// </summary>
        public void StopStimulation()
        {
            if (_currentState == CitizenState.Stimulated || _currentState == CitizenState.Agitated)
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
            _instability = 0f; // Reset to zero, not baseline
            _timeSinceStimulationStart = 0f;
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
                case CitizenState.Stimulated:
                    return _instability * 0.5f;
                case CitizenState.Stabilized:
                    return 0f;
                case CitizenState.Agitated:
                    return Mathf.Clamp01(_instability);
                case CitizenState.Critical_Failure:
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

        /// <summary>
        /// Sets the obedience-based instability rate multiplier
        /// </summary>
        public void SetObedienceMultiplier(float multiplier)
        {
            _obedienceInstabilityMultiplier = multiplier;
        }

        /// <summary>
        /// Gets the current grace period multiplier for UI display
        /// </summary>
        public float GracePeriodMultiplier => CalculateGracePeriodMultiplier();

        /// <summary>
        /// Gets remaining grace period time in seconds
        /// </summary>
        public float RemainingGracePeriod
        {
            get
            {
                float remaining = (GRACE_PERIOD_DURATION + GRACE_PERIOD_FADE_DURATION) - _timeSinceStimulationStart;
                return Mathf.Max(0f, remaining);
            }
        }
    }
}

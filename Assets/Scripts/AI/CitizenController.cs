using UnityEngine;
using System.Collections;
using NeuralWaveBureau.Data;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Per-citizen behavior controller. Manages wave evaluation, state machine, and animation updates.
    /// Attach this to each citizen GameObject in the scene.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CitizenController : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField]
        private string _citizenId = "citizen_01";

        [Header("Neural Profile")]
        [SerializeField]
        private NeuralProfile _neuralProfile;

        [Header("Components")]
        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private AudioSource _audioSource;

        // AI systems
        private WaveEvaluator _evaluator;
        private EmotionStateMachine _stateMachine;
        private AISettings _settings;

        // Evaluation coroutine
        private Coroutine _evaluationCoroutine;

        // Current wave sample being evaluated
        private WaveSample _currentSample;

        // Is this citizen currently active
        private bool _isActive;

        // Public accessors
        public string CitizenId => _citizenId;
        public NeuralProfile Profile => _neuralProfile;
        public CitizenState CurrentState => _stateMachine?.CurrentState ?? CitizenState.Idle;
        public float EvaluationScore => _evaluator?.SmoothedScore ?? 0f;
        public float Instability => _stateMachine?.Instability ?? 0f;
        public bool IsActive => _isActive;

        // Events
        public delegate void CitizenEventDelegate(CitizenController citizen);
        public event CitizenEventDelegate OnStabilized;
        public event CitizenEventDelegate OnCriticalFailure;
        public event CitizenEventDelegate OnRecovered;

                /// <summary>
        /// Initializes the citizen with settings and profile
        /// </summary>
        public void Initialize(AISettings settings, NeuralProfile profile = null)
        {
            _settings = settings;

            // Use provided profile or keep existing
            if (profile != null)
            {
                _neuralProfile = profile;
            }

            // Validate profile
            if (_neuralProfile == null)
            {
                Debug.LogError($"[CitizenController] {_citizenId}: No neural profile assigned!");
                return;
            }

            // Create AI systems
            _evaluator = new WaveEvaluator(_neuralProfile, _settings);
            _stateMachine = new EmotionStateMachine(_neuralProfile, _settings);

            // Subscribe to state change events
            _stateMachine.OnStateChanged += HandleStateChanged;

            if (_settings.enableVerboseLogging)
            {
                Debug.Log($"[CitizenController] {_citizenId} initialized with profile: {_neuralProfile.displayName}");
            }
        }

        /// <summary>
        /// Starts wave stimulation for this citizen
        /// </summary>
        public void StartStimulation()
        {
            if (_stateMachine == null)
            {
                Debug.LogError($"[CitizenController] {_citizenId}: Not initialized!");
                return;
            }

            _isActive = true;
            _stateMachine.StartStimulation();

            // Start evaluation coroutine
            if (_evaluationCoroutine != null)
            {
                StopCoroutine(_evaluationCoroutine);
            }
            _evaluationCoroutine = StartCoroutine(EvaluationLoop());

            if (_settings.enableVerboseLogging)
            {
                Debug.Log($"[CitizenController] {_citizenId}: Started stimulation");
            }
        }

        /// <summary>
        /// Stops wave stimulation
        /// </summary>
        public void StopStimulation()
        {
            _isActive = false;
            _stateMachine?.StopStimulation();

            if (_evaluationCoroutine != null)
            {
                StopCoroutine(_evaluationCoroutine);
                _evaluationCoroutine = null;
            }

            if (_settings.enableVerboseLogging)
            {
                Debug.Log($"[CitizenController] {_citizenId}: Stopped stimulation");
            }
        }

        /// <summary>
        /// Updates the current wave sample (called by AIManager or external input system)
        /// </summary>
        public void UpdateWaveSample(WaveSample sample)
        {
            _currentSample = sample;
        }

        /// <summary>
        /// Evaluation loop running at fixed sample rate
        /// </summary>
        private IEnumerator EvaluationLoop()
        {
            float sampleInterval = _settings.GetSampleInterval();

            while (_isActive)
            {
                // Evaluate current sample
                float score = _evaluator.Evaluate(_currentSample);

                // Update state machine
                _stateMachine.Update(score, sampleInterval);

                // Update animator
                //UpdateAnimator();

                // Wait for next sample
                yield return new WaitForSeconds(sampleInterval);
            }
        }

        /// <summary>
        /// Updates animator parameters based on state machine
        /// </summary>
        private void UpdateAnimator()
        {
            if (_animator == null)
                return;

            float agitation = _stateMachine.GetAgitationLevel();
            float composure = _stateMachine.GetComposureLevel();

            _animator.SetFloat("agitation", agitation);
            _animator.SetFloat("composure", composure);

            // Trigger critical animation if in critical failure
            if (_stateMachine.CurrentState == CitizenState.CriticalFailure)
            {
                _animator.SetTrigger("critical");
            }
        }

        /// <summary>
        /// Handles state change events from state machine
        /// </summary>
        private void HandleStateChanged(CitizenState oldState, CitizenState newState)
        {
            if (_settings.enableVerboseLogging)
            {
                Debug.Log($"[CitizenController] {_citizenId}: State changed {oldState} -> {newState}");
            }

            // Play reaction audio
            PlayReactionAudio(newState);

            // Invoke events
            switch (newState)
            {
                case CitizenState.Stabilized:
                    OnStabilized?.Invoke(this);
                    break;
                case CitizenState.CriticalFailure:
                    OnCriticalFailure?.Invoke(this);
                    break;
                case CitizenState.Recovering:
                    OnRecovered?.Invoke(this);
                    break;
            }
        }

        /// <summary>
        /// Plays appropriate audio for state
        /// </summary>
        private void PlayReactionAudio(CitizenState state)
        {
            if (_audioSource == null || _neuralProfile.reactionClips == null || _neuralProfile.reactionClips.Length == 0)
                return;

            AudioClip clip = null;

            // Simple mapping: use index based on state
            int clipIndex = (int)state;
            if (clipIndex >= 0 && clipIndex < _neuralProfile.reactionClips.Length)
            {
                clip = _neuralProfile.reactionClips[clipIndex];
            }

            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Resets citizen to idle state
        /// </summary>
        public void Reset()
        {
            StopStimulation();
            _evaluator?.Reset();
            _stateMachine?.Reset();
            _currentSample = WaveSample.Zero();
        }

        /// <summary>
        /// Gets per-band scores for debugging
        /// </summary>
        public float[] GetBandScores()
        {
            return _evaluator?.GetBandScores(_currentSample) ?? new float[NeuralProfile.BandCount];
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged -= HandleStateChanged;
            }
        }

        private void OnValidate()
        {
            // Auto-assign animator
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }
    }
}

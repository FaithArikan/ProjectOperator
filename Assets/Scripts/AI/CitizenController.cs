using UnityEngine;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NeuralWaveBureau.Data;
using NeuralWaveBureau.UI;

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

        [SerializeField]
        private CitizenFootstepSystem _footstepSystem;

        [Header("Ragdoll")]
        [SerializeField]
        private Rigidbody[] _ragdollBodies;

        [SerializeField]
        private Collider[] _ragdollColliders;

        [SerializeField]
        private float _ragdollForce = 500f;

        [SerializeField]
        private Transform _ragdollForcePoint;

        // AI systems
        private WaveEvaluator _evaluator;
        private EmotionStateMachine _stateMachine;
        private AISettings _settings;

        // Evaluation task cancellation
        private CancellationTokenSource _evaluationCts;

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
        public float RemainingGracePeriod => _stateMachine?.RemainingGracePeriod ?? 0f;
        public float GracePeriodMultiplier => _stateMachine?.GracePeriodMultiplier ?? 1f;

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

            // Start evaluation task
            _evaluationCts?.Cancel();
            _evaluationCts?.Dispose();
            _evaluationCts = new CancellationTokenSource();

            EvaluationLoop(_evaluationCts.Token).Forget();

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

            if (_evaluationCts != null)
            {
                _evaluationCts.Cancel();
                _evaluationCts.Dispose();
                _evaluationCts = null;
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
        private async UniTaskVoid EvaluationLoop(CancellationToken token)
        {
            float sampleInterval = _settings.GetSampleInterval();

            try
            {
                while (_isActive && !token.IsCancellationRequested)
                {
                    // Get current obedience level from ObedienceController
                    float obedienceLevel = ObedienceController.Instance != null
                        ? ObedienceController.Instance.CurrentObedience
                        : 50f; // Default to neutral if not available

                    // Evaluate wave matching (kept for potential future use/UI display)
                    float waveScore = _evaluator.Evaluate(_currentSample);

                    // Update dynamic obedience based on performance (ONLY if user has started tweaking)
                    if (ObedienceController.Instance != null && AIManager.Instance != null && AIManager.Instance.HasUserInteracted)
                    {
                        ObedienceController.Instance.UpdateDynamicObedience(waveScore, sampleInterval);
                        obedienceLevel = ObedienceController.Instance.CurrentObedience;
                    }

                    // Update state machine with obedience as the evaluation metric
                    _stateMachine.Update(waveScore, obedienceLevel, sampleInterval);

                    // Update animator
                    //UpdateAnimator();

                    // Wait for next sample
                    await UniTask.Delay(System.TimeSpan.FromSeconds(sampleInterval), cancellationToken: token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Task cancelled
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
                    ActivateRagdoll();
                    OnCriticalFailure?.Invoke(this);
                    break;
                case CitizenState.Recovering:
                    OnRecovered?.Invoke(this);
                    break;
            }
        }

        /// <summary>
        /// Test ragdoll activation from context menu
        /// </summary>
        [ContextMenu("Test Ragdoll")]
        public void TestRagdoll()
        {
            ActivateRagdoll();
        }

        /// <summary>
        /// Test ragdoll deactivation from context menu
        /// </summary>
        [ContextMenu("Reset Ragdoll")]
        public void ResetRagdoll()
        {
            DeactivateRagdoll();
        }

        /// <summary>
        /// Activates ragdoll physics for citizen death
        /// </summary>
        private void ActivateRagdoll()
        {
            // Check if ragdoll is set up
            if (_ragdollBodies == null || _ragdollBodies.Length == 0)
            {
                if (_settings != null && _settings.enableVerboseLogging)
                {
                    Debug.LogWarning($"[CitizenController] {_citizenId}: No ragdoll bodies assigned. Skipping ragdoll activation.");
                }
                return;
            }

            // Disable animator to allow physics to take over
            if (_animator != null)
            {
                _animator.enabled = false;
            }

            // Enable all ragdoll rigidbodies
            int activeRagdollCount = 0;
            foreach (var rb in _ragdollBodies)
            {
                if (rb != null)
                {
                    rb.isKinematic = false;
                    activeRagdollCount++;

                    // Apply death force
                    if (_ragdollForcePoint != null)
                    {
                        Vector3 forceDirection = (rb.position - _ragdollForcePoint.position).normalized;
                        rb.AddForce(forceDirection * _ragdollForce, ForceMode.Impulse);
                    }
                    else
                    {
                        // Default: apply upward and backward force
                        rb.AddForce(Vector3.up * _ragdollForce * 0.5f + Vector3.back * _ragdollForce * 0.3f, ForceMode.Impulse);
                    }
                }
            }

            // Enable all ragdoll colliders
            if (_ragdollColliders != null)
            {
                foreach (var col in _ragdollColliders)
                {
                    if (col != null)
                    {
                        col.enabled = true;
                    }
                }
            }

            if (_settings != null && _settings.enableVerboseLogging)
            {
                Debug.Log($"[CitizenController] {_citizenId}: Ragdoll activated ({activeRagdollCount} rigidbodies)");
            }

            // Trigger game over panel after a short delay
            TriggerGameOver().Forget();
        }

        /// <summary>
        /// Triggers the game over screen after a delay
        /// </summary>
        private async UniTaskVoid TriggerGameOver()
        {
            // Wait a bit to let the player see the ragdoll effect
            await UniTask.Delay(System.TimeSpan.FromSeconds(2f));

            // Trigger game over via GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
                Debug.Log($"[CitizenController] {_citizenId}: Game Over triggered!");
            }
            else
            {
                Debug.LogWarning("[CitizenController] GameManager instance not found in scene!");
            }
        }

        /// <summary>
        /// Deactivates ragdoll and returns to animated state
        /// </summary>
        private void DeactivateRagdoll()
        {
            // Check if ragdoll is set up
            if (_ragdollBodies == null || _ragdollBodies.Length == 0)
            {
                // Just re-enable animator if no ragdoll
                if (_animator != null)
                {
                    _animator.enabled = true;
                }
                return;
            }

            // Disable ragdoll rigidbodies
            foreach (var rb in _ragdollBodies)
            {
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
            }

            // Disable ragdoll colliders
            if (_ragdollColliders != null)
            {
                foreach (var col in _ragdollColliders)
                {
                    if (col != null)
                    {
                        col.enabled = false;
                    }
                }
            }

            // Re-enable animator
            if (_animator != null)
            {
                _animator.enabled = true;
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
            DeactivateRagdoll();
        }

        /// <summary>
        /// Gets per-band scores for debugging
        /// </summary>
        public float[] GetBandScores()
        {
            return _evaluator?.GetBandScores(_currentSample) ?? new float[NeuralProfile.BandCount];
        }

        /// <summary>
        /// Sets the obedience-based instability rate multiplier
        /// </summary>
        public void SetObedienceMultiplier(float multiplier)
        {
            _stateMachine?.SetObedienceMultiplier(multiplier);
        }

        private void OnDestroy()
        {
            if (_evaluationCts != null)
            {
                _evaluationCts.Cancel();
                _evaluationCts.Dispose();
            }

            // Unsubscribe from events
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged -= HandleStateChanged;
            }
        }
    }
}

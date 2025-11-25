using UnityEngine;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Handles footstep sound playback for citizens.
    /// Can be triggered by animation events or called programmatically.
    /// </summary>
    public class CitizenFootstepSystem : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField]
        [Tooltip("Audio source for footstep sounds (uses CitizenController's if not specified)")]
        private AudioSource _footstepAudioSource;

        [SerializeField]
        [Tooltip("Array of footstep sound clips to randomly choose from")]
        private AudioClip[] _footstepClips;

        [Header("Variation Settings")]
        [SerializeField]
        [Range(0.8f, 1.2f)]
        [Tooltip("Minimum pitch variation for footsteps")]
        private float _minPitch = 0.9f;

        [SerializeField]
        [Range(0.8f, 1.2f)]
        [Tooltip("Maximum pitch variation for footsteps")]
        private float _maxPitch = 1.1f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Minimum volume for footsteps")]
        private float _minVolume = 0.3f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Maximum volume for footsteps")]
        private float _maxVolume = 0.6f;

        [Header("Auto-Play Settings")]
        [SerializeField]
        [Tooltip("Enable automatic footstep playback based on movement")]
        private bool _autoPlayEnabled = false;

        [SerializeField]
        [Tooltip("Minimum distance citizen must move before playing next footstep")]
        private float _stepDistance = 0.5f;

        [SerializeField]
        [Tooltip("Minimum time between footsteps (prevents too-rapid playback)")]
        private float _minStepInterval = 0.2f;

        // State tracking for auto-play
        private Vector3 _lastStepPosition;
        private float _distanceTraveled = 0f;
        private float _timeSinceLastStep = 0f;
        private bool _isMoving = false;

        private void Awake()
        {
            // If no audio source assigned, try to get one from parent CitizenController
            if (_footstepAudioSource == null)
            {
                _footstepAudioSource = GetComponent<AudioSource>();
            }

            _lastStepPosition = transform.position;
        }

        private void Update()
        {
            if (_autoPlayEnabled)
            {
                UpdateAutoPlay();
            }
        }

        /// <summary>
        /// Updates automatic footstep playback based on movement
        /// </summary>
        private void UpdateAutoPlay()
        {
            _timeSinceLastStep += Time.deltaTime;

            // Calculate distance traveled since last step
            float frameDistance = Vector3.Distance(transform.position, _lastStepPosition);
            _distanceTraveled += frameDistance;
            _lastStepPosition = transform.position;

            // Check if citizen is moving
            _isMoving = frameDistance > 0.001f;

            // Play footstep if moved enough distance and enough time has passed
            if (_isMoving && _distanceTraveled >= _stepDistance && _timeSinceLastStep >= _minStepInterval)
            {
                PlayFootstep();
                _distanceTraveled = 0f;
                _timeSinceLastStep = 0f;
            }
        }

        /// <summary>
        /// Plays a footstep sound with random variation.
        /// Can be called from animation events: void OnFootstep()
        /// </summary>
        public void PlayFootstep()
        {
            if (_footstepAudioSource == null)
            {
                Debug.LogWarning($"[CitizenFootstepSystem] {gameObject.name}: No audio source assigned!");
                return;
            }

            if (_footstepClips == null || _footstepClips.Length == 0)
            {
                Debug.LogWarning($"[CitizenFootstepSystem] {gameObject.name}: No footstep clips assigned!");
                return;
            }

            // Select random clip
            AudioClip clip = _footstepClips[Random.Range(0, _footstepClips.Length)];

            if (clip == null)
            {
                return;
            }

            // Apply random pitch and volume variation
            float originalPitch = _footstepAudioSource.pitch;
            float originalVolume = _footstepAudioSource.volume;

            _footstepAudioSource.pitch = Random.Range(_minPitch, _maxPitch);
            _footstepAudioSource.volume = Random.Range(_minVolume, _maxVolume);

            // Play the sound
            _footstepAudioSource.PlayOneShot(clip);

            // Reset pitch and volume for other sounds
            _footstepAudioSource.pitch = originalPitch;
            _footstepAudioSource.volume = originalVolume;
        }

        /// <summary>
        /// Animation event callback for left foot step
        /// </summary>
        public void OnLeftFootstep()
        {
            PlayFootstep();
        }

        /// <summary>
        /// Animation event callback for right foot step
        /// </summary>
        public void OnRightFootstep()
        {
            PlayFootstep();
        }

        /// <summary>
        /// Sets the footstep sound clips at runtime
        /// </summary>
        public void SetFootstepClips(AudioClip[] clips)
        {
            _footstepClips = clips;
        }

        /// <summary>
        /// Enables or disables automatic footstep playback
        /// </summary>
        public void SetAutoPlayEnabled(bool enabled)
        {
            _autoPlayEnabled = enabled;
            if (enabled)
            {
                _lastStepPosition = transform.position;
                _distanceTraveled = 0f;
                _timeSinceLastStep = 0f;
            }
        }

        /// <summary>
        /// Sets the audio source for footsteps
        /// </summary>
        public void SetAudioSource(AudioSource source)
        {
            _footstepAudioSource = source;
        }
    }
}

using UnityEngine;

namespace NeuralWaveBureau.Data
{
    /// <summary>
    /// ScriptableObject defining a citizen's neural wave targets and behavior parameters.
    /// Used to determine how a citizen reacts to player's wave manipulation.
    /// </summary>
    [CreateAssetMenu(fileName = "NeuralProfile", menuName = "Neural Wave Bureau/Neural Profile", order = 1)]
    public class NeuralProfile : ScriptableObject
    {
        public GameObject prefab;

        [Header("Identity")]
        [Tooltip("Unique identifier for this profile")]
        public string profileId = "default";

        [Tooltip("Display name shown to the player")]
        public string displayName = "Citizen";

        [Header("Neural Band Targets")]
        [Tooltip("Target values for each brain wave band (Delta, Theta, Alpha, Beta, Gamma). Normalized 0..1")]
        [SerializeField]
        private float[] _bandTargets = new float[5] { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f };

        [Tooltip("Allowed variance for each band to be considered successful. Same length as bandTargets")]
        [SerializeField]
        private float[] _bandTolerance = new float[5] { 0.15f, 0.15f, 0.15f, 0.15f, 0.15f };

        [Tooltip("Optional weights for each band in evaluation. If empty, all bands weighted equally")]
        [SerializeField]
        private float[] _bandWeights = new float[5] { 1f, 1f, 1f, 1f, 1f };

        [Header("Stability Settings")]
        [Tooltip("DEPRECATED - No longer used. Citizens now start at 0 instability.")]
        [Range(0f, 1f)]
        public float baselineInstability = 0f;

        [Tooltip("How quickly instability builds when obedience is low")]
        [Range(0f, 2f)]
        public float instabilityRate = 0.5f;

        [Header("Behavior")]
        [Tooltip("Minimum time (seconds) the citizen must be stimulated before stabilization")]
        [Range(0f, 10f)]
        public float minStimulationTime = 2f;

        [Tooltip("Time (seconds) before agitated citizen can recover")]
        [Range(0f, 20f)]
        public float recoveryTime = 5f;

        [Header("Obedience")]
        [Tooltip("Starting obedience level when this citizen arrives at the station (0-100)")]
        [Range(0f, 100f)]
        public float startingObedience = 50f;

        [Header("Audio")]
        [Tooltip("Optional audio clips played during citizen reactions")]
        public AudioClip[] reactionClips;

        // Public accessors
        public float[] BandTargets => _bandTargets;
        public float[] BandTolerance => _bandTolerance;
        public float[] BandWeights => _bandWeights;

        /// <summary>
        /// Band names for debugging and UI display
        /// </summary>
        public static readonly string[] BandNames = { "Delta", "Theta", "Alpha", "Beta", "Gamma" };

        /// <summary>
        /// Number of brain wave bands
        /// </summary>
        public const int BandCount = 5;

        private void OnValidate()
        {
            // Ensure arrays are correct length
            if (_bandTargets.Length != BandCount)
            {
                System.Array.Resize(ref _bandTargets, BandCount);
            }

            if (_bandTolerance.Length != BandCount)
            {
                System.Array.Resize(ref _bandTolerance, BandCount);
            }

            if (_bandWeights.Length != BandCount)
            {
                System.Array.Resize(ref _bandWeights, BandCount);
            }

            // Clamp values to valid ranges
            for (int i = 0; i < BandCount; i++)
            {
                _bandTargets[i] = Mathf.Clamp01(_bandTargets[i]);
                _bandTolerance[i] = Mathf.Clamp(_bandTolerance[i], 0.01f, 1f);
                _bandWeights[i] = Mathf.Max(0f, _bandWeights[i]);
            }
        }
    }
}

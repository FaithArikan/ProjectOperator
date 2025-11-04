using UnityEngine;

namespace NeuralWaveBureau.Data
{
    /// <summary>
    /// Global settings for the AI system. Controls thresholds, evaluation parameters, and debug options.
    /// </summary>
    [CreateAssetMenu(fileName = "AISettings", menuName = "Neural Wave Bureau/AI Settings", order = 0)]
    public class AISettings : ScriptableObject
    {
        [Header("Wave Evaluation")]
        [Tooltip("Similarity score threshold for successful wave match (0..1)")]
        [Range(0f, 1f)]
        public float successThreshold = 0.75f;

        [Tooltip("Score below this triggers instability buildup (0..1)")]
        [Range(0f, 1f)]
        public float overloadThreshold = 0.25f;

        [Tooltip("Instability level that triggers critical failure (0..1)")]
        [Range(0f, 1f)]
        public float instabilityFailThreshold = 0.8f;

        [Header("Sampling & Smoothing")]
        [Tooltip("How often to sample and evaluate wave data (Hz)")]
        [Range(10f, 60f)]
        public float sampleRate = 30f;

        [Tooltip("Time constant for exponential moving average smoothing (seconds)")]
        [Range(0.05f, 2f)]
        public float smoothingTau = 0.3f;

        [Tooltip("Recovery rate for instability when wave is correct (per second)")]
        [Range(0f, 1f)]
        public float instabilityRecoveryRate = 0.2f;

        [Header("Debug & Telemetry")]
        [Tooltip("Enable verbose logging for AI systems")]
        public bool enableVerboseLogging = false;

        [Tooltip("Show wave visualizer in game view")]
        public bool showWaveVisualizer = true;

        [Tooltip("Show instability meter in game view")]
        public bool showInstabilityMeter = true;

        [Header("Performance")]
        [Tooltip("Maximum number of active citizens that can be evaluated simultaneously")]
        [Range(1, 10)]
        public int maxActiveCitizens = 2;

        /// <summary>
        /// Gets the computed smoothing alpha from tau and sample rate
        /// </summary>
        public float GetSmoothingAlpha()
        {
            float dt = 1f / sampleRate;
            return 1f - Mathf.Exp(-dt / smoothingTau);
        }

        /// <summary>
        /// Gets the sample interval in seconds
        /// </summary>
        public float GetSampleInterval()
        {
            return 1f / sampleRate;
        }

        private void OnValidate()
        {
            // Ensure overload threshold is less than success threshold
            if (overloadThreshold >= successThreshold)
            {
                overloadThreshold = successThreshold * 0.5f;
            }
        }
    }
}

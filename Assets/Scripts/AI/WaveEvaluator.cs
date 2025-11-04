using UnityEngine;
using NeuralWaveBureau.Data;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Runtime data structure representing a wave sample with band energies
    /// </summary>
    [System.Serializable]
    public struct WaveSample
    {
        // Timestamp of this sample
        public float timestamp;

        // Band energy values (delta, theta, alpha, beta, gamma) normalized 0..1
        public float[] bandValues;

        public WaveSample(float timestamp, float[] bandValues)
        {
            this.timestamp = timestamp;
            this.bandValues = bandValues ?? new float[NeuralProfile.BandCount];
        }

        /// <summary>
        /// Creates a wave sample with all values set to zero
        /// </summary>
        public static WaveSample Zero()
        {
            return new WaveSample(0f, new float[NeuralProfile.BandCount]);
        }

        /// <summary>
        /// Validates and clamps band values to valid range
        /// </summary>
        public void Validate()
        {
            if (bandValues == null || bandValues.Length != NeuralProfile.BandCount)
            {
                bandValues = new float[NeuralProfile.BandCount];
            }

            for (int i = 0; i < bandValues.Length; i++)
            {
                // Handle NaN/Infinity by clamping to 0
                if (float.IsNaN(bandValues[i]) || float.IsInfinity(bandValues[i]))
                {
                    bandValues[i] = 0f;
                }
                else
                {
                    bandValues[i] = Mathf.Clamp01(bandValues[i]);
                }
            }
        }
    }

    /// <summary>
    /// Evaluates similarity between player's wave output and a target neural profile.
    /// Uses weighted band comparison with exponential moving average smoothing.
    /// </summary>
    public class WaveEvaluator
    {
        // Reference to the target profile
        private NeuralProfile _targetProfile;

        // Current smoothed evaluation score (0..1)
        private float _smoothedScore;

        // Last computed raw score before smoothing
        private float _rawScore;

        // Settings reference
        private AISettings _settings;

        public float SmoothedScore => _smoothedScore;
        public float RawScore => _rawScore;

        public WaveEvaluator(NeuralProfile profile, AISettings settings)
        {
            _targetProfile = profile;
            _settings = settings;
            _smoothedScore = 0f;
            _rawScore = 0f;
        }

        /// <summary>
        /// Evaluates a wave sample against the target profile and updates smoothed score
        /// </summary>
        /// <param name="sample">Wave sample to evaluate</param>
        /// <returns>Current smoothed score (0..1)</returns>
        public float Evaluate(WaveSample sample)
        {
            if (_targetProfile == null || sample.bandValues == null)
            {
                _rawScore = 0f;
                return _smoothedScore;
            }

            // Validate sample data
            sample.Validate();

            // Compute raw similarity score
            _rawScore = ComputeSimilarity(sample);

            // Apply exponential moving average smoothing
            float alpha = _settings.GetSmoothingAlpha();
            _smoothedScore = alpha * _rawScore + (1f - alpha) * _smoothedScore;

            return _smoothedScore;
        }

        /// <summary>
        /// Computes similarity between sample and target profile using weighted band differences
        /// </summary>
        private float ComputeSimilarity(WaveSample sample)
        {
            float[] targets = _targetProfile.BandTargets;
            float[] tolerances = _targetProfile.BandTolerance;
            float[] weights = _targetProfile.BandWeights;

            float weightedSum = 0f;
            float totalWeight = 0f;

            for (int i = 0; i < NeuralProfile.BandCount; i++)
            {
                // Compute normalized difference for this band
                // d[i] = 1 - abs(sample[i] - target[i]) / tolerance[i], clamped to 0..1
                float difference = Mathf.Abs(sample.bandValues[i] - targets[i]);
                float normalizedDiff = 1f - Mathf.Clamp01(difference / tolerances[i]);

                // Apply weight for this band
                weightedSum += weights[i] * normalizedDiff;
                totalWeight += weights[i];
            }

            // Compute weighted average
            if (totalWeight > 0f)
            {
                return weightedSum / totalWeight;
            }

            return 0f;
        }

        /// <summary>
        /// Resets the smoothed score (useful when starting new evaluation)
        /// </summary>
        public void Reset()
        {
            _smoothedScore = 0f;
            _rawScore = 0f;
        }

        /// <summary>
        /// Gets per-band similarity scores for debugging/visualization
        /// </summary>
        public float[] GetBandScores(WaveSample sample)
        {
            sample.Validate();

            float[] targets = _targetProfile.BandTargets;
            float[] tolerances = _targetProfile.BandTolerance;
            float[] scores = new float[NeuralProfile.BandCount];

            for (int i = 0; i < NeuralProfile.BandCount; i++)
            {
                float difference = Mathf.Abs(sample.bandValues[i] - targets[i]);
                scores[i] = 1f - Mathf.Clamp01(difference / tolerances[i]);
            }

            return scores;
        }

        /// <summary>
        /// Checks if current score indicates successful wave match
        /// </summary>
        public bool IsSuccessful()
        {
            return _smoothedScore >= _settings.successThreshold;
        }

        /// <summary>
        /// Checks if current score indicates overload/incorrect wave
        /// </summary>
        public bool IsOverloaded()
        {
            return _smoothedScore <= _settings.overloadThreshold;
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Manages historical wave data for waveform visualization.
    /// Maintains a circular buffer of wave samples for each brain band.
    /// </summary>
    public class DataBufferManager
    {
        private const int DEFAULT_BUFFER_SIZE = 120; // 2 seconds at 60 FPS

        // Circular buffers for each band
        private Queue<float>[] _bandBuffers;
        private int _bufferSize;

        // Statistics for each band
        private float[] _minValues;
        private float[] _maxValues;
        private float[] _avgValues;

        // Cached arrays to avoid per-frame allocations
        private float[][] _cachedBandData;
        private float[] _lastValues;
        private bool[] _dataDirty;

        public int BufferSize => _bufferSize;
        public int BandCount => _bandBuffers.Length;

        public DataBufferManager(int bandCount = 5, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            _bufferSize = bufferSize;
            _bandBuffers = new Queue<float>[bandCount];
            _minValues = new float[bandCount];
            _maxValues = new float[bandCount];
            _avgValues = new float[bandCount];

            // Initialize cached arrays
            _cachedBandData = new float[bandCount][];
            _lastValues = new float[bandCount];
            _dataDirty = new bool[bandCount];

            // Initialize buffers
            for (int i = 0; i < bandCount; i++)
            {
                _bandBuffers[i] = new Queue<float>(_bufferSize);
                _cachedBandData[i] = new float[_bufferSize];
                _lastValues[i] = 0f;
                _dataDirty[i] = true;
                _minValues[i] = 0f;
                _maxValues[i] = 1f;
                _avgValues[i] = 0.5f;
            }
        }

        /// <summary>
        /// Adds a new wave sample to all band buffers
        /// </summary>
        public void AddSample(float[] bandValues)
        {
            if (bandValues == null || bandValues.Length != _bandBuffers.Length)
                return;

            for (int i = 0; i < _bandBuffers.Length; i++)
            {
                // Add to buffer
                _bandBuffers[i].Enqueue(bandValues[i]);

                // Update last value cache
                _lastValues[i] = bandValues[i];

                // Remove oldest if buffer is full
                if (_bandBuffers[i].Count > _bufferSize)
                {
                    _bandBuffers[i].Dequeue();
                }

                // Mark data as dirty so it will be refreshed on next access
                _dataDirty[i] = true;

                // Update statistics
                UpdateStatistics(i);
            }
        }

        /// <summary>
        /// Gets all samples for a specific band
        /// </summary>
        public float[] GetBandData(int bandIndex)
        {
            if (bandIndex < 0 || bandIndex >= _bandBuffers.Length)
                return System.Array.Empty<float>(); // Use cached empty array instead of new allocation

            // Only refresh cached data if it's dirty (data has changed)
            if (_dataDirty[bandIndex])
            {
                // Copy Queue data to cached array - this still allocates via ToArray()
                // but we can optimize further by manually copying
                int count = _bandBuffers[bandIndex].Count;

                // Manually copy to avoid ToArray allocation
                int index = 0;
                foreach (float value in _bandBuffers[bandIndex])
                {
                    _cachedBandData[bandIndex][index++] = value;
                }

                // If buffer isn't full, pad with zeros
                for (int i = count; i < _bufferSize; i++)
                {
                    _cachedBandData[bandIndex][i] = 0f;
                }

                _dataDirty[bandIndex] = false;
            }

            return _cachedBandData[bandIndex];
        }

        /// <summary>
        /// Gets the most recent sample for a specific band
        /// </summary>
        public float GetLatestValue(int bandIndex)
        {
            if (bandIndex < 0 || bandIndex >= _bandBuffers.Length)
                return 0f;

            // Return cached last value - no allocation needed
            return _lastValues[bandIndex];
        }

        /// <summary>
        /// Gets all latest values for all bands
        /// </summary>
        public float[] GetLatestValues()
        {
            float[] values = new float[_bandBuffers.Length];
            for (int i = 0; i < _bandBuffers.Length; i++)
            {
                values[i] = GetLatestValue(i);
            }
            return values;
        }

        /// <summary>
        /// Clears all buffer data
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _bandBuffers.Length; i++)
            {
                _bandBuffers[i].Clear();
            }
        }

        /// <summary>
        /// Gets the minimum value for a band in the current buffer
        /// </summary>
        public float GetMinValue(int bandIndex)
        {
            return _minValues[bandIndex];
        }

        /// <summary>
        /// Gets the maximum value for a band in the current buffer
        /// </summary>
        public float GetMaxValue(int bandIndex)
        {
            return _maxValues[bandIndex];
        }

        /// <summary>
        /// Gets the average value for a band in the current buffer
        /// </summary>
        public float GetAvgValue(int bandIndex)
        {
            return _avgValues[bandIndex];
        }

        /// <summary>
        /// Updates statistics for a specific band
        /// </summary>
        private void UpdateStatistics(int bandIndex)
        {
            if (_bandBuffers[bandIndex].Count == 0)
                return;

            // Iterate Queue directly instead of converting to array - eliminates allocation
            float min = float.MaxValue;
            float max = float.MinValue;
            float sum = 0f;
            int count = 0;

            foreach (float value in _bandBuffers[bandIndex])
            {
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
                sum += value;
                count++;
            }

            _minValues[bandIndex] = min;
            _maxValues[bandIndex] = max;
            _avgValues[bandIndex] = sum / count;
        }

        /// <summary>
        /// Resizes the buffer (clears existing data)
        /// </summary>
        public void ResizeBuffer(int newSize)
        {
            _bufferSize = newSize;
            Clear();

            // Recreate buffers and cached arrays
            for (int i = 0; i < _bandBuffers.Length; i++)
            {
                _bandBuffers[i] = new Queue<float>(_bufferSize);
                _cachedBandData[i] = new float[_bufferSize];
                _dataDirty[i] = true;
            }
        }

        /// <summary>
        /// Gets the current buffer fill percentage (0..1)
        /// </summary>
        public float GetBufferFillPercentage()
        {
            if (_bandBuffers.Length == 0)
                return 0f;

            return (float)_bandBuffers[0].Count / _bufferSize;
        }
    }
}

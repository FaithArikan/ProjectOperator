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

        public int BufferSize => _bufferSize;
        public int BandCount => _bandBuffers.Length;

        public DataBufferManager(int bandCount = 5, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            _bufferSize = bufferSize;
            _bandBuffers = new Queue<float>[bandCount];
            _minValues = new float[bandCount];
            _maxValues = new float[bandCount];
            _avgValues = new float[bandCount];

            // Initialize buffers
            for (int i = 0; i < bandCount; i++)
            {
                _bandBuffers[i] = new Queue<float>(_bufferSize);
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

                // Remove oldest if buffer is full
                if (_bandBuffers[i].Count > _bufferSize)
                {
                    _bandBuffers[i].Dequeue();
                }

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
                return new float[0];

            return _bandBuffers[bandIndex].ToArray();
        }

        /// <summary>
        /// Gets the most recent sample for a specific band
        /// </summary>
        public float GetLatestValue(int bandIndex)
        {
            if (bandIndex < 0 || bandIndex >= _bandBuffers.Length)
                return 0f;

            if (_bandBuffers[bandIndex].Count == 0)
                return 0f;

            // Peek at the last value
            float[] array = _bandBuffers[bandIndex].ToArray();
            return array[array.Length - 1];
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

            float[] data = _bandBuffers[bandIndex].ToArray();
            float min = float.MaxValue;
            float max = float.MinValue;
            float sum = 0f;

            foreach (float value in data)
            {
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
                sum += value;
            }

            _minValues[bandIndex] = min;
            _maxValues[bandIndex] = max;
            _avgValues[bandIndex] = sum / data.Length;
        }

        /// <summary>
        /// Resizes the buffer (clears existing data)
        /// </summary>
        public void ResizeBuffer(int newSize)
        {
            _bufferSize = newSize;
            Clear();

            // Recreate buffers
            for (int i = 0; i < _bandBuffers.Length; i++)
            {
                _bandBuffers[i] = new Queue<float>(_bufferSize);
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

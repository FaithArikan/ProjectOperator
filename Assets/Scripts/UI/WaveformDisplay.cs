using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Renders animated waveform line graphs for brain wave data.
    /// Uses UILineRenderer or Unity UI Image components to draw scrolling waveforms.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class WaveformDisplay : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField]
        private int _bandIndex = 0; // Which band to display (0-4)

        [SerializeField]
        private Color _waveColor = Color.green;

        [SerializeField]
        private Color _targetColor = Color.yellow;

        [SerializeField]
        [Range(1f, 5f)]
        private float _lineThickness = 2f;

        [SerializeField]
        private bool _showTargetLine = true;

        [SerializeField]
        private bool _showToleranceZone = true;

        [SerializeField]
        private Color _toleranceColor = new Color(1f, 1f, 0f, 0.2f);

        [Header("Animation")]
        [SerializeField]
        private float _scrollSpeed = 1f;

        [SerializeField]
        private bool _smoothTransitions = true;

        [Header("Wave Settings")]
        [SerializeField]
        [Range(0.5f, 4f)]
        private float _amplitudeMultiplier = 1f; // Makes waves have more dramatic ups and downs

        // Components
        private RectTransform _rectTransform;
        private Texture2D _waveTexture;
        private RawImage _displayImage;

        // Data
        private DataBufferManager _bufferManager;
        private float _targetValue;
        private float _toleranceValue;

        // Internal state
        private int _textureWidth = 512;
        private int _textureHeight = 256;
        private bool _isDirty = true;
        private bool _hasStarted = false; // Controls when waveform starts rendering

        // Performance optimization - cached arrays to avoid per-frame allocations
        private Color32[] _clearColorCache;
        private Color32[] _pixelBuffer;

        public int BandIndex { get => _bandIndex; set => _bandIndex = value; }
        public Color WaveColor { get => _waveColor; set => _waveColor = value; }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            // Create or get RawImage component
            _displayImage = GetComponent<RawImage>();
            if (_displayImage == null)
            {
                _displayImage = gameObject.AddComponent<RawImage>();
            }

            // Create texture for waveform
            CreateWaveTexture();

            // Start with alpha 0 - will fade in when AnimateIn is called
            if (_displayImage != null)
            {
                var color = _displayImage.color;
                color.a = 0f;
                _displayImage.color = color;
            }
        }

        /// <summary>
        /// Initializes the waveform display with a data buffer
        /// </summary>
        public void Initialize(DataBufferManager bufferManager, int bandIndex)
        {
            _bufferManager = bufferManager;
            _bandIndex = bandIndex;
            _isDirty = true;
            Debug.Log($"[WaveformDisplay] Initialized - Band {_bandIndex}, Color: {_waveColor}");
        }

        /// <summary>
        /// Sets the target value and tolerance for this band
        /// </summary>
        public void SetTarget(float target, float tolerance)
        {
            _targetValue = target;
            _toleranceValue = tolerance;
            _isDirty = true;
        }

        /// <summary>
        /// Creates the texture used for rendering waveforms
        /// </summary>
        private void CreateWaveTexture()
        {
            _waveTexture = new Texture2D(_textureWidth, _textureHeight, TextureFormat.RGBA32, false);
            _waveTexture.filterMode = FilterMode.Bilinear;
            _waveTexture.wrapMode = TextureWrapMode.Clamp;

            // Initialize cached arrays for performance (avoid per-frame allocations)
            int pixelCount = _textureWidth * _textureHeight;
            _clearColorCache = new Color32[pixelCount];
            _pixelBuffer = new Color32[pixelCount];

            // Pre-fill clear cache with transparent black
            Color32 transparent = new Color32(0, 0, 0, 0);
            for (int i = 0; i < pixelCount; i++)
            {
                _clearColorCache[i] = transparent;
            }

            // Clear texture
            ClearTexture();

            // Assign to RawImage
            _displayImage.texture = _waveTexture;
        }

        /// <summary>
        /// Clears the texture to transparent black
        /// </summary>
        private void ClearTexture()
        {
            // Use cached clear array instead of allocating new array every frame
            // This eliminates ~2MB allocation per frame
            _waveTexture.SetPixels32(_clearColorCache);
            _waveTexture.Apply();
        }

        /// <summary>
        /// Updates the waveform display
        /// </summary>
        private void Update()
        {
            // Only draw waveform if monitoring has started
            if (_hasStarted)
            {
                DrawWaveform();
            }
        }

        /// <summary>
        /// Draws the waveform on the texture
        /// </summary>
        private void DrawWaveform()
        {
            // Copy clear cache to pixel buffer (fast array copy)
            System.Array.Copy(_clearColorCache, _pixelBuffer, _pixelBuffer.Length);

            // Draw tolerance zone if enabled
            if (_showToleranceZone)
            {
                DrawToleranceZone();
            }

            // Draw target line if enabled
            if (_showTargetLine)
            {
                DrawTargetLine();
            }

            // Draw waveform data
            DrawWaveData();

            // Apply all changes at once using batched SetPixels32 (faster than SetPixels)
            _waveTexture.SetPixels32(_pixelBuffer);
            _waveTexture.Apply();
        }

        /// <summary>
        /// Draws the tolerance zone as a filled rectangle
        /// </summary>
        private void DrawToleranceZone()
        {
            // Anchor tolerance zone from the top of the texture
            float normalizedTarget = _targetValue * _amplitudeMultiplier;
            float normalizedTolerance = _toleranceValue * _amplitudeMultiplier;

            int maxY = Mathf.Clamp((int)((normalizedTarget + normalizedTolerance) * (_textureHeight - 1)), 0, _textureHeight - 1);
            int minY = Mathf.Clamp((int)((normalizedTarget - normalizedTolerance) * (_textureHeight - 1)), 0, _textureHeight - 1);

            Color32 toleranceColor32 = _toleranceColor;

            for (int x = 0; x < _textureWidth; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    int index = y * _textureWidth + x;
                    _pixelBuffer[index] = toleranceColor32;
                }
            }
        }

        /// <summary>
        /// Draws the target line
        /// </summary>
        private void DrawTargetLine()
        {
            // Anchor target line from the top of the texture
            float normalizedTarget = _targetValue * _amplitudeMultiplier;
            int targetY = Mathf.Clamp((int)(normalizedTarget * (_textureHeight - 1)), 0, _textureHeight - 1);

            Color32 targetColor32 = _targetColor;

            for (int x = 0; x < _textureWidth; x++)
            {
                // Draw line with thickness
                for (int t = -(int)_lineThickness / 2; t <= (int)_lineThickness / 2; t++)
                {
                    int y = targetY + t;
                    if (y >= 0 && y < _textureHeight)
                    {
                        int index = y * _textureWidth + x;
                        _pixelBuffer[index] = targetColor32;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the actual wave data
        /// </summary>
        private void DrawWaveData()
        {
            float[] data = _bufferManager.GetBandData(_bandIndex);
            if (data.Length < 2)
            {
                return;
            }

            // Convert to Color32 once instead of implicit conversion each call
            Color32 waveColor32 = _waveColor;

            // Calculate x step
            float xStep = (float)_textureWidth / data.Length;

            // Draw lines between points
            for (int i = 0; i < data.Length - 1; i++)
            {
                int x1 = (int)(i * xStep);
                // Anchor from top and apply amplitude multiplier for more dramatic ups and downs
                float normalizedY1 = data[i] * _amplitudeMultiplier;
                int y1 = Mathf.Clamp((int)(normalizedY1 * (_textureHeight - 1)), 0, _textureHeight - 1);

                int x2 = (int)((i + 1) * xStep);
                float normalizedY2 = data[i + 1] * _amplitudeMultiplier;
                int y2 = Mathf.Clamp((int)(normalizedY2 * (_textureHeight - 1)), 0, _textureHeight - 1);

                DrawLine(x1, y1, x2, y2, waveColor32);
            }
        }

        /// <summary>
        /// Draws a line between two points using Bresenham's algorithm
        /// </summary>
        private void DrawLine(int x0, int y0, int x1, int y1, Color32 color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                // Draw pixel with thickness using pixel buffer
                for (int tx = -(int)_lineThickness / 2; tx <= (int)_lineThickness / 2; tx++)
                {
                    for (int ty = -(int)_lineThickness / 2; ty <= (int)_lineThickness / 2; ty++)
                    {
                        int px = x0 + tx;
                        int py = y0 + ty;
                        if (px >= 0 && px < _textureWidth && py >= 0 && py < _textureHeight)
                        {
                            int index = py * _textureWidth + px;
                            _pixelBuffer[index] = color;
                        }
                    }
                }

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        /// <summary>
        /// Animates the waveform appearance
        /// </summary>
        public void AnimateIn(float duration = 0.5f)
        {
            _hasStarted = true; // Start rendering waveform
            _displayImage.DOFade(1f, duration).SetEase(Ease.OutCubic);
        }

        /// <summary>
        /// Animates the waveform disappearance
        /// </summary>
        public void AnimateOut(float duration = 0.3f, System.Action onComplete = null)
        {
            _hasStarted = false; // Stop rendering waveform
            _displayImage.DOFade(0f, duration).SetEase(Ease.InCubic).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// Pulses the waveform (for alerts)
        /// </summary>
        public void Pulse(float intensity = 1.2f, float duration = 0.3f)
        {
            transform.DOScale(Vector3.one * intensity, duration * 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    transform.DOScale(Vector3.one, duration * 0.5f).SetEase(Ease.InQuad);
                });
        }

        private void OnDestroy()
        {
            if (_waveTexture != null)
            {
                Destroy(_waveTexture);
            }
        }
    }
}

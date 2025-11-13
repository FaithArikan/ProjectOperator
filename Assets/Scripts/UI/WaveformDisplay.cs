using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using NeuralWaveBureau.Data;

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
            Color[] clearColors = new Color[_textureWidth * _textureHeight];
            for (int i = 0; i < clearColors.Length; i++)
            {
                clearColors[i] = new Color(0, 0, 0, 0);
            }
            _waveTexture.SetPixels(clearColors);
            _waveTexture.Apply();
        }

        /// <summary>
        /// Updates the waveform display
        /// </summary>
        private void Update()
        {
            if (_bufferManager == null)
            {
                // Log once per second that buffer is missing
                if (Time.frameCount % 60 == 0)
                {
                    Debug.LogWarning($"[WaveformDisplay] Band {_bandIndex}: No buffer manager assigned! Call Initialize() first.");
                }
                return;
            }

            // Redraw waveform
            DrawWaveform();
        }

        /// <summary>
        /// Draws the waveform on the texture
        /// </summary>
        private void DrawWaveform()
        {
            // Clear texture
            ClearTexture();

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

            // Apply changes
            _waveTexture.Apply();
        }

        /// <summary>
        /// Draws the tolerance zone as a filled rectangle
        /// </summary>
        private void DrawToleranceZone()
        {
            int minY = Mathf.Clamp((int)((_targetValue - _toleranceValue) * _textureHeight), 0, _textureHeight - 1);
            int maxY = Mathf.Clamp((int)((_targetValue + _toleranceValue) * _textureHeight), 0, _textureHeight - 1);

            for (int x = 0; x < _textureWidth; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    _waveTexture.SetPixel(x, y, _toleranceColor);
                }
            }
        }

        /// <summary>
        /// Draws the target line
        /// </summary>
        private void DrawTargetLine()
        {
            int targetY = Mathf.Clamp((int)(_targetValue * _textureHeight), 0, _textureHeight - 1);

            for (int x = 0; x < _textureWidth; x++)
            {
                // Draw line with thickness
                for (int t = -(int)_lineThickness / 2; t <= (int)_lineThickness / 2; t++)
                {
                    int y = targetY + t;
                    if (y >= 0 && y < _textureHeight)
                    {
                        _waveTexture.SetPixel(x, y, _targetColor);
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
                Debug.LogWarning($"[WaveformDisplay] Band {_bandIndex}: Buffer has insufficient data (length: {data.Length})");
                return;
            }

            // Debug: Log first few values occasionally
            if (Time.frameCount % 120 == 0) // Every 2 seconds at 60fps
            {
                string preview = $"[WaveformDisplay] Band {_bandIndex} data: [{data[0]:F2}, {data[1]:F2}, {data[2]:F2}...{data[data.Length - 1]:F2}] (length: {data.Length})";
                Debug.Log(preview);
            }

            // Calculate x step
            float xStep = (float)_textureWidth / data.Length;

            // Draw lines between points
            for (int i = 0; i < data.Length - 1; i++)
            {
                int x1 = (int)(i * xStep);
                int y1 = Mathf.Clamp((int)(data[i] * _textureHeight), 0, _textureHeight - 1);
                int x2 = (int)((i + 1) * xStep);
                int y2 = Mathf.Clamp((int)(data[i + 1] * _textureHeight), 0, _textureHeight - 1);

                DrawLine(x1, y1, x2, y2, _waveColor);
            }
        }

        /// <summary>
        /// Draws a line between two points using Bresenham's algorithm
        /// </summary>
        private void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                // Draw pixel with thickness
                for (int tx = -(int)_lineThickness / 2; tx <= (int)_lineThickness / 2; tx++)
                {
                    for (int ty = -(int)_lineThickness / 2; ty <= (int)_lineThickness / 2; ty++)
                    {
                        int px = x0 + tx;
                        int py = y0 + ty;
                        if (px >= 0 && px < _textureWidth && py >= 0 && py < _textureHeight)
                        {
                            _waveTexture.SetPixel(px, py, color);
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
            _displayImage.DOFade(1f, duration).SetEase(Ease.OutCubic);
        }

        /// <summary>
        /// Animates the waveform disappearance
        /// </summary>
        public void AnimateOut(float duration = 0.3f)
        {
            _displayImage.DOFade(0f, duration).SetEase(Ease.InCubic);
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

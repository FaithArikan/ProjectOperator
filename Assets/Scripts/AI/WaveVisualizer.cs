using UnityEngine;
using NeuralWaveBureau.Data;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Debug visualizer for wave data and evaluation scores.
    /// Displays on-screen graphs comparing player wave vs target bands.
    /// </summary>
    public class WaveVisualizer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private bool _showVisualizer = true;

        [SerializeField]
        private Vector2 _position = new Vector2(10, 10);

        [SerializeField]
        private Vector2 _size = new Vector2(400, 300);

        [SerializeField]
        private Color _targetColor = Color.green;

        [SerializeField]
        private Color _currentColor = Color.cyan;

        [SerializeField]
        private Color _scoreColor = Color.yellow;

        private AIManager _aiManager;
        private GUIStyle _labelStyle;
        private GUIStyle _boxStyle;

        private void Start()
        {
            _aiManager = AIManager.Instance;

            // Initialize GUI styles
            _labelStyle = new GUIStyle();
            _labelStyle.normal.textColor = Color.white;
            _labelStyle.fontSize = 12;

            _boxStyle = new GUIStyle();
            _boxStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.7f));
        }

        private void OnGUI()
        {
            if (!_showVisualizer || _aiManager == null || _aiManager.ActiveCitizen == null)
                return;

            CitizenController citizen = _aiManager.ActiveCitizen;
            NeuralProfile profile = citizen.Profile;
            WaveSample currentSample = _aiManager.CurrentWaveSample;

            // Draw background box
            GUI.Box(new Rect(_position.x, _position.y, _size.x, _size.y), "", _boxStyle);

            // Draw title
            GUI.Label(new Rect(_position.x + 10, _position.y + 5, _size.x - 20, 20),
                $"Wave Visualizer - {profile.displayName}", _labelStyle);

            // Draw band comparison
            float barWidth = (_size.x - 40) / NeuralProfile.BandCount;
            float barHeight = _size.y - 120;
            float startX = _position.x + 20;
            float startY = _position.y + 40;

            for (int i = 0; i < NeuralProfile.BandCount; i++)
            {
                float x = startX + i * barWidth;

                // Draw band name
                GUI.Label(new Rect(x, startY + barHeight + 5, barWidth, 20),
                    NeuralProfile.BandNames[i], _labelStyle);

                // Draw target bar
                float targetHeight = profile.BandTargets[i] * barHeight;
                DrawBar(new Rect(x + 5, startY + barHeight - targetHeight, barWidth * 0.4f, targetHeight), _targetColor);

                // Draw current bar
                float currentHeight = currentSample.bandValues[i] * barHeight;
                DrawBar(new Rect(x + barWidth * 0.5f, startY + barHeight - currentHeight, barWidth * 0.4f, currentHeight), _currentColor);

                // Draw tolerance range
                float tolerance = profile.BandTolerance[i];
                float toleranceMin = Mathf.Max(0, profile.BandTargets[i] - tolerance) * barHeight;
                float toleranceMax = Mathf.Min(1, profile.BandTargets[i] + tolerance) * barHeight;
                DrawLine(new Vector2(x, startY + barHeight - toleranceMin),
                         new Vector2(x + barWidth, startY + barHeight - toleranceMin), Color.white);
                DrawLine(new Vector2(x, startY + barHeight - toleranceMax),
                         new Vector2(x + barWidth, startY + barHeight - toleranceMax), Color.white);
            }

            // Draw evaluation score
            float scoreY = _position.y + _size.y - 60;
            GUI.Label(new Rect(_position.x + 10, scoreY, _size.x - 20, 20),
                $"Evaluation Score: {citizen.EvaluationScore:F2} / {_aiManager.Settings.successThreshold:F2}", _labelStyle);

            // Draw instability meter
            float instabilityY = scoreY + 20;
            GUI.Label(new Rect(_position.x + 10, instabilityY, 100, 20), "Instability:", _labelStyle);

            Rect meterRect = new Rect(_position.x + 110, instabilityY + 5, _size.x - 130, 10);
            DrawBar(meterRect, new Color(0.3f, 0.3f, 0.3f));

            float instabilityWidth = citizen.Instability * (meterRect.width - 4);
            Color instabilityColor = Color.Lerp(Color.green, Color.red, citizen.Instability);
            DrawBar(new Rect(meterRect.x + 2, meterRect.y + 2, instabilityWidth, meterRect.height - 4), instabilityColor);

            // Draw state
            float stateY = instabilityY + 20;
            GUI.Label(new Rect(_position.x + 10, stateY, _size.x - 20, 20),
                $"State: {citizen.CurrentState}", _labelStyle);
        }

        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        private void DrawBar(Rect rect, Color color)
        {
            Texture2D texture = MakeTexture(2, 2, color);
            GUI.DrawTexture(rect, texture);
        }

        /// <summary>
        /// Draws a line (simple implementation using GUI.DrawTexture)
        /// </summary>
        private void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            float width = Vector2.Distance(start, end);
            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

            GUIUtility.RotateAroundPivot(angle, start);
            DrawBar(new Rect(start.x, start.y, width, 2), color);
            GUIUtility.RotateAroundPivot(-angle, start);
        }

        /// <summary>
        /// Creates a solid color texture
        /// </summary>
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Toggle visualizer visibility
        /// </summary>
        public void ToggleVisualizer()
        {
            _showVisualizer = !_showVisualizer;
        }
    }
}

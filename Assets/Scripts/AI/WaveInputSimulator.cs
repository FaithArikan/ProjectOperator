using UnityEngine;
using UnityEngine.InputSystem;
using NeuralWaveBureau.UI;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Simulates wave input for testing and development.
    /// Allows keyboard control to test different wave patterns.
    /// </summary>
    public class WaveInputSimulator : MonoBehaviour
    {
        [Header("Test Patterns")]
        [SerializeField]
        private float[] _currentBands = new float[5] { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f };

        [SerializeField]
        [Range(0f, 1f)]
        private float _noiseLevel = 0.1f;

        [Header("Keyboard Control")]
        [Tooltip("Key to increase selected band value")]
        [SerializeField]
        private Key _increaseKey = Key.UpArrow;

        [Tooltip("Key to decrease selected band value")]
        [SerializeField]
        private Key _decreaseKey = Key.DownArrow;

        [SerializeField]
        private int _selectedBand = 0;

        [SerializeField]
        [Range(0.1f, 2f)]
        [Tooltip("How fast values change when holding up/down")]
        private float _adjustmentSpeed = 1.0f;

        [Header("Knob Control")]
        [SerializeField]
        [Tooltip("Radio knobs to control each band value (Delta, Theta, Alpha, Beta, Gamma)")]
        private NeuralWaveBureau.UI.RadioKnobController[] _radioKnobs = new NeuralWaveBureau.UI.RadioKnobController[5];

        private AIManager _aiManager;
        private float _lastAdjustmentTime = 0f;

        private void Awake()
        {
            // Validate and reset invalid key values (can happen when upgrading from old Input System)
            if (!System.Enum.IsDefined(typeof(Key), _increaseKey) || _increaseKey == Key.None)
            {
                _increaseKey = Key.UpArrow;
            }

            if (!System.Enum.IsDefined(typeof(Key), _decreaseKey) || _decreaseKey == Key.None)
            {
                _decreaseKey = Key.DownArrow;
            }
        }

        private void Start()
        {
            _aiManager = AIManager.Instance;

            if (_aiManager == null)
            {
                Debug.LogError("[WaveInputSimulator] AIManager not found!");
            }

            // Subscribe to radio knob value changes for each band
            for (int i = 0; i < _radioKnobs.Length && i < 5; i++)
            {
                if (_radioKnobs[i] != null)
                {
                    int bandIndex = i; // Capture for closure
                    _radioKnobs[i].OnValueChanged += (value) => OnKnobValueChanged(bandIndex, value);
                    // Initialize knob to current band value
                    _radioKnobs[i].SetValue(_currentBands[i], false);
                    Debug.Log($"[WaveInputSimulator] Radio knob {i + 1} connected - controlling Band {i + 1}");
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from radio knobs
            for (int i = 0; i < _radioKnobs.Length; i++)
            {
                if (_radioKnobs[i] != null)
                {
                    _radioKnobs[i].OnValueChanged = null;
                }
            }
        }

        /// <summary>
        /// Called when a radio knob value changes
        /// </summary>
        private void OnKnobValueChanged(int bandIndex, float value)
        {
            _currentBands[bandIndex] = value;
            Debug.Log($"[WaveInputSimulator] Knob adjusted Band {bandIndex + 1} to {value:F2}");
        }

        private void Update()
        {
            if (_aiManager == null)
                return;

            // Keyboard controls for testing
            HandleInput();

            // Add noise to simulate real wave fluctuations
            float[] noisyBands = new float[5];
            for (int i = 0; i < 5; i++)
            {
                float noise = Random.Range(-_noiseLevel, _noiseLevel);
                noisyBands[i] = Mathf.Clamp01(_currentBands[i] + noise);
            }

            // Send to AIManager
            _aiManager.SetWaveSample(noisyBands);
        }

        private void HandleInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            // Select band with number keys
            int previousBand = _selectedBand;
            if (keyboard[Key.Digit1].wasPressedThisFrame) _selectedBand = 0;
            if (keyboard[Key.Digit2].wasPressedThisFrame) _selectedBand = 1;
            if (keyboard[Key.Digit3].wasPressedThisFrame) _selectedBand = 2;
            if (keyboard[Key.Digit4].wasPressedThisFrame) _selectedBand = 3;
            if (keyboard[Key.Digit5].wasPressedThisFrame) _selectedBand = 4;

            // Log band selection change
            if (_selectedBand != previousBand)
            {
                string[] bandNames = { "Delta (Red)", "Theta (Orange)", "Alpha (Green)", "Beta (Blue)", "Gamma (Purple)" };
                Debug.Log($"[WaveInputSimulator] Selected Band {_selectedBand + 1}: {bandNames[_selectedBand]} - Current Value: {_currentBands[_selectedBand]:F2}");
            }

            // Adjust selected band with feedback
            if (keyboard[_increaseKey].isPressed)
            {
                _currentBands[_selectedBand] = Mathf.Clamp01(_currentBands[_selectedBand] + Time.deltaTime * _adjustmentSpeed);

                // Log periodically while adjusting
                if (Time.time - _lastAdjustmentTime > 0.5f)
                {
                    Debug.Log($"[WaveInputSimulator] Band {_selectedBand + 1} INCREASED to {_currentBands[_selectedBand]:F2}");
                    _lastAdjustmentTime = Time.time;
                }
            }

            if (keyboard[_decreaseKey].isPressed)
            {
                _currentBands[_selectedBand] = Mathf.Clamp01(_currentBands[_selectedBand] - Time.deltaTime * _adjustmentSpeed);

                // Log periodically while adjusting
                if (Time.time - _lastAdjustmentTime > 0.5f)
                {
                    Debug.Log($"[WaveInputSimulator] Band {_selectedBand + 1} DECREASED to {_currentBands[_selectedBand]:F2}");
                    _lastAdjustmentTime = Time.time;
                }
            }

            // Quick presets
            if (keyboard[Key.Q].wasPressedThisFrame) SetPreset("Ordinary");
            if (keyboard[Key.W].wasPressedThisFrame) SetPreset("Artist");
            if (keyboard[Key.E].wasPressedThisFrame) SetPreset("Rebel");
            if (keyboard[Key.R].wasPressedThisFrame) SetPreset("Random");
        }

        /// <summary>
        /// Sets a test preset pattern
        /// </summary>
        public void SetPreset(string presetName)
        {
            switch (presetName)
            {
                case "Ordinary":
                    _currentBands = new float[] { 0.1f, 0.2f, 0.6f, 0.6f, 0.2f };
                    break;
                case "Artist":
                    _currentBands = new float[] { 0.05f, 0.4f, 0.7f, 0.5f, 0.3f };
                    break;
                case "Rebel":
                    _currentBands = new float[] { 0.05f, 0.15f, 0.3f, 0.8f, 0.9f };
                    break;
                case "Random":
                    for (int i = 0; i < 5; i++)
                    {
                        _currentBands[i] = Random.Range(0f, 1f);
                    }
                    break;
            }

            string bandsStr = $"[{_currentBands[0]:F2}, {_currentBands[1]:F2}, {_currentBands[2]:F2}, {_currentBands[3]:F2}, {_currentBands[4]:F2}]";
            Debug.Log($"[WaveInputSimulator] Preset loaded: {presetName} - Bands: {bandsStr}");
        }

        private void OnGUI()
        {
            // Define band colors matching WaveformDisplay
            Color[] bandColors = new Color[]
            {
                new Color(1f, 0.2f, 0.2f),      // Delta - Red
                new Color(1f, 0.6f, 0.2f),      // Theta - Orange
                new Color(0.2f, 1f, 0.2f),      // Alpha - Green
                new Color(0.2f, 0.6f, 1f),      // Beta - Blue
                new Color(0.8f, 0.2f, 1f)       // Gamma - Purple
            };

            string[] bandNames = { "Delta", "Theta", "Alpha", "Beta", "Gamma" };

            // Panel setup
            float panelX = 10;
            float panelY = Screen.height - 280;
            float panelWidth = 380;
            float panelHeight = 270;

            // Background
            GUI.Box(new Rect(panelX - 5, panelY - 5, panelWidth + 10, panelHeight + 10), "");

            // Styles
            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.yellow;

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = 13;
            labelStyle.normal.textColor = Color.white;

            GUIStyle selectedStyle = new GUIStyle(labelStyle);
            selectedStyle.fontStyle = FontStyle.Bold;
            selectedStyle.normal.textColor = Color.yellow;

            float y = panelY;

            // Header
            GUI.Label(new Rect(panelX, y, panelWidth, 22), "=== WAVE INPUT SIMULATOR ===", headerStyle);
            y += 28;

            // Band displays with value bars
            for (int i = 0; i < 5; i++)
            {
                bool isSelected = (i == _selectedBand);
                GUIStyle bandStyle = isSelected ? selectedStyle : labelStyle;

                // Band name and number
                string prefix = isSelected ? "► " : "  ";
                string bandLabel = $"{prefix}{i + 1}. {bandNames[i]}:";
                GUI.Label(new Rect(panelX, y, 100, 20), bandLabel, bandStyle);

                // Value bar background
                float barX = panelX + 100;
                float barWidth = 180;
                float barHeight = 16;
                GUI.Box(new Rect(barX, y + 2, barWidth, barHeight), "");

                // Value bar fill
                float fillWidth = _currentBands[i] * barWidth;
                Color barColor = bandColors[i];
                if (!isSelected) barColor.a = 0.6f; // Dim non-selected bars

                Texture2D barTexture = new Texture2D(1, 1);
                barTexture.SetPixel(0, 0, barColor);
                barTexture.Apply();

                GUI.DrawTexture(new Rect(barX + 1, y + 3, fillWidth - 2, barHeight - 2), barTexture);

                // Value text
                GUIStyle valueStyle = new GUIStyle(labelStyle);
                valueStyle.alignment = TextAnchor.MiddleRight;
                if (isSelected) valueStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(barX + barWidth + 5, y, 50, 20), $"{_currentBands[i]:F2}", valueStyle);

                y += 25;
            }

            y += 10;

            // Controls
            GUIStyle controlStyle = new GUIStyle(labelStyle);
            controlStyle.fontSize = 11;
            controlStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

            GUI.Label(new Rect(panelX, y, panelWidth, 18), "Controls:", labelStyle);
            y += 18;
            GUI.Label(new Rect(panelX + 10, y, panelWidth, 16), "• 1-5: Select Band", controlStyle);
            y += 16;
            GUI.Label(new Rect(panelX + 10, y, panelWidth, 16), "• ↑↓: Increase/Decrease Selected Band", controlStyle);
            y += 16;
            GUI.Label(new Rect(panelX + 10, y, panelWidth, 16), "• Q/W/E/R: Presets (Ordinary/Artist/Rebel/Random)", controlStyle);
            y += 16;

            // Current preset hint
            GUIStyle hintStyle = new GUIStyle(labelStyle);
            hintStyle.fontSize = 10;
            hintStyle.normal.textColor = Color.cyan;
            hintStyle.fontStyle = FontStyle.Italic;
            GUI.Label(new Rect(panelX, y, panelWidth, 16), "TIP: Press ↑ or ↓ and HOLD to see values change!", hintStyle);
        }
    }
}

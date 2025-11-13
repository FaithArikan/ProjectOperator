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

        private AIManager _aiManager;

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
            if (keyboard[Key.Digit1].wasPressedThisFrame) _selectedBand = 0;
            if (keyboard[Key.Digit2].wasPressedThisFrame) _selectedBand = 1;
            if (keyboard[Key.Digit3].wasPressedThisFrame) _selectedBand = 2;
            if (keyboard[Key.Digit4].wasPressedThisFrame) _selectedBand = 3;
            if (keyboard[Key.Digit5].wasPressedThisFrame) _selectedBand = 4;

            // Adjust selected band
            if (keyboard[_increaseKey].isPressed)
            {
                _currentBands[_selectedBand] = Mathf.Clamp01(_currentBands[_selectedBand] + Time.deltaTime * 0.5f);
            }

            if (keyboard[_decreaseKey].isPressed)
            {
                _currentBands[_selectedBand] = Mathf.Clamp01(_currentBands[_selectedBand] - Time.deltaTime * 0.5f);
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
            // Simple on-screen help
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 14;

            GUIStyle headerStyle = new GUIStyle(style);
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;

            float y = Screen.height - 160;

            // Header
            GUI.Label(new Rect(10, y, 400, 20), "=== Wave Input Simulator ===", headerStyle);
            y += 25;

            // Monitor status
            var monitor = FindFirstObjectByType<BrainActivityMonitor>();
            if (monitor != null)
            {
                GUIStyle statusStyle = new GUIStyle(style);
                statusStyle.normal.textColor = Color.yellow;
                statusStyle.fontSize = 12;
                // Note: We can't access private fields, but we can guide the user
                GUI.Label(new Rect(10, y, 500, 20), "Monitor found! Press F1 on BrainMonitorTestSetup to activate", statusStyle);
                y += 20;
            }

            // Controls
            GUI.Label(new Rect(10, y, 400, 20), $"Selected Band: {_selectedBand} (1-5: Select | ↑↓: Adjust)", style);
            y += 20;
            GUI.Label(new Rect(10, y, 500, 20), "Presets: Q=Ordinary | W=Artist | E=Rebel | R=Random", style);
            y += 20;

            // Current values
            string currentValues = $"Values: [{_currentBands[0]:F2}, {_currentBands[1]:F2}, {_currentBands[2]:F2}, {_currentBands[3]:F2}, {_currentBands[4]:F2}]";
            GUIStyle valueStyle = new GUIStyle(style);
            valueStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(10, y, 600, 20), currentValues, valueStyle);
        }
    }
}

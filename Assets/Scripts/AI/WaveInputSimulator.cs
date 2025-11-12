using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

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
        private Key _increaseKey = Key.W;

        private Key _decreaseKey = Key.S;

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

            Debug.Log($"[WaveInputSimulator] Preset loaded: {presetName}");
        }

        private void OnGUI()
        {
            // Simple on-screen help
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 14;

            float y = Screen.height - 120;
            GUI.Label(new Rect(10, y, 300, 20), "Wave Input Simulator", style);
            //GUI.Label(new Rect(10, y + 20, 300, 20), $"Selected Band: {_selectedBand} ({NeuralProfile.BandNames[_selectedBand]})", style);
            GUI.Label(new Rect(10, y + 40, 300, 20), "1-5: Select Band | ↑↓: Adjust", style);
            GUI.Label(new Rect(10, y + 60, 400, 20), "Q: Ordinary | W: Artist | E: Rebel | R: Random", style);
        }
    }
}

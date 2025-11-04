using UnityEngine;

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
        [SerializeField]
        private KeyCode _increaseKey = KeyCode.UpArrow;

        [SerializeField]
        private KeyCode _decreaseKey = KeyCode.DownArrow;

        [SerializeField]
        private int _selectedBand = 0;

        private AIManager _aiManager;

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
            // Select band with number keys
            if (Input.GetKeyDown(KeyCode.Alpha1)) _selectedBand = 0;
            if (Input.GetKeyDown(KeyCode.Alpha2)) _selectedBand = 1;
            if (Input.GetKeyDown(KeyCode.Alpha3)) _selectedBand = 2;
            if (Input.GetKeyDown(KeyCode.Alpha4)) _selectedBand = 3;
            if (Input.GetKeyDown(KeyCode.Alpha5)) _selectedBand = 4;

            // Adjust selected band
            if (Input.GetKey(_increaseKey))
            {
                _currentBands[_selectedBand] = Mathf.Clamp01(_currentBands[_selectedBand] + Time.deltaTime * 0.5f);
            }

            if (Input.GetKey(_decreaseKey))
            {
                _currentBands[_selectedBand] = Mathf.Clamp01(_currentBands[_selectedBand] - Time.deltaTime * 0.5f);
            }

            // Quick presets
            if (Input.GetKeyDown(KeyCode.Q)) SetPreset("Ordinary");
            if (Input.GetKeyDown(KeyCode.W)) SetPreset("Artist");
            if (Input.GetKeyDown(KeyCode.E)) SetPreset("Rebel");
            if (Input.GetKeyDown(KeyCode.R)) SetPreset("Random");
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
            GUI.Label(new Rect(10, y + 20, 300, 20), $"Selected Band: {_selectedBand} ({NeuralProfile.BandNames[_selectedBand]})", style);
            GUI.Label(new Rect(10, y + 40, 300, 20), "1-5: Select Band | ↑↓: Adjust", style);
            GUI.Label(new Rect(10, y + 60, 400, 20), "Q: Ordinary | W: Artist | E: Rebel | R: Random", style);
        }
    }
}

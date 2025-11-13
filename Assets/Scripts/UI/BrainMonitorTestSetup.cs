using UnityEngine;
using NeuralWaveBureau.AI;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Quick setup script for testing the Brain Activity Monitor
    /// Attach this to your BrainActivityMonitor GameObject
    /// </summary>
    public class BrainMonitorTestSetup : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField]
        private bool _autoSetup = true;

        [SerializeField]
        private float _setupDelay = 0.5f;

        private BrainActivityMonitor _monitor;
        private AIManager _aiManager;

        private void Start()
        {
            if (_autoSetup)
            {
                Invoke(nameof(Setup), _setupDelay);
            }
        }

        private void Setup()
        {
            _monitor = GetComponent<BrainActivityMonitor>();
            _aiManager = AIManager.Instance;

            if (_monitor == null)
            {
                Debug.LogError("[BrainMonitorTestSetup] BrainActivityMonitor component not found!");
                return;
            }

            if (_aiManager == null)
            {
                Debug.LogError("[BrainMonitorTestSetup] AIManager not found in scene!");
                return;
            }

            // Find or create a test citizen
            CitizenController citizen = FindObjectOfType<CitizenController>();

            if (citizen == null)
            {
                Debug.LogWarning("[BrainMonitorTestSetup] No citizen found! The monitor needs a citizen to display data.");
                Debug.LogWarning("[BrainMonitorTestSetup] Create a GameObject with CitizenController and a NeuralProfile assigned.");
                return;
            }

            // Setup the monitor
            Debug.Log("[BrainMonitorTestSetup] Setting up monitor...");

            // 1. Set active citizen
            _monitor.SetActiveCitizen(citizen);
            Debug.Log($"[BrainMonitorTestSetup] Active citizen set: {citizen.CitizenId}");

            // 2. Power on
            _monitor.PowerOn();
            Debug.Log("[BrainMonitorTestSetup] Monitor powered on");

            // 3. Start monitoring (after a short delay for power-on animation)
            Invoke(nameof(StartMonitoring), 2f);
        }

        private void StartMonitoring()
        {
            if (_monitor != null)
            {
                _monitor.StartMonitoring();
                Debug.Log("[BrainMonitorTestSetup] Monitoring started!");
                Debug.Log("[BrainMonitorTestSetup] You can now use Q/W/E/R to change presets and see waveforms update!");
            }
        }

        // Optional: Keyboard shortcut to manually trigger setup
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Setup();
            }
        }
    }
}

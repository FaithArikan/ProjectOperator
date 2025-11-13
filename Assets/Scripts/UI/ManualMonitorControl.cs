using UnityEngine;
using NeuralWaveBureau.AI;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Manual keyboard controls for testing the monitor
    /// Attach to any GameObject
    /// </summary>
    public class ManualMonitorControl : MonoBehaviour
    {
        private BrainActivityMonitor _monitor;
        private bool _hasSetup = false;

        private void Start()
        {
            _monitor = FindFirstObjectByType<BrainActivityMonitor>();
            if (_monitor == null)
            {
                Debug.LogError("ManualMonitorControl: No BrainActivityMonitor found!");
                enabled = false;
            }
        }

        private void Update()
        {
            if (_monitor == null)
                return;

            // Press F1 to setup
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Setup();
            }

            // Press F2 to power on
            if (Input.GetKeyDown(KeyCode.F2))
            {
                Debug.Log("Manual: Powering on monitor...");
                _monitor.PowerOn();
            }

            // Press F3 to start monitoring
            if (Input.GetKeyDown(KeyCode.F3))
            {
                Debug.Log("Manual: Starting monitoring...");
                _monitor.StartMonitoring();
            }

            // Press F4 to stop
            if (Input.GetKeyDown(KeyCode.F4))
            {
                Debug.Log("Manual: Stopping monitoring...");
                _monitor.StopMonitoring();
            }

            // Press F5 to reset
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Debug.Log("Manual: Resetting monitor...");
                _monitor.ResetMonitor();
            }
        }

        private void Setup()
        {
            if (_hasSetup)
            {
                Debug.LogWarning("Already set up! Press F4 to stop, F5 to reset first.");
                return;
            }

            Debug.Log("=== Manual Setup Starting ===");

            // Find citizen
            var citizen = FindFirstObjectByType<CitizenController>();
            if (citizen == null)
            {
                Debug.LogError("No citizen found! Create a GameObject with CitizenController first.");
                return;
            }

            // Set active citizen
            _monitor.SetActiveCitizen(citizen);
            Debug.Log($"Set active citizen: {citizen.CitizenId}");

            // Power on
            _monitor.PowerOn();
            Debug.Log("Monitor powered on");

            // Start monitoring after delay
            Invoke(nameof(StartMonitoring), 2f);
        }

        private void StartMonitoring()
        {
            _monitor.StartMonitoring();
            Debug.Log("Monitoring started!");
            _hasSetup = true;
            Debug.Log("=== Setup Complete ===");
            Debug.Log("Controls:");
            Debug.Log("  Q/W/E/R - Change wave presets");
            Debug.Log("  1-5 - Select band");
            Debug.Log("  ↑↓ - Adjust selected band");
            Debug.Log("  F4 - Stop monitoring");
            Debug.Log("  F5 - Reset");
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 14;
            style.padding = new RectOffset(10, 10, 10, 10);

            float x = Screen.width - 250;
            float y = 10;
            float width = 240;
            float height = 150;

            GUI.Box(new Rect(x - 5, y - 5, width + 10, height + 10), "");

            GUIStyle headerStyle = new GUIStyle(style);
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.yellow;

            GUI.Label(new Rect(x, y, width, 20), "Manual Controls", headerStyle);
            y += 25;

            style.fontSize = 12;
            GUI.Label(new Rect(x, y, width, 20), "F1 - Auto Setup", style);
            y += 20;
            GUI.Label(new Rect(x, y, width, 20), "F2 - Power On", style);
            y += 20;
            GUI.Label(new Rect(x, y, width, 20), "F3 - Start Monitor", style);
            y += 20;
            GUI.Label(new Rect(x, y, width, 20), "F4 - Stop Monitor", style);
            y += 20;
            GUI.Label(new Rect(x, y, width, 20), "F5 - Reset", style);
        }
    }
}

using UnityEngine;
using NeuralWaveBureau.AI;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Emergency fix script - automatically sets up the monitor on Start
    /// Attach to BrainActivityMonitor GameObject
    /// </summary>
    public class QuickMonitorFix : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField]
        private bool _autoSetup = false; // Set to true to auto-power on at start

        private void Start()
        {
            if (!_autoSetup)
                return;

            Debug.Log("=== QUICK MONITOR FIX: Starting ===");

            // Get monitor
            var monitor = GetComponent<BrainActivityMonitor>();
            if (monitor == null)
            {
                Debug.LogError("QuickMonitorFix: No BrainActivityMonitor on this GameObject!");
                return;
            }

            // Find citizen
            var citizen = FindFirstObjectByType<CitizenController>();
            if (citizen == null)
            {
                Debug.LogError("QuickMonitorFix: No CitizenController found in scene!");
                Debug.LogError("Create a GameObject with CitizenController + Animator + NeuralProfile");
                return;
            }

            // Check citizen has profile
            if (citizen.Profile == null)
            {
                Debug.LogError($"QuickMonitorFix: Citizen '{citizen.CitizenId}' has no NeuralProfile assigned!");
                return;
            }

            Debug.Log($"QuickMonitorFix: Found citizen '{citizen.CitizenId}'");

            // Setup sequence
            Invoke(nameof(SetupMonitor), 0.5f); // Wait half second for everything to initialize
        }

        private void SetupMonitor()
        {
            var monitor = GetComponent<BrainActivityMonitor>();
            var citizen = FindFirstObjectByType<CitizenController>();

            Debug.Log("QuickMonitorFix: Step 1 - Setting active citizen...");
            monitor.SetActiveCitizen(citizen);

            Debug.Log("QuickMonitorFix: Step 2 - Powering on monitor...");
            monitor.PowerOn();

            Debug.Log("QuickMonitorFix: Step 3 - Starting monitoring...");
            Invoke(nameof(StartMonitoring), 2f); // Wait for power-on animation
        }

        private void StartMonitoring()
        {
            var monitor = GetComponent<BrainActivityMonitor>();
            monitor.StartMonitoring();

            Debug.Log("=== QUICK MONITOR FIX: COMPLETE ===");
            Debug.Log("Monitor is now active!");
            Debug.Log("Press Q/W/E/R to change wave presets");
            Debug.Log("Buffer should start filling with data now");
        }
    }
}

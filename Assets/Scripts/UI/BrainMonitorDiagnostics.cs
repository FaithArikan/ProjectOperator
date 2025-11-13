using UnityEngine;
using NeuralWaveBureau.AI;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Diagnostic tool to debug Brain Activity Monitor issues.
    /// Attach to any GameObject and it will report the system status every second.
    /// </summary>
    public class BrainMonitorDiagnostics : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private bool _enableLogging = true;

        [SerializeField]
        private float _logInterval = 2f; // Log every 2 seconds

        private float _logTimer = 0f;
        private BrainActivityMonitor _monitor;
        private AIManager _aiManager;
        private WaveInputSimulator _simulator;

        private void Start()
        {
            // Find components
            _monitor = FindFirstObjectByType<BrainActivityMonitor>();
            _aiManager = AIManager.Instance;
            _simulator = FindFirstObjectByType<WaveInputSimulator>();

            if (_enableLogging)
            {
                Debug.Log("=== Brain Monitor Diagnostics Started ===");
                RunInitialDiagnostics();
            }
        }

        private void Update()
        {
            if (!_enableLogging)
                return;

            _logTimer += Time.deltaTime;
            if (_logTimer >= _logInterval)
            {
                _logTimer = 0f;
                LogStatus();
            }
        }

        private void RunInitialDiagnostics()
        {
            Debug.Log("=== Initial System Check ===");

            // Check AIManager
            if (_aiManager == null)
            {
                Debug.LogError("❌ AIManager not found! Create a GameObject with AIManager component.");
            }
            else
            {
                Debug.Log($"✓ AIManager found");
                if (_aiManager.Settings == null)
                {
                    Debug.LogError("  ❌ AIManager has no AISettings assigned!");
                }
                else
                {
                    Debug.Log($"  ✓ AISettings assigned: {_aiManager.Settings.name}");
                }

                if (_aiManager.ActiveCitizen == null)
                {
                    Debug.LogWarning("  ⚠ No active citizen set");
                }
                else
                {
                    Debug.Log($"  ✓ Active citizen: {_aiManager.ActiveCitizen.CitizenId}");
                }
            }

            // Check Monitor
            if (_monitor == null)
            {
                Debug.LogError("❌ BrainActivityMonitor not found!");
            }
            else
            {
                Debug.Log($"✓ BrainActivityMonitor found on: {_monitor.gameObject.name}");
            }

            // Check Wave Input Simulator
            if (_simulator == null)
            {
                Debug.LogWarning("⚠ WaveInputSimulator not found - no keyboard input available");
            }
            else
            {
                Debug.Log($"✓ WaveInputSimulator found");
            }

            // Check Citizens
            var citizens = FindObjectsByType<CitizenController>(FindObjectsSortMode.None);
            if (citizens.Length == 0)
            {
                Debug.LogError("❌ No citizens found! Create a GameObject with CitizenController + Animator + NeuralProfile");
            }
            else
            {
                Debug.Log($"✓ Found {citizens.Length} citizen(s):");
                foreach (var citizen in citizens)
                {
                    if (citizen.Profile == null)
                    {
                        Debug.LogError($"  ❌ Citizen '{citizen.CitizenId}' has no NeuralProfile!");
                    }
                    else
                    {
                        Debug.Log($"  ✓ {citizen.CitizenId}: {citizen.Profile.name} - State: {citizen.CurrentState}");
                    }
                }
            }

            Debug.Log("=== End Initial Check ===\n");
        }

        private void LogStatus()
        {
            if (_aiManager == null)
                return;

            Debug.Log("=== System Status ===");

            // AIManager status
            var sample = _aiManager.CurrentWaveSample;
            string waveValues = $"[{sample.bandValues[0]:F2}, {sample.bandValues[1]:F2}, {sample.bandValues[2]:F2}, {sample.bandValues[3]:F2}, {sample.bandValues[4]:F2}]";
            Debug.Log($"Current Wave Sample: {waveValues}");

            // Active citizen status
            if (_aiManager.ActiveCitizen != null)
            {
                var citizen = _aiManager.ActiveCitizen;
                Debug.Log($"Active Citizen: {citizen.CitizenId}");
                Debug.Log($"  State: {citizen.CurrentState}");
                Debug.Log($"  Evaluation Score: {citizen.EvaluationScore:F3}");
                Debug.Log($"  Instability: {citizen.Instability:F2}");
                Debug.Log($"  Is Active: {citizen.IsActive}");
            }
            else
            {
                Debug.LogWarning("No active citizen - waveforms won't update!");
            }

            // Monitor status (we can't access private fields, but we can infer)
            if (_monitor != null)
            {
                Debug.Log($"Monitor found on: {_monitor.gameObject.name}");
                Debug.Log("  Note: Monitor must be powered on and monitoring to show waveforms");
                Debug.Log("  Press F1 on BrainMonitorTestSetup to auto-setup");
            }

            Debug.Log("===================\n");
        }

        private void OnGUI()
        {
            if (!_enableLogging)
                return;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.padding = new RectOffset(5, 5, 5, 5);

            GUIStyle headerStyle = new GUIStyle(style);
            headerStyle.fontSize = 14;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.yellow;

            float x = 10;
            float y = 10;
            float width = 400;
            float lineHeight = 20;

            // Background
            GUI.Box(new Rect(x - 5, y - 5, width + 10, 250), "");

            // Header
            GUI.Label(new Rect(x, y, width, lineHeight), "=== BRAIN MONITOR DIAGNOSTICS ===", headerStyle);
            y += lineHeight + 5;

            // Status indicators
            GUIStyle okStyle = new GUIStyle(style);
            okStyle.normal.textColor = Color.green;

            GUIStyle errorStyle = new GUIStyle(style);
            errorStyle.normal.textColor = Color.red;

            GUIStyle warnStyle = new GUIStyle(style);
            warnStyle.normal.textColor = Color.yellow;

            // AIManager
            if (_aiManager == null)
            {
                GUI.Label(new Rect(x, y, width, lineHeight), "❌ AIManager: NOT FOUND", errorStyle);
            }
            else
            {
                GUI.Label(new Rect(x, y, width, lineHeight), "✓ AIManager: OK", okStyle);
            }
            y += lineHeight;

            // Active Citizen
            if (_aiManager != null && _aiManager.ActiveCitizen != null)
            {
                GUI.Label(new Rect(x, y, width, lineHeight), $"✓ Active Citizen: {_aiManager.ActiveCitizen.CitizenId}", okStyle);
                y += lineHeight;
                GUI.Label(new Rect(x, y, width, lineHeight), $"  State: {_aiManager.ActiveCitizen.CurrentState}", style);
                y += lineHeight;
                GUI.Label(new Rect(x, y, width, lineHeight), $"  Is Active: {_aiManager.ActiveCitizen.IsActive}", style);
            }
            else
            {
                GUI.Label(new Rect(x, y, width, lineHeight), "❌ Active Citizen: NONE", errorStyle);
                y += lineHeight;
                GUI.Label(new Rect(x, y, width, lineHeight), "  → Monitor won't update without citizen!", warnStyle);
            }
            y += lineHeight;

            // Wave values
            if (_aiManager != null)
            {
                var sample = _aiManager.CurrentWaveSample;
                string values = $"Wave: [{sample.bandValues[0]:F2}, {sample.bandValues[1]:F2}, {sample.bandValues[2]:F2}, {sample.bandValues[3]:F2}, {sample.bandValues[4]:F2}]";
                GUI.Label(new Rect(x, y, width + 100, lineHeight), values, style);
            }
            y += lineHeight;

            // Instructions
            y += 5;
            GUI.Label(new Rect(x, y, width, lineHeight), "Instructions:", headerStyle);
            y += lineHeight;
            GUI.Label(new Rect(x, y, width, lineHeight), "1. Press F1 to auto-setup monitor", style);
            y += lineHeight;
            GUI.Label(new Rect(x, y, width, lineHeight), "2. Press Q/W/E/R for presets", style);
            y += lineHeight;
            GUI.Label(new Rect(x, y, width, lineHeight), "3. Use ↑↓ to adjust selected band", style);
        }
    }
}

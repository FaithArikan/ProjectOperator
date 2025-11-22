using UnityEngine;
using System;
using UnityEngine.Events;
using NeuralWaveBureau.UI;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Defines a monitoring station where citizens stand to have their brain waves monitored.
    /// Place this GameObject behind/in front of your monitor in the scene.
    /// </summary>
    public class MonitoringStation : MonoBehaviour
    {
        [Header("Position Settings")]
        [SerializeField]
        [Tooltip("Offset from this transform where citizen should stand")]
        private Vector3 _citizenPositionOffset = Vector3.zero;

        [SerializeField]
        [Tooltip("Direction the citizen should face (relative to this transform)")]
        private Vector3 _facingDirection = Vector3.forward;

        [Header("Auto-Monitoring")]
        [SerializeField]
        [Tooltip("Automatically start monitoring when citizen arrives")]
        private bool _autoStartMonitoring = true;

        [SerializeField]
        [Tooltip("Delay before starting monitoring after arrival (seconds)")]
        private float _monitoringStartDelay = 0.5f;

        [Header("Visual Helpers")]
        [SerializeField]
        private bool _showGizmos = true;

        [SerializeField]
        private Color _gizmoColor = Color.cyan;

        // Current citizen at this station
        private CitizenMovement _currentCitizen;
        private BrainActivityMonitor _brainMonitor;

        // Events
        public event Action<CitizenMovement> OnCitizenArrivedAtStation;
        public event Action<CitizenMovement> OnCitizenLeftStation;

        [Header("Unity Events")]
        public Action OnCitizenArrivedEvent;
        public Action OnCitizenLeftEvent;

        public CitizenMovement CurrentCitizen => _currentCitizen;
        public bool IsOccupied => _currentCitizen != null;

        private void Awake()
        {
            // Find brain monitor in scene
            _brainMonitor = FindFirstObjectByType<BrainActivityMonitor>();
            if (_brainMonitor == null)
            {
                Debug.LogWarning("[MonitoringStation] No BrainActivityMonitor found in scene!");
            }
        }

        /// <summary>
        /// Gets the world position where citizen should stand
        /// </summary>
        public Vector3 GetCitizenPosition()
        {
            return transform.position + transform.TransformDirection(_citizenPositionOffset);
        }

        /// <summary>
        /// Gets the world-space direction citizen should face
        /// </summary>
        public Vector3 GetFacingDirection()
        {
            return transform.TransformDirection(_facingDirection.normalized);
        }

        /// <summary>
        /// Called when a citizen arrives at this station
        /// </summary>
        public void OnCitizenArrived(CitizenMovement citizen)
        {
            if (citizen == null)
                return;

            // Remove previous citizen if any
            if (_currentCitizen != null && _currentCitizen != citizen)
            {
                RemoveCitizen();
            }

            _currentCitizen = citizen;
            Debug.Log($"[MonitoringStation] Citizen {citizen.CitizenController.CitizenId} arrived at station");

            OnCitizenArrivedAtStation?.Invoke(citizen);
            OnCitizenArrivedEvent?.Invoke();

            // Auto-start monitoring if enabled
            if (_autoStartMonitoring && _brainMonitor != null)
            {
                StartMonitoringAfterDelay();
            }
        }

        /// <summary>
        /// Starts monitoring the current citizen after a delay
        /// </summary>
        private void StartMonitoringAfterDelay()
        {
            if (_currentCitizen == null)
                return;

            // Use DOTween delay if available, otherwise use Invoke
            if (_monitoringStartDelay > 0)
            {
                Invoke(nameof(StartMonitoring), _monitoringStartDelay);
            }
            else
            {
                StartMonitoring();
            }
        }

        /// <summary>
        /// Starts monitoring the current citizen
        /// </summary>
        private void StartMonitoring()
        {
            if (_currentCitizen == null || _brainMonitor == null)
                return;

            // Set the citizen in the brain monitor
            _brainMonitor.SetActiveCitizen(_currentCitizen.CitizenController);

            // Power on the monitor if it's not already on
            _brainMonitor.PowerOn();

            // Explicitly start monitoring to show waves
            _brainMonitor.StartMonitoring();

            Debug.Log($"[MonitoringStation] Started monitoring citizen {_currentCitizen.CitizenController.CitizenId}");
        }

        /// <summary>
        /// Removes the current citizen from this station
        /// </summary>
        public void RemoveCitizen()
        {
            if (_currentCitizen == null)
                return;

            CitizenMovement citizen = _currentCitizen;
            _currentCitizen = null;

            Debug.Log($"[MonitoringStation] Citizen {citizen.CitizenController.CitizenId} left station");
            OnCitizenLeftStation?.Invoke(citizen);
            OnCitizenLeftEvent?.Invoke();
        }

        /// <summary>
        /// Sends a citizen to this station
        /// </summary>
        public void SendCitizenHere(CitizenMovement citizen)
        {
            if (citizen == null)
                return;

            citizen.MoveToStation(this);
        }

        private void OnDrawGizmos()
        {
            if (!_showGizmos)
                return;

            Vector3 citizenPos = GetCitizenPosition();
            Vector3 facingDir = GetFacingDirection();

            // Draw position marker
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(citizenPos, 0.3f);
            Gizmos.DrawLine(transform.position, citizenPos);

            // Draw facing direction
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(citizenPos, facingDir * 0.5f);

            // Draw arrow for facing direction
            Vector3 arrowTip = citizenPos + facingDir * 0.5f;
            Vector3 arrowRight = Quaternion.Euler(0, 30, 0) * -facingDir * 0.15f;
            Vector3 arrowLeft = Quaternion.Euler(0, -30, 0) * -facingDir * 0.15f;
            Gizmos.DrawLine(arrowTip, arrowTip + arrowRight);
            Gizmos.DrawLine(arrowTip, arrowTip + arrowLeft);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showGizmos)
                return;

            // Draw station bounds when selected
            Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 0.3f);
            Gizmos.DrawCube(GetCitizenPosition(), new Vector3(0.5f, 2f, 0.5f));
        }
    }
}

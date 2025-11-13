using UnityEngine;
using Unity.Cinemachine;

namespace NeuralWaveBureau
{
    /// <summary>
    /// Manages camera positioning and transitions using Cinemachine.
    /// Controls camera movement between room view and monitor close-up.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [Header("Cinemachine Virtual Cameras")]
        [Tooltip("Virtual camera for viewing the full room")]
        [SerializeField]
        private CinemachineCamera _roomViewCamera;

        [Tooltip("Virtual camera focused on the monitor")]
        [SerializeField]
        private CinemachineCamera _monitorViewCamera;

        [Header("Camera Settings")]
        [SerializeField]
        [Tooltip("Base priority for cameras (active camera will have +10)")]
        private int _basePriority = 10;

        [SerializeField]
        [Tooltip("Duration for camera blend transitions")]
        private float _blendDuration = 1.5f;

        // State
        private bool _isInMonitorView = false;

        // Singleton pattern
        public static CameraManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Initialize camera priorities - start in room view
            if (_roomViewCamera != null && _monitorViewCamera != null)
            {
                _roomViewCamera.Priority.Value = _basePriority + 10;
                _monitorViewCamera.Priority.Value = _basePriority;
                _isInMonitorView = false;
            }
        }

        /// <summary>
        /// Moves camera to monitor view (close-up)
        /// </summary>
        public void MoveToMonitorView()
        {
            if (_isInMonitorView || _monitorViewCamera == null || _roomViewCamera == null)
                return;

            _isInMonitorView = true;

            // Switch camera priority to activate monitor view
            _monitorViewCamera.Priority.Value = _basePriority + 10;
            _roomViewCamera.Priority.Value = _basePriority;
        }

        /// <summary>
        /// Moves camera back to room view
        /// </summary>
        public void MoveToRoomView()
        {
            if (!_isInMonitorView || _roomViewCamera == null || _monitorViewCamera == null)
                return;

            _isInMonitorView = false;

            // Switch camera priority to activate room view
            _roomViewCamera.Priority.Value = _basePriority + 10;
            _monitorViewCamera.Priority.Value = _basePriority;
        }

        /// <summary>
        /// Toggles between room view and monitor view
        /// </summary>
        public void ToggleView()
        {
            if (_isInMonitorView)
            {
                MoveToRoomView();
            }
            else
            {
                MoveToMonitorView();
            }
        }

        /// <summary>
        /// Gets whether camera is currently in monitor view
        /// </summary>
        public bool IsInMonitorView => _isInMonitorView;

        /// <summary>
        /// Gets the blend duration for camera transitions
        /// </summary>
        public float BlendDuration => _blendDuration;

        #if UNITY_EDITOR
        /// <summary>
        /// Forces switch to room view (Editor helper)
        /// </summary>
        [ContextMenu("Force Switch to Room View")]
        private void ForceRoomView()
        {
            if (_roomViewCamera != null && _monitorViewCamera != null)
            {
                _roomViewCamera.Priority.Value = _basePriority + 10;
                _monitorViewCamera.Priority.Value = _basePriority;
                _isInMonitorView = false;
                Debug.Log("Switched to Room View");
            }
        }

        /// <summary>
        /// Forces switch to monitor view (Editor helper)
        /// </summary>
        [ContextMenu("Force Switch to Monitor View")]
        private void ForceMonitorView()
        {
            if (_roomViewCamera != null && _monitorViewCamera != null)
            {
                _monitorViewCamera.Priority.Value = _basePriority + 10;
                _roomViewCamera.Priority.Value = _basePriority;
                _isInMonitorView = true;
                Debug.Log("Switched to Monitor View");
            }
        }
        #endif
    }
}

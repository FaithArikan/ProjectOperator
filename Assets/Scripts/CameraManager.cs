using UnityEngine;
using Unity.Cinemachine;

namespace NeuralWaveBureau
{
    /// <summary>
    /// Camera view states for different screen interactions.
    /// </summary>
    public enum CameraView
    {
        Room,
        Rules,
        Monitor
    }

    /// <summary>
    /// Manages camera positioning and transitions using Cinemachine.
    /// Controls camera movement between room view, rules monitor, and brain activity monitor.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [Header("Cinemachine Virtual Cameras")]
        [Tooltip("Virtual camera for viewing the full room")]
        [SerializeField]
        private CinemachineCamera _roomViewCamera;

        [Tooltip("Virtual camera focused on the rules/instructions monitor")]
        [SerializeField]
        private CinemachineCamera _rulesViewCamera;

        [Tooltip("Virtual camera focused on the brain activity monitor")]
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
        private CameraView _currentView = CameraView.Room;

        // Singleton pattern
        public static CameraManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Initialize camera priorities - start in room view
            if (_roomViewCamera != null)
            {
                _roomViewCamera.Priority.Value = _basePriority + 10;
            }

            if (_rulesViewCamera != null)
            {
                _rulesViewCamera.Priority.Value = _basePriority;
            }

            if (_monitorViewCamera != null)
            {
                _monitorViewCamera.Priority.Value = _basePriority;
            }

            _currentView = CameraView.Room;
        }

        /// <summary>
        /// Moves camera to the rules/instructions monitor view.
        /// </summary>
        public void MoveToRulesView()
        {
            if (_currentView == CameraView.Rules || _rulesViewCamera == null)
                return;

            Debug.Log("CameraManager: Moving to Rules View");
            _currentView = CameraView.Rules;
            SetActiveCamera(_rulesViewCamera);
        }

        /// <summary>
        /// Moves camera to brain activity monitor view (close-up).
        /// </summary>
        public void MoveToMonitorView()
        {
            if (_currentView == CameraView.Monitor || _monitorViewCamera == null)
                return;

            Debug.Log("CameraManager: Moving to Monitor View");
            _currentView = CameraView.Monitor;
            SetActiveCamera(_monitorViewCamera);
        }

        /// <summary>
        /// Moves camera back to room view.
        /// </summary>
        public void MoveToRoomView()
        {
            if (_currentView == CameraView.Room || _roomViewCamera == null)
                return;

            Debug.Log("CameraManager: Moving to Room View");
            _currentView = CameraView.Room;
            SetActiveCamera(_roomViewCamera);
        }

        /// <summary>
        /// Moves camera to a specific Cinemachine camera (flexible method for any camera).
        /// Use this when you have a custom camera that's not part of the predefined views.
        /// </summary>
        /// <param name="targetCamera">The Cinemachine camera to activate</param>
        public void MoveToCamera(CinemachineCamera targetCamera)
        {
            if (targetCamera == null)
            {
                Debug.LogWarning("CameraManager: Cannot move to null camera");
                return;
            }

            Debug.Log($"CameraManager: Moving to camera: {targetCamera.name}");

            // Reset all registered cameras to base priority
            SetAllCamerasToBasePriority();

            // Activate the target camera
            targetCamera.Priority.Value = _basePriority + 10;

            // Update state based on which camera was activated
            UpdateCurrentViewState(targetCamera);
        }

        /// <summary>
        /// Sets the specified camera as active by adjusting priorities.
        /// </summary>
        private void SetActiveCamera(CinemachineCamera activeCamera)
        {
            // Set all cameras to base priority
            SetAllCamerasToBasePriority();

            // Boost the active camera's priority
            if (activeCamera != null)
                activeCamera.Priority.Value = _basePriority + 10;
        }

        /// <summary>
        /// Resets all registered cameras to base priority.
        /// </summary>
        private void SetAllCamerasToBasePriority()
        {
            if (_roomViewCamera != null)
                _roomViewCamera.Priority.Value = _basePriority;

            if (_rulesViewCamera != null)
                _rulesViewCamera.Priority.Value = _basePriority;

            if (_monitorViewCamera != null)
                _monitorViewCamera.Priority.Value = _basePriority;
        }

        /// <summary>
        /// Updates the current view state based on which camera is active.
        /// </summary>
        private void UpdateCurrentViewState(CinemachineCamera activeCamera)
        {
            if (activeCamera == _roomViewCamera)
                _currentView = CameraView.Room;
            else if (activeCamera == _rulesViewCamera)
                _currentView = CameraView.Rules;
            else if (activeCamera == _monitorViewCamera)
                _currentView = CameraView.Monitor;
            // For custom cameras, we don't update the enum state
        }

        /// <summary>
        /// Toggles between room view and monitor view (legacy support).
        /// </summary>
        public void ToggleView()
        {
            if (_currentView == CameraView.Monitor)
            {
                MoveToRoomView();
            }
            else
            {
                MoveToMonitorView();
            }
        }

        /// <summary>
        /// Gets the current camera view.
        /// </summary>
        public CameraView CurrentView => _currentView;

        /// <summary>
        /// Gets whether camera is currently in monitor view (legacy support).
        /// </summary>
        public bool IsInMonitorView => _currentView == CameraView.Monitor;

        /// <summary>
        /// Gets the blend duration for camera transitions
        /// </summary>
        public float BlendDuration => _blendDuration;

        /// <summary>
        /// Checks if the specified camera is currently active.
        /// </summary>
        /// <param name="camera">The camera to check</param>
        /// <returns>True if the camera is active (has elevated priority)</returns>
        public bool IsCameraActive(CinemachineCamera camera)
        {
            if (camera == null)
                return false;

            // A camera is active if its priority is above base priority
            return camera.Priority.Value > _basePriority;
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Forces switch to room view (Editor helper)
        /// </summary>
        [ContextMenu("Force Switch to Room View")]
        private void ForceRoomView()
        {
            if (_roomViewCamera != null)
            {
                _currentView = CameraView.Room;
                SetActiveCamera(_roomViewCamera);
                Debug.Log("Switched to Room View");
            }
        }

        /// <summary>
        /// Forces switch to rules view (Editor helper)
        /// </summary>
        [ContextMenu("Force Switch to Rules View")]
        private void ForceRulesView()
        {
            if (_rulesViewCamera != null)
            {
                _currentView = CameraView.Rules;
                SetActiveCamera(_rulesViewCamera);
                Debug.Log("Switched to Rules View");
            }
        }

        /// <summary>
        /// Forces switch to monitor view (Editor helper)
        /// </summary>
        [ContextMenu("Force Switch to Monitor View")]
        private void ForceMonitorView()
        {
            if (_monitorViewCamera != null)
            {
                _currentView = CameraView.Monitor;
                SetActiveCamera(_monitorViewCamera);
                Debug.Log("Switched to Monitor View");
            }
        }
        #endif
    }
}

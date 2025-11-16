using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Unity.Cinemachine;
using NeuralWaveBureau;

/// <summary>
/// Handles player interaction with clickable screens in the 3D environment.
/// Attach this to any screen object that should be clickable.
/// Uses the new Unity Input System for mouse detection.
/// Automatically switches to the assigned Cinemachine camera when interacted with.
/// Ignores interactions when pointer is over UI elements.
/// </summary>
[RequireComponent(typeof(Collider))]
public class InteractableScreen : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private string _screenName = "Screen";
    [SerializeField] private float _interactionDistance = 10f;
    [SerializeField] private bool _isInteractable = true;

    [Header("Camera Settings")]
    [Tooltip("The Cinemachine camera to switch to when this screen is clicked")]
    [SerializeField] private CinemachineCamera _screenCamera;

    [Header("Visual Feedback")]
    [SerializeField] private Material _normalMaterial;
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private Renderer _screenRenderer;

    [Header("Events")]
    [SerializeField] private UnityEvent _onInteract;

    private Camera _mainCamera;
    private bool _isHighlighted = false;

    private void Awake()
    {
        _mainCamera = Camera.main;

        // Ensure we have a collider for raycasting
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogError($"InteractableScreen on {gameObject.name} requires a Collider component!");
        }

        // Auto-find renderer if not assigned
        if (_screenRenderer == null)
        {
            _screenRenderer = GetComponent<Renderer>();
        }
    }

    /// <summary>
    /// Checks if the pointer is currently over a UI element.
    /// </summary>
    private bool IsPointerOverUI()
    {
        // Check if EventSystem exists
        if (EventSystem.current == null)
            return false;

        // Check if pointer is over a UI GameObject
        return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// Called when the player interacts with this screen.
    /// Toggles between this screen's camera and room view.
    /// </summary>
    public void Interact()
    {
        // If this camera is already active, return to room view
        if (CameraManager.Instance.IsCameraActive(_screenCamera))
        {
            CameraManager.Instance.MoveToRoomView();
        }
        else
        {
            // Switch to this screen's camera
            CameraManager.Instance.MoveToCamera(_screenCamera);
        }

        // Invoke any additional interaction events
        _onInteract?.Invoke();
    }

    /// <summary>
    /// Applies visual highlight when hovering.
    /// </summary>
    private void Highlight(bool highlight)
    {
        _isHighlighted = highlight;

        if (_screenRenderer != null && _highlightMaterial != null && _normalMaterial != null)
        {
            _screenRenderer.material = highlight ? _highlightMaterial : _normalMaterial;
        }
    }

    /// <summary>
    /// Enables or disables interaction with this screen.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        _isInteractable = interactable;

        if (!_isInteractable && _isHighlighted)
        {
            Highlight(false);
        }
    }

    /// <summary>
    /// Public method to trigger interaction programmatically.
    /// </summary>
    public void TriggerInteraction()
    {
        if (_isInteractable)
        {
            Interact();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize interaction distance in editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

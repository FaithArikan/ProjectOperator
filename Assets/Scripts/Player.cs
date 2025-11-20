using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private InputSystem_Actions _inputActions;
    private Camera _mainCamera;
    private HandInteractor _handInteractor;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _mainCamera = Camera.main;
        _handInteractor = GetComponent<HandInteractor>();
    }

    private void OnEnable()
    {
        _inputActions.Player.Interact.performed += OnInteract;
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Player.Interact.performed -= OnInteract;
        _inputActions.Disable();
    }

    private void OnDestroy()
    {
        _inputActions?.Dispose();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        RaycastHit hit;
        // Shoot ray from center of screen
        if (Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out hit, 2f))
        {
            // Check if object is a computer button
            if (hit.transform.CompareTag("ComputerButton"))
            {
                // Get the InteractionPoint child from the button
                Transform touchPoint = hit.transform.Find("InteractionPoint");

                // Tell the hand interactor to do its job
                if (_handInteractor != null)
                {
                    _handInteractor.TryInteract(touchPoint);
                }
            }
        }
    }
}

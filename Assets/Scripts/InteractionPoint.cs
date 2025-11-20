using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionPoint : MonoBehaviour
{
    [SerializeField] private HandInteractor _handInteractor;

    public void OnInteract()
    {
        _handInteractor.TryInteract(transform);
    }
}

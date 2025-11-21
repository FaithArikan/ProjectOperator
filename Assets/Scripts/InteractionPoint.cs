using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionPoint : MonoBehaviour
{
    [SerializeField] private HandInteractor _handInteractor;
    [SerializeField] private InteractionType _interactionType;
    [SerializeField] private float reachSpeed;

    public void OnInteract()
    {
        _handInteractor.TryInteract(transform, _interactionType, reachSpeed);
    }
}
public enum InteractionType
{
    Power,
    Done,
}
using UnityEngine;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionPoint : MonoBehaviour
{
    [SerializeField] private HandInteractor _handInteractor;
    [SerializeField] private InteractionType _interactionType;
    [SerializeField] private float reachSpeed;
    [SerializeField] private Transform _interactionTarget;

    public void OnInteract()
    {
        Transform target = _interactionTarget != null ? _interactionTarget : transform;
        _handInteractor.TryInteract(target, _interactionType, reachSpeed);
    }
}
public enum InteractionType
{
    Power,
    Done,
    Next,
    Settings,
    Play,
    Quit,
}
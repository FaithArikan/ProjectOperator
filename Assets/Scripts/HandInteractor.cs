using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.Events;
using Unity.VisualScripting;
using NeuralWaveBureau.UI;
using NeuralWaveBureau.AI;

public class HandInteractor : MonoBehaviour
{
    [Header("IK Setup")]
    public Rig armRig;
    public TwoBoneIKConstraint constraint;

    private Transform _ikTarget;
    private Transform _originalTargetParent;
    private Vector3 _originalTargetLocalPos;
    private Quaternion _originalTargetLocalRot;

    private void Awake()
    {
        if (armRig == null && constraint != null)
            armRig = constraint.GetComponentInParent<Rig>();

        if (constraint != null && constraint.data.target != null)
        {
            _ikTarget = constraint.data.target;
            _originalTargetParent = _ikTarget.parent;
            _originalTargetLocalPos = _ikTarget.localPosition;
            _originalTargetLocalRot = _ikTarget.localRotation;
        }
    }

    [Header("Settings")]
    public float reachSpeed = 2f;
    public Action<InteractionType, float, Transform> OnReachTarget;

    private CancellationTokenSource _reachCts;


    private void OnEnable()
    {
        OnReachTarget += OnReachTargetListener;
    }

    private void OnDisable()
    {
        OnReachTarget -= OnReachTargetListener;
    }

    private void OnReachTargetListener(InteractionType interactionType, float reachSpeed, Transform targetPoint)
    {
        this.reachSpeed = reachSpeed;

        switch (interactionType)
        {
            case InteractionType.Power:
                BrainActivityMonitor.Instance.TogglePower();
                break;
            case InteractionType.Done:
                CitizenSpawner.Instance.FinishCurrentCitizen();
                break;
        }
    }

    private void OnDestroy()
    {
        _reachCts?.Cancel();
        _reachCts?.Dispose();
    }

    // Call this function when the player presses the button
    public void TryInteract(Transform objectToTouch, InteractionType interactionType, float reachSpeed)
    {
        if (_reachCts != null)
        {
            _reachCts.Cancel();
            _reachCts.Dispose();
        }
        _reachCts = new CancellationTokenSource();

        ReachAndPress(objectToTouch, _reachCts.Token, interactionType, reachSpeed).Forget();
    }

    async UniTaskVoid ReachAndPress(Transform targetPoint, CancellationToken token, InteractionType interactionType, float reachSpeed)
    {
        // Parent the IK target to the interaction point so it follows it
        if (_ikTarget != null)
        {
            _ikTarget.SetParent(targetPoint);
            _ikTarget.localPosition = Vector3.zero;
            //_ikTarget.localRotation = Quaternion.identity;
        }

        // Ensure the main Rig is active so the constraint can work
        if (armRig != null) armRig.weight = 1f;

        // 2. Smoothly increase IK weight (Reach out)
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * reachSpeed;
            constraint.weight = Mathf.Lerp(0f, 1f, timer);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }

        OnReachTarget?.Invoke(interactionType, reachSpeed, targetPoint);

        // Optional: Wait a tiny bit to simulate holding the button
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);

        // 4. Smoothly decrease IK weight (Return hand to body)
        timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * reachSpeed;
            constraint.weight = Mathf.Lerp(1f, 0f, timer);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }

        // Restore the IK target to its original parent and position
        if (_ikTarget != null)
        {
            _ikTarget.SetParent(_originalTargetParent);
            _ikTarget.localPosition = _originalTargetLocalPos;
            _ikTarget.localRotation = _originalTargetLocalRot;
        }
    }
}
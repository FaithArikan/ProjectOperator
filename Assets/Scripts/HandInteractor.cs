using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.Events;

public class HandInteractor : MonoBehaviour
{
    [Header("IK Setup")]
    public Rig armRig;
    public TwoBoneIKConstraint constraint;

    private void Awake()
    {
        if (armRig == null && constraint != null)
            armRig = constraint.GetComponentInParent<Rig>();
    }

    [Header("Settings")]
    public float reachSpeed = 5f;
    public UnityEvent OnReachTarget;

    private CancellationTokenSource _reachCts;

    private void OnDestroy()
    {
        _reachCts?.Cancel();
        _reachCts?.Dispose();
    }

    // Call this function when the player presses the button
    public void TryInteract(Transform objectToTouch)
    {
        if (_reachCts != null)
        {
            _reachCts.Cancel();
            _reachCts.Dispose();
        }
        _reachCts = new CancellationTokenSource();

        ReachAndPress(objectToTouch, _reachCts.Token).Forget();
    }

    async UniTaskVoid ReachAndPress(Transform targetPoint, CancellationToken token)
    {
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

        OnReachTarget.Invoke();

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
    }
}
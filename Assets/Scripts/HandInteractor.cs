using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class HandInteractor : MonoBehaviour
{
    [Header("IK Setup")]
    public Rig armRig; // Reference to the Rig component
    public Transform handTarget; // The target object the hand follows
    
    [Header("Settings")]
    public float reachSpeed = 5f;
    
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
        // 1. Move the IK target to the exact position of the button
        handTarget.position = targetPoint.position;
        handTarget.rotation = targetPoint.rotation; // Optional: match rotation

        // 2. Smoothly increase IK weight (Reach out)
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * reachSpeed;
            armRig.weight = Mathf.Lerp(0f, 1f, timer);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }

        // 3. The moment of the "Press"
        // Add your logic here! (e.g., play sound, turn on screen)
        Debug.Log("Button Pressed!");

        // Optional: Wait a tiny bit to simulate holding the button
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);

        // 4. Smoothly decrease IK weight (Return hand to body)
        timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * reachSpeed;
            armRig.weight = Mathf.Lerp(1f, 0f, timer);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }
    }
}
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// Manages fan rotation animation and audio.
/// Rotates the fan on the Z-axis using UniTask for smooth animation.
/// </summary>
public class FanManager : MonoBehaviour
{
    [Header("Rotor Settings")]
    [SerializeField] private Transform rotorTransform;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private bool rotateClockwise = true;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource fanAudioSource;
    [SerializeField] private AudioClip fanSound;
    [SerializeField] private bool playOnStart = true;
    [SerializeField][Range(0f, 1f)] private float volume = 0.5f;

    private CancellationTokenSource cancellationTokenSource;
    private bool isRunning;

    private void Start()
    {
        SetupAudioSource();

        if (playOnStart)
        {
            StartFan();
        }
    }

    private void SetupAudioSource()
    {
        fanAudioSource.clip = fanSound;
        fanAudioSource.loop = true;
        fanAudioSource.volume = volume;
        fanAudioSource.playOnAwake = false;
        fanAudioSource.spatialBlend = 1f;
    }

    /// <summary>
    /// Starts the fan rotation and audio
    /// </summary>
    public void StartFan()
    {
        if (isRunning) return;

        isRunning = true;
        cancellationTokenSource = new CancellationTokenSource();

        RotateFanAsync(cancellationTokenSource.Token).Forget();

        if (fanSound != null)
        {
            fanAudioSource.Play();
        }
    }

    /// <summary>
    /// Stops the fan rotation and audio
    /// </summary>
    public void StopFan()
    {
        if (!isRunning) return;

        isRunning = false;
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;

        fanAudioSource.Stop();
    }

    private async UniTaskVoid RotateFanAsync(CancellationToken cancellationToken)
    {
        float direction = rotateClockwise ? -1f : 1f;

        while (!cancellationToken.IsCancellationRequested)
        {
            float rotationAmount = rotationSpeed * Time.deltaTime * direction;
            rotorTransform.Rotate(0f, 0f, rotationAmount, Space.Self);

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }
    }

    /// <summary>
    /// Sets the rotation speed of the fan
    /// </summary>
    public void SetSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    /// <summary>
    /// Sets the volume of the fan audio
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (fanAudioSource != null)
        {
            fanAudioSource.volume = volume;
        }
    }

    /// <summary>
    /// Toggles the fan on/off
    /// </summary>
    public void ToggleFan()
    {
        if (isRunning)
        {
            StopFan();
        }
        else
        {
            StartFan();
        }
    }

    /// <summary>
    /// Returns whether the fan is currently running
    /// </summary>
    public bool IsRunning => isRunning;

    private void OnDestroy()
    {
        StopFan();
    }

    private void OnDisable()
    {
        StopFan();
    }
}

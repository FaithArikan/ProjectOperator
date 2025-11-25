using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using Random = UnityEngine.Random;

public class RandomSoundPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] soundClips;

    [Header("Timing Settings")]
    [SerializeField] private float minDelay = 5f;
    [SerializeField] private float maxDelay = 15f;

    [Header("Variation Settings")]
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;
    [SerializeField] private float minVolume = 0.8f;
    [SerializeField] private float maxVolume = 1.0f;

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        PlayRandomSoundsRoutine().Forget();
    }

    private async UniTaskVoid PlayRandomSoundsRoutine()
    {
        // Get the cancellation token that cancels when this GameObject is destroyed
        var token = this.GetCancellationTokenOnDestroy();

        while (!token.IsCancellationRequested)
        {
            // Wait for a random delay
            float delay = Random.Range(minDelay, maxDelay);

            // Wait with cancellation token support
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

            PlayRandomSound();
        }
    }

    private void PlayRandomSound()
    {
        if (audioSource == null) return;
        if (soundClips == null || soundClips.Length == 0) return;

        AudioClip clip;

        // If there is only 1 clip, play it without RNG variations
        if (soundClips.Length == 1)
        {
            clip = soundClips[0];
            audioSource.pitch = 1f;
            audioSource.volume = maxVolume;
        }
        else
        {
            // Pick a random clip
            clip = soundClips[Random.Range(0, soundClips.Length)];

            // Apply random variations
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.volume = Random.Range(minVolume, maxVolume);
        }

        // Play the sound
        audioSource.PlayOneShot(clip);
    }

    public void SetClips(AudioClip[] newClips)
    {
        soundClips = newClips;
    }
}

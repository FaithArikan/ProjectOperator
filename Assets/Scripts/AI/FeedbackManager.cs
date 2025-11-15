using UnityEngine;
using System.Collections.Generic;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Lighting mode options for the feedback system
    /// </summary>
    public enum LightingMode
    {
        Dynamic,    // Color and intensity change based on performance
        Static,     // Fixed color and intensity
        Off         // Lighting disabled
    }

    /// <summary>
    /// Manages audio and visual feedback for wave manipulation.
    /// Controls lights, VFX, audio tones, and alarm systems.
    /// </summary>
    public class FeedbackManager : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField]
        private AudioSource _toneSource;

        [SerializeField]
        private AudioSource _ambientSource;

        [SerializeField]
        private AudioClip _successClip;

        [SerializeField]
        private AudioClip _alarmClip;

        [Header("Particles")]
        [SerializeField]
        private List<ParticleSystem> _feedbackParticles = new List<ParticleSystem>();

        // Current ambient pulse intensity
        private float _currentPulseIntensity;

        // Alarm level (0..1)
        private float _alarmLevel;

        private void Awake()
        {
            // Initialize particle pools
            foreach (var ps in _feedbackParticles)
            {
                if (ps != null)
                {
                    ps.Stop();
                }
            }
        }

        /// <summary>
        /// Sets the ambient pulse intensity based on evaluation score
        /// </summary>
        /// <param name="intensity">Score from 0..1 (0 = bad, 1 = good)</param>
        public void SetAmbientPulse(float intensity)
        {
            _currentPulseIntensity = Mathf.Clamp01(intensity);
            UpdateParticles();
        }

        /// <summary>
        /// Sets the alarm level
        /// </summary>
        public void SetAlarmLevel(float level)
        {
            _alarmLevel = Mathf.Clamp01(level);
        }

        /// <summary>
        /// Plays a tone at specified frequency and amplitude
        /// </summary>
        public void PlayTone(float frequency, float amplitude)
        {
            if (_toneSource == null)
                return;

            // Simple pitch-shift approach (requires audio clip)
            _toneSource.pitch = Mathf.Clamp(frequency / 440f, 0.5f, 2f); // Normalize to A440
            _toneSource.volume = Mathf.Clamp01(amplitude);
            _toneSource.Play();
        }

        /// <summary>
        /// Triggers success feedback
        /// </summary>
        public void PlaySuccessFeedback()
        {
            if (_successClip != null && _toneSource != null)
            {
                _toneSource.PlayOneShot(_successClip);
            }

            // Burst of good particles
            TriggerParticleBurst(0.5f);
        }

        /// <summary>
        /// Triggers alarm
        /// </summary>
        public void TriggerAlarm()
        {
            if (_alarmClip != null && _ambientSource != null)
            {
                _ambientSource.PlayOneShot(_alarmClip);
            }

                        // Burst of particles
            TriggerParticleBurst(1f);
        }

        /// <summary>
        /// Plays a reaction audio clip
        /// </summary>
        public void PlayReactionClip(AudioClip clip)
        {
            if (clip != null && _toneSource != null)
            {
                _toneSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Updates particle systems based on intensity
        /// </summary>
        private void UpdateParticles()
        {
            foreach (var ps in _feedbackParticles)
            {
                if (ps == null)
                    continue;

                var emission = ps.emission;
                emission.rateOverTime = _currentPulseIntensity * 10f; // Scale rate
            }
        }

        /// <summary>
        /// Triggers a particle burst
        /// </summary>
        private void TriggerParticleBurst(float intensity)
        {
            foreach (var ps in _feedbackParticles)
            {
                if (ps != null)
                {
                    ps.Emit((int)(intensity * 20f));
                }
            }
        }
    }
}

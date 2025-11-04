using UnityEngine;
using System.Collections.Generic;

namespace NeuralWaveBureau.AI
{
    /// <summary>
    /// Manages audio and visual feedback for wave manipulation.
    /// Controls lights, VFX, audio tones, and alarm systems.
    /// </summary>
    public class FeedbackManager : MonoBehaviour
    {
        [Header("Lighting")]
        [SerializeField]
        private Light _ambientLight;

        [SerializeField]
        private Color _goodColor = Color.green;

        [SerializeField]
        private Color _neutralColor = Color.white;

        [SerializeField]
        private Color _badColor = Color.red;

        [SerializeField]
        [Range(0f, 8f)]
        private float _maxIntensity = 2f;

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
            UpdateLighting();
            UpdateParticles();
        }

        /// <summary>
        /// Sets the alarm level
        /// </summary>
        public void SetAlarmLevel(float level)
        {
            _alarmLevel = Mathf.Clamp01(level);
            UpdateLighting();
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

            // Flash red
            StartCoroutine(FlashEffect(_badColor));

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
        /// Updates lighting based on pulse intensity and alarm level
        /// </summary>
        private void UpdateLighting()
        {
            if (_ambientLight == null)
                return;

            // Blend color based on intensity (red -> white -> green)
            Color targetColor;
            if (_alarmLevel > 0.5f)
            {
                // Alarm override
                targetColor = Color.Lerp(_badColor, _neutralColor, 1f - _alarmLevel);
            }
            else if (_currentPulseIntensity >= 0.5f)
            {
                // Good zone (green)
                targetColor = Color.Lerp(_neutralColor, _goodColor, (_currentPulseIntensity - 0.5f) * 2f);
            }
            else
            {
                // Bad zone (red)
                targetColor = Color.Lerp(_badColor, _neutralColor, _currentPulseIntensity * 2f);
            }

            _ambientLight.color = targetColor;

            // Intensity maps to evaluation score
            float targetIntensity = Mathf.Lerp(0.5f, _maxIntensity, _currentPulseIntensity);
            _ambientLight.intensity = targetIntensity;
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

        /// <summary>
        /// Flash effect coroutine
        /// </summary>
        private System.Collections.IEnumerator FlashEffect(Color flashColor)
        {
            if (_ambientLight == null)
                yield break;

            Color originalColor = _ambientLight.color;
            float originalIntensity = _ambientLight.intensity;

            // Flash
            _ambientLight.color = flashColor;
            _ambientLight.intensity = _maxIntensity;

            yield return new WaitForSeconds(0.1f);

            // Restore
            _ambientLight.color = originalColor;
            _ambientLight.intensity = originalIntensity;
        }

        private void Update()
        {
            // Add subtle pulsing to ambient light
            if (_ambientLight != null && _currentPulseIntensity > 0.1f)
            {
                float pulse = Mathf.Sin(Time.time * 2f) * 0.1f * _currentPulseIntensity;
                _ambientLight.intensity += pulse;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Manages CRT monitor visual effects including scanlines, flicker, and distortion.
    /// Apply this to the main UI panel to get retro computer monitor aesthetic.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class CRTScreenEffect : MonoBehaviour
    {
        [Header("Effect Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        private float _scanlineIntensity = 0.3f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _flickerIntensity = 0.05f;

        [SerializeField]
        [Range(0f, 0.1f)]
        private float _chromaticAberration = 0.01f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _vignette = 0.3f;

        [SerializeField]
        [Range(0f, 0.05f)]
        private float _screenCurvature = 0.02f;

        [Header("Animation")]
        [SerializeField]
        private bool _enableFlicker = true;

        [SerializeField]
        private float _flickerSpeed = 10f;

        [SerializeField]
        private bool _enableScanlineScroll = true;

        [SerializeField]
        private float _scanlineScrollSpeed = 0.1f;

        [Header("Glitch Effects")]
        [SerializeField]
        private bool _enableRandomGlitches = true;

        [SerializeField]
        [Range(0f, 1f)]
        private float _glitchProbability = 0.01f;

        // Components
        private RawImage _rawImage;
        private Material _effectMaterial;

        // Runtime state
        private float _time;
        private float _scanlineOffset;
        private bool _isGlitching = false;

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
        }

        private void Update()
        {
            _time += Time.deltaTime;

            // Update scanline scroll
            if (_enableScanlineScroll)
            {
                _scanlineOffset += _scanlineScrollSpeed * Time.deltaTime;
                if (_scanlineOffset > 1f)
                {
                    _scanlineOffset -= 1f;
                }
            }

            // Apply flicker
            if (_enableFlicker)
            {
                ApplyFlicker();
            }

            // Random glitches
            if (_enableRandomGlitches && Random.value < _glitchProbability * Time.deltaTime && !_isGlitching)
            {
                TriggerGlitch();
            }

            // Update material properties if custom shader is used
            UpdateMaterialProperties();
        }

        /// <summary>
        /// Updates material shader properties
        /// </summary>
        private void UpdateMaterialProperties()
        {
            if (_effectMaterial == null)
                return;

            // These properties would be used by a custom CRT shader
            if (_effectMaterial.HasProperty("_ScanlineIntensity"))
            {
                _effectMaterial.SetFloat("_ScanlineIntensity", _scanlineIntensity);
            }

            if (_effectMaterial.HasProperty("_ScanlineOffset"))
            {
                _effectMaterial.SetFloat("_ScanlineOffset", _scanlineOffset);
            }

            if (_effectMaterial.HasProperty("_ChromaticAberration"))
            {
                _effectMaterial.SetFloat("_ChromaticAberration", _chromaticAberration);
            }

            if (_effectMaterial.HasProperty("_Vignette"))
            {
                _effectMaterial.SetFloat("_Vignette", _vignette);
            }

            if (_effectMaterial.HasProperty("_Curvature"))
            {
                _effectMaterial.SetFloat("_Curvature", _screenCurvature);
            }
        }

        /// <summary>
        /// Applies screen flicker effect
        /// </summary>
        private void ApplyFlicker()
        {
            if (_rawImage == null)
                return;

            // Subtle brightness variation
            float flicker = 1f + Mathf.Sin(_time * _flickerSpeed) * _flickerIntensity * 0.5f;
            flicker += Mathf.PerlinNoise(_time * _flickerSpeed * 2f, 0f) * _flickerIntensity * 0.5f;

            Color currentColor = _rawImage.color;
            currentColor.a = Mathf.Clamp01(flicker);
            _rawImage.color = currentColor;
        }

        /// <summary>
        /// Triggers a screen glitch effect
        /// </summary>
        public void TriggerGlitch(float duration = 0.1f, float intensity = 1f)
        {
            if (_isGlitching)
                return;

            _isGlitching = true;

            // Random horizontal offset
            RectTransform rectTransform = _rawImage.rectTransform;
            Vector2 originalPosition = rectTransform.anchoredPosition;
            float glitchOffset = Random.Range(-10f, 10f) * intensity;

            rectTransform.DOAnchorPosX(originalPosition.x + glitchOffset, duration * 0.3f)
                .SetEase(Ease.OutElastic)
                .OnComplete(() =>
                {
                    rectTransform.DOAnchorPosX(originalPosition.x, duration * 0.7f)
                        .SetEase(Ease.InOutQuad)
                        .OnComplete(() => _isGlitching = false);
                });

            // Color distortion
            Color originalColor = _rawImage.color;
            Color glitchColor = new Color(
                originalColor.r * Random.Range(0.8f, 1.2f),
                originalColor.g * Random.Range(0.8f, 1.2f),
                originalColor.b * Random.Range(0.8f, 1.2f),
                originalColor.a
            );

            _rawImage.DOColor(glitchColor, duration * 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _rawImage.DOColor(originalColor, duration * 0.5f).SetEase(Ease.InQuad);
                });
        }

        /// <summary>
        /// Power on animation
        /// </summary>
        public void PowerOn(float duration = 1.5f)
        {
            // Start with screen off
            _rawImage.color = new Color(1f, 1f, 1f, 0f);
            transform.localScale = new Vector3(1f, 0.05f, 1f);

            // Animate scale (like old CRT turning on)
            transform.DOScale(Vector3.one, duration * 0.7f).SetEase(Ease.OutBounce);

            // Fade in
            _rawImage.DOFade(1f, duration).SetEase(Ease.OutQuad);

            // Add a few glitches during power on
            DOVirtual.DelayedCall(duration * 0.3f, () => TriggerGlitch(0.2f, 2f));
            DOVirtual.DelayedCall(duration * 0.6f, () => TriggerGlitch(0.15f, 1.5f));
        }

        /// <summary>
        /// Power off animation
        /// </summary>
        public void PowerOff(float duration = 0.8f)
        {
            // Collapse to horizontal line
            transform.DOScale(new Vector3(1f, 0.05f, 1f), duration * 0.7f)
                .SetEase(Ease.InCubic);

            // Fade out
            _rawImage.DOFade(0f, duration).SetEase(Ease.InQuad);

            // Flash before turning off
            DOVirtual.DelayedCall(duration * 0.1f, () =>
            {
                _rawImage.DOFade(1.5f, 0.05f).OnComplete(() =>
                {
                    _rawImage.DOFade(0f, duration * 0.85f);
                });
            });
        }

        /// <summary>
        /// Screen static effect (for errors or critical states)
        /// </summary>
        public void ShowStatic(float duration = 0.5f, float intensity = 1f)
        {
            // Rapid flicker
            Sequence staticSequence = DOTween.Sequence();

            for (int i = 0; i < 10; i++)
            {
                float randomAlpha = Random.Range(0.5f, 1f) * intensity;
                staticSequence.Append(_rawImage.DOFade(randomAlpha, duration / 10f));
            }

            staticSequence.OnComplete(() =>
            {
                _rawImage.DOFade(1f, 0.1f);
            });
        }

        /// <summary>
        /// Sets scanline intensity
        /// </summary>
        public void SetScanlineIntensity(float intensity, bool animate = true)
        {
            if (animate)
            {
                DOTween.To(() => _scanlineIntensity, x => _scanlineIntensity = x, intensity, 0.5f);
            }
            else
            {
                _scanlineIntensity = intensity;
            }
        }

        /// <summary>
        /// Sets flicker intensity
        /// </summary>
        public void SetFlickerIntensity(float intensity, bool animate = true)
        {
            if (animate)
            {
                DOTween.To(() => _flickerIntensity, x => _flickerIntensity = x, intensity, 0.5f);
            }
            else
            {
                _flickerIntensity = intensity;
            }
        }

        private void OnDestroy()
        {
            if (_effectMaterial != null)
            {
                Destroy(_effectMaterial);
            }
        }
    }
}

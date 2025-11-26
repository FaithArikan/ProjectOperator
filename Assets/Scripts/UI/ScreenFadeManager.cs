using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Manages black screen fade transitions between game states
    /// </summary>
    public class ScreenFadeManager : MonoBehaviour
    {
        [Header("Fade Settings")]
        [SerializeField] private Image _blackScreenImage;
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private Ease _fadeEase = Ease.InOutQuad;

        [Header("Options")]
        [Tooltip("Enable/disable fade transitions")]
        public bool enableFadeTransitions = true;

        private static ScreenFadeManager _instance;
        public static ScreenFadeManager Instance => _instance;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _instance = this;

            // Get or add canvas group for fading
            if (_blackScreenImage != null)
            {
                _canvasGroup = _blackScreenImage.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = _blackScreenImage.gameObject.AddComponent<CanvasGroup>();
                }

                // Start fully transparent
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _blackScreenImage.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            DOTween.Kill(this);
        }

        /// <summary>
        /// Fades to black, executes action, then fades back in
        /// </summary>
        public void FadeTransition(Action onFadeComplete)
        {
            if (!enableFadeTransitions || _blackScreenImage == null || _canvasGroup == null)
            {
                // If transitions disabled, just execute the action immediately
                onFadeComplete?.Invoke();
                return;
            }

            DOTween.Kill(this);

            // Fade to black
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.DOFade(1f, _fadeDuration)
                .SetEase(_fadeEase)
                .SetId(this)
                .OnComplete(() =>
                {
                    // Execute the action when fully black
                    onFadeComplete?.Invoke();

                    // Fade back in after a short delay
                    DOVirtual.DelayedCall(0.1f, () =>
                    {
                        FadeIn();
                    }).SetId(this);
                });
        }

        /// <summary>
        /// Fades to black
        /// </summary>
        public void FadeOut(Action onComplete = null)
        {
            if (!enableFadeTransitions || _blackScreenImage == null || _canvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            DOTween.Kill(this);

            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.DOFade(1f, _fadeDuration)
                .SetEase(_fadeEase)
                .SetId(this)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// Fades from black to transparent
        /// </summary>
        public void FadeIn(Action onComplete = null)
        {
            if (!enableFadeTransitions || _blackScreenImage == null || _canvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            DOTween.Kill(this);

            _canvasGroup.DOFade(0f, _fadeDuration)
                .SetEase(_fadeEase)
                .SetId(this)
                .OnComplete(() =>
                {
                    _canvasGroup.blocksRaycasts = false;
                    onComplete?.Invoke();
                });
        }

        /// <summary>
        /// Instantly sets the screen to black (no animation)
        /// </summary>
        public void SetBlackInstant()
        {
            if (_canvasGroup != null)
            {
                DOTween.Kill(this);
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>
        /// Instantly sets the screen to transparent (no animation)
        /// </summary>
        public void SetTransparentInstant()
        {
            if (_canvasGroup != null)
            {
                DOTween.Kill(this);
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }
        }
    }
}

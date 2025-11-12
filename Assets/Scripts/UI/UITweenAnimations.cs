using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Centralized DOTween animation utilities for UI elements.
    /// Provides consistent animation styles across the Brain Activity Monitor UI.
    /// </summary>
    public static class UITweenAnimations
    {
        // Animation durations
        public const float FAST_DURATION = 0.2f;
        public const float NORMAL_DURATION = 0.4f;
        public const float SLOW_DURATION = 0.6f;

        /// <summary>
        /// Slides a panel in from the specified direction
        /// </summary>
        public static Tween SlideIn(RectTransform rectTransform, SlideDirection direction, float duration = NORMAL_DURATION, Ease ease = Ease.OutCubic)
        {
            Vector2 originalPosition = rectTransform.anchoredPosition;
            Vector2 startPosition = GetOffscreenPosition(rectTransform, direction);

            rectTransform.anchoredPosition = startPosition;
            return rectTransform.DOAnchorPos(originalPosition, duration).SetEase(ease);
        }

        /// <summary>
        /// Slides a panel out to the specified direction
        /// </summary>
        public static Tween SlideOut(RectTransform rectTransform, SlideDirection direction, float duration = NORMAL_DURATION, Ease ease = Ease.InCubic)
        {
            Vector2 endPosition = GetOffscreenPosition(rectTransform, direction);
            return rectTransform.DOAnchorPos(endPosition, duration).SetEase(ease);
        }

        /// <summary>
        /// Fades in a CanvasGroup
        /// </summary>
        public static Tween FadeIn(CanvasGroup canvasGroup, float duration = NORMAL_DURATION, Ease ease = Ease.OutQuad)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(true);
            return canvasGroup.DOFade(1f, duration).SetEase(ease);
        }

        /// <summary>
        /// Fades out a CanvasGroup
        /// </summary>
        public static Tween FadeOut(CanvasGroup canvasGroup, float duration = NORMAL_DURATION, Ease ease = Ease.InQuad, bool deactivateOnComplete = true)
        {
            Tween tween = canvasGroup.DOFade(0f, duration).SetEase(ease);

            if (deactivateOnComplete)
            {
                tween.OnComplete(() => canvasGroup.gameObject.SetActive(false));
            }

            return tween;
        }

        /// <summary>
        /// Scales in with bounce effect
        /// </summary>
        public static Tween ScaleInBounce(Transform transform, float duration = NORMAL_DURATION)
        {
            transform.localScale = Vector3.zero;
            return transform.DOScale(Vector3.one, duration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Scales out with shrink effect
        /// </summary>
        public static Tween ScaleOut(Transform transform, float duration = FAST_DURATION)
        {
            return transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);
        }

        /// <summary>
        /// Button press animation
        /// </summary>
        public static Sequence ButtonPress(Transform button, float pressScale = 0.9f, float duration = FAST_DURATION)
        {
            return DOTween.Sequence()
                .Append(button.DOScale(Vector3.one * pressScale, duration * 0.5f).SetEase(Ease.OutQuad))
                .Append(button.DOScale(Vector3.one, duration * 0.5f).SetEase(Ease.OutBounce));
        }

        /// <summary>
        /// Punch scale (like a heartbeat)
        /// </summary>
        public static Tween Pulse(Transform transform, float strength = 0.1f, float duration = NORMAL_DURATION)
        {
            return transform.DOPunchScale(Vector3.one * strength, duration, 1, 0.5f);
        }

        /// <summary>
        /// Shake animation for errors/alerts
        /// </summary>
        public static Tween Shake(Transform transform, float strength = 20f, float duration = FAST_DURATION)
        {
            return transform.DOShakePosition(duration, strength, 20, 90f, false, true);
        }

        /// <summary>
        /// Rotates continuously (for loading spinners)
        /// </summary>
        public static Tween SpinContinuous(Transform transform, float duration = 1f)
        {
            return transform.DORotate(new Vector3(0, 0, -360f), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        /// <summary>
        /// Color flash animation
        /// </summary>
        public static Sequence ColorFlash(Graphic graphic, Color flashColor, float duration = FAST_DURATION)
        {
            Color originalColor = graphic.color;
            return DOTween.Sequence()
                .Append(graphic.DOColor(flashColor, duration * 0.5f))
                .Append(graphic.DOColor(originalColor, duration * 0.5f));
        }

        /// <summary>
        /// Color flash for text
        /// </summary>
        public static Sequence TextColorFlash(TextMeshProUGUI text, Color flashColor, float duration = FAST_DURATION)
        {
            Color originalColor = text.color;
            return DOTween.Sequence()
                .Append(text.DOColor(flashColor, duration * 0.5f))
                .Append(text.DOColor(originalColor, duration * 0.5f));
        }

        /// <summary>
        /// Number counter animation
        /// </summary>
        public static Tween AnimateNumber(float from, float to, float duration, System.Action<float> onUpdate)
        {
            float current = from;
            return DOTween.To(() => current, x =>
            {
                current = x;
                onUpdate?.Invoke(current);
            }, to, duration).SetEase(Ease.OutCubic);
        }

        /// <summary>
        /// Typewriter text effect
        /// </summary>
        public static Tween TypewriterText(TextMeshProUGUI text, string fullText, float duration)
        {
            text.text = "";
            int totalChars = fullText.Length;
            float timePerChar = duration / totalChars;

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i <= totalChars; i++)
            {
                int currentIndex = i;
                sequence.AppendCallback(() =>
                {
                    text.text = fullText.Substring(0, currentIndex);
                });
                sequence.AppendInterval(timePerChar);
            }

            return sequence;
        }

        /// <summary>
        /// Glow effect for alerts
        /// </summary>
        public static Tween GlowLoop(Graphic graphic, Color glowColor, float duration = 1f)
        {
            Color originalColor = graphic.color;
            return DOTween.Sequence()
                .Append(graphic.DOColor(glowColor, duration * 0.5f))
                .Append(graphic.DOColor(originalColor, duration * 0.5f))
                .SetLoops(-1, LoopType.Restart);
        }

        /// <summary>
        /// Slider value animation
        /// </summary>
        public static Tween AnimateSlider(Slider slider, float targetValue, float duration = NORMAL_DURATION)
        {
            return DOTween.To(() => slider.value, x => slider.value = x, targetValue, duration)
                .SetEase(Ease.OutCubic);
        }

        /// <summary>
        /// Panel expand/collapse animation
        /// </summary>
        public static Tween ExpandVertical(RectTransform rectTransform, float targetHeight, float duration = NORMAL_DURATION)
        {
            return rectTransform.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x, targetHeight), duration)
                .SetEase(Ease.OutCubic);
        }

        /// <summary>
        /// Glitch effect (random position shifts)
        /// </summary>
        public static Sequence GlitchEffect(RectTransform rectTransform, float intensity = 10f, float duration = 0.1f)
        {
            Vector2 originalPosition = rectTransform.anchoredPosition;
            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < 5; i++)
            {
                Vector2 randomOffset = new Vector2(
                    Random.Range(-intensity, intensity),
                    Random.Range(-intensity, intensity)
                );
                sequence.Append(rectTransform.DOAnchorPos(originalPosition + randomOffset, duration / 5f));
            }

            sequence.Append(rectTransform.DOAnchorPos(originalPosition, duration / 5f));
            return sequence;
        }

        /// <summary>
        /// Critical alert animation (continuous)
        /// </summary>
        public static Sequence CriticalAlert(Transform transform, Color alertColor, Graphic graphic = null)
        {
            Sequence sequence = DOTween.Sequence();

            // Scale pulse
            sequence.Join(transform.DOPunchScale(Vector3.one * 0.15f, 0.5f, 2, 0.5f));

            // Color flash if graphic provided
            if (graphic != null)
            {
                Color originalColor = graphic.color;
                sequence.Join(DOTween.Sequence()
                    .Append(graphic.DOColor(alertColor, 0.25f))
                    .Append(graphic.DOColor(originalColor, 0.25f))
                );
            }

            sequence.SetLoops(-1, LoopType.Restart);
            return sequence;
        }

        /// <summary>
        /// Gets an offscreen position for slide animations
        /// </summary>
        private static Vector2 GetOffscreenPosition(RectTransform rectTransform, SlideDirection direction)
        {
            Vector2 current = rectTransform.anchoredPosition;
            Rect rect = rectTransform.rect;

            switch (direction)
            {
                case SlideDirection.Left:
                    return new Vector2(current.x - Screen.width, current.y);
                case SlideDirection.Right:
                    return new Vector2(current.x + Screen.width, current.y);
                case SlideDirection.Up:
                    return new Vector2(current.x, current.y + Screen.height);
                case SlideDirection.Down:
                    return new Vector2(current.x, current.y - Screen.height);
                default:
                    return current;
            }
        }
    }

    /// <summary>
    /// Direction for slide animations
    /// </summary>
    public enum SlideDirection
    {
        Left,
        Right,
        Up,
        Down
    }
}

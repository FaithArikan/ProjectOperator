using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine.Events;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Reusable typewriter effect component that displays text character by character.
    /// Can be used on any TextMeshProUGUI component for a retro terminal feel.
    /// </summary>
    public class TypewriterEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _textComponent;

        [Header("Settings")]
        [SerializeField] private float _typewriterSpeed = 0.03f;
        [SerializeField] private bool _playOnEnable = false;
        [TextArea(5, 15)]
        [SerializeField] private string _textToDisplay = "";

        [Header("Events")]
        [SerializeField] private UnityEvent _onTypewriterComplete;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isTyping = false;

        private void Awake()
        {
            // Auto-find TextMeshProUGUI if not assigned
            if (_textComponent == null)
            {
                _textComponent = GetComponent<TextMeshProUGUI>();
            }
        }

        private void OnEnable()
        {
            if (_playOnEnable && !string.IsNullOrEmpty(_textToDisplay))
            {
                PlayTypewriter(_textToDisplay).Forget();
            }
        }

        private void OnDestroy()
        {
            StopTypewriter();
        }

        /// <summary>
        /// Plays the typewriter effect with the given text.
        /// </summary>
        /// <param name="text">The text to display</param>
        public async UniTask PlayTypewriter(string text)
        {
            if (_textComponent == null)
            {
                Debug.LogWarning("TypewriterEffect: TextMeshProUGUI component is not assigned!");
                return;
            }

            // Cancel any ongoing typewriter effect
            StopTypewriter();

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                await TypewriterAsync(text, token);
                _onTypewriterComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // Effect was cancelled
            }
            finally
            {
                _isTyping = false;
            }
        }

        /// <summary>
        /// Plays the typewriter effect with custom speed.
        /// </summary>
        public async UniTask PlayTypewriter(string text, float customSpeed)
        {
            float originalSpeed = _typewriterSpeed;
            _typewriterSpeed = customSpeed;
            await PlayTypewriter(text);
            _typewriterSpeed = originalSpeed;
        }

        /// <summary>
        /// Stops the typewriter effect and displays the full text immediately.
        /// </summary>
        public void SkipToEnd()
        {
            if (_isTyping && _textComponent != null)
            {
                StopTypewriter();
                _textComponent.text = _textToDisplay;
                _onTypewriterComplete?.Invoke();
            }
        }

        /// <summary>
        /// Stops the typewriter effect.
        /// </summary>
        public void StopTypewriter()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            _isTyping = false;
        }

        /// <summary>
        /// Clears the text component.
        /// </summary>
        public void ClearText()
        {
            if (_textComponent != null)
            {
                _textComponent.text = "";
            }
        }

        /// <summary>
        /// Core typewriter logic - displays text character by character.
        /// </summary>
        private async UniTask TypewriterAsync(string fullText, CancellationToken token)
        {
            _isTyping = true;
            _textComponent.text = "";

            foreach (char c in fullText)
            {
                _textComponent.text += c;
                await UniTask.Delay(TimeSpan.FromSeconds(_typewriterSpeed), cancellationToken: token);
            }
        }

        /// <summary>
        /// Sets the text to be displayed (for use with PlayOnEnable).
        /// </summary>
        public void SetText(string text)
        {
            _textToDisplay = text;
        }

        /// <summary>
        /// Gets or sets the typewriter speed.
        /// </summary>
        public float TypewriterSpeed
        {
            get => _typewriterSpeed;
            set => _typewriterSpeed = Mathf.Max(0, value);
        }

        public bool IsTyping => _isTyping;
    }
}

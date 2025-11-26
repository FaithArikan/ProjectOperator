using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using NeuralWaveBureau.AI;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Manages the game over screen with fade-in animation and menu options.
    /// </summary>
    public class GameOverPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _panelContent;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _quitButton;

        [Header("Animation Settings")]
        [SerializeField] private float _fadeDuration = 1.5f;
        [SerializeField] private float _delayBeforeFade = 0.5f;
        [SerializeField] private Ease _fadeEase = Ease.InOutQuad;

        private static GameOverPanel _instance;

        public static GameOverPanel Instance => _instance;

        private void Awake()
        {
            _instance = this;

            // Start hidden
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            if (_panelContent != null)
            {
                _panelContent.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            // Kill any active tweens
            DOTween.Kill(this);
        }

        /// <summary>
        /// Shows the game over panel with a fade-in animation.
        /// </summary>
        public void ShowGameOver()
        {
            _canvasGroup.gameObject.SetActive(true);

            if (_panelContent != null)
            {
                _panelContent.SetActive(true);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = true;

                // Kill any existing fade tweens
                DOTween.Kill(this);

                // Fade in the panel
                _canvasGroup.DOFade(1f, _fadeDuration)
                    .SetDelay(_delayBeforeFade)
                    .SetEase(_fadeEase)
                    .SetId(this)
                    .OnComplete(() =>
                    {
                        _canvasGroup.interactable = true;
                    });
            }

            Debug.Log("Game Over panel displayed.");
        }

        /// <summary>
        /// Hides the game over panel.
        /// </summary>
        public void HideGameOver()
        {
            _canvasGroup.gameObject.SetActive(false);

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;

                DOTween.Kill(this);

                _canvasGroup.DOFade(0f, 0.5f)
                    .SetEase(Ease.OutQuad)
                    .SetId(this)
                    .OnComplete(() =>
                    {
                        if (_panelContent != null)
                        {
                            _panelContent.SetActive(false);
                        }
                    });
            }
        }

        /// <summary>
        /// Test method to show game over panel from Inspector context menu
        /// </summary>
        [ContextMenu("Test Show Game Over")]
        private void TestShowGameOver()
        {
            ShowGameOver();
        }

        /// <summary>
        /// Test method to hide game over panel from Inspector context menu
        /// </summary>
        [ContextMenu("Test Hide Game Over")]
        private void TestHideGameOver()
        {
            HideGameOver();
        }

        public void OnRetryClicked()
        {
            Debug.Log("Retry button clicked. Restarting game...");

            // Disable buttons to prevent multiple clicks
            if (_retryButton != null) _retryButton.interactable = false;
            if (_quitButton != null) _quitButton.interactable = false;

            // Reset the game state
            ResetGame();

            // Hide game over panel
            HideGameOver();

            // Re-enable buttons for next time
            if (_retryButton != null) _retryButton.interactable = true;
            if (_quitButton != null) _quitButton.interactable = true;

            // Return to menu via GameManager
            if (global::GameManager.Instance != null)
            {
                global::GameManager.Instance.ReturnToMenu();
            }
        }

        public void OnQuitClicked()
        {
            Debug.Log("Quit button clicked.");

            // Disable buttons to prevent multiple clicks
            if (_retryButton != null) _retryButton.interactable = false;
            if (_quitButton != null) _quitButton.interactable = false;

#if UNITY_EDITOR
            // If in editor, stop playing
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // If in build, quit application
            Application.Quit();
#endif
        }

        /// <summary>
        /// Resets all game systems to initial state
        /// </summary>
        private void ResetGame()
        {
            // Reset all citizens
            if (CitizenSpawner.Instance != null)
            {
                foreach (var citizen in CitizenSpawner.Instance._spawnedCitizens)
                {
                    citizen.GetComponent<CitizenController>().Reset();
                }
            }

            // Stop AI stimulation
            if (AIManager.Instance != null)
            {
                AIManager.Instance.StopStimulation();
            }

            // Reset obedience controller
            if (ObedienceController.Instance != null)
            {
                ObedienceController.Instance.SetObedience(50f); // Reset to default
            }

            Debug.Log("[GameOverPanel] Game state reset.");
        }
    }
}

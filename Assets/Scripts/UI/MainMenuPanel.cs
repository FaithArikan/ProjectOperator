using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace NeuralWaveBureau.UI
{
    /// <summary>
    /// Manages the main menu panel that appears at game start
    /// </summary>
    public class MainMenuPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _menuContent;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        [Header("Settings Panel")]
        [SerializeField] private GameObject _settingsPanel;

        [Header("Animation Settings")]
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private Ease _fadeEase = Ease.InOutQuad;

        private static MainMenuPanel _instance;
        public static MainMenuPanel Instance => _instance;

        private void Awake()
        {
            _instance = this;

            // Start hidden - menu will be shown when power button is first pressed
            HideMenu(instant: true);

            // Hide settings panel initially
            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(false);
            }

            // Add button listeners
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(OnPlayClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            // Clean up button listeners
            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(OnPlayClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveListener(OnQuitClicked);
            }

            DOTween.Kill(this);
        }

        /// <summary>
        /// Shows the main menu
        /// </summary>
        public void ShowMenu(bool instant = false)
        {
            if (_menuContent != null)
            {
                _menuContent.SetActive(true);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;

                DOTween.Kill(this);

                if (instant)
                {
                    _canvasGroup.alpha = 1f;
                    _canvasGroup.interactable = true;
                }
                else
                {
                    _canvasGroup.interactable = false;
                    _canvasGroup.DOFade(1f, _fadeDuration)
                        .SetEase(_fadeEase)
                        .SetId(this)
                        .OnComplete(() =>
                        {
                            _canvasGroup.interactable = true;
                        });
                }
            }

            Debug.Log("[MainMenuPanel] Menu shown.");
        }

        /// <summary>
        /// Hides the main menu
        /// </summary>
        public void HideMenu(bool instant = false)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;

                DOTween.Kill(this);

                if (instant)
                {
                    _canvasGroup.alpha = 0f;
                    if (_menuContent != null)
                    {
                        _menuContent.SetActive(false);
                    }
                }
                else
                {
                    _canvasGroup.DOFade(0f, _fadeDuration)
                        .SetEase(_fadeEase)
                        .SetId(this)
                        .OnComplete(() =>
                        {
                            if (_menuContent != null)
                            {
                                _menuContent.SetActive(false);
                            }
                        });
                }
            }

            Debug.Log("[MainMenuPanel] Menu hidden.");
        }

        /// <summary>
        /// Called when Play button is clicked
        /// </summary>
        private void OnPlayClicked()
        {
            Debug.Log("[MainMenuPanel] Play button clicked.");

            // Disable buttons to prevent multiple clicks
            SetButtonsInteractable(false);

            // Hide menu
            HideMenu();

            // Start the game via GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
            else
            {
                Debug.LogWarning("[MainMenuPanel] GameManager instance not found!");
            }
        }

        /// <summary>
        /// Called when Settings button is clicked
        /// </summary>
        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenuPanel] Settings button clicked.");

            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Called when Quit button is clicked
        /// </summary>
        private void OnQuitClicked()
        {
            Debug.Log("[MainMenuPanel] Quit button clicked.");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Sets interactable state for all menu buttons
        /// </summary>
        private void SetButtonsInteractable(bool interactable)
        {
            if (_playButton != null) _playButton.interactable = interactable;
            if (_settingsButton != null) _settingsButton.interactable = interactable;
            if (_quitButton != null) _quitButton.interactable = interactable;
        }

        /// <summary>
        /// Test method to show menu from context menu
        /// </summary>
        [ContextMenu("Test Show Menu")]
        private void TestShowMenu()
        {
            ShowMenu();
            SetButtonsInteractable(true);
        }

        /// <summary>
        /// Test method to hide menu from context menu
        /// </summary>
        [ContextMenu("Test Hide Menu")]
        private void TestHideMenu()
        {
            HideMenu();
        }
    }
}

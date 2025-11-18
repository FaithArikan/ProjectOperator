using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;

/// <summary>
/// Controls the initial menu scene and handles starting the game
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startGameButton;

    public GameObject menuSceneEventSystem;

    [SerializeField] private GameObject loadingPanel;
    
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private CanvasGroup loadingPanelCanvasGroup;
    [SerializeField] private LoadingAnimator loadingAnimator;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private void Start()
    {
        loadingPanelCanvasGroup.alpha = 0f;

        loadingPanel.SetActive(false);
    }

    public void StartGame()
    {
        OnStartGameClicked().Forget();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Called when the start game button is clicked
    /// </summary>
    private async UniTaskVoid OnStartGameClicked()
    {
        // Disable button to prevent multiple clicks
        startGameButton.interactable = false;

        // Show loading panel and fade in
        loadingPanel.SetActive(true);

        loadingPanelCanvasGroup.alpha = 0f;
        loadingPanelCanvasGroup.DOFade(1f, fadeInDuration);

        loadingAnimator.StartAnimation();

        // Wait for SceneController to be ready
        if (SceneController.Instance == null)
        {
            Debug.LogError("SceneController instance not found!");
            return;
        }

        // Track loading start time
        float startTime = Time.time;
        float minimumLoadingTime = 3.0f;

        await UniTask.Delay(System.TimeSpan.FromSeconds(fadeInDuration));
        mainMenuPanel.SetActive(false);

        // Load game scene additively (but don't unload menu yet)
        await SceneController.Instance.LoadGameSceneAsync();
        menuSceneEventSystem.SetActive(false);

        // Calculate elapsed time
        float elapsedTime = Time.time - startTime;

        // If loading took less than 3 seconds, wait for the remaining time
        if (elapsedTime < minimumLoadingTime)
        {
            float remainingTime = minimumLoadingTime - elapsedTime;
            await UniTask.Delay(System.TimeSpan.FromSeconds(remainingTime));
        }
        
        await loadingPanelCanvasGroup.DOFade(0f, fadeOutDuration).AsyncWaitForCompletion();

        // Now unload the menu scene (this will destroy the loading panel)
        await SceneController.Instance.UnloadSceneAsync("MenuScene");
    }
}

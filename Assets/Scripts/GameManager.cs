using UnityEngine;
using NeuralWaveBureau.UI;
using NeuralWaveBureau.AI;
using UnityEngine.Experimental.GlobalIllumination;

/// <summary>
/// Central game manager that coordinates game states, menu flow, and game over logic
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private GameState _currentState = GameState.Menu;

    private static GameManager _instance;
    public static GameManager Instance => _instance;

    public GameState CurrentState => _currentState;

    // Track if the game has ever been started this session
    private bool _hasGameStarted = false;
    public bool HasGameStarted => _hasGameStarted;

    public Light directionalLight;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        // Start in menu state but don't show the menu yet
        // Menu will be shown when player first presses the power button
        _currentState = GameState.Menu;

        // Ensure menu is hidden at start
        if (MainMenuPanel.Instance != null)
        {
            MainMenuPanel.Instance.HideMenu(instant: true);
        }
    }

    /// <summary>
    /// Starts the game from menu
    /// </summary>
    public void StartGame()
    {
        Debug.Log("[GameManager] Starting game...");
        _hasGameStarted = true;
        SetGameState(GameState.Playing);
    }

    /// <summary>
    /// Triggers game over
    /// </summary>
    public void TriggerGameOver()
    {
        if (_currentState == GameState.GameOver)
            return;

        Debug.Log("[GameManager] Game Over!");
        SetGameState(GameState.GameOver);
    }

    /// <summary>
    /// Returns to main menu
    /// </summary>
    public void ReturnToMenu()
    {
        Debug.Log("[GameManager] Returning to menu...");
        SetGameState(GameState.Menu);
    }

    /// <summary>
    /// Sets the current game state and updates UI accordingly
    /// </summary>
    private void SetGameState(GameState newState)
    {
        if (_currentState == newState)
            return;

        GameState previousState = _currentState;
        _currentState = newState;

        Debug.Log($"[GameManager] State changed: {previousState} -> {newState}");

        // Handle state transitions
        switch (newState)
        {
            case GameState.Menu:
                OnEnterMenuState();
                break;

            case GameState.Playing:
                OnEnterPlayingState();
                break;

            case GameState.GameOver:
                OnEnterGameOverState();
                break;
        }
    }

    private void OnEnterMenuState()
    {
        // Use fade transition if available and enabled
        if (ScreenFadeManager.Instance != null && ScreenFadeManager.Instance.enableFadeTransitions)
        {
            ScreenFadeManager.Instance.FadeTransition(() =>
            {
                // This executes when screen is fully black
                PerformMenuStateTransition();
            });
        }
        else
        {
            // No fade - instant transition
            PerformMenuStateTransition();
        }
    }

    private void PerformMenuStateTransition()
    {
        // Show main menu
        if (MainMenuPanel.Instance != null)
        {
            MainMenuPanel.Instance.ShowMenu();
        }

        // Hide game over panel
        if (GameOverPanel.Instance != null)
        {
            GameOverPanel.Instance.HideGameOver();
        }

        // Hide/turn off brain monitor
        if (BrainActivityMonitor.Instance != null)
        {
            BrainActivityMonitor.Instance.PowerOff();
        }

        // Stop AI stimulation
        if (AIManager.Instance != null)
        {
            AIManager.Instance.StopStimulation();
        }
    }

    private void OnEnterPlayingState()
    {
        // Use fade transition if available and enabled
        if (ScreenFadeManager.Instance != null && ScreenFadeManager.Instance.enableFadeTransitions)
        {
            ScreenFadeManager.Instance.FadeTransition(() =>
            {
                // This executes when screen is fully black
                PerformPlayingStateTransition();
            });
        }
        else
        {
            // No fade - instant transition
            PerformPlayingStateTransition();
        }
    }

    private void PerformPlayingStateTransition()
    {
        // Hide main menu
        if (MainMenuPanel.Instance != null)
        {
            MainMenuPanel.Instance.HideMenu(instant: true);
        }

        // Hide game over panel
        if (GameOverPanel.Instance != null)
        {
            GameOverPanel.Instance.HideGameOver();
        }

        // Power on the brain monitor when starting the game
        if (BrainActivityMonitor.Instance != null)
        {
            BrainActivityMonitor.Instance.PowerOn();
        }
    }

    private void OnEnterGameOverState()
    {
        // Hide main menu (should already be hidden)
        if (MainMenuPanel.Instance != null)
        {
            MainMenuPanel.Instance.HideMenu(instant: true);
        }

        // Show game over panel
        if (GameOverPanel.Instance != null)
        {
            GameOverPanel.Instance.ShowGameOver();
        }

        // Stop AI stimulation
        if (AIManager.Instance != null)
        {
            AIManager.Instance.StopStimulation();
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}

/// <summary>
/// Represents the current state of the game
/// </summary>
public enum GameState
{
    Menu,
    Playing,
    GameOver
}

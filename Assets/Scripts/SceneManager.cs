using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// Manages scene loading and transitions using additive loading for seamless scene management
/// </summary>
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private string gameSceneName = "GameScene";

    public event Action<string> OnSceneLoadStarted;
    public event Action<string> OnSceneLoadCompleted;
    public event Action<string> OnSceneUnloadStarted;
    public event Action<string> OnSceneUnloadCompleted;

    private void Awake()
    {
        Instance = this;
    }
    
    /// <summary>
    /// Loads the game scene additively
    /// </summary>
    public async UniTask LoadGameSceneAsync()
    {
        await LoadSceneAdditiveAsync(gameSceneName);
    }

    /// <summary>
    /// Loads a scene additively
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public async UniTask LoadSceneAdditiveAsync(string sceneName)
    {
        // Check if scene is already loaded
        if (IsSceneLoaded(sceneName))
        {
            Debug.LogWarning($"Scene '{sceneName}' is already loaded!");
            return;
        }

        OnSceneLoadStarted?.Invoke(sceneName);

        // Load scene additively
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            await UniTask.Yield();
        }

        // Set the newly loaded scene as active
        Scene loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
        if (loadedScene.IsValid())
        {
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(loadedScene);
        }

        OnSceneLoadCompleted?.Invoke(sceneName);
    }

    /// <summary>
    /// Unloads a scene
    /// </summary>
    /// <param name="sceneName">Name of the scene to unload</param>
    public async UniTask UnloadSceneAsync(string sceneName)
    {
        // Check if scene is loaded
        if (!IsSceneLoaded(sceneName))
        {
            Debug.LogWarning($"Scene '{sceneName}' is not loaded, cannot unload!");
            return;
        }

        OnSceneUnloadStarted?.Invoke(sceneName);

        // Unload scene
        AsyncOperation asyncUnload = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);

        // Wait until the scene is fully unloaded
        while (!asyncUnload.isDone)
        {
            await UniTask.Yield();
        }

        OnSceneUnloadCompleted?.Invoke(sceneName);
    }

    /// <summary>
    /// Loads the game scene and unloads the menu scene
    /// </summary>
    public async UniTask TransitionToGameSceneAsync()
    {
        // Load game scene first
        await LoadSceneAdditiveAsync(gameSceneName);

        // Then unload menu scene
        await UnloadSceneAsync(menuSceneName);
    }

    /// <summary>
    /// Loads the menu scene and unloads the game scene
    /// </summary>
    public async UniTask TransitionToMenuSceneAsync()
    {
        // Load menu scene first
        await LoadSceneAdditiveAsync(menuSceneName);

        // Then unload game scene
        await UnloadSceneAsync(gameSceneName);
    }

    /// <summary>
    /// Checks if a scene is currently loaded
    /// </summary>
    /// <param name="sceneName">Name of the scene to check</param>
    /// <returns>True if scene is loaded, false otherwise</returns>
    public bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the currently active scene name
    /// </summary>
    public string GetActiveSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}
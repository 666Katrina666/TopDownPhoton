using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using Zenject;
using Core.Base;

/// <summary>
/// Сервис для управления сценами
/// </summary>
public class SceneService : LoggableMonoBehaviour, ISceneService
{
    
    public string CurrentSceneName => SceneManager.GetActiveScene().name;
    
    private void Awake()
    {
        Log($"Awake. Current scene: {CurrentSceneName}");
    }
    public void LoadScene(string sceneName)
    {
        if (IsSceneLoaded(sceneName))
        {
            Log($"Already in scene {sceneName}, skipping load");
            return;
        }
        
        Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
    
    public async Task LoadSceneAsync(string sceneName)
    {
        if (IsSceneLoaded(sceneName))
        {
            Log($"Already in scene {sceneName}, skipping load");
            return;
        }
        
        Log($"Loading scene async: {sceneName}");
        var operation = SceneManager.LoadSceneAsync(sceneName);
        
        while (!operation.isDone)
        {
            await Task.Yield();
        }
        
        Log($"Scene {sceneName} loaded successfully");
    }
    
    public bool IsSceneLoaded(string sceneName)
    {
        return CurrentSceneName == sceneName;
    }
    
    public void SetCurrentScene(string sceneName)
    {
        Log($"Setting current scene to: {sceneName}");
    }
} 
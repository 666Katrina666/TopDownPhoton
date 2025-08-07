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
    
    public void LoadScene(string sceneName)
    {
        if (IsSceneLoaded(sceneName))
        {
            return;
        }
        
        SceneManager.LoadScene(sceneName);
    }
    
    public async Task LoadSceneAsync(string sceneName)
    {
        if (IsSceneLoaded(sceneName))
        {
            return;
        }
        
        var operation = SceneManager.LoadSceneAsync(sceneName);
        
        while (!operation.isDone)
        {
            await Task.Yield();
        }
    }
    
    public bool IsSceneLoaded(string sceneName)
    {
        return CurrentSceneName == sceneName;
    }
    
    public void SetCurrentScene(string sceneName)
    {
    }
} 
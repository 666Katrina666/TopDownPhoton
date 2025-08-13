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
    #region Properties
    public string CurrentSceneName => SceneManager.GetActiveScene().name;
    #endregion
    
    #region API
    /// <summary>
    /// Загружает синхронно указанную сцену, если она ещё не активна
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (IsSceneLoaded(sceneName))
        {
            return;
        }
        
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// Загружает асинхронно указанную сцену, если она ещё не активна
    /// </summary>
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
    
    /// <summary>
    /// Проверяет, активна ли указанная сцена
    /// </summary>
    public bool IsSceneLoaded(string sceneName)
    {
        return CurrentSceneName == sceneName;
    }
    
    /// <summary>
    /// Устанавливает текущее имя сцены (точка расширения)
    /// </summary>
    public void SetCurrentScene(string sceneName)
    {
    }
    #endregion
} 
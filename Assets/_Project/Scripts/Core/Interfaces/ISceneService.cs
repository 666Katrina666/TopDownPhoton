using System.Threading.Tasks;

/// <summary>
/// Интерфейс для управления сценами
/// </summary>
public interface ISceneService
{
    string CurrentSceneName { get; }
    
    void LoadScene(string sceneName);
    Task LoadSceneAsync(string sceneName);
    bool IsSceneLoaded(string sceneName);
    void SetCurrentScene(string sceneName);
} 
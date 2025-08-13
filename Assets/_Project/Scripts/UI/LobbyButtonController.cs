using UnityEngine;
using Sirenix.OdinInspector;
using Zenject;
using Core.Base;

/// <summary>
/// Управляет кнопками лобби через EventBus
/// </summary>
public class LobbyButtonController : LoggableMonoBehaviour
{
    #region Runtime Data
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private GameConfig _gameConfig;
    #endregion
    
    #region Dependencies
    [Inject] private NetworkController _networkController;
    #endregion
    
    #region Unity Callbacks
    private void Start()
    {
        _gameConfig = ConfigManager.GameConfig;
        
        if (_gameConfig == null)
        {
            LogWarning("GameConfig not found!");
        }
    }
    #endregion
    
    #region Commands
    /// <summary>
    /// Начать игру - переход в игровую сцену
    /// </summary>
    public void StartGame()
    {
        string gameSceneName = _gameConfig?.GameSceneName ?? "GameScene";
        EventBus.RaiseEvent(new GameStartedEvent(gameSceneName));
    }
    
    /// <summary>
    /// Вернуться в главное меню
    /// </summary>
    public void ReturnToMainMenu()
    {
        string mainMenuSceneName = _gameConfig?.MainMenuSceneName ?? "MainMenuScene";
        if (_networkController != null)
        {
            _networkController.LoadScene(mainMenuSceneName);
        }
        else
        {
            LogError("NetworkController not found!");
        }
    }
    
    /// <summary>
    /// Обновить список игроков в лобби
    /// </summary>
    public void RefreshPlayerList()
    {
        EventBus.RaiseEvent(new RefreshLobbyPlayerListEvent());
    }
    
    /// <summary>
    /// Покинуть лобби
    /// </summary>
    public void LeaveLobby()
    {
        EventBus.RaiseEvent(new LeaveLobbyEvent());
    }
    #endregion
} 
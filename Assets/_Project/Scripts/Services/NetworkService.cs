using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using Zenject;
using Core.Base;

/// <summary>
/// Сервис для работы с сетевыми компонентами
/// </summary>
public class NetworkService : LoggableMonoBehaviour, INetworkService
{
    #region Runtime Data
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkRunner _networkRunner;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private string _currentRoomName = "";
    #endregion

    #region Properties
    public NetworkRunner NetworkRunner => _networkRunner;
    public bool IsConnected => _networkRunner != null && _networkRunner.IsRunning;
    public bool IsServer => _networkRunner != null && _networkRunner.IsServer;
    public bool IsClient => _networkRunner != null && _networkRunner.IsClient;
    public PlayerRef LocalPlayer => _networkRunner?.LocalPlayer ?? default;
    public string CurrentRoomName => _currentRoomName;
    #endregion

    #region API
    /// <summary>
    /// Подключается к сети (точка расширения под конкретную реализацию)
    /// </summary>
    public void Connect()
    {
    }
    
    /// <summary>
    /// Подключается к лобби с указанным именем комнаты
    /// </summary>
    /// <param name="roomName">Имя комнаты (опционально)</param>
    public void ConnectToLobby(string roomName = "")
    {
        _currentRoomName = roomName;
    }
    
    /// <summary>
    /// Отключается от сети и очищает текущее имя комнаты
    /// </summary>
    public void Disconnect()
    {
        if (_networkRunner != null)
        {
            _networkRunner.Shutdown();
        }
        _currentRoomName = "";
    }
    
    /// <summary>
    /// Полное завершение работы сетевого сервиса
    /// </summary>
    public void Shutdown()
    {
        if (_networkRunner != null)
        {
            _networkRunner.Shutdown();
        }
        _currentRoomName = "";
    }
    
    /// <summary>
    /// Загружает сцену через NetworkRunner, если он запущен, иначе через SceneManager
    /// </summary>
    /// <param name="sceneName">Имя сцены</param>
    public void LoadScene(string sceneName)
    {
        // Проверяем, что сцена не пустая
        if (string.IsNullOrEmpty(sceneName))
        {
            LogError("Scene name is null or empty!");
            return;
        }
        
        // Проверяем, не загружена ли уже эта сцена
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == sceneName)
        {
            return;
        }
        
        // Проверяем, что NetworkRunner существует, работает и не находится в процессе завершения
        if (_networkRunner != null && _networkRunner.IsRunning && !_networkRunner.IsShutdown)
        {
            _networkRunner.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
    #endregion

    #region Dependencies
    [Inject]
    private void Construct(NetworkRunner networkRunner)
    {
        _networkRunner = networkRunner;
    }
    #endregion
} 
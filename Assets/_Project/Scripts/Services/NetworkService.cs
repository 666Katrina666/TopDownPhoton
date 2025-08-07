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
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkRunner _networkRunner;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private string _currentRoomName = "";
    
    public NetworkRunner NetworkRunner => _networkRunner;
    public bool IsConnected => _networkRunner != null && _networkRunner.IsRunning;
    public bool IsServer => _networkRunner != null && _networkRunner.IsServer;
    public bool IsClient => _networkRunner != null && _networkRunner.IsClient;
    public PlayerRef LocalPlayer => _networkRunner?.LocalPlayer ?? default;
    public string CurrentRoomName => _currentRoomName;
    
    public void Connect()
    {
    }
    
    public void ConnectToLobby(string roomName = "")
    {
        _currentRoomName = roomName;
    }
    
    public void Disconnect()
    {
        if (_networkRunner != null)
        {
            _networkRunner.Shutdown();
        }
        _currentRoomName = "";
    }
    
    public void Shutdown()
    {
        if (_networkRunner != null)
        {
            _networkRunner.Shutdown();
        }
        _currentRoomName = "";
    }
    
    public void LoadScene(string sceneName)
    {
        // Проверяем, что сцена не пустая
        if (string.IsNullOrEmpty(sceneName))
        {
            LogError("NetworkService - Scene name is null or empty!");
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
    
    [Inject]
    private void Construct(NetworkRunner networkRunner)
    {
        _networkRunner = networkRunner;
    }
} 
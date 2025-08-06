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
    
    private void Awake()
    {
        Log("Awake");
    }
    
    private void OnDestroy()
    {
        Log("OnDestroy");
    }
    public void Connect()
    {
        Log("Connect called");
    }
    
    public void ConnectToLobby(string roomName = "")
    {
        Log($"Connecting to lobby: {roomName}");
        _currentRoomName = roomName;
    }
    
    public void Disconnect()
    {
        if (_networkRunner != null)
        {
            Log("Disconnecting from network");
            _networkRunner.Shutdown();
        }
        _currentRoomName = "";
    }
    
    public void Shutdown()
    {
        if (_networkRunner != null)
        {
            Log("Shutting down network");
            _networkRunner.Shutdown();
        }
        _currentRoomName = "";
    }
    
    public void LoadScene(string sceneName)
    {
        Log($"NetworkService - LoadScene called with: {sceneName}");
        
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
            Log($"NetworkService - Scene '{sceneName}' is already loaded, skipping load");
            return;
        }
        
        // Проверяем, что NetworkRunner существует, работает и не находится в процессе завершения
        if (_networkRunner != null && _networkRunner.IsRunning && !_networkRunner.IsShutdown)
        {
            Log($"NetworkService - Loading scene via NetworkRunner: {sceneName}");
            _networkRunner.LoadScene(sceneName);
        }
        else
        {
            Log($"NetworkService - NetworkRunner unavailable (null: {_networkRunner == null}, running: {_networkRunner?.IsRunning}, shutdown: {_networkRunner?.IsShutdown}), loading scene via SceneManager: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
    }
    
    [Inject]
    private void Construct(NetworkRunner networkRunner)
    {
        _networkRunner = networkRunner;
        Log($"NetworkRunner injected: {_networkRunner}");
    }
} 
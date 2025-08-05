using Fusion;
using UnityEngine;
using Sirenix.OdinInspector;
using Zenject;
using Core.Base;

/// <summary>
/// Сервис для работы с сетевыми компонентами
/// </summary>
public class NetworkService : LoggableMonoBehaviour, INetworkService
{
    
    [Title("Runtime Data")]
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkRunner _networkRunner;
    
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
    
    [Inject]
    private void Construct(NetworkRunner networkRunner)
    {
        _networkRunner = networkRunner;
        Log($"NetworkRunner injected: {_networkRunner}");
    }
} 
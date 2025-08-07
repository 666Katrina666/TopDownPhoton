using Fusion;
using Fusion.Sockets;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using Zenject;

/// <summary>
/// Обработчик сетевых колбэков для управления переходами между сценами
/// </summary>
public class NetworkCallbackHandler : NetworkCallbackBase
{
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkController _networkController;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkRunner _networkRunner;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkConnectionManager _connectionManager;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkInputHandler _inputHandler;
    
    [Inject] private NetworkController _injectedNetworkController;
    [Inject] private NetworkRunner _injectedNetworkRunner;
    [Inject] private NetworkConnectionManager _injectedConnectionManager;
    [InjectOptional] private NetworkInputHandler _injectedInputHandler;
    [Inject] private DiContainer _container;
    
    private void Start()
    {
        _networkController = _injectedNetworkController;
        _networkRunner = _injectedNetworkRunner;
        _connectionManager = _injectedConnectionManager;
        _inputHandler = _injectedInputHandler;
        
        // Попытка получить зависимости из контейнера, если они не инжектированы
        if (_networkController == null && _container != null)
        {
            LogWarning("NetworkController is null, attempting to resolve from container");
            _networkController = _container.Resolve<NetworkController>();
        }
        
        if (_networkRunner == null && _container != null)
        {
            LogWarning("NetworkRunner is null, attempting to resolve from container");
            _networkRunner = _container.Resolve<NetworkRunner>();
        }
        
        if (_connectionManager == null && _container != null)
        {
            LogWarning("ConnectionManager is null, attempting to resolve from container");
            _connectionManager = _container.Resolve<NetworkConnectionManager>();
        }
        
        // NetworkInputHandler может отсутствовать на MainMenuScene, это нормально
        if (_inputHandler == null && _container != null)
        {
            Log("NetworkInputHandler not found on scene, this is normal for MainMenuScene");
        }
        
        if (_networkRunner != null)
        {
            SetupNetworkRunner(_networkRunner);
        }
    }
    
    private void SetupNetworkRunner(NetworkRunner networkRunner)
    {
        if (_networkRunner != networkRunner)
        {
            _networkRunner = networkRunner;
            _networkRunner.AddCallbacks(this);
        }
    }
    
    public override void OnConnectedToServer(NetworkRunner runner)
    {
        EventBus.RaiseEvent(new ConnectionStateChangedEvent(ConnectionState.Connected, "Connected to server"));
    }
    
    public override void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        LogWarning($"Connection failed: {reason}. RemoteAddress: {remoteAddress}");
        
        EventBus.RaiseEvent(new ConnectionErrorEvent($"Connection failed: {reason}"));
        EventBus.RaiseEvent(new ConnectionStateChangedEvent(ConnectionState.Error, $"Connection failed: {reason}"));
        
        _networkController?.LoadScene("MainMenuScene");
    }
    
    public override void OnDisconnectedFromServer(NetworkRunner runner)
    {
        EventBus.RaiseEvent(new ConnectionStateChangedEvent(ConnectionState.Disconnected, "Disconnected from server"));
        
        _networkController?.LoadScene("MainMenuScene");
    }
    
    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        EventBus.RaiseEvent(new PlayerJoinedEvent(player, player == runner.LocalPlayer));
        
        if (player == runner.LocalPlayer && runner.IsServer)
        {
            _networkController?.LoadScene("LobbyScene");
        }
    }
    
    public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
    {
        EventBus.RaiseEvent(new ConnectionStateChangedEvent(ConnectionState.Disconnected, $"Shutdown: {shutdownReason}"));
        
        _networkController?.LoadScene("MainMenuScene");
    }
    
    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        EventBus.RaiseEvent(new PlayerLeftEvent(player, player == runner.LocalPlayer));
    }
    
    public override void OnSceneLoadStart(NetworkRunner runner)
    {
        EventBus.RaiseEvent(new SceneLoadStartEvent());
    }
    
    public override void OnSceneLoadDone(NetworkRunner runner) 
    {
        EventBus.RaiseEvent(new SceneLoadDoneEvent());
        
        // Генерируем событие готовности сети для координации спавна игроков
        EventBus.RaiseEvent(new NetworkReadyEvent(runner, runner.IsServer));
    }
    
    public override void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // NetworkInputHandler теперь сам обрабатывает ввод через свой OnInput callback
        // Здесь оставляем пустую реализацию, чтобы избежать дублирования
        Log("NetworkCallbackHandler OnInput called - input handled by NetworkInputHandler");
    }
} 
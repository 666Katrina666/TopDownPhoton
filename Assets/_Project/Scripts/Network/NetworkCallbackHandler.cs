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
    
    [Inject] private NetworkController _injectedNetworkController;
    [Inject] private NetworkRunner _injectedNetworkRunner;
    [Inject] private NetworkConnectionManager _injectedConnectionManager;
    [Inject] private DiContainer _container;
    
    private void Awake()
    {
        Log("Awake");
    }
    
    private void OnDestroy()
    {
        Log("OnDestroy");
    }
    
    private void Start()
    {
        _networkController = _injectedNetworkController;
        _networkRunner = _injectedNetworkRunner;
        _connectionManager = _injectedConnectionManager;
        
        Log($"Dependencies injected: NetworkController={_networkController}, NetworkRunner={_networkRunner}, ConnectionManager={_connectionManager}");
        
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
            
            Log($"NetworkRunner setup complete: {_networkRunner}");
        }
    }
    public override void OnConnectedToServer(NetworkRunner runner)
    {
        Log($"Connected to server. Runner: {runner}, IsServer: {runner.IsServer}, IsClient: {runner.IsClient}, LocalPlayer: {runner.LocalPlayer}");
        
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
        Log($"Disconnected from server. Runner: {runner}");
        
        EventBus.RaiseEvent(new ConnectionStateChangedEvent(ConnectionState.Disconnected, "Disconnected from server"));
        
        _networkController?.LoadScene("MainMenuScene");
    }
    
    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        Log($"Player joined. Player: {player}, IsLocalPlayer: {player == runner.LocalPlayer}, IsServer: {runner.IsServer}");
        
        EventBus.RaiseEvent(new PlayerJoinedEvent(player, player == runner.LocalPlayer));
        
        if (player == runner.LocalPlayer && runner.IsServer)
        {
            _networkController?.LoadScene("LobbyScene");
        }
    }
    
    public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
    {
        Log($"Shutdown reason: {shutdownReason}. Runner: {runner}");
        
        EventBus.RaiseEvent(new ConnectionStateChangedEvent(ConnectionState.Disconnected, $"Shutdown: {shutdownReason}"));
        
        _networkController?.LoadScene("MainMenuScene");
    }
    
    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        Log($"Player left. Player: {player}, IsLocalPlayer: {player == runner.LocalPlayer}");
        
        EventBus.RaiseEvent(new PlayerLeftEvent(player, player == runner.LocalPlayer));
    }
    
    public override void OnSceneLoadStart(NetworkRunner runner)
    {
        Log($"Scene load start. Runner: {runner}");
        
        EventBus.RaiseEvent(new SceneLoadStartEvent());
    }
    
    public override void OnSceneLoadDone(NetworkRunner runner) 
    {
        Log($"Scene load done. Runner: {runner}, IsServer: {runner.IsServer}, ActivePlayers: {runner.ActivePlayers.Count()}");
        
        EventBus.RaiseEvent(new SceneLoadDoneEvent());
    }
} 
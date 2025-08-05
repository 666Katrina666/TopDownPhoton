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
    private const string DEBUG_PREFIX = "[NetworkCallbackHandler]";
    
    [Title("Runtime Data")]
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkController _networkController;
    
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkRunner _networkRunner;
    
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkConnectionManager _connectionManager;
    
    [Inject] private NetworkController _injectedNetworkController;
    [Inject] private NetworkRunner _injectedNetworkRunner;
    [Inject] private NetworkConnectionManager _injectedConnectionManager;
    
    private void Awake()
    {
        Debug.Log($"{DEBUG_PREFIX} - Awake");
    }
    
    private void OnDestroy()
    {
        Debug.Log($"{DEBUG_PREFIX} - OnDestroy");
    }
    
    private void Start()
    {
        _networkController = _injectedNetworkController;
        _networkRunner = _injectedNetworkRunner;
        _connectionManager = _injectedConnectionManager;
        
        Debug.Log($"{DEBUG_PREFIX} - Dependencies injected: NetworkController={_networkController}, NetworkRunner={_networkRunner}, ConnectionManager={_connectionManager}");
        
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
            
            Debug.Log($"{DEBUG_PREFIX} - NetworkRunner setup complete: {_networkRunner}");
        }
    }
    public override void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log($"{DEBUG_PREFIX} - Connected to server. Runner: {runner}, IsServer: {runner.IsServer}, IsClient: {runner.IsClient}, LocalPlayer: {runner.LocalPlayer}");
        
        EventBus.RaiseEvent(new ConnectionStateChangedEvent(ConnectionState.Connected, "Connected to server"));
    }
    
    public override void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogWarning($"{DEBUG_PREFIX} - Connection failed: {reason}. RemoteAddress: {remoteAddress}");
        
        EventBus.RaiseEvent(new ConnectionErrorEvent($"Connection failed: {reason}"));
        EventBus.RaiseEvent(new ConnectionStateChangedEvent(ConnectionState.Error, $"Connection failed: {reason}"));
        
        _networkController?.LoadScene("MainMenuScene");
    }
    
    public override void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log($"{DEBUG_PREFIX} - Disconnected from server. Runner: {runner}");
        
        EventBus.RaiseEvent(new ConnectionStateChangedEvent(ConnectionState.Disconnected, "Disconnected from server"));
        
        _networkController?.LoadScene("MainMenuScene");
    }
    
    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        Debug.Log($"{DEBUG_PREFIX} - Player joined. Player: {player}, IsLocalPlayer: {player == runner.LocalPlayer}, IsServer: {runner.IsServer}");
        
        EventBus.RaiseEvent(new PlayerJoinedEvent(player, player == runner.LocalPlayer));
        
        if (player == runner.LocalPlayer && runner.IsServer)
        {
            _networkController?.LoadScene("LobbyScene");
        }
    }
    
    public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
    {
        Debug.Log($"{DEBUG_PREFIX} - Shutdown reason: {shutdownReason}. Runner: {runner}");
        
        EventBus.RaiseEvent(new ConnectionStateChangedEvent(ConnectionState.Disconnected, $"Shutdown: {shutdownReason}"));
        
        _networkController?.LoadScene("MainMenuScene");
    }
    
    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        Debug.Log($"{DEBUG_PREFIX} - Player left. Player: {player}, IsLocalPlayer: {player == runner.LocalPlayer}");
        
        EventBus.RaiseEvent(new PlayerLeftEvent(player, player == runner.LocalPlayer));
    }
    
    public override void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log($"{DEBUG_PREFIX} - Scene load start. Runner: {runner}");
        
        EventBus.RaiseEvent(new SceneLoadStartEvent());
    }
    
    public override void OnSceneLoadDone(NetworkRunner runner) 
    {
        Debug.Log($"{DEBUG_PREFIX} - Scene load done. Runner: {runner}, IsServer: {runner.IsServer}, ActivePlayers: {runner.ActivePlayers.Count()}");
        
        EventBus.RaiseEvent(new SceneLoadDoneEvent());
    }
} 
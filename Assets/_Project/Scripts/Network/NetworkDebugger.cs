using Fusion;
using Fusion.Sockets;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using Zenject;

/// <summary>
/// Отладочный компонент для диагностики проблем с сетью
/// </summary>
public class NetworkDebugger : NetworkCallbackBase, IDebugService
{
    private const string DEBUG_PREFIX = "[NetworkDebugger]";
    
    [Title("Runtime Data")]
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkRunner _networkRunner;
    
    private GameConfig _gameConfig;
    
    [Inject] private NetworkRunner _injectedNetworkRunner;
    
    private void Awake()
    {
        Debug.Log($"{DEBUG_PREFIX} - Awake");
    }
    
    [Inject]
    private void PostInject()
    {
        Debug.Log($"{DEBUG_PREFIX} - PostInject - NetworkRunner injected: {_injectedNetworkRunner}");
    }
    
    private void OnDestroy()
    {
        if (_networkRunner != null)
        {
            _networkRunner.RemoveCallbacks(this);
            Debug.Log($"{DEBUG_PREFIX} - Callbacks removed from NetworkRunner");
        }
    }
    public void LogNetworkState()
    {
        if (_networkRunner != null)
        {
            Debug.Log($"{DEBUG_PREFIX} - Network State: Runner={_networkRunner}, IsServer={_networkRunner.IsServer}, IsClient={_networkRunner.IsClient}, LocalPlayer={_networkRunner.LocalPlayer}, ActivePlayers={_networkRunner.ActivePlayers.Count()}");
            
            foreach (var player in _networkRunner.ActivePlayers)
            {
                LogPlayerInfo(player);
            }
        }
        else
        {
            Debug.LogWarning($"{DEBUG_PREFIX} - NetworkRunner is null!");
        }
    }
    
    public void LogPlayerInfo(PlayerRef player)
    {
        if (_networkRunner != null)
        {
            Debug.Log($"{DEBUG_PREFIX} - Active Player: {player}, IsLocalPlayer: {player == _networkRunner.LocalPlayer}");
        }
    }
    
    public void LogConnectionInfo()
    {
        if (_networkRunner != null)
        {
            Debug.Log($"{DEBUG_PREFIX} - Connection Info: IsConnected={_networkRunner.IsRunning}, IsServer={_networkRunner.IsServer}, IsClient={_networkRunner.IsClient}, LocalPlayer={_networkRunner.LocalPlayer}");
        }
    }
    
    [Button("Log Network State")]
    private void LogNetworkStateButton()
    {
        LogNetworkState();
    }
    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (_gameConfig?.EnableDebugLogs == true)
        {
            Debug.Log($"{DEBUG_PREFIX} - Player joined: {player}, IsLocalPlayer: {player == runner.LocalPlayer}, IsServer: {runner.IsServer}, ActivePlayers: {runner.ActivePlayers.Count()}");
        }
    }
    
    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_gameConfig?.EnableDebugLogs == true)
        {
            Debug.Log($"{DEBUG_PREFIX} - Player left: {player}, IsLocalPlayer: {player == runner.LocalPlayer}, ActivePlayers: {runner.ActivePlayers.Count()}");
        }
    }
    
    public override void OnConnectedToServer(NetworkRunner runner)
    {
        if (_gameConfig?.EnableDebugLogs == true)
        {
            Debug.Log($"{DEBUG_PREFIX} - Connected to server. Runner: {runner}, IsServer: {runner.IsServer}, IsClient: {runner.IsClient}, LocalPlayer: {runner.LocalPlayer}");
        }
    }
    
    public override void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"{DEBUG_PREFIX} - Connection failed: {reason}, RemoteAddress: {remoteAddress}");
    }
    
    public override void OnDisconnectedFromServer(NetworkRunner runner)
    {
        if (_gameConfig?.EnableDebugLogs == true)
        {
            Debug.Log($"{DEBUG_PREFIX} - Disconnected from server. Runner: {runner}");
        }
    }
    
    public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (_gameConfig?.EnableDebugLogs == true)
        {
            Debug.Log($"{DEBUG_PREFIX} - Shutdown: {shutdownReason}, Runner: {runner}");
        }
    }
    
    public override void OnSceneLoadStart(NetworkRunner runner) 
    {
        if (_gameConfig?.EnableDebugLogs == true)
        {
            Debug.Log($"{DEBUG_PREFIX} - Scene load start: {runner}");
        }
    }
    
    public override void OnSceneLoadDone(NetworkRunner runner) 
    {
        if (_gameConfig?.EnableDebugLogs == true)
        {
            Debug.Log($"{DEBUG_PREFIX} - Scene load done: {runner}, IsServer: {runner.IsServer}, ActivePlayers: {runner.ActivePlayers.Count()}");
        }
    }
    
    private void Start()
    {
        Debug.Log($"{DEBUG_PREFIX} - Start");
        
        _networkRunner = _injectedNetworkRunner;
        _gameConfig = ConfigManager.GameConfig;
        
        Debug.Log($"{DEBUG_PREFIX} - Dependencies initialized: NetworkRunner={_networkRunner}, GameConfig={_gameConfig}");
        
        if (_gameConfig != null && !_gameConfig.EnableNetworkDebugger)
        {
            Debug.Log($"{DEBUG_PREFIX} - Network debugger disabled in config");
            return;
        }
        
        if (_networkRunner != null)
        {
            _networkRunner.AddCallbacks(this);
            Debug.Log($"{DEBUG_PREFIX} - NetworkRunner callbacks added: {_networkRunner}");
        }
        else
        {
            Debug.LogError($"{DEBUG_PREFIX} - NetworkRunner is null!");
        }
    }
} 
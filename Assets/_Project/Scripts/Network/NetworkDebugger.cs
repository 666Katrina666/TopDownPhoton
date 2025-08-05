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
    [Title("Runtime Data")]
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkRunner _networkRunner;
    
    private GameConfig _gameConfig;
    
    [Inject] private NetworkRunner _injectedNetworkRunner;
    [Inject] private DiContainer _container;
    
    private void Awake()
    {
        Log("Awake");
    }
    
    [Inject]
    private void PostInject()
    {
        Log($"PostInject - NetworkRunner injected: {_injectedNetworkRunner}");
    }
    
    private void OnDestroy()
    {
        if (_networkRunner != null)
        {
            _networkRunner.RemoveCallbacks(this);
            Log("Callbacks removed from NetworkRunner");
        }
    }
    public void LogNetworkState()
    {
        if (_networkRunner != null)
        {
            Log($"Network State: Runner={_networkRunner}, IsServer={_networkRunner.IsServer}, IsClient={_networkRunner.IsClient}, LocalPlayer={_networkRunner.LocalPlayer}, ActivePlayers={_networkRunner.ActivePlayers.Count()}");
            
            foreach (var player in _networkRunner.ActivePlayers)
            {
                LogPlayerInfo(player);
            }
        }
        else
        {
            LogWarning("NetworkRunner is null!");
        }
    }
    
    public void LogPlayerInfo(PlayerRef player)
    {
        if (_networkRunner != null)
        {
            Log($"Active Player: {player}, IsLocalPlayer: {player == _networkRunner.LocalPlayer}");
        }
    }
    
    public void LogConnectionInfo()
    {
        if (_networkRunner != null)
        {
            Log($"Connection Info: IsConnected={_networkRunner.IsRunning}, IsServer={_networkRunner.IsServer}, IsClient={_networkRunner.IsClient}, LocalPlayer={_networkRunner.LocalPlayer}");
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
            Log($"Player joined: {player}, IsLocalPlayer: {player == runner.LocalPlayer}, IsServer: {runner.IsServer}, ActivePlayers: {runner.ActivePlayers.Count()}");
        }
    }
    
    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_gameConfig?.EnableDebugLogs == true)
        {
            Log($"Player left: {player}, IsLocalPlayer: {player == runner.LocalPlayer}, ActivePlayers: {runner.ActivePlayers.Count()}");
        }
    }
    
    public override void OnConnectedToServer(NetworkRunner runner)
    {
        if (_gameConfig?.EnableDebugLogs == true)
        {
            Log($"Connected to server. Runner: {runner}, IsServer: {runner.IsServer}, IsClient: {runner.IsClient}, LocalPlayer: {runner.LocalPlayer}");
        }
    }
    
    public override void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        LogError($"Connection failed: {reason}, RemoteAddress: {remoteAddress}");
    }
    
    public override void OnDisconnectedFromServer(NetworkRunner runner)
    {
        if (_gameConfig?.EnableDebugLogs == true)
        {
            Log($"Disconnected from server. Runner: {runner}");
        }
    }
    
    public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (_gameConfig?.EnableDebugLogs == true)
        {
            Log($"Shutdown: {shutdownReason}, Runner: {runner}");
        }
    }
    
    public override void OnSceneLoadStart(NetworkRunner runner) 
    {
        if (_gameConfig?.EnableDebugLogs == true)
        {
            Log($"Scene load start: {runner}");
        }
    }
    
    public override void OnSceneLoadDone(NetworkRunner runner) 
    {
        if (_gameConfig?.EnableDebugLogs == true)
        {
            Log($"Scene load done: {runner}, IsServer: {runner.IsServer}, ActivePlayers: {runner.ActivePlayers.Count()}");
        }
    }
    
    private void Start()
    {
        Log("Start");
        
        _networkRunner = _injectedNetworkRunner;
        _gameConfig = ConfigManager.GameConfig;
        
        Log($"Dependencies initialized: NetworkRunner={_networkRunner}, GameConfig={_gameConfig}");
        
        // Попытка получить NetworkRunner из контейнера, если он не инжектирован
        if (_networkRunner == null && _container != null)
        {
            LogWarning("NetworkRunner is null, attempting to resolve from container");
            _networkRunner = _container.Resolve<NetworkRunner>();
            Log($"NetworkRunner resolved from container: {_networkRunner != null}");
        }
        
        if (_gameConfig != null && !_gameConfig.EnableNetworkDebugger)
        {
            Log("Network debugger disabled in config");
            return;
        }
        
        if (_networkRunner != null)
        {
            _networkRunner.AddCallbacks(this);
            Log($"NetworkRunner callbacks added: {_networkRunner}");
        }
        else
        {
            LogError("NetworkRunner is null!");
        }
    }
} 
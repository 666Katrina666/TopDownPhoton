using Fusion;
using UnityEngine;
using Sirenix.OdinInspector;
using Zenject;

/// <summary>
/// Управляет спавном игроков в сетевой игре
/// </summary>
public class PlayerSpawner : NetworkCallbackBase
{
    private const string DEBUG_PREFIX = "[PlayerSpawner]";
    
    [Title("Runtime Data")]
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private IPlayerFactory _playerFactory;
    
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkRunner _networkRunner;
    
    private GameConfig _gameConfig;
    
    private void Awake()
    {
        _gameConfig = ConfigManager.GameConfig;
        
        if (_gameConfig == null)
        {
            Debug.LogWarning($"{DEBUG_PREFIX} - GameConfig not found!");
        }
    }
    
    private void OnDestroy()
    {
    }
    
    private void SetupNetworkRunner(NetworkRunner networkRunner)
    {
        if (_networkRunner != networkRunner)
        {
            _networkRunner = networkRunner;
            _networkRunner.AddCallbacks(this);
            
            if (_networkRunner.IsServer)
            {
                SpawnExistingPlayers();
            }
        }
    }
    
    /// <summary>
    /// Спавнит уже подключенных игроков при загрузке GameScene
    /// </summary>
    private void SpawnExistingPlayers()
    {
        if (_playerFactory == null)
        {
            Debug.LogError($"{DEBUG_PREFIX} - PlayerFactory not found!");
            return;
        }
        
        foreach (var player in _networkRunner.ActivePlayers)
        {
            if (!_playerFactory.IsPlayerSpawned(player))
            {
                SpawnPlayer(_networkRunner, player);
            }
        }
    }
    
    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (_playerFactory == null)
        {
            Debug.LogError($"{DEBUG_PREFIX} - PlayerFactory not found!");
            return;
        }
        
        if (_playerFactory.IsPlayerSpawned(player))
        {
            return;
        }
        
        float spawnOffsetX = _gameConfig?.SpawnOffsetX ?? 2f;
        float spawnHeight = _gameConfig?.SpawnHeight ?? 1f;
        
        Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * spawnOffsetX, spawnHeight, 0);
        
        NetworkObject networkPlayerObject = _playerFactory.CreatePlayer(player, spawnPosition);
        
        if (networkPlayerObject == null)
        {
            Debug.LogError($"{DEBUG_PREFIX} - Failed to create player {player}!");
        }
    }
    
    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            SpawnPlayer(runner, player);
        }
    }
    
    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_playerFactory != null)
        {
            var playerObjects = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
            foreach (var obj in playerObjects)
            {
                if (obj.InputAuthority == player)
                {
                    _playerFactory.DestroyPlayer(obj);
                    break;
                }
            }
        }
    }
    [Inject]
    private void Construct(IPlayerFactory playerFactory, NetworkRunner networkRunner)
    {
        _playerFactory = playerFactory;
        _networkRunner = networkRunner;
        Debug.Log($"{DEBUG_PREFIX} - Dependencies injected: PlayerFactory={_playerFactory}, NetworkRunner={_networkRunner}");
    }
}
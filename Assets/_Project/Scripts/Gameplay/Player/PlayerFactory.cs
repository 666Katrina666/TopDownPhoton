using Fusion;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Zenject;
using Core.Base;

/// <summary>
/// Фабрика для создания игроков в сетевой игре
/// </summary>
public class PlayerFactory : LoggableMonoBehaviour, IPlayerFactory
{
    
    [Title("Player Factory Settings")]
    [FoldoutGroup("Factory Settings")]
    [InfoBox("Префаб игрока для спавна в сети")]
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    
    [Title("Runtime Data")]
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
    
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkRunner _networkRunner;
    
    private GameConfig _gameConfig;
    public NetworkObject CreatePlayer(PlayerRef playerRef, Vector3 position)
    {
        if (_networkRunner == null)
        {
            LogError("NetworkRunner is null!");
            return null;
        }
        
        if (_spawnedPlayers.ContainsKey(playerRef))
        {
            LogWarning($"Player {playerRef} already spawned!");
            return _spawnedPlayers[playerRef];
        }
        
        NetworkObject networkPlayerObject = _networkRunner.Spawn(_playerPrefab, position, Quaternion.identity, playerRef);
        
        if (networkPlayerObject != null)
        {
            _spawnedPlayers.Add(playerRef, networkPlayerObject);
            Log($"Player {playerRef} spawned at {position}");
            
            // Отправляем событие о создании игрока
            EventBus.RaiseEvent(new PlayerSpawnedEvent(playerRef, networkPlayerObject));
        }
        else
        {
            LogError($"Failed to spawn player {playerRef}!");
        }
        
        return networkPlayerObject;
    }
    
    public void DestroyPlayer(NetworkObject playerObject)
    {
        if (playerObject == null) return;
        
        var playerRef = playerObject.InputAuthority;
        if (_spawnedPlayers.ContainsKey(playerRef))
        {
            _spawnedPlayers.Remove(playerRef);
            Log($"Player {playerRef} removed from tracking");
            
            // Отправляем событие об удалении игрока
            EventBus.RaiseEvent(new PlayerDespawnedEvent(playerRef));
        }
        
        if (_networkRunner != null)
        {
            _networkRunner.Despawn(playerObject);
            Log($"Player {playerRef} despawned");
        }
    }
    
    public bool IsPlayerSpawned(PlayerRef playerRef)
    {
        return _spawnedPlayers.ContainsKey(playerRef);
    }
    
    private void Awake()
    {
        _gameConfig = ConfigManager.GameConfig;
        
        if (_gameConfig == null)
        {
            LogWarning("GameConfig not found!");
        }
    }
    
    private void OnDestroy()
    {
    }
    
    [Inject]
    private void Construct(NetworkRunner networkRunner)
    {
        _networkRunner = networkRunner;
        Log($"NetworkRunner injected: {_networkRunner}");
    }
} 
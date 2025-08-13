using Fusion;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Zenject;
using Core.Base;
using System.Collections;
using System.Linq;

/// <summary>
/// Фабрика для создания игроков в сетевой игре
/// </summary>
public class PlayerFactory : LoggableMonoBehaviour, IPlayerFactory
{
    [FoldoutGroup("Factory Settings")]
    [InfoBox("Префаб игрока для спавна в сети")]
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkRunner _networkRunner;
    
    private GameConfig _gameConfig;
    public NetworkObject CreatePlayer(PlayerRef playerRef, Vector3 position)
    {
        if (_networkRunner == null)
        {
            LogError("[Создание игрока] - NetworkRunner null!");
            return null;
        }
        
        if (!_playerPrefab.IsValid)
        {
            LogError("[Создание игрока] - PlayerPrefab не назначен!");
            return null;
        }
        
        if (_spawnedPlayers.ContainsKey(playerRef))
        {
            LogWarning($"[Создание игрока] - Игрок {playerRef} уже заспавнен!");
            return _spawnedPlayers[playerRef];
        }
        
        try
        {
            NetworkObject networkPlayerObject = _networkRunner.Spawn(_playerPrefab, position, Quaternion.identity, playerRef);
            
            if (networkPlayerObject != null)
            {
                _spawnedPlayers.Add(playerRef, networkPlayerObject);
                
                EventBus.RaiseEvent(new PlayerSpawnedEvent(playerRef, networkPlayerObject));
            }
            else
            {
                LogError($"[Создание игрока] - NetworkRunner.Spawn вернул null для игрока {playerRef}!");
            }
        }
        catch (System.Exception ex)
        {
            LogError($"[Создание игрока] - Исключение при спавне игрока {playerRef}: {ex.Message}");
        }
        
        return _spawnedPlayers.TryGetValue(playerRef, out var result) ? result : null;
    }
    
    public void DestroyPlayer(NetworkObject playerObject)
    {
        if (playerObject == null)
        {
            LogWarning("[Удаление игрока] - PlayerObject null, пропускаем");
            return;
        }
        
        var playerRef = playerObject.InputAuthority;
        
        if (_spawnedPlayers.ContainsKey(playerRef))
        {
            _spawnedPlayers.Remove(playerRef);
            
            EventBus.RaiseEvent(new PlayerDespawnedEvent(playerRef));
        }
        else
        {
            LogWarning($"[Удаление игрока] - Игрок {playerRef} не найден в отслеживании");
        }
        
        if (_networkRunner != null)
        {
            _networkRunner.Despawn(playerObject);
        }
        else
        {
            LogError("[Удаление игрока] - NetworkRunner null, не можем деспавнить");
        }
    }
    
    public bool IsPlayerSpawned(PlayerRef playerRef)
    {
        return _spawnedPlayers.ContainsKey(playerRef);
    }
    
    public void DestroyPlayerByPlayerRef(PlayerRef playerRef)
    {
        if (_spawnedPlayers.TryGetValue(playerRef, out NetworkObject playerObject))
        {
            DestroyPlayer(playerObject);
        }
        else
        {
            LogWarning($"[Удаление по PlayerRef] - Игрок {playerRef} не найден в заспавненных игроках!");
        }
    }
    
    private void Awake()
    {
        _gameConfig = ConfigManager.GameConfig;
        
        if (_gameConfig == null)
        {
            LogWarning("[Конфигурация] - GameConfig не найден!");
        }
    }
    
    [Inject]
    private void Construct(NetworkRunner networkRunner)
    {
        _networkRunner = networkRunner;
        
        if (_networkRunner == null)
        {
            LogWarning("[Zenject] - NetworkRunner null при инъекции!");
        }
    }
} 
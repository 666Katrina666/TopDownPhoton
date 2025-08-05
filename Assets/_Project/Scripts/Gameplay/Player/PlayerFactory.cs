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
        Log($"[PlayerFactory] [Создание игрока] - Начало создания игрока {playerRef} на позиции {position}");
        
        if (_networkRunner == null)
        {
            LogError("[PlayerFactory] [Создание игрока] - NetworkRunner null!");
            return null;
        }
        
        if (!_playerPrefab.IsValid)
        {
            LogError("[PlayerFactory] [Создание игрока] - PlayerPrefab не назначен!");
            return null;
        }
        
        if (_spawnedPlayers.ContainsKey(playerRef))
        {
            LogWarning($"[PlayerFactory] [Создание игрока] - Игрок {playerRef} уже заспавнен!");
            return _spawnedPlayers[playerRef];
        }
        
        Log($"[PlayerFactory] [Создание игрока] - Спавним через NetworkRunner: {_networkRunner.name}");
        NetworkObject networkPlayerObject = _networkRunner.Spawn(_playerPrefab, position, Quaternion.identity, playerRef);
        
        if (networkPlayerObject != null)
        {
            _spawnedPlayers.Add(playerRef, networkPlayerObject);
            Log($"[PlayerFactory] [Создание игрока] - Игрок {playerRef} успешно создан: {networkPlayerObject.name}");
            
            // Отправляем событие о создании игрока
            EventBus.RaiseEvent(new PlayerSpawnedEvent(playerRef, networkPlayerObject));
            Log($"[PlayerFactory] [Создание игрока] - Событие PlayerSpawnedEvent отправлено");
        }
        else
        {
            LogError($"[PlayerFactory] [Создание игрока] - Не удалось создать игрока {playerRef}!");
        }
        
        Log($"[PlayerFactory] [Создание игрока] - Завершено, заспавненных игроков: {_spawnedPlayers.Count}");
        return networkPlayerObject;
    }
    
    public void DestroyPlayer(NetworkObject playerObject)
    {
        Log($"[PlayerFactory] [Удаление игрока] - Начало удаления игрока: {playerObject?.name ?? "null"}");
        
        if (playerObject == null)
        {
            LogWarning("[PlayerFactory] [Удаление игрока] - PlayerObject null, пропускаем");
            return;
        }
        
        var playerRef = playerObject.InputAuthority;
        Log($"[PlayerFactory] [Удаление игрока] - PlayerRef: {playerRef}");
        
        if (_spawnedPlayers.ContainsKey(playerRef))
        {
            _spawnedPlayers.Remove(playerRef);
            Log($"[PlayerFactory] [Удаление игрока] - Игрок {playerRef} удален из отслеживания");
            
            // Отправляем событие об удалении игрока
            EventBus.RaiseEvent(new PlayerDespawnedEvent(playerRef));
            Log($"[PlayerFactory] [Удаление игрока] - Событие PlayerDespawnedEvent отправлено");
        }
        else
        {
            LogWarning($"[PlayerFactory] [Удаление игрока] - Игрок {playerRef} не найден в отслеживании");
        }
        
        if (_networkRunner != null)
        {
            _networkRunner.Despawn(playerObject);
            Log($"[PlayerFactory] [Удаление игрока] - Игрок {playerRef} деспавнен через NetworkRunner");
        }
        else
        {
            LogError("[PlayerFactory] [Удаление игрока] - NetworkRunner null, не можем деспавнить");
        }
        
        Log($"[PlayerFactory] [Удаление игрока] - Завершено, осталось игроков: {_spawnedPlayers.Count}");
    }
    
    public bool IsPlayerSpawned(PlayerRef playerRef)
    {
        return _spawnedPlayers.ContainsKey(playerRef);
    }
    
    public void DestroyPlayerByPlayerRef(PlayerRef playerRef)
    {
        Log($"[PlayerFactory] [Удаление по PlayerRef] - Начало удаления игрока {playerRef}");
        
        if (_spawnedPlayers.TryGetValue(playerRef, out NetworkObject playerObject))
        {
            Log($"[PlayerFactory] [Удаление по PlayerRef] - Игрок {playerRef} найден, удаляем через DestroyPlayer");
            DestroyPlayer(playerObject);
        }
        else
        {
            LogWarning($"[PlayerFactory] [Удаление по PlayerRef] - Игрок {playerRef} не найден в заспавненных игроках!");
            Log($"[PlayerFactory] [Удаление по PlayerRef] - Доступные игроки: {string.Join(", ", _spawnedPlayers.Keys)}");
        }
    }
    
    private void Awake()
    {
        Log($"[PlayerFactory] [Инициализация] - Awake вызван для объекта {gameObject.name} (ID: {GetInstanceID()})");
        _gameConfig = ConfigManager.GameConfig;
        
        if (_gameConfig == null)
        {
            LogWarning("[PlayerFactory] [Конфигурация] - GameConfig не найден!");
        }
        else
        {
            Log("[PlayerFactory] [Конфигурация] - GameConfig загружен");
        }
        
        Log($"[PlayerFactory] [Префаб] - PlayerPrefab: {(_playerPrefab.IsValid ? "назначен" : "не назначен")}");
    }
    
    private void OnDestroy()
    {
        Log($"[PlayerFactory] [Уничтожение] - OnDestroy вызван для объекта {gameObject.name} (ID: {GetInstanceID()})");
    }
    
    [Inject]
    private void Construct(NetworkRunner networkRunner)
    {
        Log($"[PlayerFactory] [Zenject] - Начало инъекции NetworkRunner для объекта {gameObject.name} (ID: {GetInstanceID()})");
        
        _networkRunner = networkRunner;
        
        if (_networkRunner == null)
        {
            LogWarning("[PlayerFactory] [Zenject] - NetworkRunner null при инъекции!");
        }
        else
        {
            Log($"[PlayerFactory] [Zenject] - NetworkRunner успешно инжектирован: {_networkRunner.name}");
        }
        
        Log("[PlayerFactory] [Zenject] - Инъекция NetworkRunner завершена");
    }
} 
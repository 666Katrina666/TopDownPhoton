using Fusion;
using UnityEngine;
using Sirenix.OdinInspector;
using Zenject;
using System.Collections;
using System.Linq;

/// <summary>
/// Управляет спавном игроков в сетевой игре
/// </summary>
public class PlayerSpawner : NetworkCallbackBase
{
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private IPlayerFactory _playerFactory;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkRunner _networkRunner;
    
    private GameConfig _gameConfig;
    private bool _isInitialized = false;
    
    private void Awake()
    {
        Log($"[PlayerSpawner] [Инициализация] - Awake вызван для объекта {gameObject.name} (ID: {GetInstanceID()})");
        _gameConfig = ConfigManager.GameConfig;
        
        if (_gameConfig == null)
        {
            LogWarning("[PlayerSpawner] [Конфигурация] - GameConfig не найден!");
        }
        else
        {
            Log($"[PlayerSpawner] [Конфигурация] - GameConfig загружен: SpawnOffsetX={_gameConfig.SpawnOffsetX}, SpawnHeight={_gameConfig.SpawnHeight}");
        }
    }
    
    private void OnEnable()
    {
        Log("[PlayerSpawner] [EventBus] - Подписываемся на NetworkReadyEvent");
        EventBus.Subscribe<NetworkReadyEvent>(OnNetworkReady);
        
        // Дополнительная проверка на случай, если событие уже прошло
        if (_isInitialized && _networkRunner != null && _networkRunner.IsRunning && _networkRunner.IsServer)
        {
            Log("[PlayerSpawner] [EventBus] - Дополнительная проверка: NetworkRunner готов, спавним игроков");
            SpawnExistingPlayers();
        }
    }
    
    private void OnDisable()
    {
        Log("[PlayerSpawner] [EventBus] - Отписываемся от NetworkReadyEvent");
        EventBus.Unsubscribe<NetworkReadyEvent>(OnNetworkReady);
    }
    
    private void OnDestroy()
    {
        Log($"[PlayerSpawner] [Уничтожение] - OnDestroy вызван для объекта {gameObject.name} (ID: {GetInstanceID()})");
    }
    
    /// <summary>
    /// Обработчик события готовности сети
    /// </summary>
    private void OnNetworkReady(NetworkReadyEvent evt)
    {
        Log($"[PlayerSpawner] [EventBus] - Получено событие NetworkReadyEvent, IsServer={evt.IsServer}");
        
        if (!_isInitialized)
        {
            LogWarning("[PlayerSpawner] [EventBus] - PlayerSpawner еще не инициализирован, пропускаем событие");
            return;
        }
        
        if (evt.IsServer)
        {
            Log("[PlayerSpawner] [EventBus] - Это сервер, спавним существующих игроков");
            SpawnExistingPlayers();
        }
        else
        {
            Log("[PlayerSpawner] [EventBus] - Это не сервер, пропускаем спавн существующих игроков");
        }
    }
    
    /// <summary>
    /// Спавнит уже подключенных игроков при загрузке GameScene
    /// </summary>
    private void SpawnExistingPlayers()
    {
        Log("[PlayerSpawner] [Спавн существующих игроков] - Начало");
        
        if (_playerFactory == null)
        {
            LogError("[PlayerSpawner] [Спавн существующих игроков] - PlayerFactory не найден!");
            return;
        }
        
        if (_networkRunner == null)
        {
            LogError("[PlayerSpawner] [Спавн существующих игроков] - NetworkRunner не найден!");
            return;
        }
        
        // Проверяем состояние сети
        if (!_networkRunner.IsRunning)
        {
            LogError("[PlayerSpawner] [Спавн существующих игроков] - NetworkRunner не запущен!");
            return;
        }
        
        if (!_networkRunner.IsServer)
        {
            LogWarning("[PlayerSpawner] [Спавн существующих игроков] - Попытка спавна на клиенте, пропускаем");
            return;
        }
        
        Log($"[PlayerSpawner] [Спавн существующих игроков] - Активных игроков: {_networkRunner.ActivePlayers.Count()}");
        Log($"[PlayerSpawner] [Спавн существующих игроков] - Список игроков: {string.Join(", ", _networkRunner.ActivePlayers)}");
        
        foreach (var player in _networkRunner.ActivePlayers)
        {
            Log($"[PlayerSpawner] [Спавн существующих игроков] - Проверка игрока {player}");
            if (!_playerFactory.IsPlayerSpawned(player))
            {
                Log($"[PlayerSpawner] [Спавн существующих игроков] - Спавним игрока {player}");
                SpawnPlayer(_networkRunner, player);
            }
            else
            {
                Log($"[PlayerSpawner] [Спавн существующих игроков] - Игрок {player} уже заспавнен");
            }
        }
        
        Log("[PlayerSpawner] [Спавн существующих игроков] - Завершено");
    }
    
    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        Log($"[PlayerSpawner] [Спавн игрока] - Начало спавна игрока {player}");
        
        if (_playerFactory == null)
        {
            LogError("[PlayerSpawner] [Спавн игрока] - PlayerFactory не найден!");
            return;
        }
        
        if (_playerFactory.IsPlayerSpawned(player))
        {
            Log($"[PlayerSpawner] [Спавн игрока] - Игрок {player} уже заспавнен, пропускаем");
            return;
        }
        
        // Проверяем состояние сети
        if (runner == null)
        {
            LogError("[PlayerSpawner] [Спавн игрока] - NetworkRunner null!");
            return;
        }
        
        if (!runner.IsRunning)
        {
            LogError("[PlayerSpawner] [Спавн игрока] - NetworkRunner не запущен!");
            return;
        }
        
        if (!runner.IsServer)
        {
            LogWarning("[PlayerSpawner] [Спавн игрока] - Попытка спавна на клиенте, пропускаем");
            return;
        }
        
        float spawnOffsetX = _gameConfig?.SpawnOffsetX ?? 1f;
        float spawnHeight = _gameConfig?.SpawnHeight ?? 1f;
        
        Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount - 1) * spawnOffsetX, spawnHeight, 0);
        Log($"[PlayerSpawner] [Спавн игрока] - Позиция спавна: {spawnPosition}, offsetX={spawnOffsetX}, height={spawnHeight}");
        
        NetworkObject networkPlayerObject = _playerFactory.CreatePlayer(player, spawnPosition);
        
        if (networkPlayerObject == null)
        {
            LogError($"[PlayerSpawner] [Спавн игрока] - Не удалось создать игрока {player}!");
        }
        else
        {
            Log($"[PlayerSpawner] [Спавн игрока] - Игрок {player} успешно создан: {networkPlayerObject.name}");
        }
    }
    
    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Log($"[PlayerSpawner] [Событие] - Игрок {player} присоединился, IsServer={runner.IsServer}");
        
        if (runner.IsServer && _playerFactory != null)
        {
            Log($"[PlayerSpawner] [Событие] - Спавним игрока {player} (сервер)");
            SpawnPlayer(runner, player);
        }
        else
        {
            Log($"[PlayerSpawner] [Событие] - Пропускаем спавн игрока {player} (не сервер или PlayerFactory null)");
        }
    }
    
    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Log($"[PlayerSpawner] [Событие] - Игрок {player} покинул игру");
        
        if (_playerFactory != null)
        {
            Log($"[PlayerSpawner] [Событие] - Удаляем игрока {player} через PlayerFactory");
            _playerFactory.DestroyPlayerByPlayerRef(player);
        }
        else
        {
            LogError("[PlayerSpawner] [Событие] - PlayerFactory null, не можем удалить игрока");
        }
    }
    [Inject]
    private void Construct(IPlayerFactory playerFactory, NetworkRunner networkRunner)
    {
        Log($"[PlayerSpawner] [Zenject] - Начало инъекции зависимостей для объекта {gameObject.name} (ID: {GetInstanceID()})");
        
        _playerFactory = playerFactory;
        _networkRunner = networkRunner;
        _isInitialized = true;
        
        Log($"[PlayerSpawner] [Zenject] - PlayerFactory: {(_playerFactory != null ? "инжектирован" : "null")}");
        Log($"[PlayerSpawner] [Zenject] - NetworkRunner: {(_networkRunner != null ? "инжектирован" : "null")}");
        
        if (_networkRunner != null)
        {
            Log("[PlayerSpawner] [Zenject] - Добавляем PlayerSpawner как callback к NetworkRunner");
            _networkRunner.AddCallbacks(this);
            
            // Проверяем, не было ли уже сгенерировано событие NetworkReadyEvent
            if (_networkRunner.IsRunning && _networkRunner.IsServer)
            {
                Log("[PlayerSpawner] [Zenject] - NetworkRunner уже запущен и это сервер, спавним игроков немедленно");
                SpawnExistingPlayers();
            }
            else
            {
                Log("[PlayerSpawner] [Zenject] - Ожидаем событие NetworkReadyEvent для спавна игроков");
            }
        }
        else
        {
            LogWarning("[PlayerSpawner] [Zenject] - NetworkRunner null при инъекции!");
        }
        
        Log("[PlayerSpawner] [Zenject] - Инъекция зависимостей завершена");
    }
}
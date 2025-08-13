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
    #region Runtime Data
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private IPlayerFactory _playerFactory;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private NetworkRunner _networkRunner;
    
    private GameConfig _gameConfig;
    private bool _isInitialized = false;
    #endregion
    
    #region Unity Callbacks
    private void Awake()
    {
        Log($"[Инициализация] - Awake вызван для объекта {gameObject.name} (ID: {GetInstanceID()})");
        _gameConfig = ConfigManager.GameConfig;
        
        if (_gameConfig == null)
        {
            LogWarning("[Конфигурация] - GameConfig не найден!");
        }
        else
        {
            Log($"[Конфигурация] - GameConfig загружен: SpawnOffsetX={_gameConfig.SpawnOffsetX}, SpawnHeight={_gameConfig.SpawnHeight}");
        }
    }
    
    private void OnEnable()
    {
        Log("[EventBus] - Подписываемся на NetworkReadyEvent");
        EventBus.Subscribe<NetworkReadyEvent>(OnNetworkReady);
    }
    
    private void OnDisable()
    {
        Log("[EventBus] - Отписываемся от NetworkReadyEvent");
        EventBus.Unsubscribe<NetworkReadyEvent>(OnNetworkReady);
    }
    
    private void OnDestroy()
    {
        Log($"[Уничтожение] - OnDestroy вызван для объекта {gameObject.name} (ID: {GetInstanceID()})");
    }
    #endregion
    
    /// <summary>
    /// Обработчик события готовности сети
    /// </summary>
    private void OnNetworkReady(NetworkReadyEvent evt)
    {
        Log($"[EventBus] - Получено событие NetworkReadyEvent, IsServer={evt.IsServer}");
        
        if (!_isInitialized)
        {
            LogWarning("[EventBus] - PlayerSpawner еще не инициализирован, пропускаем событие");
            return;
        }
        
        if (evt.IsServer)
        {
            Log("[EventBus] - Это сервер, спавним существующих игроков");
            SpawnExistingPlayers();
        }
        else
        {
            Log("[EventBus] - Это не сервер, пропускаем спавн существующих игроков");
        }
    }
    
    /// <summary>
    /// Спавнит уже подключенных игроков при загрузке GameScene
    /// </summary>
    private void SpawnExistingPlayers()
    {
        Log("[Спавн существующих игроков] - Начало");
        
        if (_playerFactory == null)
        {
            LogError("[Спавн существующих игроков] - PlayerFactory не найден!");
            return;
        }
        
        if (_networkRunner == null)
        {
            LogError("[Спавн существующих игроков] - NetworkRunner не найден!");
            return;
        }
        
        // Проверяем состояние сети
        if (!_networkRunner.IsRunning)
        {
            LogError("[Спавн существующих игроков] - NetworkRunner не запущен!");
            return;
        }
        
        if (!_networkRunner.IsServer)
        {
            LogWarning("[Спавн существующих игроков] - Попытка спавна на клиенте, пропускаем");
            return;
        }
        
        Log($"[Спавн существующих игроков] - Активных игроков: {_networkRunner.ActivePlayers.Count()}");
        Log($"[Спавн существующих игроков] - Список игроков: {string.Join(", ", _networkRunner.ActivePlayers)}");
        
        foreach (var player in _networkRunner.ActivePlayers)
        {
            Log($"[Спавн существующих игроков] - Проверка игрока {player}");
            if (!_playerFactory.IsPlayerSpawned(player))
            {
                Log($"[Спавн существующих игроков] - Спавним игрока {player}");
                SpawnPlayer(_networkRunner, player);
            }
            else
            {
                Log($"[Спавн существующих игроков] - Игрок {player} уже заспавнен");
            }
        }
        
        Log("[Спавн существующих игроков] - Завершено");
    }
    
    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        Log($"[Спавн игрока] - Начало спавна игрока {player}");
        
        if (_playerFactory == null)
        {
            LogError("[Спавн игрока] - PlayerFactory не найден!");
            return;
        }
        
        if (_playerFactory.IsPlayerSpawned(player))
        {
            Log($"[Спавн игрока] - Игрок {player} уже заспавнен, пропускаем");
            return;
        }
        
        // Проверяем состояние сети
        if (runner == null)
        {
            LogError("[Спавн игрока] - NetworkRunner null!");
            return;
        }
        
        if (!runner.IsRunning)
        {
            LogError("[Спавн игрока] - NetworkRunner не запущен!");
            return;
        }
        
        if (!runner.IsServer)
        {
            LogWarning("[Спавн игрока] - Попытка спавна на клиенте, пропускаем");
            return;
        }
        
        float spawnOffsetX = _gameConfig?.SpawnOffsetX ?? 1f;
        float spawnHeight = _gameConfig?.SpawnHeight ?? 1f;
        
        Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount - 1) * spawnOffsetX, spawnHeight, 0);
        Log($"[Спавн игрока] - Позиция спавна: {spawnPosition}, offsetX={spawnOffsetX}, height={spawnHeight}");
        
        NetworkObject networkPlayerObject = _playerFactory.CreatePlayer(player, spawnPosition);
        
        if (networkPlayerObject == null)
        {
            LogError($"[Спавн игрока] - Не удалось создать игрока {player}!");
        }
        else
        {
            Log($"[Спавн игрока] - Игрок {player} успешно создан: {networkPlayerObject.name}");
        }
    }
    
    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Log($"[Событие] - Игрок {player} присоединился, IsServer={runner.IsServer}");
        
        if (runner.IsServer && _playerFactory != null)
        {
            Log($"[Событие] - Спавним игрока {player} (сервер)");
            SpawnPlayer(runner, player);
        }
        else
        {
            Log($"[Событие] - Пропускаем спавн игрока {player} (не сервер или PlayerFactory null)");
        }
    }
    
    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Log($"[Событие] - Игрок {player} покинул игру");
        
        if (_playerFactory != null)
        {
            Log($"[Событие] - Удаляем игрока {player} через PlayerFactory");
            _playerFactory.DestroyPlayerByPlayerRef(player);
        }
        else
        {
            LogError("[Событие] - PlayerFactory null, не можем удалить игрока");
        }
    }
    [Inject]
    private void Construct(IPlayerFactory playerFactory, NetworkRunner networkRunner)
    {
        Log($"[Zenject] - Начало инъекции зависимостей для объекта {gameObject.name} (ID: {GetInstanceID()})");
        
        _playerFactory = playerFactory;
        _networkRunner = networkRunner;
        _isInitialized = true;
        
        Log($"[Zenject] - PlayerFactory: {(_playerFactory != null ? "инжектирован" : "null")}");
        Log($"[Zenject] - NetworkRunner: {(_networkRunner != null ? "инжектирован" : "null")}");
        
        if (_networkRunner != null)
        {
            Log("[Zenject] - Добавляем PlayerSpawner как callback к NetworkRunner");
            _networkRunner.AddCallbacks(this);
            
            // Проверяем, не было ли уже сгенерировано событие NetworkReadyEvent
            if (_networkRunner.IsRunning && _networkRunner.IsServer)
            {
                Log("[Zenject] - NetworkRunner уже запущен и это сервер, спавним игроков немедленно");
                SpawnExistingPlayers();
            }
            else
            {
                Log("[Zenject] - Ожидаем событие NetworkReadyEvent для спавна игроков");
            }
        }
        else
        {
            LogWarning("[Zenject] - NetworkRunner null при инъекции!");
        }
        
        Log("[Zenject] - Инъекция зависимостей завершена");
    }
}
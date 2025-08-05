using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using Zenject;
using Core.Base;

/// <summary>
/// Основной инициализатор игры с DI
/// Заменяет ProjectBootstrap, SceneBootstrap и MainMenuBootstrap
/// </summary>
public class GameBootstrap : LoggableMonoBehaviour
{
    [Title("Game Bootstrap")]
    [FoldoutGroup("Dependencies")]
    [InfoBox("Зависимости")]
    [ShowInInspector, ReadOnly]
    private GameConfig _gameConfig;
    
    [ShowInInspector, ReadOnly]
    private INetworkService _networkService;
    
    [ShowInInspector, ReadOnly]
    private ISceneService _sceneService;
    
    [Inject] private INetworkService _injectedNetworkService;
    [Inject] private ISceneService _injectedSceneService;
    
    [Title("Settings")]
    [FoldoutGroup("Settings")]
    [InfoBox("Настройки инициализации")]
    [SerializeField] private bool _isPersistent = true;
    
    [SerializeField] private bool _handleSceneChanges = true;
    
    private void Awake()
    {
        Log("Awake");
        
        GameBootstrap[] existingBootstraps = FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None);
        if (existingBootstraps.Length > 1)
        {
            LogWarning("Multiple GameBootstrap found! Destroying duplicate");
            Destroy(gameObject);
            return;
        }
        
        if (_isPersistent)
        {
            DontDestroyOnLoad(gameObject);
            Log("Made persistent");
        }
        
        if (_handleSceneChanges)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }
    
    private void OnDestroy()
    {
        Log("OnDestroy");
        
        UnsubscribeFromEvents();
        
        if (_handleSceneChanges)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    private void Start()
    {
        Log("Start method called");
        
        _gameConfig = ConfigManager.GameConfig;
        _networkService = _injectedNetworkService;
        _sceneService = _injectedSceneService;
        
        Log($"Dependencies initialized. GameConfig: {_gameConfig?.name}");
        
        SubscribeToEvents();
        
        InitializeGame();
    }
    
    private void SubscribeToEvents()
    {
        EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
        EventBus.Subscribe<SceneChangedEvent>(OnSceneChanged);
        EventBus.Subscribe<NetworkConnectedEvent>(OnNetworkConnected);
    }
    
    private void UnsubscribeFromEvents()
    {
        EventBus.Unsubscribe<GameStartedEvent>(OnGameStarted);
        EventBus.Unsubscribe<SceneChangedEvent>(OnSceneChanged);
        EventBus.Unsubscribe<NetworkConnectedEvent>(OnNetworkConnected);
    }
    
    private void InitializeGame()
    {
        Log("Initializing game...");
        
        if (_gameConfig != null)
        {
            Log($"Game config loaded: {_gameConfig.name}");
        }
        
        if (_networkService != null)
        {
            Log("Network service ready: True (service initialized)");
        }
        else
        {
            LogWarning("Network service is null!");
        }
        
        if (_sceneService != null)
        {
            Log($"Scene service ready: {_sceneService.CurrentSceneName}");
        }
        
        Log("Game initialization completed");
    }
    private void OnGameStarted(GameStartedEvent evt)
    {
        Log($"Game started: {evt.GameSceneName}");
    }
    
    private void OnSceneChanged(SceneChangedEvent evt)
    {
        Log($"Scene changed from {evt.PreviousScene} to {evt.CurrentScene}");
    }
    
    private void OnNetworkConnected(NetworkConnectedEvent evt)
    {
        Log($"Network connected: {evt.IsConnected}, Room: {evt.RoomName}");
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Log($"Scene loaded: {scene.name}, mode: {mode}");
        
        if (_sceneService != null)
        {
            EventBus.RaiseEvent(new SceneChangedEvent(_sceneService.CurrentSceneName, scene.name));
        }
    }
} 
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
    #region Dependencies
    [FoldoutGroup("Dependencies")]
    [InfoBox("Зависимости")]
    [ShowInInspector, ReadOnly]
    private GameConfig _gameConfig;
    [FoldoutGroup("Dependencies")]
    [ShowInInspector, ReadOnly]
    private INetworkService _networkService;
    [FoldoutGroup("Dependencies")]
    [ShowInInspector, ReadOnly]
    private ISceneService _sceneService;
    
    [Inject] private INetworkService _injectedNetworkService;
    [Inject] private ISceneService _injectedSceneService;
    [Inject] private DiContainer _container;
    #endregion
    
    #region Settings
    [FoldoutGroup("Settings")]
    [InfoBox("Настройки инициализации")]
    [SerializeField] private bool _isPersistent = true;
    [FoldoutGroup("Settings")]
    [SerializeField] private bool _handleSceneChanges = true;
    #endregion
    
    #region Unity Callbacks
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
        
        // Принудительно инициализируем сервисы, если они еще не созданы
        if (_networkService == null && _container != null)
        {
            LogWarning("NetworkService is null, attempting to resolve from container");
            try
            {
                _networkService = _container.Resolve<INetworkService>();
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to resolve NetworkService: {ex.Message}");
            }
        }
        
        if (_sceneService == null && _container != null)
        {
            LogWarning("SceneService is null, attempting to resolve from container");
            try
            {
                _sceneService = _container.Resolve<ISceneService>();
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to resolve SceneService: {ex.Message}");
            }
        }
        
        SubscribeToEvents();
        
        InitializeGame();
    }
    #endregion
    
    #region Initialization
    private void SubscribeToEvents()
    {
        EventBus.Subscribe<SceneChangedEvent>(OnSceneChanged);
        EventBus.Subscribe<NetworkConnectedEvent>(OnNetworkConnected);
    }
    
    private void UnsubscribeFromEvents()
    {
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
    #endregion
    #region Event Handlers
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
    #endregion
} 
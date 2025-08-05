using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Sirenix.OdinInspector;
using Zenject;
using Core.Base;

/// <summary>
/// Упрощенный контроллер для управления сетевой логикой
/// Объединяет функциональность NetworkSceneManager и NetworkSceneController
/// </summary>
public class NetworkController : LoggableMonoBehaviour
{
    [FoldoutGroup("Dependencies")]
    [InfoBox("Зависимости")]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private INetworkService _networkService;
    [FoldoutGroup("Dependencies")]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private ISceneService _sceneService;
    [FoldoutGroup("Dependencies")]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private DiContainer _container;
    
    [FoldoutGroup("Settings")]
    [InfoBox("Настройки сетевого контроллера")]
    [SerializeField] private string _defaultGameScene = "GameScene";
    [FoldoutGroup("Settings")]
    [SerializeField] private string _defaultMainMenuScene = "MainMenuScene";
    
    private void Awake()
    {
        Log("Awake");
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    [Inject]
    private void Construct(INetworkService networkService, ISceneService sceneService, DiContainer container)
    {
        Log("NetworkController - Construct called");
        _networkService = networkService;
        _sceneService = sceneService;
        _container = container;
        
        Log($"NetworkService injected: {_networkService != null}");
        Log($"SceneService injected: {_sceneService != null}");
    }
    
    private void OnDestroy()
    {
        Log("OnDestroy");
        
        UnsubscribeFromEvents();
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void Start()
    {
        Log("NetworkController Start");
        
        // Проверяем, что зависимости инжектированы
        if (_networkService == null || _sceneService == null)
        {
            LogWarning("Dependencies not injected! NetworkService: " + (_networkService != null) + ", SceneService: " + (_sceneService != null));
            
            // Попытка получить зависимости из контейнера
            if (_networkService == null && _container != null)
            {
                _networkService = _container.Resolve<INetworkService>();
                Log($"NetworkService resolved from container: {_networkService != null}");
            }
            
            if (_sceneService == null && _container != null)
            {
                _sceneService = _container.Resolve<ISceneService>();
                Log($"SceneService resolved from container: {_sceneService != null}");
            }
            
            if (_networkService == null || _sceneService == null)
            {
                LogError("Failed to resolve dependencies from container!");
                return;
            }
        }
        
        SubscribeToEvents();
        Log("NetworkController subscribed to events");
    }
    
    private void SubscribeToEvents()
    {
        EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
        EventBus.Subscribe<LeaveLobbyEvent>(OnLeaveLobby);
    }
    
    private void UnsubscribeFromEvents()
    {
        EventBus.Unsubscribe<GameStartedEvent>(OnGameStarted);
        EventBus.Unsubscribe<LeaveLobbyEvent>(OnLeaveLobby);
    }
    
    private void OnGameStarted(GameStartedEvent evt)
    {
        Log($"NetworkController - Game started: {evt.GameSceneName}");
        
        string sceneName = string.IsNullOrEmpty(evt.GameSceneName) ? _defaultGameScene : evt.GameSceneName;
        Log($"NetworkController - Loading scene: {sceneName}");
        LoadScene(sceneName);
    }
    
    private void OnLeaveLobby(LeaveLobbyEvent evt)
    {
        Log("Leaving lobby");
        
        if (_networkService != null)
        {
            _networkService.Disconnect();
        }
        else
        {
            LogWarning("NetworkService is null during disconnect");
        }
        
        LoadScene(_defaultMainMenuScene);
    }
    /// <summary>
    /// Загружает сцену через NetworkRunner
    /// </summary>
    public void LoadScene(string sceneName)
    {
        Log($"NetworkController - LoadScene called with: {sceneName}");
        
        if (string.IsNullOrEmpty(sceneName))
        {
            LogError("NetworkController - Scene name is null or empty!");
            return;
        }
        
        if (_sceneService == null)
        {
            LogError("NetworkController - SceneService is null! Cannot load scene.");
            return;
        }
        
        if (_networkService == null)
        {
            LogError("NetworkController - NetworkService is null! Cannot load scene.");
            return;
        }
        
        string currentScene = _sceneService.CurrentSceneName ?? SceneManager.GetActiveScene().name;
        Log($"NetworkController - Current scene: {currentScene}");
        
        Log($"NetworkController - Calling NetworkService.LoadScene: {sceneName}");
        _networkService.LoadScene(sceneName);
        
        EventBus.RaiseEvent(new SceneChangedEvent(currentScene, sceneName));
    }
    
    /// <summary>
    /// Подключается к лобби
    /// </summary>
    public void ConnectToLobby(string roomName = "")
    {
        Log($"Connecting to lobby: {roomName}");
        
        if (_networkService != null)
        {
            _networkService.ConnectToLobby(roomName);
        }
        else
        {
            LogError("NetworkService is null!");
        }
    }
    
    /// <summary>
    /// Начинает игру
    /// </summary>
    public void StartGame(string gameSceneName = "")
    {
        Log($"Starting game: {gameSceneName}");
        
        EventBus.RaiseEvent(new GameStartedEvent(gameSceneName));
    }
    
    /// <summary>
    /// Выходит из лобби
    /// </summary>
    public void LeaveLobby()
    {
        Log("Leaving lobby");
        
        EventBus.RaiseEvent(new LeaveLobbyEvent());
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Log($"Scene loaded: {scene.name}, mode: {mode}");
        
        if (_sceneService != null)
        {
            _sceneService.SetCurrentScene(scene.name);
        }
        else
        {
            LogWarning("SceneService is null during scene load");
        }
        
        if (_networkService != null && _networkService.IsConnected)
        {
            EventBus.RaiseEvent(new NetworkConnectedEvent(true, _networkService.CurrentRoomName));
        }
        else if (_networkService == null)
        {
            LogWarning("NetworkService is null during scene load");
        }
    }
} 
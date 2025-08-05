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
    
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private ISceneService _sceneService;
    
    [Inject] private INetworkService _injectedNetworkService;
    [Inject] private ISceneService _injectedSceneService;
    
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
    
    private void OnDestroy()
    {
        Log("OnDestroy");
        
        UnsubscribeFromEvents();
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void Start()
    {
        Log("Start");
        
        _networkService = _injectedNetworkService;
        _sceneService = _injectedSceneService;
        
        SubscribeToEvents();
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
        Log($"Game started: {evt.GameSceneName}");
        
        string sceneName = string.IsNullOrEmpty(evt.GameSceneName) ? _defaultGameScene : evt.GameSceneName;
        LoadScene(sceneName);
    }
    
    private void OnLeaveLobby(LeaveLobbyEvent evt)
    {
        Log("Leaving lobby");
        
        if (_networkService != null)
        {
            _networkService.Disconnect();
        }
        
        LoadScene(_defaultMainMenuScene);
    }
    /// <summary>
    /// Загружает сцену
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            LogError("Scene name is null or empty!");
            return;
        }
        
        Log($"Loading scene: {sceneName}");
        
        string currentScene = _sceneService?.CurrentSceneName ?? SceneManager.GetActiveScene().name;
        
        SceneManager.LoadScene(sceneName);
        
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
        
        if (_networkService != null && _networkService.IsConnected)
        {
            EventBus.RaiseEvent(new NetworkConnectedEvent(true, _networkService.CurrentRoomName));
        }
    }
} 
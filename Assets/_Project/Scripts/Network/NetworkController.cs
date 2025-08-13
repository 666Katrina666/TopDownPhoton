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
    #region Dependencies
    [FoldoutGroup("Dependencies")]
    [InfoBox("Зависимости")]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private INetworkService _networkService;
    [FoldoutGroup("Dependencies")]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private ISceneService _sceneService;
    [FoldoutGroup("Dependencies")]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private IScreenFadeService _screenFadeService;
    private bool _isManualSceneChange;
    [FoldoutGroup("Dependencies")]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private DiContainer _container;
    #endregion
    
    #region Settings
    [FoldoutGroup("Settings")]
    [InfoBox("Настройки сетевого контроллера")]
    [SerializeField] private string _defaultGameScene = "GameScene";
    [FoldoutGroup("Settings")]
    [SerializeField] private string _defaultMainMenuScene = "MainMenuScene";
    #endregion
    
    #region Unity Callbacks
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    [Inject]
    private void Construct(INetworkService networkService, ISceneService sceneService, IScreenFadeService screenFadeService, DiContainer container)
    {
        _networkService = networkService;
        _sceneService = sceneService;
        _screenFadeService = screenFadeService;
        _container = container;
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void Start()
    {
        // Зависимости должны быть инжектированы через Zenject
        SubscribeToEvents();
    }
    #endregion
    
    private void SubscribeToEvents()
    {
        EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
        EventBus.Subscribe<LeaveLobbyEvent>(OnLeaveLobby);
        EventBus.Subscribe<SceneLoadStartEvent>(OnSceneLoadStart);
        EventBus.Subscribe<SceneLoadDoneEvent>(OnSceneLoadDone);
    }
    
    private void UnsubscribeFromEvents()
    {
        EventBus.Unsubscribe<GameStartedEvent>(OnGameStarted);
        EventBus.Unsubscribe<LeaveLobbyEvent>(OnLeaveLobby);
        EventBus.Unsubscribe<SceneLoadStartEvent>(OnSceneLoadStart);
        EventBus.Unsubscribe<SceneLoadDoneEvent>(OnSceneLoadDone);
    }
    
    private void OnGameStarted(GameStartedEvent evt)
    {
        string sceneName = string.IsNullOrEmpty(evt.GameSceneName) ? _defaultGameScene : evt.GameSceneName;
        LoadScene(sceneName);
    }
    
    private void OnLeaveLobby(LeaveLobbyEvent evt)
    {
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
        if (string.IsNullOrEmpty(sceneName))
        {
            LogError("Scene name is null or empty!");
            return;
        }
        
        if (_sceneService == null)
        {
            LogError("SceneService is null! Cannot load scene.");
            return;
        }
        
        if (_networkService == null)
        {
            LogError("NetworkService is null! Cannot load scene.");
            return;
        }
        
        string currentScene = _sceneService.CurrentSceneName ?? SceneManager.GetActiveScene().name;
        Log($"[FadeFlow] - Request LoadScene '{sceneName}' from '{currentScene}'");

        if (currentScene == sceneName)
        {
            LogWarning("[FadeFlow] - Target scene equals current scene, skipping fade and load");
            return;
        }
        LoadSceneWithFade(sceneName);
        
        EventBus.RaiseEvent(new SceneChangedEvent(currentScene, sceneName));
    }
    
    /// <summary>
    /// Подключается к лобби
    /// </summary>
    public void ConnectToLobby(string roomName = "")
    {
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
        EventBus.RaiseEvent(new GameStartedEvent(gameSceneName));
    }
    
    /// <summary>
    /// Выходит из лобби
    /// </summary>
    public void LeaveLobby()
    {
        EventBus.RaiseEvent(new LeaveLobbyEvent());
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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

        // После загрузки сцены делаем fade-in
        FadeInAfterLoad();
    }

    private async void LoadSceneWithFade(string sceneName)
    {
        if (_screenFadeService != null)
        {
            Log($"[FadeFlow] - FadeOut before loading '{sceneName}'");
            await _screenFadeService.FadeOut(0.25f);
        }
        else
        {
            LogWarning("ScreenFadeService is null, loading scene without fade");
        }
        _isManualSceneChange = true;
        Log($"[FadeFlow] - Calling NetworkService.LoadScene('{sceneName}')");
        _networkService.LoadScene(sceneName);
    }

    private async void FadeInAfterLoad()
    {
        if (_screenFadeService != null)
        {
            Log("[FadeFlow] - FadeIn after scene loaded");
            await _screenFadeService.FadeIn(0.25f);
        }
    }

    private async void OnSceneLoadStart(SceneLoadStartEvent _)
    {
        Log("[FadeFlow] - Received SceneLoadStartEvent");
        if (!_isManualSceneChange && _screenFadeService != null)
        {
            Log("[FadeFlow] - External load detected -> FadeOut now");
            await _screenFadeService.FadeOut(0.2f);
        }
    }

    private void OnSceneLoadDone(SceneLoadDoneEvent _)
    {
        Log("[FadeFlow] - Received SceneLoadDoneEvent -> triggering FadeIn");
        _isManualSceneChange = false;
        FadeInAfterLoad();
    }
} 
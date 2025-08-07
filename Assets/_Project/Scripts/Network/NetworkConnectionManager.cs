using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System.Threading;
using Zenject;
using Core.Base;

/// <summary>
/// Менеджер для управления сетевыми подключениями
/// </summary>
public class NetworkConnectionManager : LoggableMonoBehaviour
{
    
    [Title("Runtime Data")]
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private ConnectionState _currentState = ConnectionState.Disconnected;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private string _currentRoomName = "";
    
    private INetworkService _networkService;
    private NetworkRunner _networkRunner;
    private ISceneService _sceneService;
    private NetworkCallbackHandler _callbackHandler;
    
    [Inject] private INetworkService _injectedNetworkService;
    [Inject] private NetworkRunner _injectedNetworkRunner;
    [Inject] private ISceneService _injectedSceneService;
    [Inject] private NetworkCallbackHandler _injectedCallbackHandler;
    
    [FoldoutGroup("Network Configuration")]
    [SerializeField] private string _defaultSceneName = "LobbyScene";
    [FoldoutGroup("Network Configuration")]
    [SerializeField] private int _maxPlayerCount = 4;
    
    private CancellationTokenSource _connectionCancellationTokenSource;
    
    public ConnectionState CurrentState => _currentState;
    public string CurrentRoomName => _currentRoomName;
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        CancelConnectionOperation();
    }
    
    private void Start()
    {
        _networkService = _injectedNetworkService;
        _networkRunner = _injectedNetworkRunner;
        _sceneService = _injectedSceneService;
        _callbackHandler = _injectedCallbackHandler;
        
        SubscribeToEvents();
    }
    
    private void SubscribeToEvents()
    {
        EventBus.Subscribe<StartHostRequestEvent>(OnStartHostRequest);
        EventBus.Subscribe<StartClientRequestEvent>(OnStartClientRequest);
    }
    
    private void UnsubscribeFromEvents()
    {
        EventBus.Unsubscribe<StartHostRequestEvent>(OnStartHostRequest);
        EventBus.Unsubscribe<StartClientRequestEvent>(OnStartClientRequest);
    }
    
    private void OnStartHostRequest(StartHostRequestEvent evt)
    {
        StartHost(evt.RoomName);
    }
    
    private void OnStartClientRequest(StartClientRequestEvent evt)
    {
        StartClient(evt.RoomName);
    }
    
    /// <summary>
    /// Запускает хост
    /// </summary>
    public async void StartHost(string roomName = "")
    {
        if (_currentState == ConnectionState.Connecting)
        {
            LogWarning("Already connecting, ignoring request");
            return;
        }
        
        _currentRoomName = roomName;
        SetConnectionState(ConnectionState.Connecting, "Starting host...");
        
        try
        {
            await StartHostAsync(roomName);
        }
        catch (System.Exception ex)
        {
            LogError($"Host start failed: {ex.Message}");
            SetConnectionState(ConnectionState.Error, $"Host start failed: {ex.Message}");
            EventBus.RaiseEvent(new ConnectionErrorEvent(ex.Message));
        }
    }
    
    /// <summary>
    /// Подключается как клиент
    /// </summary>
    public async void StartClient(string roomName = "")
    {
        if (_currentState == ConnectionState.Connecting)
        {
            LogWarning("Already connecting, ignoring request");
            return;
        }
        
        _currentRoomName = roomName;
        SetConnectionState(ConnectionState.Connecting, "Connecting to server...");
        
        try
        {
            await StartClientAsync(roomName);
        }
        catch (System.Exception ex)
        {
            LogError($"Client start failed: {ex.Message}");
            SetConnectionState(ConnectionState.Error, $"Client start failed: {ex.Message}");
            EventBus.RaiseEvent(new ConnectionErrorEvent(ex.Message));
        }
    }
    
    /// <summary>
    /// Отключается от сети
    /// </summary>
    public async void Disconnect()
    {
        CancelConnectionOperation();
        
        if (_networkRunner != null && _networkRunner.IsRunning)
        {
            await _networkRunner.Shutdown();
        }
        
        if (_networkService != null)
        {
            _networkService.Disconnect();
        }
        
        SetConnectionState(ConnectionState.Disconnected, "Disconnected");
        _currentRoomName = "";
    }
    
    private async Task StartHostAsync(string roomName)
    {
        CancelConnectionOperation();
        _connectionCancellationTokenSource = new CancellationTokenSource();
        
        await PrepareNetworkRunner();
        
        SetupNetworkComponents();
        
        var startGameArgs = CreateStartGameArgs(GameMode.Host, roomName);
        
        var result = await _networkRunner.StartGame(startGameArgs);
        
        if (!result.Ok)
        {
            throw new System.Exception($"Failed to start host: {result.ShutdownReason}");
        }
        
        SetConnectionState(ConnectionState.Connected, "Host started");
    }
    
    private async Task StartClientAsync(string roomName)
    {
        CancelConnectionOperation();
        _connectionCancellationTokenSource = new CancellationTokenSource();
        
        await PrepareNetworkRunner();
        
        SetupNetworkComponents();
        
        var startGameArgs = CreateStartGameArgs(GameMode.Client, roomName);
        
        var result = await _networkRunner.StartGame(startGameArgs);
        
        if (!result.Ok)
        {
            throw new System.Exception($"Failed to connect as client: {result.ShutdownReason}");
        }
        
        SetConnectionState(ConnectionState.Connected, "Connected to server");
    }
    
    private async Task PrepareNetworkRunner()
    {
        if (_networkRunner != null && _networkRunner.IsRunning)
        {
            await _networkRunner.Shutdown();
        }
        
        if (_networkRunner == null)
        {
            var runnerObject = new GameObject("NetworkRunner");
            _networkRunner = runnerObject.AddComponent<NetworkRunner>();
            DontDestroyOnLoad(runnerObject);
        }
    }
    
    private void SetupNetworkComponents()
    {
        var sceneManager = _networkRunner.GetComponent<INetworkSceneManager>();
        if (sceneManager == null)
        {
            var defaultSceneManager = _networkRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            defaultSceneManager.IsSceneTakeOverEnabled = false;
        }
        
        var objectProvider = _networkRunner.GetComponent<INetworkObjectProvider>();
        if (objectProvider == null)
        {
            _networkRunner.gameObject.AddComponent<NetworkObjectProviderDefault>();
        }
        
        if (_callbackHandler != null)
        {
            _networkRunner.AddCallbacks(_callbackHandler);
        }
    }
    
    private StartGameArgs CreateStartGameArgs(GameMode gameMode, string roomName)
    {
        var sceneManager = _networkRunner.GetComponent<INetworkSceneManager>();
        var objectProvider = _networkRunner.GetComponent<INetworkObjectProvider>();
        
        var sceneInfo = new NetworkSceneInfo();
        if (!string.IsNullOrEmpty(_defaultSceneName))
        {
            var sceneRef = sceneManager.GetSceneRef(_defaultSceneName);
            sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Single);
        }
        
        return new StartGameArgs
        {
            GameMode = gameMode,
            SessionName = roomName,
            Scene = sceneInfo,
            PlayerCount = _maxPlayerCount,
            SceneManager = sceneManager,
            ObjectProvider = objectProvider,
            OnGameStarted = OnGameStarted,
            StartGameCancellationToken = _connectionCancellationTokenSource?.Token ?? CancellationToken.None
        };
    }
    
    private void OnGameStarted(NetworkRunner runner)
    {
        if (_sceneService != null)
        {
            _sceneService.LoadScene(_defaultSceneName);
        }
    }
    
    private void CancelConnectionOperation()
    {
        if (_connectionCancellationTokenSource != null)
        {
            _connectionCancellationTokenSource.Cancel();
            _connectionCancellationTokenSource.Dispose();
            _connectionCancellationTokenSource = null;
        }
    }
    
    private void SetConnectionState(ConnectionState state, string message = "")
    {
        _currentState = state;
        
        EventBus.RaiseEvent(new ConnectionStateChangedEvent(state, message));
    }
} 
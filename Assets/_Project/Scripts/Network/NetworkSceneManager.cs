using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Управляет сетевым спавном игроков и основной логикой игры
/// </summary>
public class NetworkSceneManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _networkRunner;
    [SerializeField] string LobbySceneName = "LobbyScene";
    [SerializeField] string MainMenuSceneName = "MainMenuScene";

    private void Awake()
    {
        _networkRunner = GetComponent<NetworkRunner>();

        EventBus.Subscribe<StartGameRequestEvent>(OnStartGameRequest);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<StartGameRequestEvent>(OnStartGameRequest);
    }

    private void OnStartGameRequest(StartGameRequestEvent evt)
    {
        if (_networkRunner.IsServer)
        {
            _networkRunner.LoadScene(evt.GameSceneName); // TODO: заменить на LoadSceneAsync
            Debug.Log($"[NetworkSceneManager] - Loaded scene: {evt.GameSceneName}");
        }
        else
        {
            Debug.Log($"[NetworkSceneManager] - Not server, skipping load");
        }
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("[NetworkSceneManager] - Connected to server");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogWarning($"Connection failed: {reason}");
        LoadScene(MainMenuSceneName);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log("Disconnected from server");
        LoadScene(MainMenuSceneName);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        Debug.Log("Player joined");
        LoadScene(LobbySceneName);
    }
    
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
    {
        Debug.Log($"[NetworkSceneManager] - Shutdown reason: {shutdownReason}");
        LoadScene(MainMenuSceneName);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    private void LoadScene(string sceneName)
    {        
        if (SceneManager.GetActiveScene().name == sceneName) 
        {
            Debug.Log($"[NetworkSceneManager] - Already in scene {sceneName}, skipping load");
            return;
        }
        
        Debug.Log($"[NetworkSceneManager] - Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}

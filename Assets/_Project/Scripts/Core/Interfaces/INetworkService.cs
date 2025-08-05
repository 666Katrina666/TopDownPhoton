using Fusion;

/// <summary>
/// Интерфейс для работы с сетевыми компонентами
/// </summary>
public interface INetworkService
{
    NetworkRunner NetworkRunner { get; }
    bool IsConnected { get; }
    bool IsServer { get; }
    bool IsClient { get; }
    PlayerRef LocalPlayer { get; }
    string CurrentRoomName { get; }
    
    void Connect();
    void ConnectToLobby(string roomName = "");
    void Disconnect();
    void Shutdown();
    void LoadScene(string sceneName);
} 
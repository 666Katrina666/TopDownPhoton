using UnityEngine;
using Fusion;

/// <summary>
/// Событие запроса на запуск хоста
/// </summary>
public struct StartHostRequestEvent
{
    public string RoomName;
    
    public StartHostRequestEvent(string roomName = "")
    {
        RoomName = roomName;
    }
}

/// <summary>
/// Событие запроса на подключение клиента
/// </summary>
public struct StartClientRequestEvent
{
    public string RoomName;
    
    public StartClientRequestEvent(string roomName = "")
    {
        RoomName = roomName;
    }
}

/// <summary>
/// Событие изменения состояния подключения
/// </summary>
public struct ConnectionStateChangedEvent
{
    public ConnectionState State;
    public string Message;
    
    public ConnectionStateChangedEvent(ConnectionState state, string message = "")
    {
        State = state;
        Message = message;
    }
}

/// <summary>
/// Событие ошибки подключения
/// </summary>
public struct ConnectionErrorEvent
{
    public string ErrorMessage;
    
    public ConnectionErrorEvent(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Событие присоединения игрока
/// </summary>
public struct PlayerJoinedEvent
{
    public PlayerRef Player;
    public bool IsLocalPlayer;
    
    public PlayerJoinedEvent(PlayerRef player, bool isLocalPlayer)
    {
        Player = player;
        IsLocalPlayer = isLocalPlayer;
    }
}

/// <summary>
/// Событие выхода игрока
/// </summary>
public struct PlayerLeftEvent
{
    public PlayerRef Player;
    public bool IsLocalPlayer;
    
    public PlayerLeftEvent(PlayerRef player, bool isLocalPlayer)
    {
        Player = player;
        IsLocalPlayer = isLocalPlayer;
    }
}

/// <summary>
/// Событие начала загрузки сцены
/// </summary>
public struct SceneLoadStartEvent
{
}

/// <summary>
/// Событие завершения загрузки сцены
/// </summary>
public struct SceneLoadDoneEvent
{
}

/// <summary>
/// Событие подключения к сети
/// </summary>
public struct NetworkConnectedEvent
{
    public bool IsConnected;
    public string RoomName;
    
    public NetworkConnectedEvent(bool isConnected, string roomName = "")
    {
        IsConnected = isConnected;
        RoomName = roomName;
    }
}

/// <summary>
/// Состояния подключения
/// </summary>
public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Error
} 
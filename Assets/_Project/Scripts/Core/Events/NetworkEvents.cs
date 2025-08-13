using UnityEngine;
using Fusion;

/// <summary>
/// Событие запроса на запуск хоста
/// </summary>
public struct StartHostRequestEvent
{
    public string RoomName { get; }
    
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
    public string RoomName { get; }
    
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
    public ConnectionState State { get; }
    public string Message { get; }
    
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
    public string ErrorMessage { get; }
    
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
    public PlayerRef Player { get; }
    public bool IsLocalPlayer { get; }
    
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
    public PlayerRef Player { get; }
    public bool IsLocalPlayer { get; }
    
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
    public bool IsConnected { get; }
    public string RoomName { get; }
    
    public NetworkConnectedEvent(bool isConnected, string roomName = "")
    {
        IsConnected = isConnected;
        RoomName = roomName;
    }
}

/// <summary>
/// Событие готовности сети к спавну объектов
/// </summary>
public struct NetworkReadyEvent
{
    public NetworkRunner Runner { get; }
    public bool IsServer { get; }
    
    public NetworkReadyEvent(NetworkRunner runner, bool isServer)
    {
        Runner = runner;
        IsServer = isServer;
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
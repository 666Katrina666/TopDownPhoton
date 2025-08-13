using UnityEngine;
using Fusion;

/// <summary>
/// Событие о начале игры
/// </summary>
public struct GameStartedEvent
{
    /// <summary>
    /// Имя сцены, которую нужно загрузить
    /// </summary>
    public string GameSceneName { get; }

    public GameStartedEvent(string gameSceneName)
    {
        GameSceneName = gameSceneName;
    }
}

/// <summary>
/// Событие о создании игрока
/// </summary>
public struct PlayerSpawnedEvent
{
    /// <summary>
    /// Ссылка на игрока в Fusion
    /// </summary>
    public PlayerRef PlayerRef { get; }

    /// <summary>
    /// Сетевой объект игрока
    /// </summary>
    public NetworkObject PlayerObject { get; }

    public PlayerSpawnedEvent(PlayerRef playerRef, NetworkObject playerObject)
    {
        PlayerRef = playerRef;
        PlayerObject = playerObject;
    }
}

/// <summary>
/// Событие об удалении игрока
/// </summary>
public struct PlayerDespawnedEvent
{
    /// <summary>
    /// Ссылка на игрока в Fusion
    /// </summary>
    public PlayerRef PlayerRef { get; }

    public PlayerDespawnedEvent(PlayerRef playerRef)
    {
        PlayerRef = playerRef;
    }
}

/// <summary>
/// Событие о смене сцены
/// </summary>
public struct SceneChangedEvent
{
    /// <summary>
    /// Имя предыдущей сцены
    /// </summary>
    public string PreviousScene { get; }

    /// <summary>
    /// Имя текущей сцены
    /// </summary>
    public string CurrentScene { get; }

    public SceneChangedEvent(string previousScene, string currentScene)
    {
        PreviousScene = previousScene;
        CurrentScene = currentScene;
    }
}



/// <summary>
/// Событие для обновления списка игроков в лобби
/// </summary>
public struct RefreshLobbyPlayerListEvent
{
}

/// <summary>
/// Событие для выхода из лобби
/// </summary>
public struct LeaveLobbyEvent
{
}
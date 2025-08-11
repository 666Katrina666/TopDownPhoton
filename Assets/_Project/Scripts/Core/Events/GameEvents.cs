using UnityEngine;
using Fusion;

/// <summary>
/// Событие о начале игры
/// </summary>
public struct GameStartedEvent
{
    public string GameSceneName;
    
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
    public PlayerRef PlayerRef;
    public NetworkObject PlayerObject;
    
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
    public PlayerRef PlayerRef;
    
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
    public string PreviousScene;
    public string CurrentScene;
    
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
    // Пустая структура для события
}

/// <summary>
/// Событие для выхода из лобби
/// </summary>
public struct LeaveLobbyEvent
{
    // Пустая структура для события
}

/// <summary>
/// Событие об изменении анимации персонажа
/// </summary>
public struct PlayerAnimationChangedEvent
{
    public PlayerRef PlayerRef;
    public int Direction;
    public bool IsMoving;
    
    public PlayerAnimationChangedEvent(PlayerRef playerRef, int direction, bool isMoving)
    {
        PlayerRef = playerRef;
        Direction = direction;
        IsMoving = isMoving;
    }
}
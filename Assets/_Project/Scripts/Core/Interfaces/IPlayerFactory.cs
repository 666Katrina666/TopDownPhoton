using Fusion;
using UnityEngine;

/// <summary>
/// Интерфейс для создания игроков
/// </summary>
public interface IPlayerFactory
{
    NetworkObject CreatePlayer(PlayerRef playerRef, Vector3 position);
    void DestroyPlayer(NetworkObject playerObject);
    bool IsPlayerSpawned(PlayerRef playerRef);
} 
using Fusion;

/// <summary>
/// Интерфейс для отладочных сервисов
/// </summary>
public interface IDebugService
{
    void LogNetworkState();
    void LogPlayerInfo(PlayerRef player);
    void LogConnectionInfo();
} 
using UnityEngine;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Основной контроллер главного меню
/// </summary>
public class MainMenuController : LoggableMonoBehaviour
{
    /// <summary>
    /// Запускает хост
    /// </summary>
    public void StartHost()
    {
        EventBus.RaiseEvent(new StartHostRequestEvent());
    }
    
    /// <summary>
    /// Подключается как клиент
    /// </summary>
    public void StartClient()
    {
        EventBus.RaiseEvent(new StartClientRequestEvent());
    }
    
    /// <summary>
    /// Запускает хост с указанным именем комнаты
    /// </summary>
    public void StartHostWithRoom(string roomName)
    {
        EventBus.RaiseEvent(new StartHostRequestEvent(roomName));
    }
    
    /// <summary>
    /// Подключается как клиент к указанной комнате
    /// </summary>
    public void StartClientWithRoom(string roomName)
    {
        EventBus.RaiseEvent(new StartClientRequestEvent(roomName));
    }
} 
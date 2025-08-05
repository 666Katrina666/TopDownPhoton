using UnityEngine;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Основной контроллер главного меню
/// </summary>
public class MainMenuController : LoggableMonoBehaviour
{
    
    private void Awake()
    {
        Log("Awake");
    }
    
    private void OnDestroy()
    {
        Log("OnDestroy");
    }
    /// <summary>
    /// Запускает хост
    /// </summary>
    public void StartHost()
    {
        Log("Start host requested");
        EventBus.RaiseEvent(new StartHostRequestEvent());
    }
    
    /// <summary>
    /// Подключается как клиент
    /// </summary>
    public void StartClient()
    {
        Log("Start client requested");
        EventBus.RaiseEvent(new StartClientRequestEvent());
    }
    
    /// <summary>
    /// Запускает хост с указанным именем комнаты
    /// </summary>
    public void StartHostWithRoom(string roomName)
    {
        Log($"Start host with room requested: {roomName}");
        EventBus.RaiseEvent(new StartHostRequestEvent(roomName));
    }
    
    /// <summary>
    /// Подключается как клиент к указанной комнате
    /// </summary>
    public void StartClientWithRoom(string roomName)
    {
        Log($"Start client with room requested: {roomName}");
        EventBus.RaiseEvent(new StartClientRequestEvent(roomName));
    }
} 
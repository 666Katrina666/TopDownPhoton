using System;
using System.Collections.Generic;

/// <summary>
/// Централизованная система событий для связи между компонентами
/// </summary>
public static class EventBus
{
    #region State
    private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    #endregion

    #region API
    /// <summary>
    /// Подписка на событие определённого типа
    /// </summary>
    public static void Subscribe<T>(Action<T> callback) where T : struct
    {
        var type = typeof(T);
        if (!_subscribers.TryGetValue(type, out var list))
        {
            list = new List<Delegate>();
            _subscribers[type] = list;
        }
        
        list.Add(callback);
    }
    
    /// <summary>
    /// Отписка от события
    /// </summary>
    public static void Unsubscribe<T>(Action<T> callback) where T : struct
    {
        var type = typeof(T);
        if (_subscribers.TryGetValue(type, out var list))
        {
            list.Remove(callback);
            if (list.Count == 0)
                _subscribers.Remove(type);
        }
    }
    
    /// <summary>
    /// Генерация (вызов) события
    /// </summary>
    public static void RaiseEvent<T>(T evt) where T : struct
    {
        var type = typeof(T);
        if (_subscribers.TryGetValue(type, out var list))
        {
            var listCopy = list.ToArray();
            foreach (var callback in listCopy)
            {
                ((Action<T>)callback)?.Invoke(evt);
            }
        }
    }
    
    /// <summary>
    /// Очистка всех подписчиков (например, при перезапуске сцены)
    /// </summary>
    public static void Clear()
    {
        _subscribers.Clear();
    }
    #endregion
}

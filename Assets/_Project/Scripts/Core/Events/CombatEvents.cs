using Fusion;
using UnityEngine;

/// <summary>
/// Событие получения урона конкретной сетевой сущностью
/// </summary>
public struct DamageTakenEvent
{
    /// <summary>
    /// Цель, которая получила урон (сетевой объект)
    /// </summary>
    public NetworkObject Target;

    /// <summary>
    /// Источник урона (владелец ввода/игрок)
    /// </summary>
    public PlayerRef Source;

    /// <summary>
    /// Количество нанесённого урона (нормализованное, неотрицательное)
    /// </summary>
    public int Amount;

    public DamageTakenEvent(NetworkObject target, PlayerRef source, int amount)
    {
        Target = target;
        Source = source;
        Amount = amount;
    }
}



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
    public NetworkObject Target { get; }

    /// <summary>
    /// Источник урона (владелец ввода/игрок)
    /// </summary>
    public PlayerRef Source { get; }

    /// <summary>
    /// Количество нанесённого урона (нормализованное, неотрицательное)
    /// </summary>
    public int Amount { get; }

    public DamageTakenEvent(NetworkObject target, PlayerRef source, int amount)
    {
        Target = target;
        Source = source;
        Amount = amount;
    }
}

/// <summary>
/// Событие изменения здоровья
/// </summary>
public struct HealthChangedEvent
{
	public NetworkObject Target { get; }
	public int Current { get; }
	public int Max { get; }

	public HealthChangedEvent(NetworkObject target, int current, int max)
	{
		Target = target;
		Current = current;
		Max = max;
	}
}

/// <summary>
/// Событие смерти объекта
/// </summary>
public struct DeathEvent
{
	/// <summary>
	/// Умерший объект (сетевой объект)
	/// </summary>
	public NetworkObject Target { get; }

	/// <summary>
	/// Источник смерти (владелец ввода/игрок)
	/// </summary>
	public PlayerRef Source { get; }

	public DeathEvent(NetworkObject target, PlayerRef source)
	{
		Target = target;
		Source = source;
	}
}



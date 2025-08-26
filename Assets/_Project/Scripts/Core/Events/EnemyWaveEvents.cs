using Fusion;
using UnityEngine;

/// <summary>
/// Событие начала волны
/// </summary>
public struct WaveStartedEvent
{
	/// <summary>
	/// Номер волны (0-based)
	/// </summary>
	public int WaveNumber { get; set; }

	/// <summary>
	/// Количество очков на волну
	/// </summary>
	public int PointsLimit { get; set; }
}

/// <summary>
/// Событие завершения волны
/// </summary>
public struct WaveCompletedEvent
{
	/// <summary>
	/// Номер завершенной волны (0-based)
	/// </summary>
	public int WaveNumber { get; set; }
}

/// <summary>
/// Событие спавна юнита
/// </summary>
public struct EnemySpawnedEvent
{
	/// <summary>
	/// Сетевой объект юнита
	/// </summary>
	public NetworkObject Enemy { get; set; }

	/// <summary>
	/// Позиция спавна
	/// </summary>
	public Vector3 SpawnPosition { get; set; }
}

/// <summary>
/// Событие готовности юнита к возврату в пул (после завершения анимации смерти)
/// </summary>
public struct EnemyReturnedToPoolEvent
{
	/// <summary>
	/// Сетевой объект юнита
	/// </summary>
	public NetworkObject Enemy { get; set; }
}


using UnityEngine;

/// <summary>
/// Интерфейс сервиса тряски камеры
/// </summary>
public interface ICameraShakeService
{
	/// <summary>
	/// Небольшая тряска по умолчанию
	/// </summary>
	void ShakeMinor();

	/// <summary>
	/// Кастомная тряска камеры
	/// </summary>
	void ShakeCustom(float duration, Vector3 strength, int vibrato, float randomness, bool useUnscaledTime = true);
}

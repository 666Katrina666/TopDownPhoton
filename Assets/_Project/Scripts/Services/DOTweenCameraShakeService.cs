using UnityEngine;
using DG.Tweening;
using Zenject;
using Core.Base;

/// <summary>
/// Реализация ICameraShakeService на DOTween для 2D/топ-даун камеры
/// </summary>
public class DOTweenCameraShakeService : LoggableMonoBehaviour, ICameraShakeService
{
	[InjectOptional] private Camera _injectedCamera;
	private Transform _targetTransform;
	private Tween _activeTween;

	private void Awake()
	{
		_targetTransform = (_injectedCamera != null ? _injectedCamera.transform : Camera.main != null ? Camera.main.transform : null);
		if (_targetTransform == null)
		{
			LogWarning("Camera not found for shake service");
		}
	}

	private void OnDestroy()
	{
		_activeTween?.Kill(false);
		_activeTween = null;
	}

	public void ShakeMinor()
	{
		ShakeCustom(0.15f, new Vector3(0.1f, 0.1f, 0f), 12, 90f, true);
	}

	public void ShakeCustom(float duration, Vector3 strength, int vibrato, float randomness, bool useUnscaledTime = true)
	{
		if (_targetTransform == null) return;
		_activeTween?.Kill(false);
		_activeTween = _targetTransform.DOShakePosition(duration, strength, vibrato, randomness, false, true)
			.SetUpdate(useUnscaledTime)
			.SetRecyclable(true);
	}
}

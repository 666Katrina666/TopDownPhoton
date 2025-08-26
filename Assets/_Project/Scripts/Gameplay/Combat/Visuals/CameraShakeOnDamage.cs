using UnityEngine;
using Fusion;
using Sirenix.OdinInspector;
using Zenject;
using Core.Base;

/// <summary>
/// Тряска камеры при получении урона локальным аватаром
/// </summary>
public class CameraShakeOnDamage : LoggableMonoBehaviour
{
	#region Dependencies
	[Inject] private ICameraShakeService _cameraShakeService;
	[Inject] private INetworkService _networkService;
	#endregion

	#region Runtime
	[ShowInInspector, Sirenix.OdinInspector.ReadOnly]
	private PlayerRef _localPlayer;
	#endregion

	private void Start()
	{
		_localPlayer = _networkService != null ? _networkService.LocalPlayer : default;
		EventBus.Subscribe<DamageTakenEvent>(OnDamageTaken);
	}

	private void OnDestroy()
	{
		EventBus.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
	}

	private void OnDamageTaken(DamageTakenEvent evt)
	{
		if (evt.Target == null) return;
		if (_networkService == null || _cameraShakeService == null) return;
		// Только если урон получил наш локальный аватар
		if (evt.Target.InputAuthority == _networkService.LocalPlayer)
		{
			_cameraShakeService.ShakeMinor();
			Log("Local damage received -> camera shake");
		}
	}
}

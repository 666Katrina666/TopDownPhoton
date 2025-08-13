using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Zenject;
using Sirenix.OdinInspector;
using Core.Base;
using Fusion;

/// <summary>
/// Визуализация HP: передний слайдер мгновенно, задний плавно догоняет
/// </summary>
public class HealthBarView : LoggableMonoBehaviour
{
	#region Inspector
	[FoldoutGroup("Dependencies")]
	[SerializeField] private Slider _frontSlider;
	[FoldoutGroup("Dependencies")]
	[SerializeField] private Slider _backSlider;
	[FoldoutGroup("Dependencies")]
	[SerializeField] private HealthBarConfig _config;
	#endregion

	#region Runtime
	private NetworkObject _target;
	private Tween _backTween;
	private float _hideTimer;
	private CanvasGroup _canvasGroup;
	private Camera _camera;
	#endregion

	private void Awake()
	{
		_target = GetComponentInParent<NetworkObject>();
		_canvasGroup = GetComponent<CanvasGroup>();
		_camera = Camera.main;
	}

	private void OnEnable()
	{
		EventBus.Subscribe<HealthChangedEvent>(OnHealthChanged);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe<HealthChangedEvent>(OnHealthChanged);
		KillTween();
	}

	private void LateUpdate()
	{
		if (_config != null)
		{
			// Follow (child object already follows parent). Apply Y offset and billboard if needed
			if (_config.billboard && _camera != null)
			{
				transform.rotation = Quaternion.LookRotation(_camera.transform.forward);
			}
			var local = transform.localPosition;
			local.y = _config.yOffset;
			transform.localPosition = local;

			if (_config.autoHide && _canvasGroup != null)
			{
				if (_hideTimer > 0f)
				{
					_hideTimer -= (_config.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
					if (_hideTimer <= 0f)
					{
						_canvasGroup.alpha = 0f;
					}
				}
			}
		}
	}

	private void OnHealthChanged(HealthChangedEvent evt)
	{
		if (_target == null || evt.Target != _target) return;
		if (_frontSlider == null || _backSlider == null) return;

		float value = evt.Max > 0 ? Mathf.Clamp01((float)evt.Current / evt.Max) : 0f;

		// Front: instant
		_frontSlider.value = value;

		// Back: tween to value
		KillTween();
		float delay = _config != null ? _config.tailDelay : 0.2f;
		float duration = _config != null ? _config.tailDuration : 0.35f;
		Ease ease = _config != null ? _config.tailEase : Ease.OutCubic;
		bool unscaled = _config != null && _config.useUnscaledTime;
		_backTween = DOTween.To(() => _backSlider.value, v => _backSlider.value = v, value, duration)
			.SetDelay(delay)
			.SetEase(ease)
			.SetUpdate(unscaled)
			.SetRecyclable(true);

		// Visibility
		if (_canvasGroup != null && _config != null && _config.autoHide)
		{
			_canvasGroup.alpha = 1f;
			_hideTimer = _config.hideDelay;
		}

		Log($"HP changed: {evt.Current}/{evt.Max} -> value={value:0.00}");
	}

	private void KillTween()
	{
		if (_backTween != null && _backTween.IsActive())
		{
			_backTween.Kill(false);
			_backTween = null;
		}
	}
}

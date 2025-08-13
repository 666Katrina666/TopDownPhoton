using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using Zenject;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Анимации кнопки: hover/click через ITweenService
/// </summary>
public class UIButtonTween : LoggableMonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
	#region Dependencies
	[Inject] private ITweenService _tweenService;
	#endregion

	#region Settings
	[FoldoutGroup("Dependencies")]
	[SerializeField] private UIButtonTweenConfig _config;
	[FoldoutGroup("Dependencies")]
	[SerializeField] private Transform _targetScale;
	#endregion

	#region State
	private bool _isHovered;
	private bool _isPressed;
	private Tween _scaleTween;
	private Vector3 _baseScale;
	#endregion

	private void Reset()
	{
		_targetScale = transform;
	}

	private void Awake()
	{
		if (_targetScale == null) _targetScale = transform;
		_baseScale = _config != null ? _config.scaleNormal : Vector3.one;
	}

	private void OnEnable()
	{
		ApplyImmediate(_config != null ? _config.scaleNormal : Vector3.one);
	}

	private void OnDisable()
	{
		KillAllTweens();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_isHovered = true;
		if (_isPressed) return;
		TweenTo(_config.scaleHover, _config.durationHoverIn, _config.easeHover);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isHovered = false;
		if (_isPressed) return;
		TweenTo(_config.scaleNormal, _config.durationHoverOut, _config.easeHover);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_isPressed = true;
		TweenTo(_config.scalePressed, _config.durationPressIn, _config.easePress);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_isPressed = false;
		var targetScale = _isHovered ? _config.scaleHover : _config.scaleNormal;
		TweenTo(targetScale, _config.durationPressOut, _config.easePress);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (_config.durationClickBounce > 0f)
		{
			var bounceTo = _baseScale * _config.clickBounceMultiplier;
			KillScale();
			_scaleTween = _targetScale.DOScale(bounceTo, _config.durationClickBounce * 0.5f)
				.SetEase(_config.easeBounce)
				.SetUpdate(_config.useUnscaledTime)
				.OnComplete(() =>
				{
					_scaleTween = _targetScale.DOScale(_baseScale, _config.durationClickBounce * 0.5f)
						.SetEase(_config.easeBounce)
						.SetUpdate(_config.useUnscaledTime);
				});
		}
	}

	private void TweenTo(Vector3 scale, float scaleDuration, Ease ease)
	{
		_baseScale = scale;
		KillScale();
		_scaleTween = _tweenService.ScaleTransform(_targetScale, scale, scaleDuration, ease)
			?.SetUpdate(_config.useUnscaledTime);
	}

	private void ApplyImmediate(Vector3 scale)
	{
		KillAllTweens();
		_targetScale.localScale = scale;
	}

	private void KillScale()
	{
		_scaleTween?.Kill(false);
		_scaleTween = null;
	}

	private void KillAllTweens()
	{
		KillScale();
	}
}



using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Core.Base;

/// <summary>
/// Реализация ITweenService на базе DOTween
/// </summary>
public class DOTweenService : LoggableMonoBehaviour, ITweenService
{
	#region Unity Callbacks
	private void Awake()
	{
		DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
		Log("DOTween initialized");
	}
	#endregion

	#region ITweenService
	public Tween FadeCanvasGroup(CanvasGroup target, float endValue, float duration, Ease ease = Ease.Linear)
	{
		if (target == null)
		{
			LogWarning("FadeCanvasGroup target is null");
			return null;
		}
		return target.DOFade(endValue, duration).SetEase(ease);
	}

	public Tween ScaleTransform(Transform target, Vector3 endValue, float duration, Ease ease = Ease.OutQuad)
	{
		if (target == null)
		{
			LogWarning("ScaleTransform target is null");
			return null;
		}
		return target.DOScale(endValue, duration).SetEase(ease);
	}

	public Tween ColorGraphic(Graphic target, Color endValue, float duration, Ease ease = Ease.Linear)
	{
		if (target == null)
		{
			LogWarning("ColorGraphic target is null");
			return null;
		}
		return target.DOColor(endValue, duration).SetEase(ease);
	}

	public Tween ShakeAnchorPos(RectTransform target, float duration, Vector2 strength, int vibrato = 10, float randomness = 90f, bool snapping = false, bool fadeOut = true)
	{
		if (target == null)
		{
			LogWarning("ShakeAnchorPos target is null");
			return null;
		}
		return target.DOShakeAnchorPos(duration, strength, vibrato, randomness, snapping, fadeOut);
	}
	#endregion
}



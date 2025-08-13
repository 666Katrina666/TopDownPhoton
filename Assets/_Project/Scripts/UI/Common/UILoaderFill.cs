using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Бесконечная анимация заливки для Image (type = Filled)
/// </summary>
public class UILoaderFill : LoggableMonoBehaviour
{
	[FoldoutGroup("Dependencies")]
	[SerializeField] private Image _image;

	[FoldoutGroup("Settings")]
	[SerializeField] private float _duration = 1.2f;
	[FoldoutGroup("Settings")]
	[SerializeField] private bool _useUnscaledTime = true;

	private Tween _fillTween;

	private void Reset()
	{
		_image = GetComponent<Image>();
	}

	private void Awake()
	{
		if (_image == null) _image = GetComponent<Image>();
	}

	private void OnEnable()
	{
		StartFill();
	}

	private void OnDisable()
	{
		KillFill();
	}

	private void StartFill()
	{
		KillFill();
		if (_image == null)
		{
			LogWarning("Image is null");
			return;
		}
		_image.type = Image.Type.Filled;
		_image.fillAmount = 0f;
		_fillTween = DOTween
			.To(() => _image.fillAmount, v => _image.fillAmount = v, 1f, _duration)
			.SetEase(Ease.Linear)
			.SetLoops(-1, LoopType.Restart)
			.SetUpdate(_useUnscaledTime);
		Log("[Fill] - started");
	}

	private void KillFill()
	{
		if (_fillTween != null && _fillTween.IsActive())
		{
			_fillTween.Kill(false);
			_fillTween = null;
			Log("[Fill] - stopped");
		}
	}
}

using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Бесконечный лоадер-спиннер: крутит целевой RectTransform с заданной скоростью
/// </summary>
public class UILoaderSpinner : LoggableMonoBehaviour
{
	[FoldoutGroup("Dependencies")]
	[SerializeField] private RectTransform _target;

	[FoldoutGroup("Settings")]
	[LabelText("Speed (deg/sec)")]
	[SerializeField] private float _speedDegPerSec = 180f;
	[FoldoutGroup("Settings")]
	[SerializeField] private bool _clockwise = true;
	[FoldoutGroup("Settings")]
	[SerializeField] private bool _useUnscaledTime = true;

	private Tween _spinTween;

	private void Reset()
	{
		_target = GetComponent<RectTransform>();
	}

	private void Awake()
	{
		if (_target == null)
		{
			_target = GetComponent<RectTransform>();
		}
	}

	private void OnEnable()
	{
		StartSpin();
	}

	private void OnDisable()
	{
		KillSpin();
	}

	private void StartSpin()
	{
		KillSpin();
		if (_target == null)
		{
			LogWarning("Target RectTransform is null");
			return;
		}

		float duration = _speedDegPerSec <= 0.01f ? 1f : 360f / Mathf.Abs(_speedDegPerSec);
		float z = _clockwise ? -360f : 360f;
		_spinTween = _target
			.DORotate(new Vector3(0f, 0f, z), duration, RotateMode.FastBeyond360)
			.SetEase(Ease.Linear)
			.SetLoops(-1, LoopType.Restart)
			.SetUpdate(_useUnscaledTime);
		Log("[Spin] - started");
	}

	private void KillSpin()
	{
		if (_spinTween != null && _spinTween.IsActive())
		{
			_spinTween.Kill(false);
			_spinTween = null;
			Log("[Spin] - stopped");
		}
	}
}

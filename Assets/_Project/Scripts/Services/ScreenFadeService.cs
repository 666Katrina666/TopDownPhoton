using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Core.Base;

/// <summary>
/// Сервис для управления экранным затемнением через CanvasGroup
/// </summary>
public class ScreenFadeService : LoggableMonoBehaviour, IScreenFadeService
{
	#region Dependencies
	[Inject] private ITweenService _tweenService;
	#endregion

	#region State
	private Canvas _overlayCanvas;
	private CanvasGroup _canvasGroup;
	private Tween _activeTween;
	private bool _isQuitting;
	#endregion

	#region Unity Callbacks
	private void Awake()
	{
		EnsureOverlay();
		Log("[FadeFlow] - Initialized overlay (on Awake)");
	}

	private void OnApplicationQuit()
	{
		_isQuitting = true;
		KillActiveTween();
	}

	private void OnDestroy()
	{
		_isQuitting = true;
		KillActiveTween();
	}
	#endregion

	#region IScreenFadeService
	public async Task FadeOut(float duration = 0.25f)
	{
		if (_isQuitting) return;
		EnsureOverlay();
		if (_isQuitting || _canvasGroup == null) return;
		_canvasGroup.blocksRaycasts = true;
		_canvasGroup.interactable = false;
		Log($"[FadeFlow] - FadeOut start, duration={duration}, alpha(before)={_canvasGroup.alpha:0.00}");
		StartFade(1f, duration, Ease.InOutQuad);
		await Wait(duration);
		if (_isQuitting || _canvasGroup == null) return;
		_canvasGroup.alpha = 1f;
		Log($"[FadeFlow] - FadeOut done, alpha(after)={_canvasGroup.alpha:0.00}");
	}

	public async Task FadeIn(float duration = 0.25f)
	{
		if (_isQuitting) return;
		EnsureOverlay();
		if (_isQuitting || _canvasGroup == null) return;
		Log($"[FadeFlow] - FadeIn start, duration={duration}, alpha(before)={_canvasGroup.alpha:0.00}");
		StartFade(0f, duration, Ease.InOutQuad);
		await Wait(duration);
		if (_isQuitting || _canvasGroup == null) return;
		_canvasGroup.alpha = 0f;
		_canvasGroup.blocksRaycasts = false;
		Log($"[FadeFlow] - FadeIn done, alpha(after)={_canvasGroup.alpha:0.00}");
	}

	public void SetInstant(float alpha)
	{
		if (_isQuitting) return;
		EnsureOverlay();
		if (_canvasGroup == null) return;
		_canvasGroup.alpha = Mathf.Clamp01(alpha);
		_canvasGroup.blocksRaycasts = alpha > 0.99f;
		Log($"[FadeFlow] - SetInstant alpha={_canvasGroup.alpha:0.00}, blocksRaycasts={_canvasGroup.blocksRaycasts}");
	}
	#endregion

	#region Private
	private void EnsureOverlay()
	{
		if (_isQuitting) return;
		if (_overlayCanvas != null && _canvasGroup != null)
		{
			return;
		}

		var go = new GameObject("ScreenFadeOverlay");
		DontDestroyOnLoad(go);
		_overlayCanvas = go.AddComponent<Canvas>();
		_overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		_overlayCanvas.sortingOrder = 32760; // близко к максимуму, чтобы быть поверх UI
		go.AddComponent<CanvasScaler>();
		go.AddComponent<GraphicRaycaster>();

		var rect = go.GetComponent<RectTransform>();
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;

		var imageGO = new GameObject("FadeImage");
		imageGO.transform.SetParent(go.transform, false);
		var image = imageGO.AddComponent<Image>();
		image.color = Color.black;
		var imageRect = imageGO.GetComponent<RectTransform>();
		imageRect.anchorMin = Vector2.zero;
		imageRect.anchorMax = Vector2.one;
		imageRect.offsetMin = Vector2.zero;
		imageRect.offsetMax = Vector2.zero;

		_canvasGroup = go.AddComponent<CanvasGroup>();
		_canvasGroup.alpha = 0f;
		_canvasGroup.blocksRaycasts = false;
		Log("[FadeFlow] - Overlay created (Canvas, Image, CanvasGroup)");
	}

	private void StartFade(float targetAlpha, float duration, Ease ease)
	{
		if (_isQuitting || _canvasGroup == null) return;
		if (_activeTween != null && _activeTween.IsActive())
		{
			Log($"[FadeFlow] - Killing active tween before new fade. Current alpha={_canvasGroup.alpha:0.00}");
			_activeTween.Kill(false);
		}
		var tween = _tweenService.FadeCanvasGroup(_canvasGroup, targetAlpha, duration, ease);
		_activeTween = tween?.SetUpdate(true).SetRecyclable(true);
	}

	private void KillActiveTween()
	{
		if (_activeTween != null && _activeTween.IsActive())
		{
			_activeTween.Kill(false);
			_activeTween = null;
		}
	}

	private static async Task Wait(float seconds)
	{
		float target = Mathf.Max(0f, seconds);
		float elapsed = 0f;
		while (elapsed < target)
		{
			await Task.Yield();
			elapsed += Time.unscaledDeltaTime;
		}
	}
	#endregion
}



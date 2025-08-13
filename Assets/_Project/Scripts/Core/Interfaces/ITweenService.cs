using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Интерфейс сервиса твинов (анимаций) для абстракции от конкретной библиотеки
/// </summary>
public interface ITweenService
{
	/// <summary>
	/// Плавно изменяет альфу у CanvasGroup
	/// </summary>
	Tween FadeCanvasGroup(CanvasGroup target, float endValue, float duration, Ease ease = Ease.Linear);

	/// <summary>
	/// Плавно масштабирует Transform
	/// </summary>
	Tween ScaleTransform(Transform target, Vector3 endValue, float duration, Ease ease = Ease.OutQuad);

	/// <summary>
	/// Плавно изменяет цвет графического элемента
	/// </summary>
	Tween ColorGraphic(Graphic target, Color endValue, float duration, Ease ease = Ease.Linear);

	/// <summary>
	/// Короткий shake для RectTransform по anchor position
	/// </summary>
	Tween ShakeAnchorPos(RectTransform target, float duration, Vector2 strength, int vibrato = 10, float randomness = 90f, bool snapping = false, bool fadeOut = true);
}



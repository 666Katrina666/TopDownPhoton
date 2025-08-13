using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

/// <summary>
/// Конфигурация поведения HealthBarView
/// </summary>
[CreateAssetMenu(fileName = "HealthBarConfig", menuName = "UI/HealthBarConfig")]
public class HealthBarConfig : ScriptableObject
{
	[FoldoutGroup("Follow")]
	[LabelText("YOffset")]
	public float yOffset = 1.5f;
	[FoldoutGroup("Follow")]
	[LabelText("Billboard")]
	public bool billboard = true;

	[FoldoutGroup("Front (instant)")]
	[LabelText("Use Slider Value")] public bool frontIsSlider = true;
	[FoldoutGroup("Back (tail)")]
	[LabelText("Use Slider Value")] public bool backIsSlider = true;

	[FoldoutGroup("Tail Tween")]
	[LabelText("Delay")] public float tailDelay = 0.2f;
	[FoldoutGroup("Tail Tween")]
	[LabelText("Duration")] public float tailDuration = 0.35f;
	[FoldoutGroup("Tail Tween")]
	[LabelText("Ease")] public Ease tailEase = Ease.OutCubic;
	[FoldoutGroup("Tail Tween")]
	[LabelText("Use Unscaled Time")] public bool useUnscaledTime = true;

	[FoldoutGroup("Visibility")]
	[LabelText("Auto Hide")] public bool autoHide = true;
	[FoldoutGroup("Visibility")]
	[LabelText("Hide Delay")] public float hideDelay = 1.25f;
}

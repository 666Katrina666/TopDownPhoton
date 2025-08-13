using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

/// <summary>
/// Конфигурация анимаций кнопки для состояний hover/click
/// </summary>
[CreateAssetMenu(fileName = "UIButtonTweenConfig", menuName = "UI/UIButtonTweenConfig")]
public class UIButtonTweenConfig : ScriptableObject
{
	#region Scale Settings
	[FoldoutGroup("Scale Settings")]
	[LabelText("Normal")] public Vector3 scaleNormal = Vector3.one;
	[FoldoutGroup("Scale Settings")]
	[LabelText("Hover")] public Vector3 scaleHover = new Vector3(1.05f, 1.05f, 1f);
	[FoldoutGroup("Scale Settings")]
	[LabelText("Pressed")] public Vector3 scalePressed = new Vector3(0.95f, 0.95f, 1f);
	[FoldoutGroup("Scale Settings")]
	[LabelText("Disabled")] public Vector3 scaleDisabled = new Vector3(1f, 1f, 1f);

	[FoldoutGroup("Scale Settings")]
	[LabelText("Hover In Duration")] public float durationHoverIn = 0.12f;
	[FoldoutGroup("Scale Settings")]
	[LabelText("Hover Out Duration")] public float durationHoverOut = 0.12f;
	[FoldoutGroup("Scale Settings")]
	[LabelText("Press In Duration")] public float durationPressIn = 0.08f;
	[FoldoutGroup("Scale Settings")]
	[LabelText("Press Out Duration")] public float durationPressOut = 0.08f;
	[FoldoutGroup("Scale Settings")]
	[LabelText("Click Bounce Duration")] public float durationClickBounce = 0.2f;
	[FoldoutGroup("Scale Settings")]
	[LabelText("Click Bounce Multiplier")] public float clickBounceMultiplier = 1.08f;

	[FoldoutGroup("Scale Settings")]
	[LabelText("Hover Ease")] public Ease easeHover = Ease.OutQuad;
	[FoldoutGroup("Scale Settings")]
	[LabelText("Press Ease")] public Ease easePress = Ease.OutQuad;
	[FoldoutGroup("Scale Settings")]
	[LabelText("Bounce Ease")] public Ease easeBounce = Ease.OutBack;
	#endregion

	#region General
	[FoldoutGroup("General")]
	[LabelText("Use Unscaled Time")] public bool useUnscaledTime = true;
	#endregion
}



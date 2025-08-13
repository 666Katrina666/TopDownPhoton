using UnityEngine;
using Fusion;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Меняет спрайт манекена в зависимости от здоровья.
/// Последний спрайт в массиве отображается только при HP == 0.
/// Остальные спрайты (0..Length-2) соответствуют уровням HP > 0.
/// Порядок: 0 при полном HP, далее возрастает при убывании HP.
/// </summary>
public class HealthSpriteView : LoggableMonoBehaviour
{
	#region Inspector
	[FoldoutGroup("Dependencies")]
	[SerializeField] private SpriteRenderer _renderer;
	[FoldoutGroup("Dependencies")]
	[SerializeField] private NetworkObject _target;
	[FoldoutGroup("Settings")]
	[InfoBox("Последний спрайт — для смерти (HP == 0). Остальные — уровни HP > 0. Нужно минимум 2 спрайта.")]
	[SerializeField] private Sprite[] _sprites = System.Array.Empty<Sprite>();
	#endregion

	#region Runtime
	private int _lastIndex = -1;
	#endregion

	private void Awake()
	{
		if (_renderer == null) _renderer = GetComponentInChildren<SpriteRenderer>();
		if (_target == null) _target = GetComponentInParent<NetworkObject>();
	}

	private void OnEnable()
	{
		EventBus.Subscribe<HealthChangedEvent>(OnHealthChanged);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe<HealthChangedEvent>(OnHealthChanged);
	}

	private void OnHealthChanged(HealthChangedEvent evt)
	{
		if (_target == null || evt.Target != _target) return;
		if (_renderer == null) return;
		if (_sprites == null || _sprites.Length < 2)
		{
			LogWarning("Sprites array must contain at least 2 elements (last is death)");
			return;
		}

		int index = ComputeIndex(evt.Current, evt.Max, _sprites.Length);
		if (index < 0 || index >= _sprites.Length) return;
		if (_lastIndex == index) return;
		_lastIndex = index;
		_renderer.sprite = _sprites[index];
		Log($"Changed sprite to index {index} (HP={evt.Current}/{evt.Max})");
	}

	private static int ComputeIndex(int current, int max, int totalSprites)
	{
		int deathIndex = totalSprites - 1;
		if (max <= 0) return deathIndex;
		if (current <= 0) return deathIndex;
		int levelsCount = Mathf.Max(1, totalSprites - 1); // количество уровней >0
		float ratio = Mathf.Clamp01((float)current / max); // 1 at full HP, 0 at zero
		float inverted = 1f - ratio; // 0 at full HP -> index 0
		int level = Mathf.Clamp(Mathf.CeilToInt(levelsCount * inverted), 1, levelsCount) - 1; // 0..levelsCount-1
		return level;
	}
}

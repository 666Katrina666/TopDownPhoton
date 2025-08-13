using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Zenject;
using Core.Base;
using DG.Tweening;

/// <summary>
/// Делает указанный UI-элемент интерактивным только для хоста (сервера).
/// Для клиентов кнопка/селектабл будет неактивным.
/// </summary>
public class HostOnlyInteractable : LoggableMonoBehaviour
{
	#region UI References
	[FoldoutGroup("UI References")]
	[SerializeField] private Selectable _target;
	#endregion

	#region Settings
	[FoldoutGroup("Settings")]
	[Tooltip("Если включено — для клиентов элемент будет отключён")] 
	[SerializeField] private bool _disableForClients = true;
	[FoldoutGroup("Settings")]
	[SerializeField] private float _shakeDuration = 0.15f;
	[FoldoutGroup("Settings")]
	[SerializeField] private Vector2 _shakeStrength = new Vector2(10f, 0f);
	[FoldoutGroup("Settings")]
	[SerializeField] private int _shakeVibrato = 10;
	[FoldoutGroup("Settings")]
	[SerializeField] private float _shakeRandomness = 90f;
	[FoldoutGroup("Settings")]
	[SerializeField] private float _shakeCooldown = 0.25f;
	#endregion

	#region Dependencies
    [FoldoutGroup("Dependencies")]
	[ShowInInspector, Sirenix.OdinInspector.ReadOnly]
	private INetworkService _networkService;
	[FoldoutGroup("Dependencies")]
	[ShowInInspector, Sirenix.OdinInspector.ReadOnly]
	private ITweenService _tweenService;

	[Inject]
	private void Construct(INetworkService networkService, ITweenService tweenService)
	{
		_networkService = networkService;
		_tweenService = tweenService;
	}
	#endregion

	#region State
	private RectTransform _rectTransform;
	private Button _button;
	private float _lastShakeTime;
	#endregion

	#region Unity Callbacks
	private void Awake()
	{
		if (_target == null)
		{
			_target = GetComponent<Selectable>();
		}
		_rectTransform = (_target != null ? _target.transform : transform) as RectTransform;
		_button = GetComponent<Button>();
	}

	private void OnEnable()
	{
		SubscribeToEvents();
		SubscribeToUI();
		Refresh();
	}

	private void OnDisable()
	{
		UnsubscribeFromEvents();
		UnsubscribeFromUI();
	}
	#endregion

	#region EventBus
	private void SubscribeToEvents()
	{
		EventBus.Subscribe<NetworkConnectedEvent>(OnNetworkConnected);
		EventBus.Subscribe<SceneChangedEvent>(OnSceneChanged);
	}

	private void UnsubscribeFromEvents()
	{
		EventBus.Unsubscribe<NetworkConnectedEvent>(OnNetworkConnected);
		EventBus.Unsubscribe<SceneChangedEvent>(OnSceneChanged);
	}

	private void OnNetworkConnected(NetworkConnectedEvent evt)
	{
		Refresh();
	}

	private void OnSceneChanged(SceneChangedEvent evt)
	{
		Refresh();
	}
	#endregion

	#region UI Hooks
	private void SubscribeToUI()
	{
		if (_button != null)
		{
			_button.onClick.AddListener(OnButtonClicked);
		}
	}

	private void UnsubscribeFromUI()
	{
		if (_button != null)
		{
			_button.onClick.RemoveListener(OnButtonClicked);
		}
	}

	private void OnButtonClicked()
	{
		bool isServer = _networkService != null && _networkService.IsServer;
		if (!isServer)
		{
			TryShakeDenied();
		}
	}
	#endregion

	#region Logic
	/// <summary>
	/// Обновляет состояние интерактивности целевого элемента в зависимости от роли (хост/клиент).
	/// </summary>
	private void Refresh()
	{
		if (_target == null)
		{
			return;
		}

		bool isServer = _networkService != null && _networkService.IsServer;
		if (isServer)
		{
			_target.interactable = true;
		}
		else
		{
			_target.interactable = !_disableForClients;
		}

		Log($"refreshed, IsServer={isServer}, Interactable={_target.interactable}");
	}

	private void TryShakeDenied()
	{
		if (_rectTransform == null || _tweenService == null)
		{
			return;
		}
		if (Time.unscaledTime - _lastShakeTime < _shakeCooldown)
		{
			return;
		}
		_lastShakeTime = Time.unscaledTime;
		_tweenService.ShakeAnchorPos(_rectTransform, _shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness, false, true)
			?.SetUpdate(true);
		Log("[Access denied] - shake");
	}
	#endregion
}



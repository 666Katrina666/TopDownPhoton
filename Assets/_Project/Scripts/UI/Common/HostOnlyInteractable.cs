using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Zenject;
using Core.Base;

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
	#endregion

	#region Dependencies
    [FoldoutGroup("Dependencies")]
	[ShowInInspector, Sirenix.OdinInspector.ReadOnly]
	private INetworkService _networkService;

	[Inject]
	private void Construct(INetworkService networkService)
	{
		_networkService = networkService;
	}
	#endregion

	#region Unity Callbacks
	private void Awake()
	{
		if (_target == null)
		{
			_target = GetComponent<Selectable>();
		}
	}

	private void OnEnable()
	{
		SubscribeToEvents();
		Refresh();
	}

	private void OnDisable()
	{
		UnsubscribeFromEvents();
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
	#endregion
}



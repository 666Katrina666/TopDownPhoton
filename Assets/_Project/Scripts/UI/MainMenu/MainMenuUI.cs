using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// UI контроллер для главного меню
/// </summary>
public class MainMenuUI : LoggableMonoBehaviour
{
    #region UI References
    [FoldoutGroup("UI References")]
    [InfoBox("Ссылки на UI элементы")]
    [SerializeField] private Button _hostButton;
    [FoldoutGroup("UI References")]
    [SerializeField] private Button _joinButton;
    [FoldoutGroup("UI References")]
    [SerializeField] private TextMeshProUGUI _statusText;
    [FoldoutGroup("UI References")]
    [SerializeField] private GameObject _loadingIndicator;
    [FoldoutGroup("UI References")]
    [SerializeField] private TextMeshProUGUI _errorText;
    #endregion

    #region Runtime Data
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private ConnectionState _currentState = ConnectionState.Disconnected;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        ValidateUIReferences();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void Start()
    {
        SubscribeToEvents();
        UpdateUI();
    }
    #endregion
    
    private void SubscribeToEvents()
    {
        EventBus.Subscribe<ConnectionStateChangedEvent>(OnConnectionStateChanged);
        EventBus.Subscribe<ConnectionErrorEvent>(OnConnectionError);
    }
    
    private void UnsubscribeFromEvents()
    {
        EventBus.Unsubscribe<ConnectionStateChangedEvent>(OnConnectionStateChanged);
        EventBus.Unsubscribe<ConnectionErrorEvent>(OnConnectionError);
    }
    private void OnConnectionStateChanged(ConnectionStateChangedEvent evt)
    {
        _currentState = evt.State;
        UpdateUI();
        
        if (_statusText != null)
        {
            _statusText.text = evt.Message;
        }
    }
    
    private void OnConnectionError(ConnectionErrorEvent evt)
    {
        LogError($"Connection error: {evt.ErrorMessage}");
        
        _currentState = ConnectionState.Error;
        UpdateUI();
        
        if (_errorText != null)
        {
            _errorText.text = evt.ErrorMessage;
            _errorText.gameObject.SetActive(true);
        }
    }
    
    private void ValidateUIReferences()
    {
        if (_hostButton == null)
        {
            LogWarning("Host button not assigned!");
        }
        
        if (_joinButton == null)
        {
            LogWarning("Join button not assigned!");
        }
        
        if (_statusText == null)
        {
            LogWarning("Status text not assigned!");
        }
        
        if (_loadingIndicator == null)
        {
            LogWarning("Loading indicator not assigned!");
        }
        
        if (_errorText == null)
        {
            LogWarning("Error text not assigned!");
        }
    }
    
    private void UpdateUI()
    {
        bool isConnecting = _currentState == ConnectionState.Connecting;
        bool isError = _currentState == ConnectionState.Error;
        
        if (_hostButton != null)
        {
            _hostButton.interactable = !isConnecting;
        }
        
        if (_joinButton != null)
        {
            _joinButton.interactable = !isConnecting;
        }
        
        if (_loadingIndicator != null)
        {
            _loadingIndicator.SetActive(isConnecting);
        }
        
        if (_errorText != null && !isError)
        {
            _errorText.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Очищает сообщение об ошибке
    /// </summary>
    public void ClearError()
    {
        if (_errorText != null)
        {
            _errorText.gameObject.SetActive(false);
        }
        
        if (_currentState == ConnectionState.Error)
        {
            _currentState = ConnectionState.Disconnected;
            UpdateUI();
        }
    }
    
    /// <summary>
    /// Устанавливает текст статуса
    /// </summary>
    public void SetStatusText(string text)
    {
        if (_statusText != null)
        {
            _statusText.text = text;
        }
    }
} 
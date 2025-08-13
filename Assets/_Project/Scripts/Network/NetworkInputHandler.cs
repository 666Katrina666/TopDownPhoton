using UnityEngine;
using Fusion;
using Sirenix.OdinInspector;
using Core.Base;
using Zenject;

/// <summary>
/// Глобальный обработчик ввода для сетевой игры
/// Собирает ввод с InputActions и передает его в NetworkCallbackHandler
/// Должен находиться на сцене GameScene (НЕ на префабе игрока)
/// </summary>
public class NetworkInputHandler : NetworkCallbackBase
{
    #region Runtime Data
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private bool _isConnected;
    
    [Inject] private NetworkRunner _networkRunner;
    
    private InputActions _inputActions;
    
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    [SerializeField] private Camera _camera;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        _inputActions = new InputActions();
    }
    
    private void Start()
    {
        _isConnected = _networkRunner != null;
        
        Log($"Initialized. Connected: {_isConnected}");
        
        // Регистрируем себя как callback в NetworkRunner
        if (_networkRunner != null)
        {
            _networkRunner.AddCallbacks(this);
            Log("Registered as NetworkRunner callback");
        }

        if (_camera == null)
        {
            _camera = Camera.main;
            Log("Camera cached from Camera.main");
        }
    }
    
    private void OnEnable()
    {
        _inputActions?.Enable();
    }
    
    private void OnDisable()
    {
        _inputActions?.Disable();
    }
    
    private void OnDestroy()
    {
        _inputActions?.Dispose();
    }
    #endregion
    
    /// <summary>
    /// Callback для обработки ввода от NetworkRunner
    /// Передает данные ввода в сеть
    /// </summary>
    public override void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData inputData = GetInputData();
        input.Set(inputData);
    }
    
    /// <summary>
    /// Обрабатывает ввод игрока и возвращает данные для сети
    /// </summary>
    /// <returns>Данные ввода для передачи по сети</returns>
    public NetworkInputData GetInputData()
    {
        Vector2 direction = GetMovementInput();
        bool isFiring = GetFireInput();
        
        return new NetworkInputData
        {
            moveDirection = direction,
            isMoving = direction.magnitude > 0.1f,
            isFiring = isFiring,
            mouseWorldPosition = GetMouseWorldPosition()
        };
    }
    
    private Vector2 GetMovementInput()
    {
        return _inputActions.Movement.Move.ReadValue<Vector2>();
    }

    private bool GetFireInput()
    {
        bool pressed = _inputActions.Combat.FirePrimary.WasPressedThisFrame();
        if (pressed)
            Debug.Log("[NetworkInputHandler] - FirePrimary action WasPressedThisFrame");
        return pressed;
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector2 screenPos = _inputActions.Combat.MousePosition.ReadValue<Vector2>();

        if (_camera == null)
        {
            _camera = Camera.main;
        }

        if (_camera != null)
        {
            Vector3 sp = new Vector3(screenPos.x, screenPos.y, -_camera.transform.position.z);
            Vector3 wp = _camera.ScreenToWorldPoint(sp);
            return new Vector2(wp.x, wp.y);
        }

        return screenPos;
    }
}

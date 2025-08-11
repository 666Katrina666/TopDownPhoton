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
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private bool _isConnected;
    
    [Inject] private NetworkRunner _networkRunner;
    
    private InputActions _inputActions;
    
    private void Awake()
    {
        _inputActions = new InputActions();
    }
    
    private void Start()
    {
        _isConnected = _networkRunner != null;
        
        Log($"[NetworkInputHandler] - Initialized. Connected: {_isConnected}");
        
        // Регистрируем себя как callback в NetworkRunner
        if (_networkRunner != null)
        {
            _networkRunner.AddCallbacks(this);
            Log("[NetworkInputHandler] - Registered as NetworkRunner callback");
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
    
    /// <summary>
    /// Callback для обработки ввода от NetworkRunner
    /// Передает данные ввода в сеть
    /// </summary>
    public override void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData inputData = GetInputData();
        input.Set(inputData);
        if (inputData.isFiring)
        {
            Debug.Log($"[NetworkInputHandler] - Fire captured this frame. Dir={inputData.moveDirection}");
        }
    }
    
    /// <summary>
    /// Обрабатывает ввод игрока и возвращает данные для сети
    /// </summary>
    /// <returns>Данные ввода для передачи по сети</returns>
    public NetworkInputData GetInputData()
    {
        Vector2 direction = GetMovementInput();
        bool isInteracting = GetInteractionInput();
        bool isFiring = GetFireInput();
        
        return new NetworkInputData
        {
            moveDirection = direction,
            isMoving = direction.magnitude > 0.1f,
            isInteracting = isInteracting,
            isFiring = isFiring
        };
    }
    
    private Vector2 GetMovementInput()
    {
        if (_inputActions != null)
        {
            return _inputActions.Movement.Move.ReadValue<Vector2>();
        }
        
        // Fallback на старый ввод, если InputActions не инициализированы
        Vector2 direction = Vector2.zero;
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) direction += Vector2.up;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) direction += Vector2.down;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) direction += Vector2.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) direction += Vector2.right;
        
        // Нормализуем вектор для равномерной скорости по диагонали
        return Vector2.ClampMagnitude(direction, 1f);
    }
    
    private bool GetInteractionInput()
    {
        // Используем старый ввод для взаимодействия, так как в локальном InputActions нет действия Interact
        return Input.GetKey(KeyCode.E);
    }

    private bool GetFireInput()
    {
        // Если в InputActions добавлено действие FirePrimary, используем его, иначе fallback на ЛКМ
        try
        {
            var fire = _inputActions?.FindAction("Combat/FirePrimary", throwIfNotFound: false);
            if (fire != null)
            {
                bool pressed = fire.WasPressedThisFrame();
                if (pressed)
                {
                    Debug.Log("[NetworkInputHandler] - FirePrimary action WasPressedThisFrame");
                }
                return pressed;
            }
        }
        catch { /* игнорируем отсутствие карты действий */ }

        return Input.GetMouseButtonDown(0);
    }
}

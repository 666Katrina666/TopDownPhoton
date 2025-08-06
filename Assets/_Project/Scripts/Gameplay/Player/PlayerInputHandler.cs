using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

/// <summary>
/// Обрабатывает ввод игрока и отправляет его по сети
/// </summary>
public class PlayerInputHandler : NetworkBehaviour, INetworkInput
{
    private InputActions _inputActions;
    private Vector2 _currentMoveDirection;

    public override void Spawned()
    {
        _inputActions = new InputActions();
        _inputActions.Enable();
        
        // Подписываемся на события движения
        _inputActions.Movement.Move.performed += OnMovePerformed;
        _inputActions.Movement.Move.canceled += OnMoveCanceled;
        
        Debug.Log($"[PlayerInputHandler] - Компонент инициализирован, HasInputAuthority: {Object.HasInputAuthority}");
        Debug.Log($"[PlayerInputHandler] - InputActions созданы и включены");
        Debug.Log($"[PlayerInputHandler] - Подписка на события движения установлена");
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (_inputActions != null)
        {
            _inputActions.Movement.Move.performed -= OnMovePerformed;
            _inputActions.Movement.Move.canceled -= OnMoveCanceled;
            _inputActions.Disable();
            _inputActions.Dispose();
        }
        
        Debug.Log($"[PlayerInputHandler] - Компонент уничтожен, InputActions очищены");
    }

    private void OnEnable()
    {
        if (_inputActions != null)
        {
            _inputActions.Enable();
            Debug.Log($"[PlayerInputHandler] - InputActions включены");
        }
    }

    private void OnDisable()
    {
        if (_inputActions != null)
        {
            _inputActions.Disable();
            Debug.Log($"[PlayerInputHandler] - InputActions отключены");
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _currentMoveDirection = context.ReadValue<Vector2>();
        Debug.Log($"[PlayerInputHandler] - Начало движения: {_currentMoveDirection}, HasInputAuthority: {Object.HasInputAuthority}");
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _currentMoveDirection = Vector2.zero;
        Debug.Log($"[PlayerInputHandler] - Остановка движения, HasInputAuthority: {Object.HasInputAuthority}");
    }

    /// <summary>
    /// Отправляет ввод по сети (вызывается Fusion автоматически)
    /// </summary>
    public void GetInput(out NetworkInputData data)
    {
        data.moveDirection = _currentMoveDirection;
        data.isMoving = _currentMoveDirection.magnitude > 0.1f;
        
        Debug.Log($"[PlayerInputHandler] - Отправляем ввод по сети: {data.moveDirection}, isMoving: {data.isMoving}, HasInputAuthority: {Object.HasInputAuthority}");
    }
} 
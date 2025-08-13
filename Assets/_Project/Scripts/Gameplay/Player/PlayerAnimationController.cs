using UnityEngine;
using Fusion;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Контроллер анимаций персонажа с поддержкой сетевой синхронизации
/// Управляет параметрами Direction и IsMoving для Animator
/// </summary>
public class PlayerAnimationController : LoggableNetworkBehaviour
{
    private enum MovementDirection
    {
        Front = 0,
        Back = 1,
        Left = 2,
        Right = 3
    }
    
    #region Animation Components
    [FoldoutGroup("Animation Components")]
    [InfoBox("Компоненты анимации")]
    [SerializeField] private Animator _animator;
    [FoldoutGroup("Animation Components")]
    [SerializeField] private NetworkMecanimAnimator _networkMecanimAnimator;
    #endregion

    #region Settings
    [FoldoutGroup("Animation Settings")]
    [InfoBox("Настройки анимаций")]
    [SerializeField] private float _movementThreshold = 0.1f;
    [FoldoutGroup("Animation Settings")]
    [SerializeField] private float _directionThreshold = 0.5f;
    #endregion

    #region Runtime Data
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private int _currentDirection;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private bool _currentIsMoving;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private Vector2 _lastMoveDirection;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private int _lastAppliedDirection = int.MinValue;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private bool _lastAppliedIsMoving;
    #endregion

    #region Networked
    [Networked] private int NetworkedDirection { get; set; }
    [Networked] private bool NetworkedIsMoving { get; set; }
    #endregion

    #region Constants
    private static readonly int DIRECTION_HASH = Animator.StringToHash("Direction");
    private static readonly int IS_MOVING_HASH = Animator.StringToHash("IsMoving");
    #endregion
    
    #region Unity Callbacks
    private void Awake()
    {
        ValidateComponents();
    }
    
    public override void Spawned()
    {
        Log("[Спавн] - Анимационный контроллер заспавнен");
        ApplyNetworkedParamsToAnimator();
    }
    
    public override void FixedUpdateNetwork()
    {
        ApplyNetworkedParamsToAnimator();
    }
    #endregion
    
    /// <summary>
    /// Обновляет параметры анимации на основе ввода
    /// </summary>
    public void UpdateAnimationParameters(NetworkInputData input)
    {
        bool isMoving = input.isMoving && input.moveDirection.magnitude > _movementThreshold;
        int direction = DetermineDirection(input.moveDirection);
        
        if (direction != _currentDirection || isMoving != _currentIsMoving)
        {
            _currentDirection = direction;
            _currentIsMoving = isMoving;
            _lastMoveDirection = input.moveDirection;
            
            if (Object.HasStateAuthority)
            {
                NetworkedDirection = direction;
                NetworkedIsMoving = isMoving;
            }
            
            if (Object.HasInputAuthority)
            {
                ApplyAnimatorParams(direction, isMoving);
            }
            
            Log($"[Обновление анимации] - Direction: {direction}, IsMoving: {isMoving}");
        }
    }
    
    /// <summary>
    /// Определяет направление на основе вектора движения
    /// </summary>
    private int DetermineDirection(Vector2 moveDirection)
    {
        if (moveDirection.magnitude < _directionThreshold)
        {
            return _currentDirection; // Сохраняем последнее направление
        }
        
        if (Mathf.Abs(moveDirection.y) > Mathf.Abs(moveDirection.x))
        {
            return moveDirection.y > 0 ? (int)MovementDirection.Back : (int)MovementDirection.Front;
        }
        else
        {
            return moveDirection.x < 0 ? (int)MovementDirection.Left : (int)MovementDirection.Right;
        }
    }
    
    /// <summary>
    /// Применяет сетевые параметры к Animator
    /// </summary>
    private void ApplyNetworkedParamsToAnimator()
    {
        ApplyAnimatorParams(NetworkedDirection, NetworkedIsMoving);
    }
    
    /// <summary>
    /// Применяет параметры к Animator с защитой от лишних записей
    /// </summary>
    private void ApplyAnimatorParams(int direction, bool isMoving)
    {
        if (_animator == null)
            return;

        bool changed = false;

        if (_lastAppliedDirection != direction)
        {
            _animator.SetInteger(DIRECTION_HASH, direction);
            _lastAppliedDirection = direction;
            changed = true;
        }

        if (_lastAppliedIsMoving != isMoving)
        {
            _animator.SetBool(IS_MOVING_HASH, isMoving);
            _lastAppliedIsMoving = isMoving;
            changed = true;
        }

        if (changed)
        {
            Log($"[Animator] - Applied params Direction={direction}, IsMoving={isMoving}");
        }
    }

    /// <summary>
    /// Проверяет и настраивает необходимые компоненты
    /// </summary>
    private void ValidateComponents()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                LogError("[Валидация] - Animator не найден!");
            }
        }
        
        if (_networkMecanimAnimator == null)
        {
            _networkMecanimAnimator = GetComponent<NetworkMecanimAnimator>();
            if (_networkMecanimAnimator == null)
            {
                LogWarning("[Валидация] - NetworkMecanimAnimator не найден");
            }
        }
    }
    
    /// <summary>
    /// Получает текущее направление персонажа
    /// </summary>
    public int GetCurrentDirection() => _currentDirection;
    
    /// <summary>
    /// Получает текущее состояние движения
    /// </summary>
    public bool GetCurrentIsMoving() => _currentIsMoving;
}

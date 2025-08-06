using UnityEngine;
using Fusion;

/// <summary>
/// Управляет движением игрока на хосте
/// Получает ввод от клиентов и применяет движение
/// </summary>
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 15f;

    private Vector2 _currentVelocity;

    public override void FixedUpdateNetwork()
    {
        // Получаем ввод только на хосте
        if (GetInput(out NetworkInputData data))
        {
            // Применяем движение на основе полученного ввода
            ApplyMovement(data.moveDirection, data.isMoving);
            
            Debug.Log($"[PlayerMovement] - Получен ввод: {data.moveDirection}, isMoving: {data.isMoving}, HasInputAuthority: {Object.HasInputAuthority}");
        }
        else
        {
            // Если нет ввода, применяем замедление
            ApplyMovement(Vector2.zero, false);
        }
    }

    private void ApplyMovement(Vector2 moveDirection, bool isMoving)
    {
        // Вычисляем целевую скорость
        Vector2 targetVelocity = moveDirection * _moveSpeed;
        
        // Применяем ускорение или замедление
        float accelerationRate = isMoving ? _acceleration : _deceleration;
        _currentVelocity = Vector2.MoveTowards(_currentVelocity, targetVelocity, accelerationRate * Runner.DeltaTime);
        
        // Применяем движение к Transform
        Vector3 newPosition = transform.position + new Vector3(_currentVelocity.x, _currentVelocity.y, 0) * Runner.DeltaTime;
        transform.position = newPosition;
        
        Debug.Log($"[PlayerMovement] - ApplyMovement: moveDirection={moveDirection}, targetVelocity={targetVelocity}, _currentVelocity={_currentVelocity}, HasInputAuthority={Object.HasInputAuthority}");
        
        if (isMoving)
        {
            Debug.Log($"[PlayerMovement] - Применяем движение: позиция={transform.position}, скорость={_currentVelocity}");
        }
    }
} 
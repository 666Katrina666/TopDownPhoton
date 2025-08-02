using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Обрабатывает ввод игрока с использованием FixedUpdateNetwork для лучшей сетевой синхронизации
/// </summary>
public class PlayerInputHandler : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    private InputActions inputActions;
    private Rigidbody2D rb;

    public override void Spawned()
    {
        Debug.Log("[PlayerInputHandler_Legacy] - Player spawned");
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
        }
        
        if (Object.HasInputAuthority)
        {
            inputActions = new InputActions();
            inputActions.Enable();
            Debug.Log("[PlayerInputHandler_Legacy] - Input authority detected, input actions enabled");
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        Debug.Log("[PlayerInputHandler_Legacy] - Player despawned");
        inputActions?.Disable();
        inputActions?.Dispose();
    }

    private void FixedUpdate()
    {
        if (Object != null && Object.HasInputAuthority && inputActions != null && inputActions.Movement.Move.ReadValue<Vector2>().magnitude > 0.1f)
        {
            Vector2 movement = inputActions.Movement.Move.ReadValue<Vector2>().normalized * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
        }
    }
} 
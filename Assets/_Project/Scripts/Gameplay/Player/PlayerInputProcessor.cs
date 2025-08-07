using UnityEngine;
using Fusion;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Обработчик ввода для конкретного игрока
/// Получает данные ввода из сети и применяет их к персонажу
/// </summary>
public class PlayerInputProcessor : LoggableNetworkBehaviour
{
    [FoldoutGroup("Movement Settings")]
    [InfoBox("Настройки движения")]
    [SerializeField] private float _moveSpeed = 5f;
    
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private NetworkRunner _networkRunner;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private PlayerRef _playerRef;
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private bool _hasInputAuthority;
    
    private NetworkObject _networkObject;
    private Rigidbody2D _rigidbody;
    
    private void Awake()
    {
        _networkObject = GetComponent<NetworkObject>();
        _rigidbody = GetComponent<Rigidbody2D>();
        
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0f;
            _rigidbody.linearDamping = 0f;
            _rigidbody.angularDamping = 0f;
        }
    }
    
    public override void Spawned()
    {
        _networkRunner = Runner;
        _playerRef = Object.InputAuthority;
        _hasInputAuthority = Object.HasInputAuthority;
        
        Log($"PlayerInputProcessor spawned. PlayerRef: {_playerRef}, HasInputAuthority: {_hasInputAuthority}");
    }
    
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            ProcessInput(input);
        }
    }
    
    private void ProcessInput(NetworkInputData input)
    {
        if (input.isMoving)
        {
            Vector2 movement = input.moveDirection * _moveSpeed * Runner.DeltaTime;
            _rigidbody.MovePosition(_rigidbody.position + movement);
        }
    }
}

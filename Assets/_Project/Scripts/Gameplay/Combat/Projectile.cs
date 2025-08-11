using Fusion;
using UnityEngine;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Сетевой снаряд с ограниченным временем жизни и уроном по целям
/// </summary>
public class Projectile : LoggableNetworkBehaviour
{
    [FoldoutGroup("Runtime Data", false)]
    [Networked] private Vector2 Direction { get; set; }

    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private float _speed;

    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private int _damage;

    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    private int _despawnTick;
    private float _lifetimeSeconds;

    /// <summary>
    /// Инициализация параметров снаряда. Вызывается фабрикой до завершения спавна
    /// </summary>
    public void Initialize(Vector2 direction, float speed, float lifetimeSeconds, int damage)
    {
        Direction = direction.sqrMagnitude > 0 ? direction.normalized : Vector2.right;
        _speed = Mathf.Max(0f, speed);
        _damage = Mathf.Max(0, damage);
        _lifetimeSeconds = Mathf.Max(0.01f, lifetimeSeconds);
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            _despawnTick = Runner.Tick + Mathf.CeilToInt(_lifetimeSeconds / Runner.DeltaTime);
        }
        Log("[Projectile] - Заспавнен");
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority == false)
        {
            return;
        }

        // Движение вперёд
        transform.position += (Vector3)(Direction * _speed * Runner.DeltaTime);

        // Принудительное уничтожение по времени жизни
        if (_despawnTick > 0 && Runner.Tick >= _despawnTick)
        {
            Log("[Projectile] - Lifetime expired, despawn");
            Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Object.HasStateAuthority == false)
        {
            return;
        }

        // Игнорируем владельца
        var otherNo = other.GetComponentInParent<NetworkObject>();
        if (otherNo != null && otherNo.InputAuthority == Object.InputAuthority)
        {
            return;
        }

        var health = other.GetComponentInParent<Health>();
        if (health != null)
        {
            health.ApplyDamage(_damage, Object.InputAuthority);
            Log($"[Projectile] - Hit {other.name}, damage={_damage}");
            Runner.Despawn(Object);
        }
        else
        {
            Debug.Log($"[Projectile] - Trigger with {other.name}, no Health component found");
        }
    }
}



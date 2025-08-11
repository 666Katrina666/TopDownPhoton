using Fusion;
using UnityEngine;
using Zenject;

/// <summary>
/// Реализация фабрики снарядов через Fusion Runner
/// </summary>
public class ProjectileFactory : IProjectileFactory
{
    private readonly NetworkRunner _networkRunner;
    private readonly CombatConfig _combatConfig;

    [Inject]
    public ProjectileFactory(NetworkRunner runner, CombatConfig combatConfig)
    {
        _networkRunner = runner;
        _combatConfig = combatConfig;
        Debug.Log("[ProjectileFactory] - Зависимости инжектированы");
    }

    /// <inheritdoc />
    public Projectile Spawn(PlayerRef owner, Vector2 position, Vector2 direction)
    {
        if (_networkRunner == null)
        {
            Debug.LogError("[ProjectileFactory] - NetworkRunner null");
            return null;
        }

        if (!_combatConfig || !_combatConfig.ProjectilePrefab.IsValid)
        {
            Debug.LogError("[ProjectileFactory] - CombatConfig или Projectile Prefab не назначены");
            return null;
        }

        Vector2 normalizedDirection = direction.sqrMagnitude > 0 ? direction.normalized : Vector2.right;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, new Vector3(normalizedDirection.x, normalizedDirection.y, 0f));

        Projectile spawnedProjectile = null;

        NetworkObject obj = _networkRunner.Spawn(
            _combatConfig.ProjectilePrefab,
            position,
            rotation,
            owner,
            onBeforeSpawned: (runner, o) =>
            {
                spawnedProjectile = o.GetComponent<Projectile>();
                if (spawnedProjectile == null)
                {
                    spawnedProjectile = o.GetComponentInChildren<Projectile>();
                }
                if (spawnedProjectile != null)
                {
                    spawnedProjectile.Initialize(
                        normalizedDirection,
                        _combatConfig.ProjectileSpeed,
                        _combatConfig.ProjectileLifetime,
                        _combatConfig.ProjectileDamage
                    );
                }
            }
        );

        if (obj == null)
        {
            Debug.LogError("[ProjectileFactory] - Runner.Spawn вернул null (NetworkObject)");
            return null;
        }

        if (spawnedProjectile == null)
        {
            // Повторная попытка получить компонент уже после спавна
            spawnedProjectile = obj.GetComponent<Projectile>() ?? obj.GetComponentInChildren<Projectile>();
            if (spawnedProjectile == null)
            {
                Debug.LogError($"[ProjectileFactory] - Компонент Projectile отсутствует на префабе '{obj.name}'. Убедитесь, что он добавлен на корень или дочерний объект префаба.");
                return null;
            }
            // Если нашли после, то инициализируем
            spawnedProjectile.Initialize(
                normalizedDirection,
                _combatConfig.ProjectileSpeed,
                _combatConfig.ProjectileLifetime,
                _combatConfig.ProjectileDamage
            );
        }

        Debug.Log($"[ProjectileFactory] - Spawn by {owner} at {position} dir={normalizedDirection}, prefabValid={_combatConfig.ProjectilePrefab.IsValid}");
        return spawnedProjectile;
    }
}



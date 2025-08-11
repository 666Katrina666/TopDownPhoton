using UnityEngine;
using Fusion;
using Sirenix.OdinInspector;

/// <summary>
/// Конфигурация боевой системы и параметров снарядов
/// </summary>
[CreateAssetMenu(fileName = "CombatConfig", menuName = "Configs/CombatConfig")]
public class CombatConfig : ScriptableObject
{
    [FoldoutGroup("Projectile"), InfoBox("Параметры снаряда и ссылка на префаб")] 
    [SerializeField] private NetworkPrefabRef _projectilePrefab;
    [FoldoutGroup("Projectile"), LabelText("Speed"), Min(0f)]
    [SerializeField] private float _projectileSpeed = 10f;
    [FoldoutGroup("Projectile"), LabelText("Lifetime (sec)"), Min(0.1f)]
    [SerializeField] private float _projectileLifetime = 3f;
    [FoldoutGroup("Projectile"), LabelText("Damage"), Min(0)]
    [SerializeField] private int _projectileDamage = 1;
    [FoldoutGroup("Projectile"), LabelText("Spawn Offset"), Min(0f)]
    [SerializeField] private float _spawnOffset = 0.6f;

    /// <summary>
    /// Префаб снаряда (Fusion NetworkPrefabRef)
    /// </summary>
    public NetworkPrefabRef ProjectilePrefab => _projectilePrefab;
    /// <summary>
    /// Скорость полёта снаряда
    /// </summary>
    public float ProjectileSpeed => _projectileSpeed;
    /// <summary>
    /// Время жизни снаряда в секундах
    /// </summary>
    public float ProjectileLifetime => _projectileLifetime;
    /// <summary>
    /// Базовый урон снаряда
    /// </summary>
    public int ProjectileDamage => _projectileDamage;
    /// <summary>
    /// Смещение точки спавна от центра персонажа
    /// </summary>
    public float SpawnOffset => _spawnOffset;
}



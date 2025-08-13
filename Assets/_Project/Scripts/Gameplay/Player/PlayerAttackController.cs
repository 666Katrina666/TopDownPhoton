using UnityEngine;
using Fusion;
using Zenject;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Обработка атаки игрока и вызов фабрики снарядов
/// </summary>
public class PlayerAttackController : LoggableNetworkBehaviour
{
    #region Settings
    [FoldoutGroup("Attack Settings"), InfoBox("Настройки атаки игрока")]
    [SerializeField, Min(0f)] private float _spawnOffsetOverride = -1f;
    #endregion

    #region Runtime Data
    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private Vector2 _lastDirection = Vector2.right;

    private IProjectileFactory _projectileFactory;
    private CombatConfig _combatConfig;
    #endregion

    #region Dependencies
    [Inject]
    private void Construct(IProjectileFactory projectileFactory, CombatConfig combatConfig)
    {
        _projectileFactory = projectileFactory;
        _combatConfig = combatConfig;
        Log("Инжекция зависимостей завершена");
    }
    #endregion

    #region Unity Callbacks

    public override void FixedUpdateNetwork()
    {
        Log($"Tick. HasStateAuthority={Object.HasStateAuthority}, HasInputAuthority={Object.HasInputAuthority}");

        if (GetInput(out NetworkInputData input) == false)
        {
            Log("Нет входных данных");
            return;
        }

        if (input.moveDirection.sqrMagnitude > 0.0001f)
        {
            _lastDirection = input.moveDirection.normalized;
        }

        if (input.isFiring == false)
        {
            return;
        }

        if (Object.HasStateAuthority == false)
        {
            Log("Fire получен, но нет StateAuthority, пропускаем спавн");
            return;
        }

        Vector2 from = transform.position;
        Vector2 to = input.mouseWorldPosition;
        Vector2 dir = (to - from);
        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = _lastDirection.sqrMagnitude > 0 ? _lastDirection : Vector2.right;
        }
        else
        {
            dir.Normalize();
        }
        float spawnOffset = _spawnOffsetOverride >= 0f && _combatConfig ? _spawnOffsetOverride : (_combatConfig ? _combatConfig.SpawnOffset : 0.6f);
        Vector2 spawnPos = from + dir * spawnOffset;

        var proj = _projectileFactory?.Spawn(Object.InputAuthority, spawnPos, dir);
        Log($"Fire input: {input.isFiring}, spawned={(proj!=null)} dir={dir} pos={spawnPos} mouse={to}");
    }
    #endregion
}



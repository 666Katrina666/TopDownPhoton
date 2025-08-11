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
    [FoldoutGroup("Attack Settings"), InfoBox("Настройки атаки игрока")]
    [SerializeField, Min(0f)] private float _spawnOffsetOverride = -1f;

    [FoldoutGroup("Runtime Data", false)]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] private Vector2 _lastDirection = Vector2.right;

    private IProjectileFactory _projectileFactory;
    private CombatConfig _combatConfig;
    private bool _isSpawned;

    [Inject]
    private void Construct(IProjectileFactory projectileFactory, CombatConfig combatConfig)
    {
        _projectileFactory = projectileFactory;
        _combatConfig = combatConfig;
        Log("[PlayerAttackController] - Инжекция зависимостей завершена");
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerAnimationChangedEvent>(OnPlayerAnimationChanged);
        Log("[PlayerAttackController] - OnEnable, подписка на события выполнена");
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerAnimationChangedEvent>(OnPlayerAnimationChanged);
        Log("[PlayerAttackController] - OnDisable, отписка от событий выполнена");
    }

    public override void Spawned()
    {
        _isSpawned = true;
        Log("[PlayerAttackController] - Spawned");
    }

    public override void FixedUpdateNetwork()
    {
        // Диагностика состояния
        Log($"[PlayerAttackController] - Tick. HasStateAuthority={Object.HasStateAuthority}, HasInputAuthority={Object.HasInputAuthority}");

        if (GetInput(out NetworkInputData input) == false)
        {
            Log("[PlayerAttackController] - Нет входных данных");
            return;
        }

        // Обновляем последнее направление
        if (input.moveDirection.sqrMagnitude > 0.0001f)
        {
            _lastDirection = input.moveDirection.normalized;
            //Log($"[PlayerAttackController] - Обновление направления: {_lastDirection}");
        }

        if (input.isFiring == false)
        {
            return;
        }

        if (Object.HasStateAuthority == false)
        {
            Log("[PlayerAttackController] - Fire получен, но нет StateAuthority, пропускаем спавн");
            return;
        }

        Vector2 dir = _lastDirection.sqrMagnitude > 0 ? _lastDirection : Vector2.right;
        float spawnOffset = _spawnOffsetOverride >= 0f && _combatConfig ? _spawnOffsetOverride : (_combatConfig ? _combatConfig.SpawnOffset : 0.6f);
        Vector2 spawnPos = (Vector2)transform.position + dir * spawnOffset;

        var proj = _projectileFactory?.Spawn(Object.InputAuthority, spawnPos, dir);
        Log($"[PlayerAttackController] - Fire input: {input.isFiring}, spawned={(proj!=null)} dir={dir} pos={spawnPos}");
    }

    private void OnPlayerAnimationChanged(PlayerAnimationChangedEvent evt)
    {
        // Может вызываться до Spawned() компонента
        if (_isSpawned == false)
        {
            return;
        }

        if (evt.PlayerRef != Object.InputAuthority)
        {
            return;
        }

        // Направление из анимации маппится на 4 стороны. Используем последнее движение для точного угла.
        // Здесь оставляем только обновление флага движения, направление уже отслеживаем из ввода.
        if (evt.IsMoving && evt.Direction >= 0)
        {
            // no-op: направление уже обновлено от ввода
        }
    }
}



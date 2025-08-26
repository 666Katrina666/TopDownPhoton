using Fusion;
using UnityEngine;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Компонент здоровья для игроков и манекенов
/// </summary>
public class Health : LoggableNetworkBehaviour
{
    [FoldoutGroup("Health"), LabelText("Max HP"), Min(1)]
    [SerializeField] private int _maxHp = 10;

    [FoldoutGroup("Runtime Data", false)]
    [Networked] public int CurrentHp { get; private set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            CurrentHp = _maxHp;
        }
        Log($"Spawned with HP={CurrentHp}/{_maxHp}");
        EventBus.RaiseEvent(new HealthChangedEvent(Object, CurrentHp, _maxHp));
    }

    /// <summary>
    /// Применяет урон. Доступно только владельцу состояния
    /// </summary>
    public void ApplyDamage(int amount, PlayerRef source)
    {
        if (Object.HasStateAuthority == false)
        {
            return;
        }

        int dmg = Mathf.Max(0, amount);
        if (dmg == 0)
        {
            return;
        }

        CurrentHp = Mathf.Max(0, CurrentHp - dmg);
        Log($"Damage {dmg} from {source}. HP={CurrentHp}/{_maxHp}");
        EventBus.RaiseEvent(new HealthChangedEvent(Object, CurrentHp, _maxHp));
        RpcNotifyDamage(dmg);

        if (CurrentHp <= 0)
        {
            Log("HP reached 0");
            EventBus.RaiseEvent(new DeathEvent(Object, source));
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcNotifyDamage(int amount)
    {
        int dmg = Mathf.Max(0, amount);
        if (dmg <= 0) return;
        EventBus.RaiseEvent(new DamageTakenEvent(Object, default, dmg));
    }

    /// <summary>
    /// Восстанавливает здоровье до максимального значения.
    /// Разрешено только владельцу состояния (State Authority).
    /// </summary>
    public void ResetToMax()
    {
        if (Object.HasStateAuthority == false)
        {
            return;
        }

        CurrentHp = _maxHp;
        EventBus.RaiseEvent(new HealthChangedEvent(Object, CurrentHp, _maxHp));
    }
}



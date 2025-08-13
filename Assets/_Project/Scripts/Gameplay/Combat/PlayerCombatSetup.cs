using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Настраивает боевые компоненты на заспавненных игроках через EventBus
/// </summary>
public class PlayerCombatSetup : LoggableMonoBehaviour
{
    [Inject] private DiContainer _container;

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
        Log("Подписка на PlayerSpawnedEvent");
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
    }

    private void OnPlayerSpawned(PlayerSpawnedEvent evt)
    {
        if (evt.PlayerObject == null)
        {
            return;
        }

        var go = evt.PlayerObject.gameObject;

        Debug.Log($"[PlayerCombatSetup] - Получен PlayerSpawnedEvent: {go.name}, hasAttack={go.GetComponent<PlayerAttackController>()!=null}, hasHealth={go.GetComponent<Health>()!=null}");

        // Добавляем контроллер атаки при отсутствии
        if (go.GetComponent<PlayerAttackController>() == null)
        {
            go.AddComponent<PlayerAttackController>();
            Log($"PlayerAttackController добавлен на игрока");
        }

        // Добавляем здоровье при отсутствии
        if (go.GetComponentInChildren<Health>() == null)
        {
            go.AddComponent<Health>();
            Log("Health добавлен на игрока");
        }

        // Инжектим зависимости на объект игрока, чтобы контроллер атаки получил фабрику
        _container.InjectGameObject(go);

        Log($"Настроен бой для игрока {evt.PlayerRef}");
    }
}



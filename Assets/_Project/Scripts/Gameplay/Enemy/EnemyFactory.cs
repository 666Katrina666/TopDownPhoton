using UnityEngine;
using Sirenix.OdinInspector;
using Zenject;
using DG.Tweening;
using Core.Base;
using Fusion;

/// <summary>
/// Фабрика юнитов: запускает волны, спавнит юнитов из пула, списывает очки
/// </summary>
public class EnemyFactory : LoggableNetworkBehaviour
{
    #region Settings
    [Title("Настройки фабрики")]
    [SerializeField] private float _spawnInterval = 1.5f;
    [SerializeField] private float _nextWaveDelay = 2f;

    [Title("Зоны спавна")]
    [SerializeField] private SpawnZone[] _spawnZones;

    [Title("Поведение")]
    [SerializeField] private bool _autoStartOnAwake = true;
    #endregion

    #region DI
    [Inject] private IWaveManager _waveManager;
    #endregion

    #region Runtime
    private int _currentWave;
    private Sequence _spawnSequence;
    private Tween _nextWaveTween;
    private readonly System.Collections.Generic.HashSet<NetworkObject> _aliveEnemies = new System.Collections.Generic.HashSet<NetworkObject>();
    private bool _spawningStopped; // очки закончились — спавн остановлен
    private bool _nextWaveScheduled; // защита от повторного запуска
    #endregion

    /// <summary>
    /// Инициализация после спавна сетевого объекта
    /// </summary>
    public override void Spawned()
    {
        Log($"[EnemyFactory] Spawned: stateAuthority={Object.HasStateAuthority}, autoStart={_autoStartOnAwake}");
        Log($"[EnemyFactory] Spawned: зон спавна = {_spawnZones?.Length ?? 0}");
        EventBus.Subscribe<DeathEvent>(OnDeathEvent);
        if (_autoStartOnAwake && Object.HasStateAuthority)
        {
            StartWave(0);
        }
    }

    private void OnDestroy()
    {
        _spawnSequence?.Kill();
        _nextWaveTween?.Kill();
        EventBus.Unsubscribe<DeathEvent>(OnDeathEvent);
    }

    /// <summary>
    /// Запускает волну по индексу
    /// </summary>
    public void StartWave(int waveIndex)
    {
        if (Object != null && Object.HasStateAuthority == false)
        {
            Log("[EnemyFactory] StartWave: пропуск, нет StateAuthority");
            return;
        }
        Log($"[EnemyFactory] StartWave: {waveIndex}");
        _currentWave = waveIndex;
        _waveManager.StartWave(_currentWave);
        _spawnSequence?.Kill();
        _nextWaveTween?.Kill();
        _aliveEnemies.Clear();
        _spawningStopped = false;
        _nextWaveScheduled = false;
        BuildSpawnLoop();
    }

    /// <summary>
    /// Стартует следующую волну
    /// </summary>
    private void StartNextWave()
    {
        Log($"[EnemyFactory] StartNextWave: запуск волны {_currentWave + 1}");
        StartWave(_currentWave + 1);
    }

    /// <summary>
    /// Создает бесконечный цикл спавна через DOTween до завершения волны
    /// </summary>
    private void BuildSpawnLoop()
    {
        if (Object != null && Object.HasStateAuthority == false)
        {
            Log("[EnemyFactory] BuildSpawnLoop: пропуск, нет StateAuthority");
            return;
        }
        Log($"[EnemyFactory] BuildSpawnLoop: интервал {_spawnInterval}с");
        _spawnSequence = DOTween.Sequence()
            .AppendInterval(_spawnInterval)
            .AppendCallback(() =>
            {
                SpawnTick();
                if (_waveManager.IsWaveComplete)
                {
                    HandleWaveComplete();
                }
            })
            .SetLoops(-1, LoopType.Restart);
        _spawnSequence.Play();
        Log($"[EnemyFactory] BuildSpawnLoop: последовательность запущена");
    }

    /// <summary>
    /// Обработка завершения волны и запуск следующей с задержкой
    /// </summary>
    private void HandleWaveComplete()
    {
        Log($"[EnemyFactory] HandleWaveComplete: волна {_currentWave} завершена");
        _spawnSequence?.Kill();
        _spawningStopped = true;
        TryScheduleNextWaveIfCleared();
    }

    /// <summary>
    /// Один тик спавна: выбор юнита по весам, проверка очков, спавн из пула
    /// </summary>
    private void SpawnTick()
    {
        Log($"[EnemyFactory] SpawnTick: начало");
        if (Object != null && Object.HasStateAuthority == false)
        {
            Log("[EnemyFactory] SpawnTick: пропуск, нет StateAuthority");
            return;
        }
        
        // Проверяем инициализацию менеджеров
        if (_waveManager == null)
        {
            Log($"[EnemyFactory] SpawnTick: _waveManager == null");
            return;
        }
        
        var enemyData = _waveManager.GetRandomEnemy();
        if (enemyData == null)
        {
            Log($"[EnemyFactory] SpawnTick: enemyData == null");
            return;
        }
        
        Log($"[EnemyFactory] SpawnTick: выбран юнит {enemyData.EnemyPrefab?.name}, стоимость {enemyData.Cost}");
        
        if (_waveManager.CanSpawnEnemy(enemyData) == false)
        {
            Log($"[EnemyFactory] SpawnTick: нельзя спавнить юнита (не хватает очков или волна завершена)");
            return;
        }

        if (_spawnZones == null || _spawnZones.Length == 0)
        {
            Log($"[EnemyFactory] SpawnTick: нет зон спавна");
            return;
        }

        var zone = _spawnZones[Random.Range(0, _spawnZones.Length)];
        var position = zone.GetRandomPosition();
        Log($"[EnemyFactory] SpawnTick: позиция спавна {position}");

        // Спавним через Fusion без пула
        NetworkObject enemy = null;
        var prefabNO = enemyData.EnemyPrefab != null ? enemyData.EnemyPrefab.GetComponent<NetworkObject>() : null;
        try
        {
            if (prefabNO != null)
            {
                Log($"[EnemyFactory] SpawnTick: Runner.Spawn(NetworkObject) {prefabNO.name}");
                enemy = Runner.Spawn(prefabNO, position, Quaternion.identity);
            }
            else
            {
                Log($"[EnemyFactory] SpawnTick: нет NetworkObject на префабе, делаем локальный Instantiate()");
                var enemyGO = Instantiate(enemyData.EnemyPrefab, position, Quaternion.identity);
                enemy = enemyGO.AddComponent<NetworkObject>();
            }
        }
        catch (System.Exception ex)
        {
            LogError($"[EnemyFactory] SpawnTick: Runner.Spawn exception: {ex.Message}");
            return;
        }
        
        _waveManager.SpendPoints(enemyData.Cost);
        Log($"[EnemyFactory] SpawnTick: юнит заспавнен успешно");
        if (enemy != null)
        {
            _aliveEnemies.Add(enemy);
        }

        EventBus.RaiseEvent(new EnemySpawnedEvent
        {
            Enemy = enemy,
            SpawnPosition = position
        });
    }

    /// <summary>
    /// Обработка события смерти — вычитаем живых и проверяем переход на следующую волну
    /// </summary>
    private void OnDeathEvent(DeathEvent evt)
    {
        if (evt.Target == null) return;
        if (_aliveEnemies.Remove(evt.Target))
        {
            Log($"[EnemyFactory] OnDeathEvent: осталось живых {_aliveEnemies.Count}");
            TryScheduleNextWaveIfCleared();
        }
    }

    private void TryScheduleNextWaveIfCleared()
    {
        if (_spawningStopped == false) return; // очки ещё есть — спавн продолжается
        if (_aliveEnemies.Count > 0) return; // ждём пока все умрут
        if (_nextWaveScheduled) return;
        _nextWaveScheduled = true;
        Log($"[EnemyFactory] TryScheduleNextWaveIfCleared: все юниты мертвы, следующая волна через {_nextWaveDelay}с");
        _nextWaveTween = DOVirtual.DelayedCall(_nextWaveDelay, StartNextWave);
    }

    #region Gizmos (Editor)
    /// <summary>
    /// Отображает зоны спавна в редакторе
    /// </summary>
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        if (_spawnZones == null || _spawnZones.Length == 0) return;

        Gizmos.color = new Color(0f, 1f, 0f, 1f);
        for (int i = 0; i < _spawnZones.Length; i++)
        {
            var zone = _spawnZones[i];
            var center = new Vector3(zone.Center.x, zone.Center.y, 0f);
            var size = new Vector3(zone.Size.x, zone.Size.y, 0f);

            Gizmos.DrawWireCube(center, size);
            Gizmos.DrawSphere(center, 0.05f);
        }
    }
    #endregion
}

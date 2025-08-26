# TODO: Фабрика Юнитов (Enemy Factory)

## Обзор системы
Фабрика юнитов работает по принципу волн с бесконечным циклом в мультиплеерной игре. Каждая волна имеет ограниченное количество очков и набор типов юнитов, которые могут быть заспавнены. Спавн происходит только на хосте, а синхронизация через Fusion NetworkRunner.

## Архитектура (SOLID + Unity Style Guide)

### 1. Структуры данных

#### WaveData (ScriptableObject)
```csharp
[CreateAssetMenu(fileName = "WaveData", menuName = "Gameplay/Wave Data")]
public class WaveData : ScriptableObject
{
    [Title("Настройки волны")]
    [SerializeField] private int _pointsLimit; // Лимит очков на волну
    [SerializeField] private float _pointsMultiplier = 1.2f; // Множитель очков для бесконечных волн
    
    [Title("Доступные юниты")]
    [SerializeField] private List<EnemySpawnData> _availableEnemies; // Список доступных юнитов
    
    // Properties для соблюдения инкапсуляции
    public int PointsLimit => _pointsLimit;
    public float PointsMultiplier => _pointsMultiplier;
    public IReadOnlyList<EnemySpawnData> AvailableEnemies => _availableEnemies.AsReadOnly();
}

[System.Serializable]
public class EnemySpawnData
{
    [SerializeField] private GameObject _enemyPrefab; // Префаб юнита (NetworkObject)
    [SerializeField] private int _cost; // Стоимость в очках
    [SerializeField, Range(0f, 1f)] private float _spawnWeight = 1f; // Вес для случайного выбора
    
    // Properties
    public GameObject EnemyPrefab => _enemyPrefab;
    public int Cost => _cost;
    public float SpawnWeight => _spawnWeight;
}
```

#### IEnemyFactory (Interface)
```csharp
/// <summary>
/// Интерфейс фабрики юнитов для соблюдения принципа инверсии зависимостей
/// </summary>
public interface IEnemyFactory
{
    void Initialize();
    void StartWave(int waveNumber);
    void StopSpawning();
    bool IsWaveComplete { get; }
    int CurrentWave { get; }
}
```

#### EnemyFactory (NetworkBehaviour + Zenject)
```csharp
/// <summary>
/// Фабрика юнитов с сетевой синхронизацией
/// </summary>
public class EnemyFactory : NetworkBehaviour, IEnemyFactory
{
    [Title("Настройки фабрики")]
    [SerializeField] private WaveData[] _waveConfigurations; // Конфигурации волн
    [SerializeField] private float _spawnInterval = 2f; // Интервал между спавном
    
    [Title("Пул объектов")]
    [SerializeField] private int _poolPreloadCount = 10; // Количество предзагруженных объектов
    
    [Title("Зоны спавна")]
    [SerializeField] private SpawnZone[] _spawnZones; // Зоны для спавна юнитов
    
    // Zenject зависимости
    [Inject] private NetworkRunner _networkRunner;
    [Inject] private IEnemyPoolManager _poolManager;
    [Inject] private IWaveManager _waveManager;
    
    // Сетевые переменные
    [Networked] private int _currentWave { get; set; }
    [Networked] private bool _isSpawning { get; set; }
    
    // Локальные переменные
    private float _spawnTimer;
    private TickTimer _spawnTimerTick;
    
    // Properties
    public bool IsWaveComplete => _waveManager.IsWaveComplete;
    public int CurrentWave => _currentWave;
}
```

### 2. Система пула объектов

#### IEnemyPoolManager (Interface)
```csharp
/// <summary>
/// Интерфейс менеджера пула юнитов
/// </summary>
public interface IEnemyPoolManager
{
    void Initialize();
    NetworkObject GetEnemy(GameObject prefab, Vector3 position);
    void ReturnEnemy(NetworkObject enemy);
    void PreloadPools();
}
```

#### EnemyPoolManager
```csharp
/// <summary>
/// Менеджер пула юнитов с сетевой синхронизацией
/// </summary>
public class EnemyPoolManager : IEnemyPoolManager
{
    [Inject] private NetworkRunner _networkRunner;
    [Inject] private DiContainer _container;
    
    private Dictionary<GameObject, PoolBase<NetworkObject>> _pools;
    private Dictionary<NetworkObject, GameObject> _enemyToPrefabMap;
    
    // Методы:
    // - GetEnemy(GameObject prefab, Vector3 position) - создает NetworkObject
    // - ReturnEnemy(NetworkObject enemy) - возвращает в пул с сбросом состояния
    // - PreloadPools() - предзагружает объекты в пулы
    // - ResetEnemyState(NetworkObject enemy) - сбрасывает состояние юнита
}
```

#### EnemyPoolObject (Адаптация PoolBase)
```csharp
/// <summary>
/// Специализированный пул для NetworkObject юнитов
/// </summary>
public class EnemyPoolObject : PoolBase<NetworkObject>
{
    private readonly NetworkRunner _networkRunner;
    private readonly Action<NetworkObject> _resetAction;
    
    public EnemyPoolObject(
        Func<NetworkObject> preloadFunc, 
        Action<NetworkObject> getAction, 
        Action<NetworkObject> returnAction,
        NetworkRunner networkRunner,
        Action<NetworkObject> resetAction,
        int preloadCount) : base(preloadFunc, getAction, returnAction, preloadCount)
    {
        _networkRunner = networkRunner;
        _resetAction = resetAction;
    }
    
    public NetworkObject Get(Vector3 position, Transform parent = null)
    {
        NetworkObject enemy = base.Get(parent);
        enemy.transform.position = position;
        return enemy;
    }
    
    public override void Return(NetworkObject item)
    {
        _resetAction?.Invoke(item);
        base.Return(item);
    }
}
```

### 3. Логика волн

#### IWaveManager (Interface)
```csharp
/// <summary>
/// Интерфейс менеджера волн
/// </summary>
public interface IWaveManager
{
    void StartWave(int waveNumber);
    bool CanSpawnEnemy(EnemySpawnData enemyData);
    EnemySpawnData GetRandomEnemy();
    bool IsWaveComplete { get; }
    int RemainingPoints { get; }
    WaveData CurrentWaveData { get; }
}
```

#### WaveManager
```csharp
/// <summary>
/// Менеджер волн с логикой бесконечных волн
/// </summary>
public class WaveManager : IWaveManager
{
    [Inject] private WaveData[] _waveConfigurations;
    
    private int _currentWave = 0;
    private int _remainingPoints;
    private WaveData _currentWaveData;
    private List<EnemySpawnData> _availableEnemies;
    
    // Методы:
    // - StartWave(int waveNumber) - инициализирует новую волну
    // - CanSpawnEnemy(EnemySpawnData enemyData) - проверяет возможность спавна
    // - GetRandomEnemy() - возвращает случайного юнита с учетом весов
    // - IsWaveComplete() - проверяет завершение волны
    // - GetNextWaveData() - получает конфигурацию следующей волны
    // - CalculateInfiniteWavePoints() - рассчитывает очки для бесконечных волн
}
```

## Алгоритм работы (Мультиплеер)

### 1. Инициализация
1. **Хост**: Создание пулов для каждого типа юнита
2. **Хост**: Предзагрузка NetworkObject в пулы
3. **Все клиенты**: Подписка на сетевые события
4. **Хост**: Настройка WaveManager и начало первой волны

### 2. Цикл волны (только на хосте)
1. **Начало волны**: 
   - Установка лимита очков и доступных юнитов
   - Отправка события `WaveStartedEvent` всем клиентам
2. **Спавн юнитов**: 
   - Выбор случайного юнита из доступных (с учетом веса)
   - Проверка достаточности очков
   - Получение NetworkObject из пула
   - Размещение в случайной позиции из SpawnZone
   - Сброс состояния юнита (здоровье, коллайдер, UI)
   - Вычитание стоимости из очков
   - Отправка события `EnemySpawnedEvent` всем клиентам
3. **Завершение волны**: Когда очки закончились, отправка `WaveCompletedEvent`

### 3. Сетевые события
- **WaveStartedEvent**: Уведомляет всех клиентов о начале волны
- **WaveCompletedEvent**: Уведомляет о завершении волны
- **EnemySpawnedEvent**: Синхронизирует появление юнита
- **EnemyDeathEvent**: Обрабатывается DeathHandler, возвращает в пул

### 4. Возврат в пул (DeathHandler)
1. **DeathHandler.OnDeath()**: Выключает UI и коллайдер
2. **EnemyPoolManager.ReturnEnemy()**: 
   - Включает обратно UI и коллайдер
   - Восстанавливает здоровье до максимума
   - Сбрасывает все состояния юнита
   - Возвращает в пул

### 5. Бесконечные волны
- После последней конфигурации используем последний элемент массива
- Множаем лимит очков: `pointsLimit * (multiplier ^ (currentWave - lastConfiguredWave))`

## Преимущества данной архитектуры

### 1. Соблюдение SOLID принципов
- **Single Responsibility**: Каждый класс отвечает за одну область
- **Open/Closed**: Легко расширять через интерфейсы
- **Liskov Substitution**: Интерфейсы позволяют заменять реализации
- **Interface Segregation**: Специализированные интерфейсы
- **Dependency Inversion**: Зависимости через интерфейсы

### 2. Использование ScriptableObject для конфигурации
- **Гибкость**: Легко настраивать волны без изменения кода
- **Переиспользование**: Можно создавать разные наборы волн для разных уровней
- **Версионирование**: Изменения в конфигурации не затрагивают код
- **Инкапсуляция**: Приватные поля с публичными свойствами

### 3. Система пула объектов с сетевым управлением
- **Производительность**: Избегаем частого создания/уничтожения NetworkObject
- **Память**: Контролируем количество активных объектов
- **Скорость**: Быстрый спавн без загрузки
- **Синхронизация**: Автоматическая синхронизация через Fusion

### 4. Модульная архитектура с интерфейсами
- **Разделение ответственности**: Каждый класс отвечает за свою область
- **Тестируемость**: Легко тестировать через моки интерфейсов
- **Расширяемость**: Просто добавлять новые типы юнитов и механики
- **Заменяемость**: Можно легко заменить реализации

### 5. Интеграция с Zenject и Fusion
- **Dependency Injection**: Чистые зависимости между компонентами
- **Сетевая синхронизация**: Легкая интеграция с Fusion NetworkRunner
- **Управление жизненным циклом**: Автоматическое управление сервисами
- **Хост-клиент архитектура**: Только хост спавнит, все клиенты синхронизируются

## Детальный план реализации (Поэтапно)

### Этап 1: Базовая структура данных (1-2 дня)
**Цель**: Создать фундамент для системы волн без сетевой синхронизации

#### 1.1 Создание ScriptableObject
- [ ] Создать `EnemySpawnData.cs` (простая структура данных)
- [ ] Создать `WaveData.cs` (ScriptableObject с настройками волны)
- [ ] Создать тестовые конфигурации волн в Unity

#### 1.2 Создание интерфейсов
- [ ] Создать `IWaveManager.cs` (интерфейс для управления волнами)
- [ ] Создать `IEnemyPoolManager.cs` (интерфейс для пула)
- [ ] Создать `IEnemyFactory.cs` (интерфейс фабрики)

#### 1.3 Создание SpawnZone
- [ ] Создать `SpawnZone.cs` (класс для зон спавна)
- [ ] Добавить метод `GetRandomPosition()`

**Тестирование**: Создать простой тест-скрипт для проверки создания конфигураций

---

### Этап 2: Система пула объектов (2-3 дня)
**Цель**: Реализовать пул для обычных GameObject (без сети)

#### 2.1 Адаптация PoolBase
- [ ] Создать `EnemyPoolObject.cs` (наследник от PoolBase)
- [ ] Добавить поддержку позиции спавна
- [ ] Добавить Action для сброса состояния

#### 2.2 Создание EnemyPoolManager
- [ ] Реализовать `EnemyPoolManager.cs` (без сетевой синхронизации)
- [ ] Добавить методы `GetEnemy()`, `ReturnEnemy()`, `PreloadPools()`
- [ ] Добавить словарь для маппинга объектов

#### 2.3 Создание EnemyStateReset
- [ ] Создать `EnemyStateReset.cs` (компонент для сброса состояния)
- [ ] Добавить метод `ResetState()` для восстановления здоровья, UI, коллайдера

**Тестирование**: Создать простую сцену с пулом и проверить создание/возврат объектов

---

### Этап 3: Система волн (2-3 дня)
**Цель**: Реализовать логику управления волнами

#### 3.1 Создание WaveManager
- [ ] Реализовать `WaveManager.cs` (без сетевых переменных)
- [ ] Добавить логику бесконечных волн
- [ ] Реализовать выбор случайных юнитов с весами

#### 3.2 Создание событий
- [ ] Создать `WaveStartedEvent.cs`
- [ ] Создать `WaveCompletedEvent.cs`
- [ ] Создать `EnemySpawnedEvent.cs`

#### 3.3 Интеграция с EventBus
- [ ] Подключить события к EventBus
- [ ] Добавить подписки на события в тестовом UI

**Тестирование**: Создать тестовую сцену с WaveManager и проверить логику волн

---

### Этап 4: Базовая фабрика (2-3 дня)
**Цель**: Создать фабрику без сетевой синхронизации

#### 4.1 Создание EnemyFactory
- [ ] Реализовать `EnemyFactory.cs` (наследуется от MonoBehaviour)
- [ ] Добавить Zenject зависимости
- [ ] Реализовать базовую логику спавна
- [ ] Интегрировать с WaveManager и PoolManager

#### 4.2 Zenject интеграция
- [ ] Создать Installer для фабрики
- [ ] Настроить bindings для интерфейсов
- [ ] Добавить ScriptableObject ресурсы

#### 4.3 Интеграция с DeathHandler
- [ ] Модифицировать `DeathHandler.cs` для работы с пулом
- [ ] Добавить событие `EnemyReturnedToPoolEvent`
- [ ] Подключить возврат в пул при смерти

**Тестирование**: Полноценная тестовая сцена с фабрикой, волнами и пулом

---

### Этап 5: Сетевая синхронизация (3-4 дня)
**Цель**: Добавить поддержку мультиплеера

#### 5.1 Модификация для NetworkBehaviour
- [ ] Изменить `EnemyFactory.cs` на NetworkBehaviour
- [ ] Добавить сетевые переменные `[Networked]`
- [ ] Добавить проверки на хост

#### 5.2 Сетевая синхронизация пула
- [ ] Модифицировать `EnemyPoolManager.cs` для работы с NetworkObject
- [ ] Добавить NetworkRunner зависимости
- [ ] Обновить методы для создания NetworkObject

#### 5.3 Сетевые события
- [ ] Добавить RPC вызовы для событий
- [ ] Синхронизировать спавн между хостом и клиентами
- [ ] Добавить обработку сетевых событий

**Тестирование**: Тестирование в мультиплеере (хост + клиент)

---

### Этап 6: Оптимизация и полировка (2-3 дня)
**Цель**: Улучшить производительность и добавить фичи

#### 6.1 Оптимизация пула
- [ ] Добавить пул спавн-позиций
- [ ] Оптимизировать выбор юнитов (кеширование)
- [ ] Добавить профилирование

#### 6.2 Дополнительные фичи
- [ ] Создать `BossWaveData.cs` для боссов
- [ ] Добавить систему боссов
- [ ] Добавить конфигурацию сложности

#### 6.3 Финальная интеграция
- [ ] Создать UI для отображения волн
- [ ] Добавить настройки в меню
- [ ] Финальное тестирование

**Тестирование**: Полное тестирование системы в мультиплеере

---

## Преимущества поэтапной разработки

### 1. **Постепенное тестирование**
- Каждый этап можно протестировать отдельно
- Легко найти и исправить ошибки
- Можно изменить архитектуру на ранних этапах

### 2. **Гибкость разработки**
- Можно остановиться на любом этапе
- Легко добавить новые фичи между этапами
- Возможность пересмотреть подход

### 3. **Управление рисками**
- Сетевые сложности изолированы в этапе 5
- Базовую функциональность можно показать раньше
- Меньше вероятность больших рефакторингов

### 4. **Командная работа**
- Разные разработчики могут работать над разными этапами
- Четкое разделение ответственности
- Параллельная разработка возможна

## Критерии готовности каждого этапа

### Этап 1: ✅ ВЫПОЛНЕН
- ScriptableObject созданы и работают
- Интерфейсы определены
- Можно создать конфигурацию волны в Unity
- Тестовый скрипт создан для проверки

### Этап 2: ✅ ВЫПОЛНЕН
- Пул создает и возвращает объекты
- EnemyStateReset работает корректно
- Нет утечек памяти
- Тестовый скрипт создан для проверки

### Этап 3: ✅ Готов
- Волны запускаются и завершаются
- События отправляются корректно
- Логика бесконечных волн работает

### Этап 4: ✅ Готов
- Фабрика спавнит юнитов
- Zenject работает корректно
- DeathHandler возвращает в пул

### Этап 5: ✅ Готов
- Мультиплеер работает стабильно
- Синхронизация корректная
- Нет десинхронизаций

### Этап 6: ✅ Готов
- Производительность удовлетворительная
- Все фичи работают
- UI отображает информацию корректно

## Дополнительные идеи

### 1. Система событий (EventBus)
```csharp
/// <summary>
/// События для синхронизации между хостом и клиентами
/// </summary>
public class WaveStartedEvent 
{ 
    public int WaveNumber { get; set; }
    public int PointsLimit { get; set; }
}

public class WaveCompletedEvent 
{ 
    public int WaveNumber { get; set; }
    public float CompletionTime { get; set; }
}

public class EnemySpawnedEvent 
{ 
    public NetworkObject Enemy { get; set; }
    public Vector3 SpawnPosition { get; set; }
    public int EnemyType { get; set; }
}

public class EnemyReturnedToPoolEvent
{
    public NetworkObject Enemy { get; set; }
}
```

### 2. Конфигурация спавна
```csharp
[System.Serializable]
public class SpawnZone
{
    [Title("Зона спавна")]
    [SerializeField] private Vector2 _center;
    [SerializeField] private Vector2 _size;
    [SerializeField] private float _minDistanceFromPlayer = 5f;
    
    // Properties
    public Vector2 Center => _center;
    public Vector2 Size => _size;
    public float MinDistanceFromPlayer => _minDistanceFromPlayer;
    
    /// <summary>
    /// Получает случайную позицию в зоне спавна
    /// </summary>
    public Vector3 GetRandomPosition()
    {
        float x = Random.Range(_center.x - _size.x / 2, _center.x + _size.x / 2);
        float y = Random.Range(_center.y - _size.y / 2, _center.y + _size.y / 2);
        return new Vector3(x, y, 0);
    }
}
```

### 3. Система боссов
```csharp
[CreateAssetMenu(fileName = "BossWaveData", menuName = "Gameplay/Boss Wave Data")]
public class BossWaveData : WaveData
{
    [Title("Настройки босса")]
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private int _bossSpawnDelay = 5;
    [SerializeField] private bool _spawnBossFirst = true;
    
    // Properties
    public GameObject BossPrefab => _bossPrefab;
    public int BossSpawnDelay => _bossSpawnDelay;
    public bool SpawnBossFirst => _spawnBossFirst;
}
```

### 4. Компонент для сброса состояния юнита
```csharp
/// <summary>
/// Компонент для сброса состояния юнита при возврате в пул
/// </summary>
public class EnemyStateReset : MonoBehaviour
{
    [Title("Компоненты для сброса")]
    [SerializeField] private Health _health;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private GameObject _healthBar;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    /// <summary>
    /// Сбрасывает состояние юнита к начальному
    /// </summary>
    public void ResetState()
    {
        if (_health != null)
            _health.ResetToMax();
            
        if (_collider != null)
            _collider.enabled = true;
            
        if (_healthBar != null)
            _healthBar.SetActive(true);
            
        if (_spriteRenderer != null)
            _spriteRenderer.color = Color.white;
    }
}
```

## Файловая структура
```
Assets/_Project/Scripts/Gameplay/
├── Enemy/
│   ├── Interfaces/
│   │   ├── IEnemyFactory.cs
│   │   ├── IEnemyPoolManager.cs
│   │   └── IWaveManager.cs
│   ├── EnemyFactory.cs
│   ├── EnemyPoolManager.cs
│   ├── EnemyPoolObject.cs
│   ├── WaveManager.cs
│   ├── EnemyStateReset.cs
│   └── Data/
│       ├── WaveData.cs
│       ├── EnemySpawnData.cs
│       ├── BossWaveData.cs
│       └── SpawnZone.cs
├── Events/
│   ├── WaveStartedEvent.cs
│   ├── WaveCompletedEvent.cs
│   ├── EnemySpawnedEvent.cs
│   └── EnemyReturnedToPoolEvent.cs
└── EnemyFactory_TODO.md
```

## Zenject Bindings
```csharp
// В Installer
Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();
Container.Bind<IEnemyPoolManager>().To<EnemyPoolManager>().AsSingle();
Container.Bind<IWaveManager>().To<WaveManager>().AsSingle();
Container.Bind<WaveData[]>().FromScriptableObjectResource("WaveConfigurations").AsSingle();
```

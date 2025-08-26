using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using Fusion;
using System.Collections;

/// <summary>
/// Установщик для игровой сцены
/// Наследует от SceneInstaller и добавляет компоненты, специфичные для игрового процесса
/// </summary>
public class GameSceneInstaller : MonoInstaller
{    
    #region Prefabs
    [FoldoutGroup("Prefabs")]
    [InfoBox("Префабы для игровых компонентов")]
    [SerializeField] private GameObject playerFactoryPrefab;
    [FoldoutGroup("Prefabs")]
    [SerializeField] private GameObject playerSpawnerPrefab;
    [FoldoutGroup("Prefabs")]
    [SerializeField] private GameObject networkInputHandlerPrefab;
    #endregion

    #region Combat
    [FoldoutGroup("Combat")]
    [InfoBox("Конфигурация боя и префаб снаряда")]
    [SerializeField] private CombatConfig combatConfig;
    [FoldoutGroup("Combat")]
    [SerializeField] private WaveConfigurations waveConfigurations;
    #endregion
    
    #region Settings
    [FoldoutGroup("Debug Settings")]
    [InfoBox("Настройки отладки")]
    [SerializeField] private bool enableLogging = true;
    #endregion
    
    /// <summary>
    /// Выводит сообщение в консоль, если включено логирование
    /// </summary>
    /// <param name="message">Сообщение для вывода</param>
    private void Log(string message)
    {
        if (enableLogging)
        {
            Debug.Log($"[{GetType().Name}] - {message}");
        }
    }

    /// <summary>
    /// Выводит предупреждение в консоль, если включено логирование
    /// </summary>
    /// <param name="message">Сообщение для вывода</param>
    private void LogWarning(string message)
    {
        if (enableLogging)
        {
            Debug.LogWarning($"[{GetType().Name}] - {message}");
        }
    }

    /// <summary>
    /// Выводит ошибку в консоль, если включено логирование
    /// </summary>
    /// <param name="message">Сообщение для вывода</param>
    private void LogError(string message)
    {
        if (enableLogging)
        {
            Debug.LogError($"[{GetType().Name}] - {message}");
        }
    }
    
    #region Unity Callbacks
    public override void InstallBindings()
    {
        Log("Starting GameScene installation...");
        
        // Устанавливаем биндинги для игровых компонентов
        InstallPlayerBindings();

        // Бинды боя
        InstallCombatBindings();
        
        // Создаём настройщик боевых компонентов сразу, до возможного спавна игроков
        CreateCombatSetupImmediate();

        Log("GameScene installation completed successfully");
        
        // Запускаем создание объектов после завершения установки
        StartCoroutine(CreatePlayerComponentsAfterInstall());
    }
    #endregion
    
    /// <summary>
    /// Устанавливает биндинги для игровых компонентов
    /// </summary>
    #region Installation
    private void InstallPlayerBindings()
    {
        Log("Installing player bindings...");
        
        // Проверяем наличие префабов
        if (playerFactoryPrefab == null)
        {
            LogError("PlayerFactory prefab не назначен!");
            return;
        }
        
        if (playerSpawnerPrefab == null)
        {
            LogError("PlayerSpawner prefab не назначен!");
            return;
        }
        
        if (networkInputHandlerPrefab == null)
        {
            LogError("NetworkInputHandler prefab не назначен!");
            return;
        }
        
        // Биндинги будут установлены после создания объектов
        Log("Player bindings installation completed");
    }

    /// <summary>
    /// Устанавливает биндинги для боевой системы
    /// </summary>
    private void InstallCombatBindings()
    {
        if (combatConfig == null)
        {
            LogWarning("CombatConfig не назначен! Боевые биндинги будут частично недоступны.");
        }
        else
        {
            Container.Bind<CombatConfig>().FromInstance(combatConfig).AsSingle();
        }

        if (waveConfigurations == null)
        {
            LogWarning("WaveConfigurations не назначен! Система волн будет недоступна.");
        }
        else
        {
            Container.Bind<WaveData[]>().FromInstance(waveConfigurations.Waves).AsSingle();
        }

        Container.Bind<IProjectileFactory>().To<ProjectileFactory>().AsSingle();
        Container.Bind<IWaveManager>().To<WaveManager>().AsSingle();
        
        Log("Combat bindings installed successfully");
    }
    
    /// <summary>
    /// Создает игровые компоненты после завершения установки всех биндингов
    /// </summary>
    private IEnumerator CreatePlayerComponentsAfterInstall()
    {
        // Ждем один кадр, чтобы убедиться, что все установки завершены
        yield return null;
        
        Log("Creating player components after installation...");
        
        // Создаем PlayerFactory из префаба
        var playerFactoryInstance = Container.InstantiatePrefab(playerFactoryPrefab);
        var playerFactory = playerFactoryInstance.GetComponent<PlayerFactory>();
        
        if (playerFactory == null)
        {
            LogError("PlayerFactory компонент не найден в префабе!");
            yield break;
        }
        
        // Привязываем созданный экземпляр к биндингу
        Container.Bind<PlayerFactory>().FromInstance(playerFactory).AsSingle();
        Container.Bind<IPlayerFactory>().To<PlayerFactory>().FromResolve();
        Log($"PlayerFactory created and bound as singleton - GameObject: {playerFactoryInstance.name} (ID: {playerFactoryInstance.GetInstanceID()})");
        
        // Создаем PlayerSpawner из префаба
        var playerSpawnerInstance = Container.InstantiatePrefab(playerSpawnerPrefab);
        var playerSpawner = playerSpawnerInstance.GetComponent<PlayerSpawner>();
        
        if (playerSpawner == null)
        {
            LogError("PlayerSpawner компонент не найден в префабе!");
            yield break;
        }
        
        // Привязываем созданный экземпляр к биндингу
        Container.Bind<PlayerSpawner>().FromInstance(playerSpawner).AsSingle();
        Log($"PlayerSpawner created and bound as singleton - GameObject: {playerSpawnerInstance.name} (ID: {playerSpawnerInstance.GetInstanceID()})");
        
        // Создаем NetworkInputHandler из префаба
        var networkInputHandlerInstance = Container.InstantiatePrefab(networkInputHandlerPrefab);
        var networkInputHandler = networkInputHandlerInstance.GetComponent<NetworkInputHandler>();
        
        if (networkInputHandler == null)
        {
            LogError("NetworkInputHandler компонент не найден в префабе!");
            yield break;
        }
        
        // Привязываем созданный экземпляр к биндингу
        Container.Bind<NetworkInputHandler>().FromInstance(networkInputHandler).AsSingle();
        Log($"NetworkInputHandler created and bound as singleton - GameObject: {networkInputHandlerInstance.name} (ID: {networkInputHandlerInstance.GetInstanceID()})");
        
        Log("Player components creation completed successfully");
    }

    /// <summary>
    /// Создаёт PlayerCombatSetup немедленно, чтобы успеть на подписку до спавна игроков
    /// </summary>
    private void CreateCombatSetupImmediate()
    {
        var existing = GetComponentInChildren<PlayerCombatSetup>(true);
        if (existing != null)
        {
            Container.Inject(existing);
            Log("PlayerCombatSetup already exists, injected");
        }
        else
        {
            var combatSetupGo = new GameObject("PlayerCombatSetup");
            combatSetupGo.transform.SetParent(this.transform);
            var combatSetup = combatSetupGo.AddComponent<PlayerCombatSetup>();
            Container.Inject(combatSetup);
            Log("PlayerCombatSetup created and injected (immediate)");
        }

        // Пул объектов отключен — прямое инстанцирование врагов
    }
    #endregion
} 
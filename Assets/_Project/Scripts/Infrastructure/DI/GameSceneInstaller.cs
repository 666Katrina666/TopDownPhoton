using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using Fusion;

/// <summary>
/// Установщик для игровой сцены
/// Наследует от SceneInstaller и добавляет компоненты, специфичные для игрового процесса
/// </summary>
public class GameSceneInstaller : MonoInstaller
{    
    [FoldoutGroup("Debug Settings")]
    [InfoBox("Настройки отладки")]
    [SerializeField] private bool enableLogging = true;
    
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
    
    public override void InstallBindings()
    {
        Log("Starting GameScene installation...");
        
        // Сначала устанавливаем базовые сценовые компоненты
        base.InstallBindings();
        
        // Затем добавляем игровые компоненты
        InstallPlayerComponents();
        
        Log("GameScene installation completed successfully");
    }
    
    private void InstallPlayerComponents()
    {
        Log("Installing player components...");
        
        // Создаем PlayerFactory динамически
        Container.Bind<PlayerFactory>().FromNewComponentOnNewGameObject().AsSingle();
        Log("PlayerFactory bound as singleton");
        
        // Привязываем интерфейс к реализации
        Container.Bind<IPlayerFactory>().To<PlayerFactory>().FromResolve();
        Log("IPlayerFactory bound to PlayerFactory");
        
        // Создаем PlayerSpawner динамически
        Container.Bind<PlayerSpawner>().FromNewComponentOnNewGameObject().AsSingle();
        Log("PlayerSpawner bound as singleton");
    }
} 
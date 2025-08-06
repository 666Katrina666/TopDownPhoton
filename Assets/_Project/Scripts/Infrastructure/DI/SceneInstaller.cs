using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;

/// <summary>
/// Установщик для сцен-специфичных компонентов
/// Управляет компонентами, которые должны существовать только в текущей сцене
/// Привязан к SceneContext
/// </summary>
public class SceneInstaller : MonoInstaller
{    
    [FoldoutGroup("Debug Settings")]
    [InfoBox("Настройки отладки")]
    [SerializeField] private bool enableLogging = true;
    
    [FoldoutGroup("Scene")]
    [InfoBox("Сценовые компоненты")]
    [SerializeField] private bool _installGameBootstrap = true;
    
    /// <summary>
    /// Выводит сообщение в консоль, если включено логирование
    /// </summary>
    /// <param name="message">Сообщение для вывода</param>
    protected void Log(string message)
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
    protected void LogWarning(string message)
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
    protected void LogError(string message)
    {
        if (enableLogging)
        {
            Debug.LogError($"[{GetType().Name}] - {message}");
        }
    }
    
    public override void InstallBindings()
    {
        Log("Starting Scene installation...");
        
        InstallSceneComponents();
        
        Log("Scene installation completed successfully");
    }
    
    private void InstallSceneComponents()
    {
        if (_installGameBootstrap)
        {
            Container.Bind<GameBootstrap>().FromComponentInHierarchy().AsSingle();
            Log("GameBootstrap bound");
        }
        
        Container.Bind<MainMenuController>().FromComponentInHierarchy().AsSingle();
        Log("MainMenuController bound");
        
        Container.Bind<MainMenuUI>().FromComponentInHierarchy().AsSingle();
        Log("MainMenuUI bound");
        
        Container.Bind<NetworkConnectionManager>().FromComponentInHierarchy().AsSingle();
        Log("NetworkConnectionManager bound");
        
        Container.Bind<NetworkCallbackHandler>().FromComponentInHierarchy().AsSingle();
        Log("NetworkCallbackHandler bound");
        
        Container.Bind<NetworkConfigurationChecker>().FromComponentInHierarchy().AsSingle();
        Log("NetworkConfigurationChecker bound");
        
        Log("Scene components installed");
    }
    
    public override void Start()
    {
        base.Start();
        
        Log($"Start method called for object {gameObject.name} (ID: {GetInstanceID()})");
    }
} 
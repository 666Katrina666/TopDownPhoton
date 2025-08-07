using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using Fusion;

/// <summary>
/// Глобальный установщик для проекта
/// Управляет сервисами, которые должны существовать на протяжении всей игры
/// Привязан к ProjectContext
/// </summary>
[CreateAssetMenu(fileName = "ProjectInstaller", menuName = "Installers/ProjectInstaller")]
public class ProjectInstaller : ScriptableObjectInstaller<ProjectInstaller>
{    
    [FoldoutGroup("Debug Settings")]
    [InfoBox("Настройки отладки")]
    [SerializeField] private bool enableLogging = true;
    
    [FoldoutGroup("Network")]
    [InfoBox("Сетевые компоненты")]
    [SerializeField] private GameObject _networkServicePrefab;
    
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
        Log("Starting Project installation...");
        
        // Сначала устанавливаем NetworkRunner
        InstallNetworkRunner();
        
        // Затем устанавливаем сервисы и контроллеры
        InstallGlobalServices();
        
        // В конце устанавливаем отладочные сервисы
        InstallDebugServices();
        
        Log("Project installation completed successfully");
    }
    
    private void InstallNetworkRunner()
    {
        Log("Installing NetworkRunner...");
        
        var existingRunner = FindFirstObjectByType<NetworkRunner>();
        
        if (existingRunner != null)
        {
            Container.Bind<NetworkRunner>().FromInstance(existingRunner).AsSingle();
            Log($"Existing NetworkRunner bound: {existingRunner.name}");
        }
        else
        {
            Log("No existing NetworkRunner found, creating new one...");
            var runnerObject = new GameObject("NetworkRunner");
            var networkRunner = runnerObject.AddComponent<NetworkRunner>();
            
            // Устанавливаем DontDestroyOnLoad для NetworkRunner
            DontDestroyOnLoad(runnerObject);
            
            Container.Bind<NetworkRunner>().FromInstance(networkRunner).AsSingle();
            Log($"New NetworkRunner created and bound: {networkRunner.name}");
        }
    }
    
    private void InstallGlobalServices()
    {
        Log("Installing global services...");
        
        // Сначала создаем сервисы
        Container.Bind<INetworkService>().To<NetworkService>().FromMethod(CreateNetworkService).AsSingle();
        Container.Bind<ISceneService>().To<SceneService>().FromMethod(CreateSceneService).AsSingle();
        
        // Затем создаем контроллеры, которые зависят от сервисов
        Container.Bind<NetworkController>().FromMethod(CreateNetworkController).AsSingle();
        
        Log("Global services installed");
    }
    
    private NetworkService CreateNetworkService(InjectContext context)
    {
        // Проверяем, есть ли уже NetworkService в сцене или в DontDestroyOnLoad
        var existingService = FindFirstObjectByType<NetworkService>();
        
        if (existingService != null)
        {
            Log($"Existing NetworkService found: {existingService.name}");
            return existingService;
        }
        
        // Сначала пытаемся загрузить из префаба
        var networkServicePrefab = Resources.Load<GameObject>("NetworkService");
        
        if (networkServicePrefab != null)
        {
            var networkServiceInstance = Container.InstantiatePrefab(networkServicePrefab);
            var prefabNetworkService = networkServiceInstance.GetComponent<NetworkService>();
            
            if (prefabNetworkService != null)
            {
                // Устанавливаем родителя в null, чтобы объект стал корневым
                networkServiceInstance.transform.SetParent(null);
                DontDestroyOnLoad(networkServiceInstance);
                Log("NetworkService created from prefab and bound as persistent singleton");
                return prefabNetworkService;
            }
            else
            {
                LogWarning("NetworkService component not found in prefab, creating manually");
            }
        }
        else
        {
            LogWarning("NetworkService prefab not found in Resources, creating manually");
        }
        
        // Создаем вручную, если префаб недоступен
        var serviceObject = new GameObject("NetworkService");
        var manualNetworkService = Container.InstantiateComponent<NetworkService>(serviceObject);
        
        // Устанавливаем DontDestroyOnLoad чтобы объект не уничтожался при смене сцен
        DontDestroyOnLoad(serviceObject);
        
        Log("NetworkService created manually and bound as persistent singleton");
        return manualNetworkService;
    }
    
    private SceneService CreateSceneService(InjectContext context)
    {
        // Проверяем, есть ли уже SceneService в сцене или в DontDestroyOnLoad
        var existingService = FindFirstObjectByType<SceneService>();
        
        if (existingService != null)
        {
            Log($"Existing SceneService found: {existingService.name}");
            return existingService;
        }
        
        var serviceObject = new GameObject("SceneService");
        // Используем Container.InstantiateComponent для правильной инъекции зависимостей
        var sceneService = Container.InstantiateComponent<SceneService>(serviceObject);
        
        // Устанавливаем DontDestroyOnLoad чтобы объект не уничтожался при смене сцен
        DontDestroyOnLoad(serviceObject);
        
        Log("SceneService created as persistent singleton");
        return sceneService;
    }
    
    private NetworkController CreateNetworkController(InjectContext context)
    {
        // Проверяем еще раз, на случай если NetworkController был создан между проверками
        var existingController = FindFirstObjectByType<NetworkController>();
        
        if (existingController != null)
        {
            Log($"Existing NetworkController found during creation: {existingController.name}");
            return existingController;
        }
        
        var controllerObject = new GameObject("NetworkController");
        // Используем Container.InstantiateComponent для правильной инъекции зависимостей
        var networkController = Container.InstantiateComponent<NetworkController>(controllerObject);
        
        // Устанавливаем DontDestroyOnLoad чтобы объект не уничтожался при смене сцен
        DontDestroyOnLoad(controllerObject);
        
        Log("NetworkController created as persistent singleton");
        return networkController;
    }
    
    private void InstallDebugServices()
    {
        // Создаем NetworkDebugger динамически после установки NetworkRunner
        Container.Bind<NetworkDebugger>().FromMethod(CreateNetworkDebugger).AsSingle();
        Container.Bind<IDebugService>().To<NetworkDebugger>().FromResolve();
        Log("NetworkDebugger bound as IDebugService");
    }
    
    private NetworkDebugger CreateNetworkDebugger(InjectContext context)
    {
        // Проверяем, есть ли уже NetworkDebugger в сцене или в DontDestroyOnLoad
        var existingDebugger = FindFirstObjectByType<NetworkDebugger>();
        
        if (existingDebugger != null)
        {
            Log($"Existing NetworkDebugger found: {existingDebugger.name}");
            return existingDebugger;
        }
        
        var debuggerObject = new GameObject("NetworkDebugger");
        // Используем Container.InstantiateComponent для правильной инъекции зависимостей
        var networkDebugger = Container.InstantiateComponent<NetworkDebugger>(debuggerObject);
        
        // Устанавливаем DontDestroyOnLoad чтобы объект не уничтожался при смене сцен
        DontDestroyOnLoad(debuggerObject);
        
        Log("NetworkDebugger created as persistent singleton");
        return networkDebugger;
    }
} 
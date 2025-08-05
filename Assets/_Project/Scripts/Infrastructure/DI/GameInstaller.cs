using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using Fusion;

/// <summary>
/// Основной установщик для игры с DI
/// Заменяет ProjectInstaller, SceneInstaller, NetworkInstaller и ValidationInstaller
/// </summary>
public class GameInstaller : MonoInstaller
{    
    [Title("Network")]
    [FoldoutGroup("Network")]
    [InfoBox("Сетевые компоненты")]
    [SerializeField] private GameObject _networkServicePrefab;
    
    [Title("Scene")]
    [FoldoutGroup("Scene")]
    [InfoBox("Сценовые компоненты")]
    [SerializeField] private bool _installGameBootstrap = true;
    
    public override void InstallBindings()
    {
        Debug.Log("[GameInstaller] - Starting installation...");
        
        InstallConfiguration();
        
        InstallNetworkRunner();
        
        InstallNetworkServiceBindings();
        
        InstallSceneComponents();
        
        InstallFactories();
        
        InstallDebugServices();
        
        Debug.Log("[GameInstaller] - Installation completed successfully");
    }
    
    private void InstallConfiguration()
    {
        Debug.Log("[GameInstaller] - Configuration installation skipped - GameConfig managed by ConfigManager");
    }
    
    private void InstallNetworkServiceBindings()
    {
        // Привязываем интерфейсы, но не создаем экземпляры
        Container.Bind<INetworkService>().To<NetworkService>().FromMethod(CreateNetworkService).AsSingle();
        Container.Bind<ISceneService>().To<SceneService>().FromNewComponentOnNewGameObject().AsSingle();
        Debug.Log("[GameInstaller] - Network service bindings installed");
    }
    
    private NetworkService CreateNetworkService(InjectContext context)
    {
        // Проверяем, есть ли уже NetworkService в сцене
        var existingService = FindFirstObjectByType<NetworkService>();
        
        if (existingService != null)
        {
            Debug.Log($"[GameInstaller] - Existing NetworkService found: {existingService.name}");
            return existingService;
        }
        
        if (_networkServicePrefab != null)
        {
            var networkServiceInstance = Container.InstantiatePrefab(_networkServicePrefab);
            var networkService = networkServiceInstance.GetComponent<NetworkService>();
            
            if (networkService != null)
            {
                Debug.Log("[GameInstaller] - NetworkService created and bound");
                return networkService;
            }
            else
            {
                Debug.LogError("[GameInstaller] - NetworkService component not found in prefab!");
            }
        }
        else
        {
            Debug.LogError("[GameInstaller] - NetworkService prefab not assigned!");
        }
        
        return null;
    }
    
    private void InstallNetworkRunner()
    {
        Debug.Log("[GameInstaller] - Installing NetworkRunner...");
        
        var existingRunner = FindFirstObjectByType<NetworkRunner>();
        
        if (existingRunner != null)
        {
            Container.Bind<NetworkRunner>().FromInstance(existingRunner).AsSingle();
            Debug.Log($"[GameInstaller] - Existing NetworkRunner bound: {existingRunner.name}");
        }
        else
        {
            Debug.Log("[GameInstaller] - No existing NetworkRunner found, creating new one...");
            var runnerObject = new GameObject("NetworkRunner");
            var networkRunner = runnerObject.AddComponent<NetworkRunner>();
            
            Container.Bind<NetworkRunner>().FromInstance(networkRunner).AsSingle();
            Debug.Log($"[GameInstaller] - New NetworkRunner created and bound: {networkRunner.name}");
        }
    }
    
    private void InstallSceneComponents()
    {
        if (_installGameBootstrap)
        {
            Container.Bind<GameBootstrap>().FromComponentInHierarchy().AsSingle();
            Debug.Log("[GameInstaller] - GameBootstrap bound");
        }
        
        Container.Bind<NetworkController>().FromComponentInHierarchy().AsSingle();
        Debug.Log("[GameInstaller] - NetworkController bound");
        
        Container.Bind<MainMenuController>().FromComponentInHierarchy().AsSingle();
        Debug.Log("[GameInstaller] - MainMenuController bound");
        
        Container.Bind<MainMenuUI>().FromComponentInHierarchy().AsSingle();
        Debug.Log("[GameInstaller] - MainMenuUI bound");
        
        Container.Bind<NetworkConnectionManager>().FromComponentInHierarchy().AsSingle();
        Debug.Log("[GameInstaller] - NetworkConnectionManager bound");
        
        Container.Bind<NetworkCallbackHandler>().FromComponentInHierarchy().AsSingle();
        Debug.Log("[GameInstaller] - NetworkCallbackHandler bound");
        
        Container.Bind<NetworkConfigurationChecker>().FromComponentInHierarchy().AsSingle();
        Debug.Log("[GameInstaller] - NetworkConfigurationChecker bound");
        
        Container.Bind<PlayerSpawner>().FromComponentInHierarchy().AsSingle();
        Debug.Log("[GameInstaller] - PlayerSpawner bound");
        
        Container.Bind<PlayerFactory>().FromComponentInHierarchy().AsSingle();
        Debug.Log("[GameInstaller] - PlayerFactory bound");
        
        Container.Bind<IPlayerFactory>().To<PlayerFactory>().FromResolve();
        Debug.Log("[GameInstaller] - IPlayerFactory bound");
    }
    
    private void InstallFactories()
    {
        Debug.Log("[GameInstaller] - Factories installed");
    }
    
    private void InstallDebugServices()
    {
        // Создаем NetworkDebugger динамически
        Container.Bind<NetworkDebugger>().FromMethod(CreateNetworkDebugger).AsSingle();
        Container.Bind<IDebugService>().To<NetworkDebugger>().FromResolve();
        Debug.Log("[GameInstaller] - NetworkDebugger bound as IDebugService");
    }
    
    private NetworkDebugger CreateNetworkDebugger(InjectContext context)
    {
        // Проверяем, есть ли уже NetworkDebugger в сцене
        var existingDebugger = FindFirstObjectByType<NetworkDebugger>();
        
        if (existingDebugger != null)
        {
            Debug.Log($"[GameInstaller] - Existing NetworkDebugger found: {existingDebugger.name}");
            return existingDebugger;
        }
        
        var debuggerObject = new GameObject("NetworkDebugger");
        var networkDebugger = debuggerObject.AddComponent<NetworkDebugger>();
        
        Debug.Log("[GameInstaller] - NetworkDebugger created dynamically");
        return networkDebugger;
    }
    
    public override void Start()
    {
        base.Start();
        
        Debug.Log("[GameInstaller] - Start method called");
    }
} 
using UnityEngine;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Менеджер конфигурации игры
/// Предоставляет доступ к GameConfig через статический метод
/// </summary>
public class ConfigManager : LoggableMonoBehaviour
{
    [Title("Configuration")]
    [FoldoutGroup("Game Config")]
    [InfoBox("Основная конфигурация игры")]
    [SerializeField] private GameConfig _gameConfig;
    
    [ShowInInspector, ReadOnly]
    private static ConfigManager _instance;
    
    public static GameConfig GameConfig
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("[ConfigManager] - ConfigManager instance not found! Make sure ConfigManager is present in the scene.");
                return null;
            }
            
            return _instance._gameConfig;
        }
    }
    
    private void Awake()
    {
        Log("Awake");
        
        // Проверяем, что у нас только один экземпляр
        ConfigManager[] existingManagers = FindObjectsByType<ConfigManager>(FindObjectsSortMode.None);
        if (existingManagers.Length > 1)
        {
            LogWarning("Multiple ConfigManager found! Destroying duplicate");
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (_gameConfig == null)
        {
            LogError("GameConfig is not assigned! Please assign GameConfig in the inspector.");
        }
        else
        {
            Log($"GameConfig loaded: {_gameConfig.name}");
        }
    }
    
    private void OnDestroy()
    {
        Log("OnDestroy");
        
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    /// <summary>
    /// Проверяет, инициализирован ли ConfigManager
    /// </summary>
    public static bool IsInitialized => _instance != null && _instance._gameConfig != null;
} 
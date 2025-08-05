using UnityEngine;
using Sirenix.OdinInspector;
using Core.Base;

/// <summary>
/// Пример использования ConfigManager
/// Демонстрирует как получить доступ к GameConfig из любого места
/// </summary>
public class ConfigManagerExample : LoggableMonoBehaviour
{
    [Title("Config Manager Example")]
    [FoldoutGroup("Runtime Info")]
    [ShowInInspector, ReadOnly]
    private bool _isConfigInitialized;
    
    [ShowInInspector, ReadOnly]
    private string _currentGameScene;
    
    [ShowInInspector, ReadOnly]
    private bool _debugLogsEnabled;
    
    private void Start()
    {
        Log("Start");
        
        // Проверяем инициализацию ConfigManager
        _isConfigInitialized = ConfigManager.IsInitialized;
        
        if (_isConfigInitialized)
        {
            // Получаем данные из конфига
            _currentGameScene = ConfigManager.GameConfig.GameSceneName;
            _debugLogsEnabled = ConfigManager.GameConfig.EnableDebugLogs;
            
            Log($"Config loaded successfully. GameScene: {_currentGameScene}, DebugLogs: {_debugLogsEnabled}");
        }
        else
        {
            LogError("ConfigManager is not initialized!");
        }
    }
    
    [Button("Test Config Access")]
    private void TestConfigAccess()
    {
        if (ConfigManager.IsInitialized)
        {
            var config = ConfigManager.GameConfig;
            Log("Config test successful:");
            Log($"  - Game Scene: {config.GameSceneName}");
            Log($"  - Lobby Scene: {config.LobbySceneName}");
            Log($"  - Spawn Offset X: {config.SpawnOffsetX}");
            Log($"  - Debug Logs: {config.EnableDebugLogs}");
        }
        else
        {
            LogError("Cannot access config - ConfigManager not initialized!");
        }
    }
} 
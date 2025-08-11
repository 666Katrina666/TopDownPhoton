using UnityEngine;
using UnityEditor;

/// <summary>
/// Вспомогательный класс для настройки ConfigManager
/// </summary>
public static class ConfigManagerSetup
{
    /// <summary>
    /// Создает префаб ConfigManager с назначенным GameConfig
    /// </summary>
    [MenuItem("Game/Setup/Create ConfigManager Prefab")]
    public static void CreateConfigManagerPrefab()
    {
        // Создаем GameObject с ConfigManager
        GameObject configManagerGO = new GameObject("ConfigManager");
        ConfigManager configManager = configManagerGO.AddComponent<ConfigManager>();
        
        // Загружаем GameConfig из ScriptableObjects/Resources
        GameConfig gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/_Project/ScriptableObjects/Resources/GameConfig.asset");
        
        if (gameConfig != null)
        {
            // Используем reflection для установки приватного поля
            var field = typeof(ConfigManager).GetField("_gameConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(configManager, gameConfig);
            
            Debug.Log("[ConfigManagerSetup] - GameConfig assigned to ConfigManager");
        }
        else
        {
            Debug.LogError("[ConfigManagerSetup] - GameConfig not found at Assets/_Project/ScriptableObjects/Resources/GameConfig.asset");
        }
        
        // Создаем префаб
        string prefabPath = "Assets/_Project/Prefabs/SceneManagers/ConfigManager.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(configManagerGO, prefabPath);
        
        // Удаляем временный GameObject
        Object.DestroyImmediate(configManagerGO);
        
        Debug.Log($"[ConfigManagerSetup] - ConfigManager prefab created at {prefabPath}");
        
        // Выделяем созданный префаб в Project window
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }
} 
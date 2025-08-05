using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Централизованная конфигурация игры
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Configuration/GameConfig")]
public class GameConfig : ScriptableObject
{
    [FoldoutGroup("Scene Names")]
    [InfoBox("Названия сцен для загрузки")]
    [SerializeField] private string _gameSceneName = "GameScene";
    [FoldoutGroup("Scene Names")]
    [SerializeField] private string _lobbySceneName = "LobbyScene";
    [FoldoutGroup("Scene Names")]
    [SerializeField] private string _mainMenuSceneName = "MainMenuScene";
    
    [FoldoutGroup("Player Settings")]
    [InfoBox("Настройки спавна игроков")]
    [SerializeField] private float _spawnOffsetX = 2f;
    [FoldoutGroup("Player Settings")]
    [SerializeField] private float _spawnHeight = 1f;
    
    [FoldoutGroup("Debug Settings")]
    [InfoBox("Настройки отладки")]
    [SerializeField] private bool _enableNetworkDebugger = true;
    [FoldoutGroup("Debug Settings")]
    [SerializeField] private bool _enableDebugLogs = true;
    
    public string GameSceneName => _gameSceneName;
    public string LobbySceneName => _lobbySceneName;
    public string MainMenuSceneName => _mainMenuSceneName;
    
    public float SpawnOffsetX => _spawnOffsetX;
    public float SpawnHeight => _spawnHeight;
    
    public bool EnableNetworkDebugger => _enableNetworkDebugger;
    public bool EnableDebugLogs => _enableDebugLogs;
} 
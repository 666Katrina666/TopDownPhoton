using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using Fusion;

/// <summary>
/// Основной установщик для игры
/// Наследует от SceneInstaller и добавляет базовые сценовые компоненты
/// Глобальные сервисы управляются через ProjectInstaller
/// </summary>
public class GameInstaller : SceneInstaller
{    
    public override void InstallBindings()
    {
        Log("Starting Game installation...");
        
        // Сначала устанавливаем базовые сценовые компоненты
        base.InstallBindings();
        
        // Затем добавляем дополнительные компоненты если нужно
        InstallAdditionalComponents();
        
        Log("Game installation completed successfully");
    }
    
    private void InstallAdditionalComponents()
    {
        Log("Installing additional components...");
        
        // Здесь можно добавить дополнительные компоненты, специфичные для игры
        // но не относящиеся к конкретной сцене
        
        Log("Additional components installed");
    }
} 
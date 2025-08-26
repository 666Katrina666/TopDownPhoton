using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// Конфигурация волны с настройками и доступными юнитами
/// </summary>
[CreateAssetMenu(fileName = "WaveData", menuName = "Gameplay/Wave Data")]
public class WaveData : ScriptableObject
{
    [Title("Настройки волны")]
    [SerializeField] private int _pointsLimit; // Лимит очков на волну
    [SerializeField] private float _pointsMultiplier = 1.2f; // Множитель очков для бесконечных волн
    
    [Title("Доступные юниты")]
    [SerializeField] private List<EnemySpawnData> _availableEnemies; // Список доступных юнитов
    
    // Properties для соблюдения инкапсуляции
    public int PointsLimit => _pointsLimit;
    public float PointsMultiplier => _pointsMultiplier;
    public IReadOnlyList<EnemySpawnData> AvailableEnemies => _availableEnemies.AsReadOnly();
}

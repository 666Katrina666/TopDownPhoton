using UnityEngine;

/// <summary>
/// Интерфейс менеджера волн
/// </summary>
public interface IWaveManager
{
    void StartWave(int waveNumber);
    bool CanSpawnEnemy(EnemySpawnData enemyData);
    EnemySpawnData GetRandomEnemy();
    void SpendPoints(int cost);
    bool IsWaveComplete { get; }
    int RemainingPoints { get; }
    WaveData CurrentWaveData { get; }
}

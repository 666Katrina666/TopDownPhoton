using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// Менеджер волн с логикой бесконечных волн
/// </summary>
public class WaveManager : IWaveManager
{
    [Inject] private WaveData[] _waveConfigurations;

    private int _currentWaveIndex;
    private int _remainingPoints;
    private WaveData _currentWaveData;

    /// <summary>
    /// Текущая конфигурация волны
    /// </summary>
    public WaveData CurrentWaveData => _currentWaveData;

    /// <summary>
    /// Оставшиеся очки волны
    /// </summary>
    public int RemainingPoints => _remainingPoints;

    /// <summary>
    /// Завершена ли волна (очков не осталось)
    /// </summary>
    public bool IsWaveComplete => _remainingPoints <= 0;

    /// <summary>
    /// Запускает указанную волну
    /// </summary>
    public void StartWave(int waveNumber)
    {
        _currentWaveIndex = waveNumber;
        _currentWaveData = GetWaveDataForIndex(waveNumber);
        _remainingPoints = CalculatePointsForWave(waveNumber);

        EventBus.RaiseEvent(new WaveStartedEvent
        {
            WaveNumber = waveNumber,
            PointsLimit = _remainingPoints
        });
    }

    /// <summary>
    /// Проверяет, можно ли заспавнить юнита с учетом оставшихся очков
    /// </summary>
    public bool CanSpawnEnemy(EnemySpawnData enemyData)
    {
        return enemyData.Cost <= _remainingPoints;
    }

    /// <summary>
    /// Возвращает случайного юнита с учетом весов появлений
    /// </summary>
    public EnemySpawnData GetRandomEnemy()
    {
        var list = _currentWaveData.AvailableEnemies;
        float total = 0f;
        for (int i = 0; i < list.Count; i++) total += list[i].SpawnWeight;

        float pick = Random.value * total;
        float cumulative = 0f;
        for (int i = 0; i < list.Count; i++)
        {
            cumulative += list[i].SpawnWeight;
            if (pick <= cumulative)
                return list[i];
        }
        return list[list.Count - 1];
    }

    /// <summary>
    /// Списывает очки после выбора юнита и завершает волну при необходимости
    /// </summary>
    public void SpendPoints(int cost)
    {
        _remainingPoints = Mathf.Max(0, _remainingPoints - Mathf.Max(0, cost));
        if (_remainingPoints <= 0)
        {
            EventBus.RaiseEvent(new WaveCompletedEvent
            {
                WaveNumber = _currentWaveIndex
            });
        }
    }

    private WaveData GetWaveDataForIndex(int waveIndex)
    {
        int lastIndex = Mathf.Max(0, _waveConfigurations.Length - 1);
        return waveIndex <= lastIndex ? _waveConfigurations[waveIndex] : _waveConfigurations[lastIndex];
    }

    private int CalculatePointsForWave(int waveIndex)
    {
        int lastIndex = Mathf.Max(0, _waveConfigurations.Length - 1);
        if (waveIndex <= lastIndex)
        {
            return _waveConfigurations[waveIndex].PointsLimit;
        }
        var last = _waveConfigurations[lastIndex];
        int extra = waveIndex - lastIndex;
        float scaled = last.PointsLimit * Mathf.Pow(last.PointsMultiplier, extra);
        return Mathf.RoundToInt(scaled);
    }
}


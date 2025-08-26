/// <summary>
/// Интерфейс фабрики юнитов для соблюдения принципа инверсии зависимостей
/// </summary>
public interface IEnemyFactory
{
    void Initialize();
    void StartWave(int waveNumber);
    void StopSpawning();
    bool IsWaveComplete { get; }
    int CurrentWave { get; }
}

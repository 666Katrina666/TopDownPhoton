using UnityEngine;

/// <summary>
/// Данные для спавна юнита в волне
/// </summary>
[System.Serializable]
public class EnemySpawnData
{
    [SerializeField] private GameObject _enemyPrefab; // Префаб юнита (NetworkObject)
    [SerializeField] private int _cost; // Стоимость в очках
    [SerializeField, Range(0f, 1f)] private float _spawnWeight = 1f; // Вес для случайного выбора
    
    // Properties для соблюдения инкапсуляции
    public GameObject EnemyPrefab => _enemyPrefab;
    public int Cost => _cost;
    public float SpawnWeight => _spawnWeight;
}

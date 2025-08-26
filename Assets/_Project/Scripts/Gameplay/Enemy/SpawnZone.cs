using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Зона спавна юнитов с настройками позиции
/// </summary>
[System.Serializable]
public class SpawnZone
{
    [Title("Зона спавна")]
    [SerializeField] private Vector2 _center;
    [SerializeField] private Vector2 _size;
    [SerializeField] private float _minDistanceFromPlayer = 5f;
    
    // Properties
    public Vector2 Center => _center;
    public Vector2 Size => _size;
    public float MinDistanceFromPlayer => _minDistanceFromPlayer;
    
    /// <summary>
    /// Получает случайную позицию в зоне спавна
    /// </summary>
    public Vector3 GetRandomPosition()
    {
        float x = Random.Range(_center.x - _size.x / 2, _center.x + _size.x / 2);
        float y = Random.Range(_center.y - _size.y / 2, _center.y + _size.y / 2);
        return new Vector3(x, y, 0);
    }
}

using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Конфигурация случайного движения юнита с обходом препятствий.
/// Хранит настраиваемые параметры для поведения перемещения.
/// </summary>
[CreateAssetMenu(fileName = "RandomMoveConfig", menuName = "_Project/Movement/RandomMoveConfig")]
public class RandomMoveConfig : ScriptableObject
{
    [Title("Скорость и тайминги")]
    [Min(0f)] public float moveSpeed = 2f;
    [Min(0f)] public float maximumDurationInDirection = 2f;
    [Min(0f)] public float obstaclesCheckFrequency = 0.05f;
    [Min(0f)] public float minHoldDuration = 1.5f;
    [Min(0f)] public float maxHoldDuration = 3.5f;
    [Min(0f)] public float directionSmoothing = 8f;

    [Title("Обход препятствий")]
    public LayerMask obstacleLayerMask;
    [Min(0f)] public float obstacleDetectionDistance = 1f;
    [Min(0f)] public float obstacleRepathCooldown = 0.1f;
    [Min(0f)] public float dodgeNoise = 0.2f;

    [Title("Границы экрана")]
    public bool respectScreenBounds = true;
    [Min(0f)] public float screenBoundsPadding = 0.25f;

    [Title("Диапазон случайного направления")]
    public Vector2 minimumRandomDirection = new Vector2(-1f, -1f);
    public Vector2 maximumRandomDirection = new Vector2(1f, 1f);
}



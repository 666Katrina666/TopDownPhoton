using UnityEngine;
using Sirenix.OdinInspector;
using Fusion;

/// <summary>
/// Компонент случайного движения юнита с обходом препятствий.
/// Использует конфигурацию для параметров и выполняет перемещение через Rigidbody2D.
/// </summary>
public class UnitRandomMover : MonoBehaviour
{
    [Title("Конфигурация")]
    [SerializeField] private RandomMoveConfig _config;

    [Title("Компоненты")] 
    [SerializeField, Required] private Rigidbody2D _rigidbody;
    [SerializeField, Required] private Collider2D _collider;
    [SerializeField, Required] private NetworkObject _networkObject;

    [Title("Состояние")]
    [SerializeField, Sirenix.OdinInspector.ReadOnly] private Vector2 _currentDirection;
    [SerializeField, Sirenix.OdinInspector.ReadOnly] private Vector2 _targetDirection;
    [SerializeField, Sirenix.OdinInspector.ReadOnly] private float _currentHoldDuration;

    private float _lastObstacleDetectionTime;
    private float _lastDirectionChangeTime;
    private float _lastRepathFromObstacleTime;

    /// <summary>
    /// Инициализация внутренних ссылок и установка стартового направления.
    /// </summary>
    private void Awake()
    {
        PickRandomDirection();
    }

    /// <summary>
    /// Подписка на события.
    /// </summary>
    private void OnEnable()
    {
        EventBus.Subscribe<DeathEvent>(OnDeathEvent);
    }

    /// <summary>
    /// Отписка от событий.
    /// </summary>
    private void OnDisable()
    {
        EventBus.Unsubscribe<DeathEvent>(OnDeathEvent);
    }

    /// <summary>
    /// Фикс-обновление перемещения и проверок обхода.
    /// </summary>
    private void FixedUpdate()
    {
        CheckForObstacles();
        CheckForDuration();
        SmoothDirection();
        Move();
    }

    /// <summary>
    /// Применяет движение к Rigidbody2D.
    /// </summary>
    private void Move()
    {
        Vector2 moveDir = _currentDirection.sqrMagnitude > 0.0001f ? _currentDirection.normalized : Vector2.zero;
        Vector2 delta = moveDir * _config.moveSpeed * Time.fixedDeltaTime;

        if (_config.respectScreenBounds)
        {
            Vector3 worldMin, worldMax;
            GetScreenWorldBounds(out worldMin, out worldMax);

            Vector2 target = _rigidbody.position + delta;
            float halfW = _collider.bounds.extents.x + _config.screenBoundsPadding;
            float halfH = _collider.bounds.extents.y + _config.screenBoundsPadding;
            target.x = Mathf.Clamp(target.x, worldMin.x + halfW, worldMax.x - halfW);
            target.y = Mathf.Clamp(target.y, worldMin.y + halfH, worldMax.y - halfH);

            // если кламп изменил цель, подстроим направление вдоль границы
            if ((target - (Vector2)_rigidbody.position - delta).sqrMagnitude > 0.000001f)
            {
                Vector2 toEdge = (target - _rigidbody.position);
                if (toEdge.sqrMagnitude > 0.000001f)
                {
                    SetTargetDirection(toEdge.normalized);
                }
            }

            _rigidbody.MovePosition(target);
        }
        else
        {
            _rigidbody.MovePosition(_rigidbody.position + delta);
        }
    }

    /// <summary>
    /// Получает мировые границы текущего видимого экрана основной камеры.
    /// </summary>
    private void GetScreenWorldBounds(out Vector3 min, out Vector3 max)
    {
        Camera cam = Camera.main;
        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, Mathf.Abs(cam.transform.position.z)));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, Mathf.Abs(cam.transform.position.z)));
        min = bl;
        max = tr;
    }

    /// <summary>
    /// Проверяет препятствия по направлению движения и выбирает новое направление при обнаружении.
    /// </summary>
    private void CheckForObstacles()
    {
        if (Time.time - _lastObstacleDetectionTime < _config.obstaclesCheckFrequency)
            return;

        Vector2 origin = _collider.bounds.center;
        Vector2 size = _collider.bounds.size;
        float distance = Mathf.Max(_config.obstacleDetectionDistance, 0f);
        Vector2 forward = _currentDirection.sqrMagnitude > 0.0001f ? _currentDirection.normalized : _targetDirection.normalized;
        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, forward, distance, _config.obstacleLayerMask);
        if (hit && Time.time - _lastRepathFromObstacleTime > _config.obstacleRepathCooldown)
        {
            Vector2 normal = hit.normal;
            Vector2 tangent = new Vector2(-normal.y, normal.x).normalized;
            float side = Random.value < 0.5f ? -1f : 1f;
            Vector2 dodgeDir = tangent * side;
            if (_config.dodgeNoise > 0f)
            {
                Vector2 noise = new Vector2(Random.Range(-_config.dodgeNoise, _config.dodgeNoise), Random.Range(-_config.dodgeNoise, _config.dodgeNoise));
                dodgeDir = (dodgeDir + noise).normalized;
            }
            SetTargetDirection(dodgeDir);
            _lastRepathFromObstacleTime = Time.time;
        }

        _lastObstacleDetectionTime = Time.time;
    }

    /// <summary>
    /// Проверяет срок движения в текущем направлении и при необходимости меняет его.
    /// </summary>
    private void CheckForDuration()
    {
        if (Time.time - _lastDirectionChangeTime > _currentHoldDuration)
        {
            PickRandomDirection();
        }
    }

    /// <summary>
    /// Выбирает новое случайное направление из диапазона конфига.
    /// </summary>
    private void PickRandomDirection()
    {
        _targetDirection.x = Random.Range(_config.minimumRandomDirection.x, _config.maximumRandomDirection.x);
        _targetDirection.y = Random.Range(_config.minimumRandomDirection.y, _config.maximumRandomDirection.y);
        _currentHoldDuration = Mathf.Max(_config.minHoldDuration, _config.maxHoldDuration);
        if (_config.maxHoldDuration > _config.minHoldDuration)
        {
            _currentHoldDuration = Random.Range(_config.minHoldDuration, _config.maxHoldDuration);
        }
        _lastDirectionChangeTime = Time.time;
    }

    /// <summary>
    /// Плавно приближает текущее направление к целевому.
    /// </summary>
    private void SmoothDirection()
    {
        float smoothing = Mathf.Max(_config.directionSmoothing, 0f);
        if (smoothing <= 0f)
        {
            _currentDirection = _targetDirection;
            return;
        }
        _currentDirection = Vector2.Lerp(_currentDirection, _targetDirection, 1f - Mathf.Exp(-smoothing * Time.fixedDeltaTime));
    }

    /// <summary>
    /// Устанавливает новое целевое направление и перезапускает интервал удержания.
    /// </summary>
    private void SetTargetDirection(Vector2 dir)
    {
        _targetDirection = dir;
        _lastDirectionChangeTime = Time.time;
        _currentHoldDuration = Random.Range(_config.minHoldDuration, _config.maxHoldDuration);
    }

    /// <summary>
    /// Реакция на событие смерти: отключает компонент для текущего сетевого объекта.
    /// </summary>
    private void OnDeathEvent(DeathEvent evt)
    {
        if (evt.Target != _networkObject) return;
        enabled = false;
    }
}



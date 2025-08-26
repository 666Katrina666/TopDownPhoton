using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Fusion;

/// <summary>
/// Обработчик смерти объекта с анимацией исчезновения
/// </summary>
public class DeathHandler : MonoBehaviour
{
    [Header("Настройки смерти")]
    [SerializeField] private float delayBeforeFade = 2f; // Задержка перед началом исчезновения
    [SerializeField] private float fadeDuration = 3f; // Длительность исчезновения
    
    [Header("Компоненты")]
    [SerializeField] private GameObject healthBar; // UI полоска здоровья (ребенок с компонентом HealthBarView)
    [SerializeField] private Collider2D objectCollider; // Коллайдер объекта
    [SerializeField] private SpriteRenderer spriteRenderer; // Спрайт рендерер для анимации прозрачности
    
    private bool isDead = false;
    private Sequence deathSequence; // Последовательность анимаций смерти
    private NetworkObject networkObject; // Сетевой объект для проверки
    
    /// <summary>
    /// Инициализация компонентов
    /// </summary>
    private void Start()
    {
        if (healthBar == null)
            healthBar = transform.Find("HealthBar")?.gameObject;
            
        if (objectCollider == null)
            objectCollider = GetComponent<Collider2D>();
            
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        networkObject = GetComponentInParent<NetworkObject>();
        
        EventBus.Subscribe<DeathEvent>(OnDeathEvent);
    }
    
    /// <summary>
    /// Обработка события смерти
    /// </summary>
    private void OnDeathEvent(DeathEvent evt)
    {
        if (networkObject != null && evt.Target != networkObject) return;
        
        OnDeath();
    }
    
    /// <summary>
    /// Обработка смерти объекта
    /// </summary>
    public void OnDeath()
    {
        if (isDead) return;
        
        isDead = true;
        
        DisableComponents();
        StartDeathSequence();
    }
    
    /// <summary>
    /// Создание последовательности анимаций смерти
    /// </summary>
    private void StartDeathSequence()
    {
        if (deathSequence != null)
        {
            deathSequence.Kill();
        }
        
        deathSequence = DOTween.Sequence();
        
        deathSequence
            .AppendInterval(delayBeforeFade)
            .Append(spriteRenderer.DOFade(0f, fadeDuration))
            .OnComplete(Death);
            
        deathSequence.Play();
    }
    
    /// <summary>
    /// Отключение UI и коллайдера
    /// </summary>
    private void DisableComponents()
    {
        if (healthBar != null)
            healthBar.SetActive(false);
            
        if (objectCollider != null)
            objectCollider.enabled = false;
    }
    
    /// <summary>
    /// Завершение жизни объекта (пул отключен)
    /// </summary>
    private void Death()
    {
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Очистка при уничтожении объекта
    /// </summary>
    private void OnDestroy()
    {
        EventBus.Unsubscribe<DeathEvent>(OnDeathEvent);
        
        if (deathSequence != null)
        {
            deathSequence.Kill();
            deathSequence = null;
        }
    }
}

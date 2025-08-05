using UnityEngine;

namespace Core.Base
{
    /// <summary>
    /// Базовый класс для MonoBehaviour с системой логирования
    /// </summary>
    public abstract class LoggableMonoBehaviour : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] protected bool enableLogging = true;

        /// <summary>
        /// Выводит сообщение в консоль, если включено логирование
        /// </summary>
        /// <param name="message">Сообщение для вывода</param>
        protected void Log(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"[{GetType().Name}] - {message}");
            }
        }

        /// <summary>
        /// Выводит предупреждение в консоль, если включено логирование
        /// </summary>
        /// <param name="message">Сообщение для вывода</param>
        protected void LogWarning(string message)
        {
            if (enableLogging)
            {
                Debug.LogWarning($"[{GetType().Name}] - {message}");
            }
        }

        /// <summary>
        /// Выводит ошибку в консоль, если включено логирование
        /// </summary>
        /// <param name="message">Сообщение для вывода</param>
        protected void LogError(string message)
        {
            if (enableLogging)
            {
                Debug.LogError($"[{GetType().Name}] - {message}");
            }
        }
    }
} 
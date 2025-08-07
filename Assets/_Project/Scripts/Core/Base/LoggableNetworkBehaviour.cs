using UnityEngine;
using Fusion;
using Sirenix.OdinInspector;

namespace Core.Base
{
    /// <summary>
    /// Базовый класс для NetworkBehaviour с системой логирования
    /// </summary>
    public abstract class LoggableNetworkBehaviour : NetworkBehaviour
    {
        [FoldoutGroup("Debug Settings")]
        [InfoBox("Настройки отладки")]
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
            Debug.LogWarning($"[{GetType().Name}] - {message}");
        }

        /// <summary>
        /// Выводит ошибку в консоль, если включено логирование
        /// </summary>
        /// <param name="message">Сообщение для вывода</param>
        protected void LogError(string message)
        {
            Debug.LogError($"[{GetType().Name}] - {message}");
        }
    }
}

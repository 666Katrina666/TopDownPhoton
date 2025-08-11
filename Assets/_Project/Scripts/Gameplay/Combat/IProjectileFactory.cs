using Fusion;
using UnityEngine;

/// <summary>
/// Фабрика для создания снарядов
/// </summary>
public interface IProjectileFactory
{
    /// <summary>
    /// Создаёт сетевой снаряд
    /// </summary>
    /// <param name="owner">Владалец ввода, от чьего имени создаётся снаряд</param>
    /// <param name="position">Позиция спавна</param>
    /// <param name="direction">Нормализованное направление полёта</param>
    /// <returns>Компонент снаряда или null при неудаче</returns>
    Projectile Spawn(PlayerRef owner, Vector2 position, Vector2 direction);
}



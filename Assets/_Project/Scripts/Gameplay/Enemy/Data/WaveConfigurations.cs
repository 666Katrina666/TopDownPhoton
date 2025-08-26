using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Конфигурация всех волн игры
/// </summary>
[CreateAssetMenu(fileName = "WaveConfigurations", menuName = "Gameplay/Wave Configurations")]
public class WaveConfigurations : ScriptableObject
{
    [Title("Конфигурации волн")]
    [InfoBox("Массив конфигураций волн. Индекс массива = номер волны")]
    [SerializeField] private WaveData[] _waves;
    
    /// <summary>
    /// Массив конфигураций волн
    /// </summary>
    public WaveData[] Waves => _waves;
}


using UnityEngine;
using Fusion;

/// <summary>
/// Структура для передачи ввода игрока по сети
/// </summary>
public struct NetworkInputData : INetworkInput
{
    public Vector2 moveDirection;
    public bool isMoving;
    public bool isInteracting;
    public bool isFiring;
} 
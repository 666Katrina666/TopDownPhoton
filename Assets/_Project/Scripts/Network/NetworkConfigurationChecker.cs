using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using Zenject;
using Core.Base;

/// <summary>
/// Проверяет правильность конфигурации NetworkRunner
/// </summary>
public class NetworkConfigurationChecker : LoggableMonoBehaviour
{
    
    [Title("Configuration Check")]
    [FoldoutGroup("Check Results", false)]
    
    [Inject] private NetworkRunner _networkRunner;
    [Inject] private PlayerSpawner _playerSpawner;
    
    private void Start()
    {
        CheckConfiguration();
    }
    
    [Button("Check Configuration")]
    private void CheckConfiguration()
    {
        CheckNetworkRunner();
        CheckPlayerSpawner();
        CheckPlayerPrefab();
    }
    
    private void CheckNetworkRunner()
    {
        if (_networkRunner != null)
        {
            Log($"NetworkRunner found: {_networkRunner}, IsServer: {_networkRunner.IsServer}, IsClient: {_networkRunner.IsClient}, IsRunning: {_networkRunner.IsRunning}");
        }
    }
    
    private void CheckPlayerSpawner()
    {
        if (_playerSpawner != null)
        {
            Log($"PlayerSpawner found: {_playerSpawner}");
        }
    }
    
    private void CheckPlayerPrefab()
    {
        if (_playerSpawner != null)
        {
            var field = typeof(PlayerSpawner).GetField("_playerPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var prefabRef = field.GetValue(_playerSpawner);
                if (prefabRef != null && !prefabRef.Equals(default(NetworkPrefabRef)))
                {
                }
                else
                {
                    LogError("PlayerPrefab not configured in PlayerSpawner!");
                }
            }
        }
    }
    
    [Button("Log Network State")]
    private void LogNetworkState()
    {
        if (_networkRunner != null)
        {
            Log($"Network State: Runner={_networkRunner}, IsServer={_networkRunner.IsServer}, IsClient={_networkRunner.IsClient}, IsRunning={_networkRunner.IsRunning}, LocalPlayer={_networkRunner.LocalPlayer}, ActivePlayers={_networkRunner.ActivePlayers.Count()}");
            
            foreach (var player in _networkRunner.ActivePlayers)
            {
                Log($"Active Player: {player}, IsLocalPlayer: {player == _networkRunner.LocalPlayer}");
            }
        }
    }
} 
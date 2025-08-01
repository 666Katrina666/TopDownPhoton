using UnityEngine;
using Fusion;

public struct StartGameRequestEvent
{
    public string GameSceneName;
    
    public StartGameRequestEvent(string gameSceneName)
    {
        GameSceneName = gameSceneName;
    }
}
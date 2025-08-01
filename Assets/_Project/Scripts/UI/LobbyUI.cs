using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] string gameSceneName = "GameScene";
    [SerializeField] private Button startGameButton;
    
    private void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        // Получаем NetworkRunner из сцены
        var networkRunner = FindFirstObjectByType<NetworkRunner>();
        startGameButton.onClick.AddListener(OnStartGameButtonPressed);
    }
    
    /// <summary>
    /// Вызывается при нажатии кнопки "Начать игру"
    /// </summary>
    public void OnStartGameButtonPressed()
    {
        EventBus.RaiseEvent(new StartGameRequestEvent(gameSceneName));
    }
}
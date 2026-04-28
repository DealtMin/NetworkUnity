using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private GameObject _menuPanel;
    
    public static string PlayerNickname { get; private set; } = "Player";

    private void Start()
    {
        // Подписываемся на события NetworkManager
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        
        // Показываем меню при старте
        ShowMenu();
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    public void StartAsHost()
    {
        SaveNickname();
        NetworkManager.Singleton.StartHost();
    }

    public void StartAsClient()
    {
        SaveNickname();
        NetworkManager.Singleton.StartClient();
    }

    private void SaveNickname()
    {
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }
    
    private void OnClientConnected(ulong clientId)
    {
        // Проверяем, что это наш локальный клиент подключился
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            HideMenu();
            Debug.Log($"[ConnectionUI] Connected as {PlayerNickname}, hiding menu");
        }
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        // Если отключился наш клиент - показываем меню снова
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            ShowMenu();
            Debug.Log("[ConnectionUI] Disconnected, showing menu");
        }
    }
    
    private void HideMenu()
    {
        if (_menuPanel != null)
        {
            _menuPanel.SetActive(false);
        }
        else
        {
            // Если панель не назначена, скрываем весь объект
            gameObject.SetActive(false);
        }
    }
    
    private void ShowMenu()
    {
        if (_menuPanel != null)
        {
            _menuPanel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}
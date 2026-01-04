using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Connection;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main menu controller - handles hosting, joining, and transitioning to game.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenuPanel;
    public GameObject lobbyPanel;
    
    [Header("Main Menu Buttons")]
    public Button hostButton;
    public Button joinButton;
    public Button quitButton;
    
    [Header("Lobby UI")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI playerCountText;
    public Button startMatchButton;
    public Button cancelButton;
    
    [Header("Settings")]
    public string gameSceneName = "DuelScreen";
    public int requiredPlayers = 2;

    private NetworkManager _networkManager;
    private bool _isHost;

    private void Start()
    {
        _networkManager = InstanceFinder.NetworkManager;
        
        // Wire up buttons
        hostButton?.onClick.AddListener(OnHostClicked);
        joinButton?.onClick.AddListener(OnJoinClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);
        startMatchButton?.onClick.AddListener(OnStartMatchClicked);
        cancelButton?.onClick.AddListener(OnCancelClicked);
        
        // Subscribe to network events
        if (_networkManager != null)
        {
            _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
            _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        }
        
        ShowMainMenu();
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
        {
            _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            _networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        }
    }

    #region Button Handlers
    
    private void OnHostClicked()
    {
        Debug.Log("[MainMenu] Starting host...");
        _isHost = true;
        
        _networkManager.ServerManager.StartConnection();
        _networkManager.ClientManager.StartConnection();
        
        ShowLobby();
        UpdateStatus("Starting server...");
    }

    private void OnJoinClicked()
    {
        Debug.Log("[MainMenu] Joining game...");
        _isHost = false;
        
        _networkManager.ClientManager.StartConnection();
        
        ShowLobby();
        UpdateStatus("Connecting...");
    }

    private void OnQuitClicked()
    {
        Debug.Log("[MainMenu] Quitting...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private void OnStartMatchClicked()
    {
        if (!_isHost) return;
        
        Debug.Log("[MainMenu] Starting match!");
        LoadGameScene();
    }

    private void OnCancelClicked()
    {
        Debug.Log("[MainMenu] Cancelling...");
        
        if (_networkManager.ServerManager.Started)
            _networkManager.ServerManager.StopConnection(true);
        
        if (_networkManager.ClientManager.Started)
            _networkManager.ClientManager.StopConnection();
        
        ShowMainMenu();
    }
    
    #endregion

    #region Network Events
    
    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            Debug.Log("[MainMenu] Server started!");
            UpdateStatus("Waiting for opponent...");
            UpdatePlayerCount();
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            Debug.Log("[MainMenu] Server stopped");
        }
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            Debug.Log("[MainMenu] Connected to server!");
            if (!_isHost)
            {
                UpdateStatus("Connected! Waiting for host to start...");
            }
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            Debug.Log("[MainMenu] Disconnected from server");
            if (lobbyPanel.activeSelf)
            {
                UpdateStatus("Disconnected");
                ShowMainMenu();
            }
        }
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        // Only host cares about remote connections
        if (!_isHost) return;
        
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            Debug.Log($"[MainMenu] Player joined: {conn.ClientId}");
            UpdatePlayerCount();
            CheckCanStartMatch();
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            Debug.Log($"[MainMenu] Player left: {conn.ClientId}");
            UpdatePlayerCount();
            CheckCanStartMatch();
        }
    }
    
    #endregion

    #region UI Helpers
    
    private void ShowMainMenu()
    {
        mainMenuPanel?.SetActive(true);
        lobbyPanel?.SetActive(false);
    }

    private void ShowLobby()
    {
        mainMenuPanel?.SetActive(false);
        lobbyPanel?.SetActive(true);
        
        // Only host can start match
        if (startMatchButton != null)
            startMatchButton.gameObject.SetActive(_isHost);
    }

    private void UpdateStatus(string status)
    {
        if (statusText != null)
            statusText.text = status;
    }

    private void UpdatePlayerCount()
    {
        if (playerCountText == null) return;
        
        int count = _networkManager.ServerManager.Clients.Count;
        playerCountText.text = $"Players: {count}/{requiredPlayers}";
    }

    private void CheckCanStartMatch()
    {
        if (startMatchButton == null) return;
        
        int count = _networkManager.ServerManager.Clients.Count;
        startMatchButton.interactable = count >= requiredPlayers;
        
        if (count >= requiredPlayers)
        {
            UpdateStatus("Ready to start!");
        }
        else
        {
            UpdateStatus("Waiting for opponent...");
        }
    }
    
    #endregion

    #region Scene Loading
    
    private void LoadGameScene()
    {
        Debug.Log($"[MainMenu] Loading scene: {gameSceneName}");
        
        // Use FishNet's scene manager for networked scene loading
        SceneLoadData sld = new SceneLoadData(gameSceneName);
        sld.ReplaceScenes = ReplaceOption.All;
        
        _networkManager.SceneManager.LoadGlobalScenes(sld);
    }
    
    #endregion
}

using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Main menu controller - handles matchmaking and transitioning to game.
/// First player becomes host, second player joins automatically.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenuPanel;
    public GameObject lobbyPanel;
    
    [Header("Main Menu Buttons")]
    public Button joinButton;  // Single button for matchmaking
    public Button quitButton;
    
    [Header("Lobby UI")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI playerCountText;
    public Button cancelButton;
    
    [Header("Settings")]
    public string gameSceneName = "DuelScreen";
    public int requiredPlayers = 2;
    public float connectionTimeout = 3f;  // Seconds to wait before becoming host

    private NetworkManager _networkManager;
    private bool _isHost;
    private bool _isConnecting;
    private Coroutine _connectionAttempt;

    private void Start()
    {
        _networkManager = InstanceFinder.NetworkManager;
        
        // Wire up buttons
        joinButton?.onClick.AddListener(OnJoinClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);
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
    
    private void OnJoinClicked()
    {
        Debug.Log("[MainMenu] Finding match...");
        ShowLobby();
        UpdateStatus("Finding match...");
        
        // Try to join first, become host if no one is hosting
        _connectionAttempt = StartCoroutine(TryJoinOrHost());
    }

    private IEnumerator TryJoinOrHost()
    {
        _isConnecting = true;
        _isHost = false;
        
        // Try to connect as client first
        Debug.Log("[MainMenu] Attempting to join existing game...");
        _networkManager.ClientManager.StartConnection();
        
        // Wait for connection or timeout
        float elapsed = 0f;
        while (elapsed < connectionTimeout && _isConnecting)
        {
            // If we successfully connected as client, we're done
            if (_networkManager.ClientManager.Started && !_isHost)
            {
                Debug.Log("[MainMenu] Successfully joined existing game!");
                _isConnecting = false;
                yield break;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Connection timed out or failed - become host instead
        if (_isConnecting)
        {
            Debug.Log("[MainMenu] No existing game found, becoming host...");
            
            // Stop client attempt if still going
            if (_networkManager.ClientManager.Started)
                _networkManager.ClientManager.StopConnection();
            
            // Start as host (server + client)
            _isHost = true;
            _networkManager.ServerManager.StartConnection();
            _networkManager.ClientManager.StartConnection();
            
            _isConnecting = false;
        }
    }

    private void OnQuitClicked()
    {
        Debug.Log("[MainMenu] Quitting...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private void OnCancelClicked()
    {
        Debug.Log("[MainMenu] Cancelling...");
        
        _isConnecting = false;
        if (_connectionAttempt != null)
        {
            StopCoroutine(_connectionAttempt);
            _connectionAttempt = null;
        }
        
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
            Debug.Log("[MainMenu] Server started - hosting game!");
            UpdateStatus("Hosting game... Waiting for opponent...");
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
            _isConnecting = false;  // Successfully connected
            
            if (!_isHost)
            {
                UpdateStatus("Connected! Waiting for match to start...");
            }
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            Debug.Log("[MainMenu] Disconnected from server");
            
            // If we're still trying to connect, this is expected (failed join attempt)
            if (!_isConnecting && lobbyPanel.activeSelf)
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
        int count = _networkManager.ServerManager.Clients.Count;
        
        if (count >= requiredPlayers)
        {
            UpdateStatus("Match starting...");
            // Auto-start when enough players
            LoadGameScene();
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
        
        // Collect all spawned NetworkPlayer objects to move to new scene
        System.Collections.Generic.List<NetworkObject> movedObjects = new System.Collections.Generic.List<NetworkObject>();
        
        // Get all NetworkObjects owned by connected clients
        foreach (NetworkConnection conn in _networkManager.ServerManager.Clients.Values)
        {
            foreach (NetworkObject nob in conn.Objects)
            {
                movedObjects.Add(nob);
                Debug.Log($"[MainMenu] Moving NetworkObject: {nob.name} (Owner: {conn.ClientId})");
            }
        }
        
        Debug.Log($"[MainMenu] Moving {movedObjects.Count} NetworkObjects to {gameSceneName}");
        
        // Use FishNet's scene manager for networked scene loading
        SceneLoadData sld = new SceneLoadData(gameSceneName);
        sld.ReplaceScenes = ReplaceOption.All;
        
        // Keep spawned network objects (like NetworkPlayer) when changing scenes
        sld.Options.AllowStacking = false;
        sld.MovedNetworkObjects = movedObjects.ToArray();
        
        _networkManager.SceneManager.LoadGlobalScenes(sld);
    }
    
    #endregion
}

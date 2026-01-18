using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles player connections - spawns player objects when clients connect,
/// tracks all connected players, and handles cleanup on disconnect.
/// Supports reconnection during grace period.
/// </summary>
public class PlayerConnectionHandler : MonoBehaviour
{
    public static PlayerConnectionHandler Instance { get; private set; }
    
    [Header("Player Prefab")]
    [Tooltip("The NetworkPlayer prefab to spawn for each connected player")]
    public NetworkObject playerPrefab;

    [Header("Debug")]
    [SerializeField] private bool logConnections = true;

    /// <summary>
    /// All currently connected players, keyed by their connection ID.
    /// </summary>
    public Dictionary<int, NetworkPlayer> ConnectedPlayers { get; private set; } = new Dictionary<int, NetworkPlayer>();
    
    /// <summary>
    /// Saved state of disconnected players awaiting reconnection, keyed by their PlayerId.
    /// We save state and DESPAWN (not preserve) so reconnection spawns fresh.
    /// </summary>
    private Dictionary<int, DisconnectedPlayerState> _disconnectedPlayerStates = new Dictionary<int, DisconnectedPlayerState>();

    /// <summary>
    /// Number of currently connected players.
    /// </summary>
    public int PlayerCount => ConnectedPlayers.Count;

    private NetworkManager _networkManager;
    
    // Client-side reconnection state
    private bool _isWaitingForHostReconnect = false;
    private float _hostReconnectTimer = 0f;
    private float _hostReconnectGracePeriod = 120f; // 2 minutes
    private float _reconnectAttemptInterval = 3f; // Try every 3 seconds
    private float _nextReconnectAttempt = 0f;
    
    // Flag to track intentional disconnects vs unexpected disconnects
    // When true, we should NOT enter reconnect-wait mode
    private bool _isIntentionalDisconnect = false;
    private GameStateSnapshot _savedGameState;
    private string _lastServerAddress;
    private ushort _lastServerPort;
    
    // Direct file logging for debugging reconnection (independent of Unity logging)
    private static StreamWriter _reconnectLog;
    private static string _reconnectLogPath;
    
    private static void InitReconnectLog()
    {
        if (_reconnectLog != null) return;
        
        // Use Logs folder (not Docs) with .log extension - Unity ignores these for domain reload
        string dir = Application.isEditor 
            ? Path.Combine(Application.dataPath, "..", "Logs", "GameLogs")
            : Path.Combine(Application.dataPath, "..", "..", "Logs", "GameLogs");
        
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
            
        string filename = Application.isEditor ? "reconnect_editor.log" : "reconnect_build.log";
        _reconnectLogPath = Path.Combine(dir, filename);
        
        _reconnectLog = new StreamWriter(_reconnectLogPath, false);
        _reconnectLog.AutoFlush = true;
        _reconnectLog.WriteLine($"=== Reconnect log started: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
    }
    
    private static void LogReconnect(string message)
    {
        InitReconnectLog();
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
        _reconnectLog?.WriteLine($"[{timestamp}] {message}");
    }

    private void Awake()
    {
        LogReconnect($"Awake called on instance {GetInstanceID()}, gameObject={gameObject.name}, scene={gameObject.scene.name}");
        
        // Singleton pattern - destroy duplicate instances
        if (Instance != null && Instance != this)
        {
            Debug.Log($"[PlayerConnectionHandler] Duplicate instance {GetInstanceID()} found (original={Instance.GetInstanceID()}), destroying this one");
            LogReconnect($"Duplicate instance {GetInstanceID()} found (original={Instance.GetInstanceID()}), destroying this one");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        Debug.Log($"[PlayerConnectionHandler] Awake - initializing instance {GetInstanceID()}...");
        LogReconnect($"Instance {GetInstanceID()} set as singleton");
        
        // Persist across scene loads
        DontDestroyOnLoad(gameObject);
        LogReconnect($"DontDestroyOnLoad called for instance {GetInstanceID()}");
        
        _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null)
        {
            Debug.LogError("[PlayerConnectionHandler] NetworkManager not found!");
            return;
        }

        // Initialize the static reconnection manager
        ReconnectionManager.Initialize();

        // Subscribe to server events for remote client connections
        _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        Debug.Log("[PlayerConnectionHandler] Subscribed to ServerManager.OnRemoteConnectionState");
        
        // Subscribe to client events for detecting host disconnect
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
        Debug.Log("[PlayerConnectionHandler] Subscribed to ClientManager.OnClientConnectionState");
    }

    private void OnDestroy()
    {
        LogReconnect($"OnDestroy called on instance {GetInstanceID()}! _isWaitingForHostReconnect={_isWaitingForHostReconnect}, Instance={(Instance == this ? "this" : (Instance == null ? "null" : Instance.GetInstanceID().ToString()))}");
        
        // Don't clear Instance if we're not the singleton (we're being destroyed as a duplicate)
        if (Instance == this)
        {
            LogReconnect($"WARNING: Singleton instance {GetInstanceID()} is being destroyed!");
            Instance = null;
        }
        
        if (_networkManager != null)
        {
            _networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        }
        
        // Close reconnect log
        if (_reconnectLog != null)
        {
            _reconnectLog.WriteLine($"=== Reconnect log ended: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            _reconnectLog.Close();
            _reconnectLog = null;
        }
    }

    /// <summary>
    /// Called on the CLIENT when their connection state to the server changes.
    /// This detects when the host disconnects (client loses connection).
    /// </summary>
    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        // Check both local state and ReconnectionManager state
        bool isWaiting = _isWaitingForHostReconnect || ReconnectionManager.IsWaitingForReconnect;
        
        Debug.Log($"[PlayerConnectionHandler] OnClientConnectionState: {args.ConnectionState}, _isWaitingForHostReconnect={_isWaitingForHostReconnect}, ReconnectionManager.IsWaiting={ReconnectionManager.IsWaitingForReconnect}");
        LogReconnect($"OnClientConnectionState: {args.ConnectionState}, _isWaitingForHostReconnect={_isWaitingForHostReconnect}, ReconnectionManager.IsWaiting={ReconnectionManager.IsWaitingForReconnect}");
        
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            // Successfully connected/reconnected
            if (isWaiting)
            {
                Debug.Log("[PlayerConnectionHandler] Connection started while waiting for reconnect - triggering OnReconnectedToHost!");
                LogReconnect("Connection started while waiting for reconnect - triggering OnReconnectedToHost!");
                OnReconnectedToHost();
            }
            else
            {
                Debug.Log("[PlayerConnectionHandler] Connection started (normal connect, not reconnect)");
                LogReconnect("Connection started (normal connect, not reconnect)");
                
                // Clear intentional disconnect flag on successful connection
                _isIntentionalDisconnect = false;
                
                // Clear any stale reconnection state - this is a fresh connect, not a reconnect
                _savedGameState = null;
                ReconnectionManager.ClearSavedState();
            }
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            // We lost connection to the server
            Debug.Log("[PlayerConnectionHandler] Lost connection!");
            LogReconnect($"Lost connection! IsServerStarted={_networkManager.IsServerStarted}, _isWaitingForHostReconnect={_isWaitingForHostReconnect}, _isIntentionalDisconnect={_isIntentionalDisconnect}");
            
            // If we're not the server (i.e., we're the client that lost connection)
            // and we're not already in a reconnection wait
            // and this wasn't an intentional disconnect (client chose to leave)
            if (!_networkManager.IsServerStarted && !isWaiting && !_isIntentionalDisconnect)
            {
                OnHostDisconnected();
            }
            else
            {
                if (_isIntentionalDisconnect)
                {
                    LogReconnect("Skipping OnHostDisconnected because this was an intentional disconnect (client chose to leave)");
                    // Reset the flag
                    _isIntentionalDisconnect = false;
                }
                else
                {
                    LogReconnect($"Skipping OnHostDisconnected because IsServerStarted={_networkManager.IsServerStarted} or already waiting");
                }
            }
        }
    }
    
    /// <summary>
    /// Call this before disconnecting intentionally (e.g., returning to main menu).
    /// This prevents the client from entering reconnect-wait mode.
    /// </summary>
    public void SetIntentionalDisconnect()
    {
        _isIntentionalDisconnect = true;
        Debug.Log("[PlayerConnectionHandler] Intentional disconnect flagged");
        LogReconnect("SetIntentionalDisconnect called - will not enter reconnect mode on disconnect");
    }

    /// <summary>
    /// Called on clients when the host disconnects.
    /// </summary>
    private void OnHostDisconnected()
    {
        Debug.Log("[PlayerConnectionHandler] Host disconnected! Starting reconnection wait...");
        LogReconnect("OnHostDisconnected called - starting reconnection wait");
        
        // Save current game state
        EncounterController encounterController = FindObjectOfType<EncounterController>();
        GameStateSnapshot savedState = null;
        if (encounterController != null && NetworkGameManager.Instance != null)
        {
            savedState = GameStateSnapshot.CaptureState(encounterController, NetworkGameManager.Instance);
            _savedGameState = savedState;
            Debug.Log($"[PlayerConnectionHandler] Game state saved: {_savedGameState != null}");
            LogReconnect($"Game state saved: {_savedGameState != null}");
        }
        
        // Save server connection info for reconnection
        var transport = _networkManager.TransportManager.Transport;
        if (transport != null)
        {
            _lastServerAddress = transport.GetClientAddress();
            _lastServerPort = transport.GetPort();
            Debug.Log($"[PlayerConnectionHandler] Saved connection info: {_lastServerAddress}:{_lastServerPort}");
            LogReconnect($"Saved connection info: {_lastServerAddress}:{_lastServerPort}");
        }
        
        // Start waiting for reconnect using BOTH local state AND static manager
        _isWaitingForHostReconnect = true;
        _hostReconnectTimer = 0f;
        _nextReconnectAttempt = _reconnectAttemptInterval;
        LogReconnect($"Set _isWaitingForHostReconnect = true, timer = 0, nextAttempt = {_reconnectAttemptInterval}");
        
        // Also start the static reconnection manager as a fallback
        ReconnectionManager.StartReconnectionWait(_networkManager, _lastServerAddress, _lastServerPort, savedState);
        
        // Show waiting UI
        if (encounterController != null)
        {
            encounterController.OnHostDisconnected(_hostReconnectGracePeriod);
        }
    }
    
    private void Update()
    {
        // Handle client-side reconnection attempts
        if (_isWaitingForHostReconnect)
        {
            _hostReconnectTimer += Time.deltaTime;
            
            // Update UI
            float remainingTime = _hostReconnectGracePeriod - _hostReconnectTimer;
            EncounterController encounterController = FindObjectOfType<EncounterController>();
            if (encounterController != null)
            {
                encounterController.UpdateHostDisconnectTimer(remainingTime);
            }
            
            // Try to reconnect periodically
            _nextReconnectAttempt -= Time.deltaTime;
            if (_nextReconnectAttempt <= 0f)
            {
                _nextReconnectAttempt = _reconnectAttemptInterval;
                Debug.Log($"[PlayerConnectionHandler] Reconnect timer expired, attempting reconnect... (timer={_hostReconnectTimer:F1}s)");
                LogReconnect($"Reconnect timer expired, attempting reconnect (timer={_hostReconnectTimer:F1}s)");
                AttemptReconnect();
            }
            
            // Grace period expired
            if (_hostReconnectTimer >= _hostReconnectGracePeriod)
            {
                Debug.Log("[PlayerConnectionHandler] Host reconnect grace period expired!");
                _isWaitingForHostReconnect = false;
                _savedGameState = null;
                
                if (encounterController != null)
                {
                    encounterController.OnHostForfeited();
                }
                
                // Return to main menu
                StartCoroutine(ReturnToMainMenuDelayed(3f));
            }
        }
    }
    
    /// <summary>
    /// Attempt to reconnect to the host.
    /// </summary>
    private void AttemptReconnect()
    {
        if (string.IsNullOrEmpty(_lastServerAddress))
        {
            Debug.Log("[PlayerConnectionHandler] No server address saved, cannot reconnect");
            LogReconnect("AttemptReconnect: No server address saved, cannot reconnect");
            return;
        }
        
        Debug.Log($"[PlayerConnectionHandler] Attempting to reconnect to {_lastServerAddress}:{_lastServerPort}...");
        LogReconnect($"AttemptReconnect: Calling StartConnection to {_lastServerAddress}:{_lastServerPort}");
        
        // Try to connect
        _networkManager.ClientManager.StartConnection(_lastServerAddress, _lastServerPort);
    }
    
    /// <summary>
    /// Called when client successfully reconnects to host.
    /// </summary>
    public void OnReconnectedToHost()
    {
        // Check both local state and ReconnectionManager state
        bool wasWaiting = _isWaitingForHostReconnect || ReconnectionManager.IsWaitingForReconnect;
        GameStateSnapshot savedState = _savedGameState ?? ReconnectionManager.SavedState;
        
        Debug.Log($"[PlayerConnectionHandler] OnReconnectedToHost called! _isWaitingForHostReconnect={_isWaitingForHostReconnect}, ReconnectionManager.IsWaiting={ReconnectionManager.IsWaitingForReconnect}, savedState={(savedState != null ? "exists" : "null")}");
        LogReconnect($"OnReconnectedToHost called! wasWaiting={wasWaiting}, savedState={(savedState != null ? "exists" : "null")}");
        
        if (!wasWaiting) 
        {
            Debug.Log("[PlayerConnectionHandler] Not waiting for reconnect, ignoring");
            LogReconnect("OnReconnectedToHost: Not waiting for reconnect, ignoring");
            return;
        }
        
        Debug.Log("[PlayerConnectionHandler] Reconnected to host! Processing...");
        LogReconnect("Reconnected to host! Processing...");
        _isWaitingForHostReconnect = false;
        ReconnectionManager.OnReconnected();
        
        // Hide disconnect UI
        EncounterController encounterController = FindObjectOfType<EncounterController>();
        if (encounterController != null)
        {
            encounterController.OnHostReconnected();
        }
        
        // Restore game state locally AND send to host
        if (savedState != null)
        {
            Debug.Log("[PlayerConnectionHandler] Starting game state restoration coroutine...");
            StartCoroutine(RestoreAndSendGameState(encounterController, savedState));
        }
        else
        {
            Debug.LogWarning("[PlayerConnectionHandler] No saved game state to restore!");
        }
    }
    
    /// <summary>
    /// Restore game state locally and send to host for sync.
    /// This should only be used for TRUE host reconnection scenarios (host disconnected and came back).
    /// </summary>
    private IEnumerator RestoreAndSendGameState(EncounterController encounterController, GameStateSnapshot savedState)
    {
        // Wait for NetworkPlayer to be ready
        yield return new WaitForSeconds(0.5f);
        
        // CRITICAL CHECK: Only send state to host if this is a true host reconnection scenario.
        // If the server already has valid game state (encounterController.turnNumber > 0 or similar),
        // then the host didn't actually lose state and we should NOT overwrite it.
        
        // Restore locally first - client needs to see the game state (this is always safe)
        if (savedState != null && NetworkGameManager.Instance != null)
        {
            Debug.Log("[PlayerConnectionHandler] Restoring game state locally after host reconnect...");
            NetworkGameManager.Instance.RestoreGameStateLocally(savedState);
        }
        
        // Send to host ONLY if we believe the host truly lost state
        // The host will also validate and skip overwriting if it has valid state
        var localPlayer = NetworkGameManager.Instance?.GetLocalPlayer();
        if (localPlayer != null && savedState != null)
        {
            Debug.Log($"[PlayerConnectionHandler] Sending saved game state to host (TurnObjectId={savedState.currentTurnObjectId})...");
            Debug.Log("[PlayerConnectionHandler] Note: Host will validate and skip if it has valid state already.");
            string stateJson = savedState.ToJson();
            localPlayer.ServerRestoreGameState(stateJson);
        }
        
        // Clear the saved states
        _savedGameState = null;
        ReconnectionManager.ClearSavedState();
    }
    
    private IEnumerator ReturnToMainMenuDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Called on the server when a remote client's connection state changes.
    /// </summary>
    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        Debug.Log($"[PlayerConnectionHandler] OnRemoteConnectionState: ClientId={conn.ClientId}, State={args.ConnectionState}");
        
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            OnPlayerConnected(conn);
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            OnPlayerDisconnected(conn);
        }
    }

    /// <summary>
    /// Called when a new player connects to the server.
    /// </summary>
    private void OnPlayerConnected(NetworkConnection conn)
    {
        if (logConnections)
            Debug.Log($"[PlayerConnectionHandler] Player connected: Connection ID {conn.ClientId}");

        // Check if this is a reconnecting player
        // For now, we use a simple approach: if there's a disconnected player waiting, reconnect them
        int reconnectingPlayerId = TryGetReconnectingPlayerId();
        
        if (reconnectingPlayerId >= 0 && _disconnectedPlayerStates.TryGetValue(reconnectingPlayerId, out var savedState))
        {
            // Reconnection! Spawn fresh and restore state
            HandleReconnection(conn, reconnectingPlayerId, savedState);
            return;
        }

        // New player - spawn player object for this connection
        if (playerPrefab != null)
        {
            NetworkObject nob = _networkManager.GetPooledInstantiated(playerPrefab, true);
            
            // Get the NetworkPlayer component and set the player ID
            // The actual initialization will happen in OnStartServer after spawn
            NetworkPlayer networkPlayer = nob.GetComponent<NetworkPlayer>();
            if (networkPlayer != null)
            {
                networkPlayer.SetPlayerId(conn.ClientId);
                ConnectedPlayers[conn.ClientId] = networkPlayer;
            }
            
            // Spawn - OnStartServer will initialize the SyncVars
            _networkManager.ServerManager.Spawn(nob, conn);

            if (logConnections)
                Debug.Log($"[PlayerConnectionHandler] Spawned player object for Connection ID {conn.ClientId}. Total players: {PlayerCount}");
        }
        else
        {
            Debug.LogWarning("[PlayerConnectionHandler] No player prefab assigned!");
        }
    }
    
    /// <summary>
    /// Try to find saved state for a disconnected player waiting for reconnection.
    /// Returns the playerId if found, -1 otherwise.
    /// </summary>
    private int TryGetReconnectingPlayerId()
    {
        // Return the first disconnected player's ID (simple 1v1 game)
        foreach (var kvp in _disconnectedPlayerStates)
        {
            return kvp.Key;
        }
        return -1;
    }
    
    /// <summary>
    /// Handle a player reconnecting to their existing session.
    /// DESPAWN/RESPAWN PATTERN: Spawn a FRESH NetworkPlayer and restore saved state.
    /// </summary>
    private void HandleReconnection(NetworkConnection conn, int playerId, DisconnectedPlayerState savedState)
    {
        Debug.Log($"[PlayerConnectionHandler] Player {playerId} reconnecting with Connection ID {conn.ClientId}");
        
        // Remove from disconnected states
        _disconnectedPlayerStates.Remove(playerId);
        
        // Spawn a FRESH NetworkPlayer for the reconnecting client
        if (playerPrefab != null)
        {
            NetworkObject nob = _networkManager.GetPooledInstantiated(playerPrefab, true);
            NetworkPlayer newPlayer = nob.GetComponent<NetworkPlayer>();
            
            if (newPlayer != null)
            {
                // Set pending state BEFORE spawn - OnStartServer will apply it
                newPlayer.SetPendingState(savedState);
                
                // Track in connected players
                ConnectedPlayers[conn.ClientId] = newPlayer;
                
                // Spawn with correct ownership - OnStartServer will restore state
                _networkManager.ServerManager.Spawn(nob, conn);
                
                Debug.Log($"[PlayerConnectionHandler] Player {playerId} reconnected! Fresh NetworkPlayer spawned with connection {conn.ClientId}.");
                
                // Notify NetworkGameManager with the new player and saved state
                if (NetworkGameManager.Instance != null)
                {
                    NetworkGameManager.Instance.ServerOnPlayerReconnected(playerId, conn, newPlayer, savedState);
                }
            }
            else
            {
                Debug.LogError("[PlayerConnectionHandler] Failed to get NetworkPlayer component from prefab!");
            }
        }
        else
        {
            Debug.LogError("[PlayerConnectionHandler] Cannot respawn - playerPrefab is null!");
        }
    }

    /// <summary>
    /// Called when a player disconnects from the server.
    /// Uses DESPAWN/RESPAWN pattern: Save state, despawn, respawn fresh on reconnect.
    /// </summary>
    private void OnPlayerDisconnected(NetworkConnection conn)
    {
        if (logConnections)
            Debug.Log($"[PlayerConnectionHandler] Player disconnected: Connection ID {conn.ClientId}");

        // Get the player before removing from tracking
        NetworkPlayer disconnectedPlayer = null;
        int disconnectedPlayerId = -1;
        
        if (ConnectedPlayers.TryGetValue(conn.ClientId, out disconnectedPlayer))
        {
            disconnectedPlayerId = disconnectedPlayer.PlayerId.Value;
            
            // DESPAWN/RESPAWN PATTERN: Save state BEFORE despawning
            var savedState = DisconnectedPlayerState.Capture(disconnectedPlayer);
            if (savedState != null)
            {
                _disconnectedPlayerStates[disconnectedPlayerId] = savedState;
                Debug.Log($"[PlayerConnectionHandler] Player {disconnectedPlayerId} state saved for reconnection");
            }
            
            // DESPAWN the NetworkPlayer - we'll spawn fresh on reconnect
            // This is cleaner than trying to transfer ownership
            if (disconnectedPlayer.IsSpawned)
            {
                Debug.Log($"[PlayerConnectionHandler] Despawning NetworkPlayer for player {disconnectedPlayerId}");
                _networkManager.ServerManager.Despawn(disconnectedPlayer.NetworkObject);
            }
        }

        // Remove from active connections
        if (ConnectedPlayers.ContainsKey(conn.ClientId))
        {
            ConnectedPlayers.Remove(conn.ClientId);
        }

        if (logConnections)
            Debug.Log($"[PlayerConnectionHandler] Remaining connected players: {PlayerCount}, Disconnected awaiting reconnect: {_disconnectedPlayerStates.Count}");

        // Notify NetworkGameManager about the disconnect (server-side)
        if (disconnectedPlayerId >= 0 && NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.ServerOnPlayerDisconnected(disconnectedPlayerId);
        }
    }
    
    /// <summary>
    /// Called when grace period expires - permanently remove the disconnected player.
    /// </summary>
    public void ForfeitDisconnectedPlayer(int playerId)
    {
        if (_disconnectedPlayerStates.ContainsKey(playerId))
        {
            Debug.Log($"[PlayerConnectionHandler] Removing forfeited player {playerId}");
            _disconnectedPlayerStates.Remove(playerId);
            // NetworkPlayer was already despawned on disconnect, nothing else to do
        }
    }
    
    /// <summary>
    /// Check if a player is disconnected and awaiting reconnection.
    /// </summary>
    public bool IsPlayerDisconnected(int playerId)
    {
        return _disconnectedPlayerStates.ContainsKey(playerId);
    }

    /// <summary>
    /// Get a connected player by their connection ID.
    /// </summary>
    public NetworkPlayer GetPlayer(int connectionId)
    {
        ConnectedPlayers.TryGetValue(connectionId, out NetworkPlayer player);
        return player;
    }
}

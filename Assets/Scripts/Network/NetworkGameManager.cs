using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Manages the networked game state in the DuelScreen scene.
/// Links spawned NetworkPlayer objects to local PlayerController objects.
/// Also handles turn synchronization and disconnect handling.
/// </summary>
public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

    [Header("Scene References")]
    [Tooltip("The local player's PlayerController in the scene")]
    public PlayerController localPlayerController;
    
    [Tooltip("The opponent's PlayerController in the scene")]
    public PlayerController opponentPlayerController;
    
    [Tooltip("Reference to the EncounterController")]
    public EncounterController encounterController;

    [Header("Turn State (Synced)")]
    /// <summary>
    /// The ObjectId of the NetworkPlayer whose turn it currently is. -1 means game not started.
    /// </summary>
    public readonly SyncVar<int> CurrentTurnObjectId = new SyncVar<int>(-1);
    
    /// <summary>
    /// The current turn number (starts at 1).
    /// </summary>
    public readonly SyncVar<int> TurnNumber = new SyncVar<int>(0);
    
    /// <summary>
    /// Whether the game has started.
    /// </summary>
    public readonly SyncVar<bool> GameStarted = new SyncVar<bool>(false);
    
    /// <summary>
    /// Random seed for deterministic deck shuffling. Set by server, synced to clients.
    /// </summary>
    public readonly SyncVar<int> ShuffleSeed = new SyncVar<int>(0);
    
    /// <summary>
    /// Whether an opponent has disconnected (synced to all clients).
    /// </summary>
    public readonly SyncVar<bool> OpponentDisconnected = new SyncVar<bool>(false);

    [Header("Disconnect Settings")]
    [Tooltip("Seconds to wait for reconnection before declaring victory")]
    public float reconnectGracePeriod = 30f;
    
    [Header("Runtime State")]
    private NetworkPlayer localNetworkPlayer;
    private NetworkPlayer opponentNetworkPlayer;
    
    private Dictionary<int, NetworkPlayer> networkPlayers = new Dictionary<int, NetworkPlayer>();
    
    // Disconnect handling
    private float disconnectTimer = 0f;
    private bool isWaitingForReconnect = false;
    private int disconnectedPlayerId = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Subscribe to SyncVar changes
        CurrentTurnObjectId.OnChange += OnTurnChanged;
        TurnNumber.OnChange += OnTurnNumberChanged;
        GameStarted.OnChange += OnGameStartedChanged;
        OpponentDisconnected.OnChange += OnOpponentDisconnectedChanged;
    }
    
    private void OnDestroy()
    {
        CurrentTurnObjectId.OnChange -= OnTurnChanged;
        TurnNumber.OnChange -= OnTurnNumberChanged;
        GameStarted.OnChange -= OnGameStartedChanged;
        OpponentDisconnected.OnChange -= OnOpponentDisconnectedChanged;
    }
    
    private void Update()
    {
        // Handle reconnect grace period timer
        if (isWaitingForReconnect && !OpponentDisconnected.Value)
        {
            // Opponent reconnected!
            isWaitingForReconnect = false;
            disconnectTimer = 0f;
            Debug.Log("[NetworkGameManager] Opponent reconnected!");
            
            if (encounterController != null)
            {
                encounterController.OnOpponentReconnected();
            }
        }
        else if (isWaitingForReconnect)
        {
            disconnectTimer += Time.deltaTime;
            
            // Update UI with remaining time
            float remainingTime = reconnectGracePeriod - disconnectTimer;
            if (encounterController != null)
            {
                encounterController.UpdateDisconnectTimer(remainingTime);
            }
            
            // Grace period expired
            if (disconnectTimer >= reconnectGracePeriod)
            {
                Debug.Log("[NetworkGameManager] Reconnect grace period expired - opponent forfeits!");
                isWaitingForReconnect = false;
                
                if (encounterController != null)
                {
                    encounterController.OnOpponentForfeited();
                }
                
                // Clean up the disconnected player (server only)
                if (IsServerInitialized && disconnectedPlayerId >= 0)
                {
                    var connectionHandler = FindObjectOfType<PlayerConnectionHandler>();
                    if (connectionHandler != null)
                    {
                        connectionHandler.ForfeitDisconnectedPlayer(disconnectedPlayerId);
                    }
                }
                
                // Return to main menu after a short delay
                StartCoroutine(ReturnToMainMenuDelayed(3f));
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("[NetworkGameManager] Client started, looking for NetworkPlayers...");
        
        // Find all NetworkPlayer objects that have been spawned
        StartCoroutine(FindAndLinkPlayers());
    }

    private System.Collections.IEnumerator FindAndLinkPlayers()
    {
        // Wait for network objects to be synchronized (may take a few frames)
        int maxAttempts = 30; // Try for up to 3 seconds
        int attempts = 0;
        
        while (attempts < maxAttempts)
        {
            yield return new WaitForSeconds(0.1f);
            attempts++;
            
            // Find all NetworkPlayer objects
            NetworkPlayer[] players = FindObjectsOfType<NetworkPlayer>();
            
            if (players.Length >= 2)
            {
                Debug.Log($"[NetworkGameManager] Found {players.Length} NetworkPlayer(s) after {attempts} attempts");
                
                foreach (var player in players)
                {
                    RegisterNetworkPlayer(player);
                }
                
                // Link to local PlayerControllers
                LinkPlayersToControllers();
                yield break;
            }
            
            if (players.Length > 0 && attempts % 10 == 0)
            {
                Debug.Log($"[NetworkGameManager] Found {players.Length} NetworkPlayer(s) so far, waiting for more...");
            }
        }
        
        // If we get here, we didn't find enough players
        NetworkPlayer[] finalPlayers = FindObjectsOfType<NetworkPlayer>();
        Debug.LogWarning($"[NetworkGameManager] Timeout! Only found {finalPlayers.Length} NetworkPlayer(s) after {maxAttempts} attempts");
        
        // Link what we have
        foreach (var player in finalPlayers)
        {
            RegisterNetworkPlayer(player);
        }
        LinkPlayersToControllers();
    }

    /// <summary>
    /// Register a NetworkPlayer (called when one is found or spawned).
    /// </summary>
    public void RegisterNetworkPlayer(NetworkPlayer player)
    {
        // Use ObjectId as stable key since PlayerId SyncVar may not be synced yet on client
        int objectId = player.ObjectId;
        
        if (!networkPlayers.ContainsKey(objectId))
        {
            networkPlayers[objectId] = player;
            Debug.Log($"[NetworkGameManager] Registered NetworkPlayer: {player.PlayerName.Value} (ObjectId: {objectId}, PlayerId: {player.PlayerId.Value}, IsOwner: {player.IsOwner})");
        }
        
        // Check if this is our local player - only set if not already set
        if (player.IsOwner)
        {
            if (localNetworkPlayer == null || localNetworkPlayer == player)
            {
                localNetworkPlayer = player;
                Debug.Log($"[NetworkGameManager] Local player identified: {player.PlayerName.Value} (ObjectId: {objectId})");
            }
            else
            {
                Debug.LogWarning($"[NetworkGameManager] Local player already set to {localNetworkPlayer.PlayerName.Value}, ignoring {player.PlayerName.Value}");
            }
        }
        else
        {
            if (opponentNetworkPlayer == null || opponentNetworkPlayer == player)
            {
                opponentNetworkPlayer = player;
                Debug.Log($"[NetworkGameManager] Opponent identified: {player.PlayerName.Value} (ObjectId: {objectId})");
            }
            else
            {
                Debug.LogWarning($"[NetworkGameManager] Opponent already set to {opponentNetworkPlayer.PlayerName.Value}, ignoring {player.PlayerName.Value}");
            }
        }
        
        // Try to link after each registration
        LinkPlayersToControllers();
    }

    private void LinkPlayersToControllers()
    {
        // Link local player
        if (localNetworkPlayer != null && localPlayerController != null)
        {
            // Always re-link (needed for reconnection scenarios)
            if (localPlayerController.networkPlayer != localNetworkPlayer)
            {
                localNetworkPlayer.LinkToPlayerController(localPlayerController);
                localPlayerController.networkPlayer = localNetworkPlayer;
                Debug.Log("[NetworkGameManager] Linked local NetworkPlayer to PlayerController");
            }
        }
        
        // Link opponent
        if (opponentNetworkPlayer != null && opponentPlayerController != null)
        {
            // Always re-link (needed for reconnection scenarios)
            if (opponentPlayerController.networkPlayer != opponentNetworkPlayer)
            {
                opponentNetworkPlayer.LinkToPlayerController(opponentPlayerController);
                opponentPlayerController.networkPlayer = opponentNetworkPlayer;
                Debug.Log("[NetworkGameManager] Linked opponent NetworkPlayer to OpponentController");
            }
        }
        
        // Auto-find EncounterController if not set
        if (encounterController == null)
        {
            encounterController = FindObjectOfType<EncounterController>();
        }
        
        // Check if we should start the game (server only)
        if (IsServerInitialized && AreBothPlayersReady() && !GameStarted.Value)
        {
            Debug.Log("[NetworkGameManager] Both players ready, starting game...");
            ServerStartGame();
        }
        
        // Try to initialize encounter (for client that joined after game started)
        TryInitializeEncounter();
    }

    /// <summary>
    /// Get the NetworkPlayer for a given player ID.
    /// </summary>
    public NetworkPlayer GetNetworkPlayer(int playerId)
    {
        networkPlayers.TryGetValue(playerId, out NetworkPlayer player);
        return player;
    }

    /// <summary>
    /// Get the local player's NetworkPlayer.
    /// </summary>
    public NetworkPlayer GetLocalPlayer() => localNetworkPlayer;

    /// <summary>
    /// Get the opponent's NetworkPlayer.
    /// </summary>
    public NetworkPlayer GetOpponentPlayer() => opponentNetworkPlayer;

    /// <summary>
    /// Check if both players are connected and linked.
    /// </summary>
    public bool AreBothPlayersReady()
    {
        return localNetworkPlayer != null && 
               opponentNetworkPlayer != null &&
               localPlayerController != null &&
               opponentPlayerController != null;
    }

    #region Turn Management
    
    /// <summary>
    /// Check if it's the local player's turn.
    /// </summary>
    public bool IsLocalPlayerTurn()
    {
        if (localNetworkPlayer == null)
        {
            Debug.LogWarning("[NetworkGameManager] IsLocalPlayerTurn called but localNetworkPlayer is NULL! Trying to find it...");
            TryFindLocalPlayer();
            
            if (localNetworkPlayer == null)
            {
                Debug.LogError("[NetworkGameManager] Could not find localNetworkPlayer!");
                return false;
            }
        }
        
        // Simply check if the current turn ObjectId matches local player's ObjectId
        bool isLocal = CurrentTurnObjectId.Value == localNetworkPlayer.ObjectId;
        Debug.Log($"[NetworkGameManager] IsLocalPlayerTurn: CurrentTurnObjectId={CurrentTurnObjectId.Value}, localObjectId={localNetworkPlayer.ObjectId}, result={isLocal}");
        return isLocal;
    }
    
    /// <summary>
    /// Try to find and set the local player if not already set.
    /// </summary>
    private void TryFindLocalPlayer()
    {
        NetworkPlayer[] players = FindObjectsOfType<NetworkPlayer>();
        foreach (var player in players)
        {
            if (player.IsOwner)
            {
                localNetworkPlayer = player;
                Debug.Log($"[NetworkGameManager] Found local player: {player.PlayerName.Value} (ObjectId: {player.ObjectId})");
                return;
            }
        }
    }
    
    /// <summary>
    /// Request to end the current turn (client calls this).
    /// </summary>
    public void RequestEndTurn()
    {
        Debug.Log($"[NetworkGameManager] RequestEndTurn called. localNetworkPlayer={(localNetworkPlayer != null ? localNetworkPlayer.ObjectId.ToString() : "NULL")}, CurrentTurnObjectId={CurrentTurnObjectId.Value}");
        
        if (!IsLocalPlayerTurn())
        {
            Debug.LogWarning($"[NetworkGameManager] Cannot end turn - not your turn! localPlayer={(localNetworkPlayer != null ? localNetworkPlayer.ObjectId.ToString() : "NULL")}, currentTurn={CurrentTurnObjectId.Value}");
            return;
        }
        
        Debug.Log("[NetworkGameManager] Requesting end turn...");
        CmdEndTurn();
    }
    
    /// <summary>
    /// Server RPC to end the current turn.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void CmdEndTurn(NetworkConnection conn = null)
    {
        if (!IsServerInitialized) return;
        
        // Verify it's actually this player's turn
        NetworkPlayer requestingPlayer = null;
        foreach (var player in networkPlayers.Values)
        {
            if (player.Owner == conn)
            {
                requestingPlayer = player;
                break;
            }
        }
        
        if (requestingPlayer == null || requestingPlayer.ObjectId != CurrentTurnObjectId.Value)
        {
            Debug.LogWarning($"[NetworkGameManager] Player tried to end turn but it's not their turn!");
            return;
        }
        
        // Switch to next player
        int nextObjectId = GetNextPlayerObjectId();
        TurnNumber.Value++;
        CurrentTurnObjectId.Value = nextObjectId;
        
        Debug.Log($"[Server] Turn ended. Now ObjectId {nextObjectId}'s turn. Turn #{TurnNumber.Value}");
    }
    
    /// <summary>
    /// Server: Start the game and set first turn.
    /// </summary>
    [Server]
    public void ServerStartGame()
    {
        if (GameStarted.Value) return;
        
        // Generate a random seed for deterministic shuffling on all clients
        ShuffleSeed.Value = UnityEngine.Random.Range(1, int.MaxValue);
        Debug.Log($"[Server] Generated shuffle seed: {ShuffleSeed.Value}");
        
        // Find the player with lowest PlayerId (first connected player, i.e. the host) to go first
        int firstObjectId = -1;
        int lowestPlayerId = int.MaxValue;
        
        foreach (var kvp in networkPlayers)
        {
            NetworkPlayer player = kvp.Value;
            if (player.PlayerId.Value < lowestPlayerId)
            {
                lowestPlayerId = player.PlayerId.Value;
                firstObjectId = kvp.Key;
            }
        }
        
        if (firstObjectId == -1)
        {
            Debug.LogError("[Server] No players registered - cannot start game!");
            return;
        }
        
        CurrentTurnObjectId.Value = firstObjectId;
        TurnNumber.Value = 1;
        GameStarted.Value = true;
        
        Debug.Log($"[Server] Game started! PlayerId {lowestPlayerId} (ObjectId {firstObjectId}) goes first.");
    }
    
    private int GetNextPlayerObjectId()
    {
        // Simple 2-player toggle - find the OTHER player in the dictionary
        foreach (var kvp in networkPlayers)
        {
            if (kvp.Key != CurrentTurnObjectId.Value)
            {
                return kvp.Key;
            }
        }
        
        // Fallback - shouldn't happen
        Debug.LogWarning("[Server] Could not find next player ObjectId!");
        return CurrentTurnObjectId.Value;
    }
    
    /// <summary>
    /// Called when turn changes (SyncVar callback).
    /// </summary>
    private void OnTurnChanged(int oldValue, int newValue, bool asServer)
    {
        // On host, this fires twice (server + client). Only process the client callback.
        if (asServer && IsClientInitialized)
        {
            return; // Skip server callback on host - client callback will handle it
        }
        
        Debug.Log($"[NetworkGameManager] Turn changed: ObjectId {oldValue} -> ObjectId {newValue}");
        
        // Notify the EncounterController about turn change
        if (encounterController != null && GameStarted.Value)
        {
            bool isLocalTurn = IsLocalPlayerTurn();
            encounterController.OnNetworkTurnChanged(isLocalTurn, TurnNumber.Value);
        }
    }
    
    private void OnTurnNumberChanged(int oldValue, int newValue, bool asServer)
    {
        Debug.Log($"[NetworkGameManager] Turn number: {oldValue} -> {newValue}");
    }
    
    private void OnGameStartedChanged(bool oldValue, bool newValue, bool asServer)
    {
        Debug.Log($"[NetworkGameManager] Game started: {newValue}");
        
        if (newValue)
        {
            TryInitializeEncounter();
        }
    }
    
    /// <summary>
    /// Try to initialize the encounter. Called when game starts or when players are linked.
    /// </summary>
    private bool _encounterInitialized = false;
    private void TryInitializeEncounter()
    {
        if (_encounterInitialized) return;
        if (!GameStarted.Value) return;
        
        // Try to find encounterController if not set
        if (encounterController == null)
        {
            encounterController = FindObjectOfType<EncounterController>();
        }
        
        if (encounterController != null && localNetworkPlayer != null)
        {
            bool isLocalTurn = IsLocalPlayerTurn();
            Debug.Log($"[NetworkGameManager] Initializing encounter with seed {ShuffleSeed.Value}");
            encounterController.OnNetworkGameStarted(isLocalTurn, ShuffleSeed.Value);
            _encounterInitialized = true;
        }
        else
        {
            Debug.Log($"[NetworkGameManager] Cannot init encounter yet - encounterController={(encounterController != null)}, localPlayer={(localNetworkPlayer != null)}");
        }
    }
    
    /// <summary>
    /// Called when OpponentDisconnected SyncVar changes.
    /// </summary>
    private void OnOpponentDisconnectedChanged(bool oldValue, bool newValue, bool asServer)
    {
        Debug.Log($"[NetworkGameManager] OpponentDisconnected changed: {oldValue} -> {newValue}");
        
        if (newValue && !oldValue)
        {
            // Opponent just disconnected
            isWaitingForReconnect = true;
            disconnectTimer = 0f;
            
            if (encounterController != null)
            {
                encounterController.OnOpponentDisconnected(reconnectGracePeriod);
            }
        }
    }
    
    /// <summary>
    /// Server: Called when a player disconnects during the game.
    /// </summary>
    [Server]
    public void ServerOnPlayerDisconnected(int playerId)
    {
        if (!GameStarted.Value) return;
        
        Debug.Log($"[Server] Player {playerId} disconnected during game!");
        disconnectedPlayerId = playerId;
        OpponentDisconnected.Value = true;
    }
    
    /// <summary>
    /// Server: Called when a player reconnects.
    /// </summary>
    [Server]
    public void ServerOnPlayerReconnected(int playerId)
    {
        if (disconnectedPlayerId == playerId)
        {
            Debug.Log($"[Server] Player {playerId} reconnected!");
            OpponentDisconnected.Value = false;
            disconnectedPlayerId = -1;
        }
    }
    
    /// <summary>
    /// Return to main menu after a delay.
    /// </summary>
    private System.Collections.IEnumerator ReturnToMainMenuDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Stop networking
        if (InstanceFinder.NetworkManager != null)
        {
            if (InstanceFinder.NetworkManager.IsServerStarted)
                InstanceFinder.NetworkManager.ServerManager.StopConnection(true);
            if (InstanceFinder.NetworkManager.IsClientStarted)
                InstanceFinder.NetworkManager.ClientManager.StopConnection();
        }
        
        // Load main menu (use full namespace to avoid FishNet.SceneManager conflict)
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    #endregion
}

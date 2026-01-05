using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// FishNet code regeneration trigger - do not remove
// Last regenerated: 2026-01-04

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
    public float reconnectGracePeriod = 120f;
    
    [Header("Runtime State")]
    private NetworkPlayer localNetworkPlayer;
    private NetworkPlayer opponentNetworkPlayer;
    
    private Dictionary<int, NetworkPlayer> networkPlayers = new Dictionary<int, NetworkPlayer>();
    
    // Disconnect handling
    private float disconnectTimer = 0f;
    private bool isWaitingForReconnect = false;
    private int disconnectedPlayerId = -1;
    
    // Reconnection flag - client sets this when it was disconnected and is reconnecting
    private bool _isReconnecting = false;

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
                    if (PlayerConnectionHandler.Instance != null)
                    {
                        PlayerConnectionHandler.Instance.ForfeitDisconnectedPlayer(disconnectedPlayerId);
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
        
        // Reset cached state for reconnection scenarios
        // This ensures a reconnecting client gets fresh state from the server
        _cachedTurnObjectId = -1;
        _cachedTurnNumber = 0;
        _cachedGameStarted = false;
        _cachedShuffleSeed = 0;
        
        // Note: Don't reset _encounterInitialized here - it will be handled by RPC
        // The BufferLast RPC will re-send the game state to the reconnecting client
        
        // Find all NetworkPlayer objects that have been spawned
        StartCoroutine(FindAndLinkPlayers());
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("[NetworkGameManager] Client stopped - resetting state for potential reconnect");
        
        // If game was in progress, mark as reconnecting so we don't re-initialize from scratch
        if (GameStarted.Value && _encounterInitialized)
        {
            _isReconnecting = true;
            Debug.Log("[NetworkGameManager] Game was in progress - will wait for state restoration on reconnect");
        }
        
        // Reset state so reconnection works properly
        _encounterInitialized = false;
        localNetworkPlayer = null;
        opponentNetworkPlayer = null;
        networkPlayers.Clear();
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
        
        // On clients (non-server), prefer using cached turn state from RPC
        // This works around FishNet scene object SyncVar sync issues
        int currentTurn;
        if (!IsServerInitialized && _cachedTurnObjectId >= 0)
        {
            // Use cached value from RPC (0 or higher is valid)
            currentTurn = _cachedTurnObjectId;
        }
        else
        {
            currentTurn = CurrentTurnObjectId.Value;
        }
        
        // Only -1 is invalid (means game not started). ObjectId 0 IS valid in FishNet!
        if (currentTurn < 0)
        {
            Debug.LogWarning($"[NetworkGameManager] CurrentTurnObjectId is invalid ({currentTurn}), game might not be started yet");
            return false;
        }
        
        // Simply check if the current turn ObjectId matches local player's ObjectId
        bool isLocal = currentTurn == localNetworkPlayer.ObjectId;
        Debug.Log($"[NetworkGameManager] IsLocalPlayerTurn: CurrentTurnObjectId={currentTurn}, localObjectId={localNetworkPlayer.ObjectId}, result={isLocal}");
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
        
        // Explicitly broadcast turn state via RPC to ensure clients get it
        RpcBroadcastTurnState(CurrentTurnObjectId.Value, TurnNumber.Value, GameStarted.Value, ShuffleSeed.Value);
        
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
        
        // Explicitly broadcast turn state via RPC to ensure clients get it
        RpcBroadcastTurnState(CurrentTurnObjectId.Value, TurnNumber.Value, GameStarted.Value, ShuffleSeed.Value);
        
        Debug.Log($"[Server] Game started! PlayerId {lowestPlayerId} (ObjectId {firstObjectId}) goes first.");
    }
    
    /// <summary>
    /// ObserversRpc to explicitly send turn state to all clients.
    /// This is a workaround for scene object SyncVar issues.
    /// </summary>
    [ObserversRpc(BufferLast = true)]
    private void RpcBroadcastTurnState(int turnObjectId, int turnNumber, bool gameStarted, int shuffleSeed)
    {
        Debug.Log($"[Client RPC] Received turn state: TurnObjectId={turnObjectId}, TurnNumber={turnNumber}, GameStarted={gameStarted}, cachedTurn={_cachedTurnNumber}");
        
        // Only process on non-server clients
        if (IsServerInitialized) return;
        
        // Check if this is a duplicate/same turn (important for buffered RPCs after reconnection)
        bool isNewTurn = turnNumber != _cachedTurnNumber;
        Debug.Log($"[Client RPC] isNewTurn={isNewTurn}, localNetworkPlayer={(localNetworkPlayer != null ? localNetworkPlayer.PlayerName.Value : "null")}");
        
        // Update local cache of turn state (these are used by IsLocalPlayerTurn)
        _cachedTurnObjectId = turnObjectId;
        _cachedTurnNumber = turnNumber;
        _cachedGameStarted = gameStarted;
        _cachedShuffleSeed = shuffleSeed;
        
        // If the game just started, initialize the encounter
        if (gameStarted && !_encounterInitialized)
        {
            TryInitializeEncounterFromRpc();
        }
        
        // Notify encounter controller about turn change
        if (gameStarted)
        {
            // Try to find encounterController if not set
            if (encounterController == null)
            {
                encounterController = FindObjectOfType<EncounterController>();
                Debug.Log($"[Client RPC] Found encounterController: {encounterController != null}");
            }
            
            if (encounterController != null)
            {
                bool isLocalTurn = IsLocalPlayerTurnFromCache();
                Debug.Log($"[Client RPC] Calling OnNetworkTurnChanged. isLocalTurn={isLocalTurn}, turnNumber={turnNumber}, isNewTurn={isNewTurn}");
                encounterController.OnNetworkTurnChanged(isLocalTurn, turnNumber);
            }
            else
            {
                Debug.LogWarning("[Client RPC] encounterController is null - cannot notify turn change!");
            }
        }
    }
    
    // Cached turn state from RPC (for clients)
    private int _cachedTurnObjectId = -1;
    private int _cachedTurnNumber = 0;
    private bool _cachedGameStarted = false;
    private int _cachedShuffleSeed = 0;
    
    private bool IsLocalPlayerTurnFromCache()
    {
        if (localNetworkPlayer == null) return false;
        return localNetworkPlayer.ObjectId == _cachedTurnObjectId;
    }
    
    private void TryInitializeEncounterFromRpc()
    {
        if (_encounterInitialized) return;
        if (!_cachedGameStarted) return;
        
        if (encounterController == null)
        {
            encounterController = FindObjectOfType<EncounterController>();
        }
        
        if (encounterController != null && localNetworkPlayer != null)
        {
            bool isLocalTurn = IsLocalPlayerTurnFromCache();
            Debug.Log($"[NetworkGameManager] Initializing encounter from RPC with seed {_cachedShuffleSeed}");
            encounterController.OnNetworkGameStarted(isLocalTurn, _cachedShuffleSeed);
            _encounterInitialized = true;
        }
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
        Debug.Log($"[NetworkGameManager] Turn changed: ObjectId {oldValue} -> ObjectId {newValue} (asServer={asServer})");
        
        // Only -1 is invalid (game not started). ObjectId 0 IS valid in FishNet!
        if (newValue < 0)
        {
            Debug.Log($"[NetworkGameManager] Ignoring invalid turn value: {newValue}");
            return;
        }
        
        // On the SERVER/HOST: Notify encounter controller via SyncVar callback
        // (The RPC is for remote clients only - it skips the server)
        if (IsServerInitialized && asServer && GameStarted.Value)
        {
            if (encounterController == null)
            {
                encounterController = FindObjectOfType<EncounterController>();
            }
            
            if (encounterController != null)
            {
                bool isLocalTurn = IsLocalPlayerTurn();
                Debug.Log($"[Server] SyncVar callback - notifying encounter. isLocalTurn={isLocalTurn}, turnNumber={TurnNumber.Value}");
                encounterController.OnNetworkTurnChanged(isLocalTurn, TurnNumber.Value);
            }
        }
        // Remote clients are handled by RpcBroadcastTurnState
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
        
        // If we're reconnecting, don't do normal initialization - wait for state restoration RPC
        if (_isReconnecting)
        {
            Debug.Log("[NetworkGameManager] Reconnecting - skipping normal initialization, waiting for state restoration");
            return;
        }
        
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
        Debug.Log($"[NetworkGameManager] OpponentDisconnected changed: {oldValue} -> {newValue}, asServer={asServer}");
        
        if (newValue && !oldValue)
        {
            // Opponent just disconnected
            isWaitingForReconnect = true;
            disconnectTimer = 0f;
            
            // Try to find EncounterController if we don't have it
            if (encounterController == null)
            {
                encounterController = FindObjectOfType<EncounterController>();
                Debug.Log($"[NetworkGameManager] Found EncounterController: {encounterController != null}");
            }
            
            if (encounterController != null)
            {
                Debug.Log($"[NetworkGameManager] Calling OnOpponentDisconnected with grace period {reconnectGracePeriod}");
                encounterController.OnOpponentDisconnected(reconnectGracePeriod);
            }
            else
            {
                Debug.LogError("[NetworkGameManager] EncounterController is null! Cannot show disconnect UI.");
            }
        }
    }
    
    /// <summary>
    /// Server: Called when a player disconnects during the game.
    /// </summary>
    [Server]
    public void ServerOnPlayerDisconnected(int playerId)
    {
        Debug.Log($"[Server] ServerOnPlayerDisconnected called for player {playerId}. GameStarted={GameStarted.Value}");
        
        if (!GameStarted.Value)
        {
            Debug.Log($"[Server] Game not started, ignoring disconnect");
            return;
        }
        
        Debug.Log($"[Server] Player {playerId} disconnected during game! Setting OpponentDisconnected=true");
        disconnectedPlayerId = playerId;
        OpponentDisconnected.Value = true;
    }
    
    /// <summary>
    /// Server: Called when a player reconnects.
    /// </summary>
    [Server]
    public void ServerOnPlayerReconnected(int playerId, NetworkConnection conn = null)
    {
        if (disconnectedPlayerId == playerId)
        {
            Debug.Log($"[Server] Player {playerId} reconnected!");
            OpponentDisconnected.Value = false;
            disconnectedPlayerId = -1;
            
            // Send current game state to the reconnected player
            StartCoroutine(SendGameStateToReconnectedPlayer(playerId, conn));
        }
    }
    
    /// <summary>
    /// Send the current game state to a reconnected player after a short delay.
    /// </summary>
    [Server]
    private System.Collections.IEnumerator SendGameStateToReconnectedPlayer(int playerId, NetworkConnection conn)
    {
        // Wait longer for the client to fully reconnect, load scenes, and sync
        // The client needs time to load DuelScreen scene and initialize NetworkGameManager
        yield return new WaitForSeconds(2.0f);
        
        Debug.Log($"[Server] Sending game state to reconnected player {playerId}");
        
        // Capture current game state from server's perspective
        var snapshot = CaptureServerGameState();
        if (snapshot != null)
        {
            string snapshotJson = snapshot.ToJson();
            Debug.Log($"[Server] Captured snapshot: Turn={snapshot.turnNumber}, LocalHand={snapshot.localPlayer?.handCardIds?.Count ?? 0}, OpponentHand={snapshot.opponent?.handCardIds?.Count ?? 0}");
            
            // Send to specific reconnected client if we have the connection
            if (conn != null && conn.IsValid)
            {
                Debug.Log($"[Server] Sending targeted RPC to connection {conn.ClientId}");
                TargetRpcRestoreFullGameState(conn, snapshotJson);
            }
            else
            {
                Debug.LogWarning("[Server] Connection invalid or null, using broadcast RPC");
                RpcRestoreFullGameState(snapshotJson);
            }
        }
        else
        {
            Debug.LogWarning("[Server] Failed to capture game state for reconnected player");
        }
    }
    
    /// <summary>
    /// Capture the current game state from the server's perspective.
    /// </summary>
    [Server]
    private GameStateSnapshot CaptureServerGameState()
    {
        if (encounterController == null)
        {
            Debug.LogError("[Server] Cannot capture game state - no EncounterController!");
            return null;
        }
        
        var snapshot = new GameStateSnapshot
        {
            turnNumber = TurnNumber.Value,
            currentTurnObjectId = CurrentTurnObjectId.Value,
            shuffleSeed = ShuffleSeed.Value,
            isFromServerPerspective = true, // Mark that this was captured from server
            
            // Host player (server's local) = "local" in snapshot
            localPlayer = CapturePlayerSnapshot(encounterController.player, encounterController.playerHandController),
            // Client player (opponent from server view) = "opponent" in snapshot  
            opponent = CapturePlayerSnapshot(encounterController.opponent, encounterController.opponentHandController)
        };
        
        Debug.Log($"[Server] Captured game state: Turn {snapshot.turnNumber}, LocalHand={snapshot.localPlayer?.handCardIds?.Count ?? 0}, OpponentHand={snapshot.opponent?.handCardIds?.Count ?? 0}");
        
        return snapshot;
    }
    
    /// <summary>
    /// Capture a player's current state into a snapshot.
    /// </summary>
    private PlayerSnapshot CapturePlayerSnapshot(PlayerController controller, HandController handController)
    {
        if (controller == null) return null;
        
        var snapshot = new PlayerSnapshot
        {
            health = controller.currentHealth,
            mana = controller.currentMana,
            maxMana = controller.maxMana,
            handCardIds = new System.Collections.Generic.List<string>(),
            boardCards = new System.Collections.Generic.List<BoardCardSnapshot>(),
            deckCardIds = new System.Collections.Generic.List<string>()
        };
        
        // Capture hand cards using GetHand() method
        if (handController != null)
        {
            var hand = handController.GetHand();
            if (hand != null)
            {
                foreach (var card in hand)
                {
                    if (card != null)
                    {
                        snapshot.handCardIds.Add(card.cardName);
                    }
                }
            }
        }
        
        // Capture board cards from PlayerController.board
        if (controller.board != null)
        {
            for (int i = 0; i < controller.board.Count; i++)
            {
                var card = controller.board[i];
                if (card != null)
                {
                    string slotName = "";
                    if (card.transform.parent != null)
                    {
                        slotName = card.transform.parent.name;
                    }
                    
                    snapshot.boardCards.Add(new BoardCardSnapshot
                    {
                        slotIndex = i,
                        slotName = slotName,
                        cardId = card.cardName,
                        currentHealth = card.health,
                        maxHealth = card.health,
                        currentAttack = card.manaCost,
                        hasAttacked = card.isTapped,
                        hasSummoningSickness = card.hasSummoningSickness,
                        canAttack = !card.hasSummoningSickness && !card.isTapped
                    });
                }
            }
        }
        
        // Capture deck
        if (controller.deck != null)
        {
            foreach (var card in controller.deck)
            {
                if (card != null)
                {
                    snapshot.deckCardIds.Add(card.cardName);
                }
            }
        }
        
        return snapshot;
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
    
    /// <summary>
    /// Server: Restore game state from a snapshot (sent by reconnecting client).
    /// </summary>
    [Server]
    public void ServerRestoreGameState(GameStateSnapshot snapshot)
    {
        if (snapshot == null)
        {
            Debug.LogError("[Server] Cannot restore null game state!");
            return;
        }
        
        Debug.Log($"[Server] Restoring game state: Turn {snapshot.turnNumber}");
        
        // Restore turn state
        TurnNumber.Value = snapshot.turnNumber;
        CurrentTurnObjectId.Value = snapshot.currentTurnObjectId;
        ShuffleSeed.Value = snapshot.shuffleSeed;
        
        // Restore player health/mana via their NetworkPlayers
        if (snapshot.localPlayer != null && localNetworkPlayer != null)
        {
            // The "local" from client perspective is the client (opponent from server view)
            var opponentNP = opponentNetworkPlayer ?? localNetworkPlayer;
            if (opponentNP != null)
            {
                opponentNP.CurrentHealth.Value = snapshot.localPlayer.health;
                opponentNP.CurrentMana.Value = snapshot.localPlayer.mana;
                opponentNP.MaxMana.Value = snapshot.localPlayer.maxMana;
            }
        }
        
        if (snapshot.opponent != null)
        {
            // The "opponent" from client perspective is the host (local from server view)
            var hostNP = localNetworkPlayer;
            if (hostNP != null)
            {
                hostNP.CurrentHealth.Value = snapshot.opponent.health;
                hostNP.CurrentMana.Value = snapshot.opponent.mana;
                hostNP.MaxMana.Value = snapshot.opponent.maxMana;
            }
        }
        
        Debug.Log("[Server] Basic game state restored, broadcasting card restoration...");
        
        // Broadcast card restoration to all clients
        // The client who sent the snapshot has the "localPlayer" data which is actually from their perspective
        // We need to tell all clients to restore board/hand state
        string snapshotJson = snapshot.ToJson();
        RpcRestoreFullGameState(snapshotJson);
    }
    
    /// <summary>
    /// Notify all clients to restore the full game state including cards.
    /// </summary>
    [ObserversRpc]
    private void RpcRestoreFullGameState(string snapshotJson)
    {
        Debug.Log("[Client] Received full game state restoration RPC (broadcast)");
        ProcessGameStateRestoration(snapshotJson);
    }
    
    /// <summary>
    /// Send game state restoration to a specific reconnected client.
    /// </summary>
    [TargetRpc]
    private void TargetRpcRestoreFullGameState(NetworkConnection conn, string snapshotJson)
    {
        Debug.Log($"[Client] Received targeted game state restoration RPC");
        ProcessGameStateRestoration(snapshotJson);
    }
    
    /// <summary>
    /// Process the game state restoration (shared by both broadcast and targeted RPCs).
    /// </summary>
    private void ProcessGameStateRestoration(string snapshotJson)
    {
        Debug.Log("[Client] Processing game state restoration...");
        Debug.Log($"[Client] State: isServer={IsServer}, localPlayer={(localNetworkPlayer != null ? localNetworkPlayer.name : "null")}, encounterController={(encounterController != null)}");
        
        var snapshot = GameStateSnapshot.FromJson(snapshotJson);
        if (snapshot == null)
        {
            Debug.LogError("[Client] Failed to deserialize game state snapshot!");
            return;
        }
        
        Debug.Log($"[Client] Snapshot: Turn={snapshot.turnNumber}, LocalHand={snapshot.localPlayer?.handCardIds?.Count ?? 0}, OpponentHand={snapshot.opponent?.handCardIds?.Count ?? 0}, FromServer={snapshot.isFromServerPerspective}");
        
        // Clear reconnection flag since we're now restoring
        if (_isReconnecting)
        {
            Debug.Log("[Client] Reconnection state restoration - clearing reconnect flag");
            _isReconnecting = false;
        }
        
        // Mark encounter as initialized for reconnecting clients
        _encounterInitialized = true;
        
        // CRITICAL: Re-register and re-link NetworkPlayers after reconnection
        // This ensures RPCs like RpcExecuteCardPlay can find the correct HandController
        Debug.Log("[Client] Re-registering NetworkPlayers after reconnection...");
        ReRegisterNetworkPlayers();
        
        // Ensure CardLibrary is populated before restoring cards
        if (CardLibrary.Instance != null)
        {
            // Force re-populate in case the library wasn't properly initialized
            if (CardLibrary.Instance.GetAllCards()?.Count == 0)
            {
                Debug.Log("[Client] CardLibrary empty, forcing re-populate...");
                CardLibrary.Instance.ForceRepopulate();
            }
            Debug.Log($"[Client] CardLibrary has {CardLibrary.Instance.GetAllCards()?.Count ?? 0} cards");
        }
        else
        {
            Debug.LogWarning("[Client] CardLibrary.Instance is null! Creating...");
            CardLibrary.EnsureInitialized();
            Debug.Log($"[Client] CardLibrary now has {CardLibrary.Instance.GetAllCards()?.Count ?? 0} cards");
        }
        
        // Restore cards on this client
        RestoreCardsFromSnapshot(snapshot);
        
        // Restore and broadcast turn state after cards are restored
        if (encounterController != null)
        {
            // Determine whose turn it is
            bool isLocalTurn = IsLocalPlayerTurn();
            encounterController.RestoreTurnState(isLocalTurn, snapshot.turnNumber);
            encounterController.OnGameStateRestored();
            Debug.Log($"[Client] Game state restoration complete! Turn={snapshot.turnNumber}, IsLocalTurn={isLocalTurn}");
        }
        else
        {
            Debug.LogWarning("[Client] EncounterController is null, cannot restore turn state!");
        }
    }
    
    /// <summary>
    /// Re-register and re-link all NetworkPlayers. Called after reconnection to ensure
    /// RPCs can correctly find the HandController for each player.
    /// </summary>
    private void ReRegisterNetworkPlayers()
    {
        // First, ensure we have references to the scene objects
        // These are needed for linking and restoration
        if (encounterController == null)
        {
            encounterController = FindObjectOfType<EncounterController>();
            Debug.Log($"[Client] Found EncounterController: {(encounterController != null ? "yes" : "no")}");
        }
        
        // Find PlayerControllers if not set (they're scene objects named "Player" and "Opponent")
        if (localPlayerController == null || opponentPlayerController == null)
        {
            PlayerController[] controllers = FindObjectsOfType<PlayerController>();
            foreach (var controller in controllers)
            {
                if (controller.gameObject.name == "Player" && localPlayerController == null)
                {
                    localPlayerController = controller;
                    Debug.Log("[Client] Found localPlayerController");
                }
                else if (controller.gameObject.name == "Opponent" && opponentPlayerController == null)
                {
                    opponentPlayerController = controller;
                    Debug.Log("[Client] Found opponentPlayerController");
                }
            }
        }
        
        // Clear existing NetworkPlayer registrations
        networkPlayers.Clear();
        localNetworkPlayer = null;
        opponentNetworkPlayer = null;
        
        // Find all NetworkPlayer objects in the scene
        NetworkPlayer[] players = FindObjectsOfType<NetworkPlayer>();
        Debug.Log($"[Client] Found {players.Length} NetworkPlayer(s) to re-register");
        
        foreach (var player in players)
        {
            RegisterNetworkPlayer(player);
        }
        
        // Force re-link to controllers
        LinkPlayersToControllers();
        
        // Log the state after re-linking
        Debug.Log($"[Client] After re-registration: localNetworkPlayer={(localNetworkPlayer != null ? localNetworkPlayer.PlayerName.Value : "null")}, opponentNetworkPlayer={(opponentNetworkPlayer != null ? opponentNetworkPlayer.PlayerName.Value : "null")}");
        
        // Verify LinkedPlayerController is set
        if (localNetworkPlayer != null)
        {
            Debug.Log($"[Client] localNetworkPlayer.LinkedPlayerController={(localNetworkPlayer.LinkedPlayerController != null ? localNetworkPlayer.LinkedPlayerController.gameObject.name : "null")}");
        }
        if (opponentNetworkPlayer != null)
        {
            Debug.Log($"[Client] opponentNetworkPlayer.LinkedPlayerController={(opponentNetworkPlayer.LinkedPlayerController != null ? opponentNetworkPlayer.LinkedPlayerController.gameObject.name : "null")}");
        }
        
        // Also verify PlayerController.networkPlayer is set (needed for HandController to send commands)
        if (localPlayerController != null)
        {
            Debug.Log($"[Client] localPlayerController.networkPlayer={(localPlayerController.networkPlayer != null ? localPlayerController.networkPlayer.PlayerName.Value : "null")}");
        }
    }
    
    /// <summary>
    /// Restore game state locally (called by client when host reconnects).
    /// This allows clients to restore their view without waiting for server RPC.
    /// </summary>
    public void RestoreGameStateLocally(GameStateSnapshot snapshot)
    {
        if (snapshot == null)
        {
            Debug.LogError("[Client] Cannot restore null game state locally!");
            return;
        }
        
        Debug.Log($"[Client] Restoring game state locally: Turn {snapshot.turnNumber}");
        
        // Clear reconnection flag since we're now restoring
        _isReconnecting = false;
        _encounterInitialized = true;
        
        // The snapshot was captured from THIS client's perspective (before host disconnect)
        // So isFromServerPerspective should be false
        snapshot.isFromServerPerspective = false;
        
        // Restore cards
        RestoreCardsFromSnapshot(snapshot);
        
        // Restore turn state
        if (encounterController != null)
        {
            bool isLocalTurn = IsLocalPlayerTurn();
            encounterController.RestoreTurnState(isLocalTurn, snapshot.turnNumber);
            encounterController.OnGameStateRestored();
        }
    }
    
    /// <summary>
    /// Restore all cards (hand, board, deck) from a snapshot.
    /// </summary>
    private void RestoreCardsFromSnapshot(GameStateSnapshot snapshot)
    {
        Debug.Log("[Client] Restoring cards from snapshot...");
        
        // Ensure CardLibrary is initialized
        CardLibrary.EnsureInitialized();
        
        if (CardLibrary.Instance == null)
        {
            Debug.LogError("[Client] CardLibrary not available! Cannot restore cards.");
            return;
        }
        
        // Determine which snapshot data applies to which local controller
        // This depends on whether we're the host or client AND who captured the snapshot
        bool isServer = IsServerInitialized;
        bool snapshotFromServer = snapshot.isFromServerPerspective;
        
        Debug.Log($"[Client] RestoreCardsFromSnapshot: isServer={isServer}, snapshotFromServer={snapshotFromServer}");
        
        PlayerSnapshot mySnapshot;
        PlayerSnapshot theirSnapshot;
        PlayerController myController;
        PlayerController theirController;
        HandController myHand;
        HandController theirHand;
        string mySlotPrefix;
        string theirSlotPrefix;
        bool needsSlotTranslation;
        
        if (snapshotFromServer)
        {
            // Snapshot was captured from SERVER (host) perspective:
            // - snapshot.localPlayer = HOST's data
            // - snapshot.opponent = CLIENT's data
            
            if (isServer)
            {
                // We ARE the server - snapshot.localPlayer is our data
                mySnapshot = snapshot.localPlayer;
                theirSnapshot = snapshot.opponent;
                needsSlotTranslation = false;
            }
            else
            {
                // We are CLIENT - snapshot.opponent is our data
                mySnapshot = snapshot.opponent;
                theirSnapshot = snapshot.localPlayer;
                needsSlotTranslation = true; // Need to swap slot names since perspective is different
            }
        }
        else
        {
            // Snapshot was captured from CLIENT perspective (original logic):
            // - snapshot.localPlayer = CAPTURING CLIENT's data
            // - snapshot.opponent = HOST's data
            
            if (isServer)
            {
                // On server receiving client snapshot:
                // - snapshot.opponent is the HOST (us)
                // - snapshot.localPlayer is the CLIENT
                mySnapshot = snapshot.opponent;
                theirSnapshot = snapshot.localPlayer;
                needsSlotTranslation = true;
            }
            else
            {
                // On client: snapshot.localPlayer is ME
                mySnapshot = snapshot.localPlayer;
                theirSnapshot = snapshot.opponent;
                needsSlotTranslation = false;
            }
        }
        
        myController = localPlayerController;
        theirController = opponentPlayerController;
        myHand = encounterController?.playerHandController;
        theirHand = encounterController?.opponentHandController;
        mySlotPrefix = "PlayerSlot";
        theirSlotPrefix = "OpponentSlot";
        
        Debug.Log($"[Client] Restoring - mySnapshot hand: {mySnapshot?.handCardIds?.Count ?? 0}, theirSnapshot hand: {theirSnapshot?.handCardIds?.Count ?? 0}");
        
        // Clear current board and hands before restoring
        ClearBoardAndHands();
        
        // Restore my state
        if (mySnapshot != null && myController != null)
        {
            RestorePlayerCards(mySnapshot, myController, myHand, mySlotPrefix, needsSlotTranslation);
        }
        
        // Restore opponent state
        if (theirSnapshot != null && theirController != null)
        {
            // On server: theirSnapshot is from client's own perspective, needs translation
            RestorePlayerCards(theirSnapshot, theirController, theirHand, theirSlotPrefix, needsSlotTranslation);
        }
        
        Debug.Log("[Client] Card restoration complete!");
    }
    
    /// <summary>
    /// Clear all cards from board and hands.
    /// </summary>
    private void ClearBoardAndHands()
    {
        Debug.Log("[Client] Clearing board and hands...");
        
        // Clear player slots
        for (int i = 1; i <= 3; i++)
        {
            ClearSlot($"PlayerSlot-{i}");
            ClearSlot($"OpponentSlot-{i}");
        }
        
        // Clear player boards
        if (localPlayerController != null)
        {
            localPlayerController.board.Clear();
        }
        if (opponentPlayerController != null)
        {
            opponentPlayerController.board.Clear();
        }
        
        // Clear hands
        var playerHand = encounterController?.playerHandController;
        var oppHand = encounterController?.opponentHandController;
        
        if (playerHand != null)
        {
            var hand = playerHand.GetHand();
            if (hand != null)
            {
                foreach (var card in hand.ToArray())
                {
                    if (card != null) Destroy(card.gameObject);
                }
                hand.Clear();
            }
        }
        
        if (oppHand != null)
        {
            var hand = oppHand.GetHand();
            if (hand != null)
            {
                foreach (var card in hand.ToArray())
                {
                    if (card != null) Destroy(card.gameObject);
                }
                hand.Clear();
            }
        }
    }
    
    /// <summary>
    /// Translate a slot name from one perspective to the other.
    /// PlayerSlot-X <-> OpponentSlot-X, with slot mirroring (1<->3 swap for front/back)
    /// </summary>
    private string TranslateSlotName(string slotName)
    {
        if (string.IsNullOrEmpty(slotName)) return slotName;
        
        // Extract the slot number and mirror it (1<->3, 2 stays)
        string MirrorSlotNumber(string name)
        {
            if (name.EndsWith("-1")) return name.Substring(0, name.Length - 1) + "3";
            if (name.EndsWith("-3")) return name.Substring(0, name.Length - 1) + "1";
            return name; // -2 stays the same
        }
        
        if (slotName.StartsWith("PlayerSlot-"))
        {
            string translated = slotName.Replace("PlayerSlot-", "OpponentSlot-");
            return MirrorSlotNumber(translated);
        }
        else if (slotName.StartsWith("OpponentSlot-"))
        {
            string translated = slotName.Replace("OpponentSlot-", "PlayerSlot-");
            return MirrorSlotNumber(translated);
        }
        
        return slotName;
    }
    
    /// <summary>
    /// Clear a single board slot.
    /// </summary>
    private void ClearSlot(string slotName)
    {
        var slot = GameObject.Find(slotName);
        if (slot != null)
        {
            var card = slot.GetComponentInChildren<CardController>();
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
    }
    
    /// <summary>
    /// Restore cards for a single player.
    /// </summary>
    /// <param name="snapshot">The player's state snapshot</param>
    /// <param name="controller">The PlayerController to restore to</param>
    /// <param name="handController">The HandController for this player</param>
    /// <param name="slotPrefix">The slot prefix to use (e.g., "PlayerSlot" or "OpponentSlot")</param>
    /// <param name="translateSlots">If true, translate slot names from opposite perspective</param>
    private void RestorePlayerCards(PlayerSnapshot snapshot, PlayerController controller, HandController handController, string slotPrefix, bool translateSlots = false)
    {
        Debug.Log($"[Client] Restoring cards for {controller.name}: " +
                  $"{snapshot.handCardIds.Count} hand, {snapshot.boardCards.Count} board, {snapshot.deckCardIds.Count} deck");
        
        // Restore board cards first
        foreach (var boardCard in snapshot.boardCards)
        {
            var newCard = CardLibrary.Instance.InstantiateCard(boardCard.cardId);
            if (newCard == null)
            {
                Debug.LogWarning($"[Client] Could not create card: {boardCard.cardId}");
                continue;
            }
            
            // Determine the correct slot name
            string slotName;
            if (!string.IsNullOrEmpty(boardCard.slotName) && translateSlots)
            {
                // Translate slot name from other perspective
                // If captured as "PlayerSlot-X", it should be "OpponentSlot-X" on the other side, and vice versa
                slotName = TranslateSlotName(boardCard.slotName);
                Debug.Log($"[Client] Translated slot {boardCard.slotName} -> {slotName}");
            }
            else if (!string.IsNullOrEmpty(boardCard.slotName))
            {
                // Use the slot name as captured
                slotName = boardCard.slotName;
            }
            else
            {
                // Fallback: use slot prefix with index
                slotName = $"{slotPrefix}-{boardCard.slotIndex + 1}";
            }
            
            var slot = GameObject.Find(slotName);
            if (slot == null)
            {
                Debug.LogWarning($"[Client] Could not find slot: {slotName}");
                Destroy(newCard.gameObject);
                continue;
            }
            
            // Position card in slot
            newCard.transform.SetParent(slot.transform);
            newCard.transform.localPosition = Vector3.zero;
            newCard.transform.localRotation = Quaternion.identity;
            newCard.transform.localScale = Vector3.one;
            
            // Restore card state
            newCard.health = boardCard.currentHealth;
            newCard.isTapped = boardCard.hasAttacked;
            newCard.hasSummoningSickness = boardCard.hasSummoningSickness;
            newCard.isInHand = false;
            newCard.isInPlay = true;
            newCard.owningPlayer = controller;
            
            // Update visuals
            newCard.UpdateCardUI();
            newCard.UpdateVisualEffects();
            
            // Add to board list
            controller.board.Add(newCard);
            
            Debug.Log($"[Client] Restored board card: {boardCard.cardId} to {slotName} with HP={boardCard.currentHealth}");
        }
        
        // Restore hand cards
        if (handController != null)
        {
            foreach (var cardId in snapshot.handCardIds)
            {
                var newCard = CardLibrary.Instance.InstantiateCard(cardId);
                if (newCard == null)
                {
                    Debug.LogWarning($"[Client] Could not create hand card: {cardId}");
                    continue;
                }
                
                newCard.owningPlayer = controller;
                newCard.isInHand = true;
                newCard.isInPlay = false;
                
                // Add to hand using HandController (use AddExistingCardToHand since card is already instantiated)
                handController.AddExistingCardToHand(newCard);
                
                Debug.Log($"[Client] Restored hand card: {cardId}");
            }
        }
        
        // Restore deck order
        if (controller.deck != null)
        {
            controller.deck.Clear();
            foreach (var cardId in snapshot.deckCardIds)
            {
                var newCard = CardLibrary.Instance.InstantiateCard(cardId);
                if (newCard != null)
                {
                    newCard.owningPlayer = controller;
                    newCard.isInHand = false;
                    newCard.isInPlay = false;
                    // Hide deck cards
                    newCard.gameObject.SetActive(false);
                    controller.deck.Add(newCard);
                }
            }
            Debug.Log($"[Client] Restored {controller.deck.Count} deck cards to {controller.gameObject.name}");
        }
    }
    
    /// <summary>
    /// Notify all clients that game state has been restored after reconnection.
    /// </summary>
    [ObserversRpc]
    private void RpcGameStateRestored()
    {
        Debug.Log("[Client] Game state restored - resuming game!");
        
        if (encounterController != null)
        {
            encounterController.OnGameStateRestored();
        }
    }
    
    #endregion
}

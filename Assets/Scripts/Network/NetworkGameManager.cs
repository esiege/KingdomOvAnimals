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
        Debug.Log($"[Client RPC] Received turn state: TurnObjectId={turnObjectId}, TurnNumber={turnNumber}, GameStarted={gameStarted}");
        
        // Only process on non-server clients
        if (IsServerInitialized) return;
        
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
                Debug.Log($"[Client RPC] Calling OnNetworkTurnChanged. isLocalTurn={isLocalTurn}, turnNumber={turnNumber}");
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
        Debug.Log("[Client] Received full game state restoration RPC");
        
        var snapshot = GameStateSnapshot.FromJson(snapshotJson);
        if (snapshot == null)
        {
            Debug.LogError("[Client] Failed to deserialize game state snapshot!");
            return;
        }
        
        // Restore cards on this client
        RestoreCardsFromSnapshot(snapshot);
        
        // Notify encounter controller
        if (encounterController != null)
        {
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
        // This depends on whether we're the host or client
        bool isServer = IsServerInitialized;
        
        PlayerSnapshot mySnapshot;
        PlayerSnapshot theirSnapshot;
        PlayerController myController;
        PlayerController theirController;
        HandController myHand;
        HandController theirHand;
        string mySlotPrefix;
        string theirSlotPrefix;
        bool needsSlotTranslation;
        
        if (isServer)
        {
            // On server: "opponent" in snapshot is ME (host), "localPlayer" is the client
            // The snapshot was captured from the CLIENT's perspective, so:
            // - snapshot.localPlayer has slots like "PlayerSlot-X" (client's view of their slots)
            // - snapshot.opponent has slots like "OpponentSlot-X" (client's view of host slots)
            // On server:
            // - My slots (host) are "PlayerSlot-X", so snapshot.opponent slots need translation from "OpponentSlot" -> "PlayerSlot"
            // - Their slots (client) are "OpponentSlot-X", so snapshot.localPlayer slots need translation from "PlayerSlot" -> "OpponentSlot"
            mySnapshot = snapshot.opponent;
            theirSnapshot = snapshot.localPlayer;
            myController = localPlayerController;
            theirController = opponentPlayerController;
            myHand = encounterController?.playerHandController;
            theirHand = encounterController?.opponentHandController;
            mySlotPrefix = "PlayerSlot";
            theirSlotPrefix = "OpponentSlot";
            needsSlotTranslation = true; // Server needs to translate client's slot names
        }
        else
        {
            // On client: "localPlayer" in snapshot is ME, "opponent" is the host
            // The snapshot was captured from THIS client's perspective, so no translation needed
            mySnapshot = snapshot.localPlayer;
            theirSnapshot = snapshot.opponent;
            myController = localPlayerController;
            theirController = opponentPlayerController;
            myHand = encounterController?.playerHandController;
            theirHand = encounterController?.opponentHandController;
            mySlotPrefix = "PlayerSlot";
            theirSlotPrefix = "OpponentSlot";
            needsSlotTranslation = false; // Client uses its own perspective
        }
        
        // Clear current board and hands before restoring
        ClearBoardAndHands();
        
        // Restore my state
        if (mySnapshot != null && myController != null)
        {
            // On server: mySnapshot is from opponent's perspective, needs translation
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
    /// PlayerSlot-X <-> OpponentSlot-X
    /// </summary>
    private string TranslateSlotName(string slotName)
    {
        if (string.IsNullOrEmpty(slotName)) return slotName;
        
        if (slotName.StartsWith("PlayerSlot-"))
        {
            return slotName.Replace("PlayerSlot-", "OpponentSlot-");
        }
        else if (slotName.StartsWith("OpponentSlot-"))
        {
            return slotName.Replace("OpponentSlot-", "PlayerSlot-");
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
            Debug.Log($"[Client] Restored {controller.deck.Count} deck cards");
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

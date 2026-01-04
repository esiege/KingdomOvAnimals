using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the networked game state in the DuelScreen scene.
/// Links spawned NetworkPlayer objects to local PlayerController objects.
/// Also handles turn synchronization.
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

    [Header("Runtime State")]
    private NetworkPlayer localNetworkPlayer;
    private NetworkPlayer opponentNetworkPlayer;
    
    private Dictionary<int, NetworkPlayer> networkPlayers = new Dictionary<int, NetworkPlayer>();

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
    }
    
    private void OnDestroy()
    {
        CurrentTurnObjectId.OnChange -= OnTurnChanged;
        TurnNumber.OnChange -= OnTurnNumberChanged;
        GameStarted.OnChange -= OnGameStartedChanged;
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
            Debug.Log($"[NetworkGameManager] Registered NetworkPlayer: {player.PlayerName.Value} (ObjectId: {objectId}, PlayerId: {player.PlayerId.Value})");
        }
        
        // Check if this is our local player
        if (player.IsOwner)
        {
            localNetworkPlayer = player;
            Debug.Log($"[NetworkGameManager] Local player identified: {player.PlayerName.Value}");
        }
        else
        {
            opponentNetworkPlayer = player;
            Debug.Log($"[NetworkGameManager] Opponent identified: {player.PlayerName.Value}");
        }
        
        // Try to link after each registration
        LinkPlayersToControllers();
    }

    private void LinkPlayersToControllers()
    {
        // Link local player
        if (localNetworkPlayer != null && localPlayerController != null)
        {
            if (localPlayerController.networkPlayer == null)
            {
                localNetworkPlayer.LinkToPlayerController(localPlayerController);
                localPlayerController.networkPlayer = localNetworkPlayer;
                Debug.Log("[NetworkGameManager] Linked local NetworkPlayer to PlayerController");
            }
        }
        
        // Link opponent
        if (opponentNetworkPlayer != null && opponentPlayerController != null)
        {
            if (opponentPlayerController.networkPlayer == null)
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
        if (localNetworkPlayer == null) return false;
        
        // Simply check if the current turn ObjectId matches local player's ObjectId
        bool isLocal = CurrentTurnObjectId.Value == localNetworkPlayer.ObjectId;
        Debug.Log($"[NetworkGameManager] IsLocalPlayerTurn: CurrentTurnObjectId={CurrentTurnObjectId.Value}, localObjectId={localNetworkPlayer.ObjectId}, result={isLocal}");
        return isLocal;
    }
    
    /// <summary>
    /// Request to end the current turn (client calls this).
    /// </summary>
    public void RequestEndTurn()
    {
        if (!IsLocalPlayerTurn())
        {
            Debug.LogWarning("[NetworkGameManager] Cannot end turn - not your turn!");
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
        
        // Find the lowest ObjectId (first spawned player) to go first
        int firstObjectId = -1;
        foreach (var kvp in networkPlayers)
        {
            if (firstObjectId == -1 || kvp.Key < firstObjectId)
            {
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
        
        Debug.Log($"[Server] Game started! ObjectId {firstObjectId} goes first.");
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
    
    #endregion
}

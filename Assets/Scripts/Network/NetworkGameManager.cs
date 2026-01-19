using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using KOA.Network;
using KOA.Data;

/// <summary>
/// Manages the networked game state in the DuelScreen scene.
/// Story 036: Simplified to work with NetworkBoardState architecture.
/// Handles turn management, player registration, and game start.
/// </summary>
public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

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
    private KOA.Network.NetworkPlayer localNetworkPlayer;
    private KOA.Network.NetworkPlayer opponentNetworkPlayer;
    
    private Dictionary<int, KOA.Network.NetworkPlayer> networkPlayers = new Dictionary<int, KOA.Network.NetworkPlayer>();
    
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
        
        // Diagnostic: Log SyncVar indices
        LogSyncTypeIndices();
    }
    
    /// <summary>
    /// Logs all SyncType indices registered for this NetworkBehaviour.
    /// Used to diagnose index mismatch between editor and build.
    /// </summary>
    private void LogSyncTypeIndices()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[NetworkGameManager] SyncType indices on {(Application.isEditor ? "EDITOR" : "BUILD")}:");
        
        // Use reflection to access FishNet's internal _syncTypes dictionary
        var field = typeof(FishNet.Object.NetworkBehaviour).GetField("_syncTypes", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            var syncTypes = field.GetValue(this) as System.Collections.IDictionary;
            if (syncTypes != null)
            {
                sb.AppendLine($"  Total SyncTypes registered: {syncTypes.Count}");
                foreach (System.Collections.DictionaryEntry entry in syncTypes)
                {
                    var syncBase = entry.Value as FishNet.Object.Synchronizing.Internal.SyncBase;
                    string typeName = syncBase?.GetType().Name ?? "unknown";
                    sb.AppendLine($"  Index {entry.Key}: {typeName}");
                }
            }
            else
            {
                sb.AppendLine("  _syncTypes is null (not yet initialized)");
            }
        }
        else
        {
            sb.AppendLine("  Could not find _syncTypes field via reflection");
        }
        
        Debug.Log(sb.ToString());
    }
    
    private void OnDestroy()
    {
        CurrentTurnObjectId.OnChange -= OnTurnChanged;
        TurnNumber.OnChange -= OnTurnNumberChanged;
        GameStarted.OnChange -= OnGameStartedChanged;
        OpponentDisconnected.OnChange -= OnOpponentDisconnectedChanged;
        
        if (Instance == this)
        {
            Instance = null;
        }
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
        }
        else if (isWaitingForReconnect)
        {
            disconnectTimer += Time.deltaTime;
            
            // Grace period expired
            if (disconnectTimer >= reconnectGracePeriod)
            {
                Debug.Log("[NetworkGameManager] Reconnect grace period expired - opponent forfeits!");
                isWaitingForReconnect = false;
                
                // Return to main menu after a short delay
                StartCoroutine(ReturnToMainMenuDelayed(3f));
            }
        }
    }
    
    private IEnumerator ReturnToMainMenuDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("[NetworkGameManager] OnStartClient - finding existing players");
        
        // Find all existing NetworkPlayers
        var existingPlayers = FindObjectsOfType<KOA.Network.NetworkPlayer>();
        foreach (var player in existingPlayers)
        {
            RegisterNetworkPlayer(player);
        }
    }

    /// <summary>
    /// Register a NetworkPlayer (called when one is found or spawned).
    /// </summary>
    public void RegisterNetworkPlayer(KOA.Network.NetworkPlayer player)
    {
        int objectId = player.ObjectId;
        
        if (!networkPlayers.ContainsKey(objectId))
        {
            networkPlayers[objectId] = player;
            Debug.Log($"[NetworkGameManager] Registered NetworkPlayer: {player.PlayerName.Value} (ObjectId: {objectId}, PlayerId: {player.PlayerId.Value}, IsOwner: {player.IsOwner})");
        }
        
        // Check if this is our local player
        if (player.IsOwner)
        {
            if (localNetworkPlayer == null || localNetworkPlayer == player)
            {
                localNetworkPlayer = player;
                Debug.Log($"[NetworkGameManager] Local player identified: {player.PlayerName.Value} (ObjectId: {objectId})");
            }
        }
        else
        {
            if (opponentNetworkPlayer == null || opponentNetworkPlayer == player)
            {
                opponentNetworkPlayer = player;
                Debug.Log($"[NetworkGameManager] Opponent identified: {player.PlayerName.Value} (ObjectId: {objectId})");
            }
        }
        
        // Check if both players are connected and start game
        if (IsServerInitialized && AreBothPlayersConnected() && !GameStarted.Value)
        {
            ServerStartGame();
        }
    }

    /// <summary>
    /// Check if both players are connected.
    /// </summary>
    public bool AreBothPlayersConnected()
    {
        return localNetworkPlayer != null && opponentNetworkPlayer != null;
    }

    /// <summary>
    /// Get the NetworkPlayer for a given ObjectId.
    /// </summary>
    public KOA.Network.NetworkPlayer GetNetworkPlayer(int objectId)
    {
        networkPlayers.TryGetValue(objectId, out KOA.Network.NetworkPlayer player);
        return player;
    }

    /// <summary>
    /// Get the local player's NetworkPlayer.
    /// </summary>
    public KOA.Network.NetworkPlayer GetLocalPlayer() => localNetworkPlayer;

    /// <summary>
    /// Get the opponent's NetworkPlayer.
    /// </summary>
    public KOA.Network.NetworkPlayer GetOpponentPlayer() => opponentNetworkPlayer;

    #region Turn Management

    /// <summary>
    /// Server: Start the game.
    /// </summary>
    [Server]
    public void ServerStartGame()
    {
        if (GameStarted.Value)
        {
            Debug.LogWarning("[Server] Game already started");
            return;
        }
        
        Debug.Log("[Server] Starting game!");
        
        // Generate shuffle seed
        ShuffleSeed.Value = Random.Range(int.MinValue, int.MaxValue);
        
        // Initialize NetworkBoardState with decks
        InitializeNetworkBoardState();
        
        // Set first turn
        TurnNumber.Value = 1;
        
        // Randomly pick starting player
        var players = new List<KOA.Network.NetworkPlayer>(networkPlayers.Values);
        if (players.Count >= 2)
        {
            int startingIndex = Random.Range(0, 2);
            CurrentTurnObjectId.Value = players[startingIndex].ObjectId;
            Debug.Log($"[Server] First turn: {players[startingIndex].PlayerName.Value}");
        }
        
        GameStarted.Value = true;
        
        // Notify clients
        RpcGameStarted(CurrentTurnObjectId.Value, TurnNumber.Value);
    }
    
    /// <summary>
    /// Initialize NetworkBoardState with player decks.
    /// </summary>
    [Server]
    private void InitializeNetworkBoardState()
    {
        if (NetworkBoardState.Instance == null)
        {
            Debug.LogError("[Server] NetworkBoardState not found!");
            return;
        }
        
        // Load default deck from Resources
        var deckData = Resources.Load<DeckData>("Decks/DefaultDeck");
        if (deckData == null)
        {
            // Try loading any deck
            var allDecks = Resources.LoadAll<DeckData>("Decks");
            if (allDecks.Length > 0)
            {
                deckData = allDecks[0];
            }
        }
        
        if (deckData == null)
        {
            Debug.LogError("[Server] No DeckData found in Resources/Decks!");
            return;
        }
        
        // Convert DeckData to list of card IDs for both players
        var player0Deck = new List<string>();
        var player1Deck = new List<string>();
        
        foreach (var cardData in deckData.cards)
        {
            if (cardData != null)
            {
                string cardId = !string.IsNullOrEmpty(cardData.id) ? cardData.id : cardData.name;
                player0Deck.Add(cardId);
                player1Deck.Add(cardId);
            }
        }
        
        Debug.Log($"[Server] Initializing NetworkBoardState with {player0Deck.Count} cards per player");
        NetworkBoardState.Instance.InitializeGame(player0Deck, player1Deck);
    }

    /// <summary>
    /// Client requests to end their turn.
    /// </summary>
    public void RequestEndTurn()
    {
        if (localNetworkPlayer == null)
        {
            Debug.LogWarning("[NetworkGameManager] Cannot end turn - local player not found");
            return;
        }
        
        CmdEndTurn(localNetworkPlayer.ObjectId);
    }
    
    /// <summary>
    /// Server RPC: End turn request from client.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void CmdEndTurn(int requestingObjectId)
    {
        // Validate it's actually their turn
        if (CurrentTurnObjectId.Value != requestingObjectId)
        {
            Debug.LogWarning($"[Server] Player {requestingObjectId} tried to end turn but it's {CurrentTurnObjectId.Value}'s turn");
            return;
        }
        
        // Find next player
        KOA.Network.NetworkPlayer currentPlayer = null;
        KOA.Network.NetworkPlayer nextPlayer = null;
        
        foreach (var player in networkPlayers.Values)
        {
            if (player.ObjectId == requestingObjectId)
            {
                currentPlayer = player;
            }
            else
            {
                nextPlayer = player;
            }
        }
        
        if (nextPlayer == null)
        {
            Debug.LogWarning("[Server] Could not find next player!");
            return;
        }
        
        // Update turn state
        TurnNumber.Value++;
        CurrentTurnObjectId.Value = nextPlayer.ObjectId;
        
        Debug.Log($"[Server] Turn ended. Now turn {TurnNumber.Value}: {nextPlayer.PlayerName.Value}");
        
        // NetworkBoardState will handle turn-end logic via its own turn tracking
        // The BoardState.CurrentTurnPlayerId is already synced via SyncVar
        
        // Notify clients
        RpcTurnChanged(CurrentTurnObjectId.Value, TurnNumber.Value);
    }
    
    /// <summary>
    /// Check if it's the local player's turn.
    /// </summary>
    public bool IsLocalPlayerTurn()
    {
        if (localNetworkPlayer == null) return false;
        return CurrentTurnObjectId.Value == localNetworkPlayer.ObjectId;
    }
    
    /// <summary>
    /// Get the local player's PlayerId (0 or 1).
    /// </summary>
    public int GetLocalPlayerId()
    {
        if (localNetworkPlayer == null) return -1;
        return localNetworkPlayer.PlayerId.Value;
    }

    #endregion

    #region SyncVar Callbacks

    private void OnTurnChanged(int prev, int next, bool asServer)
    {
        Debug.Log($"[NetworkGameManager] Turn changed: ObjectId {prev} -> {next}");
    }
    
    private void OnTurnNumberChanged(int prev, int next, bool asServer)
    {
        Debug.Log($"[NetworkGameManager] Turn number: {prev} -> {next}");
    }
    
    private void OnGameStartedChanged(bool prev, bool next, bool asServer)
    {
        Debug.Log($"[NetworkGameManager] Game started: {prev} -> {next}");
    }
    
    private void OnOpponentDisconnectedChanged(bool prev, bool next, bool asServer)
    {
        if (next && !prev)
        {
            Debug.Log("[NetworkGameManager] Opponent disconnected - starting grace period");
            isWaitingForReconnect = true;
            disconnectTimer = 0f;
        }
    }

    #endregion

    #region RPCs

    /// <summary>
    /// Notify all clients that the game has started.
    /// </summary>
    [ObserversRpc]
    private void RpcGameStarted(int firstTurnObjectId, int turnNumber)
    {
        Debug.Log($"[Client RPC] Game started! First turn: ObjectId {firstTurnObjectId}, Turn {turnNumber}");
    }
    
    /// <summary>
    /// Notify all clients of a turn change.
    /// </summary>
    [ObserversRpc]
    private void RpcTurnChanged(int newTurnObjectId, int turnNumber)
    {
        Debug.Log($"[Client RPC] Turn changed: ObjectId {newTurnObjectId}, Turn {turnNumber}");
    }

    #endregion

    #region Disconnect Handling

    /// <summary>
    /// Called by server when a player disconnects.
    /// </summary>
    [Server]
    public void OnPlayerDisconnected(int playerId)
    {
        Debug.Log($"[Server] Player {playerId} disconnected");
        OpponentDisconnected.Value = true;
        disconnectedPlayerId = playerId;
    }
    
    /// <summary>
    /// Called by server when a player reconnects.
    /// </summary>
    [Server]
    public void OnPlayerReconnected(int playerId)
    {
        Debug.Log($"[Server] Player {playerId} reconnected");
        OpponentDisconnected.Value = false;
        disconnectedPlayerId = -1;
    }

    #endregion
}

using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player connections - spawns player objects when clients connect,
/// tracks all connected players, and handles cleanup on disconnect.
/// Supports reconnection during grace period.
/// </summary>
public class PlayerConnectionHandler : MonoBehaviour
{
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
    /// Disconnected players awaiting reconnection, keyed by their PlayerId.
    /// </summary>
    private Dictionary<int, NetworkPlayer> _disconnectedPlayers = new Dictionary<int, NetworkPlayer>();

    /// <summary>
    /// Number of currently connected players.
    /// </summary>
    public int PlayerCount => ConnectedPlayers.Count;

    private NetworkManager _networkManager;

    private void Awake()
    {
        _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null)
        {
            Debug.LogError("NetworkManager not found!");
            return;
        }

        // Subscribe to server events for remote client connections
        _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        
        // Subscribe to client events for detecting host disconnect
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
        {
            _networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        }
    }

    /// <summary>
    /// Called on the CLIENT when their connection state to the server changes.
    /// This detects when the host disconnects (client loses connection).
    /// </summary>
    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            // We lost connection to the server (host disconnected)
            Debug.Log("[PlayerConnectionHandler] Lost connection to host!");
            
            // If we're not the server (i.e., we're the client that lost connection)
            if (!_networkManager.IsServerStarted)
            {
                OnHostDisconnected();
            }
        }
    }

    /// <summary>
    /// Called on clients when the host disconnects.
    /// </summary>
    private void OnHostDisconnected()
    {
        Debug.Log("[PlayerConnectionHandler] Host disconnected! Returning to main menu...");
        
        // Show message to user and return to menu
        EncounterController encounterController = FindObjectOfType<EncounterController>();
        if (encounterController != null)
        {
            encounterController.OnHostDisconnected();
        }
        else
        {
            // Fallback: just return to menu immediately
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>
    /// Called on the server when a remote client's connection state changes.
    /// </summary>
    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
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
        NetworkPlayer reconnectingPlayer = TryGetReconnectingPlayer();
        
        if (reconnectingPlayer != null)
        {
            // Reconnection!
            HandleReconnection(conn, reconnectingPlayer);
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
    /// Try to find a disconnected player waiting for reconnection.
    /// </summary>
    private NetworkPlayer TryGetReconnectingPlayer()
    {
        // Return the first disconnected player (simple 1v1 game)
        foreach (var kvp in _disconnectedPlayers)
        {
            return kvp.Value;
        }
        return null;
    }
    
    /// <summary>
    /// Handle a player reconnecting to their existing session.
    /// </summary>
    private void HandleReconnection(NetworkConnection conn, NetworkPlayer player)
    {
        int playerId = player.PlayerId.Value;
        Debug.Log($"[PlayerConnectionHandler] Player {playerId} reconnecting with Connection ID {conn.ClientId}");
        
        // Remove from disconnected list
        _disconnectedPlayers.Remove(playerId);
        
        // Add to connected players with new connection ID
        ConnectedPlayers[conn.ClientId] = player;
        
        // Transfer ownership back to the reconnecting client
        player.GiveOwnership(conn);
        
        Debug.Log($"[PlayerConnectionHandler] Player {playerId} reconnected! Ownership transferred.");
        
        // Notify NetworkGameManager
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.ServerOnPlayerReconnected(playerId);
        }
    }

    /// <summary>
    /// Called when a player disconnects from the server.
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
            
            // Transfer ownership to server to prevent despawn
            disconnectedPlayer.RemoveOwnership();
            
            // Move to disconnected players for potential reconnection
            _disconnectedPlayers[disconnectedPlayerId] = disconnectedPlayer;
            
            Debug.Log($"[PlayerConnectionHandler] Player {disconnectedPlayerId} preserved for reconnection");
        }

        // Remove from active connections
        if (ConnectedPlayers.ContainsKey(conn.ClientId))
        {
            ConnectedPlayers.Remove(conn.ClientId);
        }

        if (logConnections)
            Debug.Log($"[PlayerConnectionHandler] Remaining connected players: {PlayerCount}, Disconnected awaiting reconnect: {_disconnectedPlayers.Count}");

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
        if (_disconnectedPlayers.TryGetValue(playerId, out NetworkPlayer player))
        {
            Debug.Log($"[PlayerConnectionHandler] Removing forfeited player {playerId}");
            _disconnectedPlayers.Remove(playerId);
            
            // Now despawn since they forfeited
            if (player != null && player.IsSpawned)
            {
                _networkManager.ServerManager.Despawn(player.NetworkObject);
            }
        }
    }
    
    /// <summary>
    /// Check if a player is disconnected and awaiting reconnection.
    /// </summary>
    public bool IsPlayerDisconnected(int playerId)
    {
        return _disconnectedPlayers.ContainsKey(playerId);
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

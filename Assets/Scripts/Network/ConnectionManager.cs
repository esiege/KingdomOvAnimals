using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

/// <summary>
/// Simple connection manager for Kingdom Ov Animals.
/// Handles starting/stopping server and client connections.
/// </summary>
public class ConnectionManager : MonoBehaviour
{
    private NetworkManager _networkManager;

    private void Awake()
    {
        _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null)
        {
            Debug.LogError("NetworkManager not found! Make sure NetworkManager prefab is in the scene.");
            return;
        }

        // Subscribe to connection events
        _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
        {
            _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        }
    }

    /// <summary>
    /// Start as Host (Server + Client)
    /// </summary>
    public void StartHost()
    {
        if (_networkManager == null) return;

        Debug.Log("Starting Host...");
        _networkManager.ServerManager.StartConnection();
        _networkManager.ClientManager.StartConnection();
    }

    /// <summary>
    /// Start as Server only
    /// </summary>
    public void StartServer()
    {
        if (_networkManager == null) return;

        Debug.Log("Starting Server...");
        _networkManager.ServerManager.StartConnection();
    }

    /// <summary>
    /// Start as Client and connect to server
    /// </summary>
    public void StartClient()
    {
        if (_networkManager == null) return;

        Debug.Log("Starting Client...");
        _networkManager.ClientManager.StartConnection();
    }

    /// <summary>
    /// Stop all connections
    /// </summary>
    public void StopConnection()
    {
        if (_networkManager == null) return;

        Debug.Log("Stopping connections...");
        
        if (_networkManager.ServerManager.Started)
            _networkManager.ServerManager.StopConnection(true);
        
        if (_networkManager.ClientManager.Started)
            _networkManager.ClientManager.StopConnection();
    }

    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        Debug.Log($"Server connection state: {args.ConnectionState}");
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        Debug.Log($"Client connection state: {args.ConnectionState}");
        
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            Debug.Log("Successfully connected to server!");
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            Debug.Log("Disconnected from server.");
        }
    }
}

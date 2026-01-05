using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Static reconnection manager that handles client reconnection attempts.
/// This survives MonoBehaviour destruction because it uses static state
/// and Unity's EditorApplication hooks.
/// </summary>
public static class ReconnectionManager
{
    // State
    private static bool _isWaitingForReconnect = false;
    private static float _reconnectStartTime;
    private static float _lastAttemptTime;
    private static float _gracePeriod = 120f;
    private static float _attemptInterval = 3f;
    private static string _serverAddress;
    private static ushort _serverPort;
    private static GameStateSnapshot _savedState;
    private static NetworkManager _networkManager;
    
    // Logging
    private static StreamWriter _log;
    private static bool _initialized = false;
    
    public static bool IsWaitingForReconnect => _isWaitingForReconnect;
    public static GameStateSnapshot SavedState => _savedState;
    
    /// <summary>
    /// Initialize the reconnection manager. Call this early in the game.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        
        InitLog();
        Log("ReconnectionManager initialized");
        
        // Hook into Unity's update loop
        Application.quitting += OnApplicationQuit;
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.update += EditorUpdate;
#endif
    }
    
    private static void InitLog()
    {
        if (_log != null) return;
        
        // Use Logs folder (not Docs) with .log extension - Unity ignores these for domain reload
        string dir = Application.isEditor 
            ? Path.Combine(Application.dataPath, "..", "Logs", "GameLogs")
            : Path.Combine(Application.dataPath, "..", "..", "Logs", "GameLogs");
        
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
            
        string filename = Application.isEditor ? "reconnect_manager_editor.log" : "reconnect_manager_build.log";
        string path = Path.Combine(dir, filename);
        
        _log = new StreamWriter(path, false);
        _log.AutoFlush = true;
        _log.WriteLine($"=== ReconnectionManager log started: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
    }
    
    private static void Log(string message)
    {
        InitLog();
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        _log?.WriteLine($"[{timestamp}] {message}");
    }
    
    /// <summary>
    /// Start waiting for reconnection after host disconnects.
    /// </summary>
    public static void StartReconnectionWait(NetworkManager networkManager, string address, ushort port, GameStateSnapshot savedState)
    {
        Log($"StartReconnectionWait called: address={address}:{port}, savedState={(savedState != null ? "exists" : "null")}");
        
        _networkManager = networkManager;
        _serverAddress = address;
        _serverPort = port;
        _savedState = savedState;
        _isWaitingForReconnect = true;
        _reconnectStartTime = Time.realtimeSinceStartup;
        _lastAttemptTime = _reconnectStartTime;
        
        Log($"Reconnection wait started. Grace period: {_gracePeriod}s, Attempt interval: {_attemptInterval}s");
    }
    
    /// <summary>
    /// Stop waiting for reconnection (e.g., when reconnected or grace period expired).
    /// </summary>
    public static void StopReconnectionWait(string reason)
    {
        Log($"StopReconnectionWait called: reason={reason}");
        _isWaitingForReconnect = false;
        _savedState = null;
    }
    
    /// <summary>
    /// Called when successfully reconnected. Clears the saved state.
    /// </summary>
    public static void OnReconnected()
    {
        Log("OnReconnected called - clearing saved state");
        _isWaitingForReconnect = false;
        // Keep _savedState until it's been used by PlayerConnectionHandler
    }
    
    /// <summary>
    /// Clear the saved state after it's been used.
    /// </summary>
    public static void ClearSavedState()
    {
        Log("ClearSavedState called");
        _savedState = null;
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Editor update loop - runs even when MonoBehaviours are destroyed.
    /// </summary>
    private static void EditorUpdate()
    {
        if (!_isWaitingForReconnect) return;
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            Log("EditorUpdate: Play mode stopped, canceling reconnection wait");
            _isWaitingForReconnect = false;
            return;
        }
        
        float currentTime = Time.realtimeSinceStartup;
        float elapsed = currentTime - _reconnectStartTime;
        
        // Check grace period
        if (elapsed >= _gracePeriod)
        {
            Log($"Grace period expired after {elapsed:F1}s");
            _isWaitingForReconnect = false;
            _savedState = null;
            // Note: UI update would need to happen via PlayerConnectionHandler if it still exists
            return;
        }
        
        // Attempt reconnection periodically
        if (currentTime - _lastAttemptTime >= _attemptInterval)
        {
            _lastAttemptTime = currentTime;
            AttemptReconnect();
        }
    }
#endif
    
    private static void AttemptReconnect()
    {
        if (string.IsNullOrEmpty(_serverAddress))
        {
            Log("AttemptReconnect: No server address, cannot reconnect");
            return;
        }
        
        // Get NetworkManager if we don't have it
        if (_networkManager == null)
        {
            _networkManager = InstanceFinder.NetworkManager;
            if (_networkManager == null)
            {
                Log("AttemptReconnect: NetworkManager not found!");
                return;
            }
        }
        
        // Check if already connected
        if (_networkManager.ClientManager.Started)
        {
            Log("AttemptReconnect: Already connected, stopping reconnection wait");
            _isWaitingForReconnect = false;
            return;
        }
        
        Log($"AttemptReconnect: Attempting to connect to {_serverAddress}:{_serverPort}");
        
        try
        {
            _networkManager.ClientManager.StartConnection(_serverAddress, _serverPort);
        }
        catch (Exception e)
        {
            Log($"AttemptReconnect: Exception: {e.Message}");
        }
    }
    
    private static void OnApplicationQuit()
    {
        Log("Application quitting");
        CloseLog();
    }
    
    private static void CloseLog()
    {
        if (_log != null)
        {
            _log.WriteLine($"=== Log ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            _log.Close();
            _log = null;
        }
    }
}

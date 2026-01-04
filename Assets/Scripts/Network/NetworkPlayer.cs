using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

/// <summary>
/// Represents a connected player in the network.
/// This is spawned for each player that connects.
/// Contains synced game state (health, mana) for this player.
/// </summary>
public class NetworkPlayer : NetworkBehaviour
{
    #region Synced Identity
    
    /// <summary>
    /// The player's connection ID (synced to all clients).
    /// </summary>
    public readonly SyncVar<int> PlayerId = new SyncVar<int>();

    /// <summary>
    /// Player's display name (synced to all clients).
    /// </summary>
    public readonly SyncVar<string> PlayerName = new SyncVar<string>();

    /// <summary>
    /// Is this player ready to start the game?
    /// </summary>
    public readonly SyncVar<bool> IsReady = new SyncVar<bool>();
    
    #endregion
    
    #region Synced Game State
    
    /// <summary>
    /// Player's current health.
    /// Note: No constructor default - must be set in OnStartServer() to ensure proper sync.
    /// </summary>
    public readonly SyncVar<int> CurrentHealth = new SyncVar<int>();
    
    /// <summary>
    /// Player's maximum health.
    /// </summary>
    public readonly SyncVar<int> MaxHealth = new SyncVar<int>();
    
    /// <summary>
    /// Player's current mana.
    /// </summary>
    public readonly SyncVar<int> CurrentMana = new SyncVar<int>();
    
    /// <summary>
    /// Player's maximum mana.
    /// </summary>
    public readonly SyncVar<int> MaxMana = new SyncVar<int>();
    
    #endregion
    
    #region Local References
    
    /// <summary>
    /// Reference to the local PlayerController this NetworkPlayer controls.
    /// Set by EncounterController when the game starts.
    /// </summary>
    [System.NonSerialized]
    public PlayerController LinkedPlayerController;
    
    /// <summary>
    /// Event fired when any game state changes (for UI updates).
    /// </summary>
    public System.Action OnStateChanged;
    
    #endregion

    private void Awake()
    {
        // Subscribe to sync callbacks
        CurrentHealth.OnChange += OnHealthChanged;
        MaxHealth.OnChange += OnMaxHealthChanged;
        CurrentMana.OnChange += OnManaChanged;
        MaxMana.OnChange += OnMaxManaChanged;
    }

    private void OnDestroy()
    {
        CurrentHealth.OnChange -= OnHealthChanged;
        MaxHealth.OnChange -= OnMaxHealthChanged;
        CurrentMana.OnChange -= OnManaChanged;
        MaxMana.OnChange -= OnMaxManaChanged;
    }

    // Store the player ID to be used during OnStartServer
    private int _pendingPlayerId = -1;

    /// <summary>
    /// Called by server to set the player ID before spawn.
    /// Actual initialization happens in OnStartServer.
    /// </summary>
    public void SetPlayerId(int playerId)
    {
        _pendingPlayerId = playerId;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        // Set identity values
        if (_pendingPlayerId >= 0)
        {
            PlayerId.Value = _pendingPlayerId;
            PlayerName.Value = $"Player {_pendingPlayerId}";
        }
        
        // DEBUG: Log before setting values
        Debug.Log($"[NetworkPlayer] OnStartServer BEFORE: Player {_pendingPlayerId} - CurrentHealth.Value={CurrentHealth.Value}");
        
        // MUST explicitly set game state values here - this marks them as dirty
        // so they get serialized to clients. Constructor defaults don't work because
        // FishNet only syncs values that have been "set" (marked dirty).
        CurrentHealth.Value = 20;
        MaxHealth.Value = 20;
        CurrentMana.Value = 1;
        MaxMana.Value = 1;
        
        // DEBUG: Log after setting values
        Debug.Log($"[NetworkPlayer] OnStartServer AFTER: Player {_pendingPlayerId} - CurrentHealth.Value={CurrentHealth.Value}");
        
        Debug.Log($"[NetworkPlayer] Initialized on server: {PlayerName.Value} (ID: {PlayerId.Value}) - Health: {CurrentHealth.Value}, Mana: {CurrentMana.Value}");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        // DEBUG: Log client-side values at start
        Debug.Log($"[NetworkPlayer] OnStartClient: {PlayerName.Value} (ID: {PlayerId.Value}) - Health={CurrentHealth.Value}, IsOwner={IsOwner}, IsServerStarted={base.IsServerStarted}");
        
        if (IsOwner)
        {
            Debug.Log($"[NetworkPlayer] You are: {PlayerName.Value} (ID: {PlayerId.Value})");
        }
        else
        {
            Debug.Log($"[NetworkPlayer] Other player joined: {PlayerName.Value} (ID: {PlayerId.Value})");
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        
        if (!IsOwner)
        {
            Debug.Log($"[NetworkPlayer] Player left: {PlayerName.Value} (ID: {PlayerId.Value})");
        }
    }

    /// <summary>
    /// Called by owning client to set ready state.
    /// </summary>
    [ServerRpc]
    public void SetReady(bool ready)
    {
        IsReady.Value = ready;
        Debug.Log($"[NetworkPlayer] {PlayerName.Value} ready: {IsReady.Value}");
    }
    
    #region SyncVar Callbacks
    
    private void OnHealthChanged(int prev, int next, bool asServer)
    {
        // On HOST, the callback 'next' parameter can be incorrect during OnStartCallback.
        // Use CurrentHealth.Value which is always correct.
        int actualHealth = CurrentHealth.Value;
        
        // Only log/process if there's a real change
        if (prev != actualHealth)
        {
            Debug.Log($"[NetworkPlayer] {PlayerName.Value} health: {prev} -> {actualHealth} (asServer: {asServer})");
        }
        
        UpdateLinkedController();
        OnStateChanged?.Invoke();
    }
    
    private void OnMaxHealthChanged(int prev, int next, bool asServer)
    {
        UpdateLinkedController();
        OnStateChanged?.Invoke();
    }
    
    private void OnManaChanged(int prev, int next, bool asServer)
    {
        // Only log if there's an actual change (not just initial sync with same value)
        if (prev != next)
        {
            Debug.Log($"[NetworkPlayer] {PlayerName.Value} mana: {prev} -> {next} (asServer: {asServer})");
        }
        UpdateLinkedController();
        OnStateChanged?.Invoke();
    }
    
    private void OnMaxManaChanged(int prev, int next, bool asServer)
    {
        UpdateLinkedController();
        OnStateChanged?.Invoke();
    }
    
    private void UpdateLinkedController()
    {
        if (LinkedPlayerController != null)
        {
            Debug.Log($"[NetworkPlayer] UpdateLinkedController: {PlayerName.Value} pushing Health={CurrentHealth.Value}, Mana={CurrentMana.Value} to {LinkedPlayerController.gameObject.name}");
            LinkedPlayerController.currentHealth = CurrentHealth.Value;
            LinkedPlayerController.maxHealth = MaxHealth.Value;
            LinkedPlayerController.currentMana = CurrentMana.Value;
            LinkedPlayerController.maxMana = MaxMana.Value;
            LinkedPlayerController.UpdatePlayerUI();
        }
    }
    
    #endregion
    
    #region Server RPCs (Client requests, Server validates)
    
    [ServerRpc(RequireOwnership = false)]
    public void CmdTakeDamage(int amount)
    {
        if (amount <= 0) return;
        
        int newHealth = Mathf.Max(0, CurrentHealth.Value - amount);
        CurrentHealth.Value = newHealth;
        
        Debug.Log($"[Server] {PlayerName.Value} took {amount} damage. Health: {newHealth}");
        
        if (newHealth <= 0)
        {
            Debug.Log($"[Server] {PlayerName.Value} has died!");
            // TODO: Game over logic
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void CmdHeal(int amount)
    {
        if (amount <= 0) return;
        
        int newHealth = Mathf.Min(MaxHealth.Value, CurrentHealth.Value + amount);
        CurrentHealth.Value = newHealth;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void CmdSpendMana(int amount)
    {
        if (amount <= 0) return;
        
        if (CurrentMana.Value >= amount)
        {
            CurrentMana.Value -= amount;
            Debug.Log($"[Server] {PlayerName.Value} spent {amount} mana. Remaining: {CurrentMana.Value}");
        }
        else
        {
            Debug.LogWarning($"[Server] {PlayerName.Value} doesn't have enough mana! Has {CurrentMana.Value}, needs {amount}");
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void CmdRefillMana()
    {
        CurrentMana.Value = MaxMana.Value;
        Debug.Log($"[Server] {PlayerName.Value} mana refilled to {MaxMana.Value}");
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void CmdIncreaseMaxMana(int amount)
    {
        if (amount <= 0) return;
        
        MaxMana.Value += amount;
        CurrentMana.Value = MaxMana.Value;
        Debug.Log($"[Server] {PlayerName.Value} max mana increased to {MaxMana.Value}");
    }
    
    #endregion
    
    #region Helper Methods
    
    public bool HasEnoughMana(int cost) => CurrentMana.Value >= cost;
    public bool IsAlive => CurrentHealth.Value > 0;
    
    /// <summary>
    /// Link this NetworkPlayer to a local PlayerController for UI updates.
    /// </summary>
    public void LinkToPlayerController(PlayerController controller)
    {
        LinkedPlayerController = controller;
        UpdateLinkedController();
        Debug.Log($"[NetworkPlayer] {PlayerName.Value} linked to {controller.gameObject.name}");
    }
    
    #endregion
}

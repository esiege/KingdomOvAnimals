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
    /// Initial value set to 20 so both server and client start with same expectation.
    /// </summary>
    public readonly SyncVar<int> CurrentHealth = new SyncVar<int>(20);
    
    /// <summary>
    /// Player's maximum health.
    /// </summary>
    public readonly SyncVar<int> MaxHealth = new SyncVar<int>(20);
    
    /// <summary>
    /// Player's current mana.
    /// </summary>
    public readonly SyncVar<int> CurrentMana = new SyncVar<int>(1);
    
    /// <summary>
    /// Player's maximum mana.
    /// </summary>
    public readonly SyncVar<int> MaxMana = new SyncVar<int>(1);
    
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
        
        // Initial values (20 health, 1 mana) are set in SyncVar constructors.
        // ForceUpdateLinkedController() handles pushing these to the UI.
        
        Debug.Log($"[NetworkPlayer] Spawned: {PlayerName.Value} (ID: {PlayerId.Value})");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
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
        if (prev != next)
        {
            Debug.Log($"[NetworkPlayer] {PlayerName.Value} health: {prev} -> {next} (asServer: {asServer})");
        }
        
        // Use next parameter (the new value from callback) for UI updates
        if (LinkedPlayerController != null && next != prev)
        {
            LinkedPlayerController.currentHealth = next;
            LinkedPlayerController.UpdatePlayerUI();
        }
        OnStateChanged?.Invoke();
    }
    
    private void OnMaxHealthChanged(int prev, int next, bool asServer)
    {
        if (LinkedPlayerController != null && next != prev)
        {
            LinkedPlayerController.maxHealth = next;
            LinkedPlayerController.UpdatePlayerUI();
        }
        OnStateChanged?.Invoke();
    }
    
    private void OnManaChanged(int prev, int next, bool asServer)
    {
        if (prev != next)
        {
            Debug.Log($"[NetworkPlayer] {PlayerName.Value} mana: {prev} -> {next} (asServer: {asServer})");
        }
        if (LinkedPlayerController != null && next != prev)
        {
            LinkedPlayerController.currentMana = next;
            LinkedPlayerController.UpdatePlayerUI();
        }
        OnStateChanged?.Invoke();
    }
    
    private void OnMaxManaChanged(int prev, int next, bool asServer)
    {
        if (LinkedPlayerController != null && next != prev)
        {
            LinkedPlayerController.maxMana = next;
            LinkedPlayerController.UpdatePlayerUI();
        }
        OnStateChanged?.Invoke();
    }
    
    private void UpdateLinkedController()
    {
        // Don't call this directly - use UpdateLinkedControllerWithValues instead
    }
    
    private void UpdateLinkedControllerWithValues(int health, int maxHealth, int mana, int maxMana)
    {
        if (LinkedPlayerController != null)
        {
            Debug.Log($"[NetworkPlayer] UpdateLinkedController: {PlayerName.Value} pushing Health={health}, Mana={mana} to {LinkedPlayerController.gameObject.name}");
            LinkedPlayerController.currentHealth = health;
            LinkedPlayerController.maxHealth = maxHealth;
            LinkedPlayerController.currentMana = mana;
            LinkedPlayerController.maxMana = maxMana;
            LinkedPlayerController.UpdatePlayerUI();
        }
    }
    
    /// <summary>
    /// Force push current values to linked controller. 
    /// Call this after linking to ensure UI is up to date.
    /// </summary>
    public void ForceUpdateLinkedController()
    {
        // Use constructor default values (20, 20, 1, 1) since SyncVar sync is broken
        int health = CurrentHealth.Value > 0 ? CurrentHealth.Value : 20;
        int maxHealth = MaxHealth.Value > 0 ? MaxHealth.Value : 20;
        int mana = CurrentMana.Value;
        int maxMana = MaxMana.Value > 0 ? MaxMana.Value : 1;
        
        UpdateLinkedControllerWithValues(health, maxHealth, mana, maxMana);
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
        ForceUpdateLinkedController();  // Use forced values since SyncVar sync is unreliable
        Debug.Log($"[NetworkPlayer] {PlayerName.Value} linked to {controller.gameObject.name}");
    }
    
    #endregion
}

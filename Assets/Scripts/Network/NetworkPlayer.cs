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
        PlayerId.OnChange += OnPlayerIdChanged;
        CurrentHealth.OnChange += OnHealthChanged;
        MaxHealth.OnChange += OnMaxHealthChanged;
        CurrentMana.OnChange += OnManaChanged;
        MaxMana.OnChange += OnMaxManaChanged;
    }

    private void OnDestroy()
    {
        PlayerId.OnChange -= OnPlayerIdChanged;
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
        
        // Use FishNet's Owner.ClientId as the canonical player ID
        // This ensures consistency between server and client
        PlayerId.Value = Owner.ClientId;
        PlayerName.Value = $"Player {Owner.ClientId}";
        Debug.Log($"[NetworkPlayer] Server: PlayerId set to Owner.ClientId={Owner.ClientId}");
        
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
    
    private void OnPlayerIdChanged(int prev, int next, bool asServer)
    {
        Debug.Log($"[NetworkPlayer] PlayerId changed: {prev} -> {next} (asServer: {asServer}, IsOwner: {IsOwner})");
    }
    
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
            
            // If this is the local player and it's their turn, refresh playable cards
            if (IsOwner)
            {
                var encounterController = GameObject.FindObjectOfType<EncounterController>();
                if (encounterController != null && encounterController.currentPlayer == LinkedPlayerController)
                {
                    encounterController.playerHandController.HidePlayableHand();
                    encounterController.playerHandController.VisualizePlayableHand();
                }
            }
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
        Debug.Log($"[Server] CmdIncreaseMaxMana received for {PlayerName.Value}, amount: {amount}");
        if (amount <= 0) return;
        
        MaxMana.Value += amount;
        CurrentMana.Value = MaxMana.Value;
        Debug.Log($"[Server] {PlayerName.Value} max mana increased to {MaxMana.Value}");
    }
    
    /// <summary>
    /// Client requests to play a card from hand to a board slot.
    /// </summary>
    /// <param name="cardIndex">Index of the card in the player's hand</param>
    /// <param name="slotIndex">Board slot index (0, 1, or 2)</param>
    [ServerRpc]
    public void CmdPlayCard(int cardIndex, int slotIndex)
    {
        // Validate turn - must be this player's turn (compare ObjectIds)
        var gameManager = NetworkGameManager.Instance;
        if (gameManager == null || gameManager.CurrentTurnObjectId.Value != ObjectId)
        {
            Debug.LogWarning($"[Server] {PlayerName.Value} tried to play card but it's not their turn! (CurrentTurn={gameManager?.CurrentTurnObjectId.Value}, MyObjectId={ObjectId})");
            return;
        }
        
        // Get the card from hand
        if (LinkedPlayerController == null)
        {
            Debug.LogError($"[Server] {PlayerName.Value} has no LinkedPlayerController!");
            return;
        }
        
        var handController = GetHandController();
        if (handController == null)
        {
            Debug.LogError($"[Server] Could not find HandController for {PlayerName.Value}");
            return;
        }
        
        var hand = handController.GetHand();
        if (cardIndex < 0 || cardIndex >= hand.Count)
        {
            Debug.LogWarning($"[Server] Invalid card index {cardIndex} for hand size {hand.Count}");
            return;
        }
        
        var card = hand[cardIndex];
        
        // Validate mana
        if (CurrentMana.Value < card.manaCost)
        {
            Debug.LogWarning($"[Server] {PlayerName.Value} doesn't have enough mana for {card.cardName}. Has {CurrentMana.Value}, needs {card.manaCost}");
            return;
        }
        
        // Validate slot
        string slotName = $"PlayerSlot-{slotIndex + 1}";
        if (LinkedPlayerController.gameObject.name == "Opponent")
        {
            slotName = $"OpponentSlot-{slotIndex + 1}";
        }
        
        var slot = GameObject.Find(slotName);
        if (slot == null)
        {
            Debug.LogError($"[Server] Could not find slot {slotName}");
            return;
        }
        
        if (slot.GetComponentInChildren<CardController>() != null)
        {
            Debug.LogWarning($"[Server] Slot {slotName} is already occupied!");
            return;
        }
        
        // All validations passed - execute the play
        Debug.Log($"[Server] {PlayerName.Value} playing {card.cardName} to slot {slotIndex}");
        
        // Deduct mana (SyncVar will sync to clients)
        CurrentMana.Value -= card.manaCost;
        
        // Broadcast to all clients to execute the card play
        RpcExecuteCardPlay(cardIndex, slotIndex, card.cardName);
    }
    
    /// <summary>
    /// Called on all clients to execute a card play.
    /// </summary>
    [ObserversRpc]
    private void RpcExecuteCardPlay(int cardIndex, int slotIndex, string cardName)
    {
        Debug.Log($"[Client] Executing card play: {cardName} to slot {slotIndex} for {PlayerName.Value}");
        
        // Find the hand controller for this player
        var handController = GetHandController();
        if (handController == null)
        {
            Debug.LogError($"[Client] Could not find HandController for {PlayerName.Value}");
            return;
        }
        
        var hand = handController.GetHand();
        if (cardIndex < 0 || cardIndex >= hand.Count)
        {
            Debug.LogWarning($"[Client] Card index {cardIndex} out of range for hand size {hand.Count}");
            return;
        }
        
        var card = hand[cardIndex];
        
        // Find the slot
        string slotName = $"PlayerSlot-{slotIndex + 1}";
        if (LinkedPlayerController != null && LinkedPlayerController.gameObject.name == "Opponent")
        {
            slotName = $"OpponentSlot-{slotIndex + 1}";
        }
        
        var slot = GameObject.Find(slotName);
        if (slot == null)
        {
            Debug.LogError($"[Client] Could not find slot {slotName}");
            return;
        }
        
        // Execute the card play locally
        ExecuteLocalCardPlay(card, slot, handController);
    }
    
    /// <summary>
    /// Executes the card play on the local client.
    /// </summary>
    private void ExecuteLocalCardPlay(CardController card, GameObject slot, HandController handController)
    {
        // Move card to slot
        card.transform.SetParent(slot.transform);
        card.transform.position = slot.transform.position;
        card.transform.localPosition = Vector3.zero;
        
        // Update card state
        card.isActive = false;
        card.isInHand = false;
        card.SetSummoningSickness(true);
        card.UnflipCard();
        card.EnterPlay();
        
        // Remove from hand
        handController.RemoveCardFromHand(card.id);
        
        // Add to board
        if (LinkedPlayerController != null)
        {
            LinkedPlayerController.AddCardToBoard(card);
        }
        
        Debug.Log($"[Client] {card.cardName} played to {slot.name}");
    }
    
    /// <summary>
    /// Gets the HandController associated with this player.
    /// </summary>
    private HandController GetHandController()
    {
        var encounterController = GameObject.FindObjectOfType<EncounterController>();
        if (encounterController == null) return null;
        
        // Determine which hand controller based on whether this is the local player
        if (LinkedPlayerController == encounterController.player)
        {
            return encounterController.playerHandController;
        }
        else if (LinkedPlayerController == encounterController.opponent)
        {
            return encounterController.opponentHandController;
        }
        
        return null;
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

using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;

// FishNet code regeneration trigger - do not remove
// Last regenerated: 2026-01-04

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
    
    // Store pending state for reconnection (applied in OnStartServer)
    private DisconnectedPlayerState _pendingState = null;

    /// <summary>
    /// Called by server to set the player ID before spawn.
    /// Actual initialization happens in OnStartServer.
    /// </summary>
    public void SetPlayerId(int playerId)
    {
        _pendingPlayerId = playerId;
    }
    
    /// <summary>
    /// Set the state to be restored when the NetworkPlayer spawns.
    /// Must be called BEFORE spawn.
    /// </summary>
    public void SetPendingState(DisconnectedPlayerState state)
    {
        _pendingState = state;
        if (state != null)
        {
            _pendingPlayerId = state.playerId;
            Debug.Log($"[NetworkPlayer] Pending state set for player {state.playerId}: HP={state.health}, Mana={state.mana}");
        }
    }
    
    /// <summary>
    /// Restore SyncVar state from saved DisconnectedPlayerState.
    /// Called AFTER spawn so SyncVars sync to clients properly.
    /// Card restoration happens separately via NetworkGameManager.
    /// </summary>
    [Obsolete("Use SetPendingState before spawn instead")]
    public void RestoreFromState(DisconnectedPlayerState state)
    {
        if (state == null)
        {
            Debug.LogWarning("[NetworkPlayer] RestoreFromState called with null state!");
            return;
        }
        
        Debug.Log($"[NetworkPlayer] Restoring state for player {state.playerId}: HP={state.health}/{state.maxHealth}, Mana={state.mana}/{state.maxMana}");
        
        // Restore all SyncVars - these will sync to clients since we're spawned
        PlayerId.Value = state.playerId;
        PlayerName.Value = state.playerName;
        CurrentHealth.Value = state.health;
        MaxHealth.Value = state.maxHealth;
        CurrentMana.Value = state.mana;
        MaxMana.Value = state.maxMana;
        
        Debug.Log($"[NetworkPlayer] State restored! PlayerId={PlayerId.Value}, HP={CurrentHealth.Value}, Mana={CurrentMana.Value}");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        // Check if this is a reconnection with pending state
        if (_pendingState != null)
        {
            // Reconnection - restore ALL state from pending state
            PlayerId.Value = _pendingState.playerId;
            PlayerName.Value = _pendingState.playerName;
            CurrentHealth.Value = _pendingState.health;
            MaxHealth.Value = _pendingState.maxHealth;
            CurrentMana.Value = _pendingState.mana;
            MaxMana.Value = _pendingState.maxMana;
            
            Debug.Log($"[NetworkPlayer] Server (reconnect): Restored state for player {_pendingState.playerId}: HP={_pendingState.health}/{_pendingState.maxHealth}, Mana={_pendingState.mana}/{_pendingState.maxMana}");
            
            // Clear pending state
            _pendingState = null;
            _pendingPlayerId = -1;
        }
        else if (_pendingPlayerId >= 0)
        {
            // Reconnection without full state - just use the preserved player ID
            PlayerId.Value = _pendingPlayerId;
            PlayerName.Value = $"Player {_pendingPlayerId}";
            Debug.Log($"[NetworkPlayer] Server (reconnect): PlayerId set to preserved ID={_pendingPlayerId}");
            _pendingPlayerId = -1;
        }
        else
        {
            // New player - use FishNet's Owner.ClientId as the canonical player ID
            PlayerId.Value = Owner.ClientId;
            PlayerName.Value = $"Player {Owner.ClientId}";
            Debug.Log($"[NetworkPlayer] Server: PlayerId set to Owner.ClientId={Owner.ClientId}");
        }
        
        Debug.Log($"[NetworkPlayer] Spawned: {PlayerName.Value} (ID: {PlayerId.Value}), HP={CurrentHealth.Value}, Mana={CurrentMana.Value}");
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
    
    /// <summary>
    /// Called when ownership changes (used for reconnection).
    /// </summary>
    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);
        
        if (IsOwner)
        {
            Debug.Log($"[NetworkPlayer] Ownership gained! You are now: {PlayerName.Value} (ID: {PlayerId.Value})");
            
            // Re-register with NetworkGameManager (for reconnection)
            if (NetworkGameManager.Instance != null)
            {
                NetworkGameManager.Instance.RegisterNetworkPlayer(this);
            }
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
        
        // Ignore bogus health=0 callbacks during initial sync (FishNet timing issue)
        // ForceUpdateLinkedController sets the correct initial value, don't let sync overwrite it
        if (next == 0 && !asServer)
        {
            Debug.Log($"[NetworkPlayer] Ignoring client-side health=0 callback");
            return;
        }
        
        // Apply valid health changes to UI
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
        // For opponent cards, mirror the slot index so front/back perspective is correct
        // Slot 1 (front) <-> Slot 3 (back) swap for opponent view
        int displaySlotIndex = slotIndex;
        string slotName = $"PlayerSlot-{slotIndex + 1}";
        if (LinkedPlayerController != null && LinkedPlayerController.gameObject.name == "Opponent")
        {
            // Mirror slots: 0->2, 1->1, 2->0 (slot 1 stays, slots 0 and 2 swap)
            if (slotIndex == 0) displaySlotIndex = 2;
            else if (slotIndex == 2) displaySlotIndex = 0;
            // slotIndex 1 (middle) stays the same
            
            slotName = $"OpponentSlot-{displaySlotIndex + 1}";
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
        
        // Refresh hand highlighting (clear stuck highlights)
        handController.VisualizePlayableHand();
        
        Debug.Log($"[Client] {card.cardName} played to {slot.name}");
    }
    
    #region Card Draw (Network Synced)
    
    /// <summary>
    /// Server triggers a card draw for this player.
    /// Called by NetworkGameManager when a turn starts.
    /// </summary>
    [Server]
    public void ServerDrawCard()
    {
        if (LinkedPlayerController == null || LinkedPlayerController.deck == null)
        {
            Debug.LogWarning($"[Server] Cannot draw card - LinkedPlayerController or deck is null");
            return;
        }
        
        if (LinkedPlayerController.deck.Count == 0)
        {
            Debug.Log($"[Server] {PlayerName.Value} has no cards to draw");
            return;
        }
        
        // Get the card name that will be drawn (top of deck)
        var topCard = LinkedPlayerController.deck[0];
        string cardName = topCard.cardName;
        
        Debug.Log($"[Server] {PlayerName.Value} drawing card: {cardName}");
        
        // Broadcast to all clients
        RpcExecuteCardDraw(cardName);
    }
    
    /// <summary>
    /// Called on all clients to execute a card draw.
    /// NOTE: BufferLast is NOT used because reconnecting clients get their hand state
    /// from the state restoration snapshot, not from buffered RPCs.
    /// </summary>
    [ObserversRpc]
    private void RpcExecuteCardDraw(string cardName)
    {
        Debug.Log($"[Client] RpcExecuteCardDraw: {PlayerName.Value} draws {cardName}");
        
        var encounterController = FindObjectOfType<EncounterController>();
        if (encounterController == null)
        {
            Debug.LogWarning("[Client] RpcExecuteCardDraw: EncounterController not found (may be in state restoration)");
            return;
        }
        
        var handController = GetHandController();
        if (handController == null)
        {
            Debug.LogWarning($"[Client] RpcExecuteCardDraw: HandController not found for {PlayerName.Value} (may be in state restoration)");
            return;
        }
        
        if (LinkedPlayerController == null || LinkedPlayerController.deck == null)
        {
            Debug.LogWarning($"[Client] RpcExecuteCardDraw: LinkedPlayerController or deck is null (may be in state restoration)");
            return;
        }
        
        // Check if deck has cards
        if (LinkedPlayerController.deck.Count == 0)
        {
            Debug.Log($"[Client] {PlayerName.Value} has no cards to draw (deck empty)");
            return;
        }
        
        // Check hand size limit
        if (handController.GetHand().Count >= encounterController.maxHandSize)
        {
            Debug.Log($"[Client] {PlayerName.Value} cannot draw - hand is full");
            return;
        }
        
        // Draw the top card from deck
        CardController drawnCard = LinkedPlayerController.deck[0];
        LinkedPlayerController.deck.RemoveAt(0);
        
        // Verify it's the expected card (sanity check)
        if (drawnCard.cardName != cardName)
        {
            Debug.LogWarning($"[Client] Card mismatch! Expected {cardName}, got {drawnCard.cardName}. Decks may be out of sync.");
        }
        
        // Set ownership and add to hand
        drawnCard.owningPlayer = LinkedPlayerController;
        handController.AddCardToHand(drawnCard);
        
        Debug.Log($"[Client] {PlayerName.Value} drew {drawnCard.cardName}. Deck remaining: {LinkedPlayerController.deck.Count}");
    }
    
    #endregion

    /// <summary>
    /// Gets the HandController associated with this player.
    /// </summary>
    public HandController GetHandController()
    {
        var encounterController = GameObject.FindObjectOfType<EncounterController>();
        if (encounterController == null)
        {
            Debug.LogWarning($"[NetworkPlayer] GetHandController: EncounterController is null!");
            return null;
        }
        
        // Determine which hand controller based on whether this is the local player
        if (LinkedPlayerController == null)
        {
            Debug.LogWarning($"[NetworkPlayer] GetHandController: LinkedPlayerController is null for {PlayerName.Value}!");
            return null;
        }
        
        if (LinkedPlayerController == encounterController.player)
        {
            return encounterController.playerHandController;
        }
        else if (LinkedPlayerController == encounterController.opponent)
        {
            return encounterController.opponentHandController;
        }
        
        Debug.LogWarning($"[NetworkPlayer] GetHandController: LinkedPlayerController ({LinkedPlayerController.name}) doesn't match player ({encounterController.player?.name}) or opponent ({encounterController.opponent?.name})");
        return null;
    }
    
    #endregion
    
    #region Reconnection Support
    
    /// <summary>
    /// Client sends saved game state to host for restoration after host reconnects.
    /// </summary>
    [ServerRpc]
    public void ServerRestoreGameState(string gameStateJson)
    {
        Debug.Log($"[Server] Received game state for restoration from Player {PlayerId.Value}");
        
        GameStateSnapshot snapshot = GameStateSnapshot.FromJson(gameStateJson);
        if (snapshot == null)
        {
            Debug.LogError("[Server] Failed to deserialize game state!");
            return;
        }
        
        // Restore game state via NetworkGameManager
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.ServerRestoreGameState(snapshot);
        }
    }
    
    #endregion
    
    #region Combat/Ability RPCs
    
    /// <summary>
    /// Client requests to use an ability from a card on the board targeting another card.
    /// Uses slot names for card identification since card IDs aren't synced.
    /// </summary>
    /// <param name="attackerSlotName">Name of the slot containing the attacking card (e.g., "PlayerSlot-1")</param>
    /// <param name="targetSlotName">Name of the slot containing the target card (e.g., "OpponentSlot-2")</param>
    /// <param name="isOffensive">True for offensive ability, false for defensive/support</param>
    [ServerRpc]
    public void CmdUseAbilityOnCard(string attackerSlotName, string targetSlotName, bool isOffensive)
    {
        // Validate turn
        var gameManager = NetworkGameManager.Instance;
        if (gameManager == null || gameManager.CurrentTurnObjectId.Value != ObjectId)
        {
            Debug.LogWarning($"[Server] {PlayerName.Value} tried to use ability but it's not their turn!");
            return;
        }
        
        // Translate client slot names to server perspective
        string serverAttackerSlot = TranslateClientSlotToServer(attackerSlotName, PlayerId.Value);
        string serverTargetSlot = TranslateClientSlotToServer(targetSlotName, PlayerId.Value);
        Debug.Log($"[Server] CmdUseAbilityOnCard: client sent attacker='{attackerSlotName}', target='{targetSlotName}'; server resolved to attacker='{serverAttackerSlot}', target='{serverTargetSlot}' for Player {PlayerId.Value}");
        
        // Find attacker card by slot (using server perspective)
        CardController attacker = FindCardInSlot(serverAttackerSlot);
        if (attacker == null)
        {
            Debug.LogWarning($"[Server] Could not find attacker card in slot {serverAttackerSlot} (client: {attackerSlotName})");
            return;
        }
        
        // Validate attacker belongs to this player
        if (attacker.owningPlayer != LinkedPlayerController)
        {
            Debug.LogWarning($"[Server] {PlayerName.Value} tried to use a card they don't own! Card owner: {attacker.owningPlayer?.name}, Expected: {LinkedPlayerController?.name}");
            return;
        }
        
        // Validate attacker can act (not tapped, no summoning sickness)
        if (attacker.isTapped)
        {
            Debug.LogWarning($"[Server] {attacker.cardName} is already tapped!");
            return;
        }
        
        if (attacker.hasSummoningSickness)
        {
            Debug.LogWarning($"[Server] {attacker.cardName} has summoning sickness!");
            return;
        }
        
        // Find target card by slot (using server perspective)
        CardController target = FindCardInSlot(serverTargetSlot);
        if (target == null)
        {
            Debug.LogWarning($"[Server] Could not find target card in slot {serverTargetSlot} (client: {targetSlotName})");
            return;
        }
        
        // Get owner IDs and slot indices for perspective-independent RPC
        int attackerOwnerId = attacker.owningPlayer?.networkPlayer?.PlayerId.Value ?? -1;
        int attackerSlotIndex = GetSlotIndex(serverAttackerSlot);
        int targetOwnerId = target.owningPlayer?.networkPlayer?.PlayerId.Value ?? -1;
        int targetSlotIndex = GetSlotIndex(serverTargetSlot);
        
        // Get the ability and damage amount
        GameObject abilityObj = isOffensive ? attacker.offensiveAbility : attacker.supportAbility;
        if (abilityObj == null)
        {
            Debug.LogWarning($"[Server] {attacker.cardName} has no {(isOffensive ? "offensive" : "support")} ability!");
            return;
        }
        
        DamageAbility damageAbility = abilityObj.GetComponentInChildren<DamageAbility>();
        int damageAmount = damageAbility != null ? damageAbility.damageAmount : 0;
        
        Debug.Log($"[Server] {attacker.cardName} uses ability on {target.cardName} for {damageAmount} damage");
        
        // Broadcast ability use to all clients with owner IDs for correct perspective
        RpcExecuteAbilityOnCard(attackerOwnerId, attackerSlotIndex, targetOwnerId, targetSlotIndex, isOffensive, damageAmount);
    }
    
    /// <summary>
    /// Client requests to use a flip ability (card from hand) targeting a card on the board.
    /// </summary>
    /// <param name="handIndex">Index of the card in the player's hand</param>
    /// <param name="targetSlotName">Name of the slot containing the target card</param>
    /// <param name="isOffensive">True for offensive ability, false for defensive/support</param>
    [ServerRpc]
    public void CmdUseFlipAbilityOnCard(int handIndex, string targetSlotName, bool isOffensive)
    {
        // Validate turn
        var gameManager = NetworkGameManager.Instance;
        if (gameManager == null || gameManager.CurrentTurnObjectId.Value != ObjectId)
        {
            Debug.LogWarning($"[Server] {PlayerName.Value} tried to use flip ability but it's not their turn!");
            return;
        }
        
        // Get the hand controller and card
        var handController = GetHandController();
        if (handController == null)
        {
            Debug.LogError($"[Server] Could not find HandController for {PlayerName.Value}");
            return;
        }
        
        var hand = handController.GetHand();
        if (handIndex < 0 || handIndex >= hand.Count)
        {
            Debug.LogWarning($"[Server] Invalid hand index {handIndex} for hand size {hand.Count}");
            return;
        }
        
        var attacker = hand[handIndex];
        
        // Validate mana
        if (CurrentMana.Value < attacker.manaCost)
        {
            Debug.LogWarning($"[Server] {PlayerName.Value} doesn't have enough mana for {attacker.cardName}. Has {CurrentMana.Value}, needs {attacker.manaCost}");
            return;
        }
        
        // Validate card not already flipped
        if (attacker.isFlipped)
        {
            Debug.LogWarning($"[Server] {attacker.cardName} is already flipped!");
            return;
        }
        
        // Translate client target slot name to server perspective
        string serverTargetSlot = TranslateClientSlotToServer(targetSlotName, PlayerId.Value);
        Debug.Log($"[Server] CmdUseFlipAbilityOnCard: client sent target='{targetSlotName}', server resolved to '{serverTargetSlot}' for Player {PlayerId.Value}");
        
        // Find target card by slot and get owner info
        CardController target = FindCardInSlot(serverTargetSlot);
        if (target == null)
        {
            Debug.LogWarning($"[Server] Could not find target card in slot {serverTargetSlot} (client: {targetSlotName})");
            return;
        }
        
        // Get target owner's player ID and slot index for perspective-independent RPC
        int targetOwnerId = target.owningPlayer?.networkPlayer?.PlayerId.Value ?? -1;
        int targetSlotIndex = GetSlotIndex(serverTargetSlot);
        
        // Get the ability and damage amount
        GameObject abilityObj = isOffensive ? attacker.offensiveAbility : attacker.supportAbility;
        if (abilityObj == null)
        {
            Debug.LogWarning($"[Server] {attacker.cardName} has no {(isOffensive ? "offensive" : "support")} ability!");
            return;
        }
        
        DamageAbility damageAbility = abilityObj.GetComponentInChildren<DamageAbility>();
        int damageAmount = damageAbility != null ? damageAbility.damageAmount : 0;
        
        // Deduct mana
        CurrentMana.Value -= attacker.manaCost;
        
        Debug.Log($"[Server] {attacker.cardName} (flip) uses ability on {target.cardName} for {damageAmount} damage");
        
        // Broadcast flip ability use to all clients with owner ID for correct perspective
        RpcExecuteFlipAbilityOnCard(handIndex, targetOwnerId, targetSlotIndex, isOffensive, damageAmount);
    }
    
    /// <summary>
    /// Client requests to use a flip ability (card from hand) targeting a player.
    /// </summary>
    [ServerRpc]
    public void CmdUseFlipAbilityOnPlayer(int handIndex, int targetPlayerId, bool isOffensive)
    {
        // Validate turn
        var gameManager = NetworkGameManager.Instance;
        if (gameManager == null || gameManager.CurrentTurnObjectId.Value != ObjectId)
        {
            Debug.LogWarning($"[Server] {PlayerName.Value} tried to use flip ability but it's not their turn!");
            return;
        }
        
        // Get the hand controller and card
        var handController = GetHandController();
        if (handController == null)
        {
            Debug.LogError($"[Server] Could not find HandController for {PlayerName.Value}");
            return;
        }
        
        var hand = handController.GetHand();
        if (handIndex < 0 || handIndex >= hand.Count)
        {
            Debug.LogWarning($"[Server] Invalid hand index {handIndex} for hand size {hand.Count}");
            return;
        }
        
        var attacker = hand[handIndex];
        
        // Validate mana
        if (CurrentMana.Value < attacker.manaCost)
        {
            Debug.LogWarning($"[Server] {PlayerName.Value} doesn't have enough mana for {attacker.cardName}. Has {CurrentMana.Value}, needs {attacker.manaCost}");
            return;
        }
        
        // Validate card not already flipped
        if (attacker.isFlipped)
        {
            Debug.LogWarning($"[Server] {attacker.cardName} is already flipped!");
            return;
        }
        
        // Get the ability and damage amount
        GameObject abilityObj = isOffensive ? attacker.offensiveAbility : attacker.supportAbility;
        if (abilityObj == null)
        {
            Debug.LogWarning($"[Server] {attacker.cardName} has no {(isOffensive ? "offensive" : "support")} ability!");
            return;
        }
        
        DamageAbility damageAbility = abilityObj.GetComponentInChildren<DamageAbility>();
        int damageAmount = damageAbility != null ? damageAbility.damageAmount : 0;
        
        // Deduct mana
        CurrentMana.Value -= attacker.manaCost;
        
        Debug.Log($"[Server] {attacker.cardName} (flip) uses ability on Player {targetPlayerId} for {damageAmount} damage");
        
        // Apply damage to target player via their NetworkPlayer
        NetworkPlayer targetNetworkPlayer = FindNetworkPlayerById(targetPlayerId);
        if (targetNetworkPlayer != null && damageAmount > 0)
        {
            targetNetworkPlayer.CurrentHealth.Value -= damageAmount;
        }
        
        // Broadcast flip ability use to all clients
        RpcExecuteFlipAbilityOnPlayer(handIndex, targetPlayerId, isOffensive, damageAmount);
    }
    
    /// <summary>
    /// Client requests to use an ability from a card targeting a player.
    /// </summary>
    [ServerRpc]
    public void CmdUseAbilityOnPlayer(string attackerSlotName, int targetPlayerId, bool isOffensive)
    {
        // Validate turn
        var gameManager = NetworkGameManager.Instance;
        if (gameManager == null || gameManager.CurrentTurnObjectId.Value != ObjectId)
        {
            Debug.LogWarning($"[Server] {PlayerName.Value} tried to use ability but it's not their turn!");
            return;
        }
        
        // Translate client slot name to server perspective
        string serverSlotName = TranslateClientSlotToServer(attackerSlotName, PlayerId.Value);
        Debug.Log($"[Server] CmdUseAbilityOnPlayer: client sent '{attackerSlotName}', server resolved to '{serverSlotName}' for Player {PlayerId.Value}");
        
        // Find attacker card by slot (using server perspective)
        CardController attacker = FindCardInSlot(serverSlotName);
        if (attacker == null)
        {
            Debug.LogWarning($"[Server] Could not find attacker card in slot {serverSlotName} (client: {attackerSlotName})");
            return;
        }
        
        // Validate attacker belongs to this player
        if (attacker.owningPlayer != LinkedPlayerController)
        {
            Debug.LogWarning($"[Server] {PlayerName.Value} tried to use a card they don't own! Card owner: {attacker.owningPlayer?.name}, Expected: {LinkedPlayerController?.name}");
            return;
        }
        
        // Validate attacker can act
        if (attacker.isTapped)
        {
            Debug.LogWarning($"[Server] {attacker.cardName} is already tapped!");
            return;
        }
        
        if (attacker.hasSummoningSickness)
        {
            Debug.LogWarning($"[Server] {attacker.cardName} has summoning sickness!");
            return;
        }
        
        // Get the ability and damage amount
        GameObject abilityObj = isOffensive ? attacker.offensiveAbility : attacker.supportAbility;
        if (abilityObj == null)
        {
            Debug.LogWarning($"[Server] {attacker.cardName} has no {(isOffensive ? "offensive" : "support")} ability!");
            return;
        }
        
        DamageAbility damageAbility = abilityObj.GetComponentInChildren<DamageAbility>();
        int damageAmount = damageAbility != null ? damageAbility.damageAmount : 0;
        
        Debug.Log($"[Server] {attacker.cardName} uses ability on Player {targetPlayerId} for {damageAmount} damage");
        
        // Get attacker owner ID and slot index for perspective-independent RPC
        int attackerOwnerId = attacker.owningPlayer?.networkPlayer?.PlayerId.Value ?? -1;
        int attackerSlotIndex = GetSlotIndex(serverSlotName);
        
        // Apply damage to target player via their NetworkPlayer
        NetworkPlayer targetNetworkPlayer = FindNetworkPlayerById(targetPlayerId);
        if (targetNetworkPlayer != null && damageAmount > 0)
        {
            targetNetworkPlayer.CurrentHealth.Value -= damageAmount;
        }
        
        // Broadcast ability use to all clients with owner ID for correct perspective
        RpcExecuteAbilityOnPlayer(attackerOwnerId, attackerSlotIndex, targetPlayerId, isOffensive, damageAmount);
    }
    
    /// <summary>
    /// Broadcast to all clients to execute an ability on a card target.
    /// Uses owner IDs + slot indices so each client resolves to correct perspective.
    /// </summary>
    [ObserversRpc]
    private void RpcExecuteAbilityOnCard(int attackerOwnerId, int attackerSlotIndex, int targetOwnerId, int targetSlotIndex, bool isOffensive, int damageAmount)
    {
        Debug.Log($"[Client] Executing ability: attackerOwner={attackerOwnerId}, attackerSlot={attackerSlotIndex}, targetOwner={targetOwnerId}, targetSlot={targetSlotIndex}, damage={damageAmount}");
        
        // Resolve slot names based on local perspective
        string attackerSlotName = ResolveSlotNameForPlayer(attackerOwnerId, attackerSlotIndex);
        string targetSlotName = ResolveSlotNameForPlayer(targetOwnerId, targetSlotIndex);
        
        CardController attacker = FindCardInSlot(attackerSlotName);
        CardController target = FindCardInSlot(targetSlotName);
        
        if (attacker == null || target == null)
        {
            Debug.LogWarning($"[Client] Could not find attacker ({attackerSlotName}) or target ({targetSlotName}) card for ability execution");
            return;
        }
        
        // Tap the attacker
        attacker.TapCard();
        
        // Apply damage to target
        if (damageAmount > 0)
        {
            target.TakeDamage(damageAmount);
        }
        else
        {
            // For non-damage abilities, call the ability directly
            if (isOffensive)
                attacker.ActivateOffensiveAbility(target);
            else
                attacker.ActivateDefensiveAbility(target);
        }
    }
    
    /// <summary>
    /// Broadcast to all clients to execute an ability on a player target.
    /// Uses attacker owner ID + slot index so each client resolves to correct perspective.
    /// </summary>
    [ObserversRpc]
    private void RpcExecuteAbilityOnPlayer(int attackerOwnerId, int attackerSlotIndex, int targetPlayerId, bool isOffensive, int damageAmount)
    {
        Debug.Log($"[Client] Executing ability on player: attackerOwner={attackerOwnerId}, attackerSlot={attackerSlotIndex}, targetPlayer={targetPlayerId}, damage={damageAmount}");
        
        // Resolve attacker slot name based on local perspective
        string attackerSlotName = ResolveSlotNameForPlayer(attackerOwnerId, attackerSlotIndex);
        
        CardController attacker = FindCardInSlot(attackerSlotName);
        if (attacker == null)
        {
            Debug.LogWarning($"[Client] Could not find attacker card in {attackerSlotName} for ability execution");
            return;
        }
        
        // Tap the attacker
        attacker.TapCard();
        
        // Player damage is handled by SyncVar on CurrentHealth, no need to apply locally
    }
    
    /// <summary>
    /// Broadcast to all clients to execute a flip ability (card from hand) on a card target.
    /// Uses target owner ID + slot index so each client resolves to correct perspective.
    /// </summary>
    [ObserversRpc]
    private void RpcExecuteFlipAbilityOnCard(int handIndex, int targetOwnerId, int targetSlotIndex, bool isOffensive, int damageAmount)
    {
        Debug.Log($"[Client] Executing flip ability: hand index={handIndex}, targetOwner={targetOwnerId}, slotIndex={targetSlotIndex}, damage={damageAmount}");
        
        // Get the hand controller and card
        var handController = GetHandController();
        if (handController == null)
        {
            Debug.LogError($"[Client] Could not find HandController");
            return;
        }
        
        var hand = handController.GetHand();
        if (handIndex < 0 || handIndex >= hand.Count)
        {
            Debug.LogWarning($"[Client] Hand index {handIndex} out of range for hand size {hand.Count}");
            return;
        }
        
        var attacker = hand[handIndex];
        
        // Resolve slot name based on whether target owner is local player or opponent
        string targetSlotName = ResolveSlotNameForPlayer(targetOwnerId, targetSlotIndex);
        
        // Find target card by slot
        CardController target = FindCardInSlot(targetSlotName);
        if (target == null)
        {
            Debug.LogWarning($"[Client] Could not find target card in slot {targetSlotName}");
            return;
        }
        
        // Flip the attacker card
        attacker.FlipCard();
        
        // Apply damage to target
        if (damageAmount > 0)
        {
            target.TakeDamage(damageAmount);
        }
        else
        {
            // For non-damage abilities, call the ability directly
            if (isOffensive)
                attacker.ActivateOffensiveAbility(target);
            else
                attacker.ActivateDefensiveAbility(target);
        }
    }
    
    /// <summary>
    /// Broadcast to all clients to execute a flip ability (card from hand) on a player target.
    /// </summary>
    [ObserversRpc]
    private void RpcExecuteFlipAbilityOnPlayer(int handIndex, int targetPlayerId, bool isOffensive, int damageAmount)
    {
        Debug.Log($"[Client] Executing flip ability on player: hand index={handIndex}, targetPlayer={targetPlayerId}, damage={damageAmount}");
        
        // Get the hand controller and card
        var handController = GetHandController();
        if (handController == null)
        {
            Debug.LogError($"[Client] Could not find HandController");
            return;
        }
        
        var hand = handController.GetHand();
        if (handIndex < 0 || handIndex >= hand.Count)
        {
            Debug.LogWarning($"[Client] Hand index {handIndex} out of range for hand size {hand.Count}");
            return;
        }
        
        var attacker = hand[handIndex];
        
        // Flip the attacker card
        attacker.FlipCard();
        
        // Player damage is handled by SyncVar on CurrentHealth, no need to apply locally
    }
    
    /// <summary>
    /// Finds a card in a specific slot by slot name.
    /// Tries the exact slot name first, then tries the flipped perspective if not found.
    /// This handles the case where slot names are relative to each client's perspective.
    /// </summary>
    private CardController FindCardInSlot(string slotName)
    {
        // Try the exact slot name first
        var slot = GameObject.Find(slotName);
        if (slot != null)
        {
            var card = slot.GetComponentInChildren<CardController>();
            if (card != null) return card;
        }
        
        // Try the flipped perspective (OpponentSlot <-> PlayerSlot)
        string flippedSlotName = FlipSlotPerspective(slotName);
        if (flippedSlotName != null)
        {
            slot = GameObject.Find(flippedSlotName);
            if (slot != null)
            {
                var card = slot.GetComponentInChildren<CardController>();
                if (card != null)
                {
                    Debug.Log($"Found card in flipped slot: {slotName} -> {flippedSlotName}");
                    return card;
                }
            }
        }
        
        Debug.LogWarning($"Could not find card in slot: {slotName} or {flippedSlotName}");
        return null;
    }
    
    /// <summary>
    /// Flips slot perspective: PlayerSlot-X becomes OpponentSlot-X and vice versa.
    /// Also mirrors slot numbers (1<->3) for front/back visual consistency.
    /// </summary>
    private string FlipSlotPerspective(string slotName)
    {
        // Helper to mirror slot numbers: 1<->3, 2 stays
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
        return null;
    }
    
    /// <summary>
    /// Translates a client's slot name to the server's perspective.
    /// On server, Player 0 is always the "local" view, so:
    /// - If Player 0 sends "PlayerSlot-X", server sees it as "PlayerSlot-X"
    /// - If Player 1 sends "PlayerSlot-X", server sees it as "OpponentSlot-X" (their cards are on opponent side from server view)
    /// </summary>
    private string TranslateClientSlotToServer(string clientSlotName, int clientPlayerId)
    {
        // Server perspective is always Player 0's view
        // If the client is Player 0, no translation needed
        if (clientPlayerId == 0)
        {
            return clientSlotName;
        }
        
        // If client is not Player 0, flip the perspective
        // Their "PlayerSlot" is server's "OpponentSlot" and vice versa
        return FlipSlotPerspective(clientSlotName) ?? clientSlotName;
    }
    
    /// <summary>
    /// Finds a NetworkPlayer by their player ID.
    /// </summary>
    private NetworkPlayer FindNetworkPlayerById(int playerId)
    {
        foreach (var np in GameObject.FindObjectsOfType<NetworkPlayer>())
        {
            if (np.PlayerId.Value == playerId) return np;
        }
        return null;
    }
    
    /// <summary>
    /// Extracts the slot index (0, 1, 2) from a slot name like "PlayerSlot-2" or "OpponentSlot-1".
    /// </summary>
    private int GetSlotIndex(string slotName)
    {
        // Slot names are like "PlayerSlot-1", "OpponentSlot-2", etc.
        // The number at the end is 1-indexed, so we subtract 1 to get 0-indexed
        if (string.IsNullOrEmpty(slotName)) return -1;
        
        int dashIndex = slotName.LastIndexOf('-');
        if (dashIndex >= 0 && dashIndex < slotName.Length - 1)
        {
            if (int.TryParse(slotName.Substring(dashIndex + 1), out int slotNumber))
            {
                return slotNumber - 1; // Convert to 0-indexed
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Resolves the correct slot name based on target owner ID from each client's perspective.
    /// If target owner is the local player, returns "PlayerSlot-X", otherwise "OpponentSlot-X".
    /// </summary>
    private string ResolveSlotNameForPlayer(int targetOwnerId, int slotIndex)
    {
        // Get the local NetworkGameManager to determine who the local player is
        var gameManager = NetworkGameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogWarning("[ResolveSlotName] No NetworkGameManager found");
            return $"PlayerSlot-{slotIndex + 1}";
        }
        
        // Find the local player's NetworkPlayer
        NetworkPlayer localPlayer = null;
        foreach (var np in GameObject.FindObjectsOfType<NetworkPlayer>())
        {
            if (np.IsOwner)
            {
                localPlayer = np;
                break;
            }
        }
        
        if (localPlayer == null)
        {
            Debug.LogWarning("[ResolveSlotName] Could not find local NetworkPlayer");
            return $"PlayerSlot-{slotIndex + 1}";
        }
        
        // If the target owner is the local player, use PlayerSlot, otherwise OpponentSlot
        // For opponent slots, mirror the index: 0<->2 (front/back swap for visual consistency)
        if (targetOwnerId == localPlayer.PlayerId.Value)
        {
            return $"PlayerSlot-{slotIndex + 1}";
        }
        else
        {
            // Mirror slots for opponent: 0->2, 1->1, 2->0
            int mirroredIndex = slotIndex;
            if (slotIndex == 0) mirroredIndex = 2;
            else if (slotIndex == 2) mirroredIndex = 0;
            
            return $"OpponentSlot-{mirroredIndex + 1}";
        }
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

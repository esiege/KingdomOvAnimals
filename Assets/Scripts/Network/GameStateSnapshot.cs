using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable snapshot of the entire game state for reconnection purposes.
/// Used when the host disconnects and needs to restore state when they return.
/// </summary>
[Serializable]
public class GameStateSnapshot
{
    // Game info
    public int turnNumber;
    public int currentTurnObjectId;
    public int shuffleSeed;
    
    // Player states
    public PlayerSnapshot localPlayer;
    public PlayerSnapshot opponent;
    
    // Timestamp for validation
    public long timestamp;
    
    /// <summary>
    /// Create a snapshot of the current game state.
    /// </summary>
    public static GameStateSnapshot CaptureState(EncounterController encounter, NetworkGameManager gameManager)
    {
        if (encounter == null || gameManager == null)
        {
            Debug.LogError("[GameStateSnapshot] Cannot capture state - missing references");
            return null;
        }
        
        Debug.Log($"[GameStateSnapshot] Starting capture. TurnNumber={gameManager.TurnNumber.Value}");
        
        var snapshot = new GameStateSnapshot
        {
            turnNumber = gameManager.TurnNumber.Value,
            currentTurnObjectId = gameManager.CurrentTurnObjectId.Value,
            shuffleSeed = gameManager.ShuffleSeed.Value,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        
        // Use NetworkGameManager's references since those are what get linked to NetworkPlayers
        // Fall back to EncounterController's references if not set
        var playerController = gameManager.localPlayerController ?? encounter.player;
        var opponentController = gameManager.opponentPlayerController ?? encounter.opponent;
        var playerHand = encounter.playerHandController;
        var opponentHand = encounter.opponentHandController;
        
        Debug.Log($"[GameStateSnapshot] Player controller: {(playerController != null ? playerController.name : "NULL")}, " +
                  $"currentHealth={playerController?.currentHealth ?? -1}");
        Debug.Log($"[GameStateSnapshot] Opponent controller: {(opponentController != null ? opponentController.name : "NULL")}, " +
                  $"currentHealth={opponentController?.currentHealth ?? -1}");
        
        if (playerController != null)
        {
            snapshot.localPlayer = PlayerSnapshot.Capture(playerController, playerHand, true);
        }
        
        if (opponentController != null)
        {
            snapshot.opponent = PlayerSnapshot.Capture(opponentController, opponentHand, false);
        }
        
        Debug.Log($"[GameStateSnapshot] Captured state: Turn {snapshot.turnNumber}, " +
                  $"Player HP: {snapshot.localPlayer?.health ?? -1}, " +
                  $"Opponent HP: {snapshot.opponent?.health ?? -1}");
        
        return snapshot;
    }
    
    /// <summary>
    /// Serialize to JSON for network transfer.
    /// </summary>
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
    
    /// <summary>
    /// Deserialize from JSON.
    /// </summary>
    public static GameStateSnapshot FromJson(string json)
    {
        try
        {
            return JsonUtility.FromJson<GameStateSnapshot>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameStateSnapshot] Failed to deserialize: {e.Message}");
            return null;
        }
    }
}

/// <summary>
/// Snapshot of a single player's state.
/// </summary>
[Serializable]
public class PlayerSnapshot
{
    public int playerId;
    public int health;
    public int mana;
    public int maxMana;
    
    // Cards in hand (by card ID/name)
    public List<string> handCardIds = new List<string>();
    
    // Cards on board
    public List<BoardCardSnapshot> boardCards = new List<BoardCardSnapshot>();
    
    // Deck state (remaining card IDs in order)
    public List<string> deckCardIds = new List<string>();
    
    // Graveyard
    public List<string> graveyardCardIds = new List<string>();
    
    /// <summary>
    /// Capture a player's current state.
    /// </summary>
    public static PlayerSnapshot Capture(PlayerController controller, HandController handController, bool isLocalPlayer)
    {
        if (controller == null) 
        {
            Debug.LogWarning("[PlayerSnapshot] Controller is null!");
            return null;
        }
        
        // Try to get values from NetworkPlayer first, fall back to local PlayerController values
        int capturedHealth = controller.currentHealth;
        int capturedMana = controller.currentMana;
        int capturedMaxMana = controller.maxMana;
        int capturedPlayerId = 0;
        
        // Only use NetworkPlayer values if it's still valid
        if (controller.networkPlayer != null && controller.networkPlayer.IsSpawned)
        {
            capturedHealth = controller.networkPlayer.CurrentHealth.Value;
            capturedMana = controller.networkPlayer.CurrentMana.Value;
            capturedMaxMana = controller.networkPlayer.MaxMana.Value;
            capturedPlayerId = controller.networkPlayer.PlayerId.Value;
            Debug.Log($"[PlayerSnapshot] Using NetworkPlayer values: HP={capturedHealth}, Mana={capturedMana}");
        }
        else
        {
            Debug.Log($"[PlayerSnapshot] NetworkPlayer unavailable, using local values: HP={capturedHealth}, Mana={capturedMana}");
        }
        
        var snapshot = new PlayerSnapshot
        {
            health = capturedHealth,
            mana = capturedMana,
            maxMana = capturedMaxMana,
            playerId = capturedPlayerId
        };
        
        // Capture hand from HandController
        if (handController != null)
        {
            var hand = handController.GetHand();
            if (hand != null)
            {
                foreach (var card in hand)
                {
                    if (card != null)
                    {
                        snapshot.handCardIds.Add(card.cardName);
                    }
                }
            }
        }
        
        // Capture board from PlayerController
        if (controller.board != null)
        {
            for (int i = 0; i < controller.board.Count; i++)
            {
                var card = controller.board[i];
                if (card != null)
                {
                    // Get the actual slot name from the card's parent
                    string slotName = "";
                    if (card.transform.parent != null)
                    {
                        slotName = card.transform.parent.name;
                    }
                    
                    snapshot.boardCards.Add(new BoardCardSnapshot
                    {
                        slotIndex = i,
                        slotName = slotName,
                        cardId = card.cardName,
                        currentHealth = card.health,
                        maxHealth = card.health, // CardController doesn't have max, use current
                        currentAttack = card.manaCost, // No attack field, use manaCost as placeholder
                        hasAttacked = card.isTapped,
                        hasSummoningSickness = card.hasSummoningSickness,
                        canAttack = !card.hasSummoningSickness && !card.isTapped
                    });
                }
            }
        }
        
        // Capture deck (remaining cards)
        if (controller.deck != null)
        {
            foreach (var card in controller.deck)
            {
                if (card != null)
                {
                    snapshot.deckCardIds.Add(card.cardName);
                }
            }
        }
        
        return snapshot;
    }
}

/// <summary>
/// Snapshot of a card on the board.
/// </summary>
[Serializable]
public class BoardCardSnapshot
{
    public int slotIndex;
    public string slotName; // The actual slot name (e.g., "PlayerSlot-1")
    public string cardId;
    public int currentHealth;
    public int maxHealth;
    public int currentAttack;
    public bool hasAttacked;
    public bool hasSummoningSickness;
    public bool canAttack;
}

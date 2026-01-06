using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Captures complete state of a disconnected player for reconnection.
/// Used by the Despawn/Respawn reconnection pattern.
/// </summary>
[Serializable]
public class DisconnectedPlayerState
{
    // Identity
    public int playerId;
    public string playerName;
    public int oldObjectId; // The ObjectId before disconnect (for turn tracking)
    
    // Turn state
    public bool wasTheirTurn; // Was it this player's turn when they disconnected?
    
    // Game state
    public int health;
    public int maxHealth;
    public int mana;
    public int maxMana;
    
    // Cards
    public List<string> handCardIds = new List<string>();
    public List<BoardCardSnapshot> boardCards = new List<BoardCardSnapshot>();
    public List<string> deckCardIds = new List<string>();
    public List<string> graveyardCardIds = new List<string>();
    
    // Timestamp for grace period
    public float disconnectTime;
    
    /// <summary>
    /// Capture state from a NetworkPlayer and its linked PlayerController.
    /// Called on server when a player disconnects.
    /// </summary>
    public static DisconnectedPlayerState Capture(NetworkPlayer networkPlayer)
    {
        if (networkPlayer == null)
        {
            Debug.LogError("[DisconnectedPlayerState] Cannot capture - NetworkPlayer is null");
            return null;
        }
        
        // Check if it's this player's turn
        bool isTheirTurn = false;
        if (NetworkGameManager.Instance != null)
        {
            isTheirTurn = NetworkGameManager.Instance.CurrentTurnObjectId.Value == networkPlayer.ObjectId;
        }
        
        var state = new DisconnectedPlayerState
        {
            playerId = networkPlayer.PlayerId.Value,
            playerName = networkPlayer.PlayerName.Value,
            oldObjectId = networkPlayer.ObjectId,
            wasTheirTurn = isTheirTurn,
            health = networkPlayer.CurrentHealth.Value,
            maxHealth = networkPlayer.MaxHealth.Value,
            mana = networkPlayer.CurrentMana.Value,
            maxMana = networkPlayer.MaxMana.Value,
            disconnectTime = Time.time
        };
        
        // Capture cards from LinkedPlayerController if available
        var controller = networkPlayer.LinkedPlayerController;
        if (controller != null)
        {
            // Capture hand
            var handController = networkPlayer.GetHandController();
            if (handController != null)
            {
                var hand = handController.GetHand();
                if (hand != null)
                {
                    foreach (var card in hand)
                    {
                        if (card != null)
                        {
                            state.handCardIds.Add(card.cardName);
                        }
                    }
                }
            }
            
            // Capture board
            if (controller.board != null)
            {
                for (int i = 0; i < controller.board.Count; i++)
                {
                    var card = controller.board[i];
                    if (card != null)
                    {
                        string slotName = card.transform.parent != null ? card.transform.parent.name : "";
                        
                        state.boardCards.Add(new BoardCardSnapshot
                        {
                            slotIndex = i,
                            slotName = slotName,
                            cardId = card.cardName,
                            currentHealth = card.health,
                            maxHealth = card.health,
                            currentAttack = card.manaCost,
                            hasAttacked = card.isTapped,
                            hasSummoningSickness = card.hasSummoningSickness,
                            canAttack = !card.hasSummoningSickness && !card.isTapped
                        });
                    }
                }
            }
            
            // Capture deck
            if (controller.deck != null)
            {
                foreach (var card in controller.deck)
                {
                    if (card != null)
                    {
                        state.deckCardIds.Add(card.cardName);
                    }
                }
            }
            
            // Capture graveyard
            if (controller.graveyard != null)
            {
                foreach (var card in controller.graveyard)
                {
                    if (card != null)
                    {
                        state.graveyardCardIds.Add(card.cardName);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning($"[DisconnectedPlayerState] LinkedPlayerController is null for player {state.playerId}. Card state not captured.");
        }
        
        Debug.Log($"[DisconnectedPlayerState] Captured state for player {state.playerId} (ObjectId={state.oldObjectId}): " +
                  $"HP={state.health}/{state.maxHealth}, Mana={state.mana}/{state.maxMana}, " +
                  $"Hand={state.handCardIds.Count}, Board={state.boardCards.Count}, Deck={state.deckCardIds.Count}, " +
                  $"WasTheirTurn={state.wasTheirTurn}");
        
        return state;
    }
    
    /// <summary>
    /// Check if the grace period has expired.
    /// </summary>
    public bool HasGracePeriodExpired(float gracePeriodSeconds)
    {
        return (Time.time - disconnectTime) > gracePeriodSeconds;
    }
}

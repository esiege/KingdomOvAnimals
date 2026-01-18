using System;
using System.Collections.Generic;
using FishNet.Serializing;
using KOA.Data;

namespace KOA.Model
{
    /// <summary>
    /// State for a single player's board, hand, and resources.
    /// </summary>
    [Serializable]
    public class PlayerBoardState
    {
        public const int BOARD_SIZE = 3;
        public const int MAX_HAND_SIZE = 7;
        
        /// <summary>
        /// Player ID (0 or 1).
        /// </summary>
        public int PlayerId;
        
        /// <summary>
        /// Current health.
        /// </summary>
        public int Health;
        
        /// <summary>
        /// Maximum health.
        /// </summary>
        public int MaxHealth;
        
        /// <summary>
        /// Current mana.
        /// </summary>
        public int Mana;
        
        /// <summary>
        /// Maximum mana (increases each turn up to 10).
        /// </summary>
        public int MaxMana;
        
        /// <summary>
        /// Cards on the board. Fixed size array.
        /// Null/empty CardState means slot is empty.
        /// Index 0 = front, 1 = middle, 2 = back.
        /// </summary>
        public CardState[] Board;
        
        /// <summary>
        /// Cards in hand.
        /// </summary>
        public List<CardState> Hand;
        
        /// <summary>
        /// Cards remaining in deck (CardData IDs).
        /// </summary>
        public List<string> Deck;
        
        public PlayerBoardState()
        {
            PlayerId = -1;
            Health = 20;
            MaxHealth = 20;
            Mana = 1;
            MaxMana = 1;
            Board = new CardState[BOARD_SIZE];
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                Board[i] = CardState.Empty;
            }
            Hand = new List<CardState>();
            Deck = new List<string>();
        }
        
        public PlayerBoardState(int playerId) : this()
        {
            PlayerId = playerId;
        }
        
        /// <summary>
        /// Check if a board slot is empty.
        /// </summary>
        public bool IsSlotEmpty(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= BOARD_SIZE) return false;
            return Board[slotIndex] == null || !Board[slotIndex].IsValid;
        }
        
        /// <summary>
        /// Get card in a slot, or null if empty.
        /// </summary>
        public CardState GetCardInSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= BOARD_SIZE) return null;
            var card = Board[slotIndex];
            return (card != null && card.IsValid) ? card : null;
        }
        
        /// <summary>
        /// Place a card in a slot. Returns false if slot is occupied.
        /// </summary>
        public bool PlaceCard(int slotIndex, CardState card)
        {
            if (!IsSlotEmpty(slotIndex)) return false;
            Board[slotIndex] = card;
            return true;
        }
        
        /// <summary>
        /// Remove card from a slot. Returns the removed card or null.
        /// </summary>
        public CardState RemoveCard(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= BOARD_SIZE) return null;
            var card = Board[slotIndex];
            Board[slotIndex] = CardState.Empty;
            return (card != null && card.IsValid) ? card : null;
        }
        
        /// <summary>
        /// Get all valid cards on board.
        /// </summary>
        public List<CardState> GetCardsOnBoard()
        {
            var cards = new List<CardState>();
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                if (!IsSlotEmpty(i))
                {
                    cards.Add(Board[i]);
                }
            }
            return cards;
        }
        
        /// <summary>
        /// Find which slot a card is in by instance ID. Returns -1 if not found.
        /// </summary>
        public int FindSlotByInstanceId(int instanceId)
        {
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                if (Board[i] != null && Board[i].InstanceId == instanceId)
                {
                    return i;
                }
            }
            return -1;
        }
        
        /// <summary>
        /// Check if player can afford to play a card.
        /// </summary>
        public bool CanAfford(int manaCost)
        {
            return Mana >= manaCost;
        }
        
        /// <summary>
        /// Spend mana. Returns false if not enough.
        /// </summary>
        public bool SpendMana(int amount)
        {
            if (Mana < amount) return false;
            Mana -= amount;
            return true;
        }
        
        /// <summary>
        /// Called at start of this player's turn.
        /// </summary>
        public void OnTurnStart(int turnNumber)
        {
            // Increase max mana (cap at 10)
            MaxMana = Math.Min(10, (turnNumber + 1) / 2 + 1);
            Mana = MaxMana;
            
            // Reset all cards on board
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                if (Board[i] != null && Board[i].IsValid)
                {
                    Board[i].OnTurnStart();
                }
            }
        }
        
        /// <summary>
        /// Draw a card from deck to hand. Returns the drawn card or null if deck empty.
        /// </summary>
        public CardState DrawCard(CardLibrary cardLibrary)
        {
            if (Deck.Count == 0) return null;
            if (Hand.Count >= MAX_HAND_SIZE) return null;
            
            string cardDataId = Deck[0];
            Deck.RemoveAt(0);
            
            CardData cardData = cardLibrary.GetCardById(cardDataId);
            if (cardData == null) return null;
            
            var card = new CardState(cardData, PlayerId);
            card.HasSummoningSickness = false; // Hand cards don't have sickness
            Hand.Add(card);
            return card;
        }
        
        /// <summary>
        /// Get a card from hand by index.
        /// </summary>
        public CardState GetHandCard(int handIndex)
        {
            if (handIndex < 0 || handIndex >= Hand.Count) return null;
            return Hand[handIndex];
        }
        
        /// <summary>
        /// Remove a card from hand by index. Returns the removed card.
        /// </summary>
        public CardState RemoveHandCard(int handIndex)
        {
            if (handIndex < 0 || handIndex >= Hand.Count) return null;
            var card = Hand[handIndex];
            Hand.RemoveAt(handIndex);
            return card;
        }
        
        /// <summary>
        /// Check if player is alive.
        /// </summary>
        public bool IsAlive => Health > 0;
        
        /// <summary>
        /// Apply damage to player.
        /// </summary>
        public void TakeDamage(int amount)
        {
            Health = Math.Max(0, Health - amount);
        }
        
        /// <summary>
        /// Heal player.
        /// </summary>
        public void Heal(int amount)
        {
            Health = Math.Min(MaxHealth, Health + amount);
        }
    }
    
    /// <summary>
    /// FishNet serializer for PlayerBoardState.
    /// </summary>
    public static class PlayerBoardStateSerializer
    {
        public static void WritePlayerBoardState(this Writer writer, PlayerBoardState value)
        {
            writer.WriteInt32(value.PlayerId);
            writer.WriteInt32(value.Health);
            writer.WriteInt32(value.MaxHealth);
            writer.WriteInt32(value.Mana);
            writer.WriteInt32(value.MaxMana);
            
            // Write board
            for (int i = 0; i < PlayerBoardState.BOARD_SIZE; i++)
            {
                writer.WriteCardState(value.Board[i]);
            }
            
            // Write hand
            writer.WriteInt32(value.Hand.Count);
            foreach (var card in value.Hand)
            {
                writer.WriteCardState(card);
            }
            
            // Write deck (just IDs)
            writer.WriteInt32(value.Deck.Count);
            foreach (var cardId in value.Deck)
            {
                writer.WriteString(cardId);
            }
        }
        
        public static PlayerBoardState ReadPlayerBoardState(this Reader reader)
        {
            var state = new PlayerBoardState();
            state.PlayerId = reader.ReadInt32();
            state.Health = reader.ReadInt32();
            state.MaxHealth = reader.ReadInt32();
            state.Mana = reader.ReadInt32();
            state.MaxMana = reader.ReadInt32();
            
            // Read board
            for (int i = 0; i < PlayerBoardState.BOARD_SIZE; i++)
            {
                state.Board[i] = reader.ReadCardState();
            }
            
            // Read hand
            int handCount = reader.ReadInt32();
            state.Hand = new List<CardState>(handCount);
            for (int i = 0; i < handCount; i++)
            {
                state.Hand.Add(reader.ReadCardState());
            }
            
            // Read deck
            int deckCount = reader.ReadInt32();
            state.Deck = new List<string>(deckCount);
            for (int i = 0; i < deckCount; i++)
            {
                state.Deck.Add(reader.ReadString());
            }
            
            return state;
        }
    }
}

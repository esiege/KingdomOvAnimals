using System;
using FishNet.Serializing;
using KOA.Data;
using UnityEngine;

namespace KOA.Model
{
    /// <summary>
    /// Lightweight data representation of a card instance.
    /// This is the authoritative state synced over the network.
    /// Separate from CardView (visual) and CardData (template).
    /// </summary>
    [Serializable]
    public class CardState
    {
        /// <summary>
        /// Unique identifier for this card instance.
        /// Used to match CardState to CardView across network.
        /// </summary>
        public int InstanceId;
        
        /// <summary>
        /// Reference to the CardData ScriptableObject ID.
        /// Used to look up base stats, abilities, artwork.
        /// </summary>
        public string CardDataId;
        
        /// <summary>
        /// Player who owns this card (0 or 1).
        /// </summary>
        public int OwnerId;
        
        /// <summary>
        /// Current health (may differ from CardData.health due to damage).
        /// </summary>
        public int CurrentHealth;
        
        /// <summary>
        /// Whether this card has used its action this turn.
        /// </summary>
        public bool IsTapped;
        
        /// <summary>
        /// Whether this card was just played and can't act yet.
        /// </summary>
        public bool HasSummoningSickness;
        
        /// <summary>
        /// Whether this card is frozen (can't act).
        /// </summary>
        public bool IsFrozen;
        
        /// <summary>
        /// Whether this card is in defending mode.
        /// </summary>
        public bool IsDefending;
        
        /// <summary>
        /// Whether this card is face-down (flipped).
        /// </summary>
        public bool IsFlipped;
        
        // Static counter for generating unique instance IDs
        private static int _nextInstanceId = 1;
        
        /// <summary>
        /// Create an empty/null card state.
        /// </summary>
        public CardState()
        {
            InstanceId = 0;
            CardDataId = null;
            OwnerId = -1;
            CurrentHealth = 0;
        }
        
        /// <summary>
        /// Create a new card state from a CardData template.
        /// </summary>
        public CardState(CardData cardData, int ownerId)
        {
            InstanceId = _nextInstanceId++;
            CardDataId = cardData.id;
            OwnerId = ownerId;
            CurrentHealth = cardData.health;
            IsTapped = false;
            HasSummoningSickness = true;
            IsFrozen = false;
            IsDefending = false;
            IsFlipped = false;
        }
        
        /// <summary>
        /// Check if this is a valid card (not empty/null).
        /// </summary>
        public bool IsValid => InstanceId > 0 && !string.IsNullOrEmpty(CardDataId);
        
        /// <summary>
        /// Check if this card can use abilities (not tapped, no sickness, not frozen).
        /// </summary>
        public bool CanAct => IsValid && !IsTapped && !HasSummoningSickness && !IsFrozen;
        
        /// <summary>
        /// Apply damage to this card. Returns true if card dies.
        /// </summary>
        public bool TakeDamage(int amount)
        {
            CurrentHealth -= amount;
            return CurrentHealth <= 0;
        }
        
        /// <summary>
        /// Heal this card up to max health.
        /// </summary>
        public void Heal(int amount, int maxHealth)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        }
        
        /// <summary>
        /// Reset turn-based state (untap, remove sickness if had it).
        /// </summary>
        public void OnTurnStart()
        {
            IsTapped = false;
            HasSummoningSickness = false;
        }
        
        /// <summary>
        /// Create a deep copy of this card state.
        /// </summary>
        public CardState Clone()
        {
            return new CardState
            {
                InstanceId = InstanceId,
                CardDataId = CardDataId,
                OwnerId = OwnerId,
                CurrentHealth = CurrentHealth,
                IsTapped = IsTapped,
                HasSummoningSickness = HasSummoningSickness,
                IsFrozen = IsFrozen,
                IsDefending = IsDefending,
                IsFlipped = IsFlipped
            };
        }
        
        /// <summary>
        /// Create an empty card state (represents empty slot).
        /// </summary>
        public static CardState Empty => new CardState();
        
        public override string ToString()
        {
            if (!IsValid) return "[Empty]";
            return $"[Card #{InstanceId}: {CardDataId}, Owner={OwnerId}, HP={CurrentHealth}]";
        }
    }
    
    /// <summary>
    /// FishNet serializer for CardState.
    /// </summary>
    public static class CardStateSerializer
    {
        public static void WriteCardState(this Writer writer, CardState value)
        {
            // Write a flag for null/empty
            bool isValid = value != null && value.IsValid;
            writer.WriteBoolean(isValid);
            
            if (!isValid) return;
            
            writer.WriteInt32(value.InstanceId);
            writer.WriteString(value.CardDataId);
            writer.WriteInt32(value.OwnerId);
            writer.WriteInt32(value.CurrentHealth);
            writer.WriteBoolean(value.IsTapped);
            writer.WriteBoolean(value.HasSummoningSickness);
            writer.WriteBoolean(value.IsFrozen);
            writer.WriteBoolean(value.IsDefending);
            writer.WriteBoolean(value.IsFlipped);
        }
        
        public static CardState ReadCardState(this Reader reader)
        {
            bool isValid = reader.ReadBoolean();
            if (!isValid) return CardState.Empty;
            
            return new CardState
            {
                InstanceId = reader.ReadInt32(),
                CardDataId = reader.ReadString(),
                OwnerId = reader.ReadInt32(),
                CurrentHealth = reader.ReadInt32(),
                IsTapped = reader.ReadBoolean(),
                HasSummoningSickness = reader.ReadBoolean(),
                IsFrozen = reader.ReadBoolean(),
                IsDefending = reader.ReadBoolean(),
                IsFlipped = reader.ReadBoolean()
            };
        }
    }
}

using UnityEngine;

namespace KOA.Data
{
    /// <summary>
    /// ScriptableObject that defines a card's data.
    /// This is the source of truth for card stats; CardController reads from this.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCard", menuName = "KOA/Card Data", order = 0)]
    public class CardData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this card")]
        public string id;
        
        [Tooltip("Display name shown to players")]
        public string displayName;
        
        [TextArea(2, 4)]
        [Tooltip("Flavor text or card description")]
        public string description;

        [Header("Stats")]
        [Tooltip("Card's health points")]
        [Min(1)]
        public int health = 1;
        
        [Tooltip("Mana cost to play this card")]
        [Min(0)]
        public int manaCost;

        [Header("Abilities")]
        [Tooltip("Offensive ability - used when attacking enemy cards or player")]
        public AbilityData offensiveAbility;
        
        [Tooltip("Defensive/Support ability - used on self or allies")]
        public AbilityData defensiveAbility;

        [Header("Visuals")]
        [Tooltip("Card artwork displayed on the card")]
        public Sprite artwork;
        
        [Tooltip("Card frame/border style")]
        public Sprite cardFrame;

        [Header("Categorization")]
        [Tooltip("Card type for filtering/rules")]
        public CardType cardType = CardType.Creature;
        
        [Tooltip("Faction/family this card belongs to")]
        public string faction;

        /// <summary>
        /// Creates a summary string for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"{displayName} ({id}) - HP:{health} Mana:{manaCost}";
        }
    }

    /// <summary>
    /// Types of cards in the game.
    /// </summary>
    public enum CardType
    {
        Creature,   // Standard unit card
        Spell,      // One-time effect card
        Artifact,   // Persistent effect card
        Leader      // Hero/champion card
    }
}

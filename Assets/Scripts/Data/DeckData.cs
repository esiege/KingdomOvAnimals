using System.Collections.Generic;
using UnityEngine;

namespace KOA.Data
{
    /// <summary>
    /// ScriptableObject that defines a deck configuration.
    /// Contains a list of cards that make up the deck.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDeck", menuName = "KOA/Deck Data", order = 2)]
    public class DeckData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this deck")]
        public string id;
        
        [Tooltip("Display name shown to players")]
        public string deckName;
        
        [TextArea(2, 4)]
        [Tooltip("Description of the deck's strategy/theme")]
        public string description;

        [Header("Cards")]
        [Tooltip("Cards in this deck (duplicates allowed)")]
        public List<CardData> cards = new List<CardData>();

        [Header("Rules")]
        [Tooltip("Minimum number of cards required")]
        public int minCards = 10;
        
        [Tooltip("Maximum number of cards allowed")]
        public int maxCards = 30;
        
        [Tooltip("Maximum copies of a single card allowed")]
        public int maxCopiesPerCard = 4;

        /// <summary>
        /// Returns the total number of cards in the deck.
        /// </summary>
        public int CardCount => cards?.Count ?? 0;

        /// <summary>
        /// Checks if the deck is valid according to its rules.
        /// </summary>
        public bool IsValid(out string error)
        {
            if (cards == null || cards.Count == 0)
            {
                error = "Deck is empty";
                return false;
            }
            
            if (cards.Count < minCards)
            {
                error = $"Deck has {cards.Count} cards, minimum is {minCards}";
                return false;
            }
            
            if (cards.Count > maxCards)
            {
                error = $"Deck has {cards.Count} cards, maximum is {maxCards}";
                return false;
            }
            
            // Check for max copies
            var cardCounts = new Dictionary<string, int>();
            foreach (var card in cards)
            {
                if (card == null) continue;
                
                if (!cardCounts.ContainsKey(card.id))
                    cardCounts[card.id] = 0;
                cardCounts[card.id]++;
                
                if (cardCounts[card.id] > maxCopiesPerCard)
                {
                    error = $"Too many copies of {card.displayName} ({cardCounts[card.id]}/{maxCopiesPerCard})";
                    return false;
                }
            }
            
            error = null;
            return true;
        }

        /// <summary>
        /// Gets a shuffled copy of the deck's cards.
        /// </summary>
        public List<CardData> GetShuffledDeck()
        {
            var shuffled = new List<CardData>(cards);
            
            // Fisher-Yates shuffle
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }
            
            return shuffled;
        }

        /// <summary>
        /// Gets a count of each unique card in the deck.
        /// </summary>
        public Dictionary<CardData, int> GetCardCounts()
        {
            var counts = new Dictionary<CardData, int>();
            foreach (var card in cards)
            {
                if (card == null) continue;
                
                if (!counts.ContainsKey(card))
                    counts[card] = 0;
                counts[card]++;
            }
            return counts;
        }
    }
}

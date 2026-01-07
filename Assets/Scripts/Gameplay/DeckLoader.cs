using System.Collections.Generic;
using UnityEngine;
using KOA.Data;

namespace KOA.Gameplay
{
    /// <summary>
    /// Manages a player's deck at runtime: loading, shuffling, and drawing cards.
    /// </summary>
    public class DeckLoader
    {
        private List<CardData> deckCards = new List<CardData>();
        private List<CardData> drawnCards = new List<CardData>();
        private DeckData loadedDeck;
        
        /// <summary>
        /// Load a deck and shuffle it.
        /// </summary>
        public void LoadDeck(DeckData deckData)
        {
            if (deckData == null)
            {
                Debug.LogError("[DeckLoader] Cannot load null DeckData");
                return;
            }
            
            loadedDeck = deckData;
            deckCards.Clear();
            drawnCards.Clear();
            
            // Copy cards from deck data
            if (deckData.cards != null)
            {
                foreach (var card in deckData.cards)
                {
                    if (card != null)
                    {
                        deckCards.Add(card);
                    }
                }
            }
            
            Shuffle();
            
            Debug.Log($"[DeckLoader] Loaded deck '{deckData.deckName}' with {deckCards.Count} cards");
        }
        
        /// <summary>
        /// Draw the next card from the deck.
        /// </summary>
        /// <returns>The drawn CardData, or null if deck is empty</returns>
        public CardData DrawCard()
        {
            if (deckCards.Count == 0)
            {
                Debug.LogWarning("[DeckLoader] Attempted to draw from empty deck");
                return null;
            }
            
            CardData card = deckCards[0];
            deckCards.RemoveAt(0);
            drawnCards.Add(card);
            
            Debug.Log($"[DeckLoader] Drew card: {card.displayName} ({deckCards.Count} remaining)");
            return card;
        }
        
        /// <summary>
        /// Get the number of cards remaining in the deck.
        /// </summary>
        public int GetRemainingCardCount()
        {
            return deckCards.Count;
        }
        
        /// <summary>
        /// Get the number of cards that have been drawn.
        /// </summary>
        public int GetDrawnCardCount()
        {
            return drawnCards.Count;
        }
        
        /// <summary>
        /// Get the name of the loaded deck.
        /// </summary>
        public string GetDeckName()
        {
            return loadedDeck != null ? loadedDeck.deckName : "No Deck";
        }
        
        /// <summary>
        /// Check if the deck is empty.
        /// </summary>
        public bool IsEmpty()
        {
            return deckCards.Count == 0;
        }
        
        /// <summary>
        /// Shuffle the deck using Fisher-Yates algorithm.
        /// </summary>
        private void Shuffle()
        {
            int n = deckCards.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                CardData temp = deckCards[i];
                deckCards[i] = deckCards[j];
                deckCards[j] = temp;
            }
            
            Debug.Log($"[DeckLoader] Shuffled {n} cards");
        }
        
        /// <summary>
        /// Reset the deck to its original state (unshuffled).
        /// Useful for testing or rematch scenarios.
        /// </summary>
        public void ResetDeck()
        {
            if (loadedDeck != null)
            {
                LoadDeck(loadedDeck);
            }
        }
        
        /// <summary>
        /// Get a list of all drawn cards (for graveyard/discard pile).
        /// </summary>
        public List<CardData> GetDrawnCards()
        {
            return new List<CardData>(drawnCards);
        }
    }
}

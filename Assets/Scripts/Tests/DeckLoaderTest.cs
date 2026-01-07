using UnityEngine;
using KOA.Data;
using KOA.Gameplay;

namespace KOA.Tests
{
    /// <summary>
    /// Simple test script to verify DeckLoader functionality.
    /// Attach to any GameObject and run in Play mode.
    /// </summary>
    public class DeckLoaderTest : MonoBehaviour
    {
        [SerializeField] private string deckName = "starter_deck";
        
        private void Start()
        {
            TestDeckLoader();
        }
        
        private void TestDeckLoader()
        {
            Debug.Log("=== DeckLoader Test Start ===");
            
            // Load deck from Resources
            DeckData deck = Resources.Load<DeckData>($"Decks/{deckName}");
            if (deck == null)
            {
                Debug.LogError($"Failed to load deck: {deckName}");
                return;
            }
            
            DeckLoader loader = new DeckLoader();
            
            // Test 1: Load deck
            loader.LoadDeck(deck);
            Debug.Log($"✓ Loaded deck: {loader.GetDeckName()}");
            Debug.Log($"✓ Remaining cards: {loader.GetRemainingCardCount()}");
            
            // Test 2: Draw initial hand (5 cards)
            Debug.Log("\n--- Drawing Initial Hand (5 cards) ---");
            for (int i = 0; i < 5; i++)
            {
                CardData card = loader.DrawCard();
                if (card != null)
                {
                    Debug.Log($"  Card {i+1}: {card.displayName} ({card.manaCost}⬡ {card.health}♥)");
                }
            }
            Debug.Log($"✓ Remaining in deck: {loader.GetRemainingCardCount()}");
            Debug.Log($"✓ Drawn so far: {loader.GetDrawnCardCount()}");
            
            // Test 3: Draw remaining cards
            Debug.Log("\n--- Drawing Rest of Deck ---");
            int drawCount = 0;
            while (!loader.IsEmpty())
            {
                CardData card = loader.DrawCard();
                if (card != null)
                {
                    drawCount++;
                    Debug.Log($"  Drew: {card.displayName}");
                }
            }
            Debug.Log($"✓ Drew {drawCount} more cards");
            Debug.Log($"✓ Deck is empty: {loader.IsEmpty()}");
            
            // Test 4: Try drawing from empty deck
            Debug.Log("\n--- Drawing from Empty Deck ---");
            CardData emptyCard = loader.DrawCard();
            Debug.Log($"✓ Empty deck returns: {(emptyCard == null ? "null (correct)" : "ERROR")}");
            
            // Test 5: Reset and redraw
            Debug.Log("\n--- Reset Deck ---");
            loader.ResetDeck();
            Debug.Log($"✓ After reset: {loader.GetRemainingCardCount()} cards");
            
            Debug.Log("\n=== DeckLoader Test Complete ===");
        }
    }
}

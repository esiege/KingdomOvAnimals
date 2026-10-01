# Story 021: Deck Loader System

## Status: Complete
## Sprint: 03
## Dependencies: 020
## Started: 2026-01-07
## Completed: 2026-01-07 

---

## User Story

**As a** player  
**I want** my deck to be loaded at match start  
**So that** I can draw cards during the game

---

## Description

- Create DeckLoader system to load DeckData at runtime
- Shuffle deck at match start
- Draw initial hand (configurable size)
- Track remaining cards in deck
- Handle empty deck scenario

---

## Acceptance Criteria

- [x] I can load a deck and see it shuffled
- [x] I can draw cards one at a time from the deck
- [x] I can see how many cards remain in the deck
- [x] Drawing from an empty deck returns nothing (no crash)
- [x] I can reset the deck to draw again

---

## Technical Notes

### DeckLoader.cs
```csharp
public class DeckLoader
{
    private List<CardData> deckCards = new List<CardData>();
    private List<CardData> drawnCards = new List<CardData>();
    
    public void LoadDeck(DeckData deckData)
    {
        deckCards.Clear();
        drawnCards.Clear();
        deckCards.AddRange(deckData.cards);
        Shuffle();
    }
    
    public CardData DrawCard()
    {
        if (deckCards.Count == 0) return null;
        var card = deckCards[0];
        deckCards.RemoveAt(0);
        drawnCards.Add(card);
        return card;
    }
    
    public int GetRemainingCardCount() => deckCards.Count;
    
    private void Shuffle() { /* Fisher-Yates */ }
}
```

### Integration
- GameManager holds DeckLoader for each player
- Match start: LoadDeck() → DrawCard() x initialHandSize
- Player action: "Draw Card" button calls DrawCard()

---

## Definition of Done

- [ ] Code complete
- [ ] Unit tests pass (if applicable)
- [ ] Acceptance criteria met
- [ ] Code reviewed
- [ ] Merged to main branch
- [ ] Story closed in backlog

---

## Notes

- Initial hand size should be configurable (default 5)
- Consider "mill" mechanics (deck runs out → player loses?)
- Future: Card cycling, deck search, graveyard

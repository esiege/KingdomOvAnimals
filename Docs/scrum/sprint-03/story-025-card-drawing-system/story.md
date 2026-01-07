# Story 025: Card Drawing System

## Status: Not Started
## Sprint: 03
## Dependencies: 021, 024
## Started: 

---

## User Story

**As a** player  
**I want** to draw cards from my deck during my turn  
**So that** I can play more cards

---

## Description

- Add "Draw Card" action during player turn
- Network sync draw events
- Handle full hand (discard or limit)
- Visual animation for card draw
- Show remaining deck count

---

## Acceptance Criteria

- [ ] I can click Draw Card during my turn
- [ ] A new card appears in my hand
- [ ] I see the deck count decrease
- [ ] Opponent sees that I drew (but not which card)
- [ ] Full hand prevents drawing more cards
- [ ] Empty deck shows appropriate message

---

## Technical Notes

### HandController.cs
```csharp
public class HandController : NetworkBehaviour
{
    [SerializeField] private int maxHandSize = 10;
    private List<CardController> cardsInHand = new List<CardController>();
    private DeckLoader deckLoader;
    
    [ServerRpc]
    public void DrawCardServerRpc()
    {
        if (cardsInHand.Count >= maxHandSize)
        {
            // Discard or prevent draw
            return;
        }
        
        CardData cardData = deckLoader.DrawCard();
        if (cardData == null)
        {
            // Empty deck
            return;
        }
        
        SpawnCard(cardData);
        UpdateDeckCountClientRpc(deckLoader.GetRemainingCardCount());
    }
    
    private void SpawnCard(CardData cardData)
    {
        // Instantiate card prefab, add to hand
        var cardObj = Instantiate(cardPrefab, handTransform);
        var card = cardObj.GetComponent<CardController>();
        card.Initialize(cardData);
        cardsInHand.Add(card);
    }
    
    [ClientRpc]
    private void UpdateDeckCountClientRpc(int count)
    {
        deckCountText.text = $"{count} cards";
    }
}
```

### UI Updates
- Deck count badge in corner
- Draw card button (disabled if not your turn or deck empty)
- Hand limit indicator

### Animation
- Card slides from deck pile to hand
- Optional: Card flip/reveal effect

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

- Max hand size configurable (default 10)
- Consider "draw X cards" abilities
- Future: Card cycling, targeted draw, deck search
- Mulligan system (optional starting hand reroll)

# Story 024: Match Setup with Decks

## Status: Not Started
## Sprint: 03
## Dependencies: 021, 022
## Started: 

---

## User Story

**As a** player  
**I want** to select my deck before a match  
**So that** I can play with my chosen strategy

---

## Description

- Add deck selection to matchmaking flow
- Server validates deck is legal
- Load deck for both players at match start
- Spawn initial hand for each player
- Display deck name in match UI

---

## Acceptance Criteria

- [ ] Deck selection UI before entering matchmaking queue
- [ ] Dropdown/list shows available decks from Resources/Decks
- [ ] Selected deck ID sent to server on match start
- [ ] Server validates deck (exists, legal card count, etc.)
- [ ] Both players load their chosen decks
- [ ] Initial hands drawn (default 5 cards each)
- [ ] Match UI shows "Deck: [name]" or deck icon
- [ ] Invalid deck shows error and prevents match start

---

## Technical Notes

### Matchmaking Flow Changes
```
1. Main Menu → Find Match (NEW: Select Deck)
2. Enter Matchmaking Queue (send deckId)
3. Match Found
4. Server: LoadDeck(player1DeckId), LoadDeck(player2DeckId)
5. Server: DrawInitialHands()
6. Match Start
```

### DeckSelectionUI.cs
```csharp
public class DeckSelectionUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown deckDropdown;
    private List<DeckData> availableDecks;
    
    void Start()
    {
        availableDecks = Resources.LoadAll<DeckData>("Decks").ToList();
        PopulateDropdown();
    }
    
    public string GetSelectedDeckId()
    {
        int index = deckDropdown.value;
        return availableDecks[index].id;
    }
}
```

### MatchManager.cs
```csharp
[Server]
public void StartMatch(string player1DeckId, string player2DeckId)
{
    // Validate decks
    var p1Deck = LoadDeck(player1DeckId);
    var p2Deck = LoadDeck(player2DeckId);
    
    if (!ValidateDeck(p1Deck) || !ValidateDeck(p2Deck))
    {
        CancelMatch("Invalid deck");
        return;
    }
    
    // Load decks
    player1DeckLoader.LoadDeck(p1Deck);
    player2DeckLoader.LoadDeck(p2Deck);
    
    // Draw hands
    DrawInitialHands();
}
```

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

- Default to "Starter Deck" if no deck selected
- Validate deck rules: min/max cards, max copies per card
- Future: Deck bans, format restrictions, meta checks

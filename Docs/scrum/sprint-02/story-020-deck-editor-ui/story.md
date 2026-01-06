# Story 020: Deck Editor UI

## Status: Not Started
## Sprint: 02
## Dependencies: 015, 016

---

## User Story

**As a** designer  
**I want** to create and edit decks  
**So that** I can define starter decks and test compositions

---

## Description

- DeckData SO with list of CardData references and deck name
- UI to list existing decks, create new decks
- Available cards list on left, deck contents on right
- Add/remove cards from deck (click or drag)
- Show deck stats (card count, avg mana cost)
- Save deck to disk

---

## Acceptance Criteria

- [ ] I can select a deck from a dropdown
- [ ] I see available cards on the left and deck contents on the right
- [ ] Clicking + on a card adds it to the deck
- [ ] Clicking - on a deck card removes it
- [ ] I see card count and average stats displayed
- [ ] Clicking New Deck creates an empty deck
- [ ] Clicking Save Deck persists changes (survives exiting play mode)
- [ ] Clicking Delete removes the deck after confirmation

---

## Technical Notes

### DeckData.cs
```csharp
[CreateAssetMenu(fileName = "NewDeck", menuName = "KOA/Deck Data")]
public class DeckData : ScriptableObject
{
    public string id;
    public string displayName;
    public List<CardData> cards = new();
    
    public int CardCount => cards.Count;
    
    public bool IsValid(int minCards = 10, int maxCards = 30)
    {
        return cards.Count >= minCards && cards.Count <= maxCards;
    }
}
```

### UI Layout
```
┌─────────────────────────────────────────────────────────────────┐
│  [← Back]                    Deck Editor                        │
├─────────────────────────────────────────────────────────────────┤
│  Deck: [Starter Deck ▼]  [+ New Deck]  [Delete]                 │
├──────────────────────────────┬──────────────────────────────────┤
│  Available Cards             │  Deck Contents (12/30)           │
│  ┌────────────────────────┐  │  ┌────────────────────────────┐  │
│  │  Lion          [+]     │  │  │  Lion x2         [-]       │  │
│  │  Elephant      [+]     │  │  │  Elephant x1     [-]       │  │
│  │  Snake         [+]     │  │  │  Snake x3        [-]       │  │
│  │  Eagle         [+]     │  │  │  Eagle x2        [-]       │  │
│  │  Bear          [+]     │  │  │  Bear x2         [-]       │  │
│  │                        │  │  │  Lion x2         [-]       │  │
│  └────────────────────────┘  │  └────────────────────────────┘  │
├──────────────────────────────┴──────────────────────────────────┤
│  Stats: 12 cards │ Avg HP: 3.2                                  │
│  [Save Deck]                                                    │
└─────────────────────────────────────────────────────────────────┘
```

### DeckEditorUI.cs
```csharp
public class DeckEditorUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown deckDropdown;
    [SerializeField] private Transform availableCardsContent;
    [SerializeField] private Transform deckContentsContent;
    [SerializeField] private TMP_Text statsText;
    
    private DeckData selectedDeck;
    
    public void AddCardToDeck(CardData card)
    {
        selectedDeck.cards.Add(card);
        RefreshDeckContents();
        UpdateStats();
    }
    
    public void RemoveCardFromDeck(int index)
    {
        selectedDeck.cards.RemoveAt(index);
        RefreshDeckContents();
        UpdateStats();
    }
    
    private void UpdateStats()
    {
        float avgHp = selectedDeck.cards.Average(c => c.health);
        statsText.text = $"{selectedDeck.CardCount} cards │ Avg HP: {avgHp:F1}";
    }
}
```

---

## Tasks

- [ ] Create DeckData ScriptableObject
- [ ] Create deck list dropdown
- [ ] Create available cards list
- [ ] Create deck contents list
- [ ] Implement add/remove cards
- [ ] Show deck stats
- [ ] Implement Save functionality
- [ ] Implement New/Delete deck

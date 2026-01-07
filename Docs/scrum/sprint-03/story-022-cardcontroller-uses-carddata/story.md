# Story 022: CardController Uses CardData

## Status: Not Started
## Sprint: 03
## Dependencies: 012, 021
## Started: 

---

## User Story

**As a** developer  
**I want** CardController to initialize from CardData  
**So that** all card stats come from data assets

---

## Description

- Refactor CardController to use CardData
- Remove hardcoded stats
- Initialize() reads from CardData reference
- Link to AbilityData for offensive/defensive abilities
- Maintain existing network sync

---

## Acceptance Criteria

- [ ] I can assign a CardData asset to a CardController in Inspector
- [ ] Card displays the name from CardData
- [ ] Card displays the health from CardData
- [ ] Card displays the artwork from CardData
- [ ] Using offensive ability triggers the CardData's offensive ability
- [ ] Using defensive ability triggers the CardData's defensive ability
- [ ] In multiplayer, both players see the same card stats

---

## Technical Notes

### CardController.cs Changes
```csharp
public class CardController : NetworkBehaviour
{
    public CardData cardData;
    
    [SyncVar] private int currentHealth;
    [SyncVar] private string cardDataId; // For network sync
    
    public void Initialize(CardData data)
    {
        cardData = data;
        cardDataId = data.id;
        currentHealth = data.health;
        UpdateDisplay();
    }
    
    [Server]
    public void UseOffensiveAbility(CardController target)
    {
        if (cardData.offensiveAbility != null)
        {
            AbilityExecutor.Execute(cardData.offensiveAbility, this, target);
        }
    }
}
```

### Network Sync Strategy
- SyncVar: cardDataId (string)
- On client: Look up CardData from CardLibrary using ID
- Don't sync entire CardData (too large)

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

- Story 014 started this, but needs completion
- Ensure CardLibrary loaded before any card initialization
- Test with network (host + client both see same card)

# Story 022: CardController Uses CardData

## Status: Complete ✓
## Sprint: 03
## Dependencies: 012, 021
## Started: 2025-01-08
## Completed: 2026-01-07

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

- [x] I can assign a CardData asset to a CardController in Inspector
- [x] Card displays the name from CardData
- [x] Card displays the health from CardData
- [x] Card displays the artwork from CardData
- [x] Using offensive ability triggers the CardData's offensive ability
- [x] Using defensive ability triggers the CardData's defensive ability
- [ ] In multiplayer, both players see the same card stats

---

## Testing Instructions

1. Open any scene with a card prefab (or create an empty scene)
2. Add a CardController to a GameObject
3. Assign a CardData asset (from Resources/Cards) to the CardController's `cardData` field
4. Alternatively, add the `CardControllerDataTest` script and assign test cards
5. Enter Play mode - the test script will output results to Console

**For Multiplayer Test:**
1. Build a standalone client
2. Run as Host in editor
3. Run client as separate instance
4. Summon a card with CardData - both instances should show the same name/health

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

# Story 014: Migrate CardController to CardData

## Status: Complete ✓
## Sprint: 02
## Dependencies: 012
## Started: 2026-01-06
## Completed: 2026-01-06

---

## User Story

**As a** developer  
**I want** CardController to initialize from CardData  
**So that** cards get their stats from data assets

---

## Description

- CardController has a CardData reference field
- Initialize() reads from CardData instead of hardcoded values
- Abilities execute using AbilityData values
- Existing functionality preserved

---

## Acceptance Criteria

- [ ] Card prefabs have a CardData field visible in Inspector
- [ ] Assigning a CardData to a card shows its stats correctly
- [ ] Playing the game, cards display health from CardData
- [ ] Using offensive ability deals damage from AbilityData
- [ ] Using defensive ability works as defined in AbilityData
- [ ] No gameplay regressions from before migration

---

## Technical Notes

### Current CardController (pseudocode)
```csharp
public class CardController : MonoBehaviour
{
    public int health = 3;  // Hardcoded
    public int maxHealth = 3;
    // ... other hardcoded values
}
```

### New CardController
```csharp
public class CardController : MonoBehaviour
{
    [SerializeField] private CardData cardData;
    
    private int health;
    private int maxHealth;
    
    public void Initialize(CardData data)
    {
        cardData = data;
        health = data.health;
        maxHealth = data.health;
        // Set up abilities from data.offensiveAbility, data.defensiveAbility
    }
}
```

### Migration Strategy
1. Add CardData field alongside existing hardcoded values
2. Add Initialize(CardData) method
3. Update card prefabs to reference CardData assets
4. Remove hardcoded values once working

---

## Tasks

- [ ] Add CardData field to CardController
- [ ] Create Initialize(CardData) method
- [ ] Update ability execution to use AbilityData
- [ ] Test with existing card prefabs
- [ ] Remove deprecated hardcoded fields

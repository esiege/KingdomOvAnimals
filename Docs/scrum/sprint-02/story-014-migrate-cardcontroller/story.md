# Story 014: Migrate CardController to CardData

## Status: Not Started
## Sprint: 02
## Dependencies: 012

---

## User Story

**As a** developer  
**I want** CardController to initialize from CardData  
**So that** cards get their stats from data assets

---

## Acceptance Criteria

- [ ] CardController has a CardData reference field
- [ ] Initialize() reads from CardData instead of hardcoded values
- [ ] Abilities execute using AbilityData values
- [ ] Existing functionality preserved

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

# Story 019: Create Initial Card/Ability Data Assets

## Status: Not Started
## Sprint: 02
## Dependencies: 017, 018

---

## User Story

**As a** developer  
**I want** CardData and AbilityData assets for existing cards  
**So that** the game works with the new system

---

## Acceptance Criteria

- [ ] AbilityData assets for all existing abilities
- [ ] CardData assets for each existing card (Lion, Elephant, etc.)
- [ ] Stats match current hardcoded values
- [ ] Abilities properly linked to cards
- [ ] Game runs correctly with new data system

---

## Technical Notes

### Current Cards to Migrate
Based on existing card prefabs, create data for:

| Card | Health | Offensive Ability | Defensive Ability |
|------|--------|------------------|-------------------|
| Lion | 3 | Claw Attack (2 dmg) | Roar (buff) |
| Elephant | 5 | Stomp (3 dmg) | Thick Skin (armor) |
| Snake | 2 | Poison Bite (1 dmg + poison) | Shed Skin (heal) |
| Eagle | 2 | Dive Attack (2 dmg) | Evasion (dodge) |
| Bear | 4 | Maul (3 dmg) | Hibernate (heal) |

*(Actual values need to be pulled from existing code)*

### Migration Process
1. Use Card Management scene to create abilities
2. Use Card Management scene to create cards
3. Link abilities to cards
4. Update card spawning to use CardLibrary
5. Test gameplay

### Folder Structure
```
Assets/Resources/
├── Cards/
│   ├── lion.asset
│   ├── elephant.asset
│   └── ...
├── Abilities/
│   ├── claw_attack.asset
│   ├── stomp.asset
│   └── ...
└── Effects/
    ├── GenericDamageEffect.prefab
    └── GenericHealEffect.prefab
```

---

## Tasks

- [ ] Document current card stats from code
- [ ] Create AbilityData assets for each ability
- [ ] Create CardData assets for each card
- [ ] Link abilities to cards
- [ ] Update card spawning code
- [ ] Verify gameplay works
- [ ] Remove deprecated hardcoded values

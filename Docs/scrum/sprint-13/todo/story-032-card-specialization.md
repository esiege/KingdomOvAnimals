# Story 032: Card Specialization/Upgrade System

## Story Information

- **Story ID**: 032
- **Story Points**: 5
- **Priority**: Medium
- **Sprint**: Sprint 13

> **Rescheduled 2026-08-12 (vdate):** moved from Sprint 04 to Sprint 13. Still fits the current pitch as-is.
- **Status**: Planned
- **Assigned**: Unassigned
- **Created**: 2026-01-17

## User Story

**As a** player  
**I want** to specialize my cards through story choices  
**So that** my deck evolves with unique upgraded versions of base cards

## Background / Context

Some story branches offer specialization choices that upgrade existing cards:
- "The wolf rises to pack leader! Choose a specialty: Mage, Fighter, or Guardian"
- This transforms Wolf → Wolf Mage (gains magic abilities)

Specializations add strategic depth without requiring entirely new card designs.

## Acceptance Criteria

- [ ] **AC1**: CardUpgradeData defines transformation (baseCardId → upgradedCardId)
- [ ] **AC2**: SpecializationType enum (Mage, Fighter, Guardian, Healer, Assassin, etc.)
- [ ] **AC3**: UpgradeCard(originalCardId, specializationType) transforms cards in deck
- [ ] **AC4**: Multiple copies can be upgraded (upgrade all Wolves or choose which)
- [ ] **AC5**: Upgraded cards retain thematic connection to base card
- [ ] **AC6**: Story branches can offer specialization choices
- [ ] **AC7**: UI shows before/after comparison when choosing specialization
- [ ] **AC8**: Upgraded cards properly synced if used in networked duel

## Tasks

### Data Model
- [ ] Create SpecializationType enum with 6-8 types
- [ ] Create CardUpgradeData (baseCardId, specializationType, upgradedCardId)
- [ ] Create upgrade registry/lookup system
- [ ] Define upgrade paths for template cards (Wolf variants)

### Upgrade Logic
- [ ] Implement CardSpecializationManager
- [ ] Implement GetAvailableUpgrades(cardId) 
- [ ] Implement UpgradeCard(cardInstanceId, specializationType)
- [ ] Handle upgrading specific instances vs all copies

### Card Variants
- [ ] Create Wolf Mage CardData (magic-focused)
- [ ] Create Wolf Fighter CardData (damage-focused)
- [ ] Create Wolf Guardian CardData (defense-focused)
- [ ] Ensure base stats differ meaningfully

### UI Integration
- [ ] Create SpecializationChoiceUI component
- [ ] Show stat comparison (before/after)
- [ ] Highlight ability changes
- [ ] Animation for card transformation

### Testing
- [ ] Test single card upgrade
- [ ] Test batch upgrades
- [ ] Verify upgraded cards work in duels
- [ ] Test network sync of upgraded cards

## Technical Notes

- **Location**: `Assets/Scripts/Logic/CardSpecializationManager.cs`
- **Card Variants**: `Assets/Resources/Cards/Variants/`
- **Namespace**: `KOA.Logic`

### Specialization Types
| Type | Theme | Stat Focus |
|------|-------|------------|
| Mage | Magic, spells | +Ability damage |
| Fighter | Combat, strength | +Base attack |
| Guardian | Defense, protection | +Health |
| Healer | Support, restoration | +Heal ability |
| Assassin | Stealth, burst | +Critical effects |
| Summoner | Allies, tokens | +Summon effects |

### Example Upgrade
```
Wolf (Basic)
- Health: 3
- Offensive: Bite (2 damage)
- Defensive: Howl (buff allies)

Wolf Mage (Mage Specialization)
- Health: 2 (-1)
- Offensive: Frost Fang (3 damage + slow)
- Defensive: Moon Blessing (heal + draw)
```

## Dependencies

- **Depends On**: 031 (Card Pack Reward), 012 (CardData)
- **Blocks**: None
- **Related**: AbilityData from Story 018

## Questions / Decisions

### Open Questions
- [ ] Can cards be upgraded multiple times?
- [ ] Do upgrades cost resources or are they free choices?
- [ ] Specialization locked to first choice or can change?

### Decisions Made
- **6 base specializations**: Mage, Fighter, Guardian, Healer, Assassin, Summoner
  - Date: 2026-01-17
  - Rationale: Covers major RPG archetypes, expandable later

## Definition of Done Checklist

- [ ] All acceptance criteria met
- [ ] All tasks completed
- [ ] Code reviewed
- [ ] Template upgrades (Wolf variants) created
- [ ] UI shows clear stat changes
- [ ] No critical bugs

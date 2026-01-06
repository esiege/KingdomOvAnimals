# Product Backlog - Kingdom Ov Animals

Stories in priority order. We commit to **1 story at a time per sprint**.

---

## Sprint 01 - Networking Foundation ✓ COMPLETE

All 11 stories completed. See [Completed Stories](#completed-stories) below.

---

## Sprint 02 - Card Management System

### Epic: Card Management Screen

A data-driven card system with a dedicated Unity scene for managing cards, abilities, and decks.

| Story | Title | Status | Dependencies |
|-------|-------|--------|--------------|
| 012 | Create Core Data ScriptableObjects | Not Started | None |
| 013 | Create Base Effect Prefabs | Not Started | 012 |
| 014 | Migrate CardController to CardData | Not Started | 012 |
| 015 | Create CardLibrary System | Not Started | 012, 014 |
| 016 | Create Card Management Scene Shell | Not Started | None |
| 017 | Card List & Editor UI | Not Started | 012, 016 |
| 018 | Ability List & Editor UI | Not Started | 012, 016 |
| 019 | Create Initial Card/Ability Data Assets | Not Started | 017, 018 |
| 020 | Deck Editor UI | Not Started | 015, 016 |

---

### Story Details

#### 012 - Create Core Data ScriptableObjects
```
As a developer
I want CardData, AbilityData, and EffectData ScriptableObject classes
So that card definitions are data-driven and editable

Acceptance Criteria:
- [ ] CardData SO with: id, displayName, health, offensiveAbility, defensiveAbility, artwork
- [ ] AbilityData SO with: id, displayName, description, manaCost, damage, healAmount, 
      duration, targetType, effectPrefab, animationType, vfxPrefab
- [ ] TargetType enum (Self, SingleEnemy, AllEnemies, SingleAlly, AllAllies)
- [ ] AnimationType enum (Melee, Projectile, AOE, Buff, etc.)
- [ ] [CreateAssetMenu] attributes for manual creation if needed
- [ ] Proper serialization (shows in inspector)
```

#### 013 - Create Base Effect Prefabs
```
As a developer
I want generic effect prefabs that read from AbilityData
So that designers can create most abilities without coding

Acceptance Criteria:
- [ ] AbilityEffect base class with Execute(AbilityData, CardController target)
- [ ] GenericDamageEffect - deals data.damage to target
- [ ] GenericHealEffect - heals data.healAmount to target
- [ ] Effect prefabs in Resources/Effects/ folder
- [ ] Effects can be assigned to AbilityData.effectPrefab
```

#### 014 - Migrate CardController to CardData
```
As a developer
I want CardController to initialize from CardData
So that cards get their stats from data assets

Acceptance Criteria:
- [ ] CardController has a CardData reference field
- [ ] Initialize() reads from CardData instead of hardcoded values
- [ ] Abilities execute using AbilityData values
- [ ] Existing functionality preserved
```

#### 015 - Create CardLibrary System
```
As a developer
I want a CardLibrary that loads all CardData assets
So that cards can be looked up by ID at runtime

Acceptance Criteria:
- [ ] CardLibrary singleton loads all CardData from Resources
- [ ] GetCardById(string id) returns CardData
- [ ] GetAllCards() returns list for deck building
- [ ] GetAbilityById(string id) returns AbilityData
- [ ] Works in builds (not editor-only)
```

#### 016 - Create Card Management Scene Shell
```
As a designer
I want a Card Management scene with a main menu
So that I can navigate to different editors

Acceptance Criteria:
- [ ] New scene "CardManagement" in Scenes folder
- [ ] Main menu with buttons: Edit Cards, Edit Abilities, Edit Decks
- [ ] Panel navigation system (show/hide panels)
- [ ] Back button to return to main menu
- [ ] Scene NOT added to build settings
```

#### 017 - Card List & Editor UI
```
As a designer
I want to see all cards and edit them
So that I can balance card stats

Acceptance Criteria:
- [ ] Left panel: scrollable list of all CardData assets
- [ ] Search/filter by name
- [ ] Right panel: selected card's editable fields
- [ ] Dropdown to assign offensive/defensive abilities
- [ ] Save button persists changes to disk (#if UNITY_EDITOR)
- [ ] New Card button creates new CardData asset
- [ ] Delete button with confirmation
```

#### 018 - Ability List & Editor UI
```
As a designer
I want to see all abilities and edit their values
So that I can design and balance abilities

Acceptance Criteria:
- [ ] List of all AbilityData assets
- [ ] Edit: name, description, mana cost
- [ ] Edit: damage, healAmount, duration values
- [ ] Dropdown for targetType, animationType
- [ ] Dropdown for effectPrefab (shows available effect prefabs)
- [ ] Save persists to disk
- [ ] New/Delete ability buttons
```

#### 019 - Create Initial Card/Ability Data Assets
```
As a developer
I want CardData and AbilityData assets for existing cards
So that the game works with the new system

Acceptance Criteria:
- [ ] AbilityData assets for all existing abilities
- [ ] CardData assets for each existing card (Lion, Elephant, etc.)
- [ ] Stats match current hardcoded values
- [ ] Abilities properly linked to cards
- [ ] Game runs correctly with new data system
```

#### 020 - Deck Editor UI
```
As a designer
I want to create and edit decks
So that I can define starter decks and test compositions

Acceptance Criteria:
- [ ] DeckData SO with list of CardData references and deck name
- [ ] UI to list existing decks, create new decks
- [ ] Available cards list on left, deck contents on right
- [ ] Add/remove cards from deck (click or drag)
- [ ] Show deck stats (card count, avg mana cost)
- [ ] Save deck to disk
```

---

## Future Backlog

*(Stories to be prioritized in future sprints)*

---

## Completed Stories

| Story | Sprint | Completed |
|-------|--------|-----------|
| 001 - Setup FishNet NetworkManager scene | Sprint 01 | 2026-01-03 |
| 002 - Create player connection handler | Sprint 01 | 2026-01-03 |
| 003 - Create main menu scene | Sprint 01 | 2026-01-03 |
| 004 - Sync player health and mana | Sprint 01 | 2026-01-03 |
| 005 - Sync card plays | Sprint 01 | 2026-01-04 |
| 006 - Sync combat and abilities | Sprint 01 | 2026-01-04 |
| 007 - Implement matchmaking queue | Sprint 01 | 2026-01-04 |
| 008 - Network-authorize turn switching | Sprint 01 | 2026-01-04 |
| 009 - Handle player disconnect | Sprint 01 | 2026-01-04 |
| 010 - Add reconnection support | Sprint 01 | 2026-01-06 |
| 011 - Show opponent connection status | Sprint 01 | 2026-01-06 |

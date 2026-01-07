# Product Backlog - Kingdom Ov Animals

Stories in priority order. We commit to **1 story at a time per sprint**.

---

## Sprint 01 - Networking Foundation ✓ COMPLETE

All 11 stories completed. See [Completed Stories](#completed-stories) below.

---

## Sprint 02 - Card Management System ✓ COMPLETE

### Epic: Card Management Screen

A data-driven card system with a dedicated Unity scene for managing cards, abilities, and decks.

| Story | Title | Status | Dependencies |
|-------|-------|--------|--------------|
| 012 | Create Core Data ScriptableObjects | Complete ✓ | None |
| 013 | Create Base Effect Prefabs | Complete ✓ | 012 |
| 014 | Migrate CardController to CardData | Complete ✓ | 012 |
| 015 | Create CardLibrary System | Complete ✓ | 012, 014 |
| 016 | Create Card Management Scene Shell | Complete ✓ | None |
| 017 | Card List & Editor UI | Complete ✓ | 012, 016 |
| 018 | Ability List & Editor UI | Complete ✓ | 012, 016 |
| 019 | Create Initial Card/Ability Data Assets | Complete ✓ | 017, 018 |
| 020 | Deck Editor UI | Complete ✓ | 015, 016 |

---

## Sprint 03 - Runtime Integration

### Epic: Connect Data System to Gameplay

Integrate the card management system into the actual duel/match gameplay.

| Story | Title | Status | Dependencies |
|-------|-------|--------|--------------|
| 021 | Deck Loader System | Complete ✓ | 020 |
| 022 | CardController Uses CardData | In Progress | 012, 021 |
| 023 | Ability Execution System | Not Started | 018, 022 |
| 024 | Match Setup with Decks | Not Started | 021, 022 |
| 025 | Card Drawing System | Not Started | 021, 024 |

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

#### 021 - Deck Loader System
```
As a player
I want my deck to be loaded at match start
So that I can draw cards during the game

Acceptance Criteria:
- [ ] DeckLoader.LoadDeck(DeckData) creates shuffled card list
- [ ] DrawCard() returns next card from deck (or null if empty)
- [ ] GetRemainingCardCount() returns cards left
- [ ] Initial hand drawn at match start
- [ ] Empty deck handled gracefully (no crashes)
- [ ] Works in networked multiplayer (both players have separate decks)
```

#### 022 - CardController Uses CardData
```
As a developer
I want CardController to initialize from CardData
So that all card stats come from data assets

Acceptance Criteria:
- [ ] CardController has public CardData cardData field
- [ ] Initialize(CardData) sets displayName, health, abilities
- [ ] UseOffensiveAbility() executes cardData.offensiveAbility
- [ ] UseDefensiveAbility() executes cardData.defensiveAbility
- [ ] Card artwork loaded from cardData.artwork
- [ ] All existing functionality preserved (health sync, damage, etc.)
- [ ] Network sync still works (replicate CardData ID, not entire object)
```

#### 023 - Ability Execution System
```
As a player
I want abilities to execute with correct effects
So that combat works as designed

Acceptance Criteria:
- [ ] AbilityExecutor.Execute(AbilityData, caster, target) runs ability
- [ ] Routes to correct behavior based on behaviorType
- [ ] DamageAbility deals damage to target
- [ ] HealAbility heals target
- [ ] PoisonAbility applies damage-over-time
- [ ] StunAbility disables card for N turns
- [ ] BuffAttackAbility increases attack stat
- [ ] ReturnToHandAbility returns card to hand
- [ ] DrawCardAbility draws card from deck
- [ ] Network synced (all clients see effect)
```

#### 024 - Match Setup with Decks
```
As a player
I want to select my deck before a match
So that I can play with my chosen strategy

Acceptance Criteria:
- [ ] Deck selection UI before entering matchmaking queue
- [ ] Dropdown/list shows available decks from Resources/Decks
- [ ] Selected deck ID sent to server on match start
- [ ] Server validates deck (exists, legal card count, etc.)
- [ ] Both players load their chosen decks
- [ ] Initial hands drawn (default 5 cards each)
- [ ] Match UI shows "Deck: [name]" or deck icon
- [ ] Invalid deck shows error and prevents match start
```

#### 025 - Card Drawing System
```
As a player
I want to draw cards from my deck during my turn
So that I can play more cards

Acceptance Criteria:
- [ ] "Draw Card" button appears during player turn
- [ ] Clicking draws next card from deck
- [ ] Card appears in hand with animation
- [ ] Opponent sees "Player drew a card" (not which card)
- [ ] Full hand prevents drawing (or auto-discards)
- [ ] Deck count UI shows "X cards remaining"
- [ ] Empty deck handled (button disabled or message shown)
- [ ] Network synced (server authority, clients update UI)
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
| 012 - Create Core Data ScriptableObjects | Sprint 02 | 2026-01-06 |
| 013 - Create Base Effect Prefabs | Sprint 02 | 2026-01-06 |
| 014 - Migrate CardController to CardData | Sprint 02 | 2026-01-06 |
| 015 - Create CardLibrary System | Sprint 02 | 2026-01-06 |
| 016 - Create Card Management Scene Shell | Sprint 02 | 2026-01-06 |
| 017 - Card List & Editor UI | Sprint 02 | 2026-01-06 |
| 018 - Ability List & Editor UI | Sprint 02 | 2026-01-06 |
| 019 - Create Initial Card/Ability Data Assets | Sprint 02 | 2026-01-06 |
| 020 - Deck Editor UI | Sprint 02 | 2026-01-07 |

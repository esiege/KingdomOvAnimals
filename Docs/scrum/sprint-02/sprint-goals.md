# Sprint 02 Goals

## Primary Goal

**Build a data-driven card system with a Card Management scene for easy card/ability creation and balancing.**

## Success Criteria

By the end of this sprint, we should have:

1. [ ] ScriptableObject-based card and ability definitions
2. [ ] A Card Management scene to create/edit cards, abilities, and decks
3. [ ] CardLibrary system for runtime card lookup
4. [ ] All existing cards migrated to the new data system
5. [ ] Game functioning with the new architecture

## Key Focus Areas

### 1. Data Architecture
Establish clean separation of data and behavior:
- CardData, AbilityData as ScriptableObjects (editable data)
- Effect prefabs handle behavior (coded once, reused)
- CardLibrary provides runtime access

### 2. Card Management Scene
A dedicated Unity scene (editor-only) for:
- Creating and editing cards
- Creating and editing abilities
- Building and managing decks
- All changes persist to disk via AssetDatabase

### 3. Migration
Seamlessly transition from hardcoded values to data-driven:
- CardController reads from CardData
- Abilities execute using AbilityData values
- No gameplay changes, just architecture improvement

## Non-Goals (Out of Scope)

- New card designs or abilities
- Visual card preview in management scene
- Complex effect composition (multi-effect abilities)
- Balance tuning (that comes after tooling)

## Definition of Done for Sprint

- [ ] All committed stories complete
- [ ] Card Management scene functional in Play Mode
- [ ] Existing cards work with new data system
- [ ] No regressions in gameplay

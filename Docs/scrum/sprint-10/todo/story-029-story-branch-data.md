# Story 029: Story Branch Data Structure

## Story Information

- **Story ID**: 029
- **Story Points**: 5
- **Priority**: High
- **Sprint**: Sprint 10

> **Rescheduled 2026-08-12 (vdate):** moved from Sprint 04 to Sprint 10 ("non-combat segment nodes"). Still fits
> the current pitch as-is — each run segment presents one of these between duels.
- **Status**: Planned
- **Assigned**: Unassigned
- **Created**: 2026-01-17

## User Story

**As a** developer  
**I want** a flexible data structure for branching story scenarios  
**So that** we can present narrative choices that affect deck composition

## Background / Context

Adventure Mode presents players with multiple-choice story scenarios. Each choice:
1. Advances a narrative
2. Adds cards to the player's deck
3. May upgrade existing cards
4. Leads to the next branch or a duel

Story branches must support:
- Context from selected animal class
- References to previous choices
- Card pack rewards (e.g., "add 4x Wolves")
- Card upgrades (e.g., "Wolf becomes Wolf Mage")
- AI-generated content in future iterations

## Acceptance Criteria

- [ ] **AC1**: StoryBranchData ScriptableObject with id, title, narrativeText, choices array
- [ ] **AC2**: StoryChoiceData with choiceText, reward (CardPackReward), nextBranchId, cardUpgrade (optional)
- [ ] **AC3**: CardPackReward struct with cardId, quantity, and optional upgraded variant
- [ ] **AC4**: CardUpgradeData for specialization (original card → upgraded card)
- [ ] **AC5**: StoryBranchLibrary to load and query branches
- [ ] **AC6**: Template branches created for one animal class (Canidae/Wolves) as proof of concept
- [ ] **AC7**: Support for placeholder tokens in narrative text (e.g., "{className}", "{playerChoice}")
- [ ] **AC8**: Branches can specify requirements (e.g., "requires wolf_pack_choice")

## Tasks

### Data Model
- [ ] Create CardPackReward struct (cardId, quantity, isUpgraded)
- [ ] Create CardUpgradeData (originalCardId, upgradedCardId, specializationType)
- [ ] Create StoryChoiceData (choiceText, reward, nextBranchId, upgrade, requirements)
- [ ] Create StoryBranchData SO (id, title, narrativeText, choices[], contextRequirements)
- [ ] Create SpecializationType enum (Mage, Fighter, Guardian, etc.)

### Library System
- [ ] Create StoryBranchLibrary singleton
- [ ] Implement GetBranchById(string id)
- [ ] Implement GetStartingBranchForClass(string classId)
- [ ] Implement token replacement in narrative text

### Template Content
- [ ] Create JSON/SO for Canidae starting branch
- [ ] Create 3 branches for first board (wolf pack scenario)
- [ ] Include specialization choice (Wolf → Wolf Mage/Wolf Fighter)
- [ ] Include ally choice (add Owl/Bat as mystic ally)

### Testing
- [ ] Verify branch loading
- [ ] Test token replacement
- [ ] Validate branch chain (no broken nextBranchIds)

## Technical Notes

- **Location**: `Assets/Scripts/Data/StoryBranchData.cs`, `Assets/Scripts/Data/StoryBranchLibrary.cs`
- **Content Location**: `Assets/Resources/Data/StoryBranches/`
- **Namespace**: `KOA.Data`

### Example Branch Structure
```
Branch: canidae_start
  Title: "The Call of the Pack"
  Narrative: "You hear howling in the distance. A pack of {className} approaches..."
  Choices:
    1. "Join the hunt" → Adds 4x Wolf, next: canidae_hunt_01
    2. "Observe from afar" → Adds 2x Wolf + 1x Lone Wolf, next: canidae_lone_01
    3. "Challenge the alpha" → Adds 3x Wolf + 1x Alpha Wolf, next: canidae_alpha_01
```

## Dependencies

- **Depends On**: 027 (Animal Classification Data), 012 (CardData system)
- **Blocks**: 030 (Story Presentation UI), 035 (AI Story Generation)
- **Related**: Cards created in Sprint 02

## Questions / Decisions

### Open Questions
- [ ] How much narrative content do we hand-craft vs generate?
- [ ] Should card upgrades create new CardData or modify existing?
- [ ] Store branches as JSON or ScriptableObjects?

### Decisions Made
- **Token system for narratives**: Use {token} syntax for dynamic text
  - Date: 2026-01-17
  - Rationale: Allows AI to generate content that references player context

## Definition of Done Checklist

- [ ] All acceptance criteria met
- [ ] All tasks completed
- [ ] Code reviewed
- [ ] Template branches functional
- [ ] No broken branch chains

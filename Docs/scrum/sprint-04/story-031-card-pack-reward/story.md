# Story 031: Card Pack Reward System

## Story Information

- **Story ID**: 031
- **Story Points**: 3
- **Priority**: Medium
- **Sprint**: Sprint 04
- **Status**: Planned
- **Assigned**: Unassigned
- **Created**: 2026-01-17

## User Story

**As a** player  
**I want** to receive card packs as rewards for story choices  
**So that** my deck grows based on my narrative decisions

## Background / Context

Story choices grant card rewards to the player's adventure deck. This system:
- Adds multiple copies of cards (e.g., "4x Wolves")
- Supports upgraded/specialized variants
- Tracks which cards have been acquired
- Connects story choices to deck building

## Acceptance Criteria

- [ ] **AC1**: CardPackRewardSystem processes StoryChoiceData rewards
- [ ] **AC2**: AddCardPack(CardPackReward) adds cards to AdventureDeck
- [ ] **AC3**: Support for quantity (add X copies of a card)
- [ ] **AC4**: Support for upgraded variants (Wolf → Wolf Mage)
- [ ] **AC5**: AdventureDeck persists between story branches
- [ ] **AC6**: Deck validates (min/max card counts, duplicates allowed)
- [ ] **AC7**: Reward confirmation shows exactly what was added
- [ ] **AC8**: "Undo" not supported (choices are final)

## Tasks

### Core System
- [ ] Create AdventureDeckManager to hold current adventure deck
- [ ] Implement AddCardPack(CardPackReward) method
- [ ] Implement card instantiation from CardLibrary
- [ ] Track card acquisition history

### Integration
- [ ] Connect to StoryPresentationController choice confirmation
- [ ] Fire events when cards added (for UI feedback)
- [ ] Support upgraded card variants

### Validation
- [ ] Implement deck size limits (if any)
- [ ] Handle missing CardData gracefully
- [ ] Log warnings for configuration issues

### Testing
- [ ] Test adding single cards
- [ ] Test adding multiple copies
- [ ] Test upgraded variants
- [ ] Verify deck persistence across branches

## Technical Notes

- **Location**: `Assets/Scripts/Logic/AdventureDeckManager.cs`
- **Namespace**: `KOA.Logic`
- Builds on DeckData from Story 020

### Example Flow
```csharp
// Player chooses "Join the hunt"
var reward = new CardPackReward {
    cardId = "wolf_basic",
    quantity = 4,
    isUpgraded = false
};
AdventureDeckManager.Instance.AddCardPack(reward);
// Deck now has 4 copies of Wolf card
```

## Dependencies

- **Depends On**: 029 (Story Branch Data), 020 (Deck system), 015 (CardLibrary)
- **Blocks**: 032 (Card Specialization)
- **Related**: DeckLoader from Story 021

## Questions / Decisions

### Open Questions
- [ ] Starting deck size for adventure mode?
- [ ] Maximum deck size limit?
- [ ] Can duplicate cards exceed 4 copies?

## Definition of Done Checklist

- [ ] All acceptance criteria met
- [ ] All tasks completed
- [ ] Code reviewed
- [ ] Integration with story UI working
- [ ] No critical bugs

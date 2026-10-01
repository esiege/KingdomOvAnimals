# Story 030: Story Presentation UI

## Story Information

- **Story ID**: 030
- **Story Points**: 5
- **Priority**: Medium
- **Sprint**: Sprint 10

> **Rescheduled 2026-08-12 (vdate):** moved from Sprint 04 to Sprint 10. Still fits the current pitch as-is.
- **Status**: Planned
- **Assigned**: Unassigned
- **Created**: 2026-01-17

## User Story

**As a** player  
**I want** to read story scenarios and make choices  
**So that** I can shape my adventure and build my deck through narrative decisions

## Background / Context

After selecting a class, players experience a series of story branches. Each branch presents:
1. Narrative text describing a scenario
2. Multiple choice options (typically 2-4)
3. Preview of cards that will be added for each choice

This UI is the core loop between duels in Adventure Mode.

## Acceptance Criteria

- [ ] **AC1**: StoryBranchUI panel displays narrative text with typewriter effect (optional)
- [ ] **AC2**: Choice buttons show choice text and card reward preview
- [ ] **AC3**: Card reward preview shows card artwork thumbnails and quantity (e.g., "4x Wolf")
- [ ] **AC4**: Selecting a choice shows confirmation with full card details
- [ ] **AC5**: "Confirm Choice" adds cards to adventure deck and loads next branch
- [ ] **AC6**: Special styling for upgrade choices vs new card choices
- [ ] **AC7**: Transition effects between branches
- [ ] **AC8**: "View Current Deck" button to see cards collected so far

## Tasks

### UI Components
- [ ] Create StoryBranchPanel prefab
- [ ] Create narrative text area with scrolling support
- [ ] Create StoryChoiceButton prefab with reward preview
- [ ] Create CardRewardPreview component (thumbnail grid)
- [ ] Create choice confirmation modal
- [ ] Create deck viewer modal

### Controller Logic
- [ ] Create StoryPresentationController MonoBehaviour
- [ ] Implement LoadBranch(StoryBranchData) to populate UI
- [ ] Implement choice selection and confirmation flow
- [ ] Implement card reward application to deck
- [ ] Handle branch transitions (next branch or duel trigger)

### Visual Polish
- [ ] Typewriter effect for narrative text (coroutine-based)
- [ ] Card hover previews
- [ ] Choice button hover effects
- [ ] Transition animations (fade, slide)

### Testing
- [ ] Test with template Canidae branches
- [ ] Verify cards added to deck correctly
- [ ] Test branch chain navigation

## Technical Notes

- **Location**: `Assets/Scripts/View/AdventureMode/StoryPresentationController.cs`
- **Prefabs**: `Assets/Prefabs/UI/AdventureMode/`
- **Namespace**: `KOA.View`

### UI Layout
```
+------------------------------------------+
|            STORY BRANCH TITLE            |
+------------------------------------------+
|                                          |
|    [Narrative text describing the        |
|     scenario with atmospheric details]   |
|                                          |
+------------------------------------------+
|  Choice 1: "Join the hunt"               |
|  [Wolf][Wolf][Wolf][Wolf] +4 cards       |
+------------------------------------------+
|  Choice 2: "Observe from afar"           |
|  [Wolf][Wolf][Lone Wolf] +3 cards        |
+------------------------------------------+
|  Choice 3: "Challenge the alpha"         |
|  [Wolf][Wolf][Wolf][Alpha] +4 cards      |
+------------------------------------------+
|        [View Deck]                       |
+------------------------------------------+
```

## Dependencies

- **Depends On**: 029 (Story Branch Data Structure), 027 (Animal Classification)
- **Blocks**: 033 (Adventure Progress Tracker)
- **Related**: CardData display from Sprint 02

## Questions / Decisions

### Open Questions
- [ ] Show card stats in preview or just artwork?
- [ ] Allow changing choice before confirmation?
- [ ] Sound effects for typewriter/selection?

## Definition of Done Checklist

- [ ] All acceptance criteria met
- [ ] All tasks completed
- [ ] Code reviewed
- [ ] Visually polished
- [ ] Responsive layout
- [ ] No critical bugs

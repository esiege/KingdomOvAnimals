# Story 034: Board Map UI

## Story Information

- **Story ID**: 034
- **Story Points**: 5
- **Priority**: Low
- **Sprint**: Sprint 08

> **Rescheduled 2026-08-12 (vdate):** moved from Sprint 04 to Sprint 08, alongside Story 033's run-state model.
- **Status**: Planned
- **Assigned**: Unassigned
- **Created**: 2026-01-17

## User Story

**As a** player  
**I want** to see a visual map of my adventure progress  
**So that** I understand where I am in the run and what's ahead

## Background / Context

The adventure consists of two "boards":
- **Board 1**: 3 story branches → 3 duels (single class)
- **Board 2**: 3 story branches → 3 duels (secondary class)
- **Endgame**: Final challenges after 6 wins

A 2D map visualization helps players understand:
- Current position
- Completed nodes
- Upcoming branches/duels
- Overall progress toward victory

## Acceptance Criteria

- [ ] **AC1**: BoardMapUI shows current board as a node-based path
- [ ] **AC2**: Nodes represent: Story branches, Duels, Class selection (Board 2 start)
- [ ] **AC3**: Completed nodes visually distinct (checkmark, grayed out)
- [ ] **AC4**: Current node highlighted/animated
- [ ] **AC5**: Future nodes shown but not interactive
- [ ] **AC6**: Board 1 and Board 2 visually separated
- [ ] **AC7**: Win/loss counter displayed
- [ ] **AC8**: Lives remaining shown prominently
- [ ] **AC9**: Tap current node to proceed to that content

## Tasks

### Visual Design
- [ ] Design node layout for 3x story + 3x duel pattern
- [ ] Design board transition visual (Board 1 → Board 2)
- [ ] Create node sprites (story, duel, boss, class select)
- [ ] Create path/connection lines between nodes

### UI Implementation
- [ ] Create BoardMapPanel prefab
- [ ] Create MapNode component (state, type, position)
- [ ] Create path rendering between nodes
- [ ] Implement node state updates from AdventureProgressData

### Controller Logic
- [ ] Create BoardMapController
- [ ] Populate map from current progress
- [ ] Handle node tap/click to proceed
- [ ] Animate transitions between nodes

### Polish
- [ ] Node completion animations
- [ ] Current node pulse/glow effect
- [ ] Progress bar or percentage
- [ ] Thematic styling per animal class

## Technical Notes

- **Location**: `Assets/Scripts/View/AdventureMode/BoardMapController.cs`
- **Prefabs**: `Assets/Prefabs/UI/AdventureMode/BoardMap/`
- **Namespace**: `KOA.View`

### Map Layout
```
BOARD 1 (Canidae Theme)
========================
[Story 1] → [Duel 1] → [Story 2] → [Duel 2] → [Story 3] → [Duel 3]
                                                              ↓
                                                    [Class Selection]
                                                              ↓
BOARD 2 (Secondary Class Theme)
================================
[Story 4] → [Duel 4] → [Story 5] → [Duel 5] → [Story 6] → [Duel 6]
                                                              ↓
                                                         [ENDGAME]
```

## Dependencies

- **Depends On**: 033 (Adventure Progress Tracker), 030 (Story Presentation)
- **Blocks**: None
- **Related**: Visual theming for animal classes

## Questions / Decisions

### Open Questions
- [ ] Linear path or branching paths on map?
- [ ] Show exact content of future nodes (spoilers)?
- [ ] Animated character token moving on map?

## Definition of Done Checklist

- [ ] All acceptance criteria met
- [ ] All tasks completed
- [ ] Code reviewed
- [ ] Visually clear and intuitive
- [ ] Accurate reflection of progress
- [ ] No critical bugs

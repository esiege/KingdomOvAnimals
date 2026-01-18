# Story 028: Class Selection UI

## Story Information

- **Story ID**: 028
- **Story Points**: 3
- **Priority**: High
- **Sprint**: Sprint 04
- **Status**: Planned
- **Assigned**: Unassigned
- **Created**: 2026-01-17

## User Story

**As a** player  
**I want** to browse and select an animal class to start my adventure  
**So that** I can begin building a thematically cohesive deck

## Background / Context

This is the entry point to Adventure Mode. Players select one of 107 animal classes, which determines:
- Their starting card pool
- The theme of the first story branch
- Visual style of the first "board"

The UI should make browsing 107 options manageable through filtering and search.

## Acceptance Criteria

- [ ] **AC1**: New AdventureMode scene created with class selection as entry point
- [ ] **AC2**: Category filter buttons (Mammals, Birds, Reptiles, etc.) to narrow choices
- [ ] **AC3**: Search/filter text field to find classes by name
- [ ] **AC4**: Scrollable grid/list showing filtered animal classes
- [ ] **AC5**: Each class shows: commonName, category icon, short description
- [ ] **AC6**: Hovering/selecting a class shows expanded info with example species
- [ ] **AC7**: "Start Adventure" button confirms selection and begins first story branch
- [ ] **AC8**: Selected class stored in AdventureProgressData for later reference

## Tasks

### Scene Setup
- [ ] Create AdventureMode scene in Scenes folder
- [ ] Create AdventureModeController MonoBehaviour
- [ ] Set up Canvas with proper scaling

### UI Components
- [ ] Create category filter bar (horizontal buttons)
- [ ] Create search input field
- [ ] Create scrollable class grid using UI Toolkit or UGUI ScrollRect
- [ ] Create ClassSelectionCard prefab (icon, name, description)
- [ ] Create expanded class detail panel

### Logic
- [ ] Wire filter buttons to AnimalClassLibrary.GetClassesByCategory()
- [ ] Implement search filtering by commonName
- [ ] Handle class selection (highlight, store reference)
- [ ] Implement "Start Adventure" button logic

### Polish
- [ ] Add category icons for each animal type
- [ ] Add hover/selection visual feedback
- [ ] Add transition animation to story branch

## Technical Notes

- **Location**: `Assets/Scripts/View/AdventureMode/ClassSelectionUI.cs`
- **Scene**: `Assets/Scenes/AdventureMode.unity`
- **Namespace**: `KOA.View`
- Consider lazy loading class cards for performance (107 items)

## Dependencies

- **Depends On**: 027 (Animal Classification Data System)
- **Blocks**: 030 (Story Presentation UI)
- **Related**: None

## Questions / Decisions

### Open Questions
- [ ] Should we show all 107 at once or paginate?
- [ ] Random "featured" classes to highlight?
- [ ] Show class difficulty/complexity rating?

## Definition of Done Checklist

- [ ] All acceptance criteria met
- [ ] All tasks completed
- [ ] Code reviewed
- [ ] Responsive on different screen sizes
- [ ] No critical bugs

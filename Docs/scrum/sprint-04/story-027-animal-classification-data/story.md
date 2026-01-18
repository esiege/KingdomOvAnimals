# Story 027: Animal Classification Data System

## Story Information

- **Story ID**: 027
- **Story Points**: 5
- **Priority**: High
- **Sprint**: Sprint 04
- **Status**: Planned
- **Assigned**: Unassigned
- **Created**: 2026-01-17

## User Story

**As a** player  
**I want** to choose from a diverse collection of animal classes  
**So that** I can start my adventure with a thematic deck that fits my playstyle

## Background / Context

Adventure Mode begins with class selection. We have defined 107 animal "classes" (taxonomic families/groups) in a JSON data file. This story covers loading this data into the game and making it accessible for the class selection UI and card generation systems.

The animal classification system is the foundation for:
- Class selection at adventure start
- Thematic card generation
- Story branch context
- Second class selection after Board 1

## Acceptance Criteria

- [ ] **AC1**: AnimalClassData ScriptableObject created with id, scientificName, commonName, category, description, and children array
- [ ] **AC2**: AnimalSpeciesData struct for children with scientificName and commonName
- [ ] **AC3**: AnimalClassLibrary loads all 107 classes from JSON at runtime
- [ ] **AC4**: GetClassById(string id) returns specific AnimalClassData
- [ ] **AC5**: GetAllClasses() returns list of all classes
- [ ] **AC6**: GetClassesByCategory(string category) filters by Mammals, Birds, Reptiles, etc.
- [ ] **AC7**: JSON file properly parsed with all 107 classes and their children
- [ ] **AC8**: Works in builds (not editor-only)

## Tasks

### Data Model
- [ ] Create AnimalSpeciesData struct (scientificName, commonName)
- [ ] Create AnimalClassData class with all fields
- [ ] Create AnimalClassCategory enum (Mammals, Birds, Reptiles, Amphibians, Fish, Insects, Arachnids, Mollusks, Crustaceans, Cnidarians)

### Library System
- [ ] Create AnimalClassLibrary singleton
- [ ] Implement JSON parsing using Unity's JsonUtility or Newtonsoft.Json
- [ ] Implement GetClassById(string id) method
- [ ] Implement GetAllClasses() method
- [ ] Implement GetClassesByCategory() method
- [ ] Add caching to avoid re-parsing JSON

### Data Integration
- [ ] Copy animal-classes.json to Resources/Data/ folder
- [ ] Verify all 107 classes load correctly
- [ ] Add unit tests for library methods

### Testing
- [ ] Test JSON loading in Editor
- [ ] Test JSON loading in Build
- [ ] Verify all category counts match expected values

## Technical Notes

- **Location**: `Assets/Scripts/Data/AnimalClassData.cs`, `Assets/Scripts/Data/AnimalClassLibrary.cs`
- **JSON Location**: `Assets/Resources/Data/animal-classes.json`
- **Namespace**: `KOA.Data`
- Use `[System.Serializable]` for JSON deserialization
- Consider using Newtonsoft.Json for better nested array handling

## Dependencies

- **Depends On**: None
- **Blocks**: 028 (Class Selection UI), 029 (Story Branch Data)
- **Related**: animal-classes.json in sprint-04/data/

## Questions / Decisions

### Open Questions
- [ ] Should class selection be weighted by category (more mammal options shown)?
- [ ] Do we need localization support for common names?

### Decisions Made
- **107 classes chosen**: Mix of taxonomic families and colloquial groupings for player recognition
  - Date: 2026-01-17
  - Rationale: Pure taxonomic classes (6-9) too limiting; orders/families provide ~107 meaningful player choices

## Definition of Done Checklist

- [ ] All acceptance criteria met
- [ ] All tasks completed
- [ ] Code reviewed
- [ ] Tests written and passing
- [ ] Documentation updated
- [ ] No critical bugs
- [ ] Works in Editor and Build

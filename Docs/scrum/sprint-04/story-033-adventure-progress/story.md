# Story 033: Adventure Progress Tracker

## Story Information

- **Story ID**: 033
- **Story Points**: 3
- **Priority**: Medium
- **Sprint**: Sprint 04
- **Status**: Planned
- **Assigned**: Unassigned
- **Created**: 2026-01-17

## User Story

**As a** player  
**I want** my adventure progress to be saved  
**So that** I can continue my run after closing the game

## Background / Context

Adventure Mode runs can span multiple play sessions. We need to persist:
- Selected animal class
- Current story branch
- Collected cards (adventure deck)
- Win/loss count
- Current board (1 or 2)
- Lives remaining

## Acceptance Criteria

- [ ] **AC1**: AdventureProgressData class holds all run state
- [ ] **AC2**: Progress auto-saves after each story choice
- [ ] **AC3**: Progress auto-saves after each duel result
- [ ] **AC4**: "Continue Adventure" option on main menu if save exists
- [ ] **AC5**: "New Adventure" starts fresh (with confirmation if save exists)
- [ ] **AC6**: Save/load uses PlayerPrefs or JSON file
- [ ] **AC7**: Progress includes: classId, currentBranchId, deck contents, wins, losses, lives, currentBoard
- [ ] **AC8**: Corrupted saves handled gracefully (start new run)

## Tasks

### Data Model
- [ ] Create AdventureProgressData class
- [ ] Include all required fields
- [ ] Make serializable for JSON/PlayerPrefs

### Save System
- [ ] Create AdventureProgressManager singleton
- [ ] Implement Save() method
- [ ] Implement Load() method
- [ ] Implement HasSave() check
- [ ] Implement DeleteSave() for new runs

### Integration
- [ ] Hook save after story choice confirmation
- [ ] Hook save after duel completion
- [ ] Load progress when continuing adventure
- [ ] Update main menu with continue option

### Error Handling
- [ ] Validate loaded data
- [ ] Handle version migrations (future)
- [ ] Clear corrupted saves

### Testing
- [ ] Test save/load cycle
- [ ] Test continuing from various points
- [ ] Test new adventure overwriting save
- [ ] Test corrupted save recovery

## Technical Notes

- **Location**: `Assets/Scripts/Logic/AdventureProgressManager.cs`
- **Save Location**: PlayerPrefs key "AdventureProgress" or `Application.persistentDataPath`
- **Namespace**: `KOA.Logic`

### Progress Data Structure
```csharp
[System.Serializable]
public class AdventureProgressData
{
    public string classId;
    public string secondaryClassId; // null until Board 2
    public string currentBranchId;
    public List<string> deckCardIds; // list of card IDs with duplicates
    public int wins;
    public int losses;
    public int lives; // starts at 3?
    public int currentBoard; // 1 or 2
    public List<string> completedBranchIds; // for history
    public long lastSaveTimestamp;
}
```

## Dependencies

- **Depends On**: 029 (Story Branch Data), 031 (Card Pack Reward)
- **Blocks**: 034 (Board Map UI)
- **Related**: Main menu scene

## Questions / Decisions

### Open Questions
- [ ] How many lives does the player start with?
- [ ] Can players have multiple save slots?
- [ ] Cloud save support needed?

## Definition of Done Checklist

- [ ] All acceptance criteria met
- [ ] All tasks completed
- [ ] Code reviewed
- [ ] Save/load works reliably
- [ ] Continue option appears correctly
- [ ] No data loss scenarios

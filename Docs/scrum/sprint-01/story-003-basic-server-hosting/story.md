# Story 003: Create Main Menu Scene

## Story Information

- **Story ID**: 003
- **Priority**: High
- **Sprint**: Sprint 01
- **Status**: Done ✅
- **Created**: 2026-01-03
- **Started**: 2026-01-03
- **Completed**: 2026-01-03

## User Story

**As a** player  
**I want** a main menu  
**So that** I can choose to host or join a game before the match starts

## Background / Context

Currently the game scene immediately deals out hands. We need a menu scene that loads first, lets players choose to host or join, and only loads the game scene when a match starts.

## Acceptance Criteria

### AC1: Menu scene loads on startup
**Test:** Launch the game
**Expected:** Main menu appears with Host/Join options (not the game board)

### AC2: Can host from menu
**Test:** Click "Host Game" button
**Expected:** Server starts, shows "Waiting for opponent..."

### AC3: Can join from menu
**Test:** Click "Join Game" button (with host running)
**Expected:** Client connects, both players see "Match found!"

### AC4: Game scene loads when match ready
**Test:** 2 players connected, host clicks Start (or auto-start)
**Expected:** Both clients load into the DuelScreen scene

---

## Tasks

- [x] Create MainMenu scene
- [x] Create MainMenuUI script with Host/Join buttons
- [x] Add lobby state (waiting for players)
- [x] Implement scene loading when match starts
- [x] Set MainMenu as the startup scene in Build Settings
- [x] Test full flow: Menu → Host/Join → Game

## Technical Notes

- **Approach**: Use FishNet's SceneManager for networked scene loading
- **Technologies**: FishNet, UnityEngine.SceneManagement
- **Dependencies**: Story 001, 002

## Questions / Decisions

### Open Questions
- [ ] Should match auto-start when 2 players connect, or require host to click Start?
- [ ] What's the max player count? (Assuming 2 for now)

### Decisions Made
- *(To be filled as we work)*

# Story 041: Consolidate Turn-Tracking Systems

## Status: Not Started
## Sprint: 05
## Dependencies: None
## Created: 2026-08-12

## Background

Two independent turn counters currently exist: `NetworkGameManager.CurrentTurnObjectId`/`TurnNumber`
(ObjectId-based, driven by `EndTurnController`, doesn't draw cards or touch `BoardState`) and
`NetworkBoardState`'s `BoardState.CurrentTurnPlayerId`/`TurnNumber` (playerId-based, driven by
`InputController.EndTurn()`, actually draws a card and resets board state). See
`Docs/networking/turn-synchronization.md` for the full breakdown. This is real technical debt, not a doc bug.

## User Story
**As a** developer
**I want** a single source of truth for whose turn it is
**So that** the end-turn button and the actual duel board state can't drift apart

## Acceptance Criteria
- [ ] Decide which system survives — recommend `NetworkBoardState`'s, since `InputController` (what players
      actually interact with) already gates on it via `IsPlayerTurn()`
- [ ] Remove the losing system's turn SyncVars and `Cmd*`/`Rpc*` methods
- [ ] `EndTurnController` calls the surviving system directly
- [ ] `NetworkGameManager`'s remaining responsibilities (player registration, game start, disconnect grace
      period) still work with the turn logic removed
- [ ] Manual test: end turn via the button, confirm card draw and board reset happen every time

## Notes
Touches `Assets/Scripts/Network/NetworkGameManager.cs`, `NetworkBoardState.cs`,
`Assets/Scripts/Controllers/EndTurnController.cs`.

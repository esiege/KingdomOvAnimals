# Story 046: Run State Network Sync

## Status: Not Started
## Sprint: 08
## Dependencies: 033, 034, 044
## Created: 2026-08-12

## Background

Adventure Mode needs a live, synced notion of "where is this player in their run" (which board, which segment,
lives remaining, deck-so-far) that both the segment-node UI (Story 029/030) and duel setup can read from. Today
nothing like this exists — `NetworkBoardState` only knows about a single in-progress duel, not a multi-duel run.
This is the FishNet-sync layer underneath Stories 033/034's tracker and map UI.

## User Story
**As a** player
**I want** my run progress to be reliable and authoritative
**So that** disconnecting mid-run or a bad client can't corrupt my progress or lives

## Acceptance Criteria
- [ ] Server-authoritative run state model: current board, current segment, lives remaining, deck contents so
      far — following the same pattern as `BoardState`/`NetworkBoardState` (single source of truth, `Cmd*` for
      mutation, events for client updates)
- [ ] Run state persists across the 10 separate duels in a run (each duel presumably still uses
      `NetworkBoardState` for its own combat, but something above it tracks the run)
- [ ] Design decision: is run state tied to a persistent account/session, or only to the current connection —
      affects whether progress survives a client restart, not just a mid-duel disconnect

## Notes
This is new architecture, not an extension of existing duel code — plan for it as its own layer above
`NetworkBoardState`, not a bolt-on to it.

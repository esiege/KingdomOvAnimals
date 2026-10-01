# Story 048: Duel Result → Run State Wiring

## Status: Not Started
## Sprint: 11
## Dependencies: 046
## Created: 2026-08-12

## User Story
**As a** player
**I want** winning or losing a duel to actually affect my run
**So that** the 10 duels feel connected to a single ongoing run, not 10 disconnected matches

## Acceptance Criteria
- [ ] `NetworkBoardState.OnGameEnded` (or equivalent) feeds into the Story 046 run state: win → advance to next
      segment, loss → -1 life
- [ ] 0 lives ends the run (distinct from a single duel ending)
- [ ] Segment/board advancement correctly picks the next node's content (next story branch, or the next board's
      duel, or the final boss at segment 10)
- [ ] Manual test: play through several duels in a row within one run, confirm lives/progress track correctly
      across all of them

## Notes
This is the connective tissue between the existing `DuelScreen` combat loop and the new run/map layer — keep the
duel scene itself unaware of run-level concepts (lives, boards) where possible, matching the existing
Model→Network→View layering.

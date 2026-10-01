# Sprint 11 Goals

*(planned 2026-08-12, not yet started)*

## Primary Goal

**Connect duel outcomes to run progress, so the 10 duels feel like one run instead of 10 disconnected matches.**

## Success Criteria

By the end of this sprint, we should have:

1. [ ] A duel win advances the run to the next segment (Story 048)
2. [ ] A duel loss costs a life and continues to the next segment
3. [ ] 0 lives correctly ends the run (distinct from a single duel ending)

## Key Focus Areas

### 1. Connective Tissue, Not New Systems
This sprint wires together what Sprint 08 (run state) and the existing `DuelScreen`/`NetworkBoardState` combat
loop already provide — keep the duel scene itself unaware of run-level concepts (lives, boards) where possible,
matching the existing Model→Network→View layering.

### 2. Segment/Board Advancement Logic
Correctly picking the next node's content: next story branch, next board's duel, or the final boss at segment 10.

## Non-Goals (Out of Scope)

- Board 3 / final boss content itself (Sprint 12)
- Victory/defeat screens (Sprint 12)

## Definition of Done for Sprint

- [ ] Playing several duels in a row within one run correctly tracks lives/progress across all of them

# Sprint 03 Goals

*(added retrospectively 2026-08-13 — this sprint already ran; see `Docs/scrum/sprint-03/done/` and `todo/`)*

## Primary Goal

**Connect the data-driven card system built in Sprint 02 to actual gameplay, and — mid-sprint — replace the
underlying state architecture entirely.**

## Success Criteria

By the end of this sprint, we had:

1. ✅ Deck loading and card spawning driven by `CardData`/`DeckData` instead of hardcoded values
2. ✅ Abilities executing with real effects during a match
3. ✅ A full architecture rewrite (Story 036) — `BoardState`/`NetworkBoardState`/`BoardView` replacing the old
   `CardController`/`PlayerController`/`HandController`/`EncounterController` stack entirely
4. ⬜ Player-facing deck selection before a match (024 — rescheduled to Sprint 06, never landed here)

## Key Focus Areas

### 1. Runtime Integration (original scope)
Wire the Sprint 02 data system into live gameplay — deck loading, card spawning from `CardData`, ability
execution.

### 2. Architecture Revamp (emerged mid-sprint)
Story 036 wasn't in the original plan. The old state-tracking approach (magic strings, multiple sources of
truth across `NetworkPlayer`/`PlayerController`/`HandController`) turned out to need a clean rewrite rather than
incremental fixes — see `Docs/scrum/sprint-03/done/story-036-board-state-architecture-revamp.md` for the full
rationale. This absorbed most of the sprint and superseded 023/025/026's original implementation plans (their
goals shipped, just via the rewrite — see the "Reclassified" notes on those story files).

## Non-Goals (Out of Scope)

- Adventure Mode content (that was Sprint 04, now retired/redistributed)
- Deck selection UI (024 — didn't fit once the rewrite absorbed the sprint)
- New card designs or balance changes

## Definition of Done for Sprint

- [x] Card/ability data drives live gameplay, not hardcoded values
- [x] Old `CardController`-era classes fully removed, not just deprecated
- [x] Multiplayer duel loop functions end-to-end on the new architecture
- [ ] Deck selection before a match (deferred to Sprint 06)

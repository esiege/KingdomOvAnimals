# Sprint 08 Goals

*(planned 2026-08-12, not yet started)*

## Primary Goal

**Build the run/map state machine — the new architectural layer Adventure Mode needs above the existing
single-duel `NetworkBoardState`.**

## Success Criteria

By the end of this sprint, we should have:

1. [ ] Server-authoritative run state: current board, current segment, lives remaining, deck-so-far (Story 046)
2. [ ] Progress tracking across a run's 10 duels (Story 033, amended from its original save/load-only scope)
3. [ ] A board map UI showing run progress (Story 034)

## Key Focus Areas

### 1. New Layer, Not a Bolt-On
`NetworkBoardState` knows about one duel at a time. This sprint adds the layer above it that persists across all
10 duels in a run, following the same server-authoritative pattern (`Cmd*` mutation, events for client updates).

### 2. Session Model Decision
Decide whether run state ties to a persistent account/session or only the current connection — affects whether
progress survives a client restart, not just a mid-duel disconnect.

## Non-Goals (Out of Scope)

- Actual segment content (story branches, card rewards — Sprint 10)
- Wiring duel results into this state (Sprint 11 — this sprint builds the model, not the connections to it yet)

## Definition of Done for Sprint

- [ ] Run state model exists and syncs correctly across two clients
- [ ] Board map UI renders current run position from that state

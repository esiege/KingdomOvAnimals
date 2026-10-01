# Sprint 05 Goals

*(planned 2026-08-12, not yet started)*

## Primary Goal

**Close the two pieces of netcode technical debt found during the 2026-08-12 docs accuracy pass before any
Adventure Mode work is built on top of them.**

## Success Criteria

By the end of this sprint, we should have:

1. [ ] One turn-tracking system, not two (Story 041)
2. [ ] Reconnection actually wired to a real disconnect event, not just scaffolded (Story 042)

## Key Focus Areas

### 1. Turn-System Consolidation
`NetworkGameManager` and `NetworkBoardState` currently track turns independently and can drift apart —
`InputController` (what players interact with) gates on `NetworkBoardState`'s, so that's the one to keep. See
`Docs/networking/turn-synchronization.md`.

### 2. Reconnection
`ReconnectionManager`, `DisconnectedPlayerState`, and `NetworkPlayer`'s pending-state restore path all exist but
have no call sites connecting them to an actual FishNet disconnect event. See `Docs/networking/reconnection.md`.

## Non-Goals (Out of Scope)

- Any Adventure Mode work (starts Sprint 07)
- New gameplay features
- Ability behavior completeness (Sprint 06)

## Definition of Done for Sprint

- [ ] Only one turn-tracking system remains in the codebase
- [ ] Disconnect → grace period → reconnect → state restore works end-to-end in a manual two-client test
- [ ] Disconnect → grace period expiry → remaining player returned to MainMenu works

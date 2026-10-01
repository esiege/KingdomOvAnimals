# Story 042: Wire Reconnection End-to-End

## Status: Not Started
## Sprint: 05
## Dependencies: None
## Created: 2026-08-12

## Background

The pieces for despawn/respawn reconnection already exist — `NetworkGameManager.OnPlayerDisconnected`,
`DisconnectedPlayerState.Capture()`, `ReconnectionManager.StartReconnectionWait()`, `NetworkPlayer`'s pending-state
restore path in `OnStartServer()` — but as of vdate (2026-08-12) none of them have call sites connecting them to
an actual FishNet disconnect event. See `Docs/networking/reconnection.md` for the full inventory.

## User Story
**As a** player
**I want** to rejoin a match if I get disconnected
**So that** a dropped connection doesn't automatically end the game

## Acceptance Criteria
- [ ] A real FishNet disconnect event (`OnRemoteConnectionState`/`OnClientConnectionState`) actually triggers
      `NetworkGameManager.OnPlayerDisconnected` (server) and `ReconnectionManager.StartReconnectionWait` (client)
- [ ] `DisconnectedPlayerState.Capture()` is called at disconnect time and its result reaches the respawned
      `NetworkPlayer` via `SetPendingState()` before spawn
- [ ] Grace-period UI reflects real state (currently the timer runs even though nothing sets
      `OpponentDisconnected` today)
- [ ] Manual test: disconnect one client mid-match, reconnect within the grace period, confirm health/mana/hand/
      board/turn all restore correctly
- [ ] Manual test: let the grace period expire, confirm the remaining player is returned to `MainMenu` cleanly

## Notes
Touches `Assets/Scripts/Network/NetworkGameManager.cs`, `ReconnectionManager.cs`, `DisconnectedPlayerState.cs`,
`NetworkPlayer.cs`, `ConnectionManager.cs`.

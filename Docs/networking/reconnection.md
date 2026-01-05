# Reconnection System

Handles player disconnection with a grace period for reconnection, preserving game state.

## Overview

When a player disconnects:
- 120-second grace period begins
- Disconnected player can rejoin and restore state
- If grace period expires, disconnected player forfeits
- Remaining player wins

## Two Reconnection Scenarios

### Scenario A: Client Disconnects (Host Remains)
- Server detects disconnect via `OnRemoteConnectionState`
- Server sets `OpponentDisconnected = true`
- Server tracks disconnected player for potential reconnection
- If client reconnects, server re-links and sends current state

### Scenario B: Host Disconnects (Client Remains)
- Client detects disconnect via `OnClientConnectionState`
- Client saves local game state snapshot
- Client attempts reconnection every 3 seconds
- If host returns, client restores state locally

## GameStateSnapshot

Serializable snapshot of entire game state:

| Field | Type | Description |
|-------|------|-------------|
| `turnNumber` | int | Current turn |
| `currentTurnObjectId` | int | Whose turn |
| `shuffleSeed` | int | Original deck seed |
| `localPlayer` | PlayerSnapshot | Capturing player's state |
| `opponent` | PlayerSnapshot | Opponent's state |
| `timestamp` | long | Unix timestamp |

### PlayerSnapshot Contents

- Health, mana, max mana
- Hand card IDs (by name)
- Board cards with slot positions and status
- Deck remaining cards
- Graveyard cards

## State Capture

Triggered in `OnHostDisconnected()`:

1. Capture NetworkGameManager turn state
2. Capture both PlayerController states
3. Capture both HandController hands
4. Capture board card positions and status
5. Store in `_savedGameState`

## State Restoration

### Server-Side (Client Reconnects)
1. Client sends `ServerRestoreGameState(snapshotJson)`
2. Server validates and applies snapshot
3. Server broadcasts updated state to all clients
4. RPCs restore visual state

### Client-Side (Host Returns)
1. Client calls `RestoreGameStateLocally(snapshot)`
2. Restores PlayerController health/mana
3. Restores cards in hand/board from CardLibrary
4. Updates UI to match snapshot

## Grace Period UI

EncounterController manages disconnect UI:
- `disconnectPanel` - Overlay panel
- `disconnectStatusText` - Countdown timer display
- `UpdateDisconnectTimer(float)` - Updates countdown
- `OnOpponentReconnected()` - Hides panel
- `OnOpponentForfeited()` - Shows victory

## Limitations

Current implementation limitations:
- Card prefabs must exist in CardLibrary for restoration
- Exact card positioning may differ after restore
- Some visual states may not fully restore

## Debug Logging

Reconnection writes to separate log files:
- `Docs/reconnect_editor.txt` (Editor/Host)
- `Docs/reconnect_build.txt` (Build/Client)

Uses direct file I/O, independent of Unity's logging system.

---
*Parent: [Networking System](./README.md)*

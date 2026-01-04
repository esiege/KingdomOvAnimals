# Story 002: Create Player Connection Handler

## Story Information

- **Story ID**: 002
- **Priority**: High
- **Sprint**: Sprint 01
- **Status**: ✅ Complete
- **Created**: 2026-01-03
- **Started**: 2026-01-03
- **Completed**: 2026-01-03

## User Story

**As a** player  
**I want** my game to handle connections properly  
**So that** I can join a game and be recognized as a player

## Background / Context

We have FishNet NetworkManager working. Now we need to handle what happens when a player connects - spawning a player object, tracking connected players, etc.

## Acceptance Criteria

### AC1: Player object spawns on connection
**Test:** Connect as client to a host
**Expected:** A player object is spawned and assigned to the connecting client

### AC2: Server tracks connected players
**Test:** Connect two clients to a host, check server logs
**Expected:** Server logs show 2 players connected with unique IDs

### AC3: Player can disconnect cleanly
**Test:** Disconnect a client while connected
**Expected:** Player object is removed, server updates player count

### AC4: Host player is also tracked
**Test:** Start as Host
**Expected:** Host's player object spawns and is tracked as player 1

---

## Tasks

- [x] Create NetworkPlayer script with SyncVars
- [x] Create PlayerConnectionHandler script to handle spawn on connect
- [ ] In Unity: Create NetworkPlayer prefab with NetworkObject component
- [ ] In Unity: Add PlayerConnectionHandler to NetworkManager scene
- [ ] In Unity: Assign NetworkPlayer prefab to PlayerConnectionHandler
- [ ] In Unity: Add NetworkPlayer prefab to DefaultPrefabObjects (for spawning)
- [ ] Test: Connect two clients, verify both get NetworkPlayer spawned
- [ ] Test: Disconnect a client, verify cleanup logs

## Implementation Notes

### Scripts Created
1. **NetworkPlayer.cs** (`Assets/Scripts/Network/`)
   - NetworkBehaviour with SyncVars (PlayerId, PlayerName, IsReady)
   - Logs when clients join/leave
   - Has ServerRpc for setting ready state

2. **PlayerConnectionHandler.cs** (`Assets/Scripts/Network/`)
   - Subscribes to OnRemoteConnectionState
   - Spawns NetworkPlayer for each connecting client
   - Tracks all connected players in dictionary
   - Handles cleanup on disconnect

### Unity Setup Steps
1. Create empty GameObject, add NetworkObject component
2. Add NetworkPlayer script to it
3. Save as prefab: `Assets/Prefabs/Network/NetworkPlayer.prefab`
4. Add prefab to `DefaultPrefabObjects` asset (for network spawning)
5. Add PlayerConnectionHandler to scene, assign the prefab

## Questions / Decisions

### Open Questions
- [ ] Should we use FishNet's built-in player spawning or custom?
- [ ] Where should player data live (PlayerController vs NetworkPlayer)?

### Decisions Made
- *(To be filled as we work)*
